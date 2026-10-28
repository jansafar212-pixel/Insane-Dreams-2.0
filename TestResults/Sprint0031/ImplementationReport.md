# Sprint 0031 implementation report

Implemented by Codex — awaiting human artistic and Android playtest verification.

The shared handmade skin now covers the actual main menu, settings, level selection,
all four shop tabs, gameplay HUD, pause, death/results and enemy speech. These are
existing working screens, not replacement mockups. Approved wordmark glyphs and
menu backdrop remain intact. Dark flat panels, chunky type, square controls,
rust/gold selection and explicit unavailable feedback replace mixed presentation.

Presentation files changed this sprint:
- Assets/UI/HandmadeUI.uss (new), GameFlow.uss, WarehouseControls.uss, MissionControls.uss.
- Assets/Scripts/Progression/GameFlowController.cs: selection, item art, wallet,
  purchase availability, settings values and shared button decoration.
- Assets/Scripts/Input/WarehouseTouchControls.cs: persistent filled/empty revival
  slots, selected grenade counts, empty weapon slots, low-health/heal feedback.
- Assets/Scripts/Missions/MissionHud.cs: failure presentation class.
- Assets/Scripts/Enemies/LarperSpeechBubble.cs: shared square speech styling.
- Assets/Art/Interface0031: three tiny native sprites/textures and corresponding metas.
- Assets/Editor/Sprint0031Icons.cs: native asset creation helper.
- Assets/Scenes/Warehouse_Demo.unity: six existing sign font/material bindings only.
- Assets/Tests/PlayMode/Sprint0031Tests.cs (new); Sprint0029SystemsTests.cs updates
  the former hidden-slot assertion to the newly required persistent empty slot.
- GRAPHICS_BIBLE.md, DEVELOPMENT_LOG.md, SPRINT_RESUME.md and evidence in this folder.
No character/enemy prefab was modified by this sprint. Existing dirty files from
prior sprints are preserved; the entire Git diff is not this sprint's change list.

Reusable foundation: palette/style tokens, square panel/button states, selection
notch, icon classes, purchase-state label, filled/empty slots and speech skin.
Gameplay, prices, capacity, combat logic, aim/joystick algorithms and sensitivity,
level geometry, Officer laser/tracer and approved environment art were preserved.
ProtectedAfter.json and WorldTypographyDiff.txt record the scoped comparison.

Verification and build results are recorded in Validation.md. Before/After menu,
HUD and shop PNGs are included alongside captures of settings, level selection,
all shop tabs, pause, death, victory, grenade aiming and five actual GameView sizes.

Manual acceptance:
1. Start MainMenu; focus/press each action, change settings, re-open them and enter
   Level Selection. Check typography, contrast and the unchanged title artwork.
2. Browse all shop tabs, buy/equip weapons, ammunition, armor and Medical supplies;
   test insufficient funds and full stock, then start the mission.
3. On Android, move and aim together, jump/reload/hold heal, select each grenade,
   throw, pause/resume and use blind-fire only from usable crouched cover.
4. Fire/reload/switch weapons: current/capacity changes and reserve stays separate.
   Deplete supplies and revival charges; check unavailable/empty feedback and death,
   replay, results, level selection and persistence after relaunch.
5. Review low-health contrast, readable speech (maximum two), placement under a
   phone cutout, extreme aspect ratios, finger occlusion and sustained frame rate.
These human/device checks cannot be inferred from a passing Editor test or APK build.

Remaining limitations: no physical Android device or human artistic approval was
available. Long shop content intentionally scrolls; review finger scrolling and
comfortable reading on the smallest supported phone. Existing auto-revive remains
automatic; the optional legacy manual-revive card is styled but is not a new flow.

Capture comparisons:
- Main menu: [before](Before_MainMenu.png) / [after](After_MainMenu.png).
- Gameplay HUD: [before](Before_HUD.png) / [after](After_HUD.png).
- Shop: [before](Before_Shop.png) / [Medical after](After_ShopMedical.png),
  [Weapons after](After_ShopWeapons.png), [Ammo after](After_ShopAmmo.png),
  [Armor after](After_ShopArmor.png).

Build delivered: build/Sprint0031/AKUMU2-dev.apk. See AndroidBuildSummary.json,
ApkContentAudit.json and Validation.md for build diagnostics and exact test outcomes.
