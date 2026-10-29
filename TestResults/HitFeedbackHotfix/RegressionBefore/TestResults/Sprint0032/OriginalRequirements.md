# AKUMU 2 — SPRINT 0032
## OPERATION: THE ENEMIES HAVE BRAINS (UNFORTUNATELY)
### Enemy Navigation, Awareness, Reaction Times, Combat Fairness & AI Reliability

**Project:** AKUMU 2 (悪夢 2)  
**Studio:** 黒橋 GAMES  
**Engine:** Unity 2D URP  
**Platforms:** Android and Windows  
**Previous sprint:** 0031 — Operation: Death to PowerPoint

---

# 0. SPRINT MISSION

This sprint is dedicated to repairing and improving enemy AI reliability.

AKUMU 2's enemies are deliberately flawed, cowardly, sometimes incompetent and frequently ridiculous.

That personality is essential.

However, there is an important distinction between an enemy behaving incompetently because of its personality and behaving incompetently because the game's AI is malfunctioning.

A Larper crouching behind cover and refusing to advance is funny and intentional.

A Larper running into an invisible wall for 30 seconds is a bug.

A Samurai aggressively jumping between platforms to pursue the player is intended.

A Samurai becoming trapped because it cannot find a reachable ledge is not.

An enemy needing a moment to realize the player has appeared is believable.

An enemy instantly detecting and attacking the player on the first frame is unfair.

An enemy standing on an upper platform and ignoring repeated gunshots below it is ridiculous for the wrong reasons.

**Our goal is to improve reliability, perception and believable behavior WITHOUT increasing the enemies' overall tactical competence.**

This is extremely important.

We are not making a realistic military AI simulator.

We are making a polished, satirical action game populated by dysfunctional combatants.

The enemies should behave consistently with their character archetypes.

Their mistakes should feel authored or emergent, not caused by technical failures.

---

# 1. AUDIT THE EXISTING AI FIRST

Before implementing changes, inspect the current AI architecture.

Identify:

- Enemy base classes and archetype-specific controllers.
- Enemy movement and navigation scripts.
- Platform and ladder detection logic.
- Current enemy state machines.
- Player detection and line-of-sight calculations.
- Gunshot awareness or noise detection, if implemented.
- Enemy attack initiation and reaction timers.
- Existing suppression and fear systems.
- Enemy spawn and activation logic.
- Existing camera-visibility restrictions.
- Cover selection and cover usage.
- Samurai pursuit and jump behavior.
- Officer bunker interaction.
- Existing CPU allocation and physics-query patterns.

Determine which systems already work.

Do not replace functioning components unnecessarily.

Prefer incremental improvements over a massive AI rewrite.

Make the new behavior configurable through appropriate serialized values or existing configuration structures.

Avoid duplicating perception or navigation systems across every enemy type.

Preserve all working animations, ragdolls and combat systems.

---

# 2. ENEMY REACTION TIMES — NO MORE CAT REFLEXES

Some enemies react too quickly when first encountering the player.

This is undesirable, especially for basic Larpers.

A poorly trained coward should not have the reaction time of an elite competitive shooter.

Introduce a clear distinction between:

1. Becoming aware of a possible threat.
2. Recognizing the player.
3. Preparing to engage.
4. Beginning an attack.

These stages should not necessarily require separate animations or complex states. Use the lightest implementation that fits the existing architecture.

## 2.1 Larper reaction time

Target approximately **0.7 seconds** between first confirmed visual detection of an unanticipated player and initiating the first attack.

Make this value configurable.

Suggested initial range:

- Minimum: 0.6 seconds.
- Typical: 0.7 seconds.
- Maximum: 0.9 seconds.

Avoid excessive randomization.

Reaction timing should vary slightly but remain predictable.

Do not delay every individual shot by 0.7 seconds.

This applies to the initial engagement reaction, not the weapon's normal firing cadence.

Once already engaged, enemies should continue following their existing combat behavior.

## 2.2 Samurai reaction time

The Samurai is a noticeably more aggressive enemy.

It should react faster.

Suggested initial visual recognition delay:

**0.25–0.4 seconds.**

Once engaged, the Samurai should aggressively pursue the player rather than waiting around.

