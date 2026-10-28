using UnityEditor;
using UnityEngine;
using System;
using InsaneDreams.Combat;
using InsaneDreams.Progression;
public static class Sprint0027Setup
{
    const string Folder="Assets/Data/Sprint0027";
    static void Edit(UnityEngine.Object target,Action<SerializedObject> change){var s=new SerializedObject(target);change(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);}
    public static Sprite Icon(string name,Color color,bool smoke=false)
    {
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Data","Sprint0027");string path=Folder+"/"+name+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old!=null)return old;
        var t=new Texture2D(8,8,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<8;y++)for(int x=0;x<8;x++){bool shape=smoke?x>=1&&x<=6&&y>=1&&y<=6&&(x>=2&&x<=5||y>=2&&y<=5):x>=2&&x<=5&&y>=1&&y<=5||y==6&&x>=3&&x<=5;t.SetPixel(x,y,!shape?Color.clear:y==6?new Color32(185,156,79,255):x==2?new Color32(25,29,43,255):color);}t.Apply();AssetDatabase.CreateAsset(t,Folder+"/"+name+"Pixels.asset");var sprite=Sprite.Create(t,new Rect(0,0,8,8),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    public static string InstallThrowables()
    {
        var icons=new[]{Icon("Frag",new Color32(115,137,77,255)),Icon("Flash",new Color32(210,215,199,255)),Icon("Smoke",new Color32(12,99,116,255))};var smoke=Icon("Cloud",new Color32(145,156,146,255),true);
        string path=Folder+"/Throwable.prefab";var g=new GameObject("Throwable");g.layer=31;
        try{var body=g.AddComponent<Rigidbody2D>();body.gravityScale=1.2f;body.mass=.15f;body.linearDamping=.1f;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;body.interpolation=RigidbodyInterpolation2D.Interpolate;var collider=g.AddComponent<CircleCollider2D>();collider.radius=.09f;collider.includeLayers=1;collider.excludeLayers=~1;collider.layerOverridePriority=100;g.AddComponent<SpriteRenderer>().sortingOrder=24;var p=g.AddComponent<ThrowableProjectile2D>();Edit(p,s=>{var a=s.FindProperty("icons");a.arraySize=3;for(int i=0;i<3;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=icons[i];s.FindProperty("smokeSprite").objectReferenceValue=smoke;});g.GetComponent<SmokeCloud2D>().enabled=false;PrefabUtility.SaveAsPrefabAsset(g,path);}finally{UnityEngine.Object.DestroyImmediate(g);}
        const string player="Assets/Prefabs/Player/WarehousePlayer.prefab";g=PrefabUtility.LoadPrefabContents(player);try{var component=g.GetComponent<PlayerThrowables>();if(component==null)component=g.AddComponent<PlayerThrowables>();Edit(component,s=>{s.FindProperty("projectile").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<ThrowableProjectile2D>();var a=s.FindProperty("icons");a.arraySize=3;for(int i=0;i<3;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=icons[i];});PrefabUtility.SaveAsPrefabAsset(g,player);}finally{PrefabUtility.UnloadPrefabContents(g);}
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");var offers=new System.Collections.Generic.List<ShopOffer>(catalog.offers);string[] ids={"frag-grenade","flash-grenade","smoke-grenade"};string[] names={"Cartoon fragmentation grenade","Flashbang","Smoke grenade"};for(int i=0;i<3;i++)if(catalog.Find(ids[i])==null)offers.Add(new ShopOffer{id=ids[i],title=names[i],description="Select grenade slot, choose type, aim and release the right stick to throw. Finite stock.",category=ShopCategory.Throwables,throwable=(ThrowableKind)i,quantity=1,price=60-i*10});catalog.offers=offers.ToArray();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();return "Three throwable types, finite shop stock and existing player slot2 configured; no scenes changed";
    }
    public static void InstallFallDamage()
    {
        const string path="Assets/Prefabs/Player/WarehousePlayer.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try{if(root.GetComponent<InsaneDreams.Combat.PlayerFallDamage>()==null)root.AddComponent<InsaneDreams.Combat.PlayerFallDamage>();PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
    }
    public static void CreateMine()
    {
        string spritePath=Folder+"/Mine.asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if(sprite==null){var t=new Texture2D(12,5,TextureFormat.RGBA32,false){name="MinePixels",filterMode=FilterMode.Point};for(int y=0;y<5;y++)for(int x=0;x<12;x++)t.SetPixel(x,y,x<1||x>10||y==0?Color.clear:y==4?(x>=4&&x<=7?new Color32(185,156,79,255):Color.clear):x==1||x==10||y==1?new Color32(25,29,43,255):x==6&&y==3?new Color32(194,43,22,255):new Color32(115,137,77,255));t.Apply();AssetDatabase.CreateAsset(t,Folder+"/MinePixels.asset");sprite=Sprite.Create(t,new Rect(0,0,12,5),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect);sprite.name="FictionalMine";AssetDatabase.CreateAsset(sprite,spritePath);}
        var root=new GameObject("FictionalEnvironmentalMine");try{var renderer=root.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=4;var trigger=root.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(.9f,.3f);trigger.offset=new Vector2(0,.1f);root.AddComponent<EnvironmentalMine2D>();PrefabUtility.SaveAsPrefabAsset(root,Folder+"/EnvironmentalMine.prefab");}finally{UnityEngine.Object.DestroyImmediate(root);}AssetDatabase.SaveAssets();
    }
    public static void InstallRegions()
    {
        var paths=new System.Collections.Generic.List<string>{"Assets/Prefabs/Player/WarehousePlayer.prefab"};foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs/Enemies","Assets/Data/FirstOperation"}))paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        foreach(var path in paths){var root=PrefabUtility.LoadPrefabContents(path);try{if(root.GetComponent<Health>()!=null&&root.GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true)!=null){if(root.GetComponent<BodyRegionDamage>()==null)root.AddComponent<BodyRegionDamage>();PrefabUtility.SaveAsPrefabAsset(root,path);}}finally{PrefabUtility.UnloadPrefabContents(root);}}AssetDatabase.SaveAssets();
    }
    public static void InstallHealing()
    {
        string path=Folder+"/TapeBand.asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite==null){var t=new Texture2D(8,4,TextureFormat.RGBA32,false){name="TapeBandPixels",filterMode=FilterMode.Point};for(int y=0;y<4;y++)for(int x=0;x<8;x++)t.SetPixel(x,y,x>=1&&x<=6&&y>=1&&y<=2?(y==1?new Color32(124,132,126,255):new Color32(190,194,177,255)):Color.clear);t.Apply();AssetDatabase.CreateAsset(t,Folder+"/TapeBandPixels.asset");sprite=Sprite.Create(t,new Rect(0,0,8,4),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect);sprite.name="TapeBand";AssetDatabase.CreateAsset(sprite,path);}
        string audio=Folder+"/TapePeel.wav";if(!System.IO.File.Exists(audio)){const int rate=24000,n=6000;using(var writer=new System.IO.BinaryWriter(System.IO.File.Create(audio))){writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+n*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(n*2);var random=new System.Random(2707);float filtered=0;for(int i=0;i<n;i++){float t=(float)i/n,envelope=Mathf.Sin(t*Mathf.PI)*(.5f+.5f*Mathf.Sin(i*2*Mathf.PI*65/rate));filtered=.55f*filtered+.45f*((float)random.NextDouble()*2-1);writer.Write((short)(filtered*envelope*22000));}}AssetDatabase.ImportAsset(audio,ImportAssetOptions.ForceSynchronousImport);}
        const string player="Assets/Prefabs/Player/WarehousePlayer.prefab";var root=PrefabUtility.LoadPrefabContents(player);try{var p=root.GetComponent<InsaneDreams.Visuals.PlayerHealingPresentation>();if(p==null)p=root.AddComponent<InsaneDreams.Visuals.PlayerHealingPresentation>();Edit(p,s=>{s.FindProperty("tapeSprite").objectReferenceValue=sprite;s.FindProperty("peelingSound").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(audio);});PrefabUtility.SaveAsPrefabAsset(root,player);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
    }
    public static void SetMedicalDefaults()
    {
        const string path="Assets/Prefabs/Player/WarehousePlayer.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{Edit(root.GetComponent<PlayerRevive>(),s=>s.FindProperty("initialInjections").intValue=0);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();
    }
    public static string ConfigureOfficer()
    {
        const string path="Assets/Data/FirstOperation/Officer.prefab";
        var larper=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");
        if(larper==null)throw new InvalidOperationException("Larper baseline prefab unavailable");
        float width=new SerializedObject(larper.GetComponent<AttackExecutor>()).FindProperty("tracerWidth").floatValue;
        var root=PrefabUtility.LoadPrefabContents(path);
        try{Edit(root.GetComponent<InsaneDreams.Enemies.OfficerShotTelegraph>(),s=>s.FindProperty("windupSeconds").floatValue=.5f);Edit(root.GetComponent<AttackExecutor>(),s=>{s.FindProperty("tracerWidth").floatValue=width*2;s.FindProperty("enemyTracer").colorValue=new Color(1,0,1,.95f);});PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();return "Officer warning=.5s; magenta tracer width="+(width*2);
    }
}
