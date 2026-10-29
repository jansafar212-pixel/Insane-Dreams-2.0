# Camera, player sprite and Samurai AI — recovery completed 2026-10-23

Implemented by Codex; automated Editor verification passed. Human PC gameplay
feel, artistic approval and Android-device validation remain pending.

## Recovered scope

The earlier six-change hotfix was already finished: basic Larper reaction .30→.55s,
wallet radius1.2→1.62, Samurai jump12→13.28, crate/cover/descent navigation fixes,
the native carbine redesign and complete armor-overlay removal at zero/disable.
Its report/results are `FocusedHotfix_Report.md` and `FocusedHotfix.json`.
Those changes were preserved, not repeated.

The interrupted task was the subsequently and explicitly requested ladder camera,
player sprite, Samurai detection/aggression and navigation follow-up. Its runtime
and asset changes were already on disk before this recovery. This continuation
completed the pending test run, refreshed final visual evidence, checked references
and Console, and documented the result. No new gameplay features or replacement
architecture were added during recovery.

## Camera

The camera previously always damped toward facing, including on ladders. It now
caches the existing PlayerTraversal2D and smoothly targets zero directional offset
while IsClimbing. Aim changes do not steer framing during traversal. Dismounting
smoothly restores the normal directional composition, approximately1/3 player
viewport X facing right and2/3 facing left. One continuous damping state is used;
the existing .75-second direction transition, .12-second commitment, .16-second
follow smoothing, zoom, vertical offset, level clamps and pixel presentation remain.
Existing vertical following keeps the climbing player visible.

## Player art

Inspection found a long peach neckline, uneven shoulder/sleeve blocks, square hand
ends and trouser sprites merging into a solid blue slab. HeroRefined cleans those
shapes and separates trouser silhouettes with transparency. It retains the worker's
cap/orange shirt and the same module bounds, pivots, lengths,32PPU, flat palette,
face overlays, two arms, equipment attachments and armor parent/layers. No player
motion, collider, animation timing or new skeletal framework was changed.

[Reference/pose comparison](CameraSamuraiHotfix_PlayerReview.png) and
[actual warehouse camera render](CameraSamuraiHotfix_Warehouse.png).
The latter uses the live camera's orthographic size4.843074. Frame2622/time8.098s
advanced to frame6163/time18.799s, confirming an advancing scene rather than a
frozen screenshot. This is a controlled world render, without UI or human input.

## Samurai investigation and fixes

The small-body detection hypothesis was not reproduced: the actual .7539605×
1.028464 locomotion collider detects clear targets at multiple heights and loses
sight behind solid geometry. Detection is still22 units from the actor, not a
radius scaled from the sprite. Sight runs from collider-top minus.15 to the
target's upper collider point, uses the existing geometry mask1, and ignores actor
and target hierarchies. No hitbox enlargement, raised eyes through cover, additional
per-frame sight rays or wall-penetrating awareness was introduced. The animated
visual-sized damage hitbox remains intact.

Confirmed problems were in engagement and traversal ownership:

- Melee engagement was not recorded in the existing HasEngaged flag, and the
  shared three-second sight-memory expiry could abandon pursuit. Acquisition now
  requires explicit gameplay-camera body overlap plus the existing sampled valid
  line of sight and range. Once acquired, camera exit and occlusion do not discard
  aggro. Hidden player motion does not update LastKnownPosition; the Samurai pursues
  remembered information rather than gaining omniscience. Target loss/defeat and
  encounter reset still clear engagement.
- Generic patrol could run a FixedUpdate before combat's first Update and queue an
  unplanned jump, disrupting the elevated approach. Combat enable and target/reset
  binding now synchronously hold an unacquired Samurai until a valid acquisition.
  Basic Larper behavior is unaffected.
- A failed jump's stationary-failure flag could reject the same route forever,
  even after the obstacle disappeared. It now expires after max(5s, configured
  failedJumpCooldown). Existing cached failed-edge rejection and full swept-arc
  validation remain, so an uncleared wall does not become a valid launch.
