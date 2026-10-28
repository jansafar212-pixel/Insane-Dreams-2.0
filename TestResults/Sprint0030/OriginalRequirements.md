# AKUMU 2 — SPRINT 0030
## OPERATION: THE BUNKER IS REAL

You are working on AKUMU 2, a 2D mobile platform shooter developed in Unity.

**MISSION:** Improve the first level's Officer miniboss encounter, enemy navigation and awareness, medical systems, throwables, armor, and remaining gameplay defects.

This is an IMPLEMENTATION sprint. Inspect the existing project, understand its architecture, and modify the actual code, scenes, prefabs, and assets.

Do not merely describe solutions. Implement them.

## GLOBAL DEVELOPMENT RULES

- Preserve functioning mechanics unless explicitly requested otherwise.
- Reuse existing components, scripts, and systems wherever possible.
- Avoid unnecessary architectural rewrites.
- Prioritize mobile performance and readable gameplay.
- No new external assets or dependencies unless essential.
- Preserve the game's charming, intentionally handmade pixel-art aesthetic.
- Avoid adding unnecessarily intelligent enemy combat tactics.
- Do not change established weapon balance without a specific requirement.
- No softlocks, unreachable objectives, broken checkpoints, or newly introduced regressions.
- If existing implementations differ from assumptions, inspect the actual code and adapt accordingly.

---

# TASK 1 — OFFICER MINIBOSS BUNKER

Create a small, functional, defensive bunker/pillbox for the Officer miniboss in Level 1.

### Architecture

- Construct a crude pixel-art concrete bunker matching the existing environment.
- Make it look like something a reasonably creative teenager could build using a free pixel-art editor.
- It must have actual foreground collision geometry.
- Concrete walls should block bullets.
- Include a clearly identifiable firing slit or opening.
- Ensure the player can understand which areas are protected and which are exposed.
- Integrate the structure naturally into the existing level.
- Do not obstruct required traversal routes.
- Keep the bunker compact; do not redesign the entire level around it.

### Officer behavior

The Officer should behave like a frightened commander hiding behind fortifications.

He must:

1. Remain protected when appropriate.
2. Peek out to aim and fire his Desert Eagle.
3. Retreat behind cover after firing.
4. Respond to incoming fire by hiding or repositioning.
5. Become more vulnerable when the player approaches his position.
6. Avoid remaining permanently hidden or invulnerable.
7. Avoid walking into walls, oscillating, or becoming stuck.

The Officer should be accurate, dangerous, and irritating to fight, but fundamentally cowardly.

**Important:** This must not become a stationary turret encounter. The Officer retains his ability to move, retreat, and react dynamically.

### Officer combat requirements

- Preserve the distinct thick, high-damage projectile tracer.
- Preserve the distinct tracer color compared with ordinary weapons.
- Keep the firing telegraph at approximately 0.5 seconds.
- Ensure the telegraphing laser is stable, properly aligned, and does not glitch.
- The telegraph must indicate the eventual firing direction clearly.
- Improve Officer accuracy if the current behavior still produces excessive misses.
- Retain the Desert Eagle's dangerous individual shots.
- Give the Officer approximately twice a normal Larper's effective durability through appropriate protection, not an enormous health pool.
- Fix the Desert Eagle visuals: black grip, correctly seated magazine, no protruding magazine.

### Acceptance criteria

- Officer can fire through the intended opening.
- Concrete reliably blocks bullets.
- Officer alternates between exposure and protection.
- Officer is defeatable without exploiting bugs.
- Player can approach and complete the encounter.
- No invisible barriers or collision problems.

---

# TASK 2 — ENEMY PATHFINDING RELIABILITY

Improve navigation reliability across the existing level.

Fix:

- Enemies getting stuck against boxes.
- Enemies oscillating between destinations.
- Enemies failing to step off or around obstacles.
- Broken ladder traversal where applicable.
- Enemies selecting unreachable destinations.
- Samurai becoming stuck during pursuit.
- Enemies failing to recover from invalid navigation states.

### Restrictions

**THIS IS NOT AN ENEMY INTELLIGENCE UPGRADE.**

Do not transform the Larpers into professional soldiers.

Larpers should remain cowardly, clumsy, and restricted to their intended platform/floor traversal rules.

Samurai should remain aggressive and capable of pursuing the player across permitted platforms.

