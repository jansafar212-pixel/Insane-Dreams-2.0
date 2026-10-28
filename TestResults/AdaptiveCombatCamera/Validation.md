# Camera overhaul validation — 2026-10-27

Final selected runs: **63 tests, 61 passed, 2 failed, 0 skipped**.
Raw final results and ValidationSummary.json are in this folder. Archived first
attempts are excluded from these counts.

| Mode / suite | Passed / total |
|---|---:|
| Play: AdaptiveCombatCameraTests (new) | 9 / 9 |
| Play: LadderCameraHotfixTests (expanded directions) | 1 / 1 |
| Play: legacy camera reversal test | 1 / 1 |
| Play: FirstOperationTests | 6 / 6 |
| Play: Sprint0031Tests (existing UI) | 3 / 3 |
| Play: Hotfix00253PlaytestTests | 6 / 6 |
| Play: GrenadeResupplyHotfixTests | 4 / 4 |
| Play: Sprint0027FallTests | 6 / 6 |
| Play: Sprint0032Tests | 10 / 10 |
| Play: Sprint013Tests | 4 / 6 |
| Edit: CombatRulesTests | 7 / 7 |
| Edit: CowardsBunkerRulesTests | 2 / 2 |
| Edit: FirstOperationLayoutTests | 2 / 2 |

The two unchanged Sprint013 fixtures fail on blind-fire timing and an exact
LineRenderer count. See Report.md for evidence and limits. They use a standalone
camera without the new follow component; no unrelated AI or VFX was changed.
The suite is not reported as wholly green. No complete project-wide run claimed.

New controlled checks cover aim opposite movement, aim without firing, memory,
noise and direction chatter, immediate player velocity, nearby Samurai on both
sides, removal of threats, falls/parachute/landing, vertical aim, and bounds at
16:9, 19.5:9, 20:9 and 8:3. Deterministic steps cover 30/60/120 Hz and irregular
delta time. A warmed 1,000-step camera fixture measured **0 managed bytes**;
this does not represent whole-game allocation or mobile performance profiling.

Final native audit: compilation successful, Editor ready, Play Mode stopped,
Warehouse_Demo open and saved, 0 current Console errors and warnings, 0 missing
scene scripts, 0 missing scripts in inspected Prefabs and FirstOperation assets.
The Console's historical buffer is distinct from its current ground-truth counts.

ProtectedAfter.json confirms 27 protected non-scene files match their starting
hashes. PlayerActions matches its starting content after removing only the new
read-only getter. SceneScopeAudit.json records unchanged scene object IDs and no
unexpected object changes; settings were saved with native Editor APIs.
SnapToTarget callers are initialization, spawn/replay and checkpoint recovery.

No Android device test, human gameplay approval, APK export, or device 60 FPS /
frame-pacing / input-latency measurement was performed for this camera revision.
The prior Sprint0033 APK predates this work. Build this revision for device review.
Report.md contains the concrete phone playtest checklist and pixel-phase caveat.

Git: existing dirty work preserved; 0 staged files; no commit or push.

Allocation measurement correction (Android performance sprint, 2026-10-27): the
current Unity Mono runtime returns 0 from GC.GetAllocatedBytesForCurrentThread even
when allocating a known 8 KiB array. The earlier 1,000-step camera '0 bytes' result
is therefore unsupported, not proof of zero allocations. See
../AndroidPerformance/AllocationCounterCheck.json. Camera behavior test outcomes
remain valid; reliable allocation profiling is still required on a Development Player.
