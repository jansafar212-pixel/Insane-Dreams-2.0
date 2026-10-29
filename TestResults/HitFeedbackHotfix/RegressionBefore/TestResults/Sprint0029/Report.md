# Sprint 0029 — Operation: The Bunker Is Real

Implemented by Codex; human gameplay/artistic and Android-device verification remain separate.

## Gameplay integrity

- Preserved the approved Sprint 0028 environment sheet, its authoring recipe, Testbed scene, ragdoll implementation and ragdoll prefab (SHA-256 comparison in ProtectedAfter.json). No level regeneration or roster increase.
- Warehouse_Demo's two sandbag defenses, roof and two small timber headers are solid Default-layer, non-trigger static BoxCollider2D surfaces. Foreground sprite bounds match collision bounds. Decorative rear-plane posts remain noncolliding. Header clearance was corrected after actual player traversal exposed a blocked jump route.
- Officer starts inside the shelter at x142 instead of x140. Existing sandbag cover, reachable peek points, escape routes, and bounded hide durations remain. Retreat probability .8 → .9. Added muzzle-path validation and a request to seek a peek when eyes can see but the gun is blocked. Committed rays stop at the target boundary so walls behind the player do not incorrectly cancel shots.
- Kept Officer health, 55 damage, seven-round magazine, 2.8-second reload, .5-second warning, .2-second final direction lock, distinctive magenta heavy tracer and existing black grip/magazine art. His existing 1.25–4-degree spread is already tighter than Larper fire; no further accuracy increase.
- Close-box navigation can make a short .18-second safe backward run-up when an otherwise reachable hop has insufficient clearance. Three-second cooldown and at most two attempts per obstacle prevent loops; then existing replanning takes over. Samurai's separate platform navigation and Larper floor restrictions remain.
- Reused event-driven hearing for nearby enemy shots and explicit explosion events, preserving uncertain source memory, finite range, hearing debounce, hostile near-miss rules and camera firing restrictions. Obstructed hearing multiplier .45 → .6; no global per-frame scans.
- Larper initial reaction .55 → .7 seconds (jitter .12); Samurai .02 + .02 jitter → .25 + .08 jitter. Brief visual interruptions do not restart the first-acquisition delay. Existing automatic ladder exits were tested on all six revised ladders and retained.

## Medical, throwables and protection

- Four visible shop categories: Weapons, Ammunition, Body armor, Medical. Existing grenades live under Ammunition. Medical has exactly Duct Tape and Faintanyl with the requested exact descriptions.
- Preserved hold-to-heal and existing three-type throwable selection, aim preview, release-to-throw, flash disruption and smoke occlusion. Did not add another throwable system or change frag balancing.
- Faintanyl starts at zero, is purchased only, max two charges. A qualifying death immediately consumes/persists a charge, followed by a .35-second automatic revival and exactly two seconds of central damage protection. Last-charge reservation prevents a premature game-over. No checkpoint or pickup refill. Existing cartoon syringe animation and two inventory icons are reused.
- Ballistic vest and helmet protection are independent; top-quarter regional impacts use helmet condition. Swords, explosions and environmental damage bypass ballistic equipment. No motor or traversal speed penalties.

| Equipment | Condition | Ballistic absorption | Reload multiplier | Reserve penalty | Grenade penalty |
| --- | ---: | ---: | ---: | ---: | ---: |
| Light body armor ($30, existing price) | 60 | 65% | 1.04 | 0 | 0 |
| Heavy body armor ($180) | 90 | 75% | 1.18 | 36 | 1/type |
| Light helmet ($60) | 20 | 45% | 1.02 | 0 | 0 |
| Heavy helmet ($100) | 35 | 65% | 1.08 | 18 | 0 |

Heavy vest plus helmet yields 1.274 reload duration, 378/432 spare rounds per firearm and four/five grenades per type. Excess purchased stock remains saved rather than disappearing; changing to lighter equipment restores carrying access. Definitions/enemy reloads are not mutated. Native small flat-color helmet overlays follow the existing head and corpse presentation without changing joints or ragdoll physics.

## Presentation

- Original small vector AKUMU Block font follows the existing menu wordmark's block construction. No font package/dependency added. Applied through one shared USS import across the menu, shop and HUD.
- Exact Medical copy, larger product names and side-by-side responsive product cards. Current/capacity ammo and separate reserve are dynamically checked; reserve now fits below magazine text. Existing pause face/touch target and joystick dimensions/positions remain.
- MainMenu.png, MedicalShop.png, ArmorShop.png and HUD.png are advancing Editor gameplay-camera captures, visually inspected. PistolHUD.png shows the 27-round Automatic Buzzbox secondary, despite its historical capture filename. These are not device screenshots or human playtesting.

## Important changed files

