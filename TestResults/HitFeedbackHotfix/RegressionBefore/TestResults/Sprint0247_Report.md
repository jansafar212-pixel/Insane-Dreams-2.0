# Sprint 024.7 — implementation and verification

**Implemented by Codex — awaiting playtest verification.**

## Scope and files

This sprint changes the three authorized character presentations, directional camera, existing suppression feedback and the two requested motor speeds. It does not change encounter placement, damage, weapon capacity, spawning, progression or player physics/update ownership. Existing unrelated working-tree changes were preserved. Nothing was staged, committed or pushed.

Exact implementation and evidence paths are listed in `TestResults/Sprint0247_Files.txt`, including all 44 new native art assets. New Unity assets/scripts have normal Editor-generated metadata.

Runtime scripts: `WarehouseCameraFollow.cs`; `EnemyCombat.cs`, `LarperAwareness.cs`, `LarperPersonalityProfile.cs`; `CharacterVisualDriver.cs`, `CutoutRig.cs`, `CartoonRagdoll.cs`.

Authoring: new `Assets/Editor/Sprint0247Setup.cs` and `Sprint0247Preview.cs`. The setup applies absolute values and updates native pixels in place, preserving asset references when rerun; speeds and zoom cannot compound.

Prefabs: `WarehousePlayer`, `Larper`, `Samurai`, `FireworksExpert`, standalone `PlayerCutoutVisual`. New skins: `Hero0247`, `Samurai0247`, `Fireworks0247`. `SamuraiSword.asset` changes its equipped sprite only. Ordinary/Cowardly/Overconfident/Samurai personality assets receive speech content. Warehouse_Demo, Warehouse_Mission01 and Warehouse_Testbed receive the camera zoom setting; their encounter layouts/counts remain unchanged.

Tests: new `Sprint0247RulesTests` and `Sprint0247Tests`; existing Sprint008Presentation, Sprint0244Rig, Sprint0245Rig, Sprint0202, Sprint0224, Sprint0243 and MovementCompletion tests retain their coverage with updated native-art, visible-face, suppression-cap and speed-relative expectations. No tests were removed. `AGENTS.md`, `ART_BIBLE.md` and `DEVELOPMENT_LOG.md` record the new baseline and work.

## Actual references and art changes

Original local Enemak files are available: `HistoricalArtSources/Drive/Postavy/Enemak/Postava.png`, `Postava 1.png`, `Postava 2.png`, `1c.gif`, `2b.gif`, `6d.gif`. Postava and the rendered current Larper reference were visually inspected again. All six original hashes match Sprint0245; see `Sprint0247_ReferenceHashes.json`. No historical files were modified.

The original Postava is 1600×799 with a 144×280 opaque silhouette and five opaque colors. This is a source-image measurement, not a historical Unity PPU claim. Current Larper measurements: head 12×11, torso 12×17, arms 5×18, legs 6×18, **32 PPU**, point filtering/no mipmaps, centered body pivots and top-centered limbs. Its flat palette, sparse one-pixel face marks and color-to-color body edges remain the implementation baseline explicitly selected in this brief. There is no newly imposed thick outline.

The new artwork extends the project's existing code-authored native Sprite/Texture2D recipes. It uses crude rectangles, uneven edges and flat colors rather than a high-resolution illustration pipeline. The shared seven base colors are peach, ink, brown, red, blue, teal and light; cardboard, yellow and orange are small flat additions. Full hex values and module measurements are in `ART_BIBLE.md`.

- **Hero:** crooked yellow work cap, flat open orange shirt with awkward button marks, blue rectangular trousers, exaggerated shared expressions. Original equipment, two arms, rigid-limb animation, cover alignment and player collider remain.
- **Samurai:** oversized uneven cardboard helmet with visible face/eyes below it; mismatched cardboard/red/teal armor, tiny awkward articulated limbs and an absurdly long, slightly crooked pale sword. Six small expression overlays replace the old hidden-face treatment. Existing aggressive combat, Japanese cosplay-warrior nonsense, articulated joints and grip ownership remain. Example speech: 拙者は寿司の為に戦う。
- **Fireworks expert:** big flat yellow hardhat, block moustache, teal coat with large colored homemade rocket markings. His existing colorful hand bundle, throw mechanics, six-part animation and ragdoll fallback remain.

