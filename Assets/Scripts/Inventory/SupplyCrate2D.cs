using System.Collections.Generic;
using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Inventory
{
    public sealed class SupplyCrate2D : MonoBehaviour
    {
        [SerializeField, Min(0)] private float ductTape=50;
        [SerializeField, Min(0)] private int ammunition=72;
        [SerializeField, Min(.5f)] private float interactionDistance=1.8f;
        [SerializeField] private LayerMask geometry=1;
        private float tapeLeft;private int ammoLeft;private bool initialized;
        private readonly HashSet<PlayerSupplies> visitors=new HashSet<PlayerSupplies>();
        private readonly RaycastHit2D[] hits=new RaycastHit2D[8];
        public float TapeLeft{get{Ensure();return tapeLeft;}}
        public int AmmoLeft{get{Ensure();return ammoLeft;}}
        public bool Depleted=>TapeLeft<=.001f&&AmmoLeft==0;
        private void Awake()=>Ensure();
        private void Ensure(){if(initialized)return;initialized=true;ResetContents();}
        public void ResetContents(){initialized=true;tapeLeft=Mathf.Max(0,ductTape);ammoLeft=Mathf.Max(0,ammunition);Refresh();}
        public bool CanUse(PlayerSupplies player)=>isActiveAndEnabled&&player!=null&&player.CanCollect&&!Depleted&&Vector2.SqrMagnitude(player.transform.position-transform.position)<=interactionDistance*interactionDistance;
        public bool TrySupply(PlayerSupplies player)
        {
            if(!CanUse(player)||CombatVisibility2D.IsBlocked(player.transform.position,transform.position,geometry,hits,player.transform,transform))return false;
            float tape=player.Healing.AddDuctTape(TapeLeft);int ammo=player.Loadout.AddReserveAmmunition(AmmoLeft);
            tapeLeft=Mathf.Max(0,tapeLeft-tape);ammoLeft-=ammo;Refresh();return tape>0||ammo>0;
        }
        private void Refresh()
        {
            var label=GetComponentInChildren<TextMesh>();if(label!=null)label.text=tapeLeft<=.001f&&ammoLeft==0?"EMPTY":"TAPE + AMMO";
            var sprite=GetComponentInChildren<SpriteRenderer>();if(sprite!=null)sprite.color=tapeLeft<=.001f&&ammoLeft==0?new Color(.45f,.45f,.45f):Color.white;
        }
        private void OnTriggerEnter2D(Collider2D other){var player=other.GetComponentInParent<PlayerSupplies>();if(player!=null){visitors.Add(player);player.Register(this);}}
        private void OnTriggerExit2D(Collider2D other){var player=other.GetComponentInParent<PlayerSupplies>();if(player!=null){visitors.Remove(player);player.Unregister(this);}}
        private void OnDisable(){foreach(var player in visitors)if(player!=null)player.Unregister(this);visitors.Clear();}
    }
}
