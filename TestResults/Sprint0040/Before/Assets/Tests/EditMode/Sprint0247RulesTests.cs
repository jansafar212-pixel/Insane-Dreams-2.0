#if UNITY_EDITOR
using InsaneDreams.Visuals;
using InsaneDreams.Enemies;
using InsaneDreams.Prototype;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0247RulesTests
    {
        private static GameObject Prefab(string n)=>AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/"+(n=="WarehousePlayer"?"Player/":"Enemies/")+n+".prefab");
        private static float Height(CutoutRig r)=>new SerializedObject(r).FindProperty("torsoCenterY").floatValue+r.skin.torso.bounds.extents.y+r.skin.head.bounds.size.y-new SerializedObject(r).FindProperty("standingFeetY").floatValue;
        [Test]public void TinySamuraiHasGiantSwordButRetainsFairLivingHitboxAndCombatTuning()
        {
            var g=Prefab("Samurai");var r=g.GetComponentInChildren<CutoutRig>(true);var larper=Prefab("Larper");
            Assert.That(Height(r)/Height(larper.GetComponentInChildren<CutoutRig>(true)),Is.InRange(.65f,.75f));
            Assert.That(r.skin.weapon.bounds.size.x/Height(r),Is.InRange(1.2f,1.5f));
            Assert.Less(g.GetComponent<BoxCollider2D>().size.y,larper.GetComponent<BoxCollider2D>().size.y);
            Assert.AreEqual(new Vector3(1.05f,1.05f,1),r.transform.localScale);Assert.IsTrue(r.Segmented.IsBound);Assert.IsTrue(r.face.enabled);
            var data=new SerializedObject(g.GetComponent<SamuraiCombat>());Assert.AreEqual(1.65f,data.FindProperty("attackDistance").floatValue);Assert.AreEqual(.4f,data.FindProperty("windupSeconds").floatValue);
        }
        [TestCase("WarehousePlayer",8.4f)][TestCase("Larper",7.7f)][TestCase("Samurai",7)][TestCase("FireworksExpert",7)]
        public void SpeedsAreAbsoluteAndMotorAccelerationUnchanged(string name,float speed)
        {var m=Prefab(name).GetComponent<PlayerMotor2D>();Assert.AreEqual(speed,m.MoveSpeed,.0001f);var s=new SerializedObject(m);Assert.AreEqual(65,s.FindProperty("acceleration").floatValue);Assert.AreEqual(85,s.FindProperty("deceleration").floatValue);}
        [TestCase("WarehousePlayer")][TestCase("Samurai")][TestCase("FireworksExpert")]
        public void RedesignsRetainNativeDensityFallbacksAndNoLiveVisualPhysics(string n)
        {
            var r=Prefab(n).GetComponentInChildren<CutoutRig>(true);Assert.IsTrue(r.IsBound);Assert.IsTrue(r.skin.IsComplete);Assert.AreEqual(6,r.skin.faces.Length);
            foreach(var s in new[]{r.skin.head,r.skin.torso,r.skin.arm,r.skin.leg,r.skin.weapon}){Assert.AreEqual(32,s.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,s.texture.filterMode);Assert.AreEqual(1,s.texture.mipmapCount);}
            Assert.IsEmpty(r.GetComponentsInChildren<Collider2D>());Assert.IsEmpty(r.GetComponentsInChildren<Rigidbody2D>());
        }
        [TestCase(1280,720,0,1)][TestCase(2560,1080,.1f,.75f)]
        public void CameraCompositionUsesProjectionAndEffectiveViewport(int width,int height,float left,float span)
        {
            var g=new GameObject("S0247Camera");var target=new GameObject("Target");var rt=new RenderTexture(width,height,0);
            try
            {
                var c=g.AddComponent<Camera>();c.targetTexture=rt;var f=g.AddComponent<InsaneDreams.CameraSystem.WarehouseCameraFollow>();var s=new SerializedObject(f);
                s.FindProperty("levelMin").vector2Value=new Vector2(-200,-100);s.FindProperty("levelMax").vector2Value=new Vector2(200,100);s.FindProperty("gameplayViewport").rectValue=new Rect(left,0,span,1);s.ApplyModifiedPropertiesWithoutUndo();f.Bind(target.transform);
                float x=c.WorldToViewportPoint(target.transform.position).x;Assert.AreEqual(left+span/3,x,.002f);
                Assert.AreEqual(1.155f,new SerializedObject(f).FindProperty("gameplayZoomMultiplier").floatValue,.0001f);
                s.Update();s.FindProperty("gameplayZoomMultiplier").floatValue=1.1f;s.ApplyModifiedPropertiesWithoutUndo();f.SnapToTarget();float baseline=c.orthographicSize;
                s.Update();s.FindProperty("gameplayZoomMultiplier").floatValue=1.155f;s.ApplyModifiedPropertiesWithoutUndo();f.SnapToTarget();Assert.AreEqual(baseline/1.05f,c.orthographicSize,.0001f);
            }finally{Object.DestroyImmediate(g);Object.DestroyImmediate(target);Object.DestroyImmediate(rt);}
        }
    }
}
#endif
