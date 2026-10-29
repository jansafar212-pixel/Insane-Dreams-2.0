using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    public sealed class PlayerHealingPresentation : MonoBehaviour
    {
        [SerializeField] private Sprite tapeSprite;
        [SerializeField] private AudioClip peelingSound;
        private PlayerHealing healing;private SpriteRenderer band;private AudioSource sound;private float nextPeel;
        public bool IsWrapping=>band!=null&&band.enabled;
        public int Peels{get;private set;}
        private void Awake()
        {
            healing=GetComponent<PlayerHealing>();var rig=GetComponent<CharacterVisualDriver>().Rig;if(rig==null)return;var g=new GameObject("DuctTapeWrap");g.transform.SetParent(rig.torso,false);band=g.AddComponent<SpriteRenderer>();band.sprite=tapeSprite;band.sortingOrder=12;band.enabled=false;g=new GameObject("TapePeelAudio");g.transform.SetParent(transform,false);sound=g.AddComponent<AudioSource>();sound.playOnAwake=false;sound.spatialBlend=0;
        }
        private void OnEnable()=>healing.Healed+=Healed;
        private void OnDisable(){healing.Healed-=Healed;if(band!=null)band.enabled=false;}
        private void Healed(float amount){if(sound!=null&&peelingSound!=null&&Time.time>=nextPeel){nextPeel=Time.time+.45f;sound.PlayOneShot(peelingSound,.22f);Peels++;}}
        public void Pose(CutoutRig rig)
        {
            if(band==null)return;band.enabled=healing.IsHealing;if(!healing.IsHealing)return;float phase=Time.time*9;band.transform.localPosition=new Vector3(Mathf.Sin(phase)*.04f,.04f,-.03f);
            if(rig.Segmented!=null&&rig.Segmented.IsBound)rig.Segmented.PoseWrapping(phase);
            else{
                // The production protagonist uses the original single-piece arms.
                // Animate those existing shoulder pivots inward across the tape.
                rig.rightArm.localRotation=Quaternion.Euler(0,0,-42+Mathf.Sin(phase)*12);
                rig.leftArm.localRotation=Quaternion.Euler(0,0,45+Mathf.Cos(phase)*10);
            }
            rig.weapon.enabled=false;if(rig.magazine!=null)rig.magazine.enabled=false;rig.HideDefaultReplacementMagazine();
        }
    }
}