Do not introduce unnecessary hesitation.

However, preserve fair and readable attack animations.

Do not eliminate existing attack wind-ups.

## 2.3 Fireworks expert

The fireworks expert should have a more moderate awareness delay.

Suggested starting range:

**0.5–0.8 seconds.**

Preserve its existing throwing mechanics, fuse variability and comedic personality.

Do not overhaul its damage or explosion balancing in this sprint.

## 2.4 Officer miniboss

The Officer should recognize the player reasonably quickly but remain extremely cowardly.

Do not confuse awareness with willingness to expose himself.

He may recognize the player while remaining hidden.

Preserve:

- His Desert Eagle mechanics.
- His distinctive tracer.
- His accurate but deliberately telegraphed attacks.
- The existing 0.5-second laser warning.
- His use of the bunker.
- His tendency to retreat into cover.

The Officer should not become an aggressive assault enemy.

## 2.5 Awareness is persistent

An enemy that has already identified the player should not repeatedly restart its initial reaction timer every time visibility flickers for a frame.

Distinguish initial surprise from continued combat awareness.

Briefly losing line of sight must not reset the enemy into complete ignorance.

---

# 3. NEW AWARENESS MODEL — ENEMIES HAVE EARS

One of the biggest problems to address is that enemies may ignore obvious nearby combat merely because they cannot directly see the player.

Example:

The player is standing beneath a platform and firing repeatedly.

An enemy is directly above.

There is no direct line of sight.

The enemy continues behaving as though nothing is happening.

This should be corrected.

## 3.1 Detection sources

Enemies should be capable of reacting to:

- Direct visual detection.
- Nearby gunfire.
- Nearby explosions.
- Projectile impacts, where useful and already supported.
- Nearby allied combat activity where clearly observable.
- The loss of nearby allies only if a credible sensory event is present.

Do not give enemies supernatural omniscience.

They must not instantly know the player's exact position simply because combat occurred somewhere in the level.

## 3.2 Hearing gunshots

Introduce or improve a lightweight noise-event system.

Prefer event-driven sound notifications rather than constant expensive physics scans.

When a relevant gunshot occurs, nearby eligible enemies may receive a noise event.

The event can contain:

- World position.
- Sound category.
- Relative loudness or hearing radius.
- Event timestamp.
- Optional source association, where appropriate.

This is an abstract gameplay system.

No physically accurate sound simulation is required.

## 3.3 Hearing across platforms

Vertical separation must not make enemies completely deaf.

If a player fires a gun on a lower platform, nearby enemies on an upper platform should be capable of hearing it.

Use actual world-space distance rather than restricting detection to the same horizontal platform.

Simple terrain occlusion or attenuation is acceptable if inexpensive.

A nearby wall should not necessarily make a loud shot inaudible.

However, avoid making every enemy in the entire level instantly aware.

## 3.4 Sound localization

Hearing should not equal seeing.

An enemy hearing a gunshot may:

- Stop idling.
- Look toward the approximate sound location.
- Become suspicious.
- Move into a defensive posture.
- Investigate if its archetype permits.
- Seek cover.
- Become more alert to subsequent sounds.

It should NOT automatically acquire perfect aim through a wall.

The enemy knows something happened near a location, not necessarily exactly where the player currently stands.

---

# 4. DEFINE CLEAR AWARENESS STATES

Implement or refine a lightweight awareness state model.

Suggested states:

### UNAWARE

Normal idle/patrol behavior.

Enemy has no meaningful knowledge of an immediate threat.

### SUSPICIOUS

Enemy heard something or noticed a possible threat.

It may look toward the disturbance or change posture.

Does not automatically attack without a valid reason.

### ALERT

Enemy has stronger evidence of danger or recently confirmed the player.

It prepares to respond according to its personality.

### ENGAGED

Enemy has positively identified the player and is actively participating in combat.

The enemy may fire, retreat, use cover or chase according to its archetype.

### SEARCHING

Enemy has lost visual contact but retains a recent approximate location.

Do not require all five states if the existing controller has an equivalent, simpler implementation.

Integrate with existing AI state machines rather than stacking contradictory systems.

## Memory and decay

Enemy awareness should not disappear instantly.

