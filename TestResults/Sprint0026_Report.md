# AKUMU 2 — Sprint 0026

2026-10-24. **Implemented by Codex — awaiting human playtest and Android-device verification.**

## Mission completion first

Before editing, Hotfix00253Tests passed4/4, including physical checkpoint contact, correct locked-objective feedback, records, both checkpoint commits, exit, Results, saved rewards and return to level selection. The scenario includes a real Larper escape that remains alive and tracked before encounter defeat. This controlled pipeline was rerun after changes. Mission/checkpoint/progression scripts were not rewritten; personal saves were not used. It is not an unassisted full playthrough.

## Causes and implementation

- Suppression detection/gain already worked. FULL_PANIC stops movement, whereas Hotfix0025.4 introduced real withdrawal. Sprint0026 increases opportunities to commit that withdrawal, keeping its90% base roll and two-success quota rather than raising pressure again.
- Cover was sparse, and selection checked only one approximate crouch ray. A.25-unit face gap plus normal braking drift could leave the crouched head exposed on arrival. Selection now checks actual floor-relative crouched centre and head, uses a.08 face gap, and leaves broad obstacles to existing reachability validation instead of rejecting every width>1.8 outright. This does not authorize hopping broad/impossible geometry.
- Protected arrival previously held only1.2–2.4s and reload completion discarded cover immediately. Basic Larpers now hide/crouch2–3.5s, extend while pressure remains, and always leave the hold by a6s deadline. Reloading at useful cover retains it through recovery. Sampled LOS/ground checks abandon destroyed or flanked protection; a visible player's confirmed position or credible new incoming-fire origin updates the threat, without hidden tracking.
- Hiding cannot consume another major escape. On expiry, a short investigation of existing uncertain sound memory allows combat to resume instead of forgetting solely because hiding took time. Existing combat positioning/aim/LOS handles subsequent exposure and firing; no automatic shots through solid platforms or cover.
- A forced regression test confirmed that weapon malfunction stopped the motor during withdrawal. Jammed weapons now continue retreating while the existing weapon cycle still blocks shooting. No malfunction probability or reload choreography changed.

Major route selection retains existing reservations, safe floor/corridor checks, low-box hops, finite timeout, failed-goal cache, endpoint history and encounter leash. Failures spend no successful escape; only explicit encounter reset clears the counter. Local suppression/after-quota movement remains≤1.5units, may crouch at65% ordinary pursuit input when no hop is needed, and cannot masquerade as unlimited major escapes.

Occasional withdrawal fire uses a20% opportunity every1.1–1.6s after initial.7–1.2s, only while grounded, actually moving and visually seeing the target. It reuses ammo, imperfect aim, camera permission and immediate solid-geometry checks. Reservations keep destinations separate. Existing independent firing/reload clocks supply imperfect supporting fire; no squad scheduler or fixed sequence was added.

An actual suppressed major withdrawal can notify existing awareness subscribers once movement begins. Eligible basic Larper witnesses within6units, view cone and LOS have30% chance of2s fear influence, with an8s witness cooldown. Existing major morale influence is not overwritten, defeat cooldown/probabilities are unchanged, and witnessing does not itself publish another withdrawal or invoke FULL_PANIC/collapse. Existing defeat and rare cascade behavior remains.

## Before / after tuning

| Parameter | Before | Sprint0026 |
|---|---:|---:|
| Basic major suppression threshold |70|55|
| Sustained burst minimum misses |3|2|
| Sustained duration |.45s|.30s|
| Major decision cooldown before personality |4–6s|2–3s|
| Base major roll |90%|90%, unchanged|
| Neutral personality rolls |98% coward /90% ordinary /67.5% confident|unchanged|
| Successful major escape quota |2|2, unchanged|
| Planned distance / timeout / anchor leash |6units /3.5s /12units|unchanged|
| Ordinary / exhausted local move limit |1.5units|unchanged|
| Cover face clearance |.25units|.08units, outside collider|
| Protected hide |1.2–2.4s|2–3.5s, pressure extension capped6s|
| Per-NPC meaningful combat callout gap |3.5s|2.8s on basic Larper only|
| Maximum simultaneous bubbles |2|2, unchanged|
| Near-miss gain / decay |63.8 /12per second|unchanged|
| Rare full panic / ally fear |8% /60%|unchanged|

