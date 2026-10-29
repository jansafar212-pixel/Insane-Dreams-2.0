# Insane Dreams 2.0 — Development log

## Sprint 002 — Warehouse testbed (2026-10-18)

Open `Assets/Scenes/Warehouse_Testbed.unity`, press Play and focus the Game view.
The original `Prototype_01.unity`, `Assets/Prefabs/Player.prefab` and all three
Sprint 001 scripts are preserved unchanged. No commits or pushes were made.

### Level and controls

- Bounds: x -28 to 28, y -9 to 20. Ground surface y 0, basement y -7,
  mezzanine y 5.4, roof y 12. Palette: green ground, blue basement, amber walks.
- Route A: entry at x -24 to exit at x 24; jump over crates and two shaft gaps.
- Route B: climb the yellow shafts to the mezzanine/roof, jump the upper steps,
  and return using a ladder or parachute. Move sideways with vertical input
  released to leave a ladder at an intermediate floor.
- Route C: descend the left shaft, cross the basement, jump the low pipe bridge,
  then ascend the right shaft. Both shafts reconnect the floors.
- Keyboard: A/D or arrows move; W/S or Up/Down climb; Space jumps or jumps away
  from ladders; hold Left Shift for a parachute while falling.
- Touch/mouse: left stick moves/climbs; right stick directs a harmless pink aim
  marker; Jump is a press, Chute is a hold. Mouse dragging supports Editor tests.
- Restart Play Mode to reset the level. Falling below y -11 respawns at the most
  recently activated beacon. Checkpoints are entry, mezzanine and exit.

### Components and input boundary

- `Assets/Scripts/Prototype/PlayerMotor2D.cs`: original walking, jumping and
  grounded detection, unchanged. No double jump or new movement model added.
- `Traversal/LadderVolume`: reusable trigger, alignment speed and configurable
  top dismount offset. No ladder-name lookups or scene-specific traversal code.
- `Traversal/PlayerTraversal2D`: ladder overlap query on climb intent, gravity
  ownership, climbing, side/top exits, jump-away, reattachment delay/release latch,
  fall-only parachute and complete recovery reset. Temporarily disables the
  original motor while climbing, then returns motion ownership to it.
- `Input/PlayerActions`: shared Move, Climb, Jump, Parachute and Aim actions.
  Reads the installed Input System keyboard backend and combines it with touch
  setters. Calls the existing motor/traversal components, independent of UI.
  The original DesktopPlayerInput is disabled only on the warehouse variant.
- `Input/WarehouseTouchControls`: UIDocument pointer handlers with a separate
  captured pointer per stick, hold button handling, release/cancel/capture-loss
  cleanup, focus/pause cleanup and screen-safe-area layout.
- `Camera/WarehouseCameraFollow`: LateUpdate smoothing over the interpolated
  body, orthographic framing, aspect-aware bounds and immediate recovery snap.
- `Checkpoints/CheckpointMarker`: trigger beacon with a local SafeSpawn child.
- `Checkpoints/PlayerRecovery`: checkpoint location, fall threshold, velocity/input/
  ladder/parachute reset and camera snap. No health, inventory or persistence.
- No GameManager, singleton, new package, global input-setting change, navigation
  framework or Android build-setting change. Trigger/cast buffers are reused;
  runtime gameplay does not search the scene per frame.

### Reusable assets and authoring

- Player variant: `Assets/Prefabs/Player/WarehousePlayer.prefab`. In a new scene,
  bind PlayerRecovery's initialSpawn and followCamera fields on the instance.
- Ladder: `Assets/Prefabs/Interaction/Ladder.prefab`. Duplicate an instance;
  position it in a clear shaft and scale Y to adjust height. Keep its
  BoxCollider2D a trigger. Solid floors must have a gap around the ladder.
  Extend its top above the upper deck by at least the player's half-height.
  Configure Top Exit Horizontal Offset toward a clear adjacent landing (negative
  for a left exit). Release vertical input and move sideways for an intermediate
  dismount. Hold up at the top to dismount automatically onto that landing.
- Platform: `Assets/Prefabs/Environment/Platform_Unit.prefab`. Scale/configure the
  rectangular static collider and renderer; keep walkable solids on Default.
- Checkpoint: `Assets/Prefabs/Interaction/Checkpoint.prefab`. Place the beacon
  near a safe floor and adjust its SafeSpawn child clear of solids. The trigger
  automatically finds PlayerRecovery on the entering Rigidbody2D.
- UI: `Assets/Prefabs/UI/WarehouseControls.prefab`, `Assets/UI/WarehouseControls`
  UXML/USS and WarehousePanel.asset. Bind the instance's Actions reference to the
  player. Include one EventSystem with InputSystemUIInputModule in the scene.
- LevelMarkers contains PlayerStart and five empty EnemySpawn markers, separate
  from Environment. No enemy behavior is attached.
- All visuals reuse Sprint 001's square sprite/unlit material. Original artwork
  was not imported or changed; the canopy and aim marker have no colliders.

### Verification and limits

- Unity MCP inspected live scene hierarchy, player/prefab, trigger sizes, physics,
  camera and UI references. Scripts compile; no sprint runtime errors observed.
- Continuous Route A test reached x 24.13 from x -24 in 7.27 game seconds, with
  five ground jumps across the crates and both shaft gaps.
- Ladder tests: ascended from the basement through an intermediate floor, held
  position, descended, jumped away and restored gravity/motor ownership.
- Holding up at all three ladder tops dismounted onto their adjacent landings:
  (-10.2, 12.72), (13.8, 6.11), (24.8, 12.72), motor enabled, gravity 3.
- Basement route test descended the left shaft, crossed the lower passage,
  jumped the pipe bridge and ascended the right shaft to (13.8, 6.11). The
  harness aligned the player at the right ladder before ascent.
- Parachute test reduced a -15 falling velocity to approximately -2.9, restored
  gravity 3 and faster falling on release. Subsequently tightened the cap to
  compensate for the following physics gravity step; a final test measured
  -2.80 with the configured speed 2.8.
- Mezzanine checkpoint activated at (0, 6.25); forced fall below the level
  returned the player to that floor with zero velocity, chute off, climbing off,
  and the camera snapped back. Queued jump was cleared during recovery.
- Synthetic UI pointer tests exercised diagonal movement (0.59, 0.59), release
  to (0, 0), diagonal aiming (0.71, 0.71), Jump and Chute hold/release.
- Simulated focus-loss callback cleared a captured movement stick and held
  parachute, returned Move to zero and restored normal gravity.
- Final transition test: falling with chute active (gravity 0.6, velocity -2.8),
  climb intent engaged the ladder (gravity 0, canopy hidden), then jump-away
  restored normal gravity 3 without deploying the chute during ascent.
- Camera viewport bounds passed 16:9, 20:9 and 9:16 checks at opposite corners.
- Game-view inspection confirmed clear geometry, player, ladder and touch UI.
- Saved warehouse reloaded successfully with zero missing scripts and intact
  player variant, spawn, recovery camera, follow target, aim and UI references.
- Original Prototype_01 regression test fell/landed, jumped to y 3.04, landed
  again at y 0.71, remained grounded and retained zero rotation.
- Tests used live Play Mode and runtime repositioning for individual mechanics.
  Real Android multitouch, interruptions/rotation on a device, target framerate,
  full manual traversal of every elevated jump and physical-keyboard feel still
  need human testing. Mouse/synthetic events are not device validation.
- Android build support is unavailable in this Editor. No modules were installed
  and no Android build was attempted. Existing Unity AI Account API warnings are
  unrelated to traversal; authentication was not modified.
- Background simulation was enabled only during automated Play Mode testing.
  Camera and actor repositioning for tests were not saved into the scene.
- Final state: Warehouse_Testbed is open, saved, not dirty, and Play Mode stopped.
  Console reports no compilation failure or errors. SHA-256 comparison against
  the pre-sprint baseline found no changes to any pre-existing tracked file or
  Assets file. Existing Sprint 001 project-setting diffs and untracked work were
  preserved. All Sprint 002 work is new files and remains uncommitted.

### Future 2D navigation recommendation

Author a small surface graph later: walk edges along rectangular decks, directed
jump/drop edges between reachable ledges, and ladder edges referencing LadderVolume.
Filter graph edges by reusable capabilities (walk, climb, jump, drop), so a climbing
Larper can flank while a ground-only enemy ignores ladder edges and a stationary
enemy needs no path follower. Feed chosen motion through the same motor/traversal
boundary rather than importing player keyboard/UI logic. Keep spawn/archetype data
separate from geometry; do not infer behavior from marker names or item indices.
No full navigation implementation is justified before enemy locomotion is tested.

## Sprint 003 — Joystick jumping, loadout and first Larper (2026-10-18)

This section supersedes Sprint 002's jump-button controls and manual reference
binding instructions. Its historical notes above remain intact.

### Working controls and input arbitration

- The dedicated mobile Jump element, USS selector and event bindings are removed
  from the shared warehouse UXML used by both the scene and controls prefab.
- JoystickJumpGesture is a serializable input policy owned by PlayerActions:
  upward Y >= 0.65 consumes one gesture; Y <= 0.25 rearms; |X| >= 0.45 counts
  as a deliberate diagonal. All three thresholds are Inspector-adjustable.
- Away from ladders, upward/diagonal input requests the original grounded jump.
  Holding up never repeats, including after landing. An airborne attempt is
  consumed and rejected by the motor, not buffered until landing.
- A predominantly vertical upward gesture inside a usable ladder volume favors
  climbing. A grounded diagonal favors jumping and suppresses ladder attachment
  until neutral. Attached climbing consumes upward gestures without jumping.
  Existing top-exit/release latches prevent a held gesture jumping at an exit.
- PlayerTraversal2D exposes a reusable usable-ladder query, but contains no new
  joystick policy, inventory, combat or enemy intelligence. Its old redundant
  overlap set was removed. Input queries the ladder only on a new upward decision.
- Keyboard remains A/D or arrows, W/S or arrows to climb, Space to jump,
  Left Shift to parachute. Keys 1/2/3 select firearm 1/firearm 2/melee.
- Aim persists independently from movement. The right stick updates AimDirection
  and a FireHeld intention while deflected; no consumer fires anything. Rendering
  still uses the harmless marker, never an authoritative combat controller.
- WarehouseTouchControls keeps separate captured pointer identities for both
  sticks and the chute. Second fingers cannot hijack an occupied stick.
  PlayerActions.InputReset releases UI ownership on recovery as well as focus,
  pause, cancel/capture loss and disable. Stale held pointers cannot resume input.
- The HUD reads PlayerLoadout and exposes three selection buttons. It never owns
  or writes ammo counts. Safe-area inset math and landscape layout have tests.

### Inventory and definitions

- New `Data/DefinitionAsset` provides stable canonical GUID identities. OnValidate
  creates only missing IDs; duplicating assets requires assigning a fresh ID.
  `Insane Dreams > Validate Definitions` and an automated test detect invalid or
  duplicate IDs. No definition assets are mutated for runtime inventory state.
- `Inventory/ItemDefinition` stores name, description, icon and category.
  `WeaponDefinition` adds firearm/melee category, damage, firing interval,
  capacity, reload duration and an optional presentation prefab. These are data
  fields only, not an implemented shooting/reloading system.
- `RuntimeWeapon` owns a unique instance GUID, magazine ammo and reserve ammo.
  Melee instances always have zero ammunition and unlimited uses. WeaponSnapshot
  transfers only definition/instance identities and counts; restoration requires
  the matching resolved definition, never a transient scene-object identity.
- `PlayerLoadout` explicitly owns Firearm1, Firearm2 and Melee slots; supports
  equip, unequip, selection and empty slots. Switching preserves each instance.
- New assets: `Assets/Data/Items/{WarehouseCarbine,BackupPistol,PracticeBaton}.asset`.
  Initial firearm magazines are 24 and 8, each with 36 reserve; melee uses no ammo.
  The initial loadout is configured on WarehousePlayer.prefab. No weapon art,
  projectiles, damage, reload animation, loot, save-to-disk or crafting was added.

### Larper, spawning and entry binding

- `Enemies/EnemyArchetype` has a stable identity, prefab, explicit capability flags
  (walk/climb/jump/drop/stationary), patrol width, movement input and blocked pause.
- `Enemies/LarperBrain` is independent from the player, reuses PlayerMotor2D,
  probes floors/walls/ledges, pauses/reverses when blocked, and performs permitted
  obstacle/gap jumps with a safe landing check. It exposes Unconfigured,
  Stationary, Walking, Jumping, Falling and Blocked states. No combat exists.
- The current archetype enables Walk and Jump. Climb is represented in the data
  but an enemy ladder-route decision policy is deliberately not implemented yet;
  the demonstrated second locomotion action is jumping. Drop permission allows
  walking off ledges; stationary overrides locomotion. No navigation graph added.
- `Enemies/EnemySpawnPoint` resolves an archetype and spawns once; its Inspector
  shows missing/invalid reference warnings. All five old empty markers are typed.
  Only the ground marker at x -7 spawns on start, under Enemies. Others are
  configured but disabled for automatic spawning. Geometry owns no encounter code.
- Assets: `Assets/Data/Enemies/Larper.asset`, `Assets/Prefabs/Enemies/Larper.prefab`.
  The orange placeholder actor has no player-input scripts; its collider excludes
  the actors' shared layer so it cannot trap the player. No contact damage exists.
- `Level/LevelEntry` explicitly binds player, spawn, bounded camera and UI in
  Start after actor/UI Awake/OnEnable. Recovery, camera and controls have small
  Bind methods. Unbind clears input/camera/UI references. The warehouse's old
  fallback scene references are cleared: LevelEntry is the binding source.
- An isolated Play Mode test rebinds the same player/camera/UI between two spawn
  configurations. No second campaign scene or persistent singleton was added.
- Runtime scripts now have `InsaneDreams.Runtime.asmdef` so independent Edit/Play
  Mode test assemblies can reference them. Existing MonoScript GUIDs are retained.

### Executed verification

- Baseline Unity discovery found zero tests. Added and executed 8 Edit Mode and
  12 Play Mode tests, all passing on the final code. Exact results are stored in
  `TestResults/Sprint003.json`. No skipped or inconclusive tests in the final run.
- Edit coverage: gesture hysteresis/held transitions, empty/melee slots, both
  firearms, state retention and definition immutability, snapshots, identity and
  capability validation, authored unique IDs and left/right/top/bottom safe insets.
- Play coverage: grounded/diagonal jumping, airborne rejection, neutral rearm,
  no held-up landing jump, ladder priority/top exit/disable/destruction, gravity,
  parachuting, recovery, independent aim/fire intentions, four distinct synthetic
  Touch identities, hijack prevention, independent release/cancel/focus/recovery,
  inventory HUD, two landscape layouts, level rebinding, typed spawning,
  ground-only ledge safety, stationary restrictions and permitted Larper jumping.
- UI tests inject Unity Touch identities into UI Toolkit; they are not a claim
  about Android OS/Input System device delivery. The live InputSystemUIInputModule
  reports SingleMouseOrPenButMultiTouchAndTrack pointer behavior.
- Live warehouse: the complete ground route passed using only joystick jump
  gestures: five diagonals, x -24 to 24.04 in 7.28 game seconds. No jump-button
  event or direct Jump call was used by that route harness.
- Live Larper: exactly one spawned; sampled states included Blocked, Walking,
  Jumping and Falling, with X -14.14..0.11 and peak Y 3.04.
- Final startup with fallback references cleared verified bound level entry/UI,
  correct camera target, initialized 24-round first firearm and one enemy.
- Final Prototype_01 live sanity check also jumped to Y 3.04, landed at Y 0.71,
  became grounded and retained zero rotation. Its scene, base prefab and original
  motor/input/camera source files remain byte-for-byte unchanged from baseline.
- Compilation succeeds. One early MCP run request failed during import with a
  network/main-thread timeout, before a test run started. After imports settled,
  all complete suites ran successfully; no code error was hidden by that retry.
- Editor-only observation over 3,147 sampled frames: mean Time.unscaledDeltaTime
  1.91 ms; Profiler.GetTotalAllocatedMemoryLong snapshot 1,085,746,899 bytes.
  These include the Editor/test environment and are NOT Android frame-time,
  game-only memory, or device performance claims.
- Android Build Support remains unavailable for Unity 6000.6.6f1. No active target,
  Android settings, installed modules, credentials or security controls changed.
  No APK, hardware test or physical Android performance measurement was performed.

### Manual Android validation still required

1. In Unity Hub, add Android Build Support plus SDK/NDK/OpenJDK matching the
   installed 6000.6.6f1 Editor. This is a user action, not done by Sprint 003.
2. Connect an Android device with Developer Options/USB debugging enabled and
   approve its USB prompt. Confirm it is listed by `adb devices -l`.
3. Create an Android Build Profile including Warehouse_Testbed, choose landscape,
   enable Development Build and profiling, then Build And Run. Record device
   model, Android version, build date/settings and the uncommitted source snapshot.
4. Hold the left stick while operating the right stick and chute with other
   fingers. Release/cancel fingers independently. Check that none steals movement.
5. Test straight-up and diagonal jumps, hold up across landing, neutral/rearm,
   airborne rejection, vertical ladder entry and grounded diagonal near ladders,
   intermediate/top exits and parachuting. The stick should never auto-bounce.
6. Test both landscape orientations, display cutouts and gesture-navigation bars.
   Confirm sticks/chute/equipment controls remain visible and reachable.
7. Interrupt held touches with Home/app switch, notification overlay and screen
   lock; return and verify neutral input. Recovery-pointer behavior is automated
   in Editor; on-device forced recovery needs a development debugger/test hook
   calling PlayerRecovery.Respawn (there is no shipped debug respawn button).
   If that hook is unavailable, record device recovery testing as pending.
8. Profile several minutes of traversal with the Larper active. Record CPU/GPU
   frame times, allocation/GC and memory observations, thermal changes and errors.
   Mouse tests and injected Editor pointers do not replace any of these checks.

### Review notes and Sprint 004

- No known failing automated tests or sprint gameplay exceptions remain. Physical
  touch feel, device lifecycle delivery, safe cutouts and performance remain open.
- The shared motor still has Player/Prototype names despite actor-neutral behavior;
  avoid risky renaming until asset-migration tests exist. Top dismount offsets
  require correctly authored clear landings. Enemy climbing still needs an authored
  route/capability policy rather than unconditional ladder engagement.
- Begin Sprint 004 with device validation. Then add a small weapon-handling slice:
  equipped definition + runtime weapon state, independent aim/fire intentions,
  one firing implementation and explicit ammo/reload transitions. Keep projectile/
  hitscan execution, damage, presentation and enemy decisions separate.
- Add one capability-filtered ladder route for Larpers before a universal graph.
  Do not put inventory, firing or enemy decisions into PlayerTraversal2D.
- All work remains uncommitted. Prior sprint content/settings are preserved except
  the documented warehouse, player-variant, input/traversal/binding and UI updates.

## Sprint 004 — First warehouse combat (2026-10-18)

### Play and control scheme

- Open Warehouse_Testbed and press Play. Existing movement, joystick-up jumping,
  ladders, parachuting and slot selection remain. No jump button was reintroduced.
- Two thumbs: left stick moves/jumps/climbs; right stick aims in any direction.
  Its inner zone is aim-only. Push to magnitude >= 0.85 to fire; relax to <= 0.70
  to stop firing. Thresholds are serialized, with hysteresis to avoid edge chatter.
  This intentionally replaces Sprint 003's lower intent-only firing threshold.
- Reload has a large button next to the aim area, above Chute. Briefly release
  the right stick to reload or tap 1/2/M on the equipment panel. No permanent
  third finger is needed for movement, jumping, aiming and firing.
- Desktop: hold RMB to aim at the mouse, hold LMB while RMB is held (or F) to
  fire. R reloads; 1/2/3 selects slots. A/D, W/S, Space and Shift remain intact.
  Ordinary left-click UI interaction alone does not fire. With no mouse aiming,
  F fires along the last independent aim direction.
- Select M/3 and use the same fire gesture for a short melee strike. It has a
  cooldown and unlimited uses. It is currently a narrow forward ray, not an arc.

### Combat architecture and rules

- `Combat/WeaponCycle` owns deterministic cadence/reload transitions for each
  RuntimeWeapon. Callers supply game time, enabling exact deadline tests.
  Ammo remains on RuntimeWeapon, never on the HUD or ScriptableObject.
- Firearm ammo decreases once when an attack is accepted. Empty, reloading,
  invalid-direction and cooldown-rejected attempts consume nothing. Cooldowns
  persist on the individual instance across switching; no input resets them.
- Reload transfers reserve rounds only when its deadline completes, limited by
  magazine space and available reserve. Interrupted reloads transfer nothing.
  Switching/unequipping, recovery, focus/input reset or disable cancels the
  player's reload. Merely releasing fire does not cancel it. Empty magazines
  require player reload input; enemy reloads are automatic.
- `Combat/AttackExecutor` uses buffered 2D raycasts for both hitscan firearms and
  short melee. The nearest non-self solid collider blocks the attack, including
  crates, floors and walls. Only that first hit can receive damage. Hitscan avoids
  fast-projectile tunneling and per-shot GameObject allocation in this first slice.
  A short-lived LineRenderer is placeholder shot/strike feedback, not authority.
- WeaponDefinition now supplies configurable firearm/melee range in addition to
  existing damage, cadence, capacity and reload duration. The carbine deals 20,
  the pistol 14 and the baton 25 damage. Their existing capacities remain 24/8/0.
- `Combat/Health` provides configurable maximum/current health, faction filtering,
  source-bearing DamageInfo, finite/positive damage validation, damage/defeat
  events and a defeat latch set before callbacks. Friendly/self hits are rejected
  by default. Neutral objects can also use it; no Larper assumption is embedded.
- `Combat/PlayerCombat` consumes PlayerActions intents and selects runtime weapons.
  PlayerLoadout emits SelectionChanging so even same-frame switch-away-and-back
  cancels reload without losing ammo or resetting that instance's cooldown.
- PlayerRecovery exposes Recovered; `Combat/PlayerHealthRecovery` restores health
  and grants two seconds of protection after checkpoint/fall/defeat recovery.
  Inventory instances and their ammunition persist; recovery does not refill them.
- `Combat/DeactivateOnDefeat` makes a defeated enemy inactive. Restart Play Mode
  to reset the encounter. No ragdoll, gore, complex animation or enemy respawn loop.
- HUD observes equipped item, magazine, reserve, reload status and player health.
  It does not modify authoritative combat state.

### Enemy engagement and ladder route

- `Enemies/EnemyCombat` is separate from locomotion and attack execution. Explicit
  LevelEntry -> EnemySpawnPoint target binding avoids per-frame scene searches.
  Range and geometry line-of-sight determine engagement; losing detection returns
  control to the existing LarperBrain patrol. It aims independently and uses the
  same executor/cycle as the player, including cooldowns and finite ammo/reload.
- New `Assets/Data/Items/LarperPistol.asset`: damage 5, interval 0.8 s, magazine 6,
  reload 2 s, range 14. EnemyCombat interpolates 2-degree close spread toward
  16-degree distant spread over its configurable detection range. Close accuracy
  is more dangerous than its weak, inaccurate distant fire; no special-case damage.
- `Enemies/AuthoredLadderRoute` is one explicit ladder link plus target height,
  approach tolerance and timeout. It drives PlayerMotor2D/PlayerTraversal2D while
  temporarily suspending patrol, then returns control. Disabled/destroyed or
  unavailable links release traversal instead of leaving motion owned indefinitely.
- The warehouse now spawns two enemies: the original ground patrol and one at
  (12, -5.8), assigned to the right basement/mezzanine ladder (top 6.6). The other
  typed markers remain inactive. The route is ascent-only; no universal graph.
- Updated the warehouse player and Larper prefabs, scene entry encounter list,
  item tuning, combat material and UI. Core movement/traversal implementations
  were not rewritten; Prototype_01 and the original player prefab remain intact.

### Executed tests and live evidence

- Before changes: the existing 8 Edit Mode + 12 Play Mode tests passed. The initial
  Editor was paused in Play Mode; a stalled first attempt was cancelled, the Editor
  stopped, and the successful baseline rerun recorded. One post-import invocation
  later returned HTTP 400; settled discovery and subsequent suites completed.
- Final run: **15 Edit Mode + 20 Play Mode tests passed**, zero failed/skipped/
  inconclusive. All original 20 remain, unchanged. Added CombatRulesTests and
  CombatIntegrationTests. Exact result/timing data: `TestResults/Sprint004.json`.
- New coverage includes cadence/rapid rejection, depletion/empty behavior, exact
  reload deadlines/conservation/cancellation, immutable definitions, melee limits,
  damage filtering/single defeat, geometry blocking, independent movement/aim/fire,
  same-frame switching, player death recovery with inventory retention, enemy
  engagement/return to patrol, authored ascent/unavailable-route cleanup, distinct
  touch pointers, aim-only versus outer-fire zones and the large reload button.
- Live warehouse controlled encounter (actors repositioned for clear engagement):
  enemy fired four shots and player health reached 80; three player carbine shots
  reduced enemy health 50 -> 0, magazine 24 -> 21. Enemy became inactive while
  the player's movement motor stayed enabled. No direct attack invocation was
  used by that live harness: it fed aim/fire through PlayerActions.
- The second actual warehouse Larper completed its ladder route and returned to
  upper patrol, observed at (7.75, 6.11). Both enemies bind/spawn on normal startup.
- Live pistol depletion/reload: 0/36 -> reloading -> 8/28, conserving remaining
  rounds. Switching back retained the carbine's 21 rounds. Forced defeat returned
  the player to (-23, 0.85), health 100, fire false, same runtime weapon instance
  and both weapons' ammunition unchanged. These runtime test changes were not saved.
- Original Prototype_01 live jump sanity passed: peak Y 3.04, landed at Y 0.71,
  grounded, zero rotation. Final warehouse reopened, bound correctly, showed two
  enemies, and displayed the moved reload button/HUD without critical overlap.
- Compilation succeeds; final scene has zero missing scripts and no current
  Console errors. Warehouse_Testbed is saved, not dirty, and Play Mode stopped.
- Git whitespace check passes. Original prototype/base prefab/movement sources,
  package manifest and existing project settings retain their earlier hashes.
  Pre-existing settings diffs and untracked work were retained. No commit/push.

### Limits and Sprint 005

- Android Build Support is still unavailable for 6000.6.6f1. No modules, active
  targets or Android settings changed; no device build or physical testing done.
  Editor synthetic Touch identities do not validate OS delivery or hardware feel.
- Follow Sprint 003's Android build/device procedure above, adding these checks:
  aim softly without firing, deliberately cross/relax the fire ring while moving
  and jumping, tap reload with the right thumb, switch during reload, release one
  touch without canceling the other, recover with touches held and inspect the
  health/ammo HUD. Profile sustained firing and both enemies on real hardware.
- Balance, input-ring feel, missing sound/impact animation and single-ray melee
  need playtesting. Enemy detection is simple line-of-sight, not tactics or pursuit.
  Climbing currently demonstrates one authored ascent, not general navigation.
- Sprint 005 should prioritize real-device validation and combat feel: tune ring
  thresholds, add modest readable hit/reload/audio feedback, improve muzzle/aim
  presentation, then extend a small number of encounter/route cases with tests.
  Keep damage, ammo, input and AI separate; avoid a large weapons/navigation rewrite.

## Sprint 005 — ladder controls, combat feedback and Larper polish (2026-10-18)

### Implemented behavior and tuning

- Extended `PlayerTraversal2D` without changing `PlayerMotor2D`. Acquisition uses
  the player's actual world-space collider dimensions: the overlap query expands
  by `captureWidthMultiplier * colliderWidth` on each horizontal side (default
  1.25), and `bottomCaptureHeightMultiplier * colliderHeight` vertically (0.35).
  A ladder's own collider width also contributes to overlap eligibility. Entry
  requires vertical magnitude >= 0.35 and vertical/horizontal dominance >= 1.15.
  Walking alone never attaches. Nearby candidates must have a clear horizontal
  box-cast corridor; choose shortest axis distance, breaking exact ties by stable
  scene/hierarchy order. The reusable query buffer grows only if saturated; an
  overfull 256-result query rejects acquisition rather than making an unordered
  partial choice. Solid-cast saturation also fails closed.
- Alignment remains configurable per `LadderVolume` (8 units/second by default).
  Climbing remains 4.5 units/second, now clipped by solid box casts and ladder
  endpoints. No position teleport is used for alignment or assisted exits.
- Side dismount requires horizontal magnitude >= 0.6 and horizontal/vertical
  dominance > 1.2. Small lateral deviations keep climbing. A clear adjacent
  platform with support beneath both feet permits a smooth horizontal exit;
  otherwise normal airborne movement and gravity resume. The physical collider
  continues to prevent movement through walls after release.
- Reattachment delay remains 0.3 seconds. Entry attempted during the delay is
  consumed; release below `entryReleaseThreshold` (0.2), then deliberately enter
  again. Neutral is recognized at the input boundary as well as FixedUpdate so
  a short rendered input gesture is not lost between physics ticks.
- Automatic top exit searches both sides, preferring current horizontal intent
  or the ladder's authored exit direction. Default offset remains 1.8, at least
  1.5 player widths. `topLandingDrop` defaults to 2.5 for the existing warehouse
  endpoint heights; `sideLandingDrop` defaults to 0.8. Both feet need support,
  floor normals >= 0.65, similar support heights and a clear full-body corridor.
  No landing means hold safely at the top, with descent/side release available.
  An opposite horizontal gesture can cancel an assisted exit. Gravity resumes
  after crossing, allowing the small remaining drop to the landing naturally.
  Downward contact with a floor or reaching the bottom releases traversal and
  consumes the held gesture, allowing immediate walking without attachment loops.
- `JoystickJumpGesture` preserves upward jump threshold 0.65, neutral rearm 0.25
  and diagonal threshold 0.45. Added configurable climb dominance 1.15: (0.5,
  0.85) can deliberately climb, while the existing (0.7, 0.7) grounded diagonal
  gesture still jumps. Held climbing never turns into a jump at an exit.
- `AttackExecutor` publishes successful attack/hit and throttled empty-attempt
  events; `PlayerCombat` / `EnemyCombat` publish successful reload starts.
  `CombatFeedback` observes these events and Health, and is configured on both
  actor prefabs using the existing licensed project placeholder sprite. Reused
  muzzle (0.06s) and hit-confirmation (0.12s) sprites, hurt tint (0.15s), reload
  amber tint, empty gray tint and one short detached enemy-defeat silhouette make
  outcomes visible. Hit markers remain at the world hit point while moving.
  The defeat cue survives immediate authoritative enemy deactivation. Presentation
  never applies damage or changes ammunition/cooldown/reload deadlines.
- AudioSource hooks have configurable firing/hit/hurt/reload/empty/defeat clips
  and volume (0.5). No suitable audio assets were available: clips intentionally
  remain unassigned, so the prototype has functional hooks but no new sounds.
  No external assets, weapons, enemy types, levels, packages or build targets added.
- `EnemyCombat` separates visibility/reaction, sampled aiming and weapon execution
  from `LarperBrain` locomotion. Detection 14, range hysteresis 2, visibility memory
  0.65s, reaction 0.3s, aim sample interval 0.18s and turn speed 240 degrees/second
  are serialized. Occlusion immediately stops firing; short memory approaches the
  last visible position, then returns to patrol. Reacquisition pays reaction time.
  Preferred horizontal spacing is 5, effective shooting range 10; spread grows
  from 2 degrees at distance 3 to 16 degrees at effective range. The actual
  imperfect shot direction is checked for solid geometry before firing.
- Pursuit uses the existing motor and archetype capabilities. It pauses/retries
  blocked movement, jumps low obstructions when permitted, and avoids unsupported
  ledges during pursuit. Authored routes retain priority. Route completion now
  waits for grounded landing and anchors patrol on the destination floor before
  releasing control. This remains a small authored ascent, not general pathfinding.
- Two-thumb UI, fire-ring hysteresis, pointer identities, safe areas, reload and
  switching were retained; no new jump button. Existing layout tests exercise
  1280x720 and 2560x1080, with cutout math tested independently.

### Verification and retained work

- Baseline: all 15 Edit Mode and 20 Play Mode tests passed before implementation.
- Final: **16 Edit Mode + 31 Play Mode = 47 passing tests**, zero failed, skipped
  or inconclusive. Exact outcomes/timings are recorded in `TestResults/Sprint005.json`.
  All 35 earlier cases remain. Two old top-exit fixtures gained actual landing
  platforms instead of relying on an empty-space teleport; the player exit wait
  includes smooth crossing time. The old enemy test now waits for reaction time.
  Existing assertions were retained; these fixture/timing changes are intentional.
- Added regressions cover both offset entries, upward diagonals, smooth alignment,
  blocked/distant rejection, normal walking, deterministic competing ladders,
  side release with/without a platform, wall collision, gravity, lockout and fresh
  gestures, missing top landings, bottom release, actual UI Touch pointer entry
  and dismount with independent aim, feedback without duplicate damage, empty and
  reload cues, single defeat, player hurt, AI reaction/approach/visibility loss,
  effective range and distance spread. Existing cases cover disabled/destroyed
  ladders, held-up transitions, jumps/chute, checkpoint state, inventory, ammo,
  reload cancellation, faction filtering, spawning and authored routes.
- Live Editor verification used synthetic Touch events through the actual
  warehouse UIDocument controls, with runtime-only actor repositioning to isolate
  scenarios. These were scripted smoke checks, not human thumb or hardware tests.
  At right-ladder offsets -1.8 and +1.8, upward-diagonal gestures acquired and
  aligned to x=12. Side release at (12,-4.35) restored gravity 3 and falling
  velocity; the aim pointer remained independent. Holding the second ascent
  completed a platform exit without jumping and restored gravity 3.
- Both roof endpoints were checked with held-up UI gestures: left multi-level
  ladder exited at x=-10.24 and roof-access ladder at x=24.76, with downward
  velocity and normal gravity rather than a jump. A Game View capture confirmed
  readable, separated left/right controls, reload, chute and weapon HUD.
- Live warehouse outer-fire pointer: three carbine attacks, three hit cues,
  enemy health 50 -> 0, one defeat cue, enemy inactive, ammo 24 -> 21. The actual
  reload button produced one reload cue and ammo 21/36 -> 24/33. Held fire was
  cleared. The authored Larper route was observed Complete with grounded=True
  on the mezzanine. Runtime setup and smoke-test state were not saved.
- A separate two-second live enemy observation confirmed normal target binding,
  Engaging/visible state, two enemy attacks, player health 100 -> 90 and two
  player hurt cues. This longer observation avoided confusing the reaction delay
  in the shorter firing/reload smoke with an absent attack.
- Final compilation succeeds. Warehouse scene and actor/UI prefabs have zero
  missing scripts; typed spawns and feedback sprite references validate. The
  warehouse is saved, open and clean with Play Mode stopped. No new runtime
  errors during the final smoke. Unity AI Toolkit logged an account API timeout
  warning unrelated to gameplay; it was not hidden or treated as a test failure.
- Hash checks preserve Prototype_01, original Player.prefab, all original motor/
  desktop/follow scripts, package manifest and project/build settings. Existing
  untracked work and pre-existing settings differences remain. No commit or push.

### Limits, Android readiness and Sprint 006

- Android Build Support for Editor 6000.6.6f1 is still unavailable. No modules
  installed and no build settings changed. No APK/device validation or hardware
  performance claims. Follow the earlier Android checklist once support and a
  compatible phone are available: test both landscape orientations/cutouts,
  separate fingers, focus/pause/touch cancellation, reload/switch while moving,
  offset ladder entry, noise tolerance, deliberate dismount, held-up exits and
  reattachment after neutral. Profile sustained combat and traversal on-device.
- Real joystick feel still needs developer playtesting and device tuning. Solid
  geometry and landing rules are deliberately conservative; unusual/sloped or
  moving platforms may need authored exit points rather than larger capture/drop
  values. The warehouse's existing broad endpoint heights remain supported.
- Effects are simple placeholders, with unassigned audio clips. Most effects are
  reused per actor; defeat allocates one short-lived silhouette per enemy death.
  No pooled particle system or device profiling was introduced for two enemies.
- AI has local probes and one authored ascent only. It cannot plan across floors,
  choose arbitrary ladders, or guarantee every patrol jump across every gap. Lost
  visibility, blocks and route timeout recover locally rather than finding paths.
- Sprint 006: prioritize developer/device ladder feel, tune acquisition/dismount
  thresholds from observations, add licensed SFX and polish cue readability, then
  test prolonged encounters and route landing/patrol edge cases. Add only explicit
  route links needed by this warehouse before considering broader navigation.

## Sprint 005.1 — airborne ladder grabbing hotfix (2026-10-18)

### Cause and focused implementation

Traversal itself already checked capture candidates continuously while airborne;
it had no grounded-only or fresh-edge restriction. The problem was upstream:
`JoystickJumpGesture` consumed the jump gesture and set `ClimbSuppressed`, and
`PlayerActions` then zeroed vertical traversal input until neutral. Holding up
after jumping outside capture range could never reach the ladder controller.

- `PlayerActions` now passes ongoing upward climbing intent to traversal while
  airborne, even when that gesture originally requested a jump. This works for
  held up before capture and a new press after entering capture. Airborne jump
  gestures are still consumed, but no jump request is queued for later landing.
- `PlayerTraversal2D.HasUpwardClimbIntent` shares the existing serialized entry
  threshold (0.35) and directional dominance (1.15) with the input adapter. The
  same predicate, with absolute vertical input, preserves existing up/down entry.
  No new thresholds, capture margins, alignment speeds or movement rules added.
- Capture selection, collision casts, smooth alignment, gravity/vertical velocity
  takeover, top/bottom exits, deliberate dismount and post-dismount delay/release
  are reused unchanged. Traversal still makes the actual attachment decision in
  FixedUpdate. The airborne input exception cannot bypass its lockout or blocked
  path checks. Grounded gesture suppression and diagonal jump arbitration remain.
- A lateral/equal-diagonal jump past a ladder does not express upward-dominant
  climb intent, so it remains normal airborne movement. No extra jump occurs when
  grabbing: the motor is disabled and traversal replaces jump/fall velocity with
  bounded alignment and climbing velocity, with gravity zero while climbing.
- Runtime logic changes are limited to PlayerActions and PlayerTraversal2D. The
  motor, joystick gesture policy, combat, enemies, scene geometry, prefabs,
  packages and project/build settings were not changed for this hotfix.

### Tests and live verification

- Reconfirmed all 31 Sprint 005 Play Mode cases before implementation. Added
  AirborneLadderTests; before the fix five of the first eight cases failed,
  reproducing held-input acquisition failures from both sides and above the player.
  Fresh input already inside capture and non-climbing jumps passed, narrowing
  the cause to input suppression rather than physics/capture distance.
- Final result: **16 Edit Mode + 40 Play Mode = 56 passing tests**, zero failed,
  skipped or inconclusive. All 47 Sprint 005 cases remain unchanged and pass.
  Nine added Play Mode cases cover actual jump trajectories, held-up vertical
  capture, held upward diagonals approaching from left/right, entering range
  before a fresh upward press while falling, ordinary lateral jumps, blocked and
  distant candidates, disabling/enabling during approach, destruction before and
  after capture, continued climbing without a new gesture, bounded alignment,
  no duplicate jump, post-dismount lockout/fresh release and gravity restoration.
- One new approach case passed alone but intermittently failed in an early full
  run. Its fixture now starts 3 units from the axis, still explicitly outside
  capture, with a rendered input-sampling step and adequate flight-time margin
  under Editor load. Original Sprint 005 fixtures/assertions were not changed.
  Post-compile MCP calls occasionally returned HTTP 400; settled discovery and
  subsequent complete test runs produced the recorded results. Compilation passed.
- Scripted live warehouse smoke used real UIDocument joystick PointerDown/Move
  events with a synthetic Touch identity, not direct traversal attachment calls.
  Actors were repositioned only in Play Mode to isolate both approaches. From
  x=9, diagonal jump reached (9.45,1.91), rising at 8.47 with gravity 3 and no
  ladder attached. Changing to upward-dominant input while still outside capture
  aligned to (12,4.94), climbing at 4.21 with gravity 0. The right approach from
  x=15 similarly reached (14.55,1.91), then (12,4.85) climbing. Neither approach
  required neutral or a second upward gesture after entering capture.
- Live sideways release restored gravity 3 and downward velocity -2.35 on both
  sides, with traversal off. These runtime setup/state changes were not saved.
  This is an automated Editor smoke, not human thumb or Android hardware testing.
- Exact test results, reproduction data, baseline hashes and live observations:
  `TestResults/Sprint005_1.json`. Warehouse_Testbed is saved/open, Play Mode stopped.
  No commit or push. Android support/device validation remains as documented in
  Sprint 005; no module installation or build-setting changes were made.

## Sprint 006 — movement completion and contextual Larpers (2026-10-18)

### Crouch state and input arbitration

- Added PlayerPosture2D on WarehousePlayer, consuming the existing PlayerActions
  move vector. No extra input system or crouch/jump button. Downward magnitude
  >= 0.35 while grounded crouches; release above -0.2 requests standing. Downward
  diagonals crouch-walk. S/Down Arrow use the same path as the left joystick.
- The 0.8 x 1.4 standing BoxCollider2D becomes 0.8 x 0.77 (height multiplier
  0.55), with offset Y -0.315. This preserves feet and body position. Movement
  input is scaled by 0.45 while crouched: maximum speed 3.15 rather than 7 units/s.
  The original movement motor is unchanged; it still handles acceleration/jumps.
- Standing uses a non-trigger solid overlap query of the standing volume, with
  contact skin and conservative buffer saturation. A low ceiling keeps the actor
  crouched until clearance returns. No position correction, launch or upward
  impulse is used to stand. Jump requests first require safe standing, then use
  the existing grounded motor check. A rejected joystick jump stays consumed;
  removing the ceiling does not turn held input into a delayed jump.
- Active ladder descent takes priority over crouch. Grounded down away from a
  usable top means crouch. Deliberate down near a reachable ladder top aligns and
  descends instead. Floor contact no longer immediately cancels entry while still
  aligning from a top platform; bottom floor exits still work at the ladder axis.
  Top eligibility uses original standing height even when approaching crouched,
  retaining reach at the warehouse's tall authored endpoints. Airborne grabbing,
  directional dominance, sideways dismounts and lockout remain covered.
- PlayerPostureVisual supplies a separate reusable placeholder pose matching the
  original sliced sprite dimensions. Collision state does not depend on rendering.
  Aim marker and AttackExecutor.Origin use current collider center, lowering
  crouched aim/hitscan/muzzle cues by 0.315. Independent aim and firearm use remain.
  Recovery requests safe standing after teleporting to its authored checkpoint.

### Authoritative parachute toggle

- Existing CHUTE button and Left Shift now toggle on press edges. Button release
  only releases its physical pointer; it does not retract. Repeated held frames
  or duplicate downs do not toggle again. PlayerTraversal2D owns IsParachuting;
  the UI reads it for DEPLOY/RETRACT text and a teal deployed highlight.
- Serialized defaults: horizontal multiplier 0.4 (maximum speed 2.8 vs normal 7),
  maximum descent 2.8 units/s, descending gravity multiplier 0.2 (gravity scale
  0.6 vs normal 3), redeployment delay 0.2 seconds. Retraction is always allowed;
  a rapid redeployment attempt is rejected during that delay. No velocity boost
  is added on toggles; normal motor steering remains available at reduced speed.
- Deployment requires actual airborne clearance and no ladder attachment.
  Ascending deployment is enabled by default but keeps normal ascent gravity,
  so it cannot boost jump height; controlled descent starts on falling. Landing,
  ladder attachment, input/focus reset, defeat recovery and disabling clear it.
  Retraction/recovery restore normal gravity; attaching to a ladder uses gravity 0.
- Retained the original parachute regression case, updating its incompatible
  hold-to-release assertion to verify persistence on release and second-press
  retraction. Its descent limit, gravity and checkpoint assertions remain.

### Larper personality foundation

- LarperPersonalityProfile assets separate Ordinary, Overconfident and Cowardly
  dialogue/morale data from archetype locomotion, weapon execution and Health.
  Initial witnessed-defeat panic probabilities are 8%, 2% and 20%, respectively.
  Witness range is 9, panic interruption 2.5 seconds, per-actor morale cooldown 45s.
- LarperAwareness observes visibility transitions, four consecutive missed attacks
  and nearby teammate defeats. Events drive short configured dialogue arrays;
  no periodic random speech timer. Witnesses need range, solid line of sight and
  on-screen relevance. Inactive/off-screen actors do not produce irrelevant lines.
- EncounterSpeechGate shares a 30-second ordinary cooldown across speakers. Rare
  panic lines may override after a 5-second minimum gap, at most once per 60s.
  These settings are serialized in profiles. No line is forced every 30 seconds.
- LarperSpeechBubble creates one reusable, non-interactive UI Toolkit Label per
  speaking actor, shows it for 2.8 seconds and tracks its world position in the
  existing HUD panel. No per-frame object creation or repeated actor-wide search.
  Subscriptions and labels clean up when actors disable/destroy.
- Panic temporarily suspends ordinary combat/pursuit, cancels reload and stops
  the motor through its normal input boundary, then restores normal decisions.
  Authored active ladder routes reject panic to avoid disrupting traversal.
  Retreat, weapon dropping, richer morale and voice playback are not implemented;
  this deliberately avoids inventory/navigation expansion.
- Live testing found that existing geometry ray queries treated ladder triggers
  as walls. CombatVisibility2D now uses non-trigger contact filters for enemy
  sight, actual firing directions and witnessed-event visibility. Solid walls
  still block these queries; trigger regressions were added.

### Warehouse integration and executed validation

- Added one low beam/passage at (-21.3,1.25), world size 2 x 0.4 and underside
  1.05, between the entry checkpoint and the existing crate. It blocks standing
  height but allows 0.77 crouch height. Automatic collider tiling is disabled;
  a preview-scene test verifies authored/rendered size and clear checkpoint spawn.
  A live check caught an initial auto-tiling mismatch; it was corrected before
  final validation. No new playable level was created.
- Reused one previously inactive typed spawn as a nearby cowardly witness at
  (-10,0.8). The original ground Larper is overconfident; the authored ladder
  Larper uses the ordinary prefab profile. Three instances of the existing enemy
  type provide an encounter with actual speech/witness opportunities.
- Baseline discovered from the current project: 16 Edit Mode + 40 Play Mode = 56
  cases. All 40 Play Mode baseline cases were executed before changes. The final
  suite retains those 56 cases, with the required parachute semantic adaptation
  described above. New tests exercise movement, authored geometry, speech and morale.
- Final: **19 Edit Mode + 54 Play Mode = 73 passing tests**, zero failed/skipped/
  inconclusive. Exact outcomes are in TestResults/Sprint006.json. Coverage
  includes grounded/diagonal crouch, collider/feet/pose matching, low ceilings,
  standing and jumping, ladder top descent including crouched high endpoints,
  toggle edges/release/rapid rejection, ascent without propulsion, reduced steering
  and descent, landing/crouch, ladder/respawn/disable resets, independent crouch
  firing and three simultaneous pointer identities. Prior airborne grab and
  dismount, inventory/ammo/reload/faction and layout regressions remain.
- Personality tests cover configured data and differing probabilities, encounter
  throttling/rare overrides, contextual contact lines across speakers, off-screen
  suppression, solid-wall rejection, trigger-transparent visibility, witnessed
  panic, firing interruption and return to patrol. No randomized pass criteria:
  tests use explicit random samples or temporary profiles with probability 1.
- Live scripted warehouse checks used synthetic Touch events through actual
  UIDocument controls, not keyboard-only shortcuts. The player crouch-walked under
  the beam at 2.23 units/s, with collider 0.8 x 0.77 and feet Y 0.01. Releasing
  down beneath it retained crouch; after leaving, the collider restored to
  0.8 x 1.4 with feet still Y 0.01. Ten attacks fired while aiming independently;
  the crouched hitscan origin was Y 0.40. Respawn returned to (-23,0.71), standing,
  feet Y 0.01 and fire false. Runtime scenario positions/state were not saved.
- Live three-pointer chute tap/release stayed deployed and displayed RETRACT.
  With partial steering, X speed was 1.76 (vs 4.40 after retracting at the same
  input); descent was -2.80 with gravity scale 0.6. Second tap restored gravity 3
  and falling velocity reached -5.74. Independent aim/fire stayed active.
- Inspected live readable contact speech ("CAN WE MAKE THIS A PRIVATE LOBBY?")
  and the stylized witness panic line. The deterministic panic demonstration used
  a runtime-only cloned profile at probability 1; the witness entered Panicked,
  showed "MOM COME PICK ME UP IM SCARED!" and the defeated actor became inactive.
  The clone was discarded and saved profile probabilities verified unchanged.
  This demonstration does not claim that a random production panic was observed.
- A final UI-pointer roof check started crouched at (-9,12.72), approached the
  high multi-level endpoint with downward-diagonal input, then stood safely and
  descended at (-12,11.03), gravity 0 and Y velocity -4.21. This validates the
  standing-height reach rule through actual warehouse controls.
- Editor layout regressions cover 1280x720 and 2560x1080 plus cutout inset math.
  Final compilation succeeds, current Console has no errors, typed spawn/profile
  references validate, and scene/actor/UI prefabs have no missing scripts.
  Warehouse_Testbed is saved/open/clean and Play Mode stopped. Original Prototype_01,
  base Player prefab, original motor/desktop/camera scripts, package manifest and
  project/build settings retain their baseline hashes. No commit or push.
- Some immediately post-compile MCP calls returned transient HTTP 400 / connection
  errors. Settled discovery and complete subsequent suites produced actual results;
  no filesystem substitution was used for live scene inspection/authoring.

### Android, limitations and Sprint 007

- Current Editor 6000.6.6f1 still reports Android Build Support unavailable; active
  target remains StandaloneWindows64. No Android modules installed, build settings
  changed, device builds or real hardware tests. Editor synthetic pointers and
  screenshots do not establish real thumb feel, OS lifecycle delivery or hardware
  performance. No device performance measurements are claimed.
- Once matching support and a compatible phone are available, validate portrait
  interruption and both landscape orientations/cutouts; crouch/stand under the
  beam, crouch-jump rejection, top descent and airborne grabs; tap/release/second
  tap chute while steering/aiming/firing; landing, held touches during recovery,
  focus/background/cancellation and pointer ownership. Profile sustained combat
  and UI bubbles on hardware before increasing enemy count or effects.
- The crouch pose and speech are placeholders; no full animation rig, voiced
  dialogue, retreat, weapon dropping or social simulation. Speech choices are a
  small configured collection. World labels can overlap nearby scenery; no complex
  bubble avoidance layout was introduced. Existing audio hooks still need licensed
  clips. The local AI/authored route limits from Sprint 005 continue to apply.
- Sprint 007: prioritize developer/device movement tuning and lifecycle validation,
  readable crouch/weapon poses and licensed SFX. Then playtest three-Larper combat,
  speech timing and panic rarity in longer encounters, extend only required authored
  routes and add targeted regressions. Keep input, collision, combat authority,
  presentation and personality separated; avoid a new navigation/animation rewrite.

## Sprint 007 — implementation and recovery verification (2026-10-18)

Recovery resumed the existing work after the interrupted session. It did not
restart implementation, add features/tests, investigate more artwork, generate
replacement art, or rewrite working systems. No critical integration defect was
found in verification, so no runtime code fix was required during recovery.

### A. Completed features

The pre-interruption implementation provides reusable cutout player/Larper rigs,
replaceable skins, movement/traversal poses, independent weapon aiming and recoil,
event-driven expressions, head-following speech with basic overlap avoidance,
and a bounded jointed Larper ragdoll. Existing gameplay remains authoritative.

### B. Artwork and publication boundary

Selected historical Drive character/ragdoll parts, a firearm and a parachute
outline were integrated before interruption. Unchanged source downloads, crop
descriptions, dimensions and provenance are recorded in ART_MANIFEST.md and
HistoricalArtSources/Drive/. This is a small selected audit, not recovery of every
historical asset. Unbound fallback PNGs remain explicitly marked temporary.
Facial variants beyond the original neutral faces are temporary adaptations.

Per the recovery instruction, historical and potentially third-party artwork is
**private-prototype-only**. It must be replaced with new, independently created
modular pixel art before publication. Drive availability does not establish
publication rights. No replacement artwork was generated during recovery.

### C. Pixel rendering

Working derivatives use 32 PPU, point filtering, no mipmaps, uncompressed RGBA,
clamp wrapping and explicit pivots/sorting. URP 2D was detected before authoring.
The existing warehouse camera owns integer art-pixel zoom and position snapping;
visual children snap separately without quantizing physics. No competing Pixel
Perfect Camera was added. UI remains at screen resolution. Integer projection
and bubble placement are tested at 1280x720 and 2560x1080. Rotating cutouts still
change their raster silhouettes; human shimmer/readability review remains needed.

### D. Modular architecture

CharacterSkin contains replaceable sprite references; CutoutRig owns the head,
torso, four limbs, weapon/face/accessory mounts, speech anchor and canopy.
CharacterVisualDriver observes the existing motor, traversal, posture, aim,
combat, recovery and awareness data. It represents idle/walk/jump/fall,
crouch/crouch-walk, climb, parachute and panic without moving gameplay roots.
WarehousePlayer and Larper use nested visual prefabs. The previous posture proxy
remains disabled as a renderer while preserving its existing regression contract.

### E. Expressions and dialogue

Detection, missed attacks, delivered boasts, witnessed defeats, hurt and panic
drive neutral/smug/angry/surprised/scared/stunned overlays. No random expression
timer or parallel morale authority was introduced. The existing encounter speech
gate/cadence remains unchanged. Bubbles follow animated heads, clamp to the UI
screen, try alternate rows around earlier bubbles, and hide on disable/defeat.
Crowded screens can still exhaust the simple placement candidates.

### F. Cartoon ragdoll

The prefab has six Rigidbody2D/BoxCollider2D parts and five limited HingeJoint2D
connections. Defeat immediately deactivates the authoritative enemy as before;
the observer spawns once, copies the visual pose/appearance, transfers clamped
momentum, and keeps the body in its source scene. Mirrored limb orientation and
scene-unload cleanup are covered. Corpses use built-in TransparentFX with geometry-
only collision exclusions, avoiding existing actor/attack query masks. A four-body
cap, resting sleep and a 12-second lifetime prevent unbounded accumulation. No
gore, detached parts or new damage mechanics were added.

### G. Major existing Sprint 007 files

- Assets/Scripts/Visuals/: CharacterSkin, CutoutRig, CharacterVisualDriver,
  CartoonRagdoll and RagdollOnDefeat.
- Assets/Prefabs/Visuals/: PlayerCutoutVisual, LarperCutoutVisual,
  LarperCartoonRagdoll; bound WarehousePlayer and Larper gameplay prefabs.
- Assets/Editor/Sprint007VisualSetup.cs: explicitly invoked authoring utilities;
  no automatic execution. Recovery did not rerun asset generation.
- Existing LarperAwareness/LarperSpeechBubble, CombatFeedback,
  PlayerPostureVisual and WarehouseCameraFollow integration points.
- Assets/Tests/EditMode/VisualAssetTests.cs and
  Assets/Tests/PlayMode/VisualIntegrationTests.cs were already present at recovery.
- ART_MANIFEST.md, ANDROID_USB_WORKFLOW.md, HistoricalArtSources/Drive/ inventory
  and source hashes; final verification artifact TestResults/Sprint007.json.

### H. Recovery validation results

- Unity MCP connected to Insane-Dreams-2.0, Editor 6000.6.6f1. Compilation completed
  successfully, with no compilation errors or domain reload in progress.
- Warehouse_Testbed and all five gameplay/visual/ragdoll prefabs exist; required
  rig/skin/sprite/ragdoll references validate. Scene and prefabs have zero missing
  scripts. Existing environment, interactions, markers, camera, UI, enemy and level
  entry roots remain intact.
- Full **Edit Mode: 27/27 passed; Play Mode: 62/62 passed; total 89/89**.
  This preserves the original 19+54 = 73 tests and executes the 16 already-added
  Sprint 007 cases. No tests were added or existing coverage removed in recovery.
  Results include actual traversal/pose transitions, facing/aim, recoil, skin
  replacement, crouch alignment, expressions, speech lifecycle, one-time defeat,
  momentum/joint stability, cap/TTL and mirrored additive-scene cleanup.
- Prior implementation included an initial live Warehouse inspection showing the
  green player and modular Larpers. Recovery verified references and ran complete
  Editor/Play suites; it did not perform a new exhaustive visual playtest or claim
  human approval of animation quality.
- On resumption the Console reported an Editor service error:
  `UnityConnectWebRequestException: Token Exchange failed due a failure with the web request.`
  It was not a compilation failure. Both suites completed successfully afterward;
  final current Console counters were 0 errors / 0 warnings. Historical buffered
  entries remain distinct from current Console state. No account/settings changes
  were made to suppress or repair that external service issue.

### I. Android deployment readiness

ANDROID_USB_WORKFLOW.md documents exact-version Hub module setup, SDK/NDK/JDK,
USB debugging/authorization, ADB device checks, Run Device, Build and Run, and
Unity 6.6's documented development Patch And Run option. Matching Android support
is still unavailable; no phone deployment or performance result is claimed.
No modules were installed, active target switched, or global build settings
changed by Sprint 007/recovery. Active target remains StandaloneWindows64.

### J. Remaining work and conclusion

Before publication, replace historical/third-party visuals with independently
created modular art; replace temporary expression overlays and review pivots,
proportions, weapon attachment and limb poses with the developer. A single visual
firearm currently represents equipment without per-slot model variants. Full
warehouse art replacement, additional historical animation-frame integration and
human visual-quality acceptance remain deferred. Verify ragdoll sleep/performance
under sustained encounters on an actual Android phone; automated bounded-physics
tests do not establish device frame pacing or long-session visual quality.

Warehouse_Testbed was saved successfully, left open and clean outside Play Mode.
No commit or push was performed. Pre-existing untracked project content and prior
ProjectSettings modifications were preserved.

## Sprint 008 checkpoint — reactive ragdoll core (2026-10-18)

- Diagnosis: the previous attack mask 5 excluded ragdoll layer 1; hits only
  delivered Health damage, and defeat data contained no hit point/direction.
- Added a separate PhysicalImpact/IPhysicalImpactReceiver path. DamageInfo carries
  optional fatal-hit metadata without changing Health's filters or defeat latch.
  AttackExecutor includes the dedicated physical layer and selects the nearest
  non-trigger hit; ragdolls block shots, including shots at actors behind them.
- Ragdoll hits apply an impulse at the hit segment's closest contact point, wake
  the connected bodies and reset resting state. Fatal hits use the retained impact
  after visual-pose/momentum transfer. Damping, limits, impulse/velocity clamps,
  sleep timing, a four-body budget and 45-second absolute lifetime are tunable.
- Live warehouse core demonstration: exactly one defeat/body, initial impacted
  segment velocity about (5.14,0.62); three later hits produced impact count 4 and
  awake bodies. After 1.2 simulated seconds the body lay near (7,0.24), rotated
  roughly -81 degrees, on the floor. No duplicate defeat. Runtime demo discarded;
  Play Mode stopped and prefab changes saved. This is an Editor observation, not
  a claim of satisfying human comedy/visual-quality acceptance or device speed.
- Next: focused hit/wake/occlusion/stability tests, independent code-authored pixel
  assets and representative warehouse decoration, feedback polish, full regression
  run and final documentation. No commits/pushes. No modules/build target changes.

## Sprint 008 — verified completion (2026-10-18)

### A. Physics and gameplay

Added PhysicalImpact (point/direction/impulse/body) and IPhysicalImpactReceiver.
Health carries optional impact metadata but retains its damage filters, faction
rules and one-time defeat latch. AttackExecutor detects the nearest non-trigger
hit across damage/geometry mask 5 and physical-impact mask 2. A reusable, initially
64-entry raycast list can grow rather than silently truncating a crowded result.
No damage, ammunition, reload, firing interval, AI or traversal rule was changed.

### B. Fatal and subsequent impacts

Fatal shot data survives the immediate authoritative enemy deactivation. The
ragdoll copies the visual pose and pre-defeat velocity before applying the fatal
impulse to the nearest appropriate part. Later raycasts identify the actual
ragdoll Rigidbody2D and apply AddForceAtPosition at its contact point. Off-centre
hits produce torque through the existing hinges. Neither living Health nor a new
defeat event is required. Hits wake all connected bodies and clear resting state.
Ragdolls **block shots** aimed at living targets behind them; a closer solid wall
blocks shots before they reach a corpse. Existing awareness still observes the
original defeat once, not every corpse impact.

### C. Inspector tuning and lifecycle

- WeaponDefinition.impactImpulse: default 0.9 N·s, independently configurable per
  weapon without changing damage/ammo rules.
- CartoonRagdoll: initialDefeatMultiplier 2; postDefeatMultiplier 1.2;
  maximumImpulse 2.5 N·s; maximumVelocity 12 units/s; maximumAngularVelocity
  720 degrees/s. A zero velocity limit disables that clamp. Invalid impact data
  is ignored. Incoming movement is retained with the prior 8 units/s transfer cap.
- Masses retain torso 0.7, head 0.35, limbs 0.15, gravity scale 2. Linear damping
  0.5, angular damping 1.2; neck +/-35 degrees, configurable limb limit +/-80.
  Five hinges, continuous collision detection, no limb detachment or gore.
- Rest after 1.5 seconds below the existing linear/angular movement thresholds.
  Sleeping bodies remain collidable and can wake from impacts or physics contact.
- Maximum four registered bodies including resting bodies: at most 24 rigidbodies
  and 20 hinges. Oldest is retired deterministically before adding another.
  Absolute lifetime is 45 seconds; repeated hits do not reset it. Defaults are
  configurable. No pooling or static-remains system was needed.
- Corpse layer remains built-in TransparentFX (index 1); collider exclusions allow
  Default geometry collision, excluding actors and other corpse segments/bodies.
  The separate attack physical mask includes this layer. No global layer matrix
  or project settings changes. Source-scene ownership/unload cleanup is preserved.

### D. Independent character art and historical handling

Sprint008ArtSetup draws original pixel coordinates/palettes without reading any
historical image. The active worker and bucket-helmet Larper skins include head,
torso, arms, legs, six expression overlays, a generic scrap firearm and patchwork
canopy. All active character/visual/ragdoll prefabs now use Sprint008 assets. The
historical Drive sources, old derivatives and archive remain preserved and
separately documented in ART_MANIFEST.md. Eight local historical source/preview
hashes were checked unchanged; the ZIP retains SHA-256 B54A74842A85B102FCDF4A5B6BA66DE1B4565C3530FC0B087013A0D4A1001B15.

The 34 new PNGs are an independently code-authored foundation, not final
professional art or a claim of legal clearance. Human style/provenance and eventual
publication approval remain required. No external art pack, traced source, AI
image-generation output or unlicensed audio was added.

### E. Warehouse art and practice configuration

Eleven reusable environment sprites cover concrete, metal walkways, hazard trim,
mortar walls, riveted beams, loading door, divided windows, vent, pipe, crate and
ladder. Point/no mip/uncompressed/32 PPU conventions match the rigs. Tiled sprites
add depth with muted backgrounds and clear terrain/actor contrast; no per-frame
parallax or new environmental physics. Render ordering is documented in the art
manifest. Existing flat renderers remain disabled as references.

WarehouseArt_Sprint008 has zero colliders. All 30 existing collider identities,
positions, scales, offsets, sizes and trigger flags matched the pre-art snapshot;
the persistent geometry regression also passes. Ladder capture zones, route/spawn
references, platform gaps and checkpoints remain intact.

The optional practice controls (left panel or F6) spawn a passive target at
(6,0.85), or a drop target at (8,7.6) above the existing mezzanine landing. They do
not automatically spawn or change the regular encounter. Respawning removes the
previous practice actor, while detached bodies remain under their normal budget
and lifetime. Shoot from either side, then spawn more targets to test the cap.

### F. Combat/personality/audio presentation

Corpse hits receive a cool spark; solid geometry receives a warm spark. Existing
living-hit, hurt, reload, empty and defeat cues remain separate from these outcomes.
Feedback now runs after the visual driver so muzzle flashes follow the animated
barrel through aim, crouch and recoil. Backward aiming flips the sprite vertically
in mount space to keep its grip upright while locomotion facing stays independent.
The obsolete flat canopy renderers are disabled, leaving the modular canopy.

Event-driven faces, head-following bubbles, overlap handling and encounter speech
cadence remain unchanged. Gunfire/reload/impact/collision/movement audio references
are available; clips remain unassigned. Ragdoll collision cues have a 0.3s cooldown
and speed threshold. Basic footstep/landing/ladder hooks observe movement only.
No unlicensed clips or elaborate audio framework were introduced.

### G. Main files

- Assets/Scripts/Combat/PhysicalImpact.cs, AttackExecutor.cs, Health.cs,
  CombatFeedback.cs; Assets/Scripts/Inventory/WeaponDefinition.cs.
- Assets/Scripts/Visuals/CartoonRagdoll.cs, RagdollOnDefeat.cs,
  RagdollCollisionCue.cs, MovementAudioHooks.cs, CutoutRig.cs.
- Assets/Scripts/Level/RagdollPracticeStation.cs;
  Assets/Editor/Sprint008ArtSetup.cs; Assets/Art/Sprint008/ and new skin assets.
- Warehouse_Testbed, WarehousePlayer, Larper, both visual prefabs and ragdoll prefab.
- ReactiveRagdollTests, Sprint008PresentationTests, WeaponPresentationTests;
  TestResults/Sprint008.json and Sprint008GeometryBaseline.json; two inspected
  inline Game view captures (no retained local PNG artifacts).
- ART_MANIFEST.md and ANDROID_USB_WORKFLOW.md updated.

### H. Tests and actual live observations

Final full suites: **29/29 Edit Mode and 68/68 Play Mode = 97/97 passed**, no skips
or failures. All **89 previous test names** are retained; only eight focused new
cases were added (five physics, two presentation/geometry, one weapon orientation).
Coverage includes repeated hits, one-time defeat, nearest-hit/wall occlusion,
sleep/wake, opposite impulses, torque and clamps, absolute cleanup, active-art
bindings/imports, unchanged scene geometry, upright independent aim and muzzle
attachment. Earlier focused physics run was also 5/5.

Live Editor observations using the normal PlayerActions/PlayerCombat firing path:

- Practice target defeated after three shots: one ragdoll, one fatal impact.
- Five additional player shots hit the same corpse: impact count rose to six,
  source SpawnCount remained one. Ammo HUD changed normally.
- After four simulated seconds without firing: resting=true and all bodies asleep.
  A shot from the opposite side raised count to seven, cleared rest and imparted
  leftward velocity. No AI reactivation or extra defeat was observed.
- Drop target collapsed onto the mezzanine, torso near (8.30,5.71).
- Five additional practice defeats held the registry at four; a temporary runtime
  0.25s lifetime override then cleaned all bodies. Saved default remains 45 seconds.
- Normal encounter produced one visible head-anchored line:
  "CAN WE MAKE THIS A PRIVATE LOBBY?", with a surprised expression in the new skin.
  Other actors did not produce duplicate lines at that moment.
- Airborne grab after the existing reset/dismount lockout: climbing=true, pose=Climb,
  gravity=0 at (12,2.33), held move=(0,1). Earlier manual attempts pressed up during
  the reset lockout and were correctly rejected until release; no traversal change
  was made to bypass that rule. Mezzanine checkpoint activated at (0,6.25), normal
  gravity 3 restored after leaving climbing.

Runtime scene/test manipulations were discarded. Two Game view captures were
displayed and inspected inline; no local PNG artifacts were retained. Final
compilation succeeds; current Console has 0 errors/0 warnings.
Warehouse and five actor/visual/ragdoll prefabs have zero missing scripts and valid
required references. Automated success and these observations do not substitute
for the developer judging whether reactions are funny, readable or satisfying.

### I. Android and performance

6000.6.6f1 still reports Android Build Support unavailable, AndroidPlayer absent,
and no PATH-visible ADB. No module installation, target switch or device build
was attempted. ANDROID_USB_WORKFLOW.md retains exact setup, authorization, Run
Device and Build and Run instructions, with the supported optional patch workflow.
No phone FPS/memory claim. Four bodies/24 rigidbodies/20 hinges, sleeping contacts,
fixed cue renderers and static decorative art bound runtime work; verify actual
physics cost, frame pacing, landscape UI and multitouch on hardware.

### J–K. Limits and Sprint 009 recommendations

Human art/provenance approval, visual-quality playtesting and Android hardware
validation remain pending. Audio hooks have no assigned licensed clips. The
current single visual firearm still represents all equipment slots. The practice
targets are intentionally passive, not a new encounter/respawn system. Corpses
block bullets but do not collide with actors or one another. No pooling, static
remains, campaign level, destructive physics or new enemy/weapon roster was added.

Next sprint: developer playtest impulse/mass/damping values before increasing the
budget; profile four-body repeated impacts on the phone; approve/refine independent
art and expressions; add a small rights-reviewed sound set and equipment-specific
visuals only where needed. Preserve current authority boundaries and targeted tests.

Warehouse_Testbed is saved, open, clean and outside Play Mode. No commit or push.
Pre-existing project settings changes and untracked content were preserved.

## Sprint 009 checkpoint — authored presentation corrections (2026-10-18)

- Existing 97-test Sprint 008 baseline was inspected. Larper/player head renderer
  ratios measured 53.1%/46.3%, now 24.5%/22.8%. Added 24 small code-authored PNGs;
  larger torsos/limbs, coherent shoulders/hips/neck, matching six faces and anchors.
- Updated ragdoll geometry, neck anchors and mass balance; retained impulse,
  damping, hinge-limit, wake/sleep, cap and lifetime settings. Runtime activation
  copies absolute part scales and sets centres of mass from corresponding colliders.
- Authored reusable ladder body/caps with floor-level feet and world-pixel snapping;
  existing traversal geometry is untouched. Climbing retains one visible equipped
  item while the other arm cycles. Existing aim/fire behavior remains supported.
- Added secondary-firearm/melee appearances for existing slots and empty-slot
  handling, without changing inventory/combat authority. Parachute supporting-solid
  contact now clears the existing authoritative state immediately after physics.
- Art/prefabs/scene saved; full regression run and integrated smoke checks in
  progress. No commit/push, new gameplay system, historical-art recovery or build
  setting change. Continue with the current tests rather than reauthoring assets.

## Sprint 009 — proportions and transition polish (2026-10-18)

### A–B. Character proportions and completed presentation

Inspected the Sprint 008 implementation and 97-test artifact before editing.
Neutral standing renderer-bounds measurements (including transparent sprite
border): Larper head was 0.75 / 1.41375 = 53.1%, now **0.34375 / 1.403125 = 24.5%**.
Player was 0.625 / 1.35125 = 46.3%, now **0.3125 / 1.371875 = 22.8%**. Gameplay
standing colliders remain 0.8x1.4; movement/damage/ammunition calculations were not
changed for these proportions.

This is a whole-rig correction: 12x17 torsos, 5x18 arms, 6x18 legs, shoulders
0.1875 units either side, torso centre Y 0.09375, hips derived from leg length and
feet at -0.7. Neck/head positions derive from torso/head bounds. Crouch retains
floor-aligned feet and a compact head/torso layout. Weapon grips follow the actual
carrying-hand endpoint; speech anchors sit just above the resized head. Six
expressions per character fit 12x10 player / 12x11 Larper heads. Existing helmet,
rust workwear/purple patchwork identities, outlines and awkward cutout style remain.

### C. Ragdoll anatomy and physical compatibility

Updated all six part sprites, collider sizes/offsets and neck/shoulder/hip poses.
Neck anchor is now the actual head sprite bottom (-0.171875 for the Larper), not
the old -0.3 offset. Centres of mass are set to collider offsets when simulation
is enabled; static prefab assets do not reliably report live physics COM values,
so the COM assertion runs against the activated body. Activation copies absolute
world part scale and pose, including mirrored poses, before enabling physics.

Masses: torso 0.78, head 0.18, arms 0.13 each, legs 0.24 each; total 1.70 versus
previous 1.65. The head no longer outweighs a leg. Hinge limits +/-35 neck and
configured +/-80 limbs, damping, impulse multipliers/clamps, gravity, sleep/wake,
four-body budget and 45-second lifetime remain unchanged. No fixed falling
animation, extra anatomy segments or forced directional-collapse logic was added.
Actual impulses and contact location continue to determine the cartoon tumble.

### D. Ladder visual correction

The old decorative sprite covered the entire capture volume, including roughly
0.8 units below its adjacent floor. LadderVisual2D now owns independently authored
body/foot/handrail renderers, width and endpoint configuration, snapped in world
pixel space. Feet are -7 for the multi-level and basement/mezzanine ladders, and
5.40625 (nearest pixel to the 5.4 mezzanine) for roof access. Caps meet the body
without gaps or rendering below the supporting floor.

Top handrails retain the existing interaction endpoints, approximately 14.4, 6.6
and 13.4; their extensions above upper landings are deliberate because the current
traversal climbs to those endpoints. No capture zone, platform collider, floor
gap, acquisition margin, dismount rule or enemy route was moved. The existing
geometry fingerprint test still protects all 30 colliders.

Visual authoring uses Configure or the component's Refresh visual endpoints
context command. Runtime refresh runs in Start. An initial OnValidate renderer
refresh produced Unity OnSpriteTilingPropertyChange warnings; it was removed in
favor of field validation only, avoiding renderer mutation during validation.

### E. One-handed climbing and equipment

Removed the explicit climb-time weapon hide. The ladder hand cycles independently
while the carrying arm holds its item downward; a short visual blend smooths
entry/exit. Existing independent aim/fire while climbing remains available: explicit
aim or firing drives the carrying arm normally, with recoil and barrel feedback.
PlayerActions exposes read-only aim-intention data solely for presentation.

The visual driver reads PlayerLoadout's existing selected slot/weapon. Firearm 1
retains the scrap carbine, firearm 2 uses a compact representation, and melee uses
a simple scrap-tool representation. Empty slots hide the same renderer; no item
is duplicated, equipped or consumed by the visual system. Switching/reloading,
unlimited melee and damage rules are unchanged. These are appearances for existing
items, not new gameplay weapons or an equipment-system rewrite.

### F. Parachute landing closure

The authoritative state already cleared on the next grounded FixedUpdate probe.
The presentation gap was addressed without inventing another deployed boolean:
supporting-solid contact callbacks now clear that same traversal state immediately
after physics. Side/ceiling contacts and non-solid actor layers do not qualify;
normal.y > 0.65 and non-upward velocity are required. ClearParachute also applies
the existing redeploy delay. FixedUpdate grounded clearing remains as a fallback.
The cutout canopy and mobile UI continue to read IsParachuting. Existing toggle
edges, airborne horizontal penalty/descent cap and recovery/ladder clearing remain.

### G–H. Assets, animation and files

Added 24 small PNGs under Assets/Art/Sprint009: revised body parts and face overlays,
two equipment representations, two ladder caps. Point/no mip/uncompressed/32 PPU
and existing sorting are preserved. The warehouse tileset and primary weapon/
canopy artwork were not regenerated. No historical art was recovered or sampled.
ART_MANIFEST.md records dimensions, pivots, current bindings and review status;
the general aesthetic approval is not treated as legal clearance.

Main files: CutoutRig, CharacterVisualDriver, CharacterSkin, CartoonRagdoll,
PlayerActions, PlayerTraversal2D, new LadderVisual2D and editor authoring helper
Sprint009PresentationSetup; both gameplay/visual prefabs, ragdoll prefab, skin
assets and Warehouse_Testbed. Added Sprint009RigTests and Sprint009TransitionTests.
Two earlier assertions were updated to require visible climbing equipment and
allow the new independently authored asset directory; their test names/coverage
remain present.

### I. Automated and integrated live validation

Seven focused new cases cover proportional limits/expressions/pivots, live
collider/COM/neck alignment, ladder caps/endpoints, climbing equipment/empty slots,
ground landing/UI/held input, elevated landing/ladder/recovery and pose-preserving
ragdoll activation with further impacts. Full final results are recorded in
TestResults/Sprint009.json. Final full run: **32/32 Edit Mode + 72/72 Play Mode =
104/104 passed**, no skips/failures. All 97 prior test names are retained. The
post-warning-fix rerun also passed; compilation succeeds and final scene/prefab
checks report zero missing scripts and valid rig bindings.
The final Console retained one Unity service warning: "Account API did not become
accessible within 30 seconds. This may be due to network issues or editor focus."
It did not prevent compilation or tests; no account settings were changed.

Live warehouse observations (runtime setup discarded afterward):

- New Larpers were inspected walking/aiming/falling; a surprised expression and
  head-following "CAN WE MAKE THIS A PRIVATE LOBBY?" line were visible.
- Airborne ladder acquisition: climbing=true, gravity=0, equipped weapon visible,
  measured carrying-hand/mount gap 0. Slot 2 and melee remained visible as their
  distinct sprites. The opposite hand continued its climb cycle.
- Automatic upper exit reached (13.76,6.11), climbing=false, normal gravity 3.
- Ground parachute landing: state=false, canopy=false, horizontal multiplier 1,
  gravity 3 and mobile label DEPLOY CHUTE. Jumping with the deployment input still
  held did not redeploy; upward jump velocity remained normal.
- Elevated landing at (16,6.11) also closed state/visual and restored gravity 3.
- Revised Larper defeat: one body, initial head position gap 0; fatal plus later
  impact count 2, head mass 0.18. The body tumbled onto the warehouse floor with
  the new connected humanoid silhouette. No repeated defeat was observed.

These are Editor observations and automated checks, not subjective art/comedy
approval. Existing tests exercise additional side/bottom exits, authored enemy
routes, airborne grabs, crouching, recovery, sleep/wake and impact/cleanup behavior.

### J–K. Remaining review and Sprint 010

Matching Android Build Support remains unavailable; no physical-device validation,
module installation, build target change, FPS or memory claim. Mobile control
positions/safe-area behavior were not changed; existing landscape/layout tests
remain part of the full suite. Human review is still needed for small expressions,
one-handed poses, upper handrail lengths and the new mass distribution. Equipment
art remains a small generic representation set; no new weapon roster was added.

Sprint 010 should start with developer playtesting of these proportions and
transitions, then Android deployment/multitouch/frame-pacing checks once support
exists. Refine only demonstrated readability or physics problems; retain the
working authority boundaries and avoid adding anatomy/animation complexity solely
to make the cutouts more realistic. Publication/provenance review remains pending.

No commit or push. Final verification confirms Warehouse_Testbed saved, open,
clean and outside Play Mode. No compilation errors or current Console errors.

## Sprint 011 — Combat readability, tactical reloads and manual parachute (2026-10-18)

### Baseline and scope

The connected Editor was Unity 6000.6.6f1, Warehouse_Testbed open and stopped,
with no current Console errors/warnings or compilation failure. The latest disk
log/results were Sprint 009; no Sprint 010 implementation/results were present.
Before implementation, the actual suite passed 32 Edit Mode + 72 Play Mode =
104/104. Existing modified/untracked project files were preserved. No historical
asset investigation, new artwork, packages, campaign, weapon roster or enemies.

### A. Gun and aiming marker

PlayerActions retains independent touch/right-mouse aiming and all control math.
AimDirectionMarker is inactive in the player prefab and runtime normal gameplay.
The optional serialized debugAimMarker flag defaults false. The right aiming
stick, firing zone and touchscreen feedback remain. Visible equipment, horizontal
mirroring, upright backward aim and recoil shortening of the carrying arm remain.

### B. Explicit muzzle, truthful hitscan and bounded tracers

CutoutRig has an explicit Muzzle child on the weapon; its local anchor follows
the selected sprite's far X edge and barrel centreline (1/32 unit above the grip
axis, respecting SpriteRenderer.flipY because pixel flips do not move child
transforms). It is authored in both gameplay prefabs and created once if a replacement
rig lacks it. AttackExecutor poses the rig synchronously using the actual shot
direction, including enemy spread, before reading the world muzzle. A brief shot
pose holds that direction across Update/LateUpdate. Recoil translates/shortens the
arm rather than rotating the barrel away from the authoritative ray. Melee keeps
its existing short-range body-origin rule.

Before casting from the muzzle, a root/posture-centre-to-muzzle solid query checks
cover. The weapon and carrying hand retract together to 0.015 unit before a
blocking surface; the actor/collider is not moved. The outgoing hitscan then starts
at that actual visible muzzle and stops at the nearest valid collider. Existing
owner exclusions, geometry/living/ragdoll layers, damage filters, actual-point
impulses and miss ranges remain. Muzzle flash uses this anchor too.

Each executor preallocates a ring of six LineRenderers and reuses it, with shared
material and no per-shot GameObjects or physical bullets. Inspector controls:
tracerSeconds 0.35 (up to 1 second), tracerWidth 0.035, maximumTracers 6 (1–16),
friendlyTracer warm gold, enemyTracer red, both default alpha 0.85. Alpha fades
over lifetime; oldest trails are reused when full; disabling an actor clears them.

### C. Ragdoll tuning

Read baseline Inspector values before tuning. initialDefeatMultiplier 2.0 → 2.6;
postDefeatMultiplier 1.2 → 1.56 (+30% each), in the source defaults and
LarperCartoonRagdoll prefab. No other physics multipliers were blindly scaled.
maximumImpulse remains 2.5; linear/angular caps 12/720; masses remain
0.78/0.18/0.13/0.13/0.24/0.24. Damping 0.5/1.2, limited joints, continuous collision,
sleep/wake, location-dependent torque, four-ragdoll budget and 45-second lifetime
remain. The usual 0.9 shot gives uncapped impulses 2.34 initially and 1.404 later.
Live inspection showed a connected tumble and later response; developer
playtesting must still judge the subjective strength/comedy.

### D. Manual parachute authority and closure audit

Sprint 011 explicitly supersedes Sprint 009 automatic landing closure.
IsParachuting is the single deployment state: press deploys, release keeps it,
next press retracts. Removed grounded FixedUpdate and supporting-contact closures,
and attachment-to-ladder closure. Airborne horizontal penalty/descent cap use
ParachuteFlightActive (deployed, not grounded, not climbing). Ground movement and
gravity are normal while deployment/canopy/UI remain consistent. Climbing has
gravity zero and ordinary traversal speed; deployment survives the transition.
The canopy stays visible when manually deployed; climbing/ground locomotion still
use their proper poses, and intentional aiming/reloading can keep equipment shown.

Both PlayerActions and WarehouseTouchControls had independent focus/pause reset
paths. They now release/suspend inputs and cancel held firing/reload without
clearing deployment. Explicit ResetActions/ResetTraversal, component/controller
disable or reinitialization, LevelEntry teardown/rebinding, defeat recovery and
respawn intentionally clear deployment. No timer or animation event retracts it.
ClearParachute/manual close retains the existing redeploy delay.

### E. Existing Larper combat extended with bounded withdrawal

EnemyCombat still owns detection, imperfect sampled aim, reaction time, last-seen
memory, actual-shot-direction obstruction checks and finite WeaponCycle ammo.
It sustains fire at the equipped weapon's cadence, then commits to Withdrawing →
Reloading → re-engagement. Ordinary/cowardly/overconfident reload fractions default
0.2/0.4/0 of magazine capacity; these are configurable on the same controller,
not separate personality AIs. Overconfident enemies therefore expend longer
magazine bursts; cowardly enemies withdraw earlier. Existing speech/morale and
poor distant accuracy remain.

Withdrawal chooses away from last observed pressure once, through LarperBrain's
existing pursuit/motor. CanWithdraw checks local floor and wall safety; tactical
walking suppresses obstacle jumps. Retreat ends at 2 units, broken sight, an
unsafe/blocked route or 0.8 seconds, with checks every 0.18 seconds. A blocked,
ledge or stationary case reloads in place. There is no global cover/path search
or promise of optimal cover. Re-engagement waits 0.35 seconds and requires the
existing visibility/reaction rules; lost targets are not perfectly tracked.

Reload continues after sight/range loss; only WeaponCycle.Tick transfers ammo at
the real deadline. Panic, defeat/disable and authored ladder ownership cancel
reload/withdrawal without transfer. Ladder ownership also clears stale pursuit.
Empty reserves do not create ammo or loop through reload attempts. Timeout,
bounded movement and the re-engagement delay prevent permanent retreat and rapid
advance/retreat oscillation. Existing warehouse spawns inherit this behaviour;
no scripted mandatory retreat cycle or new encounter was added.

### F. Authoritative reload handling poses

WeaponCycle exposes start/deadline/progress. CharacterVisualDriver reads the
currently selected player weapon or enemy weapon cycle, then CutoutRig adds a
short handling dip and support-hand strokes over that exact interval. The gun
remains visible/attached while moving or climbing. The pose clears on completion,
switching, cancellation, defeat and recovery; no animation timer grants ammo.
Existing facial reactions remain event-driven. No new Animator controller.

### G. Files

Changed runtime scripts:
- Assets/Scripts/Combat/AttackExecutor.cs, CombatFeedback.cs, WeaponCycle.cs
- Assets/Scripts/Enemies/EnemyCombat.cs, LarperBrain.cs
- Assets/Scripts/Input/PlayerActions.cs, WarehouseTouchControls.cs
- Assets/Scripts/Traversal/PlayerTraversal2D.cs
- Assets/Scripts/Visuals/CutoutRig.cs, CharacterVisualDriver.cs, CartoonRagdoll.cs

Changed prefabs: Assets/Prefabs/Player/WarehousePlayer.prefab,
Assets/Prefabs/Enemies/Larper.prefab,
Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab.
Warehouse_Testbed was saved through Editor APIs; no authored geometry was changed.

Tests: new Assets/Tests/PlayMode/Sprint011CombatTests.cs (+ Unity-generated meta);
updated MovementCompletionTests.cs, Sprint009TransitionTests.cs,
VisualIntegrationTests.cs, WeaponPresentationTests.cs in that same directory.
Records: TestResults/Sprint011.json, TestResults/Sprint011Live.json and this log.

### H. Automated and live verification

Final full results: **32/32 Edit Mode + 81/81 Play Mode = 113/113 passed**, zero
failures/skips. Compared names against Sprint009.json: all 104 original names
retained; nine new focused cases. Existing landing/ladder canopy assertions were
updated for the explicitly superseding manual preference, with legacy names
retained/commented. The lower firing-origin/muzzle-flash tests now verify the
actual muzzle rather than the old torso origin/approximate tip.

New coverage: all six shot orientations in both facings and bounded trail expiry;
clipped-barrel/nearest-cover blocking; authoritative moving reload and switching;
both actor/UI focus resets versus explicit recovery; exact +30% impulse and caps;
withdraw/reload/lost-sight/re-engage; blocked retreat with panic/defeat cancellation;
empty reserves/personality thresholds; panic during withdrawal and ladder route
preemption. Existing traversal, ledge, ladder, checkpoint, damage, sleep/wake,
ragdoll collision/budget/lifetime, input and visual coverage all remain passing.
The first new orientation fixture used an extreme world coordinate, which lost
precision on a tiny diagonal floor-contact ray; it was moved near the origin.
An initial old automatic-canopy assertion was corrected, then the full suite rerun.
A final visual review also corrected the muzzle's vertical offset when the sprite
is flipped (SpriteRenderer flips pixels, not child transforms). The existing
orientation case now checks the visible mirrored barrel as well as the anchor;
the full suite was rerun after this correction.
The corrected build also repeated the twelve live standing/crouching orientations:
visible mirrored barrel gap and muzzle gap both zero, direction dot one in all
cases. A final flipped enemy spread shot measured both gaps zero and direction
dot approximately one. These final receipts are included separately in the live
record so the earlier observations remain identifiable.

Live MCP-controlled Warehouse_Testbed observations, runtime setup discarded on
exit, receipts in Sprint011Live.json and inline captures visually inspected:
- Twelve standing/crouching shots (left/right/up/down/both diagonals): immediate
  muzzle gap 0 and barrel/shot dot 1. Missing shots reached range; geometry and
  actors stopped rays. Player marker absent, right aiming control still visible.
- Actual crate at x21: crouched origin (20.49,0.54), first hit (20.50,0.54), geometry.
- Player real reload at progress ~0.335: pose/weapon visible, ammo 1/reserve 36.
- Overconfident Larper: five shots, withdraw x5 → x7.10, real reload while shot
  count held at five, refill six/reserve 36 → 30, re-engage and sixth shot.
  Enemy reload pose and weapon both visible; spread shot muzzle gap 0, barrel and
  line dot 1. Panic accepted and cancelled reload (full interruptions also tested).
- Elevated landing (16,6.11): deployment/canopy true, gravity 3, horizontal 1,
  UI RETRACT CHUTE; second press closed it.
- Airborne ladder grab: climbing true/gravity 0, visible weapon and reload pose,
  hand gap 0. Upward climbing shot had muzzle gap 0/barrel dot 1. Side dismount
  restored normal gravity 3.
- Practice bay: one defeat spawn, six bodies/five connected joints, two impacts;
  observed max speed 5.94/angular 236.56 after the later hit, lifetime 45. Further
  simulation remained finite/within caps; total active ragdolls two (earlier corpse
  included), within the unchanged four-ragdoll limit.
- Existing mezzanine checkpoint trigger selected (0,6.25); recovery returned
  there, cleared an actually deployed canopy and restored gravity 3.

Final Editor audit: Warehouse_Testbed open, saved/clean, Play Mode stopped;
30 scene colliders, valid rig bindings, zero missing scripts or missing serialized
object references across scene and gameplay/ragdoll prefabs. Compilation succeeds;
current Console errors/warnings zero. An audit initially tried the obsolete
instance-ID reflection accessor; retried successfully with Unity 6 entity IDs.

### I–K. Android, limits and Sprint 012

No physical bullets, per-shot instantiation or per-frame scene searches. Tracers
are bounded/reused; tactical safety sampling is limited; existing motor/physics,
ragdoll simulation limits and art assets are reused. These are design constraints,
not measured Android FPS/GC claims. Matching Android Build Support remains absent
in this Editor, so no Android build, USB/multitouch, frame pacing or hardware
performance validation was performed; target/modules/settings were not changed.

Local retreat is intentionally simple and may choose imperfect cover; when unsafe
it reloads in place. Finite reserves can leave an enemy dry. Very close cover
compresses the carrying arm/gun visually rather than moving gameplay colliders.
Art/comedy, reload readability at phone scale, impulse strength and encounter
balance need developer hands-on review; live inspection is not subjective signoff.

Sprint 012: playtest these exact combat cycles and close-cover/diagonal poses;
tune only demonstrated issues. Install matching Android support when desired,
then validate actual device multitouch and profile tracers/AI/ragdolls before
adding content. Continue publication/provenance review of existing artwork.

No commit or push. Warehouse_Testbed left saved, open and outside Play Mode.

## Sprint 012 — The incompetent gunfight (2026-10-18)

Baseline verified before changes: 32 Edit + 81 Play = 113 passing tests in
Unity 6000.6.6f1. Current scene was Warehouse_Testbed; compilation/Console clean.

### A–C. Buzzbox pistol and conspicuous reload

Basic Larper now equips a separate LarperBuzzbox definition: 18 rounds, 0.105s
cadence, damage 5, 18-unit ray range, 11-unit effective engagement range, 2.6s
reload, 54 reserve rounds. Ordinary/cowardly/overconfident thresholds retain
0.2/0.4/0, yielding roughly 15/11/18-shot bursts. Actual spread grows from 6°
close to 32° distant. Existing rifle/sidearm definitions and skins remain intact.
WeaponDefinition now optionally supplies equipped/magazine sprites; other weapons
fall back to their previous presentation.

Two original code-authored pixel assets: BuzzboxPistol 20×8 and separate
RidiculousMagazine 8×18, point/no mip/uncompressed/32 PPU. No historical image
input or new visual style. Magazine mirroring follows the gun, with no physics.
The existing rig now makes oversized-magazine reloads dip/wobble up to ~75°,
move the magazine outward/down by up to 0.42 unit and exaggerate support-arm
strokes. All motion reads real WeaponCycle progress; no visual ammo authority.
Player reload and carrying/climbing remain covered by prior regressions.

### D–F. Callouts and actual retreat

Root causes: the previous retreat stopped at its first lost-sight sample, even
before meaningful travel; low-ammo decisions were skipped after sight loss;
the short 0.8s window and nearby walls/edges often produced minimal travel.
The controller now preflights a local floor/wall corridor, requires 2.5 units
of safe space, aims for 3.5 units and permits 1.8s of committed movement. Breaking
sight only ends retreat after minimum travel. Recent last-seen information can
start the decision. Safety is sampled every 0.18s; panic, defeat and authored
ladder ownership still interrupt. Short/blocked routes reload in place rather
than claiming to retreat or deadlocking. No global navigation framework.

WithdrawalStarted fires only after 0.25 unit of actual movement; ReloadStarted
and WithdrawalFailed drive the existing awareness/bubble system. Profile pools
include RELOADING!, FALL BACK!, COVER ME!, MY MAG'S EMPTY!, professional-sounding
overconfident excuses, frightened cowardly lines and NO ROOM! RELOADING HERE!.
Sequential variants avoid immediate self-repetition. Combat callouts use a
0.7s encounter gap, 0.65s per-speaker gap, four-second identical-line suppression
and two-bubble admission limit. They temporarily outrank ordinary banter; existing
rare panic/ordinary gates remain. Facial reactions reuse existing expressions.

### G–H. Functional cover and warehouse population

Added LowCover_Sprint012 at (-14,0.45), size 1.6×0.9, with matching visual/collider.
All previous 30 colliders remain unchanged. The crouched muzzle has a configurable
ceiling at root +0.02; upward aim cannot lift it beyond the crouched silhouette.
Close-cover alignment bends the arm toward the grip rather than detaching the
shoulder. Obstruction checks follow torso → shoulder → muzzle: the previous direct
diagonal could falsely block a standing gun above low cover. Hitscan remains
available while crouched, with real nearest-hit collision and no invulnerability.

Four configurable spawn points added at (6,0.85), (18,0.85), (-6,6.25), (16,12.85),
bringing spawnOnStart actors to seven across ground/upper/ladder situations.
LevelEntry binds all points. Existing optional spawns/checkpoints/routes remain.

### I–J. Files and verification

Runtime: WeaponDefinition, AttackExecutor, EnemyCombat, LarperBrain,
LarperAwareness, LarperPersonalityProfile, LarperSpeechBubble, CutoutRig,
CharacterVisualDriver. Added Assets/Editor/Sprint012CombatSetup.cs; new pistol
definition/two art assets; changed Larper.prefab, three personality assets and
Warehouse_Testbed. Updated ART_MANIFEST.md and this log.

New Sprint012RulesTests (3) and Sprint012CombatTests (4); existing
Sprint008PresentationTests explicitly permits the new art/one added cover collider
while still checking every previous collider. Sprint009TransitionTests compares
Unity native sprite equality rather than CLR wrapper identity after asset reload.
Final **35/35 Edit + 85/85 Play = 120/120 passed**, no failures/skips. All 113 prior
names retained. Results and raw live receipts: TestResults/Sprint012*.json.

Live Editor checks used authored spawn positions, not repositioned AI:
- Seven actors, all Buzzbox equipped; entry player at x=-24 remained health 100.
- Multiple actors fired bursts (observed totals 30/54/22/45), actual barrel/ray
  direction dot 1 and muzzle gap 0 on sampled shots.
- Ordinary Larper: 15 shots → FALL BACK! → 3.68-unit withdrawal → RELOADING! →
  refill/reserve reduction → return to fire. Next withdrawal measured 3.71 units.
  Blocked/edge-constrained neighbours reloaded in place with appropriate callouts.
- Reload pose and long magazine visible; sampled magazine offset -0.402 unit.
  Group observation peaked at two speech bubbles. Inline captures inspected.
- Saved cover: crouched origin y0.55 stopped at x=-14.80, target stayed health50;
  standing origin y1.05 hit target, health30. Incoming shot with cover left player
  health100; temporarily removing it allowed damage to95, protection disabled.
- Initial follow-up player shots did not establish a ragdoll defeat. A separate
  isolated live hitscan emitter then verified 50→30→10→0, exactly one ragdoll,
  six bodies/five connected joints, finite positions and speed within the cap.
  Existing full-suite tests retain actual player/ragdoll impact and cleanup checks.

All runtime setup discarded afterward. Final audit: scene saved/open/stopped,
31 colliders, zero missing scripts/serialized references, compilation succeeds,
current Console errors/warnings zero. Authoring initially exceeded MCP's five-second
main-thread window; completed via deferred Editor execution and Unity null checks.

### K–L. Performance and remaining review

Hitscan and six-trail pools retained; magazine adds one cosmetic renderer per
Larper, no Rigidbody/projectiles. Corridor probes run at retreat commitment, safety
checks at 0.18s; callout labels are reused. Ragdoll anatomy, multipliers, caps,
four-ragdoll budget and lifetime unchanged. Android Build Support still absent;
no device build, mobile FPS/GC/multitouch or phone readability claim. Group rendering
was observed in the Editor, not benchmarked on hardware.

Developer review is still needed for pistol/spread balance, exaggerated reload
readability, dialogue density and collision-safe retreat fallbacks. Local actors
cannot always find 2.5 units of safe retreat; in-place reload is intentional there.
Next: hands-on warehouse testing, then Android support/device profiling before
increasing density or adding effects. No unrelated systems or new enemy classes.

No commit or push. Warehouse_Testbed saved, open, clean and outside Play Mode.

## Sprint 012.1 — Last-known-position panic fire (2026-10-18)

Baseline reverified: 35 Edit + 85 Play = 120 passed. Root cause: the existing
lastSeen value already froze on lost visibility, but Update immediately returned
Searching/Patrol and the geometry precheck suppressed shots toward obstructed aim.

EnemyCombat now rolls once on visible→hidden during a recently firing Engaging
burst. It uses existing lastSeen/sample/aim logic and a bounded BlindFiring state.
Defaults: ordinary chance 0.70, overconfident 0.95, cowardly 0.40; timeout 2s,
cowardly duration factor 0.45 (0.9s). These are serialized Inspector settings.
No acquisition without genuine sight/recent successful fire; no refresh while
hidden, no refill on sight loss, no repeated roll every frame. Reacquisition
updates memory legitimately and restores ordinary targeting/reaction rules.

Blind shots skip only the AI's no-fire-when-obstructed precheck. AttackExecutor
still owns muzzle positioning, spread trajectory, nearest-hit collision, damage,
cadence and ammo. Thus walls receive actual hits instead of being bypassed.
An empty magazine leads into existing safe retreat/reload when reserves remain;
timeout with rounds remaining returns to existing search/patrol. Reload,
withdrawal, panic, defeat, rebinding and ladder ownership cannot be displaced by
the continuation. CharacterVisualDriver keeps remembered aim visible between shots.

Modified files: Assets/Scripts/Enemies/EnemyCombat.cs,
Assets/Scripts/Visuals/CharacterVisualDriver.cs,
Assets/Tests/PlayMode/Sprint012CombatTests.cs (four new cases), and
CombatIntegrationTests.cs (explicit chance-zero configuration for its existing
immediate-return-to-patrol branch). No weapon/art/scene geometry or prefab edits.
Records: TestResults/Sprint012_1.json and Sprint012_1Live.json, plus this log.

Final suite: **35/35 Edit + 89/89 Play = 124/124 passed**, no skips/failures.
New cases cover frozen hidden memory/cover/ammo, timeout without refill, empty
magazine→physical withdrawal→reload, and never-seen/declined continuation.
All previous coverage remains; baseline names were retained.

Live Warehouse_Testbed used the existing crate at x21, temporary diagnostic actor
placement/protection and default probabilities for overconfident/ordinary trials.
Overconfident fired 16 blind rounds, ten geometry hits, emptied its magazine,
withdrew x17.03→14.20 and reloaded 18 rounds (reserve54→36). Ordinary also showed
blind fire (eight rounds/six geometry hits across later losses) and reached reload.
Both trials included later legitimate reacquisitions, so their aggregate memory
comparison was not a valid continuous-hidden measurement. A separate controlled
ordinary trial (chance temporarily1, then discarded) measured an uninterrupted
hidden interval: last-known (20,0.71), player moved to (26,0.71), memory gap0,
five extra shots/five ammo spent, final hit Geometry. Inline Game capture showed
the muzzle-aligned spread and crate impacts. No hidden-position damage shortcut.

No new per-frame scene search, physics or effects. Android hardware validation
remains unavailable. This is temporary burst memory, not search navigation;
chance-based declines and timeout exits are intentional. Phone-scale balance
still needs developer playtesting. Final scene saved/open/clean, Play Mode stopped,
compilation successful, zero current Console errors/warnings and missing scripts.
No commit or push; recovery reserve preserved by stopping after verification.

## AKUMU 2 — Sprint 012.2: Larper distance, panic and cover (2026-10-19)

Extended the current EnemyCombat/LarperBrain, retaining hitscan, real muzzle
collision, pooled tracers, health, existing traversal and all prior tests.
No game/folder rename, new artwork, enemies, combat effects or physics systems.

Tuning on the Larper prefab and LarperBuzzbox asset (Inspector-configurable):

| Parameter | Before | After |
| --- | --- | --- |
| Visual detection | 14 | 28 units |
| Engagement cutoff | effectiveRange 11 | separate engagementRange 24 |
| Accuracy distance | 3–11 | unchanged 3–11 units |
| Close / distant maximum angular spread | 6 / 32 | 3 / 40 degrees |
| Hitscan reach | 18 | 28 units |
| Reload | 2.6s | 4s, enemy variation ±5% (3.8–4.2s) |
| Intended retreat / timeout | 3.5 units / 1.8s | 6 units / 4.5s |
| Cover search | none | local 7 units, at most 32 candidates |
| Reaction jitter / extra blind spread | none | 0–0.05s / 3 degrees |

Magazine 18, cadence 0.105s, damage 5, effective accuracy distance 11 and
personality reload fractions (ordinary .2 / coward .4 / overconfident 0) retained.
Blind-fire chance .70/.40/.95, finite 2s window (coward .9s), frozen hidden memory,
real ammunition and legitimate reacquisition are preserved. Longer range is
separate from accuracy; distance continues to worsen spread.

EnemyCombat performs one bounded cover query when committing to withdrawal.
Candidates must block a line from the last seen threat to a crouched destination,
have a supported corridor and safe endpoint, and fit existing motor jump/headroom
limits. LarperBrain reuses normal walking/jumping for small obstacles (default
maximum 1.6 high / 1.8 wide). No teleporting or per-frame path search. Buffered
physics queries avoid new combat allocations. Plain six-unit walking fallback,
shorter safe displacement or in-place reload remain available when cover fails.

The prefab now includes existing PlayerPosture2D. Combat requests crouch only at
a real grounded cover endpoint during reload, then requests safe standing at
completion; existing visual rig/reload animation and callouts remain active.
WeaponCycle accepts a per-reload duration scale without editing shared weapon
definitions. Its existing Tick remains the only ammunition transfer authority.
Target loss cannot cancel or instantly complete committed reloads.

Live verification exposed and fixed two integration problems: Checkpoint_Exit's
trigger was treated as a solid wall by retreat probes, and a projected cover
position could permit crouching on top of a crate. Retreat probes now explicitly
ignore triggers, while crouching requires actual grounded arrival near the
original height. Endpoint stopping runs between tactical samples to prevent
overshoot; waiting for a hop to settle is bounded by the retreat timeout.

Code: EnemyCombat.cs, LarperBrain.cs, WeaponCycle.cs; authoring helper
Assets/Editor/Sprint0122AiSetup.cs. Authored asset changes are Larper.prefab
(tuning/posture) and LarperBuzzbox.asset (reload/reach). Player/ragdoll prefabs,
warehouse layout, geometry, artwork and UI were not changed. Existing unrelated
renderer/project-settings changes were left intact.

Validation: fresh baseline was 35/35 Edit and 88/89 Play. One old Sprint005
immediate-occlusion assertion conflicted with probabilistic Sprint012.1 blind
fire; explicitly selecting the chance-zero branch preserves that test's original
assertions and newer blind-fire coverage. The first implementation run was
36/36 Edit and 92/94 Play: an old .4s random-hit assertion and a new post-reload
ammo sample raced reaction jitter/legitimate resumed shooting. These now use a
bounded time-to-hit check and isolate committed reload completion after target
loss. Other reload-pose/deadline waits follow the configured duration.

Final full suite after all integration fixes: **36/36 Edit + 94/94 Play =
130/130 passed**, no failures/skips. All **124 previous test names retained**;
six new tests cover distant fire/poor accuracy, real point-blank damage, six-unit
fallback/four-second vulnerability, reachable cover/physical hop/crouch/trigger
handling/standing, blocked headroom fallback, and authoritative varied reload
deadlines/ammo/shared-definition safety. Existing frozen-memory, cover collision,
navigation, inventory, ragdoll and scene tests pass. Records:
TestResults/Sprint012_2.json and Sprint012_2Live.json (including baseline failures).

Actual Warehouse_Testbed Play Mode checks used temporary actor placement,
protection and camera framing. A visible player 22 units away drew six shots
(18→12 ammo); inline Game View showed the broad spread. Final cover trial moved
x16→22.298 (6.298 units), physically hopped Crate_21, settled at y.71 and crouched
behind it. Reload duration was 4.091s, with zero additional shots during reload;
completion restored 18 rounds, reserve54→36 and standing/exposure. Captures were
viewed inline; no saved screenshot or human-input playtest claim. All temporary
runtime changes were discarded. Final audit checked 80 scene/prefab behaviours:
zero missing scripts/references, all 31 warehouse colliders retained. Warehouse
saved/open/clean, Play Mode stopped; compilation successful and current Console
has zero errors/warnings (historical buffer entries are not current errors).

Limitations: cover planning is conservative and local, not general navigation;
full walls, gaps, broad crates, blocked headroom and saturated queries fall back.
It may reload exposed when no safe route exists. Existing pursuit can expose or
jump toward the target again after standing. Optional desperation melee was
deferred: player melee exists, but enemies lack a compatible equipment-switching
path, so adding it would exceed this refinement sprint. Android/device profiling
and phone-scale balance are unverified. No new dependencies or artwork.

Manual playtest checklist: approach a distant Larper and compare its spread at
close range; break sight, move behind cover and confirm frozen panic aim/timeout;
watch an empty enemy retreat behind a crate, crouch for about four seconds, then
stand/expose; repeat beside a wall, ledge and checkpoint trigger; exchange shots
over low cover standing/crouched and verify real muzzle obstruction. Minor
follow-ups: tune the 40-degree spread, reload vulnerability and short post-reload
exposure behavior from hands-on warehouse testing, then profile on Android.

No commit or push. Usage reserve retained; no Sprint 013 work begun.

## AKUMU 2 — Sprint 013: Camera-aware combat and colorful feedback (2026-10-19)

Incrementally extended existing combat, preserving hitscan, damage/collision,
weapon cadence, magazine/reload rules, panic memory, cover navigation and ragdolls.
Larper prefab detection 28→22 units; engagement 24→18 units. Accuracy remains
3→40 degrees over the existing distance curve. Magazine18, cadence .105s,
damage5, reload4s ±5% and retreat6 units are unchanged.

EnemyCombat now gates the first successful firearm attack using the actual
gameplay Camera.main (cached, with bounded retry if missing) or explicitly bound
gameplay camera. GameplayViewport projects collider body bounds, allowing partial
body entry even when the pivot is outside. It checks depth, active camera and the
visible torso's culling layer, which differs from the collision root's layer.
No Renderer.isVisible/Scene-view visibility dependency. Existing geometry LOS,
range, reaction, ammo, reload and traversal gates still apply: viewport overlap
alone never authorizes shooting through a wall. Missing/disabled cameras fail
closed for fresh enemies.

Only a successful legitimate shot establishes engagement authorization. It
persists through camera movement during the same awareness episode, so moving
the viewport cannot flicker/reset combat or suppress valid panic fire. Binding a
new target, losing tracking beyond existing memory/range rules, and re-enabling
reset authorization. There is no visibility grace for enemies that never fired.
Actual sight still updates awareness off-screen; viewport permission controls
initial attack, not enemy knowledge. Existing finite frozen last-known-position
bursts and genuine reacquisition are retained.

AttackExecutor keeps exact hitscan endpoints and its six-slot LineRenderer pool.
Tracers now last .17s instead of .35s, use cyan player/orange-red enemy colors,
subtle brightness/width variation and a fading tail. Live inspection discovered
the old URP/Unlit material ignored vertex colors/alpha, leaving every line yellow.
CombatTracer.mat now uses the existing compatible Sprites/Default shader with a
white tint and transparent queue; visual captures verified real color/fade.
No physical bullet objects or damaging ricochet mechanic were added.

CombatFeedback extends existing muzzle/surface cues with varied warm flashes,
a brief optional translucent sprite glow (no dynamic lights), and a reusable
12-sprite particle ring per actor. Explicit ImpactSurface2D metadata classifies
the warehouse's 24 solid colliders: gray concrete dust/debris, yellow metal sparks
with occasional longer cosmetic streaks, brown wood splinters/puffs; unclassified
surfaces reliably use a blue-white generic fallback. Classification comes from
the actual nearest hit collider and normal. All 31 colliders remain unchanged;
no destructible environment, new geometry or new art was introduced. Existing
point-filtered placeholder effect sprites are reused, with replaceable assets.
CharacterVisualDriver adds a directional 3-degree/.1s head/torso flinch and shorter
warm hit tint; physics bodies, colliders and ordinary hit knockback are unchanged.

Audio integration only: suitable project clips are absent. The asset inventory
contained package voice/demo clips, not usable combat sounds; none were repurposed
or downloaded. CombatFeedback has separate concrete/metal/wood/fallback impact
slots plus existing firearm, hit, hurt, reload, empty and defeat slots. Frequent
combat playback uses two reusable voices per actor, a global 12-voice cap,
controlled ±7% pitch variation and existing volume .5. Reload sound is triggered
by the real reload-start event at pitch1. Unassigned clips are silent, not errors.
The pre-existing rare detached defeat cue path remains unchanged. Required assets:
short distinct Buzzbox/carbine shots, metal spark/ricochet, wood thud/splinter,
concrete dust/debris and generic impact clips, awkward four-second reload/handling,
empty click and optional hit/hurt/defeat cues. Audio quality/mixing was not validated.
The automated voice test uses a temporary silent in-memory clip, not shipped SFX.

Created production sources: Assets/Scripts/Camera/GameplayViewport.cs,
Assets/Scripts/Combat/ImpactSurface2D.cs and Assets/Editor/Sprint013Setup.cs.
Modified production sources: EnemyCombat.cs, AttackExecutor.cs, CombatFeedback.cs
and CharacterVisualDriver.cs. Authored assets: Larper.prefab,
WarehousePlayer.prefab, CombatTracer.mat and Warehouse_Testbed.unity (surface
metadata only). Tests: new Sprint013RulesTests.cs, Sprint013Tests.cs and
CombatTestView.cs; existing CombatIntegrationTests, Sprint005Tests,
Sprint011CombatTests, Sprint012CombatTests, PersonalityIntegrationTests and
VisualIntegrationTests now frame/bind actual fixture cameras. The earlier
configurable long-range case explicitly requests its former 28/24 settings,
preserving that coverage while new tests assert the 22/18 defaults. Prior sprint
and unrelated renderer/project-settings modifications were preserved.

Live Warehouse_Testbed checks used temporary diagnostic actor placement,
protection and camera framing, including pinned actors for the camera experiment.
At 17 units, a fresh off-screen enemy had clear LOS and 18 rounds but fired zero
shots. Panning it into the gameplay view produced six shots (18→12 ammo). Panning
away after engagement produced three further shots (12→9), preserving combat.
After the shader correction, a three-enemy burst produced 7/7/8 shots, with only
1/1/2 active tracers at capture, each bounded by six tracers and 12 particles.
Metal sparks were visible; the busiest actor had eight active particles. Cyan
player rays visibly terminated at Crate_21 (wood x20.5/y.55), BasementPipeBridge
(metal x-1.84/y-5.43) and BasementFloor (concrete x4.16/y-7). Inline Game captures
were inspected for flash anchors, color, endpoint and effect readability. These
were scripted live Editor checks, not hands-on input or hardware profiling. No
saved screenshot is claimed. Runtime overrides were discarded on Play Mode exit.

Performance remains bounded: no per-frame cover search, physical bullets or new
render pipeline/dependencies. Viewport projection needs no scene-wide physics
search and is skipped once the encounter is authorized. Particle and tracer
buffers are allocated on actor initialization and reused. A cheaper Inspector
configuration is maximumImpactParticles4, maximumTracers3 and muzzleGlowEnabled
false; profile before choosing a device preset. Android/device FPS, allocation
rates, audio mix and touch readability remain unverified. Concrete contrast on
gray floors, muzzle size and group density still need hands-on tuning.

Recommended Sprint 014: supply licensed/original combat clips and tune their mix,
profile the bounded effects on supported Android hardware, and perform hands-on
camera-edge, close/distant fire, low-cover and multi-enemy readability checks.
No new weapons, enemy classes, levels, explosives, armor or physics overhaul.

Final validation after shader and visual-layer integration fixes: **38/38 Edit +
100/100 Play = 138/138 passed**, no failures/skips. All **130 previous test names
retained**; eight new tests cover Sprint013's camera, effects, audio integration,
flinch, tuning and scene contracts. Existing reload/cover/navigation/ammo/ragdoll
regressions pass. Records: TestResults/Sprint013.json and Sprint013Live.json.
Scoped git diff whitespace check passed. Compilation succeeded; zero current
Console errors. One current unrelated Unity AI Toolkit warning reports
"Account API did not become accessible within 30 seconds" (network/editor-focus
service timeout); it was recorded rather than hidden or treated as a combat bug.
Historical obsolete-API test warnings also remain in the MCP log buffer.

Final saved/open scene is Warehouse_Testbed, clean and outside Play Mode. Audit
of 104 scene/prefab behaviours found zero missing scripts/object references;
all 31 colliders and 24 explicit surface markers are intact, with no Live013
diagnostic actors persisted. Player, Larper and ragdoll prefab references intact.
No commit or push. Resource reserve preserved; no Sprint014 implementation begun.

## Sprint 014 — Warehouse Incident (2026-10-19)

Implemented a separate, playable mission in `Assets/Scenes/Warehouse_Mission01.unity`.
It is the first enabled build scene; previous build entries remain available.
Warehouse_Testbed was copied through Editor APIs and was not changed by this
sprint. Its before/after SHA256 is
`DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915`.
Earlier uncommitted Sprint012.2/013 and unrelated project changes were preserved.

The mission starts safely at the west entrance with 100 health and the existing
loadout (carbine 24/72). Objectives progress through entering the warehouse,
clearing storage, reaching processing, clearing processing, recovering records,
and escaping. The arrival guard (1), storage group (4), and processing group (5)
use existing basic Larper prefabs and personality profiles. Ten actors are
prewarmed once; future groups remain inactive until their authorized region is
entered. Encounter activation and completion are idempotent. Clearing the guard
gates storage; the storage checkpoint gates processing; records and the final
objective gate the exit. Objectives cannot be skipped by visiting regions early.

The route uses the existing warehouse floors, pipe/duck passage, west ladder,
storage mezzanine, three stepped gantry platforms, processing roof, east ladder,
records office, and east ground exit. Four wooden cover crates and seven small
signs supply navigation and corporate satire using existing visuals. No new art,
enemy classes, weapons, economy, or physics systems were added. Enemy tuning is
unchanged: 22/18 awareness ranges, 40-degree spread, 18-round magazine, .105-second
shot interval, 5 damage, four-second reload with existing variation, and six-unit
retreat behavior. Mission reset hooks clear transient AI/combat state without
retuning combat, cover, camera authorization, or navigation.

Storage and records-office checkpoints integrate with existing PlayerRecovery
and health recovery. Snapshots retain cleared encounters, objective/records
progress, ammunition/reserves, and selected weapon. Death restores 100 health and
existing recovery protection; incomplete encounters reset at authored positions,
while checkpoint-cleared groups stay cleared. The same ten actors are reused.
Progress kills roll back with a snapshot; the separate results defeat counter
retains every actual enemy defeat event across retries. Deaths count player
health defeat events. Existing fall recovery remains available.

Records require E or the mission touch interaction button, proximity inside the
records region, and an unobstructed interaction path. Interaction advances once.
The mission HUD extends the existing UI Toolkit controls and PanelSettings with
briefing, objective notifications, contextual interaction, and results. Completion
freezes elapsed game time and player actions. Results show time, actual enemies
defeated, and deaths. Replay resets objectives, checkpoints, counters, health,
loadout, ammunition, position, and enemies in place, and clears existing ragdolls.
No main menu currently exists, so Replay is the working post-mission action. An
empty results extension container supports later work without simulated rewards.

New files: `Assets/Scripts/Missions/{WarehouseMission,MissionEncounter,MissionTrigger,
MissionHud}.cs`, `Assets/UI/MissionControls.uxml`, `Assets/UI/MissionControls.uss`,
`Assets/Editor/Sprint014MissionSetup.cs`, the new mission scene,
`Assets/Tests/EditMode/Sprint014RulesTests.cs`, and
`Assets/Tests/PlayMode/Sprint014MissionTests.cs`, with Unity metadata. Modified
integration files: `Assets/Scripts/Input/PlayerActions.cs` (interaction request),
`Assets/Scripts/Enemies/EnemyCombat.cs` and `EnemySpawnPoint.cs` (encounter reset),
`ProjectSettings/EditorBuildSettings.asset`, and this log. Verification records are
`TestResults/Sprint014.json`, `Sprint014Live.json`, and `Sprint014Pilot.cs`; the
diagnostic pilot is outside Assets and is not shipped as gameplay code.

Integration checks caught and fixed mission UI prefab overrides not persisting,
inherited testbed spawns, collider-edge entry being rejected before the player's
pivot reached a trigger, mission triggers interfering with traversal floor probes,
and beacon rendering accidentally scaling trigger roots. The final six zones use
unit scale and IgnoreRaycast, retaining trigger contacts while excluding traversal
geometry probes. Beacon renderers are separate children. Trigger stay retries
handle edge entry and progression while already overlapping. Two early test
fixture errors (remote-interaction placement and the existing recovery ladder
lockout) were corrected; the lockout itself was preserved. Earlier failures and
their resolutions are retained in the test report.

Baseline: **38 Edit + 100 Play = 138/138 passed**. Final validation:
**40 Edit + 108 Play = 148/148 passed**, zero failures/skips; all 138 baseline
test names retained. Ten new tests cover scene/UI/prefab contracts, supported
route clearance, safe initialization, physical trigger edge entry, real ladder
landing, gated progression, blocked/remote interaction, checkpoint ammunition and
actor reuse, records/results/replay, and existing enemy camera/hitscan/reload rules.

Two scripted live Editor playthroughs reached completion using actual PlayerActions
movement, jump, aiming, shooting, reload, climbing, and interaction. The earlier
78.69-second diagnostic included one explicitly logged injected lethal player hit
to exercise checkpoint recovery and runtime corrections; it is not final-scene
acceptance evidence. The final saved-scene run used normal damage with no immunity,
teleports, injected damage, or direct enemy defeat calls. It completed in **51.06
game seconds**, including a roughly 25-second pilot stall and excluding Editor
inspection pauses: 54 shots, 11 actual enemy defeats, one natural player death,
10 unique cleared enemies, 20 health remaining, and three checkpoint commits.
The natural death restored the storage checkpoint and processing was completed
on retry. A storage enemy changed floors; adapting the pilot route found and
defeated it normally. Results displayed 00:51 / 11 defeats / 1 death. Submitting
the actual Replay button reset the run to its safe initial state with the same
ten inactive actors. Inline Game View captures were inspected; no saved screenshot
or hands-on human playtest is claimed. Runtime diagnostics were discarded.

The automated pilot's precise input and pauses do not establish normal human
pacing: the requested 5–10 minute first-play target remains unverified. Hands-on
navigation, difficulty, floor-changing enemy pursuit, portrait/touch readability,
and Android performance still need review. No Android build/device test was run.
Previously missing combat audio remains missing; no new audio or mix is claimed.
Recommended Sprint015: hands-on mission pacing and route clarity, tune encounters
from player evidence, validate touch controls on target hardware, profile effects,
and supply appropriately licensed combat audio. Avoid expanding systems before
these checks establish a reliable first mission.

Final Editor audit: compilation complete, zero current Console errors/warnings,
118 scene/prefab behaviours checked with zero missing scripts/object references,
38 colliders, ten spawn points, and six correctly sized mission zones. Warehouse
Mission01 is saved, clean, open, and outside Play Mode. Warehouse_Testbed and
existing character/ragdoll prefab references remain intact. No commit or push.

## Sprint 015 — Combat awareness, persistence, healing and crouch repair (2026-10-19)

Reused EnemySpawnPoint, EnemyCombat, LarperBrain, RuntimeWeapon/WeaponCycle,
Health, PlayerActions, the existing HUD, posture controller and CutoutRig. No new
enemy classes, weapons, art, dependencies, inventory or movement controllers.
The established URP renderer, colorful combat effects and gray warehouse remain.

Runtime spawn points now separate preparation from activation. They retain their
authored position, prepare at most one actor, and retry activation only when the
position is outside the gameplay camera plus a configurable 1.25-world-unit safety
margin. Clearance rejects geometry, props, the player and other actors; two foot
probes require solid support. Saturated queries and unavailable/disabled cameras
fail closed. Invalid or visible placements wait rather than relocating or forcing
a spawn. Checks run every .2 unscaled seconds, after the camera's LateUpdate, with
no yield between final validation and activation. Aspect ratio and camera size are
handled by actual viewport projection. Fixed-size query buffers avoid hot-path
allocations. Selected spawn gizmos show the body/safety margin; pending points are
magenta. Once activated, actors are not despawned or suspended when offscreen.

Warehouse_Mission01 now contains ten authored Larper prefab occupants already
present at scene load, using references on the existing ten spawn points. Encounter
triggers authorize engagement without visibly materializing enemies. Untriggered
occupants hold their authored posts; this staging condition is independent of
camera visibility. Active enemies retain perception, movement, investigation,
retreat and reload while offscreen. The first live attempt exposed untriggered
patrols wandering away from their authored floors; explicit encounter staging fixed
that integration problem and a mission regression now checks that posts are held.
An actual player hit alerts a staged encounter, preventing visible occupants from
remaining passive targets; detection, sight, camera permission and objective gates
still apply independently after that alert.
Existing actors are restored behind a .18-second opaque mission reset cover for
checkpoint recovery and Replay. This prevents visible reappearance and avoids
indefinitely postponing a reused actor whose authored position remains on-camera.
New runtime spawns always use the offscreen safety rule. The same ten mission
actors are reused. Early legitimate defeats of visible future occupants are
counted; objective gates still require the authored mission sequence.

The final alert-enabled live run exposed a latent checkpoint integration bug:
Physics2D sends trigger callbacks to disabled MonoBehaviours. The office marker,
disabled so WarehouseMission could own objective-gated checkpoint commits, could
silently steal PlayerRecovery's spawn while the saved objective still referred to
storage. CheckpointMarker.OnTriggerEnter2D now explicitly rejects disabled/inactive
markers. A physical-contact regression visits the unauthorized office, waits for
physics callbacks, applies lethal damage, and verifies storage spawn, health and
objective restoration. All nine mission tests pass after this correction.

Firing permission is separate from detection, confirmed line of sight and memory.
Every real shot, including blind fire, requires current gameplay-camera visibility
unless the Inspector capability allowOffscreenFire is enabled (default false).
Leaving the camera cancels the blind burst immediately; a historical first shot
no longer grants permanent firing permission. The capability only relaxes the
camera gate and does not grant detection or sight. No sniper was implemented.
Confirmed perception samples run every .1 seconds; last-known position changes
only on confirmed sight. Memory lasts three seconds and then returns to patrol.
Existing inaccurate panic fire is bounded by both its duration and memory expiry,
never perfectly follows a player through cover, and remains camera-gated. A fresh
solid obstruction blocks direct shots even between perception samples. Expensive
shot obstruction checks only run when weapon cadence permits another shot.
Selected combat gizmos show camera permission and the remembered position.

Cover commitment and the existing locomotion probes remain authoritative. Cover
selection now prefers the nearest verified reachable cover in the retreat corridor.
The committed destination is retained, avoiding repeated selection/oscillation.
Retreat progress is measured along the chosen direction instead of absolute
displacement. Existing blocked-path checks, fallback withdrawal, minimum travel,
4.5-second timeout, low-hop clearance, crouched reload and delayed reengagement
remain. A blocked retreat still reaches a finite reload decision rather than
deadlocking. Reload ticking continues without sight or camera visibility.

LarperBuzzbox magazine capacity increased from 18 to **27** (50%); initial enemy
reserve increased from 54 to **135**, enough for five complete empty-magazine
reloads. Both remain Inspector-configurable through the existing weapon definition
and EnemyCombat. Reload consumes real finite reserve, supports partial final
loads, and cannot fire during reload or with an empty magazine. Four-second reload
duration/variation, .105-second cadence, 5 damage, 40-degree distant spread,
22/18 detection/engagement ranges, cowardly thresholds and six-unit fallback
withdrawal remain unchanged. Existing archetype movement capabilities and the new
offscreen-fire flag supply lightweight future configuration without new AI types.

PlayerHealing uses Health.Heal without restoring defeated actors or changing damage
protection/death handling. H routes through PlayerActions.RequestHeal. Defaults:
35 HP, capped at maximum; three-second game-time cooldown; unlimitedUses=true.
The optional finite-use setting defaults to three uses when unlimited mode is
disabled. Full health, defeat, disabled actions, cooldown and exhausted uses reject
healing without consuming a use/cooldown. A successful heal displays a green
HEALED +amount readout in the existing HUD alongside current/maximum health; the
HUD then shows cooldown or H — HEAL. No consumable item system was added.

Crouch distortion came from independent pose scaling: torso .65, head .8, arms .75
and legs .4, with discontinuous offsets. CutoutRig now keeps full head/torso/limb
proportions, folds the existing rigid legs at their hips, uses a small torso lean,
and solves leg-pivot height from sprite bounds so feet stay grounded. Crouch blends
over .14 seconds with a configurable 55-degree leg pose. The root only mirrors
facing; no whole-body compression was added. Collider height is .7 of standing
(0.98 world units for the existing 1.4-unit character), fitting the 1.05-unit passage
while matching the taller uncompressed pose. Existing bottom-anchored collider
changes and blocked-standing overlap checks remain. Weapon/muzzle alignment and
recoil stay in the same rig; crouched muzzle ceiling is -.06 local Y, preserving a
low firing origin. The first broad regression caught the muzzle slightly above
the pivot; this was corrected rather than weakening the lower-origin assertion.
The rig still uses the established simple cutout limbs, not a new animation system.

New source files: Assets/Scripts/Combat/PlayerHealing.cs,
Assets/Editor/Sprint015Setup.cs, Assets/Tests/EditMode/Sprint015RulesTests.cs and
Assets/Tests/PlayMode/Sprint015Tests.cs, with Unity metadata. Modified runtime files:
Combat/Health.cs; Enemies/EnemyCombat.cs and EnemySpawnPoint.cs;
Camera/GameplayViewport.cs; Input/PlayerActions.cs and WarehouseTouchControls.cs;
Traversal/PlayerPosture2D.cs; Visuals/CutoutRig.cs; Missions/MissionEncounter.cs,
WarehouseMission.cs and MissionHud.cs; Checkpoints/CheckpointMarker.cs. Modified assets: LarperBuzzbox.asset,
Larper.prefab, WarehousePlayer.prefab and Warehouse_Mission01.unity. Existing
Sprint005, MovementCompletion, TraversalRegression, CombatIntegration, Sprint012,
Sprint013 and Sprint014 tests retain their cases, with explicit expectations
updated for intentional ammo, memory, collider and scene-occupant changes.

Inspector defaults added/changed: spawn cameraSafetyMargin=1.25, retrySeconds=.2,
supportDistance=.6, geometry=1, optional gameplayCamera and placedActor;
EnemyCombat allowOffscreenFire=false, perceptionSeconds=.1, visibilityMemory=3,
initialReserveAmmo=135; weapon magazineCapacity=27; healing amount=35,
cooldownSeconds=3, unlimitedUses=true, remainingUses=3; posture heightMultiplier=.7;
rig crouchTransitionSeconds=.14, crouchLegDegrees=55, crouchedMuzzleCeiling=-.06.

Live validation: inspected enlarged standing/crouch renders and the actual HUD in
Play Mode. Visual feet Y=.01875 versus collider feet Y=.015; head/torso/leg scales
were (1,1,1). A separate controlled healing check applied 50 test damage and routed
healing through PlayerActions, producing 85/100 and visible HEALED +35. That check
is distinct from the subsequent full mission run. An earlier completed mission input pilot
used normal movement/jumps/climbing, aim/fire, reload, interaction and healing;
no teleports, immunity, injected damage or direct enemy defeat calls. It completed
in 21.47 game seconds with ten defeats, zero deaths, 37 shots, one successful
35-HP heal, 50 HP remaining, records recovered and three checkpoint commits.
Results showed 00:21 / 10 defeats / 0 deaths. Submitting the real Replay button
restored 100 HP, initial objectives/counters and the same ten active staged actors
behind the reset cover. Runtime inspection overrides were discarded on exit.
This completed run preceded the hit-alert/checkpoint correction; the later final
run is recorded below. Scripted precise-aim runs do not establish human difficulty
or pacing.

Verification records and four diagnostic captures are retained under TestResults/
outside Assets: Sprint015.json, Sprint015Live.json, Sprint015Pilot.cs,
Sprint015Standing.png, Sprint015Crouch.png, Sprint015Healing.png and
Sprint015Results.png. Older Sprint008 capture assets were preserved. No original
artwork was sought or replacement artwork generated. Android/touch hardware,
device profiling, hands-on subjective pose review and human mission pacing remain
unverified. Existing missing combat audio is unchanged. No Sprint016 work begun.

Final saved-scene live run after hit alerts and checkpoint-contact repair:
**19.91 game seconds, ten actual defeats, zero deaths, 40 shots, two successful
35-HP heals, 85 HP remaining, three checkpoint commits, records recovered and one
completion**. Results displayed 00:19 / 10 / 0; Sprint015Results.png records this
final run. Normal gameplay input and damage rules were used throughout. Earlier
diagnostics (including the invalid-office-checkpoint death loop) were superseded
by the explicit enabled-state guard and its passing physical-contact regression.
Automated precision and diagnostic pauses are not human pacing evidence.

Final automated validation after all integration fixes: **41/41 Edit Mode +
121/121 Play Mode = 162/162 passed**, zero failures/skips. All **148 Sprint014
baseline test names retained**, with 14 additional cases (one Edit, 12 Sprint015
Play, one mission checkpoint-contact Play). Existing cover/retreat/reload, ammo,
traversal, ragdoll, camera, effects, mission objectives, checkpoint rollback,
results and Replay coverage passes. Intermediate failures and resolutions remain
in Sprint015.json. Scoped git whitespace validation passed.

Final serialized audit covered **280 scene/prefab behaviours**, including the
player, Larper and LarperCartoonRagdoll prefabs: zero missing scripts or non-null
missing object references, ten authored occupants and ten spawn points. Current
Console: **zero errors, zero warnings**, compilation complete. Historical log
buffer entries retain resolved diagnostic/compile errors, obsolete-API warnings
and the unrelated AI Toolkit account-access timeout; current ground truth is
recorded separately in Sprint015Live.json. Warehouse_Testbed's file SHA256 is
unchanged from Sprint014. Warehouse_Mission01 is saved, clean, open and outside
Play Mode; runtime camera/input overrides and diagnostic callbacks were discarded.
Earlier uncommitted work was preserved. Nothing staged, committed or pushed.

## Sprint 015.1 — Perception, combat positions and camera stability (2026-10-19)

Investigated the existing runtime components, serialized player/camera settings,
mission gating, visibility masks, shot paths, movement hooks and regression suite.
The player already uses Rigidbody2D Interpolate at the existing .02-second physics
step. WarehouseCameraFollow is the only camera writer; no Cinemachine or competing
camera scripts were found. Ladder traversal drives Rigidbody velocity in
FixedUpdate and does not switch camera targets/offsets. No movement physics or
ladder controller rewrite was necessary.

The short-sight root cause was Sprint015 encounter staging: EnemySpawnPoint bound
a null target while staged, and EnemyCombat returned before its perception checks.
Authored mission enemies therefore needed a region trigger or direct damage to
become aware, despite the unchanged 22-unit detection/18-unit engagement ranges.
Targets now remain bound, and staged actors run the existing .1-second perception
samples. Confirmed sight wakes the observer before taking damage, independently
of camera visibility. Each observer senses for itself; noticing a threat marks its
encounter engaged without magically sharing a confirmed player position with all
members. Existing region activation and damage alerts remain available. Mission
storage-region visitation is tracked separately from engagement so natural early
awareness cannot block or bypass objective/checkpoint progression. Existing tests
were updated for that intentional distinction, retaining all their test cases.

There is no restrictive field-of-view cone in the existing basic Larper policy.
Geometry mask remains layer 0; normal actor collision roots use layer 2. Sight
raycasts now explicitly ignore the observer and target hierarchies, protecting
against accidental self-blocking if layers change. Saturated visibility queries
fail closed. Geometry still blocks sight, including legitimate occlusion by the
warehouse pipe/platforms. Medium-range and elevated clear sight are tested without
damage. The camera gate still applies to every actual shot, including blind fire,
and never to perception. Distance-dependent spread, reaction timing, last-confirmed
position memory, 27/135 ammunition and the cowardly retreat/reload loop remain.

Added logical CombatNoise events using a readonly value descriptor and static
subscription, rather than a new scene manager or AI controller. MovementAudioHooks
publishes player movement independent of optional AudioClips. Noise intensity
tracks actual speed; running is louder than walking, with a .25 multiplier for
crouching. Existing footstep cadence remains .35 seconds. AttackExecutor publishes
successful firearm shots using the real muzzle-to-nearest-hit segment, before
visual tracer fading. LarperAwareness subscribes/unsubscribes alongside its existing
events and routes hostile player stimuli into EnemyCombat. There are no projectile
or sound-source searches every frame.

Hearing uses distance, movement intensity and a single obstruction attenuation
check. Player gunfire is deduplicated at .25 seconds per receiver. Heard positions
are quantized to a two-unit grid and retained for four seconds; they never replace
the last confirmed visual player position. Unconfirmed sounds produce temporary
suspicion, facing/investigation via the existing motor, then expire. Gunfire near
an actor's real shot segment produces an UnderFire state and a brief hesitation
when no target is confirmed; existing retreat/reload ownership is preserved.
Near-miss checks use shot data, not tracer renderers or physical projectile objects.
The exposed Unaware/Suspicious/Alerted/UnderFire classification complements the
existing engagement state rather than replacing its state machine. No sight or
offscreen firing permission is granted by hearing alone.

Identical combat destinations came from independently pursuing the same preferred
distance and selecting cover without checking other actors' intentions. Added a
small static reservation/occupancy registry owned by EnemyCombat lifecycle events.
Candidates respect the largest spacing requested by either actor, consider both
current occupancy and claimed positions, and use the existing supported-corridor,
obstacle and low-hop checks. Nearby alternatives are bounded and deterministic.
Long approaches use short supported steps; a clear current position is the fallback
when no travel candidate is valid. Goals remain committed between tactical samples
and are reconsidered after meaningful target movement, arrival or obstruction.
Retreat releases a firing claim and claims a reachable retreat/cover endpoint;
reload can retain that protected endpoint until completion. Defeat, panic, route
ownership, reset, disable and destruction release claims. There is no crowd manager,
global path planner or teleport separation. Selected-editor gizmos show detection,
movement-hearing range and claimed combat positions.

The camera's actual defect was quantization feedback: SmoothDamp used the previous
pixel-rounded transform as its next continuous input. That causes residual error
and inconsistent settling while interpolated physics/render frames advance, most
noticeably at ladder stops and reversals. Follow now keeps an unrounded internal
position and velocity; only the final render position is snapped. Boundary clamps
also constrain the internal state and zero the affected velocity axis. Snap/bind
initializes both states consistently. Camera LateUpdate runs after visual posing
and before camera-safe spawn activation. The existing interpolated root target,
framing, integer zoom, 32-PPU art grid, offset and .16-second smoothTime are retained.
No additional lag, camera target switching or ladder-state offsets were introduced.

New Inspector defaults: minimumCombatSpacing=1.25, positionSampleSeconds=.25;
movementHearingRange=11, gunfireHearingRange=24, obstructedHearingMultiplier=.45,
hearingMemorySeconds=4, soundPositionUncertainty=2, nearMissDistance=1.25 and
gunfireHearingInterval=.25 on EnemyCombat; runningSpeed=5 and
crouchNoiseMultiplier=.25 on MovementAudioHooks. Existing detection=22,
engagement=18, visual memory=3, perception interval=.1, stepInterval=.35 and camera
smoothTime=.16/visualPixelsPerUnit=32 remain configurable and unchanged.

New source: Assets/Scripts/Combat/CombatNoise.cs,
Assets/Scripts/Enemies/CombatPositionReservations.cs and
Assets/Tests/PlayMode/Sprint0151Tests.cs, with Unity metadata. Modified source:
Camera/WarehouseCameraFollow.cs; Combat/AttackExecutor.cs and CombatVisibility2D.cs;
Enemies/EnemyCombat.cs, EnemySpawnPoint.cs and LarperAwareness.cs;
Visuals/MovementAudioHooks.cs; Missions/MissionEncounter.cs and WarehouseMission.cs;
plus Sprint014MissionTests.cs and this log. No new enemy, weapon, level, visual
asset, package or replacement controller. Existing uncommitted work was preserved.

Final automated validation: **41/41 Edit + 131/131 Play = 172/172 passed**, zero
failures/skips. All **162 Sprint015 baseline names retained**, ten new Play cases.
They cover staged medium/elevated sight without damage, self filtering/occlusion,
quiet crouch versus obstructed running, gunfire attenuation/deduplication, real
hitscan near misses, clip-independent footsteps, stable distinct leases and cleanup,
physical multi-actor positioning, rapid camera reversals/settling and real ladder
climb/stop/dismount. Existing offscreen spawning/persistence/fire restrictions,
memory, cover/reload/ammo, healing, crouch, effects, ragdoll, objectives/checkpoints,
results and Replay coverage passes. The initial reservation fixture exposed a
missing current-position fallback; the first mission run exposed the conflation
of engagement and region visitation. Both were fixed before the final full suite.

Scripted live Warehouse_Mission01 playthrough completed with normal PlayerActions,
damage, ammunition and healing: **20.89 game seconds, ten defeats, zero deaths,
36 shots, two heals, 85 HP remaining, records recovered and three checkpoint
commits**. No immunity, teleports, injected damage or direct defeat calls were used.
The input pilot deliberately stopped on the ladder for 1.1 seconds. In the final
.3-second settled window (108 samples), target and camera Y span were both zero;
maximum follow error was .00389 world units. Across 97 actor snapshots, 55 claimed
position pairs stayed at least 1.596 units apart (configured minimum 1.25); minimum
observed actor distance was 1.158. All four awareness classifications were observed.
These are sampled live measurements, not a device performance profile or a human
feel/pacing claim. Results were visually inspected in Game View.

Records: TestResults/Sprint0151.json, Sprint0151Live.json, Sprint0151Pilot.cs,
Sprint0151Camera.csv and Sprint0151Actors.csv. Two screen-capture attempts failed
because no Game View render target was open; exact errors/stacks are preserved in
Sprint0151Live.json and MCP history. Opening Game View and stepping one completed
frame resolved capture. After recording those tool errors, the Console view was
cleared and a fresh .8-second startup check produced **zero errors/warnings**.
Final serialized audit: **280 behaviours, zero missing scripts/references**, ten
occupants, and one camera writer. Compilation complete; Warehouse_Mission01 saved,
clean, open and outside Play Mode. Warehouse_Testbed's file SHA256 remains
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.

Remaining manual checks: subjective camera feel and rapid reversals at target
device frame rates, portrait/touch/device profiling, narrow-corridor crowd behavior,
and human mission difficulty/pacing. Intentional art-grid steps remain; hearing is
a simple range/obstruction model, not multi-floor acoustic navigation, and position
claims are not a global crowd solver. Missing audio clips remain optional and do
not suppress logical awareness. No Sprint016 work, staging, commit or push.

## Sprint 016 — Enemy spacing and encounter distribution (2026-10-19)

Focused balancing only. Runtime spawn activation now has Inspector-configurable
`EnemySpawnPoint.minimumSpawnSpacing = 1.2` world units between Larper centers.
Previously there was no independent spawn-distance parameter: clearance relied
on collider overlap (nominal body width 0.8, with the existing 0.06 query inset).
The new distance is 50% above that nominal width; the separate 1.25 camera safety
margin is unchanged. A bounded, nonallocating neighborhood query checks active
Larpers. Original body/obstacle, two-foot support and final-camera checks remain.
Saturated queries fail closed. Crowded points remain pending at their original
validated locations, retrying every existing 0.2 seconds. Lower Inspector spacing
permits deliberate dense encounters; physical body clearance still applies.

Combat reservation spacing increased from 1.25 to **1.75 units (+40%)**, in the
existing component default and Larper prefab. Reachable candidate selection,
current-position fallback, cover claims, stable leases and cleanup are unchanged.
No proximity-triggered retreat or formation rules. All ten authored mission
occupants retain their irregular locations and encounter membership. Population,
offscreen restrictions, persistence and covered checkpoint/replay restoration
remain unchanged. No crouch, player, camera, awareness, weapon, healing, animation
or ragdoll changes in this sprint.

Sprint016 files: Assets/Scripts/Enemies/EnemySpawnPoint.cs and EnemyCombat.cs;
Assets/Prefabs/Enemies/Larper.prefab; Assets/Scenes/Warehouse_Mission01.unity
(saved spawn-spacing fields); Assets/Tests/PlayMode/Sprint016Tests.cs and meta;
this log; TestResults/Sprint016.json, Sprint016Live.json, Sprint016Actors.csv and
Sprint016Pilot.cs. Saving the prefab also serializes existing Sprint015.1 hearing
and footstep defaults at unchanged values. Earlier uncommitted work is preserved.

Validation: **41/41 Edit + 136/136 Play = 177/177 passed**, zero failures/skips;
all 172 Sprint015.1 test names retained. Five new Play cases cover nonoverlapping
but too-close spawns postponing then restoring three actors at their original
points, separate platforms, explicit dense configuration, and stable distinct
reservations with two, three and five simultaneous Larpers. Existing camera-safe
spawning, obstacles/unsupported points, real movement/blocked destinations,
reservation cleanup, awareness, offscreen fire, memory, retreat/reload, healing,
crouch, camera/ladder, mission and ragdoll regressions pass. An initial exact-vector
stability assertion rejected tiny physics settling; it now allows less than 0.01
units, and the final full suite passes.

The existing normal-input pilot was reused with separate Sprint016 diagnostic
keys. Actual live Warehouse_Mission01 completion: **22.07 game seconds, ten
defeats, zero deaths, two real healing requests**. No teleports, immunity or
injected damage. Across 103 snapshots/592 actor rows, 52 reserved-position pairs
had minimum separation **2.408 units**, above 1.75; all four awareness states were
observed. Actual bodies briefly approached 1.158 units: reservation spacing governs
destinations, not continuous separation while traversing. No pilot stall occurred.
This is automated Editor validation, not human pacing review or a controlled
target-device performance profile. Extra spawn work is bounded to 32 hits per
pending point at the existing retry interval, without per-query array allocation.

Final audit: 280 scene/player/Larper/ragdoll behaviours, zero missing scripts or
non-null missing references, ten occupants using 1.75 combat spacing. Compilation
succeeded; final Console has zero errors/warnings. Initial MCP timeout and
reload-status HTTP400 recovered; a Unity account token-exchange exception was also
observed in retained history, absent from the final Console. Details are recorded
in Sprint016Live.json. Older obsolete-API compilation warnings remain outside this
focused sprint. Warehouse_Mission01 saved, clean, open, outside Play Mode.
Warehouse_Testbed SHA256 remains
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.

Remaining manual checks: subjective crowd readability, narrow corridors, touch
devices and target-hardware profiling. No global crowd solver was introduced;
invalid runtime positions wait rather than force spawning. No Sprint017 work,
staging, commit or push.

## Sprint 017 — Defensive crouching and suppression stability (2026-10-19)

Larpers now sometimes briefly hold ground in the protagonist's existing goalkeeper
crouch when the existing hostile-player hitscan noise segment passes within the
existing 1.25-unit near-miss radius (including surviving direct hits). Ordinary
distant gunfire alone does not trigger it. EnemyCombat requests downward input on
the already-present PlayerPosture2D; CharacterVisualDriver and CutoutRig therefore
reuse the same 55-degree leg pose, rigid modular scales, 0.14-second blend, attached
weapon and constrained muzzle. No player/posture/rig/animation source was changed.
The same controller reduces body height to 70%, keeps feet grounded, and prevents
standing through ceilings. Once clear, normal standing resumes without more input.

Inspector tuning on EnemyCombat and Larper.prefab:
- defensiveCrouchProbability: 0.30 per eligible suppression episode.
- defensiveCrouchSeconds: randomized 0.8–2.0 seconds (Vector2 min/max).
- defensiveCrouchCooldown: randomized 5–8 seconds (Vector2 min/max).
- suppressionQuietSeconds: 0.65 seconds without a nearby shot ends an episode.

Successful AND failed rolls consume the cooldown. Every nearby round extends the
episode boundary, not the crouch timer; a continuing burst cannot reroll or keep
the actor crouched forever. Only grounded, alive actors outside panic, withdrawal,
reload, ladder-route/climb and parachute states qualify. These incompatible states
cancel an ongoing reaction. Existing low-ammo survival decisions take priority.
During the short hold, movement stops without discarding an existing reservation;
ordinary inaccurate aiming/firing, sight checks, memory and camera gates continue.
No accuracy bonus, new panic/run-away system, controller or formation logic.
An expired defensive timer clears its input; a genuinely obstructed ceiling may
keep the physical posture crouched until standing is safe, as for the player.

Production changes: Assets/Scripts/Enemies/EnemyCombat.cs and
Assets/Prefabs/Enemies/Larper.prefab (four tuning fields). Added
Assets/Tests/PlayMode/Sprint017Tests.cs plus Unity meta. Updated this log. Validation
artifacts: TestResults/Sprint017.json, Sprint017Live.json, Sprint017Actors.csv and
Sprint017Pilot.cs. All previous uncommitted work remains intact. No new visual assets,
archetypes, weapons, levels, UI, player/camera changes or art generation.

Automated validation: **41/41 Edit + 144/144 Play = 185/185 passed**, zero
failures/skips. All **177 Sprint016 test names retained**, eight focused additions.
They verify a real no-damage hitscan near miss, recognizable rigid pose and feet,
70% collider, safe crouched firing/muzzle/spread and offscreen gate, burst/failed
roll cooldowns, timeout and low-ceiling recovery, retreat/reload/panic/airborne/
disable priority, live ladder cancellation, blocked sight/frozen last-known point,
one ragdoll on death, and varied finite reactions among eight simultaneous actors.
Existing spawning, spacing, reservations, awareness, reload/cover, ammo, healing,
camera, ladders, player crouch and ragdoll regressions all pass. Initial fixtures
froze enemies above the grounded probe and incorrectly expected defeat to disable
the collider component rather than its root; those test assumptions were fixed.
One MCP test invocation during reload returned HTTP400; retry succeeded.

Actual normal-input live mission run, at unchanged default 30% chance, completed
in **33.71 game seconds with one death/checkpoint recovery, ten final mission
defeats and five healing requests**. No immunity, teleports or injected damage.
Game View was paused and visually inspected during a natural defensive crouch:
Processing_Spawn_0 had Crouch pose, 0.98 collider height, head scale 1 and six shots
fired. The same ridiculous bent-leg pose and attached firing weapon were visible.
Across 156 snapshots/803 actor rows, 22 defensive-intent rows included 16 physically
crouched rows (six captured onset before FixedUpdate applied the collider). Up to
**three simultaneous defensive actors**, and 15 snapshots mixing defensive and
other behavior, were observed. Among 99 reserved-position pairs minimum distance
was **2.204 units**, above Sprint016's unchanged 1.75 setting. No pilot stall.

Performance scope: the production reaction is driven by existing noise events,
with scalar timers/eligibility checks only; no new per-update collection, global
search, component creation/destruction or physics query. Existing posture owns
the bounded safe-standing query. Eight-actor tests and the live run passed without
an integration stall; this is not a controlled FPS comparison or target-device
CPU/GPU profile. Human combat feel/difficulty and device profiling remain manual.

Final serialized audit: 280 behaviours across scene and player/Larper/ragdoll
prefabs, zero missing scripts/non-null missing references; ten mission occupants,
all inheriting 0.30 probability. Compilation complete, final Console zero errors
and warnings. Warehouse_Mission01 saved, clean, open, Play Mode stopped. Player
crouch/pose source and Warehouse_Testbed remain unchanged by this sprint.
No staging, commit, push, Sprint018 work or unrelated feature work.

## Sprint 018 — Lightweight reactive tuning (2026-10-19)

Three bounded adjustments in existing EnemyCombat, saved on Larper.prefab:
- Defensive crouch chance 0.30 -> 0.36 (+20% per eligible episode).
- Under fire only, moving-target retarget displacement multiplier 1 -> 0.8:
  3.5 -> 2.8 units (-20%, +25% displacement sensitivity at default spacing).
  Stable leases remain valid; the existing 0.25-second sample cadence, support
  checks and reservations are unchanged. No forced movement after every shot.
- Under fire only, personality low-ammo retreat fraction multiplier 1 -> 1.2:
  ordinary 0.20 -> 0.24 (27-round magazine: five -> six rounds), cowardly
  0.40 -> 0.48 (ten -> twelve rounds), overconfident zero stays zero.
  Existing target availability, memory, distance and reserve-ammo gates still apply.

The two suppression modifiers use the existing 0.6-second under-fire timer, return
to baseline when it expires, and are Inspector-configurable/clamped to 0.8–1 and
1–1.25 respectively; retreat fractions remain capped at 0.8. Cooldowns, duration,
episode deduplication, blind-fire probabilities, imperfect aim and aggressive
overconfident behavior remain unchanged. No fixed action sequence. Nearby allies
and cover modifiers were skipped: no suitable additional decision-time cache was
available, and this sprint adds no searches or physics queries. Only scalar math
was added to existing decisions; no new managers, collections or animation edits.

Files: Assets/Scripts/Enemies/EnemyCombat.cs, Assets/Prefabs/Enemies/Larper.prefab,
Assets/Tests/PlayMode/Sprint017Tests.cs (two Sprint018 cases), this log, and
TestResults/Sprint018.json. No new production files or unrelated changes.

Validation: **41 Edit + 146 Play = 187/187 passed**, zero failures/skips, all 185
Sprint017 names retained. New tests compare 2,000 matched seeded episodes before
and after, verifying increased yet nonmandatory crouching; actual Play Mode checks
verify earlier low-ammo retreat, moving-target retargeting, modifier expiry, stable
unsuppressed leases and preservation of zero aggressive thresholds. Existing
varied multi-enemy crouching, cooldown, fire/reload/memory, spawning/spacing,
camera/ladder, healing, pose and ragdoll regressions pass. No separate manual/live
mission pilot or FPS benchmark was run this sprint; long-run frequency, subjective
feel and device performance remain manual checks. The shorter retarget threshold
increases opportunities only when the target moves enough, not every suppression.

Compilation succeeded; final Console zero errors/warnings, ten mission occupants.
Warehouse_Mission01 saved, clean, open and outside Play Mode. Existing uncommitted
work preserved. No staging, commit, push, Sprint019 or unrelated feature work.

## Sprint 019 — Larper social reactions (2026-10-19)

Reused Health.Defeated -> LarperAwareness's existing static ally notification.
All defeats/ragdolls are treated consistently. Separate dramatic-knockdown and
panicked-retreat propagation were skipped: reliable classification/deduplicated
propagation would require more infrastructure. Only actual defeats publish events;
social fear, confidence, panic and withdrawal do not relay them.

Witnessing keeps the existing nine-unit personality range and on-camera gate,
adds a configurable 100-degree half-angle (200-degree cone), and uses the existing
nonallocating sight ray with self/source filtering. Range, cooldown, compatibility
and field-of-view reject before the ray. No per-frame social scan, collection,
physics query or new manager. Speech remains the existing system, independent of
AI witnessing; a UI document is not required to apply the influence.

Ordinary default bands: **45% fear, 15% confidence, 40% minimal interruption**.
Cowardly: 55%/10%; overconfident: 35%/25%. Existing profile panic probabilities
(ordinary 8%, cowardly 20%, overconfident 2%) are INCLUDED in the fear band,
preserving the original panic path and forced-panic regression fixtures. A single
roll chooses the band. No guaranteed retreat or fixed reaction sequence.
Settings are Inspector-configurable on LarperAwareness/Larper.prefab: base fear
0.45, base confidence 0.15, influence 4 seconds, view half-angle 100 degrees.
The existing profile morale cooldown is reused (45 seconds). All eligible rolls,
including unaffected ones, consume it; cooldown is at least the influence duration.

EnemyCombat uses two scalar fields for one non-stacking timed influence, reset on
disable/death/encounter reset and expired by time comparison. Fear adds randomized
0.15–0.45-second hesitation, multiplies future suppression-crouch chance and
low-ammo retreat fraction by 1.25. Confidence multiplies those by 0.9 and 0.85 and
preferred firing distance by 0.9 (5 -> 4.5 units). Existing cooldowns, safe movement,
reservations, aim/spread, sight/memory, ammo and offscreen-fire gates remain in
charge. Zero aggressive retreat thresholds remain zero; probabilities/fractions
stay bounded. No player location is learned from an ally's defeat. New effects
cannot interrupt an active reload, withdrawal, panic, ladder route/climb or chute.

Each defeated source emits once per activation. A monotonic event ID plus one
watermark per observer deduplicates repeats and out-of-order older events without
an expanding collection. Cooldowns prevent multiple nearby defeats from refreshing
or stacking effects. A new defeat after a legitimate new lifetime receives a new
ID. Cosmetic ragdoll motion cannot recursively publish morale notifications.

Files: Assets/Scripts/Enemies/LarperAwareness.cs and EnemyCombat.cs;
Assets/Prefabs/Enemies/Larper.prefab; new Assets/Tests/PlayMode/Sprint019Tests.cs
and Unity meta; this log; TestResults/Sprint019.json. No new production source,
personality assets, animations, visual assets, dialogue system or unrelated edits.

Validation: **41 Edit + 152 Play = 193/193 passed**, zero failures/skips;
all 187 Sprint018 names retained. Six added Play tests exercise real fatal damage
and ragdoll notification, finite hesitation/fear/confidence, unchanged unknown
player position, walls/range/rear-field rejection, duplicate IDs after cooldown,
fresh events after cooldown, non-stacking and reset, protected reload/traversal,
and seeded eight-witness variation without chains. Legacy forced-panic/visual-face
tests and all previous crouch, suppression, spawning/spacing, fire/reload, memory,
healing, camera/ladder and ragdoll regressions pass. Actual Unity runtime tests were
run; no separate human playthrough or device FPS profile was performed.

Final: compilation complete, ten occupants, zero missing Larper prefab scripts,
Warehouse_Mission01 saved/clean/open, Play Mode stopped. Console zero errors and
one unrelated warning: "Account API did not become accessible within 30 seconds.
This may be due to network issues or editor focus." Warehouse_Testbed SHA256 stays
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.
Remaining limits: existing on-camera social gate; no separate knockdown/retreat
events; subjective combat feel and target-device profiling remain manual.
Existing uncommitted work preserved; no staging, commit, push or Sprint020 work.

## Sprint 020 — Morale, crowd control and Android polish (2026-10-20)

Morale diagnosis: defeat notifications already arrived, but a gameplay-camera gate
discarded offscreen witnesses, an unaffected roll blocked further decisions for
45 seconds (often the whole encounter), and ordinary fear only paused for a
fraction of a second. Social witnessing now works outside the gameplay camera
while retaining distance/cone/solid LOS checks and unknown player position.
Ordinary offscreen shooting remains forbidden; explicit sniper capability is
unchanged. Legacy direct panic keeps its camera gate. Personality probabilities
are unchanged. Profile morale cooldown **45 -> 18 seconds**, influence **4 -> 5**.
Non-panic fear can request the existing cooldown-limited goalkeeper crouch or,
with visible threat, available reserves and <=55% magazine, sometimes commit to
the existing safe withdrawal. Otherwise it hesitates 0.4–0.8 seconds, bounded by
the influence lifetime. No scripted action sequence or accuracy improvement.

Rare collapse: an actual witnessed panic has an Inspector-configurable **8%**
chance to start a wave; nearby recipients have **35%** spread chance. Hard limits:
**two hops, root plus three recipients, six-unit LOS range, 20-second global
cooldown**, existing local cooldown, 0.7–1.4-second propagated panic and <=5-second
influence. A per-wave event watermark rejects duplicates. Only this bounded panic
notification propagates; normal fear, confidence and defeats do not recurse
indefinitely. No scans, propagation collections or per-frame social physics.

Crowding diagnosis: reservations separated destinations, but did not exclude paths
through allies or control approach spacing; runtime spawn distance was only 1.2.
Spawn spacing is now **1.5 units (+25%)**, postponing unsafe activation as before.
Combat spacing stays 1.75. Existing registry now rejects same-floor paths through
active allies and checks motion at 0.1-second intervals with a wider resume gap
(hysteresis). A stable registry priority resolves almost coincident starts. No
teleports, new overlap queries or pathfinder. Grounded patrol/pursuit yield; actual
airborne and authored ladder traversal retain ownership. Cover selection checks
the same clear path and reservations; blocked retreats retain finite reload fallback.

Speech hard cap moved into LarperSpeechBubble.TryShow: **maximum two globally**,
including direct callers. Lines last **3–4 seconds** (3.2 base plus length), with
no rapid replacement. A single expiring pending combat line per actor prevents
withdrawal/reload dialogue from being lost behind a readable existing bubble;
pending checks run every 0.2 seconds only while queued, expire after six seconds,
and perform no physics. Added short retreat lines such as STAND YOUR GROUND! and
I FEAR NOTHING!, selected on real retreat events. Opening lines differ by
personality; ordinary chatter and existing speech gates remain rate limited.

Android: configurable **1.125 character magnification** (12.5% larger) in the
existing camera writer, with Editor preview. It applies AFTER integer zoom
rounding, which would otherwise silently erase the adjustment. PC framing and
tracking/smoothing are unchanged; tighter actual camera bounds still govern fire
and runtime spawn visibility. Preview was disabled again after validation.
Both pad diameters **150 -> 187.5**, knobs **62 -> 77.5** (+25%); input thresholds
and normalized sensitivity unchanged. Touch areas grow with the pads. Controls
are **50% opacity**; action buttons shifted clear of larger sticks, interaction
moved above controls. Added 88-unit Heal buttons to both existing control UXMLs,
wired to PlayerActions.RequestHeal/PlayerHealing with cooldown caption. Existing
safe-area and PanelSettings scaling retained. No Android startup investigation,
startup code/build-scene edits, package additions or unrelated settings changes.

Ragdolls: existing initial multiplier **2.6 -> 2.99**, subsequent **1.56 -> 1.794**,
impulse cap **2.5 -> 2.875** (+15%). Velocity 12, angular velocity 720, damping,
joints, four-active cap, lifetime/sleep and activation/recovery remain unchanged.
Pistol impulses remain low; larger inputs gain expression without an unbounded launch.

Sprint020 files (all under Assets unless stated):
- Scripts/Enemies/EnemyCombat.cs, CombatPositionReservations.cs, LarperBrain.cs,
  EnemySpawnPoint.cs, LarperAwareness.cs, LarperSpeechBubble.cs,
  LarperPersonalityProfile.cs; Scripts/Camera/WarehouseCameraFollow.cs;
  Scripts/Input/WarehouseTouchControls.cs; Scripts/Visuals/CartoonRagdoll.cs.
- UI/WarehouseControls.uss/.uxml, UI/MissionControls.uss/.uxml;
  Data/Enemies/Personalities/Ordinary.asset, Cowardly.asset, Overconfident.asset;
  Prefabs/Enemies/Larper.prefab; Prefabs/Visuals/LarperCartoonRagdoll.prefab;
  Scenes/Warehouse_Mission01.unity (camera fields and spawn spacing only).
- New Tests/PlayMode/Sprint020Tests.cs and meta. Existing Sprint011CombatTests,
  Sprint012CombatTests, TraversalRegressionTests and Sprint016Tests retain cases
  with intentional impulse, readable dialogue timing, normalized drag distance
  and spawn-spacing expectations updated. This log and TestResults/Sprint020.json,
  Sprint020Live.json, Sprint020Actors.csv, Sprint020InitialActors.csv, Sprint020Pilot.cs.

Validation: **41 Edit + 160 Play = 201/201 passed**, zero failures/skips; all 193
Sprint019 names retained, eight new cases. They exercise offscreen observed fear
and actual crouch without shots, bounded/cooldown collapse and wall/distance
rejection, speech hard cap/readability/replacement prevention, four touch layouts
(1280x720, 960x720, 1560x720, 720x1280), real Heal click, exact Android/PC framing
ratio, moving-neighbor separation/platform independence, and measured 1.15 impulse
ratio with stability clamps. Initial full run was 157/160 Play: two old numeric/
gesture assumptions and discarded retreat dialogue were corrected; all three
targeted cases then passed, followed by the final full suite. The first focused
Heal fixture was protected from damage; removing its test-only protection fixed it.

Actual normal-input mission run with Android framing preview completed in **25.20
game seconds, ten defeats, zero deaths, three real heals**. Frames advanced from
1488 before pilot to 11341 during and 32569 at completion; Game View visually
inspected with enlarged translucent sticks and Heal. No immunity, teleport or
injected defeat was used by the pilot. Its old ladder wait was incompatible with
the closer viewport; two navigation diagnostics stalled, then the diagnostic
script was updated to side-jump, approach and return using real movement input.
Gameplay was not changed to accommodate the pilot. Initial-run samples include
25 active fear rows; the completed run includes 29 confidence rows. Natural waves
did not occur in this small sample; forced-wave tests prove their caps and expiry.
Completed run: 120 snapshots/699 actor rows, 41 reserved pairs, minimum reserved
distance **2.352**, minimum sampled body-center distance **2.330**, at most one
visible speech bubble observed (the two-bubble case is covered by tests).

Performance: crowd checks reuse the existing registry at 10 Hz, avoiding extra
physics. Path checks are bounded by the small existing actor count. Social
propagation is bounded/event-driven; speech polling exists only for one expiring
pending line. Existing ragdoll caps/sleep remain. No target-device FPS benchmark
was performed. Manual Android checks: real multitouch/comfort, notches/safe areas,
orientation/aspect layouts including interaction HUD, magnified pixel appearance,
narrow-corridor/ladder approach traffic, stronger-impact stability and CPU/GPU FPS.

Final audit: 280 scene/player/Larper/ragdoll behaviours, zero missing scripts or
non-null missing references, ten occupants. Compilation complete; final Console
zero errors/warnings. One asset-save MCP timeout recovered; readback confirmed all
values and saves, recorded in Sprint020Live.json. Warehouse_Mission01 saved, clean,
open and stopped; Android preview false. Warehouse_Testbed SHA256 remains
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.
Earlier uncommitted changes preserved. No staging, commit, push or Sprint021 work.

## Sprint 020.1 — Cowardice Calibration (2026-10-20)

Two existing Inspector parameters tuned in code defaults and Larper.prefab:
defensiveCrouchProbability **0.36 -> 0.48 (+33.3%)**;
socialFearChance **0.45 -> 0.60 (+33.3%)**. Existing personality offsets yield
cowardly/ordinary/overconfident witnessed-fear bands of **70% / 60% / 50%**
(previously 55% / 45% / 35%). More fear opportunities feed the existing brief
hesitation, defensive crouch and situational low-ammunition withdrawal decisions.
No new actions, state transitions, scans, queries or allocations were added.

Preserved: duck duration 0.8–2 seconds, duck decision cooldown 5–8 seconds,
morale cooldown 18 seconds, influence duration 5 seconds, confidence probability
and personality offsets, weapon/reload/panic-fire handling, navigation and poses.
The separate personality panic thresholds (20% / 8% / 2%) remain unchanged;
these also gate collapse-wave initiation. Root collapse probability 8%, spread
35%, 20-second global lockout, two-hop and four-actor caps are unchanged.
Increasing the wider fear band does not increase the wave-trigger probability.

Files touched for this sprint: EnemyCombat.cs, LarperAwareness.cs, Larper.prefab;
existing Sprint017Tests.cs and Sprint019Tests.cs calibration assertions; this log
and TestResults/Sprint0201.json. Seeded duck testing retains the old 30%/36%
comparison and adds 48%; social variation checks the current 60% band and mixed
fear/confidence responses. The suppression fixture now waits 0.5 rather than
0.3 seconds for its existing 0.1-second perception/0.25-second position samples,
still inside the 0.6-second suppression window. Gameplay sampling is unchanged.

Validation: compilation succeeded; **41 Edit + 160 Play = 201/201 passed**, no
failures/skips, all 201 Sprint020 case names retained. The initial Play run was
158/160: the old eight-witness sample expected a nonreacting actor at 45% fear;
at 60% it instead contained both fear and confidence, so the assertion now checks
that meaningful variation. The retarget fixture also failed its short wait,
reproduced once, then passed without gameplay changes before the sampling-aware
wait adjustment. The subsequent full Play run passed. Existing cooldown, expiry,
shooting/reloading, varied reactions, blocked/offscreen perception, bounded-wave,
spawning/spacing, camera, input, traversal and ragdoll cases remain covered.

Final readback: prefab probabilities 0.48/0.60, duck cooldown 5–8, collapse 0.08,
spread 0.35. Warehouse_Mission01 saved, clean, open and stopped; no compilation
in progress, Console zero errors. One unrelated Unity AI Toolkit account-API
timeout warning remains; captured compile history also includes pre-existing
obsolete FindObjectsSortMode warnings in VisualIntegrationTests. No gameplay
errors found in the regression suite. No manual firefight playtest or PC/Android
performance benchmark was performed; subjective feel remains for developer
playtesting. Warehouse_Testbed hash unchanged from Sprint020. All earlier dirty
work preserved. No staging, commit or push.

## Sprint 020.2 — Near-Miss Suppression (2026-10-20)

AttackExecutor now includes its actual hit Health in the existing CombatNoise
event alongside the collision-clipped shot origin/end. Damage, accuracy, timing
and tracer endpoints are unchanged. EnemyCombat tests center-to-shot-segment
distance within the existing configurable 1.25-unit radius, excludes self,
friendly, zero-length and direct-hit trajectories (including protected hits),
then checks the short closest-point-to-actor path with the existing nonallocating
geometry ray buffer. This rejects enemies behind the impact wall or beneath
cover parallel to a passing trajectory. Hearing remains a separate, attenuated
awareness path; hearing a blocked shot does not imply suppression or sight.

New reusable Combat.SuppressionMeter is a small value type: event-driven additions,
lazy linear decay, no component/manager/Update or allocated collection. Default
per-miss gain is 28 scaled from 40–100% by proximity; cap 100, quiet gap 0.65s,
decay 18 units/second. Cowardly susceptibility 1.25, ordinary 1, confident 0.75;
existing fearful morale multiplies by 1.15, confidence by 0.9. Values other than
personality multipliers use Inspector settings on EnemyCombat/Larper.prefab.

Low suppression retains existing episode-based 48% ducking and 5–8s cooldown.
Crossing 25 adds a brief 0.25s hesitation using the existing timer, at most once
per second. At 60, eligible grounded Larpers roll an existing finite withdrawal:
base probability 55%, scaled by susceptibility/morale and clamped; failed rolls
also consume a 6s decision cooldown. Actual withdrawal/cover/collision/spacing
logic and 4.5s timeout are reused. Direction and cover occlusion use the incoming
shot origin, without inventing visual player knowledge. Repeated bullets do not
restart withdrawal. On completion, adequately armed suppression retreats resume
combat after the existing reengagement delay; low-ammo retreats still reload.
Incompatible reload, panic, ladder and parachute states reject new reactions.
Reset/re-enable clears the meter. Existing retreat speech is used after real
travel; no extra dialogue, UI, animation or visual assets were added.

Ally fear remains 60%; collapse probabilities, caps and cooldowns are unchanged.
Suppression emits no ally-defeat or collapse notifications. Existing camera fire
gates and uncertain last-known-position fire remain untouched. Event callbacks
reuse the current awareness subscribers; candidate misses add one short geometry
raycast, not per-frame bullet/enemy searches. No new per-frame meter work.

Files: CombatNoise.cs, AttackExecutor.cs, new SuppressionMeter.cs (+ Unity meta),
EnemyCombat.cs, Larper.prefab; new SuppressionRulesTests.cs and Sprint0202Tests.cs
(+ Unity metas), this log and TestResults/Sprint0202.json.

Validation: compilation succeeded. **42 Edit + 166 Play = 208/208 passed**,
zero failures/skips; all 201 Sprint020.1 case names retained. Six new Play cases
first passed independently, then in the full suite: actual shot trajectory and
protected direct-hit exclusion; real wall impact and parallel-cover shielding;
proximity/personality/cap/decay/reset; sustained misses causing finite retreat
with no damage or ally death, no repeated restart or forced full-mag reload;
failed-roll cooldown with continued shooting; self/friendly/zero-length/distant
rejection. New Edit case verifies exact lazy decay, quiet gap, cap and resumption.
Existing shooting/reload, crouch, perception, morale/collapse, spacing/spawning,
camera, traversal, touch and ragdoll regressions passed unchanged.

Final Editor audit: Warehouse_Mission01 saved, clean, open and outside Play Mode;
compilation stopped successfully, zero scene missing scripts, Console zero errors
and warnings. Prefab readback confirms all suppression defaults above, ducking
0.48, ally fear 0.60, collapse 0.08 and spread 0.35. Warehouse_Testbed hash remains
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.
No manual firefight playtest or Android hardware performance measurement was
performed. Remaining verification: subjective reaction pacing under sustained
crossfire, moving retreat/cover encounters on real mission terrain, and Android
CPU/frame-time profiling with dense gunfire. No new panic-fire mode was added;
existing uncertain fire, weapon handling and contextual retreat dialogue remain.
Earlier uncommitted changes preserved; no staging, commit or push.

## Sprint 020.3 — Suppression Fear & Playtest Ammo (2026-10-20)

Small tuning pass: existing linear proximity weight now ranges **0.4 -> 1.45**,
previously **0.4 -> 1.0**. Base nearMissSuppression remains 28: closest-pass peak
gain rises **28 -> 40.6 (+45%)** before personality/morale modifiers. The valid
outer-radius minimum remains **11.2 -> 11.2**; intermediate gains taper linearly.
Detection radius, collision-clipped trajectories and direct-hit/cover rejection
are unchanged. Cap 100, thresholds 25/60, decay 18/s, quiet gap 0.65s, reaction
cooldown 6s, personality/morale multipliers and finite retreat behavior unchanged.
Ducking probability 48%, ally fear 60% and collapse settings unchanged.

WarehousePlayer.prefab uses the existing Inspector initialReserveAmmo setting:
**36 -> 108 per firearm (3x)**. This isolates playtest reserves to the player
prefab; PlayerLoadout's generic default and enemy ammunition are unchanged. Both
starting firearms still have full, unchanged magazines. Weapon definitions,
damage, firing interval, reload duration and ammo-transfer logic are untouched.
The reserve setting can be returned to 36 for later balancing; ammo stays finite.

Files changed for this sprint: EnemyCombat.cs (one weight), WarehousePlayer.prefab
(one reserve setting), existing Sprint0202Tests.cs proximity assertions and
TraversalRegressionTests.cs ammo/HUD/reload assertions, this log and
TestResults/Sprint0203.json. No new test cases, systems, states or assets.

Validation: compilation succeeded, **42/42 Edit tests passed**. The full Play
run passed 164/166. Stronger buildup triggered an accumulated-suppression retreat
inside the old duck-only cooldown fixture; Sprint017Tests.cs now sets retreat
chance to zero in that fixture, preserving dedicated retreat coverage in
Sprint0202Tests. A pre-existing UI size assertion measured 187.0636 rather than
187.5 at the current panel scale, reproduced in isolation. Sprint020Tests.cs now
allows one logical pixel of layout rounding instead of 0.1; no UI assets changed.
All 10 Sprint017 and all 8 Sprint020 cases then passed. **All 208 existing cases
have passing latest results** (42 Edit + 166 Play); this combines the full run
and targeted reruns, not a second complete Play run. All Sprint020.2 case names
retained. Additional fixture files modified: Sprint017Tests.cs, Sprint020Tests.cs.

The updated proximity case verifies a 40–50% close-pass gain increase and exactly
unchanged outer-radius strength, with existing accumulation, cap, personality,
decay and finite-retreat checks. The ammo test verifies both full magazines and
108 reserves, correct HUD reserve text, and a one-round reload producing 107
reserve with unchanged capacity. Earlier weapon/mission/checkpoint tests passed.
No manual firefight or Android hardware verification was performed.

Final readback: player reserve 108, enemy reserve 135, duck 0.48, ally fear 0.60,
collapse 0.08/spread 0.35. Warehouse_Mission01 saved, clean, open and stopped;
no compilation in progress, Console zero errors/warnings. Earlier uncommitted
work preserved; no staging, commit or push.

## Sprint 021 — Living Battlefield (2026-10-20)

Cover/repositioning: useful protection is now evaluated at crouched-head height
rather than body center. Existing threat-side endpoint, reachable corridor/hop,
occupancy and combat reservation checks remain authoritative. Commit-time scoring
slightly disfavors required hops and includes bounded +/-0.25 variation. Armed
suppression retreats that actually reach cover hold a protected crouch for a
finite randomized 1.2–2.4s, without refreshing on every bullet. Protection is
rechecked using the existing 0.25s sampler. Expiry/invalid cover releases the
hold and normal sight-checked combat resumes. Low-ammo cover reloads retain
their existing behavior. Panic, traversal and encounter reset retain priority.

The existing combat-position sampler now considers occasional repositioning,
with Inspector interval 8–13s and base chance 20% (40% when pressured or unable
to fire effectively). Decisions wait for the current movement to finish and
reuse the existing safe candidate/reservation machinery. Tactical stand-off
changes by -1.25 (advance) or +1.75 (withdraw); advance preference is 65% for
overconfident, 30% ordinary and 10% cowardly actors. Grounded/same-level gates
preserve authored vertical navigation. Failed opportunities wait another
interval; equivalent locations are not continuously reselected.

Suppression: one base-value adjustment **28 -> 31.5 (+12.5%)**, in both code and
Larper.prefab. Existing distance weights 0.4–1.45 remain unchanged: peak gain
**40.6 -> 45.675**, outer-radius gain **11.2 -> 12.6**, before personality/morale.
No second multiplier. Cap, thresholds, decay, personality factors and reaction
cooldowns are unchanged; ducking stays 48%, ally fear 60%, collapse chance 8%,
spread 35% with existing caps/global lockout. No new morale events or cascades.

Malfunctions: WeaponCycle supports a finite obstruction to firing/reload with no
ammo consumption or ownership change. Existing cadence, committed reload and
empty/melee guards remain. Only the basic Larper prefab enables the AI trigger:
**0.75% per eligible firing attempt**, **1.4s recovery**, **18s cooldown**, with
a 2s initial/reset grace. Rolls occur after cadence, camera and trajectory-LOS
checks. Recovery uses existing Reacting behavior; 35% attempt existing ducking
(still subject to its own chance/cooldown). Malfunction dialogue is attempted
on 40% of events, through the existing expiring queue, speech gates and two-bubble
cap. Five contextual lines are available on all existing personalities. No new
AI state machine, animation or enemy archetype. Player weapons are unaffected.

Spawning/persistence: inspected and preserved. Runtime activation requires the
actor bounds plus camera safety margin to be offscreen after camera LateUpdate;
collision/support and neighbor spacing are checked before reveal. Active actors
are reused and not culled on camera exit. Authored starting occupants and covered
checkpoint resets retain their explicit existing semantics. No spawn rewrite.
Ordinary offscreen fire restrictions and explicit sniper-capability hook remain.

Feedback: actual hitscan-aligned pooled tracers already fade and were retained.
Material surface-impact sprites now fade over their existing lifetime instead
of abruptly vanishing. Existing concrete/metal/wood colors, pooled particles,
pool caps, audio limits and ragdoll impulses are unchanged. No new art/shaders.

Files for this sprint: EnemyCombat.cs, WeaponCycle.cs, LarperAwareness.cs,
LarperPersonalityProfile.cs, CombatFeedback.cs, Larper.prefab; new
Sprint021RulesTests.cs / Sprint021Tests.cs and Unity metas; existing
Sprint0202Tests.cs expectations scaled for the intentional +12.5% gain; this log
and TestResults/Sprint021.json. Player reserve remains 108 per firearm; no camera,
UI layout, startup, weapon definition, ragdoll or scene-geometry changes.

Validation: **44 Edit cases passed; 174 Play cases have passing latest results,
218 total**, all 208 Sprint020.3 names retained and ten new cases. Initial full
Play run was 172/174: the uninterrupted-burst deadline and an unchanged visual
jump/fall timing check failed, then both passed in isolation. The burst fixture
now explicitly disables the optional malfunction trigger to preserve its original
uninterrupted-fire premise; every shot/retreat/reload assertion and deadline is
retained, with forced malfunction/recovery tested separately. Final full Play run
was 173/174, including all 208 previous cases passing. The remaining new test had
assumed preferred distance 5 while the prefab uses 6, allowing a valid 0.25-unit
advance; setting its controlled preferred distance to 5 preserved the >1 movement
assertion. All eight new Play cases then passed together. Thus 218 is the latest
per-case result across the full suite plus targeted reruns, not a claim that the
last full Play invocation alone passed all cases. Existing Sprint012CombatTests.cs
fixture is also modified. No existing assertions were loosened or removed.

New coverage exercises head-height/occupied cover rejection independently,
physical movement/hopping to cover, crouched finite hold and real reengagement,
lease-based tactical movement with no sample-by-sample restart, exact one-time
suppression scaling, invalid/empty/reloading/melee malfunction rejection, ammo
and cadence preservation, real AI jam/recovery/cooldown, offscreen jam/fire
rejection, zoom-aware activation with retained HP/ammo/suppression, and actual
tracer endpoint/material fade/pool invariants. Compilation checks passed after
each major stage. Existing reachable low-cover/reload test also passed early.

Live Editor integration: reused the normal-input mission pilot, with real
movement, aiming, reloads and healing and no immunity/teleports/injected damage.
Mission completed in **26.6245 game seconds, ten defeats, zero deaths, two heals,
one naturally occurring malfunction**. Frame count advanced 996 -> 10263; Game
View/HUD inspected during traversal/combat. 674 actor samples: maximum suppression
45.0659, 27 suppressed rows, seven defensive-crouch rows, at most one visible speech
bubble. No tactical reposition or cover-hold samples occurred in this short lethal
run; prolonged-fire behaviors are demonstrated by focused physical integration
tests and still require subjective firefight playtesting. Natural collapse was
not observed; unchanged wave regression tests passed. Diagnostics:
TestResults/Sprint021Pilot.cs, Sprint021Live.json, Sprint021Actors.csv and
Sprint021GameView.png (screenshot stored outside Assets, not a game art asset).

Performance: new tactical decisions reuse the 0.25s position sampler; hold-cover
occlusion rechecks are bounded at that cadence, not every frame. Cover searches
occur only at movement commitment, reuse fixed buffers and existing path/spacing
queries. Weapon malfunction/recovery is scalar timing logic. Surface fading adds
no new effect objects or pools. No target-device FPS/CPU/GPU benchmark performed.
Manual PC/Android checks remain: long firefight pacing, crowded/vertical cover
traffic, visible onset/recovery/readability of rare malfunctions, low-end Android
frame times and touch gameplay. Dedicated peeking/leaning/advanced cover-shooting
animations are deferred to avoid a new animation system; existing crouch/stand,
LOS-gated fire and safe reengagement satisfy the current cover flow.

Final audit: Warehouse_Mission01 saved, clean, open and outside Play Mode;
compilation idle, Console zero errors/warnings. 280 scene/player/Larper/ragdoll
behaviours inspected with zero missing scripts or non-null missing references.
Audit uses Unity 6.6's EntityId property after the obsolete InstanceID audit API
was rejected; this was an eval-only diagnostic, not a project compilation error.
Readback: gain 31.5, basic unreliable weapon enabled at 0.0075, duck 0.48,
ally fear 0.60, collapse 0.08/spread 0.35. Warehouse_Testbed hash unchanged:
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.
Earlier work and historical diagnostic screenshots preserved. No staging, commit,
push or Sprint022 work.

## Character Rigging Addendum — Visible Second Arm (2026-10-20)

Root cause: both sprites and shoulder pivots existed, but ordinary armed poses
gave the rear arm the same angle as the carrying arm. The torso (order 10) hid
its upper segment (order 9), while the front arm (14) covered the remainder.
This was geometric occlusion, not a missing limb or sprite.

CutoutRig now gives the free/rear arm its own relaxed silhouette (-14 degrees
standing, -20 crouched, +/-12 running swing, small existing-recoil response).
Its shoulder, sprite, scale and length are unchanged. Existing climbing/raised
poses are retained; reload handling blends from the free pose into the existing
two hand strokes and back. Rear shading is a configurable 0.88 multiplier using
the existing sprite; no new artwork. Normal depth is behind the torso; the
reaching reload hand may draw at 11, still below front arm 14 and weapon 15.
ApplySkin restores both arm renderers and caches the rear renderer/order once.
The rig's existing mirrored root handles orientation changes and independent aim.
The carrying arm, weapon mount, muzzle math and recoil shortening are unchanged.

CartoonRagdoll copies source sorting and base rear-arm shading while preserving
the current pose snapshot and mirrored directions. It deliberately uses the
stable rig shade rather than copying a transient red damage tint. No collider,
mass, hinge, anchor, impulse, velocity limit or recovery mechanic was changed.

Five relevant prefabs updated through the Editor: PlayerCutoutVisual,
LarperCutoutVisual, WarehousePlayer, Larper and LarperCartoonRagdoll. Source
changes are limited to CutoutRig.cs and CartoonRagdoll.cs. Added focused
SecondArmRigTests.cs and SecondArmIntegrationTests.cs with Unity metas.
Diagnostics outside Assets: TestResults/ArmRigPreview.cs, ArmRig_Before.png,
ArmRig_After.png and CharacterRiggingAddendum.json. Preview columns are idle,
run, crouch, reload, climb and upward aim; rows are player/enemy facing each way.
The initial before diagnostic omitted an optional player equipped-sprite
fallback; that preview-only assignment was corrected for the final pose sheet.
The weapon assets and gameplay presentation selection are unchanged.

Focused validation: seven Edit and two Play cases passed. Pixel-difference
rendering checks 56 combinations (both characters, both facings, seven poses,
with/without reload) and requires actual rear-arm pixel contribution. Other
cases check mirrored aim and carrying-hand/muzzle alignment, saved prefab
sorting/sprites, mirrored ragdoll pose transfer and five intact hinges, bounded
physics, plus real player reload/cancel/recovery with restored normal depth.

Full regression validation: **51 Edit tests passed**; full Play run was 175/176.
The unchanged Sprint005 touch-pointer ladder dismount case failed its 0.08-second
timing assertion, then passed unchanged in isolation. **All 227 cases have passing
latest results (51 Edit + 176 Play)** across the full suite plus that rerun; all
218 prior Sprint021 names are retained. No existing tests or assertions changed.
Compilation and diff checks passed. The final pose sheet was visually inspected
for both characters and facings, including attached shoulders and reload/climb
silhouettes. No new runtime renderer, physics object or framework was introduced;
changes are scalar pose/sorting operations with cached renderer access, and
spawn-time ragdoll appearance transfer. Android hardware profiling and manual
human firefight testing were not performed.

Final audit: Warehouse_Mission01 saved, clean, open and outside Play Mode;
compilation idle, Console zero errors/warnings. 280 scene/player/Larper/ragdoll
behaviours checked: zero missing scripts or non-null missing references.
Warehouse_Testbed hash remains
DBD270D874D8BDE8F12B0EA43746C00F0906104F888B392718C3F053146F3915.
Earlier dirty work preserved; no staging, commit, push or unrelated sprint work.

## Sprint 022 — Contextual player cover, peeking and blind-fire (2026-10-20)

Implemented a local PlayerCover2D extension on WarehousePlayer, reusing the
existing crouch box, actions, weapon cycle and modular poses. No enemy decision
code, weapon assets, damage rules, ragdoll joints or animation assets changed.
Solid layer-0 geometry is sampled with a bounded non-allocating overlap and
horizontal face check at 10 Hz while eligible and crouched. Suitable obstacles
must be ahead, near the feet, within reach and of low-cover height. Triggers,
floors, tall walls and one-way platform effectors are rejected. There is no
snapping or root translation. Moving away, jumping/falling, climbing, disabled
simulation, defeat, input reset or selecting melee exits contextual cover.

Concealment lowers the existing damageable box and upper-body pose below the
crate top while retaining grounded feet and existing crouch movement. Real
geometry stops bullets; rear-side/unobstructed shots still damage the player.
Aim raises that same box and pose, exposing the upper body. Accurate firing
waits until enough body is exposed, including when a ceiling blocks expansion.
Releasing Aim conceals again. Expansion checks solid clearance; a blocked exit
retries normal crouch-height restoration once space becomes available. Reload
takes priority over raised-weapon poses. Recovery and ordinary stand/jump retain
their existing gravity and collider owners.

Blind-fire retains the low body and rotates the existing carrying arm to raise
the actual gun. The muzzle clearance path follows torso/shoulder, rigid arm and
barrel; obstructions retract the gun/hand together rather than moving the player
or letting shots originate through cover. Only active player cover adds the
extra held-path segment check; enemy clearance remains unchanged. Shot spread
is applied before the synchronous firing pose, retaining real muzzle/tracer
alignment, recoil, ammo, reload, sounds and impacts. Both arms remain attached,
visible and mirrored using the existing sprites and layering.

Controls: PC S/down + F without RMB blind-fires along the remembered aim;
S/down + RMB peeks; add LMB or F for accurate fire. Android holds the move stick
down and the new FIRE button for blind-fire, or uses the aim stick to peek
(its existing outer firing zone still works). FIRE and the aim firing zone are
combined independently; pointer cancellation, reset and focus loss release them.
No other controls were remapped. FIRE is above the aim pad, clear of existing
heal/reload/chute/interaction controls.

Inspector defaults: .55 m maximum edge gap, .85–1.15 m valid cover top above
feet, .45 m peek exposure (clamped to standing height), .14 s transition scale,
5x blind spread multiplier. Player weapons previously have zero angular spread,
so a configurable 5-degree reference floor yields uniform +/-25 degrees for
blind-fire; ordinary aimed accuracy is unchanged. Body concealment uses .05 m
top clearance; detection interval is .1 s with fixed 16-candidate/8-ray-hit
buffers. Raising height probes clearance during transitions, not steady poses.

Files for this sprint: new Combat/PlayerCover2D.cs (+ Unity meta); changes to
Combat/PlayerCombat.cs, Combat/AttackExecutor.cs, Traversal/PlayerPosture2D.cs,
Visuals/CharacterVisualDriver.cs, Visuals/CutoutRig.cs and
Input/WarehouseTouchControls.cs. UI: WarehouseControls.uxml/.uss and
MissionControls.uxml. Assets: WarehousePlayer.prefab and Warehouse_Testbed.unity,
edited through Unity APIs. The testbed reuses LowCover_Sprint012 at (-14,.45),
adds a control label and moves the existing encounter at x=6 to x=-4.5, forming
a compact crate encounter with the existing Larpers at -10/-7. Original active
encounter count, geometry and level-entry bindings are preserved. Unity saving
also serialized existing default fields previously absent from this older scene.

Validation: compilation succeeded. **51/51 Edit Mode passed. Full Play Mode
187/188; all 188 have passing latest results after one unchanged rerun** at
1280x720. The Sprint020 pad-size assertion failed twice at the tiny 321x531
Game View (186.18 versus 187.5 +/-1 logical pixels), then passed unchanged using
the existing 1280x720 Landscape preset. All 227 prior names remain, with 12 new
Sprint022CoverTests covering real protected/rear hits, peek damage/exposure,
seeded blind spread versus aimed accuracy, ammo/reload/healing, left/right
clearance, invalid geometry, ceiling-blocked accurate fire, movement/disable/
defeat/recovery, jump/gravity, delayed height restoration, rigid arms/feet/head
alignment, independent touch cancellation and both UI templates at four sizes.
No existing assertions were modified. Raw full-run and rerun results are saved
in TestResults/Sprint022.json (239 passing latest cases in total).

Performed controlled live Warehouse_Testbed validation and inspected its HUD:
concealed body top .85 m, blind muzzle above .9 m cover, peek box top 1.35 m
with rendered head top 1.329 m. Both arms remain visible. Capture used temporary
runtime input/position and damage protection only; real damage/protection tests
used no such protection. TestResults/Sprint022_Warehouse.png is a camera capture
without the HUD; screen capture including the HUD was inspected through MCP.
Sprint022PosePreview.cs / Sprint022_Poses.png render existing art in both facings
for comparison, with a documented static near-face stop for the concealed gun.
No replacement artwork generated. Diagnostics live outside Assets.

Final audit: Warehouse_Testbed saved, clean, open and Play Mode stopped; compiler
idle; 108 scene/prefab behaviours checked with zero missing scripts/references.
Final live Console counts: zero errors/warnings. Captured earlier compilation
included pre-existing obsolete-API warnings, and the Editor intermittently
reported FMOD output-device / Unity AI account connectivity warnings. No new
cover gameplay warnings observed. Source whitespace checks pass; whole-tree
git diff --check flags Unity's generated blank `m_Name: ` serialization line.
Android hardware performance, human control feel and device multitouch remain
manual validation items; no Android frame-rate claim is made. Existing damage
uses the body box, not separate raised-arm hitboxes. Very high/overhung cover
can physically obstruct blind shots; ammunition is not magically redirected.
Earlier arm-addendum changes preserved. Nothing staged, committed or pushed.

## Sprint 022.1 — Forward enemy spawning hotfix (2026-10-20)

Root cause: EnemySpawnPoint treated any off-screen location as eligible. A
visible or obstructed point could wait until the player/camera passed, then
activate in the rear. It had neither a progression history nor a terminal
activation latch, so repeat activation could also reopen an inactive actor.

Changed EnemySpawnPoint.cs only in production code. Added Inspector-configurable
progressionDirection (default east/+X, matching both warehouse layouts' overall
west-to-east progression) and minimumPlayerDistance (3 m of forward clearance
from the furthest recorded player progress to the candidate's rear edge).
Direction comes from authored spawn-zone data, never player facing, aim, input
or velocity. Existing authored locations remain fixed; there is no relocation
fallback, procedural population or global spawn manager.

Every unfinished point records monotonically increasing progress, including
before its zone is requested. A passed runtime point becomes terminally skipped;
turning, camera movement, backtracking and repeated Spawn/Activate calls cannot
reopen it. Already revealed actors keep running and may remain/pursue behind
the player. Defeated actors are not reactivated by repeat triggers. Only an
explicit encounter/checkpoint/replay reset clears this lifecycle history.
Temporarily unbinding a previously bound player pauses activation, retaining
history through rebinding. A defeated bound player cannot initiate new spawns.

Preserved the existing final camera check after camera LateUpdate, 1.25 m
visibility safety margin, .2 s retry, 1.5 m enemy-spacing check, non-trigger
body clearance and two ground-support checks (.6 m distance, upward normal).
Added a camera-depth validity guard. Invalid/visible/near/crowded forward
locations wait; passed locations are skipped. No additional physics queries,
global searches or per-frame allocations were introduced; progress updates
are scalar/bounds checks and cease after activation or skipping. No enemy AI,
combat, navigation, camera or suppression code changed. Ordinary off-screen
shooting restrictions and the explicit off-screen/sniper capability are intact.

Inspected Warehouse_Testbed and Warehouse_Mission01 through Unity preview
scenes. No scene/prefab/asset edits were needed for this hotfix. Testbed retains
its nine configured points/seven enabled startup encounters; the mission's ten
existing scene occupants and staged objectives retain their current lifecycle.
Preplaced occupants remain existing actors, including covered checkpoint resets;
they are not treated as newly generated rear enemies. Skipping a missed runtime
point can reduce actual testbed population when rushing past it, as required
instead of spawning behind; configured counts were not increased or moved.

Added Sprint0221SpawnTests.cs (+ Unity meta) with nine focused Play Mode cases:
forward/rear selection with camera margin; turning/backtracking/defeat repeat
calls; a visible pending point passed by player/camera; authored reverse route;
camera movement/proximity; ground/obstacle/spacing validation; persistent actor
ammo and regular off-screen firing gate; unbind/rebind history; explicit reset
versus completed reset. Prior tests/assertions remain unchanged. Full validation
results are recorded below after the regression run.

Limitations: this lightweight guard uses authored per-zone linear progression,
not a new route graph or pathfinding system. Future reverse/branching routes
must author the appropriate direction on their spawn points. Reachable terrain
continues to use designated level positions with existing clearance/support
validation, rather than introducing navigation queries. Device profiling and
human playtesting are separate follow-up validation; no new performance claim.
Earlier uncommitted Sprint022/arm work is preserved; nothing staged or committed.

Final validation: compilation succeeded; **51/51 Edit Mode and 197/197 Play
Mode tests passed in the full runs**, including all nine new spawn tests and
all 239 previous cases. No existing tests/assertions changed. Mission enemy
counts, checkpoint rollback/replay, off-screen firing and explicit exception,
terrain/spacing, persistence, camera, combat and cover regressions are green.
Raw results: TestResults/Sprint0221.json. Source diff whitespace check passed.
Final Editor audit: Warehouse_Testbed clean/open, Play Mode stopped and compiler
idle. All authored point directions resolve to east in both warehouse scenes;
spawn archetype/reference validation remains intact. Existing Unity account
connectivity warnings are unrelated; no new spawning exceptions observed.

## Sprint 022.2 — Authored occupants and player animation stabilization (2026-10-20)

Scope confirmed by the developer: the three-enemy opening is Warehouse_Testbed,
not Warehouse_Mission01's separate one-guard Arrival group. Mission composition
and difficulty remain unchanged. Inspected both scene layouts, actual runtime
actors, pending spawn states, progression/camera filters, terrain/spacing checks,
checkpoint/replay lifecycle, player motor/interpolation, input ownership, cover,
procedural poses and ragdoll handoff before changing production code.

Verified spawning causes: the testbed's authored startup encounters were still
using runtime dynamic activation. During a live reproduction at the start,
EnemySpawn_WitnessPatrol at (-10,.8) was instantiated but inactive/pending while
the points at (-7,.8) and (-4.5,.85) were active. Its expanded body intersected
the camera safety margin. After advancing past it, the Sprint022.1 terminal
skip marked it skipped/inactive, silently removing the third opening actor.
This also made other initial occupants dependent on camera position and timing.

Fixed the scene through Unity APIs: seven existing enabled startup points now
reference seven scene-authored instances of the existing Larper prefab using
the already established placedActor path. All seven exist before the first
gameplay frame; exactly three belong to the opening at -10/-7/-4.5. Positions,
personalities, archetypes, enabled point counts, weapons and terrain are unchanged.
All seven placement bodies passed solid-clearance checks; opening terrain
support and spacing are checked by regression tests. The two deliberately
disabled startup points remain disabled. No enemies were added to encounter
composition: this restores the intended seven residents, including all three
opening Larpers. Mission01 already uses authored occupants and was not edited.

Dynamic spawning retains the authored forward direction/highest-progress guard,
camera safety margin/final LateUpdate check, proximity, spacing and ground checks.
A failed/passed dynamic member remains pending with a specific BlockReason;
it is never silently treated as fulfilled, removed from the intended count or
relocated behind the player. Backtracking cannot erase progress. Successful or
defeated actors retain the one-activation latch, health/ammo/morale and persistence.
An explicit encounter reset still controls replay/checkpoint lifecycle. Optional
Log Spawn Diagnostics is an Editor-only context-menu command: encounter/point,
intended/instantiated/active count, pending/finished state, rejection reason,
authored position, progression direction and actor presence. No automatic or
per-frame logs, and the logging code is excluded from player builds.

Verified animation causes with controlled failing Play Mode reproductions:
(1) velocity reversals of +/- .2 m/s mirrored the player even with rightward
input held; (2) .14/.16 m/s noise crossed the single .15 Idle/Walk threshold,
alternating leg poses; (3) repeated shot pose evaluation in one frame advanced
the crouch blend again. The third reproduction uses a deliberately slow blend
to measure the extra time advance independently of frame rate. These demonstrate
specific code defects, not a claim to reproduce every subjective hitch on a device.

CharacterVisualDriver now derives player facing from held movement intent and
retains it when neutral; independent aim and enemy velocity-facing are unchanged.
Player locomotion uses a small .3 m/s start / .1 m/s stop hysteresis. Motor speed,
acceleration, collision, jump and controls were not smoothed or changed. Pose
phase and CutoutRig crouch/climb blends advance once per render frame, including
frames where synchronous muzzle preparation is followed by LateUpdate. Poses
can still refresh the actual aim/weapon/recoil/hitbox presentation immediately.
Recoil, hit flinch, reload and ragdoll parameters remain intact. No cover detection
retuning was necessary; repeated stationary cover and moving exit tests are stable.

Ruled out during inspection: the old DesktopPlayerInput is disabled; there is
one modular rig/driver; Rigidbody2D already uses Interpolate/Continuous with
rotation frozen. The fallback crouch renderer does not render over a modular rig,
and the player's current recovery architecture is checkpoint respawn, not a live
physics ragdoll. Existing enemy ragdoll physics/ownership are preserved. No
speculative interpolation, camera, navigation or physics-joint rewrite was made.

Files: EnemySpawnPoint.cs (deferred reasons/authored lifecycle/Editor diagnostics),
CharacterVisualDriver.cs (player facing/hysteresis/frame clock), CutoutRig.cs
(optional single-advance pose evaluation), Warehouse_Testbed.unity (existing
startup resident references), new Sprint0222MovementTests.cs and
Sprint0222OpeningTests.cs with Unity metas. Sprint0221SpawnTests.cs now requires
passed points to remain pending with PassedProgress rather than silently skipped;
the forward/backtracking/persistence/reset assertions remain. Three old Edit Mode
scene tests now assert seven authored enemy colliders separately while retaining
the exact 31 environment/player collider count and all existing geometry checks:
Sprint008PresentationTests.cs, Sprint013RulesTests.cs, Sprint014RulesTests.cs.

Focused validation: all seven new stabilization tests and all nine updated spawn
cases passed; all 51 Edit Mode tests passed. New tests cover pre-input three/seven
counts, two full scene restarts, no repeat-trigger duplicates or defeated-actor
revival, terrain support, controlled micro-velocity/facing and pose-clock repros,
fast run/reversals/stops, moving fire/reload, hit/respawn recovery, stable cover,
crouch movement, jumping/landing and rigid arm/weapon/root alignment. Full suite
and final audit results are appended below after completion.

Limitations: a dynamic point with no valid forward location stays explicitly
pending until a suitable authored location or reset is available; no automatic
alternate-point selection or navigation framework was introduced. Current required
startup counts are guaranteed through scene authoring. Human visual/control-feel
playtesting and Android device profiling remain necessary; no hardware frame-rate
claim is made. The verified movement defects are fixed without claiming that all
possible platform-specific visual artifacts have been measured. Earlier dirty
work is preserved; nothing staged, committed or pushed.

Final results: **51/51 Edit Mode and 204/204 Play Mode passed in full runs**,
255 total cases including seven new stabilization cases. All 248 previous
cases are retained, with the documented dynamic-defer and scene-collider
contract updates; two test names replace "Skipped" with the new deferred/passed
terminology. Existing ragdoll, morale, suppression, off-screen fire/sniper
exception, camera, reload, healing, cover and mission checkpoint/replay tests
are green. Compilation succeeded; source whitespace checks passed. A final
post-suite code change was comment-only and recompilation also succeeded.
Raw before/after runtime captures, controlled failure repros, full test results
and audit are saved in TestResults/Sprint0222.json.

Final live audit confirmed all three opening actors active, none pending,
all using Larper, seven instantiated/active startup actors total and zero player
movement input. This was a controlled count audit, not a human playtest; runtime
damage protection was used only for that follow-up count capture. Terrain and
damage/recovery checks used their separate automated fixtures. Final saved scene
audit: Warehouse_Testbed clean/open, Play Mode stopped, compiler idle, seven
placedActor references; 179 behaviours with zero missing scripts or dangling
references. Console ground truth: zero errors; one pre-existing Unity AI account
connectivity warning remains. Earlier compilations also emitted existing API
deprecation warnings outside the changed gameplay code. No new gameplay errors
observed. Nothing staged, committed or pushed.

## Sprint 022.3 — Movement presentation, combat spacing and suppression (2026-10-20)

Implemented by Codex:

- Continued from the existing dirty Sprint 022 work, without resetting it. No
  spawning code, scene composition, enemy count, camera controller, damage,
  detection range, health, controls, rig proportions or animation assets changed.
- Remaining movement source identified: CharacterVisualDriver rounded the player's
  already interpolated Rigidbody presentation to a 1/32-world-unit grid every
  render frame. A render callback trace measured a 0.016069-unit presentation
  error before the fix; no physics rewind or late-to-render transform conflict
  was observed in the controlled flat-ground test. The player visual now follows
  the interpolated root directly, with zero added smoothing. Enemy snapping and
  camera framing remain intact. Existing acceleration/deceleration, facing
  hysteresis and one-pose-clock-per-frame fixes are preserved.
- Crowding sources: destinations were reserved but cover objects were not;
  avoidance was bypassed while airborne, and a .75-unit vertical cutoff ignored
  neighbors standing/hopping on a low crate. Existing reservation registry now
  leases one slot per small cover collider, releases it with the owning position,
  and rejects occupied cover at selection/commit. Cached avoidance also applies
  during hops; the same-platform vertical tolerance is 1.6 units. It uses the
  last occupied platform height while airborne, rather than the jump's current
  height, so followers cannot walk underneath a jumping neighbor. Separate floors
  remain independent. No teleportation, rigid formation or new navigation system.
- Existing suppression already had clipped-segment near misses, distance weighting,
  bounded accumulation/decay, hesitation, crouching and withdrawal. Added the
  missing pressure-related aim imprecision: 0 to +10 degrees above the moderate
  threshold, returning to ordinary spread as pressure falls. Confident actors
  decay pressure 20% faster (21.6 rather than 18 units/s). Gain 31.5, distance
  weights .4–1.45, thresholds 25/60, cap 100, quiet gap .65 seconds, ordinary
  decay 18/s, retreat chance .55 and reaction cooldown 6 seconds are preserved.
- Impacts on reserved cover can create local pressure without projecting a shot
  through arbitrary walls. A separate barrier and parallel non-impact path still
  reject the event. Pressure distance accounts for the selected crate's depth
  (bounded at 1.8 units), so its front face is not incorrectly considered distant
  from the actor occupying its rear face. These checks occur on shot events,
  never in a frame scan.
- Full suppression panic uses existing personality chances (ordinary .08,
  coward .20, confident .02) only after a saturated, uninterrupted burst lasting
  at least 1.5 seconds, on an existing high-pressure decision opportunity, after
  the retreat roll fails. Each eligible panic roll, including failure, consumes
  at least 45 seconds. It reuses the finite panic action and existing dialogue;
  it never broadcasts a defeat or starts a morale-collapse chain.
- Witness pipeline inspected end to end: Health defeat -> one event ID -> nearby
  FOV/LOS test -> probability/cooldown -> timed morale -> action/expression/speech.
  Holding-cover/retreating actors previously rejected the entire event. They now
  receive eligible finite influence without interrupting cover/retreat; immediate
  panic and defensive action retain the incompatible-state guards. Reloading and
  ladder traversal remain protected. Ordinary fear .60, defensive duck .48,
  witness radius 9 with LOS, five-second influence, and bounded rare cascade are
  retained. Morale cooldown changed 18 -> 45 seconds in the three personality
  assets and source default. Unity serialized their existing malfunction lines
  when saving; no new dialogue was authored.
- Added an Inspector psychological-suppression opt-out, covering pressure,
  accuracy penalties, duck/retreat/panic and witnessed morale while retaining
  ordinary hearing. No Grandpa prefab/archetype exists in current project data;
  its future/current external prefab must disable this option. No new archetype
  was invented. Existing speech maximum two bubbles, 3–4-second reading time and
  cascade chance/caps are unchanged.

Focused automated verification initially passed 8/8 new Play Mode cases; the
movement presentation test failed before the visual fix and passed afterward.
They exercise rendering, cover-lease release, airborne/low-crate spacing versus
separate floors, pressure spread/decay/personality, cover-impact rejection rules,
immunity, cover-state witnessing and non-propagating panic cooldown. All 51 Edit
Mode cases passed in the full run. Full Play Mode results and final scene audit
are appended below when complete. These are automated Editor runs, not a human
visual/control-feel playtest or an Android performance benchmark.

Manual acceptance playtest:

1. Start Warehouse_Testbed. Count the three opening Larpers; approach, stop and
   reverse repeatedly with A/D or the movement joystick, including firing while
   moving. Watch torso/root motion rather than ordinary leg swing. Repeat around
   a crate while crouching (S/down), peeking with aim and blind-firing without aim.
2. Let the opening encounter run, then pressure enemies into retreat. Observe
   distinct ground/cover positions; brief incompetent crowding is acceptable,
   persistent overlap or all three collecting at one box is not. Backtrack and
   turn around: existing enemies persist and no new rear enemies appear.
3. Aim just past a Larper, then fire repeated close misses or hit its occupied
   cover. Compare an isolated distant miss. Look for hesitation, intermittent
   duck/retreat, inaccurate return fire and occasional finite panic. Stop firing
   for 7 seconds: normal combat should resume. Full panic should remain uncommon.
4. Defeat an ally in a nearby Larper's view, then repeat with a wall or >9-unit
   separation. Visible eligible witnesses may react differently; hidden events
   must not grant knowledge. Confirm readable dialogue with at most two bubbles.
5. Confirm reloads, cover/peek/blind fire, camera visibility restrictions, ladder
   travel, healing and ragdolls during ordinary play; check on an Android device
   for control feel, sprite crispness and sustained frame rate. Stop Play Mode.

Human visual acceptance, a hardware frame-rate comparison and a Grandpa-specific
prefab check remain unverified. The render trace identifies/removes quantized
player presentation; it does not prove absence of every platform-specific visual
artifact. Cover impact pressure is local to existing selected cover, rather than
an expensive level-wide impact/morale scan. No double jump was implemented.

Sprint 022.3 modified files (earlier dirty work in other files is preserved):
Assets/Scripts/Visuals/CharacterVisualDriver.cs;
Assets/Scripts/Enemies/CombatPositionReservations.cs, EnemyCombat.cs,
LarperBrain.cs, LarperAwareness.cs, LarperPersonalityProfile.cs;
Assets/Data/Enemies/Personalities/{Ordinary,Cowardly,Overconfident}.asset;
Assets/Tests/PlayMode/Sprint0223MotionTests.cs, Sprint0223CombatTests.cs,
Sprint0223OpeningTests.cs and their Unity-generated .meta files;
DEVELOPMENT_LOG.md and TestResults/Sprint0223*.json evidence reports.

Final validation: **51/51 Edit Mode and 213/213 Play Mode passed**, 264 total,
preserving all 255 previous cases and adding nine focused cases. The full Play
Mode suite was repeated after the platform-height fix. Long run_tests responses
hit the MCP client's 300-second response timeout; subsequent test_status calls
confirmed completed successful suites, rather than assuming success from the
transport timeout. Final complete results are saved in TestResults/Sprint0223.json.

The added 12-second real Warehouse_Testbed opening test caught the jumping-neighbor
bypass after the isolated spacing tests passed: minimum gap .312 units and
10.8 seconds of persistent crowding before the platform-height correction. After
that correction its closest horizontal gap was 1.74 units in the focused run
and 1.656 units in the full-suite run, both with zero persistent crowding, all
three opening enemies still present, and no additional startup
enemies. Detailed samples are in TestResults/Sprint0223OpeningTrace.json. The final
render callback trace reports zero player/root presentation error, zero unexpected
backward movement and zero late-to-render transform change; see
TestResults/Sprint0223MotionTrace.json. These are controlled automated runtime
observations, not a human playtest or a guarantee for every random encounter.

Final Editor audit: Warehouse_Testbed saved and open, Play Mode stopped, compiler
idle; 179 behaviours, zero missing scripts and zero dangling references; seven
placedActor references, including exactly three opening actors at authored X
positions -10, -7 and -4.5. Console ground truth after final tests: zero errors
and zero warnings. Source whitespace checks passed and the Git index is empty.
Nothing staged, committed or pushed. Manual acceptance and Android checks above
remain outstanding; no additional features were implemented.

## Sprint 022.4 — Double jump and readable suppression (2026-10-20)

Implemented by Codex, preserving Sprint 022.3's interpolated player presentation,
grounded animation, camera and horizontal acceleration/deceleration:

- PlayerMotor2D owns one additional airborne jump, enabled on WarehousePlayer
  only. Ground launch remains 12 units/s; air launch replaces vertical velocity
  with 10.2 units/s (configurable .85 multiplier), rather than adding impulses.
  Walking off a ledge leaves one air jump. Genuine floor support resets the
  budget; wall contact, cover, input reset and motor disable/re-enable do not.
  Explicit checkpoint respawn resets it. Ladder jump-off consumes the ground
  slot without granting another air slot; successful air jumping closes an
  open parachute through its existing API. Enemy motors retain single jumping.
- PC Space retains its existing pressed-this-frame input. Android uses the
  existing neutral-to-up joystick edge, now forwarded while airborne; holding
  upward cannot double jump or auto-jump after landing. Existing reachable
  ladder intent still takes priority. No new UI or input framework.
- The second jump gets an .18-second, 14-degree opposing leg kick and surprised
  face through CharacterVisualDriver. It changes no root transform, camera,
  arm/weapon alignment, grounded bob, rig assets or physics joints.
- Suppression diagnosis: distance was measured only to the changing torso
  centre. Real shots just above the visual head were weak at the old radius;
  after ducking, that same overhead path could fall outside the radius entirely.
  A failed duck roll also prevented retry for the entire continuous burst.
  Overhead proximity now also uses a standing-height head reference plus a
  configurable .35-unit bias, retaining distance falloff. LOS still targets the
  actual body. Real clipped hitscan segments, direct-hit exclusion, protective
  walls/ceilings, reserved-cover impacts and psychological immunity are retained.
- Existing shot-event processing remains the only accumulator; no per-frame
  suppression scan, allocation, projectile simulation or new manager. Ducking
  can retry during a pressured burst after its ordinary cooldown, without
  extending an active duck or rerolling for every bullet. Personality scales
  the duck chance as well as pressure gain/retreat. Full panic chances, its
  prolonged-exposure requirement/45-second limit and bounded morale cascades
  are unchanged.

Suppression settings, before -> after (source defaults and Larper prefab):

| Parameter | Before | After |
| --- | --- | --- |
| Per-event gain before distance/personality weighting | 31.5 | 40 |
| Moderate/high thresholds | 25 / 60 | 18 / 55 |
| Decay after quiet gap, units/s | 18 | 12 |
| Quiet gap, seconds | .65 | .8 |
| Ordinary defensive duck probability | .48 | .70 |
| Defensive decision cooldown, seconds | 5–8 | 3–5 |
| High-pressure retreat probability | .55 | .70 |
| High-pressure reaction cooldown, seconds | 6 | 4 |

Cap 100, near-miss radius 1.25, distance weights .4–1.45, +10-degree bounded
pressure spread, existing finite duck durations and 20% faster confident decay
are retained. Coward/ordinary/confident gain multipliers remain 1.25 / 1 / .75;
duck probabilities now also use these differences. Ordinary full panic stays
.08, coward .20 and confident .02, under the existing restrictive panic gate.
No changes to damage, weapon cadence/reloads, spawning, enemy spacing, morale
event probabilities, dialogue limits, player cover, camera or ragdoll assets.

Automated verification and final Editor audit are appended after completion.
Human PC/Android playtesting and device performance remain unverified.

Focused validation: all nine new Sprint0224Tests cases passed in Unity Play Mode.
They exercise unchanged first launch, capped second launch and horizontal momentum,
third-jump rejection, landing reset, held/fresh Android gestures, wall contact and
motor re-enable, cover exit with unchanged visual-root/weapon alignment, modest
air-jump pose, checkpoint recovery and enemy single-jump behavior. Suppression
cases use real AttackExecutor hitscan shots for a three-shot overhead burst with
no damage, visible ducking/high pressure and complete recovery, plus controlled
segments for standing-versus-ducking consistency, shielded/far rejection, a failed
duck's cooldown-limited retry in the same burst, and short-burst hesitation/retreat
without full panic or cascade. Forced probabilities isolate action paths in
several tests; the existing seeded frequency/variation suite covers ordinary
random choices. These are automated runtime tests, not human gameplay acceptance.

Legacy test updates preserve coverage while replacing obsolete tuning contracts:
Sprint017Tests.cs checks bounded pressure-related spread, .70 default ducking and
the seeded .30/.36/.48 historical comparisons plus .70 current opportunities;
Sprint0202Tests.cs checks exact new gain/head weighting, complete slower decay
and eligibility after the new four-second reaction window; Sprint021Tests.cs
checks exact new overhead values/decay/cooldown; Sprint0223CombatTests.cs allows
the new finite decay duration; TraversalRegressionTests.cs replaces the obsolete
airborne-rejection contract with one fresh-gesture airborne boost while keeping
diagonal movement, held-input and landing checks. No previous cases were removed.

One full run also exposed an older nondeterministic Sprint020Tests.cs fixture:
it teleported a newly created actor into empty air, calling that a separate
platform. Sprint022.3 intentionally remembers the last occupied floor while an
actor is airborne, so whether that fixture passed depended on neighbors' sampled
positions. The fixture now supplies a real upper platform and confirms ground
support before asserting independent movement. The corrected focused test passes.
No spacing gameplay code was changed for this fixture. The final full run follows
these test updates; 51/51 Edit Mode cases have already passed.

Modified production files: Assets/Scripts/Prototype/PlayerMotor2D.cs;
Assets/Scripts/Input/PlayerActions.cs; Assets/Scripts/Traversal/PlayerTraversal2D.cs;
Assets/Scripts/Checkpoints/PlayerRecovery.cs;
Assets/Scripts/Visuals/CharacterVisualDriver.cs;
Assets/Scripts/Enemies/EnemyCombat.cs;
Assets/Prefabs/Player/WarehousePlayer.prefab and Assets/Prefabs/Enemies/Larper.prefab.
Verification files: new Assets/Tests/PlayMode/Sprint0224Tests.cs and its Unity .meta;
the six existing test files listed above; DEVELOPMENT_LOG.md;
TestResults/Sprint0224.json. Earlier dirty work remains intact.

Manual acceptance steps:

1. In Warehouse_Testbed, run/reverse with A/D, press Space twice with a deliberate
   release between presses, then try a third press. Land on ground and boxes;
   repeat. Horizontal response and the first jump should match Sprint022.3.
2. Hold Space once; on Android hold upward once. Neither should auto-double-jump.
   On Android return to neutral and push up again while airborne for the boost.
   Repeat near walls, cover, ladder capture zones and platform edges. Look for
   the brief leg kick; confirm no root twitch, third boost or accidental refill.
3. Fire two or three shots just above exposed Larper heads, then repeat while
   they duck. Compare a distant path and a protected enemy behind solid cover.
   Look for hesitation/ducking or retreat within a short burst, with different
   responses across personalities. Stop fire for about ten seconds and confirm
   recovery, normal shooting/reloading and no permanent immobilization.
4. Check cover/peek/blind fire, enemy spacing/persistence, dialogue/morale and
   ordinary movement on PC and Android. Full panic/cascades should remain rare.
   Stop Play Mode after testing. Hardware performance and control feel need a
   human/device check; no device-level frame-rate claim is made here.

Final validation: **51/51 Edit Mode and 222/222 Play Mode passed**, 273 total,
including nine new cases and all previous coverage with the documented contract
updates. Final full runs followed the platform-fixture correction and air-jump
visual recovery reset. Compile and whitespace checks passed. Long run_tests calls
hit the MCP client's 300-second response timeout; test_status subsequently
confirmed completed suites. Full results, earlier failures/contract updates,
focused tests, parameter changes and final audit are saved in
TestResults/Sprint0224.json. No cases were skipped or removed.

Final live Editor audit: Warehouse_Testbed saved/open, Play Mode stopped and
compiler idle. The actual scene player has air jump enabled, launch speed 12 and
air multiplier .85, with Rigidbody interpolation still Interpolate. All seven
authored enemy occupants inherit gain 40 and duck chance .70, and all retain
single-jump motors. Audit found 179 behaviours, zero missing scripts and zero
dangling references. Console ground truth after the final tests: zero errors and
zero warnings. Git index empty; nothing staged, committed or pushed. Human
control-feel/visual acceptance and Android performance remain unverified.

## Sprint 023 — Suppression, Faintanyl & Duct Tape, supplies and wallets (2026-10-20)

Implemented by Codex:

- Suppression reuses Sprint022.4's real clipped hitscan segments, standing head
  envelope, protective LOS/cover checks, bounded reactions and personality weights.
  Gain changed 40 -> 49 (+22.5%); moderate/high thresholds 18/55 -> 16/48
  (-11.1%/-12.7%). Duck/retreat probabilities, panic probabilities, 45-second major
  panic gate, cascade limits, decay 12/s, quiet gap .8s and dialogue are unchanged.
- PlayerHealing replaces instant unlimited 35-HP use with continuous held healing
  at 25 HP/s. Initial tape is exactly 200 HP, capacity 200, and only HP actually
  restored is deducted, 1:1 including fractional HP. Full health/empty tape/release
  stop spending. Damage interrupts for .8s and, by default, requires release and a
  fresh hold. Shooting, reloads, jumping, climbing and parachuting take priority;
  ordinary grounded movement and crouched cover remain available without a speed
  change. These rates, capacities and damage interruption settings are Inspector
  fields. TryHeal/RequestHeal now begin held intent rather than granting instant HP.
- H uses held keyboard state; the existing Android heal button handles captured
  pointer down/up/cancel/lost-capture and input reset. Its press/release handlers
  run before Button's Clickable handler, which initially swallowed pointer-down
  in the real UI test. HUD shows current HP, tape, healing/interruption feedback
  and the satirical Faintanyl + Duct Tape identity even with an empty weapon slot.
- PlayerSupplies provides actor-local finite pickup/earnings tracking, using the
  existing interaction event and PlayerLoadout/PlayerHealing APIs. SupplyCrate2D
  has 50 tape HP and 72 reserve rounds by default, accepts E or the contextual
  SUPPLIES button within 1.8 units, and checks solid LOS. Only accepted quantities
  leave the crate. Unaccepted stock remains; depleted stock never resets from
  disable/re-enable. Player reserve ammunition has a configurable 432-round cap
  per firearm for replenishment; magazine sizes, damage and reloads are untouched.
- Two supply props per warehouse are authored through Unity's prefab/scene APIs,
  using the existing ShippingCrate art, a cyan band and a short world label. They
  sit by the safe entry and mezzanine/checkpoint area, on existing terrain. Their
  interaction volumes are trigger-only; original terrain and cover remain intact.
- EnemyWalletDrop reuses Health.Defeated, including the event invocation after
  ordinary enemy deactivation. Each Larper can drop one wallet worth 5–20 currency
  units (configurable range/chance), with a stable per-actor reward ID. WalletPickup
  automatically collects within 1.2 units with LOS and emits exactly "Wallet
  collected"; the amount never appears in combat UI. Actor drop flags and player
  reward-ID claims independently prevent duplicates. Wallets use tiny reused pixel
  sprites and triggers; no new physics bodies or update-time global searches.
- Existing WarehouseMission/MissionHud freeze earnings at completion and append
  them to the existing results statistics. No shop/menu framework was added.
  Tape, crate stocks and wallet claims persist across checkpoint death, preventing
  refill/reward farming; full Replay clears earnings/claims, resets tape and crates,
  and retires old drops. Weapon ammunition retains the existing checkpoint rollback
  policy: ammunition collected after a checkpoint can be lost on death while its
  finite crate stock remains consumed. This conservative policy is intentional;
  no broader checkpoint inventory rewrite was introduced.
- Surface impacts already faded. CombatFeedback now also fades hit confirmation
  rather than abruptly hiding it, resets its alpha on each hit, and fades pooled
  material particles over each particle's actual randomized lifetime. Existing
  colors, materials, pool sizes, tracer behavior and impact sounds remain intact.

New pickup proximity uses trigger contacts and a small nearby-crate registry.
Wallet stay LOS retries are throttled to .2s; scene enumeration occurs only at a
full mission replay. No new per-frame physics scan, global manager, movement
writer, camera behavior, rig, enemy archetype or visual redesign was introduced.
Sprint022.4's double jump and Sprint022.3's unsnapped interpolated player visual
are untouched. Earlier dirty work is preserved; nothing staged, committed or pushed.

Focused verification: 8/8 new Sprint023Tests pass, covering held/released/full/empty
healing, exact conservation, damage rearming, cover/fire/reload/air priorities,
partial capped supplies, real E interaction, depletion/toggle/replay, wall-blocked
supplies/loot, real defeated-actor automatic pickup and duplicate prevention,
and hit/material impact fading without pool growth. The mission completion/replay
test also passes with earnings retained across death, displayed only in results,
frozen after completion and reset by replay. A genuine held/released UI Toolkit
button test passes across existing screen-size/non-overlap checks. 51/51 Edit Mode
cases pass. Full Play Mode results and final audit are appended below.

Legacy coverage is retained: old instant-heal contracts in Sprint015/Sprint020/
Sprint022Cover tests now assert held finite healing, and Sprint015Rules checks
the new 200/200 configuration. Exact suppression gain checks reflect 49 while
retaining proximity/personality/decay/cooldown assertions. Three older scene tests
count the two new pickup triggers separately while retaining the exact 31 original
environment/player colliders, seven authored Larpers and all geometry/binding checks.

Modified production files: Assets/Scripts/Combat/PlayerHealing.cs, CombatFeedback.cs;
Assets/Scripts/Input/PlayerActions.cs, WarehouseTouchControls.cs;
Assets/Scripts/Inventory/PlayerLoadout.cs and new PlayerSupplies.cs,
SupplyCrate2D.cs, WalletPickup.cs; new Assets/Scripts/Enemies/EnemyWalletDrop.cs;
Assets/Scripts/Enemies/EnemyCombat.cs; Assets/Scripts/Missions/WarehouseMission.cs,
MissionHud.cs; Assets/UI/WarehouseControls.uxml, MissionControls.uxml,
WarehouseControls.uss; Assets/Prefabs/Player/WarehousePlayer.prefab,
Assets/Prefabs/Enemies/Larper.prefab and new Assets/Prefabs/Pickups/{SupplyCrate,
WalletPickup}.prefab; Assets/Scenes/Warehouse_Testbed.unity and Warehouse_Mission01.unity.
Verification: new Assets/Tests/PlayMode/Sprint023Tests.cs; existing Sprint015,
Sprint020, Sprint022Cover, Sprint014Mission, Sprint0202, Sprint021 Play Mode tests;
Sprint008Presentation, Sprint013Rules, Sprint014Rules, Sprint015Rules Edit Mode tests;
Unity-generated meta files for new assets; DEVELOPMENT_LOG.md and TestResults/Sprint023*.

Manual acceptance:
1. Damage the player, hold H / the HEAL button, then release. Check gradual HP
   gain and equal tape reduction; confirm no consumption at full HP/empty tape.
   Take a hit while healing: release/rehold after interruption. Repeat in cover,
   while walking, firing/reloading, and around single/double jumps and ladders.
2. Approach a cyan-marked supply crate near the entry or checkpoint. Press E /
   SUPPLIES. Check capacity limits, partial stock retention and EMPTY after use.
   Backtrack and die at a checkpoint: depleted stock must not refill.
3. Defeat a Larper and walk near its wallet. Check "Wallet collected" without a
   money amount; crossing the same spot/defeating a reset actor cannot reward it
   twice. Complete Mission01 to see earnings, then Replay for a fresh run.
4. Fire short overhead bursts at exposed Larpers, including after ducking, then
   stop to observe recovery. Compare genuinely protected/distant enemies. Watch
   hit and material-impact fades, normal shooting, cover, spacing and movement.
5. Verify readability/control feel and sustained frame rate on a real Android
   device. Automated Editor results do not establish hardware performance or
   human gameplay feel; those checks remain unverified.

Final Sprint023 verification (2026-10-20): 51/51 Edit Mode and 230/230 Play Mode
tests passed, 281 total (previous 273 retained plus eight focused cases).
The full Play Mode request exceeded the MCP client's 300-second response window;
test_status subsequently confirmed completed, zero failures, duration 344.55s.
Raw results, focused/UI/mission checks and scene audit: TestResults/Sprint023.json.
Actual running Warehouse_Testbed smoke restored 50 HP for approximately 50 tape,
then E replenished 50 tape and 72 reserve rounds, depleting the crate. The held
Android-style UI button was exercised through real pointer events and its HUD
appearance visually inspected in Game View. These controlled Editor checks do
not establish human combat balance or physical Android performance.

Final saved scene audit: Warehouse_Testbed has seven Larpers, seven wallet drop
bindings, one PlayerSupplies and two crates; Mission01 has ten Larpers, ten drop
bindings, one PlayerSupplies and two crates. Both have zero missing scripts and
zero dangling serialized object references. Both retain heal 25 HP/s, tape 200/200,
damage interruption .8s and suppression 49/16/48. Unity is ready, compilation
successful, current Console ground truth zero errors/warnings. Warehouse_Testbed
is saved, open and outside Play Mode. Source/UI/log whitespace checks pass;
Unity-serialized empty m_Name fields retain the serializer's trailing spaces.
Earlier modifications remain intact; index is empty, nothing committed or pushed.

## Sprint 022.5 — Mobile camera, controls and ladder-jump polish (2026-10-20)

Implemented after Sprint023; its healing, supplies, wallet and suppression work
is preserved. Scope is camera framing, touch controls and deliberate ladder jumps.

Root cause: Warehouse/Mission UXML had no dedicated touch jump control. The
joystick's upward gesture intentionally refuses jumps while climbing, so there
was no reliable explicit touch action to dismount and jump while holding climb.
Existing traversal already restores gravity, enables the motor and blocks ladder
reattachment for .3s plus a climb-release guard. Those protections are retained.

- WarehouseCameraFollow adds Inspector gameplayZoomMultiplier=1.10. The existing
  framing is divided by 1.10 after integer baseline selection, so rounding cannot
  erase the change. Base authored orthographicSize stays 7.2; existing Android
  character scale 1.125 remains. At a 720px viewport height the existing integer
  baseline is 5.625: PC becomes 5.113636, Android 5.0 becomes 4.545455. Follow
  smoothing, bounds, vertical offset and target interpolation are unchanged.
- WarehouseTouchControls adds Inspector joystickScale=1.15. Both pad dimensions
  and interactive bounds grow 187.5 to 215.625 UI units; knobs 77.5 to 89.125.
  Normalized deflection, fire thresholds, sensitivity, movement speed, existing
  safe-area insets and independent pointer captures are preserved. Nearby action
  columns move only enough to keep a gap; supply button narrows to the existing
  action-column width. Extremely narrow safe areas constrain pad size to fit.
- Both existing HUDs gain a JUMP action with independent pointer capture and one
  request per pointer-down. Up/cancel/lost-capture/input reset clear ownership.
  PlayerActions consumes one explicit press after current directional input,
  merging simultaneous button/keyboard/up-gesture requests into one jump. Holding
  the joystick does not need to stop; holding JUMP cannot auto-double-jump.
- Ladder jumps reuse motor.JumpSpeed (12, formerly traversal-only 10), not an
  increased global jump force. A deliberate attached ladder jump counts as jump
  one even after a previous flight; overlap alone never resets the counter.
  Existing second jump remains .85 of normal speed, with the usual two-jump cap
  and landing reset. No walking physics, animation, head bob or AI code changed.

Files changed in this sprint: Assets/Scripts/Camera/WarehouseCameraFollow.cs;
Assets/Scripts/Input/WarehouseTouchControls.cs, PlayerActions.cs;
Assets/Scripts/Traversal/PlayerTraversal2D.cs; Assets/Scripts/Prototype/PlayerMotor2D.cs
(normal-speed accessor and deliberate traversal jump budget only);
Assets/UI/WarehouseControls.uxml, MissionControls.uxml, WarehouseControls.uss;
Assets/Prefabs/UI/WarehouseControls.prefab; Assets/Scenes/Warehouse_Testbed.unity,
Warehouse_Mission01.unity (Editor-authored settings);
new Assets/Tests/PlayMode/Sprint0225Tests.cs and Unity-generated meta;
existing Assets/Tests/PlayMode/Sprint020Tests.cs and
Assets/Tests/EditMode/VisualAssetTests.cs (new size/zoom contracts, old checks kept);
DEVELOPMENT_LOG.md and TestResults/Sprint0225.json.

Focused Editor tests cover up/down/left/right ladder jumps via actual UI events,
one remaining air jump, held-button non-repeat, third-jump rejection, held climb
lockout, simultaneous gesture deduplication, landing reset, independent three-touch
move/aim-fire/jump, both UXML layouts at four aspect ratios and 10% zoom with
Android framing and bounds. This is controlled Unity Editor verification, not
Android-device playtesting. Final regression/audit results are recorded below.

Android manual acceptance still required: on a real device hold MOVE up/down/
left/right on a ladder and tap JUMP once; confirm immediate launch, directional
control, no reattach and one further airborne jump. Repeat with aim/fire held.
Check notches, narrow screens, HUD readability, smooth follow, cover transitions
and ordinary movement/head bob during an extended firefight. No physical Android
performance or subjective control feel is claimed by automated Editor tests.

Verification follow-up: the enlarged-stick tests now express outer firing
positions as normalized radius fractions, preserving the original inner/outer
zone checks. Traversal ownership coverage now expects the requested JUMP control.
The legacy typed-spawn fixture now binds its own small offscreen camera rather
than relying on a Camera.main left by another test. These are test-only updates
in CombatIntegrationTests.cs, MovementCompletionTests.cs and
TraversalRegressionTests.cs; runtime spawn/AI code is unchanged.
The safe-area action inset is bounded as well as the joystick diameter, avoiding
column overlap when a cutout leaves only 648 logical units of usable width.
Eight focused tests pass, including both HUD assets at that narrow width.
CombatIntegrationTests 8/8, MovementCompletionTests 11/11,
TraversalRegressionTests 12/12 and Sprint014MissionTests 9/9 pass in focused runs.
The first full run exposed three stale touch contracts plus a non-reproducing
mission-entry failure; mission entry passed both alone and with all mission cases.
Final full-suite verdict, live visual inspection and saved-reference audit follow.

Final verdict: 51/51 Edit Mode and 238/238 Play Mode passed, 289 total; all prior
281 cases retained plus eight new cases. Full Play Mode took 350.86s and exceeded
the MCP client's 300s response limit; test_status confirmed completed with zero
failures. TestResults/Sprint0225.json records final raw results and the initial
failures/focused follow-up. No unresolved automated failure remains.

Actual Warehouse_Testbed live check: confirmed advancing frames, then a captured
touch JUMP press while holding upward input detached from the west ladder.
Frames 2161 to 2165 recorded jump budget one, upward velocity exactly 12 and
restored gravity 3 with move input still (0,1). The normal Game View screenshot
was inspected: enlarged sticks, separated JUMP/fire/reload/chute columns and
health/ammo/tape HUD remain readable. All runtime preview changes were discarded.
An initial smoke attempt overlapped deferred test-runner cleanup; the fresh
standalone Play session supplied the successful evidence. One isolated eval
snippet required correcting its Editor update delegate type; project compilation
remained successful. Neither issue changed project source or saved scene data.

Saved-reference audit: Testbed seven Larpers/two crates; Mission01 ten Larpers/two
crates. Both have zero missing scripts or dangling references, player at the
original (-24,.85), zoom 1.10, Android scale 1.125, preview disabled and joystick
scale 1.15. Current Console ground truth has zero errors/warnings and no compile
failure. Warehouse_Testbed is saved, open and outside Play Mode. Physical Android
touch feel, cutout readability and device performance remain pending acceptance.
Nothing staged, committed or pushed; all earlier sprint work is preserved.

## Sprint 024 — Vertical-slice progression, equipment and specialists (2026-10-21)

Status: Implemented by Codex — awaiting playtest verification.
Milestones A–E implemented; F automated integration, visual audit and reporting
recorded below. Physical Android/Windows build playtesting is still required.

A — Progression
- MainMenu is the first enabled build scene. It provides Play, Settings and a
  desktop-only Quit action. Level selection registers only the implemented
  Warehouse_Demo mission, with title, preview icon, completion and prerequisite
  lock support. Choosing a level must enter preparation; buying is optional.
- The menu and shop use the existing UI Toolkit panel/input setup, scrolling
  layouts, focusable buttons and screen safe-area insets. Settings expose the
  existing master audio volume and touch-joystick scale, plus control reminders.
- CampaignMissionBridge connects the existing mission/result UI to return/retry
  navigation and a pause overlay (Escape or PAUSE), suspending player combat and
  input while paused and restoring time on resume/exit. The original result
  statistics and wallet earnings display remain in use.
- A GUID per attempt deduplicates settlement. Completion banks actual earnings
  and records completion once. Retry/exit captures stock and settles earned money
  without granting completion. Interrupted attempts capture stock/pending earnings
  on pause/quit; next startup settles that saved pending amount once, without
  pretending the interrupted mission was completed. Mid-mission position/checkpoint
  resume is not implemented.

B — Equipment and persistence
- GameCatalog is the central Inspector asset for real levels, supported weapons,
  prices, quantities and capacities. Starter carbine/baton remain available with
  zero starting currency; the first shop can always be skipped.
- Four shop categories: weapons, compatible ammunition, body armor, duct tape.
  Owned weapons can be assigned to primary/secondary/melee; the same firearm
  cannot be duplicated across two slots. Unowned items cannot be equipped.
  Full, owned, incompatible or unaffordable purchases are disabled. Purchase
  callbacks have a .25s debounce. Failed save writes roll back a purchase/equip.
- Prices: backup pistol $35, existing automatic Buzzbox $60; carbine 72 rounds
  $12, pistol 48 rounds $8, Buzzbox 72 rounds $15; armor 60 condition $30;
  tape 50 HP worth $10. Ammo cap 432 per weapon, tape cap 200, armor cap 60.
  Prices/quantities/caps are in Assets/Data/Progression/DemoCatalog.asset.
- BodyArmor absorbs 65% of appropriate body damage, bounded by remaining
  condition; head impacts and neutral hazards bypass it. It never replaces HP.
  Damage validation/friendly-fire defaults remain in Health. Armor condition
  appears alongside the existing HP/ammo/tape readout.
- Campaign loadouts apply after legacy mission reset but before the existing
  checkpoint snapshot. Stock comes from the profile rather than a free retry
  refill. Tape uses a small restore API on the existing PlayerHealing component;
  its actual healing, resource accounting and interruption mechanics are reused.
- GameProgress stores currency, ownership, equipment slots, ammunition, tape,
  armor, completed levels, settings and bounded settlement IDs. ProgressStore uses
  akumu2_progress_v1.json under Application.persistentDataPath with a temporary
  write and previous-file .bak. Missing data creates starters; unknown/corrupt or
  unsupported-version files are preserved and treated read-only with a warning.
  Existing unrelated saves/PlayerPrefs are not migrated, deleted or overwritten.

C — Samurai
- Separate Samurai prefab/archetype/personality/skin; 70 HP, existing motor
  moveInput 1.0 (versus the slower Larper), 25 melee damage, 1.65 reach, .45s
  windup, 1.1s recovery, .18s visual sword swing. Damage interrupts preparation.
- Reuses EnemyCombat perception, visibility/camera permission, suppression,
  morale and spacing, plus LarperBrain obstacle jumping and authored route support.
  Its adapter replaces only the final specialist action decision. Ordinary Larpers
  take the same prior path. A committed strike does not retarget; the existing
  AttackExecutor raycast handles actual range, intervening bodies and solid walls.
- Existing modular sprites/limbs are reused with a red skin/headband and a narrow
  blade accessory. Existing speech gates/two-bubble cap handle overconfident lines.
  No player rig or crouch pose is redesigned.

D — Czech fireworks expert
- Separate prefab/archetype/personality/skin; 60 HP, moveInput .55, preferred
  range 7, .85s visible preparation, 3.8s recovery, 20% large-miss chance.
  Warning destinations lock before launch; no homing. Damage/camera/visibility
  interruptions cancel preparation.
- Two projectiles are prewarmed per expert. A colored ring warns of impact;
  a visible fictional rocket follows a short arc (.95s flight, 2.6 arc height).
  Terrain collision updates the warning and delays the blast .25s. Radius 2.2,
  damage up to 24 with distance falloff, impact impulse up to 4; the brief ring
  fades over .35s. No new global projectile manager, unbounded particles or
  per-frame enemy search is used.
- A 25% per-attack opt-in allows nearby allies to be damaged. Ordinary friendly
  fire remains rejected. Blast victims are deduplicated; solid LOS is required.
  Surviving allies receive existing temporary fear/crouch influence and dialogue;
  ordinary defeat witnessing handles casualties. No recursive explosion chain.
- Teal clothing, a yellow hardhat, orange backpack and purple launcher accessory
  distinguish the expert while retaining the existing modular pixel sprites.
  Czech dialogue uses the existing speech system and display limits.

Shared impact handling
- Weak hits retain the existing small flinch. The optional demo-only
  HitReaction2D component adds bounded visual stumble at impulse 2.2+ and a 15%
  knockdown opportunity at impulse 3.5+, with .65s recovery and a 3s cooldown.
  Climbing/dead actors reject it. The existing visual driver owns the pose;
  the existing motor decelerates while input/combat are briefly suspended.
  No competing root velocity, teleport or nonlethal physics-ragdoll controller
  is introduced. Death still uses the established ragdoll path.

E — Warehouse encounter design
- Warehouse_Demo is a copy of Warehouse_Mission01, preserving the original
  Mission01/Testbed authoring and regression targets. Ten intended occupants
  remain: the opening guard stays a basic Larper; one ground-floor storage
  occupant becomes the Samurai; the later east-mezzanine occupant becomes the
  fireworks expert. Supporting Larpers and existing encounter gates stay intact.
- Three additional solid pieces: a 1.6x.35 gap step at (12,2), and two low
  cover boxes at (7.8,.45) and (18.2,.45). The step gives another double-jump route
  between the existing ground gap and elevated spaces. Original ladders, upper
  steps, boxes, barriers, alternate routes and cover arrangements are retained.
- A floor marking and office sign stage the future Officer area by the east
  exit. There is no Officer actor, boss health inflation or placeholder boss AI.
- Natural spawning, persistence, camera-aware regular firing, reservations,
  suppression, wallet drops and speech remain the existing systems.

F — Automated QA before full regression
- 54/54 Edit Mode tests pass (51 retained plus three new progress/scene checks).
- 12 focused Play Mode tests pass: money/ownership/debounce, save rollback,
  missing/future data, interrupted-run settlement, finite stock carry/retry,
  armor/body/head/hazard rules, real keyboard/mouse/touch UI navigation through
  completion and return, pause/resume, reachable alternate step, committed melee
  with wall rejection, bounded heavy-hit recovery, projectile telegraph/pool/
  destination, deduplicated and LOS-protected blast/friendly fire.
- The initial menu scene-authoring request timed out and left its panel reference
  unset. The real UI test caught this (unattached/NaN layout); the scene now
  explicitly references WarehousePanel, and an Edit Mode asset assertion protects
  that binding. Buttons are exercised after layout resolves, including the
  newly displayed results return button.
- Full regression results, final saved-reference audit and visual inspection
  evidence are appended below. Editor automation does not verify physical Android
  touch feel, hardware frame rate, built-player suspend/restart or human balancing.

Sprint024 production files
- New Assets/Scripts/Progression/{GameCatalog,ProgressStore,GameSession,
  GameFlowController,CampaignMissionBridge}.cs.
- New Assets/Scripts/Combat/{BodyArmor,HitReaction2D}.cs and
  Assets/Scripts/Enemies/{SamuraiCombat,FireworksCombat,FireworkProjectile}.cs.
- Modified Health.cs (optional armor/explicit special friendly fire),
  PlayerHealing.cs (finite-stock restore API), WarehouseTouchControls.cs (optional
  armor readout/configurable existing stick scale), WarehouseMission.cs (two
  replay integration events), EnemyCombat.cs (cached specialist action adapters),
  LarperAwareness.cs (friendly-hit reaction through existing speech), and
  CharacterVisualDriver.cs (optional specialist/impact presentation).
- New Assets/UI/GameFlow.uxml/.uss; MainMenu.unity and Warehouse_Demo.unity.
  EditorBuildSettings now starts MainMenu then Demo, retaining the old entries.
- New DemoCatalog asset; Samurai/FireworksExpert archetypes, personalities and
  skins; SamuraiSword/FireworkLauncher definitions; Samurai/FireworksExpert/
  CartoonFirework prefabs; shared FireworkWarning material; generated meta files.
- New Sprint024ProgressTests.cs, Sprint024FlowTests.cs, Sprint024EnemyTests.cs;
  DEVELOPMENT_LOG.md and TestResults/Sprint024.json. Earlier dirty files and
  existing regression cases are preserved; no staging, commit or push.

Android/Windows manual acceptance
1. Open MainMenu (or launch the build). Test Play, level status, preparation and
   proceeding with no purchase. Exercise keyboard focus, mouse and physical touch.
2. Complete or exit a run with collected wallets. Verify one balance award,
   results earnings, return/retry, ownership and completion after restarting.
3. Buy/equip a weapon and compatible ammo, armor and depleted tape. Check caps,
   insufficient funds, rapid taps, disabled full purchases and stock after scene
   transitions/retry. Test suspend/resume and low-storage/save-error feedback.
4. Take body/head hits with armor, deplete it, heal in cover, and exercise weak
   flinches/heavy recovery without movement twitching or stuck input.
5. Play the opening Larpers, middle Samurai and later fireworks expert. Dodge
   committed strikes/marked rockets, test wall protection, rare ally accidents,
   ordinary suppression/dialogue and the existing two-bubble cap.
6. Use the gap step, ladders and double jumps while aiming/firing. Test notches,
   narrow screens and scrollable shop controls. Measure sustained frame rate on
   the target Android device. New skins/accessories reuse prototype art; final
   independent publication art remains the previously documented later task.

Final-integration follow-up: the first full Play Mode run passed 249/250 and
reported one legacy low-cover shot assertion. That case passed alone and its
entire Sprint012CombatTests fixture then passed 13/13 without a gameplay change.
The new hit-reaction completion edge was corrected: a mission result cancels the
reaction without restoring controls, while checkpoint recovery explicitly clears
it and restores ownership. Existing focused cases now exercise both transitions.
Blast falloff measures true distance before choosing a fallback zero-distance
impulse direction; exact-centre damage is asserted as 24, once per actor.
All 12 focused cases and 54 Edit Mode cases pass after these integration checks.
The repeat full-suite verdict and live visual/reference audit follow.

The second full run passed 249/250, this time reporting the legacy visual crouch
pose after a fixed .15s sleep; the earlier cover case passed. Both fixtures used
sleep durations close to the existing .14s visual transition and assumed an
unfocused Editor had processed enough input/physics/presentation updates.
Test-only bounded state waits now observe the actual posture/muzzle/geometry
transition before retaining every original assertion. VisualIntegrationTests
passes 8/8 and Sprint012CombatTests 13/13. No movement or animation tuning was
changed to satisfy these cases. A missing test namespace import was fixed;
the interrupted test job was cancelled and compilation rechecked as up to date
before restarting. These intermediate failures are retained in the evidence.

Sprint024 resumed verification — 2026-10-21
- Inspected existing sources, assets, scenes and prior milestone evidence before
  proceeding. A–E are implemented; resumed F only. No production source, prefab,
  scene or gameplay tuning was changed during this resumed verification pass.
- Status: **Implemented by Codex — awaiting playtest verification**.
  Milestone F does not have a clean full-suite sign-off yet.
- Compilation is up to date without compiler errors. All 54 Edit Mode tests pass.
  Latest full Play Mode run: 247/250, 402.46 seconds; all 12 Sprint024 cases pass.
  The MCP call timed out after 300 seconds; the final result was obtained from
  test_status, rather than interpreting the timeout as success.
- Full-run failures: Sprint015Tests.FinalCameraMovementAndMissingCameraCannotExposeANewSpawn
  (final activation false), Sprint015Tests.HealingCapsRepeatsHonorsCooldownAndCannotResurrect
  (52.94 health instead of 100), and VisualIntegrationTests.WalkingFacingAimRecoilAndCrouchRemainIndependentOfGameplay
  (initial Fall instead of Idle). Subsequent complete fixture reruns passed
  Sprint015Tests 12/12 and VisualIntegrationTests 8/8. Different legacy cases
  failed in preceding full runs. This suggests timing/order sensitivity, but its
  precise cause remains unresolved; do not describe the full regression suite as
  passing. No further production changes or weakened assertions were made.
- Saved-reference audit: MainMenu, Warehouse_Demo, Warehouse_Mission01 and
  Warehouse_Testbed contain zero missing scripts and zero dangling serialized
  object references. Actor counts are respectively 0/10/10/7. Player, Larper,
  Samurai, FireworksExpert and CartoonFirework prefabs also have no missing
  scripts/dangling references. Original Player/Larper prefabs have no optional
  Sprint024 armor/hit-reaction component added.
- Actual live Editor smoke: frames advanced; inspected MainMenu, weapon and armor
  shop tabs, scrollable start/back actions, Demo HUD and pause overlay at 1280x720.
  UI navigation reached Demo without purchasing. Pause disabled player actions
  and set timeScale to zero; resume restored simulation. A dedicated GUID-named
  temporary QA save was used after startup, without deliberately changing the
  user's real campaign progress. These are automated Editor checks, not human
  gameplay or physical touchscreen verification.
- One active Demo sample: 35 draw calls, 4,476 vertices, CPU main thread 1.87 ms,
  GPU 2.19 ms, total CPU frame 17.23 ms with temporary 60 FPS test cap. Editor
  allocated memory was about 1.81 GB after extensive tests. This is neither a
  before/after performance comparison nor evidence of Android frame rate.
- Evidence: TestResults/Sprint024.json records complete latest Edit/Play results,
  focused reruns, earlier available failure results, reference and smoke audits.
  Current Editor console ground truth is zero errors/warnings; historical MCP
  buffer still contains the fixed earlier import and interrupted-runner errors.
- Remaining acceptance: investigate full-suite intermittency; human combat and
  movement playtest; Android safe areas, physical touch, sustained performance,
  and built-player persistence/suspend testing. Samurai uses the existing local
  obstacle/jump routing rather than a new global pathfinding system. Officer is
  staged only, not playable. Prototype modular art still needs the previously
  planned independently created publication replacement.
- Final state: Warehouse_Testbed saved/open, Play Mode stopped. Restored the
  temporary Application.targetFrameRate to its prior -1 after leaving Play Mode.
  MainMenu remains first in build settings; open it to test the new campaign.
  Existing unrelated dirty changes preserved. Nothing staged, committed or pushed.

SPRINT 024.1 — Shop access and warehouse exit integration hotfix (2026-10-21)
Status: **Implemented by Codex — awaiting actual player verification.**

Diagnosis
- The global on-disk build scene list was already MainMenu/Demo first, but the
  active in-memory Android profile had Override Global Scene List enabled with
  only Warehouse_Testbed. Normal profile launches therefore bypassed the entire
  campaign. Editor Play from the open Testbed also bypassed it independently.
- Testbed's Checkpoint_Exit at (23,1) contains a CheckpointMarker, whose purpose
  is setting a respawn position. Testbed has no WarehouseMission, MissionTrigger
  or campaign results bridge. Its exit cannot complete the campaign; this was a
  wrong-scene integration problem, not evidence of a faulty real mission collider.
- The actual Demo Mission_Exit is enabled, a trigger at (25,1), size 1.5x3, layer
  2, correctly bound to the mission. Player layer 2 collisions are enabled.
  Completion legitimately requires both encounter gates and recovering records.
  Previously locked-exit feedback was brief and vague; Stay ignored locked exit
  stages, including retry after an early collider-edge callback.

Focused fixes / files
- Assets/Settings/Build Profiles/Android™.asset: persist global-scene inheritance
  and synchronize the cached list with the campaign scenes, replacing the stale
  Testbed-only list. Global EditorBuildSettings already has the correct entry.
- Assets/Editor/CampaignEntry.cs (+ generated meta): AKUMU 2 > Play Campaign
  (Main Menu) opens MainMenu before entering Play Mode. Direct Testbed play stays
  available for development; there is no global Play-mode scene override.
- Assets/UI/WarehouseControls.uxml: explicit development-only/no-shop/no-results
  warning, respawn-only exit explanation and campaign entry instructions.
- WarehouseMission.cs: persistent, stage-specific locked-exit feedback while
  physically overlapping; replay clears cached contact. Existing completion,
  results and reward guards retained. No objective gate bypass introduced.
- MissionTrigger.cs: allow Stay overlap retries at locked exits too; notification
  is emitted only on first cached contact, avoiding per-step UI event spam.
- Sprint024ProgressTests.cs: validate effective startup scenes and saved build
  profile inheritance, rather than relying only on global scene settings.
- Sprint024FlowTests.cs: two new repeated full-loop tests and scroll-aware button
  input. All prior tests retained. No scene/prefab duplication or new exit.

Automated verification
- Compilation passes; 55/55 Edit Mode tests, 14/14 Sprint024 Play Mode tests,
  9/9 Sprint014MissionTests and 8/8 Sprint023Tests pass (31 relevant Play cases).
- Each new loop runs MainMenu > Levels > Shop > Demo > actual physical Exit >
  Results > Levels > Shop > second Demo > physical Exit > Results > Levels.
  One variant makes no purchases. The other buys/equips a pistol ($35), buys
  reserve ammunition ($8) and armor ($30) through UI. Pistol ownership, selected
  slot, 48 reserve rounds, armor condition and wallet persist through both runs
  and actual disk save reload. Temporary QA profiles/funds isolate test data.
- PlayerActions movement physically drives the real Rigidbody2D across the exit;
  neither new test directly calls EnterZone for the exit. An actual enemy
  rigidbody cannot activate it. Locked feedback remains visible beyond four
  seconds. Results, one earnings award, continued-contact deduplication, control
  shutdown, return navigation and second mission start are asserted.
- Intermediate objectives/defeats are controlled setup; records use the INTERACT
  button. This verifies scene/UI integration and exit physics, not natural combat
  or whole-level navigation. The older menu test invoked EnterZone directly for
  exit and could not prove collider activation or Testbed/profile entry routing.
- Initial new runs exposed a test driver clicking clipped shop buttons without
  scrolling. The driver now scrolls buttons into view before input; final runs
  pass. This is recorded alongside final results in TestResults/Sprint0241.json;
  TestResults/Sprint024.json links that supplement and retains earlier failures.
- Live Editor smoke executed Play Campaign from open Testbed and observed a
  rendered MainMenu with session MainMenu state. Final compiler/console ground
  truth: no errors or warnings. No device build or physical touch test performed.

Remaining verification / next playtest
- Use the already-open MainMenu, or AKUMU 2 > Play Campaign (Main Menu).
  Play > Warehouse preparation must show the shop before Start Mission. Fresh
  profiles with $0 can start without buying; collect wallets before later buys.
- Follow the objective HUD: guard/storage, mezzanine checkpoint, processing,
  east mezzanine records (E / INTERACT), then east ground-floor exit. Early exit
  visits should explain the remaining objective. Confirm Results earnings, return
  to Levels, buy/equip, and launch another mission without restarting.
- Human Windows/Android gameplay, natural traversal, touch/readability and built
  player restart persistence remain unverified. The previous full Play run was
  247/250 with intermittent Sprint015 spawn/healing and VisualIntegration idle
  failures that passed isolated reruns. This hotfix did not rerun or claim a green
  full legacy suite and did not change those systems.
- MainMenu is saved/open, Play Mode stopped, prior frame cap remains -1. No
  unrelated mechanics changed; earlier dirty work preserved. No staging/commit/push.

Playtest starting funds — 2026-10-21
- User requested $1000 at the start. DemoCatalog.startingMoney now configures
  fresh campaign balances at $1000; existing balances are not reset on launch.
- Granted the current local campaign a one-time $1000 credit (0 -> 1000), using
  the existing atomic save/backup path and preserving equipment/progression.
- Existing flow test fixtures explicitly retain their controlled $0 baseline.
  Compilation passes and Sprint024FlowTests passes 10/10. MainMenu reopened,
  Play Mode stopped; saved wallet verified at $1000. No staging/commit/push.

SPRINT 024.2 — Thrown fireworks, ladder clearance and readable testing encounters
Date: 2026-10-21
Status: **Implemented by Codex — awaiting playtest verification.**

Fireworks rework
- Replaced the specialist's active launcher silhouette with an oversized colorful
  pixel bundle. Removed LaunchTube; retained the legacy item asset GUID/path for
  prefab compatibility, renamed its display text and replaced its equipped sprite.
  The adapter throws from the actual hand, without calling firearm Attack/Fired.
- Existing rigid modular arm/weapon mount poses now show carrying, winding up,
  release and recovery. Held packet follows the final posed hand in LateUpdate.
  Anticipated failure shows surprise .18s before exploding, including fatal cases.
- Added real Rigidbody2D ballistic motion, continuous collision and a .4-world-unit
  circle fitting the larger bundle. Terrain collisions stop it visibly; an
  independent 1.1s landing delay allows repositioning. No instantaneous impact
  blast. Radius 2.2, damage 24, impulse 4, .95s nominal flight, 2.6 arc height,
  .85s preparation and 3.8s recovery remain Inspector-configurable.
- A single randomized 6% roll per prepared throw schedules a held failure at 65%
  of windup. Pending throw is cleared before explosion; owner and nearby allies
  can be hurt, with existing hit reactions/morale/dialogue. A weakened specialist
  can defeat himself. Ordinary throws retain 25% ally-damage opt-in and 20% bad
  miss chance. General Health self-hit rejection is unchanged and tested.
- Existing wall shielding, falloff, victim deduplication and fixed query buffers
  remain. Two prewarmed projectiles per specialist each own ten reusable colored
  spark/smoke renderers, cleaned after .35s; no per-throw GameObject allocation.
  Reused speech cooldowns/two-bubble cap with contextual Czech preparation and
  malfunction lines. No global suppression, fear or collapse rebalance.
- Optional pooled burstSound slot is available, but no suitable game AudioClip
  existed to assign. No new audio framework or camera shake was introduced.

Ladder root cause and repair
- Warehouse_Demo's AlternateGapStep was centred at (12,2), directly crossing
  Ladder_Basement_Mezzanine. This Sprint024 platform was the actual obstruction.
- Moved that one step to x=10.2, keeping y=2, size 1.6x.35 and solid collision.
  The ladder shaft is clear; surrounding platforms remain solid. Traversal,
  motor, double-jump, camera and touch-controller code were not rewritten.
- Physical tests climb basement to mezzanine and auto-land, descend to basement,
  jump away through desktop/touch paths and retain one air jump. The relocated
  step remains reachable and solid in the existing double-jump integration test.

World readability and population
- Small olive supply crate with gold ammo symbols replaces shipping-crate art;
  bright SupplyBand is disabled. Existing finite ammo/tape pickup/interaction and
  EMPTY state are unchanged. Checkpoints use grounded flag posts with CHECKPOINT
  labels, amber/green activation cue and modest pulse. Exit has a larger cyan
  door frame, pulsing header and EXIT / MISSION RESULTS sign. Sprint024.1 locks,
  persistent objective feedback, result navigation and reward deduplication remain.
- As confirmed by the user, Demo has three opening Larpers and five additions in
  total. Added: Arrival (-19.2,.85), Arrival atop existing low cover (-14,1.75),
  Storage (-.2,.85), Processing near cover (16.2,6.25), Processing/exit (25,.85).
  Groups are now 3/5/7; total 15 = 13 basic Larpers + Samurai + fireworks expert.
  Entrance HUD now shows its actual defeated/total count.
- Residents are authored before the first rendered frame with existing point,
  encounter, reset, spacing, perception and camera-firing behavior. No runtime
  spawn code changed. Testbed's original three opening enemies/seven residents
  remain; its exit still correctly identifies development-only respawn behavior.

Files / assets
- Modified FireworksCombat.cs, FireworkProjectile.cs, LarperAwareness.cs,
  CharacterVisualDriver.cs, CutoutRig.cs, CheckpointMarker.cs and WarehouseMission.cs.
  New WorldMarkerCue.cs reads existing progression state for presentation only.
- New Editor/Sprint0242Setup.cs authors repeatable assets/visuals/population.
  New Art/Sprint0242/{QuestionableBundle,AmmoSupplyCrate,CheckpointFlag}.asset
  contain point-filtered editor-authored pixel sprites, not recovered artwork.
- Updated FireworksExpert, CartoonFirework and SupplyCrate prefabs; FireworksExpert
  skin/personality, legacy FireworkLauncher item presentation; Warehouse_Demo
  geometry/residents/markers and Mission01/Testbed marker presentation.
- New Sprint0242RulesTests.cs, Sprint0242Tests.cs and Sprint0242SceneTests.cs.
  Existing Sprint024 tests retain assertions, updated for the intentional 15
  actors/new step location, deterministic successful throw and longer fuse.
  TestResults/Sprint0242.json and five Sprint0242_*.png files hold QA evidence.

Verification and limits
- 57/57 Edit Mode tests passed. Relevant Play Mode fixtures passed 74/74:
  Sprint024/024.2 21, mobile Sprint0225 8, Sprint0224 9, movement stability 1,
  opening spacing 1, Sprint023 pickups/healing 8, mission 9, second-arm 2,
  morale Sprint019 6 and spawning Sprint0221 9. Final affected 21-case fixture
  passed again after fitting the larger collider; final Edit run passed after
  grounding the flag visuals. New coverage is 2 Edit + 7 Play cases.
- New tests cover actual arc/collision/visible fuse/exactly-once damage, walls,
  held failure/self/ally damage, recovery/reuse, real ladder passage and desktop/
  touch double jump, supported separated residents and multiple-target real
  hitscan suppression/decay. Test-only probabilities deliberately control the
  defensive-response branches; production suppression values are untouched.
- Initial fixture mistakes were corrected: shots faster than the existing .2s
  carbine interval, frozen actors above grounded height, and premature ladder
  side exit instead of waiting for auto landing. No gameplay tuning was used to
  force these assertions. The repeat authoring helper's destroyed-Transform
  references were fixed; one 5s Pipeline timeout was handled through one Editor
  update and completion confirmed. Final scenes are saved, references valid.
- Actual controlled Editor rendering inspected held, landed and burst stages,
  grounded flag vs supply crate, and prominent exit with a real locked-contact
  message. Visual inspection caught the old narrow projectile collider causing
  sinking; the enlarged collider visibly fixes it. Fireworks captures precede
  final flag grounding; supply/exit captures show final marker placement.
- Saved-reference audit: zero missing scripts/dangling refs across MainMenu,
  Demo (15 actors), Mission01 (10) and Testbed (7). No remaining LaunchTube in
  those scenes. Compiler and current Editor console ground truth are clean.
- One active Editor sample: 51 draw calls, 5,020 vertices, main thread 2.35ms,
  GPU 1.86ms, total CPU frame 4.41ms. Editor allocated memory ~2.12GB after tests.
  This is not an Android benchmark or a reliable baseline comparison.
- Full legacy suite was not rerun; earlier Sprint024 full-run 247/250 intermittent
  Sprint015 spawn/healing and VisualIntegration idle failures remain unresolved.
  Isolated fixture successes do not establish full-suite reliability.

Android / human acceptance still required
1. Build/launch Landscape Left: MainMenu > Warehouse > Shop > Demo. Confirm saved
   equipment and $1000 fresh-profile funds, then return/retry without duplicate money.
2. Watch the specialist carry, wind up, throw, land and warn before exploding.
   Dodge the packet; check wall protection and occasional ally accidents. Held
   failures are intentionally rare: automated forced cases verify the branch,
   not its subjective natural frequency. Check Czech bubbles/readability.
3. Climb the x=12 basement ladder through y=2 to the mezzanine and back, enter/
   exit, jump away with touch controls and use the remaining air jump. Land on
   the relocated step and ordinary platforms; check responsiveness near boxes.
4. Distinguish the olive ammo/tape crate, checkpoint flags and cyan EXIT doorway.
   Activate a checkpoint, die/recover and check green state. Visit exit early
   for objective feedback, then complete requirements and verify Results/earnings.
5. Observe exposed/covered/mixed Larpers under real near-miss bursts and ally
   defeats. Check varied recovery, spacing, persistence, firing visibility and
   rare panic; profile sustained frame rate/temperature on the target phone.

No human gameplay or APK/device test claimed. Final MainMenu saved/open, Play
Mode stopped; Landscape Left, campaign scene list and $1000 starting funds
preserved. Existing unrelated dirty work retained. No staging, commit or push.

## Sprint 024.3 — Samurai aggression, platform pursuit and modular art (2026-10-21)

**Implemented by Codex — awaiting playtest verification**

Root causes inspected: SamuraiCombat previously stopped for the entire 1.1-second
recovery and immediately stopped pursuing when visibility broke, despite the
caller retaining a legitimate last-seen position. Shared Larper pursuit could
hop a low wall but did not plan gap or elevated-platform landings. The Samurai
reused BucketLarper body parts and a ScrapCarbine, with rectangular Blade and
RedHeadband accessories. The band was 0.52 units wide on a roughly 0.375-unit
head, at local y=0.05 and sorting order 14, above FaceOverlay's order 13: it
covered the upper face/eyes and produced the reported blindfold impression.

Exact tuning (Inspector/definition configurable):

| Parameter | Before | After |
| --- | ---: | ---: |
| SamuraiSword damage | 25 | 40 |
| Strike wind-up | 0.45 s | 0.40 s |
| Post-strike recovery | 1.10 s | 0.70 s |
| Initial visual reaction delay | 0.30 s | 0.18 s |
| Shared re-engagement delay, Samurai override | 0.35 s | 0.20 s |

Attack distance/weapon range remain 1.65; weapon firing interval remains 1.10 s.
Motor speed 7, jump speed 12, gravity scale 3, single enemy jump, health 70,
suppression susceptibility/personality and hit-interruption delay are unchanged.
One committed strike remains the moveset; no heavier/extra attack was added.
Forty damage is serious against 100 unarmored HP without being a one-hit kill.

Gameplay implementation:

- Continue toward the supplied last-seen position while EnemyCombat's existing
  memory permits it. No access to a hidden target's current position is used.
  Existing perception, panic, suppression withdrawal, knockdown and camera gates
  retain authority. Active defensive ducking interrupts wind-up and movement.
- Commit aim when anticipation begins, face that direction through the swing,
  and retain wall-blocked melee rays and damage interruption. Arm and sword now
  move together: smooth 15-to-65-degree anticipation, 0.18-second committed
  65-to-minus-35-degree swing, then recovery toward the ready pose.
- Samurai-local platform planning samples at most every 0.30 s while grounded.
  Search is 6 units horizontally, with a maximum 6.5-unit descent. Actual motor
  jump velocity and gravity determine feasible rise/flight time; no added jump
  force, teleport, position assignment, air jump or gravity override is used.
- A fixed 32-collider terrain buffer yields at most four ranked landings. At
  most 20 swept body-box checks per candidate reject obstructed arcs; saturated
  buffers reject safely. Existing combat-position occupancy rejects allied
  landing conflicts. Launch requires the existing spacing permission.
- Flight uses the existing motor input and one jump request, commits the
  landing, and brakes after reaching/passing it without retargeting in midair.
  Successful landings impose 1.2 s between planned jumps. Failed attempts impose
  3 s and block the same launch origin until at least 0.8 units of progress.
  Flight has a finite timeout. One validated 0.25 s backstep at 70% input can
  create launch room under a step; it cannot repeatedly oscillate at a wall.
- When the remembered player is directly below, a bounded support/edge check
  can approach an existing ledge with suitable ground below, then validate the
  actual jump normally. This is local pursuit, not a global route graph.
  No new per-frame physics scan, allocation, enemy search or manager was added.

Visual/encounter implementation:

- Original imagegen artwork supplies a modular head, teal/gold wrap torso,
  sleeved arm, wrapped trouser/boot leg and dedicated katana. Existing body-part
  heights and shoulder/hip/grip pivots are retained. Point filtering, no mipmaps,
  one shared atlas and a non-readable runtime texture avoid a retained CPU copy.
- Eyes/eyebrows are visibly below the crimson forehead band, with navy hair and
  a topknot. Remove the old rectangular band/blade, carbine and inherited visible
  magazine from the Samurai presentation. No Larper/player redesign occurred.
- The new head embeds its eyes. Samurai's generic Larper face overlay is disabled;
  head tilt, flinch and pose reactions remain, but no separate expression sheet
  was generated. Ragdoll copies the source face-overlay visibility as well as the
  existing six parts; the same five limited joints and physics remain intact.
- Warehouse_Demo only: Storage_Spawn_1 and its existing Occupant_Samurai_Storage
  move from (3, 0.85) to (-0.5, 6.11), on the existing central mezzanine near
  the authored upper steps. Same resident, encounter binding and enemy counts:
  15 total, one Samurai; opening three Larpers unchanged. No terrain changes.

Files changed by this sprint (other pre-existing dirty work preserved):

- `Assets/Scripts/Enemies/SamuraiCombat.cs`
- `Assets/Scripts/Prototype/PlayerMotor2D.cs` — read-only MoveSpeed getter only
- `Assets/Scripts/Visuals/CutoutRig.cs`
- `Assets/Scripts/Visuals/CharacterVisualDriver.cs`
- `Assets/Scripts/Visuals/CartoonRagdoll.cs`
- `Assets/Prefabs/Enemies/Samurai.prefab`
- `Assets/Data/Items/SamuraiSword.asset`
- `Assets/Data/Visuals/SamuraiSkin.asset`
- `Assets/Scenes/Warehouse_Demo.unity`
- `Assets/Art/Sprint0243/` — source atlas and five Sprite assets, associated metas
- `Assets/Editor/Sprint0243Setup.cs` — repeatable asset setup/placement/preview
- `Assets/Tests/PlayMode/Sprint0243Tests.cs` — 15 focused cases, associated metas
- `TestResults/Sprint0243.json`, `TestResults/Sprint0243_RigPreview.png`
- `DEVELOPMENT_LOG.md`

Automated verification through the running Unity 6000.6.6f1 Editor:

- Edit Mode: **57/57 passed**.
- Focused Play Mode: **87/87 passed**, counted once per final fixture. Includes
  15 new Samurai cases; 4 existing specialist/hit tests; 7 Sprint 024.2 cases;
  movement stability 1; double-jump/suppression 9; mobile camera/touch/ladder 8;
  morale 6; second-arm integration 2; shared visuals/ragdolls 8; spawning 9;
  opening spacing 1; suppression/positioning 7; campaign/shop/exit flow 10.
- New cases exercise physical ascent, gaps, descent including a warehouse-height
  drop, actual warehouse step dimensions, impossible/blocked arcs, changing
  target positions, finite failed attempts, launch clearance, occupied landings,
  directly-below ledge approach, 40 damage, recovery/hit interruption, defensive
  ducking, hand attachment and new-skin ragdoll/joint continuity.
- Rig preview rendered from the actual prefab in idle, run, jump and anticipation
  poses and visually inspected. This is a pose preview, not a combat playtest.
- One combined `Sprint024` batch lost its MCP connection after a 300-second
  tool timeout. No result was recoverable after the connection returned in a
  new Editor session. The relevant fixtures were rerun separately as above;
  the interrupted batch is not counted as passed. Full legacy Play Mode assembly
  was not run; earlier documented full-suite/order-dependent issues are not
  claimed resolved. Detailed final results/error are in Sprint0243.json.
- Final inspection: Demo has 15 spawn points, one bound Samurai and zero missing
  scripts. Compilation clean; final Console has zero errors/warnings. MainMenu
  open, clean and outside Play Mode; Landscape Left preserved.

Manual verification still required:

1. Start MainMenu > Play > Warehouse, with/without armor. Let the Samurai approach;
   compare 40-damage hits, anticipate the wind-up, dodge behind him and interrupt
   with gunfire. Confirm recovery is punishable without removing his pressure.
2. Bait him from the central mezzanine through UpperStep01/02/03 to the roof,
   across gaps and down toward ground. Move during jumps; check readable launches,
   landings, partial occlusion, crowded platforms and obstacles. The planner is
   local and conservative; arbitrary long or obstructed routes can still be
   unreachable, and a new actor/obstacle can move into an already committed arc.
3. Inspect both facing directions at normal gameplay zoom: exposed eyes, rear arm,
   grip, anticipation/swing/recovery, jump/fall, hit flinch and defeat/ragdoll.
4. Check live Larper suppression/morale, fireworks, three opening Larpers, shop,
   armor/healing, checkpoints, ladders/double jump, cover, pause and results.
5. Profile a sustained encounter on the Android target and judge difficulty/art
   in actual play. No human combat playtest, APK/device test, or frame-rate
   benchmark is claimed; automated physics/flow tests cannot establish feel.

No staging, commit or push. No unrelated gameplay systems rewritten.

## Sprint 024.4 — Samurai articulation, grip and platform routes (2026-10-21)

**Implemented by Codex — awaiting playtest verification**

Inspected the live Editor, Samurai prefab, modular posing, firearm muzzle
constraints, six-body ragdoll, motor, authored ladders and Sprint 024.3 planner.
The arms/legs were single SpriteRenderers rotating as rigid appendages; the grip
was inferred from the entire arm length, and generic muzzle clipping could
stretch that arm. Navigation considered one local jump at a time, without an
airborne second impulse or a connected route through intermediate platforms.

Samurai design/art and combat tuning are preserved: damage 40; wind-up 0.40 s;
recovery 0.70 s; reaction 0.18 s; re-engagement 0.20 s. No hitbox, strike timing,
weapon range, movement speed, gravity or jump-force rebalance was made.

The optional SamuraiArticulatedRig extends the existing CutoutRig:

```
LeftArm / RightArm        upper arm at shoulder
  Elbow                  forearm at elbow
    WristHand            hand at wrist
      SwordGrip          right hand only
LeftLeg / RightLeg        thigh at hip
  Knee                   shin at knee
    AnkleFoot            foot at ankle
WeaponMount              follows the actual right-hand SwordGrip
```

Six new Sprite assets crop the existing Sprint 024.3 atlas; source pixels,
head/eyes/hair, torso, palette and sword are unchanged. Adjacent crops overlap
slightly at their shared pivots (0.01125 world units) to close rotating seams.
No new bitmap, animation framework, character redesign or live limb physics was
introduced. Larper/player/fireworks rigs do not opt into the extension.

Elbows clamp to 8..135 degrees, wrists -55..55, knees -120..-2 and ankles
-40..45. Cached joint angles blend over 0.09 s; running bends alternate knees,
airborne poses tuck legs, the second impulse adds a distinct 0.24 s tuck, and
landings add 0.24 s leg compression with visual-only foot/stance correction.
The attack varies arm extension and elbow flexion through preparation, swing
and recovery while retaining the existing combat timing.

A small analytic two-link carrying arm follows a palm grip. The sword follows
the filtered hand rather than its requested endpoint. Hand sorting covers the
hilt, mirrored orientation uses the actual hand axis, and muzzle clipping
re-solves the arm instead of stretching a full-limb sprite. Climbing also updates
the grip. CharacterVisualDriver is the sole live pose owner; shared torso/head
flinch and body hit reactions remain. Death transfers the current articulated
pose to 14 ragdoll bodies and 13 limited joints. Limb mass is split 50/32/18%
between the upper/lower/end segments, preserving total configured limb mass;
gravity, collision settings and shared velocity clamps apply to all bodies.
Generic ragdolls retain their original six bodies/five joints.

Navigation extends the existing bounded ballistic validation:

- A Samurai-local cache gathers up to 48 nearby terrain nodes and eight ladders
  from a 38-by-30-unit region (64-hit query buffer), refreshed once per second
  or after substantial movement. No per-frame full-scene scan or new manager.
- A nonnegative-cost connectivity search chooses intermediate platforms and can
  initially move away from the target. It distinguishes walk, single jump,
  double jump, ladder and unreachable transitions. Route estimates alone never
  authorize a launch: body sweeps, landing occupancy and actual motor timing do.
- Tactical sampling stays at 0.30 s. Single-jump horizontal limit remains 6;
  the configurable double-jump limit is 8.5, still capped by real flight time
  and speed. Maximum descent stays 6.5. Launch candidates include platform edges
  and nearby adjustments; normal hops prefer a better launch before consuming
  a second impulse merely to compensate for a poor one.
- Only the Samurai prefab enables the existing motor's airborne jump option.
  First impulse stays 12, second 10.2 (existing 0.85 multiplier), with a planned
  configurable 0.32 s second-impulse delay. Single arcs use 20 swept checks;
  double arcs use 32 checks over the two real ballistic phases. The motor owns
  velocity, gravity, grounding and the two-jump limit. No root snapping or extra
  airborne impulses; grounding resets the budget through existing motor logic.
- Flights retain committed destinations if the player moves. Failure records
  are bounded to 16 edges: ordinary failures penalize an edge for 5 s, hit
  interruptions 3 s, ladder failures 8 s. Two seconds without launch-position
  progress triggers replanning. Existing 1.2 s successful/3 s failed jump
  cooldowns and same-origin protection remain; an alternative edge can be tried
  after a failed transition. Backstep input now explicitly ends at its deadline.
- Existing AuthoredLadderRoute gains an opt-in intermediate-platform exit using
  PlayerTraversal2D's existing side landing. Standard Configure behavior remains
  unchanged. EnemyCombat still owns perception, suppression, panic and attack
  permission; authored ladder traversal retains exclusive motor ownership.
- Optional Draw Navigation on a selected Samurai displays the destination,
  committed launch/landing, jump count and failure reason. Drawing is Editor-only
  and disabled by default; there is no release gameplay overlay.

The current Warehouse_Demo encounter remains on the existing central mezzanine.
The authored upper steps provide both single- and double-jump route opportunities
(verified using their exact dimensions in a physical test). No scene, terrain,
spawn count, shop, progression, wallet, player control or Larper tuning change.

Exact files changed/added, plus Unity-generated metas for new files/folders:

- `Assets/Scripts/Visuals/SamuraiArticulatedRig.cs` (new)
- `Assets/Scripts/Enemies/SamuraiPlatformRoutes.cs` (new)
- `Assets/Scripts/Enemies/SamuraiCombat.cs`
- `Assets/Scripts/Enemies/AuthoredLadderRoute.cs`
- `Assets/Scripts/Prototype/PlayerMotor2D.cs` — read-only AirJumpSpeed getter only
- `Assets/Scripts/Visuals/CutoutRig.cs`
- `Assets/Scripts/Visuals/CharacterVisualDriver.cs`
- `Assets/Scripts/Visuals/CartoonRagdoll.cs`
- `Assets/Prefabs/Enemies/Samurai.prefab` — only gameplay prefab modified
- `Assets/Art/Sprint0244/Samurai_UpperArm.asset`
- `Assets/Art/Sprint0244/Samurai_Forearm.asset`
- `Assets/Art/Sprint0244/Samurai_Hand.asset`
- `Assets/Art/Sprint0244/Samurai_Thigh.asset`
- `Assets/Art/Sprint0244/Samurai_Shin.asset`
- `Assets/Art/Sprint0244/Samurai_Foot.asset`
- `Assets/Editor/Sprint0244Setup.cs` (new; repeatable native Sprite/prefab setup and preview)
- `Assets/Tests/EditMode/Sprint0244RigTests.cs` (new)
- `Assets/Tests/PlayMode/Sprint0244Tests.cs` (new)
- `Assets/Tests/PlayMode/Sprint0243Tests.cs` — retain all cases, adapt three
  assertions to the actual segmented grip, expanded ragdoll, and valid route shortcuts
- `TestResults/Sprint0244.json`, `TestResults/Sprint0244_Articulation.png`
- `DEVELOPMENT_LOG.md`

Automated verification through Unity 6000.6.6f1:

- **Full Edit Mode suite: 59/59 passed.**
- **Focused Play Mode fixtures: 98/98 passed.** This is not full Play Mode-suite
  verification. Includes 11 new Sprint 024.4 cases, all 15 Sprint 024.3 cases,
  specialist tests 4, fireworks/scene fixes 7, movement stability 1, player double
  jump/suppression 9, mobile/ladder 8, morale 6, second-arm 2, shared visuals 8,
  spawning 9, opening spacing 1, combat positioning/suppression 7, campaign flow 10.
- New coverage checks hierarchy/art preservation, joint limits and attachments
  in both directions, grip orientation across poses, real higher-platform and
  wide-gap double jumps, single-jump conservation, third-impulse rejection,
  grounding reset, moving targets, destroyed destinations, blocked ceilings,
  alternate route/failed-edge penalties, intermediate ladder exit, warehouse
  single/double pursuit and 14-body ragdoll constraints/velocity limits.
- Actual-prefab pose preview visually inspected: idle, run, mirrored jump,
  second-jump tuck, wind-up and swing. Preview is not a live combat playtest.
- Final Demo inspection: 15 enemies, one Samurai, zero missing scripts, all
  articulated references bound; original head/sword assets and damage 40 intact.
  Final Console zero errors/warnings; compilation complete. MainMenu clean,
  open, outside Play Mode; Landscape Left preserved.

Manual verification still required:

1. MainMenu > Play > Warehouse: bait Samurai along the central mezzanine and
   UpperStep01/02/03 toward the roof; change platforms during his pursuit.
   Check meaningful use of ordinary hops versus a second impulse, launch
   positioning, ceilings, occupied landings, recovery after obstruction and
   routes which use an existing ladder.
2. Inspect elbows, wrists/hilt, knees and ankles at gameplay zoom during fast
   turns, run/stop, first/second jump, landing, crouch, climb, anticipation,
   slash, recovery, flinch and ragdoll. Check seams and clipping around cover.
3. Verify melee still feels threatening/fair at 40 damage and the preserved
   timings; check player double jump, ladders, Larper suppression/morale,
   fireworks, checkpoints, shop, pause/results and saved progression in play.
4. Profile sustained combat on the target Android device. No APK/device run,
   human combat playtest or Android performance benchmark is claimed.

The route cache is deliberately local and bounded, not a global navigation
solution. Conservative rejection can leave some long/dynamic routes unavailable;
new obstacles or actors can enter a committed arc. Those cases use failure
cooldowns/replanning rather than teleporting. Full legacy Play Mode-suite/order
issues from earlier sprints have not been claimed resolved. No unresolved
failures remain in the suites executed for this sprint.

Existing unrelated dirty work preserved. No staging, commit or push.

## Sprint 024.5 — Master Larper candidate, consistent Samurai, armor and interactions

**Implemented by Codex — awaiting playtest and artistic approval.**

Inspected actual Enemak Postava PNGs and selected numbered GIF pose files from
Google Drive/local references. Originals preserved; measured dimensions/palettes,
reference paths and approval rules are recorded in new ART_BIBLE.md and AGENTS.md.
The three inspected GIFs contain one frame each; no animation-timeline claim.

Added reversible LarperCandidate/SamuraiCandidate skins: deliberately primitive
flat pixels, 32 PPU, uniform peach Larper skin, red shirt/blue trousers/dark shoes,
six existing expressions plus a distinct optional panic face. Extended the existing
optional segmented cutout rig for Larper firearm/reload poses and 14-body corpses.
Samurai retains outfit/katana/AI/mobility tuning; duplicate inherited old limb chains
were removed and the blade reduced to the same simple pixel density. No player
movement/crouch-pose rewrite or new animation framework.

ArmorVisual observes existing armor restore/damage events, showing intact/damaged/
depleted vest overlays only for actual equipment and copying the state to corpses.
Ordinary Larpers remain unarmored. Existing condition-only saves cannot distinguish
depleted from unequipped after a new restore; broken visuals persist in the current
equipped run. Gameplay armor values/absorption are unchanged.

Separated the office checkpoint from the records case in Demo/Mission01: flag/zone
(15.3,6.4), supported respawn (14.5,6.25), evidence remains x20. Previously they shared
one object and collection also committed a checkpoint. Independent activation now
preserves objective snapshots and EXIT gating without simultaneous activation.

Fixed downward ladder acquisition treating crouch input as upward through Abs(y).
Downward entry now requires a valid ladder top; intentional cover has priority via
the existing bounded local probe. Verified the real LowCover_Sprint012/west ladder
arrangement from behind/on/off the box, plus left/right overlapping-ladder cases.
Upward climbing, release, jump-off/double-jump, lockouts and gravity are preserved.

Automated Unity verification: **63/63 full Edit Mode; 155/155 focused Play Mode**
across 20 fixtures. Existing tests retained; changed sprite/body-count/checkpoint
expectations reflect the requested implementation. No full Play Mode assembly run,
human gameplay validation, APK run or Android benchmark is claimed. Source diff
whitespace check passed; whole dirty-tree diff check also reports Unity serializer
trailing spaces in YAML (not hand-edited for cosmetic cleanup).

Reference sheet and controlled actual-zoom warehouse camera captures are under
TestResults/Sprint0245_*.png. Capture-only player placement/protection/armor changes
were in Play Mode with no GameSession and were discarded on stopping. Full results,
file manifest, reference accessibility, manual checklist and limitations:
TestResults/Sprint0245.json and TestResults/Sprint0245_Report.md.

Candidate art is **not approved as a permanent master**. Human playtest and artistic
review remain necessary. Warehouse_Demo saved/open and outside Play Mode; no missing
scripts. No staging, commit or push; existing unrelated work preserved.

## Sprint 024.6 — Main Menu entry, corpse collision and extended-mag pistol

**Implemented by Codex — awaiting playtest verification.**

Global Windows/default and inherited Android build scenes already began with
MainMenu; no runtime Testbed startup override was found. The confirmed Editor
cause was leaving Warehouse_Demo open with no playModeStartScene. CampaignEntry
now defaults ordinary Editor Play to MainMenu, retaining explicit open-scene
development Play and restoring the menu afterward. An Editor-only test-job
exception prevents the menu override from replacing Unity's test initialization
scene; the final real menu/shop/results flow passed with the override enabled.

Actual configured Editor Play from an open Warehouse_Demo loaded MainMenu, with
MainMenu session screen, zero enemies and advancing frames. Deliberate development
Play still loaded the warehouse and restored the menu entry on stopping. Windows
and Android binary/device launches remain manual; working build lists were not
rewritten. Campaign UI, purchases, equipped stock and wallet settlement passed
the existing ten-test progression suite using temporary saves.

Ragdoll cause: six original colliders excluded character layers, but the eight
articulated segments defaulted to excludeLayers=0. New segment colliders now copy
the original limb's exclude/include masks and override priority. Current masks
remain -2 (physical Default terrain contact, no character contact), with colliders
enabled/non-trigger and layer 1 still included in hitscan physical-impact queries.
Both Player/Enemy shots hit corpses and trigger impacts. Walking/jumping through
overlapping corpses, environmental settling and a pile beside a narrow-platform
ladder passed automated checks. Living actors and global physics matrix untouched.

Pistol: native flat dark 20×8 sprite, 32 PPU, three colors; clear 5×13 extended
magazine replaces 8×18. Visual socket moves from (4/32,-2/32) to (1/32,-2/32) beneath
the grip for this weapon only. Other weapons keep their old default. Existing aim,
reload and recoil systems retained. Damage5/interval.105/capacity27/reload4/range28/
impulse.9/reserve135 all unchanged. No unrelated Larper art or gameplay edits.

Verification: **65/65 full Edit Mode and 102/102 unique focused Play Mode** tests.
An extra configured-entry UI flow rerun passed after resolving a test-launcher
timeout caused by the menu override. No unresolved failures in completed suites;
full Play Mode assembly, human gameplay and Android profiling not claimed.
Gameplay-scale pistol image: TestResults/Sprint0246_Pistol_Gameplay.png.
Detailed root causes, files, exact filters/visual values and manual checklist:
TestResults/Sprint0246_Report.md; raw results: TestResults/Sprint0246.json.

Finish: MainMenu saved/open, default Play entry MainMenu, Play Mode stopped.
No staging, commit or push; existing unrelated dirty work preserved.

## Sprint 024.7 — graphics, directional camera and suppression (2026-10-22)

Implemented by Codex — awaiting playtest verification.

The brief explicitly selects the existing Larper as production baseline. Inspected
original Postava/current Larper references; all six historical reference hashes
remain unchanged. Updated ART_BIBLE/AGENTS. Added editable native flat-pixel hero,
tiny cardboard-cosplay Samurai and colorful amateur fireworks skins, preserving
the existing rigs, equipment, player physics and enemy roles. Samurai authored
height is 70.16% of Larper; sword length 146.03% of Samurai. Living target collider,
40 damage, 1.65 melee reach and .4/.7-second wind-up/recovery remain. Resized corpse
colliders fit copied native sprites and retain Sprint0246 contact filtering.

Camera derives one-third/two-thirds composition from final orthographic width,
aspect and configurable effective viewport. .12-second facing commitment and
.22-second framing smoothing filter rapid reversals; interpolated horizontal
displacement tracking prevents steady running lag. Vertical behavior/bounds remain.
Zoom 1.10→1.155 (+5%); Android's separate 1.125 scale stays. Player motor 7→8.4,
basic Larper 7→7.7; specialists stay 7. No multiplier compounding or acceleration,
deceleration, collision/input update rewrite.

Suppression already used clipped real shot segments. Fixed high first-pulse
hesitation being bypassed by the retreat branch. Added bounded randomized
hesitation beats and meaningful five-second contextual suppression cues using
existing flinch/faces/speech queue. Radius 1.25→1.5, overhead bias .35→.15,
gain49→58, moderate16→14, high48→42, duck .70→.80, duck cooldown3–5→2.5–4.25s.
Preserved decay12/s, quiet .8s, cap100, retreat .7/4s, personality differences,
rare panic/cascades, cover occlusion and two readable simultaneous bubbles.

Verification: full Edit Mode75/75; unique targeted Play Mode137/137. Covered
camera ratios/zoom/full-speed reversals, rendered movement, double jump/touch,
cover/spacing/spawning/offscreen firing, morale/speech, real-hit-scan suppression,
Samurai platform traversal/attacks/ragdolls, fireworks and menu/shop/results.
Updated obsolete hidden-face/native-art/hardcoded-speed expectations; no tests
removed. Two MCP transport interruptions were retried, with results recorded.
Controlled warehouse frames advanced; interior player viewportX=.333612 at
1902×870, matching one-third within pixel rounding. Protected player/diagnostic
reposition were used; no campaign save session was active. This is not a human
gameplay/artistic approval or Android performance claim.

No missing scripts/broken references in the three warehouse scenes. MainMenu
saved/open, default entry MainMenu, Play Mode stopped; current Console0errors/
0warnings. Historical compiler/account warnings are not attributed to this sprint.
No staging/commit/push. Detailed files, exact tuning, test evidence, captures,
limitations and manual checklist: TestResults/Sprint0247_Report.md,
Sprint0247_Files.txt and Sprint0247.json. Still requires human gameplay/art review,
Android device profiling and Japanese font/glyph verification.

## Sprint 024.8 — Hotfix O'Clock (2026-10-22)

Implemented by Codex — awaiting human playtest verification.

Warehouse_Demo now has three registered Samurai /17 total enemies (3/6/8 groups).
Original moved to(-4.8,.529232); added(-.8,5.929232) and(23,.529232). Authored
positions/reset points match and have valid ground/clearance/spacing. Static
inspection did not reproduce the original saved-platform overlap. Tests caught
and removed intermediate cloned child residents; setup is now idempotent.

Visual scale is exactly1.05. Per developer follow-up, living Samurai geometry now
matches visible body size/position: standing movement box about.75396×1.02846,
centered on the rebased body. A non-contact animated body hitbox tracks body bounds
through both facing directions and poses, excluding the sword. Shots/blasts ignore
the separate movement shape; empty-space hits do no damage. Mass center remains
fixed; corpse segment scale/filtering is preserved. Health70, damage40, speed7,
attack range1.65 and .4/.7-second anticipation/recovery remain unchanged.

Shrinking to actual body size exposed floating-step wedging: body-height rays can
miss an overhang. Existing single backstep recovery now recognizes the cached
route overhang, validates1.8units/.38seconds in that case, and preserves normal
1.2/.25 recovery plus existing jump/cooldown/blacklist systems. All formerly
failing authored-step/single/double-jump cases pass.

New native32PPU art: crude purple firework-logo shirt, matching sleeves, larger
bright-yellow hardhat; clearer30×11 carbine; directional15×9 white/yellow/red
carbine-only flash using existing .06-second cue/glow. Carbine stats unchanged.
Firework damage radius2.2→2.7, visual spread1.5→2.7, fading lifetime.35→.8; same
ten prewarmed sparks and damage24/LOS/falloff. Normal/short/long release fuses
.8–1.5/.25–.45/2–3.2s, with18% short/18% long. Premature held failure6→8%, at
configurable35–80% of preparation. All firework blasts can damage all teams and
self; ordinary bullet friendly-fire rules remain. Pending throws clear before
held blasts; detonation guards and pooling remain.

Camera side damping.22→.75 with existing composition, commitment, zoom1.155,
vertical behavior and input responsiveness preserved. Larper pistol interval
.105→.07 (9.52→14.29shots/s), capacity27/damage5/reload4/spread unchanged.

Validation: compilation successful; full Edit Mode78/78 and unique targeted Play
Mode92/92. Includes three-Samurai runtime pursuit/engagement/reset, exact animated
hitbox geometry and no invisible hits, traversal/ragdolls, fuse variations, held
failures, self/allied damage/dedupe/occlusion, pistol cues/ammo/reload, flash,
camera, spawning, suppression/morale, motion and campaign flow. An unfocused flow
fixture timeout was resolved with background-execution setup/restore; retry passed.
Controlled live captures show carbine flash and colorful burst. Actual prefab
blast health: Larper50→27.7548, Samurai70→54.86103, expert60→39.53453, one hit each.
These are controlled Editor checks, not human gameplay/art/Android acceptance.

Updated ART_BIBLE; detailed report/raw results/captures: TestResults/Sprint0248*.
Zero broken references/missing scripts across warehouse scenes; populations17/7/10.
MainMenu saved/open/default entry, Play Mode stopped, background-running restored
off; current Console0errors/0warnings. No staging/commit/push; unrelated work kept.

## 2026-10-22 — Focused gameplay and visual hotfix

Implemented the six requested focused changes. Checked available chats for another
active local project sprint before editing; preserved existing dirty/untracked work.

Basic pistol Larper first-sight/reacquisition reaction .30→.55 seconds, existing
0–.05 jitter unchanged. Established firing cadence unchanged. Samurai reaction
remains .18; fireworks specialist explicitly retains .30 through a prefab variant
override, preventing the parent Larper tuning from leaking into its behavior.
Wallet auto-pickup distance AND trigger radius1.20→1.62 (+35%); existing LOS,
reward-ID deduplication and amount-free `Wallet collected` notification retained.
Samurai jump velocity12→13.28; second impulse10.2→11.288 via existing .85 multiplier.
Ideal single-jump height +22.47%; actual physics apex ratio tested within20–25%.
Player and other enemy movement/jump values unchanged.

Reproduced a real Warehouse_Demo stall before x7.8 low cover at x6.61. Route cache
excluded one-unit crates and landing margins rejected their tops. Minimum node
width1.1→.9 and landing/launch side margins.15/.20→.10/.10 allow these footholds,
still using actual collider dimensions and swept arc/occupancy checks. Then
reproduced a descent stall on the alternate gap step: the old waypoint was inside
the edge and generic movement refused to leave it. A validated physical walk-off
now uses existing finite flight/landing recovery; timeout derives from distance
and gravity. Existing sampled planning, stuck-progress watchdog, failed-edge
cooldowns, arrays and gravity behavior retained; no new per-frame scan or manager.

New native four-color30×11 carbine with wooden butt/handguard, darker receiver,
thin barrel and bent magazine. Same32PPU/pivot(7,5), no mipmaps, point filtering;
weapon stats, aiming, sounds and flash unchanged. Historical Postava and actual
Larper/pistol presentation inspected; originals preserved. Controlled both-facing
idle/walk/crouch/reload sheet: TestResults/FocusedHotfix_Carbine.png.

Armor bug: old code intentionally retained a broken overlay at zero and did not
observe armor component disabling. Positive condition/active equipment is now
required; depletion/removal/disable clears the sprite and disables the overlay.
Re-equipping restores intact/damaged art. Depleted armor is not copied to corpses.
Armor damage/protection, underlying character sprites and saves remain unchanged.

Validation: compilation successful, full Edit Mode78/78, unique targeted Play
Mode76/76. Includes actual warehouse crate/cover/raised-step/upper-route traversal,
physical jump height/landing reset, first-shot delay versus automatic cadence,
wallet auto pickup at1.5/rejection at1.9/dedupe/LOS, armor transitions/corpse,
Samurai melee/blocked routes/air jumps/ladder/ragdolls/animated hurtbox, carbine
flash, player motion/double jump, scene equipment/encounters and combat. Baseline
and failed intermediate runs retained. Updated old assertions for zero-impulse
walk-off and stronger-jump shortcuts; separate single/double coverage preserved.
Art allowlist extended with original PPU/filter/mipmap safeguards unchanged.

Zero missing scripts/broken serialized references across three warehouse scenes;
camera source hashes unchanged. MainMenu saved/open/not dirty, Play Mode stopped,
background-running restored off; final Console0errors/0warnings. Human feel/art
approval and Android-device performance remain unverified; manual checklist,
file list and exact tuning: TestResults/FocusedHotfix_Report.md. Raw results:
TestResults/FocusedHotfix.json. ART_BIBLE updated. No staging/commit/push.

## 2026-10-23 — Interrupted camera/player/Samurai hotfix recovered and verified

Recovery inspection confirmed the earlier six-change hotfix was already complete:
Larper first reaction.55s, wallet radius1.62, Samurai jump13.28, crate/cover/descent
fixes, native carbine and complete depleted/removed armor visual disappearance.
Preserved that implementation and its existing report/results; did not repeat it.

The later camera/player/Samurai follow-up had explicitly been requested and was
already implemented on disk before the usage interruption. This recovery finished
pending verification, refreshed final art evidence and wrote the report/log. It
introduced no further gameplay features or source rewrites.

Camera now caches existing ladder traversal state, damping directional framing to
center during climbing and back on dismount. Climbing aim does not steer it.
Normal1/3–2/3 composition, .75 transition, .12 commitment, .16 follow smoothing,
vertical following, zoom/clamps and pixel presentation are preserved.

HeroRefined retains the yellow-cap/orange-shirt worker and32PPU module bounds/
pivots. Cleaner cap/hair, short neckline, coherent shoulders/sleeves, trimmed hand
corners and transparent trouser edges fix the fused-leg silhouette. Body/physics,
animation timing, two arms, grip/muzzle, armor and equipment states unchanged.
Native recipe/source and older art retained; historical Postava1 was inspected,
originals untouched. Shared skin reaches all three warehouse scenes.

Small Samurai collider detection hypothesis was not reproduced: real .7539605×
1.028464 collider detects unobstructed targets at different heights and solid walls
occlude correctly. Detection remains22 units; collider-top−.15 sight origin and
existing geometry mask/raycast are unchanged. No hitbox enlargement or omniscience.

Samurai acquisition now requires explicit gameplay viewport overlap plus valid
existing LOS/range. Existing engagement flag latches aggro beyond camera exit and
old3-second sight-memory timeout, while hidden player movement cannot update the
last-seen destination. Defeat/target/reset still clears it. Offscreen melee is
blocked; wind-up.4, recovery.7, damage40/range1.65 and suppression remain. Samurai
reaction.18+0–.05→.04+0–.02; startup random.3–.8→.05. Larper tuning untouched.

Confirmed traversal fixes: stationary failed-jump rejection previously persisted
forever after a temporary obstruction; now expires after at least5 seconds and
still requires validated arcs. Brief decision gaps no longer cancel finite
in-flight plans. Combat enable/target-reset synchronously suspends unacquired
Samurai patrol, preventing a first-FixedUpdate unplanned jump. Existing local
graph, arrays, sample intervals, failed-edge cooldowns and previous terrain fixes
are preserved. No new per-frame physics scans or global searches.

Verification: compile successful; full Edit78/78 and86 unique targeted Play checks
pass across retained runs, final affected fixtures rerun after the last fix.
Includes six new actual-small-body/warehouse tests, ladder/normal camera framing,
autonomous visible acquisition/cover/elevated pursuit/melee, camera exit and
occlusion memory without hidden tracking, reset, removed-obstacle replanning,
decision-gap flight, old traversal/ragdolls/hitbox, player visual integration,
double jump/motion, mobile controls, armor, combat and Larper reaction/cadence.
Earlier failed attempts retained in TestResults/CameraSamuraiHotfix.json.

Controlled final comparison sheet and live warehouse camera render reviewed;
runtime advanced frame2622→6163/time8.098→18.799s, camera size4.843074. These are
Editor evidence, not human PC playtesting or art acceptance. No Android/device
profile. Zero missing scripts/broken references in three warehouses; final Console
0errors/0warnings. MainMenu saved/open/not dirty, Play stopped, background-running
restored off. No staging/commit/push; existing uncommitted work preserved.

Detailed files/settings/manual checklist: TestResults/CameraSamuraiHotfix_Report.md;
raw tests and verification: CameraSamuraiHotfix.json and
CameraSamuraiHotfix_Verification.json. ART_BIBLE updated. Pending future agenda:
player melee tracer/swing/hit-feedback investigation is not implemented or verified.
Larper knife is explicitly excluded; their pistols remain. Human PC camera/AI feel,
refined sprite approval and Android performance still need manual validation.

## 2026-10-23 — Sprint 0025: combat readability and Faintanyl revival

Implemented by Codex — pending playtest. Reused the existing follow-up's fixed
camera/ladder, player art, armor cleanup and Samurai navigation; no repeat rewrite.
Samurai viewport activation now follows cached animated body bounds rather than
the physical box. Player melee has a .22s articulated swing and range-bounded
contact sweep; shared feedback no longer emits gunshot/muzzle cues for melee.
Actual opening fire/cover decisions and ally/panic events use the tactical speech
queue, preserving two bubbles, readable duration, priorities and repetition gates.
Enemy suppression and rare cascade probabilities remain unchanged. New player
near-miss pressure uses clipped hostile shot segments, occlusion, bounded gain,
gentle sound/border and decay; no movement, aim or camera penalties.

User clarified Faintanyl: centered death-screen revive, exactly two seconds of
invincibility, inventory maximum two. Implemented separate injection stock
(configurable initial two), in-place full-health revival, a checkpoint alternative,
input/physics suspension while dead, immediate campaign save on consumption and
no retry refill. No checkpoint reset/ammo refill on injection. Legacy saves get
the finite initial stock; empty stock preserves checkpoint fallback. Continuous
duct-tape healing and its 200-point baseline remain. Capture review fixed enemy
speech overlapping the death caption without obscuring a paused menu.

Dedicated Android BLIND FIRE is conditional on cover; normal FIRE remains outside.
Control transitions release captured input and guard against other-control release
events. Wallet radius and trigger 1.62 → 1.95 (+20.4%); LOS/deduplication retained.
Existing HeroRefined/Larper/Samurai/carbine art and muzzle/fireworks presentation
were preserved; armor disappearance passed existing regression checks.

Verification: 78 Edit Mode and 145 unique selected Play Mode tests pass (223 total,
latest result per test). Includes small-body Samurai LOS/viewport/pursuit/platform
routes, camera/ladder, local melee/no muzzle, speech/cascades, suppression, armor,
healing/wallets, cover/touch, double jump/motion, opening enemies, combat/reload,
campaign/reward flow and new keyboard/touch revive/protection/stock persistence.
Updated checkpoint-specific fixtures for finite injection stock, actual reward
banking and input timing; moved only the ladder test's isolation origin closer to
avoid floating-point camera damping stalls. No ladder/camera runtime changes.

Compilation succeeds; final Console has zero errors and one Unity AI Toolkit
account-service timeout warning. Existing obsolete API compiler warnings remain
in retained history; unrelated API migrations were not performed. Android build
configuration dry run valid; no APK build/device verification claimed. All three
warehouse scenes have zero missing scripts and inherit new player components.
Controlled runtime revive capture reviewed; not human playtesting. MainMenu open,
clean and stopped; MainMenu startup setting/background-off restored. No staging,
commit or push. Full files/parameters/root causes/manual checklist:
TestResults/Sprint0025_Report.md; raw runs including reruns: Sprint0025.json;
controlled UI evidence: Sprint0025_Revive.png. Human gameplay feel, remaining
unrepresented Samurai stuck cases, art acceptance and Android performance remain
pending playtest; no claim of human-accepted completion.

## 2026-10-23 — Hotfix 0025.1: enemy mobility, top pause, carbine exchange

Implemented by Codex — awaiting playtest verification. Ground pursuit previously
stopped horizontal drive against a box during the airborne part of a jump. It now
continues physical airborne movement; low hops validate clearance/landing and a
bounded arc, with 1.2s cooldown. Requested grounded movement without .12 units of
progress for .9s triggers existing-goal reconsideration: three-second failed-goal
exclusion, existing reachable/reserved alternatives, withdrawal fallback and
Samurai safe .35s staging backoff/failed-edge reconsideration. Spacing accounts for
actual vertical separation. No teleportation/new navigation framework. Larper
reposition interval8–13→4–7s, roll20→35%; personality choices, morale, suppression,
firing, spawning and reservations remain. New stuck/jump cooldown fields exposed.

Pause moved from bottom43% to safe-area top12px/center50%, minimum124×52. Existing
look/action preserved; mission card reserves space to its left to avoid overlap.
Other mobile action/joystick positions and camera/ladder code unchanged.

Carbine magazine was baked into its sprite with no separate magazine assigned.
Native Editor authoring separates the same pixels/palette into receiver and4×5
magazine at32PPU, retaining pivots/muzzle. Existing arms reach/remove/put aside,
fetch a distinct replacement, insert, latch and regrip on the real reload clock.
Two prewarmed components, no physics litter/new rig. Original1.5s/20damage/.2s/
24round/22range unchanged. WeaponCycle remains sole ammo authority; insertion
marker fires once at deadline; interruption transfers nothing/restores visual.
Other weapons retain their existing animations. Original source/historical art
and HeroRefined/Larper proportions preserved; actual Postava1 inspected.

Verification:78Edit+107unique selectedPlay tests pass (185 total), including six
focused hotfix cases, actual low-box crossing, bounded blocked recovery/ceiling
rejection, magazine exchange/atomic ammo/cancellation, three safe-area resolutions
and pause/resume, plus Samurai/platform/crowd/opening/weapon/effects/control/cover/
morale/revive/healing/camera/ladder/motion/campaign regressions. Initial test actor
archetype setup was corrected; raw runs retained. Final compile and focused rerun
pass; Console0errors/0warnings. Android build configuration dry-run valid; no APK
or Android-device playtest/profiling claimed. Approved Unity CLI update verified
beta.14. One tool review timeout/reload reconnect was retried successfully.

Controlled two-facing reload storyboard and live warehouse pause/reload capture
reviewed. Runtime frame17126→18766/time49.9249→50.4685 before intentional capture
freeze at.3624reload; ammo3/72 unchanged. Temporary save/capture state removed;
MainMenu clean/open/stopped/startup restored/backgroundoff. No commit/stage/push.
Details/files/parameters/manual checklist: TestResults/Hotfix00251_Report.md;
raw tests/verification: Hotfix00251.json; images: Hotfix00251_Reload.png and
Hotfix00251_Gameplay.png. Real combat feel, exceptional terrain layouts, reload
readability at device zoom and Android touch/performance remain pending playtest.

## 2026-10-23 — Hotfix 0025.2: Glock magazine physics and reliable enemy suppression

**Implemented by Codex — awaiting playtest verification.**

Diagnosed floating player Glock magazines as generic gun-child sinusoidal offsets
without hand attachment or release physics. Added player-only PistolMagazineReload
using existing fixed-length arms, socket and original reload clock: seated until20%,
extraction20–40%, release42%, replacement58–88%, regrip by100%. Three prewarmed
geometry-only Rigidbody2D magazines fall under gravity and expire after4s; pooled
simulation resumes at the actual hand Transform. BackupPistol reuses existing
approved Glock/magazine art; player prefab accepts BackupPistol/Buzzbox explicitly.
Cancellation/switching restores the seated visual; ammo remains solely WeaponCycle.
Carbine's intentionally bizarre reload is retained exactly: CutoutRig, WeaponCycle,
WarehouseCarbine and both carbine sprite assets match hotfix-start SHA256 hashes.

Actual AttackExecutor overhead shots already reached/saturated enemy suppression.
Downstream optional retreat cooldown/failed roll, reload/cover compatibility gates
and failed long-retreat→stationary-reload fallback masked meaningful movement.
After>=3 credible close misses over.45s at high pressure, an independent evasion
opportunity uses existing cover/withdrawal/reservations, retries every1.5–2.2s divided
by susceptibility, can cancel a reload without transferring ammo, and holds genuine
protective cover. Blocked long retreats try reachable/reserved short alternatives
(minimum1unit, open reach up to2); impossible escape uses.6s hesitation instead of
fake full-magazine reload. Existing rare panic opportunity runs before ordinary
escalation, retaining probability/long cooldown and no new collapse/social events.
Active Samurai keeps pressure/brief hesitation without Larper duck/withdraw/panic.
Two short suppressed speech lines reuse existing5s cue gate and two-bubble cap.
Gain58/radius1.5/moderate14/high42/max100/decay12/quiet.8, optional70%/4s, personality
weights and defensive/panic/cascade tuning unchanged. No new per-frame scans or
route framework; player pressure, camera/ladder/spawn/movement code untouched.

Verification:78Edit+84unique selectedPlay=162pass, including7focused cases for
real-hit-scan overhead pressure/actual displacement/decay, reload interruption,
blocked alternatives, wall/distance rejection, Samurai pressure/role, Glock hand
attachment/gravity/floor collision/4s cleanup/pool limit/atomic ammo and unchanged
carbine poses. Existing combat/reload/cover/morale/speech/rare-cascade/ragdoll/revive/
Samurai/navigation/double-jump/touch/motion checks pass. Early failures revealed
pooled pose reset and panic-priority interaction; corrected and affected suites
rerun, raw history retained. Static two-facing reload storyboard inspected; actual
physics tested in Play Mode, no human normal-speed or Android-device acceptance
claimed. Final compile up-to-date, current Console0errors/0warnings, native player/
Larper/Samurai prefab checks0missing refs. Android configuration dry-run valid;
no APK built. MainMenu clean/open/stopped, startup preserved/backgroundoff. No
stage/commit/push. Details, exact files/values and manual checklist:
TestResults/Hotfix00252_Report.md; raw Hotfix00252.json; native checks
Hotfix00252_Verification.json; carbine hash manifest; Hotfix00252_Reload.png.

## 2026-10-23 — Hotfix 0025.3: mission gates, Larper reloads and fireworks

**Implemented by Codex — awaiting user playtest verification.**

User identified the menu-launched Demo and a checkpoint that would not activate.
Baseline9mission/12campaign checks passed; no missing mission bindings or runtime
exceptions found. Checkpoint contacts silently rejected unmet objective stages,
and the Processing boundary existed only at y8–11 on the upper stairs. Lower-route
entry could miss that stage gate and leave office checkpoint/records locked. Exact
checkpoint/original run unavailable; do not claim this conclusively reproduced the
user's particular run. Existing eastward boundary x7.5/width2 now spans y−8…20
(center6/height28), including lower routes and high jumps, in Demo/Mission01 via
Editor APIs. Storage checkpoint and encounter objectives remain mandatory. Locked
flags explain prerequisites/remaining enemies/floors through throttled4s existing
notifications; active flags are not reported locked. Encounter callbacks evaluate
completed prerequisites, with one explicit context-menu diagnostic and no log spam.

Larper reloads had never received the player-only working magazine component or
enemy weapon clock. Reused PistolMagazineReload unchanged on basic Larper, wiring
the existing enemy cycle and adding an enemy-only articulated-grip adaptation.
No joint/pivot/proportion/art changes. Existing exchange/physics/4s cleanup and
atomic ammunition authority reused; two prewarmed debris slots per NPC. Inherited
component removed on Samurai/fireworks variants to avoid useless pools. All13Demo
and7Testbed basic Larpers inherit the fix. Player Glock source/definition/prefab,
CutoutRig, WeaponCycle, carbine definition and both carbine sprites remain byte-for-
byte equal to hotfix-start hashes; player adaptation path unchanged.

Larper-base near-miss gain58→63.8 (+10%); decay/thresholds/personality weights,
sustained evasion, rare panic/cascade rules and speech cap unchanged. Specialist
recovery3.8→3s; .85s anticipation preserved (~17% more nominal opportunities).
Projectile speed/arc/fuses/damage/radius/malfunction/survival remain. Existing pool2
still bounds concurrency. Firework visuals reuse prewarmed sprite effects:24radial
trails,6delayed sparkles,3haze, .075s crisp star flash/.14s faint soft glow;35visual
slots per projectile, original.8s expiry. Two tiny native pixel masks, no bloom,
new dynamic lights, damage-query changes or per-burst allocation/instantiation.
Cosmetic glow is a world-space wash; Android GPU/device acceptance remains open.

Verification:78Edit+80unique selectedPlay=158pass, including4new focused cases.
Controlled mission test fulfills encounters/records/checkpoint gates, physically
contacts checkpoint/lower Processing, retains records on respawn, walks through
the exit, verifies Results/rewards/save and return to Level Selection. Combat and
intermediate placement are controlled; no unassisted full gameplay run claimed.
Real-clock Larper hand/grip/drop/atomic ammo and reusable fireworks sequence/damage
checks pass. Existing mission/campaign/player reload/AI/crouch/cover/morale/rare
cascade/Samurai/weapon/fireworks/double-jump/ladder/movement regressions pass.
One old burst assertion now observes initial duck plus valid later evasion rather
than requiring a frame-dependent final crouch; damage/decay checks retained. One
transient Edit sprite identity failure passed unchanged on isolated/full reruns.
Static two-facing reload and four-phase fireworks storyboards inspected. Final
compile current, Console0errors/0warnings; five native prefab scans0missing refs;
Demo opening3 unchanged. Android configuration dry-run valid; no APK/device test.
MainMenu clean/open/stopped/startup preserved/backgroundoff. No stage/commit/push.
Full details, exact files, parameters, limitations and manual checklist:
TestResults/Hotfix00253_Report.md; raw Hotfix00253.json; protected hash manifests;
Hotfix00253_LarperReload.png and Hotfix00253_Fireworks.png. User confirmation of
normal completion, enemy reloads, suppression feel and fireworks appearance pending.

## 2026-10-24 — Hotfix 0025.4: Suppression-driven panic escapes

Implemented by Codex — awaiting user playtest verification. Started Oct23;
resumed Oct24 without restarting existing implementation. Latest recovery continued
the running test job and finalized verification/evidence/documentation. Existing
uncommitted work preserved; no staging, commits or pushes.

Root cause: FULL_PANIC stops movement; real running uses Withdrawing, previously
repeatable without a success budget. Severe sustained suppression now selects an
actual bounded withdrawal for explicitly opted-in basic Larpers. No accumulation
increase: gain63.8, thresholds14/42, decay12/s and rare full panic/cascade unchanged.
Base major roll90% at pressure70 after3misses over.45s; neutral personality results
coward98%/ordinary90%/confident67.5%. Existing morale modifies the roll within98%.
Decision cooldown4–6s divided by existing susceptibility. Maximum2 successful
escapes per explicit encounter reset; quota is charged only at a grounded endpoint,
at least2units from start and within.25vertical units. Failure/interruption/timeout
does not charge. Target/camera rebinding and disable/enable do not reset quota.

Planned destinations≤6units, duration≤3.5s, encounter-anchor leash12units, last2
endpoints excluded within1.25units. Major choices prefer farther reachable cover;
failed goals reuse existing blocked cache6s. Existing reservations, ground/low-hop
checks and motor handle movement; no teleportation/new scans/per-frame queries.
After quota, ordinary fear/ducking/accuracy/cover/reload remain; withdrawals stay
local≤1.5units. Successful major escape adds bounded.8s defensive hesitation.
Existing withdrawal dialogue event fires after actual movement;4new lines reuse
the existing speech cap2/cooldowns. Samurai/fireworks explicitly excluded; no
Officer asset exists, so distinct-archetype surrogate tests exclusion only.

Changed EnemyCombat.cs, Hotfix00254Setup.cs, Larper.prefab and3personality dialogue
assets; added6focused tests. Existing00253mission test now includes an actual
completed Larper retreat before checkpoints/records/exit/results/save/level select.
Updated stale0202configured-gain,012magazine-extraction and0245nativeSprite-identity
assertions while preserving their coverage; no production reload/visual changes.
All13protected source/prefab/scene hashes match hotfix-start manifests.

Final automated results:99unique selectedPlay+78Edit=177passed, including6focused.
Checks cover actual hitscan/physical escape, safe low-box hop, blocked/interrupted
routes, first/second success/third denial, post-cap fear/local movement, reset
boundaries, eventual suppression recovery/return fire, offscreen firing gate,
personality/exclusion and mission completion after a fleeing enemy stays tracked.
Mission setup/intermediate combat controlled: not an unassisted gameplay run.
Compile completed; post-test Console0errors/0warnings; final Console1Pipeline inspection-timeout error/0warnings. Native prefab checks
confirm basic reference/cap2/chance.9/threshold70/gain63.8 and0missing scripts on
Larper/Samurai/fireworks. A broad reference audit eval timed out; smaller checks
succeeded. Android build-configuration dry-run valid; no APK/device verification.
MainMenu clean/open/stopped/startup preserved; background execution off.

Human acceptance still required: cowardice frequency, two escapes then combat,
crowded/multifloor routes and failures, normal mission completion, rare cascades,
specialist/reload preservation and Android performance/controls. Full details and
manual checklist: TestResults/Hotfix00254_Report.md; raw test history/latest unique
results/native checks: Hotfix00254.json; protected before/after hash manifests.

## 2026-10-24 — Sprint 0026: Tactical retreats, cover and gameplay agenda

Implemented by Codex — awaiting human playtest and Android-device verification.
Inspected existing dirty work/log/AGENTS/art bible. First reran0025.3:4/4pass,
including checkpoint/records/Results/save after a real retreat. No mission rewrite.

Basic Larper withdrawal opportunities: threshold70→55; sustained misses3→2 and
duration.45→.30s; major roll cooldown4–6→2–3s before personality.90%base roll,
personality modifiers, gain63.8/decay12 and rare panic/cascades unchanged. Quota2
successful major escapes,6unit planned bound/3.5s timeout/12unit leash unchanged;
failures free, later local steps≤1.5. Existing reservations/navigation reused.

Cover checks now validate floor-relative crouched centre AND head. Old.25face gap
plus braking could expose the head; gap.08 keeps normal collision clearance.
Existing reachability validates widths instead of discarding all obstacles>1.8.
Protected arrival/reload recovery hides2–3.5s (was1.2–2.4), extends under pressure
but hard-caps6s, releases flanked/destroyed cover and resumes uncertain investigation
or normal engagement. No hidden player tracking. Local non-hop steps may crouch at
65% pursuit input. Occasional grounded withdrawal fire20%per1.1–1.6s opportunity
uses existing camera/LOS/actual trajectory/ammo logic. A forced failing test proved
gun jams froze retreat motion; fixed by keeping committed withdrawals moving while
the weapon cycle still blocks firing. No reload timing/choreography changes.

Actual suppressed major movement emits one awareness event; eligible basic witnesses
within6units/view/LOS may gain2s fear at30%chance,8s witness cooldown. No direct
panic/collapse/relay, no overwrite of existing major morale. Distinct roles intact.
Basic event callout gap3.5→2.8s; existing readable bubble lifetime/hard cap2 remain.

Added3native-authored existing-style solid wood crates to menu-launched Demo:
(3.5,.5)ground storage,(-5.5,5.9)west mezzanine,(13.3,12.5)processing roof.
10→13freestanding cover objects (+30%), excluding platforms/boundaries/hitboxes.
Editor setup idempotent; ground/solid-shot/trigger/spawn-clearance checks pass.
No objectives/spawns/ladders relocated; Testbed and Mission01 unchanged.

Secondary: analog walk/run already proportional; verified25%input/speed. Per user
clarification, down OR down-diagonal in midair after double jump queues one14units/s
dive through existing motor. Same gravity/collisions/two-jump budget, existing crouch
visual, unchanged airborne hitbox. Sideways/down landing tail.35s capped at run speed;
input reset/reversal/release cancels tail. No new animation assets. Aim stick right
inset28→44px shifts16left, retains diameter/sensitivity. Existing safe-top-centre
pause, conditional blind fire, local melee/no tracers, mild player pressure and
Faintanyl2injections/2s protection pass existing tests; hold healing preserved.
Camera, approved fireworks/frequency, player/Larper reloads and silly carbine remain.
Existing Samurai and local low-hop/stuck/ladder ownership preserved. New automatic
cross-floor/ladder retreat destination planning remains pending; no broad nav rewrite.
No Officer asset exists; basic opt-in keeps other archetypes out of the quota.

Final197unique checks pass:119selectedPlay+78Edit;11newfocused cases. Whole affected
0025.2/.3/.4 suites pass, plus suppression/morale/cascades, cover, actual rendered
motion, movement/double jump, Samurai/camera, local melee/player pressure/revival,
reloads/VFX, mission/flow/save/pause. Controlled tests, no human playthrough claim.
Initial failures retained in raw results. Duck retry test now isolates its cooldown
from earlier retreat; blocked-major fixture leaves a valid>=1unit local step;
UI assertions account for panel pixel rounding. Added route failure diagnostics.
One transient FMOD output-device initialization error occurred in an earlier run;
final affected suites passed. Compile succeeds; final Console0errors/0warnings.
Native4prefab scans0missing scripts; configured limits and3saved crate positions
confirmed. Android build-config dry-run valid, no APK/device profiling.12protected
files identical;2intentional motor/pose additions;0unexpected hash changes.
MainMenu clean/open/stopped/startup retained; background off. No stage/commit/push.

Remaining manual: retreat frequency/variety and two-success cap, multi-level/crowded
cover/failed routes, re-engagement and flanks, full normal mission completion, slide
feel near boxes/cover/ladders/platform edges, Android input/safe areas/readability/FPS.
Exact files, parameters, secondary status and manual checklist:
TestResults/Sprint0026_Report.md. Raw history/latest results Sprint0026.json;
protected before/after manifests Sprint0026_ProtectedBefore/After.json.

## 2026-10-24 — Hotfix0025.3: latest playtest follow-up after Sprint0026

Implemented by Codex — awaiting human/Android playtest verification. New brief
reuses0025.3; distinct follow-up setup/tests/report preserve the earlier hotfix.
Read current AGENTS/art bible/log and scripts/prefabs before focused edits.

Blind-fire HUD used cover instead of physical crouch; MissionControls also omitted
the button. Both HUDs now show it whenever physically crouched, hide on standing,
disable/dim without usable cover, and reject invalid pointer firing. Ordinary FIRE
and aiming joystick remain usable away from cover; disabled blind fire sits100UI
units above FIRE to avoid overlap, usable cover keeps the old position. Existing
raise/spread/LOS/ammo, pointer release and other touch input remain.

Samurai-only jumpSpeed13.28→15 (+12.95%speed/~27.6%theoreticalheight), gravity3
unchanged. Actual free-jump comparison verifies25–30%rise and proper landings;
3.2unit platform reached by one jump. Motor/player/Larper/fireworks values untouched.
Existing planner already derives arcs from motor speed. Warehouse test caught a
low-crate stall: old descending gate required>.9 vertical centre difference. Gate
.9→.3 reuses existing safe swept walk-off and drop/landing limits; whole autonomous
warehouse suite now reaches melee/pursues/replans correctly. No new nav framework,
teleportation, physics body, damage, anticipation or horizontal movement changes.

Glock tracer origin was correct, but LateUpdate moved only the flash to the later
recoiling muzzle. Explicit BackupPistol/LarperBuzzbox references now pin flash world
position/orientation to the recorded shot throughout its short life. Tracer/hitscan
remain unchanged; other weapons keep existing flash following/custom art. No sprite,
palette, pivots, ammo/rate/damage/reload edits. Native APIs authored3prefabs only;
all3warehouse scenes byte-identical, all3Demo Samurai inherit15 without overrides.

Verification:63unique targeted checks passed (54selectedPlay+9selectedEdit),
including6new focused cases. Both HUDs, disabled/functional blind fire/pointer exit,
12player Glock aim/facing/recoil/translation cases, enemy Glock and unchanged carbine;
measured higher jump/landing; actual warehouse Samurai viewport/aggro/cover/melee;
ceiling/gap/step navigation; cover, movement/ladders/weapon presentation, revival,
Larper panic escapes, combat rules and Samurai rig/tuning regressions. No existing
test assertions changed and no unnecessary full-project run. Initial warehouse
failure and passing whole-suite rerun retained in raw history.

Compile completed without errors. Post-test Console0errors/0warnings; a later broad
MCP eval timed out after10000ms and logged one Pipeline tooling error. Smaller
native retries succeeded; final Console retains1tooling error/0warnings. No logs
erased. Four native prefab scans0missing scripts. Protected files16/17identical,
only expected SamuraiCombat descent edit;0unexpected changes. MainMenu clean/open,
Play Mode stopped. No stage/commit/push. Human gameplay feel and Android touch,
safe-area/joystick-scale ergonomics and performance still need manual verification.

Exact files/values/tests/manual checklist: TestResults/Hotfix00253Followup_Report.md.
Raw results Hotfix00253Followup.json; protected before/after manifests alongside it.

## 2026-10-24 — Menu/HUD graphics rework and corrected0025.3 controls

Implemented by Codex — awaiting artistic/human/Android playtest acceptance. Read
AGENTS/art bible/current log and inspected existing UI/gameplay. Preserved the
already implemented Samurai13.28→15 (~28%height)/low-crate descent and Glock
shot-origin alignment; no repeated tuning or unrelated gameplay refactoring.

Menu: independently code-authored5×7 block wordmark, flat warehouse silhouettes,
existing ShippingCrate art and ink/paper/cardboard/yellow/orange/teal palette.
Main navigation, settings/levels/shop, scrollbar, pause, Results and revival share
flat rectangular warning-label styling. Existing callbacks/save/progression remain.
No new font dependency, generated illustration, character sprite or rig edits.

HUD: live health numbers/bar, armor/tape/revive readouts, selected weapon sprite,
large magazine/reserve fields and1/2/M tabs replace the dense equipment paragraph.
Objectives sit below vitals; pause centred in safe area, min-height56 to survive
pixel rounding. Touch sizes/radius/thresholds/sensitivity preserved; aim inset44px.
Data readout remains5Hz with cached UI references, no gameplay scans/physics added.

Removed redundant ordinary FIRE from both layouts and its field/query/layout/
pointer bindings. Secondary slot/weapon mechanics and joystick firing preserved,
verified with real touch selection plus sidearm ammo consumption. Latest corrected
blind-fire rule supersedes the previous follow-up: visible ONLY while physically
crouched AND in usable cover. Hidden in the other3states; pointer guard also checks
both. Standing/cover destruction hides/releases it while underlying blind spread,
raise/peek/LOS and input ownership remain. No merely-near-cover acquisition UI.

Verification:81unique targeted checks pass (72Play+9Edit), deduplicated reruns.
Menu settings/save/back/play/level/preparation/all shop tabs/buy/launch/pause/resume;
real HUD values/bar/icon, sidearm joystick, layout/insets; updated four-state and
cover-destruction pointer checks; existing flow/save/results/revival, cover,
movement/ladders/camera/touch layouts, Samurai pursuit/jump/landing/melee, Glock
all-direction/recoil alignment and Larper suppression/escape regressions. Existing
tests adapt to the requested removed control and new ammo fields; behavioral
assertions retained. Fresh-save sidearm fixture corrected to buy/equip in Shop;
settings slider query excludes scrollbar internals. Pause hit height enlarged
after layout regression caught subpixel rounding under its former52unit minimum.

Actual Unity MainMenu/shop/HUD captures generated and visually inspected; controlled
temporary saves and runtime poses, not human playthrough or Android verification.
All21 protected gameplay/data/prefab/warehouse-scene hashes identical. Compile
completed, final Console0errors/0warnings. MainMenu clean/open/stopped, background
off. No scenes/prefabs altered, no stage/commit/push. Body text reuses existing
Unity font; historical art unchanged. Actual app exit not invoked in Editor.

Manual remaining: artistic/readability review, Android safe areas/tap comfort/
scrolling/joystick scales/performance, normal warehouse play and desktop-build quit.
Full modifications/tests/checklist: TestResults/GraphicsUiRework_Report.md.
Evidence: GraphicsUiRework.json; protected before/after manifests; native captures
GraphicsUiRework_MainMenu.png, GraphicsUiRework_Shop.png, GraphicsUiRework_Hud.png.

## Mobile HUD & explosives expert focused hotfix (2026-10-24)

Implemented by Codex - awaiting Android/gameplay playtest verification.
Health resolved size 260x173 -> 220x141 (~31% less area); pause painted face
124x56 -> 88x40 with unchanged 124x56 touch target at safe top-center.
Startup objective 280x224 -> 224x137 (~51% less area), opacity .88 -> .65;
long text bounded with touch scrolling, mission/progress/results retained.
Only repeated startup flavor sentence omitted from compact readout.
Explosive native damage 24 -> 31.2 (+30%), radius 2.7 -> 3.2 (+18.5%).
Recovery 3s -> 1.05s +/-15%; .85s wind-up, initial 1-2s delay, fuse variation,
8% held malfunction and two-projectile/35-renderer pools preserved.
Fixed unconditional close-range retreat veto: valid ready targets can prompt a
throw while repositioning; readable wind-up still plants feet. Controlled real
pool launched 13 vs 7 times in 28s; no Larper/Samurai tuning changed.

Final unique tests: 33 Play Mode pass / 1 fail; 7 Edit Mode pass. Targeted HUD,
blast/friendly-fire/fuse/held-failure, menu/shop, cover, weapon and progression
checks pass. Older Sprint0242 suppression crouch assertion (line77) fails twice,
including isolated rerun; cause unestablished, pipeline unchanged. All21 protected
hashes match previous graphics-rework manifest. Do not claim full regression pass.
Compile successful; stopped; final current Console 0 errors/0 warnings. Initial
layout overlap corrected; fixture now resumes pause before unloading. No new
per-frame scans/managers/effect counts. No stage/commit/push.

Android tap comfort, text scrolling/readability and actual device performance,
combat balance and full-level human playtest remain required. Editor captures
and controlled tests are not Android/human verification. Full changes, exact
measurements, test limitations and manual checklist: TestResults/MobileHudPyro_Report.md.
Evidence: MobileHudPyro_Regression.json, _Before/_After.json/png,
_ThrowCounts.json, _Protected.json.

## Post-playtest hotfix / recovery completion (2026-10-24)

Resumed existing implementation; did not restart or overwrite earlier work.
Implemented by Codex - awaiting human gameplay/Android verification.
Weapon panel 219.85x219.85 -> 196x161.78 UI units (~34.4% less area), readable
weapon/ammo/icon fields and nominal44-unit selection hit heights retained.
Pause now uses two pixel bars in the existing88x40 face /124x56 hit target.
Health and objective dimensions unchanged; all12 protected hashes unchanged,
including previous explosives tuning, shared movement/ragdoll/AI and warehouse scenes.

Separate Procure Faintanyl shop button, exact description:
relax its not addictive, just habit forming
Native catalog: $75 per injection, capacity2; existing wallet/debounce/save rollback
and finite stock reused. Existing death-button revival still consumes one, restores
in place and protects for2s. New .6s one-handed flat-pixel syringe gesture follows
the free hand/facing; fire/reload briefly gated, movement retained, replay cancels.
New independently code-authored16x7 Sprite, point/32PPU, existing palette. Actual
historical Postava1 inspected; original artwork/character modules unchanged.

Player death copies the live pose before defeat observers reset it, using the
existing six-body CartoonRagdoll fallback, terrain collisions, impact/momentum,
bounded impulses, lifetime/cap and sleep. Live rig hides and controls suspend.
Revive/checkpoint retires corpse and restores presentation/simulation/controls.
Empty stock now shows1.2s collapse before automatic checkpoint recovery; existing
regression retains recovery assertions with the new intentional wait. Death screen
keeps its buttons, background .94 -> .45 so the collapse is visible.

Final unique regressions:69 Play pass /1 fail,7 Edit pass (76/77); extra explicit
legacy diagnostic excluded from totals. Five new focused cases pass again on
recovery, including actual explosion death, both-facing syringe, exactly-once
purchase/use, combat return, chosen/empty-stock checkpoint recovery and layouts.
Existing shop/progression/save/weapon/reload/cover/touch/enemy corpse/prior HUD and
explosives checks pass. Old enemy fixture used only six root bodies despite14-body
articulation; test loops now inspect/stop actual Bodies, retaining thresholds.
Final ReactiveRagdollTests5/5. Speech timing initially failed, isolated rerun passed.
Remaining ladder-top diagonal test expects velocity>3 but gets0; reproduced with
both new presentation components disabled. Movement code unchanged; cause not
resolved in this focused hotfix. Previous suppression-crouch issue remains in
prior report, not retested here. Do not claim a fully passing project suite.

Controlled native HUD/shop/syringe/death captures inspected. No human full-level
playthrough, APK/device verification or Android profiling. Final compile successful,
MainMenu clean/open/stopped; current Console0errors/0warnings. Dirty working tree
and all prior changes retained; no stage/commit/push.
Full file list, measurements, test history and manual checklist:
TestResults/PostPlaytest_Report.md; raw PostPlaytest_Regression.json;
PostPlaytest_ProtectedBefore/After.json; PostPlaytest_LegacyDiagnostics.txt;
PostPlaytest_Before/After.json and native runtime PNG captures.
Final follow-up: Unity AI Toolkit later logged one Editor account/network warning
(Account API did not become accessible within30seconds). Current Console0errors,
1warning; scripts remain compiled and MainMenu clean/open/stopped. This supersedes
the zero-warning snapshot above. No account/tooling changes were made.

## Small UI hotfix - Medical-only Faintanyl / magazine capacity (2026-10-24)

Medical tab now maps the existing serialized Faintanyl category; its purchase card
is rendered only in that section, absent from Weapons, Ammunition, Body armor and
Duct tape. Existing offer, exact description, price, cap, debounce and transactions
unchanged; no asset/category-value migration or balance edits.
Ammo HUD now shows current/magazine-capacity (for example16/27), using the selected
weapon definition; reserve remains separate. Existing cached readout refresh updates
firing/reload/slot changes. Melee keeps --. No unrelated layout/gameplay edits.
Production files: GameFlowController.cs, WarehouseTouchControls.cs. Updated existing
PostPlaytestHotfixTests, GraphicsUiReworkTests and TraversalRegressionTests assertions.
Focused verification only:3/3 Play Mode cases pass: all store categories/Medical
purchase/debounce/cap/save, actual touch firing and reload with dynamic numerator/
capacity/reserve, switching and ammo preservation; multiple simulated aspect ratios.
Native HUD capture inspected; compile successful, zero current Console errors.
Raw results: TestResults/SmallMedicalAmmo.json. No broader regressions or Android
hardware tests rerun for this small UI pass. No stage/commit/push.

## Sprint 0026 - Old-school 8-bit environment redesign (2026-10-24)

Environment-only pass for the new brief; existing gameplay/AI Sprint0026 helper
was not rerun. Warehouse_Demo, Testbed and Mission01 receive207 renderer updates:
four crude wood/industrial crate variants, chunky concrete/metal platforms, quiet
wall panels/windows/supports, simple doors/vent/pipe and matching ladder rails/rungs.
32 new native Sprite/Texture assets, shared16-color opaque palette,32PPU,
point/RGBA32/no mipmaps, at most6 colors per texture. Existing asset recipes extend
via Editor-only LevelArt0026Setup; no generated illustration or historical recovery.
Original shared crate art stays intact for MainMenu. Supply/checkpoint/mission
signals, characters, sound, UI, layout and gameplay remain unchanged.

Live Editor APIs authored scene refs. A guard caught cached collider tiling changes
before saving; collider-bearing/scaled objects now use pure visual children, so
original sprite/collider settings and transforms stay intact.16 visual children,
no added colliders/gameplay components. Exact existing bounds retained; textures
clip fractional end pixels instead of altering geometry. All3065 original protected
scene entries unchanged; all750 protected file hashes unchanged (runtime scripts,
UI/data/prefabs/original art/MainMenu/physics/build settings). Four crate variants
retained in every warehouse, including wood/metal contrast near the opening.

Final20/20 targeted checks pass:2 Edit asset/scene invariant cases,1 actual-scene
Play Mode art/solid-ray/ladder-endpoint check,12 cover regressions,4 directional
ladder detaches and1 ladder-priority/gravity-restoration case. Final native captures
Before/After0/1/2 and gameplay-camera Runtime were generated and inspected. Controlled
captures/tests are not a human playthrough or Android validation. Previous unrelated
historical test failures are not claimed fixed. Initial helper API compile errors,
authoring response timeouts and a three-variant Testbed draft were corrected or
verified before completion; final compilation/current Console0errors/0warnings.

Raw pixel payload1,028,292 bytes across32 assets; actual Android frame time/memory,
combat readability, style approval and phone-resolution sharpness require manual
playtest. No runtime generator/manager/scanning/physics changes. Warehouse_Demo
saved/open/clean, Play Mode stopped. Recovery files and prior work retained; no
stage/commit/push. Full file list, validation and manual checklist:
TestResults/LevelArt0026_Report.md. Evidence: LevelArt0026_Regression.json,
ProtectedBefore/After.json, per-scene GameplayBefore/After, Assignments,
FinalVerification and native Before/After/Runtime PNG captures.

## Sprint 0026.1 - earnest handmade environment simplification (2026-10-24)

Art-direction clarification overrides any interpretation of crude as deliberately
ugly: an enthusiastic teenager trying to draw a good game with limited experience,
not careless rectangles or generated polish. Approved crates are the benchmark.
All six crate assets and metadata are byte-identical; no crate repaint/regeneration.

Simplified26 existing non-crate textures in place; total32 unchanged. Removed
platform bolts/panel seams/chips, wall grid, fine door ribbing and pipe/beam marks.
Thicker platform edges and ladder rungs, simple two-pane slightly unequal windows,
three-slit vents and broad door divisions remain recognizable. Reduced repeated
visible windows18 ->6 per warehouse, leaving purposeful wall space. No new props,
textures, materials, shaders or runtime components. Non-crate sprites now1-3 colors;
point/RGBA32/no mipmaps/32PPU and exact dimensions retained. No intentional random
misalignment, noise or ugly distortion. Same Demo/Testbed/Mission01 layouts.

789 protected file hashes unchanged, including approved crates, original art,
main menu/HUD/shop/runtime scripts/prefabs/data/project settings. All3081 original
transform/nonvisual scene entries unchanged; collider/cover/traversal/AI/camera/
combat/progression untouched. Only background-window Renderer visibility changed
in scenes.3/3 focused tests pass:2 Edit palette/settings/saved-invariant checks,
1 actual-scene Play crate raycasts/bounds/ladder endpoints plus gameplay capture.
No broad suites repeated or unrelated old failures declared fixed. Native authoring
response timeout was checked against durable success and saved-state evidence.
Final compile/current Console0errors/0warnings, Demo saved/open/clean/stopped.

New actual gameplay-camera capture LevelArt00261_Runtime.png and earlier Before
image were inspected. These are controlled Editor captures, not human firefights,
Android profiling or artistic acceptance. Phone-size charm/readability and busy
combat/cover/ladder device review remain manual. Previous work/recovery files
preserved; no stage/commit/push. Details: TestResults/LevelArt00261_Report.md,
EditTests/PlayTests, ProtectedBefore/After, GameplayBefore/After, AuthoringResult
and FinalVerification evidence.

## Sprint 0026.2 - visibly coarser environment pixels (2026-10-24)

Resolution-only pass, not a redesign. Existing non-crate art baked into4x4 native-
pixel blocks: effective8PPU drawing grid while Sprite storage stays32PPU and exact
object/renderer/collider dimensions remain unchanged. Platforms, window/door/hazard
forms now use larger blocks. Narrow beams/pipes and ladder rails/rungs/caps use
explicit coarse cells to remain readable. No new colors/shadows/materials/textures/
props, no runtime systems; earnest handmade charm remains the goal. Crates skipped.
26 non-crate assets processed (24 actual content changes;2 already coarse/flat).

Inspection found Point/RGBA32/no mipmaps already correct; URP MSAA1, render scale1,
global quality AA0. No camera/quality change was needed or made.789 protected hashes
unchanged, including crates/metas, gameplay code, prefabs, UI/shop/menu/original art.
3/3 focused checks pass:2 Edit exact4pixel-grid/colors/settings/scene invariants,
1 actual-scene Play bounds/raycasts/ladder endpoints plus two gameplay-zoom captures.
Initial Unity runner mode-transition/restoration failure canceled; isolated stopped-
mode runs pass. Native authoring response timeout verified against durable success
and saved data. Final compile0errors;1 unrelated Unity AI Toolkit account/network
warning remains. No historical full suite/device performance claim.

Opening and ladder/mezzanine captures actually inspected: LevelArt00262_Runtime.png
and LadderRuntime.png; Before.png retained. Controlled player relocation uses the
existing camera snap only in the test, not authored scene/camera changes. Human
artistic approval and Android readability/motion/performance checks remain manual.
Demo saved/open/clean/stopped; previous work and recovery files retained; no stage/
commit/push. Details: TestResults/LevelArt00262_Report.md and EditTests/PlayTests,
Regression, ProtectedBefore/After, AuthoringResult, FinalVerification evidence.

## Sprint 0026.2 revised brief - chunky handmade details (2026-10-25)

Continued the coarse-pixel environment rather than restarting it. The revised
brief explicitly permits environmental detail and simple hard shadows. Added
one shared 256x256 Point/RGBA32/no-mipmap sheet with15 independently code-drawn
modules. All use4x4 native-pixel cells within32PPU sprites, binary alpha and at
most five opaque colors. No gradients, soft lights, microtexture or random noise.

Each of Demo/Testbed/Mission01 receives11 curated wall/background details (NOPE/
FINE signs, electrical cabinets, pipe/valve, fan, shelf, concrete patch/crack,
stain, section number, warning lamp), four small platform/floor marks and four
flat hard shadow bands.57 new SpriteRenderers total; only Transform/Renderer
components, one shared sheet/material, no runtime generation or update workload.
Sparse areas retained. Background sorting keeps actors ahead; surface marks
stay within original surface bounds. Office sign moved left after capture review
showed the ladder obscuring it. Native Unity APIs authored all scene/asset changes.

All815 protected original files remain byte-identical, including all six approved
crate assets/metas and existing coarse environment textures. All3081 original
scene transform/nonvisual snapshot entries match. No gameplay, collision, AI,
spawning, camera, character, weapon, HUD, menu or shop changes. Prior uncommitted
work/recovery files retained; nothing staged, committed or pushed.

Final focused automated results16/16:2 Edit sheet/grid/palette/component/scene
invariant checks,1 actual-Demo Play crate bounds/raycasts/ladder endpoints plus
decoration/capture check,12 existing cover tests,1 existing disabled/destroyed-
ladder gravity-restoration test. Tests repeated after final placement adjustment
where relevant. Compilation successful; final current Unity Console0errors and
0warnings (historical console buffer is separate). Demo saved/open/clean, Play
Mode stopped. No broad historical suite or Android performance claim.

Opening, ladder/mezzanine and office gameplay-camera captures visually inspected.
These are controlled Editor test captures, with combat disabled and test-only
player relocation for later views. Human artistic acceptance, busy-combat contrast,
phone-size readability and Android scrolling/performance remain manual checks.
Full file list, evidence and manual checklist: TestResults/HandmadeDetails_Report.md;
results: HandmadeDetails_Regression.json and Edit/Play/Cover/LadderTests.json;
saved-state proof: HandmadeDetails_FinalVerification.json, Console.json,
ProtectedBefore/After and per-scene Before/After snapshots. Implemented by Codex;
awaiting human artistic and device playtest verification.

## Sprint 0028 — The First Operation (2026-10-25)

Rebuilt Warehouse_Demo in place as five connected, explicitly authored industrial
encounters across x=-28..159: loading yard, storage decks, Samurai pursuit bay,
firework storage and a raised command-office finale. Continuous bounded floor,
six ladders, optional elevated approaches, separate cover and retreat positions,
existing chunky props/hard shadows and five finite ammo/tape supply crates. No
new audio/music system. Testbed and Mission01 remain intact legacy references.

Exact roster:3+3+2+2 basic Larpers,1 Samurai,1 Czech Fireworks Expert and1 Officer
(13 authored occupants, persistent and present before first render; no rear spawn
logic introduced). First guard at-18.5 rather than-16 avoids the initial touch
Jump overlay. Storage guards occupy ground/3.2/6.4 heights; Samurai58 uses the
existing small collider/navigation to reach the3.2-high deck. Expert94.8 starts
near the deck edge so solid geometry does not block the ground approach. Officer
140 stands on a reachable1.6-high dais behind separate cover with retreat space.
Only authored camera bounds changed, including lower bound-4 for readable floor
framing; camera controller and player movement code unchanged.

User explicitly requested a new Officer after inspection found none. Native
flat-pixel cap/uniform modules match Larper12x11/12x17/5x18/6x18 bounds,32PPU,
Point/no mips and existing segmented joint pivots. Postava1 actually inspected;
historical files untouched. Existing expressions/equipment reused. Officer uses
baseline Larper HP, imperfect aim, suppression/cover/reloads; new scoped pistol
18damage/1.4s interval/6rounds/4s reload and .7s visible wind-up. Close approach
within4units requests existing retreat, with4–6s cooldown. New art is provisional,
not artistically approved. GRAPHICS_BIBLE reference index updated accordingly.

Basic Larper reaction is .7s plus0–.12 jitter in this scene only (global .55/.05
prefab values preserved). Scoped fireworks variant: preparation .85->1.05s,
recovery1.05->2.1s, preferred range7->8, damage31.2->26, radius3.2->2.8;
normal fuse .8–1.5->1.1–1.8s, short .25–.45->.65–.9s, short chance18->12%,
long2–3.2->2.2–3.2s. Existing miss/malfunction variation, warning ring, friendly
fire, physical occlusion and two-projectile pool retained. Original prefabs and
weapons unchanged; all variants live under Assets/Data/FirstOperation/.

Opt-in WarehouseMission mode advances spatially through five sections. Ordinary
kills never gate progression; only Officer defeat opens dispatch exit. Persistent
locked-exit feedback is debounced. Existing menu/level-select/shop/results/reward
accounting retained. No checkpoints or injection pickups in Demo. Purchased stock
is not reset on retry; zero stock death ends the attempt through results. New
campaign saves now explicitly start with zero injections; existing saved stock
and migration data preserved. Capacity2 and2s revival protection unchanged.
Legacy checkpoint fallback defaults true; Demo explicitly disables it.

Verification:55/55 focused cases passed:6 mission Play cases (exact supported
roster, actual carbine stopped by deck, persistent actors, six ladder definitions,
controlled camera views, real-motor full route, actual small-Samurai deck landing,
Officer telegraph/offscreen gate, Expert launch toward ground/offscreen gate,
two revivals/failure/retry);12 campaign/shop/UI/physical-exit/payout cases;
9 existing movement/suppression cases;12 cover cases;9 legacy mission cases;
6 Edit layout/art/decoration checks;1 archived art Play collision/ladder case.
Same cases are not double-counted across reruns. Final compilation succeeds;
current Console0errors,11 existing Editor/test API deprecation/analyzer and Unity
AI Toolkit account/network warnings. No broad historical-suite/Android claim.

Tests caught and corrected a new Officer segmented-sprite reference mismatch and
capture-driven framing/placement issues. The scripted traversal probe was changed
from a center ray to full-body anticipation for raised steps; no movement rewrite.
Legacy tests now use the new exit bounds/objectives or preserved reference scene,
wait for the already-committed1.2s ragdoll recovery, and isolate checkpoint rollback
from autonomous AI reacquisition. Snapshot compatibility normalizes only the two
new legacy-default flags; non-default behavior remains detectable/tested.

All591 protected original art/prefab/historical/other-scene files remain byte-
identical. Demo saved/open/clean/stopped,13 valid authored points,0 missing scripts,
0 checkpoints. MainMenu remains first build scene and catalog still routes to
Warehouse_Demo. Prior four Resources performance-test-file deletions preserved;
nothing staged, committed or pushed.

Controlled opening/storage/pursuit/firework/Officer gameplay-camera captures were
inspected. Combat was disabled for composition captures; integration/route tests
use controlled inputs or programmatic defeats. This is not human combat
playtesting.10–15minute cautious pacing, emergent combat fairness/stuck cases,
Officer art approval and actual Android controls/frame rate require hands-on
review. No claim of a polished or device-verified mission.

Full layout/placements, scoped tuning, modified files and manual checklist:
TestResults/Sprint0028/Report.md. Exact results: Regression.json and suite JSON;
saved-state proof: FinalVerification.json, ProtectedCheck.json and Console.json.
Implemented by Codex — awaiting hands-on playtest verification.

## 2026-10-25 — Resumed Level 1 combat and enemy AI hotfix

Continued the actual paused implementation without regenerating First Operation.
User confirmed the documented brief constraints; no additional numerical attacker
cap was provided. Population13->15 (+15.38%), groups[3,4,3,3,2]; opening trio
unchanged. Added one storage Larper(x10/floor0) and command Larper(x129/floor3.2).
Terrain/scenery/layout and protected original art/prefabs/other scenes unchanged.

Scoped Samurai speed7->9.5, reaction.04->.02+jitter.02, recovery.7->.42,
platform sampling.3->.2, jump cooldown1.2->.85. Pursuit resumes during recovery
when the player escapes; strike cooldown/.4s anticipation, HP/damage, small
collider, jump speed15 and existing route/failed-jump validation remain.

Officer weapon asset retained, renamed Desert Eagle:18->42damage,1.4->2.4s
interval,6->7mag,4s reload unchanged, impulse.9->1.5. Reuses compact pistol
sprite/sounds, removes extended magazine; no new dedicated art/audio claimed.
Every shot has1s red laser with final.2s locked direction and no fresh spread
roll. Solid-geometry clipping/offscreen/death/disabled gates remain; one ray only
while winding. Cooldown2.4s +.8s while assigned ally lives within14units, followed
by fresh warning. Cowardly profile; retreat4->6, retry4–6->2.5–4s, preferred
range7->10, cover search7->9, withdrawal6->7, reposition chance.55->.9. Baseline
Larper HP retained. Low barricade exposed a walking-only retreat fallback after
full crouched-cover rejection: Officer now reuses validated hop corridors at
commitment. Original retreat/hop thresholds restored; no navigator/teleport added.

Expert freezes destination/fuse at preparation and completes via its own Update;
player jumping, ordinary LOS loss and missed combat ticks no longer cancel it.
Real damage/death/disabled combat/defeated target/offscreen gates still cancel.
Operation blast26->100maximum, radius2.8/impulse4/fuses/VFX unchanged; configurable
continuous normalized damage100/85/50/20/0 at0/.25/.5/.75/1; threshold0. Center
at/beyond radius causes0 damage. Existing armor, LOS, friendly/self damage,
deduplication and separately weighted impulse preserved. Officer-only cached
Health gate prevents the added optional command ally blocking completion.

61/61 targeted tests pass:4 Edit Mode and57 Play Mode (rules2, layout2, focused
combat5, Level1 integration6, campaign12, movement/suppression9, cover12, legacy
mission9, specialists4). Tests exercise real-motor route, actual small Samurai
deck landing, locked laser/shot match and warning, offscreen/geometry gates,
Officer retreat, committed moving-target throw, radial player/enemy/self/armor/
occlusion/boundary effects and optional-ally exit. Old specialist fixtures updated
for preserved31.2 global damage/always-on friendly fire/effect lifetime and real
enabled-combat camera gate; no global balance change. Initial failed runs and
resolved diagnostics retained. Previous Sprint0028 results are not double-counted.

Compilation clean; final Console ground truth0errors/0warnings (historical tool
buffer retains earlier records).605 protected hashes unchanged; exact geometry
match;15 valid saved scene occupants, assigned Officer ally,0 missing scripts.
Warehouse_Demo saved/open/clean, Play Mode stopped. No staging/commit/push;
all preexisting dirty work and four Resources performance-test deletions preserved.
Original requirements preserved verbatim in SPRINT_RESUME.md, now marked complete.

Controlled Officer laser capture inspected. Human combat pacing/fairness, extended
terrain stuck cases, audio/art presentation and actual Android frame rate/touch
controls require playtesting. No APK/device or human gameplay claim.
Full tuning, files, exact results and manual checklist:
TestResults/Level1CombatHotfix/Report.md and Regression.json.
Implemented by Codex — awaiting hands-on playtest verification.
## 2026-10-25 — Officer miniboss: Desert Eagle, full-view laser and reload

Implemented the separate Officer-only brief. No planned encounter-pacing work:
Warehouse_Demo's groups[3,4,3,3,2], all actor positions, geometry and progression
remain unchanged. Existing dirty work preserved; no staging/commit/push.

Native27x14 Desert Eagle-style pistol and3x5 magazine at32PPU/Point/no mips use
four flat opaque colors, chunky slide/barrel, hollow trigger guard and dark grip.
Existing carrying-hand pivot/joint chain retained. The actual muzzle is the
barrel edge5px above the(6,6) hand pivot, correctly mirrored. Existing Larper/
character art and historical originals unchanged; actual Postava1/current Officer
references inspected. New weapon art remains provisional pending artistic review.

Officer-only32-degree upward recoil/.09 setback recovers over.5s; free arm stays
separate while firing. Existing PistolMagazineReload and articulated exchange
now handle this weapon. Compact mag is visibly removed, physically released and
replaced; generic spare is hidden to prevent duplicate presentation. Existing
WeaponCycle owns atomic ammo transfer after the replacement seats, at2.8s
completion. Empty threshold and partial-mag refill guard keep evasive moves from
refilling the gun early. Damage42->55, magazine remains exactly7, reload4->2.8s
with jitter disabled. Original HP50, cowardly retreat/cover/support-ally behavior,
2.4s weapon interval and2.4(+.8supported) warning cooldown preserved.

Laser warning1->1.15s, final.2s direction lock unchanged. Width.035->.055, opaque
red/no glow. Range covers the camera diagonal plus muzzle offset, independent
of mobile aspect/zoom; one solid-geometry ray still stops at real platforms,
crates and walls. A short beam hitting nearby dais/cover is intentional. Visual
barrel now follows the sampled warning direction throughout tracking, rather
than nominal aim diverging from aim-error laser, and the actual shot follows the
same committed direction. Synchronous shot pose/muzzle feedback remains intact.

60/60 unique targeted cases pass (4 Edit,56 Play): rules/layout4; new Officer3;
existing combat5; Level1 integration6; Larper reload1; weapon/suppression7; player
weapon1; cover12; movement/double-jump9; campaign12. Affected Officer/combat/player
presentation checks rerun after the final alignment correction without counting
duplicates. Tests verify full-view four-direction beams at4:3/16:9/20:9, real
solid clipping, actual seven-shot55damage AI sequence, no empty/reload fire,
no duplicate reload,7-round refill with reserve135->128, fresh post-reload warning,
mirrored hand/recoil/exchange/cancellation and normal defeat. Initial checks led
to removal of duplicate generic magazine rendering and render-frame-synchronized
presentation assertions; source errors/failed final tests are not outstanding.

Compilation clean; final current Console0errors/0warnings (historical buffer
retains earlier records).805 preexisting non-code asset/reference hashes checked:
only Officer.prefab and OfficerPistol.asset changed. Scenes and all other assets/
historical originals identical. Demo open/clean, Play Mode stopped,0 missing
scripts, actual scene Officer references verified. Controlled both-facing recoil/
reload and gameplay-zoom captures inspected; no human gameplay, artistic approval
or Android device/performance claim. Full final values/files/test evidence/manual
checklist: TestResults/OfficerHotfix/Report.md and Regression.json. ART_BIBLE updated.
Implemented by Codex — awaiting hands-on playtest/artistic verification.
## 2026-10-25 — Sprint 0027: vertical combat, physics and gameplay foundations

Preserved the completed Level1/Officer work and current mostly horizontal
15-enemy mission; no layout, encounter-placement, collider or historical-art edits.
Full original request and acceptance criteria retained in
TestResults/Sprint0027/OriginalRequirements.md; configuration/files/manual checks
in Report.md;138/138 unique focused tests pass (122 Play,16 Edit, Regression.json).

Existing patrol hops now validate a complete reachable body arc before launch,
reusing pursuit safety/retry gates and existing stuck replanning. Existing uncertain
vertical gunfire hearing verified without LOS through solid platforms; no AI aim,
reaction-speed or personality boost. Normal Larper movement and Samurai production
prefabs pass obstacle/elevated pursuit, interruption and viewport/memory checks.

Player fall damage uses real upward-facing contacts: safe drop8units,8HP/excess
unit, minimum impact speed12, plus ordinary/double-jump height safety. A deployed
parachute at landing unconditionally bypasses calculation and clears the pending
flight, regardless of height/velocity. Extreme/late chute landings do0HP, with no
delayed damage; actual airborne combat damage remains active. Recovery/revival/
climbing clear tracking. Running down-gesture slide and buffered post-double-jump
landing slide reuse existing motor/posture, with finite.35s decaying momentum.

Player tool slot now selects finite frag/flash/smoke, cap5/type, shop60/50/40,
right-stick aim→13-point10Hz arc→release throw; no one-button throw. Six pooled
projectiles, actual radial/solid-LOS self/friendly damage and bounded impulse,
temporary flash sensory disruption and8s smoke sight obstruction. Smoke never
stops physical bullets or explosions. Purchase/consumption stock persists; saved
legacy slot enum/data and enemy melee remain intact. Reusable fictional mine
prefab has configurable trigger/damage, one-shot activation and no new Level1
placements. Small Point-filtered32PPU prop sprites and dry peel WAV authored
natively; no character sprite redesign.

Cumulative accepted impacts track head/torso/four limbs, configurable80/150/60
thresholds. Existing corpse copies totals and releases existing head/limb joints;
living rigs and torso root remain intact. Existing corpse cap4/lifetime45s,
nonblocking masks, velocity limits and inexpensive sleep retained and tested.
Tape remains200reserve/25HPpersecond/1pointperHP, vulnerable and interruptible.
Existing simple shoulder pivots now provide wrap fallback alongside segmented
solver; small tape band and short peel cue accompany it. Active healing blocks
AttackExecutor before ammo/effects; release/completion/interruption restores fire.

Faintanyl stays Medical-only, cap2 and no random loot/free starting stock; existing
purchased saves preserved. Two HUD pips decrement individually. Controlled shop/
mission test buys2, revives twice in place with exactly2s protection, confirms
vulnerability returns and third fatal event ends attempt.

Officer warning1.15→exactly.5s, final.2s direction lock preserved. Continuous
red geometry-clipped visual-only beam covers intended viewport sightline and
cancels on retreat/reload/offscreen/death. Actual fading tracer magenta with
width.08 versus Larper.04; existing taper/flash/sound remain.55damage,7mag,
2.8reload,2.4weapon interval and2.4(+.8supported) warning cooldown unchanged.
Tests cover visual-only warning (no ammo/noise/shot), actual timed shots/fade,
full-view four-direction/aspect beams, interruption and complete7-shot/reload.

Regression fixture maintenance was necessary for the earlier Level1 redesign:
old cover-root, roster and Samurai platform coordinates were stale. Tests now use
current covers/roster or isolated temporary geometry with production prefabs;
no old geometry restored. Rising-arc/short-retreat assertions observe actual
movement rather than a late net sample. A temporary fixture initially unloaded
the runner scene; stopped/corrected and all6 Samurai cases passed. Initial failed
evidence remains; latest unique results all pass. Final wrapping/HUD correction
rechecked6 affected cases. Controlled captures of grenade HUD/arc/pips, wrapping
and actual Officer tracer inspected; these are not human/device playtests.

Android support and SDK/NDK/JDK are installed; MainMenu first enabled build scene.
No Android phone connected. Development APK build running; outcome will be
recorded below. No staging, commits or pushes. Final gameplay/artistic acceptance
and Android performance/touch/thermal checks remain with the user's playtesting.

Sprint0027 final verification: development Android APK **Succeeded**,839.36s,
51,387,294bytes (~49MiB), at build/Sprint0027/AKUMU2-dev.apk. Official BuildReport
counts972 warnings, primarily existing Sentis/inference shader variants, plus
existing deprecated Find API/optional runtime Pipeline setup. Its3error messages
are Pipeline request/status timeouts during the long native build, not C# or
linker failures. Deferred duplicate callback refused to overwrite the finished
APK; recovered authoritative successful BuildReport and preserved that diagnostic.
AndroidBuild.json/AndroidWarnings.json contain details. No Android phone attached,
so installation, hardware FPS/thermal/touch feel and user gameplay remain untested.

Final compilation succeeds; after archiving build diagnostics, current Console
0errors/0warnings. Warehouse_Demo clean/saved/open, Play Mode stopped,0missing
scripts; all relevant humanoid rigs/skins and new prop/audio references intact.
813preexisting asset/reference hashes checked: only intended player/Larper/
fireworks/Officer prefabs and catalog differ; all scenes/historical originals
unchanged. Geometry and15-enemy counts[3,4,3,3,2] preserved.138/138unique tests
pass (122Play,16Edit);6affected presentation/control cases rerun after last fix.
Working tree intentionally dirty with prior work preserved; nothing staged,
committed or pushed. Implemented by Codex — awaiting playtest verification.

## 2026-10-25 — Sprint 0027.1: Officer, ladders, weapon and Medical cleanup

Scoped Officer accuracy: inherited close/far spread3/40degrees→1.25/4,
aim sampling.18→.06s, turning240→360degrees/s. Officer-only aim now uses actual
muzzle toward last visually observed collider center, retaining crouched offset
without live hidden-target tracking. Reaction.7s, LOS/perception, cowardly evasions,
.5s warning/.2s lock, .08magenta tracer,55damage,7rounds,2.8reload and cadence
unchanged. Controlled seeded10-unit stationary player-size test:6hits/6realshots;
existing commitment/dodging, retreat and full7-shot/reload regressions pass.

Desert Eagle grip brown→charcoal-black; original27x14/32PPU gun/pivot/muzzle
preserved. Magazine3x5→2x4, socket(-2,-3)→(-1,+1)pixels, seated beneath opaque
handle. Extracted/replacement/debris remain on existing animation/physics/cycle.
Native asset references/GUIDs preserved through Editor APIs. Pixel containment
check and both-facing recoil/reload captures inspected; no body/rig redesign.

Actual revised-level ladder baseline reproduced a stall atcenter3.870/feet3.170
against3.200platform: old exit checked.05units early and stopped climbing on a
blocked horizontal corridor. At exact marker, contact skin could still block the
full collider at the ledge. Traverse normally to marker plus configurable.04
clearance, then reuse existing supported/clear horizontal exit. No teleport or
layout edit. All6real ladders pass held-up automatic grounded exits, downward
climbing and post-cooldown no-reattach.9airborne-grab checks and11existing
ladder/touch/combat cases pass; gravity/input ownership/delay preserved.

Pause painted face88x40→52x40, same top-center symbol and124x56invisible touch
area. One visible Medical category, Duct Tape then Faintanyl; no separate tape
or drug tab/duplicates. Tape title exactly Duct Tape. Existing resource enum/IDs,
prices($10/50tape,$75/injection), descriptions, caps and atomic purchase/save
behavior preserved. Actual UI purchases award and persist both resources.

39/39unique focused tests pass (38Play,1Edit), including6fall/parachute and
cumulative detachment checks. Older test inputs now scale to joystick size and
wait for existing reaction state instead of fixed pixel/time assumptions; no
Larper gameplay change. Compilation/current Console clean,0missing scripts,
Warehouse_Demo saved/open and Play Mode stopped. Ragdoll/region code and prefab,
warehouse scenes byte-identical; geometry and15-enemy pacing[3,4,3,3,2] intact.
No detachment frequency, thresholds or exaggerated physics changed.

Files/values/evidence/manual checklist: TestResults/Sprint00271/Report.md and
Regression.json; full brief preserved as OriginalRequirements.md. Native authoring
entry point Sprint00271Setup.Apply(). No new APK build requested/performed; prior
0027APK does not contain0027.1. Real Android touch/feel, Officer dodge balance and
artistic acceptance require hands-on playtesting. No stage, commit or push.

## Grenade resupply / fragmentation / rare arm detachment — 2026-10-26

Existing supply boxes now transfer finite two-per-type grenade stock, respecting
five/type capacity and preserving unaccepted stock. Same interaction/LOS, ammo
and tape behavior; Demo boxes still hold108 conventional rounds. Batched inventory
event immediately invalidates HUD readout and uses existing campaign persistence.
No new pickup or scene changes.

Frag radius3.2→4.1(+28.125%), peak damage85→110.5(+30%); same linear falloff,
geometry protection, friendly/self rules and impulse2. Prewarmed sprite layers:
brief flash, flat red/orange/yellow fireball,8 cosmetic fragments,3 short smoke
puffs. Point-filtered8x8 mask at32PPU; duration.95s. New short synthesized boom
because inspected projectile explosionSound was unassigned. Existing camera owner
adds distance-weighted.08/.16s/range12 shake, never feeding follow smoothing.
One existing occluded combat-noise event per frag; no new AI state. Fixed area
query capacity64→128 accommodates larger radius and existing ragdoll colliders.
Flash6/1.6s and smoke3.5/8s unchanged; frag smoke is cosmetic only.

Cumulative arms60→105(+75%), head80/legs60/torso150 unchanged. Existing single-region
hit accumulation and corpse transfer retained. No CartoonRagdoll physics changes.

31/31 unique focused automated cases passed:24Play Mode,7Edit Mode. Includes
actual Demo box caps/finite partial stock/HUD/save, blast falloff/cover/self-damage,
pooled effects/reuse, flash/smoke, single-arm105 threshold, enemy death/corpse,
existing supplies/throwables/regional damage/ragdoll/corpse-player collision and
weapon ammo/reload rules. Initial crate test assumption72 corrected to actual108;
no production ammo changed. Early unapplied-prefab run canceled before final runs.
Controlled fireball/smoke captures inspected; no human gameplay/device claim.

Both warehouse scenes, ragdoll prefab/source and EnemyCombat.cs hash unchanged
from hotfix start. Warehouse_Demo clean/open, Play Mode stopped, compilation
successful/current Console0errors0warnings,0missing scripts. No APK build;
real Android performance, explosion balance/audio/art feel and observed detachment
frequency require manual playtesting. All values/file list/evidence/checklist:
TestResults/GrenadeResupplyHotfix/Report.md and Regression.json. Existing dirty
work preserved; nothing staged, committed or pushed.

## Sprint 0028 — Operation: The Coward's Bunker — 2026-10-26

Implemented in the existing Unity project (brief's Godot wording treated as an
engine-name error). Two deliverables complete: handmade environment modules and
functional Officer defensive position. Prior grenade hotfix and dirty work kept.

Eleven reusable coarse native modules on one512x512 Point/RGBA32/no-mipmap sheet,
4x4native-pixel cells at32PPU, binary alpha, <=5opaque colors/module: soil floor,
wall panel, scuffed deck, banks, tufts, rocks, sparse trees, scraps, sandbags and
timber roof/posts. Placed static scenery stays subordinate; approved crates and
character sprites untouched. Existing terrain/route, fifteen actors [3,4,3,3,2],
opening trio, platform dimensions, six ladders and mission triggers retained.

Existing cover positions x138/146 on1.6-high dais reused; front1.625x.90625→
1.625x1.125, rear1x1→1.625x1.125. One new solid roof4.5x.375 at(142.75,4.875),
open rear, cosmetic posts/earth, exposed edge x136 and world management joke sign.
All other original terrain/ladder/crate collider bounds validated against baseline.
No scene YAML/GUID edits: native CowardsBunkerSetup authoring and small eval fixes.

Existing OfficerShotTelegraph holds optional scene-only bunker references;
EnemyCombat uses current heard/visual memory, sampled reachable paths, reservations,
ordinary hop, hide/retreat/reload/pressure and stuck recovery.80%post-shot retreat
attempt near bunker; random existing1.2–2.4shides. Authored cover stand-off.65
instead of generic.08 leaves a safe jump run-up; all other slots unchanged.
A bounded1.5ssound-memory investigation after hiding prevents losing engagement
solely while taking cover. No omniscient target or fixed action script.

Actual traces caught two clearance issues: low roof blocked full hop arc, then
close generic slot prevented hopping out. Shorten roof behind front corridor and
provide run-up, keeping collision checks. Final trace shows peek/warning, shot,
return to sandbags/crouch, then emerge and shoot again; no teleport. No Officer
health/damage/accuracy/weapon/reload/laser/tracer tuning or other enemy AI changes.
55damage/7mag/2.8reload/.5swarning/.2slock and magenta heavy tracer remain.

36/36unique focused tests pass:31Play,5Edit. Includes4new bunker integration cases
(real shot obstruction/rear-flank defeat/corpse; repeated peek/fire/cover arrival;
pressure decay/real player crossing; physical grenade roof collision and actual
blast protection/open rear damage),2new asset/layout checks, existing Officer,
ladders, pause/shop, complete ground route/results, Samurai, supplies/grenades/HUD/
save and fall/parachute/double-jump. Initial test JSON reference replaced with
built-in JsonUtility; optional runtime magazine overlay excluded correctly from
environment-sprite check. Fireworks ground-approach test failed once in combined
run, passed isolated and full-suite rerun; no unrelated rebalance applied.

Controlled gameplay-camera captures inspected. No human artistic/balance or
Android-device performance claim; no APK build. Added static modules/one collider,
no environment Update/particles/lights; Officer uses bounded existing tactical
samples. Editor temporarily stalled; checkpoint preserved and work resumed after
reconnection, without forced termination/discarding unsaved work by Codex.

Full files/values/results/manual steps: TestResults/CowardsBunker/Report.md,
Regression.json, OfficerTrace.json and captures. Original brief and prior checkpoint
preserved there; SPRINT_RESUME.md updated. No stage, commit or push. Await human
playtest/artistic verification, especially bunker pressure/escape, visual approval,
fireworks intermittent timing and phone performance.

Final native state: Warehouse_Demo saved/clean/open, outside Play Mode; compilation successful,0missing scripts,0Console errors.8warnings remain (older API/account connectivity notices captured separately); no unrelated warning cleanup. FinalEditor.json/FinalConsole.json record status.

## 2026-10-26 — Sprint 0029: Operation: The Bunker Is Real

Continued the existing Sprint 0028 level without regenerating approved environment
art or discarding prior uncommitted work. Native Editor authoring saved matching
foreground collision on bunker sandbags, roof and two small timber headers. Muted
back-plane posts remain decorative. Officer starts under the roof (x140 →142),
retreat chance .8 →.9; muzzle obstruction requests a reachable peek and prevents
telegraph/shot through cover. Retained55damage,7rounds,2.8sreload,.5swarning,
.2scommit,1.25–4-degree imperfect spread, heavy magenta tracer and pistol art.
Actual traversal revealed/corrected header clearance; committed muzzle checks now
stop at target entry rather than falsely rejecting geometry behind the player.

Close reachable box hops can use a safe .18s backward run-up,3s cooldown, at most
two attempts before existing replanning. No new nav system, floor permissions or
player motor rewrite. Six automatic ladder exits and small-collider Samurai
platform pursuit retained and tested. Shared event hearing accepts nearby enemy
shots and explicit explosions, uncertain memory and offscreen firing restriction
preserved. Occluded hearing .45 →.6; Larper reaction .55 →.7 (+.12jitter), Samurai
.02(+.02) →.25(+.08). Short LOS loss does not restart initial delay. Suppression,
expert commitment, encounter counts and approved scenery are preserved.

Medical now contains exactly Duct Tape and Faintanyl with the exact requested
copy. Four visible categories retained; existing grenades sold under Ammunition.
Faintanyl defaults0, maximum2 purchased charges, automatic .35s revival delay;
charge saved at death, exactly2s central damage protection, then normal failure
when empty. Reservation avoids last-charge game-over race. No pickup/checkpoint
refills. Old cartoon animation reused, two actual syringe icons bind correctly.
Existing hold-heal and three-type aim/preview/release grenade system reused.

Added catalog-configurable vest/helmet tiers. Light body60/.65absorption/1.04reload;
heavy90/.75/1.18 plus36reserve and1grenade carrying cost. Light helmet20/.45/1.02;
heavy35/.65/1.08 plus18reserve cost. Combined heavy reload1.274,378of432 spare rounds
per firearm,4of5grenades/type; bounded minimum capacities. Uncarried purchased stock
is saved instead of discarded. Reload modifiers stay per-player runtime weapon,
including pickups; shared definitions/enemies unchanged. Separate regional head/body
ballistic condition; melee/explosion/environment bypass. No movement penalties.
Small native flat-color32PPU Point helmet overlays follow existing head/corpse;
no ragdoll joints/physics/limb-detachment edits.

Original AKUMU Block font matches existing wordmark block construction, applied via
shared USS to menu/shop/HUD. Medical cards fit landscape together; reserve ammo is
separate below current/capacity, dynamically checked through firing/switching.
Existing joystick sizes/positions, compact pause face and generous touch target
preserved. MainMenu/Medical/Armor/HUD advancing Editor captures inspected; they do
not establish human artistic approval or phone behavior/performance.

Implementation, exact changed-file groups, acceptance coverage and manual checklist:
TestResults/Sprint0029/Report.md. Original brief, native authoring evidence, per-suite
JSON and captures preserved there. Test/build totals and final Editor state follow.

Latest unique regression outcome:154/154 passed (132PlayMode,22EditMode), no skips
or unresolved failures. Includes existing social fear/spacing/near-miss suites,
complete save/menu flow and original mission checkpoint rollback/results. New
sprite bounds assertion comparesXY (renderingZthickness has no2Dcollision role).
Protected hashes all unchanged. Before Android build: Demo saved/clean/open,
outsidePlay,0missing scripts,0actualConsoleerrors/warnings, compilation clean.
Android native async development build is running; final result follows.

Final Android ARM64 IL2CPP Development APK SUCCEEDED: build/Sprint0029/AKUMU2-dev.apk,
67,235,131bytes,595.862s,0build errors.972warnings:970installed inference-package
shader warnings,1obsolete API warning,1disabled Pipeline runtime notice. These
are documented, not silently reported as a warning-free build. FullnativeReport
and compactsummary saved under TestResults/Sprint0029. APK manifest/ARM64library/
three scene data entries inspected; no physical-device launch/performance test.

Final nativeEditor: Warehouse_Demo saved/clean/open, outsidePlay, notcompiling,
0missing scripts. Console0actualerrors,82collapsedwarnings after Android build;
prebuild actualwarnings0.154/154unique tests passed. Protected asset hashes all
unchanged after build. Existing dirty tree preserved,0staged entries, no commit
orpush. Implemented by Codex — awaiting human gameplay/artistic and Android-device
verification. No known unresolved automated failure; manual checklist in Report.md.

## 2026-10-26 — Officer bunker correction: concrete pillbox

Replaced only the old Officer dugout in Warehouse_Demo with a compact concrete
pillbox, preserving the approved0028environment sheet/surroundings. Five static
nontrigger Default-layer concrete collider faces form front lower/upper sections,
rear,roof,base. One.9-unit embrasure at y2.5–3.4 is the only firing opening. Existing
Officer dais supports the shell; base stays within its thickness. Foreground
facade bands and dark interior make occupancy/cover visible. Flat native32PPU
Point art, no mipmaps, no character redesign or new dependencies.

Optional scene anchors on OfficerShotTelegraph select interior peek/hide positions
x141.05/143.25. EnemyCombat reuses motor/perception/states/suppression/reload/timers
for local movement and bounded hiding. Actual barrel rays must cross both faces
of the aperture, including protruding muzzles. Laser and committed shot respect
this gate plus existing occlusion. Original Officer encounters without the scene
anchors keep previous behavior. No damage,health,accuracy,magazine,warning-duration,
player motor or support-radius rebalance.

Controlled tests exposed/fixed arrival tolerance and downward barrel/sill clearance.
A base initially protruded below the old platform; final thickness stays within
it. The older route driver's single-jump policy could not cross the now-closed
shell; it now uses the existing double jump early enough to clear the roof. Local
physical crossing and full Level1-to-Results input traversal pass. Native authoring
and cleanup once exceeded the CLI5s response window; clean saved scene and actual
bounds were read back rather than blindly repeating the mutation. Test fixtures
were updated for a closed rear, concrete surfaces, new Officer occupancy, unchanged
nearby-ally radius and overlapping solid foundations. No gameplay weakening used.

Original brief, changed-file list, captures/traces, per-suite results and limitations:
TestResults/ConcreteBunker/Report.md. Final validation totals/state follow.

### 2026-10-27 — Interrupted pillbox follow-up safely concluded

Recovered actual dirty repository and finished pending verification without
reimplementing completed systems. Previous Play batch completed22/22; resumed
Edit checks9/9. Final focused31/31passed, no pending automated failure. Normal
carbine fire can defeat the Officer through the slit and player input then reaches
Results. Existing fullroute/ladder/Officer/Samurai/expert/genericAI tests pass;
scene/roster/collision-bound/equipment rules pass. Details and latest fixture-only
aggregation: TestResults/ConcreteBunker/Regression.json and Report.md. Earlier
Sprint0029's154case result preserved as historical validation, not misreported as
a fresh rerun. No additional gameplay code changed on this resume.

Final native state: Warehouse_Demo saved/clean/open, outsidePlay, notcompiling,
0missing scripts acrossscene/checkedprefabs,5concrete solidfaces, valid anchors/
sprites. Console0actualerrors/warnings after reviewing and archiving a Unity CLI
`editor status` dispatch error (tooling, not gamecode) and prior timeout diagnostics.
Protected environment/Testbed/ragdoll hashes unchanged. Medical/throwables/armor/
awareness improvements preserved. No0030overhaul, staging, commit orpush.

Remaining checks: human gameplay/artistic and Android-device verification. Prior
Sprint0029developmentAPK was built before this concrete correction; not rebuilt
or device-tested in this focused follow-up. Rebuild before phone playtesting.

## 2026-10-27 — Sprint 0030 implementation

Audited the requested systems against actual0029/concretefollow-up implementation;
kept existing bunker geometry, sound awareness/reaction tuning, local box recovery,
medical/revival, grenade aiming/supply/VFX, armor/helmets, ladders, movement andUI.
No scene regeneration, graphics/UIoverhaul or newAIframework.

Confirmed gaps: Officer had no ballistic protection, and the concrete-branch local
movement bypassed generic close-pressure decisions. Added finite50-point vest,
65%absorption, using existing BodyArmor and ArmorVisual/Armor_0/Armor_1. Health stays
at the normal50;10bodyhits of10damage vs5unprotected yields100body-shotEHP.
Headshots/melee/explosion/environment retain armor bypass. No weapon rebalance.
An initial test assumption100health was corrected against the actual50baseline;
vest calibrated100→50before acceptance rather than making Officer overly durable.

OfficerShotTelegraph now debounces accepted-hit retreat requests (2.5scooldown,
.5srequestexpiry), cancelling currentwarning; existing pillbox motor/state logic
handles physical retreat and subsequent combat. Confirmed close visual-memory
threats cap hiding at.45s; unconfirmed/distant targets don't. No perfect new player
knowledge, added physics queries, managers or global searches. Protection resets
at encounter/reactivation boundaries, not on ordinary hits. Configurable fields
saved natively in Officer prefab and placedDemoactor by Sprint0030Setup.

New four-case controlled suite passed: finite2xbodydurability/reset/head/melee
weakness, hitinterrupt/retreat/resume/debounce, close-vs-unconfirmed hold durations,
player vest/helmet immediate disappearance and nonstacking removal. Existing player
overlay logic already correct, so no rewrite. Controlled Officer capture inspected;
original Postava1reference viewed read-only, reused assets retain originalpivots.
Main regression batch87/87passed; additional corpse/checkpoint/Edit/build checks
continue. Full brief and scope/files/manual limitations: TestResults/Sprint0030/.

Final pre-build regression:118/118unique cases passed (94PlayMode,24EditMode).
Corpse pass-through and originalcheckpoint/replay validated. One older fixture
mixed explicitly supplied$42with automatic random enemyloot ($55); isolated only
that test's EnemyWalletDrop, preserving exact persistence/duplicate assertions.
Actual loot/drop tests passed separately; no gameplaycurrency orpriceschanged.
Final scene/reference scan: Demo saved/clean/open outsidePlay,0missing scripts,
5solidpillboxfaces,1OfficerBodyArmor,50health/50protection, validreusedsprites.
Console0actualerrors/warnings, compilation clean. Protected art/Testbed/ragdoll/
player motor/HUDUSS hashesmatch. Android async Development build_132a6482be7f
queued to build/Sprint0030/AKUMU2-dev.apk; finalresult follows.

Sprint0030final:118/118unique tests passed (94PlayMode,24EditMode); no skips or
unresolved failures. New Android ARM64 IL2CPP Development APK succeeded in157.960s,
0errors,67,235,220bytes: build/Sprint0030/AKUMU2-dev.apk. Manifest/nativeARM64lib/
three scene data files inspected; MainMenu first.972buildwarnings remain:
970installed inference shader warnings,1oldobsoleteAPIwarning,1Pipeline runtime
no-config notice. No unrelated package cleanup. Full/compactbuildreport saved.

FinalEditor: Warehouse_Demo saved/clean/open,outsidePlay,notcompiling,0missing
scripts; prefab/reference scan clean. Console0actualerrors,82collapsedwarnings
afterbuild (0beforebuild). Protected environment/Testbed/ragdoll/player motor/HUD
hashes unchanged. Working tree preserved,0staged; no stage,commit orpush.
Implementation and automated/build validation complete. Human gameplay/artistic
review and Android-device testing remain unverified; no profiler/deviceclaim.
Report/checklist/evidence: TestResults/Sprint0030/Report.md and Regression.json.

## Sprint 0031 — Operation: Death to PowerPoint (2026-10-27)

Implemented shared handmade interface styling across the existing main menu,
settings, level selection, all four shop categories, HUD, pause, death/results,
notifications and enemy speech. Approved title glyphs and backdrop retained.
Hard square panels/buttons, flat limited palette, readable block lettering,
rust/gold selection, pressed/focus/disabled feedback, real item icons and explicit
purchase readiness replace the mixed presentation. Added only three tiny native
Point icons; existing weapon, grenade, medical and armor art reused.

HUD keeps existing joystick positions/dimensions/sensitivity, aim-left placement,
input callbacks and gameplay. It adds persistent filled/empty revival slots with
immediate inventory updates, selected grenade counts, clear empty weapon slots,
low-health/heal feedback. Ammo stays current/capacity with reserve separate.
Existing blind-fire eligibility, capped speech and automatic revival preserved.
Six existing world sign font/material bindings use the existing block font; no
level geometry, environment art, enemy prefab, combat, Officer beam or data balance
was changed for this sprint. Before/after hashes and scoped scene diff preserved.

Verification: new3UI tests pass and capture advancing gameplay/menu frames;
actual GameView resolutions1280x720,1560x720,1600x720,1920x720,1920x1080 plus simulated
safe-area insets pass layout checks. Captures reviewed across all changed screens.
Missing tape icon found in visual review fixed by native stylesheet reimport.
Original wordmark glyph data and rendering method match pre-sprint source.
Final scene/prefab audit reports zero missing scripts, saved Demo, PlayMode stopped.

113 unique tests executed:108passed/5failed (Edit21/21; Play87/92). NewUI3/3passed.
Failures retained: old Sprint005 Larper shots>0 fixed-time assertion (also fails in
isolation); old Sprint0025 three assumptions about free/manual revival conflict
with current empty/automatic behavior, plus a null melee fixture. No unrelated AI,
revival or loadout balance change made to force historical tests green. Exact
Larper timing-failure cause remains unconfirmed. Current flow/shop/medical,
blind-fire, movement/doublejump/cover, ladder, healing, speech cap, ragdoll and
bunker suites pass. Test results are not a claim of human gameplay verification.

Full changed-file list, screenshots, manual acceptance checklist and limitations:
TestResults/Sprint0031/ImplementationReport.md and Validation.md. Phone touch,
performance, smallest-screen reading and human artistic approval remain manual.
No stage, commit or push. Existing dirty work from earlier sprints preserved.

Sprint0031 Android Development APK succeeded with zero build errors;972 warnings
(970package shader warnings, existing obsolete API, missing optional Pipeline runtime
config). NewAPK67,235,220bytes; packaged HandmadeUI markers and changed compiled/scene
assets verified against0030. Final Editor ready/stopped, current Console errors0.
APK: build/Sprint0031/AKUMU2-dev.apk. No physical Android run performed.

## Sprint 0032 — The enemies have brains (unfortunately), 2026-10-27

Audited existing AI before editing; preserved EnemyCombat/LarperBrain, event-driven
CombatNoise, suppression/morale/cover reservations, Samurai local graph and full-body
physical arc validation. No new AI framework, manager, archetype, player mechanics,
level/UI/art/weapon changes. Full original brief/audit: TestResults/Sprint0032.

Recognition now persists through remembered visual interruptions instead of
restarting after1s. Separate .18s camera-entry attack grace prevents first-visible-
frame shots without erasing awareness. Samurai honors its brief recognition before
pursuing while preserving committed flights. Larper already used.7s: retained,
jitter narrowed.12->.08. Samurai.25+.08 unchanged; fireworks.3+.12->.6+.1;
Officer.7+.05->.45+.05, unchanged.5s laser. Visual memory3->4.5s. Existing attack
cadence, damage, imperfect aim, magazine/reload and throw behavior unchanged.

Logical hearing already handled vertical combat and allied gunfire. Fixed smoke
incorrectly attenuating hearing; solid geometry still attenuates, finite range,
4s hearing memory,2u approximation and.25s interval unchanged. Hearing never grants
precise target memory or recursively propagates. Credible real-shot delivery tested.

Blocked grounded effort now observes1.8s (was.9), pauses.6s and limits recovery
attempts to3s cadence. Intentional stationary/airborne states do not count; direction
changes reset observation. Existing replan/reachable cover handles alternatives;
failed retreat destinations are rejected before timeout. Samurai support rays now
ignore ladder triggers and clear stale route targets. No teleport, collider growth,
collision disabling or expanded Larper mobility. Editor-only live inspector exposes
perception, timers, memory, suppression, reservations and rejected routes.

Native prefab API authoring updated four serialized prefab files (Larper,fireworks,
Officer,OperationFireworks); Samurai variants inherit shared fields. Exact diff and
resolved six-prefab tuning preserved. Scenes, UI/input/player/throwables, Officer
warning and fireworks mechanics protected hashes all unchanged. No stage/commit/push.

Final selected regressions92/92passed:78PlayMode/14EditMode, newAI10/10. Includes
small-collider multi-platform physical pursuit with intermediate/final landings,
no snapping, crate/blocked/reopened routes, perception/occlusion/camera fairness,
16-listener bounded noise, suppression/morale/spacing, actual bunker and Officer,
continuous Level1 movement to results, UI/shop and weapon/balance rules.
Fixture corrections retained all assertions: configured bare Larper as spawn does,
used visible successive targets rather than hidden initial target; older timing
checks now honor requested recognition/memory/camera grace; frozen suppression-only
fixture isolates navigation, while real blocked movement is tested separately.

Compile successful, final Console0errors/0warnings, missing scripts0scene/prefabs.
Warehouse_Demo saved/clean/open outsidePlay. Implementation/automated Editor
verification complete; no claim of exhaustive routes, human combat-feel review,
Android hardware profiling or device testing. No new APK built/requested;0031APK
predates0032AI. Old unrelated0031-documented Sprint0025 fixture failures not rerun.
Report/tuning/files/manual checklist: TestResults/Sprint0032/Report.md.
Full tests/limitations: TestResults/Sprint0032/Validation.md and Regression.json.

## Sprint 0033 — Bigger bunker, fewer problems (2026-10-27)

Enlarged the real Demo concrete pillbox4.5x3.175->9x6.35world units (2xwidth,
2xheight,4xouterarea). Clear interior3.75x2.8->8.25x5.85 (~4.6xarea). Original
floor1.6/frontx140 retained, rear149, roof7.825, opening2.5..3.9 (was2.5..3.4).
Reused approved native tiled/Point concrete sprites; no new art/prefab/character
changes. Front full-width masks became floor/ceiling trim, exposing Officer's
standing/crouched silhouette, gun/reload and longer physical retreat.

Hide/spawn143.25->146.5. Reachable firing anchor141.05->141.25 fixes a confirmed
wall-probe/anchor boundary after the longer walk; no AI threshold or architecture
change. Doubled roof exceeds existing player doublejumpheight, so one existing-
system roof-access ladder at139 preserves physical completion. Original terrain,
ladders, exit,15-enemy roster/encounter spacing preserved; player/input/UI and0032
AI source hashes unchanged. Existing ladder count is now7. No movement/weapon/
health/damage/cadence change. Longer routes naturally take longer to walk.

New ladder exposed actual laser clipping at triggers: OfficerShotTelegraph visual
ray now uses fixed-buffer non-trigger query matching real bullets/sight. Corrected
capture shows laser passing ladder and clipping solid cover; original.5s telegraph,
color/width, distinctive projectile and muzzle origin retained. This is the only
runtime source edit. Scene authoring through new Editor/Sprint0033Setup; no YAML edits.

Selected36/36regressionspassed (25PlayMode,11EditMode), including solid shell/window,
Officer peek/retreat/hide/real reload/locked warning, vest, real player ladder/roof
completion to results, all7ladder exits,5actualGameViewresolutions/simulatedcutouts,
shop/HUD and weapon rules. Geometry-dependent tests now use enlarged dimensions/
real anchor/physical route and float tolerance; assertions retained. Initial issue
artifacts archived. Advancing exposed/hidden HUD captures visually inspected.

Finalprebuild references:0missing scene/prefab scripts, five solidfaces, validvest,
MainMenu first enabled scene, Demo saved/open/stopped; Console0errors/4warnings.
Androidreadiness: IL2CPP,ARM64,API26minimum,autoSDK,LandscapeLeft,APK,developmentfalse;
settings unchanged. Native release APK build_af454f66c815 started to
build/Sprint0033/AKUMU2-playtest.apk; outcome follows. No device/hardware claims.
Report, dimensions/files/manualsteps/evidence: TestResults/Sprint0033/Report.md.
No stage/commit/push. Existingdirtywork preserved.

Sprint0033release export succeeded: ARM64 APK39,750,643bytes,
0errors/972warnings (970existing package shaders,1obsoleteAPI,1optionalPipeline
runtimeconfig). Build676.705s. APK: build/Sprint0033/AKUMU2-playtest.apk.
Packaged new roof-ladder scene data and0032AI metadata verified; no oldAPKsubstitution.
FinalEditor ready/stopped/savedDemo, currentConsole0errors/82buildwarnings.
Android configuration preserved; no physical-device run. Protected hashesmatch
except the scoped laser-trigger fix.0staged, no commit/push.

## 2026-10-27 — Adaptive Combat Camera (latest user Sprint 0031 brief)

Implemented after completed UI0031, AI0032 and bunker0033; those changes preserved.
Existing WarehouseCameraFollow remains the sole LateUpdate authority. Root causes:
movement-driven visual facing incorrectly selected attention while retreating;
1/32-unit output rounding introduced steps despite fractional zoom; layered
smoothing lacked a defined settling time; no combat memory or explicit fall lead.

New priority: meaningful weapon-independent aim/fire intent, 1-second combat
memory, committed movement, last meaningful facing. Exact critical damping with
0.5-second lead settling, 0.16 normal/0.11 combat tracking, 0.12 direction commitment,
0.30 aim threshold with hysteresis. Ladder centers over 0.4 seconds with exit debounce.
Vertical settling 0.24 seconds, 0.25-unit dead zone, falling lead up to 1.6 units and
vertical aim bias up to 1.2 units. Comfortable margins 18%; boundary easing 2 units.
Read-only existing-registry pressure within 6 units sampled every 0.15 seconds
contracts lead by at most 45%; no target locking, new physics or scene-wide search.
Continuous transform output; original projection/zoom retained. Input getter is
read-only; controls, player movement, enemy decisions and weapon handling unchanged.
Three camera scene settings authored through native Editor APIs; object IDs and
all non-camera scene objects preserved (legacy false mission default reserialized).

Modified scripts: Camera/WarehouseCameraFollow, Input/PlayerActions,
Enemies/CombatPositionReservations. New AdaptiveCombatCameraTests; expanded existing
LadderCameraHotfixTests. Scenes: Warehouse_Demo, Warehouse_Testbed, Warehouse_Mission01.
Full tuning, root-cause analysis, files and phone checklist in
TestResults/AdaptiveCombatCamera/Report.md; raw evidence and Validation.md alongside.

Selected automated results: 63 run, 61 passed, 2 failed. New camera 9/9, other Play
41/43, Edit 11/11. Two unchanged Sprint013 fixtures conflict with existing camera-entry
attack grace and current LineRenderer count; standalone tested camera has no follow
component. No AI/VFX changes to force old expectations green. Controlled fixture
covers backward aiming, memory, chatter, immediate velocity, close Samurai both
sides, fall/parachute/landing, ladders, phone aspects, 30/60/120 Hz and irregular dt.
Zero managed bytes over 1,000 warmed camera steps; not whole-game profiling.

Final Unity: compilation successful, current Console 0 errors/0 warnings, Demo
saved/open/stopped, no missing inspected scene/prefab scripts. 27 protected files
unchanged. Working tree remains dirty with prior work; nothing staged/committed/pushed.
No new APK exported. Prior0033 APK does not include this camera revision.
Implemented by Codex — awaiting human/device playtest verification. Galaxy A56
60 FPS, frame pacing, real input latency, pixel-phase appearance and comfort during
actual combat remain unverified; no global frame-rate or physics settings changed.

## 2026-10-27 — Android performance pass (latest user Sprint 0032)

Baseline captured before production edits in advancing Warehouse_Demo with all15
original actors:600frames, seeded frame-input pattern,120warmup, CPUProfiler on.
No adb device attached; GalaxyA56 FPS/GPU/memory/thermal results NOT TESTED.
Top relevant Editor categories: LateBehaviourUpdate1.051ms (sprite invalidation
nested0.473ms), RenderPlayModeViewCameras0.948ms, BehaviourUpdate0.331ms.
Repeated fallback/equipped sprite swaps dirtied renderer data despite same final
image. Existing HUD formatted unchanged readouts every0.2seconds; mission clock
formatted at0.15seconds and revive offer everyframe. Pools, sampled AI/buffers,
ragdoll sleep/wake/retirement already implemented; preserved without new frameworks.

Changed CharacterVisualDriver to resolve final weapon sprite once; CutoutRig
skips identical face/magazine sprite setters. No animation/art/pose/timing changes.
WarehouseTouchControls compares value snapshots before formatting and invalidates
on bind/enable; contextual input updates still everyframe, layout/cadence unchanged.
MissionHud caches displayed clock second. New AndroidFramePacing BeforeSceneLoad
initializer requests60FPS on Android only instead of platform-default-1. Existing
optimizedFramePacing=true, URPscale1, AAoff, IL2CPP/ARM64/API26 and0.02physics retained.
No scene/prefab/UIlayout/AI/movement/weapon/camera-controller edits this sprint.

Controlled Editor comparison: average177.52->205.64FPS; median5.424->4.577ms;
p957.242->6.392ms;p998.663->7.955ms;1%low68.92->76.57FPS. Short600frame samples
are noisy and differ in duration; intermediate after sample had worse tails.
Do not infer sustained Android frame pacing or device60FPS. Sprite-change marker
0.473->0.040ms; unchanged HUD15.41->2.26us; mission2.47->0.75us. Warmed repeated
same-frame player pose220.87->11.50us (not normal advancing-animation frame cost).
WholeEditor memory includes retained profiling state and is not Player leak proof.
Full main/render-threadCPU, GPU, Player memory and device metrics unavailable.

Measurement correction: GC.GetAllocatedBytesForCurrentThread always returns0in
this UnityMono runtime, including known8KiBallocation. Earlier camera zero-byte
claim invalid; appended correction to its Report/Validation, and camera test now
checks counter support before allocation assertion. Unsupported writes NOT TESTED;
all camera behavior assertions retained. New profiling/regression tests are Editor
only. Original baseline byte figures preserved as raw evidence but not relied on.

Selected tests67/67pass (56Play,11Edit), including currentcamera9, newHUD/equipment2,
benchmark1, existingUI/shop3, AI10, actualwarehouse6, grenade4, reactiveragdoll5,
medical/equipment4, combat/muzzle6, ladder1, corpse/reload5, Editrules11. Initial new
fixture expected equipped sidearm; corrected setup equips BackupPistol explicitly.
No production fix needed. Historical Sprint013 failures not rerun or claimed fixed.

Protected audit124original source/settings/scenes:120unchanged; only4existing
runtime scripts above changed, plus new Android initializer. Prior dirty work
preserved;0staged/no commit/no push. Native finalreferences:0missing inspected
scene/prefab scripts, Demo open/saved/stopped, currentConsole0errors/0warnings.
Final camera test-only rerun and finalEditor audit recorded in evidence.

Complete diagnosis/files/benchmark caveats/device build+15minute profiling protocol/
nextthree opportunities: TestResults/AndroidPerformance/Report.md. Raw results,
ProtectedAfter.json and allocation diagnostic alongside. No APKexport in this pass;
old0033APK predates both camera and performance work. Implemented and Editor-tested;
A56 sustained60FPS acceptance remains pending physical-device verification.

Final audit update: camera counter-correction rerun9/9passed; total remains67/67.
Demo open/saved/stopped,0missing references, compilation successful and0Console
errors. CurrentConsole32warnings are FMOD host output-device initialization failures
(output remains suspended), not script errors. Older API-obsolescence/account
warnings also exist in historical buffer. Host audio audition remains unverified;
no audio device/project configuration or gameplay changes made to hide warnings.
See TestResults/AndroidPerformance/FinalWarnings.json. No APKexport/deviceclaim.

## 2026-10-27 — Arsenal expansion (latest user Sprint 0033 brief)

Implemented following adaptivecamera/Androidperformance, preserving earlier0033
bunker work. Audit found playerBackupPistol/Carbine/automaticGlock plus enemyOfficer
Eagle; added separate playerEagle, SMG, shotgun, LMG, precisionSniper. Original
weapon assets/art/stats, shop offers/prices/ownership, enemyAI, levelcontents,
movement/physics/camera/control layout preserved. No new price/economy balancing.

SharedWeaponDefinition adds modes/pellets/spread+bloom/falloff/presentation controls.
AttackExecutor reuses hit buffers/nearestcollider logic perpellet, one magazine
round/trigger/firingcue. Actual segments feed existing suppression. Playertracer
ring6->16; no physicalbullets/per-shot InstantiateDestroy. Stock combat effects/audio
pools retained. Shotgunhit marker uses actualdamagedpellet point. Olddefaults preserve
legacyweapons; no redundant infantry/marksman rifle or movementweight penalties.

Eightgunroster stats (damage/interval/mag/reload/range): Backup14/.2/8/1.5/22;
playerEagle55/.65/7/2.8/28; SMG9/.065/30/1.8/20; Carbine20/.2/24/1.5/22;
shotgun7x8/.85/6/2.6/16; LMG14/.11/75/4.2/26; Sniper90/1/5/2.6/45;
existingGlock5/.07/27/4/28. SMGspread+/-5+bloom4, falloff6->20to35%;
shotgun24degfan, falloff4->16to15%; LMG+/-2+bloom10, falloff12->26to65%.
Bloom recovers lazily usingelapsedtime, .35*maxdegrees/sec. Newweaponpitch/flash/
recoildata distinguish feedback; reuseexistingaudio. No claim ofauditioned audio.

Snipermeaningfulrightstickhold>0.30aims cyanlaser, validreleasefiresonce, centerthen
liftcancels. DesktopRMBhold/release. Automatic1srecovery/.35srecoil; no newbolt/fire
button. Laser reusesoneLineRenderer/sharedmaterial andsampled10Hzgeometry preview.
Widepale-blue shottracer differsfromOfficerlaser/shot. Existingcamera observesaim;
no followchange. UITKPointerUp distinguishedfromcancel/captureloss; death/pause/
reload/switch/revivalcancelpendingrelease. Exiting sniper clears rightaim/fire only,
preserving leftmovement and preventing pistol/grenade carryover. Physicaldesktop
heldinputs require neutral aftercancel. Release keeps peekpose through shotframe.
Custommuzzlehelper respects crouchceiling before coverraising. No bodyart/rigpivots
or colliderdimensions changed. HUD caches CYCLING/RELEASETOFIRE with existingreadouts.

Editor/Developmentonly ArsenalTestBench: menuDEVentry saves/destroysSession, enters
standaloneTestbed, reusableguninstances andexplicitrefill180reserve, no campaign
writes/grants. Exitreloadsnormalprofile. Testbedappended tobuildlist, MainMenustays
first. Normalrelease omitslabcode/entry; scene/resources packagedbutnormalflow has
no labaccess. Newweaponacquisition/costs intentionallydeferredSprint0034.
NativeArsenalSetup creates5defs+4pointfiltered32PPU sprite/texturepairs+catalog8refs.
OldDEblackgrip/seatedmag artreusedunmodified. Removed inheritedpistolmags fromnew
longguns aftercapture review. Newartawaits humanartistic/playtest review.

Selected72/72passed (59Play,13Edit). Newchecks all8gunsmoving/jumping/crouched,
reload/interrupt/switch/armorscaling/conservation, all4enemyactualhitboxes,
shotgunnooverlappingcolliderduplicates, snipersafety/recovery/range/reuse/realrevive,
stockedgrenade-switchsafety whilemoving, actuallabload/equip/refill/exitwith identical
profilebytes. Existingweapon/muzzle, UI/shop, AI, camera, mission/roofroute, ragdoll,
grenade/medicalsupplies tests passed. Controlled360triggers/1080segments keep17line
baseline bounded; notA56FPS/zeroGC. Mono allocationcounterknownunsupported. PNG/state
captures advancing inspected. Initialfixtures fixedclock increments/corpseisolation;
labtest caughtmissingbuildscene. A testdiagnostic dependency removed, no packagesadded.

Prebuildnativeaudit: Demoopen/saved/stopped,0missing inspectedscene/prefabscripts,
compilesuccess, currentConsole0errors/0warnings. Protected423originalfiles:
412unchanged,11scoped edits (9runtime/playerprefab/buildlist).0staged/no commit/push.
Report/stats/controls/files/checklists: TestResults/Arsenal/Report.md; validation/raw
results alongside. Originalbrief copied there and resumecheckpoint updated.

AndroidDevelopmentexport build_b5950726ee99 running to
build/Arsenal/AKUMU2-arsenal-dev.apk (Developmentonly; noDeepProfile/AutoRun).
Outcome follows. No adbdeviceattached; A56FPS/multitouch/thermals/input/audio and
humanweaponfeel remain unverified. HostFMODoutput-device warnings previouslyoccurred.

## Context restoration — focused corrections and export recovery

Recovered activeSprint0033 fromactualcheckpoint/tests/changes; no restart. FirstAPK
exportCancelled690.332s with genericTaskCanceledException, noAPK; exactjson archived.
LatestLMG100loaded/200testreserve/8sreload applied natively (was75/180/4.2).
DeveloperReservecfg onlycontrols isolatedlab, no campaignammo/economy grant.
NewDevelopmentFrameOverlay: fixed120samples, unscaledFPS/mean/P95/P99,1Hztextupdate,
no per-framecollections/sorts, unchangedcamera/input/framecaps. Editor/Devonly.
FrameStatsEdit testpassed; arsenalmovement8/lab1updatedchecks passed. Existing
performancefixture exposed managedSpritewrapper referencecomparison afterimports;
verified nativeidentity ratherthanrewritinggraphics, testasserts GetEntityId now.
No futuregrenadepool/P90/AA12/Glock19/secondaryslot/economy/campaignfeatures started.
CurrentfocusedUI/camera/identitytest rerun beforeincrementalDevelopmentAPKretry.

Restoration verification completed: 73/73 selected checks pass (59 Play,14 Edit),
including focused camera9/HUD3/performance2/arsenal8/lab1/rules4/frameStats1 reruns.
Native prebuild audit clean: Demo saved/open/stopped; no missing scene/inspected
prefab scripts; current Console zero errors/warnings. Protected423 original files:
412 unchanged, same eleven scoped runtime/prefab/build-list changes. Retry export
build_369cf4a48eb7 after valid dry run; no source/asset changes during export.

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

## Sprint 0034 — Economy, Shop & Progression (2026-10-28)

Implemented the attached Sprint0034 brief in the existing catalog/session/store/UI,
not a parallel economy. Full brief/report/prices/simulations/evidence:
TestResults/Economy/Brief.md, Report.md, Prices.csv, BalanceSimulation.txt.
Native EconomySetup.Apply authored DemoCatalog only. No scene/prefab/art/GUID edits.

Level1 native audit:15 actors drop wallets,100% chance,$5-20 each. Expected full
wallet collection$187.50. Completion reward$250; failure/abandon25% wallets capped$50.
New profiles$100; existing balance/ownership/ammo/armor retained. Basic pistol now
free starter with five-magazine total floor(40 rounds) per deployment; selected
empty guns fall back to it. No premium ammo grant. Purchases include one magazine.
Eight existing guns available; permanent ownership/no duplicate repurchase; per-gun
ammo packs/caps/prices. Whole-pack fit required; existing crates honor active gun cap.

Armor/helmet ownership, condition, equip/remove/repair persist using existing armor
rules; proportional bounded reserve penalty. No movement penalty/weapon balance
changes. Medical remains tape+Faintanyl exclusively, exact description retained;
revive max2/two-second invulnerability unchanged, no free level replacements.
Wallet feedback adds amount and run total; results separate wallets/reward/banked.
Atomic save failure rolls back purchases, equipment, deployment and settlement.
Run IDs prevent duplicate payouts. Old version1 profiles migrate without resetting.

During mission, disk snapshot conservatively reserves carried stock: forced process
termination forfeits carried supplies/condition instead of restoring spent items;
banked cash, uncarried overflow and wallet claim journal remain. Normal results,
pause-menu return and normal quit return unused stock. Background/resume same
process preserves live inventory. Policy stated in shop; no per-shot/per-frame IO.

Regression checks exposed existing PlayerCombat healing predicate blocking reload
priority. Minimal CanOperate/CanUse split restores reload priority while still
blocking firing during healing; no damage/rate/reload tuning or sniper safety change.
Legacy tests adapted for intentional prices/starter/payout/caps/reservation, current
visible empty revive pips, exact Faintanyl text and float epsilon. None removed.

77/77 selected automated checks pass:63Play/14Edit. Includes new10 economy Play +3
rules/scenarios, existing12 campaign flow,9wallet/healing,4grenade/frag/ragdoll,
4equipment,1revive,3UI,8arsenal,1lab,2performance,9camera,4progress,7combat rules.
Shared shop/result captures inspected. Tests use temp profiles, not real save path.
Current native Console0errors/0warnings; Demo saved/open/stopped, no missing inspected
scripts. Scope hashes156 original files:146 unchanged,10 scoped catalog/runtime edits.
No new per-frame searches/physics work, no scene/camera/AI/movement/UI-layout changes.

Android Development export build_2b1271aa2053 now running after valid dry run to
build/Economy/AKUMU2-sprint0034-dev.apk. Final outcome follows. Do not interpret the
old Arsenal APK as containing Sprint0034. No source/asset edits during export.
No stage/commit/push. A56 real FPS/frame-time tails/thermals/touch and economy feel
remain device/human verification, distinct from passing tests and controlled captures.

Sprint0034 final Android pause refinement: pause-menu and Android background callbacks
secure exact unused inventory on disk. Resume re-reserves before controls/simulation
continue; failed reservation keeps pause active. GameSession's generic background
save defers to the bridge so callback order cannot overwrite the safe snapshot.
A killed paused/backgrounded app retains unused supplies; only an unannounced kill
while combat is active conservatively forfeits carried stock. No per-frame/shot IO.
Added controlled background/pause/resume/save-failure regression. Existing flow12,
equipment4 and lab1 reruns pass. One test-start socket hang-up was transient;
Editor remained responsive, raw error archived, new11 economy suite rerun underway.
First Development APK succeeded120.463s/0errors/976warnings; a final rebuild will
include this pause refinement and clarified shop wording. Do not use old APK hash
as final. Raw first report preserved in BuildBeforePauseRefinement.json.

Native ammo budget audit: Level1 has13x50HP +60HP +70HP targets; five existing
108-round boxes at2/38/73/112/132 (540 finite). Ideal unarmored hit lower bounds:
pistol62/SMG93/carbine46/DE17/sniper15, before misses/armor/range. Forty basic rounds
are a start safety floor; finite early/later supplies support recovery. No box
placement/content changed. Price scenarios explicitly assume existing resupply.
Final pause-aware11 economy tests pass; sharedUI3 rerun passes; selected total78/78
(64Play/14Edit). Final APK export build_7641ac5dff71 running, no source edits.

## Sprint0034 final handoff — 2026-10-28

Final implementation including safe pause/background/resume:78/78 selected tests
passed (64Play Mode,14Edit Mode); raw JSON and ValidationSummary.json. New economy
suite11/11 covers restart migration, ownership, whole-pack fit, zero-money safety,
failed-save rollback, armor condition, caps, resupply, active interruption, safe
background/pause/resume and blocked resume on write failure, and completion/shop
replay. Controlled UI captures inspected; no human or device test claim.

Final Android Development build_7641ac5dff71 Succeeded in94.615s, zero errors,
six warnings (existing obsolete APIs/development directives and disabled remote
Pipeline config). Earlier976-warning build report is archived separately; shader
cache avoided repeated shader warnings in the final incremental build. Current
Console zero errors/six warnings, compilation clean. FinalBuildWarnings.json lists
exact messages. No unrelated warning cleanup/package changes were attempted.

Fresh APK build/Economy/AKUMU2-sprint0034-dev.apk:67,236,575bytes (about64.1MiB).
SHA256 faafcb6b12ff4c5817dc83fd5ef881f587eb32cdae536077b4543fa31222a86d.
ARM64, economy fields/armor records/reserve/pause-save metadata and existing frame
overlay packaged; DemoCatalog asset included. Warehouse_Demo saved/open/stopped,
Editor ready; no missing inspected scene/prefab scripts. MainMenu remains first.

156 protected pre-sprint originals checked:146 unchanged, ten scoped runtime/catalog
changes. Git working tree:78 modified tracked,4 pre-existing deleted test resource
files,169 untracked entries overall; zero staged. Existing dirty work preserved;
no stage, commit or push. Scope lists/report identify this sprint's files rather
than attributing the entire working tree to this sprint.

Remaining: actual Galaxy A56 install/playtest for60FPS/frame-time tails/thermals,
multitouch/safe-area/scroll comfort, sound, weapon consumption and economy pacing.
No attached adb device. Analytical simulations are not player telemetry. Crates
retain premium resupply behavior; revisit consumption/prices only after playtests.
Safe pause/background preserves unused supplies; an unannounced kill during active
combat may forfeit carried supplies while preserving banked cash/ownership/overflow.
No Level2/new weapons/camera/AI/graphics overhaul started.

## Sprint 0035 — Level 1 combat/gameplay polish — 2026-10-28

Inspected prior optimization/economy work and dirty checkout. Exact brief preserved
in TestResults/Sprint0035/Brief.md; Report.md lists this sprint's scoped files and
limits. No stage/commit/push; prior changes preserved.

Removed two supplemental guards: exact10Larpers+Samurai+fireworks+Officer, original
horizontal route/cover/resupply retained. Opening live test proves one authorized
visible shooter and no other enemy shots. Existing .7-.82s Larper and .25-.33s
Samurai recognition, suppression, hearing uncertainty, bounded recovery and
viewport fire gate pass without runtime rewrites.

Bunker solid faces/slit/cover/hide/peek were working; sealed shell lacked egress.
Native Editor API creates rear doorway/exit anchor using existing wall sprite and
collider; confirmed rear flank draws Officer out by real movement. Front slit
checks, .5s laser lock, retreat/reload/damage/tracer preserved. Physical exit test,
bunker damage/defeat/roof passage and laser tests pass.

Basic pistol8=>15rounds; removed wrongly inherited ExtendedMagazine overlay from
BackupPistol asset, retaining existing ordinary sprite/grip. Ammo pack15 and five
starter magazines75rounds. No weapon balance/control changes beyond requested cap.
MissionHud now places mission card beneath actual vitals via GeometryChangedEvent,
fixing inventory-panel overlap at tested landscape/wide sizes.

Development-only catalog setting grants max(balance,10000) once with saved revision35,
atomic rollback on write failure. Release compile branch cannot grant. Actual
persistent profile2166=>10000, revision35 verified on disk; no purchase on actual
profile. Isolated existing-save purchase/restart/mission tests verify deductions
and no duplicate grant. Initial automatic review rejection resolved with explicit
section8 authorization. Normal save .bak retained.

116/116 selected checks pass:111Play/5Edit. Earlier failures were old fixture/price/
roster assumptions, plus actual HUD overlap; no test deleted. Tests now configure
legacy manual revival/melee explicitly, current stock rules retained. Raw captured
runs and final summary are in Sprint0035. Controlled playability routes/scripted
combat checks are not unassisted human gameplay. Native APK export underway;
final result follows. No device attached by adb; physical A56 performance, thermals,
multitouch comfort, complete combat feel and doorway artistic acceptance remain.

Sprint0035 final Android Development build_3c27a20026c6 Succeeded488.978s,
0errors/977warnings (970Sentis package shaders,6existing API/config/development,
1new UAC0009 development-guard deprecation). APK67,236,575bytes/64.1MiB,
SHA256 13a770232c57a260202006cfd9633d40641f71f40870ffce57ea361c770f780c.
Full BuildStatus/ValidationSummary/ApkVerification in TestResults/Sprint0035.
116/116selected checks (111Play/5Edit); fresh real MainMenu/shop capture confirms
10000usable credits, revision35, no purchases on actual profile.

Remaining Editor incident: final Play Mode stop after shop capture triggered native
fatal audio ADTM realtime-thread/mix deadlock dialog. Cause not diagnosed; no
Android reproduction/verification claimed. Restarted saved project and retained
Assets/_Recovery/0 (3).unity without applying it. Final Editor ready/stopped,
Warehouse_Demo open/clean,13actors,front slit/rear exit bound,0missing scripts;
actual save10000 after restart. Console3Pipeline startup command/timeout errors
plus1warning, compilation clean. No crash report submitted. No stage/commit/push.

## Sprint 0036 — UI, HUD, shop presentation — 2026-10-28

Full brief/report/captures/raw tests: TestResults/Sprint0036. Preserved dirty prior
work; no stage/commit/push. UI Toolkit retained; no Canvas migration, new textures,
shader/material pipeline, AI/economy/weapon/input/scene/prefab overhaul.

Refined shared handmade warehouse noticeboard system: enlarged original wordmark,
approved crate sprite reuse, dark teal panels/cream signs/ochre and rust accents,
square heavy lower edges, immediate pressed/focus states, consistent settings,
pause/results/revival treatment. Shop has five categories (separate throwables),
card previews/status, prominent wallet and real weapon facts. Medical membership,
exact Faintanyl text, armor condition/trade-offs and handlers preserved. HUD track,
frames and speech cards restyled; empty revival slots no longer show injection art.
Compact mission detail scrolls only on overflow. Actual visible joystick dimensions
and positions unchanged. No new particle effects; existing feedback retained.

Results inspection caught speech overlaying statistics. Fixed through the existing
max-two-bubble registry: terminal HUD blocks/dismisses speech, disable/reset restores.
CSS-only fix was insufficient across separate UIDocuments; final capture verified.
100/100 selected checks pass (97Play/3Edit), TestRuns.json. New2presentation tests
capture menu/settings/all categories/HUD/pause/victory/failure, five aspect-ratio
layouts, stock/purchase/restart and empty-pip/terminal suppression. Scripted terminal
captures are not organic mission victories. Full existing flow/economy/weapon/input/
traversal/parachute/medical/AI/bunker/suppression suites run; no human/device claim.
Cached-HUD baseline2/2 and post2/2 checks pass; no actual CPU/GPU/GC/FPS matrix.

Actual test save read-only audit remains10000; prior revision35 grant unchanged,
new/old/rich/disabled/failed-write/purchase/restart tests pass. No actual-profile
purchase or currency reset. User confirms three music tracks not uploaded; music
integration deferred. Existing audio effects retained; prior native Editor ADTM
stop-time fatal remains undiagnosed. Android build_7cc902915c2c running to
build/Sprint0036/AKUMU2-sprint0036-dev.apk; final outcome follows. Physical A56
performance, multitouch/safe-area comfort, audio and human art acceptance remain.

Sprint0036 final Android Development build_7cc902915c2c Succeeded124.941s,
0errors/977warnings(970Sentis package shader+7API/config/development). ARM64 APK
build/Sprint0036/AKUMU2-sprint0036-dev.apk,67,236,575bytes;SHA256
5b3d9b4099571648cd6a478f193fe3a0bbf274d817cc411af3311d44877eda13.
BuildStatus/ApkVerification/ValidationSummary archived. Final Editor ready/stopped,
Warehouse_Demo open/clean; compilation clean,Console0errors/87warnings. Real save
10000/revision35 confirmed read-only after build. No source/asset edits during
export. No Windows Player build or physical Android test/performance matrix.
Music explicitly deferred by user pending upload. Prior ADTM native fatal remains
undiagnosed. 100/100selected checks, controlled captures inspected; no human
artistic/full-combat acceptance claim. Nothing staged/committed/pushed.

## Urgent wallet release hotfix — 2026-10-28

Confirmed defect: GameSession.ApplyDevelopmentCredits was compiled out under
!UNITY_EDITOR&&!DEVELOPMENT_BUILD. Release Android clean saves therefore use
DemoCatalog serialized startingMoney100; clearing data cannot fix it. Catalog's
C# default1000 does not override Inspector100. EconomySetup also authors100; no
other runtime reset to100 found. GameSession.Data.balance is the single spendable
wallet for UI/CanBuy/Buy. One catalog, MainMenu flow binding verified. Exact APK
on disconnected phone not inspected; do not assert its provenance/build flags.

Per new explicit user direction, playtest floor now applies in release too and
at every initialization, after load/normalize/starter/migration/interruption
settlement, before UI draw. Max(balance,10000), not additive; revision35 no longer
blocks replenishment. Purchases still deduct normally; relaunch restores minimum.
Atomic failure rollback/read-only corruption protection retained. No economy
prices/startingMoney/rewards changed. Native catalog enabled floor+new diagnostic
switch. Build tag wallet-release-floor-20261028-r1; temporary logs identify
GameSession NewProgress/load/final/duplicate/atomic transaction callers and buildGUID.
Disable developmentPlaytestCredits/playtestWalletDiagnostics before final shipping.

30unique selected Play checks pass latest:wallet5/economy11/UI2/flow12. One unrelated
physical gap-jump initialfailure passed isolated rerun unchanged; raw retained.
Tests adapted for new relaunch-floor semantics; final-economy snapshots opt out.
Searches, assignment trace, catalog/defines audit and report:TestResults/WalletHotfix.
ADB zero connected devices: no Android installation/launch/purchase verification.
Release build_a43a5d61425c running without Development option to
build/WalletHotfix/AKUMU2-wallet-release-20261028-r1.apk. Final result follows.
No source/asset edits during export; no stage/commit/push, dirty prior work preserved.

Wallet hotfix final RELEASE build_a43a5d61425c Succeeded143.806s,0errors/974warnings.
APK build/WalletHotfix/AKUMU2-wallet-release-20261028-r1.apk,39,815,271bytes/38MiB;
SHA256 5fbe6b8aefb7a4ecc096148462f5ab127257e34da4fad0d46ad3065fbe4ccce8.
Aapt nondebuggableARM64, optionsomitDevelopment and EditorDevelopmentfalse;
generated releaseC++ includesMax10000/atomicCommit; APK uniquehotfixtag/diagnostic
metadata and packedDemoCatalog verified. Version0.12/code1 unchanged: usehash/tag.
33unique latestchecks pass30Play/3Edit; rawinitialjumpfailure kept, rerunpassed.
Editorstopped/compileclean; reallocalprofile10000, bothcatalogswitchestrue.
No phone attached: APK NOT installed/launched/device-tested. Do not claimphysical
Android success. Preserve data via normalinstall/update; no needclearappdata.
Disable temporaryfloor and diagnostic switches before finalshipping. No stage/commit/push.

## Expanded Sprint0034 reload/economy implementation — 2026-10-28

New full brief:TestResults/ReloadEconomy/Brief.md, not the older economy-only brief.
Preserved prior0035/0036/releasewallet work. SharedWeaponCycle now distinguishes
atomic magazine transfer from individually loaded shells; only shotgun opts in.
SMG30/1.8s uses existing physical-mag exchange rig and native scoped body/mag
assets; integrated bodymag removed in variant, original art retained as shop icon.
Shotgun6 uses2.6/6=.43333s shell steps scaled by armor, each transfers1reserve=>1loaded.
Can fire to cancel with loaded shells; completed insertions retained through switch,
death, healing, snapshot/resume. Tick sole transfer authority, bounded catchup.
Valid tape healing cancels reload; damage cooldown/grounded rules unchanged.
HUD ammo-change event invalidates cached readout for nextUIframe, safely unsubscribed
on disable/rebind; no HUD redesign. Complete weapon Icon retained for detachedbody.

66selected tests pass18Edit/48Play. New11rules+4real-clockPlay cover tactical/empty/
lowreserve/full, repeatedrequests, one/multiple shell interrupts, snapshot/resume,
movement/jump/crouch/facing, switch/death/heal, bounded renderer count, actual HUD3/6.
Existing arsenal8, economy11, wallet5, flow12, cachedHUD2, healing1, bunker5,
arsenalrules4/economyrules3 pass. Old all-at-once shotgun assertions updated to
full-chain timing, healingfixture waits existing.8s damage lockout. No testsdeleted.
PriceAmmoAudit uses actual8guns:idealunarmored50HPcost pistol1.33/carbine2.25/
Buzzbox4.44/SMG3/shotgun4/DE6/LMG2.60/sniper10. Idealpellet/range assumptions,
not playertelemetry. No obvious new price inconsistency requiring speculative
retune: existing coherent provisionalprices/caps/rewards retained, no finalbalance.
Basicpistol15/fivemags75; heavycapacities sane, separateweaponstocks unchanged.

Actual local disk balance8720 at audit; no direct-profile test purchases/resets.
Existing release-enabled playtest floor remains: next normal initialization
max(balance,10000), no stacking on shop-open/purchase; isolated wallettests pass.
Do not revert urgentwalletfix. No adbdevice; no physicalAndroidperformance claim.
Release build_832d92bfda0d exporting to build/ReloadEconomy/AKUMU2-reload-repair-release.apk;
final buildresult follows. No source/asset changes during export. No stage/commit/push.

Expanded0034 reload repair final RELEASE build_832d92bfda0d Succeeded128.494s,
0errors/974warnings. APK build/ReloadEconomy/AKUMU2-reload-repair-release.apk,
39,817,203bytes (~38MiB);SHA256
98a13f846658f116d8ec897582344c4e1407c90a49f06b3717b246aa1d128398.
AaptARM64/nondebuggable; metadata shell-mode/PoseReloadShell/ammo-event/wallettag
verified. BuildStatus/ApkVerification/ValidationSummary inTestResults/ReloadEconomy.
66/66selected tests pass18Edit/48Play. FinalEditor ready/stopped,Demoopen/clean,
0missing scripts,Console0errors/84warnings. No device install/playtest/performance
claim; controlled reload captures inspected, humanartreview pending; priorADTM
nativeaudioincident remains undiagnosed. No source/asset edits during export.
No stage/commit/push; previous dirty work preserved.

## 2026-10-28 — expanded Sprint0035 combat reliability brief

Evidence and exact modified-file inventory: TestResults/CombatReliability/Report.md.
LMG's original integrated6x6container now uses shared cached physical exchange;
DE Officer-inherited2x4mag replaced for player only with6x7black fitting sprite.
100/8s LMG,7/2.8s DE,5/2.6s sniper, weapon prices/damage/controls preserved.
No ammo arithmetic bug reproduced in shared atomic reload. Real shared issue:
manual Reload used CanOperate during healing; now uses CanUse. Sniper held-aim
cancellation/recovery/manual completion pass. SMG/shotgun regressions pass.
Throwables retain crouched cover, common swept origin for preview/release,
bounded existing10Hz trajectory, short.24s damageable throw exposure. All types,
both directions, ceiling refusal, cancel, three actual storage heights pass.
Wallets were static triggers spawned unchecked at corpse offset, root-position
radius/LOS included smoke, actor destruction erased uncollected reward. Fixed
supported placement/crouched clearance/height preference, two delayed checks,
collider-distance collection/solid-only LOS, scene-owned lifetime. Exact once
earnings and mission settlement preserved. Actual bunker/boundary/deck sites pass.
Storage-only authoring: three cloned approved crates; perception18/16/14 and
visible-entry reaction.30/.55/.85 on existing three actors; no roster/damage/global
AI/camera/traversal edits. Existing continuous real-movement route reaches results.
Urgent release wallet floor remains enabled and unchanged, max(balance,10000)
after load/settlement, no additive grant, all inventory/progression preserved.
106unique selected checks:105pass/1fail (23Editpass,82of83Playpass). Open older
FirstOperation opening test: second warmup guard becomes visible and fires within
3s; repeated atwide and16:9 views. Opening actors/AI/camera unchanged. Original
assertion restored, no production workaround; do not claim full verification.
Earlier fixture/import failures and wallet lower-floor preference were repaired;
raw runs retained. New controlled captures inspected, no human acceptance claim.
Release build_aa6a88e04ff0 underway tobuild/CombatReliability/AKUMU2-combat-reliability-release.apk.
adb devices checked with normal user access: empty. No device install/performance
verification. No code/native asset changes during export. No stage/commit/push.

Expanded0035 final release build_aa6a88e04ff0 Succeeded139.479s,0errors/974warnings.
APK build/CombatReliability/AKUMU2-combat-reliability-release.apk39,818,983bytes;
SHA25667d10f53f74bded5b92757934672896afd9d6dba9e915c5ebe948682435462d9.
Aapt ARM64/nondebuggable, new packed weapon assets/scene/catalog, generated IL2CPP
and APK method metadata verified. adb empty; no installation or Android test.
Final Editor ready/stopped, Demo open/clean,0missing scripts; code CRLF-aware
whitespace check passes. Full raw tests include the one unresolved opening test.

Follow-up before delivery: opening-crossfire defect resolved. Diagnostics showed
the second warmup guard's inherited22-unit detection allowed it to approach and
join the first fight. Scoped authored detection10, preserving normal AI/viewport
gate/personality and unchanged firing damage/range; no camera adjustment. Original
opening test unchanged. All8FirstOperation tests now pass, including real full
route and authored grenade/wallet sites. Final8Edit rules/layout pass and verify
the10-unit scene value. Final selection106/106 passes (23Edit,83Play). Earlier
failures are retained as history, not current blockers. FinalTestRuns is canonical.
Rebuilding release build_27fac48c3adc after this scene adjustment; previous APK
build_aa6a88e04ff0 is superseded. No runtime/source/native changes during export.

FINAL expanded0035: build_27fac48c3adc Succeeded175.721s,0errors/971warnings.
APK build/CombatReliability/AKUMU2-combat-reliability-release.apk39,818,983bytes;
SHA25634101a343cd859b368716a8e862a31a236f7274d96be1beb34e34758ab77ffb7.
Editor reconnected after export; final success recovered from persisted Unity
LastBuild.buildreport and archived in TestResults/CombatReliability. FinalBuildStatus,
FinalPackedAssetProof, APKmetadata/aapt and final hash verified. FinalEditorAudit:
Demo open/clean, second arrival guard detection10, walletfloor/diagnostics true,
0missing scripts.106/106selected tests pass. No device connected; no APK installed
or tested on Android hardware. Human artistic/combat/performance review pending.
No stage/commit/push. No production code/native asset edits after final export.

## 2026-10-28 — Hotfix0036.1 universal gameplay UI transparency

Small UI-only change: appended background-color rules to WarehouseControls.uss,
MissionControls.uss, GameFlow.uss and HandmadeUI.uss. Targets sticks40%, handles65%,
actions60%, weapon selection65%, vitals/ammo/mission55%, healthfill85%, gameplay
overlays/cards60–70%. Existing RGB palette and pressed/selected/healing/critical
states retained. Text/icons/group opacity and layout/touch geometry unchanged.
No Canvas/CanvasGroup in live HUD; actual runtime is UI Toolkit. No C# gameplay,
AI, weapon, physics, economy, world graphics or level changes. Main menu/shop
styles untouched; pause/results/revival overlay backgrounds transparent.
Seven existing relevant PlayMode checks pass (FinalTestRuns.json). Attached UI
probe verified resolved target alpha, groupopacity1 and HP/ammo/objective text1;
critical/healing/selected states pass. Probe closed/capture imports cleaned,
runtime background setting restored. HUD/pause/results captures inspected.
Final release build_7173418f1d13 exporting to
build/UITransparency0036_1/AKUMU2-ui-transparency-release.apk. Evidence/report:
TestResults/UITransparency0036_1. No source edits during export. No stage/commit/push.

Hotfix0036.1 final build_7173418f1d13 Succeeded53.461s,0errors/1251warnings.
APK build/UITransparency0036_1/AKUMU2-ui-transparency-release.apk39,819,759bytes;
SHA2560eac86f000b775c84a8622ac272b80af8c9939f70cb3d6c9f849cd6a796056ab.
Aapt ARM64/nondebuggable and packed assets verify all four USS changes. Final
presentation rerun passes and result button backgrounds inspected.7/7selected
PlayMode checks pass; no gameplay code changed. Editor ready/stopped. No physical
Android install/touch test claim. No stage/commit/push.

## 2026-10-29 — Sprint0037 combat impact, living reactions, player damage feedback

Full brief/report/file inventory/evidence: TestResults/Sprint0037. Existing live
architecture had pose-driven rigs and HitReaction2D with no living joints; extended
that owner with six cached collider-free angular springs/one anchor per actor.
No normal-hit corpse spawning or new ragdoll controller. Bounded directional body
regions reuse cached renderer lookup. Hand/grip/barrel alignment and handling poses
preserved. Existing collapse/recovery remains separate from death, HP and fear.
Configurable weapon hit strengths and distance scaling; pellet batches cap3.2,
rapid-hit diminishing, balance decay1.15, heavycooldown.55, fallcooldown3/grace1.2.
Enemy controller stays enabled, yields while down and resumes normal perception/
navigation; no personality/accuracy edits. Production player never locks input.
Health accepted-outcome event records actual HP/armor result without changing old
damage events. Four safe-area UI markers and edge strips,85%alpha HP flash; camera
.025world/.09s max once.65s with serialized0–1 reduction. Revive/invulnerability
no fake successful-hit feedback. Existing transparent HUD/touch geometry retained.
DE/sniper tracerScale1.6/2.2, existing variation/fade/trajectory/pool unchanged;
Officer .08baseline/enemy color untouched. Native six prefab/three scene authoring
and eight weapon assets updated through Editor APIs. CombatBefore/After snapshots
identical; current audit matches. Original sprites, terrain, profiles, prices and
progression preserved. Exact absolute file list in ModifiedFiles.txt.
133/133unique selected tests pass:15Edit/118Play,16new impact checks plus arsenal,
reload, cover/grenade/wallet, corpse, traversal, fear/personality, full mission,
revive/results, bunker, HUD and presentation regression. Raw failures retained:
spring wake needed an explicit enabled-body reset; scene unload can remove the
physics cache before its owner, now null-safe and explicitly tested. Fixture-only
armor/default-width assumptions corrected. Controlled captures inspected; not
human artistic/combat or Android FPS acceptance.
Native ADTM audio Editor crash recurred before export. Editor reconnected, saved
source/config audited, then release build_3992cd59f9ac Succeeded197.247s,
0errors/974warnings. APK build/Sprint0037/AKUMU2-combat-impact-release.apk,
39,837,579bytes,ARM64/nondebuggable. SHA256
3741ddf3f08dbc1a58cbc8e225172fe36c3e619abbc7aaedaa9423a6a274f034.
Build report, packed assets, generated IL2CPP and APK method metadata verified.
adb devices empty; no device installation/test or sustained mid-range performance
claim. Native audio issue remains undiagnosed. No source/native edits during
export. No stage/commit/push.

## Sprint0038 — 2026-10-29
Small polish: ballistic actor cues +20%, exact flush magazine extraction for BackupPistol (15/1.5s retained), five static background factory graffiti marks via native Editor authoring. Existing animation/pool retained; missing-magazine guard. 23/25 selected tests pass; two legacy AI/instant-recovery failures reproduced with pre-sprint runtime source, left outside scope. Final impact test re-passed. Report: TestResults/Sprint0038/Report.md. No APK/device verification. Two original unsaved shelf moves backed up, excluded from disk scene and restored to open Editor. No stage/commit/push.

## Sprint0039 — 2026-10-29
Completed shared arsenal/loadout refinement: oneprimary/twosecondaries with version1 save migration, SKS/AA-12/sawed-off, correctGlock15/flushmagazine retained, data-driven receiver cartridge loading and distinctsniper recovery. Fixed empty serialized weapon slot mission crash, checkpointthirdslot, storedammunition truncation and null/empty slot persistence. Grenadeboundary repairs, frag145/radius4.5, isolatedshootable/pressuremine prefab (zero campaignmines). Canopylanding/ladderretraction+exactzero landing protection+controlleddescent energyreset; connected nativecanopy. Enemy redflash serializedcolor/tintpriority fix; samehitscantracerpools pale-core shader and materialclassification8wood/11metal/7concrete. Bibles/AGENTS updated; Level1 layout/13actorroster preserved.102/102selecteduniquetests pass, compile/whitespacechecked, controlledcapturesreviewed. NativeADTM Editoraudiofatalrecurs duringtesting/reloads; -noaudio attemptdidnotresolve, restartednormally. Releasebuild_f11552b8f7ad Succeeded331.231s/0errors/977warnings. APK build/Sprint0039/AKUMU2-sprint0039-release.apk,39843255bytes,ARM64/nondebuggable,0.13/code2; SHA256945E4E44F16B2C8211EADD59A2A5CA20992C4BA9114D6E77B509CC933F64BEBE. Packedassets andactualAPKmetadata verified; phonegameplay/audio/FPS/art acceptance PENDING USER VERIFICATION. Report/checklist/absolute filelist TestResults/Sprint0039. No stage/commit/push.

## Sprint0040 — two-firearm correction and living response (2026-10-29)
Removed actual Firearm3/runtime secondary2/save/UI/packing/switching architecture; legacy raw JSON envelope safely migrates extra secondary into owned unequipped stock, first valid owned primary/normal secondary preferred. Two independent firearm states plus existing grenade tool. Enemy-only six-joint response addresses double damping and low rendered torso clamp; regional impact-point/direction torque, bounded pellet batches, late-pose integration, existing anti-lock/recovery rules preserved. Native projection1280x720/ortho4.329 controlled A/B captures: Glock1.70→4.65px, SMG.62→2.86px, DE11.63→18.52px. All11player weapon balance audit values unchanged; Health/AttackExecutor/body damage/death/AI/motor sources unchanged from sprint start.116selecteduniqueEdit/Playtests:113pass/3CowardsBunker failures; identical committed/current native geometry conflicts with test shell/roof/reardoor assumptions, preserved and reported rather than redesigned. Native scenes no missing scripts/mines/thirdfields; Demo13enemies. Android release0.14/code3 ARM64 IL2CPP/nondebuggable build_eb8380764492 Succeeded213.368s/0errors/974warnings, APK39843675bytes, SHA2564AEC0E14009A5BED508D47D8E452D2958061412235C8E3D5E9182C50F16CD5AD. Actual APK identity/newmetadata/noFirearm3 verified. No phone connected: installation/gameplay/FPS/feel/art PENDING USER VERIFICATION. Prior ADTM Editor-audio fatal unresolved. Full report/filelist/checklist/evidence TestResults/Sprint0040. Prior0039dirtychanges preserved; no stage/commit/push.

## Living hit feedback / AA-12 hotfix — resumed 2026-10-29
Recovered interrupted CombatFeedback/shader/recoil/Editor-helper changes without restarting or discarding dirty0039/0040work. Applied native helper: one shared sprite-alpha red-flash material, complete rig cache after initialization, DamageResolved lethal cancellation BEFORE death observers/corpse copying, dead checks at every entry/LateUpdate, disable cancellation and repeated-pellet tint coalescing. No lethal-first/dead flash; corpse forearms/materials restore synchronously. AA12 original6x4 magazine pixels split from34x14sprite; existing magazine-exchange animation connected, no shell loading. Ammo clock was already correct; retained after full/empty/partial/short-reserve/cancel/restart/HUD/facing tests. Capacity8/reload3s/damage8/cadence.26/7pellets unchanged. Living enemy visual impulse160→175; existing pooled2/4chip.12s accents aggregate per victim/rendered frame; shotgun muzzle+25%, automatic shotgun visual recoil.12→.16s; existing tracers/material impacts/AI/fall rules preserved.50/50selectedEdit/Playcases pass; shader no errors; native7prefabs/3scenes material references complete/no missing scripts.16weapon native audit fields unchanged; Health/WeaponCycle/HitReaction2D/CutoutRig/PistolMagazineReload/RagdollOnDefeat source hashes unchanged from originalhotfix snapshot. Controlled1280x720/native-zoom fullred/restored/death/AA12captures inspected; no phone60FPS/feel claim. Report/evidence/filelist TestResults/HitFeedbackHotfix. Android release0.14.1/code4 ARM64 IL2CPP/nondebuggable build_a6d4161649ea Succeeded263.197s/0errors/974warnings. APK39845423bytes SHA256A6CDCBAF343711D1EBC875BD72A7D49FAABF6872336A793BB40257EB252414E3. Actual APK code metadata and packed new shader/material/AA12 art verified; no phone connected/installation or gameplay verification. Full identity AndroidBuild.md. Previous bunker geometry/test drift remains open/outside scope. No stage/commit/push or disk cleanup deletions.
