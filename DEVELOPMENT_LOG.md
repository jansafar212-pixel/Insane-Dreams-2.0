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
