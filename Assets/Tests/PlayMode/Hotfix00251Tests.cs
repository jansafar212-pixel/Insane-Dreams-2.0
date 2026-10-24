#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Hotfix00251Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;private const float X=5100;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private GameObject Solid(float x,float y,Vector2 size){var g=Keep(new GameObject("HotfixObstacle"));g.transform.position=new Vector2(X+x,y);g.AddComponent<BoxCollider2D>().size=size;return g;}
        private GameObject Actor(string path,float x=0){var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),new Vector3(X+x,.85f),Quaternion.identity));var brain=g.GetComponent<LarperBrain>();if(brain!=null)brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));return g;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;Solid(0,-.5f,new Vector2(60,1));yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;}
        [UnityTest]public IEnumerator LarperPhysicallyCrossesLowBoxWithoutLosingAirborneDrive()
        {
            Solid(2.5f,.45f,new Vector2(1,.9f));var g=Actor("Assets/Prefabs/Enemies/Larper.prefab");g.GetComponent<EnemyCombat>().enabled=false;
            var brain=g.GetComponent<LarperBrain>();yield return new WaitForSeconds(.2f);brain.SetPursuit(1);bool jumped=false;float until=Time.time+3;
            while(Time.time<until&&g.transform.position.x<X+4.5f){jumped|=!g.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().IsGrounded;yield return new WaitForFixedUpdate();}
            Assert.IsTrue(jumped);Assert.Greater(g.transform.position.x,X+4.5f,"A valid grounded hop must continue past the box");
        }
        [UnityTest]public IEnumerator ImpossibleWallRequestsBoundedReplanThenRecoversWhenRemoved()
        {
            var wall=Solid(1.8f,2,new Vector2(1,4));var g=Actor("Assets/Prefabs/Enemies/Larper.prefab");g.GetComponent<EnemyCombat>().enabled=false;var brain=g.GetComponent<LarperBrain>();int replans=0;brain.ReplanRequested+=()=>replans++;
            yield return new WaitForSeconds(.2f);brain.SetPursuit(1);yield return new WaitForSeconds(3);
            Assert.That(replans,Is.InRange(1,4));Assert.Less(g.transform.position.x,X+1.3f);Assert.AreEqual(0,g.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().JumpsUsed);
            Object.Destroy(wall);yield return new WaitForSeconds(1.5f);Assert.Greater(g.transform.position.x,X+2);
        }
        [UnityTest]public IEnumerator CeilingRejectsOrdinaryObstacleHopWithoutJumpSpam()
        {
            Solid(2,.45f,new Vector2(1,.9f));Solid(2,2.2f,new Vector2(5,.3f));var g=Actor("Assets/Prefabs/Enemies/Larper.prefab");g.GetComponent<EnemyCombat>().enabled=false;yield return new WaitForSeconds(.2f);g.GetComponent<LarperBrain>().SetPursuit(1);yield return new WaitForSeconds(3);
            Assert.Less(g.transform.position.x,X+1.5f);Assert.AreEqual(0,g.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>().JumpsUsed);
        }
        [Test]public void ReloadInsertionTransfersOnceAtOriginalDeadlineAndCancellationTransfersNothing()
        {
            var d=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");var w=new RuntimeWeapon(d);w.SetAmmunition(3,5);int inserted=0;w.Cycle.MagazineInserted+=()=>{inserted++;Assert.AreEqual(8,w.MagazineAmmo);};
            Assert.AreEqual(1.5f,d.ReloadDuration);Assert.IsTrue(d.PhysicalMagazineReload);Assert.IsTrue(w.Cycle.TryReload(10));w.Cycle.Tick(11.49f);Assert.AreEqual(3,w.MagazineAmmo);Assert.AreEqual(0,inserted);w.Cycle.Tick(11.5f);w.Cycle.Tick(20);Assert.AreEqual(1,inserted);Assert.AreEqual(0,w.ReserveAmmo);
            w.SetAmmunition(3,5);Assert.IsTrue(w.Cycle.TryReload(30));w.Cycle.CancelReload();w.Cycle.Tick(40);Assert.AreEqual(1,inserted);Assert.AreEqual(3,w.MagazineAmmo);Assert.AreEqual(5,w.ReserveAmmo);
            Assert.IsFalse(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/LarperBuzzbox.asset").PhysicalMagazineReload);
        }
        [UnityTest]public IEnumerator OldAndReplacementMagazinesMoveSeparatelyAndRestoreAfterInterruption()
        {
            var g=Actor("Assets/Prefabs/Player/WarehousePlayer.prefab");g.GetComponent<CharacterVisualDriver>().enabled=false;var rig=g.GetComponentInChildren<CutoutRig>();var d=AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");
            rig.SetMagazine(d.MagazineSprite,d.MagazineMountOffset,true);
            void Pose(float p,bool reload=true){rig.Pose(CharacterPose.Idle,0,Vector2.right,1,0,FaceExpression.Neutral,false,true,reload,p);}
            Pose(.1f);Vector3 seated=rig.magazine.transform.localPosition;Pose(.4f);Assert.Greater(Vector3.Distance(seated,rig.magazine.transform.localPosition),.15f);
            Pose(.65f);var fresh=rig.weapon.transform.Find("ReplacementMagazine").GetComponent<SpriteRenderer>();Assert.IsFalse(rig.magazine.enabled);Assert.IsTrue(fresh.enabled);Vector3 outPosition=fresh.transform.localPosition;Pose(.88f);Assert.Greater(Vector3.Distance(outPosition,fresh.transform.localPosition),.15f);Assert.That(Vector2.Distance(fresh.transform.localPosition,d.MagazineMountOffset),Is.LessThan(.01f));
            Pose(0,false);Assert.IsTrue(rig.magazine.enabled);Assert.IsFalse(fresh.enabled);Assert.That(Vector2.Distance(rig.magazine.transform.localPosition,d.MagazineMountOffset),Is.LessThan(.01f));yield return null;
        }
    }
}
#endif
