# AKUMU 2 — Permanent Graphics Bible

黒橋 GAMES — Kurohashi Games · Established 2026-10-25

## Authority and purpose

This is the current visual-direction authority for environments, characters,
props and future UI. It consolidates Sprint 0026 and its refinements without
redesigning approved work. [ART_BIBLE.md](ART_BIBLE.md) retains useful rig,
attachment, provenance and implementation history. Its dated experiments and
superseded restrictions are historical; this document governs artistic direction.
Later explicit user decisions can revise it. An asset's presence in Unity, a
passing test or a Codex capture does not establish artistic approval.

**One game. One artist. One consistent graphical language.**

## MANDATORY RULES FOR CODEX

- Read this bible before graphics work; inspect the relevant approved assets and
  their actual in-game appearance before drawing. Check whether an existing module
  can be reused, recolored or adapted before creating another sprite.
- Aim for an enthusiastic teenager making a good-looking game in LibreSprite:
  modest, charming, thoughtful, colorful and slightly awkward. Never deliberately
  ugly, professionally over-rendered or generically asset-pack-like.
- Preserve chunky hard pixels, square heads, rectangular bodies, simple modular
  limbs, flat colors and readable silhouettes. More recognizable detail is welcome;
  finer resolution and more sophisticated shading are not automatic improvements.
- Approved crates and the basic Larper baseline are references to preserve.
  Do not silently promote provisional artwork to approved status.
- Preserve two visible arms, pivots, animation ownership, equipment overlays,
  grips/muzzles, colliders and gameplay. Fix art in the visual layer. Do not change
  physical dimensions, camera behavior or movement to make artwork fit.
- Preserve historical originals. Character work also requires inspecting the
  actual Enemak references identified in AGENTS.md and ART_BIBLE.md; never claim
  unavailable files were inspected. Third-party prototype references are not
  publication-ready original art.
- Use Unity Editor APIs for scene/prefab/native-asset authoring, not serialized
  YAML/GUID edits. Compare at gameplay zoom and phone size. Compilation and
  controlled captures are not human artistic or Android-device acceptance.

## Visual philosophy

**A passionate teenager making a pixel-art game in a free drawing program, with
surprisingly sophisticated gameplay underneath.**

The imagined artist has enthusiasm, imagination, humor and basic skill, but no
professional pipeline or art budget. They are trying their best. Modest art should
support expressive physics, reactive AI and readable combat. Small asymmetries,
crooked paint, odd expressions and environmental jokes give the work personality;
careless rectangles, random defects and clutter do not.

Keep three principles together: modest art/excellent mechanics; chunky pixels/
charming details; one artist/one identity. Different locations may have different
colors and subjects, but must share construction, apparent pixel scale, outlines,
shading complexity and rendering sharpness. A forest, bunker or city must not
suddenly become professional indie illustration, realistic military art or a
high-resolution texture pack.

## Shape, color, outlines and shading

Characters keep square or nearly square heads, rectangular torsos, simple modular
arms/legs and tiny expressive faces. The loose Minecraft-like geometric influence
is about immediate readability, not copying assets. Archetypes can vary in size
and costume while retaining this language. Keep both arms distinguishable and
weapons attached coherently through facing changes and articulated poses.

Use a main flat color, one darker structural shade and an occasional useful
highlight. Separate clothes, skin, equipment and background with clear color/value
boundaries. Warehouse greens/greys/browns are examples, not a universal palette.
Other settings may use very different dominant colors with the same technique.
Do not impose a single color-count limit on every complete character or level.

Current characters primarily use color-to-color boundaries rather than continuous
black contours. Faces and equipment use a few native-pixel ink marks; equipment
can have a one-pixel ink edge. Crates have darker chunky construction edges and
small rivets/brace marks. Coarse environment silhouettes and panel edges use
broader four-native-pixel cells. Match the relevant reference at world scale;
do not add a thick universal outline to every character or prop.

Hard, flat pixel shadows and simple underside bands are appropriate. No smooth
gradients, realistic illumination, glossy reflections, elaborate ambient occlusion,
anti-aliased sprite edges or delicate multi-step shading ramps. Legacy VFX masks
documented in ART_BIBLE.md are existing exceptions, not environment-art templates.

