# AKUMU 2 — Hotfix 0025.3

**Implemented by Codex — awaiting user playtest verification.**
2026-10-23. No stage, commit or push.

## Mission completion: diagnosis and correction

The user identified the scene launched from level selection (Warehouse_Demo), and reported a checkpoint that would not activate. Baseline tests (nine mission and twelve campaign-flow cases) already passed. Inspection found no missing mission references, compile exceptions or broken results/save subscriptions.

Two concrete progression problems were identified:
- Checkpoint contacts were silently rejected outside their required mission stage. Both mission CheckpointMarker components are deliberately disabled because WarehouseMission owns objective-gated activation. The flags still appear before their objectives are ready, so standing beside a locked flag gave no explanation.
- Processing's trigger occupied only y8–11 at x7.5, on the upper stairs. Players reaching the east side along the lower mezzanine/ground route could miss that boundary, leaving processing, office-checkpoint and records authorization behind their stage gates.

The existing Processing boundary now spans the playable warehouse height, including high double jumps: same x7.5 and two-unit width; center y9.5→6, height3→28 (y−8…20). Changed in Demo and Mission01 via Editor APIs; spawning/layout/navigation are unchanged. The storage checkpoint must still be committed before Processing can advance.

Locked checkpoint contacts now explain remaining entrance/storage or processing enemies (including their floors), or the preceding checkpoint/route requirement. Feedback uses the existing notification channel, throttled to its four-second duration, and doesn't relock the active flag. Encounter progress evaluates the current completed prerequisites rather than relying on which group's defeat callback arrived last. Context menu Log Mission Progress provides a single explicit diagnostic with stage, saved stage, records, checkpoint and defeat counts; no per-frame logging.

**Limits of diagnosis:** the user's original run and exact checkpoint were unavailable. The lower-route gate defect was reproducible, and silent lock rejection was confirmed in code/native scene data. This does not prove which one caused that particular run. Objective requirements have not been bypassed.

## Larper reload

The prior hotfix deliberately installed PistolMagazineReload only on the player. Enemy CharacterVisualDriver supplied no enemy weapon to that component, so Larpers retained the generic gun-parented oscillating magazine animation.

Larpers now use the existing working component unchanged, with their own WeaponCycle and Buzzbox definition. An enemy-only adaptation positions the existing articulated hands using their actual grip length, without stretching or changing joints/pivots. Removal/release/insertion uses the same20/40/42/58/88/100% timing as the player, and WeaponCycle alone transfers ammo. NPC debris is prewarmed, capped at two magazines per Larper and expires after four seconds. On cancellation, disable or destruction the existing component handles cleanup. Samurai/fireworks variants explicitly remove the inherited reload component, avoiding useless debris pools.

The player Glock component/source, player prefab, BackupPistol definition, CutoutRig, WeaponCycle, WarehouseCarbine definition and both carbine sprite assets match their hotfix-start SHA256 hashes. The new articulated adaptation is called only for a reloading, supported enemy pistol. Player reload behavior is not routed through it.

## Tuning

| Parameter | Before | After |
|---|---:|---:|
| Larper near-miss accumulation | 58 | 63.8 (+10%) |
| Fireworks specialist recovery | 3.8s | 3.0s |
| Fireworks preparation/telegraph | .85s | unchanged |
| Nominal preparation + recovery | 4.65s | 3.85s (~17% more opportunities) |

No further suppression probability/cooldown overhaul: gain improves threshold arrival, while existing personality weighting, sustained escalation, decay12/s, quiet gap.8s, rare panic/cascade rules, Samurai fear exclusion and two-speaker limit remain. Projectile speed/arc/fuse/damage/radius, held-malfunction probability and movement/survival behavior are unchanged. Two existing projectile pool slots continue to bound attack availability.

## Fireworks presentation

Existing pooled SpriteRenderer effects now sequence flash → multicolored radial trails → secondary sparkles → fading embers:
- 24 varied radial sparks/trails, six secondary sparkle slots in two delayed groups.
- Crisp native pixel-star flash (.075s), restrained soft world-space glow (.14s), three faint drifting haze sprites.
- Smooth alpha fades, curved decelerating travel and mild downward drift; post-blast radius ring is much fainter.
- Two tiny native sprites (9×9 star,16×16 soft disc) authored through Editor APIs, with no mipmaps and shared existing materials. Existing effect architecture and material-specific hit effects remain.
-35 prewarmed visual renderers per projectile; no per-burst instantiation, new physics queries or continuous enemy scans. Original .8s burst lifetime and two-projectile pool remain, so effects cannot accumulate indefinitely.
- The brief glow is a cosmetic world-space light wash, not a new dynamic-light/bloom pipeline. Actual scene lighting and Android GPU cost still need device review.

## Exact implementation files

