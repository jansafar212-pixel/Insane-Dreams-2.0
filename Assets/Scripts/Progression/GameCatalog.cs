using System;
using InsaneDreams.Inventory;
using UnityEngine;
namespace InsaneDreams.Progression
{
    public enum ShopCategory { Weapons, Ammunition, BodyArmor, DuctTape }
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
    }
    [CreateAssetMenu(menuName="AKUMU/Game Catalog")]
    public sealed class GameCatalog : ScriptableObject
    {
        public LevelEntryData[] levels=Array.Empty<LevelEntryData>();
        public ShopOffer[] offers=Array.Empty<ShopOffer>();
        public string primaryId, meleeId;
        [Min(0)] public int startingMoney=1000;
        [Min(0)] public int initialAmmo=108, ammoCapacity=432;
        [Min(1)] public float tapeCapacity=200, armorCapacity=60;
        [Range(0,1)] public float armorAbsorption=.65f;
        public ShopOffer Find(string id)=>Array.Find(offers,x=>x.id==id);
        public LevelEntryData Level(string id)=>Array.Find(levels,x=>x.id==id);
    }
}
