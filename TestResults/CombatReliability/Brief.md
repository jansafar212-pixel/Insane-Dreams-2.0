# AKUMU 2 — SPRINT 0035
## Weapon Reloads, Cover Grenades, Wallet Reliability & Combat Fairness

**Type:** Gameplay repair and polish sprint  
**Priority:** HIGH  
**Engine:** Unity 2D URP  
**Platforms:** Android + PC

### OBJECTIVE

Fix several remaining weapon reload problems, improve grenade throwing while crouched behind cover, eliminate unreachable wallet pickups, and improve the fairness of Level 1's multi-platform encounters.

This is a gameplay reliability sprint.

Do not redesign the game's graphics, introduce new weapons, or expand the campaign.

Preserve the existing fast movement, intentionally crude pixel art, chaotic enemy personalities, and Android performance.

**IMPORTANT: Implement all requested changes in the actual Unity project. Do not stop after writing a plan.**

---

## 1. LMG — RELOAD SYSTEM REPAIR

Inspect the existing player LMG weapon configuration, reload animation, ammunition bookkeeping, and related prefabs.

Identify what is currently incorrect and repair the underlying implementation.

### Requirements

- Ensure reloading works consistently with the configured LMG ammunition capacity.
- Correct the visual representation of the magazine or ammunition container.
- Ensure any detachable ammunition component has correct positioning and scale.
- Synchronize the reload visuals with the ammunition transfer.
- Prevent magazines or ammunition containers from floating or duplicating.
- Fix any incorrect animations caused by changing facing direction.
- Ensure the LMG cannot fire while its reload state explicitly prohibits firing.
- Ensure firing resumes correctly after a successful reload.
- Prevent repeated reload input from restarting or multiplying the reload operation.
- Correctly handle partially filled magazines and insufficient reserve ammunition.

The LMG should have a deliberate, relatively lengthy reload appropriate to its powerful sustained-fire role.

**Do not artificially increase reload duration simply to make the weapon frustrating.**

Respect any existing armor-related reload penalties.

### State safety

Test interruptions caused by weapon switching, player death, and mutually exclusive actions.

Ensure ammunition is never transferred twice or accidentally deleted.

Determine the correct reload visual from the existing weapon model. Do not arbitrarily introduce a different ammunition-feed mechanism.

---

## 2. DESERT EAGLE — MAGAZINE SIZE & RELOAD FIX

The current Desert Eagle reload uses a magazine that looks comically small.

Fix its visual proportions.

### Magazine appearance

- Increase the magazine sprite's visual dimensions to a believable proportion relative to the existing pixel-art pistol.
- The magazine should look like a proper full-sized pistol magazine, not a tiny rectangular object.
- Maintain the established chunky, handmade art style.
- Position the magazine correctly during the reload animation.
- When inserted, the magazine must fit the pistol grip correctly.
- Do not leave a large magazine protruding below the grip.
- Keep the Desert Eagle's grip black.
- Ensure the magazine remains correctly aligned when the player turns around.

### Reload mechanics

Inspect and repair the entire player Desert Eagle reload sequence:

- Correct ammunition transfer.
- Correct reload timing.
- No duplicated magazine sprites.
- No floating magazine objects.
- No firing during prohibited reload states.
- Reliable interruption and completion behavior.
- Correct handling of empty and partial reloads.

Preserve the Desert Eagle's existing ammunition capacity and combat balance.

**Important:** The player Desert Eagle uses normal mobile aiming and firing controls. Do not introduce sniper-style release-to-fire controls.

The Desert Eagle should remain a powerful, deliberate, relatively slow-firing handgun.

---

## 3. SNIPER RIFLE — RELOAD SYSTEM REPAIR

Inspect and repair the player's sniper rifle reload behavior.

The sniper has a special mobile control scheme, which must be preserved.

### Existing sniper controls

- Holding the aiming joystick prepares the sniper shot.
- A visible aiming laser helps the player align the shot.
- Releasing the aiming control fires one shot.
- The rifle performs its automatic post-shot bolt/recovery sequence.
- The player must have a reliable way to cancel aiming without firing.

**Do not change this control philosophy.**

### Required reload fixes

