using UnityEngine;

namespace InsaneDreams.Inventory
{
    public enum WeaponCategory { Firearm, Melee }
    public enum WeaponFireMode { Automatic, SemiAutomatic, ReleaseToFire }
    public enum FirearmRole { Primary, Secondary }
    [CreateAssetMenu(menuName = "Insane Dreams/Weapon Definition")]
    public sealed class WeaponDefinition : ItemDefinition
    {
        [SerializeField] private WeaponCategory weaponCategory;
        [SerializeField] private FirearmRole firearmRole;
        public FirearmRole FirearmRole => firearmRole;
        public bool Fits(EquipmentSlot slot)=>weaponCategory==WeaponCategory.Melee?slot==EquipmentSlot.Melee:
            firearmRole==FirearmRole.Primary?slot==EquipmentSlot.Firearm1:slot==EquipmentSlot.Firearm2||slot==EquipmentSlot.Firearm3;
        [SerializeField, Min(0)] private float damage = 10;
        [SerializeField, Min(0.01f)] private float firingInterval = 0.2f;
        [SerializeField, Min(1)] private int magazineCapacity = 12;
        [SerializeField, Min(0)] private float reloadDuration = 1.5f;
        [SerializeField] private GameObject presentationPrefab;
        [SerializeField] private Sprite equippedSprite, magazineSprite;
        [SerializeField] private Vector2 magazineMountOffset=new Vector2(.125f,-.0625f);
        public Vector2 MagazineMountOffset=>magazineMountOffset;
        public Sprite EquippedSprite => equippedSprite;
        public Sprite MagazineSprite => magazineSprite;
        [SerializeField] private bool physicalMagazineReload;
        public bool PhysicalMagazineReload=>physicalMagazineReload;
        [SerializeField] private bool loadsIndividually;
        [SerializeField] private Sprite shellSprite;
        public bool LoadsIndividually=>loadsIndividually;
        public Sprite ShellSprite=>shellSprite;
        [SerializeField] private Vector2 ammunitionInsertionOffset=new Vector2(.3f,-.04f);
        public Vector2 AmmunitionInsertionOffset=>ammunitionInsertionOffset;
        public float ReloadStepDuration=>loadsIndividually?reloadDuration/Mathf.Max(1,MagazineCapacity):reloadDuration;
        [SerializeField, Min(.1f)] private float firearmRange = 22;
        [SerializeField, Min(.1f)] private float meleeRange = 1.6f;
        [SerializeField, Min(0)] private float impactImpulse = .9f;
        public float ImpactImpulse => impactImpulse;
        [SerializeField,Range(0,4),Tooltip("Living balance/pose response only; never changes damage or corpse impulse.")] private float hitReactionStrength=1;
        public float HitReactionStrength=>hitReactionStrength;
        [Header("Player arsenal (legacy defaults preserve existing weapons)")]
        [SerializeField,Min(0),Tooltip("Developer lab refill only; does not grant campaign ammunition.")] private int developerReserve=180;
        public int DeveloperReserve=>developerReserve;
        [SerializeField] private WeaponFireMode fireMode;
        [SerializeField,Range(1,12)] private int pellets=1;
        [SerializeField,Range(0,40)] private float spreadDegrees;
        [SerializeField,Range(0,12)] private float bloomPerShot,maximumBloom;
        [SerializeField,Min(0)] private float falloffStart;
        [SerializeField,Range(0,1)] private float farDamageMultiplier=1;
        [SerializeField,Range(.5f,4)] private float recoilScale=1,tracerScale=1,flashScale=1;
        [SerializeField] private bool customMuzzle,customTracer;
        [SerializeField] private Vector2 muzzleOffset;
        [SerializeField] private Color tracerColor=Color.yellow;
        [SerializeField] private AudioClip firingSound;
        [SerializeField,Range(.5f,2)] private float firingPitch=1;
        public WeaponFireMode FireMode=>fireMode;
        public int Pellets=>weaponCategory==WeaponCategory.Firearm?Mathf.Clamp(pellets,1,12):1;
        public float SpreadDegrees=>spreadDegrees;
        public float BloomPerShot=>bloomPerShot;
        public float MaximumBloom=>maximumBloom;
        public float RecoilScale=>recoilScale;
        public float TracerScale=>tracerScale;
        public float FlashScale=>flashScale;
        public bool CustomTracer=>customTracer;
        public Color TracerColor=>tracerColor;
        public bool CustomMuzzle=>customMuzzle;
        public Vector2 MuzzleOffset=>muzzleOffset;
        public AudioClip FiringSound=>firingSound;
        public float FiringPitch=>firingPitch;
        public float DamageAtDistance(float distance)=>damage*Mathf.Lerp(1,farDamageMultiplier,Mathf.InverseLerp(Mathf.Min(falloffStart,AttackRange-.01f),AttackRange,distance));
        public float AttackRange => weaponCategory == WeaponCategory.Firearm ? firearmRange : meleeRange;
        public WeaponCategory WeaponCategory => weaponCategory;
        public override ItemCategory Category => weaponCategory == WeaponCategory.Firearm ? ItemCategory.Firearm : ItemCategory.Melee;
        public float Damage => damage;
        public float FiringInterval => firingInterval;
        public int MagazineCapacity => weaponCategory == WeaponCategory.Firearm ? magazineCapacity : 0;
        public float ReloadDuration => reloadDuration;
        public GameObject PresentationPrefab => presentationPrefab;
    }
}