Representative warehouse swatches: ink `#221F23`, wood `#895734`, plank `#B18046`,
seam `#583728`, steel `#66706D`, rust `#895C3E`, ochre `#B99C4F`, dark `#22292E`.
Basic character references include peach `#FFD6A5`, ink `#191D2B`, brown `#502B0C`,
red `#C22B16`, blue `#2242DC`, teal `#0C6374`, light `#FFFFEB`.
These are reusable examples, not a mandatory global palette. Existing non-crate
base environment textures use 1–3 opaque colors; new decoration modules use at
most five. Keep similarly small palettes per component where practical.

## Reference assets and approval status

Paths below are Unity project-relative. Inspect actual files and the associated
skin/prefab, not an earlier similarly named candidate. Approval is deliberately
separate from current implementation.

| Category | Actual references | Status and use |
|---|---|---|
| Wooden crates | `Assets/Art/LevelArt0026/CrateWoodBrace_32x32.asset`, `CrateWoodBrace_32x29.asset`, `CrateWoodPlanks_32x32.asset` in the same folder | **Explicitly approved** environment benchmark. Preserve current pixels, proportions and modest detail. |
| Industrial crates | `Assets/Art/LevelArt0026/CrateTinCorners_32x32.asset`, `CrateRustPanel_32x32.asset`, `CrateRustPanel_52x29.asset` | **Explicitly approved** crate family; no automatic coarse-grid rebake. |
| Platforms/floors | `Assets/Art/LevelArt0026/Concrete_436x26.asset`, `Concrete_1728x32.asset`, `Walkway_160x12.asset` in that folder | Current Sprint 0026.2 implementation references. Coarse technique follows user direction; final artistic/device review remains pending. |
| Walls/structures | `Assets/Art/LevelArt0026/Masonry_128x144.asset`, `Boundary_32x928.asset`, `Window_96x64.asset`, `Support_8x832.asset`, `Ladder.asset`, `LadderFoot.asset`, `LadderTop.asset` | Current environment references, not blanket artistic approval. Preserve ladder endpoints and collision relationships. |
| Industrial decoration | `Assets/Art/HandmadeEnvironmentDetails/Details.asset`: `PipeValve`, `ElectricalBox`, `BoxFan`, `StorageShelf`, `ConcreteRepair`, `NopeWarning`, `FineWarning`, `HardShadow` subassets | Current revised Sprint 0026.2 work; awaiting human artistic/device review. Reusable coarse modules, not new gameplay objects. |
| Basic Larper | `Assets/Data/Visuals/LarperCandidate.asset`; `Assets/Art/Sprint0245/Larper_Head.asset`, `Larper_Torso.asset`, `Larper_Arm.asset`, `Larper_Leg.asset`; `Assets/Prefabs/Enemies/Larper.prefab` | **Production baseline explicitly selected in Sprint 024.7**, despite the historical Candidate name. Preserve; older candidate notes do not revoke that decision. |
| Player | `Assets/Data/Visuals/HeroRefined.asset`; `Assets/Art/CameraSamuraiHotfix/WorkerCapRefined.asset`, `WorkerShirtRefined.asset`, `WorkerSleeveRefined.asset`, `WorkerTrousersRefined.asset`; `Assets/Prefabs/Player/WarehousePlayer.prefab` | Current prefab dependency and refinement reference; redesigns require artistic/playtest review. `Hero0247.asset` is an earlier recipe, not the current player reference by default. |
| Samurai | `Assets/Data/Visuals/Samurai0247.asset`; `Assets/Art/Sprint0247/SamuraiHelmet.asset`, `SamuraiCardboard.asset`, `SamuraiArm.asset`, `SamuraiLeg.asset`; `Assets/Prefabs/Enemies/Samurai.prefab` | Implemented redesign with later scale/hitbox refinements; not asserted artistically approved. Earlier high-resolution Sprint0243 art is not the target technique. |
| Fireworks expert | `Assets/Data/Visuals/Fireworks0248.asset`; `Assets/Art/Sprint0248/CheapHardhat.asset`, `FireworksShirt.asset`, `FireworksSleeve.asset`; `Assets/Art/Sprint0247/FireworksLeg.asset`; `Assets/Prefabs/Enemies/FireworksExpert.prefab` | Current redesign reference; artistic/playtest acceptance pending. |
| Officer | No Officer-specific sprite, skin or prefab identified in the inspected project | **No established approved reference.** Do not invent approval, alias another character as Officer, or create new art during documentation. Future work must establish the reference explicitly. |
| Menu/HUD/shop | Existing implementation and captures only | Functionality must be preserved; **not fully approved artistic references**. Future overhaul is separate. |

