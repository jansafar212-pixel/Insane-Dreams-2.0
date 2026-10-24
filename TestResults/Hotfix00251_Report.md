# AKUMU 2 — Hotfix 0025.1

2026-10-23. **Implemented by Codex — awaiting playtest verification.**

## Root causes and implementation

**Enemy mobility.** `LarperBrain.Pursue` applied wall and missing-floor stops during a jump, removing horizontal drive while the actor was still crossing the obstacle. Grounded pursuit now preserves airborne drive through the physical hop. Low-box jumps reuse existing movement capabilities, landing/clearance checks and motor impulses, with a conservative 16-sample arc check before launch and a cooldown. Impossible walls/ceilings remain blocked. No actor position is assigned or teleported.

Continuous requested movement previously had no progress-based route recovery. Grounded movement now requests reconsideration after .9 seconds without .12 units of progress. Ordinary combat releases its destination, avoids the failed destination for three seconds and selects another reachable/reservable position; failed withdrawals terminate through the existing reload/recovery path. Samurai invalidates a blocked approach edge and uses a short, geometry/spacing-checked .35-second backoff before reconsidering its existing platform route. Attack commitment and active flights are protected from this recovery. Existing local route caches, failed-edge tracking and jump validation remain; no new navigation framework.

Combat reposition opportunities were only every 8–13 seconds with a 20% roll, making unpressured changes uncommon. The Larper prefab now uses 4–7 seconds and 35%, retaining existing confidence/cowardice direction choices, extra pressure weighting, cover/reload/suppression priorities and reservations. Shortcomings are addressed with more opportunities, not a fixed behavior script. Spacing/path checks now account for current vertical separation as well as remembered floor height, so an actor well overhead does not unnecessarily block a ground corridor. Landing destination reservations remain intact.

**Android pause.** `.campaign-pause` explicitly used `left:43%; bottom:12px`. It now anchors at 50% of the existing safe-area parent, translates by half its own width and sits 12 pixels below the safe top. Target minimum dimensions are 124×52; existing colors/font/action and pause/resume are preserved. Only in campaign gameplay, the mission card reserves space to the left of the top-center button so text does not overlap it. Equipment and mobile joystick/action positions were not changed.

**Carbine reload.** The magazine was painted into the equipped gun sprite; the carbine had no separate assigned magazine. Generic hand/dip animation could not remove those pixels. The new native receiver and magazine are lossless separations of the existing CrookedCarbine pixels. The original source is retained; no new palette, scale, proportions, rig, grip or muzzle convention. The magazine is a distinct component, with a prewarmed replacement component and the existing support arm following the handling path.

Reload phases use the existing normalized reload clock: reach during 0–18%, remove/move the old magazine during 18–46%, put it aside by 50%, reach to a different replacement position during 50–60%, bring/insert during 62–88%, then latch/regrip/return during 88–100%. The gap and different replacement path make the exchange readable. At the original reload deadline, the existing `WeaponCycle.Tick` remains the sole ammo-transfer authority and emits the carbine insertion/ready marker exactly once. Visual completion observes that marker. Cancellation transfers nothing and the next normal pose restores the magazine; switching weapons clears the replacement component. No animation callback independently creates ammunition.

## Parameters

| Setting | Before | After |
| --- | --- | --- |
| Larper reposition interval | 8–13 s | 4–7 s |
| Larper reposition roll | 20% | 35% |
| Requested-movement stuck threshold | absent | .9 s / .12 world units |
| Ground obstacle jump cooldown | no explicit obstacle cooldown | 1.2 s |
| Failed combat destination exclusion | absent | 3 s |
| Samurai blocked-approach backoff | no progress-triggered recovery | .35 s, safe direction only |
| Pause anchor | 43% left / bottom 12 px | 50% / top 12 px within safe area |
| Carbine magazine component | none | 4×5 pixels, 32 PPU, socket (9/32, 0) |

New Inspector tuning: `LarperBrain.stuckSeconds`, `obstacleJumpCooldown`, and `WeaponDefinition.physicalMagazineReload`. Existing reposition fields remain configurable. Carbine damage **20**, firing interval **.2 s**, capacity **24**, reload **1.5 s**, range **22** are unchanged. Pistols and other weapons do not enable the physical-carbine flag.

## Files modified

