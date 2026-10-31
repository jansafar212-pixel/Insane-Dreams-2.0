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

## Mandatory quality gate — verify before expanding

**Hard rule: Do not start implementing the next new gameplay feature, weapon, enemy archetype, hazard, movement mechanic, major UI system or level until the previous addition has passed its acceptance and regression tests.** Apply this to materially changed features as well as entirely new ones. This rule supersedes sprint-number pressure and the temptation to batch unverified features.

For every addition:
1. **Define acceptance before coding.** State what must work, how the player should perceive it, and affected existing systems. Keep the change bounded.
2. **Test the entire feature, not just its happy path.** Cover normal use, boundaries, failure/recovery states, repeated use, interactions with existing systems and visual/animation integration. Enemy types require spawn, navigation, combat behavior, retreat/cover as applicable, damage/death/revive compatibility, readability, and fair engagement checks.
3. **Run automated regression checks.** Add focused Unity Edit Mode / Play Mode tests where practical, preserve previous passing tests, and verify the actual root cause for recurring defects. Do not treat code compilation or a single successful Editor run as sufficient.
4. **Build and obtain real-device verification.** Provide an Android APK plus specific manual test steps. For touch controls, camera, visual readability, performance, overall feel and encounter fairness, the project owner's Android playtest is required. Until reported, mark **PENDING USER VERIFICATION**, not PASS or DONE.
5. **Record evidence and gate status.** Maintain a short per-feature checklist or QA matrix: acceptance criteria, automated tests/results, manual checks, Android verification, regressions, unresolved defects, and explicit owner sign-off. Report FAIL honestly; do not claim unperformed verification.
6. **Fix before expanding.** If a core acceptance check fails, stop adding gameplay content, diagnose the shared cause, correct it, and rerun relevant regression checks. Subsequent work may repair bugs, strengthen tests, or improve documentation, but must not quietly pile on the next untested feature.

A feature is **COMPLETE** only after the relevant tests pass and required owner/device verification is confirmed. **PENDING USER VERIFICATION is a blocker to final acceptance**, not an excuse to call the task done. Exceptions to this sequencing require explicit project-owner approval with the risk documented. No deadline or sprint number overrides this gate.
