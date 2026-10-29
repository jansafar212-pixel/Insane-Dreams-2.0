#if UNITY_EDITOR
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0248RulesTests
    {
        [Test]public void ExactlyThreeAuthoredSamuraiHaveClearGroundAlignedResetPointsAndUnchangedCombat()
        {
            var scene=EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Demo.unity");
            try
            {
                var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>(true)).ToArray();var mission=all.Select(t=>t.GetComponent<WarehouseMission>()).Single(x=>x!=null);
                CollectionAssert.AreEqual(new[]{3,6,8},mission.Encounters.Select(g=>g.Count));var points=mission.Encounters.SelectMany(g=>g.Points).Where(p=>p.Archetype.Prefab.GetComponent<SamuraiCombat>()!=null).ToArray();Assert.AreEqual(3,points.Length);
                var solids=all.Select(t=>t.GetComponent<Collider2D>()).Where(c=>c!=null&&!c.isTrigger&&c.gameObject.layer==0).ToArray();
                foreach(var p in points)
                {
                    var a=(LarperBrain)new SerializedObject(p).FindProperty("placedActor").objectReferenceValue;Assert.AreEqual(p.transform.position,a.transform.position);var box=a.GetComponent<BoxCollider2D>();
                    Assert.IsTrue(solids.Any(c=>c.bounds.min.x<box.bounds.min.x&&c.bounds.max.x>box.bounds.max.x&&Mathf.Abs(c.bounds.max.y-box.bounds.min.y)<.04f));
                    Assert.IsFalse(solids.Any(c=>c.bounds.Intersects(box.bounds)),p.name+" intersects a solid");
                    foreach(var other in all.Select(t=>t.GetComponent<LarperBrain>()).Where(b=>b!=null&&b!=a))Assert.Greater(Vector2.Distance(a.transform.position,other.transform.position),1.5f);
                    Assert.AreEqual(1.05f,a.GetComponentInChildren<CutoutRig>(true).VisualScale);Assert.Less(box.size.y,1.1f);Assert.AreEqual(Vector2.zero,box.offset);
                    Assert.AreEqual(7,a.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().MoveSpeed);Assert.AreEqual(.4f,new SerializedObject(a.GetComponent<SamuraiCombat>()).FindProperty("windupSeconds").floatValue);
                }
                Assert.AreEqual(3,all.Count(t=>t.GetComponent<SamuraiCombat>()!=null));
            }finally{EditorSceneManager.ClosePreviewScene(scene);}
        }
        [Test]public void VisualAndFireRateChangesKeepWeaponBalanceAndPooledEffectBudget()
        {
            var w=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");Assert.AreEqual(20,w.Damage);Assert.AreEqual(.2f,w.FiringInterval);Assert.AreEqual(24,w.MagazineCapacity);Assert.AreEqual(1.5f,w.ReloadDuration);Assert.AreEqual(22,w.AttackRange);Assert.AreEqual(new Vector2(30,11),w.EquippedSprite.rect.size);Assert.AreEqual(32,w.EquippedSprite.pixelsPerUnit);
            var pistol=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset");Assert.AreEqual(.07f,pistol.FiringInterval);Assert.AreEqual(27,pistol.MagazineCapacity);Assert.AreEqual(4,pistol.ReloadDuration);Assert.AreEqual(5,pistol.Damage);
            var p=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/CartoonFirework.prefab").GetComponent<FireworkProjectile>();Assert.AreEqual(2.7f,p.Radius);Assert.AreEqual(24,new SerializedObject(p).FindProperty("damage").floatValue);
            var e=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/FireworksExpert.prefab");Assert.AreEqual("Fireworks0248",e.GetComponentInChildren<CutoutRig>(true).skin.name);Assert.AreEqual(.08f,new SerializedObject(e.GetComponent<FireworksCombat>()).FindProperty("heldMalfunctionChance").floatValue);
        }
        [Test]public void SamuraiStandingBodyHitboxMatchesVisibleBodyBoundsInBothDirections()
        {
            var g=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"));
            try
            {
                var r=g.GetComponentInChildren<CutoutRig>();
                foreach(float facing in new[]{1f,-1f})
                {
                    r.transform.localPosition=new Vector3(r.PresentationOffset.x*facing,r.PresentationOffset.y+r.GroundAlignmentOffset,0);
                    r.Pose(CharacterPose.Idle,0,Vector2.right*facing,facing,0,FaceExpression.Neutral);r.Segmented.Pose(CharacterPose.Idle,0,15,true,0);Physics2D.SyncTransforms();
                    var b=r.head.GetComponent<SpriteRenderer>().bounds;foreach(var t in r.BodyParts.Concat(r.Segmented.ExtraParts))b.Encapsulate(t.GetComponent<SpriteRenderer>().bounds);
                    var c=g.GetComponent<BoxCollider2D>().bounds;Assert.AreEqual(b.size.x,c.size.x,.0001f);Assert.AreEqual(b.size.y,c.size.y,.0001f);Assert.Less(Vector3.Distance(b.center,c.center),.0001f);
                }
            }finally{Object.DestroyImmediate(g);}
        }
    }
}
#endif