Standing authored Samurai height: **0.984375 units / Larper 1.403125 = 70.16%**. Sword sprite length: **46/32 = 1.4375 units / Samurai = 146.03%**; its 39-pixel blade region alone is 123.81% of body height. All are native 32 PPU, without scaling the entire rig. Living Samurai hitbox remains **0.8×1.4**, with attack distance **1.65**, damage **40**, wind-up **0.4 s**, recovery **0.7 s**. Smaller art does not shrink the target or extend damage reach. Corpse boxes fit copied sprites while retaining the existing contact masks, masses, joints and velocity caps.

No encounter-placement adjustments were necessary. Counts remain Demo 15 enemies, Testbed 7, Mission01 10; both intended three-Larper openings remain intact. The two redesigned specialists are in Demo as before.

## Camera and movement

The old camera was horizontally centered. The final orthographic projection now determines the offset:

`W = 2 × finalOrthographicSize × actualCameraAspect`

`p = gameplayViewport.left + gameplayViewport.width × (0.5 − facing/6)`

`cameraX = playerX + (0.5 − p) × W + existingHorizontalOffset`

Thus facing right gives p=1/3, facing left p=2/3 for a full viewport. An Inspector gameplay rectangle supports asymmetric side restrictions. Existing warehouse touch controls/HUD overlay the world; they do not crop the horizontal camera viewport, so their rectangle remains full width. Camera pixelRect/aspect and any genuine camera viewport restriction are already reflected in W. Level bounds take priority over exact composition at map edges.

Direction commitment **0.12 s** filters rapid chatter; framing SmoothDamp **0.22 s** and existing follow smoothing **0.16 s** avoid snapping. Horizontal interpolated target displacement is carried directly into the follow state, avoiding speed-dependent trailing error during steady running. Direction changes smooth the composition error. Existing vertical follow, bounds and pixel rounding remain. Nothing writes to the player's physics/presentation transform.

Zoom multiplier **1.10 → 1.155**: 1.10×1.05, applied after the existing integer art-pixel calculation. Existing Android scale 1.125 remains separate.

| Configured motor | Before | After |
|---|---:|---:|
| Player | 7 | 8.4 (+20%) |
| Basic Larper | 7 | 7.7 (+10%) |
| Samurai / fireworks | 7 | 7 |

Acceleration 65 and deceleration 85 remain unchanged. Crouch and parachute retain their existing 45% and 40% multipliers. Scene audits confirm these speeds/skins resolve on actual authored residents, not only prefab assets.

## Suppression findings and tuning

Real, geometry-clipped hitscan segments already reached the system. Direct hits, friendly/self shots and truly protective geometry were already filtered. Overhead pressure uses the existing standing-head envelope. The identified feedback defect was reaction ordering: a strong first pulse could enter the high-pressure branch and consume its four-second retreat window without any hesitation if the retreat roll failed. Ordinary suppression also lacked its own contextual speech/expression cue.

Hesitation is now independent of the major-retreat branch. Eligible pressure produces a short **0.22–0.36 s × personality susceptibility** beat, with independently randomized **1.2–1.8 s** opportunities. These finite beats leave firing/movement windows; they do not refresh continuously per bullet. Existing ducking, withdrawal, cover reservations, reloads and recovery continue to own behavior.

| Parameter | Before | After |
|---|---:|---:|
| Near-miss radius | 1.25 | 1.5 |
| Overhead distance bias | .35 | .15 |
| Base suppression gain | 49 | 58 |
| Moderate threshold | 16 | 14 |
| High threshold | 48 | 42 |
| Defensive duck chance | 70% | 80% |
| Duck decision cooldown | 3–5 s | 2.5–4.25 s |

Distance weighting remains .4–1.45; susceptibility remains cowardly 1.25, ordinary 1, overconfident .75, with faster confident decay and existing bounded morale modifiers. Suppression still caps at 100, decays 12/s after .8 s quiet, and adds up from actual shots. Major retreat probability remains .7 with a four-second decision cooldown. Full panic remains gated by prolonged saturated pressure and a minimum 45-second personality cooldown; configured ordinary/cowardly/confident probabilities remain .08/.20/.02. Cascade chance, spread and event budgets are unchanged. Suppression cues never publish ally-defeat or collapse events. Characters opting out of psychological suppression remain unaffected.

