var preview=new UnityEditor.PreviewRenderUtility();UnityEngine.Texture2D image=null;
try
{
    var sheet=new UnityEngine.GameObject("ReloadInspection");preview.AddSingleGO(sheet);
    var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab");
    var d=UnityEditor.AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");
    var phases=new[]{0f,.18f,.36f,.5f,.65f,.8f,.95f,1f};
    for(int row=0;row<2;row++)for(int col=0;col<8;col++)
    {
        var g=UnityEngine.Object.Instantiate(prefab,sheet.transform);var rig=g.GetComponent<InsaneDreams.Visuals.CutoutRig>();rig.ApplySkin(rig.skin);rig.weapon.sprite=d.EquippedSprite;rig.SetMagazine(d.MagazineSprite,d.MagazineMountOffset,true);
        rig.Pose(InsaneDreams.Visuals.CharacterPose.Idle,0,row==0?UnityEngine.Vector2.right:UnityEngine.Vector2.left,row==0?1:-1,0,InsaneDreams.Visuals.FaceExpression.Neutral,false,true,col<7,phases[col],advanceTransitions:false);
        g.transform.localPosition=new UnityEngine.Vector3(-7+col*2,.9f-row*1.8f,0);
    }
    preview.camera.orthographic=true;preview.camera.orthographicSize=2;preview.camera.transform.position=new UnityEngine.Vector3(0,0,-10);preview.camera.clearFlags=UnityEngine.CameraClearFlags.SolidColor;preview.camera.backgroundColor=new UnityEngine.Color(.1f,.14f,.18f,1);
    preview.BeginStaticPreview(new UnityEngine.Rect(0,0,2048,512));preview.Render(true,false);image=preview.EndStaticPreview();System.IO.File.WriteAllBytes("TestResults/Hotfix00251_Reload.png",image.EncodeToPNG());return "TestResults/Hotfix00251_Reload.png";
}
finally{if(image!=null)UnityEngine.Object.DestroyImmediate(image);preview.Cleanup();}
