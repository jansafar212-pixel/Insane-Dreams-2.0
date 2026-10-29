# AKUMU 2 — SPRINT 0036
## THE GREAT UI, HUD, SHOP & GAMEPLAY PRESENTATION OVERHAUL

**Studio:** 黒橋 GAMES — Kurohashi Games

**Game:** AKUMU 2 (悪夢 2)

**Engine:** Unity 2D URP

**Platforms:** Android + Windows PC

**Sprint category:** Major visual and usability overhaul

**Implementation priority:** HIGH

**Development model:** GPT-6.1 Sol Medium

---

# 0. SPRINT MISSION

AKUMU 2 has reached a significant technical milestone.

Following the recent optimization work, the game feels substantially smoother on the actual Android device than it did earlier in development, despite having more complex gameplay systems.

The objective of Sprint 0036 is to take advantage of that progress by transforming AKUMU 2's menus, shop, HUD, visual feedback, and gameplay presentation into a unified, distinctive, enjoyable experience.

The current game contains numerous functional mechanics, but portions of its interface still resemble prototypes or placeholders.

This sprint must address that gap.

**The goal is NOT to make AKUMU 2 look like a professionally polished AAA mobile game.**

The goal is to make it look like a passionate and reasonably creative teenager spent months lovingly drawing and assembling a distinctive pixel-art action game using free software.

The result should feel authentic, expressive, consistent and deliberately handmade.

It must NOT resemble:
- A generic Unity asset-pack game.
- A corporate mobile application.
- A modern minimalist productivity dashboard.
- A glossy mobile shooter.
- An AI-generated collection of unrelated visual assets.
- An intentionally ugly parody of pixel art.
- A game with mismatched art styles between screens.

The existing AKUMU 2 title treatment should serve as a primary visual reference.

**Our core principle: Character and environmental storytelling before graphical fidelity.**

Everything must remain practical and responsive on Android.

---

# 1. ABSOLUTE PRIORITY — DO NOT DESTROY PERFORMANCE

The recent Android performance improvements must be preserved.

This is the most important technical constraint of the sprint.

Before beginning implementation:

1. Inspect recent performance-related changes.
2. Identify existing UI update patterns.
3. Inspect Canvas rebuilding frequency.
4. Inspect animation and rendering overhead.
5. Understand which scripts are performance-sensitive.
6. Establish a baseline using available profiling or automated measurements.
7. Avoid modifying optimized gameplay code unnecessarily.

### Performance rules

- Avoid unnecessary per-frame UI text updates.
- Prefer event-driven UI updates.
- Cache references where appropriate.
- Avoid repeated expensive scene searches.
- Avoid dynamically creating and destroying large numbers of UI objects.
- Use pooling for repeated temporary feedback elements where useful.
- Avoid excessive Canvas rebuilds.
- Separate frequently updated UI elements from largely static elements when beneficial.
- Reuse existing sprite materials and atlases where practical.
- Avoid expensive shaders and full-screen post-processing.
- Avoid new garbage collection spikes.
- Avoid replacing simple sprites with complicated UI effects.

Maintain responsive input and consistent frame pacing.

Target stable 60 FPS on capable Android hardware.

Do not claim actual Android performance has been verified unless a device build and physical-device test were performed.

If a change looks attractive but noticeably worsens performance, simplify the implementation.

**Performance beats decorative complexity.**

---

# 2. ESTABLISH A UNIFIED GRAPHICS BIBLE

Before editing individual screens, audit the game's existing art style.

Use the current AKUMU 2 title, established character sprites, and improved Level 1 environmental art as references.

Define a consistent UI graphics system.

### Visual characteristics

- Chunky pixel shapes.
- Simple geometric silhouettes.
- Thick, readable outlines.
- Limited, coherent color palettes.
- Hard-edged shading.
- Minimal gradients.
- Slightly irregular hand-drawn details.
- Intentionally simple highlights.
- Modest texture variation.
- Consistent pixel density.
- Readable, blocky typography.
- Distinctive but restrained decorative elements.

The graphics should appear handmade rather than procedurally random.

Do not introduce arbitrary pixel noise to simulate artistic effort.

A slightly crooked sign or asymmetrical decorative detail is welcome.

Randomly misaligning essential buttons and text is not.

