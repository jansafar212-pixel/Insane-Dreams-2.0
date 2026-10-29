# AKUMU 2 — SPRINT 0037
## COMBAT IMPACT, LIVING RAGDOLLS & PLAYER DAMAGE FEEDBACK

**Type:** Major combat-polish sprint  
**Engine:** Unity 2D URP  
**Platforms:** Android + PC  
**Priority:** HIGH

### PRIMARY OBJECTIVE

Make gunfire feel significantly more powerful, satisfying, and physically convincing.

Currently, enemies are most visually interesting after they die because of the ragdoll system.

Extend that physical expressiveness into normal combat while enemies are still alive.

Also improve feedback when the player takes damage, and give the Desert Eagle and sniper rifle significantly thicker bullet tracers.

**This is a gameplay-feel sprint. Preserve existing damage values, weapon characteristics, enemy accuracy, mobility, and difficulty unless fixing an actual bug.**

---

## 1. DESERT EAGLE & SNIPER — THICKER TRACERS

The player Desert Eagle and sniper rifle should have noticeably thicker projectile tracers than ordinary firearms.

### Desert Eagle

- Increase tracer thickness to approximately 1.6× the normal pistol tracer width.
- Make its shots visually communicate substantial power.
- Preserve the existing tracer direction, speed, lifetime, and fade behavior.
- Keep the effect readable without making it excessively bright.
- Preserve its normal firing controls.

### Sniper rifle

- Increase tracer thickness to approximately 2.2× the normal pistol tracer width.
- Make the sniper's projectile feel especially powerful and distinctive.
- Preserve existing firing mechanics and accuracy.
- Preserve the special hold-to-aim, release-to-fire control scheme.
- Preserve its aiming laser as a separate effect.

### Important implementation details

Use configurable weapon-specific tracer width multipliers rather than hardcoding special cases throughout the tracer renderer.

Ensure the thicker tracers:

- Render correctly when firing diagonally.
- Remain aligned with the actual shot trajectory.
- Work when facing either direction.
- Do not clip incorrectly near the muzzle.
- Fade normally.
- Do not affect hit detection or damage.
- Do not create excessive overdraw on Android.

Preserve the Officer miniboss's existing distinct tracer color and thickness. Do not accidentally make all Desert Eagle tracers share his special enemy-only color.

**These are visual effects, not larger projectile hitboxes.**

---

## 2. LIVING ENEMY RAGDOLL REACTIONS — MAIN FEATURE

Make enemies physically respond to incoming bullets even when they survive the impact.

Use the existing hybrid active-ragdoll architecture.

Do not create an entirely separate ragdoll controller.

### Three reaction intensities

**A. Light impact**

Examples: individual pistol rounds and weaker SMG impacts.

Expected behavior:

- Small, brief physical flinch.
- Slight reaction in the impacted body region.
- Subtle upper-body movement.
- Enemy remains largely capable of fighting.
- Reaction blends smoothly back into the current animation.

Avoid dramatic knockback from every minor hit.

**B. Heavy impact**

Examples: Desert Eagle, close-range shotgun, and powerful rifle hits.

Expected behavior:

- Pronounced physical stagger.
- Clear temporary disruption of balance.
- Existing active-ragdoll joints respond naturally.
- Enemy may briefly step or lean to regain balance.
- Powerful hits should visibly feel different from weak hits.

The reaction must remain consistent with the game's cartoon physics.

**C. Accumulated impact**

Repeated impacts may temporarily overwhelm an enemy's balance.

Expected behavior:

- Impacts contribute to a separate balance-disruption value.
- This value decays naturally over time.
- Repeated sufficiently strong impacts may cause stumbling.
- An enemy may briefly fall and subsequently recover using existing ragdoll recovery behavior.
- After recovering, the enemy resumes appropriate combat behavior.

Use configurable thresholds and cooldowns.

Do not confuse balance disruption with HP damage or suppression/fear.

### Critical rule: prevent stun-locking

Automatic weapons must NOT be able to permanently disable an enemy by constantly triggering knockback.

Implement safeguards:

- Limit repeated heavy-reaction triggering.
- Give recovery appropriate priority.
- Prevent overlapping reactions from repeatedly resetting recovery.
- Use diminishing physical disruption during rapid successive hits where appropriate.
- Maintain the possibility of killing an enemy normally.
- Preserve appropriate reactions even when stagger cooldowns suppress a new full-body animation.

Enemies should feel physically vulnerable, but not helpless.

---

## 3. BODY-REGION-AWARE REACTIONS

Use the existing character body-region and hit-detection information where available.

Make reactions visually correspond to where a bullet lands.

Examples:

- Upper-body hit: torso flinches.
- Arm hit: arm and shoulder react.
- Leg hit: brief lower-body disturbance or imbalance.
- Strong impact: potentially affects the entire character's balance.

