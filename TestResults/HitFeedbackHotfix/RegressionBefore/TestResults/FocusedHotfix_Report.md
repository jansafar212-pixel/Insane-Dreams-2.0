# Focused gameplay and visual hotfix — 2026-10-22

Implemented by Codex; controlled Unity Editor tests passed as recorded below.
Human gameplay feel, artistic acceptance and Android-device validation remain pending.

## Tuning and behavior

| Setting | Before | After |
|---|---:|---:|
| Basic pistol Larper first-sight/reacquisition reaction | .30 s + 0–.05 jitter | .55 s + 0–.05 jitter |
| Wallet center-distance limit and trigger radius | 1.20 | 1.62 (+35%) |
| Samurai ground jump velocity | 12 | 13.28 |
| Samurai second-jump velocity (existing .85 multiplier) | 10.20 | 11.288 |
| Samurai ideal single-jump height at gravity 29.43 | ~2.446 | ~2.996 (+22.47%) |
| Local route minimum platform width | 1.10 | .90 |
| Candidate landing/launch body-side margin | .15/.20 | .10/.10 |

Actual sampled physics apex ratio is checked to remain between +20% and +25%.
Player and other enemy jump settings are unchanged. Samurai reaction remains .18 s;
fireworks reaction remains .30 s with an explicit variant override so the basic
Larper's slower reaction does not leak into that specialist. Established pistol
cadence, ranges, accuracy, ammo, suppression and morale parameters are unchanged.

Wallet trigger and code distance agree. Collection still uses proximity and solid
line-of-sight, records reward IDs once, and displays exactly `Wallet collected`;
amount disclosure and results accounting are unchanged.

## Samurai stuck causes and correction

The actual Warehouse_Demo low cover at x7.8 reproduced an indefinite stop at x6.61
before changes. The route cache excluded one-unit crates, and landing margins made
one-unit cover tops too narrow for the .754-wide locomotion box. Accepting those
footholds retains .10 units of clearance on each side and existing swept arc tests.

Crossing cover exposed a second real failure: on the raised alternate gap step,
the descent waypoint was INSIDE the support edge. Generic movement correctly
refused to walk off the ledge, so the Samurai stopped there indefinitely. The
planner now validates a lower landing, occupancy and horizontal clearance, then
commits a physical walk-off using existing flight/landing bookkeeping. Its finite
timeout derives from approach distance and gravity. No jump impulse, teleport,
gravity override, or new navigation manager is used for that descent.

Existing sampled planning, launch progress watchdog, failed-edge cooldowns and
failed-flight suppression remain. New clearance sweeps occur only during the
existing tactical sample; no new per-frame scans, allocations or physics loops.
The local graph still has fixed buffers and a one-second refresh cadence.

## Carbine and armor

New independently authored native carbine: stepped wooden stock, narrow neck,
dark receiver, wooden handguard, thin barrel and bent magazine. Four established
flat colors, 30x11, 32 PPU, point filtering, no mipmaps, same grip pivot (7,5).
The historical Postava original and actual Larper/pistol presentation were viewed;
historical files were untouched. Weapon damage20, interval.2, magazine24,
reload1.5, range22, sounds and existing flash remain unchanged.

Controlled both-facing idle/walk/crouch/reload review: [carbine](FocusedHotfix_Carbine.png).
This pose sheet is not human artistic approval or a live gameplay capture.

Armor's old presentation deliberately kept `Equipped=true` after depletion and
selected a broken-vest sprite. It also lacked a presentation notification when
BodyArmor was disabled. The requested behavior supersedes that old design:
the overlay now requires positive condition and active armor/presentation;
depletion/removal/disable clears its sprite and disables its renderer. Equipment
enable/disable notifies the existing observer. Depleted armor is not copied to
ragdolls. Positive armor still uses intact/damaged states. No body sprite, pose,
armor absorption/head protection, save format or health calculation changed.

## Modified files

