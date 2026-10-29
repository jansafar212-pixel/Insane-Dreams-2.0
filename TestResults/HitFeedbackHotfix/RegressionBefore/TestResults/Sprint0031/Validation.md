# Sprint 0031 — validation notes

New UI suite: 3/3 passed. Uses advancing Editor frames, real GameView resolutions
1280x720, 1560x720, 1600x720, 1920x720, 1920x1080 plus simulated cutout insets.
Covers actual purchases/equips, persistence, settings, medical caps, insufficient
funds, selected grenade stock, ammo current/capacity, pause/death/results and
minimum 44-unit action hit heights. These are Editor checks, not Android-device tests.

Visual review covered MainMenu, Settings, LevelSelect, all four shop tabs,
insufficient-funds, HUD/grenade aiming, pause/death/victory and wide/1080p captures.
A missing tape-icon marker was found and fixed by reimporting HandmadeUI.uss after
creating the native icon assets; the refreshed Medical captures show the icon.
Title glyphs and PixelLogo method match the saved pre-sprint source exactly.
All protected gameplay hashes match. Warehouse_Demo differs only in six existing
sign font/material bindings (12 serialized field changes), recorded separately.

Historical suite failures found during expanded coverage (not hidden or disabled):
- Sprint005Tests: 10/11 passed. LarperReactsApproachesAndStopsFiringImmediatelyWhenOccluded
  fails its shots>0 check after a fixed 0.6-second wait, including isolated rerun.
  No production AI/weapon/prefab logic was changed for this UI sprint. Exact cause
  of this legacy combat-timing failure remains unconfirmed; no combat tuning was
  made to force the UI sprint green.
- Sprint0025Tests: 4/8 passed. Three old revival assertions assume two free starting
  injections, manual-revive UI and legacy defaults. Current ProgressStore defaults
  to zero and PlayerRevive uses automatic revival. A fourth old melee fixture
  dereferences a missing asset/component at line36. Newer shop/revival integration
  coverage passes; these old fixtures were preserved, not silently rewritten.

Raw suite results include full failure details. Test runner artifacts are under
this directory; FirstRun and isolated results retain the timing failure evidence.
Compilation and Android build status are recorded below when complete.

Manual gaps: human artistic acceptance, physical Android multitouch/cutout behavior,
small-phone reading/finger scrolling, sustained device performance and perceived
latency. Editor pointer tests and an APK build cannot establish these outcomes.

Final automated totals (unique tests, isolated rerun not counted twice): 113 run,
108 passed, 5 failed. Edit Mode: 21/21 passed. Play Mode: 87/92 passed.
New Sprint0031Tests: 3/3 passed. The five failures are described above.
Passing suites: CombatRules7, CowardsBunkerRules2, FirstOperationLayout2,
Sprint024Progress4, Sprint0029EquipmentRules5, SuppressionRules1;
Hotfix00253Playtest6, MovementCompletion11, PersonalityIntegration3,
Sprint00271(3), Sprint0029Systems4, Sprint0030(4), CowardsBunker4,
Sprint0224(9), Sprint023(9), Sprint0246(5), Sprint024Flow12.

Final native scene/prefab audit: Warehouse_Demo saved, stopped; missing scripts0,
prefab missing scripts0; Officer pillbox5solid faces, vest resources intact,
MainMenu remains first enabled build scene. FinalReferences.json records details.

Android Development build: Succeeded, zero errors. 972 warnings:970 installed
package shader warnings, one existing FindFirstObjectByType obsolete API warning,
one Pipeline runtime-config warning (Editor Pipeline is not enabled in the Player).
APK: build/Sprint0031/AKUMU2-dev.apk, 67,235,220 bytes.
SHA256: dd7c5b7104e646eb0538e62347ab3bbc901b87e70375e6d1ebaedd57a829080e.
APK contents differ from0030, including libil2cpp, metadata, scene data and shared
assets. The packaged shared assets contain HandmadeUI, wallet-row and purchase-state
markers (ApkContentAudit.json). This is a real new build, not a renamed previous APK.
Unity reports totalSizeBytes for all build artifacts, not compressed APK size.
Final Editor ready/not compiling/stopped; current Console errors0. Device execution
and frame-rate/multitouch acceptance remain unverified.