Suggested initial parameters:

- Suspicious memory: around 3–5 seconds.
- Recent player location memory: around 4–7 seconds.
- Brief visual contact interruption: should not reset engagement.
- Sustained loss of contact: gradually transition toward search or archetype-appropriate defensive behavior.

These are tunable starting points, not mandatory universal constants.

Never allow remembered positions to function as live player tracking.

---

# 5. GROUP AWARENESS — WITHOUT TELEPATHY

Enemies near one another should sometimes react to nearby combat.

For example, if one Larper begins firing while another stands a few meters away, the second should not necessarily remain oblivious.

However, we do not want a global hive mind.

Implement limited local awareness sharing if needed.

Requirements:

- Awareness propagation is local.
- It respects meaningful proximity.
- It does not provide perfect aim coordinates.
- It does not instantly alert the entire level.
- It does not bypass camera-visibility restrictions.
- It does not create recursive endless event propagation.

Avoid repeatedly rebroadcasting the same alert through every enemy.

A direct gunshot should have more informational value than a vague allied alarm.

The system should remain simple and inexpensive.

---

# 6. NAVIGATION RELIABILITY — STOP RUNNING INTO WALLS

This is another major priority.

Enemies must navigate the existing level reliably.

They do not need brilliant pathfinding.

They need to stop malfunctioning.

Review:

- Box collisions.
- Invisible collider boundaries.
- Platform edges.
- Ladder transitions.
- Obstacles.
- Narrow passages.
- Cover objects.
- Slopes or uneven ground, if present.
- Unreachable destinations.
- Dynamic Rigidbody2D interactions.

## 6.1 Stuck detection

Introduce a lightweight stuck-detection mechanism.

For example:

An enemy is considered potentially stuck when:

- It has a valid movement destination.
- It is actively attempting to travel.
- Its actual position barely changes.
- This persists for a reasonable duration.

Suggested initial threshold:

**1.5–2 seconds of negligible progress.**

Do not classify intentional stationary behavior as being stuck.

Examples of intentional stationary behavior:

- Hiding in cover.
- Crouching while reloading.
- Waiting for a suitable attack opportunity.
- Aiming.
- Idling.
- Performing an attack animation.

## 6.2 Stuck recovery

If genuinely stuck:

1. Reevaluate the destination.
2. Check whether the current movement direction is blocked.
3. Attempt a nearby reachable waypoint where applicable.
4. Recalculate or select a new local route.
5. If the destination is unreachable, abandon it gracefully.
6. Return to an appropriate safe AI behavior.

Do not make recovery overly dramatic.

Do not routinely teleport enemies.

Do not push them through walls.

Do not disable collision to force progress.

Do not introduce endless jumping.

## 6.3 Destination validation

Enemies should not spend unlimited time attempting to reach impossible positions.

Implement cheap reachability checks where appropriate.

A destination can be unreachable for a specific archetype even if another enemy type could reach it.

This is intentional.

Avoid repeatedly recalculating the same impossible route.

Use reasonable retry intervals.

---

# 7. ARCHETYPE-SPECIFIC NAVIGATION

Navigation must respect the personalities and physical capabilities of each enemy.

## 7.1 Larper

Larpers should remain clumsy and predominantly grounded.

They are not parkour specialists.

Preserve existing platform restrictions.

A Larper should not suddenly perform impressive precision jumps or complicated multi-platform pursuits merely because pathfinding improved.

Desired behavior:

- Moves reliably on its allowed surfaces.
- Avoids getting trapped against boxes.
- Can move toward nearby cover when reachable.
- Recognizes when a destination is inaccessible.
- Does not repeatedly walk off dangerous ledges.
- Does not endlessly oscillate between two points.

A Larper on an upper platform may hear the player shooting below, become frightened and seek defensive cover.

That does not mean it must leap down to confront the player.

**Improved awareness does not imply improved courage.**

## 7.2 Samurai

The Samurai should be the opposite in terms of pursuit.

This enemy is small, fast, aggressive and willing to cross platforms.

Its navigation must reflect that.

Review:

