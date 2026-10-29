# Urgent wallet hotfix — 2026-10-28

## Confirmed cause

The old GameSession.ApplyDevelopmentCredits implementation was enclosed in
`#if UNITY_EDITOR || DEVELOPMENT_BUILD`. In a normal release Android player it
compiled to `return false`. DemoCatalog's **serialized** startingMoney is 100,
and EconomySetup also authors that value. Thus a clean release installation
creates a new 100-credit profile; deleting application data cannot fix this.
Changing GameCatalog's C# default (1000) would not override its serialized 100.

A second old condition, developmentCreditRevision >= 35, prevented replenishing
an already-granted profile after spending. The new explicit requirement supersedes
the previous once-only/development-build-only behavior: restore a minimum at every
initialization, including release APKs, without adding 10000 repeatedly.

This is a confirmed source/build-configuration defect explaining 100 in release
players. The exact APK installed on the user's disconnected phone was not pulled
or identified; do not claim its build flags or provenance were physically verified.
Previous sprint deliverables were named Development APKs; a different manual
release export would silently omit the old grant.

## Assignment and initialization trace

Project source/native-asset searches are saved in WalletSearch.txt, Assignments.txt
and HundredSources.txt (Assets, Packages and ProjectSettings). Library/generated
build caches are not additional authored wallet managers.

The actual spendable wallet is **GameSession.Data.balance**, inside GameProgress.
GameFlowController.Start creates GameSession when no singleton exists, then calls
Initialize; GameSession.Awake also initializes if a serialized catalog is supplied.
Awake rejects duplicate sessions instead of replacing the singleton. No serialized
GameSession component references were found by the script GUID search in scene,
prefab/data assets; the main-menu flow dynamically creates it. There is one native
GameCatalog asset, DemoCatalog, bound to the MainMenu GameFlowController.

Initialize: ProgressStore.Load(NewProgress) -> Normalize -> EnsureStarter -> armor
migration -> settle interrupted run -> ApplyDevelopmentCredits -> return to caller.
Only after this synchronous sequence does GameFlowController draw its menu/shop.
Existing saves supply balance through JSON deserialization; fresh saves use the
catalog's serialized 100. Normalize clamps, it does not reset to 100.

Production balance assignments:

- GameSession.NewProgress: catalog.startingMoney (currently 100).
- ProgressStore.Load: deserializes saved balance; GameProgress.Normalize clamps
  to 0..1000000. A corrupt save remains protected/read-only.
- GameSession.ApplyDevelopmentCredits: temporary `Max(balance, 10000)`.
- GameSession.Buy: subtracts actual item.price, then atomic commit.
- GameSession.Settle: adds bounded catalog payout once per run.
- GameSession.Commit: restores prior JSON snapshot on failed save.

The only authored 100 start sources are DemoCatalog.asset and EconomySetup.cs.
The C# GameCatalog default is 1000, not 100. No later runtime reset to 100 was
found. PlayerSupplies.Earnings/pendingEarnings count collected mission wallets;
they are not a separate shop wallet. Level selection and shop both render
session.Data.balance; CanBuy and Buy use that same property. No display-only
credit counter was introduced.

## Fix

Removed the build-symbol exclusion and the revision-as-blocker. The existing
catalog developmentPlaytestCredits option now explicitly controls a **temporary
all-build playtest floor**, including release APKs. At the end of initialization,
eligible writable profiles below 10000 are raised to 10000 and saved atomically.
Higher balances are preserved. Revision35 remains compatible metadata but no longer
prevents replenishment. Purchases during a live session deduct normally; only a
new initialization restores the floor. There is no per-frame or per-purchase grant.
All other profile fields, equipment, ownership and progression stay intact.

As before, write failures roll back; corrupt/read-only saves are protected rather
than overwritten. A damaged/unwritable filesystem is not claimed to support a
persistent grant or usable purchases. Interrupted-run settlement still precedes
the override; the floor no longer silently skips merely because activeRun remains.

## Temporary diagnostics and removal

Build tag: **wallet-release-floor-20261028-r1**.
Logs prefixed `[WalletHotfix wallet-release-floor-20261028-r1]` identify GameSession
NewProgress, Initialize.Load, Initialize.Final, duplicate rejection, and atomic
transaction callers (Buy, Equip, Settle, ApplyDevelopmentCredits). They include
before/after balance, save success, configured start, floor switch, development
flag and Application.buildGUID. No save-path or inventory dump is logged.