### Consistency requirements

All redesigned screens should look as though they were drawn by the same person.

This includes:

- Main menu.
- Settings menu.
- Pause menu.
- Equipment selection.
- Weapon shop.
- Medical shop.
- Armor and helmet shop.
- Player HUD.
- Mission objectives.
- Death screen.
- Victory/results screen.

Use a common visual style system rather than creating unrelated graphics for each scene.

Prefer a small reusable library of UI assets to dozens of inconsistent one-off assets.

### Technical consistency

Ensure:

- Correct pixel-art texture import settings.
- Point filtering where appropriate.
- No accidental blurry sprites.
- No unnecessary texture compression artifacts.
- Consistent sprite slicing.
- Predictable scaling.
- No distorted borders.
- Correct 9-slicing where useful.
- Proper canvas ordering.
- Consistent typography.

Do not force every UI element onto an unnecessarily tiny texture.

Readability is more important than artificial pixel purity.

---

# 3. COMPLETE MAIN MENU REDESIGN

The main menu should become a strong first impression.

It must immediately communicate the personality of AKUMU 2.

### Title presentation

Preserve the recognizable AKUMU 2 title treatment.

Make the title:

- Large.
- Chunky.
- Distinctive.
- Clearly readable.
- Visually dominant without occupying the entire screen.

Do not replace the title with generic futuristic typography.

The Japanese text 悪夢 2 may appear as a supporting decorative element if consistent with the existing presentation.

Use the established studio identity, 黒橋 GAMES, where appropriate.

### Menu composition

Create a visually coherent layout containing the existing supported actions, such as:

- Play / Continue.
- Equipment or shop access where appropriate.
- Settings.
- Exit on PC.

Respect platform-specific behavior.

Do not add buttons that lead to nonfunctional features.

Do not expose debugging or developer-only options to ordinary players.

### Background

Use a stylized, low-cost visual composition related to AKUMU 2's existing world.

Possible implementation:

- Reuse existing environmental sprites.
- Present simple industrial structures.
- Use a few static silhouettes.
- Apply limited background movement if inexpensive.
- Add occasional simple environmental details.

Avoid creating an elaborate animated scene that consumes substantial performance before the player even begins playing.

### Button design

Buttons should have:

- Thick pixel-art borders.
- Clear normal and pressed states.
- Visible selection and focus states.
- Adequate spacing.
- Clear labels.
- Consistent scale.
- Subtle feedback.

Pressed buttons should visibly depress or change appearance.

Do not use slow animations that make menu navigation feel sluggish.

### Startup behavior

Verify:

- Main menu loads reliably.
- Existing progress is recognized.
- Buttons work correctly after returning from gameplay.
- No duplicate UI elements appear.
- No invisible overlays block input.
- Menu state does not leak into gameplay.

---

# 4. MOBILE HUD — COMPLETE VISUAL OVERHAUL

The Android HUD is one of the most important parts of the game.

It must match AKUMU 2's art style without sacrificing responsive controls.

### Core philosophy

The player should understand essential combat information at a glance.

The interface must never become more distracting than the firefight.

Preserve the established mobile HUD sizing decisions unless a demonstrated usability issue requires adjustment.

### Health display

Redesign the existing health bar.

Requirements:

- Chunky, pixel-art appearance.
- Clearly visible current health.
- Simple, readable damage feedback.
- No excessive screen occupation.
- No unnecessary numerical clutter.
- Accurate updates during damage and healing.

Health loss should be visually apparent without requiring the player to read tiny text.

Optional: use a brief, inexpensive visual flash when health decreases.

Avoid excessive screen-wide red effects.

### Ammunition HUD

The ammunition display must use the existing format:

**Current rounds / Magazine capacity**

Example: **16/27**

Requirements:

- Update immediately after firing.
- Update after reloading.
- Update after weapon switching.
- Correctly reflect magazine capacity.
- Do not confuse magazine ammunition with spare ammunition.
- Keep spare ammunition visible if supported by the current design.

Use readable chunky lettering.

Do not change the actual weapon ammunition systems except to fix identified defects.

### Revival HUD

The player can carry a maximum of TWO revival injections.

Display their remaining quantity using clear, simple icons.

Requirements:

