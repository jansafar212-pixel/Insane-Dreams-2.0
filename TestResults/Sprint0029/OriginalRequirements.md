# AKUMU 2 — SPRINT 0029
## OPERATION: THE BUNKER IS REAL

**Project:** AKUMU 2 — 黒橋 GAMES  
**Type:** Major implementation, gameplay reliability and UI polish sprint  
**Starting point:** Existing project following Sprint 0028

# 0. AUTONOMOUS EXECUTION MANDATE

This is a substantial development sprint.

You are expected to work independently and complete the assigned workstreams without repeatedly requesting user input.

**DO NOT STOP AFTER EACH FEATURE TO ASK WHETHER TO CONTINUE.**

Inspect the project, identify dependencies, implement the changes, debug problems, test the results and proceed to the next task.

You have permission to make sensible implementation decisions consistent with the established design.

### Mandatory development rules

1. Inspect existing scenes, scripts, resources and project architecture before changing anything.
2. Reuse working systems rather than building unnecessary replacements.
3. Identify which agenda features are already implemented. Fix or extend them rather than duplicating them.
4. Complete related dependencies as part of each feature.
5. Do not replace functioning artwork unnecessarily.
6. Do not introduce placeholders disguised as completed features.
7. Do not add unnecessary plugins or third-party dependencies.
8. Preserve Android/mobile performance.
9. Test every major modification.
10. Continue through the sprint without pausing for routine approvals.

If you encounter a bug caused by your changes, fix it rather than leaving it for a future hotfix.

Do not declare a feature finished simply because its script compiles. Verify that it actually works in the relevant scene.

**The objective is a playable, substantially improved build — not merely a collection of code changes.**

---

# 1. PRESERVE THE NEW ENVIRONMENTAL GRAPHICS

Sprint 0028's background/environmental artwork was successful.

The backgrounds now look significantly more alive.

**DO NOT REPLACE, REGENERATE OR REDESIGN THEM.**

Preserve the existing colors, terrain details, scenery and established handmade pixel-art style.

The objective is to improve functionality and consistency around these graphics, not start another graphical overhaul.

Small visual additions required for interactive gameplay objects are permitted, especially the Officer's bunker.

---

# 2. CRITICAL FIX — THE BUNKER MUST BE REAL

## Current problem

The Officer's bunker was apparently implemented as background artwork.

Visually it resembles a bunker, but it does not function as physical, bulletproof defensive geometry.

This fundamentally defeats its intended purpose.

**Fix the underlying implementation. Do not merely draw another bunker.**

## 2.1 Real geometry

Convert or reconstruct the Officer's bunker as an actual interactive structure.

Use the project's existing 2D collision and environment systems.

Appropriate implementations may involve:

- StaticBody2D and CollisionShape2D.
- TileMap collision where suitable.
- Existing bulletproof-cover components.
- Foreground sprites with matching collision geometry.

Choose the approach that best fits the existing project.

The bunker must have physical walls and defensive surfaces capable of blocking incoming projectiles.

Bullet collisions must use the correct physics layers and masks.

Test both hitscan and physical-projectile systems if the game uses both.

Do not create a visually solid wall that bullets can pass straight through.

## 2.2 Foreground integration

The bunker must exist in the actual playable world.

Its relevant physical elements must be rendered at appropriate foreground depths.

Ensure that:

- Characters appear behind or in front of the correct bunker elements.
- Walls visually correspond to their collision shapes.
- The player cannot accidentally pass through solid bunker walls.
- Projectiles visibly strike the bunker instead of continuing through it.
- Decorative parts do not introduce invisible collision barriers.

Keep the already-approved handmade pixel-art aesthetic.

The structure should resemble a small improvised defensive position with earth, rough wood and makeshift cover.

It should be slightly ridiculous, but convincingly functional.

## 2.3 Officer occupancy and combat

The Officer must be able to physically occupy and navigate his bunker.

His defensive sequence should resemble:

**Hide → observe → expose himself → telegraph → fire → retreat → reposition.**

He must not be able to fire through bunker walls.

