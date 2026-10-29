# Sprint 0039 — implementation and verification

## A. Implemented

**Movement.** Retained the existing motor/fall system. Canopy now retracts on
ground/ladder contact; a same-physics-step latch guarantees exactly zero damage
on a deployed landing regardless of callback order. Controlled descent clears
old fall energy, so cancelling near the floor does not resurrect an earlier
high-speed impact. New unprotected descent is still dangerous. Fall damage uses
Environment damage rather than ballistic armor absorption. Existing sliding,
jump/double-jump and ladder contracts remain. Native connected-canopy art uses
the same renderer/skin with shoulder-aligned cords; originals preserved.

**Sniper.** Separated automatic post-shot recovery from actual ammunition loading.
Manual reload cannot overlap recovery and clears the old aiming gesture; weapon
switching also cancels it. Hold/laser/release-to-fire and cancel remain. Sniper's
existing five-round internal magazine now uses the shared individual-round loader
and a cartridge approaching the receiver, rather than a generic magazine-handling
gesture. Full empty reload remains2.6s; partial loading takes only the needed
steps. SKS reuses that loader. Desert Eagle retains ordinary controls.

**Arsenal.** Added three data-driven hitscan guns, store/ammo offers and Point/32PPU
native artwork: SKS (38damage, semi,10rounds), AA-12 (automatic,8rounds,7pellets),
sawed-off double barrel (secondary,2rounds,8pellets, severe range falloff).
Glock/Desert Eagle names are retained where supported. Generic older SMG/carbine/
shotgun/LMG/sniper artwork receives descriptive names rather than guessed exact
model numbers. Existing stable IDs, old prices, weapon damage/cadence/capacity/
nominal full reload durations and final economy remain unchanged. New gun prices
are provisional playtest content, not a rebalance of the existing economy.

**Equipment/save/shop.** One primary plus two different secondaries, with empty
slots allowed. New enum value3 preserves old Melee2 and original firearm values.
Shared definition role validation applies to runtime/shop; ownership remains
separate. Additive secondary2 save field migrates old placements without deleting
ownership/ammo. Unity's empty inline RuntimeWeapon representation is treated as
an empty slot, fixing a mission-start null reference found by integration tests.
Checkpoint, mission packing, pause/resume reservation and result capture now cover
all three firearms. Stored excess no longer disappears at starter allowance or
armor carry limits; previously loaded rounds above a smaller magazine capacity
move into reserve. Null/empty saved equipment IDs normalize consistently.
The release-compatible10,000-credit initialization floor is preserved.

**Mobile HUD.** All three firearm slots plus grenade selector, correct names,
readouts/selected states and cached invalidation. Existing two joysticks and their
input thresholds remain. Panel widened12points; concise equipment labels and
48point button height keep scaled touch bounds above44. Existing transparency,
pressed/empty states, pause and readability preserved. PC keys1/2/3 select guns,
4 selects grenades. The isolated developer lab also uses roles instead of indices.

**Grenades/mines.** Hardened invalid-kind/partial-array/count/release/cancellation
boundaries, retained separate frag/flash/smoke stock and trajectory controls.
Malformed short saves preserve available entries instead of erasing all types.
Frag145damage/radius4.5, up from110.5/4.1; same cover/falloff/pools and presentation.
Flash/smoke behavior unchanged. Landmine prefab uses a static trigger/shootable
plate, readable warning and the existing cached grenade burst/damage system;
delayed independent activation prevents recursive chains. No campaign mines.

**Combat visuals.** Red flashes had been overridden by serialized peach enemy
colors; native data corrected and damage tint takes priority over reload/pose
writers, restoring original colors afterward. Player gray empty flash preserved.
Single-pass pale-core/colored-fringe tracer material uses the same hitscan,
LineRenderer pools, widths/colors and short fades. No physical bullets, dynamic
shot lights or duplicate renderers. Aim/Officer telegraphs retain original
material; Officer thick red and sniper/DE distinctions remain. Dust/sparks/chips
are larger and differentiated without extra particles. Factory generation had
classified every solid as concrete; existing wood/tin/rust crates and metal
walkways now use their actual materials (Demo:8wood/11metal/7concrete).

## B. Testing

