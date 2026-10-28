# AKUMU 2 — SPRINT 0035
## LEVEL 1 COMBAT, AI & GAMEPLAY POLISH

**Project:** AKUMU 2 — Unity 2D URP, Android + PC

**Priority:** HIGH

**Sprint philosophy:** Polish existing systems. Do not introduce unnecessary new features.

---

## 1. CRITICAL — PRESERVE ANDROID PERFORMANCE

The previous optimization sprint was an enormous success. The game now runs significantly more smoothly on the actual Android phone than at any previous point in development.

THIS PERFORMANCE IMPROVEMENT MUST BE PRESERVED.

Before making changes:

- Inspect the recent optimization changes.
- Identify any performance-sensitive systems.
- Preserve object pooling, caching and other established optimizations.
- Avoid unnecessary Update() calculations.
- Avoid introducing garbage collection spikes.
- Avoid expensive physics queries where simpler existing solutions work.
- Maintain consistent frame pacing and responsive mobile controls.

Target a stable 60 FPS on capable hardware, without compromising gameplay quality.

Do not sacrifice enemy responsiveness or gameplay correctness merely to reduce CPU usage.

If actual Android profiling is unavailable, explicitly report that limitation rather than claiming the 60 FPS target has been verified.

## 2. LEVEL 1 ENCOUNTER PACING

Keep Level 1 primarily horizontal and beginner-friendly.

Use the existing roster:

- Approximately 10 Larpers.
- 1 Samurai.
- 1 Czech fireworks expert.
- 1 Officer miniboss at the end.

The level should introduce mechanics gradually.

Encounter sequence:

**Opening:** One isolated Larper to establish basic shooting.

**Early combat:** Small, separated Larper encounters introducing movement, cover and reloading.

**Middle:** Slightly more demanding firefights, followed by the aggressive Samurai encounter.

**Later:** Introduce the Czech fireworks expert with enough space for the player to understand and respond to his attacks.

**Final:** Officer miniboss in his concrete bunker, with the surrounding battlefield arranged to support his defensive personality.

Rules:

- Avoid three Larpers engaging simultaneously at the beginning.
- Ordinary encounters should generally have only one or two active attackers.
- Preserve reasonable breathing room between encounters.
- Avoid unavoidable crossfires.
- No ordinary enemy should shoot the player from outside the visible camera.
- Do not redesign the level into a complicated vertical obstacle course.
- Do not expand the enemy roster.

Keep the current art direction and overall level structure.

## 3. ENEMY REACTION TIMES

Inspect and refine enemy reaction timing.

Larpers:
- Target approximately 0.7 seconds between first visual detection and initiating an attack.
- Retain their cowardly personalities.
- Preserve existing cover-seeking, suppression and panic behavior.
- Do not make every Larper react with identical timing.
- Avoid instantaneous firing after detection.

Samurai:
- React noticeably faster than Larpers.
- Aggressively approach the player.
- Use the existing platform traversal and jumping mechanics.
- Do not remain passive when the player enters engagement range.

Officer:
- Preserve the established 0.5-second aiming telegraph.
- Ensure the laser telegraph is smooth, visually stable and aligned with the intended shot.
- Maintain his distinctive thicker, differently colored bullet tracer.
- Keep him accurate and dangerous without making his attacks unavoidable.

Do not introduce artificial intelligence sophistication that contradicts the enemies' intentionally foolish personalities.

## 4. OFFICER BUNKER FIX

Audit the Officer's concrete bunker.

The bunker must function as ACTUAL cover, not merely background decoration.

Requirements:

- Correct collision geometry.
- Projectiles interact properly with concrete.
- No bullets passing through solid cover unless explicitly intended.
- Clear shooting slit or opening.
- Officer can hide, peek, fire and retreat.
- Officer can leave cover when appropriate.
- No getting permanently stuck inside the bunker.
- No shooting through his own cover.
- No invisible walls preventing player progression.

Keep the Officer cowardly and evasive.

He should rely on accurate, highly damaging individual pistol shots rather than overwhelming ammunition volume.

Do not turn him into a stationary turret or an enormous bullet sponge.

## 5. PATHFINDING AND NAVIGATION RELIABILITY

Fix existing enemy navigation issues without making enemies unrealistically intelligent.

Check:

- Enemies getting stuck against boxes.
- Enemies oscillating near obstacles.
- Incorrect ladder handling.
- Enemies falling off platforms unexpectedly.
- Samurai failing to pursue across reachable platforms.
- NPCs remaining trapped in cover positions.
- Enemies repeatedly attempting impossible paths.

Implement lightweight recovery logic where necessary.

Respect archetype-specific movement restrictions.

Pathfinding improvements should address BUGS, not transform Larpers into elite soldiers.

## 6. SUPPRESSION AND ENEMY AWARENESS

Preserve the established suppression/fear system.

Verify:

- Near misses generate appropriate reactions.
- Repeated nearby shots accumulate suppression.
- Suppression gradually decays.
- Larpers retreat or panic when sufficiently overwhelmed.
- Occasional collapse or extreme fear reactions remain rare.
- Enemies do not instantly forget dangerous events.

Fix awareness across vertical platforms.

If the player fires repeatedly below an enemy, that enemy should be capable of hearing and reacting to the gunfire even without direct visual contact.

Hearing gunfire should not automatically grant perfect knowledge of the player's position.

Use approximate sound-origin investigation or existing suspicion mechanisms.

Respect the rule that ordinary enemies cannot attack the player from offscreen.

## 7. PLAYER COMBAT AND MOVEMENT REGRESSION PASS

Audit the following existing systems:

