using NUnit.Framework;
using UnityEditor;
using InsaneDreams.Inventory;
namespace InsaneDreams.Tests
{
    public sealed class ReloadRepairRulesTests
    {
        RuntimeWeapon Gun(string name,int loaded,int reserve){var w=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/"+name+".asset"));w.SetAmmunition(loaded,reserve);return w;}
        [TestCase(0,20)][TestCase(2,10)][TestCase(2,1)][TestCase(5,1)]
        public void ShellStepsTransferExactlyOneAndRepeatedTicksCannotDuplicate(int loaded,int reserve)
        {
            var w=Gun("Shotgun",loaded,reserve);int total=loaded+reserve,events=0;w.Cycle.MagazineInserted+=()=>events++;
            Assert.IsTrue(w.Cycle.TryReload(10));float next=w.Cycle.ReloadEndsAt;
            while(w.Cycle.IsReloading){int before=w.MagazineAmmo;w.Cycle.Tick(next-.001f);Assert.AreEqual(before,w.MagazineAmmo);w.Cycle.Tick(next);Assert.AreEqual(before+1,w.MagazineAmmo);w.Cycle.Tick(next);Assert.AreEqual(before+1,w.MagazineAmmo);Assert.AreEqual(total,w.MagazineAmmo+w.ReserveAmmo);next=w.Cycle.ReloadEndsAt;}
            Assert.AreEqual(System.Math.Min(6-loaded,reserve),events);w.Cycle.Tick(999);Assert.AreEqual(total,w.MagazineAmmo+w.ReserveAmmo);
        }
        [TestCase(1)][TestCase(2)]public void InterruptedShellsRemainUsableAndResumeDoesNotRepeatThem(int insertions)
        {
            var w=Gun("Shotgun",2,10);w.Cycle.TryReload(10);for(int i=0;i<insertions;i++)w.Cycle.Tick(w.Cycle.ReloadEndsAt);
            float now=w.Cycle.ReloadStartedAt;Assert.IsTrue(w.Cycle.TryAttack(now));Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(1+insertions,w.MagazineAmmo);Assert.AreEqual(10-insertions,w.ReserveAmmo);
            Assert.IsTrue(w.Cycle.TryReload(now+1));w.Cycle.Tick(999);Assert.AreEqual(6,w.MagazineAmmo);Assert.AreEqual(11,w.MagazineAmmo+w.ReserveAmmo);
        }
        [TestCase(0,60,30,30)][TestCase(12,60,30,42)][TestCase(12,3,15,0)]
        public void SmgAtomicExchangeConservesTacticalRounds(int loaded,int reserve,int end,int remaining)
        {
            var w=Gun("SMG",loaded,reserve);Assert.IsTrue(w.Cycle.TryReload(10));float deadline=w.Cycle.ReloadEndsAt;Assert.IsFalse(w.Cycle.TryReload(10.1f));Assert.AreEqual(deadline,w.Cycle.ReloadEndsAt);
            w.Cycle.CancelReload();w.Cycle.Tick(999);Assert.AreEqual(loaded,w.MagazineAmmo);Assert.AreEqual(reserve,w.ReserveAmmo);w.Cycle.TryReload(1000);w.Cycle.Tick(w.Cycle.ReloadEndsAt);w.Cycle.Tick(2000);Assert.AreEqual(end,w.MagazineAmmo);Assert.AreEqual(remaining,w.ReserveAmmo);
        }
        [Test]public void FullEmptyReserveArmorAndLargeTickAreBounded()
        {
            foreach(string name in new[]{"SMG","Shotgun"}){var w=Gun(name,name=="SMG"?30:6,5);Assert.IsFalse(w.Cycle.TryReload(0));w.SetAmmunition(0,0);Assert.IsFalse(w.Cycle.TryReload(0));}
            var shot=Gun("Shotgun",0,20);shot.SetReloadMultiplier(1.2f);shot.Cycle.TryReload(10);Assert.AreEqual(shot.Definition.ReloadDuration/6*1.2f,shot.Cycle.ReloadEndsAt-10,.001);shot.Cycle.Tick(999);Assert.AreEqual(6,shot.MagazineAmmo);Assert.AreEqual(14,shot.ReserveAmmo);Assert.IsFalse(shot.Cycle.IsReloading);
        }
        [Test]public void ShellSnapshotKeepsCompletedInsertionAndResumeDoesNotGrantPendingShell()
        {
            var w=Gun("Shotgun",2,10);w.Cycle.TryReload(10);w.Cycle.Tick(w.Cycle.ReloadEndsAt);
            var restored=RuntimeWeapon.Restore(w.Capture(),w.Definition);Assert.AreEqual(3,restored.MagazineAmmo);Assert.AreEqual(9,restored.ReserveAmmo);Assert.IsFalse(restored.Cycle.IsReloading);
            restored.Cycle.TryReload(20);restored.Cycle.Tick(100);Assert.AreEqual(6,restored.MagazineAmmo);Assert.AreEqual(6,restored.ReserveAmmo);
        }
    }
}
