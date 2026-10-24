#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0246Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;private const float X=48000;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private GameObject Actor(string name,float x=0)
        {var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/"+(name=="WarehousePlayer"?"Player/":"Enemies/")+name+".prefab"),new Vector3(X+x,.85f),Quaternion.identity));if(g.TryGetComponent<EnemyCombat>(out var e)){e.enabled=false;g.GetComponent<LarperBrain>().enabled=false;}if(g.TryGetComponent<PlayerActions>(out var a)){var s=new SerializedObject(a);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();}return g;}
        private CartoonRagdoll Corpse(string name,float x)
        {var g=Actor(name,x);g.GetComponent<CharacterVisualDriver>().Rig.ApplySkin(g.GetComponent<CharacterVisualDriver>().Rig.skin);g.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));var d=g.GetComponent<RagdollOnDefeat>().LastRagdoll;Keep(d.gameObject);return d;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;var floor=Keep(new GameObject("S0246Floor"));floor.transform.position=new Vector2(X,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest]public IEnumerator EverySegmentIgnoresCharactersButStillSettlesOnEnvironment()
        {var p=Actor("WarehousePlayer");foreach(string name in new[]{"Larper","Samurai"}){var d=Corpse(name,2);Assert.AreEqual(14,d.Bodies.Length);foreach(var b in d.Bodies){var c=b.GetComponent<Collider2D>();Assert.IsTrue(c.enabled);Assert.IsFalse(c.isTrigger);Assert.AreEqual(-2,c.excludeLayers.value);Assert.AreEqual(0,c.excludeLayers.value&1);Assert.AreNotEqual(0,c.excludeLayers.value&(1<<p.layer));}}yield return new WaitForSeconds(1.5f);foreach(var d in Object.FindObjectsByType<CartoonRagdoll>())foreach(var b in d.Bodies){Assert.Greater(b.position.y,-.8f);Assert.IsFalse(float.IsNaN(b.position.x));}}
        [UnityTest]public IEnumerator OverlappingCorpsesCannotBlockOrPushWalkingAndJumpingPlayer()
        {var p=Actor("WarehousePlayer");for(int i=0;i<3;i++)Corpse(i==1?"Samurai":"Larper",1+i*.2f);var a=p.GetComponent<PlayerActions>();yield return new WaitForSeconds(.3f);a.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.8f);Assert.Greater(p.transform.position.x,X+4.5f);Assert.That(p.transform.position.y,Is.InRange(.7f,.95f));a.SetTouchMove(Vector2.zero);var body=p.GetComponent<Rigidbody2D>();body.position=new Vector2(X+1.2f,.85f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();yield return new WaitForSeconds(.2f);a.Jump();yield return new WaitForSeconds(.18f);Assert.Greater(body.position.y,2);Assert.AreEqual(1,p.GetComponent<PlayerMotor2D>().JumpsUsed);foreach(var d in Object.FindObjectsByType<CartoonRagdoll>())foreach(var c in d.GetComponentsInChildren<Collider2D>())Assert.IsFalse(c.IsTouching(p.GetComponent<Collider2D>()));}
        [UnityTest]public IEnumerator PlayerAndEnemyHitscanStillHitCorpseAndApplyImpactEffects()
        {var d=Corpse("Larper",3);foreach(var b in d.Bodies){b.bodyType=RigidbodyType2D.Dynamic;b.constraints=RigidbodyConstraints2D.FreezeAll;}Physics2D.SyncTransforms();yield return null;
            foreach(var team in new[]{CombatTeam.Player,CombatTeam.Enemy}){var g=Keep(new GameObject("CorpseShooter"));g.transform.position=new Vector2(X,.85f);var hp=g.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)team;s.ApplyModifiedPropertiesWithoutUndo();var a=g.AddComponent<AttackExecutor>();int before=d.ImpactCount;Assert.IsTrue(a.Attack(new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset"),27),Vector2.right,Time.time));Assert.AreEqual(ShotSurface.Ragdoll,a.LastSurface);Assert.AreEqual(before+1,d.ImpactCount);Assert.Greater(a.LastShotEnd.x,X+2);}
        }
        [UnityTest]public IEnumerator PistolAndMagazineRemainConnectedAcrossAimFacingAndReload()
        {var g=Actor("Larper");yield return null;g.GetComponent<CharacterVisualDriver>().enabled=false;var r=g.GetComponent<CharacterVisualDriver>().Rig;var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");r.weapon.sprite=w.EquippedSprite;r.SetMagazine(w.MagazineSprite,w.MagazineMountOffset);
            foreach(float f in new[]{-1f,1f})foreach(var pose in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Crouch,CharacterPose.Climb})foreach(var aim in new[]{Vector2.right,Vector2.left,Vector2.up})foreach(bool reload in new[]{false,true}){r.Pose(pose,1,aim,f,0,FaceExpression.Neutral,true,true,reload,.5f);Assert.Less(Vector2.Distance(r.weaponMount.position,r.Segmented.grip.position),.001f);Assert.AreSame(r.weapon.transform,r.magazine.transform.parent);Assert.IsTrue(r.magazine.enabled);Assert.AreEqual(1f/32,r.magazine.transform.localPosition.x,.001f);if(!reload){Assert.AreEqual((r.weapon.flipY?1:-1)*2f/32,r.magazine.transform.localPosition.y,.001f);Assert.Greater(Vector2.Dot(r.BarrelDirection,aim),.999f);}}
        }
        [UnityTest]public IEnumerator CorpsePileCannotBlockExistingLadderTraversalOnNarrowLanding()
        {
            owned[0].GetComponent<BoxCollider2D>().size=new Vector2(4,1);
            var p=Actor("WarehousePlayer");for(int i=0;i<3;i++)Corpse(i==1?"Samurai":"Larper",.1f*i);
            var ladder=Keep(new GameObject("PileLadder"));ladder.transform.position=new Vector2(X,2);var trigger=ladder.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(1.8f,6);ladder.AddComponent<InsaneDreams.Traversal.LadderVolume>();Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.35f);p.GetComponent<PlayerActions>().SetTouchMove(Vector2.up);yield return new WaitForSeconds(.6f);
            Assert.IsTrue(p.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>().IsClimbing);Assert.Greater(p.transform.position.y,2.8f);p.GetComponent<PlayerActions>().Jump();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.IsFalse(p.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>().IsClimbing);Assert.AreEqual(3,p.GetComponent<Rigidbody2D>().gravityScale);
        }
    }
}
#endif
