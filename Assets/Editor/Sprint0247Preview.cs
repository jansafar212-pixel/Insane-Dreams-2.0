using System.Linq;
using InsaneDreams.Visuals;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class Sprint0247Preview
{
    private static void Label(UnityEngine.SceneManagement.Scene scene,string text,Vector2 point)
    {var g=new GameObject("Label");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(g,scene);g.layer=31;g.transform.position=point;var t=g.AddComponent<TextMesh>();t.text=text;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.fontSize=48;t.characterSize=.027f;t.anchor=TextAnchor.MiddleCenter;t.color=Color.white;g.GetComponent<MeshRenderer>().sharedMaterial=t.font.material;}
    public static string Sheet()
    {
        var scene=EditorSceneManager.NewPreviewScene();
        try
        {
            Label(scene,"SPRINT 024.7 / LARPER BASELINE + THREE NATIVE PIXEL REDESIGNS",new Vector2(0,4.3f));
            var names=new[]{"Larper","WarehousePlayer","Samurai","FireworksExpert"};
            var poses=new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Crouch,CharacterPose.Climb,CharacterPose.Panic};
            for(int row=0;row<4;row++)for(int col=0;col<6;col++)
            {
                string n=names[row];var g=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/"+(n=="WarehousePlayer"?"Player/":"Enemies/")+n+".prefab"),scene);
                foreach(var tr in g.GetComponentsInChildren<Transform>(true))tr.gameObject.layer=31;
                g.transform.position=new Vector2(-6.75f+col*2.7f,3.15f-row*2);
                var r=g.GetComponentInChildren<CutoutRig>(true);r.ApplySkin(r.skin);
                if(n=="Larper"){var d=AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");r.weapon.sprite=d.EquippedSprite;r.SetMagazine(d.MagazineSprite,d.MagazineMountOffset);}
                float facing=col==2?-1:1;r.Pose(poses[col],1,Vector2.right*facing,facing,0,FaceExpression.Neutral,true,true,col==4,.5f);
                if(n=="Samurai")r.Segmented.Pose(poses[col],1,col==0?65:-35,col!=2,0);
                Label(scene,(col==0?n+" / ":"")+poses[col]+(col==4?" + reload":""),new Vector2(-6.75f+col*2.7f,2.2f-row*2));
            }
            var cam=new GameObject("ReviewCamera").AddComponent<Camera>();UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cam.gameObject,scene);cam.scene=scene;cam.cullingMask=1<<31;cam.orthographic=true;cam.orthographicSize=4.6f;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.16f,.18f,.21f);
            return Sprint0245Preview.Save(cam,1920,1080,"Sprint0247_ReferenceSheet.png");
        }finally{EditorSceneManager.ClosePreviewScene(scene);}
    }
}
