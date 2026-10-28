using UnityEditor;
using UnityEngine;
public static class Sprint0031Icons
{
    public static string Create()
    {
        const string folder="Assets/Art/Interface0031";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Art","Interface0031");
        foreach(string name in new[]{"DuctTape","Coin","Ammo"})
        {
            string path=folder+"/"+name+".asset";if(AssetDatabase.LoadAssetAtPath<Sprite>(path)!=null)continue;
            var t=new Texture2D(16,16,TextureFormat.RGBA32,false){name=name+"Pixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<16;y++)for(int x=0;x<16;x++){int u=x/2,v=y/2;Color32 c=Color.clear;bool outline=false;
                if(name=="DuctTape"){bool ring=u>=1&&u<=6&&v>=1&&v<=6&&!(u>=3&&u<=4&&v>=3&&v<=4);if(ring){outline=u==1||v==1;c=outline?new Color32(114,121,101,255):new Color32(233,226,197,255);}}
                else if(name=="Coin"){bool shape=(u>=1&&u<=6&&v>=2&&v<=5)||(u>=2&&u<=5&&v>=1&&v<=6);if(shape)c=u==2||v==1?new Color32(156,108,48,255):new Color32(217,188,90,255);if(u==4&&v>=3&&v<=5)c=new Color32(233,226,197,255);}
                else if((u==1||u==3||u==5)&&v>=1&&v<=6)c=v==6?new Color32(205,120,62,255):v==1?new Color32(114,121,101,255):new Color32(217,188,90,255);
                t.SetPixel(x,y,c);
            }t.Apply();AssetDatabase.CreateAsset(t,folder+"/"+name+"Pixels.asset");var sprite=Sprite.Create(t,new Rect(0,0,16,16),Vector2.one*.5f,32,0,SpriteMeshType.FullRect);sprite.name=name;AssetDatabase.CreateAsset(sprite,path);
        }AssetDatabase.SaveAssets();return "Three small native Point-filtered UI icons created; existing grenades, syringe, armor and weapon art reused.";
    }
}
