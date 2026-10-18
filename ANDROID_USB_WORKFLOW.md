# Android USB development — Unity 6000.6.6f1

Sprint 008 recheck (2026-10-18): Editor still reports Android Build Support
unavailable; AndroidPlayer is absent from this installation and ADB is not on
PATH. Active target remains StandaloneWindows64. No modules, account settings or
build target were changed. No authorized-device deployment, FPS or memory result
is claimed. Include repeated ragdoll impacts/waking and four simultaneous six-body
ragdolls in the pending phone pass; new effects reuse a fixed number of renderers.

Verified on 2026-10-18: the connected Editor is **6000.6.6f1**. Its PlaybackEngines folder contains WebGL and Windows support, **no AndroidPlayer**. `adb` is not on PATH. No Android build, installation, phone authorization or hardware performance test was possible. The active target remains StandaloneWindows64. This sprint did not install modules or change project/build settings.

## One-time manual setup

1. In **Unity Hub → Installs**, locate **6000.6.6f1**, open its menu → **Add modules**. Select **Android Build Support**, expanding it to include **Android SDK & NDK Tools** and **OpenJDK**. Install all three for this exact Editor. If Hub cannot manage this installation, install/register the matching Editor through Hub first; do not copy another Editor's AndroidPlayer folder.
2. In that Editor, check **Edit → Preferences → External Tools → Android**. Prefer the SDK, NDK and JDK installed with Unity. Expected location: `C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\PlaybackEngines\AndroidPlayer\` (SDK, NDK, OpenJDK subfolders).
3. The Unity 6.6 dependency table specifies **SDK Build Tools 36.0.0, Command-line Tools 16, Platform Tools 37.0.1** for 6000.6.4f1 onward; **NDK r27c (27.2.12479018), OpenJDK 17, CMake 3.22.1**. Prefer Hub's matching set. These are build-tool versions, not a requirement that the phone run Android 36. [Unity setup](https://docs.unity3d.com/6000.6/Documentation/Manual/android-sdksetup.html), [version table](https://docs.unity.com/en-us/engine/6000.6/manual/platform-specific/android/getting-started/sdksetup/supported-dependency-versions).

## Connect and authorize the phone

1. On the phone enable **Developer options** (usually tap **Build number** seven times in About phone), then enable **USB debugging**. Manufacturer menu names vary.
2. Connect a data-capable USB cable, unlock the phone, and accept its RSA debugging authorization prompt for this PC. Use the manufacturer's Windows USB driver if required.
3. Once the SDK exists, this read-only PowerShell check avoids changing PATH:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe' devices -l
```

The phone should appear as `device`. `unauthorized` means the prompt still needs approval; `offline` or an empty list calls for checking cable, USB debugging and driver, then reconnecting. With multiple phones, select the intended serial/run device. [Android ADB documentation](https://developer.android.com/tools/adb).

## Build, install and launch directly

When ready to switch platforms manually, open **File → Build Profiles**, add **Android**, then **Switch Profile**. Include Warehouse_Testbed in its scene list. Disable **Export Project**; use an APK for this local iteration loop. Choose the authorized phone under **Run Device** (Refresh if needed), enable **Development Build**, and select **Build and Run**. Choose a local output such as `Builds/Android/InsaneDreams-dev.apk`. Unity builds, installs and launches on the chosen device. Subsequent Build and Run uses the same connection; Ctrl+B repeats after the initial output selection. No cloud transfer is involved. [Unity Android build process](https://docs.unity3d.com/6000.6/Documentation/Manual/android-BuildProcess.html).

## Faster iteration, when supported by the installed module

Unity 6.6 documents **Build to Device → Patch / Patch And Run** with Development Build enabled. Patch And Run deploys changes and launches; the first patch prepares the device environment and can take longer. Use the Editor's option when available rather than a custom ADB deployment script. The 6.6 documentation no longer requires an initial regular Build and Run before patching. Keep normal Build and Run as the fallback if the module/configuration cannot patch, or after changing the application identity, signing or toolchain. Neither deployment path was tested on this PC. [Build settings](https://docs.unity3d.com/6000.6/Documentation/Manual/android-build-settings.html), [Unity 6.6 application patching](https://docs.unity3d.com/6000.6/Documentation/Manual/android-AppPatching.html).

## First device acceptance pass — pending

Check both landscape orientations and cutouts; simultaneous move/aim/fire/chute touches; airborne ladder grabs and dismount lockout; low-beam crouch and safe standing; chute deployment/retraction; recovery while holding touches; background/resume and canceled pointers. Inspect head-following dialogue and facial readability, then defeat several Larpers and verify capped bodies disappear. Profile frame pacing and physics on the actual phone before increasing actor counts. Editor tests do not establish device performance or thumb feel.
