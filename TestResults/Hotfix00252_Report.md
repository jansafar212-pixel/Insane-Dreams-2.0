# AKUMU 2 — Hotfix 0025.2
2026-10-23. **Implemented by Codex — awaiting playtest verification.**

## Diagnosis and changes
The player's pistol used the generic gun-child magazine animation: a sinusoidal offset moved the magazine independently of the support hand, with no release physics. A player-only PistolMagazineReload component now uses the existing arms and magazine socket: seated until 20% of the original reload clock, extraction through 40%, release at 42%, replacement visible from 58%, inserted by 88%, regrip through completion. Fixed-length arm positioning keeps the support hand on the held magazine. The released old magazine uses a prewarmed, maximum-three Rigidbody2D pool, gravity, geometry-only collisions and four-second lifetime. Pooled bodies resume from their Transform at the hand, rather than a stale physics origin. Cancellation/switching clears the replacement pose; WeaponCycle remains the only ammunition authority.

BackupPistol now reuses the existing approved Glock sprite and extended magazine/socket already used by LarperBuzzbox. No new art, rig proportions, weapon statistics, NPC magazine debris, or carbine changes. The handler explicitly accepts only the two player pistol definitions and returns immediately for the carbine.

Actual AttackExecutor hitscan bursts demonstrated that close overhead segments already register and saturate pressure. The problem was downstream: the optional retreat roll consumed a four-second window even when it failed; the ally-reaction compatibility gate excluded reloading/cover-holding actors; and failed long retreats called BeginReload, leaving a frightened actor stationary even with ammunition. Crouch and hesitation alone could mask the accumulated pressure.

Sustained high pressure now provides an independent, bounded evasion opportunity after at least three credible misses over .45 seconds. It uses existing withdrawal/cover states and reservations, interrupts a reload only after a valid escape is selected (without transferring ammo), and holds genuinely protective occupied cover. When the normal long retreat is blocked, existing reachability/cover checks try a reserved short escape on either side (minimum one unit, open-ground reach capped at two). With no valid escape, a brief hesitation replaces the misleading full-magazine reload. No teleportation or new AI framework.

The rare existing panic decision retains its own eligibility/probability/cooldown before ordinary sustained evasion. Sustained evasion does not publish social events or seed a collapse chain. Active Samurai retains brief hesitation and pressure registration but bypasses Larper crouch/withdraw/panic branches. Existing suppression speech channels gain two lines, subject to their existing cooldown and maximum two bubbles.

## Parameters
| Parameter | Before | After |
|---|---:|---:|
| Sustained guaranteed evasion | absent; optional retreat only | high pressure + 3 misses + .45 seconds |
| Sustained evasion retry | absent | 1.5–2.2 seconds / personality susceptibility |
| Short suppression escape | absent | minimum 1 unit; open-ground reach up to 2 units |
| Genuine-cover extension | absent | 1.5 seconds |
| Failed suppression escape | stationary reload fallback | .6-second hesitation, bounded retry |
| Magazine release / cleanup | no physics / no released debris | 42% clock / 4 seconds, capacity 3 |

Existing radius1.5, gain58 with proximity weighting, moderate14/high42/max100, decay12/s, quiet gap.8s, optional retreat70%/4s, personality susceptibility1.25/1/.75 and confident recovery multiplier1.2 remain unchanged. Existing full-panic chance (normally8%), minimum45-second panic cooldown, morale/cascade probabilities, defensive-crouch tuning and player pressure remain unchanged.