When concealed, his position must genuinely interrupt incoming direct gunfire.

When peeking, he must expose an appropriate targetable portion of his character.

The bunker cannot turn him into an invulnerable enemy.

Make sure the player can approach, flank or otherwise engage the Officer using normal gameplay.

No elaborate scripted boss puzzle is required.

## 2.4 Officer behavior improvements

Preserve the Officer's existing personality: an evasive, terrified commander with a disproportionately dangerous Desert Eagle.

Specifically:

- Improve his aim compared with ordinary Larpers.
- Preserve his high individual shot damage.
- Preserve the distinctive, thicker, differently colored projectile tracer.
- Maintain the approximately 0.5-second warning laser.
- Ensure the laser is stable and properly aligned.
- Avoid instant, perfect target tracking.
- Increase his willingness to retreat into available cover.
- Allow him to peek and fire when safe opportunities arise.
- Prevent indefinite hiding or unreachable retreat positions.

Keep his black Desert Eagle grip and ensure its magazine does not protrude incorrectly from the handle.

Do not make him artificially tanky.

## 2.5 Bunker-specific acceptance tests

Verify all of the following:

- Player bullets are stopped by solid bunker walls.
- Officer bullets are stopped by solid bunker walls.
- The Officer fires only when his weapon has a valid unobstructed path.
- The warning laser respects bunker occlusion.
- The Officer can hide and emerge successfully.
- The Officer can be defeated without exploits.
- The player can traverse the area without getting stuck.
- Collision remains correct after reloading the level and on the intended mobile build.

A decorative-only bunker is an automatic failure of Sprint 0029.

---

# 3. ENEMY PATHFINDING AND LADDER RELIABILITY

Our existing AI navigation has recurring problems.

This sprint should perform a meaningful reliability improvement, not introduce an entirely new tactical AI framework.

## 3.1 General navigation repair

Investigate and fix:

- Enemies getting caught on box edges.
- Characters becoming stuck on invisible colliders.
- Oscillation between conflicting movement targets.
- Enemies repeatedly attempting impossible jumps.
- Broken ladder entry and exit.
- Navigation targets located inside inaccessible geometry.
- Characters unable to recover from temporary obstructions.

Prefer lightweight recovery logic, reachable destinations and sensible movement-state handling.

Avoid expensive continuous full-level path calculations.

## 3.2 Enemy archetype restrictions

Preserve enemy personalities and existing traversal rules.

**Larpers:** Stay on their assigned floor unless normal physics causes an accidental fall. They should not suddenly become skilled vertical pursuers.

**Samurai:** Must aggressively pursue the player across reachable platforms and use available movement mechanics appropriately. Correct jumping and platform-transition problems.

**Officer:** Must navigate between useful bunker positions and nearby escape routes without getting trapped.

**Explosives Expert:** Must reliably reach legal attack positions without abandoning attacks because of minor navigation failures.

Do not transform cowardly enemies into elite tactical operatives.

Their movements can remain clumsy, panicked and imperfect.

The distinction is important:

**Unintentionally broken pathfinding is bad. Deliberately cowardly AI is good.**

## 3.3 Player ladder regression

Following the recent level changes, ladder-top exits have become unreliable.

Fix this properly.

Once the player reaches the top of a ladder, the character should automatically dismount onto the connected platform when physically valid.

Do not require awkward manual jumps to exit.

Ensure this works with both desktop and mobile controls.

Avoid oscillation between climbing and walking states.

---

# 4. GLOBAL GUNFIRE AWARENESS AND REACTION TIMES

This is an overdue gameplay improvement.

Enemies currently sometimes ignore extremely obvious nearby combat simply because the player is on another platform or outside direct visual detection.

Meanwhile, some enemies react unnaturally quickly upon seeing the player.

Fix both issues.

## 4.1 Gunfire awareness

Implement or improve a lightweight shared sound-event awareness mechanism.

Relevant events should include:

- Player gunfire.
- Nearby enemy gunfire.
- Significant explosions.
- Other major combat noises where appropriate.

