using InsaneDreams.Missions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace InsaneDreams.Progression
{
    public sealed class CampaignMissionBridge : MonoBehaviour
    {
        [SerializeField] private WarehouseMission mission;
        [SerializeField] private UIDocument document;
        [SerializeField] private StyleSheet style;
        private bool applied,paused,actionsEnabled,combatEnabled;
        private float previousScale;
        private VisualElement overlay;
        private Combat.PlayerRevive revive;
        private GameSession Session=>GameSession.Instance;
        private void Awake()
        {
            mission.BeforeReplay+=BeforeReplay;mission.PreparingRun+=Prepare;mission.Changed+=Changed;
            revive=mission.Player.GetComponent<Combat.PlayerRevive>();
            if(revive!=null)revive.InventoryChanged+=SaveInjections;
        }
        private void SaveInjections(int count)
        {if(applied&&Session!=null&&Session.Data.faintanyl!=count){Session.Data.faintanyl=count;Session.Save();}}
        private void Start()
        {
            if(Session==null)return;
            var root=document.rootVisualElement;root.styleSheets.Add(style);
            var back=new Button(ReturnToLevels){text="LEVEL SELECTION"};back.AddToClassList("flow-button");root.Q("resultsExtras").Add(back);
            var pause=new Button(()=>SetPaused(true)){name="campaignPause",tooltip="Pause"};var pauseFace=new VisualElement{name="pauseVisual",pickingMode=PickingMode.Ignore};pauseFace.AddToClassList("pause-face");var icon=new VisualElement{name="pauseIcon",tooltip="\u23F8",pickingMode=PickingMode.Ignore};icon.AddToClassList("pause-icon");for(int i=0;i<2;i++){var bar=new VisualElement{pickingMode=PickingMode.Ignore};bar.AddToClassList("pause-bar");icon.Add(bar);}pauseFace.Add(icon);pause.Add(pauseFace);pause.AddToClassList("flow-button");pause.AddToClassList("campaign-pause");var safe=root.Q("safeArea");safe.AddToClassList("top-pause");safe.Add(pause);
            overlay=new VisualElement();overlay.AddToClassList("campaign-overlay");var panel=new VisualElement();panel.AddToClassList("campaign-panel");overlay.Add(panel);
            var title=new Label("TAKE A BREATH. THE HERO IS SELF-EMPLOYED.");title.AddToClassList("flow-title");panel.Add(title);
            Add(panel,"RESUME",()=>SetPaused(false));Add(panel,"RETRY MISSION",()=>{SetPaused(false);mission.Replay();});Add(panel,"RETURN TO LEVELS",ReturnToLevels);
            root.Add(overlay);overlay.style.display=DisplayStyle.None;
        }
        private static void Add(VisualElement root,string text,System.Action action){var b=new Button(action){text=text};b.AddToClassList("flow-button");root.Add(b);}
        private void Update()
        {
            if(Session!=null&&Keyboard.current!=null&&Application.isFocused&&Keyboard.current.escapeKey.wasPressedThisFrame)SetPaused(!paused);
        }
        public void SetPaused(bool value)
        {
            if(value==paused||mission.Stage==MissionStage.Complete&&value)return;
            paused=value;var player=mission.Player;var combat=player.GetComponent<Combat.PlayerCombat>();
            if(value){previousScale=Time.timeScale;actionsEnabled=player.enabled;combatEnabled=combat.enabled;player.SuspendInput();player.enabled=false;combat.enabled=false;Time.timeScale=0;}
            else{Time.timeScale=previousScale;player.enabled=actionsEnabled;combat.enabled=combatEnabled;}
            if(overlay!=null){overlay.style.display=value?DisplayStyle.Flex:DisplayStyle.None;if(value)overlay.BringToFront();}
        }
        private void BeforeReplay()
        {
            if(Session==null||!applied)return;Settle();applied=false;
        }
        private void Prepare()
        {
            if(Session==null)return;if(string.IsNullOrEmpty(Session.Data.activeRun))Session.BeginAttempt();applied=true;
            ApplyEquipment();
        }
        public void ApplyEquipment()
        {
            if(Session==null)return;
            var data=Session.Data;var catalog=Session.Catalog;var player=mission.Player;
            var loadout=player.GetComponent<Inventory.PlayerLoadout>();
            string[] ids={data.primary,data.secondary,data.melee};
            for(int i=0;i<3;i++)
            {
                var offer=catalog.Find(ids[i]);var slot=(Inventory.EquipmentSlot)i;
                if(offer?.weapon==null||!data.owned.Contains(ids[i])){loadout.Unequip(slot);continue;}
                var stock=data.Ammo(ids[i]);var weapon=new Inventory.RuntimeWeapon(offer.weapon);
                weapon.SetAmmunition(stock.magazine,Mathf.Min(catalog.ammoCapacity,stock.reserve));loadout.Equip(slot,weapon);
            }
            loadout.Select(loadout.Get(Inventory.EquipmentSlot.Firearm1)!=null?Inventory.EquipmentSlot.Firearm1:loadout.Get(Inventory.EquipmentSlot.Firearm2)!=null?Inventory.EquipmentSlot.Firearm2:Inventory.EquipmentSlot.Melee);
            player.GetComponent<Combat.PlayerHealing>().RestoreSupply(data.tape,catalog.tapeCapacity);
            player.GetComponent<Combat.PlayerRevive>()?.RestoreInventory(data.faintanyl);
            player.GetComponent<Combat.BodyArmor>()?.Restore(data.armor,catalog.armorCapacity,catalog.armorAbsorption);
            document.GetComponent<Input.WarehouseTouchControls>()?.SetJoystickScale(data.joystickScale);
        }
        private void CaptureEquipment()
        {
            var data=Session.Data;var loadout=mission.Player.GetComponent<Inventory.PlayerLoadout>();
            for(int i=0;i<3;i++)
            {
                var weapon=loadout.Get((Inventory.EquipmentSlot)i);if(weapon==null)continue;
                var offer=System.Array.Find(Session.Catalog.offers,x=>x.category==ShopCategory.Weapons&&x.weapon==weapon.Definition);
                if(offer==null)continue;var ammo=data.Ammo(offer.id);ammo.magazine=weapon.MagazineAmmo;ammo.reserve=weapon.ReserveAmmo;
            }
            data.tape=mission.Player.GetComponent<Combat.PlayerHealing>().DuctTape;
            var revive=mission.Player.GetComponent<Combat.PlayerRevive>();if(revive!=null)data.faintanyl=revive.Injections;
            var armor=mission.Player.GetComponent<Combat.BodyArmor>();data.armor=armor!=null?armor.Condition:0;
            data.pendingEarnings=mission.Earnings;
        }
        private void Changed()
        {
            if(applied&&mission.Stage==MissionStage.Complete)
            {mission.Player.GetComponent<Combat.HitReaction2D>()?.CancelReaction(false);Settle();}
        }
        private void Settle()
        {
            if(Session==null||string.IsNullOrEmpty(Session.Data.activeRun))return;
            CaptureEquipment();
            Session.Settle(mission.Earnings,mission.Stage==MissionStage.Complete);
        }
        public void ReturnToLevels()
        {
            if(Session==null)return;SetPaused(false);Settle();mission.Player.ResetActions();Session.Navigate(FlowScreen.Levels);SceneManager.LoadSceneAsync("MainMenu");
        }
        private void OnDestroy()
        {
            if(revive!=null)revive.InventoryChanged-=SaveInjections;
            if(paused)SetPaused(false);
            if(mission!=null){mission.BeforeReplay-=BeforeReplay;mission.PreparingRun-=Prepare;mission.Changed-=Changed;}
        }
        private void OnApplicationPause(bool paused){if(paused&&applied&&Session!=null){CaptureEquipment();Session.Save();}}
        private void OnApplicationQuit(){if(applied&&Session!=null){CaptureEquipment();Session.Save();}}
    }
}
