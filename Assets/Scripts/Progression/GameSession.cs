using System;
using UnityEngine;
namespace InsaneDreams.Progression
{
    public enum FlowScreen { MainMenu, Levels, Shop, Settings, Mission }
    public sealed class GameSession : MonoBehaviour
    {
        public static GameSession Instance{get;private set;}
        [SerializeField] private GameCatalog catalog;
        public GameCatalog Catalog=>catalog;
        public GameProgress Data{get;private set;}
        public FlowScreen Screen{get;private set;}
        public string SelectedLevel{get;private set;}
        public string Notice{get;private set;}
        public ProgressStore Store{get;private set;}
        // The mission supplies a conservative disk snapshot while equipment is deployed.
        public Func<GameProgress> MissionSaveSnapshot{get;set;}
        private float nextPurchase;
        public const string WalletHotfixBuild="wallet-release-floor-20261028-r1";
        private void TraceWallet(string operation,int before,int after,bool saved=true)
        {if(catalog!=null&&catalog.playtestWalletDiagnostics)Debug.Log($"[WalletHotfix {WalletHotfixBuild}] GameSession.{operation} {before}->{after} saved={saved} floorEnabled={catalog.developmentPlaytestCredits} configuredStart={catalog.startingMoney} development={Debug.isDebugBuild} build={Application.buildGUID}");}
        public void Initialize(GameCatalog value,string path,bool applyDevelopmentCredits=true)
        {
            catalog=value;MissionSaveSnapshot=null;nextPurchase=0;Store=new ProgressStore(path);Data=Store.Load(NewProgress);Data.Normalize();
            TraceWallet("Initialize.Load",Data.balance,Data.balance);
            EnsureStarter();MigrateLoadout();MigrateArmor();Screen=FlowScreen.MainMenu;AudioListener.volume=Data.volume;
            if(!string.IsNullOrEmpty(Data.activeRun))Settle(Data.pendingEarnings,false);
            if(applyDevelopmentCredits)ApplyDevelopmentCredits();
            TraceWallet("Initialize.Final",Data.balance,Data.balance);
        }
        public bool ApplyDevelopmentCredits()
        {
            if(catalog==null||!catalog.developmentPlaytestCredits||Store==null||Store.ReadOnly||
                Data==null||(Data.balance>=10000&&Data.developmentCreditRevision>=35))return false;
            string snapshot=JsonUtility.ToJson(Data);
            Data.balance=Mathf.Max(Data.balance,10000);
            Data.developmentCreditRevision=35;
            return Commit(snapshot);
        }
        private void Awake()
        {
            if(Instance!=null&&Instance!=this){TraceWallet("Awake.DuplicateRejected",Instance.Data?.balance??0,Instance.Data?.balance??0);Destroy(gameObject);return;}
            Instance=this;DontDestroyOnLoad(gameObject);
            if(catalog!=null)Initialize(catalog,System.IO.Path.Combine(Application.persistentDataPath,"akumu2_progress_v1.json"));
        }
        private GameProgress NewProgress()
        {
            var data=new GameProgress{balance=Mathf.Clamp(catalog.startingMoney,0,1000000),primary=catalog.primaryId,melee=catalog.meleeId,tape=catalog.tapeCapacity};
            TraceWallet("NewProgress",0,data.balance);
            foreach(var offer in catalog.offers)if(offer.starter&&offer.weapon!=null)
            {data.owned.Add(offer.id);var ammo=data.Ammo(offer.id);ammo.magazine=offer.weapon.MagazineCapacity;ammo.reserve=offer.weapon.WeaponCategory==Inventory.WeaponCategory.Firearm?Mathf.Min(catalog.ReserveCapacity(data,offer),Mathf.Max(0,catalog.basicAllowanceMagazines-1)*offer.weapon.MagazineCapacity):0;}
            return data;
        }
        private void EnsureStarter()
        {
            foreach(var offer in catalog.offers)if(offer.starter&&offer.weapon!=null&&!Data.owned.Contains(offer.id))Data.owned.Add(offer.id);
        }
        private void MigrateLoadout()
        {
            // Ownership/ammunition are independent of slots, including displaced legacy primaries.
            var ids=new[]{Data.primary,Data.secondary,Store.LegacyExtraSecondary};
            Data.primary=Data.secondary=null;
            foreach(string id in ids)
            {
                var gun=catalog.Find(id);if(gun?.weapon==null||gun.weapon.WeaponCategory!=Inventory.WeaponCategory.Firearm||!Data.owned.Contains(id))continue;
                if(gun.weapon.FirearmRole==Inventory.FirearmRole.Primary){if(Data.primary==null)Data.primary=id;}
                else if(Data.secondary==null)Data.secondary=id;
            }
            Data.loadoutRevision=40;
            foreach(var stock in Data.ammunition)
            {
                var gun=catalog.Find(stock.id)?.weapon;if(gun==null||gun.WeaponCategory!=Inventory.WeaponCategory.Firearm)continue;
                int excess=Mathf.Max(0,stock.magazine-gun.MagazineCapacity);
                stock.magazine-=excess;stock.reserve+=excess;
            }
        }
        private void MigrateArmor()
        {
            if(string.IsNullOrEmpty(Data.bodyArmorId)&&catalog.BodyTier(Data)>0)Data.bodyArmorId=Array.Find(catalog.offers,o=>o.category==ShopCategory.BodyArmor&&!o.helmet&&o.armorTier==catalog.BodyTier(Data))?.id;
            if(string.IsNullOrEmpty(Data.helmetId)&&Data.helmetTier>0)Data.helmetId=Array.Find(catalog.offers,o=>o.category==ShopCategory.BodyArmor&&o.helmet&&o.armorTier==Data.helmetTier)?.id;
            StoreArmorCondition();
        }
        public void StoreArmorCondition()
        {
            foreach(bool helmet in new[]{false,true})
            {
                string id=helmet?Data.helmetId:Data.bodyArmorId;if(string.IsNullOrEmpty(id))continue;
                var stock=Data.protection.Find(x=>x.id==id);if(stock==null){stock=new ArmorStock{id=id};Data.protection.Add(stock);}stock.condition=helmet?Data.helmet:Data.armor;
            }
        }
        public bool IsUnlocked(LevelEntryData level)=>level!=null&&(string.IsNullOrEmpty(level.prerequisite)||Data.completed.Contains(level.prerequisite));
        public void Navigate(FlowScreen screen)
        {if(screen==FlowScreen.Mission||screen==FlowScreen.Shop)return;Screen=screen;Notice=Store?.Problem;}
        public bool SelectLevel(string id)
        {
            var level=catalog.Level(id);if(Screen!=FlowScreen.Levels||!IsUnlocked(level)||string.IsNullOrEmpty(level.scene))return false;
            SelectedLevel=id;Screen=FlowScreen.Shop;Notice=null;return true;
        }
        public bool PrepareMission()
        {
            if(Screen!=FlowScreen.Shop||!IsUnlocked(catalog.Level(SelectedLevel))||!BeginAttempt())return false;
            Screen=FlowScreen.Mission;return true;
        }
        public bool BeginAttempt()
        {
            if(!string.IsNullOrEmpty(Data.activeRun))return false;
            string snapshot=JsonUtility.ToJson(Data);EnsureStarter();
            foreach(var gun in catalog.offers)if(gun.starter&&gun.weapon!=null&&gun.weapon.WeaponCategory==Inventory.WeaponCategory.Firearm)
            {
                var stock=Data.Ammo(gun.id);int target=gun.weapon.MagazineCapacity*Mathf.Max(1,catalog.basicAllowanceMagazines);
                int total=Mathf.Max(target,stock.magazine+stock.reserve);
                stock.magazine=Mathf.Max(stock.magazine,Mathf.Min(gun.weapon.MagazineCapacity,total));
                stock.reserve=Mathf.Max(0,total-stock.magazine); // Stored excess is not carried; the bridge packs it separately.
            }
            // Intentionally empty slots stay empty. Ownership and starter allowance remain separate.
            Data.activeRun=Guid.NewGuid().ToString("N");Data.activeLevel=SelectedLevel;Data.pendingEarnings=0;
            return Commit(snapshot);
        }
        public bool Settle(int earnings,bool complete)
        {
            string run=Data.activeRun;if(string.IsNullOrEmpty(run)||Data.settledRuns.Contains(run))return false;
            string snapshot=JsonUtility.ToJson(Data);
            Data.lastWallets=Mathf.Clamp(earnings,0,1000000);Data.lastPayout=catalog.Payout(Data.lastWallets,complete);Data.lastReward=complete?Mathf.Max(0,catalog.completionReward):0;Data.lastComplete=complete;
            Data.settledRuns.Add(run);if(Data.settledRuns.Count>64)Data.settledRuns.RemoveAt(0);
            Data.balance=(int)Math.Min(1000000L,(long)Data.balance+Data.lastPayout);
            if(complete&&!Data.completed.Contains(Data.activeLevel))Data.completed.Add(Data.activeLevel);
            Data.activeRun=null;Data.pendingEarnings=0;
            return Commit(snapshot);
        }
        private bool Commit(string snapshot,[System.Runtime.CompilerServices.CallerMemberName]string operation=null)
        {
            int before=JsonUtility.FromJson<GameProgress>(snapshot).balance,after=Data.balance;
            if(Save()){TraceWallet(operation,before,after);return true;}
            Data=JsonUtility.FromJson<GameProgress>(snapshot);TraceWallet(operation,before,after,false);return false;
        }
        public bool Buy(string id,float now)
        {
            if(!CanBuy(id)||float.IsNaN(now)||float.IsInfinity(now)||now<nextPurchase)return false;
            var item=catalog.Find(id);string snapshot=JsonUtility.ToJson(Data);int quantity=Mathf.Max(1,item.quantity);
            switch(item.category)
            {
                case ShopCategory.Throwables:Data.grenades[(int)item.throwable]+=quantity;break;
                case ShopCategory.Weapons:Data.owned.Add(id);Data.Ammo(id).magazine=item.weapon.MagazineCapacity;break;
                case ShopCategory.Ammunition:
                    var gun=Array.Find(catalog.offers,x=>x.weapon==item.weapon&&x.category==ShopCategory.Weapons);
                    Data.Ammo(gun.id).reserve+=quantity;break;
                case ShopCategory.BodyArmor:
                    StoreArmorCondition();var gear=Data.protection.Find(x=>x.id==id);if(gear==null){gear=new ArmorStock{id=id};Data.protection.Add(gear);}gear.condition=catalog.Protection(item.helmet,item.armorTier).capacity;SelectArmor(item,gear.condition);break;
                case ShopCategory.DuctTape:Data.tape=Mathf.Min(catalog.tapeCapacity,Data.tape+quantity);break;
                case ShopCategory.Faintanyl:Data.faintanyl+=quantity;break;
                default:return false;
            }
            Data.balance-=item.price;
            if(!Commit(snapshot))return false;
            nextPurchase=now+.25f;Notice="Purchased "+item.title+". Financial responsibility remains optional.";return true;
        }
        public bool CanBuy(string id)
        {
            if(Screen!=FlowScreen.Shop||Store==null||Store.ReadOnly)return false;
            var item=catalog.Find(id);if(item==null||item.price<0||Data.balance<item.price)return false;
            int quantity=Mathf.Max(1,item.quantity);
            switch(item.category)
            {
                case ShopCategory.Throwables:return (int)item.throwable>=0&&(int)item.throwable<3&&Data.grenades[(int)item.throwable]+(long)quantity<=catalog.GrenadeCapacity(Data);
                case ShopCategory.Weapons:return item.weapon!=null&&!Data.owned.Contains(id);
                case ShopCategory.BodyArmor:
                    float condition=(item.helmet?Data.helmetId:Data.bodyArmorId)==id?(item.helmet?Data.helmet:Data.armor):(Data.protection.Find(x=>x.id==id)?.condition??-1);
                    return condition<catalog.Protection(item.helmet,item.armorTier).capacity;
                case ShopCategory.DuctTape:return Data.tape<catalog.tapeCapacity;
                case ShopCategory.Faintanyl:return Data.faintanyl+(long)quantity<=Combat.PlayerRevive.Capacity;
                case ShopCategory.Ammunition:
                    var gun=Array.Find(catalog.offers,x=>x.category==ShopCategory.Weapons&&x.weapon==item.weapon);
                    return gun!=null&&item.weapon!=null&&item.weapon.WeaponCategory==Inventory.WeaponCategory.Firearm&&Data.owned.Contains(gun.id)&&(Data.ammunition.Find(x=>x.id==gun.id)?.reserve??0)+(long)quantity<=catalog.ReserveCapacity(Data,gun);
                default:return false;
            }
        }
        private void SelectArmor(ShopOffer item,float condition)
        {if(item.helmet){Data.helmetId=item.id;Data.helmetTier=item.armorTier;Data.helmet=condition;}else{Data.bodyArmorId=item.id;Data.bodyArmorTier=item.armorTier;Data.armor=condition;}}
        public bool EquipArmor(string id,bool helmet=false)
        {
            if(Screen!=FlowScreen.Shop||Store.ReadOnly)return false;var item=catalog.Find(id);var stock=Data.protection.Find(x=>x.id==id);
            if(id!=null&&(item?.category!=ShopCategory.BodyArmor||stock==null))return false;
            string snapshot=JsonUtility.ToJson(Data);StoreArmorCondition();
            if(id==null){if(helmet){Data.helmetId=null;Data.helmetTier=0;Data.helmet=0;}else{Data.bodyArmorId=null;Data.bodyArmorTier=0;Data.armor=0;}}
            else SelectArmor(item,stock.condition);
            return Commit(snapshot);
        }
        public bool Equip(string id,bool secondary=false)
            =>Equip(id,catalog.Find(id)?.weapon?.WeaponCategory==Inventory.WeaponCategory.Melee?Inventory.EquipmentSlot.Melee:secondary?Inventory.EquipmentSlot.Firearm2:Inventory.EquipmentSlot.Firearm1);
        public bool Equip(string id,Inventory.EquipmentSlot slot)
        {
            var item=catalog.Find(id);if(Screen!=FlowScreen.Shop||Store.ReadOnly||item?.category!=ShopCategory.Weapons||item.weapon==null||!Data.owned.Contains(id))return false;
            if(!item.weapon.Fits(slot))return false;
            string snapshot=JsonUtility.ToJson(Data);
            if(Data.primary==id)Data.primary=null;if(Data.secondary==id)Data.secondary=null;
            switch(slot){case Inventory.EquipmentSlot.Firearm1:Data.primary=id;break;case Inventory.EquipmentSlot.Firearm2:Data.secondary=id;break;case Inventory.EquipmentSlot.Melee:Data.melee=id;break;}
            if(!Commit(snapshot))return false;Notice="Equipped "+item.title;return true;
        }
        public bool Unequip(Inventory.EquipmentSlot slot)
        {
            if(Screen!=FlowScreen.Shop||Store.ReadOnly)return false;string snapshot=JsonUtility.ToJson(Data);
            switch(slot){case Inventory.EquipmentSlot.Firearm1:Data.primary=null;break;case Inventory.EquipmentSlot.Firearm2:Data.secondary=null;break;default:return false;}
            return Commit(snapshot);
        }
        public bool Save(bool reserveDeployedInventory=true)
        {
            if(Store==null)return false;
            var snapshot=reserveDeployedInventory&&!string.IsNullOrEmpty(Data.activeRun)&&MissionSaveSnapshot!=null?MissionSaveSnapshot():Data;
            if(Store.Save(snapshot))return true;Notice=Store.Problem??"Save unavailable; original data preserved.";return false;
        }
        private void OnDestroy(){if(Instance==this)Instance=null;}
        private void OnApplicationPause(bool paused){if(paused&&Data!=null&&MissionSaveSnapshot==null)Save();}
        private void OnApplicationQuit(){if(Data!=null)Save();}
    }
}
