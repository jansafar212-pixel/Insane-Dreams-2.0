#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Hotfix00253Tests
    {
        private bool background;private UnityEngine.Random.State random;private readonly List<GameObject> owned=new List<GameObject>();
        private GameSession session;private string save;private float volume;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=UnityEngine.Random.state;UnityEngine.Random.InitState(253);volume=AudioListener.volume;
            Assert.IsNull(GameSession.Instance);save=Path.Combine(Path.GetTempPath(),"Akumu253-"+Guid.NewGuid().ToString("N")+".json");
            session=new GameObject("H253Session").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();
            if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="Warehouse_Demo"||UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="MainMenu")
                yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            if(session!=null)Object.Destroy(session.gameObject);yield return null;
            foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);
            Application.runInBackground=background;UnityEngine.Random.state=random;AudioListener.volume=volume;Time.timeScale=1;
        }
        private void Float(Object target,string name,float value){var s=new SerializedObject(target);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private void Place(PlayerActions p,Vector2 position){p.SetTouchMove(Vector2.zero);var b=p.GetComponent<Rigidbody2D>();b.position=position;b.linearVelocity=Vector2.zero;p.transform.position=position;Physics2D.SyncTransforms();}
        [UnityTest]public IEnumerator RealCheckpointContactExplainsLocksAndLowerRouteCompletesAllObjectivesResultsAndSave()
        {
            session.Navigate(FlowScreen.Levels);Assert.IsTrue(session.SelectLevel("warehouse"));Assert.IsTrue(session.PrepareMission());
            yield return UnityEditor.SceneManagement.EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));yield return null;
            var m=Object.FindAnyObjectByType<WarehouseMission>();var p=m.Player;p.GetComponent<Health>().ProtectFor(120);
            var zones=Object.FindObjectsByType<MissionTrigger>(FindObjectsSortMode.None);
            var processing=Array.Find(zones,z=>z.Zone==MissionZone.Processing);
            Assert.IsTrue(processing.Region.OverlapPoint(new Vector2(7.5f,.71f)));Assert.IsTrue(processing.Region.OverlapPoint(new Vector2(7.5f,18)),"A high double jump must not skip the progression boundary");
            void Enter(MissionZone kind){var z=Array.Find(zones,x=>x.Zone==kind);Place(p,z.Region.bounds.center);Assert.IsTrue(m.EnterZone(kind,z.Region));}
            void Defeat(int index){foreach(var point in m.Encounters[index].Points)point.Spawned.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player,p.gameObject));}
            Enter(MissionZone.Entrance);Place(p,new Vector2(1.7f,6.11f));yield return new WaitForSeconds(.15f);
            Assert.AreEqual(0,m.CheckpointCommits);StringAssert.Contains("CHECKPOINT LOCKED",m.Notification);StringAssert.Contains("enemies remain",m.Notification);
            Defeat(0);Enter(MissionZone.Storage);yield return new WaitForSeconds(.2f);
            var fleeing=Array.Find(m.Encounters[1].Points,x=>x.name=="Sprint0242_Storage_Mixed").Spawned.GetComponent<EnemyCombat>();
            Float(fleeing,"panicEscapeChance",1);UnityEngine.Random.InitState(1);
            Assert.IsTrue((bool)typeof(EnemyCombat).GetMethod("TryPanicEscape",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(fleeing,new object[]{100f,(Vector2)p.transform.position}));
            float escapeEnd=Time.time+4;while(fleeing.IsPanicEscaping&&Time.time<escapeEnd)yield return new WaitForFixedUpdate();
            Assert.AreEqual(1,fleeing.PanicEscapes,"position="+fleeing.transform.position+" dest="+fleeing.RetreatDestination+" state="+fleeing.State+" jams="+fleeing.Malfunctions+" brain="+fleeing.GetComponent<LarperBrain>().State);Assert.IsTrue(fleeing.gameObject.activeInHierarchy);Assert.IsFalse(fleeing.GetComponent<Health>().IsDefeated);Assert.AreEqual(0,m.Encounters[1].DefeatedCount,"Retreat must not remove mission targets");
            Defeat(1);Assert.AreEqual(MissionStage.ReachProcessing,m.Stage);
            Place(p,new Vector2(1.7f,6.11f));yield return new WaitForSeconds(.15f);Assert.AreEqual(1,m.CheckpointCommits);Assert.IsTrue(Array.Find(zones,z=>z.Zone==MissionZone.Checkpoint).GetComponent<InsaneDreams.Checkpoints.CheckpointMarker>().IsLit);
            // Actual physics contact via the lower mezzanine route, not EnterZone(Processing).
            Place(p,new Vector2(7.5f,6.11f));yield return new WaitForSeconds(.15f);Assert.AreEqual(MissionStage.ClearProcessing,m.Stage);
            Defeat(2);Assert.AreEqual(MissionStage.RecoverRecords,m.Stage);Place(p,new Vector2(15.1f,6.11f));yield return new WaitForSeconds(.15f);Assert.AreEqual(2,m.CheckpointCommits);
            Place(p,new Vector2(20,6.11f));yield return new WaitForSeconds(.15f);Assert.IsTrue(m.CanInteract);p.RequestInteraction();Assert.IsTrue(m.HasRecords);Assert.AreEqual(MissionStage.Escape,m.Stage);
            p.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn();Assert.AreEqual(MissionStage.Escape,m.Stage);Assert.IsTrue(m.HasRecords);
            Assert.IsTrue(p.GetComponent<PlayerSupplies>().CollectWallet("h253-flow",37));int before=session.Data.balance;
            Place(p,new Vector2(23.2f,.71f));p.SetTouchMove(Vector2.right);float deadline=Time.time+4;
            while(Time.time<deadline&&m.Stage!=MissionStage.Complete)yield return new WaitForFixedUpdate();
            Assert.AreEqual(MissionStage.Complete,m.Stage);Assert.AreEqual(1,m.Completions);int expected=before+m.Earnings;Assert.AreEqual(expected,session.Data.balance);
            var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            Assert.IsFalse(root.Q("missionResults").ClassListContains("hidden"));CollectionAssert.Contains(session.Data.completed,"warehouse");
            var stored=new ProgressStore(save).Load(()=>new GameProgress());Assert.AreEqual(expected,stored.balance);CollectionAssert.Contains(stored.completed,"warehouse");
            m.GetComponent<CampaignMissionBridge>().ReturnToLevels();deadline=Time.realtimeSinceStartup+6;
            while(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="MainMenu"&&Time.realtimeSinceStartup<deadline)yield return null;
            Assert.AreEqual("MainMenu",UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);Assert.AreEqual(FlowScreen.Levels,session.Screen);
        }
        [UnityTest]public IEnumerator EnemyPistolUsesRealClockArticulatedHandsPhysicsAndAtomicAmmo()
        {
            var floor=Keep(new GameObject("H253Floor"));floor.transform.position=new Vector2(6000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);
            // Elevated frozen fixture leaves room for an observable gravity drop;
            // on the ground the long magazine can already touch the floor at release.
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(6000,1.71f),Quaternion.identity));
            g.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));g.GetComponent<LarperBrain>().enabled=false;var e=g.GetComponent<EnemyCombat>();e.enabled=false;
            g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var h=g.GetComponent<PistolMagazineReload>();Assert.IsNotNull(h);e.Weapon.SetAmmunition(3,27);Assert.IsTrue(e.Weapon.Cycle.TryReload(Time.time));
            var rig=g.GetComponent<CharacterVisualDriver>().Rig;
            yield return new WaitForSeconds(e.Weapon.Definition.ReloadDuration*.3f);
            Vector3 hand=rig.Segmented.leftHand.TransformPoint(Vector3.down*rig.Segmented.hand.bounds.size.y*.55f);
            Assert.Less(Vector3.Distance(hand,rig.magazine.transform.position),.055f,"Articulated removal hand");
            Assert.Less(Vector3.Distance(rig.weaponMount.position,rig.Segmented.grip.position),.01f);
            yield return new WaitForSeconds(e.Weapon.Definition.ReloadDuration*.14f);Assert.AreEqual(1,h.Releases);float y=h.LastReleased.position.y;
            yield return new WaitForSeconds(.15f);Assert.Less(h.LastReleased.position.y,y);Assert.AreEqual(3,e.Weapon.MagazineAmmo);
            yield return new WaitForSeconds(.6f);
            hand=rig.Segmented.leftHand.TransformPoint(Vector3.down*rig.Segmented.hand.bounds.size.y*.55f);
            Assert.Less(Vector3.Distance(hand,rig.weapon.transform.Find("PistolReplacementMagazine").position),.055f,"Articulated insertion hand");
            e.Weapon.Cycle.Tick(e.Weapon.Cycle.ReloadEndsAt);yield return null;Assert.AreEqual(27,e.Weapon.MagazineAmmo);Assert.AreEqual(3,e.Weapon.ReserveAmmo);Assert.IsFalse(e.Weapon.Cycle.IsReloading);Assert.IsTrue(rig.magazine.enabled);
        }
        [UnityTest]public IEnumerator FireworkSequenceIsBoundedReusableAndDamageIsAppliedOnlyOnce()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab")));var b=g.GetComponent<FireworkProjectile>();
            var victim=Keep(new GameObject("H253Victim"));victim.transform.position=new Vector2(6100,20);victim.layer=2;victim.AddComponent<BoxCollider2D>();var hp=victim.AddComponent<Health>();Physics2D.SyncTransforms();
            b.Prepare(null,victim.transform.position,Color.magenta,true);b.ExplodeHeld(victim.transform.position);Assert.AreEqual(68.8f,hp.Current,.1f);Assert.AreEqual(35,b.PooledVisualCount);
            int renderers=g.GetComponentsInChildren<SpriteRenderer>(true).Length;Assert.IsTrue(g.transform.Find("FireworkFlash").GetComponent<SpriteRenderer>().enabled);
            yield return new WaitForSeconds(.28f);Assert.IsFalse(g.transform.Find("FireworkFlash").GetComponent<SpriteRenderer>().enabled);Assert.IsTrue(g.transform.Find("SecondarySpark0").GetComponent<SpriteRenderer>().enabled);
            b.Detonate();Assert.AreEqual(68.8f,hp.Current,.1f);yield return new WaitForSeconds(.65f);Assert.IsFalse(b.Busy);
            b.Prepare(null,victim.transform.position,Color.cyan,true);Assert.AreEqual(renderers,g.GetComponentsInChildren<SpriteRenderer>(true).Length);
            Assert.IsFalse(g.transform.Find("SecondarySpark0").GetComponent<SpriteRenderer>().enabled);b.Cancel();
        }
        [Test]public void NativeTuningIsSmallAndPreservesOtherRolesAndRecovery()
        {
            var larper=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");var s=new SerializedObject(larper.GetComponent<EnemyCombat>());Assert.AreEqual(63.8f,s.FindProperty("nearMissSuppression").floatValue,.001f);Assert.AreEqual(12,s.FindProperty("suppressionDecayPerSecond").floatValue);
            var expert=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab");s=new SerializedObject(expert.GetComponent<FireworksCombat>());Assert.AreEqual(1.05f,s.FindProperty("recoverySeconds").floatValue);Assert.AreEqual(.85f,s.FindProperty("preparationSeconds").floatValue);
            var samurai=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");Assert.IsNull(samurai.GetComponent<PistolMagazineReload>());
        }
    }
}
#endif

