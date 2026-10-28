using InsaneDreams.Enemies;
using UnityEngine;
namespace InsaneDreams.Combat
{
    // Reusable bounded queries for thrown explosives and authored hazards.
    public sealed class AreaCombat2D
    {
        readonly Collider2D[] overlaps=new Collider2D[128];readonly Health[] affected=new Health[128];readonly Visuals.CartoonRagdoll[] corpses=new Visuals.CartoonRagdoll[128];readonly RaycastHit2D[] hits=new RaycastHit2D[8];
        public int Damage(GameObject source,CombatTeam team,Vector2 center,float radius,float maximum,float impulse)
        {
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(~0);int count=Physics2D.OverlapCircle(center,radius,filter,overlaps),used=0,corpseCount=0;if(count==overlaps.Length)return 0;
            for(int i=0;i<count;i++)
            {
                if(SamuraiBodyHitbox.IsMovementShape(overlaps[i]))continue;var hp=overlaps[i].GetComponentInParent<Health>();
                if(hp==null)
                {
                    var corpse=overlaps[i].GetComponentInParent<Visuals.CartoonRagdoll>();if(corpse==null)continue;bool seen=false;for(int j=0;j<corpseCount;j++)if(corpses[j]==corpse)seen=true;if(seen)continue;
                    Vector2 at=overlaps[i].bounds.center,axis=at-center;float force=1-axis.magnitude/radius;if(force<=0||CombatVisibility2D.IsBlocked(center,at,1,hits,null,corpse.transform,false))continue;
                    var hit=new PhysicalImpact(at,axis.sqrMagnitude>.001f?axis.normalized:Vector2.up,impulse*force,overlaps[i].attachedRigidbody);corpse.ReceiveImpact(hit);corpse.ReceiveRegionDamage(maximum*force,hit);corpses[corpseCount++]=corpse;continue;
                }
                if(hp.IsDefeated)continue;
                bool duplicate=false;for(int j=0;j<used;j++)if(affected[j]==hp)duplicate=true;if(duplicate)continue;
                Vector2 point=overlaps[i].bounds.center,delta=point-center;float weight=1-delta.magnitude/radius;if(weight<=0||CombatVisibility2D.IsBlocked(center,point,1,hits,source!=null?source.transform:null,hp.transform,false))continue;
                var impact=new PhysicalImpact(point,delta.sqrMagnitude>.001f?delta.normalized:Vector2.up,impulse*weight,hp.GetComponent<Rigidbody2D>());
                if(hp.ApplyDamage(new DamageInfo(maximum*weight,team,hp.gameObject==source?null:source,impact,true,DamageKind.Explosion)))
                {affected[used++]=hp;if(!hp.IsDefeated&&impact.Body!=null&&impact.Body.bodyType==RigidbodyType2D.Dynamic)impact.Body.AddForce(impact.Direction*Mathf.Min(2,impulse*weight),ForceMode2D.Impulse);}
            }return used;
        }
        public int Flash(GameObject source,Vector2 center,float radius,float seconds)
        {
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(~0);int count=Physics2D.OverlapCircle(center,radius,filter,overlaps),used=0;if(count==overlaps.Length)return 0;
            for(int i=0;i<count;i++)
            {
                var hp=overlaps[i].GetComponentInParent<Health>();if(hp==null||hp.IsDefeated||hp.gameObject==source)continue;bool duplicate=false;for(int j=0;j<used;j++)if(affected[j]==hp)duplicate=true;if(duplicate)continue;
                var ai=hp.GetComponent<EnemyCombat>();if(ai==null||CombatVisibility2D.IsBlocked(center,hp.transform.position,1,hits,source!=null?source.transform:null,hp.transform))continue;
                ai.DisruptSenses(seconds);affected[used++]=hp;
            }return used;
        }
    }
}
