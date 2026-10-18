#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class PersonalityIntegrationTests
    {
        private readonly List<GameObject> objects=new List<GameObject>();
        private Camera camera;
        private UIDocument document;
        private LarperPersonalityProfile profile;
        private bool background;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private GameObject Enemy(float x)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,1),Quaternion.identity));
            go.GetComponent<Rigidbody2D>().bodyType=RigidbodyType2D.Kinematic;go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            go.GetComponent<LarperAwareness>().Configure(profile);go.GetComponent<LarperSpeechBubble>().Bind(camera,document);return go;
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;LarperAwareness.ResetEncounterSpeech();
            var cam=Keep(new GameObject("PersonalityTestCamera"));camera=cam.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=8;cam.transform.position=new Vector3(6000,2,-10);
            document=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();
            profile=Object.Instantiate(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Cowardly.asset"));
            var data=new SerializedObject(profile);data.FindProperty("panicChance").floatValue=1;data.FindProperty("panicSeconds").floatValue=.35f;data.ApplyModifiedPropertiesWithoutUndo();yield return null;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {foreach(var go in objects)if(go!=null)Object.Destroy(go);Object.Destroy(profile);yield return null;Application.runInBackground=background;}
        [UnityTest] public IEnumerator DetectionSpeechIsContextualAndEncounterRateLimitPreventsSpam()
        {
            var player=Keep(new GameObject("PersonalityTarget"));player.transform.position=new Vector3(6002,1);var health=player.AddComponent<Health>();
            var trigger=Keep(new GameObject("NonSolidLadderTrigger"));trigger.transform.position=new Vector3(6001,1);trigger.AddComponent<BoxCollider2D>().size=new Vector2(.3f,4);trigger.GetComponent<BoxCollider2D>().isTrigger=true;Physics2D.SyncTransforms();
            var a=Enemy(6000);var b=Enemy(6004);a.GetComponent<EnemyCombat>().BindTarget(health);b.GetComponent<EnemyCombat>().BindTarget(health);
            yield return new WaitForSeconds(.12f);
            Assert.IsTrue(a.GetComponent<EnemyCombat>().TargetVisible,"Ladder triggers must not occlude sight");Assert.IsTrue(b.GetComponent<EnemyCombat>().TargetVisible);
            int count=a.GetComponent<LarperSpeechBubble>().LinesShown+b.GetComponent<LarperSpeechBubble>().LinesShown;Assert.AreEqual(1,count);
            player.transform.position=new Vector3(6050,1);yield return null;yield return null;
            Assert.AreEqual(count,a.GetComponent<LarperSpeechBubble>().LinesShown+b.GetComponent<LarperSpeechBubble>().LinesShown);
        }
        [UnityTest] public IEnumerator WitnessPanicRequiresVisibilityInterruptsCombatAndRecovers()
        {
            var witness=Enemy(6000);var source=Enemy(6003);
            var wall=Keep(new GameObject("WitnessWall"));wall.transform.position=new Vector3(6001.5f,1);wall.AddComponent<BoxCollider2D>().size=new Vector2(.3f,5);Physics2D.SyncTransforms();
            source.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;
            Assert.AreEqual(0,witness.GetComponent<LarperAwareness>().PanicReactions);
            Object.Destroy(wall);yield return null;var trigger=Keep(new GameObject("WitnessTrigger"));trigger.transform.position=new Vector3(6001.5f,1);trigger.AddComponent<BoxCollider2D>().isTrigger=true;
            source=Enemy(6003);Physics2D.SyncTransforms();source.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;
            Assert.AreEqual(1,witness.GetComponent<LarperAwareness>().PanicReactions);Assert.AreEqual(EngagementState.Panicked,witness.GetComponent<EnemyCombat>().State);
            StringAssert.Contains("SCARED",witness.GetComponent<LarperSpeechBubble>().LastLine);int shots=witness.GetComponent<AttackExecutor>().ShotsFired;
            yield return new WaitForSeconds(.1f);Assert.AreEqual(shots,witness.GetComponent<AttackExecutor>().ShotsFired);
            yield return new WaitForSeconds(.35f);Assert.AreEqual(EngagementState.Patrol,witness.GetComponent<EnemyCombat>().State);
        }
        [UnityTest] public IEnumerator OffscreenWitnessDoesNotGenerateSpeechOrPanic()
        {
            var witness=Enemy(6040);var source=Enemy(6043);Physics2D.SyncTransforms();source.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player));yield return null;
            Assert.AreEqual(0,witness.GetComponent<LarperAwareness>().PanicReactions);Assert.AreEqual(0,witness.GetComponent<LarperSpeechBubble>().LinesShown);
        }
    }
}
#endif
