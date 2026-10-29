using UnityEngine;
namespace InsaneDreams.Combat
{
    public enum ImpactMaterial { Generic, Concrete, Metal, Wood }
    [DisallowMultipleComponent]
    public sealed class ImpactSurface2D : MonoBehaviour
    {
        [SerializeField] private ImpactMaterial material;
        public ImpactMaterial Material => material;
        public void Configure(ImpactMaterial value) => material=value;
        public static ImpactMaterial Classify(Collider2D collider)
        {
            var surface=collider!=null?collider.GetComponentInParent<ImpactSurface2D>():null;
            return surface!=null?surface.Material:ImpactMaterial.Generic;
        }
    }
}
