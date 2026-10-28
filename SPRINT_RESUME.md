# AKUMU 2 — Sprint0037 combat impact (2026-10-29)

Current brief/report/captures/audits/tests: TestResults/Sprint0037.
Extended existing HitReaction2D with six cached collider-free angular joints and
kinematic anchor per configured actor. Native living rigs previously had no live
joints; these drive existing cutout pivots, not a new ragdoll controller. Sleeping
cache has physics off; no per-hit corpse/spawn. Original motor/hitboxes preserved.
Weapon reaction strengths pistol.65,SMG/Buzzbox.4,carbine1.1,LMG1.35,shotgun.5per
pellet (batchcap3.2),DE2.6,sniper3.2. Separate balance decay1.15, thresholds4.5/6/7,
heavycooldown.55, fixed recoveries.65/.5/.55, fallcooldown3 and grace1.2. Player
never locks control. Enemy controller stays enabled; existing AI loop yields
briefly during fall then resumes without resetting fear/personality/perception.
Health emits accepted DamageOutcome, preserving legacy events/math. New player
feedback has four bounded safe-area markers, max18% edge pulse,85% HP flash and
.025/.09s camera impulse throttled.65s; serialized shake scale0–1 can disable.
DE1.6× and sniper2.2× tracer multipliers; normal baseline.04. Officer baseline.08
and its enemy color preserved. No damage/ammo/spread/range/movement rebalance;
CombatBefore/After snapshots identical and final configuration audit matches.
133/133selected tests pass (15Edit,118Play), including16new impact checks.
New spring wake/reset and scene-unload ordering bugs caught and fixed; raw history
retained. Temporary Editor-only FPS strip hidden only in capture fixture.
RELEASE build_3992cd59f9ac Succeeded197.247s,0errors/974warnings. APK:
build/Sprint0037/AKUMU2-combat-impact-release.apk39,837,579bytes,ARM64/nondebuggable.
SHA2563741ddf3f08dbc1a58cbc8e225172fe36c3e619abbc7aaedaa9423a6a274f034.
Packed assets/IL2CPP/APK metadata verified. adb empty; no installed/phone test.
Native ADTM audio Editor failure recurred before export; Editor reconnected and
saved source/config re-audited, then fresh build succeeded. Audio issue not fixed
or attributed to this sprint. Human feel/art/device-performance review pending.
No stage/commit/push; earlier dirty work preserved. Previous handoff follows.

# AKUMU 2 — UI transparency hotfix0036.1 (2026-10-28)

Current report/captures/style audits/tests: TestResults/UITransparency0036_1.
Only background-color overrides in WarehouseControls.uss, MissionControls.uss,
GameFlow.uss and HandmadeUI.uss: sticks40%, handles65%, actions60%, weapon65%,
vitals/ammo/objective55%, healthfill85%, other gameplay cards60–70%. Text/icons,
touch bounds, geometry, gameplay code and front-end/shop remain unchanged.
Seven relevant existing PlayMode tests pass; resolved alpha and critical/healing/
selected states checked in attached UI Toolkit probe. HUD/pause/results captures
inspected. No new runtime/test scripts or CanvasGroup/parent opacity. Editor's
temporary probe and imported captures cleaned; runInBackground setting restored.
Final release build_7173418f1d13 Succeeded53.461s,0errors/1251warnings. APK:
build/UITransparency0036_1/AKUMU2-ui-transparency-release.apk,39,819,759bytes,
ARM64/nondebuggable. SHA2560eac86f000b775c84a8622ac272b80af8c9939f70cb3d6c9f849cd6a796056ab.
BuildStatus/PackedStyles/ApkVerification verify the four USS assets in release.
Editor ready/stopped. No Android-device installation/touch test claim.
No stage/commit/push; earlier dirty work preserved. Previous handoff follows.

# AKUMU 2 — expanded Sprint0035 combat reliability (2026-10-28)

