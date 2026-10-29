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
        [SerializeField] private bool firstOperation;
        public bool FirstOperation=>firstOperation;
        public bool AttemptFailed{get;private set;}
        public bool AttemptEnded=>AttemptFailed||Stage==MissionStage.Complete;
        public int OperationSection{get;private set;}
        private static readonly string[] OperationObjectives={"ENTER THE INDUSTRIAL COMPOUND", "ADVANCE THROUGH STORAGE — ladders or loading floor", "CROSS THE PURSUIT BAY — keep moving", "CROSS FIREWORK STORAGE — watch the colored warnings", "DEFEAT THE OFFICER — command office ahead"};
        private Health operationOfficer;
        private bool OfficerDefeated=>operationOfficer!=null&&operationOfficer.IsDefeated;
        private PlayerRecovery recovery;
        private Health health;
        private PlayerLoadout loadout;
        private PlayerSupplies supplies;
        private int completedEarnings;
        public int Earnings=>Stage==MissionStage.Complete?completedEarnings:supplies!=null?supplies.Earnings:0;
        private BoxCollider2D playerBox;
        private Collider2D visitedExit;
        private bool restoring,ready,officeSaved;
        private bool storageEntered;
        private float startedAt,completedTime,noticeUntil;
        private string notice;
        private MissionStage savedStage;
        private bool savedRecords;
        private readonly bool[] savedEncounters=new bool[3];
        private readonly int[] savedMagazine=new int[4],savedReserve=new int[4];
        private EquipmentSlot savedSelection;
        private readonly RaycastHit2D[] interactionHits=new RaycastHit2D[8];
        public event System.Action Changed;
        public event System.Action BeforeReplay, PreparingRun;
        public PlayerActions Player=>player;
        public MissionStage Stage{get;private set;}
        public bool HasRecords{get;private set;}
        public int Deaths{get;private set;}
        public int Kills{get;private set;}
        public int TotalDefeats{get;private set;}
        public int CheckpointCommits{get;private set;}
        public int Completions{get;private set;}
        public MissionEncounter[] Encounters=>encounters;
        public float Elapsed=>AttemptEnded?completedTime:Mathf.Max(0,Time.time-startedAt);
        public string Notification=>visitedExit!=null&&playerBox!=null&&Stage!=MissionStage.Complete&&visitedExit.bounds.Intersects(playerBox.bounds)
            ? ExitRequirement : Time.time<noticeUntil?notice:string.Empty;
        public string ExitRequirement=>firstOperation?(OfficerDefeated?"EXIT OPEN — leave through the marked dispatch door.":"EXIT LOCKED — defeat the Officer in command."):Stage switch
        {
            MissionStage.EnterWarehouse=>"EXIT LOCKED — enter the warehouse and recover the corporate records.",
            MissionStage.ClearStorage=>"EXIT LOCKED — "+Objective,
            MissionStage.ReachProcessing=>"EXIT LOCKED — activate the storage mezzanine checkpoint, then reach processing.",
            MissionStage.ClearProcessing=>"EXIT LOCKED — "+Objective,
            MissionStage.RecoverRecords=>"EXIT LOCKED — collect the records in the east mezzanine office: E / INTERACT.",
            MissionStage.Escape=>"EXIT OPEN — leave through the east ground-floor door.",
            _=>string.Empty
        };
        public bool CanInteract=>!firstOperation&&ready&&Stage==MissionStage.RecoverRecords&&recordsZone.Region.OverlapPoint(playerBox.bounds.center);
        public string Objective=>firstOperation?(AttemptFailed?"ATTEMPT ENDED — restart from the entrance":Stage==MissionStage.Complete?"OPERATION COMPLETE":OfficerDefeated?"REACH DISPATCH — marked exit beyond command":OperationObjectives[OperationSection]):Stage switch
        {
            MissionStage.EnterWarehouse=>"ENTER THE WAREHOUSE — find the corporate records",
            MissionStage.ClearStorage=>!encounters[0].Cleared?$"CLEAR THE ENTRANCE — {encounters[0].DefeatedCount}/{encounters[0].Count}":!encounters[1].IsActive&&!encounters[1].Cleared?"REACH THE STORAGE FLOOR — continue right":$"CLEAR STORAGE — {encounters[1].DefeatedCount}/{encounters[1].Count}",
            MissionStage.ReachProcessing=>"REACH PROCESSING — checkpoint on the storage mezzanine",
            MissionStage.ClearProcessing=>$"SECURE PROCESSING — {encounters[2].DefeatedCount}/{encounters[2].Count}",
            MissionStage.RecoverRecords=>"RECOVER CORPORATE RECORDS — office on the east mezzanine",
            MissionStage.Escape=>"ESCAPE THE WAREHOUSE — east ground-floor exit",
            _=>"MISSION COMPLETE"
        };
        private void Start()
        {
            recovery=player.GetComponent<PlayerRecovery>();health=player.GetComponent<Health>();loadout=player.GetComponent<PlayerLoadout>();supplies=player.GetComponent<PlayerSupplies>();playerBox=player.GetComponent<BoxCollider2D>();
            foreach(var group in encounters){group.Initialize(health);group.Progressed+=EncounterProgress;}
            if(firstOperation)operationOfficer=encounters[4].Points[0].Spawned.GetComponent<Health>();
            recovery.Recovered+=Recovered;health.Defeated+=PlayerDefeated;player.InteractionRequested+=Interact;ready=true;Replay();hud.Bind(this);
        }
        public void Replay()
        {
            if(!ready)return;BeforeReplay?.Invoke();restoring=true;AttemptFailed=false;OperationSection=0;storageEntered=false;Stage=MissionStage.EnterWarehouse;HasRecords=false;Deaths=Kills=TotalDefeats=CheckpointCommits=Completions=0;officeSaved=false;startedAt=Time.time;completedTime=0;
            completedEarnings=0;visitedExit=null;supplies?.ResetMission(!firstOperation);
            bool covered=hud!=null&&hud.CoverReset();for(int i=0;i<encounters.Length;i++)encounters[i].ResetEncounter(false,false,covered);
            foreach(EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot))){var weapon=loadout.Get(slot);if(weapon!=null){weapon.Cycle.CancelReload();weapon.SetAmmunition(weapon.Definition.MagazineCapacity,weapon.UsesAmmunition?72:0);}}
            loadout.Select(EquipmentSlot.Firearm1);player.enabled=true;player.GetComponent<PlayerCombat>().enabled=true;recovery.Bind(initialSpawn,cameraFollow);
            PreparingRun?.Invoke();if(!firstOperation)CaptureSnapshot();player.GetComponent<Rigidbody2D>().simulated=true;recovery.Respawn();ClearRagdolls();restoring=false;Notify(firstOperation?"THE FIRST OPERATION — MANY TRACERS, FEW HITS; EVERYONE SCARED.":"MISSION 01 — WAREHOUSE INCIDENT\nThe police have had five days. That's long enough.",6);
        }
        private void CaptureSnapshot()
        {
            savedStage=Stage;savedRecords=HasRecords;savedSelection=loadout.SelectedSlot;
            for(int i=0;i<encounters.Length;i++)savedEncounters[i]=encounters[i].Cleared;
            for(int i=0;i<4;i++){var weapon=loadout.Get((EquipmentSlot)i);savedMagazine[i]=weapon!=null?weapon.MagazineAmmo:0;savedReserve[i]=weapon!=null?weapon.ReserveAmmo:0;}
        }
        private void Commit(CheckpointMarker marker,Transform spawn)
        {recovery.Activate(marker,spawn.position);CaptureSnapshot();CheckpointCommits++;Notify("CHECKPOINT REACHED",2.5f);}
        private void Recovered()
        {
            if(!ready||restoring||firstOperation||Stage==MissionStage.Complete)return;restoring=true;Stage=savedStage;HasRecords=savedRecords;storageEntered=savedStage>=MissionStage.ReachProcessing;officeSaved=savedStage>=MissionStage.RecoverRecords;
            bool covered=hud!=null&&hud.CoverReset();Kills=0;for(int i=0;i<encounters.Length;i++){encounters[i].ResetEncounter(savedEncounters[i],false,covered);Kills+=encounters[i].DefeatedCount;}
            for(int i=0;i<4;i++){var weapon=loadout.Get((EquipmentSlot)i);if(weapon!=null){weapon.Cycle.CancelReload();weapon.SetAmmunition(savedMagazine[i],savedReserve[i]);}}
            loadout.Select(savedSelection);ClearRagdolls();restoring=false;Notify("BACK AT THE CHECKPOINT",3);
        }
        private void PlayerDefeated(DamageInfo damage)
        {
            if(AttemptEnded)return;Deaths++;
            if(firstOperation&&!(player.GetComponent<PlayerRevive>()?.CanOfferRevive??false))
            {AttemptFailed=true;completedTime=Time.time-startedAt;player.SuspendInput();player.enabled=false;player.GetComponent<PlayerCombat>().enabled=false;player.GetComponent<Rigidbody2D>().simulated=false;}
            Changed?.Invoke();
        }
        private void ClearRagdolls()
        {foreach(var root in gameObject.scene.GetRootGameObjects()){var ragdoll=root.GetComponent<CartoonRagdoll>();if(ragdoll!=null)ragdoll.Retire();}}
        private void EncounterProgress(MissionEncounter group)
        {
            if(restoring)return;TotalDefeats++;Kills=0;foreach(var encounter in encounters)Kills+=encounter.DefeatedCount;
            if(firstOperation){if(OfficerDefeated)Stage=MissionStage.Escape;Changed?.Invoke();return;}
            if(Stage==MissionStage.ClearStorage&&storageEntered&&encounters[0].Cleared&&encounters[1].Cleared){Stage=MissionStage.ReachProcessing;Notify("STORAGE SECURED — checkpoint upstairs to the right",4);}
            else if(Stage==MissionStage.ClearProcessing&&encounters[2].Cleared){Stage=MissionStage.RecoverRecords;Notify("PROCESSING SECURED — records office on the east mezzanine",4);}
            else Changed?.Invoke();
        }
        public bool EnterZone(MissionZone zone,Collider2D region)
        {
            if(!ready||restoring||AttemptEnded||health.IsDefeated||region==null||!region.bounds.Intersects(playerBox.bounds))return false;
            var trigger=region.GetComponent<MissionTrigger>();if(trigger==null||trigger.Mission!=this||trigger.Zone!=zone)return false;
            if(firstOperation)
            {
                if(zone==MissionZone.Exit){bool first=visitedExit!=region;visitedExit=region;if(!OfficerDefeated&&!first)return false;}
                return EnterOperationZone(zone);
            }
            switch(zone)
            {
                case MissionZone.Entrance:
                    if(Stage!=MissionStage.EnterWarehouse)return false;Stage=MissionStage.ClearStorage;encounters[0].Activate();Notify("The entrance guard seems unconvinced.",3);return true;
                case MissionZone.Storage:
                    if(Stage!=MissionStage.ClearStorage||!encounters[0].Cleared||storageEntered)return false;storageEntered=true;encounters[1].Activate();if(encounters[1].Cleared)Stage=MissionStage.ReachProcessing;Notify("STORAGE — ground floor and mezzanine. Ladder to the west.",4);return true;
                case MissionZone.Checkpoint:
                    if(Stage!=MissionStage.ReachProcessing){CheckpointBlocked(false);return false;}if(savedStage==MissionStage.ReachProcessing)return false;Commit(storageCheckpoint,storageSpawn);return true;
                case MissionZone.Processing:
                    if(Stage!=MissionStage.ReachProcessing||savedStage!=MissionStage.ReachProcessing)return false;Stage=MissionStage.ClearProcessing;encounters[2].Activate();if(encounters[2].Cleared)Stage=MissionStage.RecoverRecords;Notify("PROCESSING — gantry above, loading floor below",4);return true;
                case MissionZone.OfficeCheckpoint:
                    if(Stage!=MissionStage.RecoverRecords){CheckpointBlocked(true);return false;}if(officeSaved)return false;officeSaved=true;Commit(officeCheckpoint,officeSpawn);return true;
                case MissionZone.Records:
                    return Stage==MissionStage.RecoverRecords&&!HasRecords;
                case MissionZone.Exit:
                    bool firstContact=visitedExit!=region;
                    visitedExit=region;
                    if(Stage!=MissionStage.Escape||!HasRecords){if(firstContact)Notify(ExitRequirement,4);return false;}
                    completedEarnings=supplies!=null?supplies.Earnings:0;Stage=MissionStage.Complete;Completions++;completedTime=Time.time-startedAt;player.ResetActions();player.enabled=false;player.GetComponent<PlayerCombat>().enabled=false;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Changed?.Invoke();return true;
                default:return false;
            }
        }
        private void Interact()=>TryRecoverRecords();
        private bool EnterOperationZone(MissionZone zone)
        {
            if(zone==MissionZone.Exit)
            {
                if(!OfficerDefeated){Notify(ExitRequirement,3);return false;}
                completedEarnings=supplies!=null?supplies.Earnings:0;Stage=MissionStage.Complete;Completions++;completedTime=Time.time-startedAt;
                player.ResetActions();player.enabled=false;player.GetComponent<PlayerCombat>().enabled=false;player.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;Changed?.Invoke();return true;
            }
            int index=zone==MissionZone.Entrance?0:zone==MissionZone.Storage?1:zone==MissionZone.SamuraiArena?2:zone==MissionZone.FireworksArena?3:zone==MissionZone.Command?4:-1;
            if(index<0||index<OperationSection||index==OperationSection&&Stage!=MissionStage.EnterWarehouse)return false;
            // Progression is spatial, never gated by a frightened ordinary enemy's survival.
            for(int i=0;i<=index;i++)encounters[i].Activate();OperationSection=index;Stage=OfficerDefeated?MissionStage.Escape:MissionStage.ClearStorage;Notify(OperationObjectives[index],3);return true;
        }
        private void CheckpointBlocked(bool office)
        {
            if(Stage>=MissionStage.Escape||officeSaved&&office||(office?officeCheckpoint:storageCheckpoint).IsLit)return;
            string reason=Stage==MissionStage.ClearStorage?$"CLEAR ENTRANCE / STORAGE FIRST — {encounters[0].Count-encounters[0].DefeatedCount+encounters[1].Count-encounters[1].DefeatedCount} enemies remain (check both floors).":
                Stage==MissionStage.ClearProcessing?$"SECURE PROCESSING FIRST — {encounters[2].Count-encounters[2].DefeatedCount} enemies remain (gantry, mezzanine and exit).":
                Stage==MissionStage.ReachProcessing?(savedStage!=MissionStage.ReachProcessing?"ACTIVATE THE STORAGE CHECKPOINT FIRST.":"REACH PROCESSING — cross east of the storage stairs."):Objective;
            string message="CHECKPOINT LOCKED — "+reason;
            if(notice!=message||Time.time>=noticeUntil)Notify(message,4);
        }
        [ContextMenu("Log Mission Progress")]
        private void LogProgress()=>Debug.Log($"Mission={name} stage={Stage} saved={savedStage} records={HasRecords} checkpoints={CheckpointCommits} kills={Kills} defeats={TotalDefeats} objective={Objective}",this);
        public bool TryRecoverRecords()
        {
            if(!CanInteract)return false;
            if(CombatVisibility2D.IsBlocked(playerBox.bounds.center,recordsZone.Region.bounds.center,1,interactionHits))return false;
            HasRecords=true;Stage=MissionStage.Escape;CaptureSnapshot();Notify("Evidence secured. Probably.\nExit via the east ground floor.",5);return true;
        }
        private void Notify(string value,float seconds){notice=value;noticeUntil=Time.time+seconds;Changed?.Invoke();}
        private void OnDestroy()
        {
            if(!ready)return;if(recovery!=null)recovery.Recovered-=Recovered;if(health!=null)health.Defeated-=PlayerDefeated;if(player!=null)player.InteractionRequested-=Interact;
            foreach(var group in encounters)if(group!=null)group.Progressed-=EncounterProgress;
        }
    }
}
