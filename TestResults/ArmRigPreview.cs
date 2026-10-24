// Editor-only diagnostic, outside Assets. Renders existing sprites; no game assets created.
var preview=new UnityEditor.PreviewRenderUtility();
UnityEngine.Texture2D image=null;
try
{
    var sheet=new UnityEngine.GameObject("ArmRigPreview");preview.AddSingleGO(sheet);
    var poses=new[]{InsaneDreams.Visuals.CharacterPose.Idle,InsaneDreams.Visuals.CharacterPose.Walk,InsaneDreams.Visuals.CharacterPose.Crouch,InsaneDreams.Visuals.CharacterPose.Idle,InsaneDreams.Visuals.CharacterPose.Climb,InsaneDreams.Visuals.CharacterPose.Idle};
    for(int row=0;row<4;row++)for(int col=0;col<6;col++)
    {
        bool larper=row%2==1;var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Visuals/"+(larper?"Larper":"Player")+"CutoutVisual.prefab");
        var go=UnityEngine.Object.Instantiate(prefab,sheet.transform);var rig=go.GetComponent<InsaneDreams.Visuals.CutoutRig>();rig.ApplySkin(rig.skin);
        var weapon=UnityEditor.AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/"+(larper?"LarperBuzzbox":"WarehouseCarbine")+".asset");if(weapon.EquippedSprite!=null)rig.weapon.sprite=weapon.EquippedSprite;rig.SetMagazine(weapon.MagazineSprite);
        rig.Pose(poses[col],1,col==5?UnityEngine.Vector2.up:row<2?UnityEngine.Vector2.right:UnityEngine.Vector2.left,row<2?1:-1,0,InsaneDreams.Visuals.FaceExpression.Neutral,false,true,col==3,col==3?.5f:0);
        go.transform.localPosition=new UnityEngine.Vector3(-5.75f+col*2.3f,2.25f-row*1.8f,0);
    }
    preview.camera.orthographic=true;preview.camera.orthographicSize=4.21875f;preview.camera.transform.position=new UnityEngine.Vector3(0,0,-10);preview.camera.transform.rotation=UnityEngine.Quaternion.identity;
    preview.camera.clearFlags=UnityEngine.CameraClearFlags.SolidColor;preview.camera.backgroundColor=new UnityEngine.Color(.1f,.14f,.18f,1);
    preview.BeginStaticPreview(new UnityEngine.Rect(0,0,1920,1080));preview.Render(true,false);image=preview.EndStaticPreview();
    string suffix=UnityEditor.SessionState.GetString("ArmRigPreviewSuffix","Before");var path=System.IO.Path.Combine(System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName,"TestResults/ArmRig_"+suffix+".png");System.IO.File.WriteAllBytes(path,image.EncodeToPNG());return path;
}
finally{if(image!=null)UnityEngine.Object.DestroyImmediate(image);preview.Cleanup();}
