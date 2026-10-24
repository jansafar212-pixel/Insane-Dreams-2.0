using System.Collections.Generic;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    // Small registry owned by the existing combat components, not a scene manager.
    internal static class CombatPositionReservations
    {
        private static readonly HashSet<EnemyCombat> actors=new HashSet<EnemyCombat>();
        private static readonly Dictionary<EnemyCombat,Vector2> positions=new Dictionary<EnemyCombat,Vector2>(32);
        private static readonly Dictionary<EnemyCombat,Collider2D> covers=new Dictionary<EnemyCombat,Collider2D>(32);
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset(){actors.Clear();positions.Clear();covers.Clear();}
        public static void Register(EnemyCombat actor)=>actors.Add(actor);
        public static void Remove(EnemyCombat actor){Release(actor);actors.Remove(actor);}
        public static void Release(EnemyCombat actor){positions.Remove(actor);covers.Remove(actor);}
        // One useful slot per small crate. Opposite faces are not independent slots
        // when the whole obstacle is narrower than the actors' comfortable spacing.
        public static bool IsCoverFree(EnemyCombat actor,Collider2D cover)
        {
            foreach(var pair in covers)
                if(pair.Key!=actor&&pair.Key!=null&&pair.Key.PositionActive&&pair.Value==cover)return false;
            return true;
        }
        public static bool TryClaimCover(EnemyCombat actor,Collider2D cover,Vector2 point)
        {
            if(cover==null||!cover.isActiveAndEnabled||!IsCoverFree(actor,cover)||!TryClaim(actor,point))return false;
            covers[actor]=cover;return true;
        }
        public static bool TryGet(EnemyCombat actor,out Vector2 point)=>positions.TryGetValue(actor,out point);
        public static bool IsFree(EnemyCombat actor,Vector2 point)
        {
            foreach(var other in actors)
            {
                if(other==null||other==actor||!other.PositionActive)continue;
                float spacing=Mathf.Max(actor.MinimumCombatSpacing,other.MinimumCombatSpacing);
                if(Mathf.Abs(other.SpacingFloorY-actor.SpacingFloorY)<1.6f&&Mathf.Abs(other.transform.position.x-point.x)<spacing)return false;
                if(Vector2.SqrMagnitude((Vector2)other.transform.position-point)<spacing*spacing)return false;
                if(positions.TryGetValue(other,out var occupied)&&Vector2.SqrMagnitude(occupied-point)<spacing*spacing)return false;
            }
            return true;
        }
        public static bool IsPathFree(EnemyCombat actor,Vector2 point)
        {
            Vector2 start=actor.transform.position;float travel=point.x-start.x;
            foreach(var other in actors)
            {
                if(other==null||other==actor||!other.PositionActive||Mathf.Abs(other.SpacingFloorY-actor.SpacingFloorY)>1.6f||Mathf.Abs(other.transform.position.y-start.y)>1.6f)continue;
                float ahead=(other.transform.position.x-start.x)*Mathf.Sign(travel);
                if(ahead>.05f&&ahead<Mathf.Abs(travel))return false;
            }
            return true;
        }
        public static bool CanAdvance(EnemyCombat actor,float direction,bool blocked)
        {
            bool actorSeen=false;
            foreach(var other in actors)
            {
                if(other==actor){actorSeen=true;continue;}
                if(other==null||!other.PositionActive||Mathf.Abs(other.SpacingFloorY-actor.SpacingFloorY)>1.6f||Mathf.Abs(other.transform.position.y-actor.transform.position.y)>1.6f)continue;
                float ahead=(other.transform.position.x-actor.transform.position.x)*direction;
                if(Mathf.Abs(ahead)<.05f&&!actorSeen&&other.State!=EngagementState.Reloading)return false;
                float spacing=Mathf.Max(actor.MinimumCombatSpacing,other.MinimumCombatSpacing)+(blocked?.5f:.2f);
                if(ahead>0&&ahead<spacing)return false;
            }
            return true;
        }
        public static bool TryClaim(EnemyCombat actor,Vector2 point){if(!IsFree(actor,point)||!IsPathFree(actor,point))return false;positions[actor]=point;return true;}
    }
}
