#if UNITY_EDITOR
using System.Collections;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0027HealingTests
    {
        [UnityTest]public IEnumerator TapeWrapConsumesOnePointPerHpBlocksFireAndImmediatelyRestoresWeapons()
        {
            bool background=Application.runInBackground;Application.runInBackground=true;var floor=new GameObject("TapeFloor");floor.transform.position=new Vector2(35000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(30,1);var player=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(35000,.85f),Quaternion.identity);
            try{var hp=player.GetComponent<Health>();hp.ProtectFor(0);hp.ApplyDamage(new DamageInfo(30,CombatTeam.Enemy));yield return new WaitForSeconds(1);var h=player.GetComponent<PlayerHealing>();var a=player.GetComponent<PlayerActions>();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;int ammo=weapon.MagazineAmmo;float before=hp.Current,tape=h.DuctTape;a.SetFireIntent(true);a.SetHealIntent(true);yield return new WaitForSeconds(.2f);yield return new WaitForEndOfFrame();Assert.IsTrue(h.IsHealing);Assert.IsTrue(player.GetComponent<PlayerHealingPresentation>().IsWrapping);Assert.Greater(player.GetComponent<PlayerHealingPresentation>().Peels,0);var rig=player.GetComponentInChildren<CutoutRig>();if(rig.Segmented==null||!rig.Segmented.IsBound)Assert.That(Mathf.DeltaAngle(0,rig.rightArm.localEulerAngles.z),Is.InRange(-55f,-29f),"Simple production arm must visibly wrap instead of retaining its aim pose");Assert.AreEqual(ammo,weapon.MagazineAmmo);Assert.AreEqual(hp.Current-before,tape-h.DuctTape,.02f);Assert.IsFalse(player.GetComponent<AttackExecutor>().Attack(weapon,Vector2.right,Time.time));a.SetHealIntent(false);yield return new WaitForSeconds(.15f);yield return new WaitForEndOfFrame();Assert.IsFalse(h.IsHealing);Assert.IsFalse(player.GetComponent<PlayerHealingPresentation>().IsWrapping);Assert.Less(weapon.MagazineAmmo,ammo);}
            finally{Object.Destroy(player);Object.Destroy(floor);Application.runInBackground=background;}
        }
    }
}
#endif