- Decision gaps over.15s previously discarded in-flight plans. They now retain
  the committed physical flight, still bounded by its landing/timeout, damage,
  disable and death handling.

| Tuning | Before | After |
|---|---:|---:|
| Samurai acquisition reaction | .18s +0–.05 jitter | .04s +0–.02 jitter |
| Initial melee-controller startup pause | random .3–.8s | .05s |
| Stationary failed-flight rejection | until moving .8 units | expires after at least5s |
| Melee wind-up/recovery | .4/.7s | unchanged |
| Damage/range | 40/1.65 | unchanged |
| Jump speed/perception sample | 13.28/.1s | unchanged |

Melee cannot start or complete a wind-up while outside the gameplay viewport.
Pursuit may continue offscreen. Suppression, hit reactions and existing defensive
interruptions still function. No spawning, persistence, weapon lethality, morale,
player movement or double-jump changes were introduced. Previous crate footholds,
landing margins, walk-off handling and bounded local graph were preserved.

## Files in the interrupted follow-up

- Runtime: `Assets/Scripts/Camera/WarehouseCameraFollow.cs`,
  `Assets/Scripts/Enemies/EnemyCombat.cs`, `SamuraiCombat.cs`.
- Authoring: `Assets/Editor/CameraSamuraiHotfixSetup.cs` plus metadata;
  `Assets/Art/CameraSamuraiHotfix/`, `Assets/Data/Visuals/HeroRefined.asset`.
- Prefabs: `Assets/Prefabs/Player/WarehousePlayer.prefab`,
  `Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab`,
  `Assets/Prefabs/Enemies/Samurai.prefab`.
- Tests: new `Assets/Tests/PlayMode/CameraSamuraiHotfixTests.cs` and
  `LadderCameraHotfixTests.cs` plus metadata; updated
  `Assets/Tests/EditMode/Sprint0244RigTests.cs` reaction expectation and
  `Sprint008PresentationTests.cs` approved native-art path (pixel safeguards retained).
- Recovery deliverables: `ART_BIBLE.md`, `DEVELOPMENT_LOG.md`,
  `TestResults/CameraSamuraiHotfix*`.

## Verified versus pending

Compilation succeeds. Full Edit Mode78/78 and86 unique targeted Play Mode checks
pass across retained runs, with final affected fixtures rerun after the last fix.
Coverage includes six new small-collider/real-warehouse tests, ladder composition,
normal camera reversal, visible acquisition, offscreen attack rejection, persistent
last-seen aggro, reset, occlusion at several elevations, autonomous cover/elevated
pursuit, temporary-obstacle recovery, decision-gap flight, melee, ragdolls, animated
hitbox, player visual integration, double jump/motion, touch/ladder input, armor,
combat and preserved Larper first-shot/cadence behavior.

Earlier failed baseline/intermediate runs are retained in CameraSamuraiHotfix.json;
latest results replace earlier failures. The memory/occlusion test places its wall
between current actor positions instead of assuming a fixed position after pursuit.
All three warehouse scenes have zero missing scripts/broken serialized references.
Final Console:0errors/0warnings and no compilation failure.

PC manual checks: run MainMenu→Warehouse_Demo; enter/climb/dismount ladders while
changing aim; test visible Samurai entry and camera exit, obstruct/reveal paths,
kite across crates/upper steps, and dodge the committed wind-up. Judge aggression,
camera comfort and the refined hero at normal zoom with armor, reload and movement.
No automated check establishes universal navigation success or artistic approval.
No Android device or performance profile was run.

Future agenda: player melee tracer/swing/hit-feedback diagnosis remains planned,
unimplemented and unverified. Larper knife is explicitly excluded; basic Larpers
retain their pistols. No additional agenda items were started.

MainMenu is saved, open and not dirty; Play Mode stopped; runInBackground restored
off. Existing dirty/untracked work was retained. No staging, commit or push.
