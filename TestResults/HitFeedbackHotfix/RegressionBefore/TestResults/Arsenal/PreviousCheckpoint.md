# Android performance pass — implemented, device acceptance pending

Latest user Sprint0032 Android-performance brief preserved verbatim in
TestResults/AndroidPerformance/Brief.md. Previous camera checkpoint preserved in
PreviousCheckpoint.md there. Do not restart earlier UI/AI/bunker/camera work.
Preserve all prior dirty changes; no staging, commit or push.

Implemented measured optimizations: final equipped weapon sprite resolved once;
identical weapon/face/magazine setters avoided; unchanged HUD values compared
before formatting; mission displayed-second and revive-offer caption cached.
Android-only BeforeSceneLoad requests60FPS. No physics/AI/input/control/camera
behavior, art, layout, weapon balance, effects intensity or scene/prefab changes.
Modified existing runtime scripts: CharacterVisualDriver, CutoutRig,
WarehouseTouchControls, MissionHud. New AndroidFramePacing static initializer.
New Editor-only profiling/HUD tests; existing AdaptiveCombatCameraTests allocation
assertion now requires a working counter. All behavior assertions preserved.

Evidence/report: TestResults/AndroidPerformance/Report.md, Validation.md,
ValidationSummary.json, baseline.json/after.json/after-final.json and native audits.
Editor initial LateBehaviourUpdate1.051ms; nested sprite changes0.473ms ->0.040ms.
Unchanged HUD15.41->2.26us/call. Short full-scene600frame averages177.52->205.64FPS,
p957.242->6.392ms, p998.663->7.955ms,1%low68.92->76.57FPS. Intermediate after had
worse tails; do not extrapolate short/noisy Editor results to sustained Android.

Important correction: GC.GetAllocatedBytesForCurrentThread returns0even for known
8KiBallocation on this UnityMono runtime. Camera zero-byte claim is invalid;
Report/Validation append corrections and allocation artifact now writes NOT TESTED.
Do not use old baseline bytesPerCall zeros as allocation evidence. Need reliable
GC.Alloc/Player memory profiling on device. No whole-game zero-GC claim.

Selected results67/67 (56Play,11Edit): newHUD/equipment2, capture1, camera9, UI3,
grenade4, AI10, actualmission6, ragdoll5, equipment4, weapon/muzzle6, ladder1,
corpse/reload5 andEditrules11. Newfixture initial sidearm assumption fixed by explicit
BackupPistol equip. Camera rerun after counter validation correction recorded in
same folder. Historical Sprint013 failures were not rerun or claimed fixed.

Native final reference audit: Demo saved/open/stopped,0missing inspected scene/
prefab scripts, currentConsole0errors/32FMOD output-device warnings, compilation successful.
Protected audit124original runtime/settings/scenes:120unchanged,4scoped edits.
Current project Androidsettings remain optimizedFramePacingtrue, ARM64/IL2CPP,
API26minimum, URPscale1, AAoff andphysics0.02seconds. No native asset edits.

No Android device attached. No APK exported this sprint. Old0033APK predates camera
and these optimizations, so it is not an equivalent baseline. ActualA56FPS, GPU,
main/render-thread time, draw calls/batches, input latency, Player memory/thermal
behavior and15minute stress acceptance remain NOT TESTED. Exact DevelopmentBuild/
USBProfiler/releasecomparison protocol and nextthree optimization opportunities
are in Report.md. Next step is physical-device profiling, then only measured fixes.
Do not claim60FPS stabilization achieved. Do not rerun OptimizeHud.py (one-time
byte-preserving patch helper already applied). No further speculative rewrites.


Final warning detail:32current FMOD host output-device initialization warnings;
audio remains suspended. Do not claim audio audition verified. Historical warning
buffer additionally has obsolete API/account-service messages. No game audio code
or device settings changed. FinalWarnings.json preserves evidence.
