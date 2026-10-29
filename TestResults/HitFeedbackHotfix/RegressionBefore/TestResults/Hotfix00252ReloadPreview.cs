var preview=new UnityEditor.PreviewRenderUtility();UnityEngine.Texture2D image=null;
var pooled=new System.Collections.Generic.List<UnityEngine.GameObject>();
try
{
    var sheet=new UnityEngine.GameObject("GlockReloadInspection");preview.AddSingleGO(sheet);
    var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab");
    var d=UnityEditor.AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/BackupPistol.asset");
    var phases=new[]{0f,.2f,.3f,.41f,.5f,.65f,.8f,1f};
    var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
    for(int row=0;row<2;row++)for(int col=0;col<8;col++)
    {
        var g=UnityEngine.Object.Instantiate(prefab,sheet.transform);g.transform.localPosition=new UnityEngine.Vector3(-7+col*2,.9f-row*1.8f,0);
        var rig=g.GetComponent<InsaneDreams.Visuals.CutoutRig>();rig.ApplySkin(rig.skin);rig.weapon.sprite=d.EquippedSprite;rig.SetMagazine(d.MagazineSprite,d.MagazineMountOffset);
        var handler=g.AddComponent<InsaneDreams.Visuals.PistolMagazineReload>();var type=handler.GetType();type.GetField("pistols",flags).SetValue(handler,new[]{d});type.GetMethod("Awake",flags).Invoke(handler,null);
        foreach(var b in (UnityEngine.Rigidbody2D[])type.GetField("bodies",flags).GetValue(handler))pooled.Add(b.gameObject);
        var weapon=new InsaneDreams.Inventory.RuntimeWeapon(d);weapon.SetAmmunition(1,30);weapon.Cycle.TryReload(1);
        rig.Pose(InsaneDreams.Visuals.CharacterPose.Idle,0,row==0?UnityEngine.Vector2.right:UnityEngine.Vector2.left,row==0?1:-1,0,InsaneDreams.Visuals.FaceExpression.Neutral,false,true,col<7,phases[col],advanceTransitions:false);
        handler.Pose(rig,weapon,col<7,phases[col]);
    }
    preview.camera.orthographic=true;preview.camera.orthographicSize=2;preview.camera.transform.position=new UnityEngine.Vector3(0,0,-10);preview.camera.clearFlags=UnityEngine.CameraClearFlags.SolidColor;preview.camera.backgroundColor=new UnityEngine.Color(.1f,.14f,.18f,1);
    preview.BeginStaticPreview(new UnityEngine.Rect(0,0,2048,512));preview.Render(true,false);image=preview.EndStaticPreview();System.IO.File.WriteAllBytes("TestResults/Hotfix00252_Reload.png",image.EncodeToPNG());return "TestResults/Hotfix00252_Reload.png";
}
finally{foreach(var g in pooled)if(g!=null)UnityEngine.Object.DestroyImmediate(g);if(image!=null)UnityEngine.Object.DestroyImmediate(image);preview.Cleanup();}