- Ensure manual reloading works correctly.
- Ensure ammunition counts update accurately.
- Ensure reload animations and rifle visuals are synchronized.
- Preserve the existing configured magazine capacity.
- Prevent repeated reload inputs from causing multiple ammunition transfers.
- Prevent the rifle from getting stuck in an aiming, firing, cycling, or reloading state.
- Ensure the rifle resumes normal operation after reloading.
- Handle weapon switching during reload safely.
- Ensure reloading does not unintentionally fire a shot.
- Ensure activating reload while aiming does not accidentally trigger the release-to-fire action.

### Bolt cycle versus magazine reload

These must remain logically distinct operations.

The automatic post-shot cycle is not equivalent to replenishing the rifle's ammunition reserve or magazine.

Ensure both systems coexist without conflicting.

If the rifle uses a detachable magazine, fix its magazine visuals and attachment points. Otherwise, preserve its existing configured loading mechanism.

---

## 4. GRENADE THROWING FROM BEHIND COVER

**Major gameplay priority.**

Improve the player's ability to throw grenades while crouched behind cover.

Currently, the interaction needs to feel more reliable and natural.

### Intended gameplay behavior

The player should be able to remain crouched behind cover, select a grenade, aim its throw, and release it without needing to stand completely upright beforehand.

The grenade must leave from an appropriate position and follow a readable trajectory.

### Implementation requirements

- Allow grenade aiming while crouched in valid cover.
- Preserve the existing grenade-selection and joystick-aiming mechanics.
- Display the expected throwing trajectory.
- Ensure the trajectory preview matches the actual throw as closely as practical.
- Prevent grenades from spawning inside the player's cover collider.
- Prevent grenades from immediately colliding with the player because of incorrect spawn positioning.
- Avoid unpredictable launches in the wrong direction.
- Fix aiming and release behavior when the player faces left or right.
- Ensure grenade throwing does not break crouching or cover states.
- Allow the player to return smoothly to normal cover behavior after throwing.

### Cover interaction

A grenade should be able to pass over a cover obstacle when its predicted trajectory actually clears that obstacle.

Do not simply allow grenades to phase through solid cover.

If necessary, introduce a brief throwing posture or animation that positions the grenade's release point appropriately.

The player should not have to perform awkward joystick movements to achieve an ordinary throw from cover.

### Risk and reward

Grenade throwing from cover should be useful without making the player completely untouchable.

If the current cover system supports temporary exposure, use a brief, readable exposure window during the throw.

Do not introduce long forced standing animations that interfere with the game's fast pace.

### Grenade types

Verify the behavior with all existing player throwable types:

- Fragmentation.
- Flashbang.
- Smoke.

Ensure the throwing improvements apply consistently to each type.

Do not change their underlying gameplay effects unless an existing bug requires it.

---

## 5. WALLET PICKUP RELIABILITY — CRITICAL BUG FIX

Wallets occasionally become impossible to collect.

Investigate the actual reason.

Potential causes include wallet placement, physics collisions, unreachable positions, pickup-radius inconsistencies, destroyed or disabled triggers, and interference from environmental geometry.

Do not assume one cause without inspecting the implementation.

### Required fixes

- Ensure wallets do not spawn inside solid environmental objects.
- Prevent wallets from getting trapped behind inaccessible foreground geometry.
- Ensure wallets dropped near walls, crates, platforms, and bunkers remain collectible.
- Prevent wallets from permanently falling outside the reachable gameplay area.
- Ensure wallet physics settles correctly.
- Fix pickup detection inconsistencies.
- Preserve the existing automatic pickup system.
- Verify the enlarged pickup radius works consistently.
- Ensure collection works when the player approaches from either direction.
- Ensure wallets can be collected while crouching or moving normally.

### Recovery mechanism

Implement a lightweight fallback for wallets that become physically inaccessible.

For example, if a wallet settles in an invalid position, relocate it to a nearby valid and reachable position.

The correction should happen without the player noticing an obvious teleport during normal gameplay.

Avoid expensive continuous physics searches.

### Currency reliability

- Every wallet may be collected only once.
- Wallet collection must credit the correct amount.
- Avoid duplicated rewards.
- Avoid wallets disappearing without awarding currency.
- Ensure the mission-end wallet total updates correctly.
- Preserve any existing pickup notification.

