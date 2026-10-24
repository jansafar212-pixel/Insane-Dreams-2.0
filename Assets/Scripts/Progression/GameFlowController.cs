using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
namespace InsaneDreams.Progression
{
    [RequireComponent(typeof(UIDocument))]
    public sealed class GameFlowController : MonoBehaviour
    {
        [SerializeField] private GameCatalog catalog;
        private GameSession session;
        private VisualElement content;
        private ShopCategory category;
        private string selected;
        private bool loading;
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private void Start()
        {
            session=GameSession.Instance;
            if(session==null){session=new GameObject("CampaignSession").AddComponent<GameSession>();session.Initialize(catalog,System.IO.Path.Combine(Application.persistentDataPath,"akumu2_progress_v1.json"));}
            content=GetComponent<UIDocument>().rootVisualElement.Q("flowContent");Draw();
        }
        private void Text(VisualElement parent,string value,string style="flow-copy"){var label=new Label(value);label.AddToClassList(style);parent.Add(label);}
        private Button Action(VisualElement parent,string text,System.Action callback,bool enabled=true)
        {var button=new Button(callback){text=text};button.AddToClassList("flow-button");button.SetEnabled(enabled&&!loading);parent.Add(button);return button;}
        private void Go(FlowScreen screen){session.Navigate(screen);Draw();}
        private void Draw()
        {
            content.Clear();var data=session.Data;
            Text(content,"AKUMU 2","flow-brand");Text(content,"KUROHASHI GAMES / WAREHOUSE INCIDENT","flow-kicker");
            if(session.Store.ReadOnly)Text(content,session.Store.Problem,"flow-warning");
            if(!string.IsNullOrEmpty(session.Notice))Text(content,session.Notice,"flow-warning");
            switch(session.Screen)
            {
                case FlowScreen.MainMenu:
                    Text(content,"Unemployed. Unqualified. Unreasonably confident.");
                    Action(content,"PLAY",()=>Go(FlowScreen.Levels));Action(content,"SETTINGS",()=>Go(FlowScreen.Settings));
                    if(Application.platform!=RuntimePlatform.Android)Action(content,"QUIT",()=>Application.Quit());break;
                case FlowScreen.Settings:
                    Text(content,"SETTINGS","flow-title");
                    Text(content,"Master audio");
                    var volume=new Slider(0,1){value=data.volume};volume.AddToClassList("flow-slider");
                    volume.RegisterValueChangedCallback(e=>{data.volume=e.newValue;AudioListener.volume=e.newValue;session.Save();});content.Add(volume);
                    Text(content,"Touch joystick scale — existing control response is unchanged");
                    var scale=new Slider(.75f,1.5f){value=data.joystickScale};scale.AddToClassList("flow-slider");
                    scale.RegisterValueChangedCallback(e=>{data.joystickScale=e.newValue;session.Save();});content.Add(scale);
                    Text(content,"PC: A/D move · S crouch · SPACE jump · RMB aim · LMB/F fire · H hold heal\nTouch: MOVE / AIM sticks · JUMP · FIRE · HOLD HEAL");
                    Action(content,"BACK",()=>Go(FlowScreen.MainMenu));break;
                case FlowScreen.Levels:
                    Text(content,"LEVEL SELECTION","flow-title");Text(content,"Wallet $"+data.balance);
                    foreach(var level in catalog.levels)
                    {
                        var row=new VisualElement();row.AddToClassList("level-entry");content.Add(row);
                        if(level.icon!=null){var icon=new Image{sprite=level.icon};icon.AddToClassList("level-icon");row.Add(icon);}
                        Text(row,level.title+" — "+(data.completed.Contains(level.id)?"COMPLETED":session.IsUnlocked(level)?"AVAILABLE":"LOCKED"),"flow-title");
                        Text(row,level.description);
                        Action(row,"PREPARE EQUIPMENT",()=>{if(session.SelectLevel(level.id)){selected=null;Draw();}},session.IsUnlocked(level));
                    }
                    Action(content,"MAIN MENU",()=>Go(FlowScreen.MainMenu));break;
                case FlowScreen.Shop:
                    Text(content,"PREPARATION / THE EQUIPMENT SHOP","flow-title");
                    Text(content,"Wallet $"+data.balance+" · Tape "+data.tape.ToString("0")+"/"+catalog.tapeCapacity+" · Armor "+data.armor.ToString("0")+"/"+catalog.armorCapacity);
                    var tabs=new VisualElement();tabs.AddToClassList("flow-row");content.Add(tabs);
                    foreach(ShopCategory tab in Enum.GetValues(typeof(ShopCategory)))
                        Action(tabs,tab==ShopCategory.BodyArmor?"Body armor":tab==ShopCategory.DuctTape?"Duct tape":tab.ToString(),()=>{category=tab;selected=null;Draw();});
                    var items=new VisualElement();items.AddToClassList("flow-row");content.Add(items);
                    var list=new VisualElement();list.AddToClassList("flow-list");items.Add(list);
                    var detail=new VisualElement();detail.AddToClassList("flow-detail");items.Add(detail);
                    foreach(var offer in catalog.offers)if(offer.category==category){if(selected==null)selected=offer.id;Action(list,offer.title+"  $"+offer.price,()=>{selected=offer.id;Draw();});}
                    var item=catalog.Find(selected);
                    if(item!=null)
                    {
                        Text(detail,item.title,"flow-title");Text(detail,item.description);Text(detail,"PRICE $"+item.price);
                        if(item.category==ShopCategory.Weapons)
                        {
                            bool owned=data.owned.Contains(item.id);Text(detail,owned?"OWNED":"NOT OWNED");
                            if(owned)
                            {
                                Text(detail,"Magazine "+data.Ammo(item.id).magazine+" · Reserve "+data.Ammo(item.id).reserve);
                                Action(detail,"EQUIP / PRIMARY",()=>{session.Equip(item.id);Draw();});
                                if(item.weapon.WeaponCategory==Inventory.WeaponCategory.Firearm)Action(detail,"EQUIP / SECONDARY",()=>{session.Equip(item.id,true);Draw();});
                            }
                        }
                        if(item.category==ShopCategory.Ammunition)
                        {var gun=Array.Find(catalog.offers,x=>x.category==ShopCategory.Weapons&&x.weapon==item.weapon);Text(detail,"Reserve "+(gun==null?0:data.Ammo(gun.id).reserve)+"/"+catalog.ammoCapacity);}
                        Action(detail,"BUY",()=>{session.Buy(item.id,Time.unscaledTime);Draw();},session.CanBuy(item.id));
                    }
                    Text(content,"Equipped: "+Name(data.primary)+" / "+Name(data.secondary)+" / "+Name(data.melee));
                    Action(content,"START MISSION — PURCHASE OPTIONAL",StartMission);
                    Action(content,"BACK TO LEVELS",()=>Go(FlowScreen.Levels));break;
            }
        }
        private string Name(string id)=>catalog.Find(id)?.title??"Empty";
        private void Update()
        {
            var screen=new Vector2Int(Screen.width,Screen.height);
            if(screen==lastScreen&&lastSafeArea==Screen.safeArea)return;lastScreen=screen;lastSafeArea=Screen.safeArea;
            var safe=GetComponent<UIDocument>().rootVisualElement.Q("flowSafeArea");
            var inset=Input.WarehouseTouchControls.SafeAreaInsets(lastSafeArea,screen);
            safe.style.left=Length.Percent(inset.x*100);safe.style.right=Length.Percent(inset.y*100);
            safe.style.top=Length.Percent(inset.z*100);safe.style.bottom=Length.Percent(inset.w*100);
        }
        private void StartMission()
        {
            var level=catalog.Level(session.SelectedLevel);
            if(level==null||!Application.CanStreamedLevelBeLoaded(level.scene)){Text(content,"Mission scene is unavailable.","flow-warning");return;}
            if(!session.PrepareMission())return;loading=true;content.SetEnabled(false);SceneManager.LoadSceneAsync(level.scene);
        }
    }
}
