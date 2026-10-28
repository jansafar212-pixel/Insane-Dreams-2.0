from pathlib import Path
p=Path('Assets/Scripts/Input/WarehouseTouchControls.cs');s=p.read_bytes()
s=s.replace(b'public bool throwing,reloading,healing,waiting;',b'public bool throwing,reloading,cycling,healing,waiting;')
s=s.replace(b'&&reloading==b.reloading&&healing',b'&&reloading==b.reloading&&cycling==b.cycling&&healing')
s=s.replace(b'reloading=w!=null&&w.Cycle.IsReloading,hp=',b'reloading=w!=null&&w.Cycle.IsReloading,cycling=w!=null&&w.Cycle.IsRecovering(Time.time),hp=')
s=s.replace(b'?"RELOADING":loadout==null?',b'?"RELOADING":weapon!=null&&weapon.Cycle.IsRecovering(Time.time)?"CYCLING":loadout==null?')
p.write_bytes(s)
