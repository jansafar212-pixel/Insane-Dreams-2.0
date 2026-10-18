using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Sprint012CombatSetup
{
    private static Sprite Pixel(string name,int width,int height,Vector2 pivot,System.Func<int,int,Color> paint)
    {
        const string folder="Assets/Art/Sprint012";
        if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Sprint012");
        var image=new Texture2D(width,height,TextureFormat.RGBA32,false);
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)image.SetPixel(x,y,paint(x,y));
        image.Apply();string path=folder+"/"+name+".png";File.WriteAllBytes(path,image.EncodeToPNG());Object.DestroyImmediate(image);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=32;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;importer.wrapMode=TextureWrapMode.Clamp;
        var settings=new TextureImporterSettings();importer.ReadTextureSettings(settings);settings.spriteAlignment=(int)SpriteAlignment.Custom;settings.spritePivot=pivot;settings.spriteMeshType=SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    private static void Edit(Object target,System.Action<SerializedObject> edit)
    {var data=new SerializedObject(target);edit(data);data.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);}
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
        Color ink=new Color32(24,28,43,255),steel=new Color32(83,109,122,255),light=new Color32(166,184,183,255),gold=new Color32(239,184,62,255);
        var gun=Pixel("BuzzboxPistol",20,8,new Vector2(.2f,.5f),(x,y)=>
        {if(y>=3&&y<=6)return y==6?light:x==19||x==0?ink:x<6?gold:steel;if(x>=3&&x<=6&&y<3)return x==3||y==0?ink:gold;if(x>=9&&x<=12&&y==2)return ink;return Color.clear;});
        var mag=Pixel("RidiculousMagazine",8,18,new Vector2(.5f,1),(x,y)=>x==0||x==7||y==0?ink:y<3?gold:y%4==0?light:steel);
        const string weaponPath="Assets/Data/Items/LarperBuzzbox.asset";
        var weapon=AssetDatabase.LoadAssetAtPath<WeaponDefinition>(weaponPath);
        if(weapon==null){weapon=ScriptableObject.CreateInstance<WeaponDefinition>();AssetDatabase.CreateAsset(weapon,weaponPath);}
        Edit(weapon,s=>{if(string.IsNullOrEmpty(s.FindProperty("stableId").stringValue))s.FindProperty("stableId").stringValue=System.Guid.NewGuid().ToString("N");
            s.FindProperty("displayName").stringValue="Buzzbox Automatic Pistol";s.FindProperty("description").stringValue="Fictional bargain automatic pistol with an unnecessarily long magazine. Original Sprint 012 pixel patterns.";
            s.FindProperty("weaponCategory").enumValueIndex=0;s.FindProperty("damage").floatValue=5;s.FindProperty("firingInterval").floatValue=.105f;
            s.FindProperty("magazineCapacity").intValue=18;s.FindProperty("reloadDuration").floatValue=2.6f;s.FindProperty("firearmRange").floatValue=18;
            s.FindProperty("equippedSprite").objectReferenceValue=gun;s.FindProperty("magazineSprite").objectReferenceValue=mag;});
        string prefabPath="Assets/Prefabs/Enemies/Larper.prefab";var root=PrefabUtility.LoadPrefabContents(prefabPath);
        try{
            Edit(root.GetComponent<EnemyCombat>(),s=>{s.FindProperty("firearm").objectReferenceValue=weapon;s.FindProperty("initialReserveAmmo").intValue=54;
                s.FindProperty("closeSpread").floatValue=6;s.FindProperty("distantSpread").floatValue=32;s.FindProperty("effectiveRange").floatValue=11;s.FindProperty("preferredDistance").floatValue=6;
                s.FindProperty("withdrawalDistance").floatValue=3.5f;s.FindProperty("minimumWithdrawalDistance").floatValue=2.5f;s.FindProperty("withdrawalSeconds").floatValue=1.8f;});
            var rig=root.GetComponentInChildren<CutoutRig>(true);rig.weapon.sprite=gun;rig.SetMagazine(mag);rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral);
            PrefabUtility.SaveAsPrefabAsset(root,prefabPath);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        foreach(string name in new[]{"Ordinary","Cowardly","Overconfident"})
        {
            var profile=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+name+".asset");
            if(profile==null)continue;
            Edit(profile,s=>{
                string[] reload=name=="Overconfident"?new[]{"RELOADING! ALL PART OF THE PLAN!","I'M CONSERVING RESOURCES!","WHY IS THIS THING SO LONG?!"}:name=="Cowardly"?new[]{"WAIT! I'M RELOADING!","STOP SHOOTING!","THIS MAG HATES ME!"}:new[]{"RELOADING!","COVER ME, I'M RELOADING!","MY MAG'S EMPTY!","NOBODY SHOOT ME, I'M BUSY!"};
                string[] retreat=name=="Overconfident"?new[]{"TACTICAL REPOSITIONING!","I'M NOT RUNNING!","STRATEGIC WITHDRAWAL!"}:name=="Cowardly"?new[]{"I'M GETTING OUT OF HERE!","I NEED COVER!","I DIDN'T SIGN UP FOR THIS!"}:new[]{"FALL BACK!","COVER ME!","THIS POSITION SUCKS!"};
                foreach(var pair in new[]{("reload",reload),("withdraw",retreat),("blocked",new[]{"NO ROOM! RELOADING HERE!","BAD PLAN! HOLD ON!"})}){var array=s.FindProperty(pair.Item1);array.arraySize=pair.Item2.Length;for(int i=0;i<pair.Item2.Length;i++)array.GetArrayElementAtIndex(i).stringValue=pair.Item2[i];}
            });
        }
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();if(scene.path!="Assets/Scenes/Warehouse_Testbed.unity")throw new System.InvalidOperationException("Open Warehouse_Testbed first.");
        var cover=scene.GetRootGameObjects().FirstOrDefault(g=>g.name=="LowCover_Sprint012");if(cover==null)cover=new GameObject("LowCover_Sprint012");
        cover.transform.position=new Vector3(-14,.45f,0);var box=cover.GetComponent<BoxCollider2D>();if(box==null)box=cover.AddComponent<BoxCollider2D>();box.size=new Vector2(1.6f,.9f);
        const string crate="Assets/Art/Sprint008/ShippingCrate.png";
        var sr=cover.GetComponent<SpriteRenderer>();if(sr==null)sr=cover.AddComponent<SpriteRenderer>();sr.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(crate);sr.drawMode=SpriteDrawMode.Tiled;sr.size=box.size;sr.sortingOrder=2;
        var parent=scene.GetRootGameObjects().First(g=>g.name=="Enemies").transform;
        var positions=new[]{new Vector2(6,.85f),new Vector2(18,.85f),new Vector2(-6,6.25f),new Vector2(16,12.85f)};
        for(int i=0;i<positions.Length;i++)
        {
            string name="Sprint012_Encounter_"+i;var child=parent.Find(name);if(child==null){child=new GameObject(name).transform;child.SetParent(parent);}
            child.position=positions[i];var point=child.GetComponent<EnemySpawnPoint>();if(point==null)point=child.gameObject.AddComponent<EnemySpawnPoint>();
            Edit(point,s=>{s.FindProperty("archetype").objectReferenceValue=AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset");s.FindProperty("spawnOnStart").boolValue=true;s.FindProperty("spawnedParent").objectReferenceValue=parent;s.FindProperty("personality").objectReferenceValue=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/"+new[]{"Ordinary","Cowardly","Overconfident","Ordinary"}[i]+".asset");});
        }
        var points=Object.FindObjectsByType<EnemySpawnPoint>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        Edit(Object.FindAnyObjectByType<InsaneDreams.Level.LevelEntry>(),s=>{var array=s.FindProperty("encounters");array.arraySize=points.Length;for(int i=0;i<points.Length;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=points[i];});
        AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        return "Buzzbox: 18 rounds / .105s / 5 damage / 2.6s reload; original pistol and magazine pixels; low cover .9 high; four extra spawn points.";
    }
}