- Correct count.
- No more than two.
- Immediate update after a revival.
- Consistent with existing equipment inventory.
- No misleading icon when none remain.

### Pause button

Preserve the established top-center pause control.

Use the recognizable pause symbol.

Keep the visible button compact and pixelated.

The actual touch target must remain comfortably usable.

The visual size and functional touch area may differ.

### Mission objective display

Make objectives compact and readable.

Avoid unnecessarily long text permanently occupying the gameplay screen.

Use brief objective labels and update them when the objective changes.

Do not obstruct the player's view of enemies.

### Wallet and currency feedback

When the player collects a wallet:

- Show short currency feedback.
- Update the appropriate currency display.
- Avoid excessive overlapping messages.
- Preserve automatic collection.
- Prevent duplicate rewards.
- Use a small, playful visual response.

The screen should not fill with text when several wallets are collected close together.

---

# 5. MOBILE CONTROLS — VISUAL AND INPUT POLISH

Preserve the existing two-joystick control scheme.

### Movement joystick

Requirements:

- Maintain current responsive behavior.
- Preserve established sensitivity and dead-zone settings unless defective.
- Keep the joystick semi-transparent.
- Use a chunky, hand-drawn appearance.
- Maintain a comfortable touch radius.
- Avoid visual overlap with important gameplay events.

### Aiming joystick

Preserve the established positioning, including the slight leftward adjustment from previous sprints.

Do not accidentally revert it.

Requirements:

- Smooth aiming.
- Reliable touch capture.
- No unexpected joystick snapping.
- No missed release events.
- Correct behavior when the player changes weapons.
- Clear separation from neighboring buttons.

### Weapon-specific controls

The dedicated sniper rifle must preserve its established interaction:

- Touch and hold the aiming control to aim.
- Display the aiming laser.
- Adjust aim while holding.
- Release to fire one shot.
- Automatically cycle the weapon afterward.
- Preserve a way to cancel aiming without firing.

The player's Desert Eagle must NOT inherit this mechanic.

It uses standard mobile firing controls with a deliberately slower firing cadence.

### Other buttons

Audit:

- Reload.
- Healing.
- Grenade selection.
- Grenade aiming and throwing.
- Blind fire.
- Pause.
- Weapon selection.

Make all buttons visually consistent.

Preserve the rule that blind fire is only available in the appropriate crouched-cover state.

### Touch safety

Ensure multitouch works correctly.

Specifically test:

- Moving while aiming.
- Moving while shooting.
- Shooting while changing direction.
- Holding healing controls.
- Releasing an aimed grenade.
- Opening pause during gameplay.

No invisible UI graphics should intercept unrelated touches.

Do not reduce touch targets merely to achieve smaller visual buttons.

---

# 6. WEAPON SELECTION INTERFACE

Redesign the in-game weapon selection panel.

The panel should look handmade and consistent with the rest of the HUD.

### Requirements

- Display existing owned weapons.
- Identify the equipped weapon.
- Show ammunition information.
- Support fast weapon changes.
- Avoid excessively large screen overlays.
- Preserve correct touch behavior.
- Do not interrupt gameplay unexpectedly.
- Display appropriate weapon icons.

### Weapon icon consistency

All weapon icons should share:

- Similar visual scale.
- Consistent pixel density.
- Matching outline thickness.
- Simple limited palettes.
- Comparable detail levels.

Weapons should remain visually distinguishable.

Do not distort a weapon silhouette merely to force every icon into the same exact shape.

### Basic pistol correction

Verify the existing standard pistol:

- Has an ordinary magazine appearance.
- Does NOT show an extended magazine.
- Holds exactly 15 rounds.
- Displays correct ammunition values.

Do not inadvertently modify other weapon capacities.

---

# 7. MAJOR SHOP VISUAL REDESIGN

The shop should feel like an actual part of AKUMU 2.

It must not resemble a generic polished mobile storefront.

### Overall appearance

Create a chunky retro shop interface.

Use:

- Simple dark panels.
- Thick pixel borders.
- Readable blocky headings.
- Stylized item icons.
- Clear prices.
- Clear inventory quantities.
- Obvious selection states.
- Immediate purchase feedback.

Allow slightly humorous decorative details without making the shop harder to operate.

### Categories

Preserve existing categories for:

- Weapons.
- Ammunition.
- Armor and helmets.
- Medical.
- Grenades and other existing consumables.

Adapt to the categories already implemented rather than introducing duplicate inventory systems.

### Item cards

Each item should display relevant existing information:

- Item name.
- Price.
- Quantity or ownership.
- Short description.
- Equipped state if applicable.
- Purchase availability.

Do not overload every card with lengthy statistics.

### Purchase behavior

Ensure:

- Purchases subtract the correct amount.
- Players cannot buy items without enough money.
- Owned equipment displays correctly.
- Quantity limits are enforced.
- Purchasing feedback is immediate.
- Changes persist between sessions.
- Purchased items are available during gameplay.
- UI refreshes without requiring the player to reopen the shop.

### Equipment comparison

Where the existing data supports it, show concise differences between weapons or armor.

Do not invent misleading statistical values.

A simple, readable comparison is preferable to a complicated RPG interface.

---

# 8. MEDICAL SHOP CORRECTIONS

The Medical category must contain both existing items:

### Duct Tape

Preserve existing gameplay functionality.

- Hold-to-heal.
- Correct inventory consumption.
- Cannot shoot while healing.
- Correct purchase price.
- Correct inventory display.
- Preserve any implemented animation and sound.

### Faintanyl

This is an existing fictional satirical game consumable.

Requirements:

- Available exclusively through the Medical shop category.
- Correct purchase handling.
- Correct inventory tracking.
- Preserve its existing gameplay effects.
- Preserve the game's fictional, exaggerated presentation.

Retain the existing description:

"relax its not addictive, just habit forming"

Do not make the item appear in unrelated categories.

Do not create duplicate instances of the item in the purchasing system.

### Medical UI

Give the Medical category a distinctive but consistent visual identity.

Medical items must still share the shop's general border, font, layout and sprite style.

---

# 9. ARMOR, HELMETS AND CONSUMABLE DISPLAY

Improve inventory presentation for existing protective equipment.

### Armor

Support the established light and heavy armor types.

Show:

- Ownership.
- Current equipment.
- Relative protection.
- Relevant trade-offs.

Remember the established gameplay philosophy:

**Armor never slows player movement.**

Any existing trade-offs involve modest reserve ammunition, grenade capacity and reload-duration adjustments.

Do not rebalance armor during this UI sprint.

### Helmets

Apply the same display principles.

Keep equipment clearly distinguishable.

### Grenades

Support the existing:

- Fragmentation grenade.
- Flashbang.
- Smoke grenade.

Show quantity and selected type.

Do not redesign grenade aiming or throwing mechanics.

### Revival injections

Preserve:

- Maximum inventory of two.
- Shop-only acquisition.
- Existing revival behavior.
- Two-second post-revival invulnerability.
- No automatic in-level refills.

Keep the interface synchronized with the existing inventory system.

---

# 10. DEVELOPMENT PLAYTESTING MONEY — MANDATORY

**The player must have access to 10,000 credits for testing the arsenal and shop.**

This is a hard requirement.

Do not assume the previous sprint implemented it successfully without verification.

### Requirements

1. New development saves begin with 10,000 credits.
2. Existing development saves below 10,000 receive a one-time top-up to at least 10,000.
3. Existing balances above 10,000 remain unchanged.
4. The credits must be visible in the shop.
5. They must be spendable immediately.
6. Purchases must subtract credits correctly.
7. Restarting the game must preserve the remaining balance.
8. Reloading a save must not repeatedly award 10,000 credits.
9. The implementation must use a development-only configuration.
10. Release builds must not automatically grant development money.

Do not alter the final intended economy.

**The temporary 10,000-credit allowance is strictly for playtesting.**

Do not rebalance weapon prices, reward quantities or economic progression during this sprint.

Final economy balancing should occur after the game's weapons and gameplay are sufficiently tested.

In the eventual final economy, the player should feel moderate financial pressure while still being able to obtain premium weapons through ordinary progression without excessive grinding.

---

# 11. PAUSE MENU REDESIGN

The existing pause menu must be visually consistent with the main menu and gameplay HUD.

### Requirements

- Clearly visible pause state.
- Resume button.
- Existing settings access.
- Return-to-menu functionality if already supported.
- Readable text.
- Consistent pixel-art design.
- Correct mobile touch support.

