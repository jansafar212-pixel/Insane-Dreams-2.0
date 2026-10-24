# Sprint 024.6 — completion report

**Implemented by Codex — awaiting playtest verification.**

## Startup: confirmed cause and correction

The global build scene list already had enabled MainMenu first, followed by
Warehouse_Demo and Warehouse_Mission01. The saved Android profile inherits that
list and also contains MainMenu first; there is no separate saved Windows profile,
so the standard Windows configuration uses the global list. No runtime bootstrap
forcing Warehouse_Testbed was found. Those working scene lists were preserved.

The confirmed mismatch was Editor startup: Warehouse_Demo was left open and
`playModeStartScene` was null. Ordinary Play therefore started the open warehouse.
`CampaignEntry` now sets MainMenu as the default Editor Play entry once per Editor
launch. `AKUMU 2/Play Campaign (Main Menu)` explicitly starts the campaign, while
`AKUMU 2/Play Open Development Scene` deliberately tests the currently opened scene
and restores the menu entry afterward. Testbed remains available for that workflow.

A final integration check exposed the same override redirecting Unity's test
launcher into MainMenu. The Editor callback now temporarily clears it for active
test jobs and restores it afterward. The installed test framework exposes its
active-job flag internally; the small Editor-only reflection lookup should be
rechecked when upgrading that package. It adds no runtime/build dependency.

Actual configured Editor Play was exercised with Warehouse_Demo open: MainMenu
loaded, GameSession.Screen was MainMenu, no warehouse enemies were present, and
frame count advanced from 1527 to 16647. Explicit development Play loaded the
warehouse with no campaign session, then restored the MainMenu entry. This is
Editor runtime verification, **not a Windows executable or Android APK launch**.

The real menu/shop/mission/results/return test passed, including actual UI presses,
armor purchase/equipping, evidence/EXIT progression, reward settlement and wallet
persistence. Its controlled balance goes $100 → $70 after armor → $107 after a
$37 reward; reward cannot be banked twice. Regression fixtures use temporary saves.
Normal menu startup also initialized existing progress through the unchanged
GameSession path; no purchase/mission actions were performed on the real session.

## Ragdoll collision: root cause and exact change

The six original corpse colliders already use layer 1 with `excludeLayers = -2`
(`0xFFFFFFFE`), allowing physical contact with Default-layer terrain while excluding
layer-2 characters. Sprint 024.4/024.5's eight additional articulated limb colliders
were created with default exclusions (zero), so those limbs could push/block the
player even though the torso and original limbs did not.

`SamuraiArticulatedRig.BuildRagdoll` now copies `excludeLayers`, `includeLayers` and
`layerOverridePriority` from the corresponding original limb collider when creating
each extra segment. All 14 bodies receive consistent filters. No collider is
disabled or changed into a trigger; no global collision matrix, living-character
layer, gameplay collider or pickup/checkpoint filter was changed.

Hitscan still queries corpse layer 1 through AttackExecutor's existing
`physicalImpactLayers = 2` mask. Physics contact exclusions do not remove colliders
from those ray queries. Actual Player- and Enemy-team shots hit the corpse, report
`ShotSurface.Ragdoll` and increment its impact receiver. Environment settling,
walking/jumping through three overlapping corpses, and climbing/jumping off a
ladder beside a pile on a narrow landing passed automated checks.

## Pistol graphics

Extended the project's existing native/code-authored pixel-sprite approach. The
body is still 20×8 at 32 PPU, now three flat dark colors with a clear barrel/grip.
The formerly oversized 8×18 magazine is now a narrow 5×13 extended magazine, still
clearly protruding beneath the grip. No gradients, detailed textures or attachments.

The old magazine socket was four pixels forward of the hand. A per-weapon visual
socket now puts this pistol's magazine at (1/32,−2/32), beneath the grip. Other
weapons retain the original (4/32,−2/32) default; existing reload strokes are added
to the socket. Mirrored aim, movement, jump, crouch, climb and reload alignment pass.

**Gameplay values before → after:** damage 5→5; interval .105→.105 s; capacity 27→27;
reload 4→4 s; range 28→28; impact impulse .9→.9; enemy starting reserve 135→135.
Accuracy, recoil, AI behavior and hitscan code were not changed.

Historical Enemak files remain available locally under
`HistoricalArtSources/Drive/Postavy/Enemak/`, previously inspected and documented in
ART_BIBLE.md. They were reused as style references, not modified or claimed as newly
recovered. The pending Larper/Samurai master-art approval status is unchanged.

Screenshot: `TestResults/Sprint0246_Pistol_Gameplay.png`, controlled Play Mode camera
render at the actual warehouse zoom, 1280×720, orthographic size 5.113636. It shows
the three opening Larpers with the new pistol/magazine, including a firing pose.
The capture used direct development Play without a campaign session; capture-only
placement/protection was discarded on stopping. It is not a human gameplay review.

## Files changed

- `Assets/Editor/CampaignEntry.cs`: default campaign entry, explicit development
  Play and automated-test entry arbitration.
- `Assets/Scripts/Visuals/SamuraiArticulatedRig.cs`: inherit extra collider filters.
- `Assets/Scripts/Inventory/WeaponDefinition.cs`: visual magazine socket field.
- `Assets/Scripts/Visuals/CutoutRig.cs`, `CharacterVisualDriver.cs`: consume that
  socket while retaining existing animation logic/default behavior.
- `Assets/Editor/Sprint0246Setup.cs`, `Assets/Art/Sprint0246/`: native pistol and
  magazine authoring/assets; `Assets/Data/Items/LarperBuzzbox.asset` and
  `Assets/Prefabs/Enemies/Larper.prefab`: visual references/socket only.
- New tests: `Assets/Tests/EditMode/Sprint0246RulesTests.cs`,
  `Assets/Tests/PlayMode/Sprint0246Tests.cs`.
- Existing test expectations updated: `Sprint008PresentationTests.cs`,
  `Sprint012RulesTests.cs` (new native sprites and magazine proportions);
  `Sprint024FlowTests.cs` (load the configured first enabled scene).
- `ART_BIBLE.md`, `DEVELOPMENT_LOG.md`, this report, `Sprint0246.json` and screenshot.

## Test results and remaining verification

**65/65 full Edit Mode tests and 102/102 unique focused Play Mode tests passed.**
Complete per-test results are in `TestResults/Sprint0246.json`. Existing coverage was
retained. Coverage includes startup configuration, real UI campaign flow, both-team
corpse hitscan, impact handling, physical corpse piles, ladder access, limb/pistol
alignment, armor, evidence/checkpoints, Samurai mobility, combat, cover, double
jump, suppression, healing/supplies, spawning and fireworks physics.

The initial menu-override/test-runner confirmation timed out in MainMenu before
tests could initialize. That run was cancelled; after the Editor-only exception,
the same real UI flow passed with MainMenu entry enabled, followed by final Edit
Mode/new Play Mode checks. A temporary test-assembly reference error was corrected;
there are no unresolved compilation errors. The full Play Mode assembly was not run.

Still manual: rebuild and launch the Windows executable and Android APK; verify
menu-first startup and the complete flow on device; walk/jump/climb through corpse
piles during sustained combat; judge pistol readability/reload feel on a phone.
No binary/device launch, human gameplay review or Android performance benchmark
is claimed. Old already-exported binaries require rebuilding/reinstalling.

Final editor: MainMenu open and saved, MainMenu configured as Play entry, Play Mode
stopped. No staging, commit or push. Existing unrelated work preserved.
