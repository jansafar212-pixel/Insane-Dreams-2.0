# AKUMU2 Hotfix0036.1 — HUD background transparency

UI-only change. The game uses UI Toolkit UIDocuments, not Canvas/Image/CanvasGroup. Live inspection found zero Canvas and CanvasGroup components. No parent opacity was added. No C# gameplay, AI, weapon, physics, economy, level, sprite, layout, position or touch-size changes.

Changed only appended background-color rules in:

- Assets/UI/WarehouseControls.uss — sticks40%, handles65%, action buttons60%, weapon selection65%, vitals/ammo background55%, health track55% and fill85% including critical health; revival panel backgrounds60–65%.
- Assets/UI/MissionControls.uss — mission panel55%, interaction/results backgrounds60–65%.
- Assets/UI/GameFlow.uss — pause face60%, gameplay pause/settings overlays60–65%; normal main-menu/shop styles untouched.
- Assets/UI/HandmadeUI.uss — gameplay speech card backgrounds70%.

RGB state colors, text, essential icons and the existing disabled opacity behavior are retained. Active, healing, deployed, selected, empty, critical-health and failed-result rules retain the intended transparent backgrounds. Settings persist in the imported USS assets and are reapplied when documents/scenes are recreated. Existing scripts were inspected: health changes width and classes, not background alpha. The temporary black mission-reset cover remains a transition cover, not a gameplay HUD panel. Developer tool windows and world effects were not changed.

## Verification

Seven existing Unity PlayMode checks passed: presentation/menu/shop/mission/pause/results2, independent touch pointers/fire1, both HUD layouts across landscape/ultrawide/portrait1, HUD ammo/weapon/grenade rebind and presentation2, real pause button1. Final results:FinalTestRuns.json. Final presentation rerun and results capture also cover dynamically generated result buttons.

Additional temporary attached UI Toolkit probe verified resolved alpha targets, unchanged element/group opacity1, opaque HP/ammo/objective text, and critical-health/healing/selected state colors. Results:StyleAudit.json and StateAudit.json. Probe window was closed and temporary imported capture files removed. Runtime background-execution setting was restored. No permanent test or runtime scripts were added.

HUD.png, Pause.png and Results.png are controlled Editor captures; LiveHUD.png is a capture from the user's existing paused Play session. They were inspected for legibility and background visibility. These checks are not Android-device touch/performance verification.

Final Android release build build_7173418f1d13 succeeded in53.461seconds,0errors/1251warnings (examples include existing Pipeline configuration and Sentis shader warnings). APK:build/UITransparency0036_1/AKUMU2-ui-transparency-release.apk,39,819,759bytes, ARM64/nondebuggable. SHA256:0eac86f000b775c84a8622ac272b80af8c9939f70cb3d6c9f849cd6a796056ab. BuildStatus.json/PackedStyles.json confirm all four changed USS assets are included; aapt/hash evidence is in ApkVerification.json. Android build success is confirmed; phone installation/touch quality remains untested. Final Editor ready/stopped; no source edits during export.

No stage, commit or push. Earlier dirty work preserved.
