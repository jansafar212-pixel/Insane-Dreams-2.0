using UnityEngine;

namespace InsaneDreams.Inventory
{
    public enum WeaponCategory { Firearm, Melee }
    [CreateAssetMenu(menuName = "Insane Dreams/Weapon Definition")]
    public sealed class WeaponDefinition : ItemDefinition
    {
        [SerializeField] private WeaponCategory weaponCategory;
        [SerializeField, Min(0)] private float damage = 10;
        [SerializeField, Min(0.01f)] private float firingInterval = 0.2f;
        [SerializeField, Min(1)] private int magazineCapacity = 12;
        [SerializeField, Min(0)] private float reloadDuration = 1.5f;
        [SerializeField] private GameObject presentationPrefab;
        [SerializeField] private Sprite equippedSprite, magazineSprite;
        public Sprite EquippedSprite => equippedSprite;
        public Sprite MagazineSprite => magazineSprite;
        [SerializeField, Min(.1f)] private float firearmRange = 22;
        [SerializeField, Min(.1f)] private float meleeRange = 1.6f;
        [SerializeField, Min(0)] private float impactImpulse = .9f;
        public float ImpactImpulse => impactImpulse;
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
