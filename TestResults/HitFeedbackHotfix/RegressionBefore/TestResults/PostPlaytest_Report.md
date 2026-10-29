# Post-playtest hotfix / recovery completion

Implemented by Codex - awaiting human gameplay and Android-device verification.

## Recovered state

The requested weapon-panel, pause-icon, shop, syringe and player-death changes were already implemented when the recovery request arrived. Existing uncommitted work was retained. Recovery re-read Git status, source, native references, measurements and test evidence; it completed focused verification, corrected an outdated ragdoll test fixture, and finished documentation. No implementation was restarted.

## Completed requirements

| Item | Result |
| --- | --- |
| In-game weapon panel | Resolved 219.85 x 219.85 -> 196 x 161.78 UI units, about 34.4% less area. Less padding, 68 x 26 weapon icon, 14-unit weapon name, 36-unit magazine and 11-unit reserve text. Selection targets retain nominal 44-unit height and >44-unit width; live pointer switching verified. |
| Pause | Visible word replaced by two solid pixel bars representing the standard pause symbol. No font-glyph dependency. Existing approximately 88 x 40 face and 124 x 56 hit target, upper-center anchor, pause/resume callbacks retained. |
| Faintanyl shop | Separate **Procure Faintanyl** button and exact description **relax its not addictive, just habit forming**. One injection costs $75, configurable in DemoCatalog; existing two-injection capacity. Uses the existing wallet, purchase debounce, capacity checks, persistence and save-failure rollback path. Existing shop tabs/items remain. Button disables when full or unaffordable. |
| Faintanyl use | Existing death-screen revival action retained: consume one injection, restore in place, protect for two seconds. A .6-second configurable one-handed cartoon syringe gesture follows the real free hand and mirrors with facing. No duplicate animation/item consumption from repeated activation. Movement remains enabled after revival; firing/reload initiation pauses during the gesture, then resumes. Checkpoint/replay cancels the visual. |
| Player death | Copies the actual modular pose into the existing CartoonRagdoll prefab before any defeat/respawn observer can reset it. Final momentum and PhysicalImpact drive the collapse; stronger explosion impulses produce a stronger bounded reaction. Six-body player fallback uses existing terrain collision, joint limits, damping, sleep, velocity caps and global corpse lifetime/count bounds. Live rig hides; movement/combat/body simulation suspend through the existing revival path. |
| Recovery | Revival/checkpoint removes the player corpse and restores the live rig; movement, combat and root simulation recover. With no injections, automatic checkpoint recovery now waits 1.2 configurable seconds to allow a visible collapse. Existing empty-inventory regression was updated to wait for that deliberate delay, retaining its recovery assertions. Death overlay opacity .94 -> .45 makes the collapse visible while keeping existing buttons. |

The syringe is a new independently code-authored 16 x 7 flat-color Sprite, point filtered at 32 PPU, using the existing equipment palette. Historical Postava 1.png was actually inspected; originals and character modules/pivots remain untouched. No realistic anatomical/procedural detail or new animation framework.

## Previous completed hotfix preserved

All 12 protected hashes match the pre-hotfix snapshot (PostPlaytest_ProtectedBefore/After.json): mission UI stylesheet/controller, explosives combat and both explosive prefabs, player motor, shared ragdoll implementation/prefab, Larper prefab/AI, Warehouse_Demo and Warehouse_Testbed scenes. Native assets were authored through Unity APIs, not serialized YAML editing.

Resolved captures retain health 219.85 x 141.04, startup objective 224 x 136.89, pause visible 88.15 x 40.44 and touch 123.41 x 56. The previous 31.2 explosion damage, 3.2 radius, 1.05-second varied recovery and .85-second wind-up were not changed. Previous hotfix regression tests pass again.

## Files modified by this hotfix

- Assets/UI/WarehouseControls.uss; Assets/UI/GameFlow.uss
- Assets/Scripts/Progression/GameCatalog.cs, GameSession.cs, GameFlowController.cs, CampaignMissionBridge.cs
- Assets/Scripts/Combat/PlayerCombat.cs, PlayerRevive.cs, PlayerHealthRecovery.cs
- Assets/Scripts/Input/WarehouseTouchControls.cs (death-overlay transparency only in this hotfix)
- Assets/Scripts/Visuals/PlayerDeathRagdoll.cs, PlayerSyringeVisual.cs (new)
- Assets/Editor/PostPlaytestHotfixSetup.cs (new, explicit native asset authoring helper)
- Assets/Data/Progression/DemoCatalog.asset; Assets/Prefabs/Player/WarehousePlayer.prefab
- Assets/Art/PostPlaytestHotfix/FaintanylSyringe.asset (new; associated Unity-generated metas)
- Assets/Tests/PlayMode/PostPlaytestHotfixTests.cs (new); Sprint0025Tests.cs; ReactiveRagdollTests.cs
- ART_BIBLE.md; DEVELOPMENT_LOG.md; PostPlaytest evidence/report files under TestResults.

