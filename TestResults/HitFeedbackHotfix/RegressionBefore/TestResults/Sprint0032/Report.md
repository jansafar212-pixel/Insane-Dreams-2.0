# Sprint 0032 — implementation report

Implemented by Codex; human gameplay and Android-device verification remain manual.
Original requirements and audit are preserved in this folder.

## Changes

EnemyCombat retains its existing sampled sight, event-driven hearing, suppression,
morale, cover and combat states. Initial recognition now belongs to the remembered
encounter instead of restarting after a one-second occlusion. A separate .18-second
camera-entry grace prevents a visually unseen enemy from finishing recognition
outside the viewport and shooting on its first visible frame. Leaving the view
still blocks attacks; awareness and Samurai pursuit can persist.

Hearing already handled nearby friendly/hostile gunfire and explosions across
vertical decks. The correction removes smoke from logical sound attenuation while
retaining terrain attenuation, finite range, throttling and approximate quantized
positions. Hearing never writes precise visual memory or rebroadcasts an alert.
Existing witnessed-death LOS/distance checks and fear/suppression balance remain.
No new noise manager, morale system, state machine or global search was added.

LarperBrain now pauses and requests replanning after real blocked grounded travel.
Intentional zero-input cover/reload/aim states and airborne movement do not count.
Direction changes reset observation; retry cadence is bounded. Failed retreat
positions are blacklisted before timeout handling. Existing local clearance,
cover reservations, low-hop limits and alternative destinations remain in use.

The Samurai keeps the existing local platform graph and physical swept arcs.
Support lookup explicitly ignores ladder triggers and clears stale results. Initial
recognition now delays pursuit as requested, while committed flight remains intact.
No collider enlargement, teleportation, new movement capability or player change.

The Editor-only EnemyCombat inspector displays awareness/action, sight, camera
permission, recognition time, stale visual/approximate sound memories, reserved
destination, suppression, recovery count and Samurai route rejection. It produces
no runtime logs or Player UI.

## Tuning (seconds unless noted)

| Parameter | Before | After |
|---|---:|---:|
| Larper recognition + positive jitter | .7 + [0,.12] | .7 + [0,.08] |
| Samurai recognition + jitter | .25 + [0,.08] | unchanged |
| Fireworks recognition + jitter | .3 + [0,.12] | .6 + [0,.1] |
| Officer recognition + jitter | .7 + [0,.05] | .45 + [0,.05] |
| Recent visual memory | 3 | 4.5 |
| Camera-entry attack grace | 0 | .18 |
| Stuck observation | .9 | 1.8 |
| Stuck recovery pause | none | .6 |
| Minimum stuck-recovery retry interval | none | 3 |

Hearing memory4s/radius24/terrain attenuation.45, quantization2units and hearing
interval.25s remain unchanged. Officer warning stays.5s. Weapon damage/cadence,
throw fuses/explosions, fear probabilities, player physics and UI are unchanged.

## Scoped files

- Assets/Scripts/Enemies/EnemyCombat.cs
- Assets/Scripts/Enemies/LarperBrain.cs
- Assets/Scripts/Enemies/SamuraiCombat.cs
- Assets/Scripts/Enemies/SamuraiPlatformRoutes.cs
- Assets/Editor/EnemyCombatDebugInspector.cs (new, Editor-only)
- Assets/Prefabs/Enemies/Larper.prefab and FireworksExpert.prefab
- Assets/Data/FirstOperation/Officer.prefab and OperationFireworksExpert.prefab
- Samurai variants inherit updated shared settings without new serialized changes.
  Six prefabs were inspected/authored via Unity APIs; only four need byte changes.
- Assets/Tests/PlayMode/Sprint0032Tests.cs (new)
- Existing CameraSamuraiHotfixTests, Level1CombatHotfixTests and Sprint005Tests adapt
  timing expectations to explicit sprint requirements without dropping assertions.
  Sprint0202Tests isolates its frozen suppression-only fixture from real stuck recovery.
- DEVELOPMENT_LOG.md, SPRINT_RESUME.md and this evidence folder.

PrefabTuningDiff.txt records the exact native changes. No scenes, sprite/hitbox,
player/input/UI, Officer telegraph, fireworks mechanics or catalog were edited.
ProtectedAfter.json verifies those preserved files against the start of the sprint.

## Verification and limits

Final suite counts/results are in Validation.md and Regression.json.
The new fixtures check recognition/occlusion, offscreen entry, credible real gunshot
hearing through decks, smoke, bounded local alert delivery with16listeners,
stationary versus genuinely blocked recovery, ladder-trigger support, faster
Samurai recognition and physical successive-platform traversal without snapping.

Performance review: new recovery bookkeeping is scalar fixed-step work; hearing
uses the existing event and interval, fixed ray buffers and no new scans. Solid
support rays replace the previous two unfiltered rays at existing route decisions.
No per-frame strings, path lists, coroutines or allocations were introduced by
these changes. Debug Inspector strings exist only in Editor repaint. Existing
bounded graph limits/caches and full-body clearance remain unchanged. Sixteen
listeners were exercised concurrently; this is not a phone CPU/frame-rate profile.

Limitations: graph search remains local/bounded and primarily supports authored
box platforms and ladders. Only representative physical routes and the existing
Level1 tests were exercised, not every reachable-looking destination. No Android
device, hardware performance or human assessment of combat feel was available.
No new APK was requested/built in this sprint; the0031APK predates these AI changes.
Do not use it to judge0032behavior without rebuilding from the current source.

## Manual playtest

1. MainMenu -> First Operation. Approach an unaware Larper: recognition should be
   about.7s, then normal firing cadence. Duck behind cover for1–2s and reappear:
   no repeated full first-contact delay. Check inaccurate fire/personality remains.
2. Fire under a nearby deck and inside smoke. Upper enemies should react locally;
   they must not aim through solid terrain or follow hidden movement exactly.
3. Let ordinary enemies become alert offscreen, then pan toward them: no unseen
   attacks or single-frame surprise shot. Crossing the camera edge must not erase
   awareness or restart the entire surprise delay.
4. Lead Samurai over crates, to higher/lower platforms and around a blocked ledge.
   Check short recognition, committed readable swings, real landings and no repeated
   impossible hops. Remove a temporary obstacle in Editor and check recovery.
5. Check a Larper hiding/reloading stays still intentionally; an attempted blocked
   advance should pause/replan rather than push indefinitely. Preserve floor limits.
6. In the finale, confirm concrete stops shots, Officer reaches the slit, warns
   for.5s, fires the distinctive tracer, hides/retreats and resumes. Complete Level1.
7. Suppress Larpers and defeat a visible ally: defensive reactions must still win
   over pursuit. Check Android multitouch and sustained performance after rebuilding.
8. In Play Mode select an EnemyCombat component to inspect the new live diagnostics;
   Scene view/camera presence is not permission to attack.