- Assets/Scenes/Warehouse_Demo.unity; enemy awareness prefab settings; Assets/Data/Progression/DemoCatalog.asset; WarehousePlayer.prefab.
- EnemyCombat.cs, LarperBrain.cs, CombatNoise.cs, ThrowableProjectile2D.cs, FireworkProjectile.cs.
- GameCatalog.cs, ProgressStore.cs, GameSession.cs, CampaignMissionBridge.cs, GameFlowController.cs.
- Health.cs, AttackExecutor.cs, AreaCombat2D.cs, BodyArmor.cs, PlayerRevive.cs, WeaponCycle.cs, RuntimeWeapon.cs, PlayerLoadout.cs, PlayerThrowables.cs.
- ArmorVisual.cs, PlayerSyringeVisual.cs, WarehouseTouchControls.cs; shared typography and existing UI styles/UXML; Equipment0029 and Interface0029 assets.
- Native authoring helper Sprint0029Setup.cs; new Sprint0029 combat, system and equipment regression fixtures; existing medical/layout expectations updated to the new specification.

## Manual verification still required

1. Enter Level 1 from Main Menu; buy each Medical item and try both equipment weights. Check readable labels and reliable touch interactions on the target phone and safe area.
2. Select/aim/release all grenade types, switch back to guns, reload and collect a finite ammo crate. Confirm magazine/reserve and grenade capacity feedback.
3. Fight through the existing encounters; test cross-platform sound awareness, close-box recovery, Samurai platform pursuit, expert throws, fear and offscreen attack rules.
4. At the bunker, shoot each solid face, approach/flank, observe Officer hiding/peeking/warning/shooting/retreat and defeat him normally. Repeat after level reload.
5. Die with two purchased charges, observe two automatic revivals and short protection, then normal failure. Check helmet/vest overlays during movement and death.
6. Check six ladder exits, double jump/slide/parachute, results/wallet persistence and sustained Android frame rate. Editor tests cannot establish human combat feel or real-device performance.

Final automated results, build outcome and Editor state are recorded below when validation finishes.

### Integration assertions retained under Sprint 0029 rules

The first broader Level1CombatHotfix run reported two outdated assertions: its
fixed-x laser wall was behind the Officer after the sheltered spawn moved, and
its blast test expected ballistic vest absorption. The wall is now relative to
the actual muzzle, and the explosion test asserts damage bypass plus unchanged
vest condition. All five cases passed on rerun. No gameplay weakening was used
to make these tests pass. A genuine Officer muzzle-ray overreach (checking
geometry behind the target) was separately fixed and the six-case actual-level
suite passed afterward. Initial UI captures exposed clipped reserve text and
Medical cards below the fold; corrected layout and actual-bound assertions pass.

## Automated validation result

Latest unique results: **154 passed / 154 total**, comprising **132 Play Mode**
and **22 Edit Mode** cases; no failed or skipped cases. Regression.json records
full test names and source result files, deduplicating focused reruns. Coverage
includes new systems/combat/equipment rules; existing bunker and full Level1
route/Officer/Samurai/expert tests; medical/throwables/healing/fall/region/ragdolls;
cover/traversal/doublejump/slide; suppression/morale/spawn-spacing; save/menu/pause
flow; original mission checkpoint rollback and results; build scene order and
legacy/future save-format safety. Tests use controlled seeds/inputs and temporary
save files. This does not establish human combat feel or real-phone performance.

Pre-build native inspection found a clean saved/open Warehouse_Demo outside Play
Mode, zero missing scene/prefab scripts and zero actual Console errors/warnings.
MainMenu is the first enabled build scene. Protected assets all remain unchanged.

## Final Android build and repository state

Android ARM64 IL2CPP Development APK succeeded in595.862seconds with0errors.
Artifact: build/Sprint0029/AKUMU2-dev.apk (67,235,131bytes, about64.1MiB).
SHA-256:813b209a3fe6dd232a897adc52911b991d97a5377ea54703b82df3649044a734.
ZIP inspection confirms manifest,ARM64libil2cpp andthreebuilt scene data files.
MainMenu remains first. AndroidBuild.json is the concise result; complete native
BuildReport is AndroidBuildReport.json. This was packaging validation, not an APK
launch or physical Android test.

Build reported972warnings:970inference-package shader warnings,1existing-style
obsolete FindFirstObjectByType warning, and1Pipeline runtime disabled notice
(no runtime config). No unrelated package/compiler cleanup was undertaken.
FinalConsole.json records0actualerrors,82collapsed warnings after the build,
compilation successful. Before build actualwarnings were0; historic buffer counts
are separate. The shader warnings remain a documented limitation.

FinalEditor.json: Warehouse_Demo saved/clean/open, outsidePlay, notcompiling,
0missing scripts, Android target. Final prefab reference scan was clean.
Protected hashes remain identical after packaging. GitFinal.txt records the
preserved dirty tree;0staged entries. No stage,commit orpush performed.

All assigned code/integration workstreams are implemented. Remaining verification
is the manual gameplay/artistic/mobile checklist above; no device-performance or
human gameplay claim. No known unresolved automated test failure.
