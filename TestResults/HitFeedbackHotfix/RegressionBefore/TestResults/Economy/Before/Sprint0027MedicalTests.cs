#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
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
    public sealed class Sprint0027MedicalTests
    {
        GameSession session;string path;bool background;
        [UnityTearDown]public IEnumerator Cleanup(){yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(session!=null)Object.Destroy(session.gameObject);yield return null;if(path!=null)foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);Application.runInBackground=background;}
        [UnityTest]public IEnumerator PurchasedTwoRevivesConsumeOnePipEachProtectForTwoSecondsThenThirdDefeatEndsAttempt()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;path=Path.Combine(Path.GetTempPath(),"Medical27-"+Guid.NewGuid().ToString("N")+".json");session=new GameObject("MedicalSession").AddComponent<GameSession>();var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");session.Initialize(catalog,path);session.Data.balance=10000;session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");var offer=catalog.offers.Single(x=>x.category==ShopCategory.Faintanyl);Assert.IsTrue(session.Buy(offer.id,0));Assert.IsTrue(session.Buy(offer.id,1));Assert.IsFalse(session.CanBuy(offer.id));Assert.IsTrue(session.PrepareMission());
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;var mission=Object.FindAnyObjectByType<WarehouseMission>();foreach(var point in mission.Encounters.SelectMany(g=>g.Points)){point.Spawned.GetComponent<EnemyCombat>().enabled=false;point.Spawned.enabled=false;point.Spawned.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().SetMoveInput(0);}var hp=mission.Player.GetComponent<Health>();var revive=mission.Player.GetComponent<PlayerRevive>();var root=Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
            Assert.AreEqual(DisplayStyle.Flex,root.Q("revivePip1").resolvedStyle.display);
            for(int i=0;i<2;i++)
            {
                hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy));Assert.IsTrue(revive.TryRevive());Assert.IsFalse(revive.TryRevive());Assert.AreEqual(1-i,revive.Injections);Assert.That(revive.ProtectionRemaining,Is.InRange(1.98f,2f));yield return null;yield return new WaitForEndOfFrame();Assert.AreEqual(DisplayStyle.None,root.Q("revivePip"+(1-i)).resolvedStyle.display);Assert.IsFalse(hp.ApplyDamage(new DamageInfo(1,CombatTeam.Enemy)));yield return new WaitForSeconds(2.1f);Assert.IsTrue(hp.ApplyDamage(new DamageInfo(1,CombatTeam.Enemy)));Assert.AreEqual(99,hp.Current);
            }
            hp.ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy));Assert.IsFalse(revive.CanOfferRevive);Assert.IsTrue(mission.AttemptFailed);Assert.AreEqual(0,session.Data.faintanyl);

        }
    }
}
#endif
