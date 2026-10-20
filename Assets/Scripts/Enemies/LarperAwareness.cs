using InsaneDreams.Combat;
using UnityEngine;
namespace InsaneDreams.Enemies
{
    [RequireComponent(typeof(EnemyCombat),typeof(LarperSpeechBubble))]
    public sealed class LarperAwareness : MonoBehaviour
    {
        [SerializeField] private LarperPersonalityProfile personality;
        [SerializeField] private LayerMask geometry=1;
        [SerializeField, Min(2)] private int missedAttacksForLine=4;
        private static event System.Action<LarperAwareness,Vector2,ulong> DefeatObserved;
        private static ulong defeatSequence;
        private static event System.Action<LarperAwareness,Vector2,ulong,int> CollapseObserved;
        private static float nextCollapse;
        private static int collapseRemaining;
        [SerializeField, Range(0,.25f)] private float collapseChance=.08f;
        [SerializeField, Range(0,1)] private float collapseSpreadChance=.35f;
        [Header("Witnessed ally defeat")]
        [SerializeField, Range(0,1)] private float socialFearChance=.60f;
        [SerializeField, Range(0,1)] private float socialConfidenceChance=.15f;
        [SerializeField, Range(.1f,8)] private float socialInfluenceSeconds=5;
        [SerializeField, Range(0,180)] private float socialViewHalfAngle=100;
        private ulong lastDefeatEvent;
        private ulong lastCollapseEvent;
        private bool defeatReported;
        private BoxCollider2D actorBox;
        private InsaneDreams.Visuals.CharacterVisualDriver visual;
        private static EncounterSpeechGate speechGate=new EncounterSpeechGate();
        private Health health;
        private EnemyCombat combat;
        private AttackExecutor attacks;
        private LarperSpeechBubble bubble;
        private float nextMorale;
        private int misses;
        private float nextCallout, combatPriorityUntil;
        private int reloadLine, withdrawalLine, blockedLine;
        private int malfunctionLine;
        private void OnMalfunction(){if(isActiveAndEnabled&&Random.value<.4f)CombatLine(LarperSpeechEvent.Malfunction,ref malfunctionLine);}
        private string pendingCombatLine;
        private LarperSpeechEvent pendingCombatContext;
        private float pendingCombatUntil,nextPendingCheck;
        private readonly RaycastHit2D[] sightHits=new RaycastHit2D[8];
        public int PanicReactions { get; private set; }
        public int SocialReactions { get; private set; }
        public int CollapseReactions { get; private set; }
        public LarperPersonalityProfile Personality=>personality;
        public event System.Action<LarperSpeechEvent> Reaction;
        public event System.Action<LarperSpeechEvent> LineDelivered;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics(){DefeatObserved=null;CollapseObserved=null;nextCollapse=0;collapseRemaining=0;ResetEncounterSpeech();}
        public static void ResetEncounterSpeech()=>speechGate=new EncounterSpeechGate();
        public void Configure(LarperPersonalityProfile value)=>personality=value;
        private void Awake(){health=GetComponent<Health>();combat=GetComponent<EnemyCombat>();attacks=GetComponent<AttackExecutor>();bubble=GetComponent<LarperSpeechBubble>();actorBox=GetComponent<BoxCollider2D>();visual=GetComponent<InsaneDreams.Visuals.CharacterVisualDriver>();}
        private void OnEnable(){defeatReported=false;combat.WeaponMalfunctioned+=OnMalfunction;CollapseObserved+=WitnessCollapse;CombatNoise.Reported+=Hear;health.Defeated+=Defeated;combat.VisibilityChanged+=Visibility;attacks.Fired+=Fired;DefeatObserved+=Witness;combat.ReloadStarted+=Reload;combat.WithdrawalStarted+=Withdraw;combat.WithdrawalFailed+=Blocked;}
        private void OnDisable(){pendingCombatLine=null;combat.WeaponMalfunctioned-=OnMalfunction;CollapseObserved-=WitnessCollapse;CombatNoise.Reported-=Hear;health.Defeated-=Defeated;combat.VisibilityChanged-=Visibility;attacks.Fired-=Fired;DefeatObserved-=Witness;combat.ReloadStarted-=Reload;combat.WithdrawalStarted-=Withdraw;combat.WithdrawalFailed-=Blocked;misses=0;}
        private void Hear(CombatNoiseEvent value){if(!health.IsDefeated)combat.Hear(value);}
        private void Reload()=>CombatLine(LarperSpeechEvent.Reload,ref reloadLine);
        private void Withdraw()=>CombatLine(LarperSpeechEvent.Withdraw,ref withdrawalLine);
        private void Blocked()=>CombatLine(LarperSpeechEvent.Blocked,ref blockedLine);
        private void CombatLine(LarperSpeechEvent context,ref int cursor)
        {
            Reaction?.Invoke(context);
            if(personality==null||health.IsDefeated||!bubble.IsRelevant(transform.position))return;
            var lines=personality.Lines(context);if(lines==null||lines.Length==0)return;
            string line=lines[cursor++%lines.Length];
            if(TryCombatLine(line,context))return;
            if(pendingCombatLine==null){pendingCombatLine=line;pendingCombatContext=context;pendingCombatUntil=Time.time+6;}
        }
        private bool TryCombatLine(string line,LarperSpeechEvent context)
        {
            if(Time.time<nextCallout||bubble.IsVisible||LarperSpeechBubble.VisibleCount>=2||!speechGate.TryCombat(Time.time,line))return false;
            nextCallout=Time.time+3.5f;combatPriorityUntil=Time.time+4;
            if(!bubble.TryShow(line))return false;
            LineDelivered?.Invoke(context);return true;
        }
        private void Update()
        {
            if(pendingCombatLine==null||Time.time<nextPendingCheck)return;nextPendingCheck=Time.time+.2f;
            if(health.IsDefeated||Time.time>=pendingCombatUntil||!bubble.IsRelevant(transform.position)){pendingCombatLine=null;return;}
            if(TryCombatLine(pendingCombatLine,pendingCombatContext))pendingCombatLine=null;
        }
        private void Visibility(bool visible){var context=visible?LarperSpeechEvent.Detected:LarperSpeechEvent.Lost;Reaction?.Invoke(context);Speak(context,false);}
        private void Fired(Vector2 direction,Vector2 end,bool hit)
        { misses=hit?0:misses+1;if(misses>=missedAttacksForLine){misses=0;Reaction?.Invoke(LarperSpeechEvent.MissStreak);Speak(LarperSpeechEvent.MissStreak,false);} }
        private void Defeated(DamageInfo _)
        {
            if(health.Team!=CombatTeam.Enemy||defeatReported)return;
            defeatReported=true;DefeatObserved?.Invoke(this,(Vector2)transform.position+Vector2.up*.35f,++defeatSequence);
        }
        private void Witness(LarperAwareness source,Vector2 point,ulong eventId)
        {
            // Global event IDs are monotonic. One watermark deduplicates all sources
            // without allocating a set; reactions never publish another defeat.
            if(source==null||source==this||!source.health.IsDefeated||eventId<=lastDefeatEvent)return;
            lastDefeatEvent=eventId;
            if(personality==null||health.IsDefeated||Time.time<nextMorale||!combat.CanReactToAllyDefeat)return;
            Vector2 origin=actorBox!=null?new Vector2(actorBox.bounds.center.x,actorBox.bounds.max.y-.15f):(Vector2)transform.position;
            Vector2 delta=point-origin;float distance=delta.magnitude;
            float facing=combat.TargetVisible?Mathf.Sign(combat.AimDirection.x):visual!=null?visual.Facing:Mathf.Sign(combat.AimDirection.x);
            if(distance>personality.WitnessRange||distance>.01f&&delta.x*facing/distance<Mathf.Cos(socialViewHalfAngle*Mathf.Deg2Rad)||
                CombatVisibility2D.IsBlocked(origin,point,geometry,sightHits,transform,source.transform))return;
            // Even an unaffected roll consumes this decision window. Other defeats
            // cannot continually reroll or accumulate/refresh an existing influence.
            nextMorale=Time.time+Mathf.Max(socialInfluenceSeconds,personality.MoraleCooldown);
            float fear=Mathf.Clamp01(Mathf.Max(personality.PanicChance,socialFearChance+(personality.Disposition==LarperDisposition.Cowardly?.1f:personality.Disposition==LarperDisposition.Overconfident?-.1f:0)));
            float confidence=Mathf.Clamp(socialConfidenceChance+(personality.Disposition==LarperDisposition.Overconfident?.1f:personality.Disposition==LarperDisposition.Cowardly?-.05f:0),0,1-fear);
            float sample=Random.value;
            if(sample>=fear+confidence){Reaction?.Invoke(LarperSpeechEvent.WitnessDefeat);Speak(LarperSpeechEvent.WitnessDefeat,false);return;}
            bool fearful=sample<fear;
            if(!combat.ApplyAllyMorale(fearful,socialInfluenceSeconds))return;
            SocialReactions++;
            bool panic=fearful&&sample<personality.PanicChance&&combat.InGameplayViewport&&combat.PanicFor(personality.PanicSeconds);
            if(panic)PanicReactions++;
            else if(fearful)combat.ExpressAllyFear();
            var context=panic?LarperSpeechEvent.Panic:LarperSpeechEvent.WitnessDefeat;
            Reaction?.Invoke(context);Speak(context,panic);
            if(panic&&Time.time>=nextCollapse&&Random.value<collapseChance)
            {
                nextCollapse=Time.time+20;collapseRemaining=3;ulong chain=++defeatSequence;lastCollapseEvent=chain;
                CollapseObserved?.Invoke(this,(Vector2)transform.position+Vector2.up*.35f,chain,2);
            }
        }
        private void WitnessCollapse(LarperAwareness source,Vector2 point,ulong chain,int hops)
        {
            if(source==this||chain<=lastCollapseEvent)return;lastCollapseEvent=chain;
            if(hops<=0||collapseRemaining<=0||personality==null||Time.time<nextMorale||!combat.CanReactToAllyDefeat)return;
            Vector2 origin=actorBox.bounds.center;
            if(Vector2.SqrMagnitude(point-origin)>Mathf.Pow(Mathf.Min(6,personality.WitnessRange),2)||
                CombatVisibility2D.IsBlocked(origin,point,geometry,sightHits,transform,source.transform)||Random.value>=collapseSpreadChance)return;
            nextMorale=Time.time+Mathf.Max(socialInfluenceSeconds,personality.MoraleCooldown);
            if(!combat.ApplyAllyMorale(true,Mathf.Min(5,socialInfluenceSeconds))||!combat.PanicFor(Random.Range(.7f,1.4f)))return;
            collapseRemaining--;CollapseReactions++;PanicReactions++;SocialReactions++;
            Reaction?.Invoke(LarperSpeechEvent.Panic);Speak(LarperSpeechEvent.Panic,true);
            if(hops>1)CollapseObserved?.Invoke(this,(Vector2)transform.position+Vector2.up*.35f,chain,hops-1);
        }
        private void Speak(LarperSpeechEvent context,bool priority)
        {
            if(personality==null||health.IsDefeated||bubble.IsVisible||LarperSpeechBubble.VisibleCount>=2||!priority&&Time.time<combatPriorityUntil||!bubble.IsRelevant(transform.position))return;
            var lines=personality.Lines(context);if(lines==null||lines.Length==0||!speechGate.TryReserve(Time.time,personality.SpeechCooldown,priority,personality.PanicSpeechGap,personality.PanicSpeechCooldown))return;
            if(bubble.TryShow(lines[Random.Range(0,lines.Length)],context==LarperSpeechEvent.Panic))LineDelivered?.Invoke(context);
        }
    }
}