Apply lightweight directional impulses through existing active-ragdoll components.

Avoid expensive new per-pixel collision detection.

Do not implement anatomically realistic injuries or graphic effects.

Preserve existing hitboxes, HP calculations, and damage attribution.

### Impulse direction

Physical reaction should take the incoming shot direction into account.

A shot coming from the player's left should generally produce an appropriate response in the direction of impact.

Prevent absurd physics results caused by excessive impulses.

Use sensible maximum force and rotation limits.

---

## 4. WEAPON-SPECIFIC IMPACT PERSONALITY

Different firearms should feel different even when their damage values are similar.

Introduce configurable hit-reaction strength per weapon or weapon class.

Suggested starting behavior:

| Weapon | Reaction |
|---|---|
| Basic pistol | Small flinch |
| SMG | Repeated light reactions |
| Assault rifle | Moderate flinch |
| LMG | Consistent heavier impacts |
| Shotgun | Strong short-range reaction |
| Desert Eagle | Pronounced stagger |
| Sniper rifle | Very powerful single-hit reaction |

These are starting visual/physics guidelines, not instructions to rebalance damage.

For the shotgun, account for how many pellets connect without creating separate full-body reactions for every pellet.

For rapid-fire weapons, combine or throttle reactions appropriately.

### Enemy archetypes

Preserve existing differences between enemy types.

- Larpers should remain physically expressive and somewhat clumsy.
- Samurai should remain aggressive and recover appropriately.
- Officer should visibly react to powerful hits but retain his established durability and evasive behavior.
- Other enemy archetypes should integrate through the same configurable system.

Do not globally increase HP, damage, or enemy reaction speed.

---

## 5. PLAYER DAMAGE FEEDBACK — SECOND MAJOR FEATURE

Taking damage should feel more impactful.

The player needs to understand immediately that they have been hit, approximately where the threat came from, and whether the hit was serious.

Implement a combination of lightweight visual and physical feedback.

### A. Directional damage indicator

When taking directional damage, display a brief red indicator toward the approximate source.

Requirements:

- Support damage from left, right, above, and below.
- Keep the indicator readable on Android.
- Avoid covering the aiming area excessively.
- Fade smoothly.
- Avoid creating dozens of overlapping indicators from automatic fire.
- Use existing damage source data where possible.

If the damage has no meaningful direction, such as certain environmental hazards, use nondirectional feedback instead.

### B. Brief screen-edge damage effect

Introduce a subtle red screen-edge effect when taking damage.

- Small damage: brief, understated feedback.
- Heavy damage: stronger but still readable effect.
- Critical health: communicate danger without making gameplay difficult to see.

Avoid fully red screens or long-lasting opaque overlays.

Do not interfere with the newly transparent HUD.

### C. Physical player reactions

Use the existing player hybrid-ragdoll system to produce controlled reactions.

- Small hits: brief flinch.
- Strong hits: more pronounced physical response.
- Repeated hits: subtle accumulated disturbance.

**Do not regularly knock the player down or disable movement because of ordinary gunfire.**

Fast player movement is a core gameplay principle.

### D. Camera feedback

Introduce extremely subtle camera impulses for particularly powerful hits.

Requirements:

- Keep shake amplitude small.
- Limit duration.
- Avoid continuous camera vibration during automatic fire.
- Do not disrupt mobile aiming.
- Avoid breaking camera tracking, ladders, or parachute movement.

Support configurable reduction or disabling of camera shake.

### E. Health bar feedback

Make damage immediately noticeable in the existing health HUD.

A brief, clearly readable change or flash is sufficient.

Do not replace the health HUD or introduce excessive animations.

---

## 6. PHYSICAL REACTIONS MUST NOT BREAK AI

This is extremely important.

An enemy staggering or temporarily falling must not cause permanent AI failure.

Ensure:

- Reactions cooperate with existing locomotion.
- Enemies can recover from temporary balance loss.
- Navigation resumes appropriately.
- AI states do not become permanently locked.
- Enemy perception remains logically consistent.
- Suppression and fear remain functional.
- Enemies do not become stuck in cover.
- Enemy deaths still transition correctly into the existing death-ragdoll system.

A living enemy must not accidentally be treated as dead simply because it briefly falls.

Do not modify the enemy personalities.

Larpers must remain inaccurate at long distance but dangerous up close. That is intentional balancing and should be preserved.

---

## 7. INTEGRATE WITH EXISTING PLAYER SYSTEMS

Verify compatibility with:

- Light and heavy body armor.
- Helmets.
- Duct Tape healing.
- Revive injections.
- Temporary post-revive invulnerability.
- Parachuting.
- Crouching.
- Cover and blind firing.
- Weapon switching.
- Reloading.
- Grenade throwing.

### Damage feedback and invulnerability

