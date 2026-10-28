# Expanded Sprint 0034 — SMG/shotgun reload and economy integration

2026-10-28. This implements the newly supplied **reload/economy** brief in Brief.md,
not a repeat of the earlier economy-only Sprint0034. Prior UI, AI, movement,
progression and the urgent release wallet hotfix were retained. No commit/push.

## Causes identified

**Shotgun:** WeaponCycle had only one reload mode: wait until ReloadEndsAt and
transfer the entire missing magazine. Doorbell Shotgun had no individual-loading
data or shell renderer. There was no committed-shell progress to preserve or
shoot-to-interrupt behavior. It necessarily jumped to full and used a generic pose.

**SMG:** its native definition had no magazine sprite and physicalMagazineReload
was false. Its magazine was baked into the weapon body image, so the existing
generic reload pose could not visibly exchange a separately attached magazine.
There was no SMG-specific detached magazine pool to fix; the pistol-only debris
handler does not handle SMG. Reused the existing rig's actual exchange system,
rather than introducing a second reload state machine or physics debris system.

**HUD:** ammo was sampled every0.2s. This lag was particularly noticeable with
individual shell steps. Added ammo-change invalidation while retaining the cached
readout and existing UI structure. **Healing:** CanHeal previously prohibited
starting during reload. A valid healing action now cancels it while retaining
completed shell transfers; existing .8s post-hit lockout and grounded rules remain.

## Final reload behavior

### Rattlebox SMG

- Capacity30, nominal reload1.8s, existing armor duration multiplier.
- One atomic transfer at completion:12/60 ->30/42;12/3 ->15/0; full magazine or
  zero reserve does not start/consume. Repeated requests do not restart the clock.
- Switching, death, reset or valid healing cancels without transferring pending
  ammo. Existing tactical rounds remain; no dropped-ammo economy introduced.
- New scoped body variant removes only the baked magazine pixels. Original sprite,
  pivot, dimensions, muzzle and grips preserved. Separate3x7native magazine is
  centered on the old pixels at(6.5/32,2/32) and uses existing physical exchange
  pose/hand movement. Both mounted and replacement sprites are weapon children.
- No per-reload instantiate/destroy or loose floating debris. Cached replacement
  hides after completion/cancel/switch; actor destruction owns child cleanup.
  Repeated reload tests assert unchanged renderer count and parent identity.

### Doorbell Shotgun

- Capacity6, loadsIndividually=true only on this definition. Each step lasts
  2.6/6 = **0.433333s**, multiplied by existing armor/reload scaling. Full empty
  reload remains nominal2.6s; a partial2/6 reload needs four steps (~1.733s).
- Each completed clock boundary transfers exactly one reserve shell into loaded
  ammo.2/6 ->3/6 ->4/6 ->5/6 ->6/6. Ends on full or empty reserve.
- Firing with a loaded shell and a ready weapon cancels the pending reload step,
  then uses one round. An empty weapon cannot fire before insertion. Already
  inserted shells survive cancellation, weapon switches, death and snapshots.
  Resuming starts the next incomplete insertion; no animation callback owns ammo.
- Tick is the sole transfer authority. Duplicate ticks do not insert again;
  delayed frames catch up in a loop bounded by capacity (six steps here).
- A3x5flat red/brass shell moves into the existing weapon-local loading point;
  the rear arm follows without replacing pivots, joints or the character rig.
  One cached shell renderer is reused across all insertions. It hides outside
  reload, on switch/death/disable; no shell is spawned into world physics.

Other weapons retain magazine-fed behavior and their damage, cadence, recoil,
capacity and sniper/Desert Eagle controls. No enemy weapon opts into shell loading.

## Input, HUD and save integration

The existing Reload control and PlayerCombat handler remain. Repeated reload
requests are rejected by the current cycle. Movement/aim/crouch/jump logic was
not modified. Firing uses existing AttackExecutor -> TryAttack, which now supports
an allowed shotgun interruption. Input-reset/switch/death cancellation stays shared.

RuntimeWeapon.AmmunitionChanged fires only when clamped loaded/reserve values
actually change. WarehouseTouchControls subscribes to the equipped runtime weapon,
unsubscribes on rebind/disable/switch, and invalidates its cached timer. Each shell
appears on the next UI frame rather than waiting up to0.2s. Idle UI remains cached.
SMG inventory icons retain the complete original sprite while the world renderer
uses the body variant plus attached magazine. Shop/HUD prefer the explicit Icon
with the old equipped-sprite fallback. No HUD layout or touch target changes.

RuntimeWeapon snapshots already store actual loaded/reserve counts. A dedicated
test reloads one shell, captures/restores, verifies3+9 with no active reload, then
resumes to6+6. No pending animation state becomes free ammunition. Existing
CampaignMissionBridge pause/resume/reservation/payout implementation remains.

## Economy review and decisions

PriceAmmoAudit.json exports the actual eight weapons, prices, ammo packs, damage,
pellets, rates, ranges, nominal reload durations and heavy/base reserve limits.
Ideal unarmored50HP lower bounds (all shots hit at full damage; shotgun all pellets):

| Weapon | Purchase | Ammo pack | Ideal rounds / cost | Base / heavy reserve |
|---|---:|---:|---:|---:|
| Basic pistol |0|15 for5|4 /1.33|96 /84|
| Buzzbox |200|27 for12|10 /4.44|162 /142|
| SMG |350|30 for15|6 /3.00|180 /158|
| Carbine |500|24 for18|3 /2.25|144 /126|
| Shotgun |650|6 for24|1 /4.00|36 /32|
| Desert Eagle |1200|7 for42|1 /6.00|42 /37|
| LMG |1600|100 for65|4 /2.60|200 /175|
| Sniper |2000|5 for50|1 /10.00|30 /26|