- Jump selection.
- Platform transitions.
- Detection of reachable upper ledges.
- Movement between adjacent platforms.
- Obstacle avoidance.
- Chase target updates.
- Recovery from failed jumps.
- Attacks after landing.

The Samurai should be able to pursue a player using the level's intended traversable geometry.

It must not permanently freeze when the player is above or below it.

However, do not grant impossible mobility.

Jumps must obey actual movement physics.

Do not teleport it between platforms.

Do not allow it to jump through solid obstacles.

Do not let it cross spaces that exceed its physical capabilities.

## 7.3 Fireworks expert

Preserve the existing archetype's throwing behavior.

Improve navigation only where needed.

This enemy should not become an expert flanker.

It should generally seek reasonable positions from which its current attack behavior can function.

Preserve accidental chaotic behavior that is part of its established design.

Fix genuine movement bugs, not comedic personality flaws.

## 7.4 Officer

The Officer's bunker is his primary defensive location.

Ensure the navigation system recognizes valid positions around the bunker.

He should be able to:

- Remain protected behind appropriate cover.
- Reach legitimate firing/peeking positions.
- Retreat to the protected area.
- Avoid becoming permanently stuck against bunker collision.
- React when the player changes position.
- Stay active after other enemies are defeated.

Do not let him leave the bunker and turn into a conventional assault enemy unless the existing behavior explicitly requires it.

Do not allow bullets to ignore the bunker cover geometry.

Do not turn the Officer into a stationary turret.

His cowardice must remain an intentional gameplay feature.

---

# 8. PLATFORM GRAPH AND NAVIGATION ARCHITECTURE

If the current navigation approach cannot reliably handle multi-level terrain, introduce a lightweight platform-aware representation.

Possible model:

- Walkable platform segments.
- Reachable destinations.
- Jump connections for eligible archetypes.
- Ladder connections where appropriate.
- Cover access points.
- Valid stopping positions.

However:

**Do not introduce a huge navigation framework unless the existing system truly needs one.**

Prefer adapting the current solution.

Different archetypes may have different permitted connection types.

For example:

- Larper: mainly same-platform movement.
- Samurai: walking and permitted jump connections.
- Officer: bunker-local cover and peeking routes.

Do not require every enemy to use the same traversal abilities.

Navigation data should be cached or generated efficiently rather than rebuilt every frame.

Avoid expensive full-level path searches every time the player moves slightly.

If a new navigation graph is created, make debugging and level-authoring practical.

---

# 9. CAMERA VISIBILITY AND FAIRNESS

This is a hard design rule.

Ordinary enemies should not suddenly attack the player from outside the visible gameplay camera.

Preserve the established encounter fairness philosophy.

The player should generally be able to see ordinary enemies that are actively attacking.

Future specialist archetypes such as snipers or mortars may have deliberate exceptions.

Those exceptions are NOT part of this sprint.

## Important distinction

Enemies may become aware of combat while offscreen.

But awareness is not permission to begin unfair offscreen attacks.

For example:

An offscreen Larper may hear gunfire and become alert.

It should not begin accurately shooting the player from an unseen location.

If an enemy is barely entering the camera, apply appropriate activation and attack rules to avoid a single-frame surprise shot.

Do not create infinite waiting states or repeatedly reset reaction timers at camera boundaries.

Maintain sensible state continuity.

## Spawning and activation

Review existing spawn activation.

Ensure:

- Enemies do not appear visibly out of thin air.
- Previously activated enemies do not unnecessarily reset their AI state.
- Enemy spawning does not produce sudden unfair crossfires.
- Existing Level 1 encounter spacing is preserved.
- The current enemy roster is unchanged.
- Activation does not cause major frame spikes.

Do not redesign the entire spawning system unless required to correct a demonstrated bug.

---

# 10. SUPPRESSION AND FEAR MUST STILL MATTER

AKUMU 2's suppression system is fundamental to the enemy personality.

Do not allow new awareness and navigation logic to override fear behavior.

For Larpers, suppression and panic should often take priority over aggressive pursuit.

When threatened, their existing behaviors may include:

- Defensive crouching.
- Retreating.
- Panic responses.
- Seeking nearby cover.
- Inaccurate return fire.
- Hesitation.
- Losing confidence.

These are intended behaviors.

Do not eliminate them.

