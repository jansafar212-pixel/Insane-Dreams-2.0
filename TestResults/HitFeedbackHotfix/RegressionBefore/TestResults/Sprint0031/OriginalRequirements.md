# AKUMU 2 — SPRINT 0031
## OPERATION: DEATH TO POWERPOINT
### Comprehensive Handmade UI, HUD, Menus, Shop & Visual Identity Overhaul

**Project:** AKUMU 2 (悪夢 2)  
**Studio:** 黒橋 GAMES / Kurohashi Games  
**Engine:** Unity 2D URP  
**Platforms:** Android and Windows PC  
**Previous sprint:** 0030 — Operation: The Bunker Is Real  
**Sprint classification:** Major visual/UI overhaul, usability improvements, consistency and regression testing

---

# 0. MISSION STATEMENT

This sprint is dedicated to making AKUMU 2 visually coherent, distinctive, charming and unmistakably handmade.

The game already has a recognizable gameplay identity: fast movement, exaggerated physics, deliberately absurd enemies, aggressive combat, cowardly enemy behavior and satirical humor.

Its interface must finally match that identity.

Currently, portions of the UI resemble generic professional mobile-game interfaces, polished productivity applications or PowerPoint presentations.

That is NOT the intended aesthetic.

The desired result should resemble something an enthusiastic, reasonably talented teenager could create using a free pixel-art editor after investing significant time and personal effort.

Think:

- Chunky, old-school 8-bit-inspired graphics.
- Deliberately simple, handmade pixel shapes.
- Square corners and uneven decorative details.
- Bold, readable lettering.
- Limited, recognizable colors.
- Personality through composition rather than technical sophistication.
- Extremely readable interfaces that remain comfortable to use on Android.
- A consistent visual identity across every screen.

However, the result must NOT look unfinished.

**The distinction is essential:**

Bad amateur art is careless, inconsistent and hard to read.

Good amateur art is simple, expressive, consistent and full of character.

We are aiming for the second.

The guiding philosophy for this sprint:

**"Made like a teenager's passion project. Plays like somebody spent years perfecting the mechanics."**

Do not interpret this as permission to decrease usability or introduce deliberately broken visual design.

## Absolutely essential constraints

1. Do not rewrite functional gameplay systems.
2. Do not redesign the existing first level.
3. Preserve the improved environmental artwork from previous sprints.
4. Preserve the Officer bunker and combat changes from Sprint 0030.
5. Do not introduce new enemy archetypes.
6. Do not implement NPC-to-NPC conversations yet.
7. Do not replace gameplay systems with nonfunctional mockups.
8. Do not introduce generic asset-pack UI, glossy gradients or stock mobile-game graphics.
9. Do not sacrifice Android usability for visual style.
10. Maintain good performance on midrange Android hardware.

This must be a functioning visual overhaul, not a collection of concept art.

---

# 1. REPOSITORY AUDIT BEFORE IMPLEMENTATION

Before changing anything, inspect the existing Unity project.

Identify:

- Main menu scene and related scripts.
- Existing UI Canvas hierarchy and/or UI Toolkit implementation.
- HUD prefabs.
- Mobile joystick implementations.
- Current health/ammo display.
- Weapon selection interface.
- Grenade selection and aiming indicators.
- Existing shop categories and purchase handlers.
- Medical item interface.
- Pause menu and settings.
- Level selection, if present.
- Mission objective displays.
- Victory/death/result screens.
- Existing pixel fonts, sprites, spritesheets and UI materials.
- Existing resolution scaling and screen-safe-area handling.

Determine which screens are already functional and which need only presentation changes.

Reuse existing scripts and prefabs where appropriate.

Do not replace a working implementation merely because a replacement seems easier.

Create a clear internal implementation plan, then implement it rather than stopping at the planning stage.

Keep gameplay state and UI presentation separate wherever practical.

If existing UI components can be restyled without touching gameplay code, prefer that approach.

---

# 2. ESTABLISH THE OFFICIAL AKUMU 2 UI STYLE

Create a small reusable visual design system.

This should be consistent across the entire project.

## 2.1 Typography

The existing approved **AKUMU 2 title lettering in the main menu** is the primary stylistic reference.

Study how that lettering looks:

- Blocky.
- Chunky.
- Slightly imperfect.
- Retro.
- Bold.
- Handmade.
- Visually memorable.

