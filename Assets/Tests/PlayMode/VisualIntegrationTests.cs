#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Visuals;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using InsaneDreams.Checkpoints;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class VisualIntegrationTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private Camera camera;
        private UIDocument document;
        private bool background;
        private GameObject Keep(GameObject value){objects.Add(value);return value;}
        private GameObject Actor(bool enemy=false,float x=7000)
        {
            var root=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(enemy?"Assets/Prefabs/Enemies/Larper.prefab":"Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(x,.85f),Quaternion.identity));
            if(enemy){root.GetComponent<LarperBrain>().enabled=false;root.GetComponent<LarperSpeechBubble>().Bind(camera,document);root.GetComponent<EnemyCombat>().BindGameplayCamera(camera);}
            return root;
        }
        private void Freeze(GameObject actor){actor.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;LarperAwareness.ResetEncounterSpeech();
            var floor=Keep(new GameObject("VisualTestFloor"));floor.transform.position=new Vector3(7000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(80,1);
            var cam=Keep(new GameObject("VisualTestCamera"));camera=cam.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8;cam.transform.position=new Vector3(7000,2,-10);
            document=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();
            yield return null;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();
            foreach(var doll in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))doll.Retire();
            yield return null;Application.runInBackground=background;
        }
        [UnityTest] public IEnumerator WalkingFacingAimRecoilAndCrouchRemainIndependentOfGameplay()
        {
            var actor=Actor();var actions=actor.GetComponent<PlayerActions>();var visual=actor.GetComponent<CharacterVisualDriver>();var body=actor.GetComponent<Rigidbody2D>();
            yield return new WaitForSeconds(.25f);Assert.AreEqual(CharacterPose.Idle,visual.CurrentPose);
            actions.SetTouchMove(Vector2.right);actions.SetTouchAim(Vector2.left);yield return new WaitForSeconds(.15f);
            Assert.AreEqual(CharacterPose.Walk,visual.CurrentPose);Assert.AreEqual(1,visual.Facing);Assert.Less(visual.Aim.x,0);
            Assert.Less(visual.Rig.weaponMount.TransformVector(Vector3.right).x,0);Assert.Greater(body.linearVelocity.x,0);
            actions.SetTouchMove(Vector2.left);actions.SetTouchAim(Vector2.up);yield return new WaitForSeconds(.3f);
            Assert.AreEqual(-1,visual.Facing);Assert.Greater(visual.Rig.weaponMount.TransformVector(Vector3.right).normalized.y,.95f);
            actions.SetTouchMove(Vector2.down);actions.SetTouchAim(Vector2.right);yield return new WaitForSeconds(.15f);
            Assert.AreEqual(CharacterPose.Crouch,visual.CurrentPose);
            var bounds=actor.GetComponent<BoxCollider2D>().bounds;
            Assert.LessOrEqual(visual.Rig.head.GetComponent<SpriteRenderer>().bounds.max.y,bounds.max.y+.05f);
            Assert.That(visual.Rig.leftLeg.GetComponent<SpriteRenderer>().bounds.min.y,Is.EqualTo(bounds.min.y).Within(.04f));
            actions.SetTouchMove(new Vector2(.7f,-.7f));actions.SetFireIntent(true);yield return new WaitForSeconds(.12f);
            Assert.AreEqual(CharacterPose.CrouchWalk,visual.CurrentPose);Assert.Greater(actor.GetComponent<AttackExecutor>().ShotsFired,0);
            Assert.LessOrEqual(Vector3.Distance(visual.Rig.transform.position,actor.transform.position),.025f);
            Assert.IsFalse(actor.GetComponent<SpriteRenderer>().enabled);Assert.IsFalse(actor.transform.Find("PostureVisual").GetComponent<SpriteRenderer>().enabled);
        }
        [UnityTest] public IEnumerator JumpFallChuteLadderAndRecoveryUseActualTraversalState()
        {
            var actor=Actor();var actions=actor.GetComponent<PlayerActions>();var body=actor.GetComponent<Rigidbody2D>();var visual=actor.GetComponent<CharacterVisualDriver>();
            yield return new WaitForSeconds(.25f);actions.Jump();yield return new WaitForSeconds(.1f);Assert.AreEqual(CharacterPose.Jump,visual.CurrentPose);
            yield return new WaitForSeconds(.45f);Assert.AreEqual(CharacterPose.Fall,visual.CurrentPose);
            body.position=new Vector2(7000,6);body.linearVelocity=Vector2.down*4;Physics2D.SyncTransforms();yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);actions.SetTouchParachute(false);
            yield return null;yield return null;Assert.AreEqual(CharacterPose.Parachute,visual.CurrentPose);Assert.IsTrue(visual.Rig.parachute.enabled);
            var ladder=Keep(new GameObject("VisualLadder"));ladder.transform.position=new Vector3(7000,5);ladder.AddComponent<BoxCollider2D>().size=new Vector2(1.8f,12);ladder.GetComponent<BoxCollider2D>().isTrigger=true;ladder.AddComponent<LadderVolume>();Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.15f);Assert.AreEqual(CharacterPose.Climb,visual.CurrentPose);Assert.IsTrue(visual.Rig.parachute.enabled);Assert.IsTrue(visual.Rig.weapon.enabled);
            actor.GetComponent<PlayerRecovery>().Respawn();yield return new WaitForSeconds(.2f);Assert.IsFalse(actor.GetComponent<PlayerTraversal2D>().IsClimbing);Assert.AreEqual(3,body.gravityScale);
            Assert.AreEqual(FaceExpression.Neutral,visual.Expression);Assert.IsTrue(visual.Rig.IsBound);
            actor.SetActive(false);actor.SetActive(true);yield return null;Assert.IsTrue(visual.Rig.IsBound);
        }
        [UnityTest] public IEnumerator DeliveredBoastAndCombatMissesDriveExpressionsWithoutRandomTimers()
        {
            var actor=Actor(true);Freeze(actor);var awareness=actor.GetComponent<LarperAwareness>();awareness.Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Overconfident.asset"));
            var target=Keep(new GameObject("VisualTarget"));target.transform.position=new Vector3(7002,1);actor.GetComponent<EnemyCombat>().BindTarget(target.AddComponent<Health>());
            yield return new WaitForSeconds(.12f);var visual=actor.GetComponent<CharacterVisualDriver>();Assert.AreEqual(FaceExpression.Smug,visual.Expression);
            Assert.AreEqual(1,actor.GetComponent<LarperSpeechBubble>().LinesShown);
            actor.GetComponent<EnemyCombat>().enabled=false;
            var definition=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");
            Assert.IsNotNull(definition);var weapon=new InsaneDreams.Inventory.RuntimeWeapon(definition,36);var executor=actor.GetComponent<AttackExecutor>();
            for(int i=0;i<4;i++){executor.Attack(weapon,Vector2.up,Time.time+i*2);}
            yield return null;yield return null;Assert.AreEqual(FaceExpression.Angry,visual.Expression);
            yield return new WaitForSeconds(1.5f);Assert.AreEqual(FaceExpression.Neutral,visual.Expression);
            actor.GetComponent<Health>().ApplyDamage(new DamageInfo(1,CombatTeam.Player));yield return null;yield return null;Assert.AreEqual(FaceExpression.Stunned,visual.Expression);
        }
        [UnityTest] public IEnumerator WitnessPanicFaceSpeechAnchorOverlapAndDefeatLifecycleAreConnected()
        {
            var actor=Actor(true);var other=Actor(true,7001);Freeze(actor);Freeze(other);
            var profile=Object.Instantiate(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Cowardly.asset"));
            var settings=new SerializedObject(profile);settings.FindProperty("panicChance").floatValue=1;settings.FindProperty("panicSeconds").floatValue=.4f;settings.ApplyModifiedPropertiesWithoutUndo();actor.GetComponent<LarperAwareness>().Configure(profile);
            yield return null;var bubble=actor.GetComponent<LarperSpeechBubble>();var otherBubble=other.GetComponent<LarperSpeechBubble>();
            bubble.Show("ONE");otherBubble.Show("TWO");yield return null;yield return null;
            Assert.IsFalse(bubble.Placement.Overlaps(otherBubble.Placement));Assert.AreSame(actor.GetComponent<CharacterVisualDriver>().Rig.speechAnchor,bubble.HeadAnchor);
            other.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;yield return null;
            var visual=actor.GetComponent<CharacterVisualDriver>();Assert.AreEqual(CharacterPose.Panic,visual.CurrentPose);Assert.AreEqual(FaceExpression.Scared,visual.Expression);Assert.IsTrue(bubble.IsVisible);Assert.IsFalse(otherBubble.IsVisible);
            Assert.AreEqual(1,actor.GetComponent<LarperAwareness>().PanicReactions);
            yield return new WaitForSeconds(1.6f);Assert.AreEqual(FaceExpression.Neutral,visual.Expression);
            actor.SetActive(false);Assert.IsFalse(bubble.IsVisible);Object.Destroy(profile);
        }
        [UnityTest] public IEnumerator FatalDamageCreatesOneStableRagdollStopsEnemyAndTransfersMomentum()
        {
            var actor=Actor(true);yield return new WaitForSeconds(.2f);var body=actor.GetComponent<Rigidbody2D>();body.linearVelocity=new Vector2(3,1);
            var health=actor.GetComponent<Health>();var observer=actor.GetComponent<RagdollOnDefeat>();var point=actor.GetComponent<CharacterVisualDriver>().Rig.head.position;
            health.ApplyDamage(new DamageInfo(1000,CombatTeam.Player));Assert.IsFalse(actor.activeSelf);Assert.AreEqual(1,observer.SpawnCount);
            var doll=observer.LastRagdoll;Assert.IsNotNull(doll);Assert.That(Vector3.Distance(doll.parts[1].position,point),Is.LessThan(.01f));Assert.Greater(doll.parts[0].linearVelocity.x,2);
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(1000,CombatTeam.Player)));Assert.AreEqual(1,observer.SpawnCount);
            int shots=actor.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(2);
            Assert.AreEqual(shots,actor.GetComponent<AttackExecutor>().ShotsFired);
            foreach(var part in doll.parts){Assert.Less(part.linearVelocity.magnitude,12);Assert.Less(Mathf.Abs(part.position.x-7000),15);Assert.Greater(part.position.y,-.5f);}
            foreach(var joint in doll.GetComponentsInChildren<HingeJoint2D>()){Assert.IsNotNull(joint.connectedBody);Assert.IsTrue(joint.enabled);}
            doll.Lifetime=.1f;yield return new WaitForSeconds(.15f);Assert.IsTrue(doll==null);
        }
        [UnityTest] public IEnumerator RagdollCapAndSceneOwnedCleanupPreventAccumulation()
        {
            var actor=Actor(true);yield return null;var rig=actor.GetComponent<CharacterVisualDriver>().Rig;
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab").GetComponent<CartoonRagdoll>();
            CartoonRagdoll first=null;
            for(int i=0;i<6;i++){var doll=Object.Instantiate(prefab);doll.Activate(rig,Vector2.zero);if(i==0)first=doll;}
            Assert.AreEqual(4,CartoonRagdoll.ActiveCount);Assert.IsFalse(first.gameObject.activeSelf);yield return null;
            foreach(var doll in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))doll.Retire();yield return null;Assert.AreEqual(0,CartoonRagdoll.ActiveCount);
        }
        [UnityTest] public IEnumerator MirroredDefeatPreservesLimbDirectionAndSceneUnloadCleansBodies()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.CreateScene("VisualSceneRebindTest");var actor=Actor(true);Freeze(actor);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(actor,scene);
            yield return null;actor.GetComponent<CharacterVisualDriver>().enabled=false;
            var rig=actor.GetComponentInChildren<CutoutRig>();rig.Pose(CharacterPose.Jump,0,Vector2.up,-1,0,FaceExpression.Surprised);
            Vector3 expected=rig.leftArm.TransformVector(Vector3.down).normalized;
            actor.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));var doll=actor.GetComponent<RagdollOnDefeat>().LastRagdoll;
            Assert.AreEqual(scene,doll.gameObject.scene);Assert.Greater(Vector3.Dot(expected,doll.parts[2].transform.TransformVector(Vector3.down).normalized),.99f);
            yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene);
            Assert.IsTrue(doll==null);Assert.AreEqual(0,CartoonRagdoll.ActiveCount);
        }
        [UnityTest] public IEnumerator ActualShotRecoilAndSkinReplacementDoNotMoveActorOrChangeHealth()
        {
            var actor=Actor();yield return new WaitForSeconds(.25f);var visual=actor.GetComponent<CharacterVisualDriver>();var rig=visual.Rig;
            float before=rig.weaponMount.localPosition.x;var executor=actor.GetComponent<AttackExecutor>();var health=actor.GetComponent<Health>();float hp=health.Current;
            var weapon=new InsaneDreams.Inventory.RuntimeWeapon(AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),36);
            Assert.IsTrue(executor.Attack(weapon,Vector2.right,Time.time));yield return null;yield return null;
            Assert.Less(rig.weaponMount.localPosition.x,before-.01f);
            var original=rig.skin;var replacement=Object.Instantiate(original);replacement.clothing=Color.magenta;var point=actor.transform.position;
            rig.ApplySkin(replacement);Assert.AreEqual(Color.magenta,rig.torso.GetComponent<SpriteRenderer>().color);Assert.AreEqual(point,actor.transform.position);Assert.AreEqual(hp,health.Current);
            rig.ApplySkin(original);Object.Destroy(replacement);
        }
    }
}
#endif
