# AKUMU 2 — SPRINT 0027
## VERTICAL COMBAT, PHYSICS & GAMEPLAY SYSTEMS OVERHAUL

### MISSION

Implement the next major gameplay systems and reliability improvements for AKUMU 2 while preserving its responsive movement, chaotic cartoon physics, cowardly enemies, handmade pixel-art aesthetic, and Android performance.

Inspect existing implementations before changing anything. Extend functioning systems rather than rebuilding them unnecessarily.

**Core philosophy:** Fast player movement. Cowardly enemies. Many tracers, few hits. Everyone scared.

---

# PHASE 1 — ENEMY NAVIGATION RELIABILITY

Fix navigation softlocks without increasing enemy intelligence.

Investigate and repair enemies becoming stuck on crates, cover, invisible colliders, box edges, ladders, broken jumps, oscillating paths, and unreachable destinations.

Implement lightweight stuck detection and recovery.

Preserve archetype restrictions:

- **Larpers:** Remain on their assigned floors, though accidental falls are permitted. Maintain cowardly retreating and hiding.
- **Samurai:** Pursue aggressively across platforms using valid jumps and traversal opportunities.
- **Other archetypes:** Preserve existing traversal permissions.

Improve vertical awareness so significant nearby gunfire above or below an enemy can trigger an appropriate reaction.

Awareness must not give enemies perfect knowledge of the player's position or the ability to see through bulletproof platforms.

Do not increase reaction speed, accuracy, aggression, or tactical intelligence.

---

# PHASE 2 — VERTICAL COMBAT & FALL DAMAGE

### 2.1 Vertical combat support

Ensure existing systems support:

- Unobstructed combat between different elevations.
- Enemy awareness between nearby floors.
- Offset and intersecting platforms.
- Jumping across vertical gaps.
- Ladders and parachuting.
- Falling and landing on narrow platforms.

Do not restructure Level 1. Implement reusable systems for future, more vertical levels.

### 2.2 Fall damage — MANDATORY PARACHUTE EXCEPTION

Implement configurable fall damage.

**NORMAL FALLING:**

- Ordinary jumps and double jumps cause no fall damage.
- Small falls cause no fall damage.
- Larger falls progressively increase damage.
- Extreme falls without parachute protection can be fatal.
- Damage should account for actual falling conditions.
- Ladder dismounts and ordinary platform transitions must not accidentally trigger damage.

**PARACHUTING — ABSOLUTE RULE:**

If the player lands with the parachute deployed, **ALL FALL DAMAGE MUST BE COMPLETELY NULLIFIED.**

- Fall damage = **0 HP**, regardless of drop height.
- No partial mitigation.
- No percentage-based damage reduction.
- No damage threshold that overrides parachute protection.
- No separate landing-impact damage.
- No delayed fall damage after touching the ground.
- No damage caused by accumulated falling velocity when the parachute is active upon landing.

The parachute must provide **100% fall-damage protection**, even when descending from the highest possible platform in the game.

Implementation requirements:

1. Check whether the player's parachute is deployed at landing.
2. If deployed, bypass fall-damage calculations and application entirely.
3. Clear any pending fall-impact damage associated with that landing.
4. Preserve normal fall damage when landing without an active parachute.
5. Do not accidentally disable damage from enemy attacks, bullets, or other unrelated hazards.
6. Ensure parachute activation, landing, and deactivation do not cause one-frame damage bugs.

**ACCEPTANCE TESTS:**

- [ ] Small fall without parachute: no damage.
- [ ] Medium fall without parachute: appropriate damage.
- [ ] Extreme fall without parachute: potentially fatal.
- [ ] Medium fall with deployed parachute: zero damage.
- [ ] Extreme fall with deployed parachute: zero damage.
- [ ] Parachute deployed shortly before landing: zero damage if active at contact.
- [ ] No delayed damage after parachute landing.
- [ ] Ordinary combat damage still applies while parachuting.

**THIS IS A HARD GAMEPLAY RULE, NOT A BALANCING SUGGESTION.**

---

# PHASE 3 — PLAYER SLIDING

Complete the sliding mechanic.

**Running slide:**
- Preserve useful horizontal momentum.
- Lower player posture and collision profile appropriately.
- Support passage under suitable obstacles.
- Transition smoothly into normal movement.
- Prevent infinite momentum and stuck states.

**Double-jump landing slide:**
- Allow the player to request a slide while airborne.
- Buffer the request until landing.
- Immediately transition into sliding upon landing.
- Preserve the existing double-jump mechanic.

