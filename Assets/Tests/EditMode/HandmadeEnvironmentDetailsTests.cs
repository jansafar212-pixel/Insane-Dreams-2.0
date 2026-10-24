using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace InsaneDreams.Tests
{
    public sealed class HandmadeEnvironmentDetailsTests
    {
        const string Sheet="Assets/Art/HandmadeEnvironmentDetails/Details.asset";
        [Test] public void SharedSheetKeepsFourPixelCellsAndLimitedColors()
        {
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Sheet);Assert.NotNull(texture);Assert.AreEqual(FilterMode.Point,texture.filterMode);Assert.AreEqual(TextureFormat.RGBA32,texture.format);Assert.AreEqual(1,texture.mipmapCount);Assert.AreEqual(256,texture.width);Assert.AreEqual(256,texture.height);
            var sprites=AssetDatabase.LoadAllAssetsAtPath(Sheet).OfType<Sprite>().ToArray();Assert.AreEqual(15,sprites.Length);var pixels=texture.GetPixels32();
            foreach(var sprite in sprites)
            {
                Assert.AreEqual(32,sprite.pixelsPerUnit);int x0=(int)sprite.rect.x,y0=(int)sprite.rect.y,w=(int)sprite.rect.width,h=(int)sprite.rect.height;var colors=new System.Collections.Generic.HashSet<Color32>();bool blocks=true;
                for(int y=0;y<h;y++)for(int x=0;x<w;x++){var c=pixels[(y0+y)*256+x0+x];if(c.a!=0)colors.Add(c);Assert.IsTrue(c.a==0||c.a==255);if(!c.Equals(pixels[(y0+y/4*4)*256+x0+x/4*4]))blocks=false;}
                Assert.IsTrue(blocks,sprite.name);Assert.LessOrEqual(colors.Count,5,sprite.name);
            }
        }
        [Test] public void DecorativeObjectsHaveNoBehaviorOrCollisionAndOriginalSceneDataMatches()
        {
            var setup=AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType("HandmadeEnvironmentDetailsSetup")).First(t=>t!=null);
            foreach(string name in new[]{"Warehouse_Demo","Warehouse_Testbed","Warehouse_Mission01"})
            {
                var scene=UnityEngine.SceneManagement.SceneManager.GetSceneByPath("Assets/Scenes/"+name+".unity");bool opened=!scene.isLoaded;if(opened)scene=EditorSceneManager.OpenScene("Assets/Scenes/"+name+".unity",OpenSceneMode.Additive);
                try
                {
                    Assert.IsEmpty((string[])setup.GetMethod("Differences").Invoke(null,new object[]{scene}));var root=scene.GetRootGameObjects().Single(g=>g.name=="HandmadeEnvironmentDetails");Assert.AreEqual(19,root.transform.childCount);
                    foreach(var c in root.GetComponentsInChildren<Component>())Assert.IsTrue(c is Transform||c is SpriteRenderer,c.GetType().Name);
                    foreach(var r in root.GetComponentsInChildren<SpriteRenderer>()){Assert.NotNull(r.sprite);Assert.NotNull(r.sharedMaterial);Assert.AreEqual(Sheet,AssetDatabase.GetAssetPath(r.sprite));Assert.AreEqual(Vector3.one,r.transform.lossyScale);Assert.IsTrue(r.sortingOrder<0||r.name.StartsWith("SurfaceMark_"));}
                    foreach(var mark in root.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("SurfaceMark_"))){string surface=mark.name.Substring("SurfaceMark_".Length);var target=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<SpriteRenderer>(true)).Single(r=>r.name==surface);Assert.IsTrue(target.bounds.Contains(mark.bounds.min));Assert.IsTrue(target.bounds.Contains(mark.bounds.max));}
                }
                finally{if(opened)EditorSceneManager.CloseScene(scene,true);}
            }
        }
    }
}
