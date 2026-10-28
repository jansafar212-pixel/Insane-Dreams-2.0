using System;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class Sprint0029Setup
{
    static void Edit(Object o,Action<SerializedObject> action){var s=new SerializedObject(o);action(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(o);}
    static void Awareness(GameObject root)
    {
        var c=root.GetComponent<EnemyCombat>();if(c==null)return;
        Edit(c,s=>{s.FindProperty("obstructedHearingMultiplier").floatValue=.6f;if(root.GetComponent<SamuraiCombat>()!=null){s.FindProperty("reactionSeconds").floatValue=.25f;s.FindProperty("reactionJitterSeconds").floatValue=.08f;}});
        var kind=root.GetComponent<LarperBrain>().Archetype;
        if(root.name=="Larper"||kind!=null&&kind.name=="Larper")Edit(c,s=>{s.FindProperty("reactionSeconds").floatValue=.7f;s.FindProperty("reactionJitterSeconds").floatValue=.12f;});
    }
    public static string Critical()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode; preserve unsaved scene");
        foreach(var guid in AssetDatabase.FindAssets("t:Prefab",new[]{"Assets/Prefabs/Enemies","Assets/Data/FirstOperation"}))
        {
            string path=AssetDatabase.GUIDToAssetPath(guid);var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(asset.GetComponent<EnemyCombat>()==null)continue;
            var root=PrefabUtility.LoadPrefabContents(path);try{Awareness(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}
        }
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");var roots=scene.GetRootGameObjects();foreach(var c in roots.SelectMany(g=>g.GetComponentsInChildren<EnemyCombat>(true)))Awareness(c.gameObject);
        var bunker=roots.Single(g=>g.name=="CowardsBunker").transform;
        foreach(string name in new[]{"OfficerBarricade","OfficerRetreatCover","BunkerRoof"})
        {var r=roots.SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).Single(r=>r.name==name);r.sortingOrder=20;r.gameObject.layer=0;var box=r.GetComponent<BoxCollider2D>();box.isTrigger=false;}
        foreach(string name in new[]{"FrontTimber","RearTimber"})
        {
            var source=bunker.Find(name);source.GetComponent<SpriteRenderer>().color=new Color(.5f,.55f,.55f,1); // Receding back plane, not a foreground wall.
            string capName=name+"SolidHeader";var cap=bunker.Find(capName);if(cap==null){var g=new GameObject(capName);g.transform.SetParent(bunker,false);cap=g.transform;g.layer=0;g.AddComponent<SpriteRenderer>();g.AddComponent<BoxCollider2D>();g.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Wood);}
            cap.position=new Vector2(source.position.x,4.5f);var r=cap.GetComponent<SpriteRenderer>();r.sprite=source.GetComponent<SpriteRenderer>().sprite;r.drawMode=SpriteDrawMode.Tiled;r.size=new Vector2(.375f,.375f);r.sortingOrder=20;cap.GetComponent<BoxCollider2D>().size=r.size;
        }
        var officer=roots.SelectMany(g=>g.GetComponentsInChildren<OfficerShotTelegraph>(true)).Single();Edit(officer,s=>s.FindProperty("bunkerRetreatChance").floatValue=.9f);
        // Start inside the genuine roof/cover footprint, with an open walking doorway.
        var point=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>().Encounters[4].Points[0];point.transform.position=new Vector2(142,point.transform.position.y);officer.transform.position=new Vector2(142,officer.transform.position.y);officer.GetComponent<Rigidbody2D>().position=officer.transform.position;
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Bunker foreground collision surfaces, sheltered Officer occupancy, conservative hearing and reaction tuning saved. Background assets untouched.";
    }
    static Sprite Helmet(bool heavy)
    {
        string folder="Assets/Art/Equipment0029";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Equipment0029");string name=heavy?"HeavyHelmet":"LightHelmet",path=folder+"/"+name+".asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprite!=null)return sprite;
        int w=heavy?14:12,h=heavy?6:5;var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        Color clear=Color.clear,ink=new Color32(34,41,46,255),main=heavy?(Color)new Color32(79,91,82,255):new Color32(95,112,60,255);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){bool inside=y<h-2||x>=2&&x<w-2;if(y==h-1)inside=x>=3&&x<w-3;t.SetPixel(x,y,!inside?clear:y==0||x==0||x==w-1?ink:main);}t.Apply();AssetDatabase.CreateAsset(t,folder+"/"+name+"Pixels.asset");sprite=Sprite.Create(t,new Rect(0,0,w,h),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    public static string Equipment()
    {
        if(EditorApplication.isPlaying||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Stop Play Mode; preserve unsaved scene");
        var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset");var list=catalog.offers.ToList();var light=list.First(o=>o.category==ShopCategory.BodyArmor&&!o.helmet);light.title="Light body armor";light.armorTier=1;light.quantity=60;light.description="A modest ballistic vest. Small reload cost; movement stays fast.";
        foreach(var row in new[]{("heavy-armor","Heavy body armor",false,2,Mathf.Max(180,light.price+60)),("light-helmet","Light helmet",true,1,60),("heavy-helmet","Heavy helmet",true,2,100)})
        {var item=list.FirstOrDefault(o=>o.id==row.Item1);if(item==null){item=new ShopOffer{id=row.Item1};list.Add(item);}item.title=row.Item2;item.category=ShopCategory.BodyArmor;item.helmet=row.Item3;item.armorTier=row.Item4;item.price=row.Item5;item.quantity=Mathf.RoundToInt(catalog.Protection(item.helmet,item.armorTier).capacity);item.description=item.helmet?"Ballistic head protection. Sword attacks still hurt; heavier equipment reloads more slowly.":"Stronger ballistic protection. Slower reloads and modest carrying costs; movement stays fast.";}
        var tape=list.Single(o=>o.category==ShopCategory.DuctTape);tape.title="Duct Tape";tape.description="There is nothing duct tape can't fix.";var faintanyl=list.Single(o=>o.category==ShopCategory.Faintanyl);faintanyl.title="Faintanyl";faintanyl.description="Relax, it's not addictive. It's just habit forming.";catalog.offers=list.ToArray();EditorUtility.SetDirty(catalog);
        var lightSprite=Helmet(false);var heavySprite=Helmet(true);string playerPath="Assets/Prefabs/Player/WarehousePlayer.prefab";var root=PrefabUtility.LoadPrefabContents(playerPath);
        try
        {
            var rig=root.GetComponentInChildren<CutoutRig>(true);var head=rig.head;var t=head.Find("BallisticHelmet");if(t==null){t=new GameObject("BallisticHelmet").transform;t.SetParent(head,false);t.gameObject.AddComponent<SpriteRenderer>();}t.localPosition=new Vector3(0,.125f,-.01f);var r=t.GetComponent<SpriteRenderer>();r.enabled=false;r.sortingOrder=head.GetComponent<SpriteRenderer>().sortingOrder+1;r.sortingLayerID=head.GetComponent<SpriteRenderer>().sortingLayerID;r.sharedMaterial=head.GetComponent<SpriteRenderer>().sharedMaterial;
            Edit(root.GetComponent<PlayerRevive>(),s=>{s.FindProperty("initialInjections").intValue=0;s.FindProperty("automaticRevival").boolValue=true;});PrefabUtility.SaveAsPrefabAsset(root,playerPath);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");var player=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>().Player;var av=player.GetComponent<ArmorVisual>();var overlay=player.GetComponentInChildren<CutoutRig>(true).head.Find("BallisticHelmet").GetComponent<SpriteRenderer>();Edit(av,s=>{s.FindProperty("lightHelmet").objectReferenceValue=lightSprite;s.FindProperty("heavyHelmet").objectReferenceValue=heavySprite;s.FindProperty("helmetOverlay").objectReferenceValue=overlay;});Edit(player.GetComponent<PlayerRevive>(),s=>{s.FindProperty("automaticRevival").boolValue=true;s.FindProperty("initialInjections").intValue=0;});EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();return "Four protection choices, exact Medical copy, purchased-only automatic revivals and native helmet overlays saved.";
    }
}
