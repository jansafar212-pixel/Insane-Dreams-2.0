# Sprint 0026 - old-school environment art

Implemented by Codex; artistic/gameplay acceptance and Android-device verification remain pending.

## Applied to the actual level

207 environment renderers updated across Warehouse_Demo (menu-launched level), Warehouse_Testbed and Warehouse_Mission01. The scenes retain their existing layout and roles; this does not turn Testbed into a new final level.

- Four reusable crate constructions: diagonal-braced wood, horizontal planks, tin corner reinforcements, rusty industrial panels. Chunky flat colors, dark edges, small rivet pixels, no wood grain/reflections. Every warehouse retains all four variants; the entrance includes a wooden crate rather than two gray variants.
- Concrete floors with broad slab seams; metal platforms with clear ochre walkable tops, dark undersides and a few bolt marks. No dense diagonal shimmer across whole platforms.
- Large, quiet dark wall blocks; simple windows, support beams, rolling doors, vent and pipe. Background contrast remains lower than crates/actors. No added machinery clutter.
- Chunky outlined ladder rails/rungs and matching caps, reusing existing visual endpoint layout. Ladder volumes and traversal remain unchanged.
- Existing supply pickups, checkpoint/mission indicators and signs remain recognizable and unchanged. Obsolete narrow crate-band overlays were hidden to avoid fighting the new crate drawings.

32 native Sprite/Texture assets under Assets/Art/LevelArt0026, shared 16 opaque colors, 32 PPU, RGBA32, point filtering, one mip level, FullRect meshes. Each texture uses at most six colors including transparency. These are independently authored C# flat-pixel recipes, not generated illustration, recovered historical artwork or asset-store art. Original artwork remains unchanged.

Most sprites are baked at the existing object's world dimensions. Tiled renderer sizes preserve the exact original bounds; a fractional final pixel is clipped rather than rounding collision geometry or stretching the full texture. Decorative wall/ladder repetition stays sparse. New pixel payload is 1,028,292 bytes before Unity/CPU/GPU overhead; native asset serialization is about 2.2 MB. These are asset measurements, not device profiling.

## Geometry and systems protection

Original collider-bearing transforms were not resized or moved. Sprite assignment can change Unity's cached collider sprite-tiling fields even when auto-tiling is disabled. A pre-save invariant caught that case; affected artwork was moved to pure visual children, leaving original sprite/collider configuration intact. Non-uniformly scaled cover roots also get a counter-scaled visual child to keep square 32-PPU pixels without changing their colliders. Sixteen such visual children were added; they have no colliders or gameplay components.

All 3,065 original transform/nonvisual snapshot entries match: Demo 1,361; Testbed 753; Mission01 951. This includes colliders, layers, trigger settings, cover/impact components, ladder volumes/endpoints, camera configuration, spawns, mission/checkpoint references and original transform/active states. Saved scenes were reopened and compared again in Edit Mode. All changed crate visuals match the existing collider bounds within .0002 units, including at runtime after Start.

All 750 protected file hashes remain identical: runtime scripts, UI, data, prefabs, original art, MainMenu scene and selected project physics/build/layer settings. In particular, the menu still references the untouched original shared crate asset. No gameplay, character, HUD, shop, touch input, camera, combat, AI, sound, progression or ragdoll implementation changed.

## Files changed

- Assets/Scenes/Warehouse_Demo.unity
- Assets/Scenes/Warehouse_Testbed.unity
- Assets/Scenes/Warehouse_Mission01.unity
- Assets/Art/LevelArt0026/ (32 new native assets and Unity-generated metas)
- Assets/Editor/LevelArt0026Setup.cs (explicit Editor-only recipes, scene authoring and audit/capture helper)
- Assets/Tests/EditMode/LevelArt0026Tests.cs
- Assets/Tests/PlayMode/LevelArt0026PlayTests.cs
- ART_BIBLE.md, DEVELOPMENT_LOG.md, LevelArt0026 evidence files under TestResults.

Scene/assets were authored through live Unity Editor APIs, not serialized YAML/GUID editing. The original untracked recovery scenes were retained. Nothing staged, committed or pushed.

## Final verification: 20/20 pass

- 2 Edit Mode: texture dimensions/32 PPU/point/RGBA32/no mipmaps/bounded flat palettes; shared <=16 opaque colors; native references/materials, four crate variants in every scene, unit visual scale, exact collider/visual fits, original protected scene data and ladder art references.
- 1 new Play Mode: actual Warehouse_Demo art after Start, crate collider sizes/positions and solid raycasts, ladder visual cap endpoints, gameplay-camera screenshot. Temporary session/save, not the player's personal save.
- 12 existing Sprint022CoverTests: real cover, concealment, peeking/blind fire, shot blocking, crouch poses, touch ownership, reload/healing/jump/recovery and layout checks.
- 4 existing Sprint0225 touch ladder detach tests (up/down/left/right; retain one airborne jump).
- 1 existing ladder-priority/disabled-or-destroyed ladder gravity restoration test.

Raw results: LevelArt0026_Regression.json and individual EditTests, PlayTests, CoverTests, LadderJumpTests and LadderPriorityTests JSON files. Protected manifests, before/after per-scene gameplay snapshots, renderer inventory and assignments accompany them. No full historical suite was run; previously documented unrelated regression issues are not declared fixed.

The authoring helper's initial legacy InstanceID API compile errors were corrected for Unity 6.6 before any scene save. Transport timeouts during authoring/palette writes were checked against actual saved state instead of assuming success. The invariant prevented cached collider-tiling changes from being saved. A scene-specific variety assertion caught the loss of one variant during color placement; all four variants were restored. Final compilation succeeds; current Console has 0 errors / 0 warnings. Historical tool/compiler diagnostics remain in the Pipeline buffer.

## Visual evidence and remaining review

Native controlled Editor-camera comparisons: LevelArt0026_Before_0/1/2.png versus After_0/1/2.png. Final gameplay-camera capture: LevelArt0026_Runtime.png. These were actually rendered and visually inspected. Editor overview cameras retain the authored camera's zoom; the runtime screenshot uses the normal initialized gameplay camera and unchanged HUD. Actors were held still for the runtime composition; this is not a human combat playthrough.

Manual review: judge the deliberately crude style and crate recognition on a phone; move/fire against foreground crates with projectiles and enemies filling the screen; check platform edges, ladder readability, cover entry/peek/blind fire, and different warehouse elevations. Confirm sharpness at real Android resolution and profile frame time/texture memory on the target device. No APK/device profiling, human full-level playthrough or artistic approval is claimed. No new runtime generation, scanning, manager or physics system was introduced; that alone is not proof of unchanged device performance.

Final state: Warehouse_Demo saved, open and clean, outside Play Mode. FinalVerification.json confirms no missing/error materials in its new environment art, 32 textures, 16 opaque colors and no protected scene-data differences.
