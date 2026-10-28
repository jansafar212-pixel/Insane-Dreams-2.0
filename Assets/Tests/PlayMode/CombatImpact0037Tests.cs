#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Input;
using InsaneDreams.Visuals;
using InsaneDreams.CameraSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class CombatImpact0037Tests
    {
        const float X=106000;
        readonly List<GameObject> owned=new List<GameObject>();
        GameObject player,enemy;Health ph,eh;PlayerActions input;HitReaction2D reaction;PlayerDamageFeedback feedback;
        bool background;float volume;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        void Field(Object o,string name,float value){var s=new SerializedObject(o);s.FindProperty(name).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            var floor=Keep(new GameObject("ImpactTestFloor"));floor.transform.position=new Vector2(X,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(100,1);
            var holder=Keep(new GameObject("ImpactPlayerFactory"));holder.SetActive(false);
            player=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(X,.8f),Quaternion.identity,holder.transform));
            if(player.GetComponent<BodyArmor>()==null)player.AddComponent<BodyArmor>();holder.SetActive(true);
            player.GetComponent<PlayerHealthRecovery>().enabled=false;input=player.GetComponent<PlayerActions>();var s=new SerializedObject(input);s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();
            enemy=MakeEnemy("Assets/Prefabs/Enemies/Larper.prefab",X+5);ph=player.GetComponent<Health>();eh=enemy.GetComponent<Health>();Field(ph,"maximum",1000);ph.Restore();
            feedback=player.GetComponent<PlayerDamageFeedback>();reaction=enemy.GetComponent<HitReaction2D>();yield return new WaitForSeconds(.25f);
        }
        GameObject MakeEnemy(string path,float x)
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),new Vector3(x,.8f),Quaternion.identity));
            g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;var h=g.GetComponent<Health>();Field(h,"maximum",1000);h.Restore();return g;
        }
        [UnityTearDown]public IEnumerator Cleanup(){Time.timeScale=1;foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();foreach(var d in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))d.Retire();yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        RuntimeWeapon Gun(string name)=>new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>(name=="BackupPistol"?"Assets/Data/Items/BackupPistol.asset":"Assets/Resources/Arsenal/"+name+".asset"));
        void Hit(Health h,float strength,int region=0,Vector2? incoming=null,float damage=1)
        {
            var rig=h.GetComponentInChildren<CutoutRig>();var r=rig.BodyParts[region].GetComponent<SpriteRenderer>();h.ProtectFor(0);
            Assert.IsTrue(h.ApplyDamage(new DamageInfo(damage,h.Team==CombatTeam.Player?CombatTeam.Enemy:CombatTeam.Player,null,new PhysicalImpact(r.bounds.center,incoming??Vector2.right,1,null,strength))));
        }
        void Capture(string name,GameObject actor)
        {
            var g=new GameObject("ImpactCapture");var c=g.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=1.35f;c.transform.position=actor.transform.position+new Vector3(0,.1f,-10);var rt=new RenderTexture(640,360,24);var prior=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(640,360,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,640,360),0,0);t.Apply();Directory.CreateDirectory("TestResults/Sprint0037");File.WriteAllBytes("TestResults/Sprint0037/"+name+".png",t.EncodeToPNG());RenderTexture.active=prior;c.targetTexture=null;Object.Destroy(t);Object.Destroy(rt);Object.Destroy(g);
        }
        [UnityTest]public IEnumerator RegionalSpringsReactDirectionallyWithoutSpawningCorpsesAndReturnToPose()
        {
            Assert.IsTrue(reaction.UsesLivingPhysics);Assert.AreEqual(6,reaction.JointCount);int corpses=CartoonRagdoll.ActiveCount;
            foreach(int region in new[]{0,1,2,4})
            {
                reaction.CancelReaction();Hit(eh,1,region);yield return new WaitForSeconds(.09f);Assert.AreEqual(region,reaction.LastRegion);Assert.Greater(Mathf.Abs(reaction.RegionAngle(region)),.05f,"Actual physical spring must move");Assert.LessOrEqual(Mathf.Abs(reaction.RegionAngle(region)),26.1f);Assert.AreEqual(corpses,CartoonRagdoll.ActiveCount);Capture("Region"+region,enemy);
            }
            reaction.CancelReaction();Hit(eh,1,0,Vector2.right);yield return new WaitForSeconds(.08f);float right=reaction.RegionAngle(0);reaction.CancelReaction();Hit(eh,1,0,Vector2.left);yield return new WaitForSeconds(.08f);Assert.Less(right*reaction.RegionAngle(0),0,"Opposite incoming shots reverse angular response; first="+right+" second="+reaction.RegionAngle(0)+" direction="+reaction.Direction);
            yield return new WaitForSeconds(.9f);Assert.AreEqual(0,reaction.RegionAngle(0),.01f);Assert.IsFalse(reaction.KnockedDown);
        }
        [UnityTest]public IEnumerator NativeWeaponsHaveDistinctStrengthAndShotgunBatchesAllPelletsWithoutChangingDamage()
        {
            var def=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset");Assert.Less(Gun("SMG").Definition.HitReactionStrength,Gun("LMG").Definition.HitReactionStrength);Assert.Greater(Gun("DesertEagle").Definition.HitReactionStrength,Gun("LMG").Definition.HitReactionStrength);Assert.Greater(Gun("Sniper").Definition.HitReactionStrength,Gun("DesertEagle").Definition.HitReactionStrength);
            var box=enemy.GetComponent<BoxCollider2D>();box.size=new Vector2(.8f,5);Physics2D.SyncTransforms();int events=0;eh.Damaged+=_=>events++;
            var w=Gun("Shotgun");player.GetComponent<PlayerLoadout>().Equip(EquipmentSlot.Firearm1,w);player.GetComponent<PlayerLoadout>().Select(EquipmentSlot.Firearm1);yield return null;
            int before=reaction.PhysicalReactions;float hp=eh.Current;Assert.IsTrue(player.GetComponent<AttackExecutor>().Attack(w,Vector2.right,Time.time));yield return new WaitForFixedUpdate();yield return null;
            Assert.AreEqual(7,events);Assert.AreEqual(hp-7*w.Definition.Damage,eh.Current,.1f);Assert.AreEqual(before+1,reaction.PhysicalReactions);Assert.AreEqual(1,reaction.HeavyReactions);Assert.LessOrEqual(reaction.LastIntensity,3.2f);
        }
        [UnityTest]public IEnumerator AccumulatedFallHasFixedDeadlineAndRecoveryGraceUnderContinuousFire()
        {
            int bodies=Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
            Hit(eh,3.2f);yield return new WaitForFixedUpdate();Hit(eh,3.2f);yield return new WaitForFixedUpdate();yield return null;Assert.IsTrue(reaction.KnockedDown);int falls=reaction.Falls;yield return new WaitForSeconds(.2f);Capture("LivingFall",enemy);
            float started=Time.time;for(int i=0;i<15;i++){Hit(eh,3.2f);yield return new WaitForSeconds(.06f);}Assert.Greater(Time.time-started,.8f);Assert.IsFalse(reaction.KnockedDown,"Rapid hits cannot extend the fall deadline");Assert.AreEqual(falls,reaction.Falls);Assert.IsTrue(enemy.GetComponent<EnemyCombat>().enabled==false,"Reaction never reenables an explicitly disabled controller");
            Assert.AreEqual(bodies,Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length);Assert.IsFalse(eh.IsDefeated);Assert.AreEqual(0,CartoonRagdoll.ActiveCount);
        }
        [UnityTest]public IEnumerator RecoveringEnemyKeepsControllerAndResumesNavigationBeforeNormalDeath()
        {
            var brain=enemy.GetComponent<LarperBrain>();var ai=enemy.GetComponent<EnemyCombat>();brain.enabled=true;ai.enabled=true;ai.BindTarget(ph);
            var camera=Keep(new GameObject("ImpactAICamera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8;camera.transform.position=new Vector3(X+3,2,-10);var s=new SerializedObject(ai);s.FindProperty("gameplayCamera").objectReferenceValue=camera;s.ApplyModifiedPropertiesWithoutUndo();
            Hit(eh,3.2f);yield return new WaitForFixedUpdate();Hit(eh,3.2f);yield return new WaitForFixedUpdate();yield return null;Assert.IsTrue(reaction.KnockedDown);Assert.IsTrue(ai.enabled);Assert.IsFalse(eh.IsDefeated);yield return new WaitForSeconds(.8f);Assert.IsFalse(reaction.KnockedDown);Assert.IsTrue(ai.enabled);Assert.AreNotEqual(EngagementState.Defeated,ai.State);
            float x=enemy.transform.position.x;input.SetTouchMove(Vector2.left);yield return new WaitForSeconds(.8f);Assert.IsTrue(ai.TargetVisible||ai.HasEngaged||Mathf.Abs(enemy.transform.position.x-x)>.02f,"AI must resume perception or locomotion");
            eh.ApplyDamage(new DamageInfo(2000,CombatTeam.Player,null,new PhysicalImpact(enemy.transform.position,Vector2.right,1)));yield return null;Assert.IsTrue(eh.IsDefeated);Assert.AreEqual(1,enemy.GetComponent<RagdollOnDefeat>().SpawnCount);Assert.IsFalse(reaction.KnockedDown);Assert.AreEqual(1,CartoonRagdoll.ActiveCount);
        }
        [TestCase(1,0,0)][TestCase(-1,0,1)][TestCase(0,-1,2)][TestCase(0,1,3)]
        public void AcceptedDirectionalFeedbackUsesIncomingVectorAndNeverFadesCriticalText(float x,float y,int side)
        {
            Hit(ph,1,0,new Vector2(x,y),10);Assert.AreEqual(side,feedback.LastDirection);Assert.Greater(feedback.IndicatorAlpha(side),0);Assert.LessOrEqual(feedback.IndicatorAlpha(side),.85f);Assert.That(feedback.EdgeAlpha,Is.InRange(0,.18f));
        }
        [Test]public void ArmorOutcomeReducesFeedbackAndInvulnerabilityOrEnvironmentCannotFakeDirectionalHits()
        {
            Hit(ph,1,0,Vector2.right,20);float bare=feedback.LastSeverity;feedback.Clear();ph.Restore();var armor=player.GetComponent<BodyArmor>();armor.Restore(60);Hit(ph,1,0,Vector2.right,20);Assert.Less(feedback.LastSeverity,bare);Assert.AreEqual(993,ph.Current,.01f);
            feedback.Clear();uint revision=feedback.Revision;ph.ProtectFor(2);Assert.IsFalse(ph.ApplyDamage(new DamageInfo(20,CombatTeam.Enemy,null,new PhysicalImpact(player.transform.position,Vector2.right,1))));Assert.AreEqual(revision,feedback.Revision);Assert.AreEqual(0,feedback.EdgeAlpha);
            ph.ProtectFor(0);Assert.IsTrue(ph.ApplyDamage(new DamageInfo(5,CombatTeam.Neutral,null,new PhysicalImpact(player.transform.position,Vector2.up,1),false,DamageKind.Environment)));Assert.AreEqual(-1,feedback.LastDirection);for(int i=0;i<4;i++)Assert.AreEqual(0,feedback.IndicatorAlpha(i));Assert.Greater(feedback.EdgeAlpha,0);
        }
        [UnityTest]public IEnumerator PlayerKeepsMovementReloadAndHudInputDuringRepeatedImpactsAndEffectsFade()
        {
            var doc=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));doc.GetComponent<WarehouseTouchControls>().Bind(input);yield return null;var root=doc.GetComponent<UIDocument>().rootVisualElement;
            var w=Gun("LMG");var load=player.GetComponent<PlayerLoadout>();load.Equip(EquipmentSlot.Firearm1,w);load.Select(EquipmentSlot.Firearm1);yield return null;w.SetAmmunition(1,20);player.GetComponent<PlayerCombat>().Reload();input.SetTouchMove(Vector2.right);float before=player.transform.position.x;
            for(int i=0;i<6;i++){Hit(ph,3.2f,0,Vector2.left,2);yield return new WaitForSeconds(.08f);}
            Assert.Greater(player.transform.position.x,before+.1f);Assert.IsTrue(input.enabled);Assert.IsFalse(player.GetComponent<HitReaction2D>().KnockedDown);Assert.IsTrue(w.Cycle.IsReloading);Assert.AreEqual(1,w.MagazineAmmo);Assert.AreEqual(8,root.Q("damageFeedbackOverlay").childCount);Assert.AreEqual(PickingMode.Ignore,root.Q("damageFeedbackOverlay").pickingMode);Assert.AreEqual(.4f,root.Q("moveStick").resolvedStyle.backgroundColor.a,.01f);Assert.AreEqual(1,root.Q<Label>("healthValue").resolvedStyle.color.a,.01f);
            input.SetTouchMove(Vector2.zero);yield return new WaitForSeconds(.7f);Assert.AreEqual(0,feedback.EdgeAlpha);for(int i=0;i<4;i++)Assert.AreEqual(0,feedback.IndicatorAlpha(i));Assert.IsFalse(root.Q("healthFill").ClassListContains("damage-flash"));Assert.AreEqual(DisplayStyle.None,root.Q("damageFeedbackOverlay").resolvedStyle.display);
        }
        [UnityTest]public IEnumerator HeavyCameraImpulseIsTinyThrottledAndCanBeDisabled()
        {
            var c=Keep(new GameObject("DamageCamera")).AddComponent<Camera>();var f=c.gameObject.AddComponent<WarehouseCameraFollow>();Field(f,"orthographicSize",5);f.Bind(player.transform);feedback.BindCamera(f);Hit(ph,2,0,Vector2.right,20);Assert.AreEqual(1,feedback.CameraImpulses);Assert.That(f.ShakeMagnitude,Is.InRange(0,.026f));Hit(ph,2,0,Vector2.right,20);Assert.AreEqual(1,feedback.CameraImpulses);yield return new WaitForSeconds(.7f);Assert.AreEqual(0,f.ShakeMagnitude);feedback.CameraShakeScale=0;Hit(ph,2,0,Vector2.right,20);Assert.AreEqual(1,feedback.CameraImpulses);Assert.AreEqual(0,f.ShakeMagnitude);
        }
        [UnityTest]public IEnumerator AllArchetypesShareRegionalPhysicsAndRapidSmgCannotPermanentlyLockThem()
        {
            foreach(var path in new[]{"Assets/Prefabs/Enemies/Samurai.prefab","Assets/Prefabs/Enemies/FireworksExpert.prefab","Assets/Data/FirstOperation/Officer.prefab"})
            {
                var g=MakeEnemy(path,X+10);var h=g.GetComponent<Health>();var r=g.GetComponent<HitReaction2D>();Hit(h,3.2f);yield return new WaitForSeconds(.1f);Assert.AreEqual(6,r.JointCount);Assert.Greater(Mathf.Abs(r.RegionAngle(0)),.05f);Assert.IsFalse(h.IsDefeated);Capture(Path.GetFileNameWithoutExtension(path)+"LivingHit",g);Object.Destroy(g);yield return null;
            }
            float lockedTime=0,longest=0,current=0,smgStrength=Gun("SMG").Definition.HitReactionStrength;int heavy=reaction.HeavyReactions;
            for(int i=0;i<70;i++){Hit(eh,smgStrength);yield return new WaitForSeconds(.06f);if(reaction.KnockedDown){lockedTime+=.06f;current+=.06f;longest=Mathf.Max(longest,current);}else current=0;}
            Assert.LessOrEqual(longest,.72f);Assert.Less(lockedTime,1.5f);Assert.AreEqual(heavy,reaction.HeavyReactions);Assert.IsFalse(eh.IsDefeated);Assert.AreEqual(6,reaction.JointCount);
        }
        [UnityTest]public IEnumerator PhysicsCacheCanUnloadBeforeItsOwnerWithoutMissingReferences()
        {
            Hit(eh,1);yield return new WaitForFixedUpdate();
            var cache=(GameObject)typeof(HitReaction2D).GetField("physicsRoot",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(reaction);
            Object.Destroy(cache);yield return null;reaction.CancelReaction();Assert.AreEqual(0,reaction.RegionAngle(0));Object.Destroy(enemy);yield return null;LogAssert.NoUnexpectedReceived();
        }
        [UnityTest]public IEnumerator TracersUseNativeMultipliersAlignBothDirectionsAndFadeWithinExistingPool()
        {
            var attack=player.GetComponent<AttackExecutor>();var lines=(LineRenderer[])typeof(AttackExecutor).GetField("trails",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(attack);int count=lines.Length;float now=Time.time+10,pistolWidth=0,eagleWidth=0,baseWidth=new SerializedObject(attack).FindProperty("tracerWidth").floatValue;
            foreach(string name in new[]{"BackupPistol","DesertEagle","Sniper"})
            {
                var w=Gun(name);player.GetComponent<PlayerLoadout>().Equip(EquipmentSlot.Firearm1,w);player.GetComponent<PlayerLoadout>().Select(EquipmentSlot.Firearm1);yield return null;
                foreach(var aim in new[]{Vector2.up,new Vector2(-1,1).normalized,new Vector2(1,1).normalized})
                {
                    Assert.IsTrue(attack.Attack(w,aim,now+=2));var line=lines.Where(l=>l.enabled).LastOrDefault(l=>Vector2.Distance(l.GetPosition(1),attack.LastShotEnd)<.03f);Assert.IsNotNull(line);Assert.Greater(Vector2.Dot(((Vector2)(line.GetPosition(1)-line.GetPosition(0))).normalized,aim),.999f);
                    Assert.That(line.startWidth,Is.InRange(baseWidth*w.Definition.TracerScale*.849f,baseWidth*w.Definition.TracerScale*1.151f));if(name=="BackupPistol")pistolWidth=Mathf.Max(pistolWidth,line.startWidth);if(name=="DesertEagle"){Assert.Greater(line.startWidth,pistolWidth);eagleWidth=Mathf.Max(eagleWidth,line.startWidth);}if(name=="Sniper")Assert.Greater(line.startWidth,eagleWidth);
                }
            }
            Assert.AreEqual(1.6f,Gun("DesertEagle").Definition.TracerScale);Assert.AreEqual(2.2f,Gun("Sniper").Definition.TracerScale);yield return new WaitForSeconds(.23f);Assert.IsTrue(lines.All(l=>!l.enabled));Assert.AreEqual(count,lines.Length);
        }
    }
}
#endif
