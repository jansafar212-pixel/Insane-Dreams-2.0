# AKUMU 2 — SPRINT 0034
## LONG SPRINT: Economy, Shop, Ammunition & Reload System Overhaul

**Engine:** Unity 2D URP  
**Platforms:** Android + PC  
**Priority:** High  
**Sprint size:** LONG — comprehensive implementation and regression testing

### PRIMARY OBJECTIVE

Improve the equipment economy and ammunition systems while fixing the Rattlebox SMG and Doorbell Shotgun reload implementations.

This is an implementation sprint, not a planning or documentation sprint.

Inspect the existing project architecture, identify the actual causes of problems, implement the necessary changes, and test them.

Preserve the existing gameplay, enemy behavior, weapon characteristics, handmade pixel-art style, and mobile performance.

---

## 1. RATTLEBOX SMG — MAGAZINE RELOAD REPAIR

The Rattlebox SMG has a 30-round magazine.

Its reloading implementation needs a complete inspection and repair.

### Required changes

- Fix the existing magazine reload animation and its synchronization with the weapon.
- Ensure the magazine visually belongs to the SMG instead of floating, sticking to the wrong location, or moving independently.
- During a reload, visibly represent the magazine being replaced.
- Ensure magazine sprites stay correctly attached to their intended animation points.
- Ensure temporary magazine objects are removed properly after their animation completes.
- Prevent magazines from remaining suspended in midair.
- Prevent duplicate magazines appearing after repeated reloads.
- Prevent the reload animation from restarting unexpectedly.
- Ensure the animation works when the player faces either direction.
- Check that movement, crouching, jumping, and cover interactions do not break the reload visuals.

### Ammunition logic

A reload must transfer ammunition correctly between reserve ammunition and the magazine.

For a 30-round magazine:

- Reloading with 12 rounds remaining and sufficient reserve should restore the magazine to 30.
- Only the correct amount should be removed from reserve.
- Reloading with insufficient reserve should load only the available ammunition.
- Reloading with a full magazine must not consume reserve ammunition.
- Ammunition must never be duplicated or lost because of an interrupted animation.

Use the project's existing rules for retaining ammunition during tactical reloads. Do not introduce ammunition waste unintentionally.

### Reload interruptions

Handle interruptions reliably, including:

- Switching weapons.
- Player death.
- Starting a healing action.
- Entering other mutually exclusive weapon states.
- Repeatedly pressing the reload button.
- Reloading while moving or crouching.

A completed reload must update the ammunition exactly once.

An interrupted reload must leave the weapon in a valid state.

Do not allow the game to become stuck in a permanent reloading state.

---

## 2. DOORBELL SHOTGUN — COMPLETE RELOAD REWORK

**This is a major priority.**

The Doorbell Shotgun has a six-round capacity.

Unlike the SMG, the shotgun must use an individually animated shell-loading system rather than behaving like a detachable-magazine weapon.

### Required behavior

- Represent shells being added one at a time.
- Give each shell addition its own readable, intentionally simple pixel-art animation.
- Synchronize the ammunition counter with each successfully completed shell insertion.
- Use a short, consistent interval between shells.
- Stop automatically when the shotgun reaches its full six-round capacity.
- Stop when reserve ammunition is exhausted.
- Avoid an unnecessarily long or frustrating reload sequence.

Example:

If the shotgun is at **2/6**, reloading should progressively increase its ammunition:

`2/6 → 3/6 → 4/6 → 5/6 → 6/6`

Not instantaneously jump to 6/6 while displaying an unrelated animation.

### Interruptible reloading

The player must be able to interrupt an incomplete shotgun reload when gameplay permits.

For example:

- The shotgun contains 2 shells.
- The player begins reloading.
- Two additional shells are loaded.
- The player interrupts reloading to engage an enemy.
- The shotgun now contains 4 usable shells.

The two successfully loaded shells must remain available.

The player must not lose this progress or be forced to restart the entire loading sequence.

### State management

Implement reliable transitions between:

- Idle.
- Firing.
- Reloading.
- Reload interrupted.
- Empty.
- Weapon switching.
- Player death.

Avoid overlapping firing and reloading animations.

Avoid inserting multiple shells because of duplicate animation events.

Ensure each completed shell-loading event increases loaded ammunition by exactly one and decreases reserve ammunition by exactly one.

### Visual requirements

Match the existing AKUMU 2 art direction:

- Chunky, simple pixel art.
- Rough, handmade animations.
- Clear shell movement.
- No unnecessarily polished animation assets.
- No complex character skeletal animation overhaul.
- Minimal runtime overhead.

Reuse existing systems where sensible.

---

## 3. SHARED RELOAD SYSTEM RELIABILITY

Inspect the existing reload code for the player's weapons.