The appended Suppressed speech context uses the existing combat-event queue, a **five-second cue gate**, scared face/brief existing flinch presentation and personality-specific short lines: “THAT WAS MY HAIR!”, “TACTICAL FLOOR INSPECTION!”, “I FEAR NOTHING! DUCK!”. Existing withdrawal dialogue remains tied to actual withdrawal. The hard limit of two bubbles, 3–4 second reading time, no replacement while visible and encounter/per-speaker gates remain. Queued suppression lines expire when their pressure is gone. No new per-frame physics searches or AI manager were introduced.

## Verification and limits

Unity **6000.6.6f1** compiled successfully. **75/75 full Edit Mode tests** and **137/137 unique targeted Play Mode tests** passed after fixes. `Sprint0247.json` contains final per-test results and earlier attempts, including two transient MCP transport errors. Earlier failures from superseded sprite/hidden-face expectations and hardcoded speed-seven values were resolved; there are no outstanding failures in the completed targeted set. This is not a claim that the entire Play Mode assembly ran.

Coverage includes exact viewport math at two aspects/cropped rectangles, 5% zoom, running/reversal framing, rapid input chatter, double-jump visibility, rendered player drift/rewind, native density/proportions, visible arms, grip alignment, Samurai damage/traversal/recovery/ragdolls, real-shot suppression/distance/personality/occlusion/decay, rare morale/cascade limits, speech cap/readability, cover/spacing, off-screen firing/spawning/persistence, touch controls, parachute and menu/shop/results flow.

A controlled live Editor demonstration in Warehouse_Demo advanced from frame 6495 through 43994. Actual gameplay viewport was **1902×870**, final orthographic size **5.88474035**. At an interior diagnostic position, player X measured **0.333611965**, vs desired .333333343; the difference is below one art-grid rounding step. At the west level boundary the camera intentionally clamps rather than exposing space beyond the level. The controlled capture used a protected player and a temporary diagnostic reposition after the entrance obstruction; it is not a natural human completion/playtest. No campaign session/save was active.

Images: `Sprint0247_ReferenceSheet.png` (controlled pose sheet), `Sprint0247_Warehouse_Runtime.png` (camera world render), `Sprint0247_Warehouse_RuntimeUI.png` (composited Game view with existing HUD/controls/two bubbles). Visual review confirmed the native designs are present in actual warehouse rendering. It does not establish artistic acceptance, suppression feel, difficulty balance or target-device performance. Japanese speech content is present; Android CJK glyph rendering still needs device verification.

Final scene audit: zero missing scripts or broken serialized object references across the three warehouse scenes. MainMenu is saved/open, default Editor Play entry restored to MainMenu, Play Mode stopped. The temporary capture asset was removed through AssetDatabase after copying the evidence. Background-running used for diagnostics was restored off. Current Console: zero errors/warnings. Compile history still contains preexisting obsolete-API warnings and an unrelated AI account connectivity warning. Source whitespace checks pass; whole-working-tree diff checks also flag Unity-generated empty YAML-field whitespace, which was not hand-edited.

### Manual playtest

1. Play MainMenu → Warehouse. Review hero/fireworks art and tiny Samurai at ordinary gameplay zoom; check Japanese bubble glyphs on Android.
2. Run and reverse direction repeatedly, including wide Android landscape and map edges. Framing should settle to 1/3 right-facing and 2/3 left-facing, with no renewed twitching. Jump/double jump, crouch, cover/peek/blind-fire and climb both directions.
3. Fire a few true close misses and short overhead bursts. Expect visible finite hesitation/ducking, varied retreat/return fire and contextual speech. Stop firing and confirm recovery. Test protective cover and distant shots too.
4. Observe independent personalities, two-bubble readability and rare full panic; verify enemies still shoot/reload and maintain spacing/persistence.
5. Engage Samurai across platforms; check the huge sword's grip, readable wind-up, unchanged fair reach, hit reactions and corpse collisions. Verify fireworks throws and player equipment remain coherent.
6. Confirm normal campaign/shop/checkpoint/results flow and profile an actual Android build. No APK/device performance verification was performed here.
