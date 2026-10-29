# Arsenal expansion — latest user Sprint 0033

This follows the adaptive-camera and Android-performance briefs. The earlier
0033 bunker work is preserved. Original current brief: Brief.md in this folder.

## Implemented roster and provisional tuning

All guns keep the same player movement speed. Ordinary guns use the existing
outer aiming-stick firing threshold and repeat at their configured cadence.

| Weapon | Role | Damage | Interval | Magazine | Reload | Range |
|---|---|---:|---:|---:|---:|---:|
| Backup Pistol, preserved | Dependable backup | 14 | .20 s | 8 | 1.5 s | 22 |
| Player Desert Eagle, new separate definition | Powerful deliberate handgun | 55 | .65 s | 7 | 2.8 s | 28 |
| Rattlebox SMG | Fast close-range spray | 9 | .065 s | 30 | 1.8 s | 20 |
| Warehouse Carbine, preserved | Accurate general-purpose automatic | 20 | .20 s | 24 | 1.5 s | 22 |
| Doorbell Shotgun | Close-range pellet burst | 7 x 8 maximum | .85 s | 6 | 2.6 s | 16 |
| Lunchbox LMG | Sustained fire/suppression | 14 | .11 s | 100 | 8.0 s | 26 |
| Long Complaint Sniper | Hold/release precision | 90 | 1.0 s recovery | 5 | 2.6 s | 45 |
| Existing automatic Glock / Buzzbox, preserved | Fast automatic pistol | 5 | .07 s | 27 | 4.0 s | 28 |

Existing melee/throwable systems remain unchanged. No redundant infantry or
marksman rifle added. Existing Officer weapon remains separate: 55 damage,
2.4-second interval, seven rounds, 2.8-second reload and its existing telegraph.
The player's Desert Eagle has ordinary firing and no aiming laser/release trigger.

Accuracy/falloff distinguishes the guns beyond damage and capacity:

- SMG: random deviation +/-5 degrees plus up to 4 degrees sustained bloom;
  falloff begins at 6 units, reaching 35% damage at maximum range.
- Shotgun: seven nearest-hit rays across a 24-degree total fan. One cartridge and
  firing cue per trigger. Falloff begins at 4 units, reaching 15% pellet damage at
  maximum range. Each pellet can hit only its nearest eligible collider; overlapping
  colliders do not duplicate that pellet's health damage.
- LMG: +/-2 degrees initially, up to 10 additional bloom degrees under sustained
  fire. Falloff starts at 12 units, reaching 65% damage at maximum range.
- Bloom recovers from elapsed game time at the next shot, without per-frame work.
  Recovery rate is .35 times maximum bloom degrees/second.
- Preserved pistol/carbine/Glock and new Eagle/sniper retain full damage over
  their configured range. No penetration or realistic ballistics introduced.

New Eagle/SMG/shotgun/LMG/sniper firing pitches are respectively .70/1.45/.62/.80/.55
using the existing firing clip fallback. Flash scales 1.7/.85/1.8/1.2/1.9 and recoil
scales 2.1/.65/2/1.25/2 distinguish their presentation. Eagle/sniper impulses are
2.5/3; shotgun impulse .55 per pellet. These remain Inspector-configurable.

## Shared implementation and input safety

WeaponDefinition extends the existing configuration with fire mode, bounded pellet
count, spread/bloom, falloff and presentation fields. Defaults preserve old assets.
WeaponCycle still owns cadence, one-round debit and the single reload transfer;
armor multipliers and reload cancellation remain intact.

AttackExecutor reuses its ContactFilter2D/hit buffer and nearest-collider selection
per pellet. A trigger increments ShotsFired and emits firing feedback once. Actual
pellet segments feed existing suppression/noise; existing source cooldowns prevent
same-frame pressure spam. Hit feedback uses an actual damaged pellet point, while
central shot metadata stays aligned with the central tracer. Enemy AI was not edited.
The player's existing tracer ring is prewarmed to 16; impacts/audio still use their
existing bounded pools. No physical bullets or per-shot Instantiate/Destroy.

**Sniper on touch:** hold the existing aim joystick beyond .30 magnitude to aim;
move it to aim; release once to fire; return to its central dead zone before lifting
to cancel. Holding never fires. Recovery is automatic, with a .35-second settling
recoil presentation and a one-second firing gate; HUD shows CYCLING. Before recovery
ends an early release does not queue a later surprise shot. Hold again after recovery
or keep holding until ready and then deliberately release. No bolt/fire button added.

**Desktop:** hold RMB to aim and release RMB to fire the sniper. Ordinary weapons
retain their existing RMB/LMB and F controls. No physical keyboard/mouse or phone
playtest is claimed by the scripted touch-input checks.

