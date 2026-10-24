using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class CampaignEntry
{
    private const string Menu="Assets/Scenes/MainMenu.unity";
    private const string DevelopmentPlay="Akumu.IntentionalDevelopmentPlay";
    static CampaignEntry()
    {
        // Initialize once per Editor launch, not on every Play Mode/domain reload.
        if(!SessionState.GetBool("Akumu.CampaignEntryInitialized",false))
        {SessionState.SetBool("Akumu.CampaignEntryInitialized",true);EditorApplication.delayCall+=UseMainMenuEntry;}
        EditorApplication.playModeStateChanged+=state=>
        {
            // Unity's test launcher creates its own initialization scene. A menu
            // override must never replace it. The installed test framework exposes
            // this active-job flag internally, so query it without a runtime dependency.
            if(state==PlayModeStateChange.ExitingEditMode&&TestRunActive())
            {SessionState.SetBool("Akumu.RestoreEntryAfterTests",true);EditorSceneManager.playModeStartScene=null;}
            if(state==PlayModeStateChange.EnteredEditMode&&(SessionState.GetBool(DevelopmentPlay,false)||SessionState.GetBool("Akumu.RestoreEntryAfterTests",false)))
            {SessionState.SetBool(DevelopmentPlay,false);SessionState.SetBool("Akumu.RestoreEntryAfterTests",false);UseMainMenuEntry();}
        };
    }
    private static bool TestRunActive()
    {
        foreach(var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var type=assembly.GetType("UnityEditor.TestTools.TestRunner.Api.TestRunnerApi");
            var method=type?.GetMethod("IsRunActive",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
            if(method!=null)return (bool)method.Invoke(null,null);
        }
        return false;
    }
    public static void UseMainMenuEntry()=>EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(Menu);
    [MenuItem("AKUMU 2/Play Open Development Scene")]
    public static void PlayDevelopmentScene()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        SessionState.SetBool(DevelopmentPlay,true);EditorSceneManager.playModeStartScene=null;EditorApplication.isPlaying=true;
    }
    [MenuItem("AKUMU 2/Play Campaign (Main Menu)")]
    public static void PlayCampaign()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
        UseMainMenuEntry();EditorSceneManager.OpenScene(Menu);
        EditorApplication.isPlaying=true;
    }
}