Introduce lightweight recovery logic where necessary. Prefer reliable navigation over computationally expensive global pathfinding.

---

# TASK 3 — ENEMY AWARENESS AND REACTION TIMES

### Detection through sound

Currently, enemies may ignore obvious nearby gunfire when the player is standing on a different platform.

Correct this.

- Nearby gunfire should generate an awareness stimulus.
- Enemies should react even when the shooter is on a different vertical level, provided they are within an appropriate hearing range.
- Hearing a shot should NOT automatically provide perfect player coordinates.
- Differentiate hearing-based suspicion from confirmed visual detection.
- Enemies may investigate, take cover, become alarmed, or respond according to existing archetype behavior.
- Preserve existing line-of-sight rules.
- Do not allow ordinary enemies to attack the player from arbitrarily far offscreen.

### Reaction delays

Introduce or correct configurable detection-to-attack delays.

**Larper:** approximately 0.7 seconds after first visual confirmation before initiating fire.

**Samurai:** noticeably faster than a Larper, reflecting aggressive melee behavior.

**Officer:** appropriate reaction delay while preserving his distinctive telegraphed firing sequence.

Avoid stacking delays accidentally so enemies become unresponsive.

Preserve existing suppression, panic, retreat, and morale systems.

---

# TASK 4 — MEDICAL SYSTEM AND STORE FIXES

Audit the current medical inventory and shop logic.

### Store organization

Create or verify a dedicated **Medical** category containing:

- Duct tape
- Faintanyl (fictional satirical in-game item)

These should not appear in unrelated shop categories.

### Duct tape

Preserve the established behavior:

- Hold-to-heal interaction.
- Healing consumes the available duct-tape reserve.
- 1:1 resource-to-health conversion.
- No healing past maximum health.
- Correct HUD/inventory updates.
- Correct behavior when supplies run out.
- Preserve or repair the crude cartoon healing animation and sound feedback.

### Other medical mechanics

Inspect and repair any currently implemented Faintanyl or revival-related behavior that is broken.

Do not invent an entirely new revival system if the project does not already contain one.

Preserve existing death, checkpoint, respawn, and inventory persistence rules.

---

# TASK 5 — GRENADE AND THROWABLE SYSTEMS

Continue implementing or repairing the planned player throwable system.

The player's former melee-weapon slot should be the grenade/throwable slot. Do not change enemy melee attacks.

Support the established three fictional in-game grenade types:

- Fragmentation: damaging area effect.
- Flashbang: brief sensory disruption.
- Smoke: temporary visual obstruction.

### Controls

Grenades must NOT be automatically thrown by an instant single-button action.

The player must:

1. Select the throwable slot.
2. Select or retain the desired throwable type.
3. Aim using the existing mobile aiming controls.
4. Release/confirm the throw according to an intuitive consistent control scheme.

Integrate this into the existing mobile HUD without overcrowding it.

### Other requirements

- Ammo/supply crates must replenish the appropriate throwable inventory.
- Counts must update accurately.
- Fragmentation effects should have strong, readable cartoon VFX.
- Smoke must interact with visibility where supported.
- Flashbang effects should be temporary and recover correctly.
- No permanent AI blindness or broken combat states.
- Use pooled or lightweight effects where practical.

Preserve the existing bulletproof platform rule.

---

# TASK 6 — BODY ARMOR AND HELMETS

Finish or repair the established light/heavy armor and helmet systems.

### Equipment

- Light body armor
- Heavy body armor
- Light helmet
- Heavy helmet

### Design requirements

Armor must protect primarily against ballistic damage.

Melee attacks, including Samurai attacks, should bypass ballistic armor.

**Armor must NEVER slow ordinary player movement.**

Do not reduce:

- Running speed.
- Jump performance.
- Sliding.
- Ladder climbing.
- Movement responsiveness.
- Aiming responsiveness.

Instead, apply reasonable inventory/reload trade-offs.

Heavy armor can:

- Moderately increase weapon reload duration.
- Slightly reduce spare ammunition capacity.
- Slightly affect relevant carrying capacity.

Do not make heavy protection useless by reducing reserve ammunition to absurdly low levels.

Keep sensible minimum ammunition capacity.

### Technical requirements