### Testing locations

Specifically test wallets dropped:

- Next to walls.
- On platform edges.
- Behind cover.
- Near the Officer's bunker.
- Near crates.
- On elevated platforms.
- During ragdoll collisions.
- Near level boundaries.

**Success criterion:** A wallet dropped in a legitimate playable encounter must remain obtainable without forcing the player into impossible movement.

---

## 6. AGENDA ITEM — LEVEL 1 CROSSfire FAIRNESS

Improve the existing three-platform combat section where enemies can engage the player from several vertical positions.

This is a targeted encounter-polish task, NOT a Level 1 redesign.

### Problem

Some multi-level firefights risk overwhelming the player without providing enough readable threats, cover, or reasonable opportunities to respond.

The goal is to preserve intensity while eliminating unfair situations.

### Required improvements

**A. Vertical cover**

Ensure the player has meaningful opportunities to use cover when engaging enemies above or below.

Do not make every cover position universally safe.

Cover should provide a tactical option, not permanent invulnerability.

**B. Enemy activation**

Avoid having every enemy on multiple platforms immediately concentrate fire on the player simultaneously.

Use appropriate existing enemy perception and engagement states to create more readable encounter escalation.

Do not simply disable enemies or make them passive.

**C. Visible threats**

Preserve the existing principle that ordinary enemies should not unexpectedly shoot the player from far outside the visible camera area.

Keep explicitly designed long-range enemies such as snipers and mortars as future exceptions where applicable.

**D. Player counterplay**

The player should have opportunities to:

- Retreat.
- Change platforms.
- Take cover.
- Return fire.
- Throw grenades.
- Reposition using the existing movement system.

Avoid mandatory positions where unavoidable attacks arrive simultaneously from several levels with no practical response.

**E. Preserve enemy personalities**

Larpers should remain cowardly, inaccurate, and imperfect.

Do not turn them into coordinated elite soldiers.

Keep the Samurai aggressive.

Preserve the Officer's evasive personality and distinct Desert Eagle behavior.

Do not globally reduce enemy damage just to disguise a poorly constructed encounter.

### Scope restriction

Keep the level's existing structure and introductory role.

Make focused changes to cover placement, sightlines, enemy activation, or encounter spacing where necessary.

---

## 7. DEVELOPMENT CURRENCY HOTFIX — 10,000 CREDITS

Preserve the temporary playtesting currency allowance.

The player must have access to at least **10,000 credits** for testing weapons and equipment, including on existing save files.

### Requirements

- Work on both new and existing saves.
- Do not rely exclusively on initial-save defaults.
- Ensure the amount is reflected correctly in the shop.
- Persist the adjustment correctly.
- Prevent the adjustment from being repeatedly added every time the game launches.
- Ensure previously created saves with only 100 credits receive the development allowance.
- Do not affect normal wallet collection or purchase deductions.

Use an explicit, versioned, one-time development grant or equivalent safe mechanism.

**Keep this temporary testing allowance active through Sprint 0036.**

Do not treat 10,000 credits as the intended final-release starting economy.

---

## 8. SHARED WEAPON STATE VALIDATION

Because several reload systems are being fixed, inspect common weapon-state handling.

Verify that the different reload mechanisms do not interfere with one another.

The SMG and shotgun fixes from Sprint 0034 must remain functional.

### Regression prevention

Ensure:

- No ammunition duplication.
- No negative ammunition counts.
- No permanent reload lockouts.
- No floating magazine sprites.
- No repeated animation spawning.
- No inability to fire after switching weapons.
- No accidental sniper shots caused by reload cancellation.
- No firing while healing when the current gameplay rules prohibit it.
- No broken animations when reversing character direction.

Preserve existing reload-speed modifiers associated with armor.

Do not rewrite every weapon system unnecessarily.

---

## 9. ANDROID PERFORMANCE & CONTROL VALIDATION

The game is currently running smoothly on the actual Android device.

**Do not sacrifice this improvement.**

