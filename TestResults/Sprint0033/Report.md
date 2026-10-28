# Sprint 0033 — scope and geometry

Before:4.5w x3.175h shell, floor1.6, roof4.775, clear interior3.75x2.8.
After:9w x6.35h shell, floor1.6, roof7.825, clear interior8.25x5.85.
Exteriorwidth/height2x, area4x; clear interiorarea~4.6x.
Leftfront140 unchanged, rear149; existing dais135.5..153 supports whole shell.
Opening .9->1.4high (2.5..3.9), peek141.05->141.25 (reachable wall-probe clearance); hide/spawn143.25->146.5.
Existing five solid concrete faces resized to match sprites; same native sprites
and assets reused. Side cutaway floor/ceiling fascia exposes full Officer silhouette.
No glass/transparency/shader/newcharacterart. Capture review confirms visible gun,
standing/crouch and longer interior retreat.

Roof now6.225m above dais; player unchanged maximum doublejump~4.2m. One existing-
system ladder at139 supplies a real route to the roof. No original ladder, platform,
exit, roster or encounter trigger moved. Existing movement/input/traversal source
unchanged. Regression route now climbs rather than requiring impossible height.

The new trigger exposed a genuine Officer laser bug: unfiltered Physics2D.Raycast
stopped at navigation triggers. It now uses a fixed-buffer non-trigger ray, matching
bullets and sight. No laser/tracer appearance, warning, damage or cadence change.
First-run collision fixture likewise now tests solid geometry rather than triggers.
ProtectedAfter.json expects only this targeted OfficerShotTelegraph source difference;
all captured AI0032/player/UI/weapon/prefab/Testbed files remain identical.

Modified: Warehouse_Demo scene; new Editor/Sprint0033Setup.cs; OfficerShotTelegraph.cs
laser ray only; CowardsBunkerTests, CowardsBunkerRulesTests, FirstOperationTests,
Sprint00271Tests, Sprint0030Tests, Level1CombatHotfixTests and FirstOperationLayoutTests geometry/route waits/assertions; graphics bible,
DEVELOPMENT_LOG, checkpoint and evidence. No prefabs or artwork assets modified.

The longer approach exposed a real wall-probe/anchor mismatch: Officer stopped at141.23 while the old141.05 goal required <.18u. Moving only the scene anchor inward.2u preserves existing AI and clears this boundary deadlock. Geometry assertion uses.001u float tolerance.

Validation updates: real trigger-clipping bug fixed; inward.2u firing-anchor change
fixes a wall-probe deadlock after longer approaches. Frozen combat fixtures now use
the actual authored anchor instead of hardcoded old coordinates. No AI/physics
threshold was relaxed. Bounds comparisons use .001u tolerance for float rounding.

## Manual Android playtest

1. Install the new0033APK; launch MainMenu -> First Operation -> equipment/start.
2. Check both joysticks, reload, healing, weapon/grenade selection, pause and safe
   areas. No control positions or hit zones were changed in this hotfix.
3. At normal gameplay zoom approach the enlarged pillbox: identify the Officer's
   full silhouette, Desert Eagle, crouch/reload and retreat. Check the red warning
   crosses the ladder without clipping, lasts.5s and precedes the distinctive shot.
4. Fire at concrete versus the slit. Defeat the Officer, climb the new left roof
   ladder by holding up, exit onto the roof, cross and descend to the exit. Existing
   parachute can be used for the taller descent; player fall/jump rules unchanged.
5. Review phone readability, finger occlusion and sustained frame rate on hardware.
   Editor captures, scripted physics and an APK build do not establish those results.

Previous approved sprites and chunky palette reused; no new graphical assets or
shader. Longer paths naturally take longer to walk; weapon and AI timing values
were not modified. Original15-enemy roster and all prior terrain were preserved.


## Test coverage

Play Mode selected coverage25/25passed: bunker collision/laser/peek-hide/reload4,
0030vest/retreat4, actual FirstOperation6including physical ladder completion,
all warehouse ladders/medical controls3, mobile-aspect/UI integration3 and locked
Officer/combat5. Tests retain damage, timing and clipping assertions; geometry-
dependent coordinates, ladder counts and route/walk timeouts were updated.
Actual GameView layout checks include1280x720,1560x720,1600x720,1920x720 and1920x1080
plus simulated cutout insets. These are Editor layout/touch-path checks, not a phone.

Captured advancing Play Mode exposed/hidden HUD frames were inspected. Compared
with Before.png, full Officer/gun/crouch silhouette is now visible. Corrected laser
passes the roof ladder and still clips to actual concrete. Initial failed checks
are archived (trigger ray, reachable anchor, old frozen coordinates, float tolerance)
and rerun after the focused fixes; final results follow below.

## Final tests and export readiness

36/36selected tests passed (25PlayMode,11EditMode), no skips. Final scene/prefab
scan found0missing scripts and confirmed five solid concrete faces, valid vest
resources and unchanged Officer50HP/50protection. MainMenu remains the first enabled
build scene; Demo and Mission01 remain included. Demo saved/clean/outsidePlay.

Existing Android settings verified: IL2CPP,ARM64, minimum API26, automatic target
SDK, LandscapeLeft, APK (not app bundle), developmentfalse. Settings preserved.
Native release build queued to build/Sprint0033/AKUMU2-playtest.apk; final outcome
follows. Editor-only AI Inspector is excluded from Player; no new runtime overlay.

## Android release result — succeeded

APK: build/Sprint0033/AKUMU2-playtest.apk (39,750,643 bytes), ARM64 release,
zero build errors. 972warnings:970existing package shader warnings, one obsolete
API warning, one optional Pipeline runtime-config notice; same categories as prior
exports. Build took676.705s. SHA256:
c5b999f4c2733f20dd67e77bead32f60bd972f24364d0a75866d74596fe1f9c8.
APK scene data contains PillboxRoofAccessLadder; compiled metadata contains0032
viewportAttackDelay/stuckRecoveryPause. The new bunker AND prior AI work are in
this export. ApkContentAudit.json records evidence. No physical phone run performed.

Final Editor ready/not compiling/PlayMode stopped, Demo saved/open/clean. Console
errors0,82collapsed preexisting build warnings. No missing scene/prefab scripts.
All protected files match except the explicitly documented laser-trigger correction.
Android settings unchanged. Git: existing dirty work preserved,0staged, no commit/push.

Captures: [before](Before.png), [world cutaway](After.png),
[exposed HUD](OfficerExposedHUD.png), [hidden HUD](OfficerHiddenHUD.png).
World comparison captures use framing appropriate to each structure; physical
2xwidth/height is verified by bounds tests, not inferred from screenshot pixels.