Current brief/report/raw tests/captures: TestResults/CombatReliability.
LMG retains100/8s and exchanges its original6x6box container via existing rig;
DE retains7/2.8s/normal controls and has its own6x7black magazine fitted to grip.
Sniper5/release-to-fire/recovery unchanged; manual reload now blocked during healing.
Cover accepts throwable slot, shared swept release origin/preview, short.24s
damageable exposure. Ceiling/cancel refuses throw without spending. No phasing.
Wallet static prefab now placed on nearby support with crouched-body clearance;
bounded19offset search prefers original height, two delayed checks only. Radius
uses real collider closest point, smoke does not block pickup. Actor destruction
does not erase scene-owned uncollected reward. Existing reward claims/payout kept.
Storage only: three reused crate covers at16/0,23.5/3.2,28/6.4; three actors have
detection18/16/14 and viewport delay.30/.55/.85, normal AI/personality/damage intact.
Release wallet floor and diagnostics remain enabled; no final economy changes.
106selected unique checks:106pass. New reliability15checks pass,
actual three-height cover throws and six authored wallet sites pass, full route
reaches results. Earlier opening-crossfire failure resolved: second warmup guard
inherited22-unit detection joined too soon; local range10 makes engagement follow
player advancement. Original assertion retained; full8FirstOperation checks and
final8layout/reload rules pass. Global AI/camera/weapon damage unchanged.
Final release build_27fac48c3adc succeeded175.721s,0errors/971warnings. APK:
build/CombatReliability/AKUMU2-combat-reliability-release.apk,39,818,983bytes,
ARM64/nondebuggable. SHA25634101a343cd859b368716a8e862a31a236f7274d96be1beb34e34758ab77ffb7.
FinalBuildStatus/archived FinalBuild.buildreport, FinalPackedAssetProof and
ApkVerification are canonical; hash checked after successful completion. Editor
reconnected after export; success recovered from persisted Unity BuildReport.
FinalEditorAudit: Demo open/clean, arrival detection10, both playtest-walletflags
true,0missing scripts. Ready/stopped; no source/native changes during export.
adbdevices empty, no installed/device test claim. Previous APK build_aa6a88e04ff0
was superseded after this arrival guard fix; do not identify it as final.
No stage/commit/push. Earlier dirty work preserved. Previous handoff follows.

# AKUMU 2 — expanded Sprint0034 reload/economy handoff (2026-10-28)

Current expanded brief/report/tests/captures/audit: TestResults/ReloadEconomy.
Implemented: SMG30/1.8s physical-mag exchange via existing rig, scoped body variant
and own3x7mag; original complete art retained as inventory icon. Shotgun6 has
incremental .43333s shell steps/2.6s full-empty, one cached3x5shell+rear-arm pose.
SharedWeaponCycle supports both; fire/switch/death/valid healing cancel pending
step while retaining completed shells. Tick sole ammo authority, bounded catchup.
HUD ammunition-change event invalidates cached readout on nextUIframe, unsubscribes
on switch/rebind/disable. No movement/input layout/AI/camera/damage/cadence changes.

66/66selected checks pass18Edit/48Play; new11rules+4real-clockPlay including exact
transfer/interrupt/snapshot, movement/jump/crouch/facing, switch/death/heal and HUD.
PriceAmmoAudit exports actual8guns/cost50HP/reserves. Existing coherent provisional
prices/caps/rewards retained; no speculative final balancing. Pistol15/fivemags75.
Urgent release-enabled wallet floor preserved, no reset/testpurchase on realprofile.
Real disk8720 at audit; next normal init floors10000; don't undo real purchases.

Release build_832d92bfda0d Succeeded128.494s,0errors/974warnings. ARM64 nondebuggable
APK build/ReloadEconomy/AKUMU2-reload-repair-release.apk,39,817,203bytes, SHA256
98a13f846658f116d8ec897582344c4e1407c90a49f06b3717b246aa1d128398.
Shell code/visual/ammoevent/wallet metadata verified inAPK. No adbdevice: no install/
physicalAndroidgameplay/FPS/artacceptance claim; priorADTMnativefatal undiagnosed.
FinalEditor ready/stopped,Demoopen/clean,0missing scripts,0errors/84warnings.
No source/asset changes during export; no stage/commit/push. Preserve dirty work.

## Previous urgent wallet hotfix handoff (historical)

Current report/searches/tests/audit: TestResults/WalletHotfix/Report.md.
User reports phone still100 after app-data clear. Confirmed source defect: old
wallet floor excluded release via UNITY_EDITOR||DEVELOPMENT_BUILD; serialized
DemoCatalog100 survives C# default changes. Exact installed phone APK not inspected.
GameSession.Data.balance is actual shared UI/purchase wallet; no duplicate manager.

NEW explicit semantics supersede0035/0036: floor enabled in release APK too;
after ALL load/initialization/settlement, Max(balance,10000) at EACH initialization.
No per-purchase/frame refill. Higher balances retained; ownership/progression safe.
Revision35 remains metadata, no longer a blocker. Final economy100/prices unchanged.
developmentPlaytestCredits is temporary ALL-BUILD override; disable before final
shipping. playtestWalletDiagnostics logs origin/transaction/final/buildGUID;
also disable before shipping. Tag wallet-release-floor-20261028-r1.

