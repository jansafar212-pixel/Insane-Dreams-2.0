# Sprint 0036 — UI, HUD, shop and presentation

2026-10-28 · 黒橋 GAMES / Kurohashi Games · AKUMU 2

## 1. Architecture audit

The project already uses runtime **UI Toolkit**, not a Canvas HUD. GameFlowController
builds menu/shop trees on navigation; WarehouseTouchControls caches its readout
references/snapshots; MissionHud refreshes mission events and a sampled clock.
HandmadeUI.uss and AkumuTypography.uss already form a shared foundation. The
existing flow, atomic purchases, saves, safe-area calculation, two sticks and
pause input suspension were retained. No Canvas conversion or AI rewrite.

Issues found: old main-menu crate reference, visually small title, no item imagery
in the shop list, hidden ownership information relegated to tooltips, grenades
mixed into ammunition, a plain wallet line, ghost syringe art in empty slots,
and enemy speech drawn above results across separate UIDocuments. The last issue
required actual presentation-state handling, rather than a CSS-only sibling selector.

## 2. Files modified

- Assets/UI/HandmadeUI.uss — common pressed/focus edges and speech cards.
- Assets/UI/GameFlow.uss — main menu, shop cards/tabs/icons/wallet, settings/pause.
- Assets/UI/WarehouseControls.uss — health frame, equipment/touch appearance,
  revival frame. Existing control geometry/aim-stick right offset unchanged.
- Assets/UI/MissionControls.uss — compact objective, results/failure frames.
- Assets/Scripts/Progression/GameFlowController.cs — card icons/status, five
  categories, wallet and real weapon facts.
- Assets/Scripts/Input/WarehouseTouchControls.cs — empty injection images become
  transparent while the empty-slot mark remains visible, on count change only.
- Assets/Scripts/Missions/MissionHud.cs and Assets/Scripts/Enemies/LarperSpeechBubble.cs
  — terminal-state speech suppression and reset on HUD disable/subsystem reset.
- Assets/Tests/PlayMode/Sprint0036PresentationTests.cs (+ Unity-generated meta).
- Assets/Tests/PlayMode/MobileHudPyroTests.cs — long-notice assertion now checks
  scrolling only when content overflows; fitting content must fit its viewport.
- GRAPHICS_BIBLE.md, DEVELOPMENT_LOG.md, SPRINT_RESUME.md and this evidence folder.

No native scene/prefab, texture pixels, weapon data, progression configuration,
prices, AI combat rules, physics, input sensitivity, cameras or audio assets were
authored in this sprint. Existing suites refresh some of their prior capture files.
The whole dirty working tree includes earlier work; it is not this sprint's diff.
Nothing staged, committed or pushed.

## 3. Shared design system

The recognizable existing pixel wordmark and block font anchor the system. Ink,
dark teal, cream, warehouse ochre and rust combine square structural borders and
heavy lower edges. Pressed controls change edge weighting immediately; focus has
a visible ochre edge. Cream dialogue cards use dark ink, panic cards warmer paper.
Menus, item details, health, pause and results share the same construction. No
blur, gradients, random noise, new shaders, bitmap generation or full-screen FX.
Main-menu crates reuse the actual approved CrateWoodBrace_32x32 sprite. Item art
is the existing equipped sprite or inventory icon, fitted without distortion.
Controlled captures are implementation evidence, not human artistic approval.

## 4. Main menu and settings

The original wordmark is enlarged from its existing geometry. The action column
has a clear warehouse-rust rail; title, tagline and stamped notice remain distinct.
Background is the same inexpensive static industrial silhouette with approved
crates. PLAY, SETTINGS and desktop QUIT retain their handlers. Existing development
lab entry remains development-only; no new ordinary-player debugging entry was added.
Settings inherit the shared frame/button style. Existing volume/control-size
settings and their persistence were not rewritten.

## 5. Gameplay HUD and weapon selection

Health has a framed, thicker muted-green track with the existing danger color.
Numbers and damage/healing remain driven by the same cached runtime values.
Ammo stays **current / magazine capacity**, reserve separate; pistol remains 15.
The two existing revival slots retain capacity two. Empty slots now show a dash
without an injection silhouette. Count changes invalidate the existing snapshot;
no per-frame rebuild was introduced. Weapon/tool buttons and action controls share
new edges/focus states; existing loadout selection and grenade aiming remain.
This sprint restyles the existing equipped-loadout panel; it does not add an
all-owned-weapons inventory overlay during combat.

The mission heading is hidden to keep the objective concise. Details are bounded
to 112 layout pixels and scroll when necessary. Existing wallet pickup aggregation,
notifications, automatic collection and reward journaling were retained. No new
popup emitter or new combat particles were added.

## 6. Shop and equipment

The wallet is a prominent reusable coin/credit strip. Each existing item list
entry has fitted artwork plus a visible availability/ownership/equipped or stock
readout. Selected items use a larger preview, a consistent detail frame and concise
damage/magazine/reload facts from WeaponDefinition. No invented comparison scores.
Five category tabs: weapons, ammunition, throwables, body armor, Medical. Grenades
are now found in their own tab. Medical retains exactly Duct Tape and Faintanyl;
the exact text `relax its not addictive, just habit forming` is preserved. Both
medical cards share restrained green edges. Armor condition/trade-offs continue
to read actual catalog data and explicitly preserve movement speed. Purchases
still delegate to GameSession and redraw only after navigation/action.

