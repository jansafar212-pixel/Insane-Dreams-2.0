using UnityEngine;

namespace InsaneDreams.CameraSystem
{
    // Android ignores QualitySettings.vSyncCount. Request 60 explicitly rather
    // than the platform default; no physics, input or desktop timing changes.
    public static class AndroidFramePacing
    {
        public const int TargetFramesPerSecond=60;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure()
        {
            if(Application.platform==RuntimePlatform.Android)
                Application.targetFrameRate=TargetFramesPerSecond;
        }
    }
}
