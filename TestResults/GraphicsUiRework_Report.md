# AKUMU 2 — Menu / HUD graphics rework and corrected0025.3

2026-10-24. **Implemented by Codex — awaiting artistic review, human playtest and Android-device verification.**

## Visual changes

The main menu now uses a static block-pixel AKUMU2 wordmark, industrial warehouse silhouettes, existing crate art, a warning-label hierarchy and the established ink/paper/cardboard/yellow/orange/teal palette. The logo is authored as5×7 glyph strips in UI Toolkit; no font dependency, generated illustration or new character art. Existing readable Unity typography remains for body text. Play/settings/quit retain their callbacks; keyboard focus and shop tab selection have clear states. Settings, level selection, shop, scrollbars, pause, Results and revival share the same flat style, without glossy gradients or3D graphics.

The HUD replaces the dense equipment paragraph with separate live health numbers/bar, armor/tape/revive readouts, weapon sprite/name, large magazine/reserve counts, reload status and selected1/2/M tabs. Low health changes the bar color. Objective/notification content remains connected to the mission; the panel now sits below vitals. Pause remains top-centred in the safe area, enlarged to56UIunits to preserve a physical minimum after pixel rounding. Touch stick dimensions, knob sizing, radius, fire thresholds/hysteresis and response are unchanged; aiming pad stays at right44px (the previously approved16px left shift). Action buttons retain their hit sizes with simpler square styling. Healing/revival/supply behavior is unchanged.

## Corrected control behavior

No literal Fire Secondary control existed; the redundant ordinary FIRE element duplicated the shooting joystick. It is completely removed from both UXML layouts, its field/query/layout logic and its obsolete pointer registrations. Shared firing-intent code and secondary-weapon selection remain. A real touch selection of an equipped sidearm followed by joystick firing consumes that weapon's ammo correctly.

Blind fire now uses **posture.IsCrouched && cover.IsCovered** for visibility and availability:

| State | Button |
|---|---|
| Standing, no cover | Hidden |
| Standing beside cover | Hidden |
| Crouching, no usable cover | Hidden |
| Crouching in usable cover | Visible and functional |

The pointer handler independently checks both conditions. Standing or losing/destroying usable cover hides the control and releases captured firing input. Existing weapon raise, inaccurate blind spread, peek behavior, geometry clipping and independent pointer ownership are retained. This explicitly supersedes the previous follow-up's visible-but-disabled button away from cover.

## Samurai and Glock: preserved, not compounded

The previous follow-up already raised Samurai jump speed13.28→15 (about27.6% theoretical height; controlled actual rise within25–30%) and lowered the low-crate descent gate.9→.3. Those values remain; no second jump-height increase. Existing full warehouse tests still verify visible aggro, persistence, actual platform/crate pursuit, landings and melee. Other archetypes were not edited.

Glock flashes already share the recorded world-space shot origin/direction with tracers through recoil and translation. Player and enemy Glock, left/right/up/down/diagonal aim, and the unchanged carbine behavior were reverified. No weapon balance, reload, damage or projectile changes were made in this rework.

## Files modified

- Assets/Scripts/Progression/GameFlowController.cs — menu composition, pixel wordmark and themed scrollbar labels; existing session callbacks remain.
- Assets/Scripts/Input/WarehouseTouchControls.cs — structured HUD binding, selected tabs, corrected blind-fire rule and redundant FIRE cleanup.
- Assets/UI/GameFlow.uxml and GameFlow.uss — menu/background, paper subpages, buttons, sliders, scrollbars and pause styling.
- Assets/UI/WarehouseControls.uxml and WarehouseControls.uss — shared vitals/equipment/touch presentation.
- Assets/UI/MissionControls.uxml and MissionControls.uss — matching mission HUD and Results.
- Assets/Tests/PlayMode/GraphicsUiReworkTests.cs (+Unity-generated meta) — menu callbacks, isolated save, HUD values, sidearm touch/joystick, layout and captures.
- Assets/Tests/PlayMode/Hotfix00253PlaytestTests.cs — superseded visibility expectations updated; adds cover-destruction input release, retains Glock/jump tests.
- Assets/Tests/PlayMode/Sprint0025Tests.cs — replace obsolete FIRE checks with absence assertion; retain blind-fire behavior.
- Assets/Tests/PlayMode/Sprint022CoverTests.cs — normal fire regression uses the joystick, conditional blind-button layout checks retained.
- Assets/Tests/PlayMode/Sprint0225Tests.cs — removed control excluded from layout list, absence asserted; remaining touch sizes/spacing still checked.
- Assets/Tests/PlayMode/TraversalRegressionTests.cs — ammunition assertions read the new explicit ammo/reserve fields; inventory/reload quantities and touch selection assertions retained.
- ART_BIBLE.md, DEVELOPMENT_LOG.md and TestResults/GraphicsUiRework* evidence.

