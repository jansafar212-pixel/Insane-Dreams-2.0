# Mobile HUD & explosives expert hotfix

Implemented by Codex - awaiting Android/gameplay playtest verification.

## Changes and measured values

Same controlled Warehouse_Demo startup, resolved UI units (rounded):

| Element | Before | After |
| --- | --- | --- |
| Health panel | 260 x 173 | 220 x 141 (~31% less area) |
| Pause visible face | 124 x 56 | 88 x 40 (~29% smaller dimensions) |
| Pause touch target | 124 x 56 | 124 x 56 |
| Objective panel at startup | 280 x 224 | 224 x 137 (~51% less area) |

Health typography/padding/bar reduced; all health, armor, tape and revival readouts remain. Pause is still upper-center in the safe area with an inset painted label and unchanged Button callback/hit bounds. Objectives retain their full text and progress; the repeated startup notice drops only the flavor sentence. Long text uses a touch-scrollable, height-bounded region (140-unit maximum content height, about 153 including padding), with less opaque background (.88 -> .65). Objective tracking and results are unchanged. Other controls and aiming-stick offsets are unchanged.

| Explosives parameter | Before | After |
| --- | --- | --- |
| Center blast damage | 24 | 31.2 (+30%) |
| Effective radius | 2.7 | 3.2 (+18.5%) |
| Preparation/wind-up | .85 s | .85 s |
| Post-throw recovery | 3 s fixed | 1.05 s, +/-15% (.8925-1.2075 s) |
| Nominal wind-up + recovery | 3.85 s | 1.9 s average (~2.03x frequency) |
| Controlled 28-second real-pool launches | 7 | 13 (~1.86x) |
| Prewarmed projectiles | 2 | 2 |
| Pooled blast renderers/projectile | 35 | 35 |

Native prefabs were saved through PrefabUtility/SerializedObject, not serialized text editing. Radius already drives warning rings and blast particles/glow, so actual damage and visual scaling share the same parameter. Existing damage falloff, wall occlusion, all-team/self friendly fire, randomized short/normal/long fuses and 8% held-malfunction probability remain intact. Initial 1-2 s reaction delay remains. No new manager, scan or effect allocation loop.

Root cause of needless passivity: the old close-range retreat branch returned unconditionally below 3.2 units, preventing any attack, even when retreat was blocked. A ready specialist can now interrupt that repositioning with a throw rather than becoming permanently passive. He still plants his feet for the readable wind-up. Target visibility, camera permission, combat readiness, hurt interruption and pool availability continue to gate attacks. Repositioning resumes between throws.

## Files modified in this hotfix

- Assets/UI/WarehouseControls.uss
- Assets/UI/MissionControls.uss
- Assets/UI/GameFlow.uss
- Assets/Scripts/Missions/MissionHud.cs
- Assets/Scripts/Progression/CampaignMissionBridge.cs
- Assets/Scripts/Enemies/FireworksCombat.cs
- Assets/Prefabs/Enemies/FireworksExpert.prefab
- Assets/Prefabs/Enemies/CartoonFirework.prefab
- Assets/Tests/PlayMode/Hotfix00253Tests.cs (new damage/recovery expectations; checks retained)
- Assets/Tests/PlayMode/MobileHudPyroTests.cs (and Unity-generated meta)
- DEVELOPMENT_LOG.md; MobileHudPyro evidence/report files under TestResults.

Existing unrelated working changes were retained. No staging, commits or pushes.

## Automated/controlled verification

Final unique results: **33 Play Mode passed, 1 Play Mode failed; 7 Edit Mode passed** (40 passed / 1 failed). Full raw results: MobileHudPyro_Regression.json. The failed case was rerun alone and failed identically; it is counted once.

- MobileHudPyroTests: 3/3. Actual blast center damage, newly included 2.9-unit victim, excluded 3.4-unit victim, actual 3.2-unit warning ring, once-only damage and 35-renderer reuse. Seeded 28-second real-pool frequency comparison and valid close-range throw. HUD measurements, no health/pause/objective/stick overlaps at simulated 960x720, 1280x720 and 2340x1080 roots with synthetic safe insets; full long-notice retention, bounded scrollable layout, real pause callback.
- Hotfix00253Tests: 4/4. Blast reuse/damage; existing weapon reload; complete checkpoint/objective/results/save flow; native tuning.
- Sprint0242Tests: 4/5. Real physical arc/landing, wall blocking, held premature failure damaging owner/ally and recovery, held self-defeat passed.
- Sprint0248 targeted all-team/self damage + fuse bucket tests: 2/2.
- GraphicsUiReworkTests: 2/2. Real menu/settings/shop/start/resume callbacks and live health/ammo/slot/joystick bindings.
- Sprint024FlowTests: 12/12, including safe-area pause, progression/results, equipment, healing/revival and double jump.
- Hotfix00253PlaytestTests: 6/6 controlled tests, including crouched-cover blind fire, removed duplicate fire control, Glock origins, Samurai platform jumping.
- CombatRulesTests (Edit Mode): 7/7.

Remaining failed assertion: Sprint0242Tests.ActualHitscanBurstSuppressesSeveralEnemiesWithoutDamageOrForcedPanicThenDecays, line 77: expected first Larper IsDefensivelyCrouching after three shots, observed false. Failed both the group run and isolated rerun. This hotfix does not change that pipeline: all 21 protected gameplay/data/prefab/scene hashes match the prior graphics-rework manifest, including EnemyCombat, LarperAwareness and Larper prefab (MobileHudPyro_Protected.json). The cause is not established here; do not interpret this report as a clean full regression suite. No unrelated suppression changes were made.

One initial layout attempt overlapped health/objective; final layout passes. An initial test cleanup unloaded while paused and logged a destroyed-player reference; the fixture was corrected to resume through CampaignMissionBridge before unloading and passed. No production pause-unload change was made. Final Editor compile successful, stopped; current Console 0 errors / 0 warnings. Historical MCP buffer counters include earlier test diagnostics.

## Manual verification still required

These are Editor simulations/controlled captures, not human gameplay or Android-device verification. Compare MobileHudPyro_Before.png / After.png. On a phone, check notched safe areas, health/readout legibility, pause tapping outside its small visible face, touch scrolling of long objectives, controls at chosen joystick scale, and comfortable battlefield visibility. Fight the specialist near allies/cover: confirm readable wind-up, fair frequency/fuse variation and actual damage; verify self/ally accidents, normal player combat and no unacceptable frame-time spikes with overlapping blasts. No Android build/device profiling or full-level human playtest was performed. Fixed pool sizes were verified; that alone does not prove device performance.
