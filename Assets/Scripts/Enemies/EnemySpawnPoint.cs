using InsaneDreams.Combat;
using InsaneDreams.CameraSystem;
using UnityEngine;
namespace InsaneDreams.Enemies
{
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
        [SerializeField, Min(.05f)] private float retrySeconds=.2f;
        [SerializeField] private LayerMask geometry=1;
        [SerializeField, Min(.05f)] private float supportDistance=.6f;
        private readonly Collider2D[] overlaps=new Collider2D[32];
        private readonly RaycastHit2D[] support=new RaycastHit2D[8];
        private Health target;private bool pending,encounterEnabled=true,firstReset=true;
        private float nextAttempt;
        public EnemyArchetype Archetype=>archetype;
        public LarperBrain Spawned{get;private set;}
        public bool IsPending=>pending;
        public string ValidationError=>archetype==null?"Assign an enemy archetype.":archetype.ValidationError;
        public void BindTarget(Health value){target=value;if(Spawned!=null)Spawned.GetComponent<EnemyCombat>()?.BindTarget(encounterEnabled?target:null);}
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
            Spawned.Configure(archetype);Spawned.GetComponent<EnemyCombat>()?.SetEncounterEnabled(encounterEnabled);Spawned.GetComponent<EnemyCombat>()?.BindTarget(encounterEnabled?target:null);
            if(personality!=null)Spawned.GetComponent<LarperAwareness>()?.Configure(personality);
            if(ladderRoute!=null)Spawned.GetComponent<AuthoredLadderRoute>()?.Configure(ladderRoute,routeTargetHeight);
            return Spawned;
        }
        public LarperBrain Spawn(){if(Prepare()!=null)ActivateEncounter();return Spawned;}
        public void SetEncounterEnabled(bool value){encounterEnabled=value;if(Spawned!=null){Spawned.GetComponent<EnemyCombat>()?.SetEncounterEnabled(value);Spawned.GetComponent<EnemyCombat>()?.BindTarget(value?target:null);}}
        public void ActivateEncounter(){SetEncounterEnabled(true);pending=Spawned!=null&&!Spawned.gameObject.activeSelf;}
        private Bounds CaptureBounds()
        {
            var box=Spawned.GetComponent<BoxCollider2D>();
            return new Bounds(Spawned.transform.TransformPoint(box.offset),Vector3.Scale(new Vector3(box.size.x,box.size.y,.2f),Spawned.transform.lossyScale));
        }
        public bool CanActivate()
        {
            if(Spawned==null)return false;var camera=gameplayCamera!=null?gameplayCamera:Camera.main;
            if(camera==null||!camera.isActiveAndEnabled)return false;
            var bounds=CaptureBounds();var expanded=bounds;expanded.Expand(cameraSafetyMargin*2);
            // Collision-layer culling does not prove that the artwork is offscreen.
            if(GameplayViewport.Overlaps(camera,expanded,0,true))return false;
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(~0);
            int count=Physics2D.OverlapBox(bounds.center,(Vector2)bounds.size-Vector2.one*.06f,0,filter,overlaps);
            if(count==overlaps.Length)return false;
            for(int i=0;i<count;i++)if(!overlaps[i].transform.IsChildOf(Spawned.transform))return false;
            filter.SetLayerMask(geometry);
            for(int i=-1;i<=1;i+=2)
            {
                Vector2 foot=new Vector2(bounds.center.x+i*bounds.extents.x*.8f,bounds.min.y+.04f);
                count=Physics2D.Raycast(foot,Vector2.down,filter,support,supportDistance);
                bool grounded=false;for(int j=0;j<count;j++)if(!support[j].transform.IsChildOf(Spawned.transform)&&support[j].normal.y>.7f){grounded=true;break;}
                if(!grounded||count==support.Length)return false;
            }
            return true;
        }
        private void TryActivate()
        {
            if(!pending||Spawned==null||!CanActivate())return;
            // No yield between the final safety check and reveal.
            pending=false;Spawned.gameObject.SetActive(true);
        }
        // Run after the gameplay camera's LateUpdate, immediately before rendering.
        private void LateUpdate(){if(pending&&Time.unscaledTime>=nextAttempt){nextAttempt=Time.unscaledTime+retrySeconds;TryActivate();}}
        public void ResetEncounter(bool active,bool completed=false,bool coveredReset=false)
        {
            var actor=Prepare();if(actor==null)return;SetEncounterEnabled(active);
            if(firstReset&&placedActor!=null&&actor.gameObject.activeSelf&&!completed){firstReset=false;return;}
            firstReset=false;pending=false;var go=actor.gameObject;go.SetActive(false);
            var body=go.GetComponent<Rigidbody2D>();body.position=transform.position;body.linearVelocity=Vector2.zero;body.angularVelocity=0;body.rotation=0;go.transform.position=transform.position;
            go.GetComponent<InsaneDreams.Traversal.PlayerTraversal2D>()?.ResetTraversal();
            go.GetComponent<Health>().Restore();go.GetComponent<EnemyCombat>().ResetEncounter(active?target:null);
            actor.Configure(archetype);actor.AnchorPatrolHere();
            if(coveredReset&&placedActor!=null){go.SetActive(!completed);return;}
            pending=!completed;nextAttempt=Time.unscaledTime+retrySeconds;
        }
        private void OnDrawGizmos()
        {
            Gizmos.color=ValidationError!=null?Color.red:pending?Color.magenta:Color.yellow;
            Gizmos.DrawWireCube(transform.position,new Vector3(.8f,1.4f,.1f));
            Gizmos.color=new Color(1,.7f,0,.4f);Gizmos.DrawWireCube(transform.position,new Vector3(.8f+cameraSafetyMargin*2,1.4f+cameraSafetyMargin*2,.1f));
        }
    }
}
