# AKUMU 2 — Sprint 0027.1

Implemented by Codex — awaiting hands-on playtest verification.

## Focused fixes

Officer inherited Larper spread3°→40° and sampled aim every.18s. Officer prefab only now uses1.25°→4°, .06s aim sampling and360°/s turning instead of240°/s. Aim is calculated from the actual muzzle toward the last visually observed collider center (including crouched posture offset), rather than root-to-root parallel aiming. Offset is cached only on visible observations; existing LOS, uncertainty, reaction delay and final direction commitment remain. No target prediction/omniscience. Telegraphed shot retains exactly.5s warning/.2s lock, magenta width.08,55damage,7rounds,2.8s reload, cowardice and cadence. Larper settings unchanged.

Desert Eagle's brown grip pixels replaced with flat charcoal-black (38,42,46). Existing27x14/32PPU gun/pivot/muzzle retained. Magazine3x5→2x4; socket(-2,-3)→(-1,+1) native pixels. Inserted magazine is behind the opaque handle, while pulled/replacement/dropped magazines retain existing animation/physics. Native asset objects/GUID references preserved; no body, joint or rig redesign. Existing reload time/ammo ownership unchanged.

Revised platforms place their top exactly at the player's feet when the ladder marker is reached. Old logic tested the exit.05units early and stopped vertical motion if the horizontal path failed, trapping feet below the ledge. Baseline actual FireworkLoadingLadder stopped atcenter3.870/feet3.170 against3.200 platform. Reaching the exact marker also exposed contact-skin edge blocking. Now climb normally to the marker plus configurable.04clearance before checking the existing full-body safe horizontal exit. No teleport, scene/trigger repositioning, global physics change or new navigation owner. Existing landing support/obstruction checks, motor/gravity restoration, .3s lockout plus release requirement preserved. All6 revised ladders tested upward/downward with held touch input and grounded automatic exit.

Pause face88x40→52x40; top-center position, pause icon,124x56 invisible hit area and functionality preserved.

One visible Medical tab now contains Duct Tape then Faintanyl. Removed Duct Tape's separate tab; neither appears under unrelated categories. Tape title corrected from "Faintanyl & Duct Tape / 50" to exactly "Duct Tape". Faintanyl title/description retained. Existing resource enum/offer IDs preserved for purchases/save compatibility. Tape remains$10/50points (cap200), Faintanyl$75/1 (cap2); both purchases verified against real inventories and save.

## Files

Runtime: Assets/Scripts/Enemies/EnemyCombat.cs (Officer muzzle-based cached observed center); Assets/Scripts/Traversal/PlayerTraversal2D.cs (top clearance/endpoint); Assets/Scripts/Progression/GameFlowController.cs (Medical grouping); Assets/UI/GameFlow.uss (pause face width).
Native assets: Assets/Data/FirstOperation/Officer.prefab, OfficerPistol.asset, DesertEaglePixels.asset, DesertEagleMagazinePixels.asset, DesertEagleMagazine.asset; Assets/Data/Progression/DemoCatalog.asset. Native authoring: Assets/Editor/Sprint00271Setup.cs.
Tests: new PlayMode/Sprint00271Tests.cs and EditMode/Sprint00271RulesTests.cs; existing Sprint005Tests touch gestures scaled to current joystick size and reaction check waits for the actual existing state; PostPlaytestHotfixTests/MobileHudPyroTests updated for explicitly requested Medical/pause presentation changes. No gameplay tuning hidden in test repairs.

## Verification

**39/39 unique focused tests pass:38Play Mode,1Edit Mode (Regression.json).** New cases already pass: all6 real ladders up/down/automatic exit/no reattach,6of6 actual seeded hitscan shots at10units against stationary player-size target, narrower pause functional/hit area preserved, exactly2Medical items and real purchases/save. Edit check verifies every inserted magazine pixel fits inside opaque pistol silhouette and damage/ammo/reload stay unchanged. Officer regression4/4 and airborne ladders9/9 pass. Remaining focused regressions all pass:11ladder/touch/combat checks,6fall/parachute checks, cumulative detachment, existing Medical purchase/cap/persistence, safe-area pause, actual Officer direction lock and evasive movement. Initial failures retained separately from latest results.

Controlled Officer recoil/reload captures copied here and inspected in both facings. These are rendering/test captures, not human gameplay or Android hardware verification. No APK rebuild requested or performed for0027.1; prior0027 APK does not include this hotfix. Export a new build from the updated project for device testing.

ProtectedCurrent.json verifies ragdoll source, cumulative body damage source, ragdoll prefab and warehouse scenes byte-identical. No detachment thresholds/frequency/impulse/joints altered. No level layout, geometry or spawn distribution changes. No staging/commit/push.

## Manual playtest

- Strafe/jump against Officer: closer shots should threaten, while his.2s final lock remains dodgeable. Check retreat/cover, black grip, seated magazine, both-facing firing/reload.
- Hold upward on each of the6ladders; confirm smooth grounded top exits, downward re-entry and airborne grabbing on PC and actual Android joystick.
- Tap slimmer top-center pause icon, including edges of invisible hit area; resume normally.
- Open Medical: Duct Tape then Faintanyl, no separate top-level tabs/duplicates; spend and use both resources, preserve caps and save.
- Confirm existing exaggerated detachment/ragdolls still feel unchanged. Device feel and final artistic acceptance remain user playtesting.

## Final state

Compilation complete without errors; current Console has no errors or warnings. Warehouse_Demo open/saved/clean, Play Mode stopped,0missing scripts. Terrain/scenery and encounter counts[3,4,3,3,2] unchanged. ProtectedCurrent.json confirms both warehouse scenes and the ragdoll/region source/prefab are byte-identical. Preexisting dirty work preserved; index empty, nothing staged, committed or pushed. Controlled captures inspected: MedicalShop.png, RecoilRight/Left.png and ReloadRight/Left.png. They demonstrate presentation under controlled conditions, not human artistic approval or Android-device gameplay.
