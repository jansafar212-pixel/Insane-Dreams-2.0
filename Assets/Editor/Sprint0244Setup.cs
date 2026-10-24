using InsaneDreams.Visuals;
using InsaneDreams.Prototype;
using UnityEditor;
using UnityEngine;

public static class Sprint0244Setup
{
    private static Sprite Piece(Sprite source,string name,float bottom,float top)
    {
        string path="Assets/Art/Sprint0244/"+name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null)return existing;
        var r=source.rect;r.y+=source.rect.height*bottom;r.height=source.rect.height*(top-bottom);
        var sprite=Sprite.Create(source.texture,r,new Vector2(.5f,1),source.pixelsPerUnit,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);return sprite;
    }
    private static Transform Segment(Transform parent,string name,Sprite sprite,int order,Color tint)
    {
        var child=parent.Find(name);if(child==null){child=new GameObject(name).transform;child.SetParent(parent,false);}
        var r=child.GetComponent<SpriteRenderer>();if(r==null)r=child.gameObject.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.color=tint;r.sharedMaterial=parent.GetComponent<SpriteRenderer>().sharedMaterial;return child;
    }
    public static void Apply()
    {
        if(!AssetDatabase.IsValidFolder("Assets/Art/Sprint0244"))AssetDatabase.CreateFolder("Assets/Art","Sprint0244");
        var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>("Assets/Data/Visuals/SamuraiSkin.asset");
        var upper=Piece(skin.arm,"Samurai_UpperArm",.48f,1);var fore=Piece(skin.arm,"Samurai_Forearm",.16f,.50f);var hand=Piece(skin.arm,"Samurai_Hand",0,.18f);
        var thigh=Piece(skin.leg,"Samurai_Thigh",.48f,1);var shin=Piece(skin.leg,"Samurai_Shin",.16f,.50f);var foot=Piece(skin.leg,"Samurai_Foot",0,.18f);
        const string path="Assets/Prefabs/Enemies/Samurai.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var rig=root.GetComponentInChildren<CutoutRig>(true);var joints=rig.GetComponent<SamuraiArticulatedRig>();if(joints==null)joints=rig.gameObject.AddComponent<SamuraiArticulatedRig>();
            joints.upperArm=upper;joints.forearm=fore;joints.hand=hand;joints.thigh=thigh;joints.shin=shin;joints.foot=foot;
            joints.leftElbow=Segment(rig.leftArm,"Elbow",fore,9,rig.RearArmTint);joints.leftHand=Segment(joints.leftElbow,"WristHand",hand,9,rig.RearArmTint);
            joints.rightElbow=Segment(rig.rightArm,"Elbow",fore,14,Color.white);joints.rightHand=Segment(joints.rightElbow,"WristHand",hand,17,Color.white);
            joints.leftKnee=Segment(rig.leftLeg,"Knee",shin,8,Color.white);joints.leftFoot=Segment(joints.leftKnee,"AnkleFoot",foot,8,Color.white);
            joints.rightKnee=Segment(rig.rightLeg,"Knee",shin,9,Color.white);joints.rightFoot=Segment(joints.rightKnee,"AnkleFoot",foot,9,Color.white);
            joints.grip=joints.rightHand.Find("SwordGrip");if(joints.grip==null){joints.grip=new GameObject("SwordGrip").transform;joints.grip.SetParent(joints.rightHand,false);}
            var data=new SerializedObject(rig);data.FindProperty("segmented").objectReferenceValue=joints;data.ApplyModifiedPropertiesWithoutUndo();joints.BindSprites();
            var motor=new SerializedObject(root.GetComponent<PlayerMotor2D>());motor.FindProperty("allowAirJump").boolValue=true;motor.ApplyModifiedPropertiesWithoutUndo();
            rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral,true,true);joints.Pose(CharacterPose.Idle,0,15,true,0);
            PrefabUtility.SaveAsPrefabAsset(root,path);
        }finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    public static string Preview()
    {
        var scene=UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();RenderTexture rt=null;Texture2D image=null;Camera camera=null;var old=RenderTexture.active;
        try
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab");
            var poses=new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Jump,CharacterPose.Idle,CharacterPose.Idle};
            for(int i=0;i<6;i++)
            {
                var root=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);root.transform.position=new Vector3(-3.2f+(i%3)*3.2f,i<3?0:-2.3f,0);
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=31;
                var rig=root.GetComponentInChildren<CutoutRig>(true);rig.ApplySkin(rig.skin);if(i==3)rig.Segmented.AirJump();
                float angle=i==4?65:i==5?-35:15;rig.Pose(poses[i],1,Vector2.right,i==2?-1:1,0,FaceExpression.Neutral,true,true);rig.Segmented.Pose(poses[i],1,angle,i!=2&&i!=3,0);
            }
            var go=new GameObject("SamuraiArticulationPreview");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go,scene);camera=go.AddComponent<Camera>();camera.scene=scene;camera.transform.position=new Vector3(.1f,-.65f,-10);camera.orthographic=true;camera.orthographicSize=2.7f;camera.cullingMask=1<<31;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.28f,.30f,.34f);
            rt=new RenderTexture(1440,810,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image=new Texture2D(1440,810,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,1440,810),0,0);image.Apply();
            string path=System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../TestResults/Sprint0244_Articulation.png"));System.IO.File.WriteAllBytes(path,image.EncodeToPNG());return path;
        }
        finally{RenderTexture.active=old;if(camera!=null)camera.targetTexture=null;if(rt!=null)Object.DestroyImmediate(rt);if(image!=null)Object.DestroyImmediate(image);UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);}
    }
}
