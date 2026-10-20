using InsaneDreams.Enemies;
using UnityEngine;

namespace InsaneDreams.Combat
{
    // Observes combat outcomes only. Never applies damage, consumes ammunition, or changes attack timing.
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(Health), typeof(AttackExecutor), typeof(AudioSource))]
    public sealed class CombatFeedback : MonoBehaviour
    {
        [SerializeField] private Sprite effectSprite;
        [SerializeField] private Sprite surfaceImpactSprite;
        [SerializeField] private AudioClip geometryImpactSound, ragdollImpactSound;
        [SerializeField] private AudioClip concreteImpactSound, metalImpactSound, woodImpactSound;
        [SerializeField, Range(4,24)] private int maximumImpactParticles=12;
        [SerializeField, Range(.08f,.5f)] private float particleSeconds=.24f;
        [SerializeField, Range(0,.15f)] private float pitchVariation=.07f;
        [SerializeField] private bool muzzleGlowEnabled=true;
        private SpriteRenderer glow;
        private SpriteRenderer[] particles;
        private Vector2[] particlePositions,particleVelocities;
        private float[] particleExpires;
        private Color[] particleColors;
        private int nextParticle;
        private readonly AudioSource[] voices=new AudioSource[2];
        private readonly bool[] voiceReserved=new bool[2];
        private static int activeVoices;
        public const int GlobalVoiceLimit=12;
        public int ImpactParticleCapacity => particles!=null?particles.Length:0;
        public ImpactMaterial LastImpactMaterial {get;private set;}
        public static int ActiveAudioVoices => activeVoices;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetVoices()=>activeVoices=0;
        [SerializeField, Min(.02f)] private float muzzleSeconds = .06f;
        [SerializeField, Min(.02f)] private float hitSeconds = .12f;
        [SerializeField, Min(.02f)] private float hurtSeconds = .15f;
        [SerializeField] private Color hurtColor = new Color(1, .25f, .2f);
        [SerializeField] private AudioClip firingSound, hitSound, hurtSound, reloadSound, emptySound, defeatSound;
        [SerializeField, Range(0, 1)] private float volume = .5f;
        private Health health;
        private AttackExecutor executor;
        private PlayerCombat player;
        private EnemyCombat enemy;
        private AudioSource audioSource;
        private SpriteRenderer[] renderers;
        private Color[] colors;
        private SpriteRenderer muzzle, hit;
        private SpriteRenderer surface;
        private InsaneDreams.Visuals.CutoutRig rig;
        private float surfaceUntil;
        private Color surfaceColor;
        private Vector2 surfacePoint;
        private float muzzleUntil, hitUntil, tintUntil;
        private bool tinted;
        private Vector2 hitPoint;
        public int ShotCues { get; private set; }
        public int HitCues { get; private set; }
        public int HurtCues { get; private set; }
        public int ReloadCues { get; private set; }
        public int EmptyCues { get; private set; }
        public int DefeatCues { get; private set; }
        private void Awake()
        {
            health = GetComponent<Health>(); executor = GetComponent<AttackExecutor>();
            player = GetComponent<PlayerCombat>(); enemy = GetComponent<EnemyCombat>(); audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false; audioSource.spatialBlend = 0;
            renderers = GetComponentsInChildren<SpriteRenderer>(true); colors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) colors[i] = renderers[i].color;
            muzzle = Effect("MuzzleCue", new Color(1, .85f, .25f)); hit = Effect("HitConfirmation", Color.white);
            surface = Effect("SurfaceImpact", Color.white); if (surfaceImpactSprite != null) surface.sprite = surfaceImpactSprite;
            glow=Effect("MuzzleGlow",new Color(1,.75f,.2f,.18f));
            int capacity=Mathf.Clamp(maximumImpactParticles,4,24);
            particles=new SpriteRenderer[capacity];particlePositions=new Vector2[capacity];particleVelocities=new Vector2[capacity];particleExpires=new float[capacity];particleColors=new Color[capacity];
            for(int i=0;i<capacity;i++)particles[i]=Effect("ImpactParticle_"+i,Color.white);
            for(int i=0;i<voices.Length;i++)
            {
                var go=new GameObject("CombatVoice_"+i);go.transform.SetParent(transform,false);voices[i]=go.AddComponent<AudioSource>();
                voices[i].playOnAwake=false;voices[i].spatialBlend=0;voices[i].outputAudioMixerGroup=audioSource.outputAudioMixerGroup;
            }
            rig = GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>(true);
        }
        private SpriteRenderer Effect(string name, Color color)
        {
            var go = new GameObject(name); go.transform.SetParent(transform, false);
            var sprite = go.AddComponent<SpriteRenderer>(); sprite.sprite = effectSprite; sprite.color = color;
            sprite.sortingOrder = 30; sprite.enabled = false; return sprite;
        }
        private void OnEnable()
        {
            executor.Fired += Fired; executor.EmptyAttempt += Empty; health.Damaged += Hurt; health.Defeated += Defeat;
            executor.SurfaceHit += Surface;
            if (player != null) player.ReloadStarted += Reload;
            if (enemy != null) enemy.ReloadStarted += Reload;
        }
        private void OnDisable()
        {
            executor.Fired -= Fired; executor.EmptyAttempt -= Empty; health.Damaged -= Hurt; health.Defeated -= Defeat;
            executor.SurfaceHit -= Surface;
            if (player != null) player.ReloadStarted -= Reload;
            if (enemy != null) enemy.ReloadStarted -= Reload;
            RestoreTint(); muzzle.enabled = hit.enabled = surface.enabled = false;
            glow.enabled=false;foreach(var p in particles)p.enabled=false;
            for(int i=0;i<voices.Length;i++){voices[i].Stop();ReleaseVoice(i);}
        }
        private void ReleaseVoice(int i){if(!voiceReserved[i])return;voiceReserved[i]=false;activeVoices=Mathf.Max(0,activeVoices-1);}
        private void Sound(AudioClip clip,bool vary=true)
        {
            if(clip==null||!isActiveAndEnabled)return;
            for(int i=0;i<voices.Length;i++)
            {
                if(voices[i].isPlaying)continue;ReleaseVoice(i);if(activeVoices>=GlobalVoiceLimit)return;
                voices[i].clip=clip;voices[i].volume=volume;voices[i].pitch=vary?Random.Range(1-pitchVariation,1+pitchVariation):1;
                voices[i].Play();voiceReserved[i]=true;activeVoices++;return;
            }
        }
        private void Fired(Vector2 direction, Vector2 point, bool damaged)
        {
            ShotCues++; Sound(firingSound); muzzle.enabled = true; muzzleUntil = Time.time + muzzleSeconds;
            muzzle.transform.position = (Vector3)executor.LastShotOrigin + Vector3.back * .15f;
            float variation=Random.Range(.88f,1.12f);muzzle.transform.localScale = new Vector3(.28f*variation, .14f*variation, 1);
            muzzle.color=Color.Lerp(new Color(1,.65f,.1f),Color.white,Random.Range(.15f,.5f));
            glow.enabled=muzzleGlowEnabled;glow.transform.position=muzzle.transform.position;glow.transform.localScale=new Vector3(.45f,.3f,1);
            muzzle.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            if (!damaged) return;
            HitCues++; Sound(hitSound); hit.enabled = true; hitUntil = Time.time + hitSeconds;
            hitPoint = point; hit.transform.position = new Vector3(point.x, point.y, -.2f);
            hit.transform.localScale = new Vector3(.18f, .18f, 1); hit.transform.rotation = Quaternion.Euler(0, 0, 45);
        }
        private void Surface(Vector2 point, ShotSurface kind)
        {
            if (kind != ShotSurface.Geometry && kind != ShotSurface.Ragdoll) return;
            surfacePoint = point; surface.enabled = true; surfaceUntil = Time.time + hitSeconds;
            LastImpactMaterial=executor.LastImpactMaterial;
            Color color=LastImpactMaterial switch {ImpactMaterial.Concrete=>new Color(.65f,.68f,.7f),ImpactMaterial.Metal=>new Color(1,.9f,.35f),ImpactMaterial.Wood=>new Color(.8f,.48f,.2f),_=>new Color(.65f,.85f,1)};
            surface.color=surfaceColor=kind==ShotSurface.Ragdoll?new Color(.5f,.95f,1):color;
            surface.transform.localScale = Vector3.one * (surfaceImpactSprite != null ? 1 : .18f);
            for(int i=0;i<3;i++)
            {
                int n=nextParticle;nextParticle=(n+1)%particles.Length;var p=particles[n];p.enabled=true;p.color=particleColors[n]=color;
                particlePositions[n]=point;particleVelocities[n]=Quaternion.Euler(0,0,Random.Range(-65f,65f))*executor.LastHitNormal*Random.Range(.6f,2.5f);
                particleExpires[n]=Time.time+particleSeconds*Random.Range(.75f,1.2f);
                float length=LastImpactMaterial==ImpactMaterial.Metal?(Random.value<.2f?.3f:.13f):LastImpactMaterial==ImpactMaterial.Wood?.12f:.07f;
                p.transform.localScale=new Vector3(length,.04f,1);p.transform.rotation=Quaternion.Euler(0,0,Mathf.Atan2(particleVelocities[n].y,particleVelocities[n].x)*Mathf.Rad2Deg);
                p.transform.position=new Vector3(point.x,point.y,-.22f);
            }
            AudioClip clip=LastImpactMaterial switch{ImpactMaterial.Concrete=>concreteImpactSound,ImpactMaterial.Metal=>metalImpactSound,ImpactMaterial.Wood=>woodImpactSound,_=>geometryImpactSound};
            Sound(kind==ShotSurface.Ragdoll?ragdollImpactSound:clip!=null?clip:geometryImpactSound);
        }
        private void Hurt(DamageInfo _) { HurtCues++; Tint(hurtColor, hurtSeconds); Sound(hurtSound); }
        private void Reload() { ReloadCues++; Tint(new Color(1, .8f, .2f), .25f); Sound(reloadSound,false); }
        private void Empty() { EmptyCues++; Tint(Color.gray, .12f); Sound(emptySound); }
        private void Defeat(DamageInfo _)
        {
            DefeatCues++;
            // A short, detached silhouette survives immediate authoritative enemy deactivation.
            if (health.Team != CombatTeam.Enemy || effectSprite == null) return;
            if (GetComponent<InsaneDreams.Visuals.RagdollOnDefeat>() != null)
            {
                if (defeatSound != null)
                {
                    var cue = new GameObject("DefeatSound"); cue.transform.position = transform.position;
                    cue.AddComponent<AudioSource>().PlayOneShot(defeatSound, volume); Destroy(cue, defeatSound.length);
                }
                return;
            }
            var go = new GameObject("DefeatCue"); go.transform.position = transform.position;
            go.transform.localScale = new Vector3(.85f, .2f, 1);
            var sprite = go.AddComponent<SpriteRenderer>(); sprite.sprite = effectSprite;
            sprite.sortingOrder = 12; sprite.color = hurtColor;
            if (defeatSound != null) { var source = go.AddComponent<AudioSource>(); source.PlayOneShot(defeatSound, volume); }
            Destroy(go, defeatSound != null ? Mathf.Max(.25f, defeatSound.length) : .25f);
        }
        private void Tint(Color color, float seconds)
        {
            tintUntil = Time.time + seconds;
            tinted = true;
            for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].color = color;
        }
        private void RestoreTint() { if (!tinted) return; tinted = false; for (int i = 0; i < renderers.Length; i++) if (renderers[i] != null) renderers[i].color = colors[i]; }
        private void Update()
        {
            if (Time.time >= muzzleUntil) muzzle.enabled = false;
            if(!muzzle.enabled)glow.enabled=false;
            for(int i=0;i<voices.Length;i++)if(voiceReserved[i]&&!voices[i].isPlaying)ReleaseVoice(i);
            for(int i=0;i<particles.Length;i++)
            {
                var p=particles[i];if(!p.enabled)continue;float remaining=particleExpires[i]-Time.time;if(remaining<=0){p.enabled=false;continue;}
                particleVelocities[i]+=Vector2.down*(4*Time.deltaTime);particlePositions[i]+=particleVelocities[i]*Time.deltaTime;
                p.transform.position=new Vector3(particlePositions[i].x,particlePositions[i].y,-.22f);Color c=particleColors[i];c.a=Mathf.Clamp01(remaining/particleSeconds);p.color=c;
            }
            if (Time.time >= hitUntil) hit.enabled = false;
            else hit.transform.position = new Vector3(hitPoint.x, hitPoint.y, -.2f);
            if (Time.time >= tintUntil) RestoreTint();
            if (Time.time >= surfaceUntil) surface.enabled = false;
            else
            {
                surface.transform.position = new Vector3(surfacePoint.x, surfacePoint.y, -.2f);
                var color=surfaceColor;color.a*=Mathf.Clamp01((surfaceUntil-Time.time)/Mathf.Max(.02f,hitSeconds));surface.color=color;
            }
        }
        private void LateUpdate()
        {
            // The cutout driver (order 100) has now applied this frame's aim, crouch and recoil.
            if (muzzle.enabled && rig != null && rig.weapon.enabled && rig.weapon.sprite != null)
                muzzle.transform.position = (Vector3)rig.MuzzlePosition + Vector3.back * .15f;
            if(glow.enabled)glow.transform.position=muzzle.transform.position;
        }
    }
}
