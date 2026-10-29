#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using InsaneDreams.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class ReactiveRagdollTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private GameObject shooter, enemy;
        private AttackExecutor attack;
        private RuntimeWeapon weapon;
        private float shotTime;
        private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;
            var floor=Keep(new GameObject("ImpactFloor"));floor.transform.position=new Vector3(8003,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);
            shooter=Keep(new GameObject("ImpactShooter"));shooter.transform.position=new Vector3(8000,.9f);shooter.AddComponent<Health>();attack=shooter.AddComponent<AttackExecutor>();
            enemy=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(8003,.85f),Quaternion.identity));
            enemy.GetComponent<LarperBrain>().enabled=false;enemy.GetComponent<EnemyCombat>().enabled=false;
            weapon=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),36);shotTime=Time.time;
            yield return null;Physics2D.SyncTransforms();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var doll in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))doll.Retire();
            foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;
        }
        private bool Shoot(Vector2 point)
        {Physics2D.SyncTransforms();shotTime+=2;return attack.Attack(weapon,point-attack.Origin,shotTime);}
        private CartoonRagdoll Defeat()
        {
            for(int i=0;i<15&&!enemy.GetComponent<Health>().IsDefeated;i++)Assert.IsTrue(Shoot(enemy.transform.position));
            return enemy.GetComponent<RagdollOnDefeat>().LastRagdoll;
        }
        private static void StopBodies(CartoonRagdoll doll)
        {foreach(var body in doll.Bodies){body.linearVelocity=Vector2.zero;body.angularVelocity=0;body.Sleep();}}
        [UnityTest] public IEnumerator FatalRayTransfersHitDirectionAndRepeatedShotsReuseOneInactiveEnemyBody()
        {
            int defeats=0;enemy.GetComponent<Health>().Defeated+=_=>defeats++;
            var doll=Defeat();Assert.IsNotNull(doll);Assert.IsFalse(enemy.activeSelf);Assert.AreEqual(1,defeats);Assert.AreEqual(1,doll.ImpactCount);
            bool rightward=false;foreach(var b in doll.Bodies)rightward|=b.linearVelocity.x>2;Assert.IsTrue(rightward);
            for(int i=0;i<3;i++)Assert.IsTrue(Shoot(doll.parts[0].position));
            Assert.AreEqual(ShotSurface.Ragdoll,attack.LastSurface);Assert.AreEqual(4,doll.ImpactCount);
            Assert.AreEqual(1,defeats);Assert.AreEqual(1,enemy.GetComponent<RagdollOnDefeat>().SpawnCount);
            yield return new WaitForSeconds(.3f);Assert.IsFalse(enemy.activeSelf);
        }
        [UnityTest] public IEnumerator WallOccludesCorpseAndCorpseBlocksLivingTargetBehindIt()
        {
            var doll=Defeat();StopBodies(doll);
            var target=Keep(new GameObject("BehindCorpse"));target.transform.position=new Vector3(8006,.9f);target.AddComponent<BoxCollider2D>().size=new Vector2(.8f,2);var health=target.AddComponent<Health>();
            Assert.IsTrue(Shoot(doll.parts[0].position));Assert.AreEqual(ShotSurface.Ragdoll,attack.LastSurface);Assert.AreEqual(health.Maximum,health.Current);
            var wall=Keep(new GameObject("ImpactWall"));wall.transform.position=new Vector3(8001.5f,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,4);int count=doll.ImpactCount;
            Assert.IsTrue(Shoot(doll.parts[0].position));Assert.AreEqual(ShotSurface.Geometry,attack.LastSurface);Assert.AreEqual(count,doll.ImpactCount);Assert.AreEqual(health.Maximum,health.Current);
            yield return null;
        }
        [UnityTest] public IEnumerator RestingBodyWakesFromActualHitscanAndOppositeDirectionChangesMomentum()
        {
            var doll=Defeat();StopBodies(doll);
            var config=new SerializedObject(doll);config.FindProperty("restSeconds").floatValue=.1f;config.ApplyModifiedPropertiesWithoutUndo();
            yield return new WaitForSeconds(3); // Let contacts settle before testing the rest/wake policy.
            StopBodies(doll);yield return new WaitForSeconds(.15f);Assert.IsTrue(doll.IsResting);
            var head=doll.parts[1];shooter.transform.position=(Vector3)head.position+new Vector3(-3,0);Assert.IsTrue(Shoot(head.position));
            Assert.IsFalse(doll.IsResting);bool awake=false;foreach(var p in doll.Bodies)awake|=p.IsAwake();Assert.IsTrue(awake);
            StopBodies(doll);shooter.transform.position=(Vector3)head.position+new Vector3(3,0);Assert.IsTrue(Shoot(head.position));
            bool leftward=false;foreach(var p in doll.Bodies)leftward|=p.linearVelocity.x<-.1f;Assert.IsTrue(leftward);
        }
        [UnityTest] public IEnumerator OffCentreSegmentImpactProducesTorqueAndAbsurdImpulsesRemainBounded()
        {
            var doll=Defeat();StopBodies(doll);var part=doll.parts[0];var box=part.GetComponent<BoxCollider2D>();
            Vector2 point=box.bounds.center+new Vector3(-box.bounds.extents.x,box.bounds.extents.y*.8f);
            doll.ReceiveImpact(new PhysicalImpact(point,Vector2.right,10000,part));
            Assert.Greater(Mathf.Abs(part.angularVelocity),1);Assert.LessOrEqual(part.linearVelocity.magnitude,12.001f);Assert.LessOrEqual(Mathf.Abs(part.angularVelocity),720.01f);
            int count=doll.ImpactCount;doll.ReceiveImpact(new PhysicalImpact(point,Vector2.right,float.NaN));Assert.AreEqual(count,doll.ImpactCount);
            yield return new WaitForSeconds(2);
            foreach(var body in doll.Bodies){Assert.IsFalse(float.IsNaN(body.position.x));Assert.Greater(body.position.y,-.6f);Assert.LessOrEqual(body.linearVelocity.magnitude,12.2f);}
        }
        [UnityTest] public IEnumerator InteractionDoesNotExtendAbsoluteLifetimeOrDuplicateRegistryEntry()
        {
            var doll=Defeat();int count=CartoonRagdoll.ActiveCount;doll.Activate(enemy.GetComponent<CharacterVisualDriver>().Rig,Vector2.zero);Assert.AreEqual(count,CartoonRagdoll.ActiveCount);
            doll.Lifetime=.25f;doll.ReceiveImpact(new PhysicalImpact(doll.parts[0].position,Vector2.right,.5f,doll.parts[0]));
            yield return new WaitForSeconds(.3f);Assert.IsTrue(doll==null);Assert.AreEqual(1,enemy.GetComponent<RagdollOnDefeat>().SpawnCount);
        }
    }
}
#endif