Develop complementary UI lettering with the same structural personality.

Do NOT simply use one of those generic futuristic, ultra-clean fonts that appear in thousands of mobile games.

The UI should have its own identity.

Important: The lettering should follow the APPROVED TITLE'S STYLE, not necessarily its colors.

For example, the title may remain green while the shop could use gray, faded cream, orange and muted red.

### Typography hierarchy

Define consistent styles for:

- Screen titles.
- Section headings.
- Buttons.
- Item names.
- Item descriptions.
- Numbers.
- Currency indicators.
- Small HUD information.
- Temporary gameplay notifications.

Use heavier and larger block lettering for important information.

Secondary information should remain readable without dominating the screen.

Avoid excessive font sizes and unnecessary text shadows.

Do not allow retro styling to make text illegible.

Pay particular attention to numbers such as ammo counts, health values and prices.

## 2.2 Color philosophy

Use a restrained, handmade retro palette derived from the existing game's visual identity.

Suggested palette roles:

- Dirty cream for primary text.
- Charcoal and dark gray for panels.
- Desaturated military green for secondary accents.
- Rusty orange for selections and warnings.
- Muted red for dangerous or unavailable states.
- Faded yellow for pickups and money.
- Concrete gray for neutral elements.

These are starting directions, not a requirement to recolor previously approved art.

Avoid:

- Corporate blue gradients.
- Neon-glowing buttons.
- Glossy surfaces.
- Glassmorphism.
- Soft, modern rounded rectangles.
- Excessive transparency.
- Generic sci-fi holographic UI.
- Polished fantasy RPG ornamentation.

Not every interface should be green.

The different screens should be recognizable while clearly belonging to the same game.

## 2.3 Borders and panels

Replace generic panels with chunky, handmade components.

Preferred characteristics:

- Pixel-aligned rectangular shapes.
- Sharp corners.
- Thick, simple borders.
- Minimal hard-edged shading.
- Small intentional asymmetries.
- Occasional imperfect outlines.
- Basic hand-drawn dividers.
- Simple inset or pressed-button states.

Avoid enormous decorative frames that consume screen space.

Create reusable panel and button styles.

Ideally, modifying the central visual style should update most of the game without manually editing every individual screen.

## 2.4 Iconography

Use a unified handmade pixel-art icon language.

Icons should look like they were produced by the same artist.

Do not mix clean vector icons, photorealistic item pictures and chunky pixel symbols.

Keep silhouettes recognizable at small sizes.

Prefer existing approved assets and simple original pixel icons.

Use nearest-neighbor filtering for appropriate pixel sprites, preserve crisp pixel edges and avoid accidental blurred scaling.

Do not introduce unnecessary image files or enormous textures.

---

# 3. MAIN MENU — THE GAME'S FIRST IMPRESSION

The main menu is the highest-priority visual target.

It must look like an intentional game menu rather than a default Unity UI scene.

## Preserve what already works

The approved AKUMU 2 title is a valuable visual reference.

Do not arbitrarily replace or substantially alter that title.

Build the other elements around it.

## Redesign menu components

Improve:

- Play button.
- Continue button, if implemented.
- Level selection.
- Shop access, if available from the menu.
- Settings.
- Quit/exit controls.
- Any other existing menu elements.

Buttons should feel like chunky pieces of handmade game art.

Each needs distinct normal, hover where applicable, pressed, selected and disabled visual states.

The exact states should depend on platform and control method.

## Composition

The menu should have a deliberate visual hierarchy:

1. AKUMU 2 title.
2. Primary action.
3. Secondary actions.
4. Small decorative details or studio branding.

Avoid filling the screen with unnecessary panels.

Negative space is important.

Do not cover existing attractive artwork with opaque boxes.

If the menu has a background scene or illustration, preserve it unless there is a clear visual inconsistency.

## Personality

Introduce restrained handmade personality through:

- Uneven border details.
- Small scratches or imperfect edges.
- Slightly crooked labels where readability allows.
- Simple military/urban visual motifs already consistent with the game.
- Distinctive selection indicators.

Do not introduce random decorations unrelated to AKUMU 2.

The menu must look intentionally crude, not randomly messy.

---

# 4. COMPLETE IN-GAME HUD REDESIGN

This is the second major priority.

The current gameplay HUD must be visibly connected to the new menu aesthetic.