### Functionality

Verify that pausing correctly handles:

- Gameplay time.
- Enemy behavior.
- Player input.
- Temporary animations.
- Audio behavior.
- Joystick input state.

Resuming must not leave buttons in a permanently held state.

Opening and closing pause repeatedly must not duplicate UI elements.

Do not allow paused gameplay to continue receiving unintended combat input.

---

# 12. PLAYER DEATH AND REVIVAL PRESENTATION

Redesign the death and revival interface.

### Death presentation

Keep the existing player ragdoll system.

Do not replace or substantially rewrite it.

Use a short, readable, stylized transition from gameplay into the death interface.

The presentation should fit the game's absurd, satirical tone.

Avoid excessively long delays.

### Revival availability

If a revival injection is available:

- Clearly communicate that revival is possible.
- Display the remaining revival inventory.
- Use the existing revival mechanics.
- Preserve the two-second invulnerability period.
- Prevent duplicate or accidental revival activation.

If no revival is available, display the appropriate game-over options.

### Game-over interface

The player should understand:

- That the run has ended.
- Which options are available.
- How to restart.
- How to return to the menu.

Preserve the no-checkpoints design.

Avoid creating a new checkpoint or automatic free revival system.

---

# 13. VICTORY AND LEVEL-COMPLETION SCREEN

Improve the end-of-level results presentation.

When Level 1 is completed, show clear progression feedback.

### Required information

Display supported existing values such as:

- Mission completed.
- Currency collected.
- Mission reward.
- Total earnings.
- Updated player balance.

Include optional performance information only if the current systems already track it reliably.

Do not invent data or add unnecessary statistical tracking systems.

### Visual presentation

Use bold, chunky text.

Add a concise, satisfying completion transition.

Avoid excessively elaborate animations.

The result screen should feel rewarding, but it must not prevent the player from quickly returning to gameplay or shopping.

### Persistence

Ensure all earned currency is recorded correctly.

Do not duplicate mission rewards when reopening the results screen.

Avoid repeated payouts caused by scene reloads or accidental double submission.

---

# 14. GAMEPLAY FEEDBACK IMPROVEMENTS

This section addresses visual clarity during combat.

It should not substantially alter weapon mechanics.

### Damage feedback

Improve readability when the player receives damage.

Use short visual feedback rather than heavy full-screen effects.

Avoid obscuring enemies or making the scene difficult to read.

### Weapon feedback

Audit existing:

- Muzzle flashes.
- Bullet tracers.
- Impact particles.
- Reload feedback.
- Empty-magazine indicators.

Fix inconsistencies where visible.

Do not increase particle counts simply for spectacle.

Do not rewrite the hitscan implementation.

### Officer's special attacks

Preserve:

- The distinctive thicker bullet tracer.
- Different tracer color from ordinary weapons.
- The 0.5-second laser telegraph.
- Clear alignment between laser and subsequent shot.

The telegraph must remain readable amid other effects.

### Healing feedback

Make it obvious when healing starts, continues, and finishes.

Preserve the existing Duct Tape action.

Do not add a complex new healing mechanic.

### Pickup feedback

Make collection events readable without creating UI spam.

Several pickups in quick succession should not produce dozens of overlapping messages.

---

# 15. DIALOGUE BUBBLE VISUAL POLISH

Redesign the appearance of existing enemy speech bubbles.

Do not introduce a new dialogue-generation system.

### Visual direction

- Chunky outlined boxes.
- Consistent pixel fonts.
- Readable text.
- Modest padding.
- Simple placement above enemies.
- Clear contrast against backgrounds.
- Appropriate scaling for Android.

### Existing constraints

Preserve the maximum of TWO visible dialogue bubbles.

Combat-related communication must have priority over nonessential banter.

Avoid bubbles covering the player's most important aiming area whenever possible.

Offscreen dialogue should not create unnecessary on-screen UI.

Do not implement the planned full NPC-to-NPC conversation system yet.

That system should remain deferred until the planned enemy roster is sufficiently complete.

---

# 16. SOUND AND MUSIC PRESENTATION

Audit the existing audio integration without expanding the soundtrack.

Level 1 already has a small set of music tracks.

