AKUMU 2 — BUNKER CORRECTION / FOLLOW-UP TASK
Project: AKUMU 2
Priority: High
Scope: Focused fix to the Officer bunker implementation and encounter presentation

The current bunker is still not what is intended.

It must be changed.

## CORE CORRECTION

The Officer bunker should NOT be an improvised dirt-and-wood dugout.

It should be a **small concrete bunker / pillbox**, essentially a **box-like bulletproof structure** with a **firing hole** for the Officer to shoot out of.

Think:

- small concrete defensive box
- visibly solid and heavy
- clearly bulletproof
- simple, readable silhouette
- one main firing opening / embrasure
- the Officer hides inside like a coward and shoots through the opening

This is the intended fantasy.

---

## 1. VISUAL DESIGN REQUIREMENTS

Replace or rework the current bunker so it reads as a **compact concrete pillbox**.

Required visual traits:

- made of concrete, not wood or dirt
- boxy, compact shape
- thick walls
- a dark interior opening / firing slit / shooting hole
- clear front face
- simple handmade pixel-art rendering consistent with AKUMU 2
- slightly crude / amateur visual style, but unmistakably a concrete fortification

Optional details if they help readability:

- chipped concrete edges
- hairline cracks
- weathering
- a little dirt around the base
- rough outline shading
- slightly uneven handmade texture

Do NOT make it huge.
Do NOT make it look like a full building.
Do NOT draw it as background-only scenery.
Do NOT make it look like sandbags or timber cover.

The bunker must visually communicate:
**"This is a small concrete box for a cowardly officer to hide in."**

---

## 2. GAMEPLAY / COLLISION REQUIREMENTS

This bunker must exist as real gameplay geometry, not background art.

Implement it as actual foreground world geometry with proper collision.

Requirements:

- bunker walls are solid
- bunker walls block bullets
- bunker walls block line of fire
- player cannot walk through bunker walls
- Officer cannot shoot through solid concrete sections
- shots may only pass through the intended firing hole/opening
- warning laser / aiming telegraph must also respect bunker occlusion

If needed, use or update:

- StaticBody2D
- CollisionShape2D
- tile collisions
- existing cover/collision systems
- projectile collision masks/layers

The key rule is:

**Only the firing hole is a valid opening for shots.**
Everything else is concrete and must behave like concrete.

---

## 3. OFFICER BEHAVIOR INSIDE THE BUNKER

The Officer should use the bunker as a cowardly fortified shooting position.

Desired behavior:

1. stay inside bunker
2. remain protected by the concrete walls
3. peek or position himself at the firing hole
4. telegraph shot with the existing warning laser
5. fire through the firing hole
6. retreat deeper into cover
7. repeat while under pressure

He should feel like an annoying pest hiding inside a little concrete rat box.

He must NOT:

- stand outside the bunker for no reason
- fire through the concrete wall
- get stuck inside
- remain permanently hidden without re-engaging
- become invulnerable or unreachable

He should still be beatable through normal combat and positioning.

---

## 4. FIRING HOLE / EMBRASURE LOGIC

The firing hole is the most important part.

Implement it so that:

- it is visually obvious
- it aligns with the Officer’s firing position
- it is large enough to allow readable engagement
- it is small enough that the bunker still feels protective
- projectiles and laser telegraph can travel through it correctly
- the Officer can meaningfully hide when not using it

The player should immediately understand:

**"He is shooting from that hole."**

Do not create ambiguous collision where the opening looks open but is blocked, or looks blocked but allows bullets through.

Visual readability and collision behavior must match.

---

## 5. FOREGROUND / LAYERING

Ensure the bunker sits correctly in the playable scene.

Requirements:

- bunker is rendered as interactive foreground structure where appropriate
- Officer appears inside it correctly
- wall/opening relationships are visually clear
- projectiles visibly impact the concrete when blocked
- the firing hole reads as a real opening into a dark interior
- the bunker does not look pasted into the background

If needed, separate front/back visual elements for proper character layering.

The Officer should look like he is actually occupying the bunker interior, not merely standing in front of a painted prop.

---

## 6. PRESERVE APPROVED ENVIRONMENTAL ART

Do not redo the improved background/environment work from Sprint 0028.

That work was successful and should remain.

Only change what is necessary to make the bunker fit correctly into the environment and function properly.

The new concrete bunker should integrate with the improved level art, not replace it.

---

## 7. ACCEPTANCE CRITERIA

The bunker fix is only complete if all of the following are true:

- bunker is visibly made of concrete
- bunker is a small box/pillbox shape
- bunker has a clear firing hole/opening
- bunker is actual gameplay geometry
- bunker blocks bullets everywhere except the opening
- Officer uses it as cover
- Officer shoots through the opening only
- warning laser respects the opening and wall occlusion
- player can understand the encounter visually
- bunker is not background-only decoration
- collision and visuals match
- Officer remains beatable and does not softlock

If the bunker is still mostly decorative, still dirt/wood, or still not clearly a concrete firing box, then the task is not complete.

---

## 8. TESTING

Test:

- player bullets against bunker walls
- Officer bullets against bunker walls
- Officer line of fire through opening
- telegraph laser through opening
- Officer hide/peek/fire/retreat cycle
- player movement around bunker
- no collision desync between visuals and gameplay
- no pathfinding deadlock
- encounter completion in actual Level 1 play

Provide a concise report listing:
1. files/scenes changed
2. how collision was implemented
3. how the firing hole works
4. how Officer behavior was adjusted
5. tests performed
6. any remaining issues