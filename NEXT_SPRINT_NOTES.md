# Sprint 003 recommendations

1. Validate the warehouse on an Android device before expanding systems: genuine
   simultaneous movement/aim/action touches, focus loss, gestures, safe areas,
   landscape aspect ratios and performance. Install matching Android support
   manually when ready; Sprint 002 did not change build settings.
2. Add serializable item/weapon definition assets with stable identities and
   runtime inventory instances. Model two firearm slots and one unlimited melee
   slot explicitly; keep ammunition state out of definition assets and UI.
3. Extend PlayerActions with aim/fire intentions when combat is authorized.
   Keep aim direction independent from movement and rendering. Avoid treating
   the temporary direction marker as a weapon or combat controller.
4. Define a small set of enemy capability data (walk/climb/jump/drop/stationary).
   Prototype one Larper's locomotion using the existing motor/traversal boundary,
   then introduce surface/ladder graph edges as demonstrated needs arise.
5. Convert empty spawn markers into typed spawn-point components referencing
   archetype assets. Keep encounter rules outside geometry and movement scripts.
6. Address level transitions and runtime reference binding before multiple playable
   levels: player spawn, camera target and mobile UI binding should be configured
   by a small level-entry component, without a global all-purpose singleton.
7. Add repeatable Play Mode regression tests once traversal tuning stabilizes.
   Cover ladder destruction/disable, top exits, airborne jump rejection, gravity
   restoration, checkpoint reset and multiple UI pointers. Retain Prototype_01 as
   a compact movement sanity scene. Do not fold combat into PlayerTraversal2D.

No inventory, weapons, ammunition, enemy AI or combat was implemented in Sprint 002.

## Sprint 004 — updated after implementing Sprint 003

The recommendations above are historical. Inventory definitions/runtime loadout,
aim/fire intentions, typed spawns, one walking/jumping Larper, level entry and
20 automated regressions now exist. See DEVELOPMENT_LOG.md, Sprint 003, for the
current architecture and exact Android test instructions.

Prioritize real-device multitouch/lifecycle validation. Then build one weapon
handling/firing slice on PlayerLoadout + RuntimeWeapon + PlayerActions; do not put
combat in traversal or the aiming marker. Add explicit ammo/reload transitions,
one damage boundary and tests before enemy attacks. For locomotion, the next
increment is one authored Larper ladder route with capability filtering; Climb
is currently data only. Avoid a universal pathfinder or large inventory economy.

## Sprint 005 — updated after Sprint 004

Basic hitscan firearms, reloads, health/factions, melee, Larper shooting and one
authored ladder ascent are now implemented. The final 35-test suite includes all
20 Sprint 003 cases. Consult DEVELOPMENT_LOG.md / Sprint 004 and
TestResults/Sprint004.json for the current state.

Prioritize Android hardware validation, two-thumb fire-ring feel and readable
combat feedback (sound, muzzle/impact/reload cues). Tune the two-enemy encounter
before adding weapon categories or complex AI. Extend authored routes narrowly;
do not replace the working movement/traversal or inventory boundary.

## Sprint 006 — updated after Sprint 005

Ladder acquisition now tolerates collider-relative offsets and upward diagonals;
side/top exits use checked motion, with deliberate release and reattachment
hysteresis. CombatFeedback supplies reusable visual cues and configurable empty
audio hooks. Larper reaction, sampled aim, range/visibility memory and safe local
pursuit are implemented. Authored ascent waits for a grounded landing. The suite
now passes 47 tests, retaining all 35 earlier cases with documented fixture/timing
adaptations. See DEVELOPMENT_LOG.md / Sprint 005 and TestResults/Sprint005.json.

Prioritize developer playtesting and Android hardware validation of ladder entry,
diagonal jump arbitration, dismount noise tolerance, neutral rearm, independent
thumbs and cutouts. Matching Android Build Support remains absent; install only
with explicit authorization, then follow the documented device checklist. Tune
serialized values from real observations and profile sustained effects/combat.
Add properly licensed firing/hit/hurt/reload/empty/defeat clips to the existing
hooks. Exercise longer patrols around gaps and authored landing/route failures
before adding a few explicit route links. Keep navigation local; avoid replacing
the motor, inventory rules or combat authority with presentation/navigation code.

## Sprint 007 — updated after Sprint 006

Warehouse movement now includes safe collider crouching/crouch-walking, top-entry
arbitration and airborne ladder grabs. The parachute is press-to-toggle, with
reduced steering/descent and authoritative landing/ladder/recovery resets. Three
personality profiles drive occasional event-based speech and a rare witnessed
defeat panic. The low passage and nearby witness patrol use the existing level
and enemy type. See DEVELOPMENT_LOG.md / Sprint 006 and TestResults/Sprint006.json.

Prioritize real-device movement/lifecycle testing once matching Android support
is available. Tune crouch/joystick/chute feel from developer observations, polish
placeholder poses, add licensed SFX, then test longer encounters for speech rate,
panic rarity and local patrol/route edge cases. Do not add navigation, inventory
or animation frameworks to solve narrow presentation issues. Retreat and weapon
dropping remain optional future work; current panic pauses combat briefly.
