var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var preview=new UnityEditor.PreviewRenderUtility();UnityEngine.Texture2D image=null;var pooled=new System.Collections.Generic.List<UnityEngine.GameObject>();
try
{
 var sheet=new UnityEngine.GameObject("LarperGlockReview");preview.AddSingleGO(sheet);
 var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Enemies/Larper.prefab");
 var d=UnityEditor.AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");var phases=new[]{0f,.2f,.3f,.41f,.5f,.65f,.8f,1f};
 for(int row=0;row<2;row++)for(int col=0;col<8;col++){
  var g=UnityEngine.Object.Instantiate(prefab,sheet.transform);g.transform.localPosition=new UnityEngine.Vector3(-6.65f+col*1.9f,.9f-row*1.8f,0);
  var rig=g.GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>();rig.ApplySkin(rig.skin);rig.weapon.sprite=d.EquippedSprite;rig.SetMagazine(d.MagazineSprite,d.MagazineMountOffset);
  var h=g.GetComponent<InsaneDreams.Visuals.PistolMagazineReload>();var type=h.GetType();type.GetField("pistols",flags).SetValue(h,new[]{d});type.GetMethod("Awake",flags).Invoke(h,null);
  foreach(var b in (UnityEngine.Rigidbody2D[])type.GetField("bodies",flags).GetValue(h))pooled.Add(b.gameObject);
  var w=new InsaneDreams.Inventory.RuntimeWeapon(d);w.SetAmmunition(3,27);w.Cycle.TryReload(1);
  rig.Pose(InsaneDreams.Visuals.CharacterPose.Idle,0,row==0?UnityEngine.Vector2.right:UnityEngine.Vector2.left,row==0?1:-1,0,InsaneDreams.Visuals.FaceExpression.Neutral,false,true,col<7,phases[col],advanceTransitions:false);
  h.Pose(rig,w,col<7,phases[col]);if(col<7)rig.Segmented.PosePistolExchange(phases[col]);
 }
 preview.camera.orthographic=true;preview.camera.orthographicSize=2;preview.camera.transform.position=new UnityEngine.Vector3(0,0,-10);preview.camera.clearFlags=UnityEngine.CameraClearFlags.SolidColor;preview.camera.backgroundColor=new UnityEngine.Color(.18f,.2f,.23f);
 preview.BeginStaticPreview(new UnityEngine.Rect(0,0,2048,512));preview.Render(true,false);image=preview.EndStaticPreview();System.IO.File.WriteAllBytes("TestResults/Hotfix00253_LarperReload.png",image.EncodeToPNG());
}
finally{foreach(var g in pooled)if(g!=null)UnityEngine.Object.DestroyImmediate(g);if(image!=null)UnityEngine.Object.DestroyImmediate(image);preview.Cleanup();}
preview=new UnityEditor.PreviewRenderUtility();image=null;
try
{
 var sheet=new UnityEngine.GameObject("FireworkSequenceReview");preview.AddSingleGO(sheet);
 var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab");var phases=new[]{.02f,.18f,.38f,.68f};
 for(int i=0;i<4;i++){
  var g=UnityEngine.Object.Instantiate(prefab,sheet.transform);var b=g.GetComponent<InsaneDreams.Enemies.FireworkProjectile>();var type=b.GetType();type.GetMethod("Awake",flags).Invoke(b,null);type.GetField("burstSound",flags).SetValue(b,null);
  var p=new UnityEngine.Vector2(-11.25f+i*7.5f,0);b.Prepare(null,p,UnityEngine.Color.magenta,true);b.ExplodeHeld(p);type.GetField("hideAt",flags).SetValue(b,UnityEngine.Time.time+.8f-phases[i]);type.GetMethod("Update",flags).Invoke(b,null);
 }
 preview.camera.orthographic=true;preview.camera.orthographicSize=3.5f;preview.camera.transform.position=new UnityEngine.Vector3(0,0,-10);preview.camera.clearFlags=UnityEngine.CameraClearFlags.SolidColor;preview.camera.backgroundColor=new UnityEngine.Color(.25f,.27f,.29f);
 preview.BeginStaticPreview(new UnityEngine.Rect(0,0,2048,512));preview.Render(true,false);image=preview.EndStaticPreview();System.IO.File.WriteAllBytes("TestResults/Hotfix00253_Fireworks.png",image.EncodeToPNG());return "Controlled reload and fireworks storyboards saved";
}
finally{if(image!=null)UnityEngine.Object.DestroyImmediate(image);preview.Cleanup();}

