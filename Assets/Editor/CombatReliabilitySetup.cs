using System;
using System.Linq;
using InsaneDreams.Inventory;
using InsaneDreams.Enemies;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Object=UnityEngine.Object;

public static class CombatReliabilitySetup
{
    const string Folder="Assets/Art/ReloadRepair";
    static Sprite Save(string name,Texture2D t,Vector2 pivot)
    {
        var s=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"/"+name+".asset");if(s!=null){Object.DestroyImmediate(t);return s;}
        t.name=name+"Pixels";t.filterMode=FilterMode.Point;t.wrapMode=TextureWrapMode.Clamp;t.Apply();AssetDatabase.CreateAsset(t,Folder+"/"+name+"Pixels.asset");
        s=Sprite.Create(t,new Rect(0,0,t.width,t.height),pivot,32,0,SpriteMeshType.FullRect);s.name=name;AssetDatabase.CreateAsset(s,Folder+"/"+name+".asset");return s;
    }
    static void Exchange(WeaponDefinition w,Sprite body,Sprite icon,Sprite mag,Vector2 socket)
    {
        var s=new SerializedObject(w);s.FindProperty("equippedSprite").objectReferenceValue=body;s.FindProperty("icon").objectReferenceValue=icon;
        s.FindProperty("magazineSprite").objectReferenceValue=mag;s.FindProperty("magazineMountOffset").vector2Value=socket;s.FindProperty("physicalMagazineReload").boolValue=true;s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(w);
    }
    public static string Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty)throw new InvalidOperationException("Preserve unsaved scene work first.");
        var lmg=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/LMG.asset");var original=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Arsenal/LMGSprite.asset");var t=Object.Instantiate(original.texture);
        for(int y=1;y<=6;y++)for(int x=14;x<=19;x++)t.SetPixel(x,y,Color.clear);
        var body=Save("LmgBody",t,new Vector2(9f/37,5f/14));t=new Texture2D(6,6,TextureFormat.RGBA32,false);
        for(int y=0;y<6;y++)for(int x=0;x<6;x++)t.SetPixel(x,y,original.texture.GetPixel(x+14,y+1));
        Exchange(lmg,body,original,Save("LmgContainer",t,new Vector2(.5f,1)),new Vector2(8f/32,2f/32));
        var de=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/DesertEagle.asset");t=new Texture2D(6,7,TextureFormat.RGBA32,false);
        for(int y=0;y<7;y++)for(int x=0;x<6;x++){int edge=y*2/6;t.SetPixel(x,y,x<edge||x>edge+3?Color.clear:new Color32(25,29,35,255));}
        Exchange(de,de.EquippedSprite,de.Icon,Save("DesertEagleMagazine",t,new Vector2(.5f,1)),new Vector2(-1f/32,2f/32));
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity");
        // Local storage encounter only: shared AI, damage and population stay intact.
        string[] actors={"02_Verticality_Larper_0","02_Verticality_Larper_1","02_Verticality_Larper_2"};
        for(int i=0;i<actors.Length;i++)
        {
            var e=GameObject.Find(actors[i]).GetComponent<EnemyCombat>();var s=new SerializedObject(e);
            s.FindProperty("viewportAttackDelay").floatValue=new[]{.3f,.55f,.85f}[i];s.FindProperty("detectionRange").floatValue=new[]{18f,16f,14f}[i];s.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(e);
        }
        // The second arrival guard's inherited22-unit detection activated it
        // during the first fight. It still reacts normally as the player advances.
        var arrival=GameObject.Find("01_Warmup_Larper_1").GetComponent<EnemyCombat>();
        var arrivalData=new SerializedObject(arrival);arrivalData.FindProperty("detectionRange").floatValue=10;arrivalData.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(arrival);
        var template=GameObject.Find("StorageLowCover");
        foreach(var entry in new[]{("StorageEntryVerticalCover",16f,0f),("StorageLadderRetreatCover",23.5f,3.2f),("StorageUpperApproachCover",28f,6.4f)})
        {
            if(GameObject.Find(entry.Item1)!=null)continue;
            var g=Object.Instantiate(template,template.transform.parent);g.name=entry.Item1;
            g.transform.position=new Vector3(entry.Item2,entry.Item3+g.GetComponent<BoxCollider2D>().size.y*.5f,template.transform.position.z);
        }
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        return "LMG original 6x6 container extracted; DE 6x7 black magazine; three local storage cover options and graduated detection/viewport reaction; combat tuning preserved.";
    }
}
