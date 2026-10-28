using System;
using InsaneDreams.Inventory;
using UnityEngine;
namespace InsaneDreams.Progression
{
    public enum ShopCategory { Weapons, Ammunition, BodyArmor, DuctTape, Faintanyl, Throwables }
    [Serializable]public sealed class ArmorTuning
    {
        [Min(1)]public float capacity;[Range(0,1)]public float absorption;
        [Range(1,1.3f)]public float reloadMultiplier=1;
        [Min(0)]public int reservePenalty,grenadePenalty;
        public ArmorTuning(float capacity,float absorption,float reload,int reserve=0,int grenades=0){this.capacity=capacity;this.absorption=absorption;reloadMultiplier=reload;reservePenalty=reserve;grenadePenalty=grenades;}
    }
    [Serializable] public sealed class LevelEntryData
    {
        public string id, title, scene, description;
        public string prerequisite;
        public Sprite icon;
    }
    [Serializable] public sealed class ShopOffer
    {
        public string id, title;
        [TextArea] public string description;
        public ShopCategory category;
        [Min(0)] public int price;
        [Min(1)] public int quantity=1;
        public WeaponDefinition weapon;
        public bool starter;
        [Min(0)] public int reserveCapacity;
        public Combat.ThrowableKind throwable;
        [Range(1,2)]public int armorTier=1;
        public bool helmet;
    }
    [CreateAssetMenu(menuName="AKUMU/Game Catalog")]
    public sealed class GameCatalog : ScriptableObject
    {
        public LevelEntryData[] levels=Array.Empty<LevelEntryData>();
        public ShopOffer[] offers=Array.Empty<ShopOffer>();
        public string primaryId, meleeId;
        [Min(0)] public int startingMoney=1000;
        [Tooltip("TEMPORARY PLAYTEST: minimum 10000 at each initialization, INCLUDING release APKs. Disable before shipping.")]
        public bool developmentPlaytestCredits=true;
        [Tooltip("Temporary wallet initialization/transaction diagnostics, including release APKs.")]
        public bool playtestWalletDiagnostics=true;
        [Min(0)] public int completionReward=250;
        [Range(0,1)] public float failureWalletFraction=.25f;
        [Min(0)] public int failurePayoutCap=50;
        [Range(1,5)] public int basicAllowanceMagazines=5;
        [Min(0)] public int initialAmmo=108, ammoCapacity=432;
        [Min(1)] public float tapeCapacity=200, armorCapacity=60;
        [Range(0,1)] public float armorAbsorption=.65f;
        public ArmorTuning lightBody=new ArmorTuning(60,.65f,1.04f),heavyBody=new ArmorTuning(90,.75f,1.18f,36,1);
        public ArmorTuning lightHelmet=new ArmorTuning(20,.45f,1.02f),heavyHelmet=new ArmorTuning(35,.65f,1.08f,18);
        public int BodyTier(GameProgress data)=>data.bodyArmorTier==0&&data.armor>0?1:data.bodyArmorTier;
        public ArmorTuning Protection(bool helmet,int tier)=>helmet?(tier==2?heavyHelmet:lightHelmet):(tier==2?heavyBody:lightBody);
        public float ReloadMultiplier(GameProgress data)=>Mathf.Clamp((BodyTier(data)>0?Protection(false,BodyTier(data)).reloadMultiplier:1)*(data.helmetTier>0?Protection(true,data.helmetTier).reloadMultiplier:1),1,1.3f);
        public int ReserveCapacity(GameProgress data)=>Mathf.Max(Mathf.Min(ammoCapacity,144),ammoCapacity-(BodyTier(data)>0?Protection(false,BodyTier(data)).reservePenalty:0)-(data.helmetTier>0?Protection(true,data.helmetTier).reservePenalty:0));
        public int ReserveCapacity(GameProgress data,ShopOffer gun)
        {
            if(gun?.weapon==null)return 0;
            int limit=gun.reserveCapacity>0?gun.reserveCapacity:ammoCapacity;
            int penalty=(BodyTier(data)>0?Protection(false,BodyTier(data)).reservePenalty:0)+(data.helmetTier>0?Protection(true,data.helmetTier).reservePenalty:0);
            int floor=Mathf.Min(limit,Mathf.Max(gun.weapon.MagazineCapacity,limit/3));
            return Mathf.Max(floor,limit-Mathf.RoundToInt(limit*(penalty/(float)Mathf.Max(1,ammoCapacity))));
        }
        public int Payout(int wallets,bool complete)=>(int)Math.Min(1000000L,complete?(long)Math.Max(0,wallets)+Math.Max(0,completionReward):Math.Min(Math.Max(0,failurePayoutCap),(long)Math.Floor(Math.Max(0,wallets)*Mathf.Clamp01(failureWalletFraction))));
        public int GrenadeCapacity(GameProgress data)=>Mathf.Clamp(Combat.PlayerThrowables.Capacity-(BodyTier(data)>0?Protection(false,BodyTier(data)).grenadePenalty:0)-(data.helmetTier>0?Protection(true,data.helmetTier).grenadePenalty:0),3,Combat.PlayerThrowables.Capacity);
        public ShopOffer Find(string id)=>Array.Find(offers,x=>x.id==id);
        public LevelEntryData Level(string id)=>Array.Find(levels,x=>x.id==id);
    }
}
