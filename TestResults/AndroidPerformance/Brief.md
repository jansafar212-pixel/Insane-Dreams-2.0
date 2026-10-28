# AKUMU 2 — SPRINT 0032
## ANDROID PERFORMANCE OVERHAUL — 60 FPS STABILIZATION

### PRIMARY OBJECTIVE

Perform a comprehensive performance optimization pass on AKUMU 2, prioritizing Android devices.

**Primary target: Samsung Galaxy A56, stable 60 FPS.**

This sprint is about improving the efficiency of EXISTING gameplay systems, not simplifying the game or removing features.

AKUMU 2 must preserve its identity:

- Fast, responsive movement and combat.
- Multiple simultaneously active enemies with distinct AI behaviors.
- Dynamic ragdoll physics and stylized limb separation.
- Tracers, muzzle flashes, impact particles, and explosions.
- Suppression, panic, cover, and enemy reactions.
- The new adaptive camera from Sprint 0031.
- Handmade, deliberately crude pixel-art graphics.
- Two-joystick mobile controls.

**The goal is to make the same game run substantially better, not make a less interesting game that happens to run faster.**

---

## 1. PROFILE BEFORE OPTIMIZING

First inspect the existing project and identify the actual performance bottlenecks.

Do not blindly rewrite functioning systems.

Use appropriate Unity profiling tools, including:

- Unity Profiler.
- CPU and GPU profiling on Android.
- Memory Profiler where useful.
- Frame Debugger.
- Rendering statistics.
- Garbage collection allocation tracking.
- Physics2D performance analysis.

Establish a baseline on the real Android device using a Development Build where possible. Editor performance is not representative of Android performance.

Measure:

- Average FPS.
- Median, 95th percentile, and 99th percentile frame times.
- Main-thread CPU time.
- Render-thread and GPU time where supported.
- Draw calls and batching.
- Managed allocations per frame.
- Memory usage.
- Physics processing time.
- Performance during intense combat.

A 60 FPS target gives approximately **16.67 ms per frame**.

Aim for typical workloads comfortably below that budget, ideally around 13–14 ms or less where feasible, leaving headroom for occasional spikes.

Identify the three largest bottlenecks before making substantial changes.

Prioritize optimizations by measured impact.

---

## 2. ENEMY AI OPTIMIZATION

Inspect all existing enemy scripts, including:

- Larpers.
- Samurai.
- Czech fireworks expert.
- Officer miniboss.

Identify unnecessary computations in Update(), FixedUpdate(), and coroutines.

Optimize:

- Player detection checks.
- Line-of-sight raycasts.
- Cover evaluation.
- Pathfinding and obstacle avoidance.
- Suppression calculations.
- Panic and retreat behavior.
- Combat-state transitions.
- Enemy-to-player distance checks.
- Reloading and firing logic.

Replace redundant per-frame calculations with cached state, events, or appropriately scheduled checks.

Use staggered updates for expensive noncritical AI decisions where this improves performance.

**However, NEVER degrade critical combat responsiveness.**

Preserve the intended reaction times, including approximately 0.7 seconds for basic Larpers and faster Samurai responses.

Enemies must not become less accurate, less aware, less aggressive, or less capable as a side effect of optimization.

Do not introduce artificial processing delays that make enemies seem stupid.

Enemies must continue to behave correctly when several are on-screen simultaneously.

### Important distinction

Optimize AI decision-making frequency where appropriate, but keep physics and time-critical combat interactions correct.

---

## 3. PROJECTILES, TRACERS, AND PARTICLES

Audit the complete firing and visual-effects pipeline.

Investigate:

- Hitscan calculations.
- Bullet tracer creation.
- Muzzle flash effects.
- Impact particles.
- Explosions.
- Grenade effects.
- Object lifetime and cleanup.

Implement sensible object pooling for frequently spawned and destroyed effects where profiling justifies it.

