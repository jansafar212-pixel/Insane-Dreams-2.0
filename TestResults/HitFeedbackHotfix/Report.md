# Hit feedback / AA-12 hotfix — resumed 2026-10-29

## Recovered work

The interrupted changes were present and preserved: CombatFeedback damage/death guards, the new HitFlash shader, batched living-hit particles, CharacterVisualDriver automatic-shotgun recoil timing, and the HitFeedbackHotfixSetup native authoring helper. The native material and AA-12 magazine assets had not been authored/wired. There were no completed hotfix test results. The resumed Editor was ready, with sufficient disk space; no files were deleted to free space, and no Git reset, staging, commit or push was performed. All older Sprint 0039/0040 work remains in the dirty checkout.

## Root causes and final fix

### Living full-body flash; never a death flash

The old CombatFeedback.Hurt accepted lethal damage and had no dead-health guard. Its renderer list came from Awake before the complete rig/skin initialization was guaranteed. Death feedback did not synchronously cancel the active tint, while the segmented corpse builder copies forearm/hand/lower-limb colors and materials directly from the living rig. Thus transition order could copy temporary feedback into the corpse. Ordinary multiplicative sprite tint also made blue/dark artwork dark instead of consistently red.

The existing CombatFeedback remains the single owner. Start refreshes a cached list of the complete cutout rig after child Awake/skin binding, including articulated limbs and overlays. A surviving accepted hit applies the short native .15-second red flash immediately; repeated hits refresh the same deadline. One shared Sprite-compatible unlit silhouette material uses the original sprite alpha, so every visible colored pixel can actually become red. Original colors and shared materials are restored at expiry and disable.

Health sets IsDefeated before DamageResolved. CombatFeedback now cancels and restores in that event, before Damaged/Defeated observers can create a corpse. Hurt, Tint and LateUpdate also reject defeated actors; Defeat cancels again defensively. No coroutine, delayed material reset or weapon-specific flash logic was added. The fallback death cue is normal white rather than a red cue. Lethal-first hits intentionally produce no red flash. Existing Health and death/ragdoll mechanics were not rewritten.

Subsequent pellets only refresh the deadline; they do not redundantly assign all sprites. LateUpdate reapplies the tint after poses only where a color/material differs. No per-hit material instantiation or hierarchy scan is used.

### AA-12 magazine reload

The runtime AA-12 already had LoadsIndividually=false and a correct magazine-transfer clock, but no magazine sprite and PhysicalMagazineReload=false. Its entire magazine was painted into the equipped gun sprite, preventing the existing exchange animation from detaching it. This was a missing presentation/data connection; the ammunition engine was retained after testing it.

Editor APIs extracted the existing 6x4 magazine pixels from the original 34x14 sprite into a detachable sprite, with a matching body sprite and socket (6/32,0). Recomposition is pixel-identical to the original. The icon/original art remains untouched. PhysicalMagazineReload=true, LoadsIndividually=false and no shell sprite connect the existing CutoutRig exchange animation. No new reload controller was introduced.

Capacity remains **8**, reload duration **3 seconds**, damage **8 per pellet**, cadence **.26 seconds**, pellets **7**. Empty, partial, insufficient-reserve, full/no-reserve, cancellation/restart and completion-idempotence tests cover ammo conservation. Play Mode verifies the moving old magazine, incoming replacement, both facings, firing prevention during reload, correct HUD, switching cancellation and subsequent reload completion.

### Punchier living feedback

- Existing enemy six-joint visual impulse coefficient: 160 to 175 (about 9%). Player parameters, damage, balance accumulation, fall probability, cooldowns, recovery and AI are unchanged.
- Existing pooled impact accents: one 2-chip light or 4-chip heavy burst per victim/rendered frame, .12-second lifetime, capped aggregate strength 3.2. Multiple shotgun pellets feed one burst. No new physics or per-pellet effect objects.
- Shotgun muzzle presentation is 25% larger through existing feedback; existing tracer, muzzle colors, environment material impacts and pools are preserved.
- Automatic shotgun visual recoil lasts .16 seconds instead of .12; sniper remains .35 and ordinary guns .12. This uses firing mode/pellets, not a weapon-name special case. Cadence, spread, aim, recoil statistics and ammunition clocks are unchanged.

Glock/SMG remain lighter; rifles/LMG and Desert Eagle retain their stronger native physical coefficients. Shotguns aggregate their pellets. Existing anti-lock protections remain: major cooldown .55 seconds, fall cooldown 3 seconds, recovery grace 1.2 seconds, bounded batch and diminishing rapid impacts. The existing physical/regional response controller was reused, not replaced.

