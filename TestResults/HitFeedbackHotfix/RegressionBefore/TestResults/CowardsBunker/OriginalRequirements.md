# AKUMU 2 — SPRINT 0028
## Operation: The Coward's Bunker

**Project:** AKUMU 2 (黒橋 GAMES)  
**Primary focus:** Environmental graphics, Officer bunker, cover behavior and Level 1 polish.

### 0. CRITICAL INSTRUCTIONS

Work within the existing Godot project. Inspect relevant scripts, scenes and resources before modifying anything.

- Reuse existing systems wherever possible.
- Do not rebuild working mechanics.
- Avoid unnecessary architectural refactoring.
- Preserve the current Level 1 layout and encounter pacing.
- Do not introduce new enemy archetypes.
- Do not redesign menus, shops or the mobile HUD.
- Keep implementation focused to conserve Codex usage.
- Test modified systems and report remaining issues.

**The first level is already structurally good. We are improving it, not remaking it.**

---

## 1. ENVIRONMENTAL GRAPHICS IMPROVEMENT

Improve the background and environmental artwork of Level 1.

### Art direction

The target aesthetic is:

**A reasonably talented, enthusiastic teenager making a pixel-art game using free software.**

The artwork should look lovingly handmade, slightly imperfect, expressive and charming.

Absolutely avoid:
- Smooth vector graphics.
- Generic professional asset-pack aesthetics.
- Excessive gradients or sophisticated lighting.
- Detailed photorealistic textures.
- Inconsistent pixel sizes.
- Sterile, procedurally perfect geometric artwork.

### Required improvements

Improve existing terrain, platforms, walls and background scenery.

Introduce modest environmental detail such as:

- Rough earth and exposed dirt.
- Clumsy, chunky grass and vegetation.
- Simple rocks and scattered debris.
- Uneven soil and terrain textures.
- Sparse background trees or silhouettes.
- Small signs of a makeshift defensive position.

Use a coherent, limited palette and simple hard-edged shading.

Background artwork must remain visually subordinate to enemies, projectiles, interactive cover and the player.

Keep Level 1's introductory character: it is primarily horizontal and must not become a complicated vertical obstacle course.

### Consistency

The environmental graphics established here must be reusable as the visual foundation for subsequent levels.

Preserve the same pixel scale, outline conventions, shading style and handmade appearance throughout the project.

---

## 2. GIVE THE OFFICER A SMALL BUNKER / DUGOUT

The Officer miniboss needs his own little defensive position near the final encounter.

He is a cowardly commander who wants to survive while other people do the dangerous work.

His bunker should reinforce that characterization.

### Visual design

Construct a small, crude, readable bunker or dugout using existing environmental assets wherever possible.

Suggested appearance:

- Partially sunken defensive position.
- Rough dirt and timber construction.
- Simple sandbags or makeshift protective barriers.
- A firing opening or exposed edge.
- A sheltered area where the Officer can retreat.

The construction must fit the established handmade pixel-art style.

It should look improvised and slightly ridiculous rather than like a professionally engineered military fortification.

**Do not make the bunker enormous or turn it into a separate level.**

### Gameplay requirements

The bunker must provide genuine protection from incoming gunfire using the game's existing collision and cover mechanics.

The Officer must be able to:

1. Occupy his defensive position.
2. Hide behind cover.
3. Expose himself to acquire a firing opportunity.
4. Telegraph his Desert Eagle shot.
5. Fire and retreat behind cover.
6. Reposition or escape when pressured.

The bunker must not make the Officer invulnerable.

The player needs at least one clear way to approach, flank or force the Officer out of his position.

Avoid trapping either character in the geometry.

---

## 3. OFFICER BEHAVIOR — A TERRIFIED LITTLE PEST

Keep the Officer's current identity:

**Cowardly, evasive, comparatively accurate, and dangerous when he gets a clean shot.**

His difficulty should come from evasiveness and positioning, not excessive health.

### Defensive behavior

When the Officer is inside or near his bunker:

- Prefer cover over standing exposed.
- Occasionally peek to acquire and engage the player.
- Retreat after firing when appropriate.
- React to nearby incoming gunfire using existing suppression and fear systems.
- Become more evasive if the player advances toward his position.