Visual comparisons: `TestResults/HandmadeDetails_Runtime.png`,
`HandmadeDetails_LadderRuntime.png`, `HandmadeDetails_ProcessingRuntime.png` show
the current warehouse at gameplay zoom. `TestResults/Sprint0248_ReferenceSheet.png`
is a useful earlier multi-pose comparison, not proof of current player equipment,
pose approval or human playtesting. Historical Enemak originals live under
`HistoricalArtSources/Drive/Postavy/Enemak/`; preserve them unchanged.

## Measured implementation and standards for future work

Inspected 2026-10-25 using read-only Unity asset queries, existing Editor recipes,
rig code, development history and saved gameplay/reference images. Values below
describe the named references, not every legacy sprite in the repository.

| Property | Observed implementation / rule |
|---|---|
| Density | Current modular characters, all 32 base environment textures and the 15 detail-sheet sprites use **32 PPU**. Keep this storage density for matching additions. |
| Apparent pixel scale | Non-crate environment and new detail recipes use **4×4 native-pixel cells**, equivalent to an 8 PPU drawing canvas stored at 32 PPU. Approved crates and characters retain finer native-pixel marks; do not resample them to force uniformity. Match apparent scale within each asset family. |
| Character modules | Larper and refined player: head 12×11, torso 12×17, arm 5×18, leg 6×18 pixels. Samurai: 14×10, 10×11, 4×12, 5×12. Fireworks0248: 16×13, 14×17, 5×18, 6×18. These deliberately vary by archetype. |
| Environment canvases | Crates: 32×32, 32×29 or 52×29. Ladder body: 32×16; caps: 32×4. Window: 96×64; vent: 64×48. Long floor example: 1728×32; full-object dimensions are not evidence of high-detail drawing. |
| Shared detail sheet | Native 256×256 Texture2D with 15 Sprite subassets; examples: signs 80×48, fan 64×64, shelf 96×64, pipe/valve 64×80, cabinet 48×56. Existing recipe uses four-pixel padding and coarse cells. |
| Rendering | Inspected reference textures are Point filtered, with one mip level (no mip chain). Environment textures are RGBA32. Preserve crisp nearest-neighbor rendering; avoid lossy compression that damages pixel colors/edges. |
| Import versus native assets | Many current references are native `.asset` Texture2D/Sprite objects authored by Editor recipes, not imported PNGs. For new raster imports, use Sprite/2D, matching 32 PPU/pivots, Point, mipmaps off and non-lossy settings; inspect Android overrides. Do not pretend every native asset has TextureImporter settings. |
| Transparency | Coarse detail recipes use only alpha 0 or 255, without feathering. Preserve clean transparency and padding; check atlas edges for bleeding. Do not infer that all historical VFX use binary alpha. |
| Pivots | Current head/torso modules center at normalized (.5,.5); full arm/leg modules use (.5,1). Weapon sockets/pivots differ intentionally. Preserve each existing attachment and articulated segment pivot. |
| Scale | Character modules normally use unit scale; facing mirrors X through the existing rig. Samurai has a documented 1.05 visual-scale exception. Environment visual children can counter-scale scaled collider roots; do not reset transforms blindly. |
| World bounds | Preserve SpriteRenderer size/draw mode and collider bounds. Full-object textures can clip fractional edge pixels rather than rounding geometry or stretching all pixels. Do not pixel-snap physics actors or change the camera as an art shortcut. |

Current environment authoring uses SpriteRenderers, not a demonstrated Tilemap
pipeline. No canonical tile size or RuleTile scheme is established here. If future
levels use Tilemaps, derive cell size from approved modules and the existing
world scale; do not impose a new grid on current geometry.

