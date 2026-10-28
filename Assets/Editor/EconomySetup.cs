#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using InsaneDreams.Progression;
using InsaneDreams.Inventory;
using UnityEditor;
public static class EconomySetup
{
    public static string Apply()
    {
        var c=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");
        var arsenal=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");
        var list=new List<ShopOffer>(c.offers);
        ShopOffer Offer(string id){var o=list.Find(x=>x.id==id);if(o==null){o=new ShopOffer{id=id};list.Add(o);}return o;}
        void Gun(string id,int index,int price,int capacity,int packPrice)
        {
            var o=Offer(id);o.category=ShopCategory.Weapons;o.weapon=arsenal.weapons[index];o.title=o.weapon.DisplayName;o.price=price;o.quantity=1;o.starter=id=="pistol";o.reserveCapacity=capacity;
            o.description=id=="pistol"?"Always available. Five magazines total at each mission start. No premium ammunition allowance.":"Permanent ownership; one loaded magazine included. Spare ammunition sold separately. Handling unchanged.";
            var a=Offer(id+"-ammo");a.category=ShopCategory.Ammunition;a.weapon=o.weapon;a.title=o.title+" / "+o.weapon.MagazineCapacity+" rounds";a.price=packPrice;a.quantity=o.weapon.MagazineCapacity;a.description="One magazine of ammunition for "+o.title+" only. Requires ownership and room for the whole pack.";
        }
        Gun("pistol",0,0,96,5);Gun("buzzbox",7,200,162,12);Gun("smg",2,350,180,15);Gun("carbine",3,500,144,18);
        Gun("shotgun",4,650,36,24);Gun("desert-eagle",1,1200,42,42);Gun("lmg",5,1600,200,65);Gun("sniper",6,2000,30,50);
        Offer("armor").price=150;Offer("heavy-armor").price=300;Offer("light-helmet").price=60;Offer("heavy-helmet").price=120;
        Offer("tape").price=25;Offer("tape").quantity=50;
        Offer("faintanyl").price=175;Offer("faintanyl").quantity=1;Offer("faintanyl").description="relax its not addictive, just habit forming";
        Offer("frag-grenade").price=50;Offer("flash-grenade").price=35;Offer("smoke-grenade").price=30;
        c.offers=list.ToArray();c.primaryId="pistol";c.startingMoney=100;c.completionReward=250;c.failureWalletFraction=.25f;c.failurePayoutCap=50;c.basicAllowanceMagazines=5;
        EditorUtility.SetDirty(c);AssetDatabase.SaveAssets();return "Economy catalog updated; no scenes, weapons, armor mechanics or prefabs changed.";
    }
}
#endif
