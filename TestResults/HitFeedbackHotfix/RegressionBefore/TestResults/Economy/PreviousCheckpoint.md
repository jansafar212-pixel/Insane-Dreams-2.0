# AKUMU 2 continuation checkpoint — implementation and export complete

## Active work and authority
Actual latest active sprint is the user's Sprint 0033 Arsenal Expansion, following
adaptive combat camera 0031 and Android performance 0032. Earlier numbered bunker
sprints are separate and preserved. Exact arsenal brief and acceptance criteria:
TestResults/Arsenal/Brief.md. Prior checkpoint: TestResults/Arsenal/PreviousCheckpoint.md.
Current details/files/manual checklist: TestResults/Arsenal/Report.md.

Do not restart completed work. Preserve all uncommitted changes. No stage/commit/push.
Future shared grenade inventory, AA-12/P90/Glock19 variants, lightweight secondary
restriction, economy and campaign expansion remain deferred. Latest restoration
explicit LMG specification supersedes original provisional tuning: 100 loaded,
200 developer-lab reserve, 8-second reload. Campaign allowances remain unchanged.

## Completed and verified automatically
- Adaptive aim-priority camera and performance pass already implemented; preserved.
- Seven core weapons plus retained automatic Glock, shared hitscan/pellets,
  spread/bloom/falloff, per-gun muzzle/tracer/recoil/sound settings.
- Sniper hold/release firing, cooldown, safe cancellation on pause/death/reload/
  switch/focus/reset; right-stick cancellation preserves left-stick movement.
- Editor/Development-only Arsenal Lab, isolated profile, eight equip/refill options.
  MainMenu -> standalone Testbed -> normal menu integration preserves save bytes.
- Native asset authoring, original gun assets/stats and enemy weapons preserved.
  Player prefab tracer pool 6 -> 16; Testbed appended to build list, MainMenu first.
- Restoration corrected LMG 75/180/4.2 -> 100/200/8, added configurable lab reserve.
- Editor/Development frame overlay: 120-sample FPS/mean/p95/p99, text updates 1Hz,
  reusable storage, paused samples ignored. No framecap/input/camera modifications.
- 73/73 selected checks pass: 59 Play Mode, 14 Edit Mode. Raw JSON and summary in
  TestResults/Arsenal. Native sprite-ID test assertions replace unreliable managed
  Sprite-wrapper reference comparisons; runtime graphics unchanged.
- Controlled eight-gun capture/hitbox/reload/movement/sniper tests and stress
  360 trigger pulls /1080 rays. These are not human playtest or device evidence.
- Native audit: Demo saved/open/stopped, no missing scene/inspected prefab scripts,
  clean compilation/current Console. 423 protected original files: 412 unchanged,
  eleven scoped existing changes, no scene content/camera/AI modifications.

## Export complete / next step
First export cancelled; retry build_369cf4a48eb7 Succeeded in324.143s with zero errors
and980 warnings (mostly Sentis shader warnings; see BuildWarningSummary.json).
Fresh APK: build/Arsenal/AKUMU2-arsenal-dev.apk,67,238,173bytes.
SHA256 c6beca846c937fe34dd3abf4e36840529e1287061e0db4b64417d6ab9d8718e8.
ARM64 libraries, developer overlay/lab/release-fire metadata, arsenal/catalog and
Testbed packaging verified. MainMenu first. APK not installed on hardware.
Final Editor ready, no compilation failure/Console errors;90 build warnings remain.
Warehouse_Demo open/saved/stopped. No staged changes, commit or push.
Next step is human/Android playtesting using TestResults/Arsenal/Report.md checklist,
not restarting implementation or starting future roadmap features without request.
## Remaining verification / limitations
No Android device attached. A56 real 60FPS/frame pacing, thermals, multitouch,
weapon feel, audio and art approval require manual playtesting. See report checklist.
Host FMOD output-device warnings occurred earlier; sound audition unverified.
Mono per-thread allocation counter returns zero even for known allocations, so
zero-allocation claims are withdrawn; allocation profiling requires supported tools.
Historical Sprint013 failures were not fixed or claimed passing.
Do not rerun one-time Python patch helpers under TestResults.