Basic decision cooldown after susceptibility is1.6–2.4s cowardly,2–3s ordinary,2.67–4s confident. Gameplay values remain Inspector-configurable. No new per-frame scans, global searches or allocations: bounded cover sampling uses existing buffers; witness checks occur only on actual movement events. Added head validation costs a second LOS ray per bounded candidate/cover sample, not per-frame searches. Android performance still needs measurement on hardware.

## Environmental cover

Warehouse_Demo, the menu-launched playtest level, gains **3 crates:10→13 freestanding cover objects (+30%)**. Platforms/boundaries/hitboxes are excluded from this count. Each reuses the existing ShippingCrate sprite,1×1 solid box, matching tiled renderer and wood impact surface; no new artwork. Authoring uses Unity Editor APIs and is idempotent.

| Object under Sprint0026_Cover | Centre(x,y) | Purpose |
|---|---|---|
| TacticalCrate_0 |(3.5,.5)|Intermediate ground-floor storage withdrawal position before the east gap|
| TacticalCrate_1 |(-5.5,5.9)|West mezzanine cover between the ladder/upper storage fighting positions|
| TacticalCrate_2 |(13.3,12.5)|Processing roof approach/retreat cover before existing eastern crates|

Automated checks confirm ground support, physical bullet blocking, no objective-trigger or spawn-point overlap, and a reusable existing sprite. Existing ladders, platform gaps, checkpoint access and encounter counts were not relocated. Warehouse_Testbed and Warehouse_Mission01 were not changed. Human traversal/crossfire/readability acceptance remains pending.

## Secondary agenda status

| Item | Status and verification |
|---|---|
| Navigation / Samurai | Existing low-box physical hops, stuck recovery and Samurai pursuit preserved; relevant existing tests pass. No new cross-floor retreat planner: automatic ladder selection/gap routing for a retreat destination remains pending. Existing authored ladder routes retain ownership. |
| Joystick walk/run | Already proportional through existing motor; controlled25% input verifies25% normal speed. No new sprint tier. |
| Double jump / slide | Existing two-jump budget retained. Per user's clarification, pulling down or diagonally down **in midair after the second jump** queues one14units/s downward dive. It uses normal gravity/collision/motor ownership, consumes no additional jump, and reuses the existing crouch visual without changing the airborne physical hitbox. Sideways/down input can continue into a.35s crouched landing slide capped at normal run speed; release/reversal/reset cancels the tail. Straight down needs no sideways input. No new animation assets. |
| Android controls | Aim pad right inset28→44px (16px left); configured diameter, pointer processing and sensitivity unchanged. Pause already top-centred inside safe area; existing pause/layout tests pass. Blind fire remains cover-only. Device ergonomics/safe-area acceptance pending. |
| Camera | Existing smooth directional framing and ladder centring unchanged; camera/motion regression checks pass. |
| Melee / mild player suppression | Already implemented; local melee hit/no-firearm-cue and presentation-only player pressure checks pass. No new disruption. |
| Duct tape / Faintanyl | Existing hold healing preserved. Faintanyl death button,2-injection cap and2s protection already work in automated tests, so no replacement added. |
| Fireworks / reloads | Approved effects/frequency and all reload implementations preserved. Relevant reload/VFX regressions pass; intentionally silly carbine choreography unchanged. |
| Officer | No Officer asset/component is present. Explicit basic-Larper opt-in preserves distinct archetypes; no Officer implementation invented or actual Officer runtime test claimed. |
| Art | Existing flat crate art and existing crouch pose reused. No character sprite/rig/pivot/weapon attachment redesign. |

## Exact files modified by this sprint

