using InsaneDreams.Enemies;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyCombat)), CanEditMultipleObjects]
public sealed class EnemyCombatDebugInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(!Application.isPlaying||targets.Length!=1)return;
        var c=(EnemyCombat)target;var brain=c.GetComponent<LarperBrain>();
        EditorGUILayout.Space();EditorGUILayout.LabelField("Live AI diagnostics",EditorStyles.boldLabel);
        using(new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.TextField("Awareness / action",c.Awareness+" / "+c.State);
            EditorGUILayout.Toggle("Visual contact",c.TargetVisible);
            EditorGUILayout.Toggle("Gameplay viewport",c.InGameplayViewport);
            EditorGUILayout.Toggle("Camera permits attack",c.MayInitiateFire);
            EditorGUILayout.FloatField("Recognition remaining",c.RecognitionRemaining);
            EditorGUILayout.Vector2Field("Last seen (not live target)",c.LastKnownPosition);
            EditorGUILayout.Vector2Field("Approximate noise",c.HeardPosition);
            EditorGUILayout.FloatField("Noise memory remaining",c.HeardMemoryRemaining);
            EditorGUILayout.IntField("Accepted noise events",c.NoiseEventsHeard);
            EditorGUILayout.FloatField("Suppression",c.Suppression);
            if(c.HasCombatPosition)EditorGUILayout.Vector2Field("Reserved destination",c.CombatPosition);
            if(brain!=null){EditorGUILayout.Toggle("Stuck recovery pause",brain.IsRecovering);EditorGUILayout.IntField("Stuck recoveries",brain.StuckRecoveries);}
            var samurai=c.GetComponent<SamuraiCombat>();
            if(samurai!=null){EditorGUILayout.TextField("Platform transition",samurai.Transition.ToString());EditorGUILayout.TextField("Last route rejection",samurai.FailedTransitionReason);EditorGUILayout.Vector2Field("Committed landing",samurai.PlannedLanding);}
        }
    }
    public override bool RequiresConstantRepaint()=>Application.isPlaying;
}
