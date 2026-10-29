using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    // Presentation observes existing equipment; it never grants protection.
    public sealed class ArmorVisual : MonoBehaviour
    {
        public Sprite intact, damaged, broken;
        public SpriteRenderer overlay;
        public Sprite lightHelmet,heavyHelmet;
        public SpriteRenderer helmetOverlay;
        private BodyArmor armor;
        private void Awake(){armor=GetComponent<BodyArmor>();}
        private void OnEnable(){if(armor!=null)armor.Changed+=Refresh;Refresh();}
        private void OnDisable(){if(armor!=null)armor.Changed-=Refresh;if(overlay!=null){overlay.enabled=false;overlay.sprite=null;}if(helmetOverlay!=null){helmetOverlay.enabled=false;helmetOverlay.sprite=null;}}
        public void Refresh()
        {
            if(helmetOverlay!=null){helmetOverlay.enabled=isActiveAndEnabled&&armor!=null&&armor.isActiveAndEnabled&&armor.HelmetCondition>0;helmetOverlay.sprite=helmetOverlay.enabled?(armor.HelmetIsHeavy?heavyHelmet:lightHelmet):null;}
            if(overlay==null)return;
            overlay.enabled=isActiveAndEnabled&&armor!=null&&armor.isActiveAndEnabled&&armor.Equipped&&armor.Condition>0;
            overlay.sprite=!overlay.enabled?null:armor.Condition/armor.Capacity<=.5f?damaged:intact;
        }
        public void CopyTo(Transform torso)
        {
            Refresh();var corpse=torso.GetComponentInParent<CartoonRagdoll>();if(helmetOverlay!=null&&helmetOverlay.enabled&&corpse!=null&&corpse.Bodies.Length>1){var h=new GameObject("ArmorHelmet");h.transform.SetParent(corpse.Bodies[1].transform,false);h.transform.localPosition=helmetOverlay.transform.localPosition;var hr=h.AddComponent<SpriteRenderer>();hr.sprite=helmetOverlay.sprite;hr.color=helmetOverlay.color;hr.sortingOrder=helmetOverlay.sortingOrder;hr.sortingLayerID=helmetOverlay.sortingLayerID;hr.sharedMaterial=helmetOverlay.sharedMaterial;hr.flipX=corpse.Bodies[1].GetComponent<SpriteRenderer>().flipX;}
            if(overlay==null||!overlay.enabled)return;
            var go=new GameObject("ArmorVest");go.transform.SetParent(torso,false);
            go.transform.localPosition=overlay.transform.localPosition;go.transform.localRotation=overlay.transform.localRotation;go.transform.localScale=overlay.transform.localScale;
            var r=go.AddComponent<SpriteRenderer>();r.sprite=overlay.sprite;r.color=overlay.color;
            r.sortingLayerID=overlay.sortingLayerID;r.sortingOrder=overlay.sortingOrder;r.sharedMaterial=overlay.sharedMaterial;
            r.flipX=torso.GetComponent<SpriteRenderer>().flipX;
        }
    }
}
