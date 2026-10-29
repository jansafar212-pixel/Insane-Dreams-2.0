# AKUMU 2 — SPRINT 0027.1
## Officer Accuracy, Ladder Fixes, Weapon Art & Store Cleanup

**Objective:** Address specific problems discovered during the latest playtest. Preserve everything that already works. This is a focused hotfix, not a gameplay overhaul.

### 1. Officer Accuracy — Make Him Dangerous

The Officer miniboss currently has extremely poor accuracy. His Desert Eagle shots miss far too often, undermining his role as a dangerous, high-damage enemy.

**Required changes:**
- Significantly improve the Officer's aiming accuracy.
- Reduce excessive random bullet spread.
- Improve tracking of the player's position during the aiming phase.
- Ensure the actual shot reasonably follows the direction indicated by the telegraphing laser.
- Preserve the existing 0.5-second laser telegraph.
- Preserve the Officer's distinctive thicker, differently colored bullet tracers.
- Maintain his cowardly, evasive personality and existing damage values.
- Do not give him perfect accuracy, instant reactions, or unnatural tracking through obstacles.

**Desired result:** The Officer is frightening because his shots are dangerous and reasonably accurate, not because he has enormous health or fires constantly. A moving player should still be able to evade him.

### 2. Desert Eagle — Visual Corrections

The Officer's Desert Eagle sprite needs two specific corrections.

**Grip:**
- Change the pistol grip/handle from brown to black.
- Maintain the established crude, handmade pixel-art style.

**Magazine:**
- The magazine currently protrudes unnaturally from the bottom of the pistol grip.
- Correct its positioning and dimensions so it fits inside the handle when inserted.
- Check the idle, firing, and reload animations for magazine positioning errors.
- Do not change weapon functionality or reload timing.

### 3. Ladder System — Restore Automatic Dismounting

The revised level has introduced a regression in ladder traversal.

**Problem:** Previously, the player automatically stepped off a ladder after climbing to its upper endpoint. Now the player can remain stuck in the climbing state instead of transitioning onto the platform.

**Required changes:**
- Restore automatic dismounting when reaching the top of any ladder.
- Automatically transition the character from climbing to standing on the upper platform.
- Ensure the player does not have to jump or make awkward joystick movements to exit the ladder.
- Correctly align the player's position with the platform's walkable surface.
- Prevent the character from immediately reattaching to the ladder after dismounting.
- Prevent clipping, oscillation, freezing, or accidental falls during the transition.
- Verify that all ladders in the changed level have valid entry and exit points.
- Preserve normal downward climbing and existing airborne ladder interactions.
- Ensure the fix works reliably with Android touchscreen joystick controls.

**Important:** Investigate why the level modification broke functionality that previously worked. Fix the underlying regression rather than introducing a fragile workaround.

### 4. Pause Button — Narrower

The top-center pause button is still too wide.

**Required changes:**
- Reduce its horizontal width noticeably.
- Keep the current vertical height approximately unchanged.
- Preserve the existing pause symbol and top-center placement.
- Keep an adequately sized invisible touchscreen hit area if necessary for mobile usability.
- Do not change pause-menu functionality.

### 5. Store — Medical Category Cleanup

The store's medical items are currently organized incorrectly.

**Required structure:**

**MEDICAL**
1. **Duct Tape**
2. **Faintanyl**

**Required changes:**
- Create or correctly configure one store category named `Medical`.
- Both items must appear under that category.
- Neither item should have its own separate top-level category.
- Remove any duplicate entries from other categories.
- Rename the duct-tape store item to exactly `Duct Tape`.
- Keep the second item's name as `Faintanyl`.
- Preserve both items' existing prices, functionality, purchase behavior, inventory integration, and healing effects.
- Ensure purchasing either item actually awards the corresponding resource.
- Keep their descriptions unless the category restructuring requires minor adjustments.

Do not redesign the entire store interface.

### 6. Limb Detachment — DO NOT CHANGE

The newly implemented limb-detachment system is entertaining and must be preserved.

**Hard requirements:**
- Do not remove limb detachment.
- Do not reduce its frequency.
- Do not tone down the existing exaggerated ragdoll behavior.
- Preserve the current physics and cumulative body-part damage mechanics.
- Do not rewrite the ragdoll system as part of this hotfix.

The exaggerated physics are intentional and now part of the game's personality.

### 7. Validation and Regression Testing

Before completing the sprint, verify:

- [ ] Officer shots are noticeably more accurate.
- [ ] Officer remains evasive, telegraphed, and counterable.
- [ ] Desert Eagle grip is black.
- [ ] Magazine no longer protrudes from the grip.
- [ ] Player automatically exits ladders at the top.
- [ ] Ladder transitions work throughout the revised level.
- [ ] Downward ladder movement still works.
- [ ] Pause button is narrower without losing touchscreen usability.
- [ ] Medical category contains exactly Duct Tape and Faintanyl.
- [ ] Both medical items remain purchasable and functional.
- [ ] No medical items appear in unrelated store categories.
- [ ] Existing limb detachment and ragdoll physics remain unchanged.

### Implementation Rules

1. Inspect existing scripts, prefabs, scene objects, and store definitions before editing.
2. Prefer small, targeted modifications to existing systems.
3. Do not introduce parallel systems, duplicate managers, or unnecessary abstractions.
4. Do not silently change unrelated gameplay variables.
5. Do not alter the level layout or enemy spawn distribution.
6. Verify Unity compilation and relevant regression tests.
7. Report modified files, fixes completed, and anything that still requires manual playtesting.

**Success condition:** The Officer becomes a legitimate threat, the Desert Eagle looks correct, ladders regain their previous smooth operation, the pause button is slimmer, the store is correctly organized, and our ridiculous limb-detachment physics survive untouched.