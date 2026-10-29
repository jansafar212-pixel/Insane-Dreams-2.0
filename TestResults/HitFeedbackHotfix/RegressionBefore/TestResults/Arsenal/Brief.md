# AKUMU 2 — SPRINT 0033
## ARSENAL EXPANSION & GUNPLAY OVERHAUL

### PRIMARY OBJECTIVE

Overhaul, complete, and polish the player's weapon arsenal so that every weapon is functional, distinctive, enjoyable, and practical to use on Android.

This sprint follows:
- Sprint 0031 — Adaptive Combat Camera.
- Sprint 0032 — Android Performance Optimization.

The objective is to establish a stable, satisfying weapons foundation before introducing serious purchasing costs, ammunition prices, and economy balancing in Sprint 0034.

**Core philosophy: Every weapon needs a reason to exist.**

Do not simply add weapons that differ only in damage and magazine capacity.

Weapons should feel noticeably different through their firing rhythm, accuracy, effective range, mobility compatibility, visual feedback, and overall tactical role.

Preserve the fast, chaotic, side-scrolling identity of AKUMU 2.

---

## 1. AUDIT THE EXISTING WEAPON SYSTEM

Before implementing anything, inspect all existing player weapons and related scripts.

Identify:

- Existing weapon classes and available models.
- Weapon configuration and statistics.
- Fire modes and cooldowns.
- Reloading implementation.
- Magazine and reserve ammunition handling.
- Mobile firing controls.
- Weapon-switching behavior.
- Hitscan and damage systems.
- Tracers and impact effects.
- Muzzle flashes and audio.
- HUD integration.
- Shop and weapon ownership systems.

Reuse functioning systems wherever possible.

Prefer a shared, configurable weapon framework over creating separate duplicated scripts for every gun.

Do not rewrite the entire combat system unnecessarily.

---

## 2. COMPLETE THE CORE PLAYER ARSENAL

Ensure that the following weapon categories are represented by distinct, functional player weapons.

### Pistols

**Standard pistol**
- Reliable general-purpose backup.
- Comfortable firing cadence.
- Moderate damage.
- Reasonable accuracy.
- Simple, dependable controls.

**Desert Eagle**
- Very powerful individual shots.
- Distinctly slower firing cadence.
- Strong visual and audio feedback.
- Limited ammunition capacity.
- Excellent satisfaction per successful hit.

IMPORTANT: The player's Desert Eagle uses NORMAL mobile firing controls.

It must NOT use the sniper's hold-to-aim/release-to-fire mechanic or its laser sight.

Its slow rate of fire provides the deliberate handling.

### Submachine Guns

Provide a fast-firing, mobile-friendly SMG.

Identity:
- Rapid firing.
- Particularly effective at close range.
- Less effective at longer distances.
- Excellent for aggressive movement.
- Fast, satisfying combat rhythm.

Do not make it universally superior to pistols and assault rifles.

### Assault Rifles / Carbines

Provide a versatile automatic weapon.

Identity:
- Balanced damage and accuracy.
- Useful across ordinary combat distances.
- Dependable general-purpose primary weapon.
- Distinct from the faster but less accurate SMG.

Preserve existing carbine functionality if already implemented.

### Shotguns

Provide a close-range shotgun.

Identity:
- Multiple pellets per shot.
- Strong close-range impact.
- Clear spread visualization where useful.
- Distinct firing cadence.
- Lower effectiveness at longer ranges.

Implement pellet behavior using the existing hitscan framework where practical.

Avoid spawning expensive physical projectiles unnecessarily.

### Light Machine Guns

Provide an LMG with a distinct sustained-fire identity.

Identity:
- Large ammunition capacity.
- Sustained automatic fire.
- Strong suppression potential.
- Longer reload duration.
- Less precise sustained fire than a normal rifle.

Do NOT make the player physically slower while carrying it.

AKUMU 2's fast movement is a core mechanic.

Balance the weapon through firing characteristics, reloads, ammunition capacity, and accuracy rather than movement penalties.

### Dedicated Sniper Rifle

Provide one dedicated precision sniper rifle with its own unique mobile control scheme.

Do not add an ordinary bolt-action infantry rifle merely to inflate the number of weapons.

A semi-automatic marksman rifle is optional only if it introduces a genuinely different gameplay role and can be implemented without unnecessary complexity.

Prioritize finishing the essential categories rather than adding redundant weapons.

---

## 3. SPECIAL SNIPER RIFLE CONTROL SYSTEM

The sniper must use the previously established control concept.

**HOLD TO AIM — RELEASE TO FIRE.**

On Android:

1. Player touches and holds the aiming joystick.
2. A visible sniper aiming laser appears.
3. Moving the joystick adjusts the rifle's aim.
4. The player can take time to line up the shot.
5. Releasing the joystick fires exactly ONE shot.
6. The sniper automatically enters its recovery/bolt-cycle animation.
7. The rifle becomes ready for another shot after the recovery completes.

