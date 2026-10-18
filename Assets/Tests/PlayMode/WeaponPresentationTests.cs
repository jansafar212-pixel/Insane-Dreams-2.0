#if UNITY_EDITOR
using System.Collections;
using InsaneDreams.Input;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class WeaponPresentationTests
    {
        [UnityTest] public IEnumerator BackwardAimKeepsWeaponUprightAndMuzzleAttachedWithoutChangingCollider()
        {
            bool background=Application.runInBackground;Application.runInBackground=true;
            var floor=new GameObject("WeaponPresentationFloor");floor.transform.position=new Vector3(9000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);
            var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(9000,.85f),Quaternion.identity);
            try
            {
                var actions=actor.GetComponent<PlayerActions>();var visual=actor.GetComponent<CharacterVisualDriver>();var box=actor.GetComponent<BoxCollider2D>();var size=box.size;
                yield return new WaitForSeconds(.25f);actions.SetTouchMove(Vector2.right);actions.SetTouchAim(Vector2.left);yield return new WaitForSeconds(.15f);
                var weapon=visual.Rig.weapon;Vector3 spriteUp=weapon.transform.TransformVector(Vector3.up)*(weapon.flipY?-1:1);
                Assert.Greater(spriteUp.y,.99f,"Backward aim must not turn the gun upside down");Assert.Greater(actor.GetComponent<Rigidbody2D>().linearVelocity.x,0);
                Assert.Less(visual.Rig.weaponMount.TransformVector(Vector3.right).x,0);Assert.AreEqual(size,box.size);
                actions.SetFireIntent(true);yield return null;yield return null;
                var muzzle=actor.transform.Find("MuzzleCue");Assert.IsTrue(muzzle.GetComponent<SpriteRenderer>().enabled);
                var tip=visual.Rig.muzzle.position;Assert.Less(Vector2.Distance(muzzle.position,tip),.005f);
            }
            finally{Object.Destroy(actor);Object.Destroy(floor);Application.runInBackground=background;}
            yield return null;
        }
    }
}
#endif
