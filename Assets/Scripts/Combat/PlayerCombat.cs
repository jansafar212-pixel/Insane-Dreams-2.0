using InsaneDreams.Input;
using InsaneDreams.Inventory;
using UnityEngine;
namespace InsaneDreams.Combat
{
    [RequireComponent(typeof(PlayerActions),typeof(PlayerLoadout),typeof(AttackExecutor)),DefaultExecutionOrder(125)]
    public sealed class PlayerCombat : MonoBehaviour
    {
        private PlayerActions actions;private PlayerLoadout loadout;private AttackExecutor executor;
        private RuntimeWeapon previous,armedWeapon;private PlayerCover2D cover;private Health health;private PlayerHealing healing;
        private Visuals.PlayerSyringeVisual syringe;private bool armed,queued,releaseBlocked,wasFiring,blockFiring;
        private Vector2 releasedAim,laserEnd;private float nextLaserSample;private LineRenderer laser;
        public bool SniperAiming=>armed;public LineRenderer AimLaser=>laser;
        public event System.Action ReloadStarted;
        private void Awake(){actions=GetComponent<PlayerActions>();loadout=GetComponent<PlayerLoadout>();executor=GetComponent<AttackExecutor>();cover=GetComponent<PlayerCover2D>();syringe=GetComponent<Visuals.PlayerSyringeVisual>();health=GetComponent<Health>();healing=GetComponent<PlayerHealing>();}
        private void OnEnable(){actions.ReloadRequested+=Reload;actions.InputReset+=Cancel;actions.AimReleased+=ReleaseAim;loadout.SelectionChanging+=ChangingWeapon;health.Defeated+=Defeated;}
        private void OnDisable(){actions.ReloadRequested-=Reload;actions.InputReset-=Cancel;actions.AimReleased-=ReleaseAim;loadout.SelectionChanging-=ChangingWeapon;health.Defeated-=Defeated;Cancel();}
        private void ChangingWeapon(){bool sniper=loadout.SelectedWeapon!=null&&loadout.SelectedWeapon.Definition.FireMode==WeaponFireMode.ReleaseToFire;Cancel();if(sniper)actions.CancelWeaponAim();}
        private void Defeated(DamageInfo damage)=>Cancel();
        public void Reload(){CancelAim();if(CanUse&&loadout.SelectedWeapon!=null&&loadout.SelectedWeapon.Cycle.TryReload(Time.time))ReloadStarted?.Invoke();}
        public void Cancel(){blockFiring|=previous!=null&&previous.Definition.FireMode==WeaponFireMode.ReleaseToFire;previous?.Cycle.CancelReload();loadout.SelectedWeapon?.Cycle.CancelReload();CancelAim();}
        private void CancelAim(){armed=queued=false;armedWeapon=null;releaseBlocked=true;if(laser!=null)laser.enabled=false;}
        private bool CanOperate=>isActiveAndEnabled&&actions.isActiveAndEnabled&&!health.IsDefeated&&Time.timeScale>0&&(syringe==null||!syringe.IsPlaying);
        private bool CanUse=>CanOperate&&(healing==null||!healing.IsHealing);
        private bool ClearToShoot=>cover==null||!cover.IsCovered||!actions.HasAimIntent||cover.PeekReady;
        private void ReleaseAim(Vector2 direction,bool valid){queued=valid&&armed&&armedWeapon==loadout.SelectedWeapon&&CanUse&&ClearToShoot;releasedAim=direction;if(queued)actions.PreserveReleasedAim(direction);armed=false;if(laser!=null)laser.enabled=false;}
        private void LateUpdate()
        {
            var current=loadout.SelectedWeapon;bool pressed=actions.FireHeld&&!wasFiring;wasFiring=actions.FireHeld;
            if(!actions.FireHeld)blockFiring=false;
            if(current!=previous){previous?.Cycle.CancelReload();CancelAim();previous=current;}
            if(!actions.HasAimIntent)releaseBlocked=false;
            if(current==null||!CanUse){CancelAim();return;}
            current.Cycle.Tick(Time.time);
            if(current.Definition.FireMode==WeaponFireMode.ReleaseToFire)
            {
                if(queued){queued=false;if(armedWeapon==current&&ClearToShoot)Shoot(current,releasedAim);}
                bool mayAim=actions.HasAimIntent&&actions.AimIntentStrength>=.3f&&!releaseBlocked&&!current.Cycle.IsReloading&&!current.Cycle.IsRecovering(Time.time)&&ClearToShoot;
                if(mayAim){armed=true;armedWeapon=current;ShowLaser(current,actions.AimDirection);}else{armed=false;if(laser!=null)laser.enabled=false;}
                return;
            }
            if(laser!=null)laser.enabled=false;
            if(blockFiring||!actions.FireHeld||!ClearToShoot||current.Definition.FireMode==WeaponFireMode.SemiAutomatic&&!pressed)return;
            Shoot(current,actions.AimDirection);
        }
        private void Shoot(RuntimeWeapon current,Vector2 aim)
        {
            if(cover!=null&&cover.IsBlindFiring&&!current.Cycle.IsReloading&&!current.Cycle.IsMalfunctioning(Time.time)&&current.MagazineAmmo>0&&Time.time>=current.Cycle.NextAttackAt)
                aim=Quaternion.Euler(0,0,Random.Range(-cover.BlindSpread(0),cover.BlindSpread(0)))*aim;
            executor.Attack(current,aim,Time.time);
        }
        private void ShowLaser(RuntimeWeapon current,Vector2 aim)
        {
            if(laser==null){laser=new GameObject("PlayerSniperAim").AddComponent<LineRenderer>();laser.transform.SetParent(transform,false);laser.sharedMaterial=executor.TracerMaterial;laser.useWorldSpace=true;laser.positionCount=2;laser.startWidth=laser.endWidth=.025f;laser.sortingOrder=28;laser.startColor=new Color(.1f,1,1,.85f);laser.endColor=new Color(.1f,1,1,.35f);}
            if(Time.time>=nextLaserSample){nextLaserSample=Time.time+.1f;laserEnd=executor.PreviewEnd(aim,current.Definition.AttackRange);}
            float distance=Vector2.Distance(executor.Origin,laserEnd);laser.enabled=true;
            laser.SetPosition(0,(Vector3)executor.Origin+Vector3.back*.1f);laser.SetPosition(1,(Vector3)(executor.Origin+aim*distance)+Vector3.back*.1f);
        }
    }
}
