# AKUMU 2 project guidance

- Follow `LEVEL_DESIGN_BIBLE.md` and `GRAPHICS_BIBLE.md` together. Final creative authority remains with the project owner. Flag major unrequested design decisions; minor reversible polish may follow the established direction.
- Identify root causes before substantial bug fixes. Preserve working mechanics unless deliberately changed; reuse existing systems, avoid architectural duplication and unnecessary dependencies, and test affected integrations. Report unfinished work and verification limits honestly.
- Android hardware is normally disconnected while Codex works. Complete automated checks and supported builds independently; label actual phone gameplay checks PENDING USER VERIFICATION. Never equate Editor checks with real-device testing.

- Before any graphics-related work, read `GRAPHICS_BIBLE.md`, the current visual-direction authority, and inspect its relevant approved references. `ART_BIBLE.md` retains detailed technical/provenance history; dated or provisional entries are not blanket artistic approval.
- Before changing character graphics, read `ART_BIBLE.md` and inspect the actual Enemak reference files under `HistoricalArtSources/Drive/Postavy/Enemak/`. Preserve historical originals. Do not claim unavailable artwork was inspected.
- Sprint 024.7 explicitly selects the current basic Larper as the production implementation baseline and authorizes the protagonist, Samurai and fireworks redesigns. Preserve that baseline; new redesign artwork still requires playtest/artistic review. Older candidate-approval notes are historical.
- Preserve deliberately primitive flat-color pixel art, 32 PPU modular proportions, two visible arms, existing joint pivots, weapon grip/muzzle alignment and equipment-dependent armor overlays. Avoid increasing graphical complexity.
- Inspect existing changes and `DEVELOPMENT_LOG.md` before continuing a sprint. Preserve working movement, traversal, cover, AI, combat, progression, shop and persistence. Use Unity Editor APIs for prefab/scene authoring; do not hand-edit serialized YAML or GUIDs.
- Run relevant Unity Edit/Play Mode tests after code changes. Distinguish automated checks and controlled captures from human gameplay or Android-device verification.
- Do not stage, commit or push unless explicitly requested.