Three tracks are sufficient for now.

### Requirements

- Existing music plays correctly.
- Existing transitions are not abrupt or broken.
- Pause and resume behave consistently.
- Menu audio does not accidentally overlap gameplay music.
- Restarting the level does not create duplicate music players.
- Any implemented button sounds match the UI interactions.
- Sound feedback should be brief and unobtrusive.

Do not compose new music.

Do not create a huge audio asset pipeline.

Do not introduce a complicated adaptive music system.

Focus on correct use of the existing audio assets.

---

# 17. ADAPTIVE UI FOR ANDROID SCREEN SIZES

The interface must behave correctly on different aspect ratios.

Do not design exclusively around a single editor resolution.

### Test representative layouts

- 16:9.
- 18:9.
- 19.5:9.
- 20:9.
- A tablet-like aspect ratio where feasible.

### Requirements

- No clipped buttons.
- No unreadable text.
- No controls outside the safe area.
- Correct handling of camera cutouts.
- No overlapping joysticks and menus.
- Proper scaling of weapon selection.
- Correct shop scrolling.
- Readable item prices.
- Correct landscape orientation.
- Consistent HUD positioning.

Respect the device's safe area.

Use appropriate anchors and scaling instead of fragile hardcoded positions.

Support PC layouts without unnecessarily duplicating the entire UI architecture.

### Touch target philosophy

Visual buttons may be relatively compact.

Their interactive areas must remain large enough to operate reliably.

Do not make gameplay harder just to achieve a prettier screenshot.

---

# 18. TRANSITIONS AND GENERAL INTERFACE RESPONSIVENESS

Improve transitions between the game's existing states.

Relevant transitions include:

- Startup to main menu.
- Main menu to shop.
- Shop to equipment selection.
- Menu to mission.
- Mission to pause.
- Pause to mission.
- Mission to death.
- Death to restart.
- Mission completion to results.
- Results to menu or shop.

### Principles

Transitions should be:

- Short.
- Clear.
- Reliable.
- Visually consistent.
- Interrupt-safe.
- Free of unnecessary delays.

Use simple pixel-style fades, slides or panel changes where appropriate.

Avoid expensive blur or complex shader-driven transitions.

### State correctness

Ensure:

- The current screen is clearly defined.
- Previous interfaces do not remain active invisibly.
- Gameplay input is disabled when appropriate.
- Buttons cannot trigger duplicate actions.
- Returning from a menu preserves correct state.
- Reopening the shop does not duplicate item entries.
- Restarting the level does not leak UI state from the previous run.

---

# 19. CLEANUP OF PLACEHOLDERS AND INCONSISTENCIES

Perform a targeted audit of the game's current visual interface.

Identify:

- Placeholder icons.
- Inconsistent font sizes.
- Mismatched panel styles.
- Unnecessary text.
- Poorly aligned labels.
- Excessively large buttons.
- Unreadable small text.
- Inconsistent colors.
- Incorrect item imagery.
- Duplicated UI components.
- Buttons with unclear purposes.
- Inconsistent tooltip formatting.

Replace or repair only the elements relevant to this sprint.

Avoid rewriting unrelated systems.

### Important design judgment

Do not interpret "handmade" as "sloppy."

The player should perceive the visual imperfections as charming artistic decisions, not broken interface design.

A crudely drawn icon is acceptable.

A button that cannot be pressed reliably is not.

---

# 20. SAVE SYSTEM AND MENU REGRESSION TESTING

UI overhauls frequently introduce state-management bugs.

Test existing save integration carefully.

### Scenarios

**Scenario A — New game**

- Start a new development save.
- Verify starting inventory.
- Verify 10,000 development credits.
- Open shop.
- Purchase an item.
- Enter Level 1.
- Confirm purchased equipment is available.

**Scenario B — Existing save**

- Load an existing development save.
- Verify the one-time currency top-up if needed.
- Preserve existing owned equipment.
- Purchase another item.
- Restart the application.
- Verify the balance and inventory remain correct.

**Scenario C — Death**

- Enter a level.
- Lose all health.
- Verify revival availability.
- Use a revival if available.
- Verify correct transition and remaining inventory.
- Repeat without available revivals.
- Verify game-over behavior.

**Scenario D — Victory**

