#if UNITY_EDITOR
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
namespace InsaneDreams.Tests
{
    public sealed class AA12ReloadHotfixRulesTests
    {
        static WeaponDefinition Definition=>AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/AA12.asset");
        [TestCase(0,16,8,8)]
        [TestCase(3,13,8,8)]
        [TestCase(4,2,6,0)]
        [TestCase(0,3,3,0)]
        public void MagazineReloadTransfersOnceAtThreeSecondsAndCannotFireDuringReload(int magazine,int reserve,int expectedMagazine,int expectedReserve)
        {
            var d=Definition;Assert.AreEqual(8,d.MagazineCapacity);Assert.AreEqual(3,d.ReloadDuration);
            Assert.IsFalse(d.LoadsIndividually);Assert.IsTrue(d.PhysicalMagazineReload);Assert.NotNull(d.MagazineSprite);
            var w=new RuntimeWeapon(d);w.SetAmmunition(magazine,reserve);int inserted=0,changed=0;
            w.Cycle.MagazineInserted+=()=>inserted++;w.AmmunitionChanged+=()=>changed++;
            Assert.IsTrue(w.Cycle.TryReload(10));Assert.AreEqual(13,w.Cycle.ReloadEndsAt,.001f);
            Assert.IsFalse(w.Cycle.TryReload(11));Assert.IsFalse(w.Cycle.TryAttack(11));
            w.Cycle.Tick(12.99f);Assert.AreEqual(magazine,w.MagazineAmmo);Assert.AreEqual(reserve,w.ReserveAmmo);
            w.Cycle.Tick(13);w.Cycle.Tick(100);
            Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(expectedMagazine,w.MagazineAmmo);Assert.AreEqual(expectedReserve,w.ReserveAmmo);
            Assert.AreEqual(magazine+reserve,w.MagazineAmmo+w.ReserveAmmo);Assert.AreEqual(1,inserted);Assert.AreEqual(1,changed);
        }
        [TestCase(8,16)] [TestCase(0,0)] [TestCase(3,0)]
        public void FullMagazineOrNoReserveCannotEnterStuckReload(int magazine,int reserve)
        {var w=new RuntimeWeapon(Definition);w.SetAmmunition(magazine,reserve);Assert.IsFalse(w.Cycle.TryReload(1));Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(magazine,w.MagazineAmmo);Assert.AreEqual(reserve,w.ReserveAmmo);}
        [Test]
        public void CancellationHasNoTransferAndFreshReloadUsesFullDuration()
        {
            var w=new RuntimeWeapon(Definition);w.SetAmmunition(3,11);Assert.IsTrue(w.Cycle.TryReload(1));
            w.Cycle.Tick(2.5f);w.Cycle.CancelReload();w.Cycle.Tick(5);Assert.AreEqual(3,w.MagazineAmmo);Assert.AreEqual(11,w.ReserveAmmo);
            Assert.IsTrue(w.Cycle.TryReload(6));Assert.AreEqual(9,w.Cycle.ReloadEndsAt);w.Cycle.Tick(9);Assert.AreEqual(8,w.MagazineAmmo);Assert.AreEqual(6,w.ReserveAmmo);
        }
        [Test]
        public void SplitMagazineReconstructsOriginalPixelsExactly()
        {
            var original=AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>("Assets/Art/Arsenal0039/AA12.asset").texture;
            var body=Definition.EquippedSprite.texture;var magazine=Definition.MagazineSprite.texture;
            for(int y=0;y<14;y++)for(int x=0;x<34;x++)
            {var pixel=x>=12&&x<18&&y>=1&&y<5?magazine.GetPixel(x-12,y-1):body.GetPixel(x,y);Assert.AreEqual(original.GetPixel(x,y),pixel,$"{x},{y}");}
        }
    }
}
#endif
