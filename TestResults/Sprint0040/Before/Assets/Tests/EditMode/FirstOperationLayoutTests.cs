using System;
using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Missions;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class FirstOperationLayoutTests
    {
        [Test]public void SavedLayoutHasExactRosterSupportedStartsAndNoCheckpoints()
        {
            var s=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity",OpenSceneMode.Additive);
            try
            {
                var roots=s.GetRootGameObjects();var mission=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single();Assert.IsTrue(mission.FirstOperation);CollectionAssert.AreEqual(new[]{3,3,3,3,1},mission.Encounters.Select(g=>g.Count).ToArray());
                var points=mission.Encounters.SelectMany(g=>g.Points).ToArray();Assert.AreEqual(13,points.Length);Assert.AreEqual(10,points.Count(p=>p.Archetype.name=="Larper"));Assert.AreEqual(1,points.Count(p=>p.Archetype.name=="Officer"));Assert.IsTrue(points.All(p=>p.ValidationError==null));Assert.AreEqual(0,roots.SelectMany(g=>g.GetComponentsInChildren<InsaneDreams.Checkpoints.CheckpointMarker>(true)).Count());
                Assert.IsFalse(new SerializedObject(mission.Player.GetComponent<InsaneDreams.Combat.PlayerHealthRecovery>()).FindProperty("allowCheckpointRespawn").boolValue);Assert.AreEqual(0,new SerializedObject(mission.Player.GetComponent<InsaneDreams.Combat.PlayerRevive>()).FindProperty("initialInjections").intValue);
                var terrain=roots.Single(g=>g.name=="FirstOperationTerrain");Assert.AreEqual(7,terrain.GetComponentsInChildren<InsaneDreams.Traversal.LadderVolume>().Length);
                foreach(var floor in new[]{"LoadingFloor","StorageFloor","PursuitFloor","FireworkFloor","CommandFloor"}){var b=terrain.transform.Find(floor).GetComponent<BoxCollider2D>();Assert.AreEqual(0,b.bounds.max.y,.001f);Assert.IsFalse(b.isTrigger);}
                var scenery=roots.Single(g=>g.name=="FirstOperationScenery");Assert.AreEqual(0,scenery.GetComponentsInChildren<Collider2D>().Length);Assert.IsTrue(scenery.GetComponentsInChildren<SpriteRenderer>().All(r=>r.sprite!=null&&r.sortingOrder<0));
                var opening=mission.Encounters[0].Points.OrderBy(p=>p.transform.position.x).ToArray();for(int i=1;i<3;i++)Assert.GreaterOrEqual(opening[i].transform.position.x-opening[i-1].transform.position.x,7);
            }finally{EditorSceneManager.CloseScene(s,true);}
        }
        [Test]public void OfficerRetainsModularDimensionsAndNoHealthInflationAndMissionFireworksAreScoped()
        {
            string dir="Assets/Data/FirstOperation/";var officer=AssetDatabase.LoadAssetAtPath<GameObject>(dir+"Officer.prefab");var basic=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab");Assert.AreEqual(new SerializedObject(basic.GetComponent<InsaneDreams.Combat.Health>()).FindProperty("maximum").floatValue,new SerializedObject(officer.GetComponent<InsaneDreams.Combat.Health>()).FindProperty("maximum").floatValue);Assert.NotNull(officer.GetComponent<OfficerShotTelegraph>());
            var skin=AssetDatabase.LoadAssetAtPath<CharacterSkin>(dir+"OfficerSkin.asset");Assert.IsTrue(skin.IsComplete);var sprites=new[]{skin.head,skin.torso,skin.arm,skin.leg};CollectionAssert.AreEqual(new[]{12f,12f,5f,6f},sprites.Select(x=>x.rect.width).ToArray());CollectionAssert.AreEqual(new[]{11f,17f,18f,18f},sprites.Select(x=>x.rect.height).ToArray());foreach(var sprite in sprites){Assert.AreEqual(32,sprite.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,sprite.texture.filterMode);Assert.AreEqual(1,sprite.texture.mipmapCount);}
            var j=officer.GetComponentInChildren<SamuraiArticulatedRig>();Assert.IsTrue(j.IsBound);Assert.AreEqual(skin.arm.texture,j.upperArm.texture);Assert.AreEqual(skin.leg.texture,j.thigh.texture);
            var rocket=AssetDatabase.LoadAssetAtPath<GameObject>(dir+"OperationFirework.prefab").GetComponent<FireworkProjectile>();var r=new SerializedObject(rocket);Assert.AreEqual(100,r.FindProperty("damage").floatValue);Assert.AreEqual(2.8f,r.FindProperty("radius").floatValue);Assert.GreaterOrEqual(r.FindProperty("shortFuse").vector2Value.x,.6f);
        }
    }
}
