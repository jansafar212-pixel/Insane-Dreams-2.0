// Sprint 015 live Editor diagnostic. Uses real PlayerActions and normal combat;
// healing is requested through the same event as H. No teleports, immunity or damage injection.
// Run with S15PilotResume=false for a fresh playthrough; pauses on completion/stall.
var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
if(scene.name!="Warehouse_Mission01"||!UnityEditor.EditorApplication.isPlaying)throw new System.InvalidOperationException("Open the mission in Play Mode first.");
var mission=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<InsaneDreams.Missions.WarehouseMission>()).Single();
bool resume=UnityEditor.SessionState.GetBool("S15PilotResume",false);if(!resume)mission.Replay();
var player=mission.Player;var body=player.GetComponent<UnityEngine.Rigidbody2D>();var motor=player.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>();var traversal=player.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>();var loadout=player.GetComponent<InsaneDreams.Inventory.PlayerLoadout>();var attack=player.GetComponent<InsaneDreams.Combat.AttackExecutor>();
var enemies=mission.Encounters.SelectMany(g=>g.Points).Select(p=>p.Spawned.GetComponent<InsaneDreams.Enemies.EnemyCombat>()).ToArray();var hits=new UnityEngine.RaycastHit2D[8];
int phase=resume?UnityEditor.SessionState.GetInt("S15PilotStartPhase",0):0;bool jumped=false,recovered=false;float phaseStarted=UnityEngine.Time.time,lastSample=0;bool previousBackground=UnityEngine.Application.runInBackground;UnityEngine.Application.runInBackground=true;
var trace=resume?new System.Collections.Generic.List<string>(UnityEditor.SessionState.GetString("S15PilotTrace","").Split('\n')):new System.Collections.Generic.List<string>();if(resume)trace.Add("RESUME phase="+phase+" pos="+body.position);
trace.Add("NORMAL COMBAT ONLY; healing requested through PlayerActions when HP <= 65.");
System.Action<float> healed=amount=>trace.Add("HEALED +"+amount+" at t="+mission.Elapsed+" hp="+player.GetComponent<InsaneDreams.Combat.Health>().Current);player.GetComponent<InsaneDreams.Combat.PlayerHealing>().Healed+=healed;
System.Action recover=()=>recovered=true;player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Recovered+=recover;
System.Action<int> next=p=>{phase=p;jumped=false;phaseStarted=UnityEngine.Time.time;trace.Add("PHASE "+phase+" t="+mission.Elapsed.ToString("F2")+" pos="+body.position+" hp="+player.GetComponent<InsaneDreams.Combat.Health>().Current+" kills="+mission.Kills);};
System.Func<float,bool> walk=x=>{float dx=x-body.position.x;player.SetTouchMove(new UnityEngine.Vector2(UnityEngine.Mathf.Clamp(dx*3,-1,1),0));return UnityEngine.Mathf.Abs(dx)<.18f;};
System.Func<float,float,bool> jump=(x,y)=>{if(!jumped&&motor.IsGrounded){player.Jump();jumped=true;}walk(x);return jumped&&motor.IsGrounded&&UnityEngine.Mathf.Abs(body.position.x-x)<.4f&&UnityEngine.Mathf.Abs(body.position.y-y)<.4f;};
UnityEditor.EditorApplication.CallbackFunction tick=null;
System.Action<string> stop=status=>{player.SetTouchMove(UnityEngine.Vector2.zero);player.SetFireIntent(false);UnityEditor.EditorApplication.update-=tick;player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Recovered-=recover;player.GetComponent<InsaneDreams.Combat.PlayerHealing>().Healed-=healed;UnityEditor.SessionState.SetString("S15PilotTrace",string.Join("\n",trace));UnityEditor.SessionState.SetString("S15PilotStatus",status+" phase="+phase+" stage="+mission.Stage+" pos="+body.position+" kills="+mission.Kills+" deaths="+mission.Deaths+" time="+mission.Elapsed);UnityEditor.EditorApplication.isPaused=true;UnityEngine.Application.runInBackground=previousBackground;};
tick=()=>
{
    if(!UnityEditor.EditorApplication.isPlaying){UnityEditor.EditorApplication.update-=tick;return;}
    if(mission.Stage==InsaneDreams.Missions.MissionStage.Complete){stop("COMPLETED");return;}
    if(player.GetComponent<InsaneDreams.Combat.Health>().Current<=65)player.RequestHeal();
    if(recovered){recovered=false;next(mission.Stage==InsaneDreams.Missions.MissionStage.EnterWarehouse?0:mission.Stage==InsaneDreams.Missions.MissionStage.ReachProcessing?13:mission.Stage==InsaneDreams.Missions.MissionStage.Escape?25:23);}
    if(UnityEngine.Time.time-phaseStarted>25||mission.Elapsed>180){trace.Add("STALLED "+body.position+" stage="+mission.Stage);stop("STALLED");return;}
    if(UnityEngine.Time.time-lastSample>1){lastSample=UnityEngine.Time.time;UnityEditor.SessionState.SetString("S15PilotProgress","phase="+phase+" stage="+mission.Stage+" pos="+body.position+" hp="+player.GetComponent<InsaneDreams.Combat.Health>().Current+" kills="+mission.Kills+" time="+mission.Elapsed);}
    var weapon=loadout.SelectedWeapon;InsaneDreams.Enemies.EnemyCombat target=null;float nearest=float.PositiveInfinity;UnityEngine.Vector2 targetPoint=UnityEngine.Vector2.zero;
    foreach(var enemy in enemies)
    {
        if(!enemy.gameObject.activeInHierarchy||enemy.GetComponent<InsaneDreams.Combat.Health>().IsDefeated)continue;
        var bounds=enemy.GetComponent<UnityEngine.BoxCollider2D>().bounds;var point=new UnityEngine.Vector2(bounds.center.x,bounds.max.y-.2f);float distance=(point-(UnityEngine.Vector2)player.transform.position).sqrMagnitude;
        if(distance>=nearest||distance>weapon.Definition.AttackRange*weapon.Definition.AttackRange||!InsaneDreams.CameraSystem.GameplayViewport.Overlaps(UnityEngine.Camera.main,bounds,enemy.GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>().Rig.torso.gameObject.layer)||InsaneDreams.Combat.CombatVisibility2D.IsBlocked(attack.Origin,point,1,hits))continue;
        nearest=distance;target=enemy;targetPoint=point;
    }
    if(target!=null){player.SetTouchAim((targetPoint-attack.Origin).normalized);player.SetFireIntent(!weapon.Cycle.IsReloading&&weapon.MagazineAmmo>0);}else player.SetFireIntent(false);
    if(weapon.MagazineAmmo==0&&weapon.ReserveAmmo>0&&!weapon.Cycle.IsReloading)player.RequestReload();
    switch(phase)
    {
        case 0:player.SetTouchMove(new UnityEngine.Vector2(1,-1));if(body.position.x>=-19.4f)next(1);break;
        case 1:player.SetTouchMove(UnityEngine.Vector2.zero);if(mission.Encounters[0].Cleared)next(2);break;
        case 2:if(jump(-16,.71f))next(3);break;
        case 3:if(walk(-16)&&motor.IsGrounded)next(4);break;
        case 4:if(jump(-9,.71f))next(5);break;
        case 5:player.SetTouchMove(UnityEngine.Vector2.zero);if(mission.Encounters[1].Points.Take(2).All(p=>p.Spawned.GetComponent<InsaneDreams.Combat.Health>().IsDefeated))next(6);break;
        case 6:player.SetTouchMove(new UnityEngine.Vector2(-.35f,1));if(traversal.IsClimbing)next(7);else if(body.position.y< -2)next(28);break;
        case 7:player.SetTouchMove(UnityEngine.Vector2.up);if(body.position.y>=6.15f)next(8);break;
        case 8:player.SetTouchMove(UnityEngine.Vector2.right);if(!traversal.IsClimbing&&body.position.x>=-9.3f)next(9);break;
        case 9:player.SetTouchMove(UnityEngine.Vector2.zero);if(mission.Encounters[1].Cleared)next(10);else if(mission.Encounters[1].Points.Any(p=>!p.Spawned.GetComponent<InsaneDreams.Combat.Health>().IsDefeated&&p.Spawned.transform.position.y<body.position.y-2))next(30);break;
        case 10:if(walk(-4.7f)&&motor.IsGrounded)next(11);break;
        case 11:if(jump(.5f,6.11f))next(12);break;
        case 12:walk(2.2f);if(mission.CheckpointCommits>0)next(13);break;
        case 13:
            if(weapon.MagazineAmmo<weapon.Definition.MagazineCapacity&&weapon.ReserveAmmo>0&&!weapon.Cycle.IsReloading)player.RequestReload();
            if(weapon.Cycle.IsReloading){player.SetTouchMove(UnityEngine.Vector2.zero);break;}
            if(walk(1.35f)&&motor.IsGrounded)next(14);break;
        case 14:if(jump(4.65f,8.01f))next(15);break;
        case 15:if(jump(7.5f,9.71f))next(16);break;
        case 16:if(jump(10.4f,11.41f))next(17);break;
        case 17:if(jump(13.6f,12.71f))next(18);break;
        case 18:if(walk(14.8f)&&motor.IsGrounded)next(19);break;
        case 19:if(jump(18.2f,12.71f))next(20);break;
        case 20:if(jump(21.1f,12.71f))next(21);break;
        case 21:walk(22);if(body.position.x>21.85f)next(22);break;
        case 22:if(body.position.y>10.7f)player.SetTouchMove(UnityEngine.Vector2.zero);else walk(20);if(motor.IsGrounded&&body.position.y<8)next(23);break;
        case 23:walk(18.8f);if(mission.Encounters[2].Cleared)next(24);else if(mission.Encounters[2].Points.Any(p=>!p.Spawned.GetComponent<InsaneDreams.Combat.Health>().IsDefeated&&p.Spawned.transform.position.y>10))next(32);break;
        case 24:walk(20);if(mission.CanInteract){player.RequestInteraction();next(25);}break;
        case 25:walk(23);if(body.position.x>22.6f)next(26);break;
        case 26:if(body.position.y<4.5f)walk(25);else player.SetTouchMove(UnityEngine.Vector2.zero);if(motor.IsGrounded&&body.position.y<2)next(27);break;
        case 27:walk(25);break;
        case 28:if(walk(-12.05f)&&motor.IsGrounded)next(7);break;
        case 30:player.SetTouchMove(new UnityEngine.Vector2(-.35f,1));if(traversal.IsClimbing)next(31);break;
        case 31:player.SetTouchMove(body.position.y>2?UnityEngine.Vector2.down:UnityEngine.Vector2.zero);if(mission.Encounters[1].Cleared)next(7);break;
        case 32:player.SetTouchMove(new UnityEngine.Vector2(.35f,1));if(traversal.IsClimbing)next(33);break;
        case 33:player.SetTouchMove(UnityEngine.Vector2.up);if(body.position.y>12.95f)next(34);break;
        case 34:
            if(mission.Encounters[2].Points.Any(p=>!p.Spawned.GetComponent<InsaneDreams.Combat.Health>().IsDefeated&&p.Spawned.transform.position.y>10))walk(24.8f);
            else {walk(23.5f);if(body.position.x<23.65f)next(22);}break;
    }
};
UnityEditor.SessionState.SetString("S15PilotStatus","RUNNING");UnityEditor.EditorApplication.update+=tick;UnityEditor.EditorApplication.isPaused=false;
return "Sprint015 mission pilot armed: normal combat, real healing requests, no damage injection.";
