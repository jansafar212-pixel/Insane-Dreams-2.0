using System.Linq;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0029EquipmentRulesTests
    {
        [Test]public void CatalogHasExactMedicalCopyAndFourProtectionChoicesWithBoundedTradeoffs()
        {
            var c=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");var medical=c.offers.Where(o=>o.category==ShopCategory.Faintanyl||o.category==ShopCategory.DuctTape).ToArray();Assert.AreEqual(2,medical.Length);Assert.AreEqual("There is nothing duct tape can't fix.",medical.Single(o=>o.title=="Duct Tape").description);Assert.AreEqual("Relax, it's not addictive. It's just habit forming.",medical.Single(o=>o.title=="Faintanyl").description);Assert.AreEqual(4,c.offers.Count(o=>o.category==ShopCategory.BodyArmor));
            var none=new GameProgress();Assert.AreEqual(1,c.ReloadMultiplier(none));Assert.AreEqual(432,c.ReserveCapacity(none));Assert.AreEqual(5,c.GrenadeCapacity(none));var heavy=new GameProgress{bodyArmorTier=2,helmetTier=2};Assert.AreEqual(378,c.ReserveCapacity(heavy));Assert.AreEqual(4,c.GrenadeCapacity(heavy));Assert.AreEqual(1.274f,c.ReloadMultiplier(heavy),.001f);Assert.Greater(c.heavyBody.capacity,c.lightBody.capacity);Assert.Greater(c.heavyHelmet.capacity,c.lightHelmet.capacity);
            foreach(string name in new[]{"LightHelmet","HeavyHelmet"}){var s=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Equipment0029/"+name+".asset");Assert.NotNull(s);Assert.AreEqual(32,s.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,s.texture.filterMode);Assert.AreEqual(1,s.texture.mipmapCount);}
        }
        [Test]public void BallisticHeadAndBodyProtectionAreIndependentAndMeleeExplosionsBypassBoth()
        {
            var go=new GameObject("ArmorRules");try{var box=go.AddComponent<BoxCollider2D>();box.size=new Vector2(.8f,1.4f);var armor=go.AddComponent<BodyArmor>();typeof(BodyArmor).GetMethod("Awake",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(armor,null);Physics2D.SyncTransforms();armor.Restore(60);armor.RestoreHelmet(20);var head=new PhysicalImpact(new Vector2(0,.68f),Vector2.right,0);
                Assert.AreEqual(22,armor.Absorb(new DamageInfo(40,CombatTeam.Enemy,null,head)),.01f);Assert.AreEqual(60,armor.Condition);Assert.AreEqual(2,armor.HelmetCondition,.01f);Assert.AreEqual(14,armor.Absorb(new DamageInfo(40,CombatTeam.Enemy)),.01f);Assert.AreEqual(34,armor.Condition,.01f);
                foreach(var kind in new[]{DamageKind.Melee,DamageKind.Explosion,DamageKind.Environment})Assert.AreEqual(40,armor.Absorb(new DamageInfo(40,CombatTeam.Enemy,null,head,false,kind)));Assert.AreEqual(34,armor.Condition,.01f);Assert.AreEqual(2,armor.HelmetCondition,.01f);
            }finally{Object.DestroyImmediate(go);}
        }
        [Test]public void PlayerReloadCostsDoNotMutateDefinitionsOrEnemyReloadsAndAmmoIsConserved()
        {
            var definition=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");string before=JsonUtility.ToJson(definition);var player=new RuntimeWeapon(definition);var enemy=new RuntimeWeapon(definition);player.SetReloadMultiplier(1.274f);player.SetAmmunition(2,10);Assert.IsTrue(player.Cycle.TryReload(0));Assert.AreEqual(definition.ReloadDuration*1.274f,player.Cycle.ReloadEndsAt,.001f);player.Cycle.Tick(definition.ReloadDuration);Assert.AreEqual(2,player.MagazineAmmo);player.Cycle.Tick(player.Cycle.ReloadEndsAt+.01f);Assert.AreEqual(12,player.MagazineAmmo);Assert.AreEqual(0,player.ReserveAmmo);Assert.AreEqual(1,enemy.ReloadMultiplier);Assert.AreEqual(before,JsonUtility.ToJson(definition));
        }
        [Test]public void LegacyProgressPreservesVestWithoutGrantingRevivesOrBreakingFutureSaveFormat()
        {
            var data=JsonUtility.FromJson<GameProgress>("{\"version\":1,\"armor\":42,\"balance\":73}");data.Normalize();Assert.AreEqual(1,data.bodyArmorTier);Assert.AreEqual(42,data.armor);Assert.AreEqual(0,data.faintanyl);Assert.AreEqual(0,data.helmetTier);Assert.AreEqual(73,data.balance);
            var c=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"));try{c.heavyBody.reservePenalty=9999;c.heavyBody.grenadePenalty=9999;data.bodyArmorTier=2;Assert.AreEqual(144,c.ReserveCapacity(data));Assert.AreEqual(3,c.GrenadeCapacity(data));c.ammoCapacity=50;Assert.AreEqual(50,c.ReserveCapacity(data));}finally{Object.DestroyImmediate(c);}
        }
        [Test]public void BunkerSolidForegroundSurfacesMatchTheirColliders()
        {
            var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity",UnityEditor.SceneManagement.OpenSceneMode.Additive);try{var roots=scene.GetRootGameObjects();foreach(string name in new[]{"ConcreteFrontLower","ConcreteFrontUpper","ConcreteRearWall","ConcreteRoof","ConcreteBase"}){var c=roots.SelectMany(g=>g.GetComponentsInChildren<BoxCollider2D>()).Single(c=>c.name==name);var r=c.GetComponent<SpriteRenderer>();Assert.IsFalse(c.isTrigger);Assert.AreEqual(0,c.gameObject.layer);Assert.IsNull(c.attachedRigidbody);Assert.GreaterOrEqual(r.sortingOrder,20);Assert.Less(Vector2.Distance(c.bounds.center,r.bounds.center),.001f,name);Assert.Less(Vector2.Distance(c.bounds.size,r.bounds.size),.001f,name);}}finally{UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene,true);}
        }
    }
}
