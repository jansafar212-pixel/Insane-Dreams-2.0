# Sprint 0040 — Android checks pending user verification

Install the new Sprint0040 APK identified in AndroidBuild.md. Keep existing app data for migration testing; do not clear progression. Compare installed version 0.14 / code 3 with `adb shell dumpsys package com.GAMES.AKUMU2` when available.

- Open shop/equipment: exactly PRIMARY WEAPON and SECONDARY WEAPON. Extra formerly equipped secondaries remain purchased and selectable from owned stock, but are unequipped. Confirm money, armor, grenade quantities and ammunition survived.
- Equip Thompson as primary and Desert Eagle as secondary. Replace each with another weapon of the same role. Check ownership, magazine/reserve values, sprite, firing and interrupted reloads. No third firearm can be selected; grenades remain separate. Relaunch and repeat.
- At ordinary combat zoom, shoot surviving enemies with Glock, Thompson, assault rifle, LMG, Desert Eagle, shotgun and sniper. Check readable light/heavy differences, shotgun pellet aggregation and head/arm/leg/torso reactions from both sides and diagonally.
- Sustain automatic fire: enemies may flinch but must recover from falls and resume movement, aim and firing. Check Larper cowardice, Samurai aggression, Officer evasion, cover/reload behavior, normal death ragdolls and no detached sprites or launched roots.
- Check simultaneous mobile movement/aim, both weapon buttons, reload, independent grenades, pause/resume and readable ammunition HUD. Check prolonged combat performance/heat on the phone.
- Existing bunker geometry/test mismatch is unresolved: watch Officer front cover, rear entrance/flanking and grenade/roof traversal. Three automated tests remain red; see Report.md.

All real-device observations above are pending. Automated Editor tests and controlled captures are described separately in Report.md.
