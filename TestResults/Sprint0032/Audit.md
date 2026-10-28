# Sprint 0032 — AI reliability audit

Original brief preserved verbatim in OriginalRequirements.md. Existing dirty tree,
0031UI and earlier sprints preserved. No stage/commit/push.

Existing architecture: EnemyCombat owns sampled LOS, recent visual memory, shared
CombatNoise listener, suppression/morale, reservations and attack gates.
LarperBrain owns fixed-step ground movement and local hop clearance.
SamuraiCombat + SamuraiPlatformRoutes already provide bounded cached local graph,
swept physical single/double-jump arcs, ladder transitions and failed-edge cooldowns.
OfficerShotTelegraph/FireworksCombat own committed readable attacks. Spawning and
WarehouseMission activation remain separate and are not being redesigned.

Confirmed gaps:
- Larper prefab already .7+.12s reaction; Samurai .25+.08s; fireworks only .3+.12s.
- Reacquisition restarts full recognition after >1s despite 3s recent visual memory.
- Offscreen sight can finish recognition before first camera entry, allowing an
  immediate first visible shot. Need short separate entry gate, not recurring .7s.
- Hearing already handles 3D-in-plane distance, friendly gunfire and explosions;
  smoke was incorrectly treated as sound attenuation via the shared sight helper.
- Stuck sampling .9s can reissue a replan with no actual movement pause. A failed
  retreat destination was not recorded before its early replan return.
- Graph support raycasts use global trigger settings and may select a ladder volume
  instead of the supporting solid platform. Preserve the graph and fix that lookup.

Implementation remains inside existing AI: configurable entry grace, remembered
recognition, tuning of existing delays/memory, bounded movement recovery, solid-only
support rays. Editor-only inspector exposes state, sensory memory, timers,
reservations, suppression and route rejection. No player/weapon/UI/layout changes.

## Current implementation and tuning

Native audit confirmed the existing Larper already met the .7s target. Its mean
was not raised: jitter narrowed .12 -> .08s. Samurai remains .25+.08s. Fireworks
recognition .3+.12 -> .6+.1s. Officer .7+.05 -> .45+.05s, independent of unchanged
.5s laser warning. All visual memories3 ->4.5s; hearing memory stays4s, hearing
radius24/obstructed multiplier.45 unchanged. New viewportAttackDelay=.18s is a
separate entry guard; it does not restart the full recognition timer. Short LOS
loss now retains recognition throughout existing visual memory.

LarperBrain stuckSeconds .9 ->1.8s, stuckRecoveryPause=.6s, stuckRetrySeconds=3s.
Only nonzero attempted grounded travel counts; stationary/airborne/direction changes
reset observation. Existing replan/reachable-cover selection and failed Samurai
edge cooldowns remain authoritative. No collision disabling, teleport or new
traversal ability. Failed retreat destination now blacklisted before its timeout.

Samurai graph keeps its fixed arrays,1s geometry cache,.3s decision interval,
limited local search and full-body arc validation. Source/goal support rays now
explicitly ignore triggers; stale destination is cleared on every selection.
No replacement graph or external dependency was added.

Event-driven CombatNoise still owns real gunshot/explosion/ally notifications;
no additional broadcasts, recurring scans or inferred live-target positions.
Smoke no longer attenuates sound. Approximate coordinate quantization remains2u.
Existing solid occlusion attenuation, hearing interval.25s and fear priorities
remain. Debug counters store accepted observations only, not every projectile.
Editor-only live inspector replaces speculative runtime logs.

Initial new fixture failure corrected: bare Larper prefab relies on spawn-time
Configure(archetype); test helper now mirrors it. First8cases then passed.
Three old fixtures intentionally adapt to explicit sprint changes: Samurai no
longer assumes instant pursuit, old.6s Larper-shot wait now1s/4.6s memory wait,
and isolated Officer beam check waits.25s for camera grace. No assertions removed.

