using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace InsaneDreams.Progression
{
    [Serializable] public sealed class AmmoStock { public string id; public int magazine,reserve; }
    [Serializable] public sealed class GameProgress
    {
        public int version=1, balance, pendingEarnings;
        public int faintanyl=2;
        public string primary,secondary,melee;
        public float tape=200,armor,volume=1,joystickScale=1.15f;
        public List<string> owned=new List<string>(),completed=new List<string>(),settledRuns=new List<string>();
        public List<AmmoStock> ammunition=new List<AmmoStock>();
        public string activeRun,activeLevel;
        public AmmoStock Ammo(string id)
        {
            var value=ammunition.Find(x=>x.id==id);
            if(value==null){value=new AmmoStock{id=id};ammunition.Add(value);}return value;
        }
        public void Normalize()
        {
            owned??=new List<string>();completed??=new List<string>();settledRuns??=new List<string>();ammunition??=new List<AmmoStock>();
            owned.RemoveAll(string.IsNullOrEmpty);completed.RemoveAll(string.IsNullOrEmpty);ammunition.RemoveAll(x=>x==null||string.IsNullOrEmpty(x.id));
            balance=Mathf.Clamp(balance,0,1000000);tape=Finite(tape,200);armor=Finite(armor,0);
            faintanyl=Mathf.Clamp(faintanyl,0,2);
            volume=Mathf.Clamp01(Finite(volume,1));joystickScale=Mathf.Clamp(Finite(joystickScale,1.15f),.75f,1.5f);
            foreach(var ammo in ammunition){ammo.magazine=Mathf.Clamp(ammo.magazine,0,10000);ammo.reserve=Mathf.Clamp(ammo.reserve,0,10000);}
        }
        private static float Finite(float value,float fallback)=>float.IsNaN(value)||float.IsInfinity(value)?fallback:Mathf.Max(0,value);
    }
    public sealed class ProgressStore
    {
        public readonly string Path;
        public bool ReadOnly{get;private set;}
        public string Problem{get;private set;}
        public ProgressStore(string path){Path=path;}
        public GameProgress Load(Func<GameProgress> initial)
        {
            if(!File.Exists(Path))return initial();
            try
            {
                var text=File.ReadAllText(Path);
                if(!text.TrimStart().StartsWith("{")||!text.Contains("\"version\""))throw new InvalidDataException("Unrecognized save format.");
                var data=JsonUtility.FromJson<GameProgress>(text);
                if(data==null||data.version!=1)throw new InvalidDataException("Unsupported save version.");
                data.Normalize();return data;
            }
            catch(Exception e){ReadOnly=true;Problem="Existing save preserved: "+e.Message;return initial();}
        }
        public bool Save(GameProgress data)
        {
            if(ReadOnly)return false;
            string temp=Path+".tmp";
            try
            {
                Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
                File.WriteAllText(temp,JsonUtility.ToJson(data,true));
                if(File.Exists(Path))File.Replace(temp,Path,Path+".bak");else File.Move(temp,Path);
                Problem=null;return true;
            }
            catch(Exception e){Problem="Progress could not be saved: "+e.Message;return false;}
        }
    }
}
