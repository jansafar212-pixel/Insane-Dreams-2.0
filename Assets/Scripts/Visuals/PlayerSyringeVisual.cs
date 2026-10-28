using InsaneDreams.Checkpoints;
using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    [DefaultExecutionOrder(150)]
    [RequireComponent(typeof(PlayerRevive),typeof(CharacterVisualDriver))]
    public sealed class PlayerSyringeVisual : MonoBehaviour
    {
        [SerializeField] private Sprite syringeSprite;
        public Sprite InventoryIcon=>syringeSprite;
        [SerializeField,Range(.3f,1)] private float seconds=.6f;
        private PlayerRevive revive;private Health health;private PlayerRecovery recovery;private CutoutRig rig;
        private SpriteRenderer syringe,rearArm;private int armOrder;private float started;private bool playing;
        public bool IsPlaying=>playing&&Time.time<started+seconds&&health!=null&&!health.IsDefeated;
        public int PlayCount{get;private set;}
        public SpriteRenderer Syringe=>syringe;
        private void Awake(){revive=GetComponent<PlayerRevive>();health=GetComponent<Health>();recovery=GetComponent<PlayerRecovery>();rig=GetComponent<CharacterVisualDriver>().Rig;}
        private void OnEnable(){revive.Revived+=Play;if(recovery!=null)recovery.Recovered+=Stop;}
        private void OnDisable(){revive.Revived-=Play;if(recovery!=null)recovery.Recovered-=Stop;Stop();}
        private void Play()
        {
            if(IsPlaying||syringeSprite==null||rig==null)return;
            if(syringe==null){var g=new GameObject("FaintanylSyringe");g.transform.SetParent(rig.transform,false);syringe=g.AddComponent<SpriteRenderer>();syringe.sprite=syringeSprite;syringe.sortingLayerID=rig.weapon.sortingLayerID;syringe.sortingOrder=19;rearArm=rig.leftArm.GetComponent<SpriteRenderer>();armOrder=rearArm.sortingOrder;}
            started=Time.time;playing=true;PlayCount++;
        }
        private void Stop(){playing=false;if(syringe!=null)syringe.enabled=false;if(rearArm!=null)rearArm.sortingOrder=armOrder;}
        private void LateUpdate()
        {
            if(!playing)return;if(!IsPlaying){Stop();return;}
            float t=Mathf.Clamp01((Time.time-started)/seconds),gesture=Mathf.Sin(t*Mathf.PI);
            rig.leftArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(-14,105,gesture));rearArm.sortingOrder=18;
            Transform hand=rig.leftArm;Vector3 tip=Vector3.down*rig.skin.arm.bounds.size.y;
            if(rig.Segmented!=null&&rig.Segmented.IsBound){rig.Segmented.leftElbow.localRotation=Quaternion.Euler(0,0,20*gesture);rig.Segmented.leftHand.localRotation=Quaternion.identity;hand=rig.Segmented.leftHand;tip=Vector3.down*rig.Segmented.hand.bounds.size.y*.55f;}
            syringe.enabled=true;syringe.transform.position=hand.TransformPoint(tip);syringe.transform.rotation=rig.transform.rotation;syringe.transform.localScale=Vector3.one;
        }
    }
}
