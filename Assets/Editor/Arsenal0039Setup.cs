using System;
using System.Linq;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using UnityEditor;
using UnityEngine;

public static class Arsenal0039Setup
{
    const string Folder="Assets/Art/Arsenal0039";
    static void Edit(UnityEngine.Object o,Action<SerializedObject> change){var s=new SerializedObject(o);change(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static void F(SerializedObject s,string key,float v)=>s.FindProperty(key).floatValue=v;
    static Sprite Save(string name,Texture2D t,Vector2 pivot)
    {
        string path=Folder+"/"+name+".asset";var found=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(found!=null){UnityEngine.Object.DestroyImmediate(t);return found;}
        t.name=name+"Pixels";t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.Apply();AssetDatabase.CreateAsset(t,Folder+"/"+name+"Pixels.asset");
        var sprite=Sprite.Create(t,new Rect(0,0,t.width,t.height),pivot,32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static Sprite GunArt(string name,int width,int grip)
    {
        var t=new Texture2D(width,14,TextureFormat.RGBA32,false);
        Color ink=new Color32(25,29,43,255),steel=new Color32(112,128,127,255),wood=new Color32(137,87,52,255);
        for(int y=0;y<14;y++)for(int x=0;x<width;x++)
        {
            Color c=Color.clear;
            if(y>=7&&y<=9&&x>=3&&x<width)c=y==7||x==width-1?ink:steel;
            if(x<=grip&&y>=5&&y<=7)c=y==5||x==0?ink:wood;
            if(x>=grip-1&&x<=grip+1&&y>=2&&y<=7)c=x==grip-1||y==2?ink:wood;
            if(x>=grip+3&&x<=grip+7&&y==5)c=ink;
            if(name=="SKS"&&x>=grip+6&&x<width-6&&y>=5&&y<=7)c=y==5?ink:wood;
            if(name=="SKS"&&x>=grip+8&&x<=grip+11&&y>=3&&y<=6)c=x==grip+8||y==3?ink:steel;
            if(name=="AA12"&&x<grip&&y>=5&&y<=9)c=x==0||y==5?ink:steel;
            if(name=="AA12"&&x>=grip+3&&x<=grip+8&&y>=1&&y<=7)c=x==grip+3||x==grip+8||y==1?ink:steel;
            if(name=="AA12"&&x>=grip+9&&x<width-5&&y>=5&&y<=8)c=y==5?ink:steel;
            if(name=="SawedOff"&&x>=grip+5&&y>=6&&y<=10)c=y==6||y==8||x==width-1?ink:steel;
            if(y==10&&(x==grip+1||x==width-3))c=ink;
            t.SetPixel(x,y,c);
        }
        return Save(name,t,new Vector2(grip/(float)width,5f/14));
    }
    static WeaponDefinition NewGun(string name,string title,float damage,float cadence,int cap,float reload,float range,FirearmRole role,WeaponFireMode mode,int pellets,float spread,float far)
    {
        string path="Assets/Resources/Arsenal/"+name+".asset";var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
        if(w==null){w=ScriptableObject.CreateInstance<WeaponDefinition>();AssetDatabase.CreateAsset(w,path);Edit(w,s=>s.FindProperty("stableId").stringValue=Guid.NewGuid().ToString("N"));}
        var art=GunArt(name,name=="SKS"?43:name=="AA12"?34:24,name=="SKS"?11:name=="AA12"?9:6);
        Edit(w,s=>{s.FindProperty("displayName").stringValue=title;s.FindProperty("firearmRole").enumValueIndex=(int)role;s.FindProperty("fireMode").enumValueIndex=(int)mode;s.FindProperty("equippedSprite").objectReferenceValue=art;s.FindProperty("icon").objectReferenceValue=art;F(s,"damage",damage);F(s,"firingInterval",cadence);s.FindProperty("magazineCapacity").intValue=cap;F(s,"reloadDuration",reload);F(s,"firearmRange",range);s.FindProperty("pellets").intValue=pellets;F(s,"spreadDegrees",spread);F(s,"falloffStart",name=="SKS"?18:3);F(s,"farDamageMultiplier",far);F(s,"recoilScale",name=="SKS"?1.6f:2);F(s,"hitReactionStrength",name=="SKS"?1.8f:.55f);F(s,"impactImpulse",name=="SKS"?1.4f:.55f);s.FindProperty("customMuzzle").boolValue=true;s.FindProperty("muzzleOffset").vector2Value=new Vector2(art.bounds.max.x,3.5f/32);});
        return w;
    }
    static void Offer(GameCatalog c,WeaponDefinition w,string id,int price,int reserve,int ammoPrice)
    {
        var list=c.offers.ToList();var gun=list.Find(o=>o.id==id);if(gun==null){gun=new ShopOffer{id=id,category=ShopCategory.Weapons,price=price,weapon=w,reserveCapacity=reserve};list.Add(gun);}gun.title=w.DisplayName;gun.description=w.FirearmRole+" / "+(w.FireMode==WeaponFireMode.Automatic?"automatic":"semi-automatic")+" / "+w.MagazineCapacity+" rounds";
        var ammo=list.Find(o=>o.id==id+"-ammo");if(ammo==null){ammo=new ShopOffer{id=id+"-ammo",category=ShopCategory.Ammunition,price=ammoPrice,weapon=w,quantity=w.MagazineCapacity};list.Add(ammo);}ammo.title=w.DisplayName+" ammunition";c.offers=list.ToArray();EditorUtility.SetDirty(c);
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play first");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","Arsenal0039");
        var arsenal=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");
        foreach(var w in arsenal.weapons)
        {
            bool secondary=w.name=="BackupPistol"||w.name=="DesertEagle"||w.name=="LarperBuzzbox";
            Edit(w,s=>{s.FindProperty("firearmRole").enumValueIndex=secondary?1:0;string title=w.name=="BackupPistol"?"Glock":w.name=="SMG"?"SMG":w.name=="WarehouseCarbine"?"Carbine":w.name=="Shotgun"?"Shotgun":w.name=="LMG"?"LMG":w.name=="Sniper"?"Sniper Rifle":w.DisplayName;s.FindProperty("displayName").stringValue=title;});
        }
        var sks=NewGun("SKS","SKS",38,.33f,10,2.7f,34,FirearmRole.Primary,WeaponFireMode.SemiAutomatic,1,0,.7f);
        var aa=NewGun("AA12","AA-12",8,.26f,8,3,14,FirearmRole.Primary,WeaponFireMode.Automatic,7,22,.12f);
        var saw=NewGun("SawedOff","Sawed-off Double Barrel",12,.45f,2,2.4f,11,FirearmRole.Secondary,WeaponFireMode.SemiAutomatic,8,30,.08f);
        var casing=new Texture2D(3,5,TextureFormat.RGBA32,false);for(int y=0;y<5;y++)for(int x=0;x<3;x++)casing.SetPixel(x,y,y==4?new Color32(137,87,52,255):x==0?new Color32(112,95,51,255):new Color32(217,188,90,255));
        var cartridge=Save("RifleCartridge",casing,Vector2.one*.5f);
        foreach(var w in new[]{sks,arsenal.weapons.Single(w=>w.name=="Sniper")})Edit(w,s=>{s.FindProperty("loadsIndividually").boolValue=true;s.FindProperty("shellSprite").objectReferenceValue=cartridge;s.FindProperty("ammunitionInsertionOffset").vector2Value=new Vector2(.3f,.14f);});
        arsenal.weapons=arsenal.weapons.Concat(new[]{sks,aa,saw}).Distinct().ToArray();EditorUtility.SetDirty(arsenal);
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");
        Offer(catalog,sks,"sks",1200,120,60);Offer(catalog,aa,"aa12",2200,64,50);Offer(catalog,saw,"sawed-off",900,20,25);
        foreach(var o in catalog.offers)if(o.weapon!=null){o.title=o.weapon.DisplayName+(o.category==ShopCategory.Ammunition?" ammunition":"");}
        EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();return "3 new modular hitscan weapons, roles and display metadata; existing IDs/prices and wallet flags preserved";
    }
}
