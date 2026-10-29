# Level 1 combat and enemy AI hotfix — 2026-10-25

Implementation resumed from SPRINT_RESUME.md; the completed First Operation layout was not regenerated. User confirmed use of the documented brief constraints; no additional numerical simultaneous-attacker cap was supplied.

## Implemented changes

- Population 13 -> 15 (+15.38%): groups [3,4,3,3,2], 12 Larpers plus Samurai, Czech fireworks expert and Officer. Opening trio and geometry unchanged. Added a Cowardly storage Larper at x10/floor0 and an Ordinary command-approach Larper at x129/floor3.2. Authored persistent occupants reuse the existing spacing/reservation systems.
- Mission-only Samurai tuning: speed 7 -> 9.5; detection reaction .04 -> .02s (jitter .02); strike recovery .7 -> .42s; platform sample .3 -> .2s; jump cooldown 1.2 -> .85s. Pursuit can resume during strike recovery when the player escapes melee range; the next strike still waits. Existing small collider, navigation validation, jump speed15, .4s wind-up, failed-jump backoff, HP/damage and viewport acquisition remain.
- Officer: same scoped weapon asset/reference becomes Desert Eagle; damage18 ->42, interval1.4 ->2.4s, magazine6 ->7, impulse .9 ->1.5, reload stays4s. Existing compact pistol sprite/sounds reused; extended-mag overlay removed. No new or dedicated Desert Eagle artwork/audio claimed. Shot-anchored flash and volume .65 use existing feedback.
- Every Officer shot uses a red two-point laser: 1s warning, final .2s committed direction, identical shot direction with no fresh spread roll. Laser clips at solid geometry, cancels offscreen/death/disabled combat/stale requests, and does one geometry ray only while active. After firing: 2.4s cooldown, plus .8s while the assigned command ally is alive within14 units. A fresh warning follows cooldown; practical stationary cadence is approximately4.2s supported /3.4s alone, before other AI interruptions.
- Cowardly Officer: close retreat range4 ->6; retreat retry4–6 ->2.5–4s; preferred range7 ->10; cover search7 ->9; withdrawal6 ->7; reposition chance .55 ->.9. Uses Cowardly personality with original Larper HP and existing suppression/reload/cover logic. Requests existing reposition after firing.
- Runtime tests found that the Officer's low barricade did not qualify as full crouched cover; the walking-only retreat fallback then rejected the short available corridor. Officer-only fallback now tries existing validated hop corridors at retreat commitment, preserving terrain clearance, destination reservations and finite cooldowns. Original minimum retreat/hop-width limits are retained. No navigation framework or teleportation added.
- Expert commits destination and selected fuse at preparation. Ordinary sight loss, jumping/moving targets and combat tick gaps no longer restart/cancel the animation. Its own Update finishes the pending throw. Damage, owner death, disabled combat, defeated/missing target and lost viewport permission still cancel safely; malfunction and recovery remain.
- Operation explosive maximum26 ->100; radius2.8 and impulse4 unchanged. Opt-in continuous curve at normalized distance0/.25/.5/.75/1 gives damage100/85/50/20/0. Minimum-effective threshold configurable, set0 for continuity. Centers at/beyond radius receive0 damage even if collider edges overlap. LOS, armor, deduplication, friendly/self damage, variable fuses, warnings, pooled VFX and existing knockback weighting remain.
- Officer defeat opens the exit independently of the added optional command guard. Cached Officer Health avoids a per-frame enemy search.

## Scope and files

Runtime: Assets/Scripts/Enemies/{OfficerShotTelegraph,EnemyCombat,SamuraiCombat,FireworksCombat,FireworkProjectile}.cs; Assets/Scripts/Missions/WarehouseMission.cs.

Native Editor authoring: Assets/Editor/Level1CombatHotfixSetup.cs; Assets/Scenes/Warehouse_Demo.unity; Assets/Data/FirstOperation/{Officer.prefab,OfficerPistol.asset,OperationFirework.prefab,OperationSamurai.prefab,OperationSamurai.asset}, including generated metadata. Original Samurai/other-level assets are preserved.

Tests: Assets/Tests/EditMode/{FirstOperationLayoutTests,Level1CombatHotfixRulesTests}.cs; Assets/Tests/PlayMode/{FirstOperationTests,Level1CombatHotfixTests,Sprint024FlowTests,Sprint024EnemyTests}.cs. Existing roster/damage expectations now reflect the requested balance; the old frozen Officer timing test disables retreat only in its isolated test fixture. Campaign exit test defeats only the Officer, leaving the optional ally alive.

Documentation/evidence: DEVELOPMENT_LOG.md, SPRINT_RESUME.md, TestResults/Level1CombatHotfix/. Existing dirty Sprint0028 work and four preexisting Resources test-data deletions are preserved. Nothing staged, committed or pushed.

## Verification

61/61 targeted cases pass (4 Edit Mode,57 Play Mode): rules2, saved layout2, focused combat5, Level1 integration6, campaign/shop/results12, movement/suppression9, cover12, legacy mission9, specialists4. Exact final suite results: Regression.json and the named suite JSON files. Failed initial runs are retained as diagnostic history and are not counted as final passes.

Compilation completed without errors. Current Console ground truth:0 errors,0 warnings; the tool's historical buffer still retains earlier compile/test/network records. FinalVerification.json confirms saved/clean Warehouse_Demo, Play Mode stopped,15 valid placed actors, assigned command ally,0 missing scripts and identical geometry. All605 protected original art/prefab/historical/other-scene files match pre-resume hashes. Nothing staged; existing uncommitted changes remain. OfficerLaser.png is an inspected controlled Play Mode capture, not human playtesting.

Specialist regression fixtures were brought up to date with already-existing global31.2 blast damage, always-on friendly fire and configurable effect lifetime. The throw fixture now uses actual enabled combat and a bound camera instead of disabling combat while manually ticking an attack. No global projectile tuning was changed to satisfy those stale tests. The Officer route failure was fixed in production code, preserving the original navigation limits.

## Manual playtest still required

1. Enter Warehouse_Demo through MainMenu/level selection/shop. Fight through the original opening trio and new storage guard; check gradual pacing and overlapping fire against the unchanged cover/layout.
2. Lead the Samurai onto the3.2-unit deck and around crates. Dodge a swing and retreat: verify fast re-engagement, readable .4s anticipation and no repeated impossible hops.
3. Continuously run/jump during the expert's preparation; confirm release toward the earlier target, understandable fuse warning, recovery and imperfect misses. Compare near/mid/edge blast danger with/without armor, including nearby enemies.
4. Approach the Officer from both sides and across the barricade. Verify visible1s red warning, dodge during the final locked .2s, substantial damage, safe retreats/repositioning and less waiting after nearby ally defeat. Leave the optional guard alive and complete the dispatch exit.
5. On Android hardware verify touch controls, safe areas, laser readability, combat frame rate and fairness through a full cautious run. No APK/device or human combat-feel verification was performed here.

Implemented by Codex — awaiting hands-on playtest verification.