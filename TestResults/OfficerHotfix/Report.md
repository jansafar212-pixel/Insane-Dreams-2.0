# Officer miniboss hotfix — 2026-10-25

## Scope

Officer-only weapon/presentation hotfix. The existing15-enemy Level1 arrangement, encounter activation, progression, geometry, other archetypes and all existing uncommitted work are preserved. No encounter-pacing redesign, scene authoring or new gameplay framework.

## Root causes and implementation

- Laser length was tied to weapon range, without accounting for larger camera views or the Officer's position inside the view. At the standard warehouse zoom, some observed short beams also legitimately hit the nearby dais/cover; that geometry clipping remains intentional. It now covers at least the camera's visible diagonal plus muzzle-to-center distance and a small margin. The pistol now points along the actual sampled laser direction during tracking as well as the final lock, instead of showing nominal aim while the beam uses aim error. The same single geometry ray clips the line against real solid cover/platforms/walls. Widened crisp red line, no glow, particles, lighting system or global searches. It starts at AttackExecutor's actual constrained muzzle.
- The previous Officer weapon copied the Larper automatic pistol artwork, with its magazine removed. Independently code-authored27x14 heavy handgun silhouette at32PPU now has a stepped rectangular slide, long barrel, hollow trigger guard, slanted dark grip and four opaque flat colors. Compact3x5 magazine shares the same simple palette. Point filtering, binary transparency, no mipmaps. The hand pivot is(6,6) pixels; muzzle is on the actual barrel edge five pixels above that pivot. Historical originals and all character body modules remain unchanged. Artwork is provisional pending user artistic review.
- Existing articulated carrying hand remains the weapon anchor. Officer-only presentation adds32-degree upward recoil and.09-unit setback over.5s; it returns to the current aim without changing AI shot direction. The free arm remains visibly separate during firing; the existing support-hand movement is used for reloading.
- The existing PistolMagazineReload component did not handle the Officer's definition, which had no magazine sprite. It now handles that scoped weapon, with the existing physical magazine release/replacement and articulated hand exchange. The generic base replacement is hidden for this Officer so it cannot duplicate the specialized replacement. No separate ammo system was added.
- Shared WeaponCycle remains the only ammunition transfer authority: ammo stays empty throughout the exchange, transfers at completion when the replacement is seated, then the Officer must perform a fresh warning before shooting again. Exact2.8s reload has no AI duration jitter. Interruption/death cancels the cycle without free ammo or orphaned replacement visuals.
- Officer inherited Larper partial-magazine refill decisions after retreat. His reload threshold is now0, and finishing an evasive move with remaining rounds resumes cover/search instead of topping up. Seven deliberate shots exhaust the magazine. Cowardly movement, cover, support-ally cooldown and original HP remain.

## Configurable values

| Value | Before | After |
|---|---|---|
| Damage |42 |55 |
| Magazine |7 |7 exactly |
| Reload |4s with5% variation |2.8s, variation0 |
| Warning |1s |1.15s |
| Final direction lock |.2s |.2s unchanged |
| Weapon interval |2.4s |2.4s unchanged |
| Post-shot warning cooldown |2.4s +.8s with nearby ally |unchanged |
| Practical earliest normal shot interval |3.4s alone/4.2s supported |3.55s alone/4.35s supported, before AI interruptions |
| Beam width |.035 |.055 |
| Recoil |generic.12s arm accent |32-degree weapon/arm kick,.09 setback,.5s recovery |
| Partial-mag reload preference |Larper personality-dependent |disabled for Officer; reload at empty |

55 damage is55% of current unarmored100HP; a full-condition vest's existing65% absorption reduces a body hit to19.25HP while consuming armor. Existing head-impact/armor rules remain; no player health/armor logic changed. Two unarmored successful hits can defeat the player, with1.15s warning for each.

## Modified files

- Runtime: Assets/Scripts/Enemies/{OfficerShotTelegraph,EnemyCombat}.cs; Assets/Scripts/Visuals/{OfficerWeaponPresentation,CharacterVisualDriver,SamuraiArticulatedRig,CutoutRig}.cs. The new presentation component exists only on Officer; shared additions are inert for other characters.
- Native assets: Assets/Data/FirstOperation/Officer.prefab, OfficerPistol.asset; new DesertEagle.asset/DesertEaglePixels.asset and DesertEagleMagazine.asset/DesertEagleMagazinePixels.asset with generated metadata.
- Editor recipe: Assets/Editor/OfficerHotfixSetup.cs. Do not run the older FirstOperationSetup or Level1CombatHotfixSetup to apply this follow-up.
- Tests: Assets/Tests/PlayMode/OfficerHotfixTests.cs; updated intentional timing/balance expectations in Level1CombatHotfixTests.cs and Assets/Tests/EditMode/Level1CombatHotfixRulesTests.cs.
- Documentation/evidence: DEVELOPMENT_LOG.md, ART_BIBLE.md, TestResults/OfficerHotfix/.

## Verification

60/60 unique targeted cases pass:4 Edit Mode (rules2, layout2) and56 Play Mode (Officer3, existing combat5, Level1 integration6, Larper reload1, player/weapon suppression7, player weapon1, cover12, movement/double jump9, campaign/shop/results12). The affected Officer/combat/player-weapon cases were rerun after the final tracking-direction change; reruns are not double-counted. Exact results: Regression.json and suite JSON files. Initial failing presentation-check evidence is retained separately.

Compilation is clean. Final current Console ground truth is0 errors/0 warnings; its historical buffer retains earlier test/deprecation records. FinalVerification.json confirms Warehouse_Demo open/clean outside Play Mode,0 missing scripts, unchanged groups[3,4,3,3,2], working Officer presentation/reloader references and unchanged50HP. Of805 existing non-code asset/reference files checked, only Officer.prefab and OfficerPistol.asset changed; all scenes, other prefabs/assets and historical originals remain byte-identical. New assets are limited to the pistol/magazine sprites and textures. No staging, commits or pushes.

Focused3/3 pass: viewport coverage at4:3/16:9/20:9 in four directions, actual geometry stop/muzzle origin/grip attachment; full seven-shot real-AI ammunition/laser/damage sequence, visible exchange/no reload fire and post-reload warning; mirrored recoil/exchange/interruption/normal defeat. Controlled native camera captures were inspected; they are not human gameplay or Android-device testing.

## Manual review still needed

Play Warehouse_Demo through the existing Officer encounter. Check handgun recognizability at gameplay/phone zoom, red warning against warehouse backgrounds and camera movements, the final.2s dodge opportunity, both-facing one-handed kick/recovery, seven-shot reload vulnerability and evasive behavior while supporting guards live or are defeated. Check recoil and hand/magazine readability while retreating/crouching and after taking hits. Confirm fairness against armor and Android touch performance on hardware. No APK/device benchmark or human artistic acceptance is claimed.

Implemented by Codex — awaiting hands-on playtest/artistic verification.