An enemy on a platform above the player should be capable of hearing the player firing below them.

Awareness should not depend entirely on sharing the same floor or having a direct line of sight.

Use sensible event distances and existing enemy update mechanisms.

Avoid turning every sound into a full-level alert.

## 4.2 Awareness is not omniscience

Hearing gunfire must not reveal the player's exact position automatically.

An enemy that hears combat may:

- Become suspicious.
- Turn toward the approximate source.
- Seek nearby cover.
- Investigate within its permitted movement area.
- Become more cautious or fearful.
- Prepare to engage if the player subsequently becomes visible.

It must not instantly gain perfect aim through walls.

Preserve the established rule that normal enemies do not initiate attacks on the player while off-camera. Future specialist snipers and mortars are exceptions, not part of this sprint.

## 4.3 Reaction delays

Implement configurable first-detection reaction timing.

Suggested starting values:

**Larper: approximately 0.7 seconds** before beginning an attack after initial visual acquisition.

**Samurai: approximately 0.2–0.35 seconds**, noticeably faster than a Larper.

Officer reaction timing should preserve his separate warning-laser mechanic.

Avoid firing immediately on the very first detection frame.

Existing alert states and re-acquisition behavior should be handled sensibly; do not restart long delays on every tiny line-of-sight interruption.

## 4.4 Preserve suppression behavior

The current cowardly suppression system is an important defining feature.

Do not weaken it.

Nearby shots should still provoke readable fear responses, including crouching, retreating and inaccurate panic fire.

Preserve existing per-enemy panic-escape limitations.

The goal is improved awareness and believable reactions, not more accurate or aggressive Larpers.

---

# 5. MEDICAL SHOP AND REVIVAL SYSTEM

Implement the previously agreed medical-store structure and complete any unfinished supporting mechanics.

## 5.1 Shop organization

Retain the existing four-category store structure.

The medical category must be named exactly:

**Medical**

It should contain exactly two item types:

**Duct Tape**

Description: "There is nothing duct tape can't fix."

**Faintanyl**

Description: "Relax, it's not addictive. It's just habit forming."

Use these names consistently throughout the shop and HUD.

Remove Faintanyl from unrelated store categories.

Do not introduce duplicate purchase buttons or duplicate inventories.

## 5.2 Duct Tape

Preserve the existing hold-to-heal mechanic.

Continue using the established healing reserve and 1:1 consumption behavior.

Preserve the existing duct-tape animation and associated audio if functional.

Duct Tape must remain distinct from the revival item.

## 5.3 Faintanyl revival mechanic

Complete or repair the previously planned fictional automatic-revival system.

Rules:

- Maximum two purchased revival charges.
- Available exclusively through the Medical shop.
- No in-level syringe pickups.
- No checkpoint refills.
- A charge is consumed on a qualifying player death and revives the player.
- Revival grants exactly two seconds of temporary invulnerability.
- Once all revival charges are exhausted, subsequent death follows the normal death/game-over flow.

Display up to two small, readable syringe symbols representing remaining charges.

Remove or deactivate the appropriate symbol when a charge is consumed.

Ensure invulnerability cannot persist indefinitely or be accidentally bypassed by another damage pathway.

Keep the visual effect stylized and non-realistic, consistent with the game's intentionally crude cartoon presentation.

Do not introduce detailed real-world injection procedures.

## 5.4 Shop regression tests

Verify that:

- Both products can be purchased independently.
- Prices and wallet balances update correctly.
- Purchase limits are enforced.
- Existing purchases survive the intended menu-to-mission transition.
- Item use correctly decreases inventory.
- Checkpoints do not replenish revival charges.
- The store remains functional when reopened.
- Mobile input can reliably select both products.

---

# 6. REPLACE PLAYER MELEE WITH GRENADES

Implement the previously agreed dedicated player throwable slot.

This applies exclusively to the player.

**Do not remove or change Samurai melee attacks.**

## 6.1 Grenade types

Provide three fictional gameplay grenade types:

**Fragmentation:** Area damage with appropriate visual feedback.

