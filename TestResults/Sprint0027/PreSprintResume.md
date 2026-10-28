# AKUMU 2 — Level 1 refinement checkpoint

**Resumed 2026-10-25; implementation and targeted verification complete.**
Continue from this repository. Do not regenerate Sprint0028 or revert the existing dirty working tree. No staging, commits or pushes authorized.

## Objective and exact pacing authority

Complete LEVEL 1 COMBAT & ENEMY AI HOTFIX while preserving the remade, mostly horizontal introductory First Operation. Original requirements and acceptance criteria are preserved verbatim below.

User clarification on resume: **"Use the documented brief constraints".** No separate numerical simultaneous-attacker cap applies. Required limits: approximately15–20% more enemies with sensible integer rounding; additions distributed; opening trio unchanged and comfortably separated; no unavoidable crossfire or large dangerous-type opening groups; retain gradual introductory progression, geometry, cover, routes and existing archetypes.

Implemented13 ->15 (+15.38%); groups [3,4,3,3,2],12 Larpers plus3 specialists. Added one Cowardly Larper at x10/floor0 and one Ordinary Larper at x129/floor3.2. No other encounter changes. Emergent pacing/fairness still needs human playtesting.

## Completed implementation

- Native Editor helper applied and saved Warehouse_Demo; geometry snapshots match. No FirstOperationSetup regeneration.
- Mission-only OperationSamurai prefab/archetype plus placed-actor overrides: speed9.5, reaction.02+jitter.02, recovery.42, platform sample.2, jump cooldown.85; pursuit resumes during strike recovery if target escapes. Existing collider, validated navigation, damage/HP, jump speed15 and .4s wind-up unchanged.
- Officer scoped Desert Eagle42damage/2.4s interval/7mag/4s reload/1.5impulse; existing compact pistol graphics/sounds reused, extended magazine removed. Original Larper HP retained. No dedicated new Desert Eagle art/audio supplied.
- Red laser every shot:1s warning, last.2s locks direction; hitscan uses same committed direction, clips at solid geometry. Explicit cooldown2.4 plus.8 while assigned ally alive within14units; close retreat6, retry2.5–4, preferred distance10, cover search9, withdrawal7, reposition chance.9, Cowardly profile.
- Focused retreat test exposed blocked walking fallback beside low cover. Officer now checks existing validated hop corridors when the walking retreat is too short; original minimum retreat/hop limits retained, no teleport/new navigator.
- Expert keeps target/fuse selected at preparation and releases despite movement, sight loss or missed combat ticks. Damage/death/disabled combat/defeated target/offscreen permissions still safely cancel. Existing variable fuses, misses, malfunction and pool remain.
- Operation blast opt-in maximum100/radius2.8/impulse4; continuous normalized damage100/85/50/20/0 at0/.25/.5/.75/1; configurable minimum threshold0. Boundary/outside center damage0. Existing friendly/self damage, armor, LOS, VFX and separately weighted impulse retained.
- Actual Officer defeat, rather than entire command group clearance, opens exit. Cached Health reference; added optional Larper cannot gate completion.

## Verification so far

Compilation successful. Focused combat5/5, rules2/2, Level1 integration6/6, saved layout2/2, campaign/shop/results12/12, movement/suppression9/9, cover12/12, legacy mission9/9 pass. Specialists4/4 also pass:61/61 total (4 Edit Mode,57 Play Mode). Final exact results: TestResults/Level1CombatHotfix/Regression.json and Report.md.

Controlled tests verify actual small Samurai landing, full ground route using the real motor, laser warning/locked direction, offscreen and solid-cover gates, Officer retreat, committed moving-target throw, radial player/enemy/self damage, armor, occlusion, boundary/outside rejection, and optional-ally exit. OfficerLaser.png was inspected. This is automated/controlled verification, not human gameplay or Android-device testing.

The old specialist suite had stale assumptions from earlier hotfixes:24 rather than preserved31.2 global projectile damage, opt-in rather than already always-on friendly fire, fixed .4s rather than existing configurable effect lifetime, and manually ticking a throw with combat disabled/offscreen. Fixture updates preserve current behavior and use an actual bound camera/enabled combat; original global projectile is unchanged. Initial failing JSON evidence is retained.