- Assets/Scripts/Enemies/EnemyCombat.cs
- Assets/Scripts/Enemies/LarperAwareness.cs
- Assets/Scripts/Input/PlayerActions.cs
- Assets/Scripts/Prototype/PlayerMotor2D.cs — queued dive only; ordinary acceleration, jumps and gravity unchanged.
- Assets/Scripts/Visuals/CharacterVisualDriver.cs — select existing crouch pose during the requested midair slide only.
- Assets/UI/WarehouseControls.uss — aim inset only.
- Assets/Prefabs/Enemies/Larper.prefab — native tuning.
- Assets/Scenes/Warehouse_Demo.unity — three native-authored crates.
- Assets/Editor/Sprint0026Setup.cs (+Unity-generated meta).
- Assets/Tests/PlayMode/Sprint0026Tests.cs (+Unity-generated meta) —11 focused cases.
- Assets/Tests/PlayMode/Sprint0224Tests.cs — isolate duck-retry cooldown from the newly earlier retreat opportunity; coverage retained.
- Assets/Tests/PlayMode/Hotfix00252Tests.cs — ensure blocked-major fixture leaves a valid≥1unit local step, keep movement/ammo/no-major-charge assertions.
- Assets/Tests/PlayMode/Hotfix00253Tests.cs and Hotfix00254Tests.cs — additional failure diagnostics only; original assertions retained.
- DEVELOPMENT_LOG.md and TestResults/Sprint0026* evidence.

Existing dirty work is preserved. No staging, commits or pushes.

## Automated results and limitations

**197 unique checks passed:119 selected Play Mode tests and all78 Edit Mode tests.** Latest results are deduplicated by FullName, not summed across reruns. All11 new focused cases and the complete affected0025.2/0025.3/0025.4 suites pass. Existing suppression/morale/cascade, melee/revival, cover, analog/movement/double-jump, rendered-motion, Samurai/camera, reloads, mission/flow/save and pause checks pass. Source compiles; final Console ground truth0errors/0warnings. Android build-configuration dry-run valid; no APK build or device test.

Native prefab checks show0missing scripts on Larper, Samurai, fireworks specialist and player. Native values confirm threshold55, gain63.8, misses2/.3s, cap2, cooldown2–3 and hide2–3.5/max6. Demo crates are saved at the reported positions with existing sprites. MainMenu remains clean/open/stopped, startup MainMenu preserved, background execution off. Protected manifest:12files byte-identical;2intentional secondary changes (motor dive and visual pose flag);0unexpected changes. Scoped git diff --check passes with line-ending notices only. Raw history and latest unique results: Sprint0026.json; hash manifests: Sprint0026_ProtectedBefore.json / Sprint0026_ProtectedAfter.json.

All checks are controlled Unity tests, not human gameplay approval. The new tests cover earlier trajectory-derived withdrawal; actual cover hops/arrival; crouched concealment and finite recovery; firing after protection is removed; visible flanking; insufficient head cover; occupied-route postponement; crate support/clearance/occlusion; analog movement; diagonal and straight-down midair slide; single-jump/reset exclusion; UI layout; short witnessed-retreat influence and wall/cascade isolation; and gun-jam movement.

Initial failures and fixes are preserved in raw evidence, including one transient host FMOD output-device initialization error. Isolated reruns alone are not treated as acceptance: final affected whole suites are rerun. Existing complete mission validation controls combat and intermediate placement; it verifies gates/save/results, not the feel or traversability of an unassisted full run.

## Manual playtest

1. Start Warehouse from MainMenu. Use real short overhead bursts on ordinary/cowardly/confident Larpers; compare withdrawal frequency, not just ducking. Check two completed major escapes maximum, meaningful failures free, and later local defense/return fire.
2. Observe cover arrival on ground/mezzanine/roof, staying concealed under fire, recovery and valid-angle re-engagement. Flank the crate; the NPC should release unsafe cover. Check crowded cover/reservations and failed routes; no infinite hops, clipping or permanent hide.
3. Complete all encounters after retreats; physically activate both checkpoints, obtain records, reach exit/Results and return to selection. Confirm statistics and supplies persist.
4. Double jump, then pull joystick down or down-sideways (PC:S or S+A/D). Check a readable controlled midair dive and short crouched landing tail, no third jump, and safe behavior near boxes, cover, ladders and platform edges. Confirm normal movement still feels responsive.
5. Check rare cascades remain rare, Samurai aggression, Faintanyl protection, held healing, approved fireworks and unchanged reloads. Read event dialogue with at most2bubbles.
6. On Android hardware check frame rate, safe-area pause, aim-pad comfort/overlaps at all joystick scales, touch jump/dive, cover/blind fire and battlefield readability.