- `Assets/Scripts/Enemies/LarperBrain.cs`, `EnemyCombat.cs`, `SamuraiCombat.cs`, `CombatPositionReservations.cs`.
- `Assets/Scripts/Combat/WeaponCycle.cs`, `Assets/Scripts/Inventory/WeaponDefinition.cs`.
- `Assets/Scripts/Visuals/CutoutRig.cs`, `CharacterVisualDriver.cs`.
- `Assets/Scripts/Progression/CampaignMissionBridge.cs`, `Assets/UI/GameFlow.uss`.
- `Assets/Prefabs/Enemies/Larper.prefab`, `Assets/Data/Items/WarehouseCarbine.asset`, new `Assets/Art/Hotfix00251/` receiver/magazine Sprite and Texture assets with Unity metadata.
- New native authoring helper `Assets/Editor/Hotfix00251Setup.cs`.
- New `Assets/Tests/PlayMode/Hotfix00251Tests.cs`; added safe-area/pause case in `Sprint024FlowTests.cs`.
- `ART_BIBLE.md`, `DEVELOPMENT_LOG.md`, this report, raw `Hotfix00251.json`, controlled `Hotfix00251_Reload.png` and `Hotfix00251_Gameplay.png`, reproducible `Hotfix00251ReloadPreview.cs`, CLI version evidence `Hotfix00251_CliVersion.txt` under `TestResults/`.

Prefab/data changes used Unity Editor APIs. Preexisting dirty work and historical art were preserved. No stage, commit or push.

## Verified by tests

**78 Edit Mode + 107 unique selected Play Mode tests passed, 185 total, no remaining failures in these tests.** Repeated tests count once using their latest result. The final falling-status correction compiled successfully and all six focused checks were rerun successfully afterward.

Focused checks physically cross a low box; reject a ceiling; exercise bounded stuck events, impossible-wall safety and resumed movement when obstruction is removed; verify old/new magazine positions and interruption reset; check atomic ammo transfer, cancellation and a single insertion event; validate the top pause target at 1280×720, 1920×1080 and 2340×1080 with asymmetric simulated safe insets, no mission/equipment overlap and functional pause/resume.

Selected regressions: CameraSamuraiHotfix, Sprint0244 platform/failed-edge/double-jump/melee-rig cases, Sprint0223Opening, Sprint020 crowd/morale/cascades, Sprint0246 weapons, Sprint0248 effects, Sprint0025 melee/revive/suppression, Sprint0225 mobile, Sprint022Cover, Sprint012Combat, LadderCameraHotfix, Sprint024Flow, Sprint023 healing/supplies and Sprint0223Motion. All Edit Mode tests passed. This is targeted coverage, not every historical Play Mode fixture.

Initial focused failures came from a test actor missing the archetype configuration normally supplied by its spawn point; that harness was corrected. Raw initial and final runs are retained. A tool permission-review timeout and a temporary domain-reload connection loss were retried successfully; neither was a Unity source compile failure.

Final compilation succeeds. Final Console ground truth has zero errors/warnings; retained history includes existing obsolete-API warnings. Android **build-configuration dry run** is valid with no validation errors. **No APK was built and no Android-device gameplay/profiling was performed.** Unity CLI update was explicitly approved, applied outside the restricted shell and verified as **1.0.0-beta.14**.

## Controlled visual evidence and remaining verification

Actual historical `Postava 1.png` and the graphics bible were inspected before the visual work. The two-facing storyboard `Hotfix00251_Reload.png` renders existing player modules at eight reload phases; it is a controlled render, not artistic acceptance. The live warehouse capture shows the separated magazine mid-removal and the unobstructed top pause button. Runtime advanced frame 17126 → 18766 / time 49.9249 → 50.4685 before a deliberate capture freeze at reload progress .3624; ammo remained 3 / reserve 72. Enemies were disabled in memory for that capture. A temporary progress store was used and removed; personal campaign saves were not used.

Manual PC and Android playtesting should verify:

1. Larpers fight and reposition around multiple crates without long jams, oscillation or repeated jumps; validate cover-to-cover movement under sustained suppression and mixed-personality crowds.
2. Samurai chase across actual warehouse routes, especially previously problematic undersides/edges and dynamic blockers, without snapping, impossible loops or lost melee telegraphs. Controlled route tests cannot prove every level arrangement.
3. Pause is comfortably reachable at top center with real cutouts/aspect ratios; real multitouch controls and HUD readability remain comfortable.
4. Carbine removal/replacement is readable at normal gameplay zoom during running, crouching, cover and varied aim directions. Interrupt via weapon switch, pause, damage/death and input reset; confirm no duplication or stranded magazine.
5. Confirm Android frame rate under crowded combat. No device performance claim is made; new progress checks are arithmetic, arc queries are bounded/cooldown-gated and existing buffers/reservations are reused.

Editor handoff: MainMenu open, clean, Play Mode stopped, original MainMenu startup scene restored, background-running off. **Implemented by Codex / Verified by tests / Pending playtest**; gameplay completion awaits human acceptance.
