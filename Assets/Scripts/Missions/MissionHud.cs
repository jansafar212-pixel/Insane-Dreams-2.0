using UnityEngine;
using UnityEngine.UIElements;
namespace InsaneDreams.Missions
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class MissionHud : MonoBehaviour
    {
        private WarehouseMission mission;
        private VisualElement safe,results;
        private Label objective,notification,prompt,time,resultStats;
        private Button interact,replay;
        private float nextReadout;
        private VisualElement resetCover;
        private float coveredUntil;
        private int coveredFrame;
        public bool CoverReset()
        {
            if(resetCover==null)return false;coveredUntil=Time.unscaledTime+.18f;coveredFrame=Time.frameCount+2;
            resetCover.style.display=DisplayStyle.Flex;resetCover.BringToFront();return true;
        }
        public void Bind(WarehouseMission value)
        {
            if(mission!=null)mission.Changed-=Refresh;mission=value;if(mission!=null)mission.Changed+=Refresh;Refresh();
        }
        private void OnEnable()
        {
            var root=GetComponent<UIDocument>().rootVisualElement;safe=root.Q("safeArea");results=root.Q("missionResults");
            resetCover=new VisualElement{name="missionResetCover"};resetCover.style.position=Position.Absolute;
            resetCover.style.left=resetCover.style.right=resetCover.style.top=resetCover.style.bottom=0;
            resetCover.style.backgroundColor=Color.black;resetCover.style.display=DisplayStyle.None;root.Add(resetCover);
            objective=root.Q<Label>("objective");notification=root.Q<Label>("missionNotice");prompt=root.Q<Label>("interactionPrompt");time=root.Q<Label>("missionTime");resultStats=root.Q<Label>("resultStats");
            interact=root.Q<Button>("interact");replay=root.Q<Button>("replayMission");if(interact==null||replay==null)return;interact.clicked+=Interact;replay.clicked+=Replay;
            if(mission!=null)mission.Changed+=Refresh;Refresh();
        }
        private void OnDisable(){resetCover?.RemoveFromHierarchy();resetCover=null;if(mission!=null)mission.Changed-=Refresh;if(interact!=null)interact.clicked-=Interact;if(replay!=null)replay.clicked-=Replay;}
        private void Interact()=>mission?.Player.RequestInteraction();
        private void Replay()=>mission?.Replay();
        public static string Clock(float seconds){int total=Mathf.Max(0,Mathf.FloorToInt(seconds));return $"{total/60:00}:{total%60:00}";}
        private void Refresh()
        {
            if(mission==null||safe==null||results==null||objective==null||resultStats==null)return;bool complete=mission.Stage==MissionStage.Complete;
            safe.EnableInClassList("mission-complete",complete);results.EnableInClassList("hidden",!complete);objective.text=mission.Objective;
            resultStats.text=$"TIME   {Clock(mission.Elapsed)}\nENEMIES DEFEATED   {mission.TotalDefeats}\nDEATHS   {mission.Deaths}"+(complete?$"\nMISSION EARNINGS   ${mission.Earnings}":"");
            Readout();
        }
        private void Update(){if(resetCover!=null&&Time.frameCount>=coveredFrame&&Time.unscaledTime>=coveredUntil)resetCover.style.display=DisplayStyle.None;if(mission!=null&&Time.unscaledTime>=nextReadout){nextReadout=Time.unscaledTime+.15f;Readout();}}
        private void Readout()
        {
            if(mission==null||safe==null||notification==null||time==null||prompt==null||interact==null)return;notification.text=mission.Notification;notification.EnableInClassList("hidden",notification.text.Length==0);time.text=Clock(mission.Elapsed);
            bool available=mission.CanInteract;prompt.text=available?"CORPORATE RECORDS — E / INTERACT":"";interact.EnableInClassList("hidden",!available);
            prompt.EnableInClassList("hidden",!available);
        }
    }
}
