from pathlib import Path
p=Path('Assets/Scripts/Input/WarehouseTouchControls.cs');s=p.read_bytes()
s=s.replace(b'if (subscribedActions != null) subscribedActions.InputReset -= ClearPointers;',b'if (subscribedActions != null) { subscribedActions.InputReset -= ClearPointers;subscribedActions.AimCancelled-=ClearAimPointer; }')
s=s.replace(b'if (subscribedActions != null) subscribedActions.InputReset += ClearPointers;',b'if (subscribedActions != null) { subscribedActions.InputReset += ClearPointers;subscribedActions.AimCancelled+=ClearAimPointer; }')
s=s.replace(b'        private void ClearPointers()\r\n',b'        private void ClearAimPointer(){aimStick?.Release();firingZone=false;ReleaseFire();}\r\n        private void ClearPointers()\r\n')
p.write_bytes(s)
