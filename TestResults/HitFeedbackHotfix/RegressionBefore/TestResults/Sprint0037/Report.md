# AKUMU 2 — Sprint 0037: combat impact and damage feedback

Implemented 29 October 2026 in the existing Unity project. No commits or pushes. Earlier reload, cover-grenade, wallet and HUD transparency work is preserved.

## Existing architecture and extension

Inspection found one gameplay Rigidbody2D and no living Joint2D components on the production character prefabs. HitReaction2D previously supplied a small timed lean and an optional timed knockdown; the articulated cutout rig owned normal poses and CartoonRagdoll owned dynamic corpses. This sprint extends that same HitReaction2D owner instead of introducing another ragdoll controller.

Each configured actor prewarms six collider-free Rigidbody2D/HingeJoint2D angular springs and one kinematic anchor. The cache is reused across hits, disabled after the response settles, and destroyed with its actor. It creates no sprites, materials, corpse instances or hitboxes. Angular impulses and a damped return spring drive the existing torso, head, arm and leg pivots. Upper-body pivots follow the torso around the existing hip anchor; weapon grips follow the actual hand while the barrel direction is preserved. Living character collision and movement remain owned by the original motor. Death still creates the original bounded CartoonRagdoll through the original defeat path.

The existing nearest-body-region lookup is exposed by BodyRegionDamage.RegionAt, retaining its six/14 cached renderer bounds rather than adding per-pixel collision. Hit points and incoming direction select and bias the physical response. Return torque and angular velocity are bounded; visual angles are capped at 26 degrees for ordinary enemies, 20 for Samurai, 18 for Officer and 8 for the player, with an additional 12-degree torso cap. Player response is reduced further. Sleeping caches do not run pose work or physics simulation.

## Intensity, accumulation and recovery

WeaponDefinition.hitReactionStrength is independent of HP damage and the existing corpse impact impulse. AttackExecutor carries it with PhysicalImpact, scaled by the existing distance falloff. Existing damage, faction attribution, cadence, ammunition, accuracy/spread, range and corpse forces are unchanged.

| Weapon | Living reaction strength |
|---|---:|
| Basic pistol | 0.65 |
| SMG / Buzzbox | 0.40 |
| Assault carbine | 1.10 |
| LMG | 1.35 |
| Shotgun | 0.50 per accepted pellet |
| Desert Eagle | 2.60 |
| Sniper | 3.20 |

Accepted hits are combined once per physics step, including all shotgun pellets, and the batch is capped at 3.2 before the archetype gain. This preserves damage from every pellet without restarting a full-body reaction for each pellet. Armor reduces reaction strength without making protected impacts invisible. Rapid successive batches have diminishing disruption; balance decays at 1.15 units/second.

Heavy responses use a 0.55-second cooldown. While a new full-body heavy response is suppressed, the struck region still flinches. Local balance thresholds are 4.5 for Larper/Fireworks, 6 for Samurai and 7 for Officer. Samurai and Officer have susceptibility 0.85 and 0.8 respectively. These are stability settings, not HP, fear, suppression or currency.

An eligible grounded enemy can use the existing collapse/recovery pose when its balance threshold is crossed. Recovery lasts 0.65 seconds for ordinary enemies, 0.50 for Samurai and 0.55 for Officer. The deadline cannot be extended by incoming hits. A three-second fall cooldown and 1.2-second post-recovery grace prevent repeated falls from locking combat indefinitely. Route traversal is not interrupted. EnemyCombat remains enabled, keeps its perception/fear/suppression state, and briefly yields locomotion/attacks while the existing reaction reports KnockedDown. It resumes its normal decision loop afterward. A living collapse never sets Health.IsDefeated and never awards money or spawns a corpse.

Production player reactions never enter this control-locking path, regardless of impact strength. Reload, healing, cover, ladder and parachute poses keep their handling limbs authoritative and reduce upper-body displacement. The old explicit demo reaction remains available for legacy test fixtures; production prefabs/scenes are configured to the new living mode with a zero player knockdown chance.

## Player damage feedback

Health publishes a separate DamageOutcome with actual HP lost and armor absorbed. The original Damaged/Defeated notifications and damage calculations are retained. Invulnerability returns before this event, so blocked attacks produce no successful-hit pulse. Armor-reduced hits produce a weaker pulse reflecting their actual outcome.

PlayerDamageFeedback owns four bounded direction pulses, one short edge pulse, one HP flash and the camera throttle. Direction comes from the accepted impact vector or source position. Environmental/source-less neutral damage uses nondirectional feedback. Indicators fade smoothly over 0.55 seconds; the edge fades over 0.32 seconds with at most 18% alpha. Only four narrow strips and four small markers are rendered; the middle of the screen is never filled red. The markers respect the existing safe area and ignore pointer picking.

The existing HP bar flashes for 0.13 seconds at its existing 85% alpha. HP/ammo/objective lettering stays opaque. Accepted hits invalidate the HUD readout cache for the next UI frame. Pause and terminal screens hide the hit overlay. Existing transparent panel values, touch targets and control layout remain.

