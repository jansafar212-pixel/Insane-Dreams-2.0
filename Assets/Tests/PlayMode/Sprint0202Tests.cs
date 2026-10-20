#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0202Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private GameObject player;private Camera camera;private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object o,string name,float value){var s=new SerializedObject(o);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private GameObject Solid(Vector2 p,Vector2 size){var go=Keep(new GameObject("S0202Geometry"));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            random=Random.state;background=Application.runInBackground;Application.runInBackground=true;
            Solid(new Vector2(13000,-.5f),new Vector2(120,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(12992,.85f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(60);
            camera=Keep(new GameObject("S0202Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;camera.aspect=2;camera.transform.position=new Vector3(13000,2,-10);yield return new WaitForSeconds(.2f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var o in objects)if(o!=null)Object.Destroy(o);objects.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        private EnemyCombat Enemy(float x=13000,string disposition="Ordinary")
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,.71f),Quaternion.identity));
            go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            go.GetComponent<LarperAwareness>().Configure(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+disposition+".asset"));
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);Float(e,"defensiveCrouchProbability",0);Float(e,"suppressionRetreatChance",0);return e;
        }
        private void Near(EnemyCombat e,float offset=.9f)
        {
            var c=(Vector2)e.GetComponent<BoxCollider2D>().bounds.center;var start=new Vector2(c.x-8,c.y+offset);e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,start,start+Vector2.right*16,1));
        }
        [UnityTest] public IEnumerator RealShotsReuseClippedTrajectoryAndExcludeDirectHitsEvenWhenProtected()
        {
            var e=Enemy();yield return new WaitForSeconds(.2f);var attack=player.GetComponent<AttackExecutor>();var weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),100);
            float hp=e.GetComponent<Health>().Current;var point=new Vector2(13000,e.GetComponent<BoxCollider2D>().bounds.max.y+.15f);
            Assert.IsTrue(attack.Attack(weapon,(point-attack.Origin).normalized,Time.time));Assert.AreEqual(hp,e.GetComponent<Health>().Current);Assert.Greater(e.Suppression,0);Assert.Greater(attack.LastShotEnd.x,13000);
            e.ResetEncounter(null);yield return new WaitForSeconds(.2f);e.GetComponent<Health>().ProtectFor(5);
            Assert.IsTrue(attack.Attack(weapon,((Vector2)e.transform.position-attack.Origin).normalized,Time.time));Assert.AreEqual(ShotSurface.Character,attack.LastSurface);Assert.AreEqual(0,e.Suppression);Assert.AreEqual(hp,e.GetComponent<Health>().Current);
        }
        [UnityTest] public IEnumerator CoverTruncatesRealShotAndRejectsEndpointAndSidewaysLeakage()
        {
            var e=Enemy();var wall=Solid(new Vector2(12999.5f,2),new Vector2(.25f,5));yield return new WaitForSeconds(.2f);
            var attack=player.GetComponent<AttackExecutor>();var weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),100);
            Assert.IsTrue(attack.Attack(weapon,((Vector2)e.transform.position-attack.Origin).normalized,Time.time));Assert.Less(attack.LastShotEnd.x,13000);Assert.AreEqual(0,e.Suppression);
            // A shot parallel to a low cover's top must not frighten the actor beneath it.
            Object.Destroy(wall);yield return null;Solid(new Vector2(13000,1.65f),new Vector2(3,.2f));Near(e,1.1f);Assert.AreEqual(0,e.Suppression);
        }
        [UnityTest] public IEnumerator ProximityAccumulationDecayAndPersonalityRemainBounded()
        {
            var coward=Enemy(13000,"Cowardly");var confident=Enemy(13004,"Overconfident");yield return new WaitForSeconds(.2f);
            Near(coward);Near(confident);Assert.Greater(coward.Suppression,confident.Suppression);float far=confident.Suppression;
            confident.ResetEncounter(null);Near(confident,.1f);Assert.Greater(confident.Suppression,far);
            // Compare the same proximity and personality against Sprint020.2 tuning.
            float previousClose=28*Mathf.Lerp(.4f,1,1-.1f/1.25f)*.75f;
            Assert.Greater(confident.Suppression,previousClose*1.4f*1.125f);Assert.Less(confident.Suppression,previousClose*1.5f*1.125f);
            confident.ResetEncounter(null);Near(confident,1.25f);Assert.AreEqual(28*.4f*.75f*1.125f,confident.Suppression,.01f,"Sprint021 scales all distances once by 12.5%.");
            for(int i=0;i<20;i++)Near(coward);Assert.AreEqual(100,coward.Suppression,.001f);
            yield return new WaitForSeconds(1);Assert.Less(coward.Suppression,100);yield return new WaitForSeconds(6);Assert.AreEqual(0,coward.Suppression);
            Near(coward);coward.gameObject.SetActive(false);coward.gameObject.SetActive(true);Assert.AreEqual(0,coward.Suppression);
        }
        [UnityTest] public IEnumerator SustainedMissesAloneCanRetreatThenResumeWithoutReloadOrMoraleWave()
        {
            var e=Enemy();Float(e,"suppressionRetreatChance",1);yield return new WaitForSeconds(.2f);float hp=e.GetComponent<Health>().Current;
            for(int i=0;i<5;i++)Near(e);Assert.AreEqual(EngagementState.Withdrawing,e.State);Assert.Greater(e.RetreatDestination.x,e.transform.position.x);Assert.AreEqual(hp,e.GetComponent<Health>().Current);Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().SocialReactions);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().CollapseReactions);
            var destination=e.RetreatDestination;for(int i=0;i<30;i++)Near(e);Assert.AreEqual(destination,e.RetreatDestination);
            yield return new WaitForSeconds(4.8f);Assert.AreNotEqual(EngagementState.Withdrawing,e.State);Assert.IsFalse(e.Weapon.Cycle.IsReloading);Assert.AreEqual(27,e.Weapon.MagazineAmmo);
            Near(e);Assert.AreNotEqual(EngagementState.Withdrawing,e.State,"The same burst cannot restart retreat during its six-second decision cooldown.");
        }
        [UnityTest] public IEnumerator FailedHighRollConsumesCooldownAndGroundHoldersKeepShooting()
        {
            var e=Enemy();e.BindTarget(player.GetComponent<Health>());yield return new WaitForSeconds(.5f);for(int i=0;i<5;i++)Near(e);Assert.AreNotEqual(EngagementState.Withdrawing,e.State);
            Float(e,"suppressionRetreatChance",1);Near(e);Assert.AreNotEqual(EngagementState.Withdrawing,e.State);
            int shots=e.GetComponent<AttackExecutor>().ShotsFired;yield return new WaitForSeconds(.8f);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,shots);
            e.ResetEncounter(null);Assert.AreEqual(0,e.Suppression);
        }
        [UnityTest] public IEnumerator SelfFriendlyZeroLengthAndDistantShotsDoNotAccumulate()
        {
            var e=Enemy();yield return new WaitForSeconds(.2f);var p=(Vector2)e.transform.position;
            e.Hear(new CombatNoiseEvent(e.gameObject,CombatTeam.Player,CombatNoiseKind.Gunfire,p-Vector2.right,p+Vector2.right,1));
            e.Hear(new CombatNoiseEvent(player,CombatTeam.Enemy,CombatNoiseKind.Gunfire,p-Vector2.right,p+Vector2.right,1));
            e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,p,p,1));Near(e,2);Assert.AreEqual(0,e.Suppression);
        }
    }
}
#endif
