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
        private static event System.Action<LarperAwareness,Vector2> DefeatObserved;
        private static EncounterSpeechGate speechGate=new EncounterSpeechGate();
        private Health health;
        private EnemyCombat combat;
        private AttackExecutor attacks;
        private LarperSpeechBubble bubble;
        private float nextMorale;
        private int misses;
        private float nextCallout, combatPriorityUntil;
        private int reloadLine, withdrawalLine, blockedLine;
        private readonly RaycastHit2D[] sightHits=new RaycastHit2D[8];
        public int PanicReactions { get; private set; }
        public LarperPersonalityProfile Personality=>personality;
        public event System.Action<LarperSpeechEvent> Reaction;
        public event System.Action<LarperSpeechEvent> LineDelivered;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetStatics(){DefeatObserved=null;ResetEncounterSpeech();}
        public static void ResetEncounterSpeech()=>speechGate=new EncounterSpeechGate();
        public void Configure(LarperPersonalityProfile value)=>personality=value;
        private void Awake(){health=GetComponent<Health>();combat=GetComponent<EnemyCombat>();attacks=GetComponent<AttackExecutor>();bubble=GetComponent<LarperSpeechBubble>();}
        private void OnEnable(){health.Defeated+=Defeated;combat.VisibilityChanged+=Visibility;attacks.Fired+=Fired;DefeatObserved+=Witness;combat.ReloadStarted+=Reload;combat.WithdrawalStarted+=Withdraw;combat.WithdrawalFailed+=Blocked;}
        private void OnDisable(){health.Defeated-=Defeated;combat.VisibilityChanged-=Visibility;attacks.Fired-=Fired;DefeatObserved-=Witness;combat.ReloadStarted-=Reload;combat.WithdrawalStarted-=Withdraw;combat.WithdrawalFailed-=Blocked;misses=0;}
        private void Reload()=>CombatLine(LarperSpeechEvent.Reload,ref reloadLine);
        private void Withdraw()=>CombatLine(LarperSpeechEvent.Withdraw,ref withdrawalLine);
        private void Blocked()=>CombatLine(LarperSpeechEvent.Blocked,ref blockedLine);
        private void CombatLine(LarperSpeechEvent context,ref int cursor)
        {
            Reaction?.Invoke(context);
            if(personality==null||health.IsDefeated||Time.time<nextCallout||!bubble.IsRelevant(transform.position)||LarperSpeechBubble.VisibleCount>=2&&!bubble.IsVisible)return;
            var lines=personality.Lines(context);if(lines==null||lines.Length==0)return;
            string line=lines[cursor++%lines.Length];
            if(!speechGate.TryCombat(Time.time,line))return;
            nextCallout=Time.time+.65f;combatPriorityUntil=Time.time+2.4f;
            bubble.Show(line);LineDelivered?.Invoke(context);
        }
        private void Visibility(bool visible){var context=visible?LarperSpeechEvent.Detected:LarperSpeechEvent.Lost;Reaction?.Invoke(context);Speak(context,false);}
        private void Fired(Vector2 direction,Vector2 end,bool hit)
        { misses=hit?0:misses+1;if(misses>=missedAttacksForLine){misses=0;Reaction?.Invoke(LarperSpeechEvent.MissStreak);Speak(LarperSpeechEvent.MissStreak,false);} }
        private void Defeated(DamageInfo _){if(health.Team==CombatTeam.Enemy)DefeatObserved?.Invoke(this,transform.position);}
        private void Witness(LarperAwareness source,Vector2 point)
        {
            if(source==this||personality==null||health.IsDefeated||!bubble.IsRelevant(transform.position)||
                Vector2.Distance(transform.position,point)>personality.WitnessRange||CombatVisibility2D.IsBlocked(transform.position,point,geometry,sightHits))return;
            if(Time.time>=nextMorale && LarperMoralePolicy.ShouldPanic(personality.PanicChance,Random.value) && combat.PanicFor(personality.PanicSeconds))
            { nextMorale=Time.time+personality.MoraleCooldown;PanicReactions++;Reaction?.Invoke(LarperSpeechEvent.Panic);Speak(LarperSpeechEvent.Panic,true); }
            else {Reaction?.Invoke(LarperSpeechEvent.WitnessDefeat);Speak(LarperSpeechEvent.WitnessDefeat,false);}
        }
        private void Speak(LarperSpeechEvent context,bool priority)
        {
            if(personality==null||health.IsDefeated||!priority&&Time.time<combatPriorityUntil||!bubble.IsRelevant(transform.position))return;
            var lines=personality.Lines(context);if(lines==null||lines.Length==0||!speechGate.TryReserve(Time.time,personality.SpeechCooldown,priority,personality.PanicSpeechGap,personality.PanicSpeechCooldown))return;
            bubble.Show(lines[Random.Range(0,lines.Length)],context==LarperSpeechEvent.Panic);
            LineDelivered?.Invoke(context);
        }
    }
}
