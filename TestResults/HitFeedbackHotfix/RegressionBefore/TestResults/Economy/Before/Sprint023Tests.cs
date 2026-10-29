#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint023Tests
    {
        private const float X=28000;
        private readonly List<GameObject> objects=new List<GameObject>();
        private GameObject player;private PlayerActions actions;private PlayerHealing healing;private Health health;private PlayerSupplies supplies;
        private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object target,string name,float value){var so=new SerializedObject(target);so.FindProperty(name).floatValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private void Int(Object target,string name,int value){var so=new SerializedObject(target);so.FindProperty(name).intValue=value;so.ApplyModifiedPropertiesWithoutUndo();}
        private GameObject Solid(Vector2 position,Vector2 size){var go=Keep(new GameObject("S023Geometry"));go.transform.position=position;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(23023);
            Solid(new Vector2(X,-.5f),new Vector2(100,1));
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.85f),Quaternion.identity));
            actions=player.GetComponent<PlayerActions>();healing=player.GetComponent<PlayerHealing>();health=player.GetComponent<Health>();supplies=player.GetComponent<PlayerSupplies>();
            player.GetComponent<PlayerHealthRecovery>().enabled=false;
            var so=new SerializedObject(actions);so.FindProperty("readKeyboard").boolValue=false;so.ApplyModifiedPropertiesWithoutUndo();Float(healing,"damageInterruptionSeconds",0);
            yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        private SupplyCrate2D Crate(float offset=.9f)=>Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/SupplyCrate.prefab"),new Vector3(X+offset,.45f),Quaternion.identity)).GetComponent<SupplyCrate2D>();
        private GameObject Enemy(float offset=4)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X+offset,.71f),Quaternion.identity));go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;return go;
        }
        private EnemyWalletDrop Defeat(GameObject enemy)
        {
            enemy.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player,player));
            var rag=enemy.GetComponent<InsaneDreams.Visuals.RagdollOnDefeat>().LastRagdoll;if(rag!=null)Keep(rag.gameObject);
            return enemy.GetComponent<EnemyWalletDrop>();
        }
        [UnityTest] public IEnumerator WalletAutomaticallyCollectsAtExpandedRadiusButNotBeyondIt()
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/WalletPickup.prefab"),player.transform.position+Vector3.right*2.2f,Quaternion.identity));
            var wallet=g.GetComponent<WalletPickup>();wallet.Initialize("expanded-radius",17);
            yield return new WaitForSeconds(.3f);Assert.AreEqual(0,supplies.Earnings);Assert.IsFalse(wallet.TryCollect(supplies));
            g.transform.position=player.transform.position+Vector3.right*1.5f;Physics2D.SyncTransforms();
            yield return new WaitForSeconds(.3f);Assert.AreEqual(17,supplies.Earnings);Assert.AreEqual("Wallet collected",supplies.Notification);
            Assert.IsFalse(supplies.CollectWallet("expanded-radius",17));Assert.AreEqual(17,supplies.Earnings);
        }
        [UnityTest] public IEnumerator HeldHealingConsumesExactlyRestoredHpAndStopsOnReleaseOrFullHealth()
        {
            Assert.AreEqual(200,healing.DuctTape);health.ApplyDamage(new DamageInfo(80,CombatTeam.Enemy));actions.SetHealIntent(true);yield return new WaitForSeconds(.4f);
            Assert.That(health.Current,Is.InRange(28,35));Assert.AreEqual(health.Current-20,200-healing.DuctTape,.01f);Assert.IsTrue(healing.IsHealing);
            actions.SetHealIntent(false);yield return null;yield return null;float hp=health.Current,tape=healing.DuctTape;yield return new WaitForSeconds(.2f);Assert.AreEqual(hp,health.Current);Assert.AreEqual(tape,healing.DuctTape);
            health.Restore();actions.SetHealIntent(true);yield return new WaitForSeconds(.2f);Assert.AreEqual(tape,healing.DuctTape);Assert.IsFalse(healing.IsHealing);
        }
        [UnityTest] public IEnumerator EmptyTapeCannotHealOrResurrectAndDamageRequiresFreshIntent()
        {
            Float(healing,"initialDuctTape",5);healing.ResetSupply();health.ApplyDamage(new DamageInfo(50,CombatTeam.Enemy));actions.SetHealIntent(true);yield return new WaitForSeconds(.4f);
            Assert.AreEqual(55,health.Current,.01f);Assert.AreEqual(0,healing.DuctTape,.001f);yield return new WaitForSeconds(.2f);Assert.AreEqual(55,health.Current,.01f);
            healing.AddDuctTape(20);yield return null;health.ApplyDamage(new DamageInfo(5,CombatTeam.Enemy));float hp=health.Current;yield return new WaitForSeconds(.2f);Assert.AreEqual(hp,health.Current);Assert.IsTrue(healing.WaitingForRelease);
            actions.SetHealIntent(false);yield return null;actions.SetHealIntent(true);yield return new WaitForSeconds(.15f);Assert.Greater(health.Current,hp);
            health.ApplyDamage(new DamageInfo(1000,CombatTeam.Enemy));yield return new WaitForSeconds(.15f);Assert.AreEqual(0,health.Current);Assert.IsFalse(healing.TryHeal());
        }
        [UnityTest] public IEnumerator HealingWorksInCoverBlocksFireButReloadAndAirborneActionsTakePriority()
        {
            Solid(new Vector2(X+1.1f,.45f),new Vector2(1,.9f));actions.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.25f);Assert.IsTrue(player.GetComponent<PlayerCover2D>().IsCovered);
            health.ApplyDamage(new DamageInfo(60,CombatTeam.Enemy));actions.SetHealIntent(true);yield return new WaitForSeconds(.2f);Assert.IsTrue(healing.IsHealing);
            actions.SetFireIntent(true);yield return null;yield return null;float hp=health.Current;int ammo=player.GetComponent<PlayerLoadout>().SelectedWeapon.MagazineAmmo;yield return new WaitForSeconds(.15f);Assert.Greater(health.Current,hp);Assert.IsTrue(healing.IsHealing);Assert.AreEqual(ammo,player.GetComponent<PlayerLoadout>().SelectedWeapon.MagazineAmmo);
            actions.SetFireIntent(false);var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;weapon.SetAmmunition(1,20);actions.RequestReload();yield return new WaitForSeconds(.15f);Assert.IsFalse(healing.IsHealing);
            weapon.Cycle.CancelReload();actions.SetTouchMove(Vector2.zero);actions.Jump();yield return new WaitForSeconds(.12f);Assert.IsFalse(healing.IsHealing);
        }
        [UnityTest] public IEnumerator CrateCapsBothInventoriesAndKeepsUnacceptedContents()
        {
            health.ApplyDamage(new DamageInfo(80,CombatTeam.Enemy));actions.SetHealIntent(true);yield return new WaitForSeconds(.4f);actions.SetHealIntent(false);yield return null;yield return null;
            float tapeMissing=200-healing.DuctTape;var loadout=player.GetComponent<PlayerLoadout>();var weapon=loadout.SelectedWeapon;weapon.SetAmmunition(13,loadout.MaximumReserveAmmo-10);
            var crate=Crate();Assert.IsTrue(crate.TrySupply(supplies));Assert.AreEqual(200,healing.DuctTape,.001f);Assert.AreEqual(loadout.MaximumReserveAmmo,weapon.ReserveAmmo);Assert.AreEqual(13,weapon.MagazineAmmo);
            Assert.AreEqual(50-tapeMissing,crate.TapeLeft,.01f);Assert.AreEqual(62,crate.AmmoLeft);Assert.IsFalse(crate.TrySupply(supplies));Assert.AreEqual(62,crate.AmmoLeft);
        }
        [UnityTest] public IEnumerator RealInteractionDepletesCrateOnceAndDisableDoesNotRefillIt()
        {
            var crate=Crate();Float(crate,"ductTape",0);Int(crate,"ammunition",10);crate.ResetContents();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;weapon.SetAmmunition(5,0);
            yield return new WaitForSeconds(.15f);Assert.AreEqual(crate,supplies.NearbyCrate);actions.RequestInteraction();Assert.AreEqual(10,weapon.ReserveAmmo);Assert.IsTrue(crate.Depleted);
            actions.RequestInteraction();crate.gameObject.SetActive(false);crate.gameObject.SetActive(true);Assert.IsTrue(crate.Depleted);Assert.IsFalse(crate.TrySupply(supplies));Assert.AreEqual(10,weapon.ReserveAmmo);
            supplies.ResetMission();Assert.AreEqual(10,crate.AmmoLeft);Assert.AreEqual(0,supplies.Earnings);Assert.AreEqual(200,healing.DuctTape);
        }
        [UnityTest] public IEnumerator SuppliesAndWalletsRespectSolidBarriers()
        {
            var crate=Crate(1.1f);var wall=Solid(new Vector2(X+.55f,1.5f),new Vector2(.1f,3));Assert.IsFalse(crate.TrySupply(supplies));Assert.AreEqual(72,crate.AmmoLeft);
            var wallet=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Pickups/WalletPickup.prefab"),new Vector3(X+1,.85f),Quaternion.identity)).GetComponent<WalletPickup>();wallet.Initialize("blocked-wallet",11);
            Assert.IsFalse(wallet.TryCollect(supplies));Assert.AreEqual(0,supplies.Earnings);Object.Destroy(wall);yield return new WaitForSeconds(.3f);Assert.AreEqual(11,supplies.Earnings);Assert.AreEqual("Wallet collected",supplies.Notification);
        }
        [UnityTest] public IEnumerator RealDefeatDropsAutomaticWalletExactlyOnceAcrossEnemyReactivation()
        {
            var enemy=Enemy();var drop=Defeat(enemy);Assert.IsFalse(enemy.activeSelf);Assert.IsNotNull(drop.Wallet);var wallet=drop.Wallet;
            string id=(string)typeof(WalletPickup).GetField("rewardId",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(wallet);
            player.GetComponent<Rigidbody2D>().position=new Vector2(X+4,.85f);Physics2D.SyncTransforms();yield return new WaitForSeconds(.3f);
            Assert.That(supplies.Earnings,Is.InRange(5,20));Assert.AreEqual("Wallet collected",supplies.Notification);int money=supplies.Earnings;Assert.IsFalse(supplies.CollectWallet(id,999));Assert.AreEqual(money,supplies.Earnings);
            enemy.GetComponent<Health>().Restore();enemy.SetActive(true);Defeat(enemy);Assert.IsTrue(drop.Wallet==null);Assert.AreEqual(money,supplies.Earnings);
        }
        [UnityTest] public IEnumerator ImpactCueAndMaterialParticlesFadeWithinExistingPools()
        {
            var enemy=Enemy(5);var attack=player.GetComponent<AttackExecutor>();var weapon=player.GetComponent<PlayerLoadout>().SelectedWeapon;
            Assert.IsTrue(attack.Attack(weapon,((Vector2)enemy.transform.position-attack.Origin).normalized,Time.time));var hit=player.transform.Find("HitConfirmation").GetComponent<SpriteRenderer>();Assert.IsTrue(hit.enabled);Assert.AreEqual(1,hit.color.a,.01f);
            yield return new WaitForSeconds(.05f);Assert.That(hit.color.a,Is.InRange(.05f,.95f));yield return new WaitForSeconds(.15f);Assert.IsFalse(hit.enabled);
            Solid(new Vector2(X+2,2),new Vector2(.2f,4)).AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Metal);yield return new WaitForSeconds(weapon.Definition.FiringInterval);
            var feedback=player.GetComponent<CombatFeedback>();int capacity=feedback.ImpactParticleCapacity;Assert.IsTrue(attack.Attack(weapon,Vector2.right,Time.time));Assert.AreEqual(ImpactMaterial.Metal,feedback.LastImpactMaterial);
            var surface=player.transform.Find("SurfaceImpact").GetComponent<SpriteRenderer>();yield return new WaitForSeconds(.05f);Assert.That(surface.color.a,Is.InRange(.05f,.95f));Assert.AreEqual(capacity,feedback.ImpactParticleCapacity);
        }
    }
}
#endif