- Avoid unnecessary per-frame allocations.
- Reuse existing damage and hitbox architecture.
- Fix armor sprites failing to disappear when equipment is removed or broken.
- Avoid item duplication, incorrect protection stacking, or inventory inconsistencies.

---

# TASK 7 — COMBAT PHYSICS AND REGRESSION HOTFIXES

Audit remaining known defects in the revised first level.

### Ragdoll and limb behavior

The modular ragdoll system and exaggerated cartoon limb detachment are intentional.

Preserve them.

However:

- Avoid excessive detachment caused by ordinary low-damage hits.
- Respect cumulative body-region damage thresholds.
- Prevent duplicate detachment processing.
- Keep physics stable.
- Do not introduce expensive body-part simulations.
- Preserve existing comedic ragdoll behavior.

### Ladders

- Fix ladders affected by the recent level changes.
- Automatically dismount the player at the top where appropriate.
- Prevent the character from remaining trapped in climbing state.
- Preserve responsive mobile ladder/jump controls.

### Additional regression checks

Verify:

- Parachuting completely nullifies fall damage when deployed during landing.
- Checkpoints activate and restore correctly.
- Level-completion triggers function.
- The shop correctly deducts currency and grants items.
- Ammunition HUD shows current magazine rounds/capacity.
- The player can pass through ragdolls as intended.
- Enemies do not bunch excessively at their starting positions.
- Existing movement, double jump, crouching, sliding, healing, and reloading still function.

---

# TASK 8 — VISUAL CONSISTENCY

Preserve the current handmade environment style.

Any new assets, particularly the Officer's bunker, should use:

- Chunky pixel shapes.
- Consistent pixel scale.
- Simple hard-edged shading.
- Limited palette.
- Slightly imperfect handmade details.
- Readable mobile-scale silhouettes.

The intended aesthetic is charming amateur pixel art, NOT deliberately terrible graphics and NOT polished professional asset-pack art.

Maintain consistency with the existing redesigned Level 1.

Do not launch the full menu/HUD graphics redesign in this sprint. That is reserved for later.

---

# IMPLEMENTATION WORKFLOW

1. Inspect the repository and identify the relevant scenes, prefabs, scripts, collision layers, animation systems, AI state machines, shop logic, and inventory systems.
2. Identify existing implementations before adding new ones.
3. Fix underlying causes rather than inserting brittle workarounds.
4. Implement the bunker and Officer behavior first.
5. Address navigation and hearing/reaction logic.
6. Repair medical/store systems.
7. Complete the relevant grenade and armor functionality.
8. Resolve remaining regression issues.
9. Run available automated checks, compile/build validation, and appropriate gameplay tests.
10. Verify the first level remains completable.

Keep changes modular and avoid unrelated refactoring.

Where tests cannot be executed in the environment, explicitly report that limitation rather than claiming success.

---

# FINAL ACCEPTANCE CRITERIA

The sprint is successful when:

- The Officer has a functional concrete bunker.
- The Officer peeks, fires accurately, hides, and remains defeatable.
- The telegraph and powerful tracer work correctly.
- Enemies navigate obstacles without common softlocks.
- Gunfire produces sensible cross-platform awareness.
- Enemy reactions are appropriately delayed.
- Medical purchases and healing function.
- Throwable selection, use, and replenishment function.
- Armor protection and reload/inventory trade-offs function.
- Previously working movement and combat mechanics remain intact.
- Level 1 is playable and completable.
- Performance remains appropriate for Android/mobile.

# FINAL REPORT

After implementing the changes, provide:

1. A concise list of completed changes.
2. The important files and prefabs modified.
3. Bugs discovered and corrected.
4. Tests actually run and their results.
5. Any unimplemented or partially completed requirements.
6. Any remaining risks requiring manual playtesting.

Do not describe unfinished requirements as completed.

**CORE DESIGN PHILOSOPHY:**

AKUMU 2 should feel mechanically competent but deliberately chaotic.

Its enemies are frightened idiots trying to survive an absurd situation. Navigation should work reliably, but their personalities, cowardice, inaccurate panic fire, and ridiculous behavior must remain.

The Officer should now be the ultimate expression of that philosophy: a dangerous, self-important armchair general hiding in his little concrete bunker because he is absolutely terrified of getting shot.

Make the systems work. Preserve the comedy. Keep the game fun.