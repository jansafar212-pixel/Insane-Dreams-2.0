using System;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace InsaneDreams.Tests
{
    public sealed class LevelArt0026Tests
    {
        const string Art="Assets/Art/LevelArt0026";
        [Test] public void NewEnvironmentSpritesHaveNativePointPixelsAndSmallFlatPalettes()
        {
            var assets=AssetDatabase.FindAssets("t:Texture2D",new[]{Art});Assert.Greater(assets.Length,15);
            var palette=new HashSet<Color32>();foreach(var guid in assets)
            {
                var path=AssetDatabase.GUIDToAssetPath(guid);var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var sprite=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().Single();
                Assert.AreEqual(32,sprite.pixelsPerUnit,path);Assert.AreEqual(FilterMode.Point,texture.filterMode,path);Assert.AreEqual(1,texture.mipmapCount,path);Assert.AreEqual(TextureFormat.RGBA32,texture.format,path);Assert.LessOrEqual(texture.width,2048,path);Assert.LessOrEqual(texture.height,2048,path);
                var colors=new HashSet<Color32>(texture.GetPixels32());Assert.LessOrEqual(colors.Count,sprite.name.StartsWith("Crate")?6:3,path);Assert.IsTrue(colors.All(c=>c.a==0||c.a==255),path);foreach(var color in colors.Where(c=>c.a!=0))palette.Add(color);
                if(!sprite.name.StartsWith("Crate"))
                {
                    var pixels=texture.GetPixels32();bool blocks=true;for(int y=0;y<texture.height&&blocks;y++)for(int x=0;x<texture.width;x++)if(!pixels[(y/4*4)*texture.width+x/4*4].Equals(pixels[y*texture.width+x])){blocks=false;break;}Assert.IsTrue(blocks,path+" four-pixel drawing grid");
                }
            }
            Assert.LessOrEqual(palette.Count,16,"Shared warehouse palette");
        }
        [Test] public void SavedWarehouseScenesKeepOriginalNonvisualDataAndUseTheNewArt()
        {
            var type=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("LevelArt0026Setup")).First(t=>t!=null);var compare=type.GetMethod("GameplayDifferences");
            foreach(string name in new[]{"Warehouse_Demo","Warehouse_Testbed","Warehouse_Mission01"})
            {
                var scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity",OpenSceneMode.Additive);
                try
                {
                    bool operation=name=="Warehouse_Demo"&&scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<InsaneDreams.Missions.WarehouseMission>()).Any(m=>m.FirstOperation);if(!operation){var differences=(string[])compare.Invoke(null,new object[]{scene});Assert.IsEmpty(differences,name+" protected scene data");}
                    var all=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();var art=all.Where(r=>r.enabled&&AssetDatabase.GetAssetPath(r.sprite).StartsWith(Art)).ToArray();Assert.Greater(art.Length,40,name);Assert.AreEqual(operation?5:6,art.Count(r=>r.sprite.name.StartsWith("Window_")),name+" sparse windows");
                    Assert.GreaterOrEqual(art.Where(r=>r.sprite.name.StartsWith("Crate")).Select(r=>r.sprite.name.Split('_')[0]).Distinct().Count(),operation?3:4,name);
                    foreach(var r in art){Assert.NotNull(r.sharedMaterial,r.name);Assert.NotNull(r.sharedMaterial.shader,r.name);Assert.AreEqual(1,r.transform.lossyScale.x,.0001f,r.name);Assert.AreEqual(1,r.transform.lossyScale.y,.0001f,r.name);}
                    foreach(var child in art.Where(r=>r.name=="LevelArt0026_Visual"))
                    {
                        Assert.IsNull(child.GetComponent<Collider2D>());var box=child.transform.parent.GetComponent<BoxCollider2D>();if(box==null)continue;
                        Assert.Less(Vector2.Distance(box.bounds.center,child.bounds.center),.0002f,child.transform.parent.name);Assert.Less(Vector2.Distance(box.bounds.size,child.bounds.size),.0002f,child.transform.parent.name);
                    }
                    foreach(var ladder in scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<LadderVisual2D>(true)))foreach(var r in new[]{ladder.Body,ladder.TopCap,ladder.BottomCap})Assert.IsTrue(AssetDatabase.GetAssetPath(r.sprite).StartsWith(Art));
                }
                finally{EditorSceneManager.CloseScene(scene,true);}
            }
        }
    }
}
