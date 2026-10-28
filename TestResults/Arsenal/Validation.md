# Arsenal verification — 2026-10-27

Final selected **72/72 passed: 59 Play Mode, 13 Edit Mode**. Not a full project run.
Raw final JSONs and ValidationSummary.json retained; initial fixture/integration
failures are archived separately and excluded from totals.

| Suite | Mode | Passed |
|---|---|---:|
| ArsenalTests | Play | 8/8 |
| ArsenalLabTests | Play | 1/1 |
| Hotfix00253PlaytestTests | Play | 6/6 |
| Sprint0246Tests | Play | 5/5 |
| AndroidPerformanceRegressionTests | Play | 2/2 |
| Sprint0031Tests (existing UI/shop) | Play | 3/3 |
| Sprint0032Tests (existing AI) | Play | 10/10 |
| FirstOperationTests | Play | 6/6 |
| ReactiveRagdollTests | Play | 5/5 |
| AdaptiveCombatCameraTests | Play | 9/9 |
| GrenadeResupplyHotfixTests | Play | 4/4 |
| ArsenalRulesTests | Edit | 4/4 |
| CombatRulesTests | Edit | 7/7 |
| FirstOperationLayoutTests | Edit | 2/2 |

Arsenal-specific checks exercise all eight guns moving, airborne and crouching;
magazine empty/refill, reload timing/conservation/armor scale/interrupt/switch;
all eight through all four actual enemy archetypes' hitboxes; seven pellet damage
events against overlapping colliders with one cartridge/trigger cue; LMG bloom
build/recovery and constant movement speed; long laser and renderer reuse; sniper
hold/cancel/duplicate release/recovery/switch/pause/reset/death/actual Faintanyl
revival. Stocked-grenade switch test verifies no stale throw and movement retained.

The lab integration uses an isolated temporary save, actual MainMenu -> Testbed ->
MainMenu loads, all eight equip/refills, real firing/armed laser state and actual
GameView PNG/state capture. Profile bytes stay identical. Captures were inspected;
new art is not claimed artistically approved, and no human gameplay claim is made.

Stress.txt records 360 trigger pulls / 1,080 actual ray segments and stable 17-line
baseline (16 tracer pool + existing grenade trajectory, before lazy sniper laser).
This concentrated Editor workload is not device/sustained FPS or zero-GC evidence.
The previous sprint established that Mono's per-thread allocation counter is
unsupported; no allocation claim relies on its zero values.

Prebuild native references: Demo saved/open/stopped, no missing inspected scene or
prefab scripts, compilation successful, current Console 0 errors/0 warnings.
Host FMOD output-device warnings occurred earlier; audio audition remains unverified.
No attached Android device; real A56 multitouch, frame pacing, thermals and weapon
feel remain manual. Previous historical Sprint013 failures were not claimed fixed.

ProtectedBefore/After: 423 original files checked, 412 unchanged. Scoped existing
changes: nine runtime files, player tracer capacity in its prefab, and appended
Testbed build-list entry. All scene content, original weapon/data/art assets,
enemy scripts, camera controller, movement/traversal, UI layout assets and shop
catalog remain unchanged. Native asset authoring only; no serialized YAML/GUID edits.

BuildDryRun.json validated Android Development export; BuildStart.json records
build_b5950726ee99. Final build/package outcome is recorded in Report.md once done.
No staging, commit or push. Existing uncommitted work preserved.

Context-restoration verification complete: 73/73 selected automated checks pass
(59 Play Mode, 14 Edit Mode), including the added frame-statistics test. Focused
reruns: Arsenal 8, Lab 1, performance 2, camera 9, HUD 3, arsenal rules 4, frame
stats 1. The native sprite-identity comparison corrected a managed-wrapper fixture
failure without changing runtime rendering. LMG now 100 loaded / 200 lab reserve /
8-second reload. Frame overlay statistics/capture verified in Editor only.

First export was Cancelled, not successful, after 690.332 seconds; no APK was
produced. Retry build_369cf4a48eb7 started after tests completed and a valid dry run.
Native pre-retry audit: Warehouse_Demo open/saved/stopped, no missing scene or
inspected prefab scripts, current Console zero errors/warnings, compilation clean.
No runtime/source/asset modifications are permitted during the export.

## Final export and handoff (2026-10-28)

Retry build_369cf4a48eb7 Succeeded in324.143s, zero errors,980 warnings.
APK: build/Arsenal/AKUMU2-arsenal-dev.apk,67,238,173 bytes (about64.1MiB).
SHA256 c6beca846c937fe34dd3abf4e36840529e1287061e0db4b64417d6ab9d8718e8.
BuildReport totalSizeBytes includes additional development outputs; it is not APK
file size. BuildRetryStatus.json preserves raw outcome; ApkVerification.json and
PackagedArsenal.json record ARM64 libraries, lab/frame-overlay/release-fire metadata,
arsenal resources/catalog and Testbed scene packaging. MainMenu remains first.

Warnings are predominantly Sentis compute-shader warnings, plus obsolete Unity API
calls (including FindObjectsSortMode in the lab/test code) and disabled remote
Pipeline in Player because no RuntimePipelineConfig exists. No unrelated shader,
package or tooling changes were made to suppress these. Final current Console has
zero errors and90 build warnings; clean compilation, Editor ready/outside Play Mode.
Warehouse_Demo remains active/open/saved. No missing scripts found in native audit.

All73 selected automated checks passed; no device attached via adb. Installation,
Galaxy A56 60FPS/frame pacing/thermals/multitouch, audio and human weapon/art review
remain required. Implementation/build verification is not gameplay certification.
Git working tree preserved,0staged, no commits/pushes. See FinalGitSummary.json.
