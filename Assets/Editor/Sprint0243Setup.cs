using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEngine;

public static class Sprint0243Setup
{
    private static void Float(Object target,string field,float value)
    {var s=new SerializedObject(target);s.FindProperty(field).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(target);}
    public static void Apply()
    {
        const string atlas="Assets/Art/Sprint0243/Samurai_Modular.png";
        AssetDatabase.ImportAsset(atlas);
        var importer=(TextureImporter)AssetImporter.GetAtPath(atlas);
        importer.textureType=TextureImporterType.Default;importer.isReadable=true;importer.alphaIsTransparency=true;
        importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(atlas);var pixels=texture.GetPixels32();
        // Bounds isolate the five authored columns; crop transparent padding only.
        int[] columns={0,480,920,1160,1450,2168};
        string[] names={"Head","Torso","Arm","Leg","Katana"};
        float[] heights={11f/32,17f/32,18f/32,18f/32,0};
        var sprites=new Sprite[5];
        for(int i=0;i<5;i++)
        {
            string path="Assets/Art/Sprint0243/Samurai_"+names[i]+".asset";
            sprites[i]=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(sprites[i]!=null)continue;
            int minX=texture.width,minY=texture.height,maxX=0,maxY=0;
            for(int y=0;y<texture.height;y++)for(int x=columns[i];x<Mathf.Min(columns[i+1],texture.width);x++)
                if(pixels[y*texture.width+x].a>128){minX=Mathf.Min(x,minX);maxX=Mathf.Max(x,maxX);minY=Mathf.Min(y,minY);maxY=Mathf.Max(y,maxY);}
            var rect=new Rect(minX,minY,maxX-minX+1,maxY-minY+1);
            float ppu=i==4?rect.width/1.25f:rect.height/heights[i];
            Vector2 pivot=i==2||i==3?new Vector2(.5f,1):i==4?new Vector2(.12f,.5f):new Vector2(.5f,.5f);
            sprites[i]=Sprite.Create(texture,rect,pivot,ppu,0,SpriteMeshType.FullRect);sprites[i].name="Samurai_"+names[i];AssetDatabase.CreateAsset(sprites[i],path);
        }
        var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/SamuraiSkin.asset");
        skin.head=sprites[0];skin.torso=sprites[1];skin.arm=sprites[2];skin.leg=sprites[3];skin.weapon=skin.meleeWeapon=sprites[4];skin.clothing=Color.white;
        skin.provenance="Sprint 024.3: original AI-generated modular pixel art, imagegen, existing AKUMU proportions as reference. Embedded visible eyes; original face overlays disabled on Samurai. No third-party character artwork.";
        EditorUtility.SetDirty(skin);AssetDatabase.SaveAssetIfDirty(skin);
        var sword=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/SamuraiSword.asset");
        Float(sword,"damage",40);Float(sword,"firingInterval",1.1f);
        var ws=new SerializedObject(sword);ws.FindProperty("equippedSprite").objectReferenceValue=sprites[4];ws.FindProperty("description").stringValue="Committed close-range katana strike. Configurable Samurai melee damage.";ws.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssetIfDirty(sword);
        const string prefab="Assets/Prefabs/Enemies/Samurai.prefab";var root=PrefabUtility.LoadPrefabContents(prefab);
        try
        {
            Float(root.GetComponent<SamuraiCombat>(),"windupSeconds",.4f);Float(root.GetComponent<SamuraiCombat>(),"recoverySeconds",.7f);
            Float(root.GetComponent<EnemyCombat>(),"reactionSeconds",.18f);Float(root.GetComponent<EnemyCombat>(),"reengagementDelay",.2f);
            var rig=root.GetComponentInChildren<CutoutRig>(true);rig.ApplySkin(skin);rig.face.enabled=false;rig.SetMagazine(null);
            foreach(var t in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="RedHeadband"||t.name=="Blade").ToArray())Object.DestroyImmediate(t.gameObject);
            rig.weapon.transform.localScale=Vector3.one;
            PrefabUtility.SaveAsPrefabAsset(root,prefab);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
        importer.isReadable=false;importer.SaveAndReimport();
    }
    public static void PlaceDemoSamurai()
    {
        var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");
        var actor=Object.FindAnyObjectByType<SamuraiCombat>(FindObjectsInactive.Include);
        var point=GameObject.Find("Storage_Spawn_1");
        if(actor==null||point==null)throw new System.InvalidOperationException("Demo Samurai binding missing");
        // Existing mezzanine opens onto the authored step route; keep the same resident and encounter.
        actor.transform.position=point.transform.position=new Vector3(-.5f,6.11f,0);
        PrefabUtility.RecordPrefabInstancePropertyModifications(actor.transform);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
    }
    public static string RenderRigPreview()
    {
        var scene=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();RenderTexture rt=null;Texture2D output=null;Camera camera=null;var previous=RenderTexture.active;
        try
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");
            var poses=new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Idle};
            for(int i=0;i<4;i++)
            {
                var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);root.transform.position=new Vector3(-3+i*2,0,0);
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                var rig=root.GetComponentInChildren<CutoutRig>(true);rig.ApplySkin(rig.skin);rig.face.enabled=false;
                rig.Pose(poses[i],.8f,Vector2.right,1,0,FaceExpression.Angry,true,true);
                rig.PoseSamurai(i==3?65:i==2?-35:15);
            }
            var go=new GameObject("SamuraiRigPreviewCamera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,scene);
            camera=go.AddComponent<Camera>();camera.scene=scene;camera.transform.position=new Vector3(0,.3f,-10);camera.orthographic=true;camera.orthographicSize=1.9f;camera.cullingMask=1<<31;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.28f,.3f,.34f);
            rt=new RenderTexture(1280,512,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            output=new Texture2D(1280,512,TextureFormat.RGBA32,false);output.ReadPixels(new Rect(0,0,1280,512),0,0);output.Apply();
            string path=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../TestResults/Sprint0243_RigPreview.png"));System.IO.File.WriteAllBytes(path,output.EncodeToPNG());return path;
        }
        finally{RenderTexture.active=previous;if(camera!=null)camera.targetTexture=null;if(rt!=null)Object.DestroyImmediate(rt);if(output!=null)Object.DestroyImmediate(output);UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);}
    }
}
