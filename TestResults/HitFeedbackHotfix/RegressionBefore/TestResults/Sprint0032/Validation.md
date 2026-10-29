# Sprint 0032 validation

New controlled suite:10/10passed. Existing physical Samurai6/6; perception/combat4/4;
legacy navigation2/2; Level1combat5/5; concrete bunker4/4;0030vest/retreat4/4;
FirstOperation6/6 (including continuous scripted real movement to results);
legacy Sprint00511/11; spacing5/5; witnessed morale6/6; near-miss suppression6/6;
0031UI3/3. Remaining final checks are added below.

Initial fixture issues were retained as evidence, corrected and rerun:
- New bare-Larper fixture lacked spawn-time Configure(archetype).
- Long route initially hid the initial target behind an intermediate deck. Correct
  no-omniscience behavior was retained; test target now visibly progresses decks.
- Isolated beam test needed to honor the new viewport grace before inspecting it.
- Frozen-body suppression-only test was accidentally invoking real stuck detection;
  disabled navigation in that one fixture, retaining suppression assertions. Real
  blocked movement remains explicitly tested with physics in the new suite.
No production bug was hidden by removing coverage or changing combat balance.

Legacy timing assertions changed explicitly for requested behavior: brief Samurai
recognition before pursuit, Larper.7s recognition and4.5s visual memory. All original
LOS, cadence, damage, ammo, warning, navigation and state assertions were retained.

Editor PlayMode fixtures advance actual frames and physics. Multi-platform test
requires an intermediate landing, final landing, at least two jumps and <1u movement
per fixed step. Existing small-collider/real-crate/removed-obstacle routes and the
actual warehouse pursuit route also pass. This establishes representative routes,
not an exhaustive navigation proof or human assessment of combat feel.

Sixteen simultaneous listeners received a burst of20noise events: each accepted
one observation at the configured hearing interval and no listener rebroadcast it.
No extra event manager or per-frame physics search was added. Source review confirms
scalar recovery state/fixed buffers and no new runtime per-frame string/path-list
allocation. This is not Android hardware profiling or a measured FPS claim.

ProtectedAfter.json matches all captured scene, UI, input, player, throwables,
Officer warning and fireworks mechanics files. PrefabTuningDiff.txt contains only
intended AI tuning fields. Weapon damage/cadence, existing suppression and witnessed
fear probabilities, character art/hitboxes and level geometry remain unchanged.

Manual/device limitations: no physical Android run, no new APK, no human gameplay
or artistic review. Existing0031APK predates this code. Rebuild before playtesting
0032on a device. Report.md includes a concrete manual checklist and local-graph limits.

Historical Sprint0025 manual/free-revival and null melee-fixture failures were
previously documented under0031. They are unrelated to this AI sprint and were
not rerun or altered here. The selected-suite pass count is not a claim that every
historical project test has been repaired.

Final selected regression result:92/92passed, no skips (78PlayMode,14EditMode).
Compilation completed successfully. Native scene/prefab audit: missing scripts0,
prefab missing scripts0, Warehouse_Demo saved/clean/open, PlayMode stopped,
pillbox5solid faces, Officer50HP/50protection and vest resources intact.
Current Unity Console:0errors/0warnings, not compiling. All protected hashes match.
Resolved tuning/attack permissions saved in FinalTuning.json; current roster does
not allow offscreen firing. Git: existing working tree preserved,0staged; no
stage/commit/push. No Android APK or physical-device run performed for0032.
