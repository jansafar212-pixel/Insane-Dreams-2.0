# Selected validation — Android performance sprint

Editor tests run against Unity 6000.6.6f1, CLI beta14. Final selected results:

| Play Mode suite | Passed / total |
|---|---:|
| AndroidPerformanceRegressionTests | 2 / 2 |
| AndroidPerformanceCaptureTests | 1 / 1 |
| AdaptiveCombatCameraTests | 9 / 9 |
| Sprint0031Tests (existing UI/shop/aspects) | 3 / 3 |
| GrenadeResupplyHotfixTests | 4 / 4 |
| Sprint0032Tests (existing AI/recognition/navigation) | 10 / 10 |
| FirstOperationTests | 6 / 6 |
| ReactiveRagdollTests | 5 / 5 |
| Sprint0029SystemsTests | 4 / 4 |
| Hotfix00253PlaytestTests | 6 / 6 |
| LadderCameraHotfixTests | 1 / 1 |
| Sprint0246Tests | 5 / 5 |

| Edit Mode suite | Passed / total |
|---|---:|
| CombatRulesTests | 7 / 7 |
| CowardsBunkerRulesTests | 2 / 2 |
| FirstOperationLayoutTests | 2 / 2 |

Raw final test_status JSONs and ValidationSummary.json provide actual results;
the archived initial new-fixture run is excluded. Initial failures assumed a
pre-equipped secondary weapon; tests now explicitly equip BackupPistol. Production
inventory and progression were not changed. Existing historical Sprint013 failures
documented by the camera sprint were not rerun or claimed fixed here.

The new cache test verifies unchanged-value rejection, changed magazine/reserve,
capacity display, weapon selection, grenade type/count and bind invalidation through
the real UIDocument. Equipment checks exercise repeated poses, weapon switches,
face changes and finite muzzle position. Existing tests cover firing, reloads,
cover, camera, actual warehouse route/roof ladder, medical/shop, grenades and corpse
physics. These are automated/controlled checks, not human gameplay approval.

ProtectedBefore/After.json: 124 original source/settings/scene files audited;
120 unchanged. The only changed existing runtime files are CharacterVisualDriver,
CutoutRig, WarehouseTouchControls and MissionHud. New AndroidFramePacing initializer
does not edit camera behavior or desktop frame settings. No scene, prefab, quality,
physics, level, UI layout or graphics asset changes. Existing dirty work preserved.

AllocationCounterCheck.json proves GC.GetAllocatedBytesForCurrentThread is unsupported
in this runtime. Zero-byte results from that API are invalid. Editor capture fields
with bytesPerCall -1 mean unavailable. No allocation-free or Player memory-stability
claim is made; prior camera evidence has an appended correction.

Report.md contains short-run Editor timings, caveats, Android profiling steps and
unimplemented opportunities. No attached Android device, no 15-minute thermal run,
no device FPS/input/GPU/memory measurement and no profiling APK export in this pass.
Use the documented Development Build procedure; older0033 APK predates these changes.

Final Editor references/Console audit is stored separately. No stage, commit or push.

Final native audit: Warehouse_Demo open/saved, Play Mode stopped, compilation
successful, current Console 0 errors/32 FMOD output-device warnings. No missing inspected scene/prefab
scripts. Existing MainMenu-first enabled build scene order retained. Final selected
67/67 passed; subsequent camera rerun changes only allocation-counter validation.

