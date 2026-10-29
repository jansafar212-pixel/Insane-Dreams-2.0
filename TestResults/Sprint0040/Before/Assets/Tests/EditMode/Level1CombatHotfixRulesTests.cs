using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Level1CombatHotfixRulesTests
    {
        [Test]public void RadialCurveIsContinuousMonotonicAndZeroOutsideRadius()
        {
            var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/OperationFirework.prefab").GetComponent<FireworkProjectile>();
            float[] positions={0,.25f,.5f,.75f,1,1.1f};float[] expected={100,85,50,20,0,0};for(int i=0;i<positions.Length;i++)Assert.AreEqual(expected[i],p.DamageAtDistance(p.Radius*positions[i]),.02f);
            float previous=p.DamageAtDistance(0);for(int i=1;i<=1000;i++){float next=p.DamageAtDistance(p.Radius*i/1000);Assert.LessOrEqual(next,previous+.001f);Assert.LessOrEqual(previous-next,.6f);previous=next;}Assert.AreEqual(0,p.DamageAtDistance(float.NaN));
        }
        [Test]public void MissionOnlyTuningPreservesGeometryAndSlowHeavyWeapon()
        {
            Assert.AreEqual(System.IO.File.ReadAllText("TestResults/Level1CombatHotfix/GeometryBefore.json"),System.IO.File.ReadAllText("TestResults/Level1CombatHotfix/GeometryAfter.json"));
            var gun=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/FirstOperation/OfficerPistol.asset");Assert.AreEqual(55,gun.Damage);Assert.AreEqual(2.4f,gun.FiringInterval);Assert.AreEqual(7,gun.MagazineCapacity);Assert.AreEqual(2.8f,gun.ReloadDuration);Assert.NotNull(gun.MagazineSprite);Assert.IsTrue(gun.PhysicalMagazineReload);
            var profile=new SerializedObject(gun);Assert.AreEqual("Desert Eagle",profile.FindProperty("displayName").stringValue);
            var original=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");var scoped=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/OperationSamurai.prefab");Assert.AreEqual(.7f,new SerializedObject(original.GetComponent<SamuraiCombat>()).FindProperty("recoverySeconds").floatValue);Assert.AreEqual(.42f,new SerializedObject(scoped.GetComponent<SamuraiCombat>()).FindProperty("recoverySeconds").floatValue);
        }
    }
}
