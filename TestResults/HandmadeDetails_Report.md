# Sprint 0026.2 revised brief — environment character

Implemented by Codex; human artistic and Android playtest approval pending.

## Changes

- Added 15 independently code-drawn, flat-color sprite modules on one shared
  256x256 sheet (256 KiB base RGBA pixel payload, excluding asset overhead).
  Four-native-pixel cells, 32 PPU, Point filtering, no mipmaps, binary alpha,
  at most five opaque colors per module. No runtime art generation.
- Each warehouse receives 11 deliberately placed background details: two humorous
  signs, two electrical cabinets, a pipe/valve, fan, shelf, concrete repair/crack,
  stain, painted number and warning lamp. Some areas intentionally remain blank.
- Four small surface marks and four hard, opaque shadow bands per warehouse.
  No gradients, dynamic lights, random scattering or added physics/behavior.
- Three scenes contain 57 new SpriteRenderers total, under separate decorative
  roots. Background props sort behind characters. Surface marks remain inside
  existing platform bounds. The office sign was moved left after capture review
  revealed a ladder obscuring its lettering.
- Approved crates and the existing coarse environment assets are byte-identical.

## Files for this revision

- `Assets/Editor/HandmadeEnvironmentDetailsSetup.cs` and generated metadata.
- `Assets/Art/HandmadeEnvironmentDetails/Details.asset` and generated metadata.
- `Assets/Scenes/Warehouse_Demo.unity`, `Warehouse_Testbed.unity`,
  `Warehouse_Mission01.unity`, authored through Unity Editor APIs.
- `Assets/Tests/EditMode/HandmadeEnvironmentDetailsTests.cs` and metadata.
- Existing `Assets/Tests/PlayMode/LevelArt0026PlayTests.cs`: decoration assertions
  and three gameplay-camera captures, retaining collision/ladder checks.
- `ART_BIBLE.md`, `DEVELOPMENT_LOG.md`, and `TestResults/HandmadeDetails_*` evidence.

Earlier uncommitted art passes and recovery files are preserved. No runtime
scripts, prefabs, UI, menu, shop, camera, AI, weapon or gameplay settings changed.

## Validation and limits

All 16 focused tests passed: 2 Edit Mode, 1 warehouse Play Mode, 12 existing cover
tests and 1 existing ladder/gravity test. Compilation succeeded; final current
Unity Console has zero errors and zero warnings. Demo is saved, open, clean and
outside Play Mode. See `HandmadeDetails_Regression.json` for test totals and
`HandmadeDetails_FinalVerification.json` for saved Editor state. Focused tests
cover the shared sheet, all three scenes' decoration-only components, unchanged
original scene data, actual Demo crate bounds/raycast collision, ladder endpoints,
existing cover behavior and disabled/destroyed-ladder gravity restoration.

All 815 protected original file hashes match, including all six approved crate
assets and metadata. All 3081 original scene transform/nonvisual snapshot entries
match; additions are decorative only. Before/after records are retained.

Opening, ladder/mezzanine and office gameplay-camera screenshots were inspected.
These are controlled Editor test captures, with enemy combat disabled and the
player relocated for the latter views; they are not human combat playtests.
No Android build, device profiling or full historical regression run is claimed.

Manual review: walk/climb through Demo on a phone, check prop readability and
character/projectile contrast during busy combat, assess charm/density against
the approved crates, and check scrolling pixel sharpness and frame rate.
No staging, commits or pushes.
