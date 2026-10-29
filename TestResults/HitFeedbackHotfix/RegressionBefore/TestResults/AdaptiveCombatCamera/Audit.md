# Adaptive combat camera — current implementation

This follows0033; the user labels this camera brief Sprint0031. Do not overwrite
or repeat the earlier0031UI overhaul. Previous checkpoint preserved here.

Audit: current URP, one WarehouseCameraFollow, no PixelPerfectCamera/Cinemachine,
MSAAoff/qualityAA0, playerRigidbody Interpolate. CharacterVisualDriver.Facing is
movement-driven even while torso aims backward. Old follow consumed that facing,
rounded final camera position to1/32worldunits despite fractional final zoom,
double-smoothed direction/position, and hard-clamped direct movement at edges.
No short combat memory, explicit aim-strength handling or falling lookahead.

Implemented in the existing authoritative LateUpdate pipeline: aim/fire-intent ->
1s combat memory -> committed movement -> last meaningful facing. Camera aim
threshold.3 with release hysteresis and.12s direction commitment. Exact critical
settling for composition.5s, normaltracking.16/combat.11, laddercenter.4s plusbrief
exit hysteresis, vertical.24s/.25unit deadzone, maxfalllead1.6 at14units/s,
verticalaimlead1.2, screenmargins.18, boundaryease2units. Nearby active combat
context from existing registry sampled~6.7Hz contracts lead by at most45%, radius6.
No target lock, scene search, raycasts or per-frame collections. Input addition is
one read-only AimIntentStrength property; registry addition is read-only scalar
pressure. No movement, weapon or enemy decision rules changed.

Existing orthographic/PPU integer projection,1.155zoom and Android1.125 multiplier
retained. Continuous transform output replaces art-grid quantization; no low-res
RT, PPcomponent or competing follow added. Explicit SnapToTarget stays spawn/
checkpoint/replay only. Three scenes saved through Editor APIs withcamera settings.
First multiscene response timed out, but completed; individual native saves later
confirmed success. Scene diff includes camera fields and Mission01's unchanged
false firstOperation default being serialized; no layout/geometry changes.

Initial tests caught/ fixed stale prior combatDirection at new aim episode; initial
bounds fixture let player run outside level and now uses real padded boundarywalls.
Final new suite9/9passed; deterministic30/60/120Hz+irregular dt, managed allocation
check over1000steps, real input opposition/memory/noise, nearby threats/death,
verticalaim/fall/parachute and aspect-bound checks. Existing regressions in progress.
Raw results and initial failure evidence in this folder. No device/FPS claim.

Final: 63 selected tests, 61 passed, 2 unchanged historical Sprint013 failures. Edit11/11. Final reference/Console audits successful; Demo saved/open/stopped. See Validation.md. No APK export or physical-device validation.
