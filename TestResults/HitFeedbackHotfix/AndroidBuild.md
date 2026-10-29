# Android hotfix release — 2026-10-29

- APK: [AKUMU2-hit-feedback-hotfix-release.apk](C:/Users/jansa/Documents/GitHub/Insane-Dreams-2.0/build/HitFeedbackHotfix/AKUMU2-hit-feedback-hotfix-release.apk)
- Package: com.GAMES.AKUMU2, version **0.14.1 / code 4**.
- ARM64, IL2CPP, release/non-development, non-debuggable.
- Build build_a6d4161649ea: **Succeeded, 0 errors / 974 warnings**, 263.197 seconds.
- Build time: 2026-10-29 19:07:45–19:12:08 Europe/Prague.
- Actual APK size: **39,845,423 bytes**.
- SHA256: `A6CDCBAF343711D1EBC875BD72A7D49FAABF6872336A793BB40257EB252414E3`.

ApkBadging.txt and ApkIdentity.json verify the actual output file's version, architecture and absence of application-debuggable. ApkMetadataAudit.json finds CancelDamageFlash, CacheTintRenderers, pendingLivingImpact and damageFlashMaterial in the APK's native metadata. PackedHotfixAssets.json confirms the new AA-12 body/magazine and flash material/shader are included in the build. Full native BuildReport is retained in BuildStatus.json. FinalSourceHashes.json still matches after build: runtime/test/authoring/shader/native AA-12 source was not changed during export.

Warnings include existing deprecated Unity APIs/directives and shader/build-tool diagnostics; this is not a warning-free project. No unrelated warning cleanup or gameplay rewrite was performed.

AdbDevices.txt records no connected devices. This APK has **not** been installed or played on an Android phone here. Device gameplay, sustained 60 FPS/heat, save upgrade on the user's phone, artistic readability and gunplay feel are **PENDING USER VERIFICATION**.

Install this APK over the previous app while retaining app data. Verify installed version 0.14.1 / code 4. Test all weapons against surviving Larpers, then lethal first shots and deaths during automatic fire: full-body short red while alive, normal colors/materials on the corpse, no red forearms. Repeat AA-12 partial/empty/short-reserve reloads, firing attempts during reload, switching interruption and subsequent reload; watch HUD and ammunition conservation. Check crowded sustained fire for phone performance and normal AI recovery. No physical-device success is implied by the 50 passing Editor tests or controlled captures.
