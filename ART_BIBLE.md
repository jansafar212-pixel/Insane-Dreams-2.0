# AKUMU 2 character art

> Current visual direction and approval/reference index: [GRAPHICS_BIBLE.md](GRAPHICS_BIBLE.md).
> Read it first for graphics work. This document preserves technical rig,
> attachment, provenance and sprint history; older experiments and superseded
> restrictions do not override the permanent bible or later explicit user direction.

## Hotfix 0025.3 presentation note

Larper Glock reloads reuse the accepted player magazine component without changing
the player's pistol/carbine animation. An enemy-only articulated-hand adaptation
uses actual existing grip lengths during the exchange; no new limb art, pivots,
joint framework or ragdoll edits. Existing Larper production sprites remain.

Firework VFX add native 9×9 white pixel-star and16×16 soft-disc sprites under
`Assets/Art/Hotfix00253/`, authored by `Hotfix00253Setup` through Unity APIs. These
are effect masks, not character artwork or historical recovery. Point filtering,
no mipmaps, existing sprite materials.24 colored trail slots,6 secondary sparkle
slots,3 faint haze slots plus flash/glow are prewarmed per existing projectile;
.8-second lifetime and two-projectile pool remain. Glow is a restrained cosmetic
wash, without bloom or a new dynamic-light/rendering subsystem. Artistic review
and Android performance/readability acceptance remain pending user playtesting.

## Approval status

Sprint 024.7 explicitly selects the **current basic Larper as the production implementation baseline** and authorizes redesigns of the protagonist, Samurai and Czech fireworks expert. The new redesigns remain subject to artistic/playtest review; this does not assert that their appearance has already been accepted. Sprint 024.5 candidate notes below record their original status and measurements.

## Historical references (read-only originals)

