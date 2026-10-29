using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class SecondArmRigTests
    {
        private static Texture2D Render(PreviewRenderUtility preview)
        {preview.BeginStaticPreview(new Rect(0,0,256,256));preview.Render(true,false);return preview.EndStaticPreview();}
        [TestCase("Player",1)] [TestCase("Player",-1)] [TestCase("Larper",1)] [TestCase("Larper",-1)]
        public void RearArmActuallyContributesVisiblePixelsAcrossPoses(string who,int facing)
        {
            var preview=new PreviewRenderUtility();
            try
            {
                var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/"+who+"CutoutVisual.prefab"));preview.AddSingleGO(go);var rig=go.GetComponent<CutoutRig>();rig.ApplySkin(rig.skin);
                preview.camera.orthographic=true;preview.camera.orthographicSize=1.4f;preview.camera.transform.position=new Vector3(0,0,-10);preview.camera.transform.rotation=Quaternion.identity;preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=Color.black;
                foreach(var pose in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Crouch,CharacterPose.CrouchWalk,CharacterPose.Climb,CharacterPose.Panic,CharacterPose.Parachute})
                foreach(bool reload in new[]{false,true})
                {
                    rig.Pose(pose,1,new Vector2(facing,0),facing,0,FaceExpression.Neutral,false,true,reload,.5f);
                    var rear=rig.leftArm.GetComponent<SpriteRenderer>();rear.enabled=true;var visible=Render(preview);rear.enabled=false;var hidden=Render(preview);rear.enabled=true;
                    try
                    {
                        var a=visible.GetPixels32();var b=hidden.GetPixels32();int pixels=0;for(int i=0;i<a.Length;i++)if(a[i].r!=b[i].r||a[i].g!=b[i].g||a[i].b!=b[i].b)pixels++;
                        Assert.Greater(pixels,32,who+" "+facing+" "+pose+" reload="+reload+" rear arm must remain visible, not merely enabled.");
                    }finally{Object.DestroyImmediate(visible);Object.DestroyImmediate(hidden);}
                }
            }finally{preview.Cleanup();}
        }
        [TestCase(1)] [TestCase(-1)]
        public void FreeArmNeverChangesCarryingHandMuzzleOrMirroredAim(int facing)
        {
            var go=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab"));
            try
            {
                var r=go.GetComponent<CutoutRig>();r.ApplySkin(r.skin);
                foreach(var pose in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Crouch,CharacterPose.Climb})
                foreach(var aim in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down,new Vector2(.7f,.7f).normalized})
                {
                    r.Pose(pose,1,aim,facing,.4f,FaceExpression.Neutral,true);
                    var hand=r.rightArm.TransformPoint(Vector3.down*r.skin.arm.bounds.size.y);
                    Assert.Less(Vector3.Distance(hand,r.weaponMount.position),.001f);Assert.Greater(Vector2.Dot(r.BarrelDirection,aim),.9999f);
                    Assert.AreEqual(Vector3.one,r.leftArm.localScale);Assert.AreEqual(Mathf.Sign(facing),Mathf.Sign(r.transform.localScale.x));
                    Assert.AreEqual(r.skin.arm,r.leftArm.GetComponent<SpriteRenderer>().sprite);
                }
            }finally{Object.DestroyImmediate(go);}
        }
        [Test] public void HumanoidPrefabsKeepRearArmBehindTorsoAndFrontArmWithOriginalSprites()
        {
            foreach(var path in new[]{"Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab","Assets/Prefabs/Visuals/LarperCutoutVisual.prefab","Assets/Prefabs/Player/WarehousePlayer.prefab","Assets/Prefabs/Enemies/Larper.prefab"})
            {
                var r=AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponentInChildren<CutoutRig>(true);var rear=r.leftArm.GetComponent<SpriteRenderer>();var front=r.rightArm.GetComponent<SpriteRenderer>();var torso=r.torso.GetComponent<SpriteRenderer>();
                Assert.IsTrue(rear.enabled);Assert.IsTrue(front.enabled);Assert.AreEqual(r.Segmented!=null?r.Segmented.upperArm:r.skin.arm,rear.sprite);Assert.AreEqual(front.sprite,rear.sprite);Assert.Less(rear.sortingOrder,torso.sortingOrder);Assert.Less(torso.sortingOrder,front.sortingOrder);Assert.AreEqual(torso.sortingLayerID,rear.sortingLayerID);Assert.AreEqual(r.RearArmTint,rear.color);Assert.IsEmpty(r.GetComponentsInChildren<Rigidbody2D>());Assert.IsEmpty(r.GetComponentsInChildren<Collider2D>());
            }
        }
    }
}
