# Sprint 0033 — completed

Original requirements: TestResults/Sprint0033/OriginalRequirements.md.
Full report/files/dimensions/manual Android checklist: TestResults/Sprint0033/Report.md.
Previous0032checkpoint preserved in that folder. Do not restart this hotfix.

Implemented:9x6.35actual shell vs4.5x3.175old (2xwidth ANDheight), clearinterior
8.25x5.85, sameapprovedsprites, simplecutawaytrim forfullOfficer visibility.
Five solidfaces matchsprites. Slit2.5..3.9. Reachablepeek141.25 (fixeswallprobe
boundary), hide/spawn146.5. Existing-system roofladder139 preserves real traversal
because doubledroof exceeds unchangedjumpheight. Originalterrain/roster/pacing/
AI0032/UI0031/player/weapon/health preserved. No prefabs/art assets changed.
Onlyruntimecodechange: OfficerShotTelegraph visualray ignores navigation triggers,
matching realbullets/sight; original.5s warning/tracer/muzzle/timers retained.

Final36/36selectedtests passed:25PlayMode/11EditMode. Realpeek/fire/hide/reload,
laser/window/solidconcrete, grenade roofcollision, physicalplayerladder/roofexit
andcompletion, all7ladderexits, mobileaspectHUD/shop andweaponrules. Initial
issues/fixes preserved in artifacts; no unresolvedselectedtest failure.
Controlledworld/exposed/hiddenHUDcaptures inspected. Human/device readability,
fingerocclusion andhardwareperformance remain manual.

Android RELEASE build_af454f66c815 succeeded,0errors,972knownwarnings.
APK: build/Sprint0033/AKUMU2-playtest.apk,39,750,643bytes.
APK newladder and0032AImarkers verified. ExistingIL2CPP/ARM64/API26/autoSDK/
LandscapeLeft/APK/developmentfalse settings preserved. No physicaldevice tested.
MainMenu firstenabledscene. Demo saved/clean/open,PlayModestopped,notcompiling,
missing scene/prefab scripts0,Consoleerrors0/82collapsedbuildwarnings.
No stage/commit/push. Existingdirtywork preserved. No test/buildjob active.

Next ifrequested: install this APK and follow Report.md manualchecklist; address
reproduced findings only. Use Sprint0033Setup, not historical compactbunker generator.
CLI latestbeta14 verified; C:/Users/jansa/AppData/Local/Unity/bin/unity.exe,
command editor_status (not status), --caller plugin --skill unity-cli --format json.
ActualUnitylog nowproject Logs/Editor.log, notpreallocatedLOCALAPPDATAlog.