- Complete Level 1.
- Collect earnings.
- Open results screen.
- Return to shop.
- Verify updated currency.
- Replay the mission.
- Confirm rewards are not duplicated incorrectly.

**Scenario E — Repeated navigation**

Repeatedly navigate between:

- Main menu.
- Settings.
- Shop.
- Equipment.
- Gameplay.
- Pause.
- Results.

Check for stale controls, duplicate objects, missing references and input failures.

---

# 21. COMBAT REGRESSION TESTING

Although this sprint is visually focused, verify the existing major mechanics.

Do not change them unless necessary to repair a discovered regression.

### Player movement

- Running.
- Jumping.
- Double jumping.
- Crouching.
- Ladders.
- Sliding, if implemented.
- Parachute deployment.
- Parachute landing.

**Critical rule: Landing with an active parachute must produce exactly zero fall damage.**

### Combat

- Standard pistol firing.
- Desert Eagle firing.
- Sniper rifle aiming and firing.
- Reloading.
- Ammunition counting.
- Cover interactions.
- Blind firing.
- Grenades.
- Healing.
- Revivals.
- Armor protection.

### Enemy behavior

- Larper reactions.
- Samurai aggression.
- Fireworks expert behavior.
- Officer aiming and bunker usage.
- Suppression.
- Enemy navigation.

Avoid making changes to these systems merely because they are encountered during testing.

Report unrelated problems separately unless they prevent validation.

---

# 22. PERFORMANCE VALIDATION MATRIX

Inspect performance before and after implementation where tooling allows.

### Required scenarios

**A. Main menu**

Check for unnecessary UI animation overhead or repeated rebuilds.

**B. Shop**

Navigate rapidly across categories.

Verify no severe UI spikes.

**C. Quiet gameplay**

Move through an empty or low-activity section of Level 1.

Check frame pacing.

**D. Ordinary combat**

Engage one or two Larpers.

Check input latency and UI responsiveness.

**E. Chaotic combat**

Exercise a representative high-load encounter using existing enemies, particles and ragdolls.

Check for abnormal frame-time spikes.

**F. Death and restart**

Verify proper cleanup of visual effects and UI objects.

**G. Repeated scene navigation**

Look for increasing memory usage or duplicated UI instances.

### Reporting

Report actual measured results where available.

Do not invent frame-rate measurements.

If Android profiling cannot be performed in the execution environment, label the corresponding checks as requiring physical-device validation.

Avoid unrelated performance refactors.

---

# 23. IMPLEMENTATION STRATEGY

This is a substantial sprint. Execute it in a controlled order rather than changing everything simultaneously.

### Phase A — Audit

Inspect the current:

- Main menu.
- UI architecture.
- HUD prefabs.
- Shop prefabs.
- Inventory display.
- Save integrations.
- Canvas structure.
- Input handling.
- UI assets and fonts.

Identify reusable systems.

### Phase B — Shared design system

Create or refine shared:

- Panel styles.
- Button styles.
- Typography conventions.
- Icon treatment.
- Pixel-art borders.
- Color palette.
- UI spacing rules.

### Phase C — Menu and shop

Implement the main menu, settings, pause, and shop redesign.

Verify shop purchase correctness.

### Phase D — Gameplay HUD

Implement health, ammunition, currency, revival and objective visual improvements.

Preserve mobile controls.

### Phase E — Results and failure presentation

Implement death, revival and completion screen improvements.

Verify state transitions.

### Phase F — Feedback

Refine existing pickups, combat feedback, dialogue bubbles and audio presentation.

### Phase G — Validation

Run available automated and editor tests.

Fix regressions.

Verify persistence and performance-sensitive changes.

Prioritize correctness over decorative completeness.

If time or implementation scope becomes constrained, complete the shared design system, main menu, shop, core HUD and development-credit verification before optional decorative effects.

Do not leave critical gameplay interfaces half-integrated.

---

# 24. STRICT SCOPE EXCLUSIONS

This sprint must NOT:

- Create Level 2.
- Add enemy archetypes.
- Add new firearms.
- Rewrite enemy AI.
- Rebuild the physics engine.
- Change existing ragdoll architecture.
- Add a multiplayer system.
- Introduce online features.
- Introduce monetization.
- Add daily rewards.
- Implement battle passes.
- Add loot boxes.
- Implement the full NPC banter system.
- Create more soundtrack compositions.
- Overhaul the final economy.
- Change established weapon behavior.
- Introduce new checkpoints.
- Replace proven performance optimizations.

