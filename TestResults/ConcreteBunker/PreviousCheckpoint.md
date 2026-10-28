# Sprint 0029 — Operation: The Bunker Is Real

Sprint implementation and automated/build validation are complete.
Do not restart or rerun the scene-generation recipes. Preserve the dirty working
tree and prior sprints. No stage/commit/push is authorized.

## Authoritative requirements

Full latest brief: TestResults/Sprint0029/OriginalRequirements.md (verbatim).
Original user attachment:
C:/Users/jansa/.codex/attachments/93d05ec9-2ac8-40b2-86fa-fda7bf46b03e/Vložený text.txt
Previous checkpoint preserved in TestResults/Sprint0029/PreSprintResume.md.
Implementation/values/files/manual checklist: TestResults/Sprint0029/Report.md.

## Completed

A: Real foreground bunker collisions, matching sprite bounds and safe doorway;
Officer occupancy/peek/muzzle validation; bounded close-box run-up/replan; existing
six ladder exits and Samurai platform pursuit verified; shared uncertain enemy
shot/explosion hearing; configured initial reactions and short-LOS continuity.

B: Four shop categories and exact Medical copy; max2 purchased-only automatic
revivals, consume/persist on death, exactly2s protection, no checkpoint refills;
existing three grenade types/aim-preview-release retained; light/heavy ballistic
vest and helmet, separate regional condition, swords bypass; runtime reload and
carrying tradeoffs, saved overflow, no movement speed penalties. Native helmet
overlays preserve rigs and physics.

C: Original AKUMU Block font shared by menu/shop/HUD; Medical buttons both visible;
current/capacity and separate reserve fit; syringe icons; existing touch sizes and
positions. Advancing Editor captures visually inspected. Updated graphics bibles
record approved0028environment preservation and new UI/equipment review status.

## Validation

154 unique tests passed:132PlayMode,22EditMode,0failures/skips in latest results.
TestResults/Sprint0029/Regression.json deduplicates by full test name. Individual
JSON contains exact results. Includes real hitscan/bunker obstruction and grenade
roof collision/occlusion/flank defeat, Officer warning/direction/retreat, ground
route/results, small-collider Samurai elevated pursuit, expert commitment, all
six ladders, cover/movement/doublejump/slide/parachute zero fall damage,
suppression/morale/spacing, grenades/finite crates, regional damage/ragdolls,
medical/armor/reload/carrying/save flow/pause/checkpoints/results.

Initial failures resolved: actual header clearance and Officer ray beyond target;
Medical/reserve layout; null early-bound syringe icon. Older tests updated for
Officer sheltered position, exact Medical copy/four categories, explosion bypass;
new bounds assertion corrected to2D (SpriteRenderer has artificialZextent.2).
No gameplay weakening to satisfy old fixtures.

ProtectedBefore/After.json confirms identical environment sheet, old environment
recipe, Testbed scene, ragdoll source and ragdoll prefab. FinalReferences.json:
Warehouse_Demo clean/open/outsidePlay;0missing scripts in scene/checkedprefabs;
MainMenu first enabled build scene. ConsoleBeforeBuild.json:0actualerrors/warnings,
compilation clean. Historic Pipeline buffer counts are not current Console state.

## Final state and next step

Android ARM64 IL2CPP Development build SUCCEEDED,0errors,595.862s.
APK: build/Sprint0029/AKUMU2-dev.apk (67,235,131bytes).
AndroidBuild.json/AndroidBuildReport.json contain summary/fullreport.972build
warnings remain (970inference shaders,1obsoleteAPI,1Pipeline disabled notice).
FinalEditor.json: clean/open/savedWarehouse_Demo,outsidePlay,notcompiling,
0missing scripts. FinalConsole.json:0errors,82collapsedwarnings afterbuild.
Protected asset hashes still match. GitFinal.txt: preserved dirtytree,0staged.
No stage/commit/push. No further implementation task pending for0029.

Next action is human gameplay/artistic/device review using Report.md checklist.
Do not restart0029 or rewrite functioning features. Do not alter approved0028art.
## Remaining human verification

No known unresolved automated failure. No Android hardware or human gameplay/art
approval performed. Review bunker fight/flank readability, enemy pressure and
recovery, native font/gear aesthetics, safe-area/touch comfort and sustained
phone performance. See manual steps in Report.md. APK success alone is not proof
of phone gameplay or performance. No performance profiler measurements claimed.