30unique selectedPlay checks pass latest; raw initial unrelated jump failure and
successful rerun retained. No adb device: actual Android install/run unverified.
Release build_a43a5d61425c succeeded143.806s,0errors/974warnings,Development OFF,
options onlyDetailedBuildReport. APK:
build/WalletHotfix/AKUMU2-wallet-release-20261028-r1.apk,39,815,271bytes (~38MiB).
SHA256 5fbe6b8aefb7a4ecc096148462f5ab127257e34da4fad0d46ad3065fbe4ccce8.
Aapt confirms nondebuggableARM64; generated releaseC++ hasMax10000/Commit;
APKmetadata has unique build tag/diagnostics,DemoCatalogpacked.33unique selected
checks pass latest30Play/3Edit. Actual local save10000, Editorstopped/compileclean.
NOT installed/run on phone: noadbdevice. VersionName0.12/code1 unchanged; identify
newAPK by exactfilename/hash/buildtag, not version alone. Don't clear app data.
See final BuildStatus/ApkVerification evidence; do not install old sprint APKs.
No source/asset edits during export; no stage/commit/push. Preserve dirty prior work.

## Previous Sprint 0036 handoff (historical)

Current brief/report/raw tests/captures: TestResults/Sprint0036. Shared UI Toolkit
noticeboard restyle implemented across main/settings/shop/HUD/pause/revival/results/
speech, enlarged original logo and approved crate reuse. Shop5categories, thumbnails,
visible ownership/stock, wallet, real weapon facts. No gameplay/economy/native scene/
prefab or pixel-art asset changes. Existing joystick geometry and offsets retained.
Empty revival images transparent; terminal HUD dismisses/blocks speech using the
existing two-bubble registry, reset on HUD disable. Source and precise limits in Report.

100/100 selected checks pass97Play/3Edit; new2presentation tests check5aspect layouts
and capture all categories/pause/scripted results. Cached UI tests baseline2/after2
pass, no actual performance matrix/FPS claim. Real save read-only audit10000;
revision35 currency setting/migration untouched, purchase/restart tests pass.

User explicitly confirms the3music tracks are NOT UPLOADED YET: integration deferred,
do not invent assets or claim music transitions verified. Prior Editor native ADTM
stop-time fatal remains undiagnosed. Full physical A56 performance/multitouch/audio/
human art acceptance remains. Build_7cc902915c2c succeeded124.941s,0errors,
977warnings(970package shaders+7API/config/development). ARM64 APK
build/Sprint0036/AKUMU2-sprint0036-dev.apk,67,236,575bytes;SHA256
5b3d9b4099571648cd6a478f193fe3a0bbf274d817cc411af3311d44877eda13.
See BuildStatus.json/ValidationSummary.json. Final Editor ready/stopped,Demo
open/clean; compilation clean,Console0errors/87warnings. Real save10000/revision35.
No source/asset edits during export; no stage/commit/push. Preserve prior dirty work.

## Previous Sprint 0035 handoff (historical)

Current brief: TestResults/Sprint0035/Brief.md. Implementation and scope:
TestResults/Sprint0035/Report.md. Preserve prior dirty work; no stage/commit/push.

0035: Level1 exactly10 Larpers+Samurai+fireworks+Officer; removed two supplemental
CombatHotfix guards. Existing route preserved. Rear concrete doorway (floor1.6,
lintel3.6) and confirmed-flank Officer exit; front slit/cover/0.5s laser preserved.
Basic pistol15 rounds; ordinary existing art without extended Larper overlay;
pistol ammo pack15, five-magazine allowance75. Mission card follows real vitals
geometry via UI layout event, fixing overlap without per-frame layout work.

Development money: catalog.developmentPlaytestCredits, compile-gated to Editor or
Development build. Atomic max(balance,10000)+developmentCreditRevision35 migration.
Actual existing profile2166=>10000 confirmed on disk. No repeat grants after
purchases; rich balances preserved; failed writes rollback/retry. Release disabled.

116/116 selected checks pass (111Play/5Edit), SelectedTestSummary.json; initial
failures/raw results retained in TestRuns.json and superseded by successful reruns.
Includes fall/late parachute zero damage, traversal/touch/slide, weapons/sniper,
healing/revival/grenades/armor, suppression/hearing/nav/Samurai, bunker/laser/exit,
opening pacing, economy/resume/rewards/shop/replay. Controlled routes use scripted
defeats or disabled enemies; no unassisted human full-combat run/device claim.

Android Development build_3c27a20026c6 succeeded488.978s,0errors/977warnings
(970Sentis shader,6existing API/config/development,1new development-guard warning).
APK build/Sprint0035/AKUMU2-sprint0035-dev.apk,67,236,575bytes;SHA256
13a770232c57a260202006cfd9633d40641f71f40870ffce57ea361c770f780c.
ValidationSummary.json/BuildStatus.json/ApkVerification.json. No source/asset
changes during export. ADB has zero connected devices.
Physical A56 FPS/frame-time tails/thermals/touch, full-combat feel, doorway tactics
and artistic acceptance remain. New runtime changes preserve existing pooling,
sampled perception, caches and Android60FPS request.