102/102 unique selected tests pass; FinalTestSummary.json merges latest results
by full test name, so the focused final reruns replace earlier failures rather
than being double-counted. Coverage includes7new loadout/save rules,9new sniper/
grenade/mine/presentation Play checks,11fall checks, all11guns in the8-case arsenal
suite, reload/healing/cover reliability, grenade resupply, economy and full flow.
Final three-slot mission test also asserts all three native packs, heavy armor,
stored overflow, independent switching, HUD ammo, scaled44minimum touch bounds,
and pause/resume persistence. Physical exits repeat with and without purchases.

Compilation passed. Source/USS/UXML were checked through Unity; controlled source
art exports, SniperReload.png and ThreeSlotHud.png inspected. These captures are
not artistic approval or sustained performance benchmarks. Dense firing preserves
the existing renderer pools; no Android FPS claim is made.

Initial integration failures were investigated and repaired: empty inline weapon,
three-slot packing/checkpoints, stored ammo truncation, null/empty slot roundtrip,
and undersized four-button row. Fixture assumptions updated deliberately for
strict roles,11guns, sufficient test-only purchasing budget and individual-round
versus atomic reload. Existing economy prices and runtime constraints were not
weakened to satisfy tests.

Native Unity6.6 ADTM audio fatal/deadlock recurred during test transitions and
script reload. Editor-only -noaudio was attempted and did not eliminate it; the
Editor was restarted normally. Completed test results are recorded; stalled runs
are not counted. This pre-existing engine/editor issue remains unresolved.
Audible output, phone input feel, sustained device performance and play balance
are PENDING USER VERIFICATION. Older Sprint0038 legacy AI/instant-recovery test
issues were not redesigned as part of this sprint.

## C. Build

Release build_f11552b8f7ad **Succeeded**,331.231s,0errors/977warnings.
APK: `build/Sprint0039/AKUMU2-sprint0039-release.apk`,39,843,255bytes,
ARM64, nondebuggable, package `com.GAMES.AKUMU2`, version0.13/code2.
SHA256: `945E4E44F16B2C8211EADD59A2A5CA20992C4BA9114D6E77B509CC933F64BEBE`.
BuildStatus.json includes the detailed report (its total output size includes
other build outputs; the APK itself is the39.8MB file above). PackedAssetProof.json
confirms all3new weapons, tracer material/shader and connected canopy were packed.
ApkVerification.json reads metadata from the actual ZIP and confirms new slot,
role, reload-insertion and parachute code plus the existing release wallet-floor
tag. ApkBadging.txt confirms package/version/architecture and no debuggable flag.
Warnings are recorded in BuildWarnings.json, largely compiler deprecations and
the intentionally absent Pipeline runtime config; no warning references the new
0039 shader or asset paths. No phone installation or manual gameplay is claimed.

## D. Device checklist

See AndroidPlaytest.md — **PENDING USER VERIFICATION**. Includes legacy save/credit
floor, three weapons and empty slots, each new gun, sniper hold/cancel/release/
cycle/reload, canopy landings/cancellation/ladders, grenade stock/replenishment,
damage flashes/materials/tracers, touch targets, sound and frame pacing.

## E. Remaining work and limits

No requested implementation is intentionally left as a stub. Real-device gameplay,
artistic/balance acceptance and the native Editor audio problem remain open.
Mines are a prepared/tested future-level prefab, not Level1 content. No claim that
unrun project tests pass; coverage is the selected102tests above.

## F. Architecture

Extended existing WeaponDefinition/WeaponCycle/PlayerLoadout/GameSession and
CampaignMissionBridge; fixed shared boundaries rather than parallel inventories.
Mine delegates to the existing grenade explosion. Tracers retain the same pool.
No new dependencies, global per-frame scans, physical bullets or AI personality
changes. Native scenes/prefabs/assets were authored through Unity Editor APIs;
no YAML/GUID hand edits. Regenerated older test artifacts were preserved under
RegressionArtifacts and original records restored without changing the Git index.

## G. Design bibles and files

LEVEL_DESIGN_BIBLE.md establishes variety, fairness, AI-supporting geometry,
horizontal accessible Level1, future-only mines and mandatory Android review.
AGENTS.md explicitly requires both bibles and root-cause/regression/reporting
discipline. GRAPHICS_BIBLE.md retains the same artist authority and adds provenance
for new native art/effects. No level-layout/encounter-roster redesign. Final creative
authority remains with the owner; new art remains subject to their review.

See ModifiedFiles.txt for absolute file paths and specialist notes in this folder
for implementation detail. No staging, commit, push or publication.