Determine whether the SMG and shotgun bugs arise from shared reload logic, weapon-specific logic, animation event timing, or conflicting state transitions.

Fix problems at the appropriate level.

Avoid writing unnecessary duplicate reload systems.

Support the following distinct behaviors:

**Magazine-fed weapons:** One coherent magazine reload sequence with a controlled ammunition transfer.

**Individually loaded weapons:** Incremental ammunition transfer with interruption and resume support.

Do not force both weapon categories into identical behavior.

All weapon ammunition displays must accurately reflect the actual gameplay state.

Prevent:

- Infinite ammunition exploits.
- Double ammunition transfers.
- Negative reserve ammunition.
- Reloading beyond magazine capacity.
- Reload animation softlocks.
- Permanent inability to fire after reloading.
- Reload animations continuing after weapon switching.
- Detached magazine sprites remaining in the scene.
- Reload input being processed repeatedly during an existing reload.

Keep the reload logic modular, readable, and maintainable.

---

## 4. EQUIPMENT ECONOMY & WEAPON PRICING

Continue the previously planned Sprint 0034 economy improvements.

The economy should eventually create meaningful financial pressure without making powerful weapons practically unobtainable.

### Weapon pricing

Review the prices of existing weapons against their actual combat effectiveness.

Consider:

- Damage.
- Magazine capacity.
- Rate of fire.
- Reload speed.
- Ammunition efficiency.
- Range and accuracy.
- General usefulness during Level 1.

Maintain meaningful differences between inexpensive, mid-tier, and premium weapons.

Cheap weapons should remain useful rather than becoming deliberately worthless.

Expensive weapons should feel desirable without automatically outperforming everything in every situation.

### Cost per enemy defeated

Evaluate the approximate ammunition expenditure required to defeat ordinary enemies using different weapons.

Identify obvious economy inconsistencies, such as a weapon being prohibitively expensive to use despite having no meaningful advantages.

Make sensible provisional adjustments where the imbalance is clear.

**Important:** Do not attempt final campaign-wide economic balancing yet.

Level 1, the enemy roster, and the weapon systems are still evolving.

Establish a consistent and adjustable pricing foundation. Leave precise long-term economy tuning for later development.

---

## 5. AMMUNITION ECONOMY & RESERVE CAPACITY

Review ammunition purchasing, starting ammunition, and reserve capacity.

### Basic weapon ammunition

Ensure basic weapons receive enough ammunition to be practical during introductory gameplay.

The existing goal is approximately **five magazines' worth of usable starting ammunition** for basic weapons, including the loaded magazine where applicable.

Avoid forcing players to buy ammunition immediately just to make ordinary weapons functional.

### Different ammunition types

Preserve distinct ammunition categories where appropriate.

Avoid accidental cross-weapon ammunition sharing between incompatible weapon types.

Ensure purchases increase the correct ammunition reserve.

Different ammunition types should have sensible pricing relative to weapon effectiveness.

Premium ammunition should involve a meaningful trade-off, not simply make ordinary ammunition obsolete.

### Reserve capacity

Respect existing armor and equipment capacity rules.

Heavy armor must not reduce ammunition reserves to absurdly small quantities.

Preserve the established principle that armor can affect reload speed and carrying capacity but must not slow normal player movement.

### Prevent economic exploits

Verify that buying, equipping, switching, reloading, and returning to the shop cannot duplicate ammunition or produce unexpected free refills.

---

## 6. SHOP & PROGRESSION INTEGRATION

Review the existing equipment shop and its interaction with the ammunition system.

Fix obvious inconsistencies involving:

- Weapon ownership.
- Equipped weapon selection.
- Ammunition purchases.
- Reserve ammunition after returning from missions.
- Ammunition capacity.
- Weapon price displays.
- Currency deductions.
- Saved inventory data.

The shop should display correct weapon capacities and current ammunition-related information.

Ensure the basic pistol retains its intended **15-round magazine** and does not visually acquire an extended magazine.

Do not accidentally modify existing weapon identities while repairing the shop.

### Development testing currency

Preserve the temporary **10,000-credit development/testing allowance**, including support for existing saves.

It must be possible to test expensive weapons without repeatedly grinding for money.

Make the testing allowance idempotent so it cannot accidentally stack every time the shop opens.

Keep development/testing currency separate from the eventual release economy.

---

## 7. MOBILE CONTROLS & GAMEPLAY RESPONSIVENESS

Reloading must remain comfortable on Android.

Verify that:

- The reload button responds reliably.
- Holding the button does not create repeated reload commands.
- The player can continue moving during permitted reload actions.
- Reloading does not disable the movement joystick.
- Firing inputs behave correctly when the weapon becomes usable.
- Crouching and cover interactions remain functional.
- Weapon switching does not corrupt reload state.
- The ammunition HUD updates immediately after ammunition changes.