## 7. Development credits

Verified again, not assumed: actual persistent test profile reads **10000**.
Sprint0035's existing development-only catalog option and revision35 migration
remain unchanged. Five executed tests cover low existing save, new save, rich
save, disabled option, failed atomic write, purchase/restart and no regrant.
Shop capture tests also buy SMG and reload the temporary profile. Actual user
save was read only during this sprint; no top-up/reset or test purchases on it.
Release branch still grants no money. No economy rebalance.

## 8. State management, death and victory

Pause, revival, failure and victory inherit common frames and immediate buttons.
Existing pause/resume, retry, return, ragdoll, automatic injection and reward
handlers are retained. Results now dismiss existing speech and refuse new speech
until presentation unblocks on HUD disable/reset. This uses the existing registry
of at most two bubbles, with no scene scan or new dialogue system. It fixes actual
speech overlap in inspected results captures. A hidden terminal pause control
cannot compete with result actions. Existing no-checkpoint progression remains.

## 9. Performance audit and limits

Before editing, AndroidPerformanceRegressionTests passed 2/2; after editing they
again passed 2/2 (1.77 s suite execution, **not** a frame-rate measurement). They
check cached HUD invalidation and stable equipped native sprite identity. No
Android frame pacing, effect pools, nonalloc physics, sampled AI or movement code
was changed. Added item icons/status labels are created on screen redraw only;
this increases navigation allocations modestly and is a device profiling item.
No new per-frame searches, text polling, animation loop, material or Canvas rebuild.

No controlled CPU/GPU/GC timing matrix or Android before/after FPS measurement was
obtained. Editor screenshot overlays during test execution are heavily affected by
automation and are not performance evidence. Main menu/shop/static navigation,
quiet/ordinary/chaotic gameplay, death/restart and repeated navigation still need
on-device frame-time/GC/memory observation. Stable 60 FPS is a target, not verified.

## 10. Tests and captures

**100/100 selected tests passed: 97 Play Mode, 3 Edit Mode.** Raw final suite results
are in TestRuns.json. This is not the entire project-wide suite.

Suites: Sprint0036Presentation(2), Sprint024Flow(12), MobileHudPyro(3),
Sprint0035(5), Sprint0034Economy(11), Arsenal(8), AndroidPerformanceRegression(2),
TraversalRegression(12), Sprint0027Fall(6), Sprint0032AI(10), EconomyRules(3),
Medical(1), CowardsBunker(5), Throwables(4), Healing(1), Slide(2),
FirstOperation(7), Sprint0202Suppression(6).

The new presentation test checks all five categories at layout widths for 16:9,
18:9, 19.5:9, 20:9 and 4:3, meaningful touch heights, correct Medical membership,
purchase/restart, zero-injection art, pause time and terminal speech dismissal.
It captures MainMenu, Settings, all shop categories, HUD, Pause, Victory and Failure.
Victory/Failure images are **scripted presentation probes**, not organic completed
combat runs or evidence of payment. Existing flow/economy tests independently
exercise payout, equipment, death/revival and replay. Movement route tests control
enemies; no unassisted human full-level run is claimed.

## 11. Audio and Android limitations

AudioClip audit found NearMissPressure.wav, FragBoom.wav and TapePeel.wav, not three
music tracks. The user confirmed the tracks **have not been uploaded yet**. Music
integration/transitions are deferred until they arrive; no new composition/player
was fabricated. Existing bounded effect audio remains. The previous sprint's
native Editor ADTM audio deadlock on stopping Play remains an undiagnosed known
issue; it was not repaired or declared safe by this UI work.

Physical phone install/profiling, touchscreen comfort, cutouts, sustained combat
readability, sound and human aesthetic acceptance remain required. No attached
device test was performed. Desktop layout assertions do not prove actual multitouch.

## 12. Remaining issues and handoff

- Music awaits the user's three files, by explicit clarification.
- Native Editor ADTM stop-time fatal incident from sprint0035 remains undiagnosed.
- Full performance matrix/device profiling and human visual acceptance remain.
- Existing loadout panel restyled; no new full owned-arsenal popup was introduced.
- Existing combat/healing/pickup effects retained rather than new decorative FX.
- Representative menu captures use the Editor viewport; all five aspect ratios
  are layout checks, not five physical devices or five exported screenshots each.

Android Development build **build_7cc902915c2c succeeded in 124.941 seconds**:
zero errors, 977 warnings (970 package shader warnings, seven API/config/development
warnings). Full report: BuildStatus.json. ARM64 APK:
`build/Sprint0036/AKUMU2-sprint0036-dev.apk`, 67,236,575 bytes (~64.1 MiB).
SHA256: `5b3d9b4099571648cd6a478f193fe3a0bbf274d817cc411af3311d44877eda13`.
ApkVerification.json confirms ARM64, terminal speech method and credit metadata.
No Windows Player build was performed. No source/asset edits during export.
Final Editor is ready, stopped, Warehouse_Demo open/clean, compilation clean;
Console zero errors/87 warnings after export. The previously observed ADTM native
fatal remains a known undiagnosed issue, not dismissed by these checks. Final
actual-save read still confirms 10000/revision35. No commit or push.
