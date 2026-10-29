# AKUMU 2 — SPRINT 0033
## OPERATION: BIGGER BUNKER, FEWER PROBLEMS
### Small Hotfix — Bunker Enlargement, Visibility & Android Export Readiness

**Project:** AKUMU 2 (悪夢 2)  
**Engine:** Unity 2D URP  
**Platforms:** Android / PC  
**Sprint type:** Small pre-playtest hotfix  
**Priority:** Bunker enlargement

---

## 0. MISSION

This is a SMALL hotfix before exporting the game to Android for actual device playtesting.

Do not expand the scope.

The Officer's bunker is now functional and its overall design is approved.

However, the bunker is currently much too small. It is difficult to see what is happening inside, particularly when the Officer is occupying it.

The bunker needs to be considerably larger while maintaining its functionality and handmade visual style.

We will also address two small related polish issues.

**The objective is to make the existing level more readable and ready for Android playtesting, not to introduce new gameplay systems.**

---

## 1. PRIORITY ONE — ENLARGE THE OFFICER'S BUNKER

### Mandatory change

**Make the Officer's bunker AT LEAST TWICE ITS CURRENT SIZE.**

Prefer approximately doubling both its width and height, provided that it fits within the existing level without disrupting essential traversal.

This should create a substantially larger interior, not merely enlarge a background sprite.

The bunker must feel like an actual little defensive structure rather than a tiny box around the Officer.

### Interior visibility

The most important reason for this change is visibility.

Currently, it is difficult to observe the Officer's behavior inside the structure.

After the enlargement:

- The Officer should be clearly visible when exposed inside the bunker.
- His movement and retreat animations should be recognizable.
- His Desert Eagle should be visually identifiable.
- His firing and reloading behavior should be readable.
- His position relative to the firing slit should be apparent.
- There should be enough space to appreciate his cowardly behavior.

The player should understand when the Officer is hiding, peeking, aiming and retreating.

### Architectural appearance

Preserve the existing bunker design philosophy.

It should look like a hastily constructed defensive position drawn by an enthusiastic teenager using a free pixel-art editor.

Retain:

- Chunky pixel-art geometry.
- Simple concrete surfaces.
- Thick outlines.
- Imperfect handmade textures.
- The defensive firing slit.
- The existing deliberately crude aesthetic.

Do not replace it with a professionally rendered military fortification.

### Functional interior

Adjust the bunker geometry, internal positions and collision to suit its new dimensions.

Simply scaling the visual sprite is not sufficient.

Ensure:

- The Officer can move naturally inside.
- Peeking positions remain functional.
- Retreating remains functional.
- Cover collisions match the visible bunker.
- The firing slit has a usable line of sight.
- The Officer does not get stuck.
- The Officer does not accidentally leave his intended defensive area.
- Bullets cannot pass through solid concrete.
- The Officer's own shots are not blocked incorrectly by bunker colliders.

Do not make the Officer more aggressive.

**He must remain a cowardly, accurate, bunker-dwelling pest.**

### Level integration

Enlarging the bunker must not destroy the existing first-level layout.

Adjust nearby environmental placements only if necessary.

Ensure the enlarged structure does not:

- Block the intended player route.
- Obstruct an important ladder.
- Make the Officer impossible to engage.
- Create inaccessible geometry.
- Obscure the player's movement unnecessarily.
- Cause camera-framing problems.

Preserve the existing encounter pacing.

---

## 2. SMALL FIX ONE — IMPROVE BUNKER VISUAL READABILITY

Even after enlargement, the bunker must not obscure its own interior.

Review sprite sorting, foreground walls, the roof and the firing slit.

Ensure that the front-facing artwork does not unnecessarily cover the Officer.

Possible solutions include:

- Enlarging the visible opening.
- Adjusting foreground geometry.
- Correcting sprite sorting layers.
- Introducing a simple cutaway appearance if required.

Choose the simplest solution that maintains the bunker aesthetic.

Avoid complicated transparency shaders.

The player must be able to visually follow the Officer's actions while still understanding which surfaces are solid cover.

### Officer combat visuals

Preserve the Officer's existing combat characteristics:

- Accurate Desert Eagle shots.
- Distinctive thicker tracer.
- Distinctive tracer color.
- 0.5-second attack telegraph.
- Existing bunker retreat behavior.

Ensure the aiming laser originates from the correct position and remains visible when the Officer peeks.

Do not allow the laser to flicker because of sprite layering.

Do not change weapon damage or attack frequency.

---

## 3. SMALL FIX TWO — ANDROID PLAYTEST READINESS

Perform a limited Android export readiness check.

This is NOT a new UI redesign.

Verify that the existing gameplay interface remains usable at typical Android aspect ratios.

Focus on:

- Movement joystick visibility and responsiveness.
- Aiming joystick visibility and responsiveness.
- Reload button.
- Healing button.
- Grenade controls.
- Weapon selection.
- Health and ammo displays.
- Pause button.
- Safe-area positioning.

Do not resize or reposition functioning controls unnecessarily.

Check that the enlarged bunker remains understandable at the mobile camera zoom.

The Officer should not become visually indistinguishable on a phone screen.

### Build checks

Check:

- Android build target and project configuration.
- Scene inclusion.
- Missing asset references.
- Compilation errors.
- Unnecessary debug overlays.
- Obvious UI clipping or overlap.
- Any new problems caused directly by bunker enlargement.

Preserve the game's existing Android settings unless a genuine export-blocking issue is found.

If the environment supports building an Android APK, attempt the build and report the result.

If it cannot build the APK, explain the limitation and identify any remaining manual export steps.

Do not claim testing on a physical Android device without actually performing it.

---

## 4. STRICT NON-GOALS

Do NOT:

- Add enemies.
- Redesign Level 1.
- Implement new weapons.
- Modify enemy damage or health.
- Redesign the HUD.
- Rework the AI architecture.
- Introduce new music.
- Add new animations unrelated to the bunker.
- Introduce new gameplay mechanics.
- Rebalance the entire level.
- Change the existing player movement system.

Preserve all completed work from previous sprints, including the AI improvements of Sprint 0032 and the UI overhaul of Sprint 0031.

This should remain a quick, contained hotfix.

---

## 5. ACCEPTANCE CRITERIA

- [ ] Bunker is visibly at least twice as large.
- [ ] Interior has substantially more usable space.
- [ ] Officer is easy to observe inside the bunker.
- [ ] Existing bunker art style is preserved.
- [ ] Officer can peek, aim, fire and retreat correctly.
- [ ] Cover collision matches visible geometry.
- [ ] No bullets incorrectly penetrate solid bunker walls.
- [ ] Officer's aiming laser and tracer remain correct.
- [ ] Bunker does not obstruct level completion.
- [ ] Mobile HUD remains usable.
- [ ] No new Unity compilation errors.
- [ ] Android export configuration is checked.
- [ ] No unrelated systems are modified.

---

## 6. FINAL HANDOFF

After completing Sprint 0033:

1. Summarize exactly what changed.
2. Report the bunker's new dimensions relative to its old dimensions.
3. Identify modified files and prefabs.
4. Explain how interior visibility was improved.
5. Report any detected collision or rendering issues.
6. Report Android build readiness and any unverified items.
7. Provide a short manual Android playtest checklist.

Do not expand this sprint into a major overhaul.

**The bunker is good. It simply needs to be MUCH bigger and easier to see inside.**

Once these changes are complete, the priority is exporting AKUMU 2 to Android for hands-on testing.

**END OF SPRINT 0033 — OPERATION: BIGGER BUNKER, FEWER PROBLEMS**