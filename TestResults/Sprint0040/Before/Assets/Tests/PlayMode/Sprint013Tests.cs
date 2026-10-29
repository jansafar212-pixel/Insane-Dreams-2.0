#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint013Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private GameObject player;private EnemyCombat enemy;private Camera camera;private bool background;
        private GameObject Keep(GameObject value){objects.Add(value);return value;}
        private GameObject Solid(string name,Vector2 position,Vector2 size)
        {var go=Keep(new GameObject(name));go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        private void Freeze(GameObject go){var body=go.GetComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.constraints=RigidbodyConstraints2D.FreezeAll;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(250,1),Quaternion.identity));Freeze(player);player.GetComponent<Health>().ProtectFor(60);
            var actor=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(256,1),Quaternion.identity));Freeze(actor);actor.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            camera=Keep(new GameObject("GameplayTestCamera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4;camera.aspect=1;camera.transform.position=new Vector3(250,1,-10);
            enemy=actor.GetComponent<EnemyCombat>();enemy.BindGameplayCamera(camera);enemy.BindTarget(player.GetComponent<Health>());
            var settings=new SerializedObject(enemy);foreach(string key in new[]{"ordinaryBlindChance","overconfidentBlindChance","cowardBlindChance"})settings.FindProperty(key).floatValue=1;settings.ApplyModifiedPropertiesWithoutUndo();
            yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        private IEnumerator Engage(){for(int i=0;i<80&&enemy.GetComponent<AttackExecutor>().ShotsFired<2;i++)yield return new WaitForFixedUpdate();Assert.GreaterOrEqual(enemy.GetComponent<AttackExecutor>().ShotsFired,2);}
        [UnityTest] public IEnumerator FreshOffscreenCannotFireButBodyEdgeEntryEngagesAndCameraExitPreservesBlindMemory()
        {
            var other=Keep(new GameObject("UnrelatedCamera")).AddComponent<Camera>();other.transform.position=new Vector3(256,1,-10);other.orthographic=true;
            yield return new WaitForSeconds(.7f);Assert.IsTrue(enemy.TargetVisible);Assert.IsFalse(enemy.InGameplayViewport);Assert.AreEqual(0,enemy.GetComponent<AttackExecutor>().ShotsFired);Assert.IsFalse(enemy.HasEngaged);
            camera.transform.position=new Vector3(251.8f,1,-10);Assert.Greater(camera.WorldToViewportPoint(enemy.transform.position).x,1);Assert.IsTrue(enemy.InGameplayViewport,"Visible body edge must count even with pivot outside.");
            yield return Engage();Assert.IsTrue(enemy.HasEngaged);int shots=enemy.GetComponent<AttackExecutor>().ShotsFired;
            camera.transform.position=new Vector3(230,1,-10);yield return new WaitForSeconds(.3f);Assert.AreEqual(shots,enemy.GetComponent<AttackExecutor>().ShotsFired);Assert.IsTrue(enemy.HasEngaged);
            camera.transform.position=new Vector3(254,1,-10);yield return new WaitForSeconds(.15f);
            var known=enemy.LastKnownPosition;Solid("Occluder",new Vector2(253,1),new Vector2(.2f,6));player.transform.position=new Vector3(249,2);Physics2D.SyncTransforms();shots=enemy.GetComponent<AttackExecutor>().ShotsFired;
            yield return new WaitForSeconds(.35f);Assert.IsTrue(enemy.IsBlindFiring);Assert.AreEqual(known,enemy.LastKnownPosition);Assert.Greater(enemy.GetComponent<AttackExecutor>().ShotsFired,shots);Assert.AreEqual(ShotSurface.Geometry,enemy.GetComponent<AttackExecutor>().LastSurface);
        }
        [UnityTest] public IEnumerator InViewportOcclusionStillPreventsFreshFireUntilRealSightReturns()
        {
            camera.transform.position=new Vector3(254,1,-10);var wall=Solid("OpaqueWall",new Vector2(253,1),new Vector2(.3f,6));
            yield return new WaitForSeconds(.6f);Assert.IsTrue(enemy.InGameplayViewport);Assert.IsFalse(enemy.TargetVisible);Assert.AreEqual(0,enemy.GetComponent<AttackExecutor>().ShotsFired);
            Object.Destroy(wall);yield return null;yield return Engage();
        }
        [UnityTest] public IEnumerator MissingDisabledOrWrongLayerGameplayCameraFailsClosedForFreshEnemy()
        {
            enemy.BindGameplayCamera(null);yield return new WaitForSeconds(.45f);Assert.AreEqual(0,enemy.GetComponent<AttackExecutor>().ShotsFired);
            enemy.BindGameplayCamera(camera);camera.transform.position=new Vector3(254,1,-10);camera.enabled=false;yield return new WaitForSeconds(.2f);Assert.AreEqual(0,enemy.GetComponent<AttackExecutor>().ShotsFired);
            camera.enabled=true;camera.cullingMask=1<<enemy.gameObject.layer;yield return new WaitForSeconds(.2f);Assert.AreEqual(0,enemy.GetComponent<AttackExecutor>().ShotsFired,"Seeing the collision root layer does not reveal the torso artwork.");
            camera.cullingMask=1<<enemy.GetComponent<CharacterVisualDriver>().Rig.torso.gameObject.layer;yield return Engage();enemy.BindTarget(player.GetComponent<Health>());camera.transform.position=Vector3.back*10;
            int shots=enemy.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.5f);Assert.IsFalse(enemy.HasEngaged);Assert.AreEqual(shots,enemy.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator EverySurfaceClassUsesRealNearestHitAndAutomaticEffectsStayBounded()
        {
            enemy.enabled=false;var wall=Solid("ImpactWall",new Vector2(252,1),new Vector2(.3f,6));var marker=wall.AddComponent<ImpactSurface2D>();
            var attack=player.GetComponent<AttackExecutor>();var feedback=player.GetComponent<CombatFeedback>();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;float now=Time.time;
            foreach(var material in new[]{ImpactMaterial.Generic,ImpactMaterial.Concrete,ImpactMaterial.Metal,ImpactMaterial.Wood})
            {
                marker.Configure(material);Assert.IsTrue(attack.Attack(weapon,Vector2.right,now+=1));Assert.AreEqual(material,attack.LastImpactMaterial);Assert.AreEqual(material,feedback.LastImpactMaterial);
                Assert.AreEqual(ShotSurface.Geometry,attack.LastSurface);Assert.That(attack.LastShotEnd.x,Is.EqualTo(251.85f).Within(.01f));
                Assert.IsTrue(player.GetComponentsInChildren<LineRenderer>().Any(l=>l.enabled&&Vector2.Distance(l.GetPosition(1),attack.LastShotEnd)<.002f));yield return null;
            }
            int children=player.GetComponentsInChildren<Transform>(true).Length;
            for(int i=0;i<100;i++){weapon.SetAmmunition(18,54);Assert.IsTrue(attack.Attack(weapon,Vector2.right,now+=1));}
            Assert.AreEqual(children,player.GetComponentsInChildren<Transform>(true).Length);Assert.AreEqual(12,feedback.ImpactParticleCapacity);Assert.AreEqual(6,player.GetComponentsInChildren<LineRenderer>().Length);
            yield return new WaitForSeconds(.6f);Assert.IsFalse(player.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name.StartsWith("ImpactParticle_")&&r.enabled));Assert.IsFalse(player.GetComponentsInChildren<LineRenderer>().Any(l=>l.enabled));
        }
        [UnityTest] public IEnumerator AudioIntegrationBoundsVoicesAndReleasesThemOnDisable()
        {
            enemy.enabled=false;var feedback=player.GetComponent<CombatFeedback>();var clip=AudioClip.Create("SilentIntegrationFixture",22050,1,44100,false);
            try{
                var s=new SerializedObject(feedback);s.FindProperty("firingSound").objectReferenceValue=clip;s.ApplyModifiedPropertiesWithoutUndo();
                var attack=player.GetComponent<AttackExecutor>();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;
                for(int i=0;i<30;i++){weapon.SetAmmunition(18,54);attack.Attack(weapon,Vector2.up,Time.time+i);}
                Assert.LessOrEqual(CombatFeedback.ActiveAudioVoices,CombatFeedback.GlobalVoiceLimit);Assert.LessOrEqual(player.GetComponentsInChildren<AudioSource>().Count(a=>a.isPlaying),2);
                player.SetActive(false);Assert.AreEqual(0,CombatFeedback.ActiveAudioVoices);
            }finally{Object.Destroy(clip);}yield return null;
        }
        [UnityTest] public IEnumerator SmallDirectionalFlinchNeverMovesPhysicsOrChangesCollider()
        {
            enemy.enabled=false;enemy.GetComponent<LarperAwareness>().enabled=false;var body=enemy.GetComponent<Rigidbody2D>();var box=enemy.GetComponent<BoxCollider2D>();var position=body.position;var size=box.size;var offset=box.offset;
            var rig=enemy.GetComponent<CharacterVisualDriver>().Rig;enemy.GetComponent<Health>().ApplyDamage(new DamageInfo(5,CombatTeam.Player,player,new PhysicalImpact(position,Vector2.right,.9f)));
            yield return new WaitForSeconds(.04f);Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(0,rig.torso.localEulerAngles.z)),.1f);Assert.AreEqual(position,body.position);Assert.AreEqual(size,box.size);Assert.AreEqual(offset,box.offset);
            yield return new WaitForSeconds(.15f);Assert.Less(Mathf.Abs(Mathf.DeltaAngle(0,rig.torso.localEulerAngles.z)),.01f);Assert.AreEqual(position,body.position);
        }
    }
}
#endif