Most importantly, it must remain functional during fast combat.

## 4.1 Health display

Rework the health display using:

- A compact, blocky bar.
- Limited colors.
- Simple hard-edged borders.
- Clearly readable values if numeric values are already supported.
- Unambiguous low-health indication.

Avoid excessive rounded corners or gradient fills.

Do not make the health display enormous.

Retain the compact sizing established in previous mobile HUD hotfixes.

The health bar must not dominate the screen or interfere with aiming.

Do not alter underlying health, healing or damage calculations.

## 4.2 Ammo display

Preserve the compact current-magazine/capacity format.

Example:

**16/27**

Display it using large enough chunky numerals to read quickly.

If reserve ammunition is already represented elsewhere, preserve that information in a visually subordinate way.

Ammo values must update correctly when:

- Firing.
- Reloading.
- Switching weapons.
- Running out of ammunition.
- Acquiring ammunition.
- Using different magazine capacities.

Do not make the numbers unnecessarily animated.

Immediate readability is more important than decorative movement.

## 4.3 Revival injections

The player can carry a maximum of two revival injections.

Keep their functionality unchanged.

Design clear handmade syringe-style inventory indicators, ideally showing two slots.

Each slot should visually distinguish available and consumed/empty states.

Do not create more slots than the existing gameplay limit allows.

Ensure the HUD updates after a revival and does not temporarily display incorrect quantities.

## 4.4 Weapon selection

The weapon selector currently requires visual refinement.

Rebuild its presentation using chunky, compact panels.

Requirements:

- Clearly show the selected weapon.
- Make switching visually obvious.
- Keep weapon slots separate and recognizable.
- Avoid generic polished weapon-selection cards.
- Avoid oversized panels obscuring gameplay.
- Preserve all existing selection functionality.
- Make unavailable or empty slots visually distinct.

Retain the smaller proportions established in earlier HUD adjustments.

Do not introduce weapon switching delays.

## 4.5 Grenade/throwable interface

The player's melee slot has been replaced by a throwable selection system.

Respect that architecture.

The selection UI should distinguish the existing throwable types, including fragmentation, flash and smoke where implemented.

Do not change how these items work.

Improve their icons and selected states.

Preserve the existing joystick-based aiming process and trajectory preview.

Ensure that any trajectory indicators remain readable without looking like over-polished mobile-game overlays.

Do not invent additional grenade types.

## 4.6 Reload button

Rework the visual appearance into a simple chunky handmade button.

Keep the existing control position, interaction logic and comfortable Android touch area unless a clear collision with nearby controls requires minor adjustment.

Do not make the button so decorative that its purpose becomes unclear.

## 4.7 Blind-fire control

Preserve the existing requirement:

**The blind-fire button appears only when the player is crouching in eligible cover.**

Do not display an active blind-fire control when the action is unavailable.

The button should use the same handmade visual vocabulary as other gameplay controls.

Preserve shooting direction and current functionality.

Do not alter cover collision or blind-fire mechanics.

## 4.8 Healing control

Make the Duct Tape healing control visually consistent.

Keep hold-to-heal behavior intact.

Clearly communicate whether healing is currently available.

Do not permit firing during an existing healing action if the gameplay system already prohibits it.

Avoid changing animation or sound systems unless fixing a UI integration bug.

## 4.9 Mission objectives

Previous revisions intentionally reduced the objective display's size.

Preserve that improvement.

Use a compact rectangular pixel-art label.

Short objective descriptions must remain readable on phones.

Long descriptions should fit gracefully without filling the screen.

The objective display should be secondary to gameplay.

## 4.10 Pause button

Preserve the top-center pause control.

Use the recognizable **⏸** symbol or an equivalent handmade pause icon.

The visible button should remain compact and not excessively wide.

The invisible touch target may be larger for comfort.

Avoid replacing it with a huge labeled rectangle.

## 4.11 Wallet pickups and temporary notifications

Retain existing wallet pickup functionality.

Redesign the pickup notification with:

- Compact blocky lettering.
- Clear currency visibility.
- Simple visual entry and exit.
- Minimal screen obstruction.

Do not create large repetitive reward popups.

Preserve the total wallet reporting at level completion.

## 4.12 Speech bubbles and combat readability

Enemy speech bubbles are an important part of AKUMU 2's humor and tactical communication.