Do not add extra mandatory buttons for shotgun shell loading.

Use the existing reload control.

Preserve the player's current combat mobility.

---

## 8. ANIMATION AND VISUAL CONSISTENCY

Maintain the existing intentionally handmade AKUMU 2 graphics style.

Do not introduce polished 3D-looking magazine or shotgun animations.

Keep reload animations clear and slightly exaggerated but efficient.

Check that weapon sprites, hands, magazines, and shell visuals stay consistent when the character changes facing direction or animation state.

Use straightforward sprite animation or existing weapon visual components rather than expensive runtime effects.

Clean up temporary objects correctly.

If suitable reload sounds already exist, synchronize them appropriately. Do not expand this sprint into a full audio production overhaul.

---

## 9. REGRESSION TEST MATRIX

Perform actual gameplay tests rather than relying exclusively on code inspection.

### SMG tests

- [ ] Reload from empty.
- [ ] Reload from partially filled magazine.
- [ ] Reload with insufficient reserve.
- [ ] Attempt reload with full magazine.
- [ ] Reload while running.
- [ ] Reload while crouching.
- [ ] Reload while changing facing direction.
- [ ] Switch weapons during reload.
- [ ] Repeatedly press reload.
- [ ] Trigger death during reload.
- [ ] Confirm correct magazine visuals.
- [ ] Confirm no floating or duplicated magazines.
- [ ] Confirm ammunition is transferred exactly once.

### Shotgun tests

- [ ] Reload from 0/6 to 6/6.
- [ ] Reload from 2/6 to 6/6.
- [ ] Reload with only one reserve shell.
- [ ] Interrupt after loading one shell.
- [ ] Interrupt after loading multiple shells.
- [ ] Resume reloading after interruption.
- [ ] Fire immediately after a permitted interruption.
- [ ] Switch weapons while loading.
- [ ] Test empty reserve.
- [ ] Verify each shell increases loaded ammunition by one.
- [ ] Verify reserve decreases correctly.
- [ ] Confirm no animation softlock.

### Shop and economy tests

- [ ] Purchase a weapon.
- [ ] Purchase ammunition.
- [ ] Switch between weapon categories.
- [ ] Return to the shop after gameplay.
- [ ] Verify currency deductions.
- [ ] Verify reserve ammunition persistence.
- [ ] Verify save/load behavior.
- [ ] Verify testing credits on existing saves.
- [ ] Confirm correct starting ammunition for basic weapons.
- [ ] Confirm the basic pistol retains 15-round capacity.

### General regressions

- [ ] Other weapons still fire and reload correctly.
- [ ] Duct Tape healing remains functional.
- [ ] Weapon switching works correctly.
- [ ] Armor penalties work as intended.
- [ ] Enemy combat behavior remains unchanged.
- [ ] No new Unity console errors.
- [ ] No additional softlocks.
- [ ] Android performance remains stable.

---

## 10. IMPLEMENTATION RULES

1. Inspect relevant existing scripts, prefabs, animation controllers, and inventory systems before modifying them.
2. Prefer fixing root causes rather than adding temporary workarounds.
3. Reuse existing weapon code where appropriate.
4. Avoid unnecessary refactoring unrelated systems.
5. Preserve current weapon damage and enemy combat tuning unless a confirmed bug requires correction.
6. Do not redesign Level 1.
7. Do not introduce unrelated gameplay mechanics.
8. Do not replace the existing HUD design.
9. Do not change the game's visual identity.
10. Keep the implementation performant for mid-range Android phones.

### Completion criteria

Sprint 0034 is complete when:

**Rattlebox SMG:** Magazine reloading looks correct, behaves correctly, and maintains accurate ammunition counts.

**Doorbell Shotgun:** Reloading visibly adds shells individually, allows appropriate interruption, and reliably preserves loaded ammunition.

**Economy:** Weapons and ammunition have coherent provisional prices, starting ammunition is sufficient, and shop transactions work correctly.

**Integration:** Existing saves, development credits, inventory, equipment, and gameplay remain functional.

**Quality:** No new reload-related softlocks, duplicated ammunition, floating magazines, or obvious performance regressions.

### Final Codex report

After implementation, provide:

1. A summary of changes made.
2. The relevant files/scripts modified.
3. The underlying causes identified for the SMG and shotgun issues.
4. The final reload behavior for each weapon.
5. Economy and ammunition pricing changes.
6. Tests performed and results.
7. Any remaining limitations or issues requiring manual Unity/Android verification.

**IMPLEMENT THE FULL SPRINT. DO NOT STOP AFTER WRITING AN IMPLEMENTATION PLAN.**

Prioritize reliable, enjoyable gameplay over elaborate animation, and preserve everything already working in AKUMU 2.