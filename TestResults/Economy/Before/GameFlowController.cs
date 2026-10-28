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
        private static readonly string[] LogoGlyphs={
            "01110"+"11011"+"11011"+"11111"+"11011"+"11011"+"11011",
            "11011"+"11011"+"11110"+"11100"+"11110"+"11011"+"11011",
            "11011"+"11011"+"11011"+"11011"+"11011"+"11011"+"01110",
            "10001"+"11011"+"10101"+"10101"+"10001"+"10001"+"10001",
            "01110"+"11011"+"00011"+"00110"+"01100"+"11000"+"11111"};
        private Rect lastSafeArea;
        private Vector2Int lastScreen;
        private void Start()
        {
            session=GameSession.Instance;
            if(session==null){session=new GameObject("CampaignSession").AddComponent<GameSession>();session.Initialize(catalog,System.IO.Path.Combine(Application.persistentDataPath,"akumu2_progress_v1.json"));}
            var root=GetComponent<UIDocument>().rootVisualElement;
            var scroll=root.Q<ScrollView>();scroll.verticalScroller.lowButton.text="^";scroll.verticalScroller.highButton.text="v";
            scroll.verticalScroller.lowButton.AddToClassList("flow-scroll-arrow");scroll.verticalScroller.highButton.AddToClassList("flow-scroll-arrow");
            content=root.Q("flowContent");Draw();
        }
        private void Text(VisualElement parent,string value,string style="flow-copy"){var label=new Label(value);label.AddToClassList(style);parent.Add(label);}
        private Button Action(VisualElement parent,string text,System.Action callback,bool enabled=true)
        {var button=new Button(callback){text=text};button.AddToClassList("flow-button");var notch=new VisualElement{pickingMode=PickingMode.Ignore};notch.AddToClassList("button-notch");button.Add(notch);button.SetEnabled(enabled&&!loading);parent.Add(button);return button;}
        private void Go(FlowScreen screen){session.Navigate(screen);Draw();}
        private void PixelLogo(VisualElement parent)
        {
            var logo=new VisualElement{name="akumuWordmark",tooltip="AKUMU 2",pickingMode=PickingMode.Ignore};logo.AddToClassList("flow-logo");parent.Add(logo);int x=0;
            foreach(int index in new[]{0,1,2,3,2,-1,4})
            {
                if(index<0){x+=30;continue;}string glyph=LogoGlyphs[index];
                for(int y=0;y<7;y++)for(int col=0;col<5;)
                {
                    if(glyph[y*5+col]!='1'){col++;continue;}int start=col;while(col<5&&glyph[y*5+col]=='1')col++;
                    var strip=new VisualElement{pickingMode=PickingMode.Ignore};strip.style.position=Position.Absolute;strip.style.left=Length.Percent((x+start*10)/390f*100);strip.style.top=y*10;strip.style.width=Length.Percent((col-start)*10/390f*100);strip.style.height=10;strip.style.backgroundColor=new Color(1,1,235f/255);logo.Add(strip);
                }
                x+=60;
            }
        }
        private void Draw()
        {
            content.Clear();var data=session.Data;
            foreach(string screenClass in new[]{"mainmenu","settings","levels","shop"})content.EnableInClassList("screen-"+screenClass,session.Screen.ToString().ToLowerInvariant()==screenClass);
            content.EnableInClassList("front-page",session.Screen==FlowScreen.MainMenu);
            if(session.Screen!=FlowScreen.MainMenu)Text(content,"AKUMU 2","flow-brand");
            Text(content,"KUROHASHI GAMES  /  FIELD OPERATIONS","flow-kicker");
            if(session.Store.ReadOnly)Text(content,session.Store.Problem,"flow-warning");
            if(!string.IsNullOrEmpty(session.Notice))Text(content,session.Notice,"flow-warning");
            switch(session.Screen)
            {
                case FlowScreen.MainMenu:
                    var front=new VisualElement();front.AddToClassList("flow-front");content.Add(front);
                    var hero=new VisualElement();hero.AddToClassList("flow-hero");front.Add(hero);
                    PixelLogo(hero);
                    Text(hero,"BAD DREAMS.\nWORSE EMPLOYEES.","flow-tagline");
                    Text(hero,"NO TRAINING PROVIDED","flow-stamp");
                    Text(hero,"Unemployed. Unqualified.\nUnreasonably confident.","flow-intro");
                    var nav=new VisualElement();nav.AddToClassList("flow-nav");front.Add(nav);
                    Text(nav,"YOUR NEXT BAD DECISION","flow-nav-title");
                    Action(nav,"PLAY",()=>Go(FlowScreen.Levels)).AddToClassList("flow-primary");
                    Action(nav,"SETTINGS",()=>Go(FlowScreen.Settings));
                    if(Application.platform!=RuntimePlatform.Android)Action(nav,"QUIT",()=>Application.Quit()).AddToClassList("flow-quiet");
                    Text(nav,"Clock in. Cause problems.\nTry to clock out.","flow-nav-note");
                    Text(content,"INJURIES WILL NOT BE REIMBURSED.   //   EST. BAD IDEAS","flow-footer");break;
                case FlowScreen.Settings:
                    Text(content,"SETTINGS","flow-title");
                    var audioRow=new VisualElement();audioRow.AddToClassList("flow-row");content.Add(audioRow);Text(audioRow,"MASTER AUDIO");var audioValue=new Label(Mathf.RoundToInt(data.volume*100)+"% "){name="volumeReadout"};audioValue.AddToClassList("setting-value");audioRow.Add(audioValue);
                    var volume=new Slider(0,1){value=data.volume};volume.AddToClassList("flow-slider");
                    volume.RegisterValueChangedCallback(e=>{data.volume=e.newValue;AudioListener.volume=e.newValue;audioValue.text=Mathf.RoundToInt(e.newValue*100)+"%";session.Save();});content.Add(volume);
                    var touchRow=new VisualElement();touchRow.AddToClassList("flow-row");content.Add(touchRow);Text(touchRow,"TOUCH CONTROL SIZE");var touchValue=new Label(Mathf.RoundToInt(data.joystickScale*100)+"%"){name="joystickReadout"};touchValue.AddToClassList("setting-value");touchRow.Add(touchValue);
                    var scale=new Slider(.75f,1.5f){value=data.joystickScale};scale.AddToClassList("flow-slider");
                    scale.RegisterValueChangedCallback(e=>{data.joystickScale=e.newValue;touchValue.text=Mathf.RoundToInt(e.newValue*100)+"%";session.Save();});content.Add(scale);
                    Text(content,"PC: A/D move · S crouch · SPACE jump · RMB aim · LMB/F fire · H hold heal\nTouch: MOVE / AIM sticks · push AIM to fire · JUMP · HOLD HEAL");
                    Action(content,"BACK",()=>Go(FlowScreen.MainMenu));break;
                case FlowScreen.Levels:
                    Text(content,"LEVEL SELECTION","flow-title");Wallet(content,"$"+data.balance+" / PETTY CASH");
                    foreach(var level in catalog.levels)
                    {
                        var row=new VisualElement();row.AddToClassList("level-entry");row.EnableInClassList("is-complete",data.completed.Contains(level.id));row.EnableInClassList("is-locked",!session.IsUnlocked(level));row.EnableInClassList("is-selected",level.id==session.SelectedLevel);content.Add(row);
                        if(level.icon!=null){var icon=new Image{sprite=level.icon};icon.AddToClassList("level-icon");row.Add(icon);}
                        Text(row,level.title+" — "+(data.completed.Contains(level.id)?"COMPLETED":session.IsUnlocked(level)?"AVAILABLE":"LOCKED"),"flow-title");
                        Text(row,level.description);
                        Action(row,"PREPARE EQUIPMENT",()=>{if(session.SelectLevel(level.id)){selected=null;Draw();}},session.IsUnlocked(level));
                    }
                    Action(content,"MAIN MENU",()=>Go(FlowScreen.MainMenu));break;
                case FlowScreen.Shop:
                    Text(content,"PREPARATION / THE EQUIPMENT SHOP","flow-title");
                    Text(content,$"Wallet ${data.balance} · Duct Tape {data.tape:0}/{catalog.tapeCapacity} · Vest {data.armor:0} · Helmet {data.helmet:0}");
                    Text(content,$"Carry limits: {catalog.ReserveCapacity(data)} spare rounds per firearm / {catalog.GrenadeCapacity(data)} grenades per type. Reload +{(catalog.ReloadMultiplier(data)-1)*100:0}%.");
                    var tabs=new VisualElement();tabs.AddToClassList("flow-row");content.Add(tabs);
                    tabs.name="shopCategories";
                    foreach(ShopCategory tab in new[]{ShopCategory.Weapons,ShopCategory.Ammunition,ShopCategory.BodyArmor,ShopCategory.Faintanyl})
                    {var button=Action(tabs,tab==ShopCategory.Faintanyl?"Medical":tab==ShopCategory.BodyArmor?"Body armor":tab.ToString(),()=>{category=tab;selected=null;Draw();});button.EnableInClassList("is-selected",tab==category);}
                    if(category!=ShopCategory.Faintanyl)
                    {
                    var items=new VisualElement();items.AddToClassList("flow-row");content.Add(items);
                    var list=new VisualElement();list.AddToClassList("flow-list");items.Add(list);
                    var detail=new VisualElement();detail.AddToClassList("flow-detail");items.Add(detail);
                    foreach(var offer in catalog.offers)if((offer.category==category||category==ShopCategory.Ammunition&&offer.category==ShopCategory.Throwables)&&!(offer.weapon!=null&&offer.weapon.WeaponCategory==Inventory.WeaponCategory.Melee)){if(selected==null)selected=offer.id;var entry=Action(list,offer.title+"  $"+offer.price,()=>{selected=offer.id;Draw();});entry.EnableInClassList("is-selected",selected==offer.id);entry.tooltip=offer.category==ShopCategory.Weapons&&data.owned.Contains(offer.id)?"OWNED":PurchaseState(offer);}
                    var item=catalog.Find(selected);
                    if(item!=null)
                    {
                        ItemArt(detail,item);Text(detail,item.title,"flow-title");Text(detail,item.description);Text(detail,"PRICE $"+item.price);
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
                        {var gun=Array.Find(catalog.offers,x=>x.category==ShopCategory.Weapons&&x.weapon==item.weapon);Text(detail,"Reserve "+(gun==null?0:data.Ammo(gun.id).reserve)+" / carry limit "+catalog.ReserveCapacity(data));}
                        if(item.category==ShopCategory.Throwables)Text(detail,"Stock "+data.grenades[(int)item.throwable]+" / carry limit "+catalog.GrenadeCapacity(data)+". Aim, then release to throw.");
                        if(item.category==ShopCategory.BodyArmor){var stats=catalog.Protection(item.helmet,Mathf.Clamp(item.armorTier,1,2));Text(detail,$"Protection {stats.capacity:0} / absorbs {stats.absorption*100:0}% ballistic damage");Text(detail,$"Reload +{(stats.reloadMultiplier-1)*100:0}% / reserve -{stats.reservePenalty} / grenades -{stats.grenadePenalty}");Text(detail,"Movement speed unchanged. Replaces current equipment in this slot; excess ammunition stays stored between missions.");}
                        PurchaseReadout(detail,item);Action(detail,"BUY",()=>{session.Buy(item.id,Time.unscaledTime);Draw();},session.CanBuy(item.id));
                    }
                    }
                    var faintanyl=Array.Find(catalog.offers,x=>x.category==ShopCategory.Faintanyl);
                    if(category==ShopCategory.Faintanyl)
                    {
                        var medical=new VisualElement{name="medicalShop"};content.Add(medical);
                        var tape=Array.Find(catalog.offers,x=>x.category==ShopCategory.DuctTape);
                        if(tape!=null){var panel=new VisualElement{name="ductTapeShop"};panel.AddToClassList("flow-detail");medical.Add(panel);ItemArt(panel,tape);Text(panel,tape.title,"flow-title");Text(panel,$"RESERVE {data.tape:0}/{catalog.tapeCapacity} / ${tape.price}");Text(panel,tape.description);PurchaseReadout(panel,tape);Action(panel,"Buy Duct Tape",()=>{session.Buy(tape.id,Time.unscaledTime);Draw();},session.CanBuy(tape.id)).name="procureDuctTape";}
                        if(faintanyl!=null){var consumable=new VisualElement{name="faintanylShop"};consumable.AddToClassList("flow-detail");medical.Add(consumable);ItemArt(consumable,faintanyl);Text(consumable,faintanyl.title,"flow-title");Text(consumable,$"STOCK {data.faintanyl}/{Combat.PlayerRevive.Capacity} / ${faintanyl.price}");Text(consumable,faintanyl.description);Text(consumable,"Automatic fictional revival after defeat; exactly 2 seconds of protection.");PurchaseReadout(consumable,faintanyl);Action(consumable,"Procure Faintanyl",()=>{session.Buy(faintanyl.id,Time.unscaledTime);Draw();},session.CanBuy(faintanyl.id)).name="procureFaintanyl";}
                    }
                    Text(content,"Equipped: "+Name(data.primary)+" / "+Name(data.secondary)+" / THROWABLES");
                    Action(content,"START MISSION — PURCHASE OPTIONAL",StartMission);
                    Action(content,"BACK TO LEVELS",()=>Go(FlowScreen.Levels));break;
            }
        }
        private void Wallet(VisualElement parent,string text)
        {var row=new VisualElement();row.AddToClassList("wallet-row");var icon=new VisualElement{pickingMode=PickingMode.Ignore};icon.AddToClassList("ui-icon");icon.AddToClassList("coin-mark");row.Add(icon);Text(row,text,"wallet-value");parent.Add(row);}
        private void ItemArt(VisualElement parent,ShopOffer offer)
        {
            if(offer.weapon!=null&&offer.category==ShopCategory.Weapons){var image=new Image{sprite=offer.weapon.EquippedSprite,scaleMode=ScaleMode.ScaleToFit,pickingMode=PickingMode.Ignore};image.AddToClassList("shop-icon");parent.Add(image);return;}
            string icon=offer.category==ShopCategory.DuctTape?"tape-mark":offer.category==ShopCategory.Faintanyl?"syringe-mark":offer.category==ShopCategory.BodyArmor?(offer.helmet?"helmet-mark":"vest-mark"):offer.category==ShopCategory.Throwables?(offer.throwable==Combat.ThrowableKind.Fragmentation?"frag-mark":offer.throwable==Combat.ThrowableKind.Flashbang?"flash-mark":"smoke-mark"):"ammo-mark";
            var art=new VisualElement{pickingMode=PickingMode.Ignore};art.AddToClassList("ui-icon");art.AddToClassList("shop-icon");art.AddToClassList(icon);parent.Add(art);
        }
        private string PurchaseState(ShopOffer offer)
        {
            if(session.Store.ReadOnly)return "SAVE READ-ONLY";
            if(offer.category==ShopCategory.Weapons&&session.Data.owned.Contains(offer.id))return "OWNED / EQUIP BELOW";
            if(session.Data.balance<offer.price)return "NEED $"+(offer.price-session.Data.balance)+" MORE";
            if(session.CanBuy(offer.id))return "READY / $"+offer.price;
            if(offer.category==ShopCategory.Ammunition){var gun=Array.Find(catalog.offers,o=>o.category==ShopCategory.Weapons&&o.weapon==offer.weapon);if(gun!=null&&!session.Data.owned.Contains(gun.id))return "WEAPON REQUIRED";}
            return "STOCK FULL / ALREADY EQUIPPED";
        }
        private void PurchaseReadout(VisualElement parent,ShopOffer offer)
        {var status=new Label(PurchaseState(offer));status.AddToClassList("purchase-state");status.EnableInClassList("unavailable",!session.CanBuy(offer.id));parent.Add(status);}
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
