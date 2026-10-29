# Sprint 0028 — The First Operation

Implementation and automated/controlled verification are distinct from human playtesting.

## Authored encounter plan

West-to-east route, x=-28..159, continuous safe floor at y=0; bounded sides/ceiling.
All thirteen enemies are authored occupants, not camera-triggered rear spawns.
Main menu continues to launch Warehouse_Demo through the existing catalog/shop.
Warehouse_Testbed and Warehouse_Mission01 remain legacy test/checkpoint references.

| Area | Bounds / platforms (top heights) | Assigned starting positions (x, floor y) | Routes, cover and risks |
|---|---|---|---|
| Warm-up | -28..4; ground | Larpers (-18.5,0),(-8,0),(0,0) | Enter -24; 8–10.5-unit spacing, individual cover at -21,-12,-4. No crossfire at entry. Coward/ordinary/confident variation. Avoid single shared box. |
| Storage / verticality | 4..40; low deck x14..28 y3.2; high x26..37 y6.4 | Larpers (18,3.2),(31,0),(35,6.4) | Ground primary route, ladders x13.3/25.3 optional elevated advantage. Solid decks shield below; fire around edges, not through floors. Three separate cover objects. |
| Samurai interruption | 40..74; step x54..57 y1.6; deck x60..72 y3.2 | Samurai (58,0), Larpers (64,0),(68,3.2) | Wide approach and retreat room; step/ladder x59.3 support existing small-body pursuit. Only sparse cover; avoid a low ceiling over launch. LOS around deck edge matters. |
| Fireworks disaster | 74..110; lookout x84..91 y6.4; loading deck x94..105 y3.2 | Expert (94.8,3.2), Larpers (86,0),(106,0) | Ground primary route; ladders x91.7 and93.3 optional. Long open dodge lanes, cover x79/88.5/108/102. Friendly fire retained; solid decks occlude blasts/vision. |
| Officer / command | 110..159; flank x123.5..132.5 y3.2; office dais x135.5..153 y1.6 | Officer (140,1.6) | Long approach; cover114/125; raised defensive barricade138 and retreat cover146. Flank ladder122.8, reachable ground route, exit155. Only Officer defeat gates exit. Locked-exit feedback persists while inside, with debounced notification events. |

Supply crates at x2/38/73/112/132 provide existing finite ammo/tape interactions
(108 reserve rounds and60 tape each, subject to existing capacity). No injection
pickups or checkpoints. Purchased injection inventory remains capped at2; revival
still provides2s protection. Death without stock ends the attempt; retry starts at
entrance without replenishing purchased inventory. No defeat-all-enemies gate.

## Mission-scoped tuning

Existing weapons and global specialist prefabs remain unchanged.

| Parameter | Existing | Operation variant |
|---|---:|---:|
| Basic Larper reaction | global prefab preserved | .7s + random0.. .12s (scene overrides only) |
| Firework preparation | .85s | 1.05s |
| Firework recovery |1.05s |2.1s (existing variation retained) |
| Preferred throw range |7 |8 |
| Blast damage |31.2 |26 |
| Blast radius |3.2 |2.8 |
| Normal fuse |.8–1.5s |1.1–1.8s |
| Short fuse |.25–.45s |.65–.9s |
| Short-fuse probability |18% |12% |
| Long fuse |2–3.2s |2.2–3.2s |

Bad misses, held malfunctions, warning ring, friendly fire, geometry occlusion and
small existing projectile/VFX pool are reused. Threat and fairness need hands-on review.

Officer did not previously exist. User explicitly requested a new design matching
the graphics bible. Independent flat-pixel cap/uniform modules use the Larper's
12x11/12x17/5x18/6x18 bounds,32PPU, Point, unchanged pivots and segmented rig.
Historical Postava1 inspected as reference only; historical files untouched.
Officer uses baseline Larper health (no inflation), existing imperfect aim,
suppression, retreat, reload and cover reservation. New Officer-only pistol has
18damage,1.4s firing interval,6round magazine,4s reload; each eligible shot adds
.7s visible weapon-color wind-up, canceled when shooting gates no longer hold.
Existing combat retains viewport/LOS checks; no offscreen attack exemption. An Officer-only close-approach gate (<4 units,4–6s cooldown) requests the existing withdrawal action.
New appearance awaits artistic review. The opening guard moved to x-18.5 and the Officer to a1.6-high dais after gameplay-camera captures exposed touch-overlay overlap. Only authored camera bounds changed (lower bound -4); camera controller/movement code unchanged.

## Validation

See accompanying JSON and capture files for actual executed tests/results.
Controlled actor repositioning or programmatic defeat verifies integration, not
human ability to win. No claim of10–15minute pacing, Android frame rate, device
controls or complete interactive gameplay polish can be made without playtesting.

