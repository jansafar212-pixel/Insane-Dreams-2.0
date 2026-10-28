# AKUMU 2 — Grenade resupply / fragmentation / arm detachment COMPLETE

Implemented by Codex — awaiting human playtest verification. Do not restart.

Latest request: existing ammo boxes replenish fragmentation/flashbang/smoke within
existing caps, stronger chunky frag VFX and +25–30%radius/+30%damage, arms +75%
cumulative detachment threshold. No other balance/layout/AI changes requested.

All implementation complete.31/31focused tests pass(24Play,7Edit). Full file list,
values, evidence, known limits and manual steps: TestResults/GrenadeResupplyHotfix/Report.md.
Final results: Regression.json. Previous checkpoint: PreHotfixResume.md there.

Boxes finite2/type, cap5/type; existing108bullet Demo stock untouched. Frag
radius4.1/damage110.5, same falloff/cover/friendly rules; pooled chunky native mask,
short boom, restrained distance shake. Flash/smoke unchanged. Arms105,legs60,
head80,torso150. Single-region cumulative damage/physics preserved.

Warehouse_Demo clean/open, Play Mode stopped; compile/current Console clean,
0missing scripts. Both warehouse scenes, ragdoll prefab/source and EnemyCombat.cs
byte-identical to hotfix start. Preexisting dirty work preserved. No pending test,
new build, stage, commit or push. Previous APK is older; export fresh for device.

Remaining: manual grenade resupply while spending/switching, normal firefight
VFX/audio/shake and blast balance, unusual localized damage detachment frequency,
Android device frame/touch checks. No known final focused automated failure.
Do not regenerate levels or repeat native setup unless intentionally reauthoring.