Avoid repeated Instantiate()/Destroy() calls during ordinary firefights.

Pool objects such as:

- Tracers.
- Muzzle flash effects.
- Reusable impact effects.
- Frequently spawned particle systems.

Ensure pooled objects correctly reset all transient state when reused.

Prevent stale visuals, incorrect positions, ghost effects, or duplicate damage events.

**Do not reduce the visual intensity of combat merely to improve performance.**

The player should see the same effects, ideally with fewer frame-time spikes.

Preserve special weapon visuals, including the Officer's distinctive thicker tracer.

---

## 4. RAGDOLL AND PHYSICS OPTIMIZATION

AKUMU 2 uses a hybrid active-ragdoll system, which can be computationally expensive.

Inspect the existing Rigidbody2D, Joint2D, Collider2D, and collision architecture.

Optimize unnecessary physics work without changing the intended behavior.

Priorities:

- Reduce redundant physics queries.
- Ensure collision layers are configured efficiently.
- Avoid unnecessary collision pairs.
- Review excessive rigidbody wake-ups.
- Check for costly joint configurations.
- Prevent inactive ragdolls from performing unnecessary script work.
- Ensure discarded physics objects are cleaned up.
- Investigate whether offscreen inactive ragdolls can safely sleep.

Do not freeze active enemies or disable physics interactions needed for gameplay.

Preserve:

- Stumbling.
- Falling.
- Recovering.
- Impact responses.
- Existing ragdoll behavior.
- Existing body-part damage logic.

Avoid changing the global physics timestep as a shortcut unless measurements demonstrate a benefit and gameplay correctness is verified.

**The ragdoll comedy stays. The wasted CPU cycles go.**

---

## 5. RENDERING AND GPU OPTIMIZATION

Inspect the Unity 2D URP rendering pipeline.

Identify unnecessary rendering costs from:

- Excessive draw calls.
- Sprite overdraw.
- Transparencies.
- Particle effects.
- Expensive shader variants.
- Unnecessary lighting passes.
- Poor sprite batching.
- Excessive background layers.
- Redundant material instances.

Optimize sprite atlases, compatible batching, materials, and render ordering where useful.

Review sprite import settings and texture memory usage.

Preserve the game's established art direction:

- Chunky pixels.
- Hard pixel edges.
- Handmade textures.
- Limited palettes.
- Consistent sprite scale.
- Intentionally imperfect artwork.

Avoid texture filtering or scaling changes that introduce blurry pixel art.

Do not replace the existing aesthetic with smooth or generic mobile-game graphics.

Do not automatically disable important visual effects.

If render scaling or resolution reduction is genuinely necessary, document the trade-offs and test whether pixel clarity remains acceptable before enabling it.

Do not introduce visual flickering, shimmering, or unstable pixel positioning.

---

## 6. MOBILE UI AND HUD OPTIMIZATION

Inspect the mobile HUD and menu implementation.

Pay particular attention to:

- Movement joystick.
- Aiming joystick.
- Weapon selection.
- Reload button.
- Healing button.
- Grenade interface.
- Ammo counter.
- Health display.
- Revive icons.
- Pause menu.
- Shop UI.

Avoid unnecessary full-Canvas rebuilds.

Update visual elements only when their underlying values change.

For example, ammunition text should update on firing, reloading, or weapon switching, not needlessly every rendered frame.

Separate frequently changing UI from static UI where this reduces rebuild costs.

Avoid allocations from repeated string formatting.

Preserve current interface sizes, positions, and functionality.

**Input must remain extremely responsive.**

No optimization may introduce additional perceived joystick latency.

---

## 7. CAMERA PERFORMANCE INTEGRATION

Sprint 0031 introduced the adaptive combat camera overhaul.

Do not redesign its behavior.

Instead, audit the new camera implementation for performance issues.

Ensure:

- No unnecessary scene-wide searches.
- No repeated component lookups.
- No excessive per-frame allocations.
- No duplicate transform updates.
- No conflict between camera movement and Rigidbody2D interpolation.
- No frame-rate-dependent smoothing.
- No redundant calculations of camera targets.

Maintain the established camera behavior:

- Aiming takes priority over movement direction.
- Combat-direction memory.
- Smooth directional transitions.
- Ladder centering.
- Smooth vertical tracking.
- Stable level boundaries.

Do not sacrifice camera quality to recover a small amount of performance.

---

## 8. MEMORY AND GARBAGE COLLECTION

Audit garbage generation during gameplay.

Look for unnecessary:

- Temporary arrays and lists.
- Repeated LINQ operations.
- String allocations.
- Instantiate()/Destroy() operations.
- Closures or delegates created every frame.
- Repeated GetComponent calls.
- Expensive searches through scene objects.

Use caches, reusable buffers, and allocation-free alternatives where appropriate.

Do not blindly replace readable code with complex micro-optimizations that provide no measurable benefit.

Target **zero recurring managed allocations during steady-state combat where practical**.

Temporary allocations during loading or infrequent menu operations are less important than allocations occurring repeatedly during gameplay.

Check for memory leaks during longer play sessions.

Object pools must have sensible limits and must not grow indefinitely.

---

## 9. OFFSCREEN OBJECT OPTIMIZATION

Apply safe optimizations to objects that are not currently visible.

Examples:

- Disable purely cosmetic offscreen particle emission.
- Avoid updating invisible decorative animations unnecessarily.
- Allow inactive physics objects to sleep.
- Skip irrelevant visual-only work outside the camera view.

But respect a critical gameplay rule:

**Offscreen enemies must continue obeying the existing combat and awareness rules.**

Do not disable their AI, erase their memory, or make them fail to react to relevant nearby events simply because the camera does not display them.

Do not change the game's intentional restriction against ordinary enemies attacking from offscreen.

Culling should primarily eliminate unnecessary rendering and cosmetic work, not alter gameplay simulation.

---

## 10. LOADING AND FRAME SPIKE REDUCTION

Investigate sudden frame-time spikes caused by:

- First-time particle effects.
- New enemy activation.
- Explosions.
- Opening menus.
- Weapon switching.
- Physics object initialization.
- Scene transitions.
- Asset loading.
- Runtime resource instantiation.

Where justified, prewarm small effect pools or preload frequently used assets.

Do not preload excessive content if this substantially increases RAM usage.

Minimize avoidable hitches during combat.

Initial scene loading may take time; ordinary gameplay should not repeatedly freeze when familiar effects appear.

---

## 11. ANDROID-SPECIFIC OPTIMIZATION

Prioritize the Samsung Galaxy A56 while maintaining compatibility with desktop builds.

Review:

- Android graphics settings.
- Unity quality settings.
- VSync and target frame rate configuration.
- URP renderer settings.
- Sprite texture compression.
- Render scale.
- Unnecessary post-processing.
- Android memory usage.
- Touch-input latency.
- Thermal throttling during sustained gameplay.

Set up the game appropriately for a 60 FPS target.

Do not assume a high FPS value in the Unity Editor means Android performance is satisfactory.

Avoid unnecessary changes to Android build configuration or minimum supported OS versions.

Do not introduce new third-party optimization packages unless they provide a documented and necessary benefit.

Maintain consistent visuals and gameplay between Android and PC.

---

## 12. STRESS-TEST LEVEL 1

Use the current introductory level as the primary performance benchmark.

It includes approximately:

- 10 Larpers.
- 1 Samurai.
- 1 fireworks expert.
- 1 Officer miniboss.
- Cover and bunker geometry.
- Existing environmental effects.

Test realistic combat scenarios with many active systems simultaneously.

Specific stress tests:

1. Multiple Larpers firing and reloading.
2. Simultaneous tracers and impact effects.
3. Samurai actively pursuing the player.
4. Fireworks expert triggering explosions.
5. Several ragdolls active simultaneously.
6. Player firing while retreating with the camera tracking aim direction.
7. Smoke and grenade effects active.
8. Rapid weapon switching and HUD updates.
9. Ladder climbing and parachute traversal.
10. Extended play to check thermal and memory behavior.

Record results before and after optimization under comparable conditions.

Test sustained performance, not merely the first 30 seconds.

Aim for stable gameplay after 15 minutes of continuous activity on the target device.

If the physical Android device is unavailable, provide the exact build and profiling procedure and clearly distinguish verified Editor results from unverified device performance.

---

## 13. PERFORMANCE ACCEPTANCE CRITERIA

### Target: stable 60 FPS

The target is approximately 16.67 ms per frame on the Samsung Galaxy A56.

Success criteria:

- Normal gameplay runs at or close to a consistent 60 FPS on the target device.
- Combat does not routinely cause substantial FPS drops.
- Major frame-time spikes are measurably reduced.
- Average FPS is not used to conceal poor frame pacing.
- Camera motion remains smooth and responsive.
- Touch controls have no additional latency.
- Enemy behaviors remain functionally equivalent.
- Visual effects remain intact.
- Ragdoll physics remain intact.
- Memory usage remains stable during extended sessions.
- No new crashes, softlocks, or rendering artifacts.

Also record 1% low FPS and percentile frame times to assess consistency.

If genuine hardware limitations prevent a particular target, document the measured limitation rather than claiming success without evidence.

---

## 14. STRICT SCOPE CONTROL

This sprint must NOT:

- Redesign the level.
- Change enemy numbers.
- Nerf enemy AI.
- Reduce weapon damage.
- Remove combat effects.
- Remove ragdolls.
- Change player movement.
- Alter weapon balance.
- Simplify suppression.
- Redesign camera framing.
- Change the art style.
- Modify shop economy.
- Redesign the HUD.
- Introduce a completely new architecture without good reason.

Avoid speculative, project-wide rewrites.

Prefer targeted changes with measurable benefits.

Do not sacrifice maintainability for tiny theoretical improvements.

Any optimization affecting gameplay behavior requires regression testing.

---

## 15. FINAL DELIVERABLES

Implement the optimizations directly in the existing Unity project.

After completing the sprint, provide:

**A. Performance diagnosis**
- Biggest measured bottlenecks.
- Initial profiling results.
- Why those bottlenecks occurred.

**B. Implementation**
- Modified scripts and assets.
- Optimizations implemented.
- Explanation of major architectural changes.

**C. Benchmark comparison**

Provide a table containing:

| Metric | Before | After |
|---|---|---|
| Average FPS | Measured | Measured |
| 1% low FPS | Measured | Measured |
| 95th percentile frame time | Measured | Measured |
| 99th percentile frame time | Measured | Measured |
| Main-thread CPU time | Measured | Measured |
| GPU time | Measured if available | Measured if available |
| Managed allocations/frame | Measured | Measured |
| Memory usage | Measured | Measured |

Mark unavailable measurements as NOT TESTED. Never invent benchmark numbers.

**D. Regression testing**
- Gameplay systems verified.
- Remaining issues.
- Android-specific concerns.
- Any optimizations requiring further device validation.

**E. Further opportunities**
- List the next three highest-impact potential optimizations that were identified but not implemented, if any.
- Do not expand this sprint endlessly to pursue marginal gains.

### FINAL DESIGN PRINCIPLE

**Optimize wasted work, not gameplay complexity.**

AKUMU 2 should retain the exact qualities that make it fun: frantic combat, cowardly enemies, absurd physics, chunky visuals, and responsive movement.

We want the player to notice that everything runs smoothly — not notice which features were removed to achieve it.

**Implement, profile, verify, and report. Do not stop at theoretical suggestions.**