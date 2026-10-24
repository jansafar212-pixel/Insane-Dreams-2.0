# Sprint 024.5 completion report

**Implemented by Codex — awaiting playtest and artistic approval.**

## Graphics and references

The actual Enemak folder was accessible. Inspected `Postava.png`, `Postava 1.png`, `Postava 2.png`, `1c.gif`, `2b.gif`, `6d.gif`; local copies are under `HistoricalArtSources/Drive/Postavy/Enemak/`. Other entries were listed, not individually inspected. Selected GIFs each contain one frame, so they were treated as pose references. Original files were not changed. Measured dimensions/palettes and provenance are in `ART_BIBLE.md`; hashes in `Sprint0245_ReferenceHashes.json`.

LarperCandidate uses a primitive red shirt, blue trousers, dark shoes/hair and uniform peach skin, with no helmet or headband. Full body parts retain 32 PPU and the existing 12×11 head/12×17 torso/5×18 arm/6×18 leg dimensions. Six existing expressions plus an optional distinct full-panic face use tiny pixel changes. Upper/lower arms, wrists/hands, thighs, knees, shins, ankles/feet share the optional segmented cutout extension; actual pistol aim/reload poses feed its hand target. No new animation framework or live visual physics.

SamuraiCandidate shares the same density/palette simplicity, retains teal outfit, red sash, navy hair/topknot and katana. Detailed older limb chains inherited alongside the new chains were removed to prevent duplicate rendering. The existing sword was reduced to 40×10/four flat colors with unchanged 1.25-unit width. Grip follows the hand, including mirrored poses. Damage, aggression, mobility, navigation and jump tuning were not changed.

Candidates are integrated reversibly on the relevant gameplay prefabs for review. Earlier skins/art remain intact; the generic old visual prefab remains a fallback. **Neither candidate is approved as a permanent master.**

Reference sheet: `Sprint0245_ReferenceSheet.png`. Built-in imagegen source/prompt: `Assets/Art/Sprint0245/CandidateSource.png`, `Sprint0245_ArtPrompt.md`. Native Unity authoring samples that source to a tiny flat-color atlas; it does not relabel generated art as historical recovery.

## Armor

An event-driven `ArmorVisual` observes existing BodyArmor equipment/condition. No armor → hidden; above 50% → intact; at/below 50% → damaged; depleted during the equipped run → broken. A 10×13 vest follows the torso and copies its last visible state onto the ragdoll torso. Existing damage absorption/capacity and equipment values are unchanged. The Demo player has the overlay alongside its existing armor component; ordinary Larpers have no armor and were not given protection.

Existing saves only encode remaining armor condition, so exhausted and unequipped armor cannot be distinguished after a fresh restore. Broken presentation persists for the current equipped run; no save-format migration was introduced.

## Warehouse interaction fixes

- **Root cause, evidence/checkpoint:** Mission_Records contained the evidence and CheckpointMarker on the same object; both entering records and collecting evidence committed a checkpoint. Evidence remains at x20/east mezzanine. An independent OfficeCheckpoint zone/flag is now at (15.3,6.4), with supported respawn (14.5,6.25), in both Demo and Mission01. Its 1.3-wide trigger ends at x15.95; the 2-wide evidence trigger begins at x19, leaving 3.05 units between them versus a .8-unit player width. Collection snapshots objective/ammo progress without activating another checkpoint. The existing cyan EXIT frame/results label remains distinct; completion gates are preserved.
- **Root cause, ladder/cover:** ladder acquisition applied `Abs(climbInput)`, allowing down/crouch input to acquire a ladder at any reachable height. No cover ownership was checked before traversal took the motor. Down now enters only a valid ladder top; the existing bounded cover probe gives intentional grounded cover priority before posture/traversal. Covered players keep control until they exit; upward climbing remains available afterward. Capture margins, alignment, gravity restoration and jump budgets are retained. The real LowCover_Sprint012 at (-14,.45) near the west ladder x−12 was checked behind the box, on its top and falling off its right edge. Separate left/right synthetic cases cover overlapping top/mid-ladder arrangements.

## Files modified for this sprint

