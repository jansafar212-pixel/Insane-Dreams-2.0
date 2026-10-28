using System;
using UnityEngine;
namespace InsaneDreams.Combat
{
    public enum CombatNoiseKind { Movement, Gunfire, Explosion }
    public readonly struct CombatNoiseEvent
    {
        public readonly GameObject Source;
        public readonly CombatTeam Team;
        public readonly CombatNoiseKind Kind;
        public readonly Vector2 Position,End;
        public readonly float Intensity;
        public readonly Health HitTarget;
        public CombatNoiseEvent(GameObject source,CombatTeam team,CombatNoiseKind kind,Vector2 position,Vector2 end,float intensity,Health hitTarget=null)
        {Source=source;Team=team;Kind=kind;Position=position;End=end;Intensity=intensity;HitTarget=hitTarget;}
    }
    // Logical sound is independent of optional AudioClips and visual tracer lifetimes.
    public static class CombatNoise
    {
        public static event Action<CombatNoiseEvent> Reported;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset()=>Reported=null;
        public static void Report(CombatNoiseEvent value)=>Reported?.Invoke(value);
    }
}
