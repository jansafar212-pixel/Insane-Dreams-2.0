using System.IO;
using InsaneDreams.Combat;
using UnityEditor;
using UnityEngine;
public static class GrenadeResupplyHotfixSetup
{
    const string Folder="Assets/Data/Sprint0027/";
    public static string Apply()
    {
        if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode");
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Folder+"FragBurst.asset");
        if(sprite==null){var t=new Texture2D(8,8,TextureFormat.RGBA32,false){name="FragBurstPixels",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};string[] rows={"00111000","01111110","11111111","11111111","11111111","11111110","01111110","00111100"};for(int y=0;y<8;y++)for(int x=0;x<8;x++)t.SetPixel(x,y,rows[y][x]=='1'?Color.white:Color.clear);t.Apply();AssetDatabase.CreateAsset(t,Folder+"FragBurstPixels.asset");sprite=Sprite.Create(t,new Rect(0,0,8,8),new Vector2(.5f,.5f),32,0,SpriteMeshType.FullRect);sprite.name="FragBurst";AssetDatabase.CreateAsset(sprite,Folder+"FragBurst.asset");}
        string audio=Folder+"FragBoom.wav";if(!File.Exists(audio)){const int rate=22050,n=13230;using(var w=new BinaryWriter(File.Create(audio))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+n*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(rate);w.Write(rate*2);w.Write((short)2);w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(n*2);var random=new System.Random(272);float filtered=0;for(int i=0;i<n;i++){float t=(float)i/rate;filtered=.7f*filtered+.3f*((float)random.NextDouble()*2-1);float sample=(.55f*Mathf.Sin(2*Mathf.PI*(88*t-32*t*t))*Mathf.Exp(-8*t)+filtered*.9f*Mathf.Exp(-11*t))*Mathf.Min(1,t*500);w.Write((short)(Mathf.Clamp(sample,-1,1)*25000));}}AssetDatabase.ImportAsset(audio,ImportAssetOptions.ForceSynchronousImport);}
        string path=Folder+"Throwable.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{var s=new SerializedObject(root.GetComponent<ThrowableProjectile2D>());s.FindProperty("blastRadius").floatValue=4.1f;s.FindProperty("fragmentationDamage").floatValue=110.5f;s.FindProperty("burstSprite").objectReferenceValue=sprite;s.FindProperty("fragmentSprite").objectReferenceValue=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Prototype/PlaceholderSquare.png");s.FindProperty("explosionSound").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(audio);s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}AssetDatabase.SaveAssets();return "Frag radius4.1/damage110.5; pooled chunky mask/debris and short boom assigned. No scenes modified.";
    }
}