Manual priorities: play from the actual menu, buy supplies, judge fight duration
and openings; test edge sightlines, all ladders/double jumps, cover/reload/switching,
Samurai routes from both sides/elevations, firework dodge windows/friendly fire,
Officer wind-up/retreat/approach and exit; exhaust revivals and retry; repeat on
Android in both facing directions, check HUD occlusion, thermals/frame time and
whether10–15minutes is realistic. Do not add HP or empty corridors just to pad time.


## Files changed

- `Assets/Scenes/Warehouse_Demo.unity`: handcrafted layout, five encounters,
  finite supplies, no checkpoints, final Officer/exit; saved through Editor APIs.
- `Assets/Data/Progression/DemoCatalog.asset`: Level1 title/description; same route.
- `Assets/Data/FirstOperation/`: new Officer skin/modules/segments/prefab/archetype,
  Officer pistol and mission-specific fireworks prefab/archetype variants.
- `Assets/Editor/FirstOperationSetup.cs`: explicit reproducible authoring source. Existing `LevelArt0026Setup.cs` snapshot reader normalizes only the new legacy-default flags (checkpoint fallback=true, Operation mode=false); opposite values remain detectable.
- `Assets/Scripts/Missions/WarehouseMission.cs`, `MissionTrigger.cs`, `MissionHud.cs`:
  opt-in five-section, Officer-only completion and failed-attempt results mode.
- `Assets/Scripts/Progression/CampaignMissionBridge.cs`: settle failed attempts
  through existing accounting; `GameSession.cs`: new saves start with zero
  injections; existing stored inventory/migration defaults remain unchanged.
- `Assets/Scripts/Combat/PlayerHealthRecovery.cs`: scene-configurable checkpoint
  fallback (default remains enabled for legacy scenes).
- `Assets/Scripts/Inventory/PlayerSupplies.cs`: optional revival inventory reset,
  preserving legacy default and avoiding Operation retry refills.
- `Assets/Scripts/Enemies/OfficerShotTelegraph.cs`, `EnemyCombat.cs`: optional
  Officer-only wind-up and close-approach withdrawal; existing Larper path intact.
- `Assets/Tests/EditMode/FirstOperationLayoutTests.cs`,
  `Assets/Tests/PlayMode/FirstOperationTests.cs`, existing `Sprint024FlowTests.cs`:
  new integration cases and updated actual-menu/physical-exit progression branches using actual exit bounds instead of old coordinates.
- Legacy art tests retain their archived-scene collision/decoration checks; Demo assertions now recognize its authorized layout. Legacy checkpoint tests wait for the already-established1.2s corpse recovery before asserting rollback; the rollback-only case disables autonomous AI so perception cannot race its inactive-group assertion.
- `GRAPHICS_BIBLE.md`: Officer reference now provisional, not previously absent.
- `DEVELOPMENT_LOG.md` and this report/evidence.

No audio system or special music trigger was added. No new dynamic-lighting,
AI manager, animation framework or navigation subsystem. MainMenu/HUD styles,
shop offers and all original art/character prefabs remain unchanged.






## Final results and remaining verification

**55/55 unique focused tests passed** (see `Regression.json`):6 new mission Play,
12 campaign/menu/shop/physical-exit/accounting,9 movement/suppression,12 cover,
9 legacy mission,6 Edit layout/art/decoration,1 archived-art Play case. Reruns and
the separately run single menu test are not counted twice. The expert actually
launched toward the ground approach; the Officer actually fired after wind-up;
the Samurai actually landed on the intended deck with its original small collider.
The controlled player traversed the complete ground route using movement/jump
inputs, and actual carbine hitscan stopped at the solid deck underside.

Compilation successful, zero current Console errors. Eleven warnings remain from
existing Editor/test deprecated/analyzer API use and Unity AI Toolkit account
access; no unrelated warning cleanup or Console clearing performed. Native
verification confirms Demo saved/open/clean/stopped,13 valid authored points,
zero missing scripts, zero checkpoints, MainMenu first build scene and catalog
route to Demo. All591 protected original files match. The four pre-existing
Resources performance-test-file deletions remain; nothing staged/committed/pushed.

Five final controlled gameplay-camera images: `View_-24.png`, `View_10.png`,
`View_49.png`, `View_90.png`, `View_134.png`. An earlier `View_80.png` is retained
as iteration evidence. New Officer references and archived art checks preserve
all original crate/character artwork. Initial test failures and adjustments are
recorded in DEVELOPMENT_LOG; early PlayTests_FirstRun/SecondRun evidence remains.

**Implemented by Codex — awaiting hands-on playtest verification.**10–15minute
pacing has not been established. Full live firefights, all possible AI knockbacks/
retreat situations, every alternate approach and Android-device performance/input
remain manual. The automated tests and controlled captures do not establish fun,
artistic acceptance or a polished mission. Launch through PLAY -> level selection
-> THE FIRST OPERATION -> shop -> START MISSION and follow the five areas; judge
Officer telegraph/dodging and firework warning/fuse windows before final balance.
