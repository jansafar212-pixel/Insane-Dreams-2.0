#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Visuals;
using InsaneDreams.CameraSystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0247Tests
    {
        private readonly List<GameObject> owned=new List<GameObject>();private bool background;private Random.State random;
        private GameObject Keep(GameObject g){owned.Add(g);return g;}
        private GameObject Actor(string n)=>Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/"+(n=="WarehousePlayer"?"Player/":"Enemies/")+n+".prefab"),new Vector3(47000,.71f),Quaternion.identity));
        private static void Float(Object o,string name,float v){var s=new SerializedObject(o);s.FindProperty(name).floatValue=v;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;random=Random.state;Random.InitState(247);var floor=Keep(new GameObject("S0247Floor"));floor.transform.position=new Vector2(47000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(100,1);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Random.state=random;Application.runInBackground=background;}
        [UnityTest]public IEnumerator CameraReversesSmoothlyRejectsRapidChatterAndKeepsJumpVisible()
        {
            var p=Actor("WarehousePlayer");var a=p.GetComponent<PlayerActions>();var data=new SerializedObject(a);data.FindProperty("readKeyboard").boolValue=false;data.ApplyModifiedPropertiesWithoutUndo();
            var c=Keep(new GameObject("S0247Camera")).AddComponent<Camera>();c.orthographic=true;c.aspect=16f/9;var f=c.gameObject.AddComponent<WarehouseCameraFollow>();
            var s=new SerializedObject(f);s.FindProperty("visualPixelsPerUnit").intValue=0;s.FindProperty("levelMin").vector2Value=new Vector2(46900,-9);s.FindProperty("levelMax").vector2Value=new Vector2(47100,20);s.ApplyModifiedPropertiesWithoutUndo();f.Bind(p.transform);
            yield return new WaitForSeconds(.3f);Assert.AreEqual(1f/3,c.WorldToViewportPoint(p.transform.position).x,.003f);
            a.SetTouchMove(Vector2.right);yield return new WaitForSeconds(.7f);Assert.AreEqual(1f/3,c.WorldToViewportPoint(p.transform.position).x,.005f,"Full-speed motion must retain the ratio rather than accumulate follow lag.");
            a.SetTouchMove(Vector2.left);yield return new WaitForSeconds(.18f);a.SetTouchMove(Vector2.zero);float before=c.transform.position.x;
            yield return null;Assert.Less(Mathf.Abs(c.transform.position.x-before),1);yield return new WaitForSeconds(3.4f);
            Assert.AreEqual(2f/3,c.WorldToViewportPoint(p.transform.position).x,.008f);
            for(int i=0;i<8;i++){a.SetTouchMove(i%2==0?Vector2.right:Vector2.left);yield return new WaitForSeconds(.04f);}
            Assert.Greater(f.DesiredPlayerViewportX,.60f,"Sub-commit reversals must not flip framing.");
            a.SetTouchMove(Vector2.zero);a.Jump();yield return new WaitForSeconds(.25f);a.Jump();yield return new WaitForSeconds(.2f);
            var point=c.WorldToViewportPoint(p.transform.position);Assert.That(point.x,Is.InRange(.05f,.95f));Assert.That(point.y,Is.InRange(.05f,.95f));
            Assert.Less(Vector3.Distance(p.GetComponent<CharacterVisualDriver>().Rig.transform.position,p.transform.position),.001f);
        }
        [UnityTest]public IEnumerator StrongFirstMissShowsHesitationEvenWhenRetreatAndDuckRollsFail()
        {
            var e=Actor("Larper").GetComponent<EnemyCombat>();e.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            Float(e,"defensiveCrouchProbability",0);Float(e,"suppressionRetreatChance",0);yield return new WaitForSeconds(.2f);
            int reactions=0;e.GetComponent<LarperAwareness>().Reaction+=ctx=>{if(ctx==LarperSpeechEvent.Suppressed)reactions++;};
            e.Hear(new CombatNoiseEvent(null,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(46992,1.6f),new Vector2(47008,1.6f),1));
            Assert.Greater(e.Suppression,42);var field=typeof(EnemyCombat).GetField("hesitateUntil",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);Assert.Greater((float)field.GetValue(e),Time.time);Assert.AreEqual(1,reactions);
            for(int i=0;i<10;i++)e.Hear(new CombatNoiseEvent(null,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(46992,1.6f),new Vector2(47008,1.6f),1));
            Assert.AreEqual(1,reactions);Assert.LessOrEqual(e.Suppression,100);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().PanicReactions);Assert.AreEqual(0,e.GetComponent<LarperAwareness>().CollapseReactions);
            yield return new WaitForSeconds(10);Assert.AreEqual(0,e.Suppression);Assert.IsFalse(e.IsDefensivelyCrouching);
        }
        [UnityTest]public IEnumerator TinySamuraiCorpseFitsCopiedSpritesAndPreservesContactMasks()
        {
            var g=Actor("Samurai");g.GetComponent<EnemyCombat>().enabled=false;g.GetComponent<LarperBrain>().enabled=false;yield return new WaitForSeconds(.25f);
            g.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;var d=g.GetComponent<RagdollOnDefeat>().LastRagdoll;owned.Add(d.gameObject);Assert.AreEqual(14,d.Bodies.Length);
            foreach(var b in d.Bodies){var box=b.GetComponent<BoxCollider2D>();Assert.AreEqual((Vector2)b.GetComponent<SpriteRenderer>().sprite.bounds.size,box.size);Assert.AreEqual(-2,box.excludeLayers.value);}
            yield return new WaitForSeconds(.5f);foreach(var b in d.Bodies)Assert.LessOrEqual(b.linearVelocity.magnitude,12.1f);
        }
    }
}
#endif
