# Grenade resupply, fragmentation VFX and rare arm detachment

Implemented by Codex — awaiting human playtest verification.

## Changes and values

- Existing supply boxes now contain a finite configurable two grenades per type (fragmentation, flashbang, smoke). Partial transfers respect the existing five-per-type cap and leave unaccepted stock in the box. Disable/re-enable does not replenish boxes. Existing explicit mission reset restores all box contents. Ordinary ammunition, tape, interaction range, obstruction checks and presentation are preserved. The five Demo boxes retain their existing 108 conventional rounds each.
- One inventory event per accepted batch refreshes the mobile HUD on its next frame and uses the existing campaign save path.
- Fragmentation radius 3.2 → 4.1 (+28.125%); peak damage 85 → 110.5 (+30%). Existing linear falloff, geometry obstruction, friendly/self-damage rules and impulse 2 remain. Bounded area-query capacity 64 → 128 accommodates the larger blast and existing multi-collider corpses.
- Fragmentation only: brief bright flash, three flat-color fire layers, eight outward cosmetic debris sprites, three short grey smoke puffs. Prewarmed 15 renderers/projectile, existing six-projectile pool; no detonation-time GameObjects, particle systems or fragment physics. The native 8x8 stepped mask is point filtered, 32 PPU. Settings: fire diameter 3.6, smoke diameter 4.8, debris travel 4.2, duration .95 seconds. Cosmetic smoke does not conceal targets.
- New short deterministic mono boom (.6 seconds, 22050 Hz), because the inspected existing projectile had no assigned explosion clip. Distance-limited shake amplitude .08, duration .16 seconds, range 12; amplitude hard-clamped .12. Shake uses the existing camera owner and never feeds back into follow smoothing. One existing occluded combat-noise event per frag detonation.
- Flash radius 6/duration 1.6 and smoke radius 3.5/duration 8 remain unchanged; no added damage, sound or shake for these types.
- Independent cumulative arm threshold 60 → 105 (+75%). Head 80, legs 60, torso 150 unchanged. Existing localized region selection, corpse transfer and physics remain. A single hit still accumulates into one region rather than distributing to both arms.

## Files for this hotfix

Production scripts: Assets/Scripts/Combat/{PlayerThrowables,ThrowableProjectile2D,AreaCombat2D,BodyRegionDamage}.cs; Assets/Scripts/Inventory/{PlayerSupplies,SupplyCrate2D}.cs; Assets/Scripts/Input/WarehouseTouchControls.cs; Assets/Scripts/Camera/WarehouseCameraFollow.cs.
Native authoring: Assets/Editor/GrenadeResupplyHotfixSetup.cs; Assets/Data/Sprint0027/Throwable.prefab; new FragBurstPixels.asset, FragBurst.asset, FragBoom.wav and Unity metadata in that folder. Existing placeholder square reused for debris. Prefab updated through Unity APIs.
Coverage: Assets/Tests/PlayMode/GrenadeResupplyHotfixTests.cs and metadata.
Documentation: ART_BIBLE.md, DEVELOPMENT_LOG.md, SPRINT_RESUME.md; evidence in this directory. Earlier dirty changes elsewhere are preserved and are not attributed to this hotfix.

## Verification

See Regression.json for final unique automated results. New controlled cases verify actual Demo box stock/cap/partial acceptance/HUD/save, real frag damage/falloff/cover/self-damage, unchanged flash/smoke parameters and pooled lifecycle, actual enemy death/corpse, cumulative single-arm detachment and distance-limited expiring shake. Existing throwable, supply, regional damage, ragdoll and corpse/player collision checks are included.

The initial box assertion assumed the generic 72-round prefab default; the actual Demo boxes have 108. The test was corrected to verify each box's existing amount. No ammunition balance change was made. A premature test run before native prefab assignment was canceled; final results use the applied prefab.

FragFireball.png and FragSmokeDebris.png are inspected automated camera captures, not human gameplay. ProtectedAfter.json confirms both warehouse scenes, ragdoll prefab/source and EnemyCombat.cs byte-identical to hotfix start.

## Manual verification still required

1. Export a fresh APK; previous APKs do not include this hotfix. Spend each grenade type, use an existing box, confirm partial/full inventory and HUD counts while switching types; revisit an exhausted box.
2. Throw frag beside/away from enemies and behind protective geometry. Judge the larger fireball, short smoke, audio loudness, restrained shake and threat/falloff in a normal firefight.
3. Compare flash and smoke function with their prior behavior. Observe ordinary deaths keeping arms and unusually heavy repeated localized hits still allowing detachment.
4. Profile sustained grenade use on a real Android device. Counts/pooling are bounded, but no device frame-rate or thermal claim is made.

No human balance/art acceptance, Android-device playtest, APK build or full regression-suite run claimed. No known failures in the final focused checks. No staging, commit or push.

Final result: **31/31 unique automated cases passed (24 Play Mode, 7 Edit Mode)**. Successful compilation; current Console zero errors/warnings. Warehouse_Demo clean/open, Play Mode stopped, zero missing scripts.
