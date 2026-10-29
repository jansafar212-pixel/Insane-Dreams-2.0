from pathlib import Path
p=Path('Assets/Scripts/Input/WarehouseTouchControls.cs');s=p.read_bytes()
anchor=b'                UpdateFireIntent();\r\n            });'
assert s.count(anchor)==1
s=s.replace(anchor,b'                UpdateFireIntent();\r\n            },()=>actions?.BeginTouchAim(),normal=>actions?.EndTouchAim(normal));')
s=s.replace(b'            private int pointer = -1;',b'            private int pointer = -1;\r\n            private readonly System.Action started;private readonly System.Action<bool> finished;')
s=s.replace(b'System.Action<Vector2> callback)',b'System.Action<Vector2> callback,System.Action started=null,System.Action<bool> finished=null)')
s=s.replace(b'changed = callback;',b'changed = callback;this.started=started;this.finished=finished;')
s=s.replace(b'pad.CapturePointer(pointer); SetPosition(evt.position);',b'pad.CapturePointer(pointer);started?.Invoke(); SetPosition(evt.position);')
s=s.replace(b'if (evt.pointerId == pointer) Release(); }\r\n            private void Cancel',b'if (evt.pointerId == pointer) Release(true); }\r\n            private void Cancel')
s=s.replace(b'            public void Release()\r\n            {\r\n                int old = pointer; pointer = -1;',b'            public void Release()=>Release(false);\r\n            private void Release(bool normal)\r\n            {\r\n                int old = pointer; pointer = -1;\r\n                if(old>=0)finished?.Invoke(normal);')
p.write_bytes(s)
