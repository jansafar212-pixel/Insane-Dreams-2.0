using UnityEngine;
namespace InsaneDreams.CameraSystem
{
    public static class GameplayViewport
    {
        // Actual body bounds in one gameplay camera; Scene view visibility is irrelevant.
        public static bool Overlaps(Camera camera, Bounds body, int layer, bool ignoreCullingMask=false)
        {
            if(camera==null||!camera.isActiveAndEnabled||!ignoreCullingMask&&(camera.cullingMask&(1<<layer))==0)return false;
            float minX=float.PositiveInfinity,minY=minX,maxX=float.NegativeInfinity,maxY=maxX;bool inDepth=false;
            for(int i=0;i<8;i++)
            {
                Vector3 p=camera.WorldToViewportPoint(body.center+Vector3.Scale(body.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));
                if(p.z<camera.nearClipPlane||p.z>camera.farClipPlane)continue;
                inDepth=true;minX=Mathf.Min(minX,p.x);maxX=Mathf.Max(maxX,p.x);minY=Mathf.Min(minY,p.y);maxY=Mathf.Max(maxY,p.y);
            }
            return inDepth&&maxX>0&&minX<1&&maxY>0&&minY<1;
        }
    }
}
