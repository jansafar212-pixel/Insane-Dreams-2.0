# AKUMU 2 — Sprint 0025

2026-10-23. **Implemented by Codex — pending playtest.** Automated verification is described separately below. Human acceptance and Android-device performance have not been verified.

## Changes and root causes

- **Samurai:** viewport eligibility still used the physical movement box. It now uses the existing cached animated body-renderer envelope, excluding the sword. This separates visible activation from physical collider dimensions without enlarging either hitbox. Existing line-of-sight checks, short reaction, persistent acquired aggression, melee telegraphs and physical platform navigation remain. Actual small-body detection at different elevations, walls, warehouse cover, elevated steps, failed-edge recovery and committed flight passed existing tests; no additional navigation rewrite was justified by those scenarios. Unrepresented stuck cases still need a reproducible playtest location.
- **Camera:** retained the already implemented directional damping, commitment interval, normal one-third/two-thirds composition and fixed ladder behavior. Rapid reversal and ladder tests passed. No camera or ladder gameplay code changed in this sprint.
- **Player melee:** tracers were already restricted to ammunition attacks, but the shared Fired feedback always emitted gunshot/muzzle cues and the player had no dedicated swing. Feedback now distinguishes ammunition attacks; the wrench swings through the existing arm/hand attachment for .22 seconds. A short contact sweep uses radius min(.16, range × .25), with sweep length reduced by that radius so total reach stays within the weapon's configured range. Solid blocking, single-target contact damage, enemy hurt feedback and material impacts remain; firearm raycasts and statistics are unchanged.
- **Speech:** actual firing and cover selection lacked direct callouts, while witnessed defeat/panic speech could be lost to the long generic-chatter gate. Added event-driven opening-fire and cover lines; routed ally/panic lines through the existing tactical queue. Pending panic takes priority over witness/suppression, then reload/withdrawal, then ordinary callouts. Existing active bubbles are never replaced. Hard cap two, existing configurable 3.2-second display (clamped 3–4), 3.5-second per-actor callout spacing and six-second pending expiry remain. Opening-fire opportunities are limited to one per eight seconds. No new speech manager.
- **Suppression:** enemy accumulation, personalities, fear/panic probabilities and rare bounded cascades remain unchanged; tactical speech makes their existing reactions more observable. Added presentation-only player pressure from real clipped hostile shot segments, with distance weighting and solid occlusion. A thin amber screen border and quiet short near-miss sound accumulate and decay. No movement, aim, damage or camera penalty; no per-frame physics scan.
- **Faintanyl:** previously only part of the healing HUD caption; no separate item/effect existed, and lethal damage immediately respawned the player. Per the user's clarification, added a centered death-time revive button and checkpoint alternative. Revival consumes one injection, restores health in place and grants exactly two seconds of invincibility. It does not call checkpoint recovery or refill ammunition/healing. Inventory is capped at two, defaults to two for playtesting and is shown separately from duct tape. Campaign consumption saves immediately; checkpoint recovery and mission retry do not replenish campaign stock. Legacy saves receive the two-injection default. Empty inventory retains existing checkpoint fallback. No new shop offer or injection pickup was requested/added. The death overlay stays above battlefield speech and below an active pause menu.
- **Android blind fire:** replaced the ambiguous always-present control with a dedicated BLIND FIRE control shown only in cover; normal FIRE remains outside cover. Entering blind fire clears aim intent, preserving existing blind-fire stance. Leaving cover releases its captured fire input. Pointer-release ownership is checked against the active button so old-control events cannot release another control.
- **Wallet:** existing configurable pickup radius and native prefab trigger increase moderately, 1.62 → 1.95 world units (+20.4%). Existing LOS, reward identity and duplicate prevention remain.
- **Existing graphics/equipment:** retained HeroRefined, current Larper/Samurai modular art, native CrookedCarbine and existing flash/firework presentation. Armor disappearance on depletion/removal remains the earlier fix and passed regression tests. No character-art regeneration, hitbox enlargement, animation-rig redesign, or weapon-balance changes. Duct tape remains continuous healing with a 200 HP-equivalent baseline reserve.

## New player-pressure settings

Inspector configurable: near-miss radius 1.2 units; maximum close gain .22; decay .45/second; gain interval .08 seconds; maximum sound volume .07. Pressure is bounded 0–1. Sound spacing is .3 seconds, with a .08-second cue; screen border is three pixels at at most .12 alpha. Physics occlusion runs only on eligible shot events with a reused buffer. Ordinary enemy fear/full-panic/cascade tuning was not increased.

## Modified files

Paths are relative to the project root; Unity-generated metadata accompanies new assets/scripts.

