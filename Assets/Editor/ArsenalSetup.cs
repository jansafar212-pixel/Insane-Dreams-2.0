using System;
using System.IO;
using InsaneDreams.Inventory;
using UnityEditor;
using UnityEngine;
public static class ArsenalSetup
{
    const string Folder="Assets/Resources/Arsenal";
    static void Edit(UnityEngine.Object value,Action<SerializedObject> edit){var s=new SerializedObject(value);edit(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(value);}
    static void Float(SerializedObject s,string n,float v)=>s.FindProperty(n).floatValue=v;
    static void Int(SerializedObject s,string n,int v)=>s.FindProperty(n).intValue=v;
    static WeaponDefinition Gun(string name,string title,float damage,float interval,int capacity,float reload,float range)
    {
        string path=Folder+"/"+name+".asset";var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
        if(w==null){w=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset"));w.name=name;AssetDatabase.CreateAsset(w,path);Edit(w,s=>s.FindProperty("stableId").stringValue=Guid.NewGuid().ToString("N"));}
        Edit(w,s=>{s.FindProperty("displayName").stringValue=title;Float(s,"damage",damage);Float(s,"firingInterval",interval);Int(s,"magazineCapacity",capacity);Float(s,"reloadDuration",reload);Float(s,"firearmRange",range);s.FindProperty("magazineSprite").objectReferenceValue=null;s.FindProperty("physicalMagazineReload").boolValue=false;});return w;
    }
    static Sprite Art(string name,int width,int grip,bool longGun,bool drum)
    {
        string path=Folder+"/"+name+"Sprite.asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old!=null)return old;
        var t=new Texture2D(width,14,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        Color ink=new Color32(25,29,43,255),steel=new Color32(112,128,127,255),wood=new Color32(137,87,52,255),light=new Color32(185,190,162,255);
        for(int y=0;y<14;y++)for(int x=0;x<width;x++)
        {
            Color c=Color.clear;
            if(y>=7&&y<=10&&x>=3&&x<width-5)c=y==7||y==10?ink:steel;
            if(y==8&&x>=width-6)c=x==width-1?ink:steel;
            if(y>=2&&y<=7&&x>=grip-2&&x<=grip)c=x==grip-2?ink:wood;
            if(y>=5&&y<=7&&x>=grip+1&&x<=grip+5)c=y==5||x==grip+5?ink:Color.clear;
            if(x<=4&&y>=5&&y<=8)c=x==0||y==5?ink:wood;
            if(longGun&&y>=5&&y<=7&&x>=grip+8&&x<width-7)c=y==5?ink:wood;
            if(name=="Sniper"&&y>=11&&y<=13&&x>=grip+2&&x<=grip+13)c=y==11||y==13||x==grip+13?ink:light;
            if(drum&&x>=grip+5&&x<=grip+10&&y>=1&&y<=6)c=y==1||x==grip+5||x==grip+10?ink:steel;
            if(!longGun&&!drum&&x>=grip+5&&x<=grip+7&&y>=1&&y<=7)c=x==grip+5?ink:steel;
            t.SetPixel(x,y,c);
        }
        t.Apply();AssetDatabase.CreateAsset(t,Folder+"/"+name+"Pixels.asset");var sprite=Sprite.Create(t,new Rect(0,0,width,14),new Vector2((float)grip/width,5f/14),32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    static void Presentation(WeaponDefinition w,Sprite sprite,float muzzleY,float pitch,float recoil,float flash)
    {
        Edit(w,s=>{s.FindProperty("equippedSprite").objectReferenceValue=sprite;s.FindProperty("icon").objectReferenceValue=sprite;s.FindProperty("customMuzzle").boolValue=true;s.FindProperty("muzzleOffset").vector2Value=new Vector2(sprite.bounds.max.x,muzzleY);Float(s,"firingPitch",pitch);Float(s,"recoilScale",recoil);Float(s,"flashScale",flash);});
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Stop Play Mode");
        if(!AssetDatabase.IsValidFolder("Assets/Resources"))AssetDatabase.CreateFolder("Assets","Resources");if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Resources","Arsenal");
        var de=Gun("DesertEagle","Desert Eagle",55,.65f,7,2.8f,28);
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Data/FirstOperation/DesertEagle.asset");Presentation(de,sprite,5f/32,.7f,2.1f,1.7f);
        var officer=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/FirstOperation/OfficerPistol.asset");Edit(de,s=>{s.FindProperty("magazineSprite").objectReferenceValue=officer.MagazineSprite;s.FindProperty("magazineMountOffset").vector2Value=officer.MagazineMountOffset;s.FindProperty("physicalMagazineReload").boolValue=true;Float(s,"impactImpulse",2.5f);});
        var smg=Gun("SMG","Rattlebox SMG",9,.065f,30,1.8f,20);Presentation(smg,Art("SMG",29,7,false,false),3.5f/32,1.45f,.65f,.85f);Edit(smg,s=>{Float(s,"spreadDegrees",5);Float(s,"bloomPerShot",.5f);Float(s,"maximumBloom",4);Float(s,"falloffStart",6);Float(s,"farDamageMultiplier",.35f);});
        var shotgun=Gun("Shotgun","Doorbell Shotgun",8,.85f,6,2.6f,16);Presentation(shotgun,Art("Shotgun",38,10,true,false),3.5f/32,.62f,2,1.8f);Edit(shotgun,s=>{Int(s,"pellets",7);Float(s,"spreadDegrees",24);Float(s,"falloffStart",4);Float(s,"farDamageMultiplier",.15f);Float(s,"impactImpulse",.55f);});
        var lmg=Gun("LMG","Lunchbox LMG",14,.11f,100,8,26);Presentation(lmg,Art("LMG",37,9,true,true),3.5f/32,.8f,1.25f,1.2f);Edit(lmg,s=>{Int(s,"developerReserve",200);Float(s,"spreadDegrees",2);Float(s,"bloomPerShot",1);Float(s,"maximumBloom",10);Float(s,"falloffStart",12);Float(s,"farDamageMultiplier",.65f);});
        var sniper=Gun("Sniper","Long Complaint Sniper",90,1,5,2.6f,45);Presentation(sniper,Art("Sniper",46,12,true,false),3.5f/32,.55f,2,1.9f);Edit(sniper,s=>{s.FindProperty("fireMode").enumValueIndex=(int)WeaponFireMode.ReleaseToFire;Float(s,"impactImpulse",3);s.FindProperty("customTracer").boolValue=true;s.FindProperty("tracerColor").colorValue=new Color(.7f,.85f,1,1);Float(s,"tracerScale",1.8f);});
        var catalog=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");if(catalog==null){catalog=ScriptableObject.CreateInstance<ArsenalCatalog>();AssetDatabase.CreateAsset(catalog,"Assets/Resources/ArsenalCatalog.asset");}
        catalog.weapons=new[]{AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset"),de,smg,AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),shotgun,lmg,sniper,AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset")};EditorUtility.SetDirty(catalog);
        var scenes=new System.Collections.Generic.List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if(!scenes.Exists(x=>x.path=="Assets/Scenes/Warehouse_Testbed.unity"))scenes.Add(new EditorBuildSettingsScene("Assets/Scenes/Warehouse_Testbed.unity",true));
        EditorBuildSettings.scenes=scenes.ToArray(); // MainMenu stays first; only dev entry opens this scene.
        var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Player/WarehousePlayer.prefab");try{Edit(root.GetComponent<InsaneDreams.Combat.AttackExecutor>(),s=>Int(s,"maximumTracers",16));PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Player/WarehousePlayer.prefab");}finally{PrefabUtility.UnloadPrefabContents(root);}
        AssetDatabase.SaveAssets();Directory.CreateDirectory("TestResults/Arsenal");foreach(var w in catalog.weapons)if(w.EquippedSprite!=null)File.WriteAllBytes("TestResults/Arsenal/"+w.name+".png",w.EquippedSprite.texture.EncodeToPNG());
        return "Seven core roles plus retained automatic Glock; Testbed appended to build scenes, MainMenu remains first. No enemy definitions/prices/ownership/scene-content changes; player tracer ring 16.";
    }
}
