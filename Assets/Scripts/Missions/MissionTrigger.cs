using InsaneDreams.Input;
using UnityEngine;
namespace InsaneDreams.Missions
{
    public enum MissionZone { Entrance, Storage, Checkpoint, Processing, Records, Exit, OfficeCheckpoint }
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class MissionTrigger : MonoBehaviour
    {
        [SerializeField] private WarehouseMission mission;
        [SerializeField] private MissionZone zone;
        public MissionZone Zone=>zone;
        public WarehouseMission Mission=>mission;
        public Collider2D Region=>GetComponent<BoxCollider2D>();
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other.attachedRigidbody!=null&&other.attachedRigidbody.GetComponent<PlayerActions>()==mission.Player)
                mission.EnterZone(zone,Region);
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            // Contact callbacks can precede a measurable overlap by one physics step.
            // Also allow an eligible stage change while already inside a zone.
            OnTriggerEnter2D(other);
        }
    }
}
