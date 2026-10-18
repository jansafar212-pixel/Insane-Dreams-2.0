# Art manifest — Sprint 009

## Sprint 009 active revisions (2026-10-18)

The approved general Sprint 008 aesthetic is retained. Only anatomy, matching
expressions, two equipment representations and ladder caps were redrawn with
code-authored pixels. No historical art was fetched, traced or sampled. Existing
warehouse tiles, primary scrap firearm and patchwork canopy are reused unchanged.
These are original foundation assets subject to human review, not legal clearance.

New outputs under `Assets/Art/Sprint009/` (24 PNGs, same point/no mip/uncompressed/
32 PPU/full-rect conventions):

| Pieces | Dimensions | Pivot / change |
|---|---|---|
| Worker head + six faces | 12×10 each | Centre; safety helmet/neutral, smug, angry, surprised, scared, stunned. |
| BucketLarper head + six faces | 12×11 each | Centre; existing slate-helmet/purple-clothing identity retained. |
| Two torsos | 12×17 | Centre; substantial body and shoulders. |
| Two arms | 5×18 | Top-centre shoulder attachment, longer reach. |
| Two legs | 6×18 | Top-centre hip attachment, chunky existing boot style. |
| CompactFirearm | 20×8 | (0.15,0.5), +X; appearance for existing firearm slot 2, no new weapon definition. |
| ScrapWrench | 18×8 | (0.15,0.5), +X; appearance for the existing melee slot. |
| LadderFoot, LadderHandrail | 32×4 each | Centre; reusable visual terminations, no collider. |

Neutral standing renderer-bounds measurements: Larper head **0.34375 / 1.403125
= 24.5%** (previous 0.75 / 1.41375 = 53.1%); player head **0.3125 / 1.371875
= 22.8%** (previous 0.625 / 1.35125 = 46.3%). These measure sprite bounds including
transparent border pixels, rather than asserting exact opaque-pixel anatomy or
subjective visual approval.

The existing Sprint008WorkerSkin / Sprint008BucketLarperSkin assets now reference
the Sprint 009 body parts and overlays. Names are retained to preserve references.
CutoutRig derives neck, shoulder and hip heights from the skin dimensions and
anchors feet at -0.7 world units; gameplay colliders remain unchanged. Shoulder
half-width is 0.1875, torso centre Y 0.09375. Crouch uses its own compact layout.
Speech anchors are just above each resized head. The weapon mount follows the
carrying arm's actual hand endpoint; empty equipment hides the one renderer.

Ragdoll sprites, collider dimensions/offsets, neck anchor and mass distribution
match this same skin. Activation transfers each visual part's world pose and
absolute scale. Historical assets and earlier PNGs remain preserved, unbound from
the active character anatomy. Earlier authoring utilities can restore older art;
use Sprint009PresentationSetup.Apply for this configuration.

LadderVisual2D separately authors visual endpoint heights and body/top/bottom
renderers, with world-pixel snapping. Feet are at -7 for both basement ladders and
the nearest pixel to 5.4 beside the roof-access mezzanine. Top handrails retain the
existing traversal endpoint heights (about 14.4, 6.6, 13.4) so a character never
climbs beyond the visible ladder. Their extensions above upper landings are
intentional handrails matching existing traversal; all acquisition/exit colliders
and open platform gaps remain unchanged.

## Sprint 008 foundation (retained implementation history)

## Current active artwork (2026-10-18)

