#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace InsaneDreams.Combat
{
    // Developer-only, standalone Testbed. Never grants ownership or writes a save.
    public sealed class ArsenalTestBench : MonoBehaviour
    {
        private static bool requested;
        private VisualElement panel,root;private PlayerLoadout loadout;private RuntimeWeapon[] weapons;
        public static bool IsRequested=>requested;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset(){requested=false;SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
        private static void Loaded(Scene scene,LoadSceneMode mode)
        {if(scene.name=="MainMenu"||scene.name=="Warehouse_Testbed")new GameObject("DeveloperArsenalBench").AddComponent<ArsenalTestBench>();}
        private IEnumerator Start()
        {
            yield return null;yield return null;
            var scene=gameObject.scene;
            foreach(var doc in FindObjectsByType<UIDocument>(FindObjectsSortMode.None))
                if(doc.gameObject.scene==scene&&doc.rootVisualElement.Q(scene.name=="MainMenu"?"flowContent":"safeArea")!=null){root=doc.rootVisualElement;break;}
            if(root==null)yield break;
            if(scene.name=="MainMenu")
            {
                var entry=new Button(()=>StartCoroutine(Enter())){text="DEV / ARSENAL LAB",name="developerArsenalEntry"};entry.AddToClassList("flow-button");entry.style.position=Position.Absolute;entry.style.right=8;entry.style.bottom=8;root.Add(entry);yield break;
            }
            if(GameSession.Instance!=null)yield break; // Never test against a campaign profile.
            loadout=FindAnyObjectByType<PlayerLoadout>();if(loadout==null)yield break;
            var catalog=Resources.Load<ArsenalCatalog>("ArsenalCatalog");if(catalog==null)yield break;
            weapons=new RuntimeWeapon[catalog.weapons.Length];for(int i=0;i<weapons.Length;i++)weapons[i]=new RuntimeWeapon(catalog.weapons[i],catalog.weapons[i].DeveloperReserve);
            var toggle=new Button(()=>{loadout.GetComponent<Input.PlayerActions>().SuspendInput();panel.style.display=panel.style.display==DisplayStyle.Flex?DisplayStyle.None:DisplayStyle.Flex;}){text="DEV ARSENAL",name="developerArsenalToggle"};
            toggle.style.position=Position.Absolute;toggle.style.right=8;toggle.style.top=190;toggle.style.height=38;root.Add(toggle);
            panel=new ScrollView();panel.name="developerArsenalPanel";panel.style.position=Position.Absolute;panel.style.left=Length.Percent(30);panel.style.right=Length.Percent(30);panel.style.top=40;panel.style.bottom=40;panel.style.backgroundColor=new Color(.1f,.12f,.14f,.98f);
            panel.Add(new Label("DEVELOPER ONLY / NO CAMPAIGN SAVE"));
            panel.Add(new Label("SNIPER: HOLD AIM / RELEASE TO FIRE. CENTER TO CANCEL."));
            for(int i=0;i<weapons.Length;i++){int index=i;var button=new Button(()=>Equip(index)){text=weapons[i].Definition.DisplayName};button.style.height=44;panel.Add(button);}
            var refill=new Button(Refill){text="REFILL TEST AMMO"};refill.style.height=44;panel.Add(refill);
            var close=new Button(()=>panel.style.display=DisplayStyle.None){text="CLOSE / PLAY"};close.style.height=44;panel.Add(close);
            var exit=new Button(Exit){text="EXIT LAB / NORMAL MENU"};exit.style.height=44;panel.Add(exit);
            root.Add(panel);panel.style.display=requested?DisplayStyle.Flex:DisplayStyle.None;
        }
        private IEnumerator Enter()
        {
            if(requested)yield break;requested=true;
            // Menu purchases already save normally. Persist before leaving, destroy
            // the session, then load a scene whose bridge cannot touch that profile.
            var session=GameSession.Instance;if(session!=null){session.Save();Destroy(session.gameObject);}yield return null;
            SceneManager.LoadScene("Warehouse_Testbed");
        }
        public void Equip(int index)
        {
            if(GameSession.Instance!=null||weapons==null||index<0||index>=weapons.Length)return;
            loadout.GetComponent<Input.PlayerActions>().SuspendInput();var slot=index<2||index==7?EquipmentSlot.Firearm2:EquipmentSlot.Firearm1;
            loadout.Equip(slot,weapons[index]);loadout.Select(slot);panel.style.display=DisplayStyle.None;
        }
        public void Refill(){if(GameSession.Instance==null&&weapons!=null)foreach(var weapon in weapons){weapon.Cycle.CancelReload();weapon.SetAmmunition(weapon.Definition.MagazineCapacity,weapon.Definition.DeveloperReserve);}}
        public void Exit(){requested=false;if(loadout!=null)loadout.GetComponent<Input.PlayerActions>().SuspendInput();SceneManager.LoadScene("MainMenu");}
    }
}
#endif