**Flashbang:** Temporarily disrupts affected enemies' awareness and combat behavior.

**Smoke:** Creates a temporary obscuring cloud that meaningfully interferes with enemy visual detection and targeting.

Use the project's existing damage, AI and visual-effect systems.

Avoid building an entirely separate combat framework.

## 6.2 Selection and aiming

Grenades must not behave like an instant, arbitrary one-button explosion.

The intended interaction is:

1. Select the throwable slot.
2. Choose an available grenade type.
3. Aim using the existing aiming-control system.
4. Display a simple approximate throw trajectory.
5. Release or confirm the throw using a consistent mobile-friendly input.

Integrate this with the existing right-side aiming joystick without breaking normal firearms.

Avoid introducing unnecessarily complicated gestures or extra buttons.

## 6.3 Inventory and interactions

Grenades should have proper ammunition/inventory accounting.

Prevent throwing when inventory is empty.

Make grenade availability and currently selected type understandable from the HUD.

Integrate purchases through an appropriate existing shop category without unnecessarily changing the four-category structure.

Ensure smoke and flash effects interact meaningfully with enemy awareness instead of being purely decorative.

Keep particle counts and simulation costs appropriate for Android.

## 6.4 Regression

Confirm that selecting, aiming, throwing and returning to firearms all work reliably.

Do not break weapon switching, reloading, shooting or the existing aiming joystick.

---

# 7. BODY ARMOR AND HELMET BALANCING

Complete the previously agreed light/heavy equipment trade-offs using the existing armor implementation where possible.

## 7.1 Equipment categories

Support:

- Light body armor.
- Heavy body armor.
- Light helmet.
- Heavy helmet.

Use sensible relative protection values consistent with existing damage balance.

Do not create absurd levels of damage absorption.

## 7.2 Mobility must remain intact

**Armor must not reduce movement speed.**

Preserve:

- Running speed.
- Jumping.
- Double jumping.
- Sliding.
- Ladder movement.
- Parachute behavior.
- Aiming responsiveness.

Fast and responsive movement is a foundational AKUMU 2 mechanic.

## 7.3 Equipment trade-offs

Use modest inventory-capacity adjustments and reload-time penalties.

Light equipment should introduce relatively small trade-offs.

Heavy equipment should offer stronger ballistic protection but cause a noticeable, moderate increase in firearm reload duration.

Heavy equipment may also moderately reduce spare-ammunition and grenade capacity.

Do not stack every equipment penalty until the player has almost no ammunition.

Enforce sensible minimum reserve capacities.

Keep penalties straightforward, readable and easy to tune.

## 7.4 Damage categories

Ballistic armor primarily protects against firearms.

Existing Samurai melee attacks must bypass ballistic body armor.

Do not make a ballistic vest inexplicably negate sword attacks.

Reuse existing regional hitboxes where possible rather than implementing expensive per-pixel collision.

Preserve the game's current working ragdoll limb-detachment system.

Do not undertake a complete ragdoll rewrite.

---

# 8. CHUNKY HANDMADE UI AND HUD POLISH

Now that the environmental art has a defined visual identity, begin applying that identity to the interface.

The current **AKUMU 2** lettering in the main menu is the primary style reference.

The entire interface should look like it belongs to the same game.

## Visual direction

Favor:

- Chunky pixelated lettering.
- Blocky buttons.
- Limited color palettes.
- Hard edges.
- Simple handmade icons.
- Slightly imperfect old-school pixel-art styling.
- Strong visual readability on mobile screens.

Avoid:

- Generic glossy mobile-game interfaces.
- Smooth modern UI widgets.
- Overly elaborate animated menus.
- Tiny unreadable text.
- Excessive decorative detail.

**Make the UI look intentionally handmade, not intentionally incompetent.**

## Specific fixes

### Main menu and store

Bring obvious visual inconsistencies closer to the established AKUMU 2 style.

Do not rewrite functional menu navigation.

Ensure all buttons retain proper mobile touch areas.

### Ammunition HUD

Display magazine ammunition in compact:

**Current / Capacity**

