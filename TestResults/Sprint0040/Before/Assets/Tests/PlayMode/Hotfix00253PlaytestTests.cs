#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;

namespace InsaneDreams.Tests
{
 public sealed class Hotfix00253PlaytestTests
 {
  const float X=7400;readonly List<GameObject> owned=new List<GameObject>();bool background;Random.State random;GameObject player;
  GameObject Keep(GameObject g){owned.Add(g);return g;}
  GameObject Solid(float x,float top,Vector2 size){var g=Keep(new GameObject("H253FollowupSolid"));g.transform.position=new Vector2(X+x,top-size.y*.5f);g.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return g;}
  T Asset<T>(string path)where T:Object=>AssetDatabase.LoadAssetAtPath<T>(path);
  [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(253);Solid(0,0,new Vector2(100,1));player=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.71f),Quaternion.identity));player.GetComponent<Health>().ProtectFor(120);var s=new SerializedObject(player.GetComponent<PlayerActions>());s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();yield return new WaitForSeconds(.2f);}
  [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;Random.state=random;}
  UIDocument Controls(string layout)
  {
   var parent=Keep(new GameObject("FollowupControls"));parent.SetActive(false);var ui=Object.Instantiate(Asset<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"),parent.transform);
   var doc=ui.GetComponent<UIDocument>();doc.visualTreeAsset=Asset<VisualTreeAsset>("Assets/UI/"+layout+".uxml");ui.GetComponent<WarehouseTouchControls>().Bind(player.GetComponent<PlayerActions>());parent.SetActive(true);return doc;
  }
  void Press(VisualElement button){using(var e=PointerDownEvent.GetPooled(new Event{type=EventType.MouseDown,button=0,mousePosition=button.worldBound.center})){e.target=button;button.SendEvent(e);}}
  [UnityTest]public IEnumerator BothHudLayoutsHideBlindFireWithoutUsableCrouchedCoverAndRemoveDuplicateFire()
  {
   var a=player.GetComponent<PlayerActions>();foreach(var layout in new[]{"WarehouseControls","MissionControls"})
   {
    var doc=Controls(layout);yield return new WaitForSeconds(.2f);var blind=doc.rootVisualElement.Q("blindFireButton");Assert.NotNull(blind,layout);Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);
    a.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.25f);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.IsFalse(player.GetComponent<PlayerCover2D>().IsCovered);Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);Assert.IsFalse(blind.enabledInHierarchy);
    Assert.IsNull(doc.rootVisualElement.Q("fireButton"));
    int shots=player.GetComponent<AttackExecutor>().ShotsFired;Press(blind);yield return null;Assert.IsFalse(a.FireHeld);Assert.AreEqual(shots,player.GetComponent<AttackExecutor>().ShotsFired);
    a.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.2f);Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);Object.Destroy(doc.transform.parent.gameObject);yield return null;
   }
  }
  [UnityTest]public IEnumerator StandingBesideCoverStaysHiddenThenCrouchedPressWorksAndExitReleasesPointer()
  {
   var crate=Solid(1.1f,.9f,new Vector2(1,.9f));var a=player.GetComponent<PlayerActions>();a.SetTouchAim(Vector2.right);var doc=Controls("MissionControls");yield return new WaitForSeconds(.25f);var blind=doc.rootVisualElement.Q("blindFireButton");Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);
   a.SetTouchMove(Vector2.down);yield return new WaitForSeconds(.3f);a.SetTouchAim(Vector2.zero);yield return null;Assert.IsTrue(player.GetComponent<PlayerCover2D>().IsCovered);Assert.AreEqual(DisplayStyle.Flex,blind.resolvedStyle.display);Assert.IsTrue(blind.enabledInHierarchy);Press(blind);yield return new WaitForSeconds(.15f);Assert.IsTrue(player.GetComponent<PlayerCover2D>().IsBlindFiring);Assert.Greater(player.GetComponent<AttackExecutor>().ShotsFired,0);
   Object.Destroy(crate);yield return new WaitForSeconds(.25f);Assert.IsTrue(player.GetComponent<PlayerPosture2D>().IsCrouched);Assert.IsFalse(player.GetComponent<PlayerCover2D>().IsCovered);Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);Assert.IsFalse(a.FireHeld);
   a.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.25f);Assert.AreEqual(DisplayStyle.None,blind.resolvedStyle.display);Assert.IsFalse(a.FireHeld);Assert.IsFalse(player.GetComponent<PlayerCover2D>().IsBlindFiring);
  }
  [UnityTest]public IEnumerator GlockFlashAndTrailsShareRecordedOriginAcrossFacingAimRecoilAndMovement()
  {
   var a=player.GetComponent<PlayerActions>();var l=player.GetComponent<PlayerLoadout>();l.Select(EquipmentSlot.Firearm2);yield return null;var attack=player.GetComponent<AttackExecutor>();var body=player.GetComponent<Rigidbody2D>();
   foreach(float facing in new[]{-1f,1f})foreach(var aim in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down,new Vector2(1,1).normalized,new Vector2(-1,-1).normalized})
   {
    a.SetTouchMove(Vector2.right*facing);a.SetTouchAim(aim);yield return new WaitForSeconds(.15f);l.SelectedWeapon.SetAmmunition(20,60);
    Assert.IsTrue(attack.Attack(l.SelectedWeapon,aim,Time.time));var origin=attack.LastShotOrigin;var flash=player.transform.Find("MuzzleCue");Assert.Less(Vector2.Distance(origin,player.GetComponent<CharacterVisualDriver>().Rig.MuzzlePosition),.002f);
    // Force the same next-frame recoil/translation which separated the old flash and line.
    player.GetComponent<CharacterVisualDriver>().PrepareShot(aim);body.position+=Vector2.right*facing*.05f;yield return new WaitForEndOfFrame();
    Assert.Less(Vector2.Distance(flash.position,origin),.002f);Assert.Greater(Vector2.Dot(flash.right,aim),.9999f);Assert.IsTrue(player.GetComponentsInChildren<LineRenderer>().Any(t=>t.enabled&&Vector2.Distance(t.GetPosition(0),flash.position)<.002f&&Vector2.Distance(t.GetPosition(1),attack.LastShotEnd)<.002f));
    yield return new WaitForSeconds(.2f);
   }
  }
  [UnityTest]public IEnumerator CarbineRetainsLegacyFollowingFlashAndEnemyGlockUsesShotAnchor()
  {
   var l=player.GetComponent<PlayerLoadout>();l.Select(EquipmentSlot.Firearm1);var a=player.GetComponent<PlayerActions>();a.SetTouchAim(Vector2.up);yield return null;Assert.IsTrue(player.GetComponent<AttackExecutor>().Attack(l.SelectedWeapon,Vector2.up,Time.time));player.GetComponent<CharacterVisualDriver>().PrepareShot(Vector2.up);yield return new WaitForEndOfFrame();Assert.Less(Vector2.Distance(player.transform.Find("MuzzleCue").position,player.GetComponent<CharacterVisualDriver>().Rig.MuzzlePosition),.005f);
   var enemy=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X+8,.71f),Quaternion.identity));var combat=enemy.GetComponent<EnemyCombat>();combat.enabled=false;enemy.GetComponent<LarperBrain>().enabled=false;yield return new WaitForSeconds(.2f);var attack=enemy.GetComponent<AttackExecutor>();Assert.IsTrue(attack.Attack(combat.Weapon,Vector2.left,Time.time));var origin=attack.LastShotOrigin;enemy.GetComponent<Rigidbody2D>().position+=Vector2.right*.1f;yield return new WaitForEndOfFrame();Assert.Less(Vector2.Distance(enemy.transform.Find("MuzzleCue").position,origin),.002f);
  }
  GameObject Samurai(float x)
  {
   var g=Keep(Object.Instantiate(Asset<GameObject>("Assets/Prefabs/Enemies/Samurai.prefab"),new Vector3(X+x,.53f),Quaternion.identity));g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;return g;
  }
  [UnityTest]public IEnumerator SamuraiOnlyJumpRiseIncreasesByTwentyFiveToThirtyPercentAndLands()
  {
   var old=Samurai(-10);var revised=Samurai(10);var baseline=new SerializedObject(old.GetComponent<PlayerMotor2D>());baseline.FindProperty("jumpSpeed").floatValue=13.28f;baseline.ApplyModifiedPropertiesWithoutUndo();yield return new WaitForSeconds(.2f);
   var om=old.GetComponent<PlayerMotor2D>();var nm=revised.GetComponent<PlayerMotor2D>();Assert.AreEqual(15,nm.JumpSpeed);Assert.AreEqual(3,revised.GetComponent<Rigidbody2D>().gravityScale);float oy=old.transform.position.y,ny=revised.transform.position.y,oh=0,nh=0;om.RequestJump();nm.RequestJump();float end=Time.time+1.5f;
   while(Time.time<end){yield return new WaitForFixedUpdate();oh=Mathf.Max(oh,old.transform.position.y-oy);nh=Mathf.Max(nh,revised.transform.position.y-ny);}Assert.That(nh/oh,Is.InRange(1.25f,1.30f),"actual rises old="+oh+" new="+nh);Assert.IsTrue(om.IsGrounded);Assert.IsTrue(nm.IsGrounded);Assert.AreEqual(oy,old.transform.position.y,.05f);Assert.AreEqual(ny,revised.transform.position.y,.05f);
   foreach(var name in new[]{"Larper","FireworksExpert"})Assert.AreEqual(12,new SerializedObject(Asset<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab").GetComponent<PlayerMotor2D>()).FindProperty("jumpSpeed").floatValue);
  }
  [UnityTest]public IEnumerator HigherSingleJumpReachesPlatformWithoutExtraAirImpulse()
  {
   Object.Destroy(owned[0]);yield return null;Solid(0,0,new Vector2(3,.5f));Solid(3.5f,3.2f,new Vector2(2.6f,.5f));var g=Samurai(0);var target=Keep(new GameObject("ElevatedTarget"));target.layer=2;target.transform.position=new Vector2(X+3.5f,3.9f);var health=target.AddComponent<Health>();var s=g.GetComponent<SamuraiCombat>();var motor=g.GetComponent<PlayerMotor2D>();int extra=0;motor.AirJumped+=()=>extra++;yield return new WaitForSeconds(.25f);float until=Time.time+4;
   while(Time.time<until){s.Tick(health,target.transform.position,true,true,true);yield return new WaitForFixedUpdate();if(s.PlatformJumps>0&&!s.PlatformJumpActive&&motor.IsGrounded)break;}Assert.Greater(s.PlatformJumps,0,s.FailedTransitionReason);Assert.AreEqual(0,extra);Assert.IsTrue(motor.IsGrounded);Assert.AreEqual(3.2f+g.GetComponent<BoxCollider2D>().size.y*.5f,g.transform.position.y,.15f);
  }
 }
}
#endif