This is a presentation and usability sprint.

Its purpose is to make existing features feel cohesive and complete.

---

# 25. FINAL ACCEPTANCE CRITERIA

Sprint 0036 is complete when:

### Visual consistency

- [ ] Main menu matches AKUMU 2's handmade pixel-art direction.
- [ ] HUD has a coherent chunky visual style.
- [ ] Shop matches the rest of the game.
- [ ] Pause menu matches the main menu.
- [ ] Weapon icons have consistent presentation.
- [ ] Medical and armor interfaces are visually consistent.
- [ ] Death and victory screens are properly styled.
- [ ] Dialogue bubbles match the game's aesthetic.

### Functionality

- [ ] Main menu navigation works.
- [ ] Shop purchases work.
- [ ] Currency displays correctly.
- [ ] Development saves have access to 10,000 testing credits.
- [ ] Development money cannot be repeatedly duplicated on reload.
- [ ] Weapon inventory and equipment persist.
- [ ] Medical items appear in the correct category.
- [ ] Ammunition HUD updates correctly.
- [ ] Revival HUD correctly displays a maximum of two injections.
- [ ] Pause and resume work reliably.
- [ ] Death and revival screens function.
- [ ] Victory screen displays correct rewards.
- [ ] Existing gameplay controls remain functional.

### Mobile quality

- [ ] Interface scales correctly on representative phone aspect ratios.
- [ ] Buttons are comfortable to press.
- [ ] Joysticks remain responsive.
- [ ] No major UI elements overlap.
- [ ] Safe areas are respected.
- [ ] Multitouch works.
- [ ] HUD remains readable during intense combat.

### Technical quality

- [ ] No introduced compiler errors.
- [ ] No significant new UI-related runtime errors.
- [ ] No newly broken save behavior.
- [ ] No known purchase duplication bugs.
- [ ] No major new gameplay regressions.
- [ ] No unnecessary expensive rendering effects.
- [ ] Existing optimization improvements are preserved.
- [ ] Available tests are executed and their results documented.

---

# 26. FINAL CODEX DELIVERABLES

At the conclusion of the sprint, provide a detailed implementation report.

Include:

1. **Architecture audit:** Describe the original UI structure and important issues found.
2. **Files modified:** List relevant scripts, scenes, prefabs, UI assets and configuration files.
3. **Design system:** Describe shared visual decisions, asset reuse and how consistency was enforced.
4. **Main menu:** Explain what changed and how navigation works.
5. **HUD:** Explain health, ammunition, currency, revivals and objectives.
6. **Shop:** Describe layout improvements, purchase handling and category corrections.
7. **Development credits:** Confirm exactly how the 10,000-credit test allowance works on existing and new saves.
8. **State management:** Explain changes to pause, death, revival and victory interfaces.
9. **Performance:** Identify potentially expensive changes and provide real measurement results where available.
10. **Tests:** List tests executed, their outcomes and any untested areas.
11. **Android limitations:** Identify anything that still requires validation on a physical phone.
12. **Remaining issues:** Document unresolved problems clearly.

Do not claim features are implemented unless the relevant project code and assets were actually modified.

Do not claim tests passed unless they were executed.

Do not claim Android performance is preserved solely because a desktop editor build succeeds.

---

# FINAL SPRINT DIRECTIVE

AKUMU 2 already has the foundations of a distinctive game:

- Fast, responsive movement.
- A ridiculous cast of enemies.
- Satisfying gunplay.
- Suppression and cowardice-driven AI.
- Stylized ragdoll physics.
- Explosives and unusual weapons.
- A coherent satirical identity.
- An intentionally handmade pixel-art world.

The current mission is to make its interface belong to that same world.

The player should not feel as though they are switching between a handmade indie shooter and a generic Unity prototype every time they open a menu.

Every screen should feel like part of AKUMU 2.

**Build something charming, chunky, readable, fast and distinctly handmade.**

**Protect Android performance. Preserve established gameplay. Implement real improvements.**

# END OF SPRINT 0036