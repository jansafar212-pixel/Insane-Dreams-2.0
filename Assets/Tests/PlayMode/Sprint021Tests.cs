#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class Sprint021Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private GameObject player;private Camera camera;private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object o,string name,float value){var s=new SerializedObject(o);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private void Private(Object o,string name,object value)=>o.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(o,value);
        private GameObject Solid(Vector2 p,Vector2 size){var go=Keep(new GameObject("S021Geometry"));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            random=Random.state;Random.InitState(21021);background=Application.runInBackground;Application.runInBackground=true;
            Solid(new Vector2(14000,-.5f),new Vector2(180,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(13992,.85f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(60);
            camera=Keep(new GameObject("S021Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=12;camera.aspect=2;camera.transform.position=new Vector3(14000,2,-10);yield return new WaitForSeconds(.2f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var o in objects)if(o!=null)Object.Destroy(o);objects.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        private EnemyCombat Enemy(float x=14000,bool freeze=true)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,.71f),Quaternion.identity));go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));if(freeze)go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);Float(e,"defensiveCrouchProbability",0);Float(e,"malfunctionChance",0);return e;
        }
        private void Near(EnemyCombat e)
        {var p=(Vector2)e.GetComponent<BoxCollider2D>().bounds.center+Vector2.up*.9f;e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,p-Vector2.right*8,p+Vector2.right*8,1));}
        [UnityTest] public IEnumerator SuppressionScalesExactlyOnceAtEveryDistanceAndKeepsCooldowns()
        {
            var e=Enemy();yield return new WaitForSeconds(.2f);Float(e,"suppressionRetreatChance",0);Near(e);
            Assert.AreEqual(31.5f*Mathf.Lerp(.4f,1.45f,1-.9f/1.25f),e.Suppression,.01f);
            var s=new SerializedObject(e);Assert.AreEqual(18,s.FindProperty("suppressionDecayPerSecond").floatValue);Assert.AreEqual(6,s.FindProperty("suppressionReactionCooldown").floatValue);
            e.ResetEncounter(null);var p=(Vector2)e.GetComponent<BoxCollider2D>().bounds.center+Vector2.up*1.25f;e.Hear(new CombatNoiseEvent(player,CombatTeam.Player,CombatNoiseKind.Gunfire,p-Vector2.right*8,p+Vector2.right*8,1));Assert.AreEqual(12.6f,e.Suppression,.01f);
        }
        [UnityTest] public IEnumerator RealRetreatReachesCoverHoldsBrieflyThenReengages()
        {
            var crate=Solid(new Vector2(14003,.45f),new Vector2(1.2f,.9f));var e=Enemy(freeze:false);Float(e,"suppressionRetreatChance",1);yield return new WaitForSeconds(.2f);
            for(int i=0;i<4;i++)Near(e);Assert.AreEqual(crate.GetComponent<Collider2D>(),e.SelectedCover);
            for(int i=0;i<300&&!e.IsHoldingCover;i++)yield return new WaitForFixedUpdate();Assert.IsTrue(e.IsHoldingCover,"Must physically reach useful cover rather than just reserve it.");
            e.BindTarget(player.GetComponent<Health>());yield return new WaitForSeconds(.2f);Assert.IsTrue(e.GetComponent<InsaneDreams.Traversal.PlayerPosture2D>().IsCrouched);int shots=e.GetComponent<AttackExecutor>().ShotsFired;
            yield return new WaitForSeconds(.3f);Assert.AreEqual(shots,e.GetComponent<AttackExecutor>().ShotsFired);
            yield return new WaitForSeconds(4);Assert.IsFalse(e.IsHoldingCover);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,shots);Assert.IsFalse(e.Weapon.Cycle.IsReloading);
        }
        [UnityTest] public IEnumerator OccupiedAndHeadExposingCoverAreRejected()
        {
            var crate=Solid(new Vector2(14003,.45f),new Vector2(1.2f,.9f));var e=Enemy();var occupant=Enemy(14004.3f);Float(e,"suppressionRetreatChance",1);yield return new WaitForSeconds(.2f);for(int i=0;i<4;i++)Near(e);Assert.IsNull(e.SelectedCover);
            occupant.gameObject.SetActive(false);Object.Destroy(crate);yield return null;e.ResetEncounter(null);Solid(new Vector2(14003,.275f),new Vector2(1.2f,.55f));for(int i=0;i<4;i++)Near(e);Assert.IsNull(e.SelectedCover,"Center-only concealment is not useful head cover.");
        }
        [UnityTest] public IEnumerator RepositionUsesExistingLeaseAndDoesNotRestartEverySample()
        {
            player.GetComponent<Rigidbody2D>().position=new Vector2(13995,.85f);Physics2D.SyncTransforms();var e=Enemy();e.BindTarget(player.GetComponent<Health>());Float(e,"preferredDistance",5);Float(e,"combatRepositionChance",1);
            var s=new SerializedObject(e);s.FindProperty("combatRepositionSeconds").vector2Value=new Vector2(.2f,.2f);s.ApplyModifiedPropertiesWithoutUndo();yield return new WaitForSeconds(.9f);
            Assert.AreEqual(1,e.TacticalRepositions);Assert.IsTrue(e.HasCombatPosition);Assert.Greater(Mathf.Abs(e.CombatPosition.x-e.transform.position.x),1);
            var held=e.CombatPosition;yield return new WaitForSeconds(.3f);Assert.AreEqual(1,e.TacticalRepositions);Assert.AreEqual(held,e.CombatPosition);
        }
        [UnityTest] public IEnumerator LegitimateMalfunctionConsumesNoAmmoRecoversAndHonorsCooldown()
        {
            var e=Enemy();e.BindTarget(player.GetComponent<Health>());Float(e,"malfunctionChance",1);Float(e,"malfunctionRecoverySeconds",.8f);Private(e,"nextMalfunction",0f);
            for(int i=0;i<80&&e.Malfunctions==0;i++)yield return new WaitForFixedUpdate();Assert.AreEqual(1,e.Malfunctions);Assert.IsTrue(e.Weapon.Cycle.IsMalfunctioning(Time.time));Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(135,e.Weapon.ReserveAmmo);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
            yield return new WaitForSeconds(.3f);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);yield return new WaitForSeconds(.8f);Assert.Greater(e.GetComponent<AttackExecutor>().ShotsFired,0);Assert.AreEqual(1,e.Malfunctions);
            e.ResetEncounter(null);Assert.IsFalse(e.Weapon.Cycle.IsMalfunctioning(Time.time));Assert.AreEqual(27,e.Weapon.MagazineAmmo);
        }
        [UnityTest] public IEnumerator OffscreenOperationsCannotRollMalfunctionOrFire()
        {
            var e=Enemy();e.BindTarget(player.GetComponent<Health>());Float(e,"malfunctionChance",1);Private(e,"nextMalfunction",0f);camera.transform.position=new Vector3(13800,2,-10);
            yield return new WaitForSeconds(1);Assert.IsTrue(e.TargetVisible);Assert.AreEqual(0,e.Malfunctions);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator SpawnWaitsForZoomAndPersistsWithHealthAmmoAndSuppression()
        {
            var go=Keep(new GameObject("S021Spawn"));go.transform.position=new Vector2(14035,.71f);var point=go.AddComponent<EnemySpawnPoint>();var s=new SerializedObject(point);s.FindProperty("archetype").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");s.FindProperty("spawnOnStart").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();point.BindGameplayCamera(camera);
            camera.orthographicSize=20;var actor=point.Spawn();Keep(actor.gameObject);Assert.IsFalse(actor.gameObject.activeSelf);yield return new WaitForSeconds(.25f);Assert.IsTrue(point.IsPending);
            camera.orthographicSize=8;yield return new WaitForSeconds(.3f);Assert.IsTrue(actor.gameObject.activeSelf);var e=actor.GetComponent<EnemyCombat>();Float(e,"suppressionRetreatChance",0);actor.GetComponent<Health>().ApplyDamage(new DamageInfo(5,CombatTeam.Player));e.Weapon.SetAmmunition(19,99);Near(e);float hp=actor.GetComponent<Health>().Current;
            camera.transform.position=new Vector3(14035,2,-10);Assert.AreSame(actor,point.Spawn());yield return new WaitForSeconds(.2f);camera.transform.position=new Vector3(13800,2,-10);yield return new WaitForSeconds(.3f);
            Assert.IsTrue(actor.gameObject.activeSelf);Assert.AreEqual(hp,actor.GetComponent<Health>().Current);Assert.AreEqual(19,e.Weapon.MagazineAmmo);Assert.AreEqual(99,e.Weapon.ReserveAmmo);Assert.Greater(e.Suppression,0);Assert.AreSame(actor,point.Spawn());
        }
        [UnityTest] public IEnumerator ActualTracerAndMaterialImpactFadeWithinExistingPools()
        {
            var wall=Solid(new Vector2(13998,2),new Vector2(.3f,5));wall.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Wood);var attack=player.GetComponent<AttackExecutor>();var feedback=player.GetComponent<CombatFeedback>();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;
            Assert.IsTrue(attack.Attack(weapon,Vector2.right,Time.time));var trails=player.GetComponentsInChildren<LineRenderer>();var line=trails.First(t=>t.enabled);Assert.Less(Vector2.Distance(line.GetPosition(1),attack.LastShotEnd),.001f);Assert.Less(Vector2.Distance(line.GetPosition(0),attack.LastShotOrigin),.001f);
            var surface=player.transform.Find("SurfaceImpact").GetComponent<SpriteRenderer>();float alpha=surface.color.a,tracerAlpha=line.startColor.a;Assert.AreEqual(ImpactMaterial.Wood,feedback.LastImpactMaterial);int capacity=feedback.ImpactParticleCapacity;
            yield return new WaitForSeconds(.06f);Assert.Less(surface.color.a,alpha);Assert.Less(line.startColor.a,tracerAlpha);yield return new WaitForSeconds(.3f);Assert.IsFalse(surface.enabled);Assert.IsFalse(line.enabled);Assert.AreEqual(capacity,feedback.ImpactParticleCapacity);Assert.AreEqual(trails.Length,player.GetComponentsInChildren<LineRenderer>().Length);
        }
    }
}
#endif