Do not overwrite their system.

Where appropriate, bring their borders and typography into closer alignment with the handmade style.

Respect the existing maximum of two simultaneously visible bubbles.

Combat warnings and fear reactions take priority over ambient dialogue.

Do not implement the future NPC conversation system in this sprint.

---

# 5. ANDROID CONTROLS — KEEP THEM GOOD TO USE

This section is critical.

The game is designed to run on Android as well as PC.

A visually charming interface that is frustrating to control would be a regression.

## Movement joystick

Preserve:

- Current movement sensitivity.
- Existing movement range.
- Smooth movement response.
- Existing joystick scale and position unless a verified UI overlap requires adjustment.
- Existing transparency conventions where they improve visibility.

You may redraw the visible joystick base and thumb to fit the new pixel-art style.

Do not rewrite the movement input implementation.

## Aiming joystick

Preserve:

- Aiming sensitivity.
- Direction calculations.
- Shooting behavior.
- Current visual dimensions.
- The previously established slight leftward positional adjustment.

Redesign only the visual appearance where possible.

## Touch ergonomics

Visible pixel-art buttons can be small and compact while their interaction zones remain comfortably large.

Ensure:

- Joysticks do not accidentally activate nearby buttons.
- Reload remains accessible.
- Healing controls are usable without precision tapping.
- Weapon and grenade selectors do not overlap.
- Buttons do not cover the player unnecessarily.
- Controls remain usable during camera movement.
- Safe areas are respected on phones with display cutouts.

Do not reduce input response speed.

## Screen-size adaptability

Verify UI layout on representative aspect ratios:

- 16:9.
- 19.5:9.
- 20:9.
- A wider aspect ratio where practical.
- PC at 1920×1080.

Handle Android display cutouts and varying safe areas.

Use appropriate Unity anchors and Canvas scaling.

Do not rely entirely on hard-coded pixel coordinates.

The UI must adapt without stretching sprites into blurry rectangles.

---

# 6. SHOP INTERFACE — COMPLETE VISUAL OVERHAUL

The shop is one of the strongest candidates for redesign.

It should resemble a crude but lovingly assembled in-game equipment store.

Not a corporate dashboard.

Not an online banking app.

Not a standard Unity demonstration interface.

## General layout

Use an organized but distinctly handmade structure.

Possible composition:

- Chunky title.
- Category navigation.
- Item list or grid.
- Selected item details.
- Price.
- Current funds.
- Purchase action.
- Back/close control.

Keep the existing shop categories and item data.

Do not add entirely new categories unless necessary to restore previously specified behavior.

## Medical category

Ensure the Medical section correctly contains the existing medical products.

Duct Tape and Faintanyl must belong under Medical, not be scattered across unrelated categories.

Maintain the existing restriction that Faintanyl is shop-only.

Existing revival-related products should remain correctly categorized and functional.

Do not modify their effects or prices as part of this visual sprint.

## Item art

Where item icons require replacement, use simple pixel-art drawings.

They should look like they were made by the same person who drew the game's other sprites.

Avoid clean vector shopping icons.

Avoid photorealistic assets.

Use a consistent visual scale.

## Purchase buttons

A purchase button should clearly communicate:

- Available purchase.
- Insufficient funds.
- Item unavailable or maximum quantity reached, when applicable.
- Selected item.
- Successful purchase.

Use color plus shape/text distinctions rather than color alone.

Do not rebuild the underlying purchase system.

## Important: functionality

Check that buying an item actually updates:

- Currency.
- Inventory.
- Relevant HUD indicators.
- Availability states.
- Existing persistence mechanisms, where present.

Do not merely make the purchase button display a new value without updating game state.

Ensure the shop does not accidentally allow repeat purchases that bypass existing quantity restrictions.

---

# 7. PAUSE MENU, SETTINGS AND LEVEL SELECTION

These screens should no longer feel visually disconnected.

## Pause menu

Redesign using the same chunky UI language.

Keep existing functionality:

- Resume.
- Settings, if accessible.
- Restart, if implemented.
- Return to menu.
- Other currently supported actions.

Preserve existing pause behavior.

Do not leave combat simulation active if the existing system pauses it.

Ensure closing the menu restores the correct state.

## Settings

Preserve all implemented settings.

Use large enough interactive rows and readable labels.