- Runtime: `Assets/Scripts/Enemies/SamuraiCombat.cs`, `SamuraiPlatformRoutes.cs`;
  `Assets/Scripts/Inventory/WalletPickup.cs`; `Assets/Scripts/Combat/BodyArmor.cs`;
  `Assets/Scripts/Visuals/ArmorVisual.cs`.
- Prefabs: `Assets/Prefabs/Enemies/{Larper,Samurai,FireworksExpert}.prefab`,
  `Assets/Prefabs/Pickups/WalletPickup.prefab`,
  `Assets/Prefabs/Player/WarehousePlayer.prefab`.
- Art/data: `Assets/Art/FocusedHotfix/` (Sprite/Texture2D plus Unity metadata),
  `Assets/Data/Items/WarehouseCarbine.asset`;
  native authoring recipe `Assets/Editor/FocusedHotfixSetup.cs` and metadata.
- Tests: new `Assets/Tests/PlayMode/FocusedHotfixTests.cs` and metadata;
  existing `Sprint023Tests.cs`, `Sprint012CombatTests.cs`, `Sprint0245Tests.cs`,
  `Sprint0243Tests.cs`, `Sprint0244Tests.cs`;
  `Assets/Tests/EditMode/Sprint008PresentationTests.cs`.
- Documentation/evidence: `ART_BIBLE.md`, `DEVELOPMENT_LOG.md`,
  `TestResults/FocusedHotfix*`.

The available chat listing showed no other active local project sprint. Existing
dirty/untracked work was preserved. Camera source hashes match the pre-hotfix
snapshot; no camera, scene placement, spawning, player movement or input source
was edited. Prefab, Sprite and Texture2D authoring used Unity APIs, never YAML edits.

## Verification

Compilation succeeded. Full Edit Mode: 78/78 passed. Unique targeted Play Mode:
76/76 passed (including five fireworks tests after preserving its reaction
override). Results, including original attempts, are retained in `FocusedHotfix.json`.
Reference audit: zero missing scripts and broken serialized references across
Warehouse_Demo, Warehouse_Testbed and Warehouse_Mission01. Final Console ground
truth: zero errors/warnings, no compilation failure. MainMenu is saved, open,
not dirty; Play Mode stopped and background-running restored off.

The focused tests exercise actual Warehouse_Demo geometry and existing crate,
low cover, raised alternate step and upper platforms with normal Rigidbody motion.
Other checks cover first-shot delay versus established cadence, real automatic
wallet pickup at1.5/rejection at1.9 and deduplication, armor equip/damage/deplete/
disable/re-equip/removal and absent corpse vest, player motion/double jump,
Samurai melee/air-jump/ladder/blocked arcs/recovery/ragdolls, exact animated
Samurai hurtbox, carbine flash and combat integration.

Two old traversal assertions were adapted to the requested behavior: a physical
walk-off need not consume a jump, and the stronger jump may skip an ordinary
warehouse step. Separate single-jump and double-jump tests remain. The art path
allowlist was extended to the new folder, retaining point/PPU/mipmap checks.
Failed baseline/intermediate runs are preserved rather than presented as passes.

## Manual follow-up

1. Start MainMenu -> Warehouse_Demo. Check that ordinary Larpers' first shots feel
   readable while their subsequent pistol bursts remain fast.
2. Walk past wallets at the wider distance; confirm the amount appears only in
   results and a wallet cannot pay twice.
3. Kite Samurai across the opening/storage crates, x7.8 low cover, alternate gap
   step and upper route. Check readability, pursuit and recovery during moving
   targets, crowding, hits and suppression. Automated scenarios do not cover every
   dynamic combination or guarantee navigation to inaccessible terrain.
4. Check the carbine at normal gameplay zoom in both facings, while aiming,
   firing, moving, crouching and reloading; confirm the crude silhouette fits.
5. Equip armor, damage it, exhaust it, remove/re-equip it and die with/without
   remaining armor. Confirm no lingering vest and unchanged body/pose rendering.
6. Repeat on Android. No APK/device performance profile or human playtest was run.
