using System.Linq;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class Sprint0245Preview
{
    private static void Label(UnityEngine.SceneManagement.Scene scene,string text,Vector2 p,float size=.025f)
    {var g=new GameObject("ReviewLabel");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(g,scene);g.layer=31;g.transform.position=p;var t=g.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=48;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=Color.white;g.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;g.GetComponent<MeshRenderer>().sortingOrder=30;}
    private static CutoutRig Actor(UnityEngine.SceneManagement.Scene scene,string name,Vector2 p,CharacterPose pose,float facing=1,bool reload=false)
    {
        var root=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab"),scene);root.transform.position=p;
        foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
        var r=root.GetComponentInChildren<CutoutRig>(true);r.ApplySkin(r.skin);
        if(name=="Larper"){var data=new SerializedObject(root.GetComponent<InsaneDreams.Enemies.EnemyCombat>());var definition=(InsaneDreams.Inventory.WeaponDefinition)data.FindProperty("firearm").objectReferenceValue;r.weapon.sprite=definition.EquippedSprite;}
        r.Pose(pose,1,new Vector2(facing,0),facing,0,FaceExpression.Neutral,true,true,reload,.5f);if(name=="Samurai")r.Segmented.Pose(pose,1,pose==CharacterPose.Idle?65:-35,pose!=CharacterPose.Jump,0);return r;
    }
    public static string Sheet()
    {
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            Label(scene,"AKUMU 2 / SPRINT 024.5 — CANDIDATE, AWAITING ARTISTIC APPROVAL",new Vector2(0,4.9f),.032f);
            var poses=new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Crouch,CharacterPose.Climb,CharacterPose.Panic};
            for(int i=0;i<6;i++){Actor(scene,"Larper",new Vector2(-6.5f+i*2.6f,3.3f),poses[i],i==2?-1:1,i==4);Label(scene,i==4?"Climb / reload":poses[i].ToString(),new Vector2(-6.5f+i*2.6f,2.35f));}
            string[] expressions={"Neutral","Overconfident","Angry","Nervous","Terrified / panic","Stunned"};
            for(int i=0;i<6;i++){var r=Actor(scene,"Larper",new Vector2(-6.5f+i*2.6f,1),CharacterPose.Idle);r.SetExpression((FaceExpression)i);Label(scene,expressions[i],new Vector2(-6.5f+i*2.6f,.05f));}
            for(int i=0;i<3;i++){Actor(scene,"Samurai",new Vector2(-6.5f+i*2.6f,-1.5f),i==0?CharacterPose.Idle:i==1?CharacterPose.Jump:CharacterPose.Walk);Label(scene,new[]{"Samurai wind-up","Jump / katana grip","Run"}[i],new Vector2(-6.5f+i*2.6f,-2.5f));}
            for(int i=0;i<3;i++){var r=Actor(scene,"Larper",new Vector2(1.3f+i*2.6f,-1.5f),CharacterPose.Idle);var g=new GameObject("Vest");g.layer=31;g.transform.SetParent(r.torso,false);var s=g.AddComponent<SpriteRenderer>();s.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0245/Armor_"+i+".asset");s.sharedMaterial=r.weapon.sharedMaterial;s.sortingOrder=11;Label(scene,new[]{"Armor intact","Damaged","Broken (same run)"}[i],new Vector2(1.3f+i*2.6f,-2.5f));}
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab").GetComponentInChildren<CutoutRig>();var j=prefab.Segmented;
            var parts=new[]{prefab.skin.head,prefab.skin.torso,j.upperArm,j.forearm,j.hand,j.thigh,j.shin,j.foot};string[] names={"Head","Torso","Upper arm","Forearm","Hand / wrist","Thigh","Shin / knee","Foot / ankle"};
            for(int i=0;i<parts.Length;i++){var g=new GameObject("Component");g.layer=31;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(g,scene);g.transform.position=new Vector2(-7+i*2,-3.5f);var r=g.AddComponent<SpriteRenderer>();r.sprite=parts[i];r.sharedMaterial=prefab.weapon.sharedMaterial;Label(scene,names[i],new Vector2(-7+i*2,-4.3f),.023f);}
            Label(scene,"32 PPU • flat 8-color atlas • originals preserved • modular joints, no gameplay colliders",new Vector2(0,-4.9f),.028f);
            var camera=new GameObject("ReviewCamera").AddComponent<Camera>();UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(camera.gameObject,scene);camera.scene=scene;camera.cullingMask=1<<31;camera.orthographic=true;camera.orthographicSize=5.4f;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color(.16f,.18f,.21f);camera.clearFlags=CameraClearFlags.SolidColor;
            return Save(camera,1920,1200,"Sprint0245_ReferenceSheet.png");
        }finally{EditorSceneManager.ClosePreviewScene(scene);}
    }
    public static string Save(Camera camera,int w,int h,string name)
    {var old=RenderTexture.active;var target=camera.targetTexture;var rt=new RenderTexture(w,h,24);var t=new Texture2D(w,h,TextureFormat.RGBA32,false);try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();string path=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../TestResults/"+name));System.IO.File.WriteAllBytes(path,t.EncodeToPNG());return path;}finally{camera.targetTexture=target;RenderTexture.active=old;Object.DestroyImmediate(t);Object.DestroyImmediate(rt);}}
}
