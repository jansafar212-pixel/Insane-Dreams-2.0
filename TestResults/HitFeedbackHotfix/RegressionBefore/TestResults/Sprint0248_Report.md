# Sprint 024.8 — Hotfix O'Clock

**Implemented by Codex — awaiting human playtest verification.**

## Samurai placement, scale and requested hitbox correction

The gameplay map is Warehouse_Demo, launched by MainMenu. The original saved Samurai and reset point were both at (−0.5,6.11), on supported mezzanine ground. Static inspection did not reproduce an initial platform overlap, so that specific original playtest symptom is not claimed as conclusively reproduced.

The actor and its reset point were moved together. Two authored Samurai were added through existing MissionEncounter/EnemySpawnPoint references; no Larper spawning code was rewritten. Final positions:

| Samurai | Position | Encounter |
|---|---|---|
| Original storage Samurai | (−4.8, 0.529232) | Storage ground floor |
| Additional storage Samurai | (−0.8, 5.929232) | Storage mezzanine |
| Additional processing Samurai | (23, 0.529232) | Eastern ground floor |

Demo now has exactly **three Samurai / 17 total enemies**, encounter counts **3/6/8**. Testbed remains seven enemies; Mission01 remains ten. Positions have ground support, solid clearance and >1.5-unit separation from other authored actors. The additional points are registered for encounter completion and replay/checkpoint reset. An intermediate authoring duplication was caught by tests and removed; copying point settings now avoids cloning a point's child resident.

All Samurai visuals use **1.05** relative scale, including the sword. Per the developer's follow-up, the old 0.8×1.4 body collider was replaced by the measured standing body bounds: approximately **0.7539605×1.02846408**, centered around a rebased visual body. Spawn heights use the new half-height plus .015 ground clearance. Living health **70**, motor speed **7**, jump impulse, melee damage **40**, range **1.65**, wind-up **.4 s** and recovery **.7 s** remain unchanged.

`SamuraiBodyHitbox` follows the actual animated body-renderer envelope after poses, including both facing directions, running, crouching, air and climbing. The held sword is excluded. This is a body bounding rectangle rather than per-pixel polygon collision. It makes no physical contacts, so moving limbs do not resize the movement contact shape or push the actor. Bullet/melee ray queries and firework damage ignore the separate locomotion shape and use the animated hitbox. Tests verify that shooting through empty space covered only by the locomotion shape causes no damage, while hitting the visible body does. Center of mass stays fixed, and corpse segments preserve 1.05 scale and the existing contact filters.

Fitting the smaller body exposed a reproducible traversal failure: its head-height wall rays can pass beneath a floating step. The existing bounded one-time backstep now also recognizes an overhang from the already-cached route target, with a validated 1.8-unit corridor/.38-second recovery in that case. Ordinary recovery remains 1.2 units/.25 seconds. Existing single/double jumps, route caching, blacklist and failure cooldowns remain; no new navigation system or frame-time query loop was added. All three previously failing ledge/warehouse-step tests pass.

## Graphics and VFX

Read ART_BIBLE, project guidance, prior log and relevant local reference/preview artwork. Historical Enemak artwork remains unchanged. New art extends the established native, editable C# pixel recipes: 32 PPU, point filtering, flat colors, no mipmaps, no new image framework.

- Fireworks0248: contrasting crude purple shirt with a badly drawn yellow firework/spark emblem and scribbled white marks; matching sleeves; larger 16×13 bright-yellow stepped hardhat/head with a broad cheap-looking brim. Existing faces, limbs, held bundle, animation and mechanics remain.
- WarehouseCarbine: 30×11 chunky native sprite, with distinct wood/cardboard stock and grip, steel receiver/barrel and visible magazine. Damage20, interval.2, capacity24, reload1.5 and range22 remain. Its equipped sprite is the only weapon-definition change.
- Carbine flash: new 15×9 directional white/yellow/red native sprite, assigned only to WarehouseCarbine feedback. Existing .06-second lifetime, variation, subtle glow, firing event, muzzle attachment and audio limits remain. Automated checks verify the sprite appears at the actual muzzle and expires after a shot.
- Fireworks: the existing ten pooled spark renderers expand with curved travel and multiple bright colors to 2.7 units, fading over .8 seconds, versus old 1.5-unit spread/.35-second fade. No added particle dependency or per-shot renderer allocation. The damaging radius grows moderately **2.2→2.7**; damage24, falloff, impulse, per-target deduplication, walls/platform occlusion and saturated-query safeguards remain.

