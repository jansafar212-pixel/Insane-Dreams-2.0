# AKUMU 2 — Hotfix 0025.4: Suppression-driven panic escapes

**Implemented by Codex — awaiting user playtest verification.**
Started 2026-10-23; recovery finalized 2026-10-24.

The escape implementation, prefab opt-in, dialogue and regression coverage were already present when the latest resume brief arrived. Recovery continued the running final test job, inspected Git/current Editor state, and finalized evidence and documentation without restarting or adding gameplay features. Recent HEAD remains 1b24b2a; unrelated pre-existing dirty work is preserved.

## Implementation and diagnosis

Suppression already registered credible trajectories and accumulated pressure. However, full panic used the existing Panicked state, which stops movement. Actual running was a different, repeatable withdrawal branch without a per-NPC success budget, and protective cover could keep an actor ducking indefinitely.

The new frequent individual panic escape reuses Withdrawing, existing motor movement, cover search, reservations, safe low hops and route/stuck checks. It does not increase the rare group morale-collapse probability or publish new ally/cascade events. No new AI state machine, manager, player movement, reload or rendering changes.

Only the explicitly assigned basic Larper EnemyArchetype is eligible. Samurai and fireworks components are also excluded. No Officer script/prefab/archetype is present in the current project's enemy assets; a controlled distinct-archetype test verifies that a future/different Officer retains the existing withdrawal path without inheriting the Larper quota. No Officer implementation was invented.

## Exact probabilities and bounds

| Parameter | Before | After |
|---|---|---|
| Frequent major moving panic escape | no distinct decision |90% base at pressure≥70, ≥3 misses over≥.45s |
| Personality-adjusted major roll | absent | cowardly98%, ordinary90%, overconfident67.5% before temporary morale |
| Temporary morale influence | existing suppression weights | same fear×1.15/confidence×.9 also influences major roll; final cap98% |
| Major decision cooldown | absent |4–6s / existing susceptibility (cowardly3.2–4.8s, ordinary4–6s, confident5.33–8s) |
| Successful major escapes per NPC | unlimited repeated withdrawals | configurable maximum2 per explicit encounter reset |
| Successful escape requirement | no counter | reach reserved endpoint, grounded at planned elevation (±.25), at least2 units moved |
| Major travel limit | withdrawal6 / cover search7 units | maximum planned destination distance6 units; existing motor inertia unchanged |
| Major duration limit |4.5s withdrawal |3.5s maximum |
| Encounter-area leash | none |12 units from spawn/reset anchor |
| Recent endpoint reuse | no panic-specific history | reject endpoints within1.25 units of either of the last two successful destinations |
| Minor suppression movement / post-cap withdrawal | could initiate repeated long retreats |≤1.5 units |
| Post-success defensive pause | existing cover/reload recovery | additional bounded .8s hesitation/duck for successful escape; existing useful-cover hold preserved |

A failed random roll consumes only its decision cooldown. Failed-to-start, blocked, interrupted or timed-out incomplete escapes do not spend a successful-escape charge. The count is not reset on camera changes, target rebinding or component disable/enable. ResetEncounter clears it when mission/spawn code explicitly rebuilds a fresh encounter or checkpoint retry.

Major choices can abandon occupied cover and interrupt a reload only after a valid movement plan is selected. The path must stay away from incoming-fire origin and the existing remembered player position, remain on reachable terrain within the leash, and respect position/cover reservations. It never reads a hidden player's current position for the new route filter. Impossible escapes retain ordinary fear/hesitation instead of teleporting, despawning or charging quota.

Major escape searches prefer farther valid cover; ordinary cover preferences remain unchanged. Failed destinations reuse the existing blocked-position cache for6s to discourage repeated failed goals.

After two successes, suppression gain/decay, accuracy penalties, ducking, flinching, cover holds, brief hesitation and local movement remain. Low-ammo withdrawal is also kept local after quota exhaustion to avoid a new long chase through that path; ammo/reload timing is unchanged. Enemies remain targetable, tracked by their original spawn/mission entries and capable of re-engaging. Normal run speed and animations are reused.

**Unchanged:** near-miss gain63.8, thresholds14/42/max100, decay12/s, quiet gap.8s, optional ordinary retreat70%/4s, existing rare full-panic8%/long cooldown and group collapse rules. Ordinary high-pressure/sustained decisions now select bounded local movement on basic Larpers unless the major escape roll commits. Other archetypes retain their previous decisions.

## Contextual presentation

Movement itself interrupts firing and visibly abandons position. Existing WithdrawalStarted notification still fires only after actual .25-unit movement, using the established scared reaction and speech channel. Four lines supplement the existing withdraw arrays on Ordinary/Cowardly/Overconfident: TACTICAL RETREAT!, I'M TOO RICH TO DIE!, COVER ME, IDIOTS!, THIS IS NOT LIKE WARZONE!. Existing two-bubble hard cap, relevance filtering, cooldowns and readability timing are unchanged.

