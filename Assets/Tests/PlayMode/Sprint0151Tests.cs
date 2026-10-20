#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.CameraSystem;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0151Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private GameObject player;private PlayerActions actions;private Camera camera;private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private GameObject Solid(string name,Vector2 position,Vector2 size){var go=Keep(new GameObject(name));go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        private void Float(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;Solid("S151Floor",new Vector2(8500,-.5f),new Vector2(120,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(8500,.85f),Quaternion.identity));actions=player.GetComponent<PlayerActions>();player.GetComponent<Health>().ProtectFor(60);
            camera=Keep(new GameObject("S151Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=4;camera.aspect=1;camera.transform.position=new Vector3(8500,2,-10);yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;}
        private EnemyCombat Enemy(float x=8518,float y=.85f,bool freeze=true)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,y),Quaternion.identity));go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            if(freeze)go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);e.BindTarget(player.GetComponent<Health>());return e;
        }
        private void Noise(CombatNoiseKind kind,Vector2 position,float intensity=1)=>CombatNoise.Report(new CombatNoiseEvent(player,CombatTeam.Player,kind,position,position,intensity));
        [UnityTest] public IEnumerator StagedLarperSeesAtMediumRangeAndElevationWithoutDamageOrCameraPermission()
        {
            var e=Enemy();e.SetEncounterEnabled(false);yield return new WaitForSeconds(.5f);Assert.IsTrue(e.TargetVisible);Assert.AreEqual(ThreatAwareness.Alerted,e.Awareness);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);Assert.AreEqual(50,e.GetComponent<Health>().Current);
            var high=Enemy(8516,5);high.SetEncounterEnabled(false);yield return new WaitForSeconds(.25f);Assert.IsTrue(high.TargetVisible);Assert.IsFalse(high.InGameplayViewport);Assert.AreEqual(0,high.GetComponent<AttackExecutor>().ShotsFired);
            Assert.AreEqual(22,new SerializedObject(e).FindProperty("detectionRange").floatValue);Assert.AreEqual(18,new SerializedObject(e).FindProperty("engagementRange").floatValue);
        }
        [UnityTest] public IEnumerator SightIgnoresOwnColliderButWallsStillBlockConfirmedKnowledge()
        {
            var e=Enemy(8516);e.gameObject.layer=0;yield return new WaitForSeconds(.25f);Assert.IsTrue(e.TargetVisible);var known=e.LastKnownPosition;
            Solid("S151Wall",new Vector2(8508,3),new Vector2(.3f,10));player.GetComponent<Rigidbody2D>().position=new Vector2(8499,2);Physics2D.SyncTransforms();yield return new WaitForSeconds(.2f);Assert.IsFalse(e.TargetVisible);Assert.AreEqual(known,e.LastKnownPosition);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator ObstructedRunningBehindEnemyIsHeardButCrouchAndDistantNoiseAreWeaker()
        {
            var e=Enemy(8510);Solid("S151HearingWall",new Vector2(8508,2),new Vector2(.3f,6));e.SetEncounterEnabled(false);yield return new WaitForSeconds(.2f);Assert.IsFalse(e.TargetVisible);
            Noise(CombatNoiseKind.Movement,new Vector2(8507.3f,.8f),.25f);Assert.AreEqual(ThreatAwareness.Unaware,e.Awareness);
            Noise(CombatNoiseKind.Movement,new Vector2(8507.3f,.8f));Assert.AreEqual(ThreatAwareness.Suspicious,e.Awareness);var approximate=e.HeardPosition;Assert.AreNotEqual(new Vector2(8507.3f,.8f),approximate);Assert.AreEqual(Vector2.zero,e.LastKnownPosition);
            Noise(CombatNoiseKind.Gunfire,new Vector2(8400,0));Assert.AreEqual(approximate,e.HeardPosition);
            player.GetComponent<Rigidbody2D>().position=new Vector2(8498,.85f);Physics2D.SyncTransforms();yield return new WaitForSeconds(4.3f);Assert.AreEqual(ThreatAwareness.Unaware,e.Awareness);Assert.AreEqual(approximate,e.HeardPosition);
        }
        [UnityTest] public IEnumerator ObstructedGunfireCreatesTemporarySuspicionWithoutOffscreenShooting()
        {
            var e=Enemy(8510);Solid("S151GunWall",new Vector2(8505,2),new Vector2(.3f,6));yield return new WaitForSeconds(.2f);
            Noise(CombatNoiseKind.Gunfire,new Vector2(8500.3f,.8f));Assert.AreEqual(ThreatAwareness.Suspicious,e.Awareness);Assert.IsFalse(e.TargetVisible);var heard=e.HeardPosition;
            Noise(CombatNoiseKind.Gunfire,new Vector2(8502.9f,.8f));Assert.AreEqual(heard,e.HeardPosition,"Automatic-fire reports are deduplicated.");yield return new WaitForSeconds(.4f);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);Assert.AreEqual(Vector2.zero,e.LastKnownPosition);
        }
        [UnityTest] public IEnumerator RealHitscanNearMissAlertsWithoutDamageOrPhysicalTracerSearch()
        {
            var e=Enemy(8508);e.BindTarget(null);var attack=player.GetComponent<AttackExecutor>();var weapon=player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>().SelectedWeapon;float health=e.GetComponent<Health>().Current;
            Vector2 aim=(new Vector2(8508,1.75f)-attack.Origin).normalized;Assert.IsTrue(attack.Attack(weapon,aim,Time.time));Assert.AreEqual(health,e.GetComponent<Health>().Current);Assert.AreEqual(ThreatAwareness.UnderFire,e.Awareness);Assert.AreEqual(Vector2.zero,e.LastKnownPosition);Assert.Greater(attack.LastShotEnd.x,8508);yield return new WaitForSeconds(.2f);Assert.AreEqual(0,e.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator LogicalFootstepsExistWithoutClipsAndRunningIsLouderThanCrouching()
        {
            var events=new List<float>();System.Action<CombatNoiseEvent> record=n=>{if(n.Source==player&&n.Kind==CombatNoiseKind.Movement)events.Add(n.Intensity);};CombatNoise.Reported+=record;
            try{
                actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.8f);Assert.Greater(events.Count,0);float running=events.Max();events.Clear();
                actions.SetTouchMove(new Vector2(.5f,0));yield return new WaitForSeconds(.8f);Assert.Greater(events.Count,0);float walking=events.Min();events.Clear();
                actions.SetTouchMove(new Vector2(.7f,-.7f));yield return new WaitForSeconds(.8f);Assert.Greater(events.Count,0);Assert.Less(events.Max(),walking);Assert.Less(walking,running);
            }finally{CombatNoise.Reported-=record;}
        }
        [UnityTest] public IEnumerator SeveralLarpersReserveDistinctStablePositionsAndReleaseOnStateChanges()
        {
            camera.transform.position=new Vector3(8508,2,-10);camera.orthographicSize=20;
            var actors=new[]{Enemy(8510),Enemy(8512),Enemy(8514)};yield return new WaitForSeconds(.55f);foreach(var e in actors)Assert.IsTrue(e.HasCombatPosition);
            var positions=actors.Select(e=>e.CombatPosition).ToArray();for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)Assert.GreaterOrEqual(Vector2.Distance(positions[i],positions[j]),1.249f);
            yield return new WaitForSeconds(.3f);CollectionAssert.AreEqual(positions,actors.Select(e=>e.CombatPosition).ToArray());
            actors[0].PanicFor(1);Assert.IsFalse(actors[0].HasCombatPosition);actors[1].gameObject.SetActive(false);Assert.IsFalse(actors[1].HasCombatPosition);actors[2].GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));Assert.IsFalse(actors[2].HasCombatPosition);
            Object.Destroy(actors[1].gameObject);var replacement=Enemy(8512);yield return new WaitForSeconds(.3f);Assert.IsTrue(replacement.HasCombatPosition);
        }
        [UnityTest] public IEnumerator MovingLarpersSpreadAndBlockedPositionsDoNotCauseClipping()
        {
            camera.transform.position=new Vector3(8507,2,-10);camera.orthographicSize=20;var a=Enemy(8510,.85f,false);var b=Enemy(8513,.85f,false);yield return new WaitForSeconds(1.5f);
            Assert.GreaterOrEqual(Mathf.Abs(a.transform.position.x-b.transform.position.x),1.1f);Assert.IsTrue(a.HasCombatPosition);Assert.IsTrue(b.HasCombatPosition);
            Solid("S151BlockedDestination",new Vector2(8503,1),new Vector2(1,3));yield return new WaitForSeconds(.4f);Assert.Greater(a.transform.position.x,8503.8f);Assert.Greater(b.transform.position.x,8503.8f);
        }
        private WarehouseCameraFollow Follow()
        {
            var follow=camera.gameObject.AddComponent<WarehouseCameraFollow>();var s=new SerializedObject(follow);s.FindProperty("levelMin").vector2Value=new Vector2(8440,-9);s.FindProperty("levelMax").vector2Value=new Vector2(8560,20);s.FindProperty("orthographicSize").floatValue=4;s.ApplyModifiedPropertiesWithoutUndo();follow.Bind(player.transform);return follow;
        }
        [UnityTest] public IEnumerator FastReversalsSettleToTheTargetWithoutRoundedFeedbackOrResidualDrift()
        {
            var follow=Follow();Assert.AreEqual(RigidbodyInterpolation2D.Interpolate,player.GetComponent<Rigidbody2D>().interpolation);
            actions.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.8f);actions.SetTouchMove(Vector2.left);yield return new WaitForSeconds(.45f);actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(1.4f);
            Assert.AreEqual(player.transform.position.x,camera.transform.position.x,.025f);float x=camera.transform.position.x;yield return new WaitForSeconds(.35f);Assert.AreEqual(x,camera.transform.position.x,.001f);Assert.AreSame(player.transform,follow.Target);Assert.AreEqual(-10,camera.transform.position.z);
        }
        [UnityTest] public IEnumerator LadderClimbMidwayStopAndExitKeepOneStableCameraTarget()
        {
            var follow=Follow();var ladder=Solid("S151Ladder",new Vector2(8500,5),new Vector2(2,12));ladder.GetComponent<BoxCollider2D>().isTrigger=true;ladder.AddComponent<LadderVolume>();Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.8f);Assert.IsTrue(player.GetComponent<PlayerTraversal2D>().IsClimbing);actions.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(1.4f);
            float y=camera.transform.position.y;Assert.AreEqual(player.transform.position.y+1.2f,y,.025f);yield return new WaitForSeconds(.3f);Assert.AreEqual(y,camera.transform.position.y,.001f);Assert.AreSame(player.transform,follow.Target);
            actions.Jump();yield return new WaitForSeconds(.3f);Assert.IsFalse(player.GetComponent<PlayerTraversal2D>().IsClimbing);Assert.AreSame(player.transform,follow.Target);Assert.AreEqual(-10,camera.transform.position.z);
        }
    }
}
#endif