Normal PointerUp is distinct from pointer cancellation/capture loss. Reset, pause,
death, revival, reload, invalid/central aim and weapon switches discard pending shots.
Leaving sniper clears only aim/fire and its right pointer; left-stick movement stays
active, and the old hold cannot turn into a grenade throw or pistol shot. Desktop
physical held aim/fire must become neutral after cancellation. Valid release retains
the aim/peek pose only through its shot frame so cover does not retract first.

The cyan player aiming line is distinct from the Officer warning and heavy shot.
It reuses one LineRenderer/shared material and samples the same blocking geometry
at 10 Hz, with no per-frame raycast. Direction/positions render continuously. Sniper
shots have a wider pale-blue tracer. Existing camera consumes the same meaningful
aim signals; no camera-follow code or zoom was changed.

## Developer arsenal lab and normal progression

Editor or **Development Build**: MainMenu > DEV / ARSENAL LAB. This opens the existing
Warehouse_Testbed with eight guns, preserved per-gun RuntimeWeapon instances, and an
explicit REFILL TEST AMMO button (finite magazines and 180 reserve per gun; LMG uses the restored 200-round reserve).

Before entering, the menu session saves legitimate current data and is destroyed.
The standalone testbed has no GameSession, so the campaign bridge cannot capture
test ammo, grant ownership, save rewards or alter normal progression. EXIT LAB /
NORMAL MENU loads the original saved profile. An integration test compares profile
bytes before/after equipping/refilling every gun and returning to the menu.

Lab code/entry is compiled out of normal release builds. Testbed is appended to the
shared build list so the runtime scene load works; MainMenu remains first and normal
level selection still opens Warehouse_Demo. No scene contents or encounters changed.
The extra testbed scene/resources are packaged, but normal release progression has
no lab entry. New weapon acquisition/prices are deliberately deferred to Sprint0034;
new guns are immediately available in the lab, not silently granted to campaign saves.

## Modified files/assets

- Inventory: WeaponDefinition.cs; new ArsenalCatalog.cs. New development-only Camera/DevelopmentFrameOverlay.cs.
- Combat: AttackExecutor.cs, WeaponCycle.cs, PlayerCombat.cs, CombatFeedback.cs;
  new developer-only ArsenalTestBench.cs.
- Input: PlayerActions.cs, WarehouseTouchControls.cs (gesture events/right-pointer
  cancellation; sniper header and cached recovery state; layout unchanged).
- Visuals: CharacterVisualDriver.cs, CutoutRig.cs (data-driven muzzle, cover/crouch
  alignment and recoil; existing sprite-setter caching preserved).
- New Assets/Editor/ArsenalSetup.cs; native authoring, no YAML/GUID editing.
- Assets/Resources/ArsenalCatalog.asset and Arsenal/: five new gun definitions,
  four native sprite/texture pairs, point filtering/no mipmaps/32 PPU/flat palette.
  Existing Eagle black grip and seated magazine artwork reused without changes.
  New long guns do not inherit the old extended pistol magazine.
- WarehousePlayer.prefab: maximumTracers 6 -> 16 only. Linked Testbed instance
  verified to inherit 16. No physical/visual body dimensions or joints changed.
- ProjectSettings/EditorBuildSettings.asset: Testbed appended only.
- New ArsenalRulesTests.cs, ArsenalTests.cs, ArsenalLabTests.cs; DevelopmentFrameStatsTests.cs and native-ID performance fixture correction; evidence here;
  DEVELOPMENT_LOG.md and SPRINT_RESUME.md.

Existing weapon assets, prices, ownership/catalog offers, normal level layout,
enemy behavior, suppression, health, physics, movement, camera and UI layout assets
are preserved. ProtectedBefore/After.json records the actual scope.

## Verification and limits

Final suite counts, raw results and Editor/reference checks: Validation.md.
New checks include all eight guns moving/jumping/crouching, empty/reload/interrupt/
switch conservation with armor scaling, all eight against all four actual enemy
hitboxes, seven pellets versus overlapping colliders, sniper release/cancel/recovery/
switch/pause/death/real Faintanyl revival, grenade-switch safety with movement kept,
long unobstructed laser and renderer reuse, bloom build/recovery, and lab/save isolation.

Controlled stress: 360 trigger pulls / 1,080 ray segments, tracer count stays bounded
(17 lines including the existing grenade trajectory, before lazy sniper line).
This concentrated Editor test is not sustained device profiling or a zero-GC claim;
the current Unity Mono allocation counter is known unsupported from the previous
sprint. No Android performance certification is inferred from it.

Advancing lab captures and state snapshots were inspected: Lab-DesertEagle.png,
Lab-Shotgun.png, Lab-LMG.png, Lab-Sniper.png. They verify controlled presentation,
firing/ammo and armed laser state, not artistic approval or human gameplay feel.

Resolved during verification: stale trigger crossing sniper selection, missing
Testbed build-list entry, inherited pistol magazines, and custom muzzle crouch/peek
placement. Fixture fixes advanced synthetic cooldown times and retired prior corpses
between isolated hitbox rows (real corpse bullet blocking is preserved).

