# Sprint 0026.2 - coarser environment pixel grid

Implemented; awaiting artistic acceptance and Android-device review.

## Change

Existing non-crate environment pixels are baked into **4 x 4 native-pixel blocks**, equivalent to drawing on an **8 PPU logical canvas** and scaling up with nearest-neighbor. Native Sprite PPU remains 32, so no transforms, object dimensions, sprite rectangles, tiling sizes, collider geometry or camera behavior change. Partial cells at existing fractional bounds remain clipped rather than rounding the level layout.

Platforms, window/door divisions, hazard steps and other structures use that same coarse grid. Narrow support beams/pipes use two broad color columns; ladder rails, rungs and caps use explicit four-pixel cells so downsampling cannot erase their readable structure. No new colors, soft shading ramps, shadows, shaders, props or texture files were introduced. Non-crates still have at most three colors including transparency. Flat backgrounds remain flat; their deliberate empty space was preserved.

The approved crates are excluded from the bake. Their artwork, construction details, references and metadata remain unchanged. The target remains earnest handmade amateur art, not ugliness or a new environment redesign.

## Technical inspection

Textures were already Point-filtered, uncompressed RGBA32, mipmap-free (one level), and uniformly 32 PPU. The inspected URP asset uses MSAA sample count 1 and render scale 1; global QualitySettings anti-aliasing was 0. These settings were not changed. There was no texture-import blur to fix; the change reduces authored visual resolution instead.

26 existing non-crate assets processed, with 24 actual file-content changes; two were already unchanged by coarse sampling. Total remains 32 textures. No scene authoring was required in this pass. The Editor-only recipe now also bakes new non-crate recipe output to this grid and rejects authoring during Play Mode. Nothing runs per frame.

## Verification

- **789 protected file hashes unchanged**, including all approved crate assets/metadata, runtime code, character/enemy/player prefabs, UI, shop, original art, main menu, data and selected project settings. Only expected non-crate art changes are outside that protected set.
- **3/3 focused checks pass**: two Edit Mode tests and one Play Mode test. Exact four-pixel cell uniformity, bounded color counts, Point/RGBA32/mip/PPU settings, saved scene invariants, crate variants, sparse windows, materials/references, collider-to-visual bounds, real crate raycasts and ladder endpoints were verified.
- Actual gameplay-camera captures generated and inspected: **LevelArt00262_Runtime.png** at the opening and **LevelArt00262_LadderRuntime.png** near the ladder/mezzanine. The latter temporarily relocates the test player and uses the existing camera snap API; no authored camera or scene state is saved from that fixture. Earlier comparison: LevelArt00262_Before.png.
- The first test-run attempt hit a Unity Edit/Play restoration-state error and was canceled. The two modes were then run separately from a stopped Editor and passed. That aborted attempt is not counted as successful verification. An authoring-response timeout was checked against the durable success file and actual saved asset state before proceeding.
- Final compilation successful, zero current Console errors. One unrelated Unity AI Toolkit account/network timeout warning remains. Historical Pipeline-buffer errors were not manually cleared.

No full historical suite, human combat playthrough, Android hardware test or device performance profiling was performed. Technical checks and controlled captures do not constitute artistic approval. Previously documented unrelated issues are not claimed fixed.

## Files / state

- Assets/Art/LevelArt0026/: existing non-crate assets only.
- Assets/Editor/LevelArt0026Setup.cs: deterministic coarse-pixel bake and stopped-mode guard.
- Assets/Tests/EditMode/LevelArt0026Tests.cs: exact block-grid check.
- Assets/Tests/PlayMode/LevelArt0026PlayTests.cs: new capture paths and a second controlled gameplay-zoom view.
- ART_BIBLE.md, DEVELOPMENT_LOG.md and LevelArt00262 evidence files under TestResults.

Warehouse_Demo is saved/open/clean outside Play Mode. All earlier uncommitted work and recovery files retained. Nothing staged, committed or pushed.

Manual review: judge chunk size at normal phone scale, especially ladder rails/rungs, short platform edges, hazard stripes and windows while moving and firing. Confirm that readability/charm remain intact and no clipped edge is mistaken for collision. Check actual Android rendering/performance; the camera and gameplay intentionally remain unchanged.
