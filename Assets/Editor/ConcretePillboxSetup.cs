using System;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object=UnityEngine.Object;
public static class ConcretePillboxSetup
{
    const string Folder="Assets/Art/ConcretePillbox";
    static Sprite Panel(string name,float width,float height,bool dark=false)
    {
        string path=Folder+"/"+name+"_"+Mathf.RoundToInt(width*32)+"x"+Mathf.RoundToInt(height*32)+".asset";var old=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(old!=null)return old;
        int w=Mathf.RoundToInt(width*32),h=Mathf.RoundToInt(height*32);
        var t=new Texture2D(w,h,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        Color32 main=dark?new Color32(28,35,36,255):new Color32(141,148,139,255),shade=dark?main:new Color32(83,94,89,255),light=dark?main:new Color32(180,184,166,255);
        for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=main;if(y<4||x<4||x>=w-4)c=shade;else if(y>=h-4)c=light;
            // A few deliberate coarse chips; no noise or fine shading.
            if(!dark&&w>32&&h>20&&((x>=20&&x<28&&y>=8&&y<12)||(x>=28&&x<32&&y>=12&&y<20)))c=shade;t.SetPixel(x,y,c);}
        t.Apply();AssetDatabase.CreateAsset(t,Folder+"/"+name+"_"+w+"x"+h+"Pixels.asset");var s=Sprite.Create(t,new Rect(0,0,w,h),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.CreateAsset(s,path);return s;
    }
    static GameObject Surface(Transform parent,string name,float x,float y,float w,float h,bool solid=true,bool dark=false,int order=20)
    {
        var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.position=new Vector2(x,y);var r=g.AddComponent<SpriteRenderer>();r.sprite=Panel(name,w,h,dark);r.drawMode=SpriteDrawMode.Tiled;r.size=new Vector2(w,h);r.sortingOrder=order;
        if(solid){g.layer=0;g.AddComponent<BoxCollider2D>().size=new Vector2(w,h);g.AddComponent<ImpactSurface2D>().Configure(ImpactMaterial.Concrete);}return g;
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Preserve unsaved scene and stop Play Mode first");
        if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets/Art","ConcretePillbox");
        var s=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");var roots=s.GetRootGameObjects();
        var officer=roots.SelectMany(g=>g.GetComponentsInChildren<OfficerShotTelegraph>(true)).Single();
        var old=roots.Single(g=>g.name=="CowardsBunker");Object.DestroyImmediate(old);
        foreach(string name in new[]{"OfficerBarricade","OfficerRetreatCover"}){var t=roots.Where(g=>g!=null).SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).FirstOrDefault(t=>t.name==name);if(t!=null)Object.DestroyImmediate(t.gameObject);}
        var root=new GameObject("CowardsBunker");var p=root.transform;
        // Closed side-view shell: one opening in the left/front wall.
        var cover=Surface(p,"ConcreteFrontLower",140.1875f,2.05f,.375f,.9f);
        Surface(p,"ConcreteFrontUpper",140.1875f,3.9f,.375f,1);
        Surface(p,"ConcreteRearWall",144.3125f,3,.375f,2.8f);
        Surface(p,"ConcreteRoof",142.25f,4.5875f,4.5f,.375f);
        Surface(p,"ConcreteBase",142.25f,1.5375f,4.5f,.125f);
        Surface(p,"DarkInterior",142.25f,3,4.5f,2.8f,false,true,-5);
        // Z-facing facade occludes the occupant's legs/ceiling; collision is on
        // the matching exterior shell, leaving room to walk INSIDE the box.
        Surface(p,"FrontFaceLower",142.25f,2.05f,4.5f,.9f,false);
        Surface(p,"FrontFaceUpper",142.25f,3.9f,4.5f,1,false);
        var peek=new GameObject("OfficerFiringHolePosition");peek.transform.SetParent(p,false);peek.transform.position=new Vector2(141.05f,2.33f);
        var hide=new GameObject("OfficerInteriorHide");hide.transform.SetParent(p,false);hide.transform.position=new Vector2(143.25f,2.33f);
        var so=new SerializedObject(officer);so.FindProperty("bunkerPeek").objectReferenceValue=peek.transform;so.FindProperty("bunkerHide").objectReferenceValue=hide.transform;so.FindProperty("bunkerCover").objectReferenceValue=cover.GetComponent<Collider2D>();so.FindProperty("embrasureHeight").vector2Value=new Vector2(2.5f,3.4f);so.ApplyModifiedPropertiesWithoutUndo();
        var mission=Object.FindAnyObjectByType<InsaneDreams.Missions.WarehouseMission>();var point=mission.Encounters[4].Points[0];point.transform.position=hide.transform.position;officer.transform.position=hide.transform.position;officer.GetComponent<Rigidbody2D>().position=hide.transform.position;
        EditorSceneManager.MarkSceneDirty(s);EditorSceneManager.SaveScene(s);AssetDatabase.SaveAssets();return "Concrete pillbox saved: 4.5x3.175 shell, five solid concrete faces, one .9-high front embrasure; interior peek/hide anchors, approved surroundings preserved.";
    }
}
