using System.IO;
using UnityEditor;
using UnityEngine;
public static class Sprint0025Setup
{
    public static void Apply()
    {
        const string folder="Assets/Audio";if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets","Audio");
        const string sound="Assets/Audio/NearMissPressure.wav";
        if(!File.Exists(sound))
        {
            const int rate=22050,count=1764;using(var writer=new BinaryWriter(File.Create(sound)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));writer.Write(36+count*2);writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));writer.Write(16);writer.Write((short)1);writer.Write((short)1);writer.Write(rate);writer.Write(rate*2);writer.Write((short)2);writer.Write((short)16);writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));writer.Write(count*2);
                var random=new System.Random(25);float filtered=0;
                for(int i=0;i<count;i++){filtered=Mathf.Lerp(filtered,(float)random.NextDouble()*2-1,.25f);writer.Write((short)(filtered*Mathf.Sin(Mathf.PI*i/(count-1))*16000));}
            }
            AssetDatabase.ImportAsset(sound,ImportAssetOptions.ForceSynchronousImport);
        }
        const string player="Assets/Prefabs/Player/WarehousePlayer.prefab";var g=PrefabUtility.LoadPrefabContents(player);
        try{var pressure=g.GetComponent<InsaneDreams.Combat.PlayerSuppression>();if(pressure==null)pressure=g.AddComponent<InsaneDreams.Combat.PlayerSuppression>();if(g.GetComponent<InsaneDreams.Combat.PlayerRevive>()==null)g.AddComponent<InsaneDreams.Combat.PlayerRevive>();var s=new SerializedObject(pressure);s.FindProperty("nearMissSound").objectReferenceValue=AssetDatabase.LoadAssetAtPath<AudioClip>(sound);s.ApplyModifiedPropertiesWithoutUndo();PrefabUtility.SaveAsPrefabAsset(g,player);}finally{PrefabUtility.UnloadPrefabContents(g);}
        const string wallet="Assets/Prefabs/Pickups/WalletPickup.prefab";g=PrefabUtility.LoadPrefabContents(wallet);
        try{var s=new SerializedObject(g.GetComponent<InsaneDreams.Inventory.WalletPickup>());s.FindProperty("pickupRadius").floatValue=1.95f;s.ApplyModifiedPropertiesWithoutUndo();g.GetComponent<CircleCollider2D>().radius=1.95f;PrefabUtility.SaveAsPrefabAsset(g,wallet);}finally{PrefabUtility.UnloadPrefabContents(g);}
        AssetDatabase.SaveAssets();
    }
}
