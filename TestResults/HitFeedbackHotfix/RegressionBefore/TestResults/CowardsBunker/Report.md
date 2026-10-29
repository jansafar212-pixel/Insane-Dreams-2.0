# Sprint 0028 — Operation: The Coward's Bunker

Implemented by Codex — awaiting human playtest/artistic verification.

## Deliverables

Level 1 remains the existing Unity horizontal introductory operation. The uploaded
brief's Godot wording was treated as an engine-name error; no engine migration.
Fifteen enemies in the same [3,4,3,3,2] groups, opening trio, route length, platform
locations, six ladders, final mission triggers/results and current no-checkpoint
operation design are preserved. No new archetype, wave, UI/menu/shop or character art.

Environment: eleven reusable native coarse-pixel modules, deliberately placed
soil banks, sparse grass, rocks, scraps and tree silhouettes. Flat wall panels
and scuffed platform bands replace the corresponding visuals in place. Four-pixel
cells at32PPU, Point/no mipmaps, binary alpha and at most five opaque colors/module.
Approved crate pixels remain unchanged. One512x512RGBA32 sheet; no new runtime
environment scripts, lighting, particles, decorative colliders or simulations.
Human artistic/device acceptance is still required.

Bunker: existing Officer dais and two cover positions reused. Sandbags at x138
and146, feet1.6; both1.625x1.125 (front previously1.625x.90625; rear1x1).
One solid4.5x.375 timber roof centered(142.75,4.875), cosmetic timber supports,
earth lip and satirical management-shelter sign. Roof ends x145 and rear remains
open. Front firing edge x136 leaves jump run-up. Player can approach by ground,
existing flank platform/ladder, jump the low barriers or throw through the open
rear. Neither the Officer nor decorative scenery grants artificial immunity.

## Officer behavior and reasons

Changes live in the existing OfficerShotTelegraph and EnemyCombat components;
only the Demo Officer has optional bunker references. Other Officers and Larpers
keep their ordinary behavior. No new state machine, manager or pathfinding system.

A heard or recently seen threat on the approach side can prompt a reachable peek
at the authored exposed edge. The normal sampled corridor checks, jump clearance,
combat reservations and brain stuck-recovery continue to own movement. A claimed
peek remains stable through its hop. Existing crouch/reload/panic/suppression
branches retain priority; the anchor does not disclose the player's location.

Near this bunker, after a shot an Inspector-configurable80% chance attempts an
ordinary cover withdrawal. Existing random hide durations (Officer1.2–2.4s),
recovery timeouts, proximity retreat and pressure behavior remain; failed attempts
fall back to normal repositioning. The front bunker cover uses configurable.65
stand-off instead of generic.08, providing run-up for a safe return hop. All other
cover slots keep.08. Cover-hold exit retains the existing uncertain sound memory
for1.5s for this Officer, as it already does for panic-escape Larpers, allowing
re-engagement after hiding without omniscience or permanent concealment.

Desert Eagle balance unchanged:55damage, seven-round magazine,2.8sreload,
1.25–4degree base spread, .06s aim sampling/360deg/s tracking, .5swarning with
.2scommit, distinctive magenta heavy tracer. Existing solid-obstruction, viewport
permission, ammunition, reload, suppression and hit-reaction rules remain.

Initial geometry was too low over the front hop arc; the existing navigation
correctly rejected it. Shortening the roof behind that arc solved entry. Traces
then exposed that the generic close cover slot lacked room to hop out; the
scene-specific stand-off solves departure. No collision checks were bypassed.

## Changed files

- Assets/Scripts/Enemies/EnemyCombat.cs
- Assets/Scripts/Enemies/OfficerShotTelegraph.cs
- Assets/Scenes/Warehouse_Demo.unity (native Editor authoring only)
- Assets/Editor/CowardsBunkerSetup.cs and Unity metadata
- Assets/Art/CowardsBunker/Environment.asset and Unity metadata/folder
- Assets/Tests/EditMode/CowardsBunkerRulesTests.cs and metadata
- Assets/Tests/PlayMode/CowardsBunkerTests.cs and metadata
- ART_BIBLE.md, GRAPHICS_BIBLE.md, DEVELOPMENT_LOG.md, SPRINT_RESUME.md
- TestResults/CowardsBunker/ evidence, brief, checkpoint and test scripts/results

Many preexisting modified/untracked files belong to earlier sprints; GitBefore.txt
records that baseline. No stage, commit or push.

## Validation and limitations

Final unique results are recorded in Regression.json. New checks use actual Demo
actors and geometry: heard opportunity, .5swarning, two real shots separated by
reaching/crouching in cover, no positional snapping, real bullet obstruction and
rear-flank defeat/corpse, pressure decay, actual player crossing the barriers,
physical grenade roof collision, frontal blast occlusion and open-rear frag damage.
Edit checks compare all original terrain/ladder/crate collider positions and sizes
except the two intended sandbags; verify roster/supports, zero decorative colliders,
all environmental sprites assigned, coarse binary pixel grid and palette limits.
Optional runtime weapon overlays are correctly excluded from environment checks.

Existing Officer shot/laser/reload/weapon-animation, ladder, pause, Medical shop,
level route/results, Samurai traversal, throwable/ammo-box/HUD/save and fall/
parachute/double-jump checks were exercised. Consult final results for counts.
The fireworks ground-approach6second test failed once during a combined run and
passed in isolation; source and terrain in that arena were not changed. This is
reported as an intermittent timing/behavior observation, not silently rebalanced.

Controlled gameplay-camera images Bunker.png, Opening.png, Storage.png and
OfficerTelegraph.png were inspected. OfficerTrace.json records movement,
visibility, cover, crouch, warning and shots. These are automated controlled
scenarios, not human gameplay or real Android performance verification.

Unity became unresponsive during authoring; work was checkpointed and resumed
when the Editor reconnected. No forced process termination or discarded unsaved
work was performed by Codex. Native roof/run-up corrections saved after recovery.

## Manual playtest

1. Launch Warehouse from the main menu; compare entrance/storage/command artwork
   at phone size, including layering, pixel crispness and visual distraction.
2. At command, fire from the ground approach. Observe the Officer leave shelter,
   visibly warn, shoot, retreat/crouch and re-emerge with varied timing. Advance
   and flank to check evasiveness; do not expect a fixed action script.
3. Fire at the sandbags/roof; approach from the open rear and throw frag around
   cover. He must remain defeatable. Test overhead grenades and nearby cover
   interactions, especially jumps at either barrier.
4. Use the existing office flank ladder with held-up top exit. Defeat the Officer
   and reach dispatch/results. Recheck reloads, supplies, healing and mobile controls.
5. Profile normal and sustained firefights on Android. Static modules/one collider
   and bounded existing tactical samples are lightweight, but device performance,
   artistic acceptance and encounter feel are unverified. No new APK was built.

Final unique automated result: **36/36 passed: 31 Play Mode, 5 Edit Mode**. The fireworks check also passed in the final full six-case FirstOperation rerun.

Final native Editor inspection: Warehouse_Demo open and clean (saved), Play Mode
stopped, compilation successful, zero missing scripts, roster[3,4,3,3,2], zero
colliders on added earth/vegetation scenery; cover stand-off.65 and peek x136.
Current Console has zero errors and8warnings. Captured warning history includes
older obsolete-API notices and Unity Account API connectivity warning; these were
not changed as unrelated cleanup. FinalEditor.json/FinalConsole.json/Warnings.json.
