# AKUMU 2 — Level Design Bible

The project owner retains final creative authority. Read this together with
GRAPHICS_BIBLE.md before designing or changing levels.

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