No new per-frame scans, global searches, allocations or physics queries. Candidate filters run only inside the existing bounded commitment search. Active withdrawal adds a few counter/end-point checks; existing stuck/obstacle checks terminate failed motion. Ladders are not newly planned by this feature: active authored ladder traversal is protected from interruption, and escape routes reuse the existing safe ground/low-hop capability.

## Exact files changed

- Assets/Scripts/Enemies/EnemyCombat.cs — archetype opt-in, decision, counter, bounded route filters and recovery.
- Assets/Editor/Hotfix00254Setup.cs (+ generated meta) — native prefab/reference and dialogue authoring.
- Assets/Prefabs/Enemies/Larper.prefab — explicit basic archetype reference and Inspector defaults.
- Assets/Data/Enemies/Personalities/Ordinary.asset
- Assets/Data/Enemies/Personalities/Cowardly.asset
- Assets/Data/Enemies/Personalities/Overconfident.asset — withdrawal dialogue additions only.
- Assets/Tests/PlayMode/Hotfix00254Tests.cs (+ generated meta) — six focused tests.
- Assets/Tests/PlayMode/Hotfix00253Tests.cs — actual successful Demo Larper escape added before mission defeat/completion checks.
- Assets/Tests/PlayMode/Sprint0202Tests.cs — distance/personality formula assertions now read configured gain instead of stale pre-0025.3 hard-coded58; current63.8 remains asserted in focused coverage.
- Assets/Tests/PlayMode/Sprint012CombatTests.cs — legacy floating-magazine assertion now checks current physical extraction, hand/gun grips and atomic ammo/death behavior; reload implementation unchanged.
- Assets/Tests/EditMode/Sprint0245RigTests.cs — compare Unity native sprite identity rather than managed wrapper identity; the same native sprite can have different C# wrappers.
- DEVELOPMENT_LOG.md and TestResults/Hotfix00254* evidence/report.

Existing dirty work is preserved. Player/Larper reload source, carbine, fireworks, mission scripts/scenes and player/specialist prefabs are protected by hotfix-start/final hash checks.

## Tests and verification

Final latest results: **177 unique checks passed —99 selected Play Mode tests and all78 Edit Mode tests**. Includes6 new focused cases. Reruns are deduplicated by FullName; initial failures and corrected assertions are retained in TestResults/Hotfix00254.json. Existing suites cover suppression, morale/cascades, combat/reloads, mission/progression, Samurai/camera, movement/double jump and traversal.

Compilation completed without errors; post-test Console ground truth0errors/0warnings; final Console has1Pipeline inspection-timeout error,0warnings. Android configuration dry-run valid; no APK build/device run. MainMenu remains clean/open, Play Mode stopped, startup MainMenu and background execution off. Native Larper prefab confirms explicit Larper archetype, cap2, chance.9, threshold70 and unchanged gain63.8; Larper/Samurai/fireworks prefab scans show0missing scripts. A broad serialized-reference eval timed out after30000ms; smaller native checks succeeded, so no claim is made that a complete reference audit finished. All13 protected-file hash comparisons match. Scoped git diff --check passed (line-ending warnings only). No stage/commit/push.

Focused tests cover real overhead hitscan producing actual escape movement, no charge at start, successful completion, suppression recovery and eventual shooting; first/second success and third denial; normal ducking/local movement after cap; count persistence across disable/camera rebinding and explicit reset; failed and interrupted obstacles without charging; physical low-box hop without snapping and no repeated endpoint; personality/morale probabilities; distinct-archetype exclusion. Random seeds or direct decision inputs are controlled where needed.

The Demo pipeline test lets a real Larper complete a physical retreat while remaining alive/active and counted in its encounter, then defeats all required targets and completes checkpoints → records → exit → Results → saved rewards → Level Selection. Intermediate placement/combat setup is controlled, so this is not an unassisted full gameplay playthrough.

## Manual acceptance still required

1. Suppress a basic Larper with sustained close real shots. Observe an actual bounded run, brief hiding/recovery and varied personality timing. Short isolated fire should still produce ordinary defense.
2. Trigger two completed escapes on the same NPC; sustained fire afterward must not cause a third long retreat. Ducking, local movement, reloads and eventual return fire should remain.
3. Block an escape path during movement and retry later. A failed/incomplete route should not spend the charge; no snapping, repeated impossible hops, walking into the shooter or unreachable destination.
4. Check different warehouse floors, nearby allies/crowded cover and camera exit/entry. Complete the mission after fleeing enemies are defeated; no target should disappear from the objective.
5. Confirm rare morale-collapse waves remain exceptional, Samurai remains aggressive, specialist keeps the3s recovery/new fireworks, and all accepted reloads/camera/ladder/player controls remain intact.
6. Verify Android frame rate, touch play and combat feel on hardware. No device profiling or human gameplay acceptance is claimed.