Before final commercial release, disable **developmentPlaytestCredits** and
**playtestWalletDiagnostics** on DemoCatalog in the Inspector. The original
startingMoney 100/prices/rewards remain untouched. The diagnostic method/build-tag
and two switches are isolated for removal. No change to final economy tuning.

## Verification

- Five wallet/pistol checks pass, including fresh profile, existing low profile,
  previously marked low profile, rich profile, disabled switch, atomic failure,
  actual weapon purchase/equip and relaunched floor. Exact pre-restart deduction
  is checked on disk before the relaunch replenishes it.
- Eleven economy checks pass: ownership, ammo, armor, supplies, payouts,
  interruption, pause/background, rollback and replay. Tests specifically examining
  final economy snapshots opt out of the temporary floor via the existing test
  initialization argument; production calls retain the default true.
- Two UI integration checks pass: real shop categories, 10000 displayed from the
  same session wallet, purchase deduction, restart and retained ownership.
- Twelve campaign-flow checks ultimately pass. One physical gap-jump test failed
  on the first suite run and passed an isolated rerun without gameplay changes;
  the initial raw failure is retained, not hidden.

**30 unique selected Play Mode checks passed on their latest runs.** Raw results:
TestRuns.json. These are Editor automated tests, not Android device verification.

Release request uses only DetailedBuildReport, no Development flag;
EditorUserBuildSettings.development=false. Android custom defines contain no
manually supplied UNITY_EDITOR or DEVELOPMENT_BUILD. RuntimeConfiguration.json
records the enabled native switches, 100 baseline, catalog binding and build tag.
Final release build/packaging evidence is appended below after export.

ADB reports no connected devices. Therefore actual fresh installation, existing
phone-save migration, launching the APK and purchasing on Android remain untested.
The new APK must be installed instead of older Sprint0035/0036 exports. Identify
it by its unique filename/hash and the WalletHotfix build tag plus build GUID in
`adb logcat -s Unity`. No claim is made that the phone is already running it.

## Files modified

Runtime: Assets/Scripts/Progression/GameSession.cs;
Assets/Scripts/Progression/GameCatalog.cs.
Native catalog via Unity Editor API: Assets/Data/Progression/DemoCatalog.asset
(playtestWalletDiagnostics=true; existing floor switch enabled; startingMoney100).
Tests: Assets/Tests/PlayMode/{Sprint0035Tests,Sprint0034EconomyTests,
Sprint0036PresentationTests,Sprint024FlowTests}.cs.
Handoff: DEVELOPMENT_LOG.md, SPRINT_RESUME.md; TestResults/WalletHotfix evidence.
Editor's Development build option is intentionally off for this export.
No staging, commit or push. Earlier dirty project work preserved.

## Final release artifact

Release build **build_a43a5d61425c succeeded in 143.806 s**, zero errors,
974 warnings (full report in BuildStatus.json). New file:

`build/WalletHotfix/AKUMU2-wallet-release-20261028-r1.apk`

39,815,271 bytes (~38.0 MiB); SHA256:
`5fbe6b8aefb7a4ecc096148462f5ab127257e34da4fad0d46ad3065fbe4ccce8`.

`aapt dump badging` confirms package com.GAMES.AKUMU2, versionName0.12/versionCode1,
ARM64 and no application-debuggable marker. This is a release APK, unlike the
previous ~64MiB Development exports. The version is unchanged; **use the filename,
hash and runtime build tag**, not versionName alone, to distinguish this export.
APK metadata contains the unique hotfix tag, diagnostics field and grant method.
The actual generated release method saved in ReleaseWalletGenerated.cpp.txt
contains the 10000 comparison, Mathf.Max and atomic Commit; it is not an empty
release stub. BuildReport confirms DemoCatalog is packed, and its serialized floor
and diagnostics options are enabled. ApkVerification.json records these checks.

Final additional Edit Mode economy checks pass3/3: **33 unique selected checks
pass on latest runs (30Play/3Edit)**. EditTests.json supplements TestRuns.json.
Final Editor compilation clean/stopped; release option off, both playtest switches
on. Actual local persistent profile remains10000 (read-only audit).

No Android device was attached, so **this APK has been built and inspected, but
not installed or run on the user's phone**. Fresh-install/existing-install behavior
is covered by isolated profile tests and release-code inspection, not device tests.
Install this exact APK (for example `adb install -r <this APK>`) and check the
WalletHotfix tag/buildGUID in Unity logcat before attributing a device result to it.
No need to clear application data for this override; doing so would discard other
progress, which this hotfix is designed to preserve.