- Running and jumping.
- Double jumping.
- Crouching and crouch-walking.
- Sliding, if already implemented.
- Ladder entry, climbing and automatic dismounting.
- Parachute deployment and descent.
- Weapon aiming and firing.
- Reloading and ammunition display.
- Grenade selection, aiming and throwing.
- Duct Tape healing.
- Revival injections.
- Armor protection.
- Cover and blind firing.

CRITICAL PARACHUTE RULE:

Landing with an actively deployed parachute must always result in ZERO fall damage, regardless of drop height.

Without the deployed parachute, existing fall-damage rules apply.

Prevent accidental regressions in mobile joystick behavior.

## 8. DEVELOPMENT MONEY — MANDATORY

**THIS IS AN EXPLICIT IMPLEMENTATION REQUIREMENT, NOT JUST A REQUEST TO PRESERVE EXISTING CURRENCY.**

Give the player **10,000 in-game credits** for weapon and equipment playtesting.

Requirements:

- New development/playtesting saves must start with 10,000 credits.
- Existing development saves must receive a one-time top-up to at least 10,000 credits when this sprint is applied.
- The money must actually appear in the player's usable shop balance.
- The player must be able to purchase weapons, ammunition, armor and consumables with this money immediately.
- Ensure the balance persists correctly between the shop and gameplay.
- Do not overwrite the player's existing balance if it already exceeds 10,000.
- Avoid granting duplicate currency every time a save is loaded.
- Make this behavior configurable through a development-only setting.
- Keep it disabled in release builds.

**IMPORTANT:** Do not just change a starting-money constant if existing save files override it. Inspect the persistence system and ensure the currently used playtesting save receives the credits too.

Acceptance test: Load an existing development save with insufficient money, verify that the shop balance becomes at least 10,000 credits, purchase a weapon, and verify that the remaining balance is correct after restarting the game.

This is TEMPORARY TESTING MONEY.

Do not rebalance the entire economy in this sprint.

The final economy will be balanced later, after weapon and gameplay testing is complete.

### Basic pistol correction

Verify the basic pistol:

- Uses its correct ordinary magazine artwork.
- Does NOT visually have an extended magazine.
- Holds exactly 15 rounds.
- Shows the correct ammunition values on the HUD.

Do not change the established sniper rifle hold-to-aim/release-to-fire mechanic or the Desert Eagle's normal firing controls.

## 9. MOBILE PRESENTATION AND FAIRNESS

Review the Android gameplay interface.

Ensure:

- Touch controls remain responsive.
- No buttons overlap essential gameplay elements.
- Health and ammo remain readable.
- Revival inventory displays correctly.
- The pause button works reliably.
- The weapon selection menu is usable.
- The blind-fire control appears only in the appropriate cover/crouch conditions.
- Enemy speech bubbles do not overwhelm the screen.
- A maximum of two dialogue bubbles appear simultaneously.

Avoid unnecessary UI redesigns.

Preserve the established chunky, handmade pixel-art aesthetic.

Prioritize visibility and gameplay clarity.

## 10. ACTUAL LEVEL PLAYABILITY TESTING

Test the complete Level 1 gameplay loop.

Scenarios:

1. Player begins with starter equipment.
2. Opening enemy encounters function correctly.
3. Player can advance through all sections.
4. Samurai engages and pursues correctly.
5. Fireworks expert attacks without becoming unfair.
6. Officer uses bunker cover properly.
7. Player defeats the Officer.
8. Level completion triggers correctly.
9. Rewards and collected wallets register.
10. Player can return to the shop and replay the level.

Additional failure testing:

- Player dies before reaching the Officer.
- Player uses a revival injection.
- Player runs out of ammunition.
- Player attempts to heal during combat.
- Multiple enemies are defeated simultaneously.
- Enemies become suppressed while navigating obstacles.
- Player leaves and resumes the game.
- Player interacts with ladders during intense combat.

Check for softlocks, incorrect state transitions, duplicate rewards and crashes.

Where possible, add automated regression tests for reproducible bugs.

## 11. STRICT SCOPE CONTROL

DO NOT:

- Add new enemies.
- Add new weapons.
- Create additional levels.
- Rewrite the entire enemy AI architecture.
- Replace working optimization systems.
- Rework the entire economy.
- Introduce new graphical styles.
- Increase particle complexity unnecessarily.
- Add expensive procedural systems.

Prioritize the existing codebase and repair the smallest relevant system.

If a requested feature is already working correctly, leave it alone.

## 12. DEFINITION OF DONE

The sprint succeeds when:

- Level 1 remains completable from start to finish.
- Enemy reactions feel fair and distinct.
- Samurai attacks aggressively.
- Officer bunker works as real cover.
- Enemy navigation has no known major softlocks.
- Suppression and vertical awareness behave correctly.
- Existing player movement and combat mechanics still work.
- Mobile UI remains responsive.
- The player has at least 10,000 usable development credits on the existing test save before purchases.
- Development money works without duplicating currency on repeated loads.
- All fixes avoid introducing noticeable performance regressions.
- The recent Android performance improvements are preserved.

## FINAL CODEX INSTRUCTIONS

Inspect the current repository before implementation.

Do not blindly recreate existing features.

Prefer targeted improvements and regression fixes over broad rewrites.

After implementation, provide:

1. Files changed.
2. Bugs found and fixed.
3. Enemy AI adjustments.
4. Performance-sensitive changes.
5. Development-currency implementation and verification.
6. Automated tests and build results.
7. Systems that still require physical Android playtesting.
8. Remaining known issues.

**Remember AKUMU 2's identity: fast gameplay, cowardly and ridiculous enemies, intentionally handmade pixel art, satisfying combat and reliable performance.**

Make Level 1 feel complete, not merely functional.