## Unreliable fireworks and friendly fire

Fuses start on release, so short fuses can detonate in flight at the actual projectile position rather than its intended destination. Contact stops the bundle but does not reset its fuse.

| Setting | New value |
|---|---|
| Normal post-release fuse | .8–1.5 s |
| Short fuse | 18% chance; .25–.45 s |
| Long fuse | 18% chance; 2–3.2 s |
| Remaining normal-fuse chance | 64% |
| Premature held failure | 6%→8% of preparations |
| Held-failure timing | 35–80% of the .85-second preparation |
| Preparation/recovery | .85 / 3.8 s, unchanged |

All probabilities/ranges are serialized and configurable; held-failure fractions are safely clamped before release. Premature failure clears the pending throw before exploding at the actual hand position. Cancellation, two-projectile pool ownership, damage/death interruption and the detonation-once guard remain.

Every firework explosion can now hurt the thrower and all nearby factions. Previously normal throws enabled allied damage on only 25% of attempts; that lottery was removed. The shared Health/DamageInfo path is reused with explicit friendly-fire permission for explosions only. Normal bullet friendly-fire rules remain. Actual damage feeds hit reactions and existing ally fear/witness/ragdoll events; panic/cascade probabilities were not increased.

A controlled Editor experiment using actual Larper/Samurai/FireworksExpert prefabs damaged all three once: health **50→27.7548**, **70→54.86103**, **60→39.53453**, respectively. The captured blast reported three affected actors. Held failure, recovery, self-defeat, ordinary friendly-bullet rejection, obstruction and duplicate-collider/detonation cases also passed automated checks.

## Camera and automatic pistol

Camera direction damping **.22→.75 seconds** makes side switching deliberately slower. This is a SmoothDamp parameter, not the full settling time. Existing .12-second direction commitment, bounded single transition state, running displacement tracking, vertical follow and level boundaries remain. Settled composition remains one-third right-facing / two-thirds left-facing. Zoom **1.155** and separate Android scale **1.125** are unchanged; player input/facing/movement responsiveness is unchanged.

Basic Larper firing interval **.105→.07 seconds**, or approximately **9.52→14.29 shots/second**. An ideal uninterrupted 27-round burst spans **2.73→1.82 seconds** from first to last shot. Existing AI gates may interrupt a burst. Damage5, capacity27, four-second vulnerable reload, spread/distance uncertainty, malfunction, suppression and last-known-position firing remain. The existing serialized WeaponDefinition interval is the tuning control. Tests confirm finite ammunition, blocked duplicate same-time attacks, one shot cue per successful shot and the unchanged reload deadline.

## Files changed

Runtime scripts:

- `Assets/Scripts/Camera/WarehouseCameraFollow.cs`
- `Assets/Scripts/Enemies/SamuraiCombat.cs`, new `SamuraiBodyHitbox.cs`, `FireworksCombat.cs`, `FireworkProjectile.cs`
- `Assets/Scripts/Visuals/CutoutRig.cs`, `CharacterVisualDriver.cs`, `SamuraiArticulatedRig.cs`
- `Assets/Scripts/Combat/CombatFeedback.cs`, `AttackExecutor.cs`

Authoring: new `Assets/Editor/Sprint0248Setup.cs`, `Sprint0248Preview.cs`.

Assets: `Assets/Art/Sprint0248/` contains ScrapCarbine, CarbineFlash, CheapHardhat, FireworksShirt and FireworksSleeve Sprite/Texture pairs (ten native assets); new `Assets/Data/Visuals/Fireworks0248.asset`; modified `Assets/Data/Items/WarehouseCarbine.asset`, `LarperBuzzbox.asset`.