Ensure responsive Android controls and compatibility with crouching, jumping, and ladders.

---

# PHASE 4 — PLAYER GRENADE SYSTEM

Replace only the player's melee-weapon slot with a selectable throwable slot.

Do not modify enemy melee weapons or Samurai swords.

Implement:

**Fragmentation grenade:** Stylized area damage, friendly fire where appropriate, lightweight knockback, and cover interaction.

**Flashbang:** Short, comfortable visual feedback and temporary enemy sensory disruption, with no permanent AI changes.

**Smoke grenade:** Temporary sightline obstruction and concealment, using an Android-friendly implementation.

Required interaction:

1. Select grenade slot.
2. Choose grenade type.
3. Aim with the existing right joystick.
4. Display an approximate trajectory.
5. Release to throw.
6. Activate the corresponding effect.

Do not implement instant one-button throwing.

Use finite inventories and integrate grenade purchases with the existing shop.

---

# PHASE 5 — ENVIRONMENTAL LANDMINES

Create reusable fictional environmental hazards.

Requirements:

- Recognizable crude pixel-art presentation.
- Configurable activation area and damage.
- Stylized explosion and knockback.
- Appropriate interactions with player and enemies.
- No repeated activation bugs.
- No expensive physics simulation.

Do not automatically place these hazards throughout Level 1. Test them in a controlled scene.

---

# PHASE 6 — RAGDOLL & BODY-REGION DAMAGE

Improve existing modular ragdolls without rebuilding their architecture.

Fix:

- Unstable joints and twitching.
- Incorrect visual segment positions.
- Ragdolls trapping the player.
- Bodies remaining unnecessarily expensive after settling.
- Incompatible animation states after death.

The player must pass through ragdolls without becoming blocked.

Implement lightweight cumulative damage tracking for existing modular body regions, such as head, torso, arms, and legs.

Allow configurable thresholds for non-graphic cartoon-style segment separation. Thresholds depend on accumulated regional damage rather than weapon-specific triggers.

Preserve existing damage balancing where possible.

---

# PHASE 7 — DUCT-TAPE HEALING

Preserve the existing hold-to-heal mechanic:

- One tape reserve point restores one HP.
- Starting reserve: 200 HP-equivalent points.
- Healing stops at full health.
- Healing can be interrupted.
- Player remains vulnerable.
- Tape replenishes through existing shop and loot systems.

Add or improve the intentionally crude duct-tape wrapping animation and recognizable adhesive peeling sound.

**The player cannot fire while actively healing.**

Restore normal weapon functionality immediately when healing finishes or is interrupted.

---

# PHASE 8 — FAINTANYL REVIVAL SYSTEM

Repair the fictional in-game consumable's revival functionality.

Rules:

- Maximum two consumables carried.
- Purchased only in the shop's Medical section.
- Never obtained from random level loot.
- No unlimited stacking.

When an available revival is triggered:

1. Consume exactly one item.
2. Revive the player in place.
3. Restore the player to a functioning state.
4. Grant exactly two seconds of invulnerability.
5. Restore ordinary vulnerability afterward.

Prevent repeated death/revival loops.

After both items are consumed, the next fatal event ends the attempt.

Add two small HUD inventory indicators that disappear individually upon consumption.

Do not redesign the whole HUD.

---

# PHASE 9 — OFFICER MINIBOSS VISUAL & TELEGRAPH OVERHAUL

Fix the Officer's glitching warning laser and distinguish his Desert Eagle shots from regular gunfire.

### 9.1 Actual bullet tracers

- Use a unique, conspicuous tracer color, preferably vivid magenta.
- Make tracers approximately **2× thicker** than ordinary pistol tracers.
- Preserve normal tracer movement and fading.
- Retain ordinary muzzle flashes, gunshot sounds, impact effects, and damage.
- Apply these changes only to the Officer's actual Desert Eagle shots.

### 9.2 Telegraph timing

Set the Officer's visible pre-shot telegraph duration to **exactly 0.5 seconds**.

Sequence:

1. Acquire target.
2. Display warning laser.
3. Maintain warning for 0.5 seconds.
4. Fire the actual shot.
5. Remove warning laser cleanly.

Do not add another hidden aiming delay after the warning.

Preserve the existing seven-round magazine, high damage, cowardly behavior, and evasive movement.

### 9.3 Fix the warning laser

Prefer reusing the existing tracer renderer with a persistent, visual-only configuration.

