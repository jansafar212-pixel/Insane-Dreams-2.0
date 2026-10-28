# AKUMU 2 — Sprint 0030 complete

Updated2026-10-27. Implementation and automated/build validation complete.
Do not restart/regenerate0029or0030. Preserve dirty working tree. No stage/commit/
push authorized. No full menu/HUD overhaul was started; that remains separate.

## Requirements and records

Verbatim0030brief: TestResults/Sprint0030/OriginalRequirements.md.
Previous checkpoint: TestResults/Sprint0030/PreviousCheckpoint.md.
Implementation,values,files,manual limitations: TestResults/Sprint0030/Report.md.
Earlier0029/concrete briefs/reports preserved in their folders; no repeated work.

## Completed

Retained functional concrete bunker/slit/laser/projectile blocking, Level1route,
Officer peek/hide/fire/reload, local box recovery and Samurai pursuit, cross-floor
uncertain hearing/configured reactions, Medicalshop/healing/max2purchasedrevivals,
grenade aiming/release/types/supplies/smoke/flash, player armor+helmet tradeoffs,
HUDammo,physics/ragdolls/detachment,ladder exits,parachute and movement.

Added finite Officer vest via existing BodyArmor/ArmorVisual and Armor_0/Armor_1.
Normal50health +50protection at65%absorption =>100body-shotEHP (2xnormalbodyshots).
Head/melee/explosion/environment bypass. Existingweapon balance unchanged.
Acceptedhits cancel warning and request retreat with2.5sdebounce/.5sexpiry;
confirmednearbytarget caps hiding at.45s. Existing states/motor/perception reused;
no new AIframework,physicsqueries,assets orplayer movement penalties.
Playeroverlaybreak/removal logic verified correct, not rewritten.
Nativehelper: Assets/Editor/Sprint0030Setup.cs. Runtimechanges onlyEnemyCombat.cs
andOfficerShotTelegraph.cs, plusOfficer.prefab/Demoequipment hookup andtests.

## Tests/build/final state

118/118unique tests passed:94PlayMode,24EditMode. Latest results and fullnames in
TestResults/Sprint0030/Regression.json. Includes actualgunfire-to-Results,camera
restrictions,Officerart/weapon,head/bodyarmor/reactions/reset,medical/persistence,
grenades/supplies,smoke/flash,fall/parachute,ragdolls/pass-through,spacing,
suppression,slide/doublejump,ladders,checkpoints,replay,shareddefinitions/saves.
Initial100healthassumption corrected to50andvestcalibratedbeforeacceptance.
One oldercheckpointfixture's randomloot contaminated explicit$42reward; isolated
onlythatfixture's automaticdrop, preservingexactassertions andseparateloot tests.

APK: build/Sprint0030/AKUMU2-dev.apk,ARM64IL2CPPDevelopment,Succeeded,
0errors,157.960s,67,235,220bytes. AndroidBuild.json/fullAndroidBuildReport.json.
972warnings:970installedinference shaders,1oldobsoleteAPI,1Pipeline runtimeconfig
notice. No packagecleanup. MainMenu first; manifest/native library/scene data checked.

Warehouse_Demo saved/clean/open,outsidePlay,notcompiling,0missing scripts,
validvest references,1BodyArmor. Console0errors,82collapsedwarnings afterbuild.
ProtectedAfter.json confirms environment/Testbed/ragdoll/playerMotor/HUDunchanged.
GitFinal.txt: preservedunstaged/untrackedwork,0staged. No commit/push.

## Next step

No unfinished implementation or failed automated test identified. Humanplaytest/
artistic and Android-device verification remain. Install current0030APK manually
on targetdevice; reviewOfficerbodyvsheadweakness,hitinterrupts/closepressure,
slitvisibility,controls/safearea andsustainedperformance. No device/profiler claim.
Wait for user feedback or separately requested next work; do not automatically
begin a graphics/UIoverhaul or rebalance other mechanics.