Do not introduce settings that appear functional but do nothing.

Where sliders or toggles already exist, redesign their visuals without breaking persistence.

Support both mouse and Android touch inputs.

## Level selection

If level selection already exists, restyle it.

Use deliberately basic pixel-art tiles or chunky numbered buttons.

Clearly distinguish:

- Available levels.
- Locked levels.
- Currently selected level.
- Completed levels where progress tracking exists.

Do not expose unfinished levels unintentionally.

Do not rebuild the campaign or progression architecture.

---

# 8. DEATH, VICTORY AND END-OF-LEVEL SCREENS

These screens deserve stylistic consistency.

Use bold, handmade text.

Avoid massive polished overlays.

## Death screen

Clearly display the death state and existing available actions.

Preserve the revival system and its existing rules.

Do not invent unlimited continues.

If the player has no revivals remaining, the UI must accurately reflect that.

## Victory screen

Maintain clear success information.

If the screen currently displays money collected, retain that reporting.

Other existing statistics should remain accurate and readable.

Do not invent new performance statistics requiring complex gameplay telemetry.

The victory screen should feel rewarding without becoming visually extravagant.

## Visual personality

A little deliberate roughness is welcome.

The player should feel the screens belong to the same satirical universe as the enemies and environments.

Do not turn this into a generic military shooter interface.

---

# 9. PRESERVE THE ENVIRONMENTAL ART BIBLE

The first level's recent background improvements are approved.

Do not undo them.

The visual identity of the environment must remain:

- Handmade.
- Intentionally low-resolution.
- Square-headed character silhouettes.
- Chunky outlines.
- Simplified shading.
- Limited palette.
- Imperfect but charming textures.
- Basic environmental storytelling.
- Functional and readable cover geometry.

This sprint focuses primarily on interfaces.

Only modify environmental or character art when there is a specific visual integration problem.

Examples might include incorrect sprite filtering or a UI-linked world-space icon that clashes with everything else.

Do not repaint the bunker.

Do not rearrange level geometry.

Do not modify enemy spawn arrangements.

Do not replace approved textures with more polished ones.

Importantly, UI art should be consistent with environment art without needing to use precisely the same colors.

The UI and environment should feel like two parts of one project.

---

# 10. WORLD-SPACE VISUALS AND GAMEPLAY INDICATORS

Review simple world-space indicators that players encounter during gameplay.

Examples include:

- Interactable indicators.
- Pickup labels.
- Relevant ammo or health feedback.
- Objective markers already implemented.
- Existing grenade aiming displays.
- Existing Officer warning indicators.

Any restyling must maintain clarity.

**Do not change the Officer's 0.5-second laser telegraph timing or distinctive thick tracer design from Sprint 0030.**

Do not change damage, projectile handling, collision or detection systems.

Do not add dramatic UI effects that conceal attacks.

Avoid excessive visual noise.

A readable attack is more important than an attractive visual effect.

---

# 11. PERFORMANCE AND TECHNICAL IMPLEMENTATION

Maintain Android performance throughout this sprint.

Recommended approach:

1. Reuse existing Canvas/UI architecture.
2. Make common styles reusable.
3. Keep new pixel sprites small.
4. Avoid unnecessary transparent fullscreen overlays.
5. Avoid frequent layout rebuilding during combat.
6. Avoid unnecessary per-frame UI object creation.
7. Update event-driven UI when underlying values change where feasible.
8. Avoid expensive effects and shader complexity.
9. Preserve existing asset import settings that are correct.
10. Keep the scene and prefab hierarchy maintainable.

For Unity UI sprites, use appropriate pixel-art import settings, including point filtering when suitable.

Be careful about pixels-per-unit, scaling and Canvas rendering behavior.

Pixel art should remain crisp at practical output resolutions.

Do not use a lower-resolution gameplay render target merely to imitate retro graphics if that reduces visibility.

Do not force the player to sacrifice gameplay clarity.

## Shared UI components

Where useful, create reusable components for:

- Basic button.
- Selected button.
- Panel.
- Section title.
- Item slot.
- Inventory indicator.
- Numeric display.
- Notification frame.

Keep these lightweight.

Avoid building an enormous general-purpose design framework for a small indie game.

The architecture should be easy to extend when future levels and enemy types are introduced.

---

# 12. RESPONSIVE INTERFACE FEEDBACK