Particularly strong actual HP loss can request a camera impulse of 0.025 world units for 0.09 seconds, at most once per 0.65 seconds. It reuses the existing camera shake channel, without changing tracking or input. Strength, duration, cooldown, opacity, recovery and decay are serialized. PlayerDamageFeedback.CameraShakeScale supports reduction or disabling (0–1, including the Inspector); no new save format or settings menu was introduced.

## Tracers

The existing weapon-specific tracerScale field and pooled renderer are reused: player Desert Eagle is 1.6× and sniper is 2.2×. The configured player baseline is 0.04 world units, giving mean start widths 0.064 and 0.088 before the existing ±15% variation. Direction, raycasts, taper, lifetime, colors, fade, pool size and hitboxes remain unchanged. No new material or projectile collider was added. The sniper aim laser remains separate from the shot tracer.

Officer uses its own prefab AttackExecutor with its existing 0.08 baseline width and enemy tracer color; its weapon is a separate asset from the player Eagle. Neither its special width/color nor the player's existing friendly cyan palette was copied or replaced. ConfigurationAudit.json records the actual values.

## Actual checks

FinalTestRuns.json records 133 unique selected tests: all 15 EditMode and 118 PlayMode checks pass. The new impact suite contributes 16 checks, including four directions, actual moving regional springs, all four archetypes, damage-conserving pellet batching, rapid SMG fire, bounded recovery, navigation resumption, death transition, armor/helmet outcomes, revive protection, stable player movement during reloading and repeated hits, HUD picking/transparency, camera throttling/disable, diagonal tracer alignment and fade, and physics-cache destruction before its actor.

Existing arsenal/reload, cover, grenade/wallet, corpse ragdoll, traversal, personality/suppression, menu/mission/results, complete Level 1 route and Officer bunker checks also pass. CombatBefore.json and CombatAfter.json are identical, and the final native audit compares the current configuration to the same snapshot. No production HP, weapon damage, accuracy, cadence, magazine capacity or range rebalance was made. Native authoring used Unity Editor/Prefab APIs, not YAML/GUID edits.

Controlled captures of regional reactions, a living collapse, all four damage directions, critical HP, archetypes and pistol/Eagle/sniper traces are in this folder. They were inspected. The Editor-only FPS strip is removed only in the capture fixture so it does not obscure the above indicator; those images are not Android performance measurements or human gameplay/artistic acceptance.

DevelopmentTestHistory.json retains early failures. The tests exposed a real stale-angular-state problem after waking a reused spring and a real scene-unload ordering problem; both were fixed and explicitly retested. Other early test failures were fixture issues: missing pre-Awake armor in an isolated player and assuming the C# tracer default rather than the actual serialized 0.04 width. The final checks use the real configuration.

The Editor also disconnected with a recurrence of its previously recorded native ADTM audio-mix failure before export. The Editor reconnected, saved code/configuration was re-audited, and export was restarted. This native Editor audio issue is not claimed fixed by this sprint. It should be kept distinct from the corrected managed scene-unload exception.

## Changed files

The complete absolute-path inventory is in [ModifiedFiles.txt](</C:/Users/jansa/Documents/GitHub/Insane-Dreams-2.0/TestResults/Sprint0037/ModifiedFiles.txt>). It covers the health outcome/impact/reaction components, existing character pose and AI recovery integration, the new player feedback component, HUD binding/styles, native authoring helper, eight player weapon assets, six prefabs, three warehouse scenes, tests and documentation.

Only reaction components/configuration were authored on characters; original graphics, pivots, terrain, AI personalities and roster remain. The old flow test now expects an ordinary player hit to preserve mobility; its terminal-state/control/persistence checks remain. New script metas were generated by Unity. Unrelated dirty files predate this sprint.

## Release and remaining manual validation

Android ARM64 release build build_3992cd59f9ac succeeded in 197.247 seconds, with 0 errors and 974 warnings (existing Pipeline/Sentis categories; no UI stylesheet warnings). [APK](</C:/Users/jansa/Documents/GitHub/Insane-Dreams-2.0/build/Sprint0037/AKUMU2-combat-impact-release.apk>): 39,837,579 bytes, nondebuggable, package com.GAMES.AKUMU2, version0.12/code1. SHA256:3741ddf3f08dbc1a58cbc8e225172fe36c3e619abbc7aaedaa9423a6a274f034. This filename/hash identifies the new build; its unchanged version label alone does not.

BuildStatus.json, PackedAssetProof.json, GeneratedCodeProof.txt and ApkVerification.json confirm the updated weapon/prefab/scene assets and compiled living reactions, damage outcome, player feedback and shake reduction. The previous release-wallet marker is also present. adb devices returned an empty list; the APK was not installed or tested on a physical Android device. No source/native asset edits were made during export.

Human combat feel, artistic acceptance, sustained mid-range Android frame time, physical touch/aim stability and device-specific audio/lifecycle behavior still require manual playtesting. Editor automation and bounded caches do not establish device performance. Physical Android testing will only be reported if a device is actually connected and tested.
