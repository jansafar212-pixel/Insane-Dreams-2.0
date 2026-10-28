# Sprint 0034 — Economy, Shop & Progression

Implemented in the existing GameCatalog, GameSession, ProgressStore, campaign bridge
and UI Toolkit shop. Weapon tuning, camera, physics, AI, scenes, sprites, control
geometry, grenade effects and existing armor modifiers were preserved.

## Earnings and progression

Level 1 audit: 15 existing EnemyWalletDrop actors, each $5–20, drop chance 100%.
No separate kill payout or static wallet reward was added. Expected full collection
is $187.50 (actual drops/collection vary). Mission completion adds $250. New profiles
start at $100 instead of the old $1000 playtest grant; existing balances/ownership
remain untouched. Failure/abandon/interruption banks 25% of collected wallets,
rounded down, capped at $50, and never removes previously banked money.

Wallets retain the existing 1.95-unit pickup radius, obstruction checks and
per-event deduplication. Feedback now shows +$amount and current-run wallet total.
Results show wallet earnings, completion reward, payout and banked balance.
Run IDs settle once. Purchase, equip, deployment and settlement writes roll back
in-memory changes if the existing atomic local save fails. Existing version1 saves
remain readable; starter entitlement and armor records migrate without resets.

## Prices / limits (DemoCatalog.asset)

| Weapon | Ownership | Ammo pack | Pack price | Unarmored reserve cap |
|---|---:|---:|---:|---:|
| Backup Pistol | Free starter | 8 | $5 | 96 |
| Automatic Buzzbox | $200 | 27 | $12 | 162 |
| Rattlebox SMG | $350 | 30 | $15 | 180 |
| Warehouse Carbine | $500 | 24 | $18 | 144 |
| Doorbell Shotgun | $650 | 6 | $24 | 36 |
| Player Desert Eagle | $1200 | 7 | $42 | 42 |
| Lunchbox LMG | $1600 | 100 | $65 | 200 |
| Long Complaint Sniper | $2000 | 5 | $50 | 30 |

Weapon purchases include one loaded magazine. Only owned guns equip. No duplicate
ownership purchases. Ammo is per existing weapon definition; no new ammunition
framework. Ownership and room for the WHOLE pack are required; no partial-price
charges or premium mission-start refills. Existing crates still resupply the active
weapon (or first firearm when throwables selected), now using its reserve cap.

Basic pistol receives a floor of five magazines TOTAL (8 loaded +32 reserve) at
mission start/replay, never five extra magazines on top. If selected guns are both
empty, deployment falls back to the basic pistol. This prevents an economic softlock.
Existing higher stock is preserved. The LMG's handling remains100 loaded/8s reload;
its existing isolated lab still refills200 reserve without affecting campaign saves.

| Consumable / protection | Price | Quantity / limit |
|---|---:|---|
| Duct Tape | $25 | 50 healing units /200 maximum |
| Faintanyl | $175 | 1 /2 maximum |
| Fragmentation | $50 | 1 /existing per-type capacity |
| Flashbang | $35 | 1 /existing per-type capacity |
| Smoke | $30 | 1 /existing per-type capacity |
| Light body armor | $150 | Existing60 protection |
| Heavy body armor | $300 | Existing90 protection |
| Light helmet | $60 | Existing20 protection |
| Heavy helmet | $120 | Existing35 protection |

Faintanyl appears only in Medical, alongside tape. Exact description retained:
"relax its not addictive, just habit forming". Existing hold healing/fire exclusion,
automatic revival, two-injection cap and two-second protection are unchanged.

Protection ownership, equip/remove selection and condition now persist per item.
Repair purchase restores that item's condition; switching owned gear does not repair
it. Existing reload/absorption/grenade penalties and melee bypass are reused, with
NO movement penalty. Reserve cost applies proportionally to each gun's base cap,
with a minimum of one magazine or one-third of base (whichever is larger, bounded
by base). Heavy body+helmet: existing27.44% reload cost,12.5% reserve reduction,
four grenades per type. Existing armor overflow remains stored between missions.

## Conservative interrupted-mission inventory policy