## Files changed for this hotfix
- Assets/Scripts/Enemies/EnemyCombat.cs — sustained evasion and failed-path handling; Samurai fear exclusion.
- Assets/Scripts/Visuals/PistolMagazineReload.cs (+ Unity-generated meta) — player pistol hand exchange and pooled debris.
- Assets/Scripts/Visuals/CharacterVisualDriver.cs — calls the pistol-only presentation after existing pose.
- Assets/Editor/Hotfix00252Setup.cs (+ meta) — repeatable native Editor authoring.
- Assets/Prefabs/Player/WarehousePlayer.prefab — pistol component and explicit definition references.
- Assets/Data/Items/BackupPistol.asset — existing Glock sprite/magazine references only.
- Assets/Data/Enemies/Personalities/{Ordinary,Cowardly,Overconfident}.asset — two suppressed speech lines.
- Assets/Tests/PlayMode/Hotfix00252Tests.cs (+ meta) — focused regressions.
- DEVELOPMENT_LOG.md; TestResults/Hotfix00252* — report, raw evidence and controlled presentation review.

Pre-existing modified files from earlier work are retained. CutoutRig.cs, WeaponCycle.cs, WarehouseCarbine.asset and both native carbine sprite assets are checked against their hotfix-start SHA256 hashes.

## Verification
**78 Edit Mode + 84 unique selected Play Mode tests pass (162 total).** Seven new focused cases cover actual overhead hitscan registration/movement/decay, interruption of reloading without ammo transfer, short blocked-route alternatives, distant/clipped shot rejection, actual near-miss registration on Samurai without cowardly transitions, hand attachment/release/gravity/floor collision/lifetime/pool bounds/ammo completion, and unchanged carbine poses.

Existing suites executed: Hotfix00251Tests, Sprint0224Tests, Sprint0223CombatTests, Sprint020Tests, Sprint019Tests, Sprint012CombatTests, Sprint0246Tests, Sprint0025Tests, CameraSamuraiHotfixTests, Sprint0225Tests and Sprint0223MotionTests; all Edit Mode tests. The affected suppression suites and focused tests were rerun after the final panic-priority correction. Unique counts use the latest outcome per test rather than adding reruns.

Initial failures exposed a pooled-body pose reset, a too-specific direction assertion on a valid short escape, and the new evasion branch masking the existing rare panic opportunity. These were corrected; raw run history is retained in Hotfix00252.json. The Samurai test explicitly confirms that near-miss events arrived. Native evaluation also needed the Unity6 EntityId API rather than its obsolete InstanceID predecessor; that diagnostic script was corrected, with no project compilation failure.

Final compilation is up to date; current Editor Console ground truth: zero errors/zero warnings. Retained historical MCP log counters are separate. Native prefab inspection finds zero missing scripts/object references in WarehousePlayer, Larper and Samurai. Android build **configuration dry-run** is valid; this is not an APK build. Hotfix00252_Verification.json contains the native checks. The five hotfix-start carbine hashes match exactly (Hotfix00252_CarbineHashes.json).

Hotfix00252_Reload.png is an inspected, controlled static storyboard at phases0/.2/.3/.41/.5/.65/.8/1 for both facings. Released magazines remain at the sampled release point in this frozen presentation; falling, collision and cleanup were verified by actual Play Mode physics tests. This is not a human normal-speed gameplay review. No new artwork was generated.

MainMenu remains clean/open with Play Mode stopped, its startup scene restored and run-in-background off. No personal campaign save was used for captures; no scene changes were needed. No stage, commit or push.

## Manual acceptance still required
1. Equip either player pistol; reload standing/crouched, moving, facing both directions, aiming up/down, near a crate and on a platform. Check the hand-held exchange, released magazine fall, cancellation/switching and final HUD ammo at normal speed.
2. Fire real short and sustained bursts just above Larpers at different elevations. Look for flinch/duck then actual retreat or useful cover, differing personality timing, no rapid oscillation, and recovery after fire stops.
3. Repeat against blocked escape routes and protective cover. Enemies must not cross walls or freeze into a fake reload; impossible escape layouts cannot guarantee physical retreat.
4. Check rare panic/cascades, speech readability/cap, normal firing/reloading, Samurai aggressive melee, ladder camera, spawning/persistence and player double jump.
5. Verify Android-device readability, touch controls and performance. No APK/device playtest or performance benchmark is claimed.

