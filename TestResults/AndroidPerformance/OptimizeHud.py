from pathlib import Path
p=Path('Assets/Scripts/Input/WarehouseTouchControls.cs')
b=p.read_bytes()
assert b'private bool ReadoutChanged' not in b
b=b.replace(b'            nextReadout = 0;', b'            nextReadout = 0;hasReadout=false;')
b=b.replace(b'        private void OnEnable()\r\n        {',b'        private void OnEnable()\r\n        {\r\n            hasReadout=false;shownReviveOffer=-1;')
start=b'                var weapon = loadout != null ? loadout.SelectedWeapon : null;'
assert b.count(start)==1
b=b.replace(start,b'                if(ReadoutChanged())\r\n                {\r\n'+start)
end=b'            }\r\n            if(reviveOverlay!=null)'
assert b.count(end)==1
b=b.replace(end,b'                }\r\n'+end)
old=b'if(offer){reviveButton.text=$"INJECT FAINTANYL'
assert old in b
b=b.replace(old,b'if(offer){if(shownReviveOffer!=revive.Injections){shownReviveOffer=revive.Injections;reviveButton.text=$"INJECT FAINTANYL')
b=b.replace(b'/2)";if(Time.timeScale>0)',b'/2)";}if(Time.timeScale>0)')
b=b.replace(b'reviveOverlay.BringToFront();}\r\n            }',b'reviveOverlay.BringToFront();}\r\n                else shownReviveOffer=-1;\r\n            }')
helper='''        private int shownReviveOffer=-1;
        private ReadoutState shownReadout;
        private bool hasReadout;
        // Compare values before formatting. Input/context controls retain their
        // per-frame updates, and readouts retain the existing 0.2-second cadence.
        private struct ReadoutState
        {
            public InsaneDreams.Inventory.RuntimeWeapon weapon,primary,secondary;
            public Sprite icon;
            public string title,notice;
            public InsaneDreams.Inventory.SupplyCrate2D crate;
            public int magazine,capacity,reserve,slot,kind,frag,flash,smoke,maximum,injections;
            public float hp,maxHp,vest,helmet,tape,protection;
            public bool throwing,reloading,healing,waiting;
            public bool Same(ReadoutState b)=>weapon==b.weapon&&primary==b.primary&&secondary==b.secondary&&icon==b.icon&&title==b.title&&notice==b.notice&&crate==b.crate&&
                magazine==b.magazine&&capacity==b.capacity&&reserve==b.reserve&&slot==b.slot&&kind==b.kind&&frag==b.frag&&flash==b.flash&&smoke==b.smoke&&maximum==b.maximum&&injections==b.injections&&
                hp==b.hp&&maxHp==b.maxHp&&vest==b.vest&&helmet==b.helmet&&tape==b.tape&&protection==b.protection&&throwing==b.throwing&&reloading==b.reloading&&healing==b.healing&&waiting==b.waiting;
        }
        private bool ReadoutChanged()
        {
            var w=loadout!=null?loadout.SelectedWeapon:null;
            var state=new ReadoutState{weapon=w,primary=loadout!=null?loadout.Get(InsaneDreams.Inventory.EquipmentSlot.Firearm1):null,secondary=loadout!=null?loadout.Get(InsaneDreams.Inventory.EquipmentSlot.Firearm2):null,
                icon=w!=null&&w.Definition.EquippedSprite!=null?w.Definition.EquippedSprite:visualRig!=null?visualRig.weapon.sprite:null,title=w?.Definition.DisplayName,
                magazine=w!=null?w.MagazineAmmo:0,capacity=w!=null?w.Definition.MagazineCapacity:0,reserve=w!=null?w.ReserveAmmo:0,slot=loadout!=null?(int)loadout.SelectedSlot:-1,
                throwing=throwables!=null&&throwables.IsSelected,kind=throwables!=null?(int)throwables.Selected:0,maximum=throwables!=null?throwables.MaximumCapacity:0,
                frag=throwables!=null?throwables.Count((InsaneDreams.Combat.ThrowableKind)0):0,flash=throwables!=null?throwables.Count((InsaneDreams.Combat.ThrowableKind)1):0,smoke=throwables!=null?throwables.Count((InsaneDreams.Combat.ThrowableKind)2):0,
                reloading=w!=null&&w.Cycle.IsReloading,hp=health!=null?health.Current:0,maxHp=health!=null?health.Maximum:0,vest=armor!=null?armor.Condition:0,helmet=armor!=null?armor.HelmetCondition:0,
                tape=healing!=null?healing.DuctTape:0,healing=healing!=null&&healing.IsHealing,waiting=healing!=null&&healing.WaitingForRelease,
                injections=revive!=null?revive.Injections:0,protection=revive!=null?revive.ProtectionRemaining:0,crate=supplies!=null?supplies.NearbyCrate:null,notice=supplies!=null?supplies.Notification:null};
            if(hasReadout&&shownReadout.Same(state))return false;
            shownReadout=state;hasReadout=true;return true;
        }

'''
b=b.replace(b'        private void Update()\r\n',helper.replace('\n','\r\n').encode('ascii')+b'        private void Update()\r\n')
p.write_bytes(b)
