using UnityEngine;
namespace InsaneDreams.Combat
{
    public static class CombatVisibility2D
    {
        public static bool IsBlocked(Vector2 origin, Vector2 end, LayerMask geometry, RaycastHit2D[] buffer,Transform ignore=null,Transform ignoreTarget=null)
        {
            Vector2 delta=end-origin;if(delta.sqrMagnitude<.0001f)return false;
            var filter=new ContactFilter2D{useTriggers=false};filter.SetLayerMask(geometry);
            int count=Physics2D.Raycast(origin,delta.normalized,filter,buffer,delta.magnitude);
            for(int i=0;i<count;i++)if((ignore==null||!buffer[i].transform.IsChildOf(ignore))&&(ignoreTarget==null||!buffer[i].transform.IsChildOf(ignoreTarget)))return true;
            return count==buffer.Length;
        }
    }
}