Example: `16/27`

Correctly update when firing, reloading or switching weapons.

Keep reserve ammunition information distinct where already provided.

### Pause control

Keep the pause button toward the upper center.

Use a compact pause icon rather than an unnecessarily wide text button.

Reduce visual width without making the actual touch target too small.

### Medical HUD

Display Duct Tape and available revival charges clearly.

Keep them distinct and understandable without adding excessive clutter.

### Grenade interface

Introduce the new throwable controls in the same chunky graphical style.

Integrate them naturally with the existing weapon interface.

### Existing controls

Preserve comfortable joystick dimensions and positions.

Do not shrink important mobile touch targets merely to make the interface visually compact.

Keep the health bar, mission objectives and other UI elements readable without unnecessarily dominating the screen.

---

# 9. MANDATORY REGRESSION AND PERFORMANCE PASS

This sprint touches several interconnected systems, so regression testing is not optional.

### Movement

Verify walking, crouching, crouch-walking, double jumping, sliding where implemented, ladders and parachuting.

**Landing with the parachute deployed must produce exactly zero fall damage regardless of height.**

Normal unprotected fall damage remains unchanged.

### Combat

Verify firearms, reloads, magazine counts, projectile collisions, armor interactions, throwables and damage calculation.

### AI

Verify Larpers, Samurai, the Czech Explosives Expert and the Officer.

Check detection, reaction timing, suppression, legal movement, cover behavior and death handling.

### Physics

Preserve ragdolls and the existing stylized limb-detachment feature.

Check that new bunker geometry does not create collision deadlocks.

### Menus and progression

Verify the complete flow:

**Main Menu → Level Select → Shop → Mission → Results → Level Select**

Verify checkpoint functionality, level completion, wallet handling and inventory transitions.

### Platforms

Test the intended desktop configuration and Android-compatible build workflow.

Check for unnecessary allocations, expensive repeated navigation calculations, particle overload or avoidable mobile performance regressions.

If device-level testing is unavailable, perform the available automated/editor tests and explicitly report what still requires real-device verification.

---

# 10. EXECUTION ORDER AND COMPLETION POLICY

Implement in the following order:

**Priority A — Critical gameplay integrity**
1. Physical bunker and Officer integration.
2. Pathfinding, ladders and collision reliability.
3. Gunfire awareness and reaction timing.

**Priority B — Previously agreed game systems**
4. Medical store and revival.
5. Grenade slot and throwable mechanics.
6. Armor and helmet trade-offs.

**Priority C — Presentation**
7. Chunky UI/HUD integration and visual cleanup.
8. Final regression and optimization.

Do not stop after Priority A.

Proceed through all assigned workstreams autonomously.

Fix integration problems as they appear rather than accumulating broken dependencies.

Prioritize functional completeness over adding unnecessary polish.

If technical constraints prevent completing a particular item, document the exact blocker and continue with independent tasks rather than abandoning the entire sprint.

**Never report an unfinished or untested feature as complete.**

# 11. FINAL DELIVERABLES

At the end of the sprint, provide:

1. Clear list of completed features.
2. Important files and scenes modified.
3. Explanation of how the bunker now physically blocks projectiles.
4. Summary of enemy AI/navigation improvements.
5. Summary of completed medical, throwable and armor mechanics.
6. Overview of UI changes.
7. Tests executed and results.
8. Remaining defects and incomplete requirements, if any.

Do not submit a lengthy explanation of every tiny implementation decision.

Deliver a concise engineering report focused on what actually works.

## FINAL DESIGN PRINCIPLE

AKUMU 2 is a fast, chaotic, deliberately imperfect 2D action game.

The enemies are supposed to be cowardly, clumsy, funny and occasionally dangerous.

We are making the systems more reliable, not making every enemy a professional soldier.

The graphics should look like a talented amateur made them with love, not like a polished commercial asset pack.

Preserve the existing successful backgrounds and mechanics.

**Make the bunker real. Make the AI reliable. Finish the overdue systems. Make the interface chunky. Do the work autonomously.**