# AKUMU 2 — Sprint 0028: The Coward's Bunker COMPLETE

Implemented by Codex — awaiting human playtest/artistic verification.
No pending implementation, test, or build. Do not regenerate Level 1 or repeat
completed grenade resupply/VFX/arm-threshold work.

Verbatim sprint brief: TestResults/CowardsBunker/OriginalRequirements.md.
Previous checkpoint: PreSprintResume.md there. Full file list, values, test details,
manual checklist and limitations: Report.md. Regression.json contains36/36unique
passes (31Play,5Edit); OfficerTrace.json proves fire→reach cover/crouch→emerge/fire
in a controlled scenario, not a human gameplay claim.

Implemented:11 reusable coarse native environment modules on512x512sheet,32PPU,
Point/binaryalpha/4native-pixel cells. Demo floor/wall/platform visual swaps and
static earth/grass/rocks/scraps/trees. Approved crates/characters/UI untouched.
Bunker uses existing dais/two cover positions at138/146, sandbags1.625x1.125,
new roof4.5x.375center(142.75,4.875), open rear and front peek at136. Native APIs only.

Optional scene-only references on existing OfficerShotTelegraph; EnemyCombat uses
ordinary reservations/path/hops, heard/visual memory, cover hold and recovery.
80%post-shot retreat attempt, .65authored-cover stand-off for safe hop run-up,
existing random hides and1.5suncertain sound investigation on hide exit. Full hop
clearance and actual arrival tested; no collision bypass or teleport. Desert Eagle
55damage/7mag/2.8reload,1.25–4deg base spread,.5swarning/.2scommit/magenta heavy tracer
remain. No new state machine/managers/AI framework or archetypes.

Pacing/layout: same15enemies [3,4,3,3,2], unchangedopeningtrio, horizontal route,
platform dimensions, six ladders/held-up exits and final dispatch/results. This
operation already has no checkpoint markers; existing design preserved rather
than adding new checkpoints. Only two small cover bounds changed and one roof
collider added; decorative scenery noncolliding.

Final tests: new bunker4Play/2Edit, existing Officer4,0027.1ladder/shop/accuracy3,
FirstOperation6,grenadehotfix4,throwables4,fall6; layout2Edit/0027.1rules1Edit.
Initial roof/run-up problems fixed. Fireworks approach6second test failed once,
passed isolated and full6-case rerun; no unrelated behavior change. Test JSON and
optional equipment-overlay assertions corrected without gameplay changes.

Remaining human checks: visual/art approval at phone size, Officer unpredictability
under real advances/flanking/pressure, grenade and ladder feel, real Android frame
rate/touch and intermittent fireworks behavior. No device playtest/APK build done;
export fresh APK before phone testing. Existing working-tree changes preserved;
nothing staged, committed or pushed. Warehouse_Demo should remain saved/open outside
Play Mode; FinalEditor.json records final native status.

Final native inspection confirms Warehouse_Demo saved/clean/open outside Play Mode, compile successful,0missing scripts,0Console errors.8warnings remain; see final Console and warning evidence.
