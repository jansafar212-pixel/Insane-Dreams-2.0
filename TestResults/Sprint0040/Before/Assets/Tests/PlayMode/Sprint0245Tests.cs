#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0245Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private BoxCollider2D Solid(Vector2 p,Vector2 size)
        {var g=Keep(new GameObject("S0245Solid"));g.transform.position=p;var c=g.AddComponent<BoxCollider2D>();c.size=size;return c;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;Solid(new Vector2(45000,-.5f),new Vector2(40,1));yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;}
        private GameObject Actor(string name="WarehousePlayer")
        {var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/"+(name=="WarehousePlayer"?"Player/":"Enemies/")+name+".prefab"),new Vector3(45000,.85f),Quaternion.identity));var a=g.GetComponent<PlayerActions>();if(a!=null){var s=new SerializedObject(a);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();}if(g.TryGetComponent<InsaneDreams.Enemies.EnemyCombat>(out var e)){e.enabled=false;g.GetComponent<InsaneDreams.Enemies.LarperBrain>().enabled=false;}return g;}
        private IEnumerator CoverBesideLadder(float side,bool top)
        {
            var g=Actor();var a=g.GetComponent<PlayerActions>();a.SetTouchAim(Vector2.right*side);Solid(new Vector2(45000+side*1.1f,.45f),new Vector2(1,.9f));
            var l=Keep(new GameObject("S0245Ladder"));l.transform.position=new Vector2(45000+side*.2f,top?0:5);var c=l.AddComponent<BoxCollider2D>();c.isTrigger=true;c.size=new Vector2(1.8f,top?4:16);l.AddComponent<LadderVolume>();Physics2D.SyncTransforms();yield return new WaitForSeconds(.35f);
            a.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.5f);Assert.IsTrue(g.GetComponent<PlayerCover2D>().IsCovered);Assert.IsFalse(g.GetComponent<PlayerTraversal2D>().IsClimbing);Assert.Less(Mathf.Abs(g.transform.position.x-45000),.1f);
            a.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.2f);Assert.IsFalse(g.GetComponent<PlayerCover2D>().IsCovered);
            a.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.3f);Assert.IsTrue(g.GetComponent<PlayerTraversal2D>().IsClimbing);
            a.Jump();yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.IsFalse(g.GetComponent<PlayerTraversal2D>().IsClimbing);Assert.AreEqual(3,g.GetComponent<Rigidbody2D>().gravityScale);Assert.AreEqual(1,g.GetComponent<PlayerMotor2D>().JumpsUsed);
        }
        [UnityTest]public IEnumerator DownwardCoverBeatsMidLadderCaptureOnLeft(){yield return CoverBesideLadder(-1,false);}
        [UnityTest]public IEnumerator DownwardCoverBeatsLadderTopEntryOnRight(){yield return CoverBesideLadder(1,true);}
        [UnityTest]public IEnumerator UnobstructedLadderTopStillAllowsIntentionalDescent()
        {Object.Destroy(owned[0]);var g=Actor();var l=Keep(new GameObject("TopLadder"));l.transform.position=new Vector2(45000,0);var c=l.AddComponent<BoxCollider2D>();c.isTrigger=true;c.size=new Vector2(1.8f,4);l.AddComponent<LadderVolume>();yield return new WaitForSeconds(.35f);var body=g.GetComponent<Rigidbody2D>();body.position=new Vector2(45000,1.5f);body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();g.GetComponent<PlayerActions>().SetTouchMove(Vector2.down);yield return new WaitForSeconds(.15f);Assert.IsTrue(g.GetComponent<PlayerTraversal2D>().IsClimbing);}
        [UnityTest]public IEnumerator SegmentedPistolStaysInHandAcrossMirroredPosesAndReload()
        {
            var g=Actor("Larper");yield return null;g.GetComponent<CharacterVisualDriver>().enabled=false;var r=g.GetComponent<CharacterVisualDriver>().Rig;
            foreach(float f in new[]{-1f,1f})foreach(var p in new[]{CharacterPose.Idle,CharacterPose.Walk,CharacterPose.Jump,CharacterPose.Crouch,CharacterPose.Climb,CharacterPose.Panic})foreach(var aim in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down})foreach(bool reload in new[]{false,true})
            {r.Pose(p,1,aim,f,0,FaceExpression.Scared,true,true,reload,.5f);Assert.Less(Vector2.Distance(r.weaponMount.position,r.Segmented.grip.position),.001f,p.ToString());if(!reload&&r.weapon.enabled)Assert.Greater(Vector2.Dot(r.BarrelDirection,aim),.999f,p+" facing="+f+" aim="+aim);Assert.AreEqual(Vector3.one,r.rightArm.localScale);Assert.AreSame(r.Segmented.rightElbow,r.Segmented.rightHand.parent);Assert.AreSame(r.Segmented.leftKnee,r.Segmented.leftFoot.parent);}
            g.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));var doll=g.GetComponent<RagdollOnDefeat>().LastRagdoll;Keep(doll.gameObject);Assert.AreEqual(14,doll.Bodies.Length);yield return new WaitForSeconds(.4f);foreach(var b in doll.Bodies)Assert.LessOrEqual(b.linearVelocity.magnitude,12.1f);
        }
        [UnityTest]public IEnumerator ArmorTracksEquipmentDamageDepletionAndCopiesToCorpse()
        {
            var g=Actor("Larper");var armor=g.AddComponent<BodyArmor>();var visual=g.AddComponent<ArmorVisual>();var r=g.GetComponent<CharacterVisualDriver>().Rig;
            var go=new GameObject("ArmorVest");go.transform.SetParent(r.torso,false);visual.overlay=go.AddComponent<SpriteRenderer>();visual.overlay.sortingOrder=11;visual.overlay.sharedMaterial=r.weapon.sharedMaterial;
            visual.intact=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0245/Armor_0.asset");visual.damaged=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0245/Armor_1.asset");visual.broken=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprint0245/Armor_2.asset");visual.Refresh();Assert.IsFalse(visual.overlay.enabled);
            armor.Restore(60);Assert.IsTrue(visual.overlay.enabled);Assert.AreSame(visual.intact,visual.overlay.sprite);float remaining=armor.Absorb(new DamageInfo(60,CombatTeam.Player));Assert.AreEqual(21,remaining,.001f);Assert.AreSame(visual.damaged,visual.overlay.sprite);
            armor.Absorb(new DamageInfo(100,CombatTeam.Player));Assert.IsFalse(visual.overlay.enabled);Assert.IsNull(visual.overlay.sprite);Assert.AreEqual(0,armor.Condition);
            armor.Restore(60);Assert.IsTrue(visual.overlay.enabled);Assert.AreSame(visual.intact,visual.overlay.sprite);
            armor.enabled=false;Assert.IsFalse(visual.overlay.enabled);armor.enabled=true;Assert.IsTrue(visual.overlay.enabled);
            armor.Restore(0);Assert.IsFalse(visual.overlay.enabled);Assert.IsNull(visual.overlay.sprite);
            yield return null;g.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));var doll=g.GetComponent<RagdollOnDefeat>().LastRagdoll;Keep(doll.gameObject);Assert.IsNull(doll.parts[0].transform.Find("ArmorVest"));yield return null;
        }
        [UnityTest]public IEnumerator SegmentedPistolRetractionKeepsMuzzleOnNearSideOfSolidCover()
        {
            var g=Actor("Larper");Solid(new Vector2(45001,1),new Vector2(1,2));yield return null;
            g.GetComponent<CharacterVisualDriver>().enabled=false;var r=g.GetComponent<CharacterVisualDriver>().Rig;
            foreach(var p in new[]{CharacterPose.Idle,CharacterPose.Crouch})
            {r.Pose(p,0,Vector2.right,1,0,FaceExpression.Neutral,true,true);g.GetComponent<AttackExecutor>().ConstrainMuzzle();Assert.LessOrEqual(r.MuzzlePosition.x,45000.5f,"Held pistol must not protrude through solid cover: "+p);Assert.Less(Vector2.Distance(r.Segmented.grip.position,r.weaponMount.position),.001f);}
        }
    }
}
#endif
