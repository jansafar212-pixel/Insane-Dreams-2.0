using NUnit.Framework;
using UnityEditor;
using InsaneDreams.Inventory;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;
using InsaneDreams.Enemies;
namespace InsaneDreams.Tests
{
    public sealed class CombatReliabilityRulesTests
    {
        [TestCase("LMG",0,200)][TestCase("LMG",37,9)][TestCase("DesertEagle",0,20)][TestCase("DesertEagle",3,1)][TestCase("Sniper",0,20)][TestCase("Sniper",2,1)]
        public void AtomicReloadCommitsOnceWithArmorAndCancellation(string name,int loaded,int reserve)
        {
            var d=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/"+name+".asset");var w=new RuntimeWeapon(d);w.SetAmmunition(loaded,reserve);w.SetReloadMultiplier(1.2f);
            Assert.IsTrue(w.Cycle.TryReload(10));float end=w.Cycle.ReloadEndsAt;Assert.AreEqual(d.ReloadDuration*1.2f,end-10,.001f);
            Assert.IsFalse(w.Cycle.TryReload(10.1f));Assert.AreEqual(end,w.Cycle.ReloadEndsAt);Assert.IsFalse(w.Cycle.TryAttack(end-.01f));Assert.AreEqual(loaded,w.MagazineAmmo);
            w.Cycle.CancelReload();w.Cycle.Tick(99);Assert.AreEqual(loaded,w.MagazineAmmo);Assert.AreEqual(reserve,w.ReserveAmmo);
            w.Cycle.TryReload(100);end=w.Cycle.ReloadEndsAt;w.Cycle.Tick(end);w.Cycle.Tick(end+1);
            Assert.AreEqual(Mathf.Min(d.MagazineCapacity,loaded+reserve),w.MagazineAmmo);Assert.AreEqual(loaded+reserve,w.MagazineAmmo+w.ReserveAmmo);Assert.IsFalse(w.Cycle.IsReloading);
            Assert.IsTrue(w.Cycle.TryAttack(end+2));Assert.AreEqual(loaded+reserve-1,w.MagazineAmmo+w.ReserveAmmo);
            w.SetAmmunition(d.MagazineCapacity,2);Assert.IsFalse(w.Cycle.TryReload(200));w.SetAmmunition(0,0);Assert.IsFalse(w.Cycle.TryReload(200));
        }
        [Test]public void ModelsSelectTheirActualFeedAndPreserveControlAndBalance()
        {
            var l=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/LMG.asset");var d=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/DesertEagle.asset");var s=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/Sniper.asset");
            Assert.AreEqual(100,l.MagazineCapacity);Assert.AreEqual(8,l.ReloadDuration);Assert.IsTrue(l.PhysicalMagazineReload);Assert.AreEqual(new Vector2(6,6),l.MagazineSprite.rect.size);
            Assert.AreEqual(7,d.MagazineCapacity);Assert.AreEqual(2.8f,d.ReloadDuration);Assert.AreEqual(WeaponFireMode.Automatic,d.FireMode);Assert.AreEqual(new Vector2(6,7),d.MagazineSprite.rect.size);
            Assert.AreEqual(5,s.MagazineCapacity);Assert.AreEqual(WeaponFireMode.ReleaseToFire,s.FireMode);Assert.IsNull(s.MagazineSprite,"Original rifle has no detachable magazine model");
        }
        [Test]public void StorageCoverAndEscalationAreLocalAndKeepSolidDecksAndVisibleFireGate()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity",OpenSceneMode.Additive);
            try
            {
                var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();
                var covers=new[]{("StorageEntryVerticalCover",new Vector2(16,.5f)),("StorageLadderRetreatCover",new Vector2(23.5f,3.7f)),("StorageUpperApproachCover",new Vector2(28,6.9f))};
                foreach(var c in covers){var t=all.Single(t=>t.name==c.Item1);Assert.Less(Vector2.Distance(c.Item2,t.position),.001f);var box=t.GetComponent<BoxCollider2D>();Assert.IsNotNull(box);Assert.IsFalse(box.isTrigger);Assert.AreEqual(0,t.gameObject.layer);}
                float previous=0;
                foreach(int i in new[]{0,1,2}){var e=all.Single(t=>t.name=="02_Verticality_Larper_"+i).GetComponent<EnemyCombat>();var s=new SerializedObject(e);Assert.IsFalse(s.FindProperty("allowOffscreenFire").boolValue);float delay=s.FindProperty("viewportAttackDelay").floatValue;Assert.Greater(delay,previous);previous=delay;Assert.GreaterOrEqual(s.FindProperty("detectionRange").floatValue,14);}
                Assert.AreEqual(13,all.Count(t=>t.GetComponent<EnemyCombat>()!=null));
                var arrival=all.Single(t=>t.name=="01_Warmup_Larper_1").GetComponent<EnemyCombat>();
                Assert.AreEqual(10,new SerializedObject(arrival).FindProperty("detectionRange").floatValue);
                foreach(string n in new[]{"StorageLowDeck","StorageHighDeck"})Assert.IsFalse(all.Single(t=>t.name==n).GetComponent<BoxCollider2D>().isTrigger);
            }
            finally{EditorSceneManager.CloseScene(scene,true);}
        }
    }
}