Interactions should feel immediate.

Buttons must respond clearly to input.

Use simple changes such as:

- One-pixel visual displacement.
- Border inversion.
- Palette swap.
- Basic selected-state highlight.
- Simple appearance/disappearance transitions.

Avoid slow animations that delay gameplay.

Do not introduce elaborate UI animation timelines merely for cosmetic appeal.

If the game already has appropriate menu sounds, retain them.

Do not make producing an entirely new soundtrack or sound library part of Sprint 0031.

Do not add flashing effects that make the interface distracting.

The goal is responsive, handmade interaction.

---

# 13. VISUAL CONSISTENCY AUDIT

After implementation, inspect every implemented screen.

Look for style inconsistencies.

Examples of unacceptable results:

- Main menu has chunky pixel fonts, but shop uses clean modern typography.
- HUD has rounded glossy buttons while settings use crude rectangular buttons.
- Item icons have mismatched pixel densities.
- Health bar and ammunition counters use unrelated border styles.
- Some buttons have shadows while others are flat.
- Menus use entirely different padding conventions.
- Screens contain stretched or blurry sprites.
- A supposedly handmade screen resembles a corporate dashboard.
- Visual elements overlap gameplay controls.
- Text becomes unreadable on smaller screens.

Fix these systematically.

Consistency should be treated as a hard design requirement, not optional polish.

However, avoid making every screen identical.

A shop, pause menu and combat HUD have different functions and should have different layouts.

They must simply share a recognizable visual language.

---

# 14. REGRESSION TESTING

This sprint must NOT break the mechanics built over the previous thirty sprints.

Perform a targeted regression pass.

## Player movement

Verify:

- Running.
- Jumping.
- Double jumping.
- Crouching.
- Crouch-walking.
- Ladder climbing.
- Automatic ladder dismount.
- Parachuting.
- Camera following and framing.
- Existing cover behavior.

Remember:

Landing with the parachute deployed must result in **exactly zero fall damage**.

Do not accidentally alter that rule.

## Combat

Verify:

- Firing.
- Reloading.
- Weapon switching.
- Ammo display updates.
- Grenade selection and aiming.
- Blind firing.
- Healing.
- Revival inventory indicators.
- Interaction with armor systems.

No unexpected input delays should result from the UI work.

## Enemies

Verify no apparent regressions in:

- Larper reactions.
- Samurai pursuit.
- Fireworks expert behavior.
- Officer bunker behavior.
- Enemy navigation.
- Existing suppression responses.

Do not rebalance enemy AI during this sprint unless a demonstrable regression from the UI changes requires a narrowly scoped fix.

## Shop

Verify:

- Category switching.
- Item selection.
- Currency display.
- Valid purchases.
- Rejected purchases.
- Medical item placement.
- Existing inventory updates.
- Closing and reopening the shop.

## Pause and scene changes

Verify:

- Pause opens and closes correctly.
- Resume works.
- Menu navigation works.
- Existing settings remain functional.
- Scene transitions do not leave UI visible incorrectly.
- UI references do not become invalid after scene changes.

---

# 15. IMPLEMENTATION ORDER

Perform the sprint in the following order.

## Phase A — Audit and design foundation

- Inspect existing interface implementation.
- Establish a small style palette.
- Determine common typography rules.
- Create reusable handmade panels, buttons and icons.
- Preserve the approved main-menu title.

## Phase B — Main menu

- Replace generic buttons.
- Improve hierarchy.
- Apply shared typography.
- Improve visual balance.
- Verify input functionality.

## Phase C — Gameplay HUD

- Health.
- Ammo.
- Revive indicators.
- Weapon selector.
- Grenades.
- Reload.
- Blind-fire.
- Healing.
- Objectives.
- Pause button.
- Pickups and notifications.

Prioritize Android playability during this phase.

## Phase D — Shop

- Restyle categories.
- Restyle item slots.
- Improve prices and purchase feedback.
- Fix any purely UI-related classification/display problems.
- Preserve existing shop functionality.

## Phase E — Remaining screens

- Pause menu.
- Settings.
- Level selection, if present.
- Death screen.
- Victory screen.

## Phase F — World-space indicators

Apply only necessary visual adjustments.

Avoid touching unrelated game systems.

## Phase G — Consistency and regression testing

Review all screens.