## Priority architecture

Define clear arbitration between states such as:

1. Inactive/death state.
2. Existing forced animation or valid recovery state.
3. Immediate survival/fear response.
4. Ongoing attack or weapon handling.
5. Search/investigation.
6. Normal navigation.
7. Idle behavior.

The exact ordering may vary by archetype and existing logic.

The key requirement is to avoid conflicting commands.

For example, an AI should not simultaneously:

- Retreat left.
- Chase right.
- Crouch in cover.
- Attempt to climb a ladder.

One system should have clear control at a time.

## Preserve suppression accumulation

Maintain the established near-miss and impact suppression mechanics.

Do not accidentally reset suppression when the AI changes its awareness state.

Do not remove the existing accumulation and decay behavior.

Do not replace fear with a generic combat alert meter.

---

# 11. PREVENT UNFAIR ENEMY ACCURACY

This sprint must not accidentally buff enemy shooting accuracy.

Improved target detection and navigation can make encounters more challenging even without increasing damage.

Do not stack accidental difficulty buffs.

Specifically:

- Keep existing weapon accuracy values unless correcting a documented bug.
- Keep existing ammo capacities.
- Preserve current reload times.
- Preserve weapon fire rates.
- Preserve existing damage values.
- Preserve the Officer's special telegraph.
- Do not give enemies omniscient aiming through solid walls.
- Do not grant enemies instantaneous target acquisition.

A frightened Larper remains a bad shot.

A Samurai remains dangerous primarily because of mobility and aggression.

The Officer remains dangerous through accurate, readable individual attacks.

Different enemies must retain different combat identities.

---

# 12. PLAYER POSITION MEMORY

Implement believable last-known-position behavior.

If the player briefly disappears behind cover:

- The enemy should not instantly forget the encounter.
- It may remember the approximate last observed position.
- It may continue an existing defensive response.
- It may investigate or reposition if permitted by its archetype.
- It must not know where the player has secretly moved.

When the player reappears, avoid unnecessarily restarting the full first-contact reaction delay if the enemy remains combat-alert.

Conversely, do not allow permanent perfect tracking.

Player position updates should be based on valid sensory information.

Hearing should provide approximate information.

Direct line of sight should provide more precise information.

Remembered positions should become stale.

The goal is understandable behavior, not realistic military intelligence.

---

# 13. LINE-OF-SIGHT CORRECTIONS

Review all enemy sight checks.

Common issues to look for:

- Rays originating from the wrong point.
- Raycasts blocked by an enemy's own collider.
- Cover objects being ignored.
- Decorative objects accidentally blocking sight.
- Incorrect layer masks.
- Player partially visible through a platform.
- Detection flickering at platform edges.
- Sprites appearing visually exposed while collision logic treats them as hidden.

Fix actual inconsistencies.

Prefer a small number of meaningful visibility checks rather than many costly physics queries every frame.

Maintain consistency between what the player sees and what the AI can detect.

Ensure the Officer's bunker behaves as genuine cover.

Do not conflate camera visibility with physical line of sight.

An enemy can be on camera while blocked by cover.

An enemy can be physically capable of seeing the player but still be prohibited from initiating an unfair offscreen attack.

These are separate checks.

---

# 14. AI DEBUGGING TOOLS

Create or improve lightweight developer-facing AI debugging.

This is important because AI problems are often difficult to reproduce.

Useful optional debugging information:

- Current AI state.
- Current awareness state.
- Current destination.
- Whether the player is visually detected.
- Last known player position.
- Recent noise-event position.
- Reaction timer.
- Whether the enemy is considered stuck.
- Current navigation target.
- Current cover target.
- Suppression state.

Use gizmos or editor-only debug views where appropriate.

Do not make these overlays appear in normal production builds.

Avoid excessive runtime logging.

Debug tools should help answer questions such as:

"Why isn't the Samurai chasing me?"

"Why does the Larper know I'm here?"

"Why is the Officer stuck in the bunker?"

"Why is this enemy trying to cross an unreachable platform?"

Keep the debugging system small and maintainable.

---

# 15. PERFORMANCE REQUIREMENTS

The game targets Android hardware.

AI improvements must remain lightweight.