Final MainMenu/shop Play capture verified actual balance10000. On stopping that
session Editor showed fatal native audio ADTM deadlock dialog; cause not diagnosed,
Android not tested. Restarted saved project, retained Assets/_Recovery/0 (3).unity
without applying it. Final Editor ready/stopped; Demo open/clean,13actors,0missing
scripts; actual save still10000. Console3Pipeline startup command/timeout errors
and1warning; compilation clean. Do not claim no remaining crashes or audio issues.

## Previous Sprint 0034 handoff (historical)

## Authority / requirements
Original full Sprint0034 economy/shop/progression brief and acceptance criteria:
TestResults/Economy/Brief.md. Do not restart completed work. Prior completed arsenal
checkpoint: TestResults/Economy/PreviousCheckpoint.md. AGENTS.md and GRAPHICS_BIBLE.md
still apply. No stage/commit/push. No Level2/new weapons/AI/camera/graphics overhaul.

## Implemented
Existing catalog/session/local store/UI expanded, no parallel economy framework.
Eight existing firearms purchasable, permanent ownership/equip validation; separate
ammo packs/prices/reserve caps and whole-pack fit; basic pistol always available.
New profile$100 (existing profiles untouched), free basic40-round total mission
start floor, fallback to it if equipped guns empty. Premium ammo not granted.
Level1 audit15wallet drops of$5-20 each; completion$250; failure25% wallets capped$50.
Automatic pickup/obstruction checks preserved, amount+run total feedback; results
separate wallets/reward/payout/balance. Atomic rollback and run-ID payout dedup.
Armor/helmet condition/ownership/equip/remove/repair persist with existing armor
rules and no movement penalty; proportional bounded reserve cost. Medical only
tape/Faintanyl, exact description, two-revive cap and two-second immunity preserved.
Existing crates remain finite and honor equipped weapon reserve limit.

Pause/background saves exact unused supplies; resume must reserve on disk before
controls/simulation proceed. Failed resume save stays paused. OS-killed paused app
keeps unused stock. Only unannounced active-combat process kill forfeits carried
supplies (banked cash/ownership/unpacked overflow preserved). No per-frame/shot IO.
One existing PlayerCombat regression repaired: reload can take priority over
healing, but firing remains blocked while healing. Gun tuning/sniper safety intact.

## Evidence / modified files / balance
TestResults/Economy/Report.md lists all files, exact prices/rewards/limits and manual
checklist. Prices.csv / FinalCatalog.json / BalanceSimulation.txt / AmmoBudgetAudit.json.
Average first completion$538 incl starting100; SMG+ammo+tape leaves148; saving three
average completions affords$1200DE with214left, before other spending. Existing
five108-round boxes retained. These are scenarios, not measured player telemetry.
Ten scoped existing catalog/runtime changes among156 baseline originals;146 unchanged.
New EconomySetup.cs authoring helper and two regression files; native catalog APIs.
No scene/prefab/weapon-art/camera/AI/movement/control-layout changes.

## Verification / final state
78/78 selected automated checks pass (64Play,14Edit), ValidationSummary.json and raw
JSON. New economy11, legacy flow12, UI3, equipment4, medical1, wallets/healing9,
grenade4, arsenal8, lab1, performance2, camera9, rules/progress/combat14 totalEdit.
Old assertions updated for deliberate new prices/starter/payout/caps/reservations,
current visible empty revive pips, literal Faintanyl text and float epsilon. No
regression test removed. Transient test-start transport error archived and rerun.
Controlled shop/medical/results captures inspected. Not Android/human acceptance.

Final Android Development build_7641ac5dff71 Succeeded94.615s,0errors/6warnings.
APK: build/Economy/AKUMU2-sprint0034-dev.apk,67,236,575bytes.
SHA256 faafcb6b12ff4c5817dc83fd5ef881f587eb32cdae536077b4543fa31222a86d.
ApkVerification.json confirms ARM64 and new pause/economy metadata; catalog packed.
BuildStatus.json full final report; FinalBuildWarnings.json exact existing API/
directive/remote-Pipeline warnings. Earlier pre-pause build archived separately.
Editor ready, compilation clean, Console0errors/6warnings. Warehouse_Demo saved,
open, active, Play Mode stopped. No missing inspected scene/prefab scripts.
Git0staged;78modified/4pre-existingdeleted/169untracked overall. No commit/push.

## Next step
Actual A56 installation and Report.md manual checklist:60FPS/frame-time tails,
thermals, multitouch/shop scrolling/safe areas, sound and financial pacing. No adb
device attached. Preserve prior optimized systems and current ownership/saves.
Do not start future roadmap or retune prices without new playtest findings/request.