The warning laser must:

- Originate at the weapon muzzle.
- Extend along the correct aiming direction.
- Reach an appropriate obstacle or maximum visual range.
- Remain continuous and stable.
- Avoid flickering, shrinking, snapping, or stale positions.
- Update correctly during movement.
- Disappear immediately after firing, death, or cancellation.

**WARNING LASER MUST HAVE:**

- No muzzle flash.
- No impact effect.
- No sparks or bullet decals.
- No damage.
- No knockback.
- No ammunition consumption.
- No physical projectile.
- No gunshot sound.

Separate the telegraph renderer from actual projectile damage logic.

The warning laser and damaging bullet tracer must be visually distinguishable.

Prevent duplicate lasers, lingering beams, overlapping timers, and mismatches between the warning direction and subsequent shot.

Test with the Officer standing, retreating, turning, aiming at different elevations, being interrupted, and dying.

---

# PHASE 10 — REGRESSION PROTECTION

Preserve and test all established systems:

**Movement:** Running, jumping, double jumping, crouching, ladders, parachutes, camera smoothing, mobile joysticks.

**Combat:** Glock, carbine, reload animations, ammunition counts, bullets, suppression, muzzle flashes, fading tracers.

**Enemies:** Larper cowardice, reaction delays, floor restrictions, Samurai pursuit, fireworks expert, Officer behavior, retreating, cover, and spawn spacing.

**Equipment:** Armor, helmets, healing, ammunition, shop purchases, and existing reload penalties.

**Game flow:** Main Menu → Level Select → Shop → Mission → Results → Level Select.

**Fall-damage regression:** No system added in this sprint may cause the player to take fall damage when landing with a deployed parachute.

Preserve the existing Level 1 layout, encounter pacing, and enemy personalities.

---

# PHASE 11 — ANDROID PERFORMANCE

Prefer event-driven checks and existing reusable components.

Avoid unnecessary per-frame allocations, expensive particle simulations, uncontrolled physics accumulation, and creating new warning-beam objects each frame.

Pool temporary effects where appropriate.

Allow settled ragdolls to become computationally inexpensive.

Keep all controls readable and responsive on Android.

---

# EXECUTION STRATEGY

Before coding:

1. Inspect the repository, scenes, and existing implementations.
2. Identify which mechanics already work.
3. Establish available build and test baselines.

During coding:

1. Implement the numbered phases sequentially.
2. Make actual repository changes rather than merely proposing them.
3. Keep new gameplay parameters configurable.
4. Run available tests after each major subsystem.
5. Repair regressions before continuing.
6. Do not introduce unrelated mechanics or broad redesigns.
7. Preserve project conventions and reuse functional architecture.

Isolate any unfinished phase rather than destabilizing existing gameplay.

---

# FINAL ACCEPTANCE CRITERIA

- [ ] Navigation reliability improved without unwanted AI intelligence changes.
- [ ] Vertical combat functions correctly.
- [ ] Ordinary fall damage implemented.
- [ ] **Landing with a deployed parachute ALWAYS results in exactly zero fall damage.**
- [ ] Running and double-jump landing slides work.
- [ ] All three grenade types function.
- [ ] Reusable environmental landmine system functions.
- [ ] Ragdoll reliability and cumulative regional damage implemented.
- [ ] Duct-tape healing animation, sound, and firing restrictions work.
- [ ] Faintanyl revival works with two-item capacity and two-second protection.
- [ ] Officer actual tracers have distinctive color and increased thickness.
- [ ] Officer warning laser works without glitches or unintended effects.
- [ ] Officer telegraph lasts exactly 0.5 seconds.
- [ ] Existing combat and enemy personalities remain intact.
- [ ] Level 1 pacing remains intact.
- [ ] Available automated tests and builds pass.
- [ ] Android-specific behavior is verified wherever testing is possible.

Report implementation status, bugs fixed, files changed, tests executed, unfinished work, regression risks, and a manual Android playtest checklist.

Do not claim Android playtesting succeeded unless it was actually performed.

**Final sprint acceptance requires my own playtesting.**

---

## FINAL DIRECTIVE

Strengthen the foundations of AKUMU 2 for more vertical, varied, chaotic levels without sacrificing reliability or performance.

**Fast player movement. Cowardly enemies. Chaotic cartoon physics. Many tracers, few hits. Everyone scared.**

**And parachuting means ZERO fall damage. No exceptions for height.**

Implement Sprint 0027 now.