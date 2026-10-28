#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class CombatReliabilityPlayTests
    {
        const float X=95000;readonly List<GameObject> owned=new List<GameObject>();GameObject player;PlayerActions input;PlayerLoadout load;PlayerCombat combat;bool background;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        BoxCollider2D Solid(Vector2 p,Vector2 size){var g=Keep(new GameObject("ReliabilitySolid"));g.transform.position=p;var c=g.AddComponent<BoxCollider2D>();c.size=size;Physics2D.SyncTransforms();return c;}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;Solid(new Vector2(X,-.5f),new Vector2(80,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.85f),Quaternion.identity));
            player.GetComponent<PlayerHealthRecovery>().enabled=false;input=player.GetComponent<PlayerActions>();var s=new SerializedObject(input);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();load=player.GetComponent<PlayerLoadout>();combat=player.GetComponent<PlayerCombat>();yield return new WaitForSeconds(.3f);
        }
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;}
        RuntimeWeapon Equip(string name){var w=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/"+name+".asset"));load.Equip(EquipmentSlot.Firearm1,w);load.Select(EquipmentSlot.Firearm1);return w;}
        void Capture(string name)
        {
            var go=new GameObject("ReliabilityCapture");var c=go.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=1.4f;c.transform.position=player.transform.position+new Vector3(0,.1f,-10);var rt=new RenderTexture(640,360,24);var previous=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(640,360,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,640,360),0,0);t.Apply();Directory.CreateDirectory("TestResults/CombatReliability");File.WriteAllBytes("TestResults/CombatReliability/"+name+".png",t.EncodeToPNG());RenderTexture.active=previous;c.targetTexture=null;Object.Destroy(t);Object.Destroy(rt);Object.Destroy(go);
        }
        [UnityTest]public IEnumerator LmgExchangeFacesBothWaysAndCancelsWithoutLooseObjects()=>Exchange("LMG");
        [UnityTest]public IEnumerator EagleExchangeFacesBothWaysAndCancelsWithoutLooseObjects()=>Exchange("DesertEagle");
        IEnumerator Exchange(string name)
        {
                var w=Equip(name);yield return null;var rig=player.GetComponentInChildren<CutoutRig>();int count=rig.weapon.GetComponentsInChildren<SpriteRenderer>(true).Length;
                foreach(int side in new[]{1,-1})
                {
                    w.SetAmmunition(1,3);input.SetTouchAim(Vector2.right*side);input.SetTouchMove(Vector2.right*side*.4f);combat.Reload();float start=w.Cycle.ReloadStartedAt;combat.Reload();Assert.AreEqual(start,w.Cycle.ReloadStartedAt);
                    yield return new WaitForSeconds(w.Definition.ReloadDuration*.7f);Assert.AreEqual(1,w.MagazineAmmo);Capture(name+"Exchange"+side);Assert.AreEqual(rig.weapon.transform,rig.magazine.transform.parent);
                    yield return new WaitForSeconds(w.Definition.ReloadDuration*.35f);Assert.AreEqual(4,w.MagazineAmmo);Assert.AreEqual(0,w.ReserveAmmo);Assert.IsFalse(w.Cycle.IsReloading);
                    Assert.AreEqual(side,player.GetComponent<CharacterVisualDriver>().Facing);input.SetTouchMove(Vector2.zero);Capture(name+"Inserted"+side);Assert.AreEqual(count,rig.weapon.GetComponentsInChildren<SpriteRenderer>(true).Length);
                }
                w.SetAmmunition(1,3);combat.Reload();yield return new WaitForSeconds(.1f);load.Select(EquipmentSlot.Melee);yield return null;Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(1,w.MagazineAmmo);Assert.AreEqual(3,w.ReserveAmmo);
                load.Select(EquipmentSlot.Firearm1);yield return null;combat.Reload();player.GetComponent<Health>().ProtectFor(0);player.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Enemy));yield return null;Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(4,w.MagazineAmmo+w.ReserveAmmo);player.GetComponent<Health>().Restore();input.SuspendInput();
        }
        [UnityTest]public IEnumerator SniperManualReloadCancelsHeldShotAndRecoversWithoutGrantingAmmo()
        {
            var w=Equip("Sniper");w.SetAmmunition(2,8);yield return null;input.BeginTouchAim();input.SetTouchAim(Vector2.right);yield return null;yield return null;Assert.IsTrue(combat.SniperAiming);
            combat.Reload();input.EndTouchAim(true);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(0,player.GetComponent<AttackExecutor>().ShotsFired);Assert.IsFalse(combat.SniperAiming);
            yield return new WaitForSeconds(2.7f);Assert.AreEqual(5,w.MagazineAmmo);Assert.AreEqual(5,w.ReserveAmmo);
            input.BeginTouchAim();input.SetTouchAim(Vector2.right);yield return null;yield return null;input.EndTouchAim(true);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(1,player.GetComponent<AttackExecutor>().ShotsFired);Assert.AreEqual(4,w.MagazineAmmo);
            yield return new WaitForSeconds(1.1f);Assert.AreEqual(4,w.MagazineAmmo);Assert.AreEqual(5,w.ReserveAmmo);Assert.IsFalse(w.Cycle.IsRecovering(Time.time));
        }
        [UnityTest]public IEnumerator HealingCancelsLmgAndBlocksAllThreeManualReloadsAndFire()
        {
            var w=Equip("LMG");yield return null;w.SetAmmunition(1,20);combat.Reload();player.GetComponent<Health>().ProtectFor(0);player.GetComponent<Health>().ApplyDamage(new DamageInfo(70,CombatTeam.Neutral));yield return new WaitForSeconds(.85f);
            input.SetHealIntent(true);yield return null;yield return null;Assert.IsTrue(player.GetComponent<PlayerHealing>().IsHealing);Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(1,w.MagazineAmmo);
            foreach(var n in new[]{"LMG","DesertEagle","Sniper"}){w=Equip(n);w.SetAmmunition(1,20);combat.Reload();Assert.IsFalse(w.Cycle.IsReloading);input.SetFireIntent(true);yield return null;Assert.AreEqual(1,w.MagazineAmmo);input.SetFireIntent(false);}
        }
        [UnityTest]public IEnumerator LowCeilingRefusesThrowWithoutSpendingAndDelayedWalletRecoveryIsBounded()
        {
            var cover=player.GetComponent<PlayerCover2D>();var t=player.GetComponent<PlayerThrowables>();t.RestoreStock(new[]{1,1,1});load.Select(EquipmentSlot.Melee);input.SetTouchMove(Vector2.down);Solid(new Vector2(X+1.1f,.5f),Vector2.one);
            input.SetTouchAim(new Vector2(1,.5f));yield return new WaitForSeconds(.25f);Assert.IsTrue(cover.IsCovered);
            Solid(new Vector2(X,1.2f),new Vector2(1,.12f));Physics2D.SyncTransforms();Assert.IsFalse(t.TryReleaseOrigin(input.AimDirection,out _));int before=t.Throws;input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(before,t.Throws);
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/WalletPickup.prefab"),new Vector3(X+10,.2f),Quaternion.identity));var wallet=g.GetComponent<WalletPickup>();wallet.Initialize("recovery",15);
            var obstacle=Solid(new Vector2(X+10,.5f),Vector2.one);yield return new WaitForSeconds(.7f);Assert.IsFalse(obstacle.OverlapPoint(g.transform.position));int relocations=wallet.Relocations;yield return new WaitForSeconds(1.6f);Assert.AreEqual(relocations,wallet.Relocations);Assert.IsTrue(g.activeSelf);
        }
        [UnityTest]public IEnumerator AllGrenadesClearCoverOnBothSidesAndReturnToCrouch()
        {
            var t=player.GetComponent<PlayerThrowables>();var cover=player.GetComponent<PlayerCover2D>();load.Select(EquipmentSlot.Melee);input.SetTouchMove(Vector2.down);
            var crate=Solid(new Vector2(X+1.1f,.5f),Vector2.one);
            foreach(int side in new[]{1,-1})
            {
                crate.transform.position=new Vector2(X+side*1.1f,.5f);Physics2D.SyncTransforms();
                for(int kind=0;kind<3;kind++)
                {
                    t.RestoreStock(new[]{1,1,1});t.Select((ThrowableKind)kind);input.BeginTouchAim();input.SetTouchAim(new Vector2(side,.6f).normalized);yield return new WaitForSeconds(.3f);
                    Assert.IsTrue(cover.IsCovered);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.IsTrue(t.TryReleaseOrigin(input.AimDirection,out var origin));Assert.Greater(origin.y,crate.bounds.max.y+.1f);Assert.Less(Vector2.Distance(origin,t.PreviewOrigin),.03f);
                    int before=t.Throws;input.EndTouchAim(true);input.SetTouchAim(Vector2.zero);yield return null;yield return null;Assert.AreEqual(before+1,t.Throws);Assert.AreEqual(0,t.Count((ThrowableKind)kind));Assert.IsTrue(cover.IsCovered);
                    var flying=Object.FindObjectsByType<ThrowableProjectile2D>(FindObjectsSortMode.None).First(p=>p.Busy&&Vector2.Distance(p.transform.position,origin)<3);Assert.IsFalse(crate.OverlapPoint(flying.transform.position));
                    flying.gameObject.SetActive(false);yield return new WaitForSeconds(.5f);Assert.AreEqual(PlayerCoverStance.Concealed,cover.Stance);
                }
            }
            input.BeginTouchAim();input.SetTouchAim(Vector2.left);t.RestoreStock(new[]{1,1,1});yield return new WaitForSeconds(.2f);int throws=t.Throws;input.EndTouchAim(false);input.SetTouchAim(Vector2.zero);yield return null;Assert.AreEqual(throws,t.Throws);
        }
        [UnityTest]public IEnumerator WalletsRecoverFromCratesWallsElevatedDecksAndAwardExactlyOnceBothDirections()
        {
            var supplies=player.GetComponent<PlayerSupplies>();var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/WalletPickup.prefab");
            for(int i=0;i<8;i++)
            {
                float x=X+8+i*3;float y=i>=4?3.2f:0;var support=Solid(new Vector2(x,y-.25f),new Vector2(2,.5f));var obstruction=Solid(new Vector2(x,y+.6f),new Vector2(.8f,1.2f));
                var g=Keep(Object.Instantiate(prefab,new Vector3(x,y+.3f),Quaternion.identity));var wallet=g.GetComponent<WalletPickup>();wallet.Initialize("safe-"+i,20);Physics2D.SyncTransforms();Assert.IsFalse(obstruction.OverlapPoint(g.transform.position));Assert.GreaterOrEqual(g.transform.position.y,y);
                Vector2 at=g.transform.position;int side=i%2==0?1:-1;player.transform.position=new Vector3(at.x+side*.9f,at.y+.7f);player.GetComponent<Rigidbody2D>().position=player.transform.position;Physics2D.SyncTransforms();int before=supplies.Earnings;
                Assert.IsTrue(wallet.TryCollect(supplies),"case"+i);Assert.IsFalse(wallet.TryCollect(supplies));Assert.AreEqual(before+20,supplies.Earnings);Assert.IsFalse(supplies.CollectWallet("safe-"+i,20));
                Object.Destroy(obstruction.gameObject);Object.Destroy(support.gameObject);yield return null;
            }
            Assert.AreEqual(160,supplies.Earnings);Assert.IsNotEmpty(supplies.Notification);
        }
    }
}
#endif
