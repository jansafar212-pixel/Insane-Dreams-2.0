# AKUMU 2 — Sprint 0027 implementation and verification

Status: **Implemented by Codex — awaiting playtest verification.** Focused regression and development APK build complete. Final acceptance requires the user's hands-on playtest.

## Behavior and configuration

- Navigation: existing patrol now checks the same complete sampled body arc/reachable obstacle height as pursuit before jumping. Failed attempts retain existing retry cooldown; existing stuck replanning remains. Vertical gunfire produces uncertain auditory awareness without seeing through platforms. No reaction-speed, aim, or personality boosts.
- Fall damage: player contact normals establish real landings; defaults safe drop8units,8HP per excess unit, minimum impact speed12. Ordinary double-jump height is also exempt. Deployed parachute at contact clears the entire pending flight and bypasses damage before calculation. Climbing/death/recovery/revival reset flight tracking. Enemy bullet/explosive damage is unaffected.
- Sliding: existing movement/crouch own physics. Down gesture during a run (minimum3units/s) starts a finite.35s slide with decaying normal-run momentum; downward input after the double jump queues the landing slide even if released before contact. Reverse/jump/reset/incompatible traversal cancels it.
- Grenades: player slot3 replaces active wrench use; saved slot enum/data and enemy melee stay compatible. Finite cap5 of each: fragmentation60dollars, flash50, smoke40. Select type, aim right joystick (desktop hold RMB), release to throw. No fire-button tap throws. Approximate13-point arc updated10Hz. Six pooled projectiles. Defaults: speed12+upward2, fuse1.4s; frag85peak/3.2radius/2impulse, LOS and linear falloff/friendly/self damage; flash6radius/1.6s sensory disruption; smoke3.5radius/8s sight obstruction, no bullet collision or blast protection. Stock purchases/consumption persist; no free retry refill.
- Mine: reusable fictional EnvironmentalMine.prefab; configurable trigger shape, arming.3s,75peak/2.5radius/2impulse, single activation. Controlled test only; none placed in Level1.
- Regions/ragdolls: accepted impacts accumulate in six modular regions, independent of weapon identity. Defaults head80/limbs60/torso150. Totals transfer to existing corpse; thresholds release existing head/limb joints. Living rigs remain intact and torso remains root. Existing cap4,45s lifetime, velocity/impulse limits, sleep and nonblocking masks preserved.
- Tape: existing200reserve/25HP per second/1point per actual HP. Small modular wrapping gesture/band and short dry adhesive-peel audio. Active healing blocks all AttackExecutor calls before ammo/effects; release/completion/interruption restores weapon use. Damage/reload/airborne interruptions remain.
- Faintanyl: Medical shop only, cap2, no random loot or free prefab stock. Existing purchased saves preserved. Consume1 for in-place revival and exactly2s protection. Two small HUD pips disappear individually; third fatal event after two revivals ends attempt.
- Officer: warning1.15→0.5s, final.2s direction lock preserved. Red continuous geometry-clipped visual-only beam; actual magenta tracer width.08 vs ordinary Larper.04, with existing taper/fade/shot feedback. Withdrawal/death/offscreen/reload interruptions cancel beam. Damage55,7-round magazine,2.8s reload,2.4s weapon interval and2.4(+.8with ally) warning cooldown remain.

## Files changed in this sprint

New runtime files under Assets/Scripts/Combat/: PlayerFallDamage, PlayerThrowables, ThrowableProjectile2D, SmokeCloud2D, AreaCombat2D, EnvironmentalMine2D, BodyRegionDamage. New Assets/Scripts/Visuals/PlayerHealingPresentation.cs.

Existing runtime: Combat/AttackExecutor.cs, CombatVisibility2D.cs, PlayerHealing.cs; Enemies/LarperBrain.cs, EnemyCombat.cs, OfficerShotTelegraph.cs, FireworkProjectile.cs; Input/PlayerActions.cs, WarehouseTouchControls.cs; Inventory/PlayerLoadout.cs; Traversal/PlayerPosture2D.cs; Progression/GameCatalog.cs, GameSession.cs, ProgressStore.cs, GameFlowController.cs, CampaignMissionBridge.cs; Visuals/CartoonRagdoll.cs, CharacterVisualDriver.cs, SamuraiArticulatedRig.cs.

Native Editor authoring: Assets/Editor/Sprint0027Setup.cs; new Assets/Data/Sprint0027/ icons/textures, throwable/mine prefabs, tape band and peel WAV. Modified existing assets: WarehousePlayer.prefab, Larper.prefab, FireworksExpert.prefab, Officer.prefab and DemoCatalog.asset. Scene geometry/layout/pacing and all historical art unchanged relative to sprint entry.

New Play Mode suites: Sprint0027Navigation/Fall/Slide/Throwable/Mine/Region/Healing/MedicalTests. Updated existing OfficerHotfixTests, Level1CombatHotfixTests (explicit.5s warning), Sprint023Tests (explicit heal/fire rule), TraversalRegressionTests (finite grenade slot/rising arc), CameraSamuraiHotfixTests (temporary controlled geometry), Sprint0026Tests and Sprint024ProgressTests (current cover/roster fixtures), Hotfix00252Tests (actual burst movement observation). Existing pre-sprint changes are preserved; full git diff includes earlier sprints and must not be attributed solely to0027.

