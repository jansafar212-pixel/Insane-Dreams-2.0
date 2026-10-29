#if UNITY_EDITOR
using System.Collections;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
namespace InsaneDreams.Tests
{
    public sealed class Sniper0039Tests
    {
        [UnityTest]public IEnumerator PostShotCycleIsNotReloadAndManualLoadingRequiresFreshAimRelease()
        {
            bool background=Application.runInBackground;Application.runInBackground=true;
            var p=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(119000,100),Quaternion.identity);
            try
            {
                p.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
                var input=p.GetComponent<PlayerActions>();var data=new SerializedObject(input);data.FindProperty("readKeyboard").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();
                var load=p.GetComponent<PlayerLoadout>();var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/Sniper.asset"));
                gun.SetAmmunition(3,10);Assert.IsTrue(load.Equip(EquipmentSlot.Firearm1,gun));load.Select(EquipmentSlot.Firearm1);yield return null;
                var combat=p.GetComponent<PlayerCombat>();var attack=p.GetComponent<AttackExecutor>();
                input.BeginTouchAim();input.SetTouchAim(Vector2.right);yield return null;yield return null;Assert.IsTrue(combat.SniperAiming);
                input.EndTouchAim(true);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(1,attack.ShotsFired);Assert.AreEqual(2,gun.MagazineAmmo);Assert.IsTrue(gun.Cycle.IsRecovering(Time.time));Assert.IsFalse(gun.Cycle.IsReloading);
                combat.Reload();Assert.IsFalse(gun.Cycle.IsReloading);yield return new WaitForSeconds(1.05f);
                input.BeginTouchAim();input.SetTouchAim(Vector2.right);yield return null;yield return null;Assert.IsTrue(combat.SniperAiming);
                combat.Reload();Assert.IsTrue(gun.Cycle.IsReloading);input.EndTouchAim(true);input.SetTouchAim(Vector2.zero);
                yield return new WaitForSeconds(gun.Definition.ReloadStepDuration*.75f);
                var rig=p.GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>();Assert.IsTrue(rig.ReloadShell.enabled);Assert.Greater(rig.ReloadShell.transform.localPosition.y,0,"Rifle cartridge approaches the receiver from above the grip");Assert.AreEqual(2,gun.MagazineAmmo);
                var cameraObject=new GameObject("SniperReloadAuditCamera");var camera=cameraObject.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=1.3f;camera.transform.position=p.transform.position+new Vector3(0,.1f,-10);
                var rt=new RenderTexture(640,360,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var texture=new Texture2D(640,360,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,640,360),0,0);texture.Apply();System.IO.Directory.CreateDirectory("TestResults/Sprint0039");System.IO.File.WriteAllBytes("TestResults/Sprint0039/SniperReload.png",texture.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;Object.Destroy(texture);Object.Destroy(rt);Object.Destroy(cameraObject);
                yield return new WaitForSeconds(gun.Definition.ReloadDuration+.1f);
                Assert.AreEqual(1,attack.ShotsFired);Assert.AreEqual(5,gun.MagazineAmmo);Assert.AreEqual(7,gun.ReserveAmmo);Assert.IsFalse(gun.Cycle.IsReloading);Assert.IsFalse(combat.SniperAiming);
                input.BeginTouchAim();input.SetTouchAim(Vector2.right);yield return null;yield return null;input.EndTouchAim(false);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(1,attack.ShotsFired);
            }
            finally{Object.Destroy(p);Application.runInBackground=background;}
            yield return null;
        }
    }
}
#endif
