#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public sealed class CombatPresentation0039Tests
    {
        const float X=108000;
        readonly List<GameObject> owned=new List<GameObject>();
        GameObject player;bool background;float volume;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        static void Freeze(GameObject g){var b=g.GetComponent<Rigidbody2D>();if(b!=null){b.bodyType=RigidbodyType2D.Kinematic;b.constraints=RigidbodyConstraints2D.FreezeAll;}}
        GameObject Actor(string path,float x)
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),new Vector3(x,1),Quaternion.identity));Freeze(g);
            foreach(var behaviour in g.GetComponents<MonoBehaviour>())if(behaviour is EnemyCombat||behaviour is LarperBrain||behaviour is SamuraiCombat||behaviour is FireworksCombat)behaviour.enabled=false;
            return g;
        }
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            player=Actor("Assets/Prefabs/Player/WarehousePlayer.prefab",X);yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        [UnityTest]public IEnumerator EveryArchetypeFlashesRedDespiteReloadAndRepeatedHitsThenRestoresItsColors()
        {
            foreach(string path in new[]{"Assets/Prefabs/Enemies/Larper.prefab","Assets/Prefabs/Enemies/Samurai.prefab","Assets/Prefabs/Enemies/FireworksExpert.prefab","Assets/Data/FirstOperation/Officer.prefab","Assets/Data/FirstOperation/OperationFireworksExpert.prefab"})
            {
                var actor=Actor(path,X+4);yield return null;var health=actor.GetComponent<Health>();health.ProtectFor(0);
                var feedback=actor.GetComponent<CombatFeedback>();var torso=actor.GetComponentInChildren<CutoutRig>().torso.GetComponent<SpriteRenderer>();Color before=torso.color;
                Assert.IsTrue(health.ApplyDamage(new DamageInfo(1,CombatTeam.Player)));Assert.Greater(torso.color.r,torso.color.g*3,path);
                feedback.SendMessage("Reload");Assert.Greater(torso.color.r,torso.color.g*3,"Reload cannot replace red with yellow");yield return null;
                Assert.Greater(torso.color.r,torso.color.g*3,path);
                for(int i=0;i<5;i++)Assert.IsTrue(health.ApplyDamage(new DamageInfo(1,CombatTeam.Player)));
                feedback.SendMessage("Empty");Assert.Greater(torso.color.r,torso.color.g*3);
                yield return new WaitForSeconds(.25f);Assert.AreEqual(before,torso.color,path);
                actor.SetActive(false);Assert.AreEqual(before,torso.color);Object.Destroy(actor);yield return null;
            }
        }
        [UnityTest]public IEnumerator PlayerEmptyMagazineFlashRemainsGrayAndTemporary()
        {
            var load=player.GetComponent<PlayerLoadout>();var w=load.SelectedWeapon;w.SetAmmunition(0,10);
            var torso=player.GetComponentInChildren<CutoutRig>().torso.GetComponent<SpriteRenderer>();var before=torso.color;
            Assert.IsFalse(player.GetComponent<AttackExecutor>().Attack(w,Vector2.right,Time.time));Assert.AreEqual(Color.gray,torso.color);
            yield return new WaitForSeconds(.2f);Assert.AreEqual(before,torso.color);
        }
        [UnityTest]public IEnumerator BrightTracerUsesExistingPoolFadesAndKeepsOfficerColorAndAimMaterialSeparate()
        {
            var attack=player.GetComponent<AttackExecutor>();var w=player.GetComponent<PlayerLoadout>().SelectedWeapon;
            Assert.AreEqual("AKUMU/PixelTracer",attack.TracerMaterial.shader.name);Assert.AreEqual("Sprites/Default",attack.AimMaterial.shader.name);
            int count=player.GetComponentsInChildren<LineRenderer>(true).Length,objects=player.GetComponentsInChildren<Transform>(true).Length;
            for(int i=0;i<120;i++){w.SetAmmunition(w.Definition.MagazineCapacity,180);Assert.IsTrue(attack.Attack(w,Vector2.right,Time.time+20+i));}
            var line=player.GetComponentsInChildren<LineRenderer>(true).First(l=>l.enabled);float alpha=line.startColor.a;
            Assert.AreEqual(count,player.GetComponentsInChildren<LineRenderer>(true).Length);Assert.AreEqual(objects,player.GetComponentsInChildren<Transform>(true).Length);
            Assert.Less(line.endColor.a,line.startColor.a);yield return new WaitForSeconds(.06f);Assert.Less(line.startColor.a,alpha);
            yield return new WaitForSeconds(.3f);Assert.IsTrue(player.GetComponentsInChildren<LineRenderer>(true).All(l=>!l.enabled));
            var officer=Actor("Assets/Data/FirstOperation/Officer.prefab",X+10);yield return null;var oa=officer.GetComponent<AttackExecutor>();var gun=officer.GetComponent<EnemyCombat>().Weapon;
            Assert.IsTrue(oa.Attack(gun,Vector2.right,Time.time));var red=officer.GetComponentsInChildren<LineRenderer>(true).First(l=>l.enabled);
            Assert.Greater(red.startColor.r,red.startColor.g);Assert.Greater(red.startWidth,line.startWidth);Assert.AreEqual("Sprites/Default",oa.AimMaterial.shader.name);
        }
        [UnityTest]public IEnumerator ThreeMaterialImpactsRemainShortAndBoundedButHaveDifferentShapesAndColors()
        {
            var wall=Keep(new GameObject("Material0039Wall"));wall.transform.position=new Vector3(X+3,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,4);var marker=wall.AddComponent<ImpactSurface2D>();Physics2D.SyncTransforms();
            var attack=player.GetComponent<AttackExecutor>();var feedback=player.GetComponent<CombatFeedback>();var w=player.GetComponent<PlayerLoadout>().SelectedWeapon;
            var colors=new List<Color>();var widths=new List<float>();int capacity=feedback.ImpactParticleCapacity;
            foreach(var kind in new[]{ImpactMaterial.Concrete,ImpactMaterial.Metal,ImpactMaterial.Wood})
            {
                marker.Configure(kind);Assert.IsTrue(attack.Attack(w,Vector2.right,Time.time));Assert.AreEqual(kind,attack.LastImpactMaterial);Assert.AreEqual(kind,feedback.LastImpactMaterial);
                var surface=player.transform.Find("SurfaceImpact").GetComponent<SpriteRenderer>();colors.Add(surface.color);
                float oldSize=new SerializedObject(feedback).FindProperty("surfaceImpactSprite").objectReferenceValue!=null?1:.18f;Assert.AreEqual(oldSize*1.25f,surface.transform.localScale.x,.001f);
                var particles=player.GetComponentsInChildren<SpriteRenderer>().Where(p=>p.name.StartsWith("ImpactParticle_")&&p.enabled).ToArray();Assert.AreEqual(3,particles.Length);widths.Add(particles[0].transform.localScale.y);
                yield return new WaitForSeconds(.35f);Assert.IsFalse(surface.enabled);Assert.IsFalse(particles.Any(p=>p.enabled));
            }
            Assert.AreNotEqual(colors[0],colors[1]);Assert.AreNotEqual(colors[1],colors[2]);Assert.Greater(widths[0],widths[1]);Assert.AreEqual(capacity,feedback.ImpactParticleCapacity);
        }
    }
}
#endif
