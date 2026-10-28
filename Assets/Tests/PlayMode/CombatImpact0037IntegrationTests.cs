#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class CombatImpact0037IntegrationTests
    {
        GameSession session;string path;bool background;float volume;WarehouseMission mission;PlayerActions player;Health health;PlayerDamageFeedback feedback;VisualElement root;
        [UnitySetUp]public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            path=Path.Combine(Path.GetTempPath(),"AkumuImpact37-"+Guid.NewGuid()+".json");session=new GameObject("Impact37Session").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");Assert.IsTrue(session.PrepareMission());
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            mission=Object.FindAnyObjectByType<WarehouseMission>();player=mission.Player;health=player.GetComponent<Health>();feedback=player.GetComponent<PlayerDamageFeedback>();root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            // Remove only the Editor diagnostic strip from controlled captures;
            // it is absent from release and otherwise occludes the above marker.
            foreach(var strip in Object.FindObjectsByType<InsaneDreams.CameraSystem.DevelopmentFrameOverlay>(FindObjectsSortMode.None))Object.Destroy(strip);
            var s=new SerializedObject(player);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();
            foreach(var e in Object.FindObjectsByType<InsaneDreams.Enemies.EnemyCombat>(FindObjectsSortMode.None)){e.enabled=false;e.GetComponent<InsaneDreams.Enemies.LarperBrain>().enabled=false;e.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {
            Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(session!=null)Object.Destroy(session.gameObject);yield return null;Application.runInBackground=background;AudioListener.volume=volume;foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);
        }
        void Damage(Vector2 direction,float amount=2,Vector2? point=null){health.ProtectFor(0);Assert.IsTrue(health.ApplyDamage(new DamageInfo(amount,CombatTeam.Enemy,null,new PhysicalImpact(point??(Vector2)player.transform.position,direction,1,null,1))));}
        IEnumerator Capture(string name){yield return new WaitForEndOfFrame();Directory.CreateDirectory("TestResults/Sprint0037");ScreenCapture.CaptureScreenshot(Path.GetFullPath("TestResults/Sprint0037/"+name+".png"));yield return new WaitForEndOfFrame();}
        [UnityTest]public IEnumerator ActualHudShowsFourDirectionsCriticalHealthAndPreservesTransparentControlsAndPause()
        {
            var armor=player.GetComponent<BodyArmor>();armor.Restore(100,100,.65f);armor.RestoreHelmet(20,20,.45f);
            var box=player.GetComponent<Collider2D>();Damage(Vector2.right,20,new Vector2(box.bounds.center.x,box.bounds.max.y-.02f));Assert.AreEqual(89,health.Current,.01f);Assert.AreEqual(11,armor.HelmetCondition,.01f);Assert.AreEqual(100,armor.Condition,.01f);
            health.Restore();armor.Restore(0);armor.RestoreHelmet(0);feedback.Clear();Rect touch=root.Q("aimStick").worldBound;
            var directions=new[]{Vector2.right,Vector2.left,Vector2.down,Vector2.up};var names=new[]{"DamageLeft","DamageRight","DamageAbove","DamageBelow"};
            for(int i=0;i<4;i++)
            {
                feedback.Clear();Damage(directions[i]);yield return null;yield return null;Assert.AreEqual(i,feedback.LastDirection);Assert.AreEqual(8,root.Q("damageFeedbackOverlay").childCount);Assert.AreEqual(PickingMode.Ignore,root.Q("damageDirection"+i).pickingMode);Assert.IsTrue(root.Q("healthFill").ClassListContains("damage-flash"));Assert.AreEqual(1,root.Q<Label>("healthValue").resolvedStyle.color.a,.01f);Assert.AreEqual(.4f,root.Q("aimStick").resolvedStyle.backgroundColor.a,.01f);Assert.AreEqual(touch,root.Q("aimStick").worldBound);yield return Capture(names[i]);
            }
            Damage(Vector2.right,70);yield return null;yield return null;Assert.IsTrue(root.Q("healthFill").ClassListContains("critical"));yield return Capture("HeavyCriticalDamage");yield return new WaitForSeconds(.16f);Assert.AreEqual(.85f,root.Q("healthFill").resolvedStyle.backgroundColor.a,.01f);
            var bridge=mission.GetComponent<CampaignMissionBridge>();bridge.SetPaused(true);yield return null;Assert.AreEqual(DisplayStyle.None,root.Q("damageFeedbackOverlay").resolvedStyle.display);bridge.SetPaused(false);Assert.IsTrue(player.enabled);
        }
        [UnityTest]public IEnumerator ReviveProtectionProducesNoSuccessfulHitFeedbackAndWeaponTracersStayDistinct()
        {
            var revive=player.GetComponent<PlayerRevive>();revive.RestoreInventory(1);Damage(Vector2.right,1000);Assert.IsTrue(health.IsDefeated);Assert.IsTrue(revive.TryRevive());uint revision=feedback.Revision;
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(10,CombatTeam.Enemy,null,new PhysicalImpact(player.transform.position,Vector2.left,1))));Assert.AreEqual(revision,feedback.Revision);Assert.AreEqual(0,feedback.EdgeAlpha);Assert.IsFalse(player.GetComponent<HitReaction2D>().KnockedDown);Assert.IsTrue(player.enabled);
            var catalog=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");var load=player.GetComponent<PlayerLoadout>();
            foreach(var index in new[]{0,1,6})
            {
                var w=new RuntimeWeapon(catalog.weapons[index]);load.Equip(EquipmentSlot.Firearm1,w);load.Select(EquipmentSlot.Firearm1);yield return new WaitForSeconds(.25f);player.SetTouchAim(new Vector2(1,.2f));yield return null;Assert.IsTrue(player.GetComponent<AttackExecutor>().Attack(w,new Vector2(1,.2f),Time.time));yield return null;yield return Capture("Tracer"+w.Definition.name);player.SetTouchAim(Vector2.zero);
            }
        }
    }
}
#endif