605 original art/prefab/historical/other-scene hashes match pre-resume state. No artwork changed. Existing pre-sprint edits and four deleted Resources performance-test files remain untouched. Prior55 tests from Sprint0028 are not counted as current verification.

## Files for this hotfix

- Runtime: Assets/Scripts/Enemies/{OfficerShotTelegraph,EnemyCombat,SamuraiCombat,FireworksCombat,FireworkProjectile}.cs; Assets/Scripts/Missions/WarehouseMission.cs.
- Authoring: Assets/Editor/Level1CombatHotfixSetup.cs; Assets/Scenes/Warehouse_Demo.unity; Assets/Data/FirstOperation/{Officer.prefab,OfficerPistol.asset,OperationFirework.prefab,OperationSamurai.prefab,OperationSamurai.asset}; generated metadata.
- Tests: Assets/Tests/EditMode/{FirstOperationLayoutTests,Level1CombatHotfixRulesTests}.cs; Assets/Tests/PlayMode/{FirstOperationTests,Level1CombatHotfixTests,Sprint024FlowTests,Sprint024EnemyTests}.cs.
- Documentation/evidence: DEVELOPMENT_LOG.md, SPRINT_RESUME.md, TestResults/Level1CombatHotfix/.

Other dirty files belong to completed Sprint0028 and must be preserved. Full before/after tuning and manual checklist: TestResults/Level1CombatHotfix/Report.md.

## Remaining steps / known limitations

Final checks complete: compilation clean; current Console ground truth0 errors/0 warnings; Warehouse_Demo saved/open/clean and outside Play Mode;15 valid placed actors, assigned Officer ally,0 missing scripts, unchanged geometry. DEVELOPMENT_LOG and report updated; nothing staged/committed/pushed.

Human playtesting still required for cautious introductory difficulty, crossfire, Officer evasiveness, sustained moving-target combat, broader terrain stuck cases, laser/art/audio presentation and Android controls/frame rate. No APK build/device claim.

Do not start another sprint. All requested code/authoring features, targeted verification and documentation are complete; only hands-on playtest/device review remains. Any later refinement should follow actual playtest findings.

## Required regression checks for subsequent changes

Preserve15 supported occupants/[3,4,3,3,2], opening3, geometry/spacing/persistence; Samurai actual small-collider platform pursuit, fair melee and no snapping; Officer every-shot laser/late lock/cover/offscreen/retreat/cooldown; expert moving-target committed throws/fuse/pool; continuous radial curve plus armor/friendly/self/occlusion/boundary behavior; Officer-only exit; campaign/shop/results/revival; existing movement/double jump/ladder/cover/suppression and legacy checkpoints.

## Unity tooling

Editor6000.6.6f1; CLI1.0.0-beta.14 (latest checked). Executable C:/Users/jansa/AppData/Local/Unity/bin/unity.exe. All command calls use --caller plugin --skill unity-cli --format json. Use native Editor APIs for scenes/prefabs/assets; never hand-edit YAML/GUIDs. Keep compilation/test transitions sequential and do not edit scripts during tests. Async authoring can finish after a CLI timeout; inspect durable Authoring.json before retrying.

## Original requirements and acceptance criteria — verbatim

