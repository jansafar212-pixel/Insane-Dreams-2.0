# Sprint 0030 — implementation and verification report

## Scope and audit

Most requirements were already delivered in0029and its concrete correction. They
were inspected and preserved rather than regenerated: compact concrete shell/slit,
Officer muzzle/laser clipping, obstacle recovery, cross-floor uncertain hearing,
reaction delays, four shop categories, medical inventory/revival, three throwables,
armor/helmets, finite supplies, current/capacityHUD, ladders and parachute behavior.
No menu/HUD overhaul, new enemy framework, level regeneration or external asset.

The remaining gaps were Officer protection and missing local reactions in the
pillbox branch. Existing player armor overlays already removed themselves correctly
on break/removal; a controlled regression now verifies that instead of rewriting it.

## Implemented changes

- Officer retains normal50health, with a finite50-point ballistic vest at65%
  absorption. Repeated10-damage body hits require10hits instead of5:100body-shot
  effective durability, approximately twice a normal Larper. No large health pool.
- Headshots bypass the vest, preserving a readable exposed weakness. Melee,
  explosions and environmental damage retain existing ballistic-armor bypass.
- Existing ArmorVisual and native Armor_0/Armor_1 sprites are reused on the torso;
  no new character sprite, pivot or rig. Protection resets at encounter reset and
  actor reactivation, not after hits. Player armor/helmet balance is untouched.
- Accepted direct damage cancels a current warning and requests a local retreat.
 2.5s cooldown and.5s request expiry prevent repeated hits creating a permanent
  reaction loop. Death/incompatible states retain priority.
- A recently visually confirmed nearby threat (within existing RetreatDistance)
  caps pillbox hiding at.45s so an approaching player gets more exposure windows.
  Distant or unconfirmed positions do not force this response. Existing uncertainty,
  bounded movement, panic/suppression, offscreen firing gates and telegraphs remain.
- Preserved55damage,7-round magazine,2.8sreload,2.4sfiring interval,.5swarning,
  distinctive thick colored tracer, seated magazine/black grip and imperfect aim.
  Existing accuracy checks passed; no additional accuracy or weapon rebalance.

The initial durability test assumed100health; actual Officer/Larper health is50.
The vest was calibrated from100to50before acceptance to avoid overshooting2xEHP.
Other new reaction and overlay tests passed on their first run.

## Important files modified

- Assets/Scripts/Enemies/OfficerShotTelegraph.cs: finite vest initialization,
  debounced damage reaction and configurable short close-pressure hide time.
- Assets/Scripts/Enemies/EnemyCombat.cs: reset protection and consume the local
  pressure request/shortened hide in the existing pillbox branch.
- Assets/Data/FirstOperation/Officer.prefab and Assets/Scenes/Warehouse_Demo.unity:
  native BodyArmor/ArmorVisual hookup and reused vest renderer/tuning.
- Assets/Editor/Sprint0030Setup.cs: native authoring helper; does not regenerate art
  or the level. Assets/Tests/PlayMode/Sprint0030Tests.cs: four targeted regressions.
- Development log/checkpoint and a small art-integration note. New result evidence
  under TestResults/Sprint0030. No staged,committed orpushed changes.

## Manual checks still needed

Human playtesting should assess body armor versus exposed headshots, readability of
hit-cancelled warnings, close-pressure exposure/fairness, and whether fear remains
funny rather than frustrating. Review the reused vest visually and test safe-area/
touch comfort and sustained performance on the actual target phone. Controlled
Editor captures/tests and APK packaging do not establish artistic acceptance or
Android-device gameplay/performance. No profiler-derived frame-rate claim.

Final test/build outcome and Editor state follow once validation completes.

Additional verification found one nondeterministic older checkpoint fixture: actual
random enemy loot changed its explicitly supplied$42reward into$55after recovery.
The fixture now disables automatic EnemyWalletDrop only for that controlled test;
its duplicate-reward/results/replay assertions remain exact. Separate actual loot
and wallet tests passed. No currency/drop gameplay or prices changed. This test
isolation change is in Assets/Tests/PlayMode/Sprint014MissionTests.cs.

## Automated validation before build

**118/118unique tests passed:94PlayMode,24EditMode**, no skips or unresolved
failures in latest results. Regression.json records full test names and evidence.
Coverage includes actual concrete collision/slit/laser and complete firing-to-results
flow, Officer weapon timing/art/accuracy, Samurai pursuit, cross-floor hearing/
reaction timing, local obstacle recovery, Medical/revive/purchases/persistence,
throwables/smoke/flash/resupply, armor/reload/carrying, fall/parachute, ragdolls/
limb thresholds/player pass-through, spacing/suppression, movement/slide/ladders,
checkpoints, results/replay, shared definitions and save compatibility.

FinalReferences.json: Warehouse_Demo saved/clean/open,outsidePlay,0missing scripts
in scene or checked prefabs,5pillbox faces, valid vest sprite/renderer references,
exactly1OfficerBodyArmor component,50health/50configuredprotection. MainMenu first
in enabled build scenes. ConsoleBeforeBuild.json:0actualerrors/warnings,
compilation successful. ProtectedAfter.json matches approved environment, Testbed,
ragdoll source, player motor and existing HUDUSS.

Android Development build queued as build_132a6482be7f to
build/Sprint0030/AKUMU2-dev.apk, dry-runvalid. Final build result follows.

## Final build and state

Android ARM64 IL2CPP Development build **Succeeded**,0errors,157.960seconds.
APK: build/Sprint0030/AKUMU2-dev.apk —67,235,220bytes (~64.1MiB).
SHA-256:8c977e5846c56659893d5221f4f2f8abcb64c16d71625a624562a59e79180085.
Archive inspection confirms AndroidManifest,ARM64libil2cpp and3scene data files.
MainMenu remains the first build scene. AndroidBuild.json contains concise outcome;
AndroidBuildReport.json preserves the full native report. OlderAPKnot overwritten.

972buildwarnings remain:970installed inference-package shader warnings,1existing
obsolete FindFirstObjectByType warning,1Pipeline runtime disabled/no-config notice.
These match the preceding build's warning families; no unrelated package cleanup.
FinalConsole.json:0actualerrors,82collapsedwarnings afterbuild;prebuildwarnings0.
Compilation successful. This is build validation, not an APK/device gameplay run.

FinalEditor.json: Warehouse_Demo saved/clean/open,outsidePlay,notcompiling,
0missing scripts. Prefab/reference scan clean; bodyarmor count1,validvest resources,
normal50health/configured50protection. ProtectedAfter.json verifies environment,
Testbed,ragdoll source,player motor andHUDUSS unchanged. GitFinal.txt preserves
existing unstaged/untracked work. No stage,commit orpush.

All requested implementation workstreams are present and available automated checks
pass. No known unresolved automated failure. Remaining risks are human combat feel,
visual acceptance and actual Android touch/performance checks described above.
