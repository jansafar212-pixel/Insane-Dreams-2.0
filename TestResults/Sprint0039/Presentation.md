# Sprint0039 combat presentation implementation

Root causes:

- Larper and Officer native CombatFeedback serialized hurtColor is peach
  (1,.75,.6), overriding the red C# default. Samurai/fireworks inherit the base
  feedback component. ApplyPrefabs/ApplyScene explicitly restore red for enemies.
- Reload/empty feedback previously replaced a damage tint immediately; later
  weapon presentation could also reset weapon tint. Damage now has priority for
  its existing short duration, and LateUpdate reapplies that active color after
  pose scripts. Rapid hits extend a single continuous tint rather than blinking
  the actor between hits. Starting colors are sampled when a new tint begins,
  after child skin Awake, and restored on expiry/disable.
- FirstOperationSetup.Solid assigned Concrete to every solid, including visible
  wood crates, tin/rust crates and metal walkways. Native scene helper classifies
  these existing sprite modules; no geometry is changed.

Tracers use the original hitscan/raycast and same bounded renderer pool. A shared
single-pass URP unlit shader has a pale central band and a restrained colored
fringe; existing native width, weapon multiplier, lifetime, vertex alpha tail
and fade are preserved. No additional renderer, particle, physical projectile or
dynamic light is introduced. The original template material is retained as
AimMaterial so sniper aim and Officer telegraph lines keep their original
appearance; caller PlayerCombat must use AimMaterial.

Known concrete/metal/wood geometry cue scale increases25%. Same three particles
per hit / existing12-slot capacity / .24s lifetime: concrete uses larger square
dust chips and lower speed, metal longer thin yellow sparks, wood short brown
splinters. Generic/corpse cue scale remains unchanged. Character bullet cue stays
Sprint0038's .216. Tracer positions and hit calculations are unchanged.

Native authoring sequence (root owns scene saves):

1. Compile/import shader and source.
2. CombatPresentation0039Setup.ApplyPrefabs().
3. CombatPresentation0039Setup.ApplyScene(scene) in each centrally managed scene
   pass, then root saves; no automatic save inside this method.
4. Run CombatPresentation0039Tests Play Mode (4 tests), existing Sprint013Tests,
   Sprint021Tests and CombatImpact0037 tracer tests as appropriate.

The specialist did not execute Unity compilation/native mutations/tests to avoid
concurrent changes. Final validation results belong in the central sprint report.
Controlled visual/native material verification and real Android playtesting are
separate; no device performance claim is made here.
