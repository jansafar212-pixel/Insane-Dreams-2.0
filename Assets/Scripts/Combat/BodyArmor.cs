using UnityEngine;
namespace InsaneDreams.Combat
{
    public sealed class BodyArmor : MonoBehaviour
    {
        [SerializeField,Min(1)] private float capacity=60;
        [SerializeField,Range(0,1)] private float absorption=.65f;
        private Collider2D body;
        public float Condition{get;private set;}
        public bool Equipped{get;private set;}
        public event System.Action Changed;
        public float Capacity=>capacity;
        private void Awake()=>body=GetComponent<Collider2D>();
        private void OnEnable()=>Changed?.Invoke();
        private void OnDisable()=>Changed?.Invoke();
        public void Restore(float amount,float maximum=60,float ratio=.65f)
        {capacity=Mathf.Max(1,maximum);absorption=Mathf.Clamp01(ratio);Condition=float.IsNaN(amount)||float.IsInfinity(amount)?0:Mathf.Clamp(amount,0,capacity);Equipped=Condition>0;Changed?.Invoke();}
        public float Absorb(DamageInfo damage)
        {
            if(!isActiveAndEnabled||Condition<=0||damage.Team==CombatTeam.Neutral)return damage.Amount;
            if(damage.Impact.HasValue&&body!=null&&damage.Impact.Value.Point.y>body.bounds.max.y-body.bounds.size.y*.25f)return damage.Amount;
            float absorbed=Mathf.Min(Condition,damage.Amount*absorption);Condition-=absorbed;Changed?.Invoke();return damage.Amount-absorbed;
        }
    }
}