## Verification and evidence

FinalTestSummary.json and FinalTestResults.json record the actual final selected cases. InitialPlayTestFailure.json preserves the initial fixture failure: SendMessage cannot deliver to the deactivated dead actor. The corrected test invokes the private observer directly to simulate a stale callback and exercise the production dead-health guard.

**Final result: 50/50 selected cases passed, zero failed.** Nine new Edit Mode cases and five new Play Mode cases plus 36 regression cases. Recompilation completed with no script errors. NativeAudit.json confirms shaderErrors=false, all seven audited prefab feedback material links present, and no missing scripts, missing flash-material links or dirty scenes in the three warehouse scenes. All sixteen native weapon definitions' audited balance/presentation-stat fields match the pre-application snapshot exactly.

New Edit Mode coverage (AA12ReloadHotfixRulesTests): nine cases, including exact reconstruction of original magazine/body pixels and ammunition conservation. New Play Mode coverage (HitFeedbackHotfixTests): all eleven actual player firearms through AttackExecutor, full-rig color/material restoration, rapid pellets, restoration before death observers, normal segmented corpse colors, lethal-first no-flash, stale callbacks, eight simultaneous victims, and AA-12 animation/HUD/switching.

Controlled captures use the established Android gameplay projection (1280x720, orthographic 4.329004), not a close-up Scene view. LivingFullBodyRed.png includes an assertion on actual rendered red pixels, in addition to renderer color/material assertions. LivingRestored.png, DeathNormalColors.png and Aa12InsertLeft/Right.png were inspected. These are controlled Editor captures, not human gameplay/art approval.

Regression suites cover existing falls/recovery and anti-lock behavior, regional directions/facing, arsenal behavior, transparent HUD/directional player damage, existing tracer/material particles, reload repair and cached HUD/pose behavior. Historical captures overwritten by these tests are preserved under RegressionArtifacts and restored to their exact pre-run bytes, including any pre-existing uncommitted evidence.

ProtectedRuntimeAudit.json verifies Health, WeaponCycle, HitReaction2D, PistolMagazineReload, CutoutRig and RagdollOnDefeat source are unchanged from the interrupted hotfix's original snapshot. WeaponBalanceBefore/After.json audit native weapon values. The higher enemy visual impulse is an actor feedback profile, not a weapon damage/stat change.

## Modified files

Runtime scripts:
- Assets/Scripts/Combat/CombatFeedback.cs
- Assets/Scripts/Visuals/CharacterVisualDriver.cs

Authoring, shader and tests:
- Assets/Editor/HitFeedbackHotfixSetup.cs
- Assets/Shaders/HitFlash.shader
- Assets/Tests/EditMode/AA12ReloadHotfixRulesTests.cs
- Assets/Tests/PlayMode/HitFeedbackHotfixTests.cs

Native assets (Editor API authoring only):
- Assets/Resources/Arsenal/AA12.asset — body/magazine presentation links only.
- Assets/Art/HitFeedbackHotfix/ — extracted body/magazine sprite and texture assets, shared HitFlash material; Unity-generated metadata.
- CombatFeedback material references and living-enemy impulse profiles in player/enemy prefabs under Assets/Prefabs and Assets/Data/FirstOperation, plus warehouse scene instances/inherited profiles. NativeAudit.json gives exact audited paths/settings.
- ProjectSettings/ProjectSettings.asset — new release identity if the supported Android build completes.
- DEVELOPMENT_LOG.md, GRAPHICS_BIBLE.md, SPRINT_RESUME.md and this evidence directory.

No UI layout, enemy AI, economy, level layout, weapon damage/capacity/cadence or new gameplay systems were changed.

## Device verification still required

The object/pool checks demonstrate bounded runtime objects, not a measured 60 FPS on Android. Phone installation, actual save compatibility on the user's device, sustained performance/heat, readability and gunplay feel are **PENDING USER VERIFICATION**. Check the newly identified APK rather than an older Sprint 0040 build. Test surviving enemies with each weapon, death during rapid fire, normal corpse forearms, repeated AA-12 reload/switching and prolonged crowded combat. Build identity/results are recorded in AndroidBuild.md when available.

The three previously reported Sprint 0040 bunker geometry/test mismatches were outside this hotfix and remain open; they were not silently changed or claimed re-tested here.
