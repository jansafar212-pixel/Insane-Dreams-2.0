using InsaneDreams.Combat;
using InsaneDreams.CameraSystem;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    public enum SpawnBlockReason { None, Unprepared, Finished, PlayerUnavailable, CameraUnavailable, PassedProgress, PlayerProximity, CameraDepth, CameraVisible, BodyBlocked, Crowded, Unsupported }
    [DefaultExecutionOrder(200)]
    public sealed class EnemySpawnPoint : MonoBehaviour
    {
        [SerializeField] private EnemyArchetype archetype;
        [SerializeField] private bool spawnOnStart=true;
        [SerializeField] private Transform spawnedParent;
        [SerializeField] private InsaneDreams.Traversal.LadderVolume ladderRoute;
        [SerializeField] private float routeTargetHeight=6.6f;
        [SerializeField] private LarperPersonalityProfile personality;
        [Tooltip("An authored scene occupant exists before the first rendered frame; it is not a runtime spawn.")]
        [SerializeField] private LarperBrain placedActor;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField, Min(0)] private float cameraSafetyMargin=1.25f;
        [Tooltip("Minimum center distance from active Larpers for runtime activation. Set lower for intentional dense encounters; authored occupants are preserved.")]
        [SerializeField, Min(0)] private float minimumSpawnSpacing=1.5f;
        [SerializeField, Min(.05f)] private float retrySeconds=.2f;
        [SerializeField] private LayerMask geometry=1;
        [SerializeField, Min(.05f)] private float supportDistance=.6f;
        [Tooltip("Authored level progression, independent of facing. Both warehouse layouts progress west to east. Override for a reverse-route spawn zone.")]
        [SerializeField] private Vector2 progressionDirection=Vector2.right;
        [SerializeField, Min(0)] private float minimumPlayerDistance=3;
        private readonly Collider2D[] overlaps=new Collider2D[32];
        private readonly RaycastHit2D[] support=new RaycastHit2D[8];
        private Health target;private bool pending,encounterEnabled=true,firstReset=true;
        private float nextAttempt;
        private float furthestProgress=float.NegativeInfinity;
        private bool activationFinished;
        private bool targetWasBound;
        private Vector2 Progression=>progressionDirection.sqrMagnitude>.001f?progressionDirection.normalized:Vector2.right;
        public EnemyArchetype Archetype=>archetype;
        public LarperBrain Spawned{get;private set;}
        public bool IsPending=>pending;
        public SpawnBlockReason BlockReason{get;private set;}
        public bool IsProgressionBlocked=>BlockReason==SpawnBlockReason.PassedProgress;
        public string ValidationError=>archetype==null?"Assign an enemy archetype.":archetype.ValidationError;
        public void BindTarget(Health value){target=value;targetWasBound|=value!=null;if(Spawned!=null)Spawned.GetComponent<EnemyCombat>()?.BindTarget(target);}
        public void BindGameplayCamera(Camera value)=>gameplayCamera=value;
        private void Start(){if(spawnOnStart)Spawn();}
        // Synchronous prewarming cannot render a frame. Activation is separate.
        public LarperBrain Prepare()
        {
            if(Spawned!=null)return Spawned;
            if(ValidationError!=null){Debug.LogWarning($"{name}: {ValidationError}",this);return null;}
            if(placedActor!=null)Spawned=placedActor;
            else
            {
                var actor=Instantiate(archetype.Prefab,transform.position,transform.rotation,spawnedParent);
                actor.SetActive(false);Spawned=actor.GetComponent<LarperBrain>();
            }
            Spawned.Configure(archetype);Spawned.GetComponent<EnemyCombat>()?.SetEncounterEnabled(encounterEnabled);Spawned.GetComponent<EnemyCombat>()?.BindTarget(target);
            if(personality!=null)Spawned.GetComponent<LarperAwareness>()?.Configure(personality);
            if(ladderRoute!=null)Spawned.GetComponent<AuthoredLadderRoute>()?.Configure(ladderRoute,routeTargetHeight);
            if(placedActor!=null&&Spawned.gameObject.activeSelf){activationFinished=true;pending=false;BlockReason=SpawnBlockReason.None;}
            return Spawned;
        }
        public LarperBrain Spawn(){if(Prepare()!=null)ActivateEncounter();return Spawned;}
        public void SetEncounterEnabled(bool value){encounterEnabled=value;if(Spawned!=null)Spawned.GetComponent<EnemyCombat>()?.SetEncounterEnabled(value);}
        public void ActivateEncounter()
        {
            SetEncounterEnabled(true);
            // A repeat zone contact may alert survivors, but never reveal a second
            // actor or reactivate a defeated spawn. Only an explicit reset can.
            if(activationFinished)return;
            pending=Spawned!=null&&!Spawned.gameObject.activeSelf&&!Spawned.GetComponent<Health>().IsDefeated;
        }
        private void TrackProgress()
        {
            if(target!=null)furthestProgress=Mathf.Max(furthestProgress,Vector2.Dot(target.transform.position,Progression));
        }
        private bool IsPassed(Bounds bounds)=>target!=null&&Vector2.Dot(bounds.center,Progression)<=furthestProgress;
        private bool Block(SpawnBlockReason reason){BlockReason=reason;return false;}
        private Bounds CaptureBounds()
        {
            var box=Spawned.GetComponent<BoxCollider2D>();
            return new Bounds(Spawned.transform.TransformPoint(box.offset),Vector3.Scale(new Vector3(box.size.x,box.size.y,.2f),Spawned.transform.lossyScale));
        }
        public bool CanActivate()
        {
            if(Spawned==null)return Block(SpawnBlockReason.Unprepared);
            if(activationFinished)return false;
            if(target==null&&targetWasBound||target!=null&&target.IsDefeated)return Block(SpawnBlockReason.PlayerUnavailable);
            var camera=gameplayCamera!=null?gameplayCamera:Camera.main;
            if(camera==null||!camera.isActiveAndEnabled)return Block(SpawnBlockReason.CameraUnavailable);
            TrackProgress();
            var bounds=CaptureBounds();var expanded=bounds;expanded.Expand(cameraSafetyMargin*2);
            // Offscreen is not a progression direction. Rear and already visited
            // locations are invalid even when hidden by a turn or a camera pan.
            Vector2 origin=target!=null?(Vector2)target.transform.position:(Vector2)camera.transform.position;
            float progress=target!=null?furthestProgress:Vector2.Dot(origin,Progression);
            float rearProgress=Vector2.Dot(bounds.center,Progression)-Mathf.Abs(Progression.x)*bounds.extents.x-Mathf.Abs(Progression.y)*bounds.extents.y;
            if(IsPassed(bounds))return Block(SpawnBlockReason.PassedProgress);
            if(rearProgress-progress<minimumPlayerDistance)return Block(SpawnBlockReason.PlayerProximity);
            float depth=camera.WorldToViewportPoint(bounds.center).z;
            if(depth<camera.nearClipPlane||depth>camera.farClipPlane)return Block(SpawnBlockReason.CameraDepth);
            // Collision-layer culling does not prove that the artwork is offscreen.
            if(GameplayViewport.Overlaps(camera,expanded,0,true))return Block(SpawnBlockReason.CameraVisible);
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(~0);
            int count=Physics2D.OverlapBox(bounds.center,(Vector2)bounds.size-Vector2.one*.06f,0,filter,overlaps);
            if(count==overlaps.Length)return Block(SpawnBlockReason.BodyBlocked);
            for(int i=0;i<count;i++)if(!overlaps[i].transform.IsChildOf(Spawned.transform))return Block(SpawnBlockReason.BodyBlocked);
            // Body clearance alone allowed almost touching actors. Check a wider,
            // allocation-free neighborhood without treating nearby scenery as an obstacle.
            count=Physics2D.OverlapCircle(Spawned.transform.position,minimumSpawnSpacing,filter,overlaps);
            if(count==overlaps.Length)return Block(SpawnBlockReason.Crowded);
            for(int i=0;i<count;i++)
            {
                var other=overlaps[i].GetComponentInParent<LarperBrain>();
                if(other!=null&&other!=Spawned&&Vector2.SqrMagnitude(other.transform.position-Spawned.transform.position)<minimumSpawnSpacing*minimumSpawnSpacing)return Block(SpawnBlockReason.Crowded);
            }
            filter.SetLayerMask(geometry);
            for(int i=-1;i<=1;i+=2)
            {
                Vector2 foot=new Vector2(bounds.center.x+i*bounds.extents.x*.8f,bounds.min.y+.04f);
                count=Physics2D.Raycast(foot,Vector2.down,filter,support,supportDistance);
                bool grounded=false;for(int j=0;j<count;j++)if(!support[j].transform.IsChildOf(Spawned.transform)&&support[j].normal.y>.7f){grounded=true;break;}
                if(!grounded||count==support.Length)return Block(SpawnBlockReason.Unsupported);
            }
            BlockReason=SpawnBlockReason.None;return true;
        }
        private void TryActivate()
        {
            if(!pending||Spawned==null||!CanActivate())return;
            // No yield between the final safety check and reveal.
            pending=false;activationFinished=true;BlockReason=SpawnBlockReason.None;Spawned.gameObject.SetActive(true);
        }
        // Run after the gameplay camera's LateUpdate, immediately before rendering.
        private void LateUpdate()
        {
            if(activationFinished)return;
            TrackProgress();
            // Keep rejected dynamic members pending and observable. Never silently
            // shrink the authored encounter or move a late enemy into the rear.
            if(pending&&Spawned!=null&&IsPassed(CaptureBounds())){BlockReason=SpawnBlockReason.PassedProgress;return;}
            if(pending&&Time.unscaledTime>=nextAttempt){nextAttempt=Time.unscaledTime+retrySeconds;TryActivate();}
        }
        public void ResetEncounter(bool active,bool completed=false,bool coveredReset=false)
        {
            var actor=Prepare();if(actor==null)return;SetEncounterEnabled(active);
            if(firstReset&&placedActor!=null&&actor.gameObject.activeSelf&&!completed){firstReset=false;return;}
            activationFinished=completed;BlockReason=completed?SpawnBlockReason.Finished:SpawnBlockReason.None;furthestProgress=float.NegativeInfinity;TrackProgress();
            firstReset=false;pending=false;var go=actor.gameObject;go.SetActive(false);
            var body=go.GetComponent<Rigidbody2D>();body.position=transform.position;body.linearVelocity=Vector2.zero;body.angularVelocity=0;body.rotation=0;go.transform.position=transform.position;
            go.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>()?.ResetTraversal();
            go.GetComponent<Health>().Restore();go.GetComponent<EnemyCombat>().ResetEncounter(target);
            actor.Configure(archetype);actor.AnchorPatrolHere();
            if(coveredReset&&placedActor!=null){activationFinished=true;go.SetActive(!completed);return;}
            pending=!completed;nextAttempt=Time.unscaledTime+retrySeconds;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color=ValidationError!=null?Color.red:pending?Color.magenta:Color.yellow;
            Gizmos.DrawWireCube(transform.position,new Vector3(.8f,1.4f,.1f));
            Gizmos.DrawWireSphere(transform.position,minimumSpawnSpacing);
            Gizmos.color=new Color(1,.7f,0,.4f);Gizmos.DrawWireCube(transform.position,new Vector3(.8f+cameraSafetyMargin*2,1.4f+cameraSafetyMargin*2,.1f));
        }
#if UNITY_EDITOR
        [ContextMenu("Log Spawn Diagnostics")]
        private void LogDiagnostics()
        {
            var encounter=GetComponentInParent<InsaneDreams.Missions.MissionEncounter>();
            int intended=encounter!=null?encounter.Count:1,instantiated=0,active=0;
            if(encounter!=null){foreach(var point in encounter.Points)if(point.Spawned!=null){instantiated++;if(point.Spawned.gameObject.activeSelf)active++;}}
            else if(Spawned!=null){instantiated=1;active=Spawned.gameObject.activeSelf?1:0;}
            Debug.Log($"Encounter={ (encounter!=null?encounter.name:transform.parent!=null?transform.parent.name:name) } point={name} intended={intended} instantiated={instantiated} active={active} pending={pending} finished={activationFinished} reason={BlockReason} position={transform.position} direction={Progression} actor={(Spawned!=null?Spawned.name:"missing/destroyed")}",this);
        }
#endif
    }
}