Google Drive folder: [Enemak](https://drive.google.com/drive/folders/0BysSmO9jyT4SMTNKY2tuQTRUbFk?resourcekey=0-_UQIVs8eFVeJGBhru2kbPg).

Local copies: `HistoricalArtSources/Drive/Postavy/Enemak/`. Actual `Postava.png`, `Postava 1.png`, `Postava 2.png`, `1c.gif`, `2b.gif`, and `6d.gif` were inspected during Sprint 024.5. Historical originals must never be edited or overwritten. Original third-party material is reference/private-prototype material; publication needs independently created modular art and provenance review.

`Postava.png` is a 1600 × 799 canvas with opaque bounds x604–747/y200–479: a 144 × 280 silhouette surrounded by transparent padding. It has five opaque colors: brown `#4E2D05`, red `#AB2310`, blue `#0000FF`, black, white. These are source-canvas pixels, **not a historical Unity PPU setting**. No historical PPU is asserted. The body is rectangular; eye and mouth marks are tiny, flat and expressive. The extended arm and bent-leg variants provide articulation references, not a reason to increase detail. `1c.gif` is 1663 × 949 and contains one frame; it is a pose reference, not evidence of a multi-frame clip.

`Postava 1.png` and `Postava 2.png` are also 1600 × 799, with the same opaque bounds and seven/six opaque colors respectively. `2b.gif` and `6d.gif` are 1680 × 949, each one frame, showing differing bent legs, flat peach faces and hands, tiny eye/nose/mouth marks, and simple colored clothing. The numbered GIF files are separate pose references; these three inspected GIFs do not contain internal animation timelines. Other folder entries were listed, not individually inspected.

## Candidate assets and measured dimensions

- `Assets/Data/Visuals/LarperCandidate.asset`, `SamuraiCandidate.asset`: selected only on the relevant enemy prefabs. Earlier skins remain intact as fallback/reference.
- `Assets/Art/Sprint0245/CandidateSource.png`: transparent imagegen source; two rows of four isolated body parts. `Sprint0245Setup` reduces it to native Unity assets with a fixed flat palette. This is not recovered historical artwork.
- `CandidateAtlas.asset`: 64 × 64, point-filtered, no mipmaps, 32 PPU on each Sprite. Full heads 12 × 11, torsos 12 × 17, arms 5 × 18, legs 6 × 18. Unit rig scale; facing is only ±1 on X. No oversize head scaling.
- `LarperExpressions.asset`: 84 × 11, seven separate 12 × 11 frames. Neutral, overconfident, angry, nervous/surprised, terrified, stunned use the existing expression enum and event pipeline. An optional panic-face Sprite makes the existing Panic pose visibly different from fear without changing AI states or breaking six-expression fallback skins. Eye positions are x3/x8, y6; mouths use a few pixels. Do not add detailed eyes, noses or shading.
- `Samurai_Katana.asset`: 40 × 10 at 32 PPU, pivot (0.12, 0.5); 1.25-world-unit blade sprite width is preserved. Four flat colors. The older detailed source remains in Sprint0243.

The candidate atlas palette is `#FFD6A5` peach, `#191D2B` ink/navy, `#502B0C` hair/shoes, `#C22B16` red shirt, `#2242DC` blue trousers, `#0C6374` teal, `#DB2B2C` sash, `#FFFFEB` light. Transparent pixels are not a ninth color. Skin uses one peach entry across head/arms/hands; candidate rear limbs use white tint rather than brown shading. Samurai uses the same body density and flat construction, retaining teal, red sash, navy hair/topknot and visible facial marks.

## Rig and attachments

Keep `CutoutRig`/`CharacterVisualDriver` as animation owners. `SamuraiArticulatedRig` is the existing optional segmented extension, now also configured for firearm cutouts; it is not a second animation framework.

Hierarchy: shoulder → Elbow → WristHand → grip; hip → Knee → AnkleFoot. Head/torso pivots (0.5,0.5), limb/joint pivots (0.5,1). Full limb segments use source fractions: upper/thigh .48–1, forearm/shin .16–.50, hand/foot 0–.18. Adjacent pivots subtract .01125 world units for a small overlap. Never separate the visual hand from the weapon mount or move the gameplay collider to repair art.

Standing foot reference is local Y−.7. Larper crouch bends both hips toward the same folded-knee pose; feet are solved at the existing floor reference. Player posture, movement interpolation and goalkeeper crouch remain unchanged. Rear shoulder/forearm depth is 9, torso 10, front arm 14, hand 17; reload support-hand depth follows the existing base pose. Keep two distinguishable arms without new decorative accessories.

Firearm poses reuse actual base aim/reload hand targets and preserve shot direction. Katana follows the articulated hand axis and grip, including mirrored poses. Gameplay muzzle constraints remain authoritative. Never change damage, ammo or navigation to compensate for artwork. Segmented corpses reuse the existing six-root ragdoll and add eight physical limb segments (14 bodies/13 hinges); fallback rigs keep six bodies/five hinges. No live visual colliders or rigidbodies.

## Sprint 024.6 pistol refinement

`Assets/Art/Sprint0246/LarperPistol.asset` is a 20 × 8 native Sprite at 32 PPU,
pivot (.2,.5), retaining the earlier weapon dimensions/muzzle convention. It has
three flat colors: ink `#191D2B`, dark body `#3C434B`, edge `#6F7B82`.
`ExtendedMagazine.asset` is 5 × 13, pivot (.5,1), with one broad accent rather than
many small bands. The previous 8 × 18 magazine was disproportionately wide/long.
The new extended magazine remains clearly longer than the pistol body is tall.
Its per-weapon visual socket is (1/32,−2/32), directly beneath the grip; other
weapons retain the default (4/32,−2/32) socket. Existing reload handling offsets
are added to that socket. No weapon balance or projectile logic changed.

These are small extensions of the existing native/code-authored pistol sprites,
not new historical recovery or a change to the pending Larper art approval.

## Armor overlays

`ArmorStates.asset`: 30 × 13 with three 10 × 13 Sprites, 32 PPU, centered pivot. Olive rectangular vest `#536839`, ink edge, muted damage mark `#B1A568`. One overlay under the torso, order 11, unit scale. It follows the torso transform, not a separate physics body.

`ArmorVisual` observes existing `BodyArmor.Restore`/damage and enable/disable notifications. No armor means no vest. Above 50% condition: intact; positive condition at/below 50%: damaged. The focused gameplay hotfix supersedes the old broken-vest presentation: **zero condition, removal, or disabled armor clears the sprite and disables the overlay entirely**. Ordinary Larpers have no BodyArmor and acquire no protection from graphics. Corpse armor copies only a currently visible, non-depleted vest to the physical torso. The historical broken sprite is retained as an unused reference; armor damage calculations and save format are unchanged.

## Rules for future graphics work

Read the actual historical files and this document before changing characters. Favor a handful of bold flat pixels, awkward rectangular anatomy and expressive faces. No modern detailed indie rendering, gradients, realistic tactical textures, decorative helmets/headbands, dozens of pockets, detailed folds or unrelated silhouette redesigns. Reference-sheet and real gameplay-zoom reviews are required; successful compilation is not artistic approval.

Preserve existing physics, animation fallbacks, enemy personalities, mobile performance and prefab/scene references. Do not modify historical originals. Keep candidate changes reversible until explicitly accepted.

## Sprint 024.7 production baseline and redesign recipes

The historical `Postava.png` and the existing rendered Larper reference sheet were visually inspected again. All three Postava originals and the three numbered GIF pose references remain locally available; none were altered. Historical dimensions and color counts above are source measurements, not invented Unity density. The active Larper was inspected through the Editor: its standing rig height is 1.403125 world units, head 12×11, torso 12×17, arm 5×18 and leg 6×18, all 32 PPU.

Use crude rectangles, uneven top edges, flat skin and few-pixel expressions. Larper anatomy has predominantly color-to-color boundaries rather than a continuous black outline; structural marks are single native pixels. Equipment uses a one-pixel ink edge where needed. Do not add a thick universal contour or shading. Native point-filtered textures have no mipmaps; module pivots remain centered for heads/torsos, top-centered for limbs. Two visible arms, existing grip chains and physics masks remain mandatory.

`Sprint0247Setup.cs` extends the established editable C# flat-pixel recipes used by Sprint0245 faces/armor and Sprint0246 pistol sprites. These are new independently constructed native sprites, not recovered art or edits to historical images. Each module has its own Sprite/Texture2D `.asset`, 32 PPU and unit scale. The original skins and old art are retained. The code recipes are the editable source of truth, so no intermediate high-resolution illustration or new raster-editing pipeline is involved.

Shared base colors remain peach `#FFD6A5`, ink `#191D2B`, brown `#502B0C`, red `#C22B16`, blue `#2242DC`, teal `#0C6374`, light `#FFFFEB`. Small flat additions are cardboard `#B58441`, yellow `#F7C92C`, orange `#E2671E`. Each body module uses a subset rather than all ten colors. Existing firearm/firework equipment keeps its original separate palette.

- Hero0247: 12×11 uneven yellow work-cap/head, 12×17 orange open shirt with a crooked button line, 5×18 arms, 6×18 blue trouser legs/brown shoes. The same six Larper expression overlays and optional panic face provide crude expressive features. Original firearm/parachute/secondary/melee attachments remain. No player animation, collider or interpolation changes.
- Samurai0247: oversized crooked 14×10 cardboard helmet/head with a clear peach face, 10×11 asymmetrical teal/red/cardboard torso, 4×12 arms and 5×12 legs. Six separate 14×10 face overlays keep features below the helmet. Torso center −0.2, shoulders ±0.14, feet retain −0.7. Standing authored body height = −0.2 + 11/64 + 10/32 + 0.7 = **0.984375**, or **70.16%** of Larper. The 46×7 sword is **1.4375 units**, **146.03%** of that body height; the 39-pixel blade region alone is 123.81%. Sword pivot (3/46,.5) stays on the hand, with no sprite scaling. Segmented arm/leg recipes use the same existing fractions and joint limits. Living 0.8×1.4 hitbox and melee distance 1.65 remain unchanged: smaller artwork does not reduce targetability or increase damage reach. Corpse boxes fit copied native sprites, preserving floor-only collision masks on all 14 bodies.
- Fireworks0247: 14×12 crooked yellow hardhat/head with a block moustache, 14×17 teal coat with broad red/yellow/blue homemade rocket markings, 5×18 arms, 6×18 trousers. The original colorful questionable bundle remains the held combat item. Legacy six-part animation and ragdoll fallback are preserved.

`TestResults/Sprint0247_ReferenceSheet.png` renders all four characters at one projection through idle, walk, jump, crouch, climb/reload and panic. Preview poses are controlled renders, not a claim of human gameplay approval.

## Sprint 024.8 additions

The Samurai's native modules and proportions remain; an explicit **1.05 visual scale** implements the requested exact 5% enlargement. This is an intentional exception to the former unit-scale convention. The held sword scales with the body. At the developer's request, the old 0.8×1.4 living collider no longer remains as a forgiving invisible target: the standing movement shape is fitted to actual body-renderer bounds, about **0.75396×1.02846**, centered on the rebased visual body. Spawn heights derive from that shape. A separate non-contact `SamuraiBodyHitbox` tracks the animated body renderer envelope in both directions, including crouch/run/air/climb poses; the held sword is excluded. Hitscan and firework damage query this animated shape rather than the locomotion shape. This is a rectangular body envelope, not a claim of pixel-contour collision. No live colliders were added to the modular visual hierarchy or joints, and corpse physics remains separate.

Fireworks0248 uses a 14×17 contrasting purple shirt (`#AD3C95`) with a badly scribbled yellow/ink firework emblem and white marks. Its 5×18 sleeves match. A 16×13 head includes a stepped, oversized bright-yellow hardhat and broad brim; facial overlays remain below the hat. Existing articulated/fallback parts, held bundle and physics are preserved. These native flat-pixel recipes extend the established editor authoring, without editing historical references.

`Assets/Art/Sprint0248/ScrapCarbine.asset` is 30×11 at 32 PPU, pivot (7/30,5/11). Broad ink, steel and cardboard/wood shapes distinguish stock, receiver, thin barrel, grip and downward magazine. Weapon statistics remain independent of this sprite. `CarbineFlash.asset` is a 15×9 native white/yellow/red directional flash with left-center pivot, .06-second existing lifetime and the existing subtle glow. It is assigned only to WarehouseCarbine feedback.

Firework bursts reuse ten prewarmed spark renderers. The old 1.5-unit/.35-second visual spread becomes a curved expansion to 2.7 units with .8-second fading color trails. No new particle package, physics query loop, or VFX manager is introduced. The associated damaging radius is 2.7 rather than 2.2 units; platform/wall occlusion remains authoritative.

## Focused gameplay hotfix — carbine

`Assets/Art/FocusedHotfix/CrookedCarbine.asset` replaces the equipped carbine art,
using the native `FocusedHotfixSetup.Carbine` recipe. The stepped wooden butt,
narrow stock neck, dark squat receiver, wooden handguard, thin barrel and bent
magazine use four flat existing equipment colors: ink `#191D2B`, body `#3C434B`,
steel `#6F7B82`, wood `#B58441`. No gradients or realistic detail. Dimensions
remain 30×11, pivot (7/30,5/11), 32 PPU, point filtering and no mipmaps. Grip,
mirroring, muzzle convention, firing flash and all weapon statistics are retained.
Historical Postava.png and the actual Larper/pistol gameplay reference were
visually inspected; originals were preserved. The both-facing idle/walk/crouch
and reload pose review is `TestResults/FocusedHotfix_Carbine.png`; it is a
controlled render, awaiting human artistic/playtest approval.

## Player refinement — camera/Samurai follow-up, recovered 2026-10-23

`HeroRefined.asset` and `Assets/Art/CameraSamuraiHotfix/` refine the existing
orange-shirt/yellow-cap worker rather than replacing his identity. The native
`CameraSamuraiHotfixSetup` recipe keeps 12×11 head, 12×17 torso, 5×18 arms and
6×18 leg module bounds, original pivots and 32 PPU. The cap/hair silhouette is
cleaner; the old long peach neckline becomes a short opening; shoulders and
sleeves form coherent blocks, and hand corners are trimmed. Transparent trouser
edge pixels separate the legs that previously merged into a blue rectangle.
Feet, hip/shoulder anchors, arm length, weapon grip, muzzle and animation timing
are unchanged. The same flat peach/brown/yellow/orange/blue palette, inherited
expression sprites, two arms, equipment layers and armor behavior remain.

The actual historical `Postava 1.png` was inspected before authoring; originals
remain untouched. The character and fallback visual prefabs share the refined
skin, inherited by all three warehouse scenes. Previous Hero0247 assets remain.
`TestResults/CameraSamuraiHotfix_PlayerReview.png` compares the Larper, old hero
and refined hero in idle/walk/jump/crouch/climb/reload poses. The live warehouse
camera render is `TestResults/CameraSamuraiHotfix_Warehouse.png`. These controlled
Editor reviews establish integration, not human artistic or gameplay acceptance.

## Hotfix 0025.1 — carbine magazine articulation

Hotfix 0025.1 preserves HeroRefined and the Larper baseline. The carbine's existing
CrookedCarbine native pixels are separated into a 30×11 receiver and 4×5 magazine
under `Assets/Art/Hotfix00251/`, both point-filtered at 32 PPU. Receiver pivot
(7/30,5/11) and grip/muzzle bounds remain; magazine top pivot (.5,1) mounts at
(9/32,0). Same original four flat equipment colors; historical art unchanged.
Existing support/carry arms handle the old and replacement components without
new skeletons or limb scaling. The two-facing eight-phase review is
`TestResults/Hotfix00251_Reload.png`; normal-zoom runtime pause/reload evidence is
`Hotfix00251_Gameplay.png`. These are controlled reviews awaiting gameplay/art
approval, not claims of human acceptance. Actual Postava 1.png was inspected.

## 2026-10-24 — Menu and HUD visual language

Menus use flat ink#191D2B, paper#FFFFEB, cardboard#B58441, warning yellow#F7C92C,
orange#E2671E and teal#0C6374 from the existing palette. The main wordmark is an
independently code-authored5×7 block-letter pattern rendered as static UI strips;
it does not add a font package or trace historical artwork. Existing Unity text
remains for readable body copy and controls. The background uses flat warehouse
silhouettes and the unchanged Sprint008 ShippingCrate sprite. No glossy gradients,
3D props, character sprite/rig changes or historical-source modifications.

Paper shop/settings/results panels, rectangular warning-label buttons, selected
weapon tabs and matching pause/revive controls share these colors. Gameplay keeps
its original touch hit sizes/radius/thresholds and aim-pad44px right inset. A clear
health bar/numeric vitals, current weapon sprite and large magazine/reserve readouts
replace the old multiline debug-style equipment label. Do not bring back a second
ordinary FIRE control; the aim stick owns ordinary touch firing. Blind fire appears
only with physical crouching AND usable cover, per the latest corrected requirement.

Controlled native Unity captures were inspected: TestResults/GraphicsUiRework_MainMenu.png,
GraphicsUiRework_Shop.png and GraphicsUiRework_Hud.png. These demonstrate implementation
and layout, not human artistic approval or Android-device acceptance.

## Post-playtest Faintanyl equipment gesture (2026-10-24)

PostPlaytestHotfixSetup authors FaintanylSyringe.asset as an independent16x7
flat-color equipment Sprite, point filtering and32PPU, pivot(.8,.5). Existing ink,
paper, teal and yellow pixels create an exaggerated fictional syringe silhouette.
It is not historical recovery or a character redesign. Actual Postava1 reference
was inspected; historical originals and character modules/pivots remain unchanged.
The existing free arm performs a .6s non-graphic gesture; temporary arm/syringe
sorting18/19 keeps it visible over the torso, restoring normal layering afterward.
Both-facing controlled captures: PostPlaytest_Syringe0/1.png; artistic/gameplay and
Android readability acceptance remain pending. Player corpses reuse the existing
CartoonRagdoll source and limits; no enemy physics or new animation framework.

## Sprint 0026 - level environment language (2026-10-24)

Assets/Art/LevelArt0026/ contains32 independently code-authored native sprites;
LevelArt0026Setup is the explicit Editor-only recipe/authoring source. Four crate
constructions (wood brace, wood planks, tin corners, rust panel) share chunky two-
pixel construction marks, dark edges and tiny rivets. Floors/platforms use clear
flat top edges and sparse slab/bolt marks. Large dark wall blocks, muted windows,
beams, pipe/vent and simple rolling doors stay behind actors. Ladder art keeps the
existing endpoint/capture layout and readable ochre rungs. No new decorative clutter.

Shared16 opaque swatches: #221F23 #895734 #B18046 #583728 #66706D #895C3E #B99C4F
#22292E #283136 #353F42 #2D4048 #495452 #3D4242 #4E5959 #737164 #A6A284.
No gradients, wood grain, glossy reflections or noise.32PPU, point filtering,
RGBA32, no mipmaps, FullRect sprites. Full-object platform/crate pixel canvases use
existing exact renderer sizes; fractional edge pixels are clipped rather than
rounding collision bounds or stretching the whole pattern. Scaled/collider-bearing
cover roots retain all original configuration and get counter-scaled visual-only
children. No runtime art generator or new animation framework.

All old shared art and MainMenu assets remain byte-identical. Character sprites,
HUD/shop and gameplay systems are unchanged. Native Before/After overview captures
and LevelArt0026_Runtime.png were visually inspected; deliberate-crude artistic
acceptance and Android readability/performance still require human playtesting.

## Sprint 0026.1 - controlling art-direction clarification

Earnest, handmade amateur pixel art is the target: an enthusiastic teenager trying
to make a good-looking game in LibreSprite, not an artist deliberately drawing badly.
Avoid both professional/AI-looking polish and careless ugliness. Simplicity must
retain readability, visual humor, intention and charm. The approved crates are the
current environment benchmark and must not be repainted without explicit direction.

This pass removes repeated seams/bolts, the dense wall grid and fine mechanical
marks. Simple unequal two-pane windows, stout ladder rungs, broad door/vent divisions
and clear flat platform tops keep recognizable construction. Sparse wall space is
intentional. Non-crate assets use1-3 colors at existing32PPU/point-filtered dimensions;
apparent detail blocks are larger, without moving objects or changing colliders.
No random distortion/noise, new props/materials/textures or sophisticated effects.
Approved crate assets/pixels remain byte-identical. LevelArt00261_Runtime.png was
actually inspected at the gameplay camera zoom; artistic acceptance is still human.

## Sprint 0026.2 - logical pixel size, not new art direction

Non-crate environment recipes are baked on a4x4 native-pixel grid: an effective
8PPU drawing canvas within the unchanged32PPU sprites. Use nearest-neighbor blocks,
not blur or interpolation. Thin ladder/beam forms need explicit coarse cells so
their structure survives. Retain exact existing bounds and the sparse handmade
composition. No added colors, shadows, props or shaders; no deliberately ugly art.
Approved crates retain their accepted pixels and are excluded from this bake.
Point/RGBA32/no mipmaps were already correct; camera/render settings remain intact.
LevelArt00262_Runtime.png and LadderRuntime.png were inspected at gameplay zoom;
artistic acceptance remains human, and Android device verification is outstanding.

## Sprint 0026.2 revised brief - chunky environmental character (2026-10-25)

The revised brief permits curated details and simple hard shadows, superseding
the preceding pass's no-new-props/shadows restriction. Keep the effective 8PPU
drawing grid inside 32PPU sprites. Details should look thoughtfully drawn by an
enthusiastic amateur: recognizable chunky forms, a few flat colors, no gradients,
microtexture, noise or professional rendering. Approved crates remain unchanged.

HandmadeEnvironmentDetailsSetup explicitly draws 15 reusable sprite modules into
one Point/RGBA32/no-mipmap 256x256 sheet. Sparse signs, patch/crack, stain, section
number, pipe/valve, fan, electrical cabinets, shelf and warning lamp vary the walls.
Four small platform/floor marks and four opaque flat shadow bands per warehouse
add construction cues without new lighting. All detail cells are four native
pixels wide/high, alpha is binary, and each module has at most five opaque colors.
Decorative objects contain only Transform and SpriteRenderer; background sorting
keeps actors in front. No character, UI, camera, collision or gameplay redesign.
Controlled opening, ladder and office captures support visual review; final
artistic approval and actual Android readability/performance remain manual.

## 2026-10-25 — Officer Desert Eagle and handling follow-up

The Officer now uses independently code-authored DesertEagle.asset under
Assets/Data/FirstOperation/:27x14 pixels,32PPU, pivot(6,6) pixels, Point/RGBA32,
no mipmaps, four flat opaque colors plus binary transparency. A heavy stepped
slide/long barrel, hollow trigger guard and dark slanted grip distinguish it from
the preserved Larper pistol. Compact3x5 top-pivot magazine uses the same palette.
No body module, historical source pixel, arm length, physics joint or collider
changed. Weapon is anchored at the existing articulated carrying-hand grip;
actual muzzle is the barrel edge five pixels above the pivot, mirrored with the
sprite. Existing generic muzzle convention remains unchanged for other weapons.

Officer-only presentation follows the laser's sampled aim during warning, then
uses a32-degree/.09-unit one-handed kick over.5s after firing. Existing
PistolMagazineReload/segmented support-hand exchange removes/releases/replaces
the magazine on the weapon cycle's2.8s clock. The base generic spare is hidden
for Officer to avoid a duplicate of the specialized replacement. Shared rig and
ammo ownership remain; other characters retain existing presentation paths.

Controlled native camera captures in TestResults/OfficerHotfix/ compare both
facing directions in recoil and reload; OfficerLaser.png shows gameplay zoom.
Historical Postava1 and current Officer/gameplay references were inspected.
This artwork/presentation is implemented, not asserted artistically approved;
phone-size readability and hands-on combat feel still require user review.
## Sprint 0027 — small gameplay props and medical presentation (2026-10-25)

Added native-authored flat-color 32PPU Point-filtered props under
Assets/Data/Sprint0027: three 8x8 grenade icons, an 8x8 deliberately simple smoke
silhouette, a12x5 fictional mine and an8x4 duct-tape band. Binary sprite alpha,
small palettes, no imported/generative character art and no changes to original
historical artwork, approved crates or production character proportions. The
existing modular arm solver supplies the brief tape-wrap pose; the gun is hidden
while actively wrapping. A short seeded mono dry-noise peel cue accompanies it.
These are implementation props awaiting hands-on artistic/gameplay review,
not new approved visual baselines. Officer red warning remains distinct from its
actual magenta(.08-width) fading tracer. No new glow or high-detail rendering.
Controlled capture found the protagonist's original single-piece arms need a
fallback alongside the segmented solver: existing shoulder rotations sweep inward
across the tape, and normal pose ownership restores afterward. No joint positions,
sprites, physical hitboxes or animation timing changed. TapeWrap.png records the
corrected production rig; grenade type controls reuse equipment-button styling.

## Sprint 0027.1 — Desert Eagle visual correction

Existing native27x14/32PPU weapon keeps its silhouette, grip pivot and muzzle.
Brown grip pixels(65,49,40) replaced by flat charcoal-black(38,42,46), with no
new shading complexity. Existing magazine asset resized3x5→2x4 through native
Editor APIs, preserving object references/GUIDs; seated socket(-1,+1)pixels
inside opaque handle and lower sorting prevent protrusion. Existing articulated
carrying/reload animation remains; extracted magazine is visible naturally.
Controlled both-facing firing/reload captures inspected under Sprint00271;
no character parts/historical originals touched. Artistic acceptance remains human.

## Fragmentation hotfix — chunky pooled burst

Native8x8 stepped white mask, Point filtering/no mipmaps/32PPU, tinted in flat
red/orange/yellow layers plus grey smoke. Eight tiny gold/dark square fragments
reuse existing square sprite. All15 renderers/projectile prewarmed; no new
character/historical artwork, shaders, particle system or realistic volumetrics.
World positions snap32PPU; short expansion/fade only. Native source recipe:
Assets/Editor/GrenadeResupplyHotfixSetup.cs. FragBoom.wav is deterministic short
low thump/noise audio generated by that setup, not recovered historical audio.
Controlled captures: TestResults/GrenadeResupplyHotfix/FragFireball.png and
FragSmokeDebris.png. Artistic/gameplay acceptance remains subject to playtest.

## Sprint 0028 — Operation: The Coward's Bunker (environment)

Reusable native modules: Assets/Art/CowardsBunker/Environment.asset, authored by
Assets/Editor/CowardsBunkerSetup.cs. Eleven hand-placed coarse drawings: earth
floor, wall panel, scuffed deck, earth bank, grass tuft, rocks, sparse tree,
scrap, sandbags, timber roof and post. Four-native-pixel cells (8PPU drawing,
32PPU storage), Point, no mipmaps, binary alpha, small flat palettes. No random
texture generation, gradients, lights, new character or historical artwork.
Approved crates and original asset pixels preserved. Tiles/placed props reuse
one512x512 RGBA32 sheet; long collider dimensions remain independent of sprites.

Demo environment only: soil/rock/grass wall-foot details and subordinate tree
silhouettes, simple structural wall seams and scuffed platform bands. Existing
terrain, route, ladder relationships, actors and pacing preserved. Officer dais
reuses existing platform; two existing cover pieces receive sandbag art and
1.625x1.125 boxes. Small4.5x.375 timber roof at(142.75,4.875), with cosmetic posts
and an earth lip. Open front jump corridor and rear escape; no solid scenery.
TextMesh world sign uses existing font and satirical management-shelter wording.

Controlled opening/storage/bunker/laser captures in TestResults/CowardsBunker/.
Artistic acceptance and real phone readability/performance remain human review.
This is current implementation, not automatic artistic approval.

## Sprint 0029 — preservation, helmet overlays and block typography

Sprint 0029 explicitly approves the current Sprint 0028 environment artwork.
Environment.asset and CowardsBunkerSetup.cs were preserved byte-for-byte. Physical
bunker integration reuses the existing sandbag/timber sprites; no new background
art pass. Matching foreground collider bounds and an open walk/jump route are
required. Full-height muted posts remain decorative back-plane elements.

Small new light/heavy helmet overlays use 32PPU, Point filtering, binary alpha,
flat olive/grey-green plus dark outlines. They follow the existing head pivot and
are copied to the existing corpse head; no original character graphics, limb
pivots or ragdoll joints were replaced. AkumuBlock.ttf is original chunky lettering
constructed to match the current main-menu wordmark, used through shared USS.
These new equipment/UI additions have controlled Editor capture verification,
not human artistic acceptance or Android-device verification.

## 2026-10-26 — Concrete Officer pillbox follow-up

Latest user direction replaces the bunker-specific wood/dirt/sandbag treatment
with a small concrete pillbox and one firing slit. Earlier bunker art descriptions
are historical; surrounding approved0028environment assets remain unchanged.
Native ConcretePillbox sprites use32PPU, Point/no mipmaps, three flat concrete tones,
four-native-pixel structural marks and dark interior. Actual rectangular world
sizes are set through SpriteRenderer.size to avoid native canvas rounding causing
visual/collider drift. No character graphics, original references, joints, physics
rigs or weapon art were redesigned. Authoring is native Unity Editor API; capture
review is recorded in TestResults/ConcreteBunker, not asserted human approval.

## Sprint 0030 — reused Officer vest integration

Officer protection reuses existing Armor_0/Armor_1 native sprites through the
existing ArmorVisual on the torso pivot. No character base parts, weapon art,
joint pivots or approved environmental artwork were redrawn. The cap/head remain
exposed to preserve the miniboss's readable ballistic weakness. Controlled capture:
TestResults/Sprint0030/ArmoredOfficer.png. This is implementation/capture review,
not human artistic approval or Android-device readability confirmation.
