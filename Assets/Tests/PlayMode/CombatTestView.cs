#if UNITY_EDITOR
using InsaneDreams.Enemies;
using UnityEngine;
namespace InsaneDreams.Tests
{
    // Legacy isolated fixtures live thousands of units from the warehouse camera.
    // Give them an actual framed camera; never disable the production visibility gate.
    internal static class CombatTestView
    {
        public static void Frame(EnemyCombat enemy)
        {
            var go=new GameObject("FixtureGameplayCamera");go.transform.SetParent(enemy.transform,false);go.transform.localPosition=Vector3.back*10;
            var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=35;camera.depth=-100;
            enemy.BindGameplayCamera(camera);
        }
    }
}
#endif
