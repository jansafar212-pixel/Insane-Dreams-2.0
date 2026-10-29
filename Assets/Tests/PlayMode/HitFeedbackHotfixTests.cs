#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class HitFeedbackHotfixTests
    {
        readonly List<GameObject> owned=new List<GameObject>();bool background;float volume;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        GameObject Actor(bool enemy,float x)
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(enemy?"Assets/Prefabs/Enemies/Larper.prefab":"Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(x,1),Quaternion.identity));
            var body=g.GetComponent<Rigidbody2D>();body.bodyType=RigidbodyType2D.Kinematic;body.constraints=RigidbodyConstraints2D.FreezeAll;
            if(enemy){g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;}
            else{var s=new SerializedObject(g.GetComponent<PlayerActions>());s.FindProperty("readKeyboard").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();}
            var health=g.GetComponent<Health>();var h=new SerializedObject(health);h.FindProperty("maximum").floatValue=10000;h.ApplyModifiedPropertiesWithoutUndo();health.Restore();health.ProtectFor(0);return g;
        }
        static SpriteRenderer[] Parts(GameObject actor)=>actor.GetComponentInChildren<CutoutRig>(true).GetComponentsInChildren<SpriteRenderer>(true).Where(r=>r.sprite!=null).ToArray();
        static void Red(SpriteRenderer[] parts)
        {foreach(var p in parts){Assert.Greater(p.color.r,p.color.g*3,p.name);Assert.AreEqual("AKUMU/HitFlash",p.sharedMaterial.shader.name,p.name);}}
        static void Normal(SpriteRenderer[] parts,Color[] colors,Material[] materials)
        {for(int i=0;i<parts.Length;i++){Assert.AreEqual(colors[i],parts[i].color,parts[i].name);Assert.AreSame(materials[i],parts[i].sharedMaterial,parts[i].name);}}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;Directory.CreateDirectory("TestResults/HitFeedbackHotfix");yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var corpse in Object.FindObjectsByType<CartoonRagdoll>(FindObjectsSortMode.None))corpse.Retire();foreach(var g in owned)if(g!=null)Object.Destroy(g);yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        [UnityTest]public IEnumerator AllElevenNativeFirearmsFlashTheWholeLivingLarperAndRestoreColorsAndMaterials()
        {
            var shooter=Actor(false,600);var target=Actor(true,603);yield return null;yield return new WaitForEndOfFrame();
            var parts=Parts(target);Assert.GreaterOrEqual(parts.Length,14,"Includes articulated forearms, hands, lower legs and feet");
            var colors=parts.Select(p=>p.color).ToArray();var materials=parts.Select(p=>p.sharedMaterial).ToArray();
            var load=shooter.GetComponent<PlayerLoadout>();var attack=shooter.GetComponent<AttackExecutor>();
            var guns=AssetDatabase.LoadAssetAtPath<ArsenalCatalog>("Assets/Resources/ArsenalCatalog.asset").weapons;
            Assert.AreEqual(11,guns.Length);
            foreach(var gun in guns)
            {
                var w=new RuntimeWeapon(gun,30);var slot=gun.FirearmRole==FirearmRole.Primary?EquipmentSlot.Firearm1:EquipmentSlot.Firearm2;
                Assert.IsTrue(load.Equip(slot,w));load.Select(slot);yield return null;
                float before=target.GetComponent<Health>().Current;
                var center=target.GetComponentInChildren<CutoutRig>().torso.GetComponent<SpriteRenderer>().bounds.center;
                var aim=(Vector2)(center-(Vector3)attack.Origin);
                Assert.IsTrue(attack.Attack(w,aim,Time.time),gun.name);Assert.Less(target.GetComponent<Health>().Current,before,gun.name);
                Assert.IsFalse(target.GetComponent<Health>().IsDefeated);Red(parts);
                yield return new WaitForEndOfFrame();Red(parts);
                if(gun.name=="BackupPistol")Capture(target,"LivingFullBodyRed",true);
                yield return new WaitForSeconds(.21f);Normal(parts,colors,materials);
            }
            Capture(target,"LivingRestored",false);
        }
        [UnityTest]public IEnumerator RepeatedPelletsRefreshAndDeathRestoresBeforeObserversAndCannotReflashForearms()
        {
            var actor=Actor(true,620);yield return null;yield return new WaitForEndOfFrame();
            var parts=Parts(actor);var colors=parts.Select(p=>p.color).ToArray();var materials=parts.Select(p=>p.sharedMaterial).ToArray();
            var health=actor.GetComponent<Health>();var feedback=actor.GetComponent<CombatFeedback>();
            Assert.IsTrue(health.ApplyDamage(new DamageInfo(1,CombatTeam.Player)));yield return new WaitForSeconds(.1f);
            for(int i=0;i<7;i++)Assert.IsTrue(health.ApplyDamage(new DamageInfo(1,CombatTeam.Player,null,new PhysicalImpact(actor.transform.position,Vector2.right,.55f,null,.55f))));
            yield return new WaitForSeconds(.08f);Red(parts);int cues=feedback.HurtCues;bool restoredBeforeDefeat=false;
            health.Defeated+=_=>{Normal(parts,colors,materials);restoredBeforeDefeat=true;};
            health.ApplyDamage(new DamageInfo(20000,CombatTeam.Player));Assert.IsTrue(restoredBeforeDefeat);Normal(parts,colors,materials);
            var corpse=actor.GetComponent<RagdollOnDefeat>().LastRagdoll;Assert.NotNull(corpse);
            foreach(var b in corpse.Bodies){Assert.AreNotEqual("AKUMU/HitFlash",b.GetComponent<SpriteRenderer>().sharedMaterial.shader.name);}
            foreach(var extra in actor.GetComponentInChildren<CutoutRig>(true).Segmented.ExtraParts)
            {var copied=corpse.transform.Find(extra.name).GetComponent<SpriteRenderer>();Assert.AreEqual(extra.GetComponent<SpriteRenderer>().color,copied.color,extra.name);}
            // Simulate a stale callback even though native death deactivates the actor.
            var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            typeof(CombatFeedback).GetMethod("Hurt",flags).Invoke(feedback,new object[]{new DamageInfo(1,CombatTeam.Player)});
            typeof(CombatFeedback).GetMethod("Reload",flags).Invoke(feedback,null);typeof(CombatFeedback).GetMethod("Empty",flags).Invoke(feedback,null);
            Assert.IsFalse(health.ApplyDamage(new DamageInfo(1,CombatTeam.Player)));Assert.AreEqual(cues,feedback.HurtCues);
            yield return new WaitForSeconds(.25f);Normal(parts,colors,materials);Capture(corpse.gameObject,"DeathNormalColors",false);
        }
        [UnityTest]public IEnumerator FirstLethalHitNeverStartsAFlash()
        {
            var actor=Actor(true,640);yield return null;var parts=Parts(actor);var colors=parts.Select(p=>p.color).ToArray();var materials=parts.Select(p=>p.sharedMaterial).ToArray();
            actor.GetComponent<Health>().ApplyDamage(new DamageInfo(20000,CombatTeam.Player));
            Assert.AreEqual(0,actor.GetComponent<CombatFeedback>().HurtCues);Normal(parts,colors,materials);
            yield return null;Assert.NotNull(actor.GetComponent<RagdollOnDefeat>().LastRagdoll);
        }
        [UnityTest]public IEnumerator ConcurrentShotgunBatchesReuseParticlesAndExistingPhysicsWithoutPerPelletObjects()
        {
            var actors=Enumerable.Range(0,8).Select(i=>Actor(true,660+i*2)).ToArray();yield return null;yield return new WaitForEndOfFrame();
            var counts=actors.Select(a=>a.GetComponentsInChildren<Transform>(true).Length).ToArray();
            var physics=actors.Select(a=>a.GetComponent<HitReaction2D>().JointCount).ToArray();
            for(int frame=0;frame<20;frame++)
            {
                foreach(var actor in actors)for(int pellet=0;pellet<7;pellet++)actor.GetComponent<Health>().ApplyDamage(new DamageInfo(.01f,CombatTeam.Player,null,new PhysicalImpact(actor.transform.position,Vector2.right,.55f,null,.55f)));
                yield return new WaitForFixedUpdate();yield return new WaitForEndOfFrame();
                for(int i=0;i<actors.Length;i++){var a=actors[i];Assert.AreEqual(counts[i],a.GetComponentsInChildren<Transform>(true).Length);Assert.AreEqual(physics[i],a.GetComponent<HitReaction2D>().JointCount);Assert.AreEqual(12,a.GetComponent<CombatFeedback>().ImpactParticleCapacity);Assert.IsFalse(a.GetComponent<Health>().IsDefeated);Red(Parts(a));}
            }
            yield return new WaitForSeconds(1.3f);
            foreach(var a in actors){Assert.IsFalse(a.GetComponent<HitReaction2D>().KnockedDown);Assert.IsFalse(a.GetComponent<Health>().IsDefeated);}
        }
        [UnityTest]public IEnumerator Aa12NativeMagazineAnimationHudAndSwitchInterruptionConserveAmmo()
        {
            var player=Actor(false,690);yield return null;var load=player.GetComponent<PlayerLoadout>();var combat=player.GetComponent<PlayerCombat>();var rig=player.GetComponentInChildren<CutoutRig>();
            var w=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Resources/Arsenal/AA12.asset"));Assert.IsTrue(load.Equip(EquipmentSlot.Firearm1,w));load.Select(EquipmentSlot.Firearm1);
            var doc=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab")));doc.GetComponent<WarehouseTouchControls>().Bind(player.GetComponent<PlayerActions>());yield return null;
            var root=doc.GetComponent<UIDocument>().rootVisualElement;
            foreach(var aim in new[]{Vector2.right,Vector2.left})
            {
                player.GetComponent<PlayerActions>().SetTouchAim(aim);w.SetAmmunition(3,11);yield return null;var socket=rig.magazine.transform.localPosition;
                combat.Reload();Assert.IsTrue(w.Cycle.IsReloading);yield return new WaitForSeconds(1.05f);
                Assert.Greater(Vector3.Distance(socket,rig.magazine.transform.localPosition),.03f);Assert.AreEqual(3,w.MagazineAmmo);
                Assert.IsFalse(player.GetComponent<AttackExecutor>().Attack(w,aim,Time.time));yield return new WaitForSeconds(1f);
                var fresh=rig.weapon.transform.Find("ReplacementMagazine").GetComponent<SpriteRenderer>();Assert.IsTrue(fresh.enabled);Assert.IsFalse(rig.magazine.enabled);Assert.IsFalse(rig.ReloadShell!=null&&rig.ReloadShell.enabled);
                Capture(player,aim.x>0?"Aa12InsertRight":"Aa12InsertLeft",false);
                yield return new WaitForSeconds(1.1f);Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(8,w.MagazineAmmo);Assert.AreEqual(6,w.ReserveAmmo);Assert.AreEqual("8/8",root.Q<Label>("ammoValue").text);Assert.IsTrue(rig.magazine.enabled);Assert.IsFalse(fresh.enabled);
                w.SetAmmunition(3,11);combat.Reload();yield return new WaitForSeconds(.7f);load.Select(EquipmentSlot.Firearm2);yield return null;
                Assert.IsFalse(w.Cycle.IsReloading);Assert.AreEqual(3,w.MagazineAmmo);Assert.AreEqual(11,w.ReserveAmmo);Assert.IsFalse(fresh.enabled);
                load.Select(EquipmentSlot.Firearm1);combat.Reload();yield return new WaitForSeconds(3.1f);Assert.AreEqual(8,w.MagazineAmmo);Assert.AreEqual(6,w.ReserveAmmo);
            }
        }
        void Capture(GameObject subject,string name,bool red)
        {
            var go=Keep(new GameObject("HotfixGameplayProjection"));var c=go.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=4.3290043f;c.transform.position=subject.transform.position+new Vector3(0,.3f,-10);c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.14f,.18f,.19f);c.enabled=false;
            var rt=new RenderTexture(1280,720,24);var t=new Texture2D(1280,720,TextureFormat.RGB24,false);var previous=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;t.ReadPixels(new Rect(0,0,1280,720),0,0);t.Apply();
            if(red)Assert.Greater(t.GetPixels().Count(p=>p.r>.6f&&p.g<.4f&&p.b<.4f),100,"Rendered red silhouette, not merely renderer property values");
            File.WriteAllBytes("TestResults/HitFeedbackHotfix/"+name+".png",t.EncodeToPNG());RenderTexture.active=previous;c.targetTexture=null;Object.Destroy(rt);Object.Destroy(t);
        }
    }
}
#endif