Avoid:

- Full pathfinding searches every frame.
- Allocating new path lists constantly.
- Repeated expensive physics scans for every enemy.
- Large numbers of unnecessary coroutines.
- Per-frame string construction for debug messages.
- Excessive global event broadcasts.
- Rebuilding platform navigation data continuously.
- Complex behavior trees that add little value.

Prefer:

- Cached references.
- Event-driven hearing.
- Reasonable perception update intervals.
- Lightweight state transitions.
- Shared navigation utilities.
- Existing movement systems.
- Cheap stuck-detection checks.
- Simple local path reevaluation.

Do not prematurely optimize trivial operations.

But make sure the new implementation scales to future levels containing more enemy archetypes.

Test with the existing Level 1 population and a modest additional editor-only stress scenario if practical.

The player should not experience obvious stutters when several enemies become alert simultaneously.

---

# 16. LEVEL 1 VALIDATION SCENARIOS

Use the existing Level 1 as the main testing environment.

Do not redesign its layout.

Test the following cases.

## Scenario A — Ordinary Larper surprise

Player approaches an unaware Larper.

Expected:

- Larper detects the player.
- Brief recognition delay occurs.
- Larper begins its normal combat behavior.
- No instantaneous first-frame firing.
- Existing fear and cover behavior remain possible.

## Scenario B — Upper-platform awareness

Player fires repeatedly below an enemy.

Expected:

- Nearby enemy hears the activity.
- Enemy becomes suspicious or alert.
- Enemy reacts according to archetype.
- Enemy does not automatically fire through the platform.
- Enemy does not receive perfect target tracking.

## Scenario C — Samurai pursuit

Player moves between reachable platforms.

Expected:

- Samurai reacts quickly.
- Samurai attempts valid traversal.
- Samurai can complete reachable jumps.
- Samurai does not become permanently stuck.
- Samurai continues its intended aggressive behavior.

## Scenario D — Blocked Larper

Larper attempts to move toward reachable cover but encounters an obstacle.

Expected:

- Does not push into the obstacle indefinitely.
- Recognizes lack of progress.
- Attempts a valid alternate route if one exists.
- Otherwise abandons the destination.
- Remains within the archetype's traversal restrictions.

## Scenario E — Officer bunker

Officer engages from its established bunker.

Expected:

- Bunker correctly blocks shots.
- Officer can reach valid firing positions.
- Officer retreats appropriately.
- Officer does not become trapped.
- Existing laser telegraph remains 0.5 seconds.
- Existing distinctive tracer is preserved.

## Scenario F — Lost visual contact

Player breaks line of sight.

Expected:

- Enemy retains short-term encounter awareness.
- Enemy does not track the player's hidden movement perfectly.
- AI does not flicker rapidly between states.
- Behavior remains consistent with its archetype.

## Scenario G — Offscreen fairness

Enemy becomes aware of gunfire while not visible to the camera.

Expected:

- Awareness may increase.
- Enemy does not begin an unfair offscreen attack.
- Camera activation behavior remains stable.
- No instant shot on entering the visible area.

## Scenario H — Suppression priority

Larper is engaged and receives strong suppression.

Expected:

- Fear behavior can override offensive navigation.
- Larper does not continue confidently chasing despite panic.
- Existing retreat limitations remain intact.
- Suppression values are preserved across state changes.

## Scenario I — Multiple simultaneous enemies

Several enemies detect a disturbance.

Expected:

- AI remains responsive.
- No state-machine oscillation.
- No recursive alert storm.
- No sudden severe CPU spike.
- Enemies retain independent, archetype-specific reactions.

---

# 17. IMPLEMENTATION PHASES

Follow this order.

### Phase 1 — Audit

Inspect existing AI, sight, movement, spawn and fear systems.

Identify specific failure points.

### Phase 2 — Awareness

Implement or correct hearing, suspicion, memory and detection transitions.

Ensure upper/lower-platform awareness works.

### Phase 3 — Reaction timing

Introduce appropriate initial recognition delays.

Preserve attack cadence once combat is underway.

### Phase 4 — Navigation

Fix stuck detection, destination validation and recovery.

Improve platform traversal only for eligible archetypes.