There must be NO additional manual bolt-cycle button.

There must be NO separate sniper fire button.

The system must work comfortably with the existing two-joystick layout.

### Important details

- The laser must accurately indicate the intended firing direction.
- It is a PLAYER AIMING AID, not an enemy telegraph.
- It must be clearly distinguishable from the Officer miniboss's telegraphing laser.
- It must not create expensive per-frame physics queries.
- Keep the laser visually appropriate for the game's crude handmade pixel-art style.
- The sniper must fire once per valid release, never multiple times accidentally.
- Weapon switching, death, pause, UI interruption, and invalid aim states must never cause accidental shots.
- Provide a clear cancel-aim gesture, such as returning the joystick to its central dead zone before release.
- Canceling must NOT fire the rifle.
- The recovery animation should be automatic and brief enough to remain satisfying.
- Avoid overly long delays that make the sniper unpleasant.

Integrate the sniper with the adaptive camera so that aiming receives framing priority.

Do not implement new camera-follow logic.

---

## 4. WEAPON PERSONALITY & FEEDBACK

Every weapon must feel mechanically different.

Review and tune:

- Fire rate.
- Individual shot damage.
- Accuracy.
- Shot spread.
- Range-dependent effectiveness where appropriate.
- Reload duration.
- Magazine capacity.
- Visual feedback.
- Audio feedback.
- Tracer behavior.
- Camera compatibility.

Examples of desired distinctions:

- SMG: fast, frantic close-range fire.
- Carbine: dependable and versatile.
- Shotgun: powerful close-range bursts.
- LMG: sustained suppression.
- Desert Eagle: slower but hard-hitting precision.
- Sniper: deliberate, high-impact single shots.

Avoid arbitrary stat differences that players cannot actually perceive.

Preserve normal movement speed across all weapons.

Do not introduce complicated realistic weapon mechanics that are unsuitable for a fast mobile game.

---

## 5. MOBILE INPUT CONSISTENCY

Ensure every weapon functions with the established mobile control system.

Ordinary weapons must retain the existing firing behavior, with their fire mode respected.

The sniper is the explicit exception.

Test:

- Firing while moving.
- Firing while jumping.
- Firing while retreating.
- Firing from crouch.
- Blind firing from eligible cover.
- Reloading.
- Switching weapons.
- Shooting while the camera changes framing.
- Touch input interruption.
- Simultaneous movement and aiming joystick use.

Do not create input conflicts between weapons, grenades, healing, and ordinary combat.

The controls must remain immediate and comfortable on the Samsung Galaxy A56.

---

## 6. RELOADING & AMMUNITION RELIABILITY

Audit and repair ammunition handling across the arsenal.

Requirements:

- Firing consumes the correct ammunition.
- Reloading updates magazine and reserve values correctly.
- Empty magazines behave correctly.
- Switching weapons cannot duplicate ammunition.
- Interrupting reloads cannot generate free ammunition.
- Reload timing respects the weapon's configuration.
- Armor-related reload penalties continue to function.
- No floating or incorrectly positioned magazines.
- HUD ammunition displays the correct current/capacity value.

Preserve existing ammunition pickup systems.

Avoid introducing unlimited ammunition into the production game by accident.

---

## 7. TEMPORARY ARSENAL TESTING MODE

For this sprint, economy balancing must NOT obstruct weapon testing.

Provide a clearly isolated developer-only testing mode where:

- All player weapons are available.
- Ammunition can be freely replenished.
- Every weapon can be tested immediately.
- Weapon switching and reloading can be evaluated without shop restrictions.

This testing mode must not become part of normal player progression.

Do not permanently overwrite existing ownership, shop, or inventory systems.

Do not implement final weapon prices yet.

Do not balance weapon acquisition around fictional currency at this stage.

The purpose is to answer:

**Does every weapon actually feel good to use?**

Only after that question is answered should we address affordability.

---

## 8. ENEMY INTERACTION & SUPPRESSION

Make sure weapons interact correctly with existing enemy systems.

Test their effects on:

- Larpers.
- Samurai.
- Fireworks expert.
- Officer miniboss.
- Armored enemies where implemented.
- Existing suppression and fear systems.
- Existing body-region damage and ragdoll physics.

High-volume fire should generate appropriate suppression behavior using existing systems.

Powerful individual shots should produce satisfying reactions.

Do not create new enemy AI behavior for this sprint.

Do not change enemy personality or reaction-speed rules.

Prevent damage from being accidentally applied multiple times by a single hitscan event.

Ensure shotgun pellets interact correctly with existing hitboxes and damage attribution.

Preserve the existing stylized, non-graphic ragdoll presentation.

---

## 9. VISUAL CONSISTENCY

All newly implemented weapon sprites and effects must match the established AKUMU 2 graphics bible.

Permanent style:

**Retro visuals, modern mechanics, handmade personality.**

Weapons should look like creations from a talented, enthusiastic amateur pixel artist, not a polished commercial asset pack.

Requirements:

- Chunky pixels.
- Limited palettes.
- Simple readable silhouettes.
- Consistent pixel scale.
- Thick, slightly imperfect outlines.
- Handmade visual character.
- No blurry textures.
- No excessive gradients.
- No inconsistent high-definition models.

The Desert Eagle must preserve its black grip and correctly seated magazine.

Avoid disrupting previously approved weapon artwork.

---

## 10. PERFORMANCE BUDGET

This sprint follows a dedicated Android performance overhaul.

Do not undo those gains.

Requirements:

- Reuse existing hitscan architecture.
- Pool commonly spawned effects where appropriate.
- Avoid unnecessary per-frame allocations.
- Avoid expensive sniper laser calculations.
- Keep firing effects efficient.
- Prevent effect objects from accumulating.
- Avoid expensive audio or particle initialization during sustained firing.

Test particularly demanding scenarios involving:

- Continuous SMG fire.
- Sustained LMG fire.
- Shotgun pellet impacts.
- Multiple enemies firing.
- Multiple ragdolls and effects simultaneously.

Do not reduce important weapon feedback simply to improve theoretical performance.

Profile actual bottlenecks.

---

## 11. WEAPON BALANCING — GAMEPLAY ONLY

Establish provisional gameplay balance.

The aim is NOT perfect final balance.

Focus on:

- Clear strengths and weaknesses.
- Meaningful player choices.
- Consistent hit registration.
- Reasonable reloads.
- Satisfying firing cadence.
- Distinct weapon roles.
- Fair interactions with existing enemies.

Avoid making any weapon universally superior.

Do not enforce unrealistic inventory scarcity.

Do not introduce weapon purchase prices, final ammunition costs, or major shop economy changes.

Those belong in Sprint 0034.

Use serialized or data-driven weapon configurations so that future numerical adjustments are easy.

---

## 12. REGRESSION TESTING

Test every weapon on desktop and Android where possible.

Minimum test cases:

1. Equip weapon.
2. Fire weapon.
3. Empty magazine.
4. Reload.
5. Interrupt reload.
6. Switch weapons while firing.
7. Switch weapons while reloading.
8. Fire while moving.
9. Fire while jumping.
10. Fire from crouch.
11. Check eligible blind-fire behavior.
12. Test ammunition pickups.
13. Verify ammo HUD.
14. Test damage against each available enemy category.
15. Test effects and audio.
16. Test sustained combat performance.
17. Test player death and revival while a weapon is equipped.
18. Test sniper aim, cancel, release, and automatic recovery.

Do not claim Android testing has been completed unless it has actually been performed.

---

## 13. STRICT SCOPE CONTROL

Do NOT:

- Redesign Level 1.
- Change enemy placement.
- Rebalance the shop economy.
- Add final weapon prices.
- Add unnecessary bolt-action infantry rifles.
- Give the Desert Eagle sniper controls.
- Slow player movement based on weapon weight.
- Replace the combat or camera architecture unnecessarily.
- Redesign enemy AI.
- Introduce realistic ballistics simulation.
- Implement unnecessary inventory complexity.
- Replace the established pixel-art aesthetic.
- Sacrifice Android performance.

Make focused, maintainable changes.

---

## 14. ACCEPTANCE CRITERIA

Sprint 0033 is complete when:

- All intended core weapon categories are functional.
- Each weapon has a recognizable gameplay identity.
- Ordinary weapons work with existing mobile firing controls.
- The sniper uses hold-to-aim/release-to-fire correctly.
- The sniper's cancel mechanic prevents accidental shots.
- The Desert Eagle remains a conventional slow-firing weapon.
- Magazine and reload behavior are reliable.
- Existing player and enemy systems remain intact.
- Weapons have appropriate visual/audio feedback.
- Developer testing can access all weapons without economy restrictions.
- Normal progression remains unaffected.
- There is no significant Android performance regression.
- No weapon-related crashes, softlocks, or obvious duplication bugs remain.

### FINAL CODEX INSTRUCTIONS

Inspect the actual project before changing anything.

Identify which weapons and mechanics already exist and which are missing.

Implement the requested improvements directly in the Unity project.

Avoid duplicated systems and unnecessary rewrites.

At the end provide:

1. Complete player weapon roster.
2. Each weapon's gameplay identity and preliminary stats.
3. New and modified scripts.
4. Explanation of sniper controls.
5. Mobile input integration details.
6. Results of testing and regression checks.
7. Outstanding issues requiring Android playtesting.
8. Specific weapon balancing issues to revisit.

**Final design principle: AKUMU 2 needs weapons that feel different, not merely weapons that look different.**

MAKE THE ARSENAL FUN FIRST. BALANCE THE ECONOMY LATER.