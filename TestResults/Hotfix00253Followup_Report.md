# Hotfix0025.3 — latest playtest follow-up

2026-10-24. **Implemented by Codex — awaiting human playtest and Android-device verification.** This brief reuses0025.3; separate follow-up files preserve the earlier0025.3 mission/reload/fireworks work and Sprint0026.

## Changes and causes

**Blind fire:** HUD visibility used IsCovered instead of physical crouching, and the menu-launched warehouse's MissionControls.uxml omitted the control entirely. Both HUDs now display blind fire whenever PlayerPosture2D.IsCrouched is true and hide it on standing. Outside usable cover it is disabled/dimmed; direct pointer handling also rejects firing without crouch+cover. Normal FIRE and the aim joystick remain available away from cover; the disabled control is placed100UIunits above FIRE to avoid overlap. In usable cover blind fire retains the established button position, pointer capture, weapon raise, spread, direction and solid-geometry restrictions. Standing beside cover does not show it. No joystick sizing/sensitivity or crouch-input changes.

**Samurai jump:** only Samurai.prefab jumpSpeed changes13.28→15units/s (+12.95% launch speed, approximately27.6% theoretical height at unchanged gravity). A real old/new free-jump comparison passes the25–30% rise target and verifies both land. Gravity scale3, horizontal movement, attack wind-up.4s/recovery.7s, damage40, small body and viewport acquisition remain unchanged. Planning already derives arcs/reach from motor speed, so no duplicate jump system was needed. All three saved Demo Samurai inherit15 without scene overrides.

The actual warehouse regression caught landing on a.9-high crate and stalling: existing descent logic required the player's remembered centre to be>.9 below the Samurai, which ignores that low-crate case. The gate is now.3, reusing existing swept walk-off, floor/drop limits, finite landing and failed-route bookkeeping. Actual autonomous crate traversal reaches melee again; no teleportation, gravity changes or new navigation framework.

**Glock alignment:** AttackExecutor correctly recorded a world-space shot origin and tracer. CombatFeedback.LateUpdate then moved only the flash to the recoiling/moving muzzle. Glock flashes now retain the recorded firing position and orientation throughout their short life, matching their tracer, rather than drifting with later recoil/translation. Explicit Inspector weapon references opt in only BackupPistol and LarperBuzzbox on player/Larper prefabs. Other weapons retain their existing following/custom-flash behavior. No hit detection, trajectories, damage, ammunition, timing, reload, sprite, pivot, palette or effect-art changes. Glow follows the same flash as before.

## Exact files changed

- Assets/Scripts/Input/WarehouseTouchControls.cs — posture-based display, disabled availability, pointer safety and non-overlap.
- Assets/UI/MissionControls.uxml — add missing blind-fire control using existing style.
- Assets/UI/WarehouseControls.uss — disabled control opacity only.
- Assets/Scripts/Combat/CombatFeedback.cs — opt-in world-shot flash anchor.
- Assets/Scripts/Enemies/SamuraiCombat.cs — low-crate descent threshold only.
- Assets/Prefabs/Enemies/Samurai.prefab — jump15, authored through Unity APIs.
- Assets/Prefabs/Player/WarehousePlayer.prefab and Assets/Prefabs/Enemies/Larper.prefab — explicit Glock flash references, authored through Unity APIs.
- Assets/Editor/Hotfix00253PlaytestSetup.cs (+Unity-generated meta) — explicit idempotent authoring.
- Assets/Tests/PlayMode/Hotfix00253PlaytestTests.cs (+Unity-generated meta) — six focused tests.
- DEVELOPMENT_LOG.md and TestResults/Hotfix00253Followup* evidence.

## Verification

**63 unique targeted checks passed:54 selected Play Mode tests and9 selected Edit Mode tests.** Reruns are deduplicated, not added to the totals. No full-project suite was run.

Six new tests exercise both real HUD layouts, standing/crouched states, visible-disabled behavior away from cover, pointer rejection, normal-FIRE clearance, usable-cover pointer firing and release on exit; player Glock flash/tracer origin+orientation through12aim/facing cases with recoil/translation; enemy Glock alignment; unchanged carbine behavior; measured jump-rise gain/landing with other enemies' jump values unchanged; and a single jump onto a3.2-unit elevated platform without spending an extra impulse.

Existing regressions passed: CameraSamuraiHotfixTests6/6 (actual warehouse detection, aggro persistence, crate pursuit/melee and platform replanning); ceiling rejection, wide-gap double jump and warehouse step traversal; Sprint022CoverTests12/12; Sprint0025Tests8/8; MovementCompletionTests11/11; exact transformed-muzzle/shot-line test; WeaponPresentationTests; Hotfix00254Tests6/6; Sprint0244RigTests2/2; CombatRulesTests7/7. The initial autonomous crate-pursuit failure is retained in raw history; the complete six-case warehouse suite passed after the descent fix. Existing test assertions were not changed.

Compilation completed with no compiler errors. Post-test Console ground truth was0errors/0warnings. A subsequent broad MCP inspection logged exactly `Failed to handle /api/exec request: Main thread operation timed out after 10000ms`; smaller read-only retries succeeded. Final Console therefore retains that one Pipeline tooling error and0warnings, not a compilation/gameplay exception. No logs were erased to hide it.

Native checks confirm Samurai15 versus Larper12/fireworks12, all3Demo Samurai15 and0missing scripts in player/Larper/Samurai/fireworks prefabs. Protected hashes:16/17 byte-identical, with only the intentional SamuraiCombat descent change;0unexpected changes. This includes weapon definitions, AttackExecutor, reload sources, player movement/camera/rig source, Larper AI/morale and all3warehouse scenes. MainMenu remains clean/open with Play Mode stopped. No stage/commit/push. Raw history/latest results: Hotfix00253Followup.json; protected before/after manifests alongside it.

## Manual acceptance

1. On Android, crouch away from cover: blind fire must be visible/dimmed, ordinary FIRE and joystick still usable with no touch overlap. Stand: blind fire disappears. Repeat beside cover and fire/peek/leave while holding a pointer. Check safe areas and joystick scales on hardware.
2. Chase Samurai around the actual warehouse crates and upper platforms. Confirm higher jumps feel threatening, land cleanly, avoid repeated blocked arcs, retain fair melee telegraphs and acquire only after visible entry.
3. Fire the Glock left/right, horizontally/up/down/diagonally, moving, crouching, peeking and during recoil. Confirm one coherent flash→tracer→impact. Compare the unchanged carbine and reloads.
4. Smoke-test ladders/camera, double jump/midair slide, cover, Larper suppression/retreat and normal encounter progression. Automated checks are controlled evidence; human feel and Android performance/ergonomics remain unverified.
