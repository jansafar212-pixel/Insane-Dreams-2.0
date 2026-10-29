# Concrete pillbox follow-up — implementation and verification

## Changed scope

Warehouse_Demo's old timber/sandbag dugout has been replaced by a compact concrete
pillbox. The Sprint 0028 environment sheet, surrounding scenery, terrain route,
character graphics, encounter roster and unrelated gameplay are preserved.

Important changed files:
- Assets/Scenes/Warehouse_Demo.unity
- Assets/Scripts/Enemies/EnemyCombat.cs
- Assets/Scripts/Enemies/OfficerShotTelegraph.cs
- Assets/Editor/ConcretePillboxSetup.cs
- Assets/Art/ConcretePillbox/ (native Sprite/Texture2D assets)
- CowardsBunkerTests.cs, FirstOperationTests.cs, Level1CombatHotfixTests.cs,
  CowardsBunkerRulesTests.cs, Sprint0029EquipmentRulesTests.cs
- DEVELOPMENT_LOG.md, GRAPHICS_BIBLE.md, ART_BIBLE.md, SPRINT_RESUME.md

## Collision and opening

The shell is4.5world units wide, on the existing Officer dais at y1.6. Five static,
nontrigger Default-layer BoxCollider2D faces form the lower/upper front wall, rear
wall, roof and foundation. All have Concrete impact classification. The front wall
has exactly one opening at y2.5–3.4 (.9world units); its gap is also the visible
firing slit. Rear and roof have no shooting openings. Existing terrain forms the
support. The new base stays within the dais thickness to preserve lower clearance.

Flat three-color concrete, thick edges, a few coarse chips and dark interior use
32PPU Point-filtered native art, no mipmaps or soft shading. Foreground concrete
facade bands occlude the occupant below/above the slot; the colliders lie on the
matching exterior shell, leaving walkable room inside. This is a side-view cutaway,
not a full solid collider filling the room. Renderer sizes explicitly match world
sizes, avoiding rounding of small native texture dimensions into collision drift.

## Officer integration

Scene-only firing/hiding anchors x141.05/x143.25 reuse the existing Officer component,
motor, perception, suppression, reload cycle, states and timed hiding. No new AI
manager or global navigation system. He walks to the firing hole, uses the existing
half-second telegraph, fires, and usually retreats/crouches deeper inside; finite
hide timers permit returning. No teleportation, new hitboxes, health increase or
weapon damage/rate/accuracy rebalance.

Both wall-face intersections of the actual barrel ray must lie within the slit.
This also checks a muzzle extending through the hole, so standing too close cannot
bypass concrete. Opposite-facing/over-wall/below-sill shots are refused. Laser and
committed shot share this aperture rule plus existing geometry clipping. Original
Officer encounters without these scene anchors retain their existing behavior.

## Validation notes

Initial controlled tests found and corrected a .11-unit anchor arrival deadlock
and a sill too high for the actual downward-aimed pistol. The opening is now .9
high and arrival uses the motor's practical stopping tolerance. A first version
of the new base protruded beneath the platform; its thickness was reduced within
the existing dais. The old full-route test used single jumps only; it now issues
existing early double-jump inputs to traverse the closed pillbox roof, matching
the separately tested physical crossing. No player motor or jump tuning changed.

Old rear-flank tests were replaced with concrete-shell/embrasure tests. The older
platform test now accepts an overlapping solid foundation as a valid ray blocker.
Tests retain actual weapon executors, physics grenades, player input, damage,
mission progression and camera firing gates. A compiler error in the initial new
test namespace was corrected. Native authoring/asset cleanup exceeded the CLI's5s
response limit once; the saved clean scene and matching geometry were subsequently
read back and verified. It was not blindly reapplied after that timeout.

## Remaining manual review

Human artistic/playtest acceptance and real Android-device behavior remain untested.
Inspect opening/hidden poses, pressure/fairness, aiming through the slit from both
nearby elevations, player roof traversal and actual fight completion. No APK rebuilt
for this focused follow-up; build/Sprint0029/AKUMU2-dev.apk predates this correction.
Compilation/controlled Editor tests do not establish device performance or game feel.

Final test totals and Editor status will be added after regression completes.

## Final verification — resumed 2026-10-27

Recovered the existing implementation without regenerating the bunker or repeating
completed Sprint0029systems. The interrupted final Play Mode batch had already
finished successfully. Completed the remaining Edit Mode checks on resumption.

**31/31 focused tests passed:22PlayMode,9EditMode**, no skips/failures in latest
results. Regression.json selects only the final fixture results; the earlier
ConcretePillboxTests.json is preserved diagnostic history from a temporary fixture
that was replaced by the final CowardsBunkerTests. It is not an unresolved failure.
Coverage: actual player/Officer hitscan versus concrete, aperture/laser clipping,
hide/peek/fire/retreat, ground and platform targets, seven-round reload, physical
frag roof collision/blast shielding, normal gunfire defeat followed by movement
to Results, full Level1 route, Samurai/expert behavior, original generic hearing/
muzzle/navigation regressions, all six ladder exits, Medical/pause controls,
scene roster/terrain/asset bounds/equipment rules. Full Sprint0029's preceding
154-test result is retained in its own folder; it was not unnecessarily rerun or
claimed as a fresh154-case run after this correction.

FinalEditor.json: Warehouse_Demo saved/clean/open, outsidePlay, notcompiling,
0missing scene/prefab scripts,5solid pillbox faces, all pillbox sprites/anchors
present. FinalConsole.json:0actualerrors/warnings after diagnostic review. The
one displayed error was Unity CLI dispatching unavailable `editor status`, not
a gameplay/compile exception; ConsoleErrors.json preserves that and earlier tool
timeout diagnostics before clearing. No game-code changes were needed on resume.

AfterHashes.json confirms the approved environment sheet, Warehouse_Testbed and
CartoonRagdoll.cs unchanged from the follow-up baseline. EnemyCombat,
OfficerShotTelegraph and Warehouse_Demo differ as intended. Prior medical/shop/
armor/throwable work is preserved. No Sprint0030overhaul started. GitFinal.txt
records the dirty working tree,0staged changes; no commit orpush.

Implemented by Codex and verified through controlled Unity tests/captures.
Human game-feel/art acceptance and Android-device testing remain manual. The older
Sprint0029APK predates the concrete follow-up; rebuild before testing this correction
on a phone. No new APK or device test is claimed by this completion report.