Other already-dirty files belong to earlier work and were retained. No staging, commits or pushes.

## Tests and evidence

**77 unique regression checks: 69 Play Mode passed, 1 Play Mode failed; 7 Edit Mode passed.** Reruns deduplicated by full test name. Do not interpret this as a completely passing project suite. Raw initial/final runs and latest results: PostPlaytest_Regression.json. The explicit legacy diagnostic is excluded from these gameplay totals.

| Suite | Latest result |
| --- | --- |
| PostPlaytestHotfixTests | 5/5 |
| Sprint0025Tests | 8/8 after isolated speech-test rerun |
| Sprint024FlowTests | 12/12 |
| ReactiveRagdollTests | 5/5 after correcting the fixture's simulation-body collection |
| Sprint0246Tests | 5/5 |
| MobileHudPyroTests | 3/3 |
| GraphicsUiReworkTests | 2/2 |
| Hotfix00253PlaytestTests | 6/6 |
| TraversalRegressionTests | 11/12 |
| Sprint022CoverTests | 12/12 |
| CombatRulesTests (Edit Mode) | 7/7 |

Focused checks exercised real pointer weapon selection, safe-area layouts at three simulated aspect ratios, real pause callback, native shop button text/description, wallet/stock/save changes, duplicate/full/unaffordable purchase rejection, exactly-once revival/animation, combat blocking/resumption during the gesture, ordinary and actual firework deaths, facing, collision/velocity bounds, empty-stock and chosen checkpoint recovery. Existing checks covered weapon/reload/ammo, full menu/shop/mission/results/save flows, two-second revival protection, enemy corpses, cover, ladders, double jump, touch ownership, Glock effects and prior explosives tuning/frequency.

Controlled runtime captures were generated and visually inspected: PostPlaytest_HudBefore/After.png, PostPlaytest_Shop.png, PostPlaytest_Syringe0/1.png, PostPlaytest_Death0/1.png. UI rect measurements: PostPlaytest_Before/After.json. These are Editor tests/captures, not a human playthrough or Android verification.

## Remaining issues / investigation limits

- **TraversalRegressionTests.LadderTopAndDiagonalArbitrationDoNotGenerateExtraJump**, line 81: expected upward velocity >3 after a diagonal ladder-adjacent gesture, observed 0. Reproduces with both new player presentation components disabled. Shared movement/traversal code was not changed; its underlying cause remains unresolved and was not masked by an unrelated movement rewrite. Other ladder/corpse traversal checks pass. Details: PostPlaytest_LegacyDiagnostics.txt.
- Speech-duration test initially failed, then passed in isolation without changing production speech or its test. Treat its timing sensitivity as an observed limitation.
- The old enemy corpse fixture inspected/stopped only its six root bodies, omitting eight physical limb segments. Diagnostics observed root max vx=0 while an articulated body reached ~12. Fixture loops now use CartoonRagdoll.Bodies, retaining velocity thresholds and expanding rest/finite-position checks to actual simulation bodies. Final whole enemy ragdoll suite passes; no production enemy physics changes.
- The previous hotfix's older suppression-crouch failure remains documented in MobileHudPyro_Report.md; it was not retested or redesigned in this hotfix.
- No APK build, Android-device run, device profiling or full-level human playthrough was performed. Fixed body/projectile counts and controlled tests do not prove device performance or gameplay/artistic acceptance.

## Manual playtest

1. On Android, check weapon labels/icons/ammo at normal device size; tap each slot, the pause icon and its surrounding hit area. Check safe areas and the chosen joystick scale.
2. In the shop, scroll to Faintanyl, buy after consuming stock, verify exact description, $75 charge, cap 2 and persistence after relaunch; ensure other purchases still work.
3. Die to bullets and an explosion, watch the physical collapse behind the death UI, revive facing each way and while moving, and confirm one syringe/one charge. Aim/fire/reload should resume after the short gesture.
4. Decline revival, retry, return to menu, and die with zero injections; confirm checkpoint position, health, gravity, movement and weapon state. Try platform/cover/ladder deaths.
5. Recheck the reported ladder-top scenario and normal warehouse combat. Profile frame time on a real phone with several corpses/blasts. Verify no undesirable body interactions or limb clipping.

Final Editor state: MainMenu clean/open, Play Mode stopped, scripts compiled; current Console has zero errors and one Unity AI Toolkit warning: "Account API did not become accessible within 30 seconds." The warning concerns Editor account/network access, not gameplay. Historical compiler output also includes existing obsolete-API warnings. Working tree remains dirty with this hotfix and earlier uncommitted work; nothing staged, committed or pushed.