```markdown
# AKUMU 2 — LEVEL 1 COMBAT & ENEMY AI HOTFIX

## THE MISSION

Improve the existing redesigned first level of AKUMU 2 through targeted enemy AI, combat balancing, and encounter adjustments.

**Do not redesign the level from scratch.**

The existing level layout is suitable for our introductory mission. It is relatively horizontal, which is intentional. Later levels will explore more verticality, parachuting, explosives-heavy encounters, and other gameplay variations.

Our objective is to make Level 1 more exciting, dangerous, memorable, and polished while retaining its accessibility as the player's first experience.

Implement the changes below directly in the existing Unity project. Inspect the relevant scripts, prefabs, scenes, and configuration before modifying them.

Reuse existing architecture and systems wherever practical.

---

## 1. INCREASE ENEMY POPULATION SLIGHTLY

Increase the total number of enemies in Level 1 by approximately **15–20%**.

Requirements:

- Distribute additional enemies throughout the existing level.
- Avoid concentrating additional enemies in one location.
- Preserve reasonable spacing between enemy spawn positions.
- Avoid the previous problem where the first three Larpers clustered around the same box.
- Do not introduce unavoidable simultaneous attacks.
- Preserve a gradual introductory difficulty curve.
- Use existing enemy archetypes rather than inventing new ones.
- Maintain sensible cover placement and navigable routes.

The level should feel more populated without turning into an overwhelming shooting gallery.

Do not significantly modify the geometry or overall structure.

## 2. SAMURAI — MAKE THEM CONSIDERABLY MORE AGGRESSIVE

The Samurai should be one of the most aggressive enemies in the game.

Unlike cowardly Larpers, Samurai should actively seek confrontation.

Required behavior:

1. React quickly when detecting the player.
2. Begin pursuing the player almost immediately.
3. Aggressively close distance rather than lingering unnecessarily.
4. Jump between platforms when a valid path exists.
5. Pursue the player across different elevations.
6. Avoid getting stuck on boxes, platforms, or other obstacles.
7. Reduce unnecessary hesitation between attacks.
8. Resume pursuit promptly after an unsuccessful attack.
9. Preserve appropriate attack telegraphs and opportunities to evade.

Make Samurai significantly more proactive without granting unrealistic movement abilities or breaking existing navigation constraints.

**Important distinction:**

- Larpers: frightened, cautious, frequently retreating.
- Samurai: aggressive, determined, persistent.

The Samurai should feel like an irritating, relentless melee threat.

Keep damage, health, and attack timing independently configurable.

Do not solve poor pathfinding by teleporting enemies.

## 3. OFFICER MINIBOSS — COMPLETE BEHAVIOR REFINEMENT

The Officer is supposed to be an extremely cowardly miniboss armed with a powerful Desert Eagle.

He is NOT supposed to behave like an ordinary aggressive gunman or bullet sponge.

His identity is:

**Extremely dangerous individual shots, extremely cowardly personality.**

### 3.1 Weapon

Ensure the Officer uses the intended Desert Eagle weapon and appropriate existing visual representation.

Weapon characteristics:

- Very high damage per successful hit.
- Slow, deliberate firing cadence.
- Noticeable interval between attacks.
- Powerful audio/visual feedback using existing assets where available.
- No automatic rapid firing.
- No excessive firing accuracy without a clear telegraph.

His bullets should be genuinely threatening, but the player must have a fair opportunity to evade.

### 3.2 Laser sight telegraph

**This is the most important Officer change.**

Before EVERY normal shot, the Officer must visibly aim using a laser sight for approximately **1.0 second**.

Implement an explicit attack sequence:

**TARGET ACQUISITION → LASER AIM (~1 SECOND) → FIRE → COOLDOWN / REPOSITION**

Requirements:

1. Display a clear red laser projecting from the Officer's weapon toward his intended target.
2. Keep the laser visible during the aiming period.
3. The player must be able to understand the threatened shooting direction.
4. After approximately one second, the Officer fires one powerful shot.
5. Remove or briefly flash the laser when the shot fires.
6. Enter an appropriate cooldown before attempting another shot.
7. Make the aiming duration configurable.

The Officer must NEVER instantly shoot the player without the proper warning.

### 3.3 Fair aiming behavior

The laser must be a useful warning, not merely decoration.

The Officer may track the player during the initial aiming phase, but should commit to a firing direction shortly before firing.

Suggested initial behavior:

- Total laser warning: 1.0 second.
- Initial 0.8 seconds: Officer tracks the player's position.
- Final 0.2 seconds: Officer commits to his shooting direction.
- Fire along that direction.

This prevents unfair last-frame aim snapping and allows skilled players to dodge.

Ensure line of sight and cover interactions behave correctly.

The laser should not incorrectly indicate a shot passing through bulletproof geometry.

Use an inexpensive rendering approach suitable for Android.

### 3.4 Cowardly movement

The Officer should be extremely reluctant to fight at close range.

Implement or refine:

- Retreat when the player approaches.
- Prefer maintaining a substantial distance.
- Seek cover whenever reasonably possible.
- Avoid willingly entering melee range.
- Reposition between shots.
- Use existing navigation rather than teleportation.
- Avoid trapping himself against walls or obstacles.
- Become more willing to initiate attacks once his nearby allies are defeated, while retaining his cowardly personality.

The player should feel that catching the Officer is difficult because he keeps escaping, not because he has absurd amounts of health.

Preserve his miniboss identity and existing encounter triggers.

## 4. CZECH EXPLOSIVES EXPERT — COMMIT TO THROWS

The explosives expert currently has a serious behavioral problem.

When the player repeatedly jumps or otherwise keeps moving, the expert often continually delays throwing an explosive.

It feels as though he is waiting for the player to stop moving before committing to an attack.

**THIS MUST CHANGE.**

### Required behavior

Once the explosives expert chooses to attack and begins his throwing animation, he must commit to that throw.

Implement a clear attack state sequence:

**ACQUIRE TARGET → PREPARE THROW → COMMIT → RELEASE EXPLOSIVE → RECOVER**

Rules:

1. Acquire the player's position or reasonably estimate a future position.
2. Begin the throwing animation.
3. Once the attack is committed, do not restart the wind-up simply because the player moves.
4. Do not wait indefinitely for the player to stand still.
5. Release the explosive at the appropriate animation moment.
6. Permit imperfect throws and misses.
7. Apply normal recovery/cooldown behavior afterward.
8. Begin a new targeting decision only for the next attack.

If the player jumps away after the expert commits, the explosive should still be thrown toward the previously selected location or trajectory.

The expert should not possess perfect tracking abilities.

Movement by the player must not repeatedly reset the attack animation or timer.

Add appropriate safeguards against interrupted states, repeated failed attack attempts, and infinite waiting.

Keep existing variable-fuse behavior and the expert's chaotic personality.

## 5. EXPLOSIVES — SIGNIFICANTLY INCREASE LETHALITY

Explosions currently need to be more dangerous, particularly near their epicenter.

Implement **distance-based radial damage falloff**.

### Required damage behavior

An explosion should deal:

- Extremely high damage near its center.
- Substantial damage at intermediate distances.
- Reduced damage near the outer edge.
- No damage beyond its effective blast radius.

Damage must depend on distance from the explosion center.

Use a continuous falloff function rather than identical damage throughout the entire radius.

Suggested initial normalized curve:

| Distance from center | Relative damage |
|---|---|
| 0% of radius | 100% |
| 25% of radius | ~80–90% |
| 50% of radius | ~50% |
| 75% of radius | ~20% |
| 100% of radius | 0% |

Interpolate smoothly between these values.

This is an initial balancing guideline, not a demand for a specific mathematical implementation.

### Damage tuning

Increase the maximum damage enough that standing extremely close to an explosion is a major threat.

A direct or near-direct explosion should be potentially devastating.

However, avoid making every explosion instantly fatal regardless of distance.

Do not blindly increase all damage by the same multiplier.

Make the following parameters individually configurable:

- Maximum damage
- Effective blast radius
- Damage falloff curve
- Optional minimum effective damage threshold
- Existing knockback/physics impulse, if applicable

### Existing functionality

Preserve:

- Enemy friendly fire.
- Explosives damaging other nearby enemies.
- Variable fuse times.
- Existing explosion animations and effects.
- Existing environmental interactions.
- Ragdoll reactions.
- Existing armor and damage-handling systems.

Keep the visual blast reasonably consistent with its effective damage radius.

An explosion should feel chaotic and genuinely dangerous, but its consequences must remain understandable to the player.

## 6. PRESERVE LEVEL 1'S INTRODUCTORY DESIGN

Do not mistake this hotfix for permission to radically increase the entire level's difficulty.

We are simultaneously:

- Increasing enemy population.
- Making Samurai more aggressive.
- Making the Officer's attacks more dangerous.
- Improving explosives expert attack reliability.
- Increasing explosion lethality.

These changes will compound.

Ensure the result remains a reasonable introductory level.

The first encounters should remain comparatively manageable. More complex interactions can develop as the player progresses.

Do not introduce large numbers of dangerous enemy types simultaneously at the starting location.

The goal is **more interesting difficulty, not artificial difficulty**.

## 7. TECHNICAL REQUIREMENTS

Follow the existing project's conventions.

- Inspect the current implementation before editing.
- Reuse existing scripts, prefabs, animations, and AI states where possible.
- Avoid unnecessary architectural rewrites.
- Preserve existing enemy morale and suppression systems.
- Preserve existing player movement, double jump, parachute, ladders, cover, and healing.
- Preserve existing checkpoint behavior.
- Do not break mobile touchscreen controls.
- Maintain Android performance.
- Avoid expensive per-frame AI computations where unnecessary.
- Keep newly introduced balancing values configurable.
- Preserve established 8-bit pixel-art visuals.
- Do not add polished placeholder graphics or unrelated visual assets.
- Do not redesign the HUD, shop, main menu, or other levels.
- Do not initiate unrelated refactoring.

## 8. TESTING AND ACCEPTANCE CRITERIA

After implementation, build or run relevant automated tests and inspect the modified code for regressions.

Verify the following.

**Enemy population**
- Approximately 15–20% more enemies, subject to sensible integer rounding.
- No overlapping spawns or obvious clustering.
- No newly introduced navigation softlocks.

**Samurai**
- Pursue the player more aggressively.
- Jump between reachable platforms.
- Do not freeze, loop indefinitely, or abandon pursuit without reason.
- Attacks remain fair and readable.

**Officer**
- Uses the correct weapon.
- Telegraphs every normal shot with a visible laser.
- Maintains approximately one second of warning.
- Does not instantly fire upon spotting the player.
- Locks the firing direction shortly before shooting.
- Deals substantial damage on a successful hit.
- Retreats and avoids close-range engagements.
- Does not fire through bulletproof obstacles.

**Explosives expert**
- Throws successfully at a continuously moving player.
- Does not wait indefinitely when the player repeatedly jumps.
- Commits to throws after beginning the attack sequence.
- Can miss naturally.
- Does not get stuck in an attack animation.
- Preserves friendly fire and variable fuses.

**Explosion damage**
- Highest at epicenter.
- Smoothly decreases with distance.
- Approaches zero at blast-radius boundary.
- Properly affects player and enemies through the existing damage systems.
- Does not damage entities outside the effective radius.
- Does not break existing armor or knockback logic.

Do not claim that gameplay was playtested unless it actually was. If Unity runtime or Android build verification is unavailable, clearly identify what remains to be tested manually.

## 9. COMPLETION REPORT

When finished, provide:

1. A concise summary of implemented changes.
2. The main files and components modified.
3. Before-and-after values for relevant AI and damage parameters.
4. Any bugs discovered and fixed.
5. Tests performed and their results.
6. Anything requiring manual playtesting.

Do not leave requested changes implemented only partially without explicitly reporting the limitation.

---

## FINAL DESIGN PHILOSOPHY

AKUMU 2 is built around expressive enemies with distinct personalities.

Every enemy should feel different.

**Larper:** Frightened, inexperienced, inaccurate, and prone to panic.

**Samurai:** Aggressive, relentless, fast to engage, and determined to reach the player.

**Czech explosives expert:** Chaotic, dangerous, committed to throwing explosives, and potentially hazardous to his own allies.

**Officer:** An extremely cowardly commander who desperately avoids confrontation but threatens the player with heavily telegraphed, devastating Desert Eagle shots.

These personalities must be apparent through actual gameplay behavior, not merely dialogue or visual differences.

Keep the current Level 1 layout.

Improve its population, combat pacing, enemy personalities, and overall encounter quality.

**MAKE THE FIRST LEVEL MORE MEMORABLE WITHOUT MAKING IT UNFAIR.**

Implement the hotfix now.
```
