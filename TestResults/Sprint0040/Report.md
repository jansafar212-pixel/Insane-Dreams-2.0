# Sprint 0040 — loadout correction and living impacts

Implemented in the existing dirty Sprint 0039 checkout; no commits, staging, reset or push. Sprint 0040 supersedes the incorrect three-firearm design. Earlier sprint source and evidence are preserved.

## Actual loadout correction

The fault was architectural: Sprint 0039 introduced Firearm3, a serialized third RuntimeWeapon, secondary2 in GameProgress, three-gun packing, switching and shop/HUD references. These have been removed, rather than merely hidden.

EquipmentSlot now contains Firearm1 (primary), Firearm2 (secondary), and the existing Melee/throwable tool slot. Mission arrays of length three mean TWO firearms plus that existing tool, never three firearms. Invalid numeric slot 3 is rejected. Runtime getters reject wrongly classified serialized weapons as well. Shop labels are PRIMARY WEAPON and SECONDARY WEAPON; the HUD has MAIN, SIDE and independently selectable grenades. Existing touch target sizes remain usable. No weapon classification was changed.

Legacy secondary2 is parsed only through a private read-only migration envelope in ProgressStore. It is absent from the current save model. Migration examines primary, secondary, then the legacy extra, retaining the first valid owned weapon of each role. Thus the intended primary and normal secondary win; the extra is a fallback only when the normal slot is missing or invalid. All other weapons remain owned but unequipped, with their ammunition untouched. The next successful save omits secondary2. Revision 40 and repeated-load tests verify deterministic, idempotent migration. Existing ammunition normalization transfers any old over-capacity magazine rounds to reserve without discarding them. Currency, grenade stock, armor and progression are preserved; the previously authorized release playtest wallet minimum remains in effect.

## Why living hits were too subtle

The existing six-joint hybrid rig applied both angular damping 5 and a heavily damped return spring (18, damping multiplier 1.5), then capped visible torso lean at 12 degrees. Light hits settled before producing readable motion; strong weapons converged on the same visual clamp. This was a pose/physics response problem, not insufficient HP damage. No replacement ragdoll, physical bullets, particles, character redraw or gameplay-root force was introduced.

Enemy-only serialized profile values:

| Parameter | Old | New |
|---|---:|---:|
| Spring return | 18 | 9 |
| Impulse degrees | 115 | 160 |
| Angular damping | 5 | 2 |
| Spring damping multiplier | 1.5 | 1 |
| Angular speed clamp | 320 | 460 degrees/s |
| Joint angle clamp | 26 | 34 degrees |
| Rendered torso clamp | 12 | 26 degrees |
| Return torque cap | .25 | 1.5 |
| Spring response lifetime | .8 | 1.1 s |
| Torso spill, normal/heavy | .25/.55 | .45/.7 |

Actual incoming direction and impact position now produce region-specific torque, including diagonal trajectories and both facings. Existing six cached bodies stay collider-free, return to their anchors, and sleep when finished. Shotgun pellets accumulate into one bounded physical batch. Reload/cover poses retain their constrained arms while allowing .45 torso blend instead of .2. Player reaction configuration remains unchanged.

Normal Android camera projection was derived from the native camera configuration: orthographic size 4.329004 at 1280x720. Controlled before/after replays use this projection and surviving test enemies, with particle feedback disabled to measure body movement alone. These are automated captures, not Android-device gameplay approval.

| Weapon | Before peak head displacement | After |
|---|---:|---:|
| Glock | 1.70 px | 4.65 px |
| Thompson/SMG | .62 px | 2.86 px |
| Assault rifle | 4.82 px | 7.87 px |
| LMG | 7.03 px | 9.65 px |
| Desert Eagle | 11.63 px | 18.52 px |
| Close shotgun, connected pellets | 11.63 px | 20.45 px |
| Sniper | 11.63 px | 20.45 px |

Glock has a short upper-body flinch; automatic SMG hits are smaller and accumulate with diminishing effects. Rifle/LMG single impacts are progressively stronger. Desert Eagle produces pronounced recoil and existing balance-dependent stagger/fall. Shotgun pellets are aggregated; sniper has a large single impulse. See ReadabilityLatest.json and full-frame Before_/After_ PNGs. Equal saturated shotgun/sniper peaks are bounded intentionally, while their cadence/pellet behavior differs.