| Area | Files |
| --- | --- |
| Combat | `Assets/Scripts/Combat/AttackExecutor.cs`, `CombatFeedback.cs`, `PlayerHealthRecovery.cs`, new `PlayerRevive.cs`, new `PlayerSuppression.cs` |
| AI | `Assets/Scripts/Enemies/EnemyCombat.cs`, `SamuraiBodyHitbox.cs`, `LarperAwareness.cs`, `LarperPersonalityProfile.cs` |
| Player/UI | `Assets/Scripts/Visuals/CharacterVisualDriver.cs`, `Assets/Scripts/Input/WarehouseTouchControls.cs`, `Assets/UI/WarehouseControls.uxml` |
| Inventory/save | `Assets/Scripts/Inventory/PlayerSupplies.cs`, `WalletPickup.cs`, `Assets/Scripts/Progression/CampaignMissionBridge.cs`, `ProgressStore.cs` |
| Authoring/assets | new `Assets/Editor/Sprint0025Setup.cs`, `Assets/Prefabs/Player/WarehousePlayer.prefab`, `Assets/Prefabs/Pickups/WalletPickup.prefab`, new `Assets/Audio/NearMissPressure.wav` |
| Tests | new `Assets/Tests/PlayMode/Sprint0025Tests.cs`; updated `CombatIntegrationTests.cs`, `LadderCameraHotfixTests.cs`, `Sprint012CombatTests.cs`, `Sprint014MissionTests.cs`, `Sprint0222MovementTests.cs`, `Sprint022CoverTests.cs`, `Sprint023Tests.cs`, `Sprint024FlowTests.cs` in the same folder |
| Evidence | `DEVELOPMENT_LOG.md`, this report, `TestResults/Sprint0025.json`, `TestResults/Sprint0025_Revive.png` |

## Verified by tests

**78 Edit Mode + 145 unique Play Mode tests passed; zero remaining failures in the selected tests.** Counts use the latest result for each full test name, not repeated runs. Raw run history, including corrected failures and successful reruns, is preserved in `Sprint0025.json`.

Selected Play Mode coverage: Sprint0025, CameraSamuraiHotfix, LadderCameraHotfix, Sprint0247, Sprint0248, Sprint020, Sprint023, Sprint0245, Sprint0225, Sprint0224, Sprint014Mission, Sprint024Flow, Sprint0223Motion, FocusedHotfix, Sprint0244, Sprint019, CombatIntegration, Sprint012Combat, Sprint0222Movement, Sprint022Cover, Sprint0223Opening and Sprint0248Scene.

New checks cover: local melee hits/range/swing and absent firearm cues; preserved gun ammo/flash; actual hostile near-miss bursts, cover occlusion and pressure decay without movement restrictions; conditional blind-fire control and release; actual opening-fire speech and readable scheduling; sprite-based Samurai viewport entry independent of a deliberately displaced tiny physical shape; keyboard and simulated touch revival; two-use stock, duplicate prevention, two-second invincibility, checkpoint fallback/decline, unchanged ammunition/duct tape; save defaults/round-trip; immediate campaign persistence and retry stock preservation.

Existing tests demonstrate small Samurai perception/persistent pursuit and physical platform transitions, two-bubble readability, bounded three-recipient cascades/global cooldown/solid occlusion, armor removal, camera changes, double jump, touch controls, three opening enemies, spawning/persistence, weapon combat/reloads and campaign flow. This is targeted coverage, not a claim that every historical Play Mode fixture was run.

Fixture corrections preserve the relevant assertions: checkpoint-specific cases explicitly use zero injections; wallet boundary follows the larger radius; result banking compares actual mission earnings (nearby drops can legitimately be collected); blocked reload waits through existing .55-second reaction plus jitter instead of asserting at .55; conditional touch tests target the active button and allow normal input/layout updates. The ladder-camera isolation offset changed 71,000 → 7,100 units to avoid float-precision damping stalls, without changing gameplay camera/ladder code or tolerances.

Unity 6000.6.6f1 compiles successfully with no compilation failure. Final Console ground truth: zero errors and one Unity AI Toolkit account-service warning: “Account API did not become accessible within 30 seconds.” Existing obsolete FindObjectsSortMode compiler warnings also remain in retained history; no unrelated API migration was undertaken. Android build **dry-run configuration validation** passes with no validation errors. **No APK was built or tested on a device.** All three warehouse scenes are clean, contain the inherited revive/pressure components and have zero missing scripts. Build entry remains MainMenu, followed by Warehouse_Demo and Warehouse_Mission01.

`Sprint0025_Revive.png` is a controlled runtime UI capture in Warehouse_Demo with enemies temporarily disabled in memory and no campaign session. It confirms readable centered revival controls and separate inventory/healing HUD. A capture exposed speech overlapping the death caption; the final overlay ordering fixes that. This is not human gameplay acceptance. Temporary capture objects/settings were discarded; MainMenu is open, clean and outside Play Mode, with its original startup-scene setting and background ticking restored off.

## Pending playtest

1. MainMenu → Warehouse_Demo: enter Samurai view, dodge telegraphed attacks, then lead him around crates and onto/under representative platforms. Revisit a specific stuck location if one remains; check persistent pursuit after camera exit.
2. Reverse direction rapidly, run/jump/cover, and climb/dismount ladders. Assess camera comfort and unchanged movement responsiveness.
3. Select melee; verify a readable wrench swing, close contact hurt response, no gun flash/tracer, and no hit outside range. Switch back to guns and reload normally.
4. Exchange near-miss bursts: assess subtle player sound/border and recovery, enemy hesitation/duck/retreat speech, maximum two readable bubbles, and uncommon full panic.
5. On death, inject twice across separate deaths. Confirm revival in place, a two-second safe window, no ammo/healing refill and persistent spent stock after retry/relaunch. Also decline an injection and exhaust stock to check checkpoint fallback.
6. On Android hardware, verify real multi-touch blind fire/aim/release/cover transitions, revive tap usability, UI scale, sound level and frame rate. No device profiling or human artistic approval was performed.

No stage, commit or push. Existing uncommitted work preserved. Status remains **Implemented by Codex / Verified by tests / Pending playtest**; “Complete” awaits human gameplay acceptance.
