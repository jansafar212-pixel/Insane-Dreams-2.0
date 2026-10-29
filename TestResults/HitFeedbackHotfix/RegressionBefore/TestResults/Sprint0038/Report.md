# Sprint 0038 — small visual polish

Character bullet hit-confirmation sprites now use 0.216 instead of 0.18 world scale
(20% larger) for ammunition attacks from either team. Melee, geometry/corpse cues,
particles, colors, durations, damage and reaction strengths are unchanged.

The default BackupPistol had a null MagazineSprite while its reload presenter
expected a detachable sprite. Its grip/magazine was baked into the weapon art;
an earlier fix removed the inherited extended overlay without supplying a short
replacement. Extracted the original 4×4 grip into a separate flush magazine and
removed those exact pixels from a new body variant. Recomposition differs by zero
pixels from the original 20×8 pistol. Original art is preserved. Capacity remains
15, reload 1.5 seconds, atomic ammunition transfer unchanged. Null-sprite fallback
now avoids presenting/releasing an invalid magazine. The existing pistol reload
presenter and its existing three-slot debris pool perform the exchange.

Five static background marks in Warehouse_Demo: two uneven LARP ZONE tags,
two WORK SUCKS tags and a small bad skull, spread over the long factory.
Rust/paper colors reuse the environment palette, Point filtered, 32 PPU,
two-native-pixel strokes. Sorting -25, behind props/actors/platforms and in front
of the -30 wall bays. No colliders, runtime scripts, materials or layout changes.
The last tag was moved to x130 to clear the ladder and a structural beam.
Other warehouse scenes remain unchanged. Controlled authoring-camera captures
were inspected; these are not human artistic approval or Android playtesting.

## Validation

- Compilation completed without errors; scoped source whitespace check passed.
- EditTests.json: 11/11 reload rules pass.
- ReloadPlayTests.json: 5/5 pass, including the new default-pistol test: facing
  both ways, detached/moving/replacement/seated states, 7+20→15+12, 7+3→10+0,
  cancellation on switch preserving ammunition. Existing SMG, shotgun, heal
  cancellation and HUD ammunition checks also pass.
- CombatPlayFirst.json: 7/9 pass, including the new actual-raycast test for
  both teams' 0.216 character cues and unchanged geometry cue size.
- Two legacy failures reproduced with pre-sprint CombatFeedback and
  PistolMagazineReload source restored temporarily (CombatBaseline.json):
  LarperEngagesShootsAndReturnsToPatrolWhenTargetLeaves expects Patrol after
  3.2s but sees Searching; PlayerFireSwitchInterruptAndDeathRecoveryPreserveInventory
  expects immediate 100 HP after lethal damage but sees 0. Neither AI nor recovery
  was changed. These remain unresolved outside this small sprint.
- Baseline run additionally fails the NEW size test at the original 0.18, as
  expected. Final source/assets were restored, compilation passed, and
  ImpactFinal.json confirms that new test passes again.
- Final selected coverage: 23/25 unique tests pass, two baseline failures above.
- PistolArtAudit.json: identical recomposed silhouette, capacity15/reload1.5.
- Warehouse_DemoAudit.json: existing protected scene entries unchanged by adding
  the decoration root. No level-layout or gameplay component changes.

No new APK was requested/built and no Android-device verification is claimed.

## Files

- Assets/Scripts/Combat/CombatFeedback.cs
- Assets/Scripts/Visuals/PistolMagazineReload.cs
- Assets/Data/Items/BackupPistol.asset
- Assets/Scenes/Warehouse_Demo.unity
- Assets/Editor/Sprint0038Setup.cs (+meta): native Editor asset authoring only.
- Assets/Art/Sprint0038/: PistolBody, PistolMagazine, LarpZone, WorkSucks,
  BadSkull sprites and their textures/metas; folder meta.
- Assets/Tests/PlayMode/ReloadRepairPlayTests.cs
- Assets/Tests/PlayMode/CombatIntegrationTests.cs
- DEVELOPMENT_LOG.md, GRAPHICS_BIBLE.md, SPRINT_RESUME.md and this evidence folder.

Two pre-existing unsaved StorageShelf position edits were preserved separately
in UnsavedSceneBefore.unity / UnsavedShelfEdits.json, excluded from the saved
sprint scene, and restored to the open Editor scene after testing. That scene
intentionally remains dirty. Test screenshot overwrites outside this evidence
folder were restored. No staging, commit or push.
