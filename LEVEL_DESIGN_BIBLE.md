# AKUMU 2 — Level Design Bible

The project owner retains final creative authority. Read this together with
GRAPHICS_BIBLE.md before designing or changing levels.

## Non-negotiable spatial-first workflow and graybox quality gate (2026-11-05)

**Spatial design FIRST; graphics SECOND.** A technically playable level is not
automatically a well-designed level. Do not proceed to detailed environment art,
decoration, new background composition or visual polish until the spatial graybox
has passed the owner's design review. New level work must be specified in
observable spatial terms; a vague request to "make an interesting level" is
insufficient.

For every new level or major level redesign:

1. **Topology before Unity geometry.** Present a readable plan/diagram showing
   start and exit, traversable elevations, routes, ladders and drops, encounter
   spaces, cover, chokepoints, sightlines and spatial landmarks. Explain why each
   major region exists and how the player moves and fights there. Obtain owner
   approval of the proposed topology before expensive implementation.
2. **Graybox before decoration.** Implement simple placeholder geometry with
   functional collision, movement connections, encounter positions and camera
   considerations. Do not attempt to rescue weak spatial design with attractive
   backgrounds or cosmetic props.
3. **Meaningful verticality.** Elevated areas must change traversal, combat
   decisions, enemy approach angles or sightlines; decorative platforms do not
   count. Verticality alone is not quality: stacked empty rectangles can be as
   boring as one long horizontal corridor. Level 1 may remain intentionally
   accessible and mostly horizontal; subsequent levels require their own spatial
   identity, not arbitrary platform quotas.
4. **Encounter variety and spatial choices.** Avoid monotonous flat corridors.
   Include purposeful changes of elevation, openness, cover, pressure, or
   traversal rhythm appropriate to the mission. Distinct rooms/sections must
   create recognizable decisions rather than repeat the same shooting lane.
5. **Playable on a phone.** Validate ladders, jumps, crouch and cover where used;
   enemy traversal, line of sight, camera framing, offscreen fairness, touch
   precision and legibility must work at actual gameplay scale.
6. **Mandatory decision: PASS or REVISE.** Review traversal and combat through
   the whole graybox, record failures, iterate until the owner approves its
   geometry and spatial identity. Only then begin biome art and environmental
   sprite placement. Android feel and fairness remain **PENDING USER
   VERIFICATION** until actual device playtesting; final level approval requires
   the existing project-wide device/QA gate.

**Immediate application:** Level 2's current flat, weakly vertical, visually
unconvincing layout fails the spatial gate. Redesign its topology and graybox
before any graphics redesign. For later visual production, reuse/recombine the
approved sprite library for its biome; do not build every backdrop from scratch.

Every campaign level needs a recognizable gameplay identity. At most two levels
may substantially share layout, encounter composition, pacing or overall feel.
A different background alone does not create a different level. Deliberately
vary featured mechanics, enemy archetypes, hazards and traversal opportunities.

Keep Level 1 primarily horizontal and accessible. Ordinary enemies must not
attack unfairly from outside the camera. Only designated later-game threats such
as snipers or mortars may attack offscreen, with readable advance telegraphs.
Avoid spawn clumps, unavoidable crossfire, unfair reaction times and unclear
hazards. Landmines are future-level content and must not appear in Level 1.

Geometry must support the actual AI: hiding and retreat for Larpers; navigable
aggressive pursuit routes for Samurai; working cover, peek/fire positions and
retreat space for Officers. Improve placement rather than making enemies
artificially smarter to compensate for poor geometry. Preserve their personalities.

Every level requires a design review, functional checks and eventually a real
Android playtest. Automated checks and PC captures establish specific behavior,
not whether mobile combat feels fair or enjoyable. Record real-device checks as
PENDING USER VERIFICATION until the owner tests the exported build.

## Mandatory gameplay and level acceptance gate

Before introducing a new enemy archetype, hazard, movement mechanic, weapon interaction, major feature or campaign level, fully validate the previous addition. Do not stack multiple untested additions and then attempt to debug all of them together.

For new or materially changed **enemy types**, verify their complete gameplay loop: spawning, sight/awareness, pathfinding and traversal, tactical decisions, attacks, cover/retreat as applicable, damage/hit reactions, death or medic revival interactions, offscreen fairness, dialogue telegraphs and mobile performance. Check combinations with existing enemies and the geometry they actually encounter.

For each **level**, play through its complete route and major encounters; verify entrances/exits, blockers, sightlines, enemy routes, safe cover, hazards, transitions, win conditions, checkpoints/revives as applicable, player weapon reach, and stable Android camera/control readability. Validate the level's distinct identity as well as functional correctness.

Do not classify Editor tests, screenshots or an exported APK as a real-device playtest. Provide an Android build and concrete test checklist; the owner must verify feel, fairness, readability and device performance. Record untested items as **PENDING USER VERIFICATION**. A new level or feature is not accepted until its required tests have passed and the owner approves it. Failed or recurrent issues return to repair and regression testing before more content is added. See `AGENTS.md` for the project-wide quality gate.