Reuse `Details.asset` modules and approved crate variants before adding variants.
The established recipe sources are `Assets/Editor/LevelArt0026Setup.cs` and
`Assets/Editor/HandmadeEnvironmentDetailsSetup.cs`; **reading them is safe, running
authoring/bake methods changes assets**. Do not regenerate approved assets merely
to inspect them. A future SpriteAtlas must preserve Point filtering, padding,
transparency and platform sharpness; atlas migration is not required by this bible.

## Layering, animation and gameplay relationships

Keep background objects behind actors and projectiles, with enough contrast for
combat. Current handmade props use sortingOrder −16, hard shadow bands −15 and
surface marks 1. These are existing local conventions, not universal sorting-layer
IDs. Preserve the target scene's sorting layers and neighboring renderers.

The established character depth convention places rear arm near 9, torso 10,
armor 11, front arm 14 and hand 17, with intentional pose-specific overrides.
Use the actual rig hierarchy and ART_BIBLE.md for details; do not flatten all
limbs into one order. Both arms must remain distinguishable during aiming,
reload, climbing, crouch, facing changes and ragdoll transitions.

`CutoutRig`, `CharacterVisualDriver` and the existing optional articulated rig
own animation. Preserve joint chains, physics masks, hit reactions, ragdoll
fallbacks, grips, muzzle origins and equipment-dependent overlays. Armor graphics
must reflect actual equipped/remaining armor; art must not grant protection.
Never add physics to decorative wall sprites. No new animation framework is
needed to achieve this style.

## Environmental storytelling and future levels

Use recognizable large clusters: cracks/chips, rust patches, rectangles forming
pipes, chunky valves, grilles, machinery, support beams, signs, posters, fictional
ads, crude graffiti and small jokes. Detail can be plentiful when composition
supports it, but should not look procedurally scattered. Vary placement with
intention; retain negative space and clear platforms, hazards, enemies and shots.

A forest might replace pipe/steel modules with blocky trunks, foliage masses and
hand-painted notices; a bunker might use heavier slabs and warning plates; a city
might use simple shopfronts and posters. These are future examples, not requests
for new assets. Keep the same apparent pixel treatment, square construction,
outline hierarchy, flat shading and modest handmade personality across locations.

## Future UI direction — no redesign authorized here

Future UI should use chunky pixel buttons, rectangular panels, basic readable
icons, consistent outlines and limited shading. Avoid glossy mobile-app surfaces,
3D controls and elaborate effects. Touch targets, legibility, safe areas and
Android responsiveness take priority over strict pixel purity. Preserve existing
functionality and input behavior; the current menu/HUD is not automatically an
approved style template. UI overhaul needs its own implementation scope.

## Anti-patterns

Never introduce mismatched styles between levels, AI-looking modern pixel polish,
unnecessarily detailed professional spritework, sophisticated shading on simple
objects, soft/blurry sprites, smooth vector-like characters, inconsistent pixel
density, excessive clutter, generic asset-pack aesthetics or arbitrary proportion
changes. Do not disguise random ugliness or deliberate quality reduction as
amateur art. Revise an out-of-style new asset; do not redesign the game around it.

## Mandatory workflow and completion checklist

1. **Before:** read this bible and relevant technical history; inspect approved
   reference files and gameplay views; identify provisional work; check reuse;
   compare neighboring levels' pixel scale, silhouettes, outlines and palette.
2. **During:** use simple flat-pixel construction; preserve animation, physics,
   gameplay, attachments and performance. Keep decoration readable at mobile zoom.
   Author Unity assets/scenes through supported Editor APIs.
3. **After:** compare against canonical crates/Larper and relevant context. Check
   pixel scale, excessive shading/polish, silhouette, two-arm layering, both-facing
   equipment alignment, transparency/atlas edges and background/foreground contrast.
4. Verify that changes affect only the authorized scope. Run relevant automated
   tests when code or integration changes warrant them; inspect gameplay-camera
   captures. Record human artistic and Android-device checks separately.
5. Ask the practical question: **could the same enthusiastic teenager plausibly
   have drawn this and the approved crates over a few weekends in LibreSprite?**
   If the asset looks like another artist made it, revise that asset.

Documentation establishment changed no game artwork, scenes, UI or gameplay.
Existing exceptions remain documented rather than silently normalized.