Do not display ordinary successful-hit feedback when an attack was completely negated by invulnerability.

Armor-reduced hits may still generate appropriate feedback, but feedback intensity should reasonably reflect the actual outcome.

### Existing animations

Ensure player flinches do not:

- Cancel reloads incorrectly.
- Interrupt healing animations unexpectedly.
- Disable virtual joysticks.
- Break ladder climbing.
- Cause weapon sprites to detach.
- Trigger accidental shots.

---

## 8. PERFORMANCE AND TECHNICAL REQUIREMENTS

The game targets mid-range Android hardware.

Prioritize efficient implementation.

- Reuse existing Rigidbody2D and Joint2D components.
- Avoid spawning separate ragdolls for normal hit reactions.
- Avoid expensive physics searches every frame.
- Avoid excessive material instantiation.
- Reuse or pool visual indicators where appropriate.
- Prevent repeated animation event allocations.
- Throttle redundant feedback from automatic fire.
- Avoid unnecessary screen-wide rendering effects.

Use serialized, configurable parameters for:

- Impact impulse.
- Stagger threshold.
- Recovery timing.
- Impact decay.
- Reaction cooldown.
- Tracer width.
- Damage indicator intensity.
- Camera shake strength.

Where possible, centralize shared logic while preserving weapon-specific behavior.

---

## 9. REGRESSION TESTING

### Enemy reactions

- [ ] Small pistol impact creates a light flinch.
- [ ] SMG fire produces repeated but controlled reactions.
- [ ] LMG hits feel appropriately heavy.
- [ ] Desert Eagle causes pronounced impact.
- [ ] Sniper produces a powerful response.
- [ ] Shotgun reacts according to effective pellet impacts.
- [ ] Body-region reactions behave appropriately.
- [ ] Directional physical impulses work.
- [ ] Enemies recover after temporary falls.
- [ ] Rapid-fire weapons cannot permanently stun-lock enemies.
- [ ] Enemy navigation still functions after staggering.
- [ ] Existing fear and suppression systems remain intact.
- [ ] Death-ragdoll transitions remain functional.

### Player damage feedback

- [ ] Damage from the left.
- [ ] Damage from the right.
- [ ] Damage from above.
- [ ] Damage from below.
- [ ] Small damage produces restrained feedback.
- [ ] Heavy damage produces stronger feedback.
- [ ] Multiple rapid hits do not overwhelm the screen.
- [ ] Player movement remains responsive.
- [ ] Crouching and cover remain functional.
- [ ] Reloading and healing do not break.
- [ ] Revive invulnerability behaves correctly.
- [ ] Camera remains stable during movement.

### Tracers

- [ ] Desert Eagle tracer visibly thicker than ordinary pistol tracer.
- [ ] Sniper tracer visibly thicker than Desert Eagle tracer.
- [ ] Officer's special tracer appearance preserved.
- [ ] Tracers align with hit detection.
- [ ] Both firing directions work.
- [ ] Diagonal firing works.
- [ ] Tracer fade works.
- [ ] No substantial Android rendering overhead.

### General regressions

- [ ] Enemy accuracy and damage unchanged.
- [ ] Weapon damage and ammunition unchanged.
- [ ] Movement speeds unchanged.
- [ ] Previous reload fixes remain functional.
- [ ] Grenades from cover remain functional.
- [ ] Wallet collection remains reliable.
- [ ] UI transparency remains intact.
- [ ] No new physics softlocks.
- [ ] No new major performance regressions.

---

## 10. BUILD AND COMPLETION

Run appropriate automated tests and build an Android ARM64 APK.

Report actual validation results.

Do not claim physical Android testing unless a device was connected and the game was tested on it.

### Completion criteria

Sprint 0037 is complete when:

1. Living enemies respond physically to gunfire.
2. Different weapons produce meaningfully different reactions.
3. Accumulated impacts can produce temporary loss of balance without excessive stun-locking.
4. Player damage is immediately readable and impactful.
5. Player mobility remains responsive.
6. Desert Eagle and sniper tracers are noticeably thicker.
7. Existing damage, AI, weapon, and movement balance remain intact.
8. Android performance remains suitable for the game's target hardware.

### FINAL CODEX REPORT

Explain:

- Files modified.
- How the existing active-ragdoll system was extended.
- How hit intensity and accumulated impact work.
- What prevents permanent stun-locking.
- How player damage feedback works.
- Final Desert Eagle and sniper tracer multipliers.
- Tests performed.
- Build results.
- Any features requiring manual playtesting.

**IMPLEMENT THE COMPLETE SPRINT. DO NOT STOP AFTER WRITING A PLAN.**

**DESIGN PHILOSOPHY: Weapons should feel powerful because the game world physically reacts to them—not merely because they subtract larger numbers from health bars.**