Fix incorrect scaling, clipping, blurry sprites and input overlaps.

Test available game workflows.

Complete actual implementation rather than leaving disconnected prototypes.

---

# 16. ACCEPTANCE CRITERIA

Sprint 0031 is successful only if the following requirements are satisfied.

### Visual identity

- [ ] All major existing UI screens share the handmade art direction.
- [ ] The approved AKUMU 2 title is preserved.
- [ ] Main menu no longer looks like a generic Unity template.
- [ ] Shop no longer resembles a corporate dashboard.
- [ ] Gameplay HUD matches the new aesthetic.
- [ ] All major text remains readable.
- [ ] Pixel-art assets render crisply.
- [ ] No unnecessary stock graphics or polished corporate styling are introduced.

### Android usability

- [ ] Movement joystick remains responsive.
- [ ] Aiming joystick remains responsive.
- [ ] Existing joystick sensitivity is unchanged.
- [ ] Mobile buttons remain comfortably usable.
- [ ] Health, ammo and objectives remain readable.
- [ ] No critical controls overlap.
- [ ] Different aspect ratios display correctly.
- [ ] Screen-safe areas are handled appropriately.

### Gameplay integrity

- [ ] Existing shooting systems work.
- [ ] Reloading works.
- [ ] Weapon switching works.
- [ ] Grenade selection and aiming work.
- [ ] Healing works.
- [ ] Blind-fire visibility conditions are preserved.
- [ ] Revival count remains accurate.
- [ ] Shop purchases correctly update inventory/currency.
- [ ] Pause/resume functions correctly.
- [ ] Sprint 0030 Officer bunker systems remain intact.
- [ ] No major gameplay regressions.

### Technical quality

- [ ] Project compiles without new errors.
- [ ] No missing sprite references.
- [ ] No broken button handlers.
- [ ] No unnecessary per-frame allocations in the new UI.
- [ ] Shared UI styles are reusable.
- [ ] Original gameplay scene structure is preserved where practical.
- [ ] Android build remains viable.
- [ ] No unnecessary dependencies were added.

---

# 17. NON-GOALS — DO NOT EXPAND THIS SPRINT

Avoid scope creep.

The following are explicitly NOT part of Sprint 0031:

- Creating new enemies.
- Creating Level 2.
- Rebuilding Level 1.
- Implementing the future NPC social conversation system.
- Rebalancing every enemy.
- Adding new weapons.
- Expanding the soundtrack.
- Rewriting combat systems.
- Replacing ragdoll physics.
- Rebuilding the Officer bunker.
- Creating an elaborate new animation framework.
- Adding unrelated progression systems.
- Replacing approved environmental artwork.

If a minor issue is directly blocking UI functionality, fix it.

Otherwise, record it as a future task rather than expanding the sprint indefinitely.

---

# 18. FINAL VERIFICATION AND HANDOFF

At the end of the implementation:

1. Summarize the main visual changes.
2. List the actual scenes, prefabs, scripts and assets modified.
3. Identify reusable UI components created.
4. Explain which existing systems were intentionally preserved.
5. Report compilation and test results accurately.
6. Identify anything not tested or not fully implemented.
7. Call out any remaining visual inconsistencies.
8. Provide clear instructions for manually testing the changed UI in Unity.
9. Do not claim Android testing was completed if no Android device or suitable test environment was available.
10. Do not claim the redesign is complete if only a few screens were modified.

If the environment supports screenshots or before/after captures, provide them for the main menu, HUD and shop.

Prioritize a finished, functional core redesign over a large collection of half-implemented screens.

## Final artistic instruction

AKUMU 2 is not supposed to be a sleek, premium-looking game.

It is a weird, chaotic, satirical indie project with an intentionally crude artistic identity.

That identity is valuable.

Do not polish it away.

We want the player to see the interface and immediately recognize that it belongs to the same ridiculous universe as the square-headed characters, cowardly Larpers, absurd enemies, improvised cover and handmade urban environments.

The game should look like it was created with limited artistic resources but enormous enthusiasm.

Every interface element should feel intentional.

Every button should look like it belongs.

Every screen should share an identifiable artistic personality.

**Technical sophistication should exist underneath the surface. The surface itself should remain gloriously handmade.**

Implement Sprint 0031 now.

**END OF SPRINT 0031 — OPERATION: DEATH TO POWERPOINT**