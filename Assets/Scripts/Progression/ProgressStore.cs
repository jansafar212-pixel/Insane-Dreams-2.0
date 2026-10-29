using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace InsaneDreams.Progression
{
    [Serializable] public sealed class AmmoStock { public string id; public int magazine,reserve; }
    [Serializable] public sealed class ArmorStock { public string id; public float condition; }
    [Serializable] public sealed class GameProgress
    {
        public int version=1, balance, pendingEarnings;
        public int developmentCreditRevision;
        public int faintanyl;
        public int bodyArmorTier,helmetTier;
        public float helmet;
        public int[] grenades=new int[3];
        public string primary,secondary,melee;
        public int loadoutRevision;
        public float tape=200,armor,volume=1,joystickScale=1.15f;
        public List<string> owned=new List<string>(),completed=new List<string>(),settledRuns=new List<string>();
        public List<AmmoStock> ammunition=new List<AmmoStock>();
        public List<ArmorStock> protection=new List<ArmorStock>();
        public string bodyArmorId,helmetId;
        public int lastWallets,lastPayout,lastReward;
        public bool lastComplete;
        public string activeRun,activeLevel;
        public AmmoStock Ammo(string id)
        {
            var value=ammunition.Find(x=>x.id==id);
            if(value==null){value=new AmmoStock{id=id};ammunition.Add(value);}return value;
        }
        public void Normalize()
        {
            // JsonUtility can round-trip null strings as empty strings. Canonical
            // empty equipment IDs make intentionally empty slots stable in memory/disk.
            if(string.IsNullOrEmpty(primary))primary=null;if(string.IsNullOrEmpty(secondary))secondary=null;
            if(string.IsNullOrEmpty(melee))melee=null;
            owned??=new List<string>();completed??=new List<string>();settledRuns??=new List<string>();ammunition??=new List<AmmoStock>();
            owned.RemoveAll(string.IsNullOrEmpty);completed.RemoveAll(string.IsNullOrEmpty);ammunition.RemoveAll(x=>x==null||string.IsNullOrEmpty(x.id));
            protection??=new List<ArmorStock>();protection.RemoveAll(x=>x==null||string.IsNullOrEmpty(x.id));foreach(var gear in protection)gear.condition=Finite(gear.condition,0);
            pendingEarnings=Mathf.Clamp(pendingEarnings,0,1000000);
            balance=Mathf.Clamp(balance,0,1000000);tape=Finite(tape,200);armor=Finite(armor,0);
            faintanyl=Mathf.Clamp(faintanyl,0,2);
            helmet=Finite(helmet,0);bodyArmorTier=Mathf.Clamp(bodyArmorTier,0,2);helmetTier=Mathf.Clamp(helmetTier,0,2);if(bodyArmorTier==0&&armor>0)bodyArmorTier=1;if(helmetTier==0&&helmet>0)helmetTier=1;
            if(grenades==null)grenades=new int[3];else if(grenades.Length!=3)Array.Resize(ref grenades,3);
            for(int i=0;i<3;i++)grenades[i]=Mathf.Clamp(grenades[i],0,Combat.PlayerThrowables.Capacity);
            volume=Mathf.Clamp01(Finite(volume,1));joystickScale=Mathf.Clamp(Finite(joystickScale,1.15f),.75f,1.5f);
            foreach(var ammo in ammunition){ammo.magazine=Mathf.Clamp(ammo.magazine,0,10000);ammo.reserve=Mathf.Clamp(ammo.reserve,0,10000);}
        }
        private static float Finite(float value,float fallback)=>float.IsNaN(value)||float.IsInfinity(value)?fallback:Mathf.Max(0,value);
    }
    public sealed class ProgressStore
    {
        // Read-only legacy envelope; never part of the current equipment/save model.
        [Serializable] private sealed class LegacyLoadout { public string secondary2; }
        public string LegacyExtraSecondary{get;private set;}
        public readonly string Path;
        public bool ReadOnly{get;private set;}
        public string Problem{get;private set;}
        public ProgressStore(string path){Path=path;}
        public GameProgress Load(Func<GameProgress> initial)
        {
            LegacyExtraSecondary=null;
            if(!File.Exists(Path))return initial();
            try
            {
                var text=File.ReadAllText(Path);
                if(!text.TrimStart().StartsWith("{")||!text.Contains("\"version\""))throw new InvalidDataException("Unrecognized save format.");
                var data=JsonUtility.FromJson<GameProgress>(text);
                LegacyExtraSecondary=JsonUtility.FromJson<LegacyLoadout>(text)?.secondary2;
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
