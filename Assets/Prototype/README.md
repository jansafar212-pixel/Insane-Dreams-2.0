# Sprint 001 prototype

Open `Assets/Scenes/Prototype_01.unity`, press Play, then click the Game view.
Move with A/D or Left/Right; jump with Space. Jump toward each amber platform
from its side: platforms are solid, so jumping directly underneath one hits its
underside. Walk toward the right edge before jumping to the next platform.
Stop and start Play Mode to reset the player to the saved spawn.

## Configuration

- Player prefab: `Assets/Prefabs/Player.prefab`.
- PlayerMotor2D: movement speed 7, acceleration 65, deceleration 85, jump speed 12,
  ground probe distance 0.08. Ground uses Default layer; the player uses Ignore
  Raycast, keeping its own collider out of ground checks.
- Rigidbody2D: gravity scale 3, continuous collision detection, interpolation,
  frozen rotation, frictionless/non-bouncing physics material.
- DesktopPlayerInput reads the existing Input System keyboard backend. Future
  touch adapters can call SetMoveInput and RequestJump on PlayerMotor2D.
- PlatformCameraFollow exposes target, offset and smoothing time. It follows
  the interpolated player in LateUpdate; orthographic size is 6.5.
- Controls.uxml / Controls.uss and ControlsPanel.asset provide a static reminder.
- All art is a locally generated white square tinted by SpriteRenderer. The
  unlit URP 2D material needs no scene lighting or downloaded artwork.

## Verification performed in Unity 6000.6.6f1

- Compilation completed successfully; no C# compilation errors or runtime
  exceptions were reported in the final Console check.
- MCP verified the scene hierarchy, prefab connection and physics components.
- Play Mode: player fell onto the floor, became grounded and stayed upright.
- Real-frame Input System virtual-keyboard tests exercised D, RightArrow and
  Space through DesktopPlayerInput. Horizontal movement reached speed 7 and
  decelerated to rest after release.
- Player jumped onto all three platforms. Upper-platform tests started from
  repositioned approach edges in Play Mode; a continuous full traversal was
  not manually played. No saved transforms were changed by testing.
- Additional Space presses during ascent did not reset upward velocity:
  observed vertical velocity decreased from 6.11 to 3.76 during one test.
- Right boundary stopped the player at x approximately 18.09 with no rotation.
- Camera followed horizontally and vertically; Game-view capture confirmed
  player/platform colors and readable controls. Visual smoothness still merits
  human playtesting on target hardware.
- Exited Play Mode. Scene and connected prefab were saved and not dirty;
  saved spawn remained (-9, 2, 0).
- Android/touch, device builds, A/LeftArrow physical keyboard input, and unusual
  high-speed collision cases were not independently tested. No touch controls
  are implemented in this sprint.

## Repository review

No commits or pushes were made. Existing SampleScene, original content, package
manifest, input backend and authentication configuration were not edited.
Existing untracked `.codex` and `.vscode` folders were left alone.

Unity also generated a UI Toolkit runtime theme under `Assets/UI Toolkit`,
`ProjectSettings/SceneTemplateSettings.json`, an App UI config reference in
EditorBuildSettings.asset and the APP_UI_EDITOR_ONLY define in
ProjectSettings.asset. The installed App UI package's AppUIManager.ApplySettings
automatically adds the latter two when initialized. These generated changes
were retained rather than changing package behavior or deleting files.