No scenes, prefabs, character sprites, rigs, historical originals, gameplay controllers, weapon definitions, Samurai tuning or AI source were changed. All21 protected gameplay/data/prefab/warehouse-scene hashes match the start of this task. Existing unrelated uncommitted work is preserved. Nothing staged, committed or pushed.

## Automated verification

**81 unique targeted checks passed:72 selected Play Mode and9 selected Edit Mode tests.** Reruns are deduplicated by FullName. Initial test-fixture failures and final results are retained in GraphicsUiRework.json.

- New real menu test: Play/settings/back; both setting sliders and save; level/preparation; all shop tabs; purchase; mission launch; pause/resume. Quit is rendered/visually inspected and its callback preserved; actual OS application exit was not invoked in the Editor.
- New HUD test: real health/bar, sprite/ammo/reserve data, safe-area/landscape/ultrawide bounds, secondary selection through touch and actual joystick firing/release. Fresh-save fixture explicitly purchases/equips its sidearm through the existing shop; no personal save used.
- Hotfix00253PlaytestTests6/6: both HUD layouts, four visibility states across existing scenarios, dynamic cover destruction while still crouched, pointer release, Glock origin/orientation and Samurai jump/landing.
- Sprint024FlowTests12/12: progression, equipment/purchases, pause/death UI, reward/save/results and existing mission flow.
- Sprint0025Tests8/8, Sprint022CoverTests12/12, Sprint0225Tests8/8, equipment/ammo touch-selection case, MovementCompletionTests11/11, CameraSamuraiHotfixTests6/6 and Hotfix00254Tests6/6.
- CombatRulesTests7/7 and Sprint0244RigTests2/2.

Compilation completed without errors. Final Console ground truth0errors/0warnings; Editor ready and stopped. MainMenu remains open/clean, background execution restored off. No full-project test suite, APK build, Android hardware profiling or unassisted gameplay completion is claimed. Layout/pointer checks are controlled Unity tests, not real Android-device testing.

Visual inspection covered the actual native Unity captures, not a web/mockup substitute:

- GraphicsUiRework_MainMenu.png — pixel wordmark, warehouse backdrop and primary navigation.
- GraphicsUiRework_Shop.png — paper panels, tabs, purchase state and matching scrollbar.
- GraphicsUiRework_Hud.png — live75/100HP,7/123ammo in a controlled temporary session, selected weapon and touch layout.

## Manual acceptance still needed

1. Review the menu/HUD aesthetic against the established game style. Confirm logo readability, button/label contrast, equipment icons and objective text at normal gameplay distance.
2. On Android hardware/notched devices and all joystick scales, check touch comfort, safe areas, portrait/landscape changes, scrolling, pause and Results. Ensure the aim stick still feels the same.
3. Check all four blind-fire states and destroy/leave cover while holding input. No ordinary FIRE duplicate should return. Select a sidearm and fire with the aim stick; verify reload and ammo UI.
4. Play warehouse encounters, ladders, double jump/midair slide, suppression/retreat, healing and revival. Confirm the preserved Samurai jump/aggression and Glock coherence visually during normal play.
5. Verify actual application quit on a desktop build and Android performance. Controlled captures and passing automated checks are not human artistic/gameplay approval.
