using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint00271RulesTests
    {
        [Test]public void OfficerArtAndTuningPreserveCombatAndInsertedMagazineFitsOpaqueGrip()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/Officer.prefab");var aim=new SerializedObject(prefab.GetComponent<EnemyCombat>());Assert.AreEqual(1.25f,aim.FindProperty("closeSpread").floatValue);Assert.AreEqual(4,aim.FindProperty("distantSpread").floatValue);Assert.AreEqual(.06f,aim.FindProperty("aimSampleSeconds").floatValue);Assert.AreEqual(.7f,aim.FindProperty("reactionSeconds").floatValue);Assert.AreEqual(.5f,new SerializedObject(prefab.GetComponent<OfficerShotTelegraph>()).FindProperty("windupSeconds").floatValue);
            var gun=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/FirstOperation/OfficerPistol.asset");Assert.AreEqual(55,gun.Damage);Assert.AreEqual(7,gun.MagazineCapacity);Assert.AreEqual(2.8f,gun.ReloadDuration);var pistol=gun.EquippedSprite;Assert.AreEqual(new Color32(38,42,46,255),(Color32)pistol.texture.GetPixel(5,5));var mag=gun.MagazineSprite;Assert.AreEqual(new Vector2(2,4),mag.rect.size);Assert.AreEqual(32,mag.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,mag.texture.filterMode);
            for(int y=0;y<4;y++)for(int x=0;x<2;x++){Vector2 at=new Vector2(x+.5f,y+.5f)-mag.pivot+gun.MagazineMountOffset*32+pistol.pivot;Assert.Greater(pistol.texture.GetPixel(Mathf.FloorToInt(at.x),Mathf.FloorToInt(at.y)).a,0,"Inserted magazine cannot protrude outside opaque gun pixels");}
            var rig=prefab.GetComponentInChildren<CutoutRig>();Assert.Less(rig.magazine.sortingOrder,rig.weapon.sortingOrder,"Inserted magazine sits inside the opaque handle");var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");Assert.AreEqual("Duct Tape",catalog.Find("tape").title);Assert.AreEqual(10,catalog.Find("tape").price);Assert.AreEqual(50,catalog.Find("tape").quantity);Assert.AreEqual(75,catalog.Find("faintanyl").price);
        }
    }
}