WarehousePlayer, Larper, both nested visual prefabs and LarperCartoonRagdoll now
reference **Assets/Art/Sprint008/** for their character parts, six facial overlays,
weapon and canopy. Historical skins and source archives were preserved but are no
longer used by those active rigs. The obsolete flat parachute renderers are
disabled; the cutout canopy follows the existing traversal state.

`Assets/Editor/Sprint008ArtSetup.cs` independently draws each output pixel using
authored coordinates and palettes. It reads no historical sprites, photographs,
prompts, downloaded images or source crops. No tracing, legacy palette sampling,
or generative image service was used. These are deliberately scrappy **original
foundation / production-candidate assets**, not a claim of final professional art.
The worker's amber safety helmet/rust workwear and the Larper's slate bucket helmet/
purple patchwork outfit replace the old blonde-green and historical ragdoll designs.

| New path under Assets/Art/Sprint008 | Dimensions | Origin / pivot / use |
|---|---|---|
| `Worker_Head.png`, `Worker_Face_*.png` (six) | 16×20 | Code-authored worker head and neutral/smug/angry/surprised/scared/stunned overlays; centre pivot. |
| `BucketLarper_Head.png`, `BucketLarper_Face_*.png` (six) | 18×24 | Independently drawn helmeted Larper and matching expressions; centre pivot. |
| `Worker_Torso.png`, `BucketLarper_Torso.png` | 12×14 | Rust safety jacket / purple patched tunic; centre pivot. |
| `Worker_Arm.png`, `BucketLarper_Arm.png` | 5×14 | Shoulder pivots at top-centre. |
| `Worker_Leg.png`, `BucketLarper_Leg.png` | 6×15 | Hip pivots at top-centre. |
| `ScrapCarbine.png` | 24×8 | Generic code-authored scrap firearm; pivot (0.15,0.5), barrel +X. Existing weapon rules unchanged. |
| `PatchworkCanopy.png` | 64×36 | Original alternating rust/amber canopy and cords; bottom-centre pivot. |
| `ImpactSpark.png` | 8×8 | Original crisp cross/diagonal spark for geometry and corpse impacts. |
| `Concrete.png`, `MetalWalkway.png`, `HazardBeam.png` | 32×16 / 32×8 / 16×8 | Tiled floor, walkway and warning trim, centre pivots. |
| `ConcreteWall.png`, `SupportBeam.png` | 64×64 / 8×32 | Low-contrast mortar wall and riveted supports, centre pivots. |
| `LoadingDoor.png`, `FactoryWindow.png` | 48×80 / 48×32 | Industrial door and divided windows, centre pivots. |
| `Vent.png`, `Pipe.png`, `ShippingCrate.png`, `Ladder.png` | 32×24 / 8×32 / 32×32 / 32×16 | Reusable industrial detail and readable ladder tile, centre pivots. |

All 34 PNGs use 32 PPU, point filtering, no mipmaps, uncompressed RGBA, full-rect
sprite meshes and clamp wrapping. Two new skin assets live in Assets/Data/Visuals:
Sprint008WorkerSkin and Sprint008BucketLarperSkin. Sprite/skin changes remain
independent of gameplay, posture and damage authority. Ragdoll collider sizes were
adapted to its new corresponding part sprites; warehouse gameplay geometry was not.

WarehouseArt_Sprint008 contains only decorative renderers. Existing collider
transforms, dimensions, offsets and ladder capture volumes are preserved and
recorded in TestResults/Sprint008GeometryBaseline.json. Sorting uses distant wall
-30, windows -23, supports -20, door/vent -18, pipe -17, terrain 0, ladders 1,
existing checkpoints 3, actors/weapon 8–15, combat cues 30, then screen-space UI.
There is no foreground layer covering normal actor routes and no per-frame
parallax search or decorative physics. Some old flat renderers remain disabled
as editable testbed references rather than being deleted.

### Ownership status

- **Historical third-party/reference:** all selected Drive sources and derivatives
  documented below remain private archival/prototype material. They are not cleared
  for publication and the developer states much of the old art belonged to someone else.
- **Temporary prototype:** Sprint 007 TEMP and Derived PNGs/skins remain retained,
  unbound from the current Warehouse character rigs. Old authoring helpers can
  deliberately restore them; do not run those helpers for release artwork.
- **New independently authored candidates:** the Sprint 008 source-code pixel
  patterns and their PNG outputs above contain no imported/traced historical image
  data. Human style, originality and provenance review is still required.
- **Approved/legally cleared for publication:** no legal-clearance or owner approval
  is asserted merely because these assets were newly created. Record the developer's
  eventual approval separately before release.
- **Audio:** existing and new audio clip references are configurable but unassigned;
  no historical or unlicensed sounds were added.

## Sprint 007 historical audit (retained for provenance)

Audit: 2026-10-18. No AI imagery, third-party art packs, old scripts, fonts, music, unrelated photos or personal documents were imported. Downloads and the local archive remain unchanged. Only derived PNGs under `Assets/Art/Sprint007` enter Unity; source copies live outside Assets under `HistoricalArtSources`.

## Provenance and access

The developer identifies the main collection as original handmade Insane Dreams artwork in [věci k Insane dreams(NE ZDROJ)](https://drive.google.com/drive/folders/0BysSmO9jyT4SUGljTkRlTkVkZE0). Google Drive discovery succeeded during this sprint. Selected files were fetched using the Google Drive connector, then copied from its authenticated download references. No Drive contents or permissions were modified. `HistoricalArtSources/Drive/INVENTORY.json` records metadata for Postavy, its Hráč/Enemak/Ragdoll children, Zbraně, Padák, Platformy and Pozadí. This is a bounded folder audit, not a claim that every nested archive image was reviewed or recovered.

Only five relevant originals were downloaded. Other character animation frames, reloading variants, backgrounds and platforms remain on Drive and are deferred. Meme/reference images and unrelated material were excluded. Platform/environment replacement is outside this sprint.

**Recovery instruction: historical artwork, including potentially third-party material, is permitted for private prototyping only. All historical/third-party graphics must be replaced with new, independently created modular pixel art before publication.** Retrieval from the developer's Drive does not establish authorship or publication rights. No replacement artwork was generated during recovery. The replaceable skins and rigs remain the migration boundary. The local archive contains font-specific license notices (Retro Computer and Joiro Hatagaya); these do not license the images and no fonts were imported.

## Selected unchanged sources

Paths below are relative to `HistoricalArtSources/Drive/`. A `.gif.png` name reflects the actual PNG returned by Drive despite the historical `.gif` display name. GIF preview PNGs are derived first-frame format conversions, never replacements for the GIF source.

| Local source | Drive ID | Dimensions | Purpose / integration |
|---|---|---|---|
| `Postavy/Hráč/Obrazek 0.gif` | `0BzGVO20R0FApTEZiekc1bGJjYzg` | 420×500 | Original blonde, green-clothed player. Head, torso, arm and leg crops drive the player skin. |
| `Postavy/Ragdoll/Ragdol d.gif` | `0B_FYo_Xxnpf-SnFrR3piS0o2RG8` | 1366×768 | Original separated head/hat, patterned sleeve/leg and torso. Shared by living Larper and its jointed body. |
| `Postavy/Enemak/Postava.png` | `0BysSmO9jyT4SemNkQjY2QlFzT28` | 1600×799 | Original brown/red/blue character. Inspected reference; not bound to the current Larper rig. |
| `Padák/Padák.gif.png` | `0B_FYo_Xxnpf-N1ZlZ1FjOGVjM0U` | 1366×667 | Original black canopy/cord outline. Cropped, nearest-sampled and recolored cream for contrast in the warehouse. |
| `Zbraně/MKG.gif.png` | `0B_FYo_Xxnpf-cnM5QXJ3ZG5YSzA` | 1349×667 | Original firearm appearance; cropped/mirrored to the existing +X aiming convention. Visual only: no new weapon definition. |

`HistoricalArtSources/Drive/SOURCE_HASHES.json` records SHA-256 for these unchanged downloads and their local format previews. No expiring bearer/download URLs are stored.

## Exact derivatives in Assets/Art/Sprint007

Crop rectangles use source top-left coordinates `(x,y,width,height)`. Nearest-neighbour block/centre sampling preserves source colors and transparency; normalization fits the existing 0.8×1.4 collider rather than rescaling gameplay. These are adaptations, not newly discovered animation frames.

| PNG prefix/name | Source crop | Output pixels / pivot | Status |
|---|---|---|---|
| `Derived_Player_Head` / `OriginalFace` | player (170,50,80,80) | 16×16, centre | Original hair/skin retained; original face isolated as neutral overlay. |
| `Derived_Player_Torso` | player (190,130,40,170) | 12×14, centre | Original green shirt. |
| `Derived_Player_Arm` | player (150,210,40,100) | 5×14, top-centre | Original arm/hand crop. |
| `Derived_Player_Leg` | player (220,350,30,150) | 6×15, top-centre | Original trouser/boot crop. |
| `Derived_Larper_Head` / `OriginalFace` | ragdoll (678,142,90,140) | 18×28, centre | Original hat/neck/face; deliberately oversized head. |
| `Derived_Larper_Torso` | ragdoll (524,71,100,250) | 12×14, centre | Original dark torso. |
| `Derived_Larper_Arm` | ragdoll (59,88,190,50) | 5×14, top-centre | Horizontal original sleeve rotated into a vertical shoulder-pivot part. |
| `Derived_Larper_Leg` | ragdoll (431,104,50,180) | 6×15, top-centre | Original patterned limb. |
| `Derived_MKG` | firearm (240,165,890,270) | 30×9, (0.2,0.5) | Mirrored so barrel follows +X aim. |
| `Derived_Padak` | canopy (405,210,360,210) | 48×28, bottom-centre | Original alpha outline, cream recolor. |
| `Derived_*_TEMP_Face_*` | matched head layout/palette | 16×16 / 18×28, centre | **TEMPORARY** smug, angry, surprised, scared, stunned overlays. Replace these first with authored originals. |

All derivatives use 32 PPU, point filtering, no mipmaps, full-rect meshes, clamp wrapping and uncompressed RGBA. Part normalization changes proportions deliberately; originals are always available for a future art pass. No smoothing is applied. Rotating cutouts retain their source pixels but naturally change raster outlines.

## Local archive audit

`C:\Users\jansa\Downloads\Insane dreams-20261017T210428Z-1-001.zip` was inspected read-only. SHA-256: `B54A74842A85B102FCDF4A5B6BA66DE1B4565C3530FC0B087013A0D4A1001B15`.

Three relevant copies under `HistoricalArtSources/Insane dreams/Assets/Sprites/`:

| Filename | Dimensions | Status |
|---|---|---|
| `Ikona.gif.png` | 1663×905 | 24×24 face in 25px blocks, crop (500,100,600,600). Used for the initial rig fallback; remains a source reference. Its `Derived_Ikona_*` PNGs are unbound after Drive integration. |
| `Padak 4.gif` | 1680×949 | Small folded pack reference; not the open canopy, deferred. |
| `Záblesk.gif.png` | 800×799 | Flat orange effect reference; existing combat effects retained. |

Empty sprite directories in that ZIP include Backgrounds, PlayerDead, Parachute, Weapons, Enemy/1–3 and Platforms/Player. Legacy Ragdoll/RagdollV2/MkgRagdoll/Human prefabs were present but were not imported; their presence does not establish that their referenced textures exist. Unrelated archive contents were excluded. Initial `TEMP_*` body/canopy/face PNGs remain explicitly named fallback art, unbound after Drive integration.

## Replace art without rewriting gameplay

1. Preserve new originals under a provenance folder; create working PNG derivatives/imports separately. Configure point/no mip/uncompressed/32 PPU, with matching shoulder/hip/head pivots. Adjust a visual prefab's local mounts if a future silhouette requires it; leave gameplay colliders and authority on the parent.
2. Edit the `CharacterSkin` assets in `Assets/Data/Visuals/` (currently named PlayerTemporarySkin and LarperTemporarySkin because facial variants still need authored art). Their six expression entries follow `FaceExpression` order. `CutoutRig.ApplySkin` changes appearance only.
3. The nested `PlayerCutoutVisual` / `LarperCutoutVisual` prefabs own Head, Torso, four limbs, WeaponMount, FaceOverlay, AccessoryMounts, SpeechBubbleAnchor and canopy. Ragdoll sprites are copied from the living rig at defeat. If future part dimensions change, update its collider/joint anchors too and rerun integrity tests.
4. `Sprint007VisualSetup.Build()` explicitly recreates the **temporary fallback** pipeline. `IntegrateDriveOriginals()` reapplies the selected original crops. Neither runs automatically; ordinary sprite/skin replacement does not require either utility.

## Sprint 012 — Buzzbox automatic pistol

`Assets/Art/Sprint012/BuzzboxPistol.png` (20×8, grip pivot 0.2/0.5) and
`RidiculousMagazine.png` (8×18, top-centre pivot) are new pixel patterns authored
in `Assets/Editor/Sprint012CombatSetup.cs`, without historical image input.
Point filtered, 32 PPU, full-rect, uncompressed, no mipmaps. The magazine is a
separate cosmetic renderer, without physics. `LarperBuzzbox.asset` binds both;
the original rifle/sidearm assets and character skins remain available. Human
publication/provenance review remains applicable; no third-party recovery claimed.