Carried supplies are reserved on disk when deployed. Existing in-memory gameplay
and immediate HUD counts stay live. Event-driven saves preserve banked cash,
wallet claims, ownership and uncarried overflow, without per-shot or per-frame disk
writes. Normal results, pause-menu return and normal application quit return unused
inventory and settle the run. Pausing or Android backgrounding writes an exact safe snapshot of unused stock.
Resume re-reserves that stock before controls/simulation continue; a failed
reservation keeps the player paused. If the OS kills a paused/backgrounded process,
that safe unused stock survives. An unannounced force kill during active combat
forfeits carried ammo, tape, grenades, injections and worn armor condition rather
than restoring already-spent supplies.
The shop explicitly explains this policy. This is NOT a checkpoint/resume feature.

## Balance simulations

Generated from actual15 Level1 drops and current catalog in BalanceSimulation.txt:
- Poor player: $0, failed after collecting$20 -> banks$5; still has free40 basic
  rounds on next deployment. No premium ammo/injection grants.
- Average first completion: $100 + rounded-mean $188 + $250 = $538. SMG$350 +
  ammo$15 + tape$25 leaves$148. Armor/premium purchases involve alternatives.
- Saver: three average completions -> $1414 before expenses. Desert Eagle$1200
  leaves$214. Sniper$2000 needs about five average completions before expenses.
These are transparent scenarios, not observed human consumption/playtest balance.

## Changed files

Runtime: Progression/GameCatalog.cs, GameSession.cs, ProgressStore.cs,
CampaignMissionBridge.cs, GameFlowController.cs; Inventory/PlayerLoadout.cs,
PlayerSupplies.cs; Missions/MissionHud.cs. Native config:
Assets/Data/Progression/DemoCatalog.asset. New native authoring helper:
Assets/Editor/EconomySetup.cs. No scene/prefab/weapon-art authorship changes.
New tests: Sprint0034EconomyTests.cs, Sprint0034EconomyRulesTests.cs.
Existing tests updated for intentional price/starter/payout/reservation changes:
Sprint024FlowTests.cs, Sprint023Tests.cs, Sprint0027MedicalTests.cs,
Sprint0029SystemsTests.cs, GrenadeResupplyHotfixTests.cs. Old medical assertions now honor existing visible empty
revive slots and tiny float tolerance around2seconds; no runtime revive change.
Documentation: DEVELOPMENT_LOG.md, SPRINT_RESUME.md and this evidence folder.

## Verification (final results appended below)

Automated checks are controlled Editor tests, not human or Android playtesting.
Initial failed assertion records preserved where available. No tests removed.
Shop captures in this folder reuse the existing shared handmade skin. Original
profile path is never used by test sessions; test saves live in temporary folders.

## Manual device checklist

1. Fresh profile and old profile: verify balances, prior ownership/gear, shop labels,
   free basic pistol, and affordable starter supplies. Don't delete a real save.
2. Purchase each firearm/ammo pack. Close/reopen, equip primary/secondary, enter
   Level1, fire/reload/switch; check current/capacity and separate reserve counts.
3. Try full reserve, insufficient cash, double taps and full consumable stocks.
4. Buy both armor tiers/helmets. Damage one, return, change/remove/re-equip; verify
   condition conservation, reload/carry trade-offs and unchanged movement.
5. Hold-heal while damaged/in cover; verify fire exclusion. Buy two Faintanyl,
   revive twice, confirm two-second protection and no free third charge.
6. Collect physical wallets; finish/fail/return/replay, check result breakdown and
   banked cash once. Force-kill only a disposable test mission to verify reservation.
7. On A56, verify landscape safe areas, category scrolling and touch targets, then
   sustained firefights with frame display. Check60FPS/frame-time tails/thermals.
   Compare with previous optimized build under similar conditions.

## Compatibility findings during regression checks

The pre-sprint PlayerCombat CanUse condition included !IsHealing and was shared by
Reload, inadvertently blocking reload priority. A minimal split now allows reload
when the player is otherwise able to operate, while retaining healing's firing
block and all sniper cancellation/health/pause/syringe safety. No weapon tuning
changed. PlayerCombat.cs is therefore one additional scoped runtime file.

Older regression expectations were updated for the intentional new starter,
prices/rewards, per-weapon ammo caps, conservative deployed-stock snapshot and exact
Faintanyl description. Healing test setup now confirms standing clearance/airborne
state before checking air-healing exclusion. No tests removed or weakened to permit
equipment/currency duplication. The one medical timing tolerance accepts normal
float subtraction noise (2.00000024s), not a gameplay-duration change.

Physical pickup/wallet tests9/9, grenade resupply/frag/ragdoll tests4/4, new economy
Play tests10/10, complete campaign flow12/12, sharedUI3/3, medical1/1, arsenal8/8,
lab1/1, performance2/2, camera9/9, equipment integration4/4; final focused reruns and
build outcome are recorded below. Rules3/3, persistence/build-profile4/4 and combat
rules7/7 passed. Selected checks, not the entire historical project test suite.