## Verification evidence

JSON per suite in this directory. BaselineMovement.json:9/9 before changes. Sprint0027.json:18/18 new cases; Officer.json4/4. Final latest unique results: **138/138 pass —122 Play Mode,16 Edit Mode** (Regression.json). Six affected cases rerun after the final simple-arm wrap fallback/HUD label correction, without counting duplicates.

Historical fixture mismatches resolved without gameplay edits: cover tests now inspect three current route-side retreat covers; the old17-enemy/three-Samurai assertion now matches the preserved15-enemy/one-Samurai roster and current specialist stages. Samurai tests now use current production prefabs in a temporary controlled arena instead of deleted warehouse-coordinate assumptions. Ladder arbitration observes the rising arc before canopy contact; short-retreat checks observe movement during the burst instead of only net displacement. Initial failures are retained as historical evidence; final unique results all pass. Current Level1 geometry equals TestResults/Level1CombatHotfix/GeometryAfter.json.

Android support and matching SDK/NDK/JDK are installed, Android active. Enabled scene order starts MainMenu, then Warehouse_Demo. adb devices lists no connected devices. Development APK **Succeeded** in839.36s: build/Sprint0027/AKUMU2-dev.apk,51,387,294bytes (~49.0MiB). MainMenu launches first. No Android device/performance or human gameplay claim.

## Manual Android / gameplay checklist

1. Run MainMenu→level selection→shop; buy each grenade and two Medical injections. Confirm balances, type stock, weapon ammo/reloads, pause/resume and results/save flow.
2. Run/crouch/slide under suitable clearance, reverse, double-jump then pull down/release before landing; test ladder/chute transitions and camera smoothness on actual touch controls.
3. Fall from small/medium/high and narrow platforms. Normal jumps safe; larger ordinary falls hurt. Deploy parachute early and immediately before landing: exactly0fall HP, no delayed damage after undeploy; bullets can still hurt while airborne.
4. Select grenade slot/type; drag right stick to preview, release to throw. Check finite stock, frag self/friendly/cover falloff, comfortable flash interruption and smoke concealment/expiry. Confirm normal shooting and enemy Samurai melee unchanged.
5. Check isolated controlled mine (do not add one to Level1) triggers once for player/enemy. Pile corpses, walk/jump/climb through them; regional hits can detach existing cartoon head/limbs without graphic gore or permanent jitter.
6. Hold heal: wrap pose/peel sound, no firing/ammo loss, damage interrupts, release resumes weapons. Die/revive twice at same position; pips decrement,2s immunity then vulnerability. Third fatal event ends attempt.
7. Officer: red.5s warning, final direction commitment, one thick magenta fading actual tracer; duck/dodge/interruption/death do not leave beams. Desert Eagle remains55damage/7rounds/2.8reload.
8. Confirm15-enemy Level1 pacing[3,4,3,3,2], offscreen persistence, Larper suppression/fear/floor restrictions, Samurai pursuit and Czech throws. Watch frame rate, thermal behavior and touch readability on device; Editor tests do not certify them.

Remaining limits: final artistic/feel/device acceptance remains manual. Regional separation is corpse-only, with central torso root retained. Query/pool bounds deliberately limit worst-case work; they are not an unlimited battlefield simulator. Initial fixture failures were corrected and retested.

Controlled captures inspected: GrenadeHUD.png (trajectory, finite stock, type controls and revival pips), TapeWrap.png (production simple arms wrap across existing torso, weapon hidden), OfficerMagentaTracer.png (actual shot, magenta fading trail and existing muzzle feedback). A first capture exposed the missing simple-arm pose fallback and overly long grenade HUD instruction; both fixed and relevant6 cases passed. Captures used a disposable temporary campaign profile, not personal progress; no human gameplay or Android-device verification.

## Final build and integrity results

Unity BuildResult=Succeeded. Official report records972 warnings (mainly existing Sentis/inference shader variants, one deprecated Find API, optional runtime Pipeline configuration) and3 Pipeline request/status timeout errors during the long build; none is a C# or linker compilation error. All diagnostics retained in AndroidBuild.json, AndroidWarnings.json and ConsoleBuildDiagnostics.json. A deferred duplicate callback subsequently refused to overwrite the existing APK; recovered the authoritative successful report through BuildReport.GetLatestReport() and retained the duplicate-callback evidence separately. No duplicate build/overwrite was performed.

Final compilation completed without failure. After archiving diagnostics, Console history cleared:0current errors/0warnings. Warehouse_Demo saved/clean/open, Play Mode and pause stopped,0missing scripts. Current player/Larper/Samurai/fireworks/Officer rigs, skins, region components, grenade/tape/audio references all intact.813 preexisting asset/reference hashes compared: only the five intended existing assets changed; all scenes and historical originals unchanged. Level1 terrain/scenery identical; encounter counts[3,4,3,3,2]. FinalIntegrity.json includes APK SHA256. GitFinal.txt records the preserved dirty working tree; index empty, no stage/commit/push.

New work completed after the latest resume: resolved the remaining retreat observation and stale roster fixture checks, fixed production simple-arm tape wrapping and compact grenade HUD styling/text overflow, reran the affected6 cases, inspected controlled captures, built the APK and finalized logs/checkpoint. Earlier completed implementation preserved.