Do not make his peeking behavior completely predictable.

However, do not introduce elaborate tactical AI or expensive pathfinding systems.

Reuse existing cover, retreat, line-of-sight, suppression and movement logic.

### Offensive behavior

Preserve the existing Desert Eagle characteristics:

- High individual shot damage.
- Noticeably better accuracy than ordinary Larpers.
- Distinctive, thicker projectile tracer.
- Different tracer color from ordinary firearms.
- Approximately **0.5-second firing telegraph**.
- Readable, stable telegraph laser without visual glitches.

His accuracy must make him threatening, but he should not have magical instantaneous tracking or perfect accuracy.

The Officer should not shoot through solid cover.

### Anti-softlock safeguards

The Officer must not:

- Remain permanently hidden.
- Get stuck against bunker collision.
- Oscillate endlessly between movement targets.
- Become unable to finish the encounter.
- Retreat into unreachable geometry.

Add lightweight recovery behavior if needed.

**Do not turn the Officer into a stationary turret.**

---

## 4. BUNKER AND LEVEL INTEGRATION

Position the bunker intelligently within Level 1's existing final encounter.

The player should notice the defensive structure before or during the Officer encounter.

Ensure:

- The approach provides reasonable movement options.
- The Officer has useful but imperfect cover.
- Nearby enemies do not spawn inside solid objects.
- Existing ladders remain fully functional.
- The player can climb onto and automatically exit ladder tops as before.
- Grenades, projectiles and explosions respect bunker collisions.
- The camera keeps the encounter readable.
- The final checkpoint and level-completion trigger continue working.

Do not dramatically increase the level's overall length.

Do not add arbitrary enemy waves or large amounts of new geometry.

The bunker is an encounter enhancement, not a level redesign.

---

## 5. SMALL ENVIRONMENTAL POLISH

While editing Level 1, correct obvious environmental inconsistencies directly related to the new artwork or bunker.

Prioritize:

- Misaligned terrain textures.
- Unintended collision gaps.
- Decorative objects obstructing movement.
- Cover edges that catch characters.
- Inconsistent platform visuals.
- Background elements obscuring important gameplay.

Do not expand this into a comprehensive project-wide bug-fixing sprint.

Avoid adding expensive lighting, particle systems or background simulations.

Mobile performance remains a priority.

---

## 6. REGRESSION TESTING

Test the modified Level 1 using the existing test/build workflow.

### Environment

- Backgrounds display correctly.
- No missing sprites or textures.
- No unexpected collision changes.
- No noticeable performance regression.
- Visual style remains consistent.

### Officer

- Can enter and leave bunker cover.
- Can expose himself and fire.
- Telegraph remains visible and stable.
- Projectile tracer remains distinctive.
- Cannot fire through solid cover.
- Retreat and suppression behavior work.
- Cannot become permanently stuck or untouchable.
- Can be defeated normally.

### Player

- Can approach and navigate the bunker.
- Can use ladders and exit their tops normally.
- Can engage the Officer from valid positions.
- Can complete Level 1 successfully.

### Existing systems

Preserve working implementations of parachuting, fall damage, reloading, ammunition pickups, healing, grenades, checkpoints, shops and the mobile controls.

---

## 7. SCOPE AND IMPLEMENTATION DISCIPLINE

This sprint has two principal deliverables:

**A. Improved Level 1 environmental graphics.**

**B. A functional Officer bunker with entertaining defensive behavior.**

Complete these first.

Reuse existing components and assets instead of inventing new frameworks.

Make the smallest reliable changes that deliver the requested behavior. Do not rewrite functional systems merely because an alternative implementation might be cleaner.

If an issue is unrelated to this sprint, document it instead of automatically fixing it.

### Final report

At the end, provide:

1. Summary of implemented changes.
2. Files created or modified.
3. Explanation of bunker and Officer behavior.
4. Tests performed and their results.
5. Known issues or unverified behavior.

**Definition of done:** Level 1 looks more deliberately handmade, the Officer has his own convincing defensive position, and the encounter is more entertaining without breaking existing mechanics.

The Officer should feel like an annoying, slippery coward hiding in his little hole—not an invincible boss or an elite commando.