No Android device is attached. A56 multitouch/capture-loss, real keyboard/mouse,
audio audition, long-session performance and weapon feel require manual testing.
The host has reported FMOD output-device warnings; no audio success claim is made.
Historical Sprint013 fixture failures documented by earlier work were not claimed
fixed; selected regressions are not the complete project suite.

## Manual A56 / desktop checklist and balance review

1. Install the current Development APK, open MainMenu and enter DEV ARSENAL LAB.
   Choose/refill each weapon; exit and confirm normal wallet, ownership and level flow.
2. Ordinary guns: use normal outer-stick firing while running both ways, jumping,
   crouching, peeking and eligible blind firing. Empty, reload, interrupt and switch.
   Check current/capacity and separate reserve values; test existing ammo crates.
3. Sniper: hold inner/outer meaningful aim without shooting; release one shot,
   center then lift to cancel; wait/hold through recovery, release again. Test both
   directions/elevations and solid cover. Its blind-fire button must not become a
   separate sniper fire control; sniper trigger remains aim-stick release.
4. Switch sniper -> pistol/grenade mid-hold while continuing left-stick movement;
   cancel by pause, focus loss, Android interruption, death and revival. No latent shot.
5. Fight all enemy roles: close SMG spray, shotgun pellet fan/falloff, LMG suppression,
   Eagle impact and precision rifle range. Verify corpse/armor effects and ordinary
   NPC viewport fairness, reaction times and reloads.
6. Check muzzle/flash/tracer, black Eagle grip/magazine and all reload poses left,
   right, up/down, behind low cover and on ladders. Review new art at phone size.
7. Profile sustained SMG/LMG/pellet bursts with several enemies, live ragdolls, smoke
   and fireworks on A56. Use the previous performance report's 15-minute protocol;
   compare Development overhead separately from release performance.

Revisit from playtests: SMG close damage vs rapid ammo use, shotgun pellet total/
falloff and barrel clearance, LMG bloom/reload, Eagle recoil/cadence, sniper one-second
recovery/laser thickness and cancellation threshold. Economy and ammo prices remain
outside this sprint. New graphics and weapon feel await human review.

## Context-restoration continuation

The repository identified the same Sprint0033 arsenal as active; completed camera,
performance and weapon implementation was preserved. First APK export ended
Cancelled after690.332s, with 'Building Player was cancelled' / TaskCanceledException;
no APK was produced. BuildCancelled.json preserves the exact result.

Latest explicit LMG decision is now100 loaded /200developer reserve /8-second reload,
correcting75/180/4.2. Only the existing model was tuned; no AA-12, P90, other future
variants, shared grenade inventory, slot restrictions or economy changes started.
New configurable DeveloperReserve is used only in the lab, not campaign allowances.

DevelopmentFrameOverlay adds a fixed120sample rolling FPS/mean/P95/P99 display in
warehouse scenes. It updates text once persecond, sorts reusable scratch storage,
ignores paused simulation samples, and never changes framecaps, input or camera.
Editor/Development only. Real Android performance remains unverified. Additional
frame-statistics unit test covers spikes and expiration. Updated LMG/overlay capture
inspected inLab-LMG.png. Native sprite-ID assertions replace a managed-wrapper
reference comparison in the existing performance test; native identity is retained.


## Final export and handoff (2026-10-28)

Retry build_369cf4a48eb7 Succeeded in324.143s, zero errors,980 warnings.
APK: build/Arsenal/AKUMU2-arsenal-dev.apk,67,238,173 bytes (about64.1MiB).
SHA256 c6beca846c937fe34dd3abf4e36840529e1287061e0db4b64417d6ab9d8718e8.
BuildReport totalSizeBytes includes additional development outputs; it is not APK
file size. BuildRetryStatus.json preserves raw outcome; ApkVerification.json and
PackagedArsenal.json record ARM64 libraries, lab/frame-overlay/release-fire metadata,
arsenal resources/catalog and Testbed scene packaging. MainMenu remains first.

Warnings are predominantly Sentis compute-shader warnings, plus obsolete Unity API
calls (including FindObjectsSortMode in the lab/test code) and disabled remote
Pipeline in Player because no RuntimePipelineConfig exists. No unrelated shader,
package or tooling changes were made to suppress these. Final current Console has
zero errors and90 build warnings; clean compilation, Editor ready/outside Play Mode.
Warehouse_Demo remains active/open/saved. No missing scripts found in native audit.

All73 selected automated checks passed; no device attached via adb. Installation,
Galaxy A56 60FPS/frame pacing/thermals/multitouch, audio and human weapon/art review
remain required. Implementation/build verification is not gameplay certification.
Git working tree preserved,0staged, no commits/pushes. See FinalGitSummary.json.