Code:
- Assets/Scripts/Missions/WarehouseMission.cs
- Assets/Scripts/Visuals/CharacterVisualDriver.cs
- Assets/Scripts/Visuals/SamuraiArticulatedRig.cs
- Assets/Scripts/Enemies/FireworkProjectile.cs
- Assets/Editor/Hotfix00253Setup.cs (+ generated meta)
- Assets/Tests/PlayMode/Hotfix00253Tests.cs (+ generated meta)
- Assets/Tests/PlayMode/Sprint0224Tests.cs — observes the initial real duck and permits subsequent sustained evasion at the third-shot frame boundary; no-damage and eventual-recovery assertions remain.

Native Editor-authored assets:
- Assets/Scenes/Warehouse_Demo.unity
- Assets/Scenes/Warehouse_Mission01.unity
- Assets/Prefabs/Enemies/Larper.prefab
- Assets/Prefabs/Enemies/FireworksExpert.prefab
- Assets/Prefabs/Enemies/Samurai.prefab
- Assets/Prefabs/Enemies/CartoonFirework.prefab
- Assets/Art/Hotfix00253/FireworkFlash.asset, FireworkGlow.asset (+ generated metas/folder meta)

Documentation/evidence: DEVELOPMENT_LOG.md, ART_BIBLE.md, TestResults/Hotfix00253*.
Pre-existing changes from earlier sprints are preserved.

## Verification

**78 Edit Mode +80 unique selected Play Mode tests pass (158 total).** The final deduplicated count is158; reruns and overlapping baseline/final runs are not added again.

Play suites: Hotfix00253Tests (4 new focused cases), Hotfix00252Tests, Sprint014MissionTests, Sprint024FlowTests, Sprint0242Tests, Sprint0248Tests, Sprint0245Tests, Sprint0223CombatTests, Sprint0224Tests, Sprint020Tests, CameraSamuraiHotfixTests and Sprint0223MotionTests. All Edit Mode tests pass. Final mission/campaign/focused cases were rerun after extending the gate through high-jump height.

The older short-burst test was timing-sensitive at the new sustained-evasion boundary. It now verifies a real initial duck/collider crouch and permits the subsequent legitimate withdrawal, retaining actual trajectory, no-damage and eventual-decay checks. One Edit Mode panic-face reference-identity assertion failed after native asset reload; it passed unchanged in isolation and in the final full78-test rerun. Initial focused-fixture corrections accounted for floor-settled long magazines, inherited variant components and real extra wallets collected at the exit; they did not change combat damage or rewards.

Compilation succeeds; final recompile is current. Final Editor Console ground truth: zero errors/zero warnings (retained historical MCP warning counters include existing obsolete-API and account-connectivity logs). Five player/enemy/projectile prefab scans find zero missing scripts/object references. Both authored mission gates are bound, and opening enemy counts remain Demo3/Mission01 one. All eight protected player/carbine hashes remain identical to hotfix start. Android build-configuration dry-run is valid; no APK build or Android-device profiling is claimed.

Raw runs and native checks: Hotfix00253.json. Protected hashes: Hotfix00253_ProtectedBefore.json and Hotfix00253_ProtectedAfter.json. MainMenu remains clean/open with Play Mode stopped, startup MainMenu and run-in-background off. Temporary test saves are isolated from personal progress and removed. No stage, commit or push.

The focused mission test satisfies all encounter/records/checkpoint gates, contacts checkpoints and the lower Processing region through actual physics callbacks, retains collected records across respawn, walks through the exit with normal movement input, verifies Results, saved rewards/completion and return to Level Selection. Combat defeats and intermediate positioning are controlled test setup. This is a complete automated pipeline test, **not an unassisted start-to-finish gameplay playthrough**.

Focused reload checks use the real enemy clock and articulated grip positions during removal/insertion, actual gravity and atomic ammo completion. Grounded long magazines may already contact the floor on release, so the fall assertion uses an elevated frozen fixture. Effects checks cover sequence phases, no duplicate damage and reuse without renderer-count growth.

Controlled two-facing reload storyboard: Hotfix00253_LarperReload.png. Four sampled fireworks phases: Hotfix00253_Fireworks.png. Static snapshots were inspected; gravity/lifetimes were separately exercised in Play Mode. They are not user artistic/gameplay acceptance.

## User playtest still required

1. Main Menu → Level Selection → Shop → Warehouse_Demo. At the first flag, check any lock message, clear all listed entrance/storage enemies on both floors, then revisit: it should light green and save. Cross into Processing via upper and lower routes.
2. Clear processing (gantry/mezzanine/exit enemies), activate the office flag, collect records with E/INTERACT, and reach the east ground-floor exit. Confirm Results, earnings and Level Selection. Also retry from a checkpoint and after records collection.
3. Observe standing/crouched/moving Larper reloads from both facings at normal speed: hands, magazine release/collision, insertion, interruptions and return to shooting. Recheck both accepted player reloads.
4. Apply sustained real near misses: modestly quicker defense, recovery after fire stops, varied reactions, rare full panic and aggressive Samurai.
5. Check specialist telegraphs/counterplay and repeated fireworks against gray scenery. Review readability, touch controls, frame rate and effect load on Android hardware.

The hotfix is implemented; gameplay completion, reload feel, suppression balance and VFX appearance remain pending the user's playtest confirmation.

