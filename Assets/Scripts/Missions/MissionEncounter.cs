using System;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using UnityEngine;
namespace InsaneDreams.Missions
{
    public sealed class MissionEncounter : MonoBehaviour
    {
        [SerializeField] private EnemySpawnPoint[] points;
        private Health[] actors;
        private bool[] defeated;
        private Action<DamageInfo>[] handlers;
        private bool engaged;
        public event Action<MissionEncounter> Progressed;
        public int Count=>points.Length;
        public int DefeatedCount{get;private set;}
        public bool Cleared=>DefeatedCount==Count;
        public bool IsActive=>engaged&&!Cleared;
        public EnemySpawnPoint[] Points=>points;
        public void Initialize(Health player)
        {
            if(actors!=null)return;actors=new Health[points.Length];defeated=new bool[points.Length];handlers=new Action<DamageInfo>[points.Length];
            for(int i=0;i<points.Length;i++)
            {
                points[i].SetEncounterEnabled(false);points[i].BindTarget(player);var actor=points[i].Prepare();actors[i]=actor.GetComponent<Health>();
                int index=i;handlers[i]=_=>EnemyDefeated(index);actors[i].Defeated+=handlers[i];actors[i].Damaged+=AlertOnPlayerHit;actor.GetComponent<EnemyCombat>().ThreatNoticed+=Noticed;
            }
        }
        private void EnemyDefeated(int index)
        {
            if(defeated[index])return;defeated[index]=true;DefeatedCount++;
            if(Cleared)engaged=false;Progressed?.Invoke(this);
        }
        private void AlertOnPlayerHit(DamageInfo damage){if(damage.Team==CombatTeam.Player)Activate();}
        private void Noticed(){if(!Cleared)engaged=true;}
        public void ResetEncounter(bool completed,bool activate,bool coveredReset=false)
        {
            engaged=false;DefeatedCount=completed?Count:0;
            for(int i=0;i<points.Length;i++){defeated[i]=completed;points[i].ResetEncounter(!completed&&activate,completed,coveredReset);}
            engaged=!completed&&activate;
        }
        public void Activate(){if(Cleared)return;engaged=true;foreach(var point in points)if(!point.Spawned.GetComponent<Health>().IsDefeated)point.ActivateEncounter();}
        private void OnDestroy(){if(actors==null)return;for(int i=0;i<actors.Length;i++)if(actors[i]!=null){actors[i].Defeated-=handlers[i];actors[i].Damaged-=AlertOnPlayerHit;actors[i].GetComponent<EnemyCombat>().ThreatNoticed-=Noticed;}}
    }
}
