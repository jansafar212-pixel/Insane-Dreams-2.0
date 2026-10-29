using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using UnityEngine;

namespace InsaneDreams.Visuals
{
    // Officer-only pose accents. The existing rig and weapon cycle remain owners.
    public sealed class OfficerWeaponPresentation : MonoBehaviour
    {
        [SerializeField,Range(0,55)] private float recoilDegrees=32;
        [SerializeField,Min(.1f)] private float recoverySeconds=.5f;
        [SerializeField,Range(0,.2f)] private float recoilSetback=.09f;
        [SerializeField] private float muzzleHeight=5f/32;
        private AttackExecutor attacks;
        private OfficerShotTelegraph telegraph;
        private float firedAt=float.NegativeInfinity;
        public float RecoilAmount
        {
            get {float p=(Time.time-firedAt)/recoverySeconds;return p<0||p>=1?0:p<.12f?Mathf.Sin(p/.12f*Mathf.PI*.5f):(1-p)/.88f;}
        }
        private void Awake(){attacks=GetComponent<AttackExecutor>();telegraph=GetComponent<OfficerShotTelegraph>();}
        public Vector2 DisplayAim(Vector2 fallback)=>telegraph!=null&&telegraph.Winding?telegraph.Direction:fallback;
        private void OnEnable(){if(attacks==null)attacks=GetComponent<AttackExecutor>();attacks.Fired+=Fired;}
        private void OnDisable(){attacks.Fired-=Fired;firedAt=float.NegativeInfinity;}
        private void Fired(Vector2 direction,Vector2 end,bool hit){if(attacks.LastAttackUsesAmmunition)firedAt=Time.time;}
        public void Pose(CutoutRig rig,bool reloading,CharacterPose pose)
        {
            // PistolMagazineReload owns the visible exchange; do not render the
            // base rig's generic replacement on top of its replacement magazine.
            rig.HideDefaultReplacementMagazine();
            if(!rig.weapon.enabled||pose==CharacterPose.Defeated)return;
            if(!reloading&&pose!=CharacterPose.Climb&&rig.Segmented!=null)
                rig.Segmented.PosePistolRecoil(recoilDegrees*RecoilAmount,recoilSetback*RecoilAmount);
            // This pistol's heavy slide sits five pixels above its hand pivot.
            // Sprite flip mirrors pixels, so its muzzle offset must mirror too.
            rig.muzzle.localPosition=new Vector3(rig.weapon.sprite.bounds.max.x,(rig.weapon.flipY?-1:1)*muzzleHeight,0);
        }
    }
}
