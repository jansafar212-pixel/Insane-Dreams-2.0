from pathlib import Path
p=Path('Assets/Scripts/Input/WarehouseTouchControls.cs');s=p.read_bytes();s=s.replace(b'?"CYCLING":loadout==null?',b'?"CYCLING":weapon!=null&&weapon.Definition.FireMode==InsaneDreams.Inventory.WeaponFireMode.ReleaseToFire?"RELEASE TO FIRE":loadout==null?');p.write_bytes(s)
