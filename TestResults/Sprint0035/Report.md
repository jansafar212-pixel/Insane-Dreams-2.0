# Sprint 0035 — Level 1 combat and gameplay polish

Date: 2026-10-28. Work continues on the existing dirty checkout; nothing staged,
committed or pushed. Prior optimization and gameplay work was preserved.

## Changes and findings

- Level 1 now has exactly 10 Larpers, one Samurai, one fireworks expert and one
  Officer. Removed only the two supplemental CombatHotfix guards and their spawn
  references. Existing ground route, platforms, crates, resupply and five sections
  remain. A new live opening test confirms one visible authorized shooter, no shots
  from the other actors and a responsive first Larper.
- Larper 0.7 + 0–0.12 second recognition, Samurai 0.25 + 0–0.08 second recognition,
  existing suppression, hearing uncertainty, recovery cooldowns and offscreen fire
  gates already meet the requested behavior. These were tested, not rewritten.
- The concrete bunker already blocks bullets, protects the crouched Officer,
  permits slit fire and supports physical roof traversal. Its sealed rear wall
  prevented normal egress. The existing rear wall/collider now starts at y=3.6,
  leaving a 2 m rear doorway above the y=1.6 floor. A scene exit anchor and a small
  confirmed-visual-flank branch let the Officer physically walk out. Rear shots
  use ordinary geometry checks. Front slit checks, 0.5 second warning, final
  direction lock, damaging individual shots and distinctive tracer remain.
- Basic pistol was 8 rounds and mistakenly inherited the Larper extended-magazine
  overlay. It is now 15 rounds; the existing ordinary pistol sprite contains the
  short grip/magazine, so the extended overlay is removed. Ammo packs are 15 rounds;
  five starter magazines consequently total 75. No damage, cadence, recoil,
  sniper controls or Desert Eagle controls changed. Original pixel assets remain.
- The mission card overlapped the expanded medical/vitals panel. MissionHud now
  positions it below the actual vitals geometry, using GeometryChangedEvent and
  unregistering the callback on disable. No polling, scene search or new visual style.

## Development currency

GameCatalog.developmentPlaytestCredits configures the temporary floor. GameSession
applies it only under UNITY_EDITOR or DEVELOPMENT_BUILD. New and old eligible
profiles receive max(existing balance, 10000); developmentCreditRevision=35 is
saved atomically with the balance. Subsequent loads never regrant spent money.
Failed writes roll back both fields and can retry. Richer balances, ownership,
inventory and normal release economy are retained. Unsettled active runs must
settle successfully before a grant can be committed.

The actual persistent test profile was inspected: 2166 credits, no active run,
revision 0. It now has **10000 credits, revision 35**, verified from disk through
the normal store (ActualSaveCreditVerification.json). Atomic replacement retains
the store's .bak. No purchase was made on the user's actual profile. An isolated
existing profile verifies top-up, weapon purchase, restart, mission settlement and
restart again without regrant. The initial automated review rejection was resolved
by supplying section 8's explicit authorization for this exact existing-save top-up.

## Validation interpretation

All selected checks ultimately passed. TestRuns.json preserves captured runs,
including initial failures; later successful results supersede the same test name.
Some initial currency/economy/bunker results are recorded by summary below rather
than duplicated raw output. The complete project-wide test suite was not run.

Older assertions/fixtures were updated for the deliberate 13-actor roster, 15-round
pistol, current grenade price, unaccompanied Officer cooldown and prior sprint
rules (no free injections, automatic reserved-charge revival, grenade slot 3).
Legacy manual-revival/melee tests now explicitly configure those isolated modes;
normal production modes are also covered by Arsenal and Medical suites. The pyro
unit fixture now keeps combat enabled with an explicit fixture-only fire permission;
otherwise current committed-throw safety correctly cancels its throws. Safe-area
card-height tolerance allows subpixel layout rounding (155.56 <= 156).

Controlled checks cover real movement through Level 1 to results, separate actual
Officer damage/defeat and roof traversal, Samurai platform pursuit, fireworks
ground attacks, warning/laser alignment and offscreen cancellation, suppression
accumulation/decay/cover occlusion, bounded stuck recovery, repeated hearing from
below a platform, finite rewards/replay/pause/resume, healing, revival, armor,
grenades, crouch/slide/double jump/ladders/parachute, touch-pointer arbitration,
sniper safety, automatic fire and effect-pool bounds. Level flow tests deliberately
control or disable enemies and sometimes apply scripted defeat; they do not prove
an unassisted full-combat player run. HUD and bunker captures were inspected.

