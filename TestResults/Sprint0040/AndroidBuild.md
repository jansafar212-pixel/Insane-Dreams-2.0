# Android release identity — Sprint 0040

- Build: build_eb8380764492, Succeeded, 213.368 s, 0 errors / 974 warnings.
- APK: [AKUMU2-sprint0040-release.apk](../../build/Sprint0040/AKUMU2-sprint0040-release.apk)
- Package: com.GAMES.AKUMU2, version 0.14 / versionCode 3.
- ARM64 (arm64-v8a), IL2CPP, non-development, non-debuggable.
- Actual APK size: 39,843,675 bytes. BuildReport total includes other outputs and is not the APK size.
- SHA256: `4AEC0E14009A5BED508D47D8E452D2958061412235C8E3D5E9182C50F16CD5AD`
- Built 2026-10-29 15:19:11–15:22:44 Europe/Prague. APK file timestamp 15:22:42.

BuildStatus.json contains the full native report. ApkBadging.txt verifies actual packaged identity/architecture and absence of application-debuggable. ApkMetadataAudit.json checks the actual APK's global-metadata.dat: new regional response/configuration and legacy migration symbols are present; Firearm3/firearm3 are absent. The read-only legacy secondary2 parser intentionally remains for save compatibility. FinalSceneAudit.json finds zero missing scripts, zero third serialized runtime fields and zero campaign mines in all three warehouse scenes; Demo retains 13 enemies. NativeProfiles.json and WeaponBalanceAfter.json record final settings. The source was not changed after this build started.

Warnings include existing deprecated Unity APIs/directives, shader diagnostics, and Pipeline's missing optional RuntimePipelineConfig (Editor tooling is disabled in Player). The build is not warning-free. Full messages are retained in BuildStatus.json; unrelated tooling/shader refactoring was outside this sprint.

`adb devices` returned no connected devices. This APK has **not** been installed or played on a physical Android device here. Installation, migration on the user's real save, gameplay feel, sustained FPS and visual acceptance are **PENDING USER VERIFICATION**. Verify the installed package reports version 0.14 / code 3; see AndroidPlaytest.md.
