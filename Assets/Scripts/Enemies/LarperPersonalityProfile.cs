using UnityEngine;
namespace InsaneDreams.Enemies
{
    public enum LarperDisposition { Ordinary, Overconfident, Cowardly }
    public enum LarperSpeechEvent { Detected, Lost, MissStreak, WitnessDefeat, Panic, Reload, Withdraw, Blocked }
    [CreateAssetMenu(menuName="Insane Dreams/Larper Personality")]
    public sealed class LarperPersonalityProfile : ScriptableObject
    {
        [SerializeField] private LarperDisposition disposition;
        [SerializeField, Range(0,1)] private float panicChance=.08f;
        [SerializeField, Min(1)] private float witnessRange=9;
        [SerializeField, Min(.1f)] private float panicSeconds=2.5f;
        [SerializeField, Min(1)] private float moraleCooldown=45;
        [SerializeField, Min(20)] private float speechCooldown=30;
        [SerializeField, Min(1)] private float panicSpeechGap=5;
        [SerializeField, Min(20)] private float panicSpeechCooldown=60;
        [SerializeField] private string[] detected={"I HAVE THE HIGH PING."};
        [SerializeField] private string[] lost={"TACTICAL BUFFERING."};
        [SerializeField] private string[] misses={"YOUR HITBOX IS CHEATING."};
        [SerializeField] private string[] witnessed={"HE WAS LAGGING."};
        [SerializeField] private string[] panic={"MOM COME PICK ME UP IM SCARED!"};
        [SerializeField] private string[] reload={"RELOADING!", "MY MAG'S EMPTY!", "WHY IS THIS THING SO LONG?!", "NOBODY SHOOT ME, I'M BUSY!"};
        [SerializeField] private string[] withdraw={"FALL BACK!", "COVER ME!", "THIS POSITION SUCKS!"};
        [SerializeField] private string[] blocked={"NO ROOM! RELOADING HERE!", "BAD PLAN! HOLD ON!"};
        public LarperDisposition Disposition=>disposition;
        public float PanicChance=>panicChance;
        public float WitnessRange=>witnessRange;
        public float PanicSeconds=>panicSeconds;
        public float MoraleCooldown=>moraleCooldown;
        public float SpeechCooldown=>speechCooldown;
        public float PanicSpeechGap=>panicSpeechGap;
        public float PanicSpeechCooldown=>panicSpeechCooldown;
        public string[] Lines(LarperSpeechEvent context) => context switch
        { LarperSpeechEvent.Detected=>detected, LarperSpeechEvent.Lost=>lost, LarperSpeechEvent.MissStreak=>misses, LarperSpeechEvent.WitnessDefeat=>witnessed, LarperSpeechEvent.Reload=>reload, LarperSpeechEvent.Withdraw=>withdraw, LarperSpeechEvent.Blocked=>blocked, _=>panic };
    }
    // Explicit time and random sample make rate limiting/morale independently testable.
    public sealed class EncounterSpeechGate
    {
        private float nextOrdinary=float.NegativeInfinity,lastSpeech=float.NegativeInfinity,nextPanic=float.NegativeInfinity;
        private float lastCombat=float.NegativeInfinity;
        private string lastCombatLine;
        public bool TryCombat(float now, string line)
        {
            if (float.IsNaN(now) || float.IsInfinity(now) || now-lastSpeech < .7f || line==lastCombatLine && now-lastCombat < 4) return false;
            lastCombat=lastSpeech=now; lastCombatLine=line; nextOrdinary=Mathf.Max(nextOrdinary,now+5); return true;
        }
        public bool TryReserve(float now,float interval,bool priority,float priorityGap=5,float priorityCooldown=60)
        {
            if(float.IsNaN(now)||float.IsInfinity(now))return false;
            if(priority ? now-lastSpeech<priorityGap||now<nextPanic : now<nextOrdinary)return false;
            lastSpeech=now;nextOrdinary=now+Mathf.Max(20,interval);
            if(priority)nextPanic=now+priorityCooldown;return true;
        }
    }
    public static class LarperMoralePolicy
    {
        public static bool ShouldPanic(float probability,float sample)=>probability>0 && sample>=0 && sample<Mathf.Clamp01(probability);
    }
}
