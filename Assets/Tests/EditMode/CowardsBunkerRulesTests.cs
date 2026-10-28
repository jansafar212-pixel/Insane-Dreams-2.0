using System.Linq;
using InsaneDreams.Enemies;
using InsaneDreams.Missions;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace InsaneDreams.Tests
{
    public sealed class CowardsBunkerRulesTests
    {
        [System.Serializable]class Baseline {public Entry[] items;}
        [System.Serializable]class Entry {public string name;public float[] p,size;public bool trigger;}
        [Test]public void ConcreteShellPreservesSurroundingTerrainAndRoster()
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Warehouse_Demo.unity",OpenSceneMode.Additive);
            try
            {
                var roots=scene.GetRootGameObjects();var all=roots.SelectMany(g=>g.GetComponentsInChildren<Collider2D>(true)).ToArray();
                var before=JsonUtility.FromJson<Baseline>("{\"items\":"+System.IO.File.ReadAllText("TestResults/CowardsBunker/CollidersBefore.json")+"}").items;
                foreach(var item in before){string name=item.name;if(name=="OfficerBarricade"||name=="OfficerRetreatCover")continue;var c=all.Single(c=>c.name==name);Assert.AreEqual(item.p[0],c.bounds.center.x,.001f,name);Assert.AreEqual(item.p[1],c.bounds.center.y,.001f,name);Assert.AreEqual(item.size[0],c.bounds.size.x,.001f,name);Assert.AreEqual(item.size[1],c.bounds.size.y,.001f,name);Assert.AreEqual(item.trigger,c.isTrigger,name);}
                var m=roots.SelectMany(g=>g.GetComponentsInChildren<WarehouseMission>()).Single();CollectionAssert.AreEqual(new[]{3,4,3,3,2},m.Encounters.Select(e=>e.Count).ToArray());Assert.IsTrue(m.Encounters.SelectMany(e=>e.Points).All(p=>p.ValidationError==null));
                var bunker=roots.Single(g=>g.name=="CowardsBunker");var shell=bunker.GetComponentsInChildren<BoxCollider2D>();Assert.AreEqual(5,shell.Length);var roof=bunker.transform.Find("ConcreteRoof").GetComponent<BoxCollider2D>();Assert.Greater(roof.bounds.min.y-1.6f,5.8f);Assert.LessOrEqual(roof.bounds.max.x,149.01f);
                var bounds=shell[0].bounds;foreach(var c in shell){bounds.Encapsulate(c.bounds);var size=c.GetComponent<SpriteRenderer>().size;Assert.AreEqual(c.bounds.size.x,size.x,.001f,"Visible width matches collision");Assert.AreEqual(c.bounds.size.y,size.y,.001f,"Visible height matches collision");}Assert.AreEqual(9,bounds.size.x,.001f);Assert.AreEqual(6.35f,bounds.size.y,.001f);
                Assert.LessOrEqual(bunker.transform.Find("FrontFaceLower").GetComponent<SpriteRenderer>().bounds.max.y,1.73f,"Cutaway exposes limbs and crouching silhouette");Assert.Greater(bunker.transform.Find("FrontFaceUpper").GetComponent<SpriteRenderer>().bounds.min.y,6.9f);
                var officer=roots.SelectMany(g=>g.GetComponentsInChildren<OfficerShotTelegraph>()).Single();Assert.IsTrue(officer.UsesEmbrasure);Assert.AreEqual("ConcreteFrontLower",officer.BunkerCover.name);Assert.AreEqual(.5f,new SerializedObject(officer).FindProperty("windupSeconds").floatValue);
                Assert.AreEqual(0,roots.Single(g=>g.name=="FirstOperationScenery").GetComponentsInChildren<Collider2D>().Length);Assert.IsTrue(roots.Where(g=>g.name=="FirstOperationTerrain"||g.name=="FirstOperationScenery"||g.name=="CowardsBunker").SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).All(r=>r.sprite!=null),"All environment renderers must have sprites; optional runtime equipment overlays are separate");
            }finally{EditorSceneManager.CloseScene(scene,true);}
        }
        [Test]public void EnvironmentModulesUseFourPixelCellsPointFilteringBinaryAlphaAndSmallPalettes()
        {
            var sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/CowardsBunker/Environment.asset").OfType<Sprite>().ToArray();Assert.AreEqual(11,sprites.Length);
            foreach(var s in sprites){Assert.AreEqual(32,s.pixelsPerUnit);Assert.AreEqual(FilterMode.Point,s.texture.filterMode);Assert.AreEqual(1,s.texture.mipmapCount);var pixels=s.texture.GetPixels((int)s.rect.x,(int)s.rect.y,(int)s.rect.width,(int)s.rect.height);Assert.IsTrue(pixels.All(c=>c.a==0||c.a==1));Assert.LessOrEqual(pixels.Where(c=>c.a==1).Distinct().Count(),5,s.name);for(int y=0;y<s.rect.height;y++)for(int x=0;x<s.rect.width;x++)Assert.AreEqual(pixels[(y/4*4)*(int)s.rect.width+x/4*4],pixels[y*(int)s.rect.width+x],s.name);}
            Assert.IsNull(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/Officer.prefab").GetComponent<OfficerShotTelegraph>().BunkerPeek,"Bunker is scene-specific, other Officer encounters unchanged");
        }
    }
}
