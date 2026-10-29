# Sprint0039 grenade/hazard integration notes

## Audit and changes

Existing PlayerThrowables owns three per-type runtime counts, five maximum per
type (armor-adjusted carry limit3–5). GameSession purchases use persisted stock;
CampaignMissionBridge preserves uncarried excess and reserves carried consumables
on disk during active missions. That reservation intentionally prevents restoring
carried stock by killing/relaunching the app; normal mission capture/settlement
writes the actual remainder. SupplyCrate2D already uses finite stock and partial
acceptance without decrementing full player inventories.

Hardened existing boundaries rather than creating another inventory: invalid
kind reads return0; short supply arrays replenish only supplied types; capacity
changes cancel active aim; release revalidates quantity/cooldown/clearance before
consuming exactly one item after a cached projectile launches. Existing selection,
trajectory preview and release/cancel controls remain.

Malformed persisted grenade arrays previously reset all types, losing valid
entries. Root integration owns the ProgressStore change to preserve first three
valid entries and normalize missing/invalid quantities.

Fragmentation uses existing AreaCombat2D, solid cover checks, actor deduplication,
friendly/self damage and pooled presentation. Native target values: damage145
(previous110.5), radius4.5 (previous4.1), linear radial falloff. Existing visual
diameters, particle counts, timings, camera shake, sound, flashbang and smoke
values stay unchanged. Old hotfix tests retain other-type checks, with frag
expectations updated for this deliberate tuning.

## Future-level mine

Landmine2D uses one static pressure trigger, a non-obstructing shootable plate,
neutral Health1 and one cached existing ThrowableProjectile2D effect. No mine
Rigidbody, new damage architecture, scans, global manager or physical bullets.
Arming delay0.4s; pressure/ballistic/explosion activation gives0.2s red warning
before the same grenade burst. Spent state and contact disabling precede damage;
other mines prime with their own delay, preventing recursive explosion chains.
Shooting it from beyond the existing4.5 blast radius provides counterplay.

Editor helper Grenades0039Setup.Apply() updates the projectile prefab and creates
Assets/Prefabs/Hazards/Landmine.prefab plus native16×8 Point/32PPU four-color art.
It touches ZERO scenes, so no mines are placed in Level1 or any campaign level.
Art follows the inspected existing industrial/grenade palette and crude forms;
human artistic/device acceptance remains pending.

## Checks required by integrator

No Unity compile/test/mutation executed by the grenade subagent, to avoid clashes
with central integration. Root should compile, run the helper, then run:

- Grenades0039Tests (4 new PlayMode tests): inventory/cancel/switch/count bounds;
  stronger frag/falloff/cover/dedup/other-type settings; pressure/chain/one-shot
  mine behavior; hitscan mine clearing and no mines in Level1.
- Sprint0027ThrowableTests (existing4): throw controls, radial friendly cover,
  smoke/flash, purchases/save.
- GrenadeResupplyHotfixTests (existing4): finite crates/HUD/save reservation,
  pooled frag effects, existing region death, camera shake/corpse behavior.
- CombatReliabilityPlayTests relevant cover throws and low ceilings.

Scoped source whitespace check passed. No Android-device claims. Native helper
execution, compilation and test results must be reported from actual root runs.