## Pre-export result

Before the pause refinement,77/77 selected checks passed (63 Play Mode,14 Edit Mode). The pre-export Console
zero errors/warnings and compilation clean. Warehouse_Demo saved/open/stopped; no
missing inspected scene/prefab scripts. 156 protected originals checked,146 unchanged;
ten scoped runtime/catalog changes, including the reload compatibility restoration.
Build dry run valid. Development export build_2b1271aa2053 started to
build/Economy/AKUMU2-sprint0034-dev.apk. No source edits during export.

## Android pause refinement

Background or pause safely saves unused supplies; resume atomically re-reserves
before controls continue, preserving pause on write failure. GameSession defers its
own background save to the bridge to avoid callback-order clobbering. A process
killed while paused retains unused stock. Only an unannounced active-combat kill
forfeits carried stock. Added a direct pause/background/resume/save-failure test.
Flow12/equipment4/lab1 reruns passed. A transient test-start socket error is archived;
Editor remained responsive and the full economy suite is rerun for a complete result.

## Ammunition budget audit

AmmoBudgetAudit.json reads actual Level1 actors and five existing supply boxes:
13 targets50HP, fireworks60HP and Samurai70HP. Boxes contain108 rounds each,
540 finite rounds overall. Ideal single-projectile lower bounds ignoring armor,
range, headshots, misses and damage modifiers: pistol62 hits, SMG93, carbine46,
Eagle17 and sniper15. Shotgun's audit row counts individual pellet-sized hits,
not shells. These are analytical bounds, not measured player consumption.
The basic40-round allowance is a safety floor, not free ammunition for a full
mission. Existing early box at x2 and later boxes at38/73/112/132 preserve recovery
opportunities. The one-pack average purchase scenario assumes use of these finite
supplies. At50% pistol hit rate, roughly124 shots before armor/modifiers exceed
starting40 but fit within existing finite resupply. Human accuracy/collection and
premium-ammo dependence still require playtesting; crates retain prior behavior.

## Sprint0034 final handoff — 2026-10-28

Final implementation including safe pause/background/resume:78/78 selected tests
passed (64Play Mode,14Edit Mode); raw JSON and ValidationSummary.json. New economy
suite11/11 covers restart migration, ownership, whole-pack fit, zero-money safety,
failed-save rollback, armor condition, caps, resupply, active interruption, safe
background/pause/resume and blocked resume on write failure, and completion/shop
replay. Controlled UI captures inspected; no human or device test claim.

Final Android Development build_7641ac5dff71 Succeeded in94.615s, zero errors,
six warnings (existing obsolete APIs/development directives and disabled remote
Pipeline config). Earlier976-warning build report is archived separately; shader
cache avoided repeated shader warnings in the final incremental build. Current
Console zero errors/six warnings, compilation clean. FinalBuildWarnings.json lists
exact messages. No unrelated warning cleanup/package changes were attempted.

Fresh APK build/Economy/AKUMU2-sprint0034-dev.apk:67,236,575bytes (about64.1MiB).
SHA256 faafcb6b12ff4c5817dc83fd5ef881f587eb32cdae536077b4543fa31222a86d.
ARM64, economy fields/armor records/reserve/pause-save metadata and existing frame
overlay packaged; DemoCatalog asset included. Warehouse_Demo saved/open/stopped,
Editor ready; no missing inspected scene/prefab scripts. MainMenu remains first.

156 protected pre-sprint originals checked:146 unchanged, ten scoped runtime/catalog
changes. Git working tree:78 modified tracked,4 pre-existing deleted test resource
files,169 untracked entries overall; zero staged. Existing dirty work preserved;
no stage, commit or push. Scope lists/report identify this sprint's files rather
than attributing the entire working tree to this sprint.

Remaining: actual Galaxy A56 install/playtest for60FPS/frame-time tails/thermals,
multitouch/safe-area/scroll comfort, sound, weapon consumption and economy pacing.
No attached adb device. Analytical simulations are not player telemetry. Crates
retain premium resupply behavior; revisit consumption/prices only after playtests.
Safe pause/background preserves unused supplies; an unannounced kill during active
combat may forfeit carried supplies while preserving banked cash/ownership/overflow.
No Level2/new weapons/camera/AI/graphics overhaul started.
