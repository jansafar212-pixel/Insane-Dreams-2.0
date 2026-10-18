#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Visuals;
using InsaneDreams.Input;
using InsaneDreams.Traversal;
using InsaneDreams.Inventory;
using InsaneDreams.Combat;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint009TransitionTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private GameObject player;private PlayerActions actions;private Rigidbody2D body;private CharacterVisualDriver visual;private PlayerTraversal2D traversal;private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Floor(float y){var go=Keep(new GameObject("S009Floor"));go.transform.position=new Vector3(10000,y-.5f);go.AddComponent<BoxCollider2D>().size=new Vector2(40,1);}
        private void Air(float y){actions.ResetActions();traversal.ResetTraversal();body.position=new Vector2(10000,y);player.transform.position=new Vector3(10000,y);body.linearVelocity=Vector2.down*4;Physics2D.SyncTransforms();}
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;Floor(0);
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(10000,.85f),Quaternion.identity));
            actions=player.GetComponent<PlayerActions>();body=player.GetComponent<Rigidbody2D>();visual=player.GetComponent<CharacterVisualDriver>();traversal=player.GetComponent<PlayerTraversal2D>();yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup(){foreach(var o in objects)if(o!=null)Object.Destroy(o);objects.Clear();foreach(var d in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))d.Retire();yield return null;Application.runInBackground=background;}
        private void Ladder(){var go=Keep(new GameObject("S009Ladder"));go.transform.position=new Vector3(10000,4);go.AddComponent<BoxCollider2D>().size=new Vector2(1.8f,12);go.GetComponent<BoxCollider2D>().isTrigger=true;go.AddComponent<LadderVolume>();Physics2D.SyncTransforms();}
        [UnityTest] public IEnumerator ClimbingCarriesSelectedEquipmentInOneHandAndHonorsEmptySlots()
        {
            Ladder();Air(4);yield return new WaitForSeconds(.35f);actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.2f);
            Assert.IsTrue(traversal.IsClimbing);var rig=visual.Rig;var loadout=player.GetComponent<PlayerLoadout>();
            foreach(var slot in new[]{EquipmentSlot.Firearm1,EquipmentSlot.Firearm2,EquipmentSlot.Melee})
            {
                loadout.Select(slot);yield return null;yield return null;Assert.IsTrue(rig.weapon.enabled);Assert.IsNotNull(rig.weapon.sprite);
                Assert.AreEqual(slot==EquipmentSlot.Melee?rig.skin.meleeWeapon:slot==EquipmentSlot.Firearm2?rig.skin.secondaryWeapon:rig.skin.weapon,rig.weapon.sprite); // Unity native identity survives asset-wrapper reloads.
                var hand=rig.rightArm.TransformPoint(Vector3.down*rig.skin.arm.bounds.size.y);Assert.Less(Vector3.Distance(hand,rig.weaponMount.position),.01f);
                Assert.Greater(Quaternion.Angle(rig.leftArm.localRotation,rig.rightArm.localRotation),10);
            }
            loadout.Unequip(EquipmentSlot.Melee);yield return null;yield return null;Assert.IsFalse(rig.weapon.enabled);Assert.IsTrue(traversal.IsClimbing);
        }
        [UnityTest] public IEnumerator LandingClosesStateCanopyAndUiAndHeldButtonCannotRedeployOnNextJump()
        {
            // Legacy name retained. Sprint 011 supersedes the auto-close requirement.
            var ui=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));ui.GetComponent<WarehouseTouchControls>().Bind(actions);yield return null;
            Air(1.8f);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);yield return new WaitForSeconds(.12f);Assert.IsTrue(traversal.IsParachuting);Assert.IsTrue(visual.Rig.parachute.enabled);
            yield return new WaitForSeconds(.6f);Assert.IsTrue(traversal.IsParachuting);Assert.IsTrue(visual.Rig.parachute.enabled);Assert.AreEqual(1,traversal.HorizontalSpeedMultiplier);Assert.AreEqual(3,body.gravityScale);
            StringAssert.Contains("RETRACT",ui.GetComponent<UIDocument>().rootVisualElement.Q<VisualElement>("parachuteButton").Q<Label>().text);
            actions.Jump();yield return new WaitForSeconds(.1f);actions.SetTouchParachute(true);yield return null;Assert.IsTrue(traversal.IsParachuting,"Held button must not issue a second toggle");
            actions.SetTouchParachute(false);actions.SetTouchParachute(true);yield return null;Assert.IsFalse(traversal.IsParachuting);
        }
        [UnityTest] public IEnumerator ElevatedLandingLadderAndRecoveryKeepTheSameParachuteAuthority()
        {
            Floor(5);Air(7);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);actions.SetTouchParachute(false);yield return new WaitForSeconds(.65f);
            Assert.IsTrue(traversal.IsParachuting);Assert.IsTrue(visual.Rig.parachute.enabled);Assert.Greater(body.position.y,5);Assert.AreEqual(3,body.gravityScale);
            Ladder();Air(8);yield return new WaitForFixedUpdate();actions.SetTouchParachute(true);actions.SetTouchParachute(false);yield return new WaitForSeconds(.35f);actions.SetTouchMove(Vector2.up);yield return new WaitForSeconds(.15f);
            Assert.IsTrue(traversal.IsClimbing);Assert.IsTrue(traversal.IsParachuting);Assert.IsTrue(visual.Rig.weapon.enabled);
            player.GetComponent<InsaneDreams.Checkpoints.PlayerRecovery>().Respawn();yield return null;yield return null;Assert.IsFalse(traversal.IsParachuting);Assert.AreEqual(3,body.gravityScale);
        }
        [UnityTest] public IEnumerator ProportionalRagdollCopiesPoseScaleAndRetainsPostDefeatImpacts()
        {
            var actor=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(10005,1),Quaternion.identity));yield return null;
            actor.GetComponent<CharacterVisualDriver>().enabled=false;var rig=actor.GetComponentInChildren<CutoutRig>();rig.Pose(CharacterPose.Jump,0,Vector2.up,-1,0,FaceExpression.Angry);
            var originals=rig.BodyParts;var point=rig.head.position;var scale=rig.head.lossyScale;
            actor.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));var d=actor.GetComponent<RagdollOnDefeat>().LastRagdoll;
            Assert.That(Vector3.Distance(point,d.parts[1].position),Is.LessThan(.001f));Assert.AreEqual(Mathf.Abs(scale.y),d.parts[1].transform.lossyScale.y);
            for(int i=0;i<6;i++)Assert.Less(Vector3.Distance(originals[i].position,d.parts[i].position),.001f);
            foreach(var part in d.parts)Assert.Less(Vector2.Distance(part.transform.TransformPoint(part.GetComponent<Collider2D>().offset),part.worldCenterOfMass),.002f);
            d.ReceiveImpact(new PhysicalImpact(d.parts[1].position,Vector2.left,.9f,d.parts[1]));Assert.AreEqual(1,d.ImpactCount);yield return new WaitForSeconds(.3f);Assert.IsFalse(actor.activeSelf);
        }
    }
}
#endif