Existing parachute landing logic needed no change: six fall tests pass, including
late deployment and zero landing damage from high drops while combat damage still
applies. The existing two-bubble cap and conditional blind-fire control pass.

## Performance and remaining acceptance

No changes to AndroidFramePacing, input runtime, camera runtime, combat pooling,
nonalloc physics buffers, sampled perception or cached HUD/visual updates. Credit
serialization runs at profile initialization, never per frame/shot. The new Officer
branch uses existing data, movement and solid-query paths. Removing two actors
reduces level workload; no measured device-performance claim follows from this.

Physical Android profiling/install/playtesting is unavailable here. Stable 60 FPS,
frame-time tails, thermals, multitouch feel, safe areas on the actual device,
sound, full-combat difficulty, new doorway tactics and human artistic acceptance
still require the A56. The automated pass does not guarantee absence of navigation
softlocks in every player situation. No known selected regression remains failing.

## Files changed in this sprint

Runtime: Assets/Scripts/Progression/{GameSession,GameCatalog,ProgressStore}.cs;
Assets/Scripts/Enemies/{EnemyCombat,OfficerShotTelegraph}.cs;
Assets/Scripts/Missions/MissionHud.cs.

Native assets via Unity Editor APIs: Assets/Data/Items/BackupPistol.asset;
Assets/Data/Progression/DemoCatalog.asset; Assets/Scenes/Warehouse_Demo.unity.

New authoring/tests: Assets/Editor/Sprint0035Setup.cs;
Assets/Tests/PlayMode/Sprint0035Tests.cs and Unity-generated .meta files.

Adapted/expanded tests: FirstOperationLayoutTests, Sprint0034EconomyRulesTests,
FirstOperationTests, CowardsBunkerTests, Sprint0027NavigationTests,
Sprint0034EconomyTests, MobileHudPyroTests, Level1CombatHotfixTests,
Sprint0025Tests, Sprint0027ThrowableTests. Existing test capture output was also
refreshed by those suites. DEVELOPMENT_LOG.md and SPRINT_RESUME.md record handoff.
This list describes this sprint, not the entire pre-existing dirty working tree.

Build and final selected-test counts are recorded in ValidationSummary.json.

## Final build and Editor recovery

116/116 selected checks passed: 111 Play Mode, 5 Edit Mode. Android Development
build_3c27a20026c6 succeeded in 488.978 seconds, zero errors, 977 warnings:
970 Sentis package shader warnings, six existing API/config/development warnings
and one new UAC0009 deprecation warning for the development guard in GameSession.
The guard is functional: generated ARM64 C++ includes the 10000 floor/revision35
atomic commit. No package or shader rewrite was attempted.

APK: build/Sprint0035/AKUMU2-sprint0035-dev.apk, 67,236,575 bytes (~64.1 MiB).
SHA256: 13a770232c57a260202006cfd9633d40641f71f40870ffce57ea361c770f780c.
BuildStatus.json contains the full report; ApkVerification.json confirms ARM64 and
grant metadata. BuildReport totalSizeBytes also counts large supporting output;
it is not the APK download size. ActualSaveShop10000.png shows the real shop wallet
at 10000 after a fresh MainMenu Play session; frame advancement was checked.

**Remaining Editor issue:** stopping that final Play session produced a native
fatal audio dialog: "ADTM: Attempting to update control while a mix is ongoing,
but realtime thread is disabled will lead to a deadlock". Its cause is not
diagnosed. The successful build/test results precede this incident; do not infer
Android audio stability from them. Closed the fatal dialog and reopened the saved
project; preserved Unity's scene backup in Assets/_Recovery/0 (3).unity without
applying it. The real save still reads 10000 after restart. FinalSceneAudit.json:
Warehouse_Demo open, stopped, clean, 13 actors, front slit and rear exit bound,
zero missing scripts. Editor is ready, compilation clean. Console retains three
Pipeline startup command/timeout errors and one warning; these are separate from
the observed native fatal incident. No crash report was submitted.