- Avoid heavy physics queries every frame.
- Avoid excessive spawning of temporary GameObjects.
- Clean up grenade and reload visual objects.
- Avoid expensive repeated wallet-position validation.
- Preserve responsiveness of both virtual joysticks.
- Ensure grenade aiming does not interfere with the movement joystick.
- Preserve current mobile camera and control behavior.
- Avoid expanding HUD clutter.

Test the affected mechanics on Android where a device build is available.

Do not claim mobile testing was completed if only desktop or editor tests were performed.

---

## 10. REQUIRED TEST MATRIX

### Weapon reload tests

- [ ] LMG empty reload.
- [ ] LMG partial reload.
- [ ] LMG reload with insufficient reserve.
- [ ] LMG reload interruption.
- [ ] LMG magazine/ammunition-container visuals.
- [ ] Desert Eagle magazine correctly sized.
- [ ] Desert Eagle magazine correctly seated.
- [ ] Desert Eagle reload from empty.
- [ ] Desert Eagle partial reload.
- [ ] Desert Eagle visual consistency when facing left/right.
- [ ] Sniper manual reload.
- [ ] Sniper post-shot cycling.
- [ ] Sniper aim cancellation.
- [ ] Sniper weapon switching during reload.
- [ ] Sniper reload never causes accidental firing.
- [ ] SMG and shotgun reloads still work after Sprint 0034.

### Grenade tests

- [ ] Throw from standing.
- [ ] Throw from crouching.
- [ ] Throw from behind low cover.
- [ ] Throw from behind the Officer-style concrete bunker.
- [ ] Throw left and right.
- [ ] Throw from elevated platforms.
- [ ] Verify grenade trajectory preview.
- [ ] Verify no immediate incorrect collisions.
- [ ] Verify all three grenade types.
- [ ] Verify movement controls remain responsive.

### Wallet tests

- [ ] Collect wallet on flat ground.
- [ ] Collect wallet near wall.
- [ ] Collect wallet near cover.
- [ ] Collect wallet on platform.
- [ ] Collect wallet near bunker.
- [ ] Validate unreachable-wallet recovery.
- [ ] Verify currency awarded exactly once.
- [ ] Verify mission-end wallet total.

### Level 1 fairness

- [ ] Test lower-platform engagement.
- [ ] Test middle-platform engagement.
- [ ] Test upper-platform engagement.
- [ ] Confirm meaningful cover positions.
- [ ] Confirm threats are visually readable.
- [ ] Confirm grenade counterplay is functional.
- [ ] Confirm no unavoidable multi-level damage traps.

### Economy hotfix

- [ ] New save receives testing credits.
- [ ] Existing low-balance save receives testing credits.
- [ ] Reloading an already migrated save does not duplicate credits.
- [ ] Credits remain spendable.
- [ ] Wallet rewards still function.

---

## 11. FINAL IMPLEMENTATION REQUIREMENTS

Before changing code, inspect the relevant scripts, prefabs, animation controllers, weapon configurations, cover colliders, and pickup systems.

Identify which problems arise from shared systems and which require weapon-specific changes.

Prefer root-cause repairs over additional workarounds.

Avoid unrelated refactoring.

Maintain compatibility with the existing save and inventory systems.

Preserve the gameplay characteristics and graphics style of AKUMU 2.

### Sprint completion criteria

Sprint 0035 is complete when:

1. The LMG reload functions reliably.
2. The Desert Eagle uses a correctly proportioned magazine and reloads correctly.
3. The sniper reload works without interfering with its special aiming controls.
4. Grenades can be thrown reliably from behind cover.
5. Wallets remain obtainable in legitimate playable encounters.
6. The three-platform combat section offers fair and readable counterplay.
7. Existing and new saves receive the intended 10,000-credit testing allowance without repeat grants.
8. Previous weapon functionality remains intact.
9. No new obvious Android performance problems are introduced.

### Required Codex final report

Explain:

- Which files were modified.
- What caused each reload bug.
- How each bug was repaired.
- How grenade throwing from cover now works.
- How unreachable wallets are prevented or recovered.
- What changed in the three-platform encounter.
- How the development-credit grant works.
- Which tests were actually performed.
- Which tests remain unverified and require manual Android playtesting.

**IMPLEMENT THE SPRINT IN FULL. PRIORITIZE RELIABILITY, COMBAT FEEL, AND REGRESSION SAFETY.**