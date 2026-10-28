# Adaptive combat camera — handoff report

This implements the latest camera brief (user title Sprint0031), following the
completed0033bunker hotfix. Earlier0031UI work and its artifacts remain preserved.

## Root causes and ownership

WarehouseCameraFollow was already the single follow/size owner, running LateUpdate
at order150. The live scene is URP with no Cinemachine/PixelPerfectCamera competitor;
AA is off and the player Rigidbody2D already uses Interpolate. Those were retained.
CharacterVisualDriver.Facing is movement-driven, so using it for camera attention
was wrong when retreating while aiming backward. Camera output was rounded to a
1/32-world-unit grid despite fractional final zoom, producing stop/step motion as
interpolated travel crossed grid cells. A.75s SmoothDamp parameter was not a.75s
settling time and was followed by another position smoother. No combat memory or
explicit falling lead existed; movement feed-forward was abruptly clamped at bounds.

## New pipeline

Existing controller replaced internally, with one authoritative LateUpdate write:
meaningful aim (or intentional held fire), recent combat direction, committed
movement, last meaningful facing. Aim is weapon-independent and works without
firing. Strong vertical aim also has a bounded, damped bias. Hysteresis rejects tiny
inputs and rapid direction chatter; brief release holds the previous engagement.

Exact critically damped state separates lead, horizontal tracking and vertical
tracking. Normal physical horizontal displacement uses the interpolated target and
is compensated directly, avoiding speed-dependent trailing lag. Lead transitions
remain smooth through reversal. Near boundaries that compensation fades before
the limit and a soft target eases toward legal world bounds. World bounds take
precedence when ideal composition is impossible; no routine follow teleport exists.
Explicit SnapToTarget remains reserved for bind/spawn/checkpoint/replay.

Ladders smoothly center horizontally and suppress vertical aim steering, retain
damping velocity through entry/exit and briefly debounce dismount. Vertical dead
zone suppresses minor platform bob; falling lead and faster comfort recovery keep
large descent visible. Landing/parachute changes change a damped target, never reset
camera position. Existing optional explosion impulse is preserved, not expanded.

Close combat gently contracts directional lead, exposing more space on both sides.
It reads the existing combat registry~6.7times/s, uses cached awareness/activity and
bounded distance only. No enemy is selected or tracked as a camera target. New
spawns/deaths only alter a damped scalar. No new scene searches or physics queries.

Existing projection/PPU integer zoom and the exact1.155gameplay/1.125Android
multipliers are preserved; no new cinematic zoom, low-resolution target or blur.
The follow transform is continuous instead of art-grid rounded. Point-filtered
assets are unchanged. Pixel shimmer at specific display scales still needs device
artistic review; strict grid stepping was intentionally removed for fluid motion.

## Starting Inspector settings

| Parameter | Value |
|---|---:|
| Normal horizontal tracking (95% settling) | .16s |
| Combat horizontal tracking | .11s |
| Directional lead settling | .5s |
| Direction commitment | .12s |
| Combat attention memory | 1s |
| Aim dead zone | .30, release at80% of entry threshold |
| Horizontal lead | 1/6of viewport, giving default1/3player position |
| Ladder recenter | .4s |
| Vertical settling / dead zone | .24s / .25units |
| Max falling lead / full-lead speed | 1.6units /14units/s |
| Vertical aim bias | 1.2units |
| Comfortable screen margins | .18x/.18y |
| Boundary easing distance | 2units |
| Nearby combat radius / max lead contraction | 6units /45% |

Attention/framing transitions include commitment/tracking; these values are starting
points, not a claim of human-approved feel. Existing level bounds, camera offset,
zoom, controller positions/sensitivity and player acceleration remain unchanged.

## Files and preserved systems

- Assets/Scripts/Camera/WarehouseCameraFollow.cs: follow/attention implementation.
- Assets/Scripts/Input/PlayerActions.cs: one read-only AimIntentStrength getter;
  no processing, timing, gesture, firing, movement or pointer handling changes.
- Assets/Scripts/Enemies/CombatPositionReservations.cs: read-only camera-pressure
  query of existing registry; no AI state/reservation/decision changes.
- Camera settings in Warehouse_Demo, Warehouse_Testbed, Warehouse_Mission01 scenes.
  Native APIs only; SceneScopeAudit.json confirms same object IDs and no changes
  outside camera blocks except persistence of an already-false legacy mission default.
- New Assets/Tests/PlayMode/AdaptiveCombatCameraTests.cs; existing
  LadderCameraHotfixTests expands aim directions to include up/down while climbing.
- DEVELOPMENT_LOG.md, SPRINT_RESUME.md and TestResults/AdaptiveCombatCamera evidence.

EnemyCombat/LarperBrain/Samurai/Officer code, hitboxes/weapon data, movement,
parachute/fall-damage rules, traversal, UI/control layout, bunker/level geometry
remain preserved. No new package, rendering framework, enemy or mechanic added.

## Verification and remaining limits

Final suite counts/results and allocation evidence are recorded in Validation.md.
New fixtures exercise backward combat both ways, hold-to-aim without fire, memory,
noise/chatter, immediate physics input, threats on both sides, enemy removal,
up/down aim, falls/parachute/landing, padded world bounds at phone aspects,
30/60/120Hz and irregular dt, sub-art-pixel displacement and managed allocation.

These are advancing Unity Editor/controlled simulation checks, not a human or
physical Samsung Galaxy A56 playtest. No device60FPS/frame-pacing/input-latency
claim is made. Camera math has no new per-frame scene queries, physics or recurring
managed allocations in the controlled test. Whole-game CPU/GPU/thermal performance
and frame caps are separate; global frame-rate/physics settings were not changed.

## Samsung Galaxy A56 manual checklist

1. Build/install this revision (older0033APK predates this camera). Measure actual
   device FPS/frame timing at60Hz, then check cap, CPU/GPU/thermal issues separately.
2. Run both ways, stop, alternate directions, jump/double-jump and land. Confirm
   fluid motion and immediate stick response; compare slow and rapid gestures.
3. Retreat left while aiming/firing right, then mirror. Release aim for half a
   second and resume: framing should hold combat direction. Hold aim without fire.
4. Flip aim rapidly, then hold the new direction. Try tiny stick drift and up/down
   aiming. Observe smooth settling rather than flicker or whole-screen swings.
5. Fight close Samurai and opponents on both sides. Check escape/player visibility,
   no target lock and no jump when enemies spawn, die or cross the viewport.
6. Grab, climb and exit each ladder, including the taller pillbox access. Aim in
   all directions while climbing: horizontal centering should persist smoothly.
7. Fall from a tall platform; deploy/retract parachute; land. Check landing area,
   smooth recovery and unchanged fall-damage immunity when deployed.
8. Approach both level edges and the Officer bunker at normal phone zoom. Check
   HUD/joystick finger occlusion and existing offscreen-attack/spawn fairness.
9. Repeat at lower/unstable render rates and another phone aspect. Record separate
   symptoms for dropped frames, camera motion, sprite pixel phase and input delay.

## Historical fixture findings

Sprint013Tests has two failures retained unchanged: its standalone-camera panic
fire test reenters for.15s before occluding, shorter than the existing0032.18s
camera-entry attack gate; it then demands blind fire without a fresh eligible shot.
Its impact fixture also requires exactly6LineRenderers although current player has7.
These fixtures do not instantiate WarehouseCameraFollow for the tested camera.
Current AI/camera-fairness and actual combat/HUD coverage are checked separately.
No AI/VFX changes were made to force unrelated historical expectations green.

Allocation evidence correction: this Unity runtime's GC.GetAllocatedBytesForCurrentThread
counter was subsequently found unsupported (returns zero for a known 8 KiB array).
The earlier zero-byte camera figure must not be used as an allocation guarantee.
See AndroidPerformance/AllocationCounterCheck.json and Validation.md correction.