Anti-lock protections are preserved: batch strength cap 3.2; rapid-load cap 4, decay 3/s, diminishing factor 1/(1+rapidLoad*.25); major reaction cooldown .55 s; fall cooldown 3 s; recovery grace 1.2 s; balance decay 1.15/s. Larper/Samurai/Officer susceptibility remains 1/.85/.8, fall balance threshold 4.5/6/7, recovery .65/.5/.55 s. Lightweight flinches remain available while major reactions are prohibited. Existing fall probability, AI suspension/release, hitbox and limb-damage rules are unchanged. No permanent disable was added.

## Validation

116 unique selected Edit/Play Mode cases: **113 passed, 3 failed**. FinalTestResults.json records each case and the latest result. Zero-match exploratory filters are excluded. The final targeted run also passed the directional test with a normal movable gameplay Rigidbody2D, rather than a frozen root. Relevant coverage includes migration/save round trips, ownership/currency/ammo, no hidden third slot, shop/equipment flows, two-gun mission packing and pause/resume, reloads, all weapons, living falls/recovery, body regions/facing/diagonals, cover controls, grenades, parachute/fall damage and cached HUD/pose checks. These are selected regression tests, not a claim that the entire project suite is green.

The three failures are CowardsBunkerTests: front-shell coverage, rear-flank confirmation and roof-height/grenade trajectory. A native Editor preview of the exact committed HEAD scene and current scene found identical relevant colliders (BunkerBaselineGeometry.json). Tests expect the earlier taller shell/roof and door opening; the committed roof is at y=4.9175, rather than the test's 7.8 assumption, and rear clearance is about .6133. Geometry and AI were not redesigned to satisfy these unrelated expectations. Their resolution remains open; a complete pre-sprint baseline suite was not rerun, so the evidence is the unchanged native geometry comparison, not a claimed before/after full-suite reproduction.

WeaponBalanceBefore/After.json match exactly for all 11 player firearms, including damage, cadence, magazine capacity, reload duration, range, impact impulse, reaction strength, firing mode, pellets and spread. ProtectedSystemsAudit.json confirms Health, AttackExecutor, BodyRegionDamage, CartoonRagdoll, EnemyCombat, LarperBrain and PlayerMotor2D source hashes are unchanged from the start of Sprint 0040. NativeProfiles.json records player/enemy settings. No camera zoom, HP, accuracy, navigation, movement, death logic or final economy rebalance was made.

Final native audit: all three warehouse scenes have zero missing scripts, zero third serialized loadout fields and zero landmines. Demo still contains 13 enemies. Final Editor console reports no compilation failure and zero console errors. Source/UI/test whitespace checks pass; the broader dirty Git diff still reports Unity-generated empty-value trailing spaces in native catalog/prefab serialization. These were not hand-edited.

The performance tests check caching/bounded behavior only. Their controlled Editor FPS overlay is not representative of phone performance. Previous intermittent Unity ADTM Editor-audio fatal remains an unresolved development-environment concern; this sprint does not claim to repair it.

## Files and evidence

ChangedSourceFiles.txt lists the 19 existing source/UI/test files changed specifically after the Sprint 0039 snapshot. New files are Assets/Editor/Sprint0040Setup.cs and Assets/Tests/PlayMode/LivingImpact0040Tests.cs with Unity-generated metadata. Native prefab/scene profiles were authored through Editor APIs using the setup helper; player serialization was refreshed to remove the old field. Native changes include enemy Larper, Samurai, FireworksExpert, Officer/operation profiles, WarehousePlayer and warehouse scene instances/inherited profiles. ProjectSettings carries the new Android version. No YAML/GUID hand editing occurred.

Historical test artifacts overwritten by regression runs were copied to RegressionArtifacts and their original bytes restored. Before/ contains the source snapshot used for this sprint's comparison. Git's full dirty diff also includes the intentionally retained Sprint 0039 implementation, so it should not be mistaken for a Sprint 0040-only change list.

## Android release

Build result and APK identity are recorded in AndroidBuild.md after completion. Physical-device installation, gameplay, readability approval and sustained FPS are **PENDING USER VERIFICATION**. Use AndroidPlaytest.md; do not assume an Editor capture proves Android-device behavior.