Prefabs: `Assets/Prefabs/Enemies/Samurai.prefab`, `FireworksExpert.prefab`, `CartoonFirework.prefab`, `Assets/Prefabs/Player/WarehousePlayer.prefab`. The Samurai prefab adds a sibling non-contact VisibleBodyHitbox; the modular limb hierarchy remains intact.

Scenes: Warehouse_Demo (placements/registered residents/camera damping), Warehouse_Testbed and Warehouse_Mission01 (camera damping only).

Tests: new Sprint0248RulesTests, Sprint0248Tests, Sprint0248SceneTests. Updated Sprint008PresentationTests, Sprint013RulesTests, Sprint0242RulesTests, Sprint0246RulesTests, Sprint0247RulesTests; Play Mode Sprint0242Tests, Sprint0243Tests, Sprint0244Tests, Sprint0245SceneTests, Sprint0247Tests and Sprint024FlowTests. Old literal population, rate, old-body landing heights and fixed landed-fuse assumptions were updated without removing coverage. The flow fixture now enables/restores background execution like the other physics fixtures after an unfocused run timed out; the isolated retry passed.

Documentation/evidence: ART_BIBLE.md, DEVELOPMENT_LOG.md; TestResults/Sprint0248.json, Sprint0248_Report.md, Sprint0248_ReferenceSheet.png, Sprint0248_CarbineFlash.png, Sprint0248_FireworksBurst.png. New Unity metadata was generated by the Editor. No serialized YAML or GUID was hand-edited.

## Verification and remaining manual work

Unity6000.6.6f1 compiled successfully. **78/78 full Edit Mode** and **92/92 unique targeted Play Mode tests** passed, including updated/corrected reruns. Raw results retain earlier failures/timeouts rather than hiding them. This is not a claim that the entire Play Mode assembly ran.

Coverage includes exactly-three authored population/clearance/reset, all three detecting/pursuing/striking, body-sized standing geometry and animated hitbox alignment, no invisible locomotion hits, platform single/double jumps and bounded failed-jump recovery, ragdoll scale/contact masks, variable/airborne/held fuses, friendly/self blast damage, explosion deduplication/occlusion/fade/pool reuse, pistol ammunition/cues/reload, carbine flash placement/lifetime, camera switching/composition/chatter, spawning/persistence, suppression/morale, rendered player motion and menu/shop/exit/results flow.

The pose sheet and close-up VFX images were visually inspected. The VFX pictures are controlled live Editor captures with temporary frozen actors; they are not natural gameplay or an Android performance profile. Runtime frame progression and actual damage values were recorded with no campaign save session active. Final audit found zero missing scripts/broken serialized references in all three warehouse scenes, with populations17/7/10. MainMenu is saved/open and the default Play entry; Play Mode stopped; background-running restored off; current Console has zero errors/warnings. Historical compile/API warnings and resolved authoring/test failures remain in history.

Manual checklist:

1. Play MainMenu→Warehouse and encounter all three Samurai; inspect their smaller body hitboxes, feet, sword alignment and platform recovery while actively dodging.
2. Review the hardhat/shirt and carbine/flash at normal gameplay distance, in both directions and during cover, reload and ladder animations.
3. Observe repeated natural throws: normal/short/long fuses, occasional held failures, colorful fades and rare allied casualties. Check protective cover and recovery after self-damage.
4. Reverse direction repeatedly and confirm the slower camera feels comfortable while preserving the settled ratio and zoom.
5. Fight basic Larpers at close and long range: rapid wasteful bursts, poor distant accuracy and long reload windows should remain readable and fair.
6. Check healing, double jump, cover, mobile controls, campaign flow, audio at the faster cadence and Android device performance. Human feel/balance/art acceptance and APK/device verification remain pending.

No staging, commit or push. Existing unrelated working-tree changes were preserved.