### Phase 5 — Archetype integration

Verify:

- Larper cowardice.
- Samurai aggression.
- Fireworks expert's established mechanics.
- Officer's bunker behavior.

### Phase 6 — Combat fairness

Verify camera activation, attack permissions and line of sight.

Prevent offscreen attacks from ordinary archetypes.

### Phase 7 — Debugging and testing

Add lightweight debug visibility.

Perform focused regression tests.

### Phase 8 — Performance pass

Inspect avoidable allocations and expensive repeated queries.

Keep systems suitable for Android.

---

# 18. ACCEPTANCE CRITERIA

Sprint 0032 is complete when:

- [ ] Larpers no longer fire instantly on first detecting an unsuspecting player.
- [ ] Initial Larper visual reaction time targets approximately 0.7 seconds.
- [ ] Samurai reacts noticeably faster than Larpers.
- [ ] Enemies can react to nearby gunfire across vertical platforms.
- [ ] Hearing does not provide perfect target tracking.
- [ ] Enemy awareness persists reasonably after brief visual interruption.
- [ ] Awareness transitions do not constantly flicker.
- [ ] Enemies no longer become permanently trapped against ordinary obstacles.
- [ ] Unreachable destinations can be abandoned gracefully.
- [ ] Samurai can traverse valid multi-platform routes.
- [ ] Larpers do not gain unrealistic traversal abilities.
- [ ] Officer continues using his bunker correctly.
- [ ] Existing fear and suppression systems remain effective.
- [ ] Ordinary enemies cannot launch unfair offscreen attacks.
- [ ] Existing weapon and damage balancing is preserved.
- [ ] No global psychic enemy awareness is introduced.
- [ ] Multiple enemies can react simultaneously without obvious performance degradation.
- [ ] No major new Unity errors are introduced.
- [ ] Existing Level 1 remains completable.
- [ ] Existing Android and PC input systems remain intact.
- [ ] The UI improvements from Sprint 0031 remain intact.

---

# 19. IMPORTANT NON-GOALS

Do not use this sprint to:

- Redesign Level 1.
- Add enemy archetypes.
- Create Level 2.
- Rebalance every weapon.
- Change player physics.
- Rebuild the UI.
- Create new music.
- Implement enemy conversations.
- Add sophisticated squad tactics.
- Give enemies tactical flanking intelligence.
- Make Larpers more courageous.
- Introduce teleporting AI.
- Implement expensive realistic sound propagation.
- Rewrite unrelated gameplay systems.

Fix reliability, not personality.

If an unrelated problem is encountered, document it unless it directly blocks successful implementation.

---

# 20. FINAL HANDOFF REQUIREMENTS

After implementation, provide:

1. Summary of AI changes.
2. Files and scripts modified.
3. Description of new perception logic.
4. Description of reaction timing implementation.
5. Explanation of navigation fixes.
6. Details of any new serialized tuning parameters.
7. Regression testing results.
8. Performance considerations.
9. Known remaining AI limitations.
10. Clear manual playtesting instructions.

Distinguish verified working features from features that could not be tested.

Do not claim an Android device test was completed if no device was available.

Do not claim all navigation problems are solved if only a subset of routes was verified.

## FINAL DESIGN PHILOSOPHY

AKUMU 2 is populated by absurd characters, not tactical geniuses.

The Larper is a frightened amateur.

The Samurai is a relentless little menace.

The fireworks expert is chaotic and unpredictable.

The Officer believes he is a military mastermind while spending most of his time hiding in a bunker.

**Do not erase these personalities through over-engineering.**

Instead, make their behavior reliable enough that their incompetence becomes funny for the RIGHT reasons.

An enemy should lose a fight because it panicked, hesitated or made a ridiculous decision.

Not because it couldn't figure out how to walk around a wooden box.

The player should feel that the enemies inhabit the same physical world, hear the same disturbances and react to events in understandable ways.

The experience should remain chaotic, challenging, fair and funny.

**We are not making the enemies smarter at fighting. We are making them better at being themselves.**

Implement Sprint 0032 now.

**END OF SPRINT 0032 — OPERATION: THE ENEMIES HAVE BRAINS (UNFORTUNATELY)**