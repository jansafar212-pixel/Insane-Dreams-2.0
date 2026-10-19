using InsaneDreams.CameraSystem;
using InsaneDreams.Checkpoints;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using UnityEngine;
namespace InsaneDreams.Missions
{
    public enum MissionStage { EnterWarehouse, ClearStorage, ReachProcessing, ClearProcessing, RecoverRecords, Escape, Complete }
    public sealed class WarehouseMission : MonoBehaviour
    {
        [SerializeField] private PlayerActions player;
        [SerializeField] private Transform initialSpawn,storageSpawn,officeSpawn;
        [SerializeField] private CheckpointMarker storageCheckpoint,officeCheckpoint;
        [SerializeField] private MissionEncounter[] encounters;
        [SerializeField] private MissionTrigger recordsZone;
        [SerializeField] private WarehouseCameraFollow cameraFollow;
        [SerializeField] private MissionHud hud;
        private PlayerRecovery recovery;
        private Health health;
        private PlayerLoadout loadout;
        private BoxCollider2D playerBox;
        private bool restoring,ready,officeSaved;
        private float startedAt,completedTime,noticeUntil;
        private string notice;
        private MissionStage savedStage;
        private bool savedRecords;
        private readonly bool[] savedEncounters=new bool[3];
        private readonly int[] savedMagazine=new int[3],savedReserve=new int[3];
        private EquipmentSlot savedSelection;
        private readonly RaycastHit2D[] interactionHits=new RaycastHit2D[8];
        public event System.Action Changed;
        public PlayerActions Player=>player;
        public MissionStage Stage{get;private set;}
        public bool HasRecords{get;private set;}
        public int Deaths{get;private set;}
        public int Kills{get;private set;}
        public int TotalDefeats{get;private set;}
        public int CheckpointCommits{get;private set;}
        public int Completions{get;private set;}
        public MissionEncounter[] Encounters=>encounters;
        public float Elapsed=>Stage==MissionStage.Complete?completedTime:Mathf.Max(0,Time.time-startedAt);
        public string Notification=>Time.time<noticeUntil?notice:string.Empty;
        public bool CanInteract=>ready&&Stage==MissionStage.RecoverRecords&&recordsZone.Region.OverlapPoint(playerBox.bounds.center);
        public string Objective=>Stage switch
        {
            MissionStage.EnterWarehouse=>"ENTER THE WAREHOUSE — find the corporate records",
            MissionStage.ClearStorage=>!encounters[0].Cleared?"NEUTRALIZE THE ENTRANCE GUARD":!encounters[1].IsActive&&!encounters[1].Cleared?"REACH THE STORAGE FLOOR — continue right":$"CLEAR STORAGE — {encounters[1].DefeatedCount}/{encounters[1].Count}",
            MissionStage.ReachProcessing=>"REACH PROCESSING — checkpoint on the storage mezzanine",
            MissionStage.ClearProcessing=>$"SECURE PROCESSING — {encounters[2].DefeatedCount}/{encounters[2].Count}",
            MissionStage.RecoverRecords=>"RECOVER CORPORATE RECORDS — office on the east mezzanine",
            MissionStage.Escape=>"ESCAPE THE WAREHOUSE — east ground-floor exit",
            _=>"MISSION COMPLETE"
        };
        private void Start()
        {
            recovery=player.GetComponent<PlayerRecovery>();health=player.GetComponent<Health>();loadout=player.GetComponent<PlayerLoadout>();playerBox=player.GetComponent<BoxCollider2D>();
            foreach(var group in encounters){group.Initialize(health);group.Progressed+=EncounterProgress;}
            recovery.Recovered+=Recovered;health.Defeated+=PlayerDefeated;player.InteractionRequested+=Interact;ready=true;Replay();hud.Bind(this);
        }
        public void Replay()
        {
            if(!ready)return;restoring=true;Stage=MissionStage.EnterWarehouse;HasRecords=false;Deaths=Kills=TotalDefeats=CheckpointCommits=Completions=0;officeSaved=false;startedAt=Time.time;completedTime=0;
            bool covered=hud!=null&&hud.CoverReset();for(int i=0;i<encounters.Length;i++)encounters[i].ResetEncounter(false,false,covered);
            foreach(EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot))){var weapon=loadout.Get(slot);if(weapon!=null){weapon.Cycle.CancelReload();weapon.SetAmmunition(weapon.Definition.MagazineCapacity,weapon.UsesAmmunition?72:0);}}
            loadout.Select(EquipmentSlot.Firearm1);player.enabled=true;player.GetComponent<PlayerCombat>().enabled=true;recovery.Bind(initialSpawn,cameraFollow);
            CaptureSnapshot();recovery.Respawn();ClearRagdolls();restoring=false;Notify("MISSION 01 — WAREHOUSE INCIDENT\nThe police have had five days. That's long enough.",6);
        }
        private void CaptureSnapshot()
        {
            savedStage=Stage;savedRecords=HasRecords;savedSelection=loadout.SelectedSlot;
            for(int i=0;i<encounters.Length;i++)savedEncounters[i]=encounters[i].Cleared;
            for(int i=0;i<3;i++){var weapon=loadout.Get((EquipmentSlot)i);savedMagazine[i]=weapon!=null?weapon.MagazineAmmo:0;savedReserve[i]=weapon!=null?weapon.ReserveAmmo:0;}
        }
        private void Commit(CheckpointMarker marker,Transform spawn)
        {recovery.Activate(marker,spawn.position);CaptureSnapshot();CheckpointCommits++;Notify("CHECKPOINT REACHED",2.5f);}
        private void Recovered()
        {
            if(!ready||restoring||Stage==MissionStage.Complete)return;restoring=true;Stage=savedStage;HasRecords=savedRecords;officeSaved=savedStage>=MissionStage.RecoverRecords;
            bool covered=hud!=null&&hud.CoverReset();Kills=0;for(int i=0;i<encounters.Length;i++){encounters[i].ResetEncounter(savedEncounters[i],false,covered);Kills+=encounters[i].DefeatedCount;}
            for(int i=0;i<3;i++){var weapon=loadout.Get((EquipmentSlot)i);if(weapon!=null){weapon.Cycle.CancelReload();weapon.SetAmmunition(savedMagazine[i],savedReserve[i]);}}
            loadout.Select(savedSelection);ClearRagdolls();restoring=false;Notify("BACK AT THE CHECKPOINT",3);
        }
        private void PlayerDefeated(DamageInfo damage){if(Stage!=MissionStage.Complete){Deaths++;Changed?.Invoke();}}
        private void ClearRagdolls()
        {foreach(var root in gameObject.scene.GetRootGameObjects()){var ragdoll=root.GetComponent<CartoonRagdoll>();if(ragdoll!=null)ragdoll.Retire();}}
        private void EncounterProgress(MissionEncounter group)
        {
            if(restoring)return;TotalDefeats++;Kills=0;foreach(var encounter in encounters)Kills+=encounter.DefeatedCount;
            if(group==encounters[1]&&group.Cleared&&Stage==MissionStage.ClearStorage&&encounters[0].Cleared){Stage=MissionStage.ReachProcessing;Notify("STORAGE SECURED — checkpoint upstairs to the right",4);}
            else if(group==encounters[2]&&group.Cleared&&Stage==MissionStage.ClearProcessing){Stage=MissionStage.RecoverRecords;Notify("PROCESSING SECURED — records office on the east mezzanine",4);}
            else Changed?.Invoke();
        }
        public bool EnterZone(MissionZone zone,Collider2D region)
        {
            if(!ready||restoring||Stage==MissionStage.Complete||region==null||!region.bounds.Intersects(playerBox.bounds))return false;
            var trigger=region.GetComponent<MissionTrigger>();if(trigger==null||trigger.Mission!=this||trigger.Zone!=zone)return false;
            switch(zone)
            {
                case MissionZone.Entrance:
                    if(Stage!=MissionStage.EnterWarehouse)return false;Stage=MissionStage.ClearStorage;encounters[0].Activate();Notify("The entrance guard seems unconvinced.",3);return true;
                case MissionZone.Storage:
                    if(Stage!=MissionStage.ClearStorage||!encounters[0].Cleared||encounters[1].IsActive)return false;encounters[1].Activate();if(encounters[1].Cleared)Stage=MissionStage.ReachProcessing;Notify("STORAGE — ground floor and mezzanine. Ladder to the west.",4);return true;
                case MissionZone.Checkpoint:
                    if(Stage!=MissionStage.ReachProcessing||savedStage==MissionStage.ReachProcessing)return false;Commit(storageCheckpoint,storageSpawn);return true;
                case MissionZone.Processing:
                    if(Stage!=MissionStage.ReachProcessing||savedStage!=MissionStage.ReachProcessing)return false;Stage=MissionStage.ClearProcessing;encounters[2].Activate();if(encounters[2].Cleared)Stage=MissionStage.RecoverRecords;Notify("PROCESSING — gantry above, loading floor below",4);return true;
                case MissionZone.Records:
                    if(Stage!=MissionStage.RecoverRecords||officeSaved)return false;officeSaved=true;Commit(officeCheckpoint,officeSpawn);return true;
                case MissionZone.Exit:
                    if(Stage!=MissionStage.Escape||!HasRecords){Notify("The records are still inside.",2);return false;}
                    Stage=MissionStage.Complete;Completions++;completedTime=Time.time-startedAt;player.ResetActions();player.enabled=false;player.GetComponent<PlayerCombat>().enabled=false;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Changed?.Invoke();return true;
                default:return false;
            }
        }
        private void Interact()=>TryRecoverRecords();
        public bool TryRecoverRecords()
        {
            if(!CanInteract)return false;
            if(CombatVisibility2D.IsBlocked(playerBox.bounds.center,recordsZone.Region.bounds.center,1,interactionHits))return false;
            HasRecords=true;Stage=MissionStage.Escape;officeSaved=true;Commit(officeCheckpoint,officeSpawn);Notify("Evidence secured. Probably.\nExit via the east ground floor.",5);return true;
        }
        private void Notify(string value,float seconds){notice=value;noticeUntil=Time.time+seconds;Changed?.Invoke();}
        private void OnDestroy()
        {
            if(!ready)return;if(recovery!=null)recovery.Recovered-=Recovered;if(health!=null)health.Defeated-=PlayerDefeated;if(player!=null)player.InteractionRequested-=Interact;
            foreach(var group in encounters)if(group!=null)group.Progressed-=EncounterProgress;
        }
    }
}