- Runtime: `Assets/Scripts/Combat/BodyArmor.cs`, `PlayerCover2D.cs`; `Assets/Scripts/Traversal/PlayerTraversal2D.cs`; `Assets/Scripts/Visuals/ArmorVisual.cs`, `SamuraiArticulatedRig.cs`, `CutoutRig.cs`, `CartoonRagdoll.cs`, `CharacterSkin.cs`; `Assets/Scripts/Missions/MissionTrigger.cs`, `WarehouseMission.cs`.
- Authoring: `Assets/Editor/Sprint0245Setup.cs`, `Sprint0245Preview.cs`; `Assets/Art/Sprint0245/`; `Assets/Data/Visuals/LarperCandidate.asset`, `SamuraiCandidate.asset`; `Assets/Data/Items/SamuraiSword.asset` (sprite reference only).
- Prefabs: `Assets/Prefabs/Enemies/Larper.prefab`, `Samurai.prefab`, `FireworksExpert.prefab` (preserves its own presentation by opting out of inherited Larper segments). Warehouse_Demo/Mission01/Testbed were saved through Unity APIs; evidence/checkpoint edits apply to Demo/Mission01, armor to the existing Demo wearer.
- New tests: `Assets/Tests/EditMode/Sprint0245RigTests.cs`; `Assets/Tests/PlayMode/Sprint0245Tests.cs`, `Sprint0245SceneTests.cs`.
- Existing regression expectations updated without removing cases: Edit Mode `Sprint008PresentationTests`, `SecondArmRigTests`, `Sprint014RulesTests`, `Sprint0244RigTests`; Play Mode `SecondArmIntegrationTests`, `Sprint014MissionTests`. Updates reflect native candidate sprites, 14-body segmented corpses and independent checkpoint activation.
- Documentation/artifacts: `ART_BIBLE.md`, root `AGENTS.md`, `DEVELOPMENT_LOG.md`, this report, JSON results, reference hashes, prompt record and captures.

## Verification

**Unity Edit Mode: 63/63 passed. Focused Play Mode: 155/155 passed**, across 20 fixtures. Complete results are in `Sprint0245.json`. These include all new rig/armor/warehouse tests, Samurai pursuit/attack/platform jumps, visual/ragdoll integration, player cover, movement/double jump/ladders, spawning/persistence, morale/suppression, healing/supplies, shop/wallet/save/pause/results and mission progression. The full Play Mode assembly was not run; no blanket full-suite claim is made.

Controlled Unity Play Mode camera renders at 1280×720, orthographic size 5.113636, using the actual WarehouseCameraFollow zoom: `Sprint0245_Warehouse_Opening.png`, `Sprint0245_Warehouse_Samurai.png`, `Sprint0245_Warehouse_CheckpointEvidence.png`. The player was placed and protected in memory for art inspection; armor was temporarily restored in Play Mode. No GameSession was present, so these captures did not touch the real wallet/save. These are world-camera captures without overlay HUD and are **not human gameplay verification**.

No added per-frame physics/global searches. Armor updates use notifications; cover intent uses a bounded, non-allocating 10 Hz local probe only while eligible. Segmented ragdolls reuse the existing capped lifecycle. No Android device/APK test or performance benchmark is claimed.

## Manual review still required

1. Open MainMenu → Play → Warehouse Demo, buy/equip armor if desired. Approve/reject Larper appearance at ordinary zoom and compare Samurai simplicity against the reference sheet.
2. Watch running, crouch/cover, reload, panic, hit and defeat poses from both directions; check wrist/weapon and knee/ankle connections during actual combat.
3. At the low box near the west ladder, press/hold down on desktop or touch: cover/crouch must stay intentional. Release and climb upward, jump off, then use the remaining air jump.
4. After clearing processing, approach the office checkpoint flag separately from the evidence case. Collect evidence with E/INTERACT, then use the east ground-floor EXIT and confirm results/retry behavior.
5. Verify armor damage/broken presentation and profile a sustained firefight on the target Android device.

Final editor: Warehouse_Demo saved/open, Play Mode stopped, no missing scripts or Console errors. One Unity AI Toolkit Account API timeout warning remains; existing obsolete-API warnings also appeared during compilation. These are not failed gameplay tests. Existing unrelated dirty work preserved. Nothing staged, committed or pushed.
