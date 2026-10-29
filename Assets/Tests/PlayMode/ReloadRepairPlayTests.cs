#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Input;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class ReloadRepairPlayTests
    {
        GameObject player,floor,document;PlayerActions input;PlayerLoadout load;PlayerCombat combat;CutoutRig rig;bool background;
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;floor=new GameObject("ReloadFloor");floor.transform.position=new Vector2(92000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(100,1);player=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(92000,.8f),Quaternion.identity);input=player.GetComponent<PlayerActions>();var so=new SerializedObject(input);so.FindProperty("readKeyboard").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();load=player.GetComponent<PlayerLoadout>();combat=player.GetComponent<PlayerCombat>();rig=player.GetComponentInChildren<CutoutRig>();yield return new WaitForSeconds(.3f);}
        [UnityTearDown]public IEnumerator Cleanup(){if(document!=null)Object.Destroy(document);Object.Destroy(player);Object.Destroy(floor);yield return null;Application.runInBackground=background;}
        RuntimeWeapon Equip(string name){var w=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/"+name+".asset"));load.Equip(EquipmentSlot.Firearm1,w);load.Select(EquipmentSlot.Firearm1);return w;}
        void Capture(string name)
        {
            var go=new GameObject("ReloadCapture");var c=go.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=1.3f;c.transform.position=player.transform.position+new Vector3(0,.1f,-10);var rt=new RenderTexture(640,360,24);var previous=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(640,360,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,640,360),0,0);t.Apply();Directory.CreateDirectory("TestResults/ReloadEconomy");File.WriteAllBytes("TestResults/ReloadEconomy/"+name+".png",t.EncodeToPNG());RenderTexture.active=previous;c.targetTexture=null;Object.Destroy(t);Object.Destroy(rt);Object.Destroy(go);
        }
        [UnityTest]public IEnumerator SmgExchangeTracksFacingMovementAndCancelsOnSwitchWithoutDebris()
        {
            var w=Equip("SMG");yield return null;w.SetAmmunition(12,60);int children=rig.weapon.GetComponentsInChildren<SpriteRenderer>(true).Length;
            for(int i=0;i<2;i++)
            {
                input.SetTouchAim(i==0?Vector2.right:Vector2.left);input.SetTouchMove(i==0?Vector2.right:Vector2.down);combat.Reload();float started=w.Cycle.ReloadStartedAt;combat.Reload();Assert.AreEqual(started,w.Cycle.ReloadStartedAt);if(i==0){input.Jump();yield return new WaitForSeconds(.15f);Assert.IsFalse(player.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded);}
                yield return new WaitForSeconds(w.Definition.ReloadDuration*.7f);Assert.AreEqual(12,w.MagazineAmmo);Assert.AreEqual(rig.weapon.transform,rig.magazine.transform.parent);Capture("SmgExchange"+i);
                yield return new WaitForSeconds(w.Definition.ReloadDuration*.4f);Assert.AreEqual(30,w.MagazineAmmo);Assert.AreEqual(42,w.ReserveAmmo);w.SetAmmunition(12,60);
            }
            Assert.AreEqual(children,rig.weapon.GetComponentsInChildren<SpriteRenderer>(true).Length);
            combat.Reload();yield return new WaitForSeconds(.2f);load.Equip(EquipmentSlot.Firearm2,new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/Shotgun.asset")));load.Select(EquipmentSlot.Firearm2);yield return null;Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(12,w.MagazineAmmo);Assert.AreEqual(60,w.ReserveAmmo);Assert.IsFalse(rig.weapon.transform.Find("ReplacementMagazine").GetComponent<SpriteRenderer>().enabled);
        }
        [UnityTest]public IEnumerator DefaultPistolFlushMagazineMovesSeatsAndConservesPartialReloads()
        {
            var definition=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");
            var w=new RuntimeWeapon(definition);load.Equip(EquipmentSlot.Firearm1,w);load.Select(EquipmentSlot.Firearm1);
            Assert.AreEqual(15,definition.MagazineCapacity);Assert.AreEqual(1.5f,definition.ReloadDuration);
            var presentation=player.GetComponent<PistolMagazineReload>();Assert.IsTrue(presentation.Handles(definition));
            foreach(var aim in new[]{Vector2.right,Vector2.left})
            {
                input.SetTouchAim(aim);w.SetAmmunition(7,20);yield return null;yield return new WaitForEndOfFrame();
                Assert.IsTrue(rig.magazine.enabled);Assert.AreEqual(definition.MagazineSprite,rig.magazine.sprite);
                int releases=presentation.Releases;combat.Reload();yield return new WaitForSeconds(.48f);
                Assert.Less(rig.magazine.transform.localPosition.y,definition.MagazineMountOffset.y-.05f);
                yield return new WaitForSeconds(.25f);Assert.AreEqual(releases+1,presentation.Releases);Assert.IsFalse(rig.magazine.enabled);
                yield return new WaitForSeconds(.3f);var fresh=rig.weapon.transform.Find("PistolReplacementMagazine").GetComponent<SpriteRenderer>();
                Assert.IsTrue(fresh.enabled);Assert.AreEqual(definition.MagazineSprite,fresh.sprite);Capture("DefaultPistolInsertion"+(aim.x>0?"Right":"Left"));
                yield return new WaitForSeconds(.6f);yield return new WaitForEndOfFrame();
                Assert.AreEqual(15,w.MagazineAmmo);Assert.AreEqual(12,w.ReserveAmmo);Assert.IsTrue(rig.magazine.enabled);Assert.IsFalse(fresh.enabled);
                Assert.Less(Vector2.Distance(rig.magazine.transform.localPosition,definition.MagazineMountOffset),.001f);
            }
            w.SetAmmunition(7,3);combat.Reload();yield return new WaitForSeconds(1.6f);Assert.AreEqual(10,w.MagazineAmmo);Assert.AreEqual(0,w.ReserveAmmo);
            w.SetAmmunition(7,20);combat.Reload();yield return new WaitForSeconds(.8f);load.Select(EquipmentSlot.Melee);yield return null;
            Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(7,w.MagazineAmmo);Assert.AreEqual(20,w.ReserveAmmo);
            Assert.IsFalse(rig.weapon.transform.Find("PistolReplacementMagazine").GetComponent<SpriteRenderer>().enabled);
        }
        [UnityTest]public IEnumerator ShotgunRealClockShellsPersistOnFireAndDeathAndVisualIsBounded()
        {
            var w=Equip("Shotgun");yield return null;w.SetAmmunition(2,10);combat.Reload();yield return new WaitForSeconds(w.Definition.ReloadStepDuration*1.5f);Assert.AreEqual(3,w.MagazineAmmo);Assert.AreEqual(9,w.ReserveAmmo);Assert.IsNotNull(rig.ReloadShell);Capture("ShotgunShell");
            Assert.IsTrue(player.GetComponent<AttackExecutor>().Attack(w,Vector2.right,Time.time));Assert.AreEqual(2,w.MagazineAmmo);Assert.IsFalse(w.Cycle.IsReloading);yield return null;Assert.IsFalse(rig.ReloadShell.enabled);
            combat.Reload();yield return new WaitForSeconds(w.Definition.ReloadStepDuration*2.2f);Assert.AreEqual(4,w.MagazineAmmo);int total=w.MagazineAmmo+w.ReserveAmmo;load.Equip(EquipmentSlot.Firearm2,new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/SMG.asset")));load.Select(EquipmentSlot.Firearm2);yield return null;Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(4,w.MagazineAmmo);load.Select(EquipmentSlot.Firearm1);combat.Reload();
            player.GetComponent<Health>().ProtectFor(0);player.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy));yield return null;Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(total,w.MagazineAmmo+w.ReserveAmmo);Assert.IsFalse(rig.ReloadShell.enabled);Assert.AreEqual(1,rig.weapon.GetComponentsInChildren<SpriteRenderer>(true).Count(x=>x.name=="ReloadShell"));
        }
        [UnityTest]public IEnumerator StartingTapeHealingCancelsReloadWithoutDiscardingCompletedShells()
        {
            var w=Equip("Shotgun");yield return null;w.SetAmmunition(2,10);combat.Reload();yield return new WaitForSeconds(w.Definition.ReloadStepDuration*1.2f);int count=w.MagazineAmmo;
            player.GetComponent<Health>().ProtectFor(0);player.GetComponent<Health>().ApplyDamage(new DamageInfo(10,CombatTeam.Neutral));yield return new WaitForSeconds(.8f);count=w.MagazineAmmo;input.SetHealIntent(true);yield return new WaitForSeconds(.1f);Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(count,w.MagazineAmmo);Assert.IsTrue(player.GetComponent<PlayerHealing>().IsHealing);
        }
        [UnityTest]public IEnumerator HudShowsEachInsertedShellOnNextUiFrameAndRebindIsSafe()
        {
            var w=Equip("Shotgun");yield return null;w.SetAmmunition(2,10);
            document=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"));var hud=document.GetComponent<WarehouseTouchControls>();hud.Bind(input);hud.Bind(input);yield return null;
            var root=document.GetComponent<UnityEngine.UIElements.UIDocument>().rootVisualElement;
            combat.Reload();yield return new WaitForSeconds(w.Definition.ReloadStepDuration*1.1f);yield return null;
            Assert.AreEqual("3/6",root.Q<UnityEngine.UIElements.Label>("ammoValue").text);Assert.AreEqual(3,w.MagazineAmmo);Assert.AreEqual(9,w.ReserveAmmo);
        }
    }
}
#endif
