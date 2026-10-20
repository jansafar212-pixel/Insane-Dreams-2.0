#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using InsaneDreams.CameraSystem;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Input;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint020Tests
    {
        private readonly List<GameObject> objects=new List<GameObject>();private Camera camera;private UIDocument ui;private LarperPersonalityProfile profile;private bool background;private Random.State random;
        private GameObject Keep(GameObject go){objects.Add(go);return go;}
        private void Float(Object target,string key,float value){var s=new SerializedObject(target);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        private void Private(Object target,string key,object value)=>target.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
        private GameObject Solid(Vector2 p,Vector2 size){var go=Keep(new GameObject("S020Geometry"));go.transform.position=p;go.AddComponent<BoxCollider2D>().size=size;Physics2D.SyncTransforms();return go;}
        [UnitySetUp] public IEnumerator Setup()
        {
            random=Random.state;background=Application.runInBackground;Application.runInBackground=true;LarperAwareness.ResetEncounterSpeech();
            typeof(LarperAwareness).GetField("nextCollapse",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,0f);
            Solid(new Vector2(12000,-.5f),new Vector2(160,1));camera=Keep(new GameObject("S020Camera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=20;camera.aspect=2;camera.transform.position=new Vector3(12000,2,-10);
            ui=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();
            profile=Object.Instantiate(AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Ordinary.asset"));Float(profile,"panicChance",0);Float(profile,"moraleCooldown",1);yield return null;yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();Object.Destroy(profile);yield return null;
            typeof(LarperAwareness).GetField("nextCollapse",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,0f);Random.state=random;Application.runInBackground=background;
        }
        private EnemyCombat Enemy(float x=12000,bool freeze=true)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(x,.71f),Quaternion.identity));go.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));if(freeze)go.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            var a=go.GetComponent<LarperAwareness>();a.Configure(profile);go.GetComponent<LarperSpeechBubble>().Bind(camera,ui);Float(a,"socialFearChance",1);Float(a,"socialConfidenceChance",0);Float(a,"collapseChance",0);var e=go.GetComponent<EnemyCombat>();e.BindGameplayCamera(camera);return e;
        }
        private GameObject Player(float x=11992)
        {
            var go=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(x,.85f),Quaternion.identity));go.GetComponent<Health>().ProtectFor(60);return go;
        }
        private void Defeat(EnemyCombat e){Assert.IsTrue(e.GetComponent<Health>().ApplyDamage(new DamageInfo(1000,CombatTeam.Player)));var doll=e.GetComponent<RagdollOnDefeat>().LastRagdoll;if(doll!=null)Keep(doll.gameObject);}
        [UnityTest] public IEnumerator OffscreenAllyFearUsesActualObservationAndExistingCrouchWithoutGrantingShots()
        {
            var witness=Enemy();Float(witness,"defensiveCrouchProbability",1);var source=Enemy(12003);camera.transform.position=new Vector3(11800,2,-10);yield return new WaitForSeconds(.2f);Defeat(source);yield return new WaitForSeconds(.1f);
            Assert.AreEqual(-1,witness.MoraleInfluence);Assert.IsTrue(witness.IsDefensivelyCrouching);Assert.IsTrue(witness.GetComponent<InsaneDreams.Traversal.PlayerPosture2D>().IsCrouched);Assert.IsFalse(witness.TargetVisible);Assert.IsFalse(witness.IsBlindFiring);Assert.AreEqual(0,witness.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest] public IEnumerator ForcedCollapseHasThreeRecipientsAtMostAndGlobalCooldownPreventsAnotherWave()
        {
            Float(profile,"panicChance",1);Float(profile,"panicSeconds",.2f);var root=Enemy();var a=root.GetComponent<LarperAwareness>();Float(a,"collapseChance",1);
            var peers=Enumerable.Range(1,7).Select(i=>Enemy(12000-i*1.2f)).ToArray();foreach(var e in peers){Float(e.GetComponent<LarperAwareness>(),"socialViewHalfAngle",0);Float(e.GetComponent<LarperAwareness>(),"collapseSpreadChance",1);}
            var source=Enemy(12003);yield return new WaitForSeconds(.2f);Defeat(source);Assert.AreEqual(3,peers.Sum(e=>e.GetComponent<LarperAwareness>().CollapseReactions));Assert.AreEqual(1,a.PanicReactions);
            yield return new WaitForSeconds(1.6f);int total=peers.Sum(e=>e.GetComponent<LarperAwareness>().CollapseReactions);Private(a,"nextMorale",0f);source=Enemy(12003);Defeat(source);Assert.AreEqual(total,peers.Sum(e=>e.GetComponent<LarperAwareness>().CollapseReactions));
            yield return new WaitForSeconds(5.3f);Assert.IsTrue(peers.All(e=>e.MoraleInfluence==0&&e.State!=EngagementState.Panicked));
        }
        [UnityTest] public IEnumerator CollapseCannotCrossSolidWallsOrReachDistantPeers()
        {
            Float(profile,"panicChance",1);var root=Enemy();Float(root.GetComponent<LarperAwareness>(),"collapseChance",1);var blocked=Enemy(11996);var far=Enemy(11980);
            foreach(var e in new[]{blocked,far}){Float(e.GetComponent<LarperAwareness>(),"collapseSpreadChance",1);Float(e.GetComponent<LarperAwareness>(),"socialViewHalfAngle",0);}
            Solid(new Vector2(11998,2),new Vector2(.3f,5));var source=Enemy(12003);yield return new WaitForSeconds(.2f);Defeat(source);Assert.AreEqual(0,blocked.GetComponent<LarperAwareness>().CollapseReactions);Assert.AreEqual(0,far.GetComponent<LarperAwareness>().CollapseReactions);
        }
        [UnityTest] public IEnumerator SpeechHardCapReadabilityAndNoRapidReplacementAreEnforcedAtDisplayBoundary()
        {
            var actors=new[]{Enemy(12000),Enemy(12002),Enemy(12004)};yield return null;var bubbles=actors.Select(e=>e.GetComponent<LarperSpeechBubble>()).ToArray();Assert.IsTrue(bubbles[0].TryShow("STAND YOUR GROUND!"));Assert.IsTrue(bubbles[1].TryShow("I FEAR NOTHING!"));Assert.IsFalse(bubbles[2].TryShow("THIRD",true));Assert.AreEqual(2,LarperSpeechBubble.VisibleCount);
            yield return new WaitForSeconds(2.9f);Assert.IsTrue(bubbles[0].IsVisible);Assert.IsFalse(bubbles[0].TryShow("REPLACEMENT"));Assert.AreEqual("STAND YOUR GROUND!",bubbles[0].LastLine);
            yield return new WaitForSeconds(.5f);Assert.IsTrue(bubbles[2].TryShow("TACTICAL WITHDRAWAL!"));Assert.LessOrEqual(LarperSpeechBubble.VisibleCount,2);
        }
        [UnityTest] public IEnumerator TouchPadsGrowWithoutOverlapsAndHealClickUsesExistingMechanic()
        {
            var player=Player();player.GetComponent<PlayerHealthRecovery>().enabled=false;player.GetComponent<Health>().ProtectFor(0);ui.GetComponent<WarehouseTouchControls>().Bind(player.GetComponent<PlayerActions>());Assert.IsTrue(player.GetComponent<Health>().ApplyDamage(new DamageInfo(70,CombatTeam.Enemy)));
            var root=ui.rootVisualElement;var names=new[]{"moveStick","aimStick","heal","reload","parachuteButton"};
            foreach(var size in new[]{new Vector2(1280,720),new Vector2(960,720),new Vector2(1560,720),new Vector2(720,1280)})
            {
                root.style.width=size.x;root.style.height=size.y;yield return null;yield return null;
                var pads=names.Select(n=>root.Q<VisualElement>(n)).ToArray();foreach(var pad in pads){Assert.IsNotNull(pad);Assert.AreEqual(.5f,pad.resolvedStyle.opacity,.01f);}
                // UI Toolkit resolves dimensions onto the current panel's pixel grid.
                Assert.AreEqual(187.5f,pads[0].resolvedStyle.width,1f);Assert.AreEqual(187.5f,pads[1].resolvedStyle.width,1f);
                for(int i=0;i<pads.Length;i++)for(int j=i+1;j<pads.Length;j++)Assert.IsFalse(pads[i].worldBound.Overlaps(pads[j].worldBound),names[i]+" overlaps "+names[j]+" at "+size);
            }
            var button=root.Q<Button>("heal");using(var click=ClickEvent.GetPooled()){click.target=button;button.SendEvent(click);}Assert.AreEqual(65,player.GetComponent<Health>().Current);Assert.Greater(player.GetComponent<PlayerHealing>().CooldownRemaining,0);
        }
        [UnityTest] public IEnumerator AndroidFramingIsTwelveAndAHalfPercentCloserWithoutChangingPcSize()
        {
            var player=Player(12000);var follow=camera.gameObject.AddComponent<WarehouseCameraFollow>();var s=new SerializedObject(follow);s.FindProperty("levelMin").vector2Value=new Vector2(11940,-9);s.FindProperty("levelMax").vector2Value=new Vector2(12060,30);s.ApplyModifiedPropertiesWithoutUndo();follow.Bind(player.transform);float pc=camera.orthographicSize;
            s.Update();s.FindProperty("previewAndroidFraming").boolValue=true;s.ApplyModifiedPropertiesWithoutUndo();follow.SnapToTarget();Assert.AreEqual(1.125f,pc/camera.orthographicSize,.001f);s.Update();s.FindProperty("previewAndroidFraming").boolValue=false;s.ApplyModifiedPropertiesWithoutUndo();follow.SnapToTarget();Assert.AreEqual(pc,camera.orthographicSize,.001f);yield return null;
        }
        [UnityTest] public IEnumerator MovingNeighborsSeparateWithoutPassingThroughOneAnotherAndElevationsRemainIndependent()
        {
            var player=Player();var a=Enemy(12008,false);var b=Enemy(12008.1f,false);a.BindTarget(player.GetComponent<Health>());b.BindTarget(player.GetComponent<Health>());yield return new WaitForSeconds(2);
            Assert.Greater(Vector2.Distance(a.transform.position,b.transform.position),1.2f);Assert.Less(a.transform.position.x,b.transform.position.x);Assert.IsTrue(a.HasCombatPosition);Assert.IsTrue(b.HasCombatPosition);
            var high=Enemy(12008);high.GetComponent<Rigidbody2D>().position=new Vector2(12008,5);Physics2D.SyncTransforms();Assert.IsTrue(high.CanAdvance(-1),"Different platforms must not form a crowd obstacle.");
        }
        [UnityTest] public IEnumerator RagdollImpulseIncreasesFifteenPercentWhileVelocityLimitsStayIntact()
        {
            var actor=Enemy();var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Visuals/LarperCartoonRagdoll.prefab");var old=Keep(Object.Instantiate(prefab)).GetComponent<CartoonRagdoll>();var tuned=Keep(Object.Instantiate(prefab)).GetComponent<CartoonRagdoll>();old.Activate(actor.GetComponent<CharacterVisualDriver>().Rig,Vector2.zero);tuned.Activate(actor.GetComponent<CharacterVisualDriver>().Rig,Vector2.zero);
            Float(old,"initialDefeatMultiplier",2.6f);foreach(var rag in new[]{old,tuned})foreach(var part in rag.parts)part.linearVelocity=Vector2.zero;
            old.ReceiveDefeatImpact(new PhysicalImpact(old.parts[0].worldCenterOfMass,Vector2.right,.3f,old.parts[0]));tuned.ReceiveDefeatImpact(new PhysicalImpact(tuned.parts[0].worldCenterOfMass,Vector2.right,.3f,tuned.parts[0]));Assert.AreEqual(1.15f,tuned.parts[0].linearVelocity.x/old.parts[0].linearVelocity.x,.01f);
            tuned.ReceiveDefeatImpact(new PhysicalImpact(tuned.parts[0].worldCenterOfMass,Vector2.up,1000,tuned.parts[0]));yield return new WaitForSeconds(.3f);foreach(var part in tuned.parts)Assert.LessOrEqual(part.linearVelocity.magnitude,12.01f);
        }
    }
}
#endif
