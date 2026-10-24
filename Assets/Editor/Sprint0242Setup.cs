using System;
using System.Linq;
using InsaneDreams.Checkpoints;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;

public static class Sprint0242Setup
{
    private static void Edit(Object obj,Action<SerializedObject> action){var s=new SerializedObject(obj);action(s);s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(obj);}
    private static Sprite Square=>AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prototype/PlaceholderSquare.png");
    private static Sprite PixelSprite(string name,int w,int h,Func<int,int,Color> pixel)
    {
        string path="Assets/Art/Sprint0242/"+name+".asset";
        var existing=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(existing!=null)return existing;
        var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)texture.SetPixel(x,y,pixel(x,y));texture.Apply();
        AssetDatabase.CreateAsset(texture,path);var sprite=Sprite.Create(texture,new Rect(0,0,w,h),new Vector2(.5f,.5f),32);sprite.name=name;AssetDatabase.AddObjectToAsset(sprite,texture);return sprite;
    }
    public static void AssetsAndGeometry()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Art/Sprint0242"))AssetDatabase.CreateFolder("Assets/Art","Sprint0242");
        var bundle=PixelSprite("QuestionableBundle",22,26,(x,y)=>
        {
            if(x<2||x>19||y<2||y>23)return Color.clear;
            if(y>=10&&y<=13)return x==2||x==19?new Color(.15f,.12f,.2f):new Color(.95f,.8f,.36f);
            int tube=(x-2)/6,dx=(x-2)%6;int top=tube==1?23:20;
            if(y>top||dx==5)return Color.clear;if(dx==0||dx==4||y==2||y==top)return new Color(.13f,.13f,.2f);
            if((y+2*tube)%7==0)return Color.white;
            return tube==0?new Color(1,.25f,.4f):tube==1?new Color(.2f,.85f,1):new Color(.75f,.35f,1);
        });
        var crate=PixelSprite("AmmoSupplyCrate",32,20,(x,y)=>
        {
            if(x<1||x>30||y<1||y>18)return Color.clear;
            if(x==1||x==30||y==1||y==18)return new Color(.12f,.2f,.17f);
            if(y==15||x==5||x==26)return new Color(.5f,.65f,.35f);
            if(x>=10&&x<=22&&y>=6&&y<=12&&(x-10)%5<3)return new Color(1,.82f,.3f);
            return new Color(.25f,.36f,.23f);
        });
        var flag=PixelSprite("CheckpointFlag",20,14,(x,y)=>x<2||y<1||y>12||x>17-y/3?Color.clear:Color.white);
        var item=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/FireworkLauncher.asset");
        Edit(item,s=>{s.FindProperty("displayName").stringValue="Questionable Fireworks Bundle";s.FindProperty("equippedSprite").objectReferenceValue=bundle;s.FindProperty("magazineSprite").objectReferenceValue=null;});
        var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/FireworksExpertSkin.asset");skin.weapon=bundle;EditorUtility.SetDirty(skin);
        var root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemies/CartoonFirework.prefab");
        try
        {
            if(root.GetComponent<Rigidbody2D>()==null)root.AddComponent<Rigidbody2D>();if(root.GetComponent<CircleCollider2D>()==null)root.AddComponent<CircleCollider2D>();
            root.layer=2;root.GetComponent<Rigidbody2D>().simulated=false;root.GetComponent<CircleCollider2D>().radius=.4f;
            var projectile=root.GetComponent<FireworkProjectile>();var data=new SerializedObject(projectile);var render=(SpriteRenderer)data.FindProperty("rocket").objectReferenceValue;
            render.sprite=bundle;render.transform.localScale=Vector3.one;render.transform.localPosition=Vector3.zero;
            Edit(projectile,s=>{s.FindProperty("effectSprite").objectReferenceValue=Square;s.FindProperty("detonationDelay").floatValue=1.1f;});
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Enemies/CartoonFirework.prefab");
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Enemies/FireworksExpert.prefab");
        try
        {
            foreach(var t in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="LaunchTube").ToArray())Object.DestroyImmediate(t.gameObject);
            var rig=root.GetComponentInChildren<CutoutRig>();rig.weapon.sprite=bundle;rig.weapon.transform.localScale=Vector3.one;
            Edit(root.GetComponent<FireworksCombat>(),s=>s.FindProperty("heldMalfunctionChance").floatValue=.06f);
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Enemies/FireworksExpert.prefab");
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        var personality=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/FireworksExpert.asset");
        Edit(personality,s=>{SetLines(s,"detected",new[]{"MÁM TO POD KONTROLOU!","TO JE CERTIFIKOVANÝ!"});SetLines(s,"malfunction",new[]{"SAKRA!","TOHLE NEBYLO V PLÁNU!","KDE JE TEN NÁVOD?!","TO TAK MĚLO BÝT!"});});
        root=PrefabUtility.LoadPrefabContents("Assets/Prefabs/Pickups/SupplyCrate.prefab");
        try
        {
            var r=root.transform.Find("Crate").GetComponent<SpriteRenderer>();r.sprite=crate;r.transform.localScale=new Vector3(1.1f,1.1f,1);
            root.transform.Find("SupplyBand").gameObject.SetActive(false);
            var label=root.GetComponentInChildren<TextMesh>();label.characterSize=.025f;label.transform.localPosition=new Vector3(0,.48f,0);
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/Prefabs/Pickups/SupplyCrate.prefab");
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        foreach(string path in new[]{"Assets/Scenes/Warehouse_Demo.unity","Assets/Scenes/Warehouse_Mission01.unity","Assets/Scenes/Warehouse_Testbed.unity"})
        {
            var scene=EditorSceneManager.OpenScene(path);var all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            if(path.EndsWith("Warehouse_Demo.unity"))
            {
                var step=all.Single(t=>t.name=="AlternateGapStep");var old=step.position;step.position=new Vector3(10.2f,old.y,old.z);
                var surface=all.FirstOrDefault(t=>t.name=="Surface_AlternateGapStep");if(surface!=null&&!surface.IsChildOf(step))surface.position+=step.position-old;
            }
            var floors=all.Select(t=>t.GetComponent<Collider2D>()).Where(c=>c!=null&&!c.isTrigger&&c.gameObject.layer==0).ToArray();
            foreach(var marker in all.Select(t=>t.GetComponent<CheckpointMarker>()).Where(m=>m!=null).ToArray())
            {
                var old=marker.transform.Find("Sprint0242_Flag");if(old!=null)Object.DestroyImmediate(old.gameObject);
                var holder=new GameObject("Sprint0242_Flag");holder.transform.SetParent(marker.transform,false);
                float ground=floors.Where(c=>c.bounds.min.x<=marker.transform.position.x&&c.bounds.max.x>=marker.transform.position.x&&c.bounds.max.y<=marker.transform.position.y).Select(c=>c.bounds.max.y).DefaultIfEmpty(marker.transform.position.y-.475f).Max();
                holder.transform.localPosition=new Vector3(0,ground-marker.transform.position.y+.475f,0);
                Part(holder.transform,"Post",new Vector2(-.72f,.15f),new Vector2(.08f,1.25f),new Color(.75f,.8f,.85f));
                var cloth=Part(holder.transform,"Flag",new Vector2(-.42f,.55f),Vector2.one,Color.yellow);cloth.sprite=flag;
                var cue=holder.AddComponent<WorldMarkerCue>();cue.checkpoint=marker;cue.indicator=cloth;
                Edit(marker,s=>s.FindProperty("markerVisual").objectReferenceValue=cloth);
                Label(holder.transform,"CHECKPOINT",new Vector2(-.42f,1.05f),.021f);
                var beacon=marker.transform.Find("CheckpointBeacon");if(beacon!=null)beacon.gameObject.SetActive(false);
            }
            all=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            var mission=all.Select(t=>t.GetComponent<WarehouseMission>()).FirstOrDefault(m=>m!=null);
            if(mission!=null)
            {
                var exit=all.Single(t=>t.GetComponent<MissionTrigger>()?.Zone==MissionZone.Exit);
                var old=exit.Find("Sprint0242_ExitFrame");if(old!=null)Object.DestroyImmediate(old.gameObject);
                var holder=new GameObject("Sprint0242_ExitFrame");holder.transform.SetParent(exit,false);
                Part(holder.transform,"Left",new Vector2(-.72f,0),new Vector2(.12f,2.3f),new Color(.3f,.85f,.9f));
                Part(holder.transform,"Right",new Vector2(.72f,0),new Vector2(.12f,2.3f),new Color(.3f,.85f,.9f));
                var top=Part(holder.transform,"Header",new Vector2(0,1.1f),new Vector2(1.55f,.17f),Color.yellow);
                var cue=holder.AddComponent<WorldMarkerCue>();cue.mission=mission;cue.indicator=top;
                var guide=all.Single(t=>t!=null&&t.name=="ExitGuide").GetComponent<TextMesh>();guide.text="EXIT\nMISSION RESULTS";guide.characterSize=.037f;guide.transform.position=new Vector3(25,3,0);
            }
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
    }
    public static void Population()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");
        var mission=scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<WarehouseMission>()).Single();
        var archetype=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");
        var names=new[]{"Opening_Exposed","Opening_LowCover","Storage_Mixed","Processing_Cover","Exit_Exposed"};
        var groups=new[]{0,0,1,2,2};var positions=new[]{new Vector2(-19.2f,.85f),new Vector2(-14,1.75f),new Vector2(-.2f,.85f),new Vector2(16.2f,6.25f),new Vector2(25,.85f)};
        for(int i=0;i<names.Length;i++)
        {
            var group=mission.Encounters[groups[i]];string name="Sprint0242_"+names[i];
            if(group.Points.Any(p=>p.name==name))continue;
            var template=new SerializedObject(group.Points[0]);var parent=((LarperBrain)template.FindProperty("placedActor").objectReferenceValue).transform.parent;
            var actor=(GameObject)PrefabUtility.InstantiatePrefab(archetype.Prefab,scene);actor.name=name+"_Actor";actor.transform.SetParent(parent,true);actor.transform.position=positions[i];actor.AddComponent<HitReaction2D>();
            var pointObject=new GameObject(name);pointObject.transform.SetParent(group.transform,true);pointObject.transform.position=positions[i];var point=pointObject.AddComponent<EnemySpawnPoint>();
            Edit(point,s=>{s.FindProperty("archetype").objectReferenceValue=archetype;s.FindProperty("spawnOnStart").boolValue=false;s.FindProperty("placedActor").objectReferenceValue=actor.GetComponent<LarperBrain>();s.FindProperty("spawnedParent").objectReferenceValue=parent;s.FindProperty("gameplayCamera").objectReferenceValue=template.FindProperty("gameplayCamera").objectReferenceValue;s.FindProperty("personality").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+(i%2==0?"Cowardly":"Ordinary")+".asset");});
            Edit(group,s=>{var a=s.FindProperty("points");int n=a.arraySize;a.arraySize=n+1;a.GetArrayElementAtIndex(n).objectReferenceValue=point;});
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
    private static void SetLines(SerializedObject s,string field,string[] values){var a=s.FindProperty(field);a.arraySize=values.Length;for(int i=0;i<values.Length;i++)a.GetArrayElementAtIndex(i).stringValue=values[i];}
    private static SpriteRenderer Part(Transform parent,string name,Vector2 position,Vector2 size,Color color)
    {var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=new Vector3(size.x,size.y,1);var r=go.AddComponent<SpriteRenderer>();r.sprite=Square;r.color=color;r.sortingOrder=5;return r;}
    private static void Label(Transform parent,string text,Vector2 position,float size)
    {var go=new GameObject("MarkerLabel");go.transform.SetParent(parent,false);go.transform.localPosition=position;var t=go.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;var r=go.GetComponent<MeshRenderer>();r.sharedMaterial=t.font.material;r.sortingOrder=6;}
}
