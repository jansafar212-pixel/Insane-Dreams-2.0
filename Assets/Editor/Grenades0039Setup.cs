using InsaneDreams.Combat;
using UnityEditor;
using UnityEngine;

public static class Grenades0039Setup
{
    public const string MinePath="Assets/Prefabs/Hazards/Landmine.prefab";
    const string Art="Assets/Art/Sprint0039Hazards";
    static Sprite MineSprite()
    {
        var existing=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/Landmine.asset");if(existing!=null)return existing;
        if(!AssetDatabase.IsValidFolder(Art))AssetDatabase.CreateFolder("Assets/Art","Sprint0039Hazards");
        // Same small flat palette as the current grenade and industrial prop family.
        string[] rows={"................",".....######.....","...##mmmmmm##...","..#mmmmyymmmm#..",".#mmmmmyymmmmm#.","#mmmrrmmmmrrmmm#","################","................"};
        var t=new Texture2D(16,8,TextureFormat.RGBA32,false){name="LandminePixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
        for(int y=0;y<8;y++)for(int x=0;x<16;x++)t.SetPixel(x,y,rows[7-y][x] switch{'#'=>new Color32(25,29,43,255),'m'=>new Color32(115,137,77,255),'y'=>new Color32(185,156,79,255),'r'=>new Color32(137,92,62,255),_=>new Color32()});
        t.Apply();AssetDatabase.CreateAsset(t,Art+"/LandminePixels.asset");
        var sprite=Sprite.Create(t,new Rect(0,0,16,8),new Vector2(.5f,0),32,0,SpriteMeshType.FullRect);sprite.name="Landmine";AssetDatabase.CreateAsset(sprite,Art+"/Landmine.asset");return sprite;
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new System.InvalidOperationException("Stop Play Mode first");
        const string projectilePath="Assets/Data/Sprint0027/Throwable.prefab";
        var root=PrefabUtility.LoadPrefabContents(projectilePath);
        try
        {
            var s=new SerializedObject(root.GetComponent<ThrowableProjectile2D>());
            s.FindProperty("blastRadius").floatValue=4.5f;s.FindProperty("fragmentationDamage").floatValue=145;
            s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,projectilePath);
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        if(!AssetDatabase.IsValidFolder("Assets/Prefabs/Hazards"))AssetDatabase.CreateFolder("Assets/Prefabs","Hazards");
        var sprite=MineSprite();
        // A static trigger plus a queryable plate; no Rigidbody on the placed mine.
        var g=new GameObject("Landmine");g.layer=2;
        try
        {
            var r=g.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=3;
            var trigger=g.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(.65f,.36f);trigger.offset=new Vector2(0,.16f);
            var plate=new GameObject("ShootablePlate");plate.layer=2;plate.transform.SetParent(g.transform,false);
            var hit=plate.AddComponent<BoxCollider2D>();hit.size=new Vector2(.5f,.18f);hit.offset=new Vector2(0,.11f);
            // Still found by hitscan, without obstructing actors or adding a physics body.
            hit.excludeLayers=~0;
            var health=g.AddComponent<Health>();var hs=new SerializedObject(health);hs.FindProperty("maximum").floatValue=1;hs.FindProperty("team").enumValueIndex=(int)CombatTeam.Neutral;hs.ApplyModifiedPropertiesWithoutUndo();
            var mine=g.AddComponent<Landmine2D>();var so=new SerializedObject(mine);so.FindProperty("explosion").objectReferenceValue=AssetDatabase.LoadAssetAtPath<GameObject>(projectilePath).GetComponent<ThrowableProjectile2D>();so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(g,MinePath);
        }
        finally{Object.DestroyImmediate(g);}
        AssetDatabase.SaveAssets();
        return "Frag145/radius4.5; existing flash/smoke/presentation unchanged. Static shootable mine prefab prepared; ZERO scenes modified.";
    }
}
