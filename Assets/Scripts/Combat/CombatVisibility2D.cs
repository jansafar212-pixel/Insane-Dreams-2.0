using UnityEngine;
namespace InsaneDreams.Combat
{
    public static class CombatVisibility2D
    {
        public static bool IsBlocked(Vector2 origin, Vector2 end, LayerMask geometry, RaycastHit2D[] buffer)
        {
            Vector2 delta=end-origin;if(delta.sqrMagnitude<.0001f)return false;
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(geometry);
            return Physics2D.Raycast(origin,delta.normalized,filter,buffer,delta.magnitude)>0;
        }
    }
}