These are analytical scenarios, not measured player consumption. Spread, misses,
armor, hit regions, distance falloff and premium overkill change real costs.
SMG/shotgun trade close-range speed for ammunition/falloff; precision guns pay
more per shot. No clear new inconsistency justified speculative price changes.
**Prices, starting allowance, caps, armor penalties and rewards were left intact**;
the previous adjustable per-weapon foundation is already implemented and tested.
Armor never changes movement speed; even heavy reserves exceed one magazine.

Free starter is still the ordinary15-round pistol with five magazines including
loaded:75 rounds at deployment. Purchased weapons include one magazine, not a free
five-magazine premium refill. Ammo purchases target their own WeaponDefinition;
ownership, whole-pack fit and capacity enforcement remain. Existing finite crates
and stored overflow behavior retained. The new visual assets keep stable weapon
definition IDs, so existing ownership/equipment/ammo saves do not need a reset.

Temporary playtest currency remains the urgent hotfix's all-build configurable
Max(balance,10000) at initialization; it does not stack on shop-open/purchase.
An actual local profile read showed8720; no direct-profile test purchase/reset was
performed. The next normal initialization applies the existing floor. Isolated
existing-save tests verify replenishment without losing owned equipment. Do not
revert the release wallet fix or overwrite current save data to test reloads.

## Tests and controlled captures

**66/66 selected checks pass:18Edit Mode,48Play Mode.** Raw latest suite outputs
are in TestRuns.json. The entire project-wide suite was not run.

- ReloadRepairRules11: empty/partial/full/low reserve, exact single-shell boundaries,
  duplicate ticks, one/two-shell interrupts/fire/resume, atomic SMG tactical reload,
  repeated input, armor scaling, large-tick bounds and snapshot/restore.
- ReloadRepairPlay4: real-clock running/jump/crouch/two-facing SMG exchange,
  repeated reload, switch cancellation and renderer count; actual shotgun fire,
  switch/death and hidden shell; valid tape interruption; next-frame HUD3/6 and
  safe repeated Bind. Captures SmgExchange0/1.png and ShotgunShell.png inspected.
- Existing ArsenalRules4, EconomyRules3; Arsenal8, Economy11, Wallet5,
  CampaignFlow12, CachedHud/pose2, Healing1 and Officer/bunker5 all pass.

Initial new healing fixture attempted before the existing .8s damage lockout and
was corrected to wait for it, without shortening gameplay lockout. Existing arsenal
tests assumed one ReloadEndsAt meant a full shotgun reload; they now advance the
full nominal chain when testing full completion. They still assert exact totals.
The old rule that new long guns have no *pistol* magazine excludes SMG's deliberately
new own physical magazine. No test deleted. Tests/captures use isolated actors or
temporary profiles and controlled state; they are not human/device acceptance.

## Performance and limitations

No new physics queries, coroutines per shell, procedural effects, materials or
world debris. One shell renderer and existing two magazine renderers are cached;
shell catchup is bounded. Ammo event subscription changes only on weapon rebind/
selection; inactive readouts remain cached. More frequent text updates during firing
and shell insertion are intentional for accuracy and require phone profiling.

No physical Android device is connected. Actual FPS/frame-time/GC tails, thermal
behavior, mobile reload-button comfort and human artistic/gameplay feel remain
unverified. The earlier native Editor ADTM audio fatal remains undiagnosed; no
sound-system rewrite or new reload sound was introduced. The sprint does not claim
a human full-combat Level1 run or guaranteed absence of every navigation softlock.

## Files changed in this implementation

Runtime: Assets/Scripts/Combat/{WeaponCycle,PlayerHealing}.cs;
Assets/Scripts/Inventory/{WeaponDefinition,RuntimeWeapon}.cs;
Assets/Scripts/Visuals/{CutoutRig,CharacterVisualDriver}.cs;
Assets/Scripts/Input/WarehouseTouchControls.cs;
Assets/Scripts/Progression/GameFlowController.cs (inventory icon fallback only).

Native authoring helper: Assets/Editor/ReloadRepairSetup.cs (+meta).
Native data via Editor APIs: Assets/Resources/Arsenal/{SMG,Shotgun}.asset;
Assets/Art/ReloadRepair/{SmgBody,SmgMagazine,ShotgunShell}.asset plus their native
Texture2D assets/metas. Original SMGSprite and shotgun/character pixels unchanged.

Tests: new EditMode/ReloadRepairRulesTests.cs and PlayMode/ReloadRepairPlayTests.cs
(+metas); adapted EditMode/ArsenalRulesTests.cs and PlayMode/ArsenalTests.cs.
Handoff: GRAPHICS_BIBLE.md, DEVELOPMENT_LOG.md, SPRINT_RESUME.md, evidence folder.
No serialized scene/prefab YAML/GUID edits, no stage/commit/push; prior dirty work
preserved. This list does not attribute the full working tree to this sprint.

## Android build

Release build **build_832d92bfda0d succeeded in128.494s**, zero errors,974 warnings
(full BuildStatus.json). APK: `build/ReloadEconomy/AKUMU2-reload-repair-release.apk`,
39,817,203 bytes (~38MiB). SHA256:
`98a13f846658f116d8ec897582344c4e1407c90a49f06b3717b246aa1d128398`.
Aapt confirms ARM64 and no application-debuggable marker. APK metadata contains
shell-mode data, PoseReloadShell, AmmunitionChanged and the preserved wallet tag.
ApkVerification.json records packaging checks; no source/asset edits during export.
This APK was **built/inspected, not installed or played on Android**. No Windows
Player build was performed. Final Editor state is checked separately at handoff.
