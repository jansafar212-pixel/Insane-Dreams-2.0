#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Inventory;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Sprint0032Tests
    {
        const float X=9000;
        readonly List<GameObject> owned=new List<GameObject>();
        bool background;float volume;Random.State random;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        BoxCollider2D Solid(string name,Vector2 p,Vector2 size){var g=Keep(new GameObject(name));g.transform.position=p;var c=g.AddComponent<BoxCollider2D>();c.size=size;return c;}
        GameObject Actor(string name="Larper",float dx=0,float y=.72f,bool frozen=true)
        {
            var g=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/"+name+".prefab"),new Vector3(X+dx,y),Quaternion.identity));
            g.GetComponent<LarperBrain>().enabled=false;
            if(name=="Larper")g.GetComponent<LarperBrain>().Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Larper.asset"));
            if(frozen)g.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            g.GetComponent<EnemyCombat>().BindTarget(null);return g;
        }
        Health Target(float dx=6,float y=.72f)
        {
            var c=Solid("SensoryTarget",new Vector2(X+dx,y),new Vector2(.8f,1.4f));c.gameObject.layer=2;
            var hp=c.gameObject.AddComponent<Health>();var s=new SerializedObject(hp);s.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;s.FindProperty("maximum").floatValue=10000;s.ApplyModifiedPropertiesWithoutUndo();hp.Restore();return hp;
        }
        Camera CameraAt(float dx=3){var c=Keep(new GameObject("AIRegressionCamera")).AddComponent<Camera>();c.orthographic=true;c.orthographicSize=8;c.aspect=16f/9;c.transform.position=new Vector3(X+dx,4,-10);return c;}
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;random=Random.state;Random.InitState(3201);Solid("Floor",new Vector2(X,-.5f),new Vector2(100,1));yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in owned)if(g!=null)Object.Destroy(g);owned.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;Random.state=random;}

        [UnityTest]public IEnumerator InitialLarperDelayAndLongerBriefOcclusionRetainRecognitionWithoutChangingCadence()
        {
            var g=Actor();var c=g.GetComponent<EnemyCombat>();c.BindGameplayCamera(CameraAt());float first=-1,start=Time.time;g.GetComponent<AttackExecutor>().Fired+=(a,b,d)=>{if(first<0)first=Time.time;};c.BindTarget(Target());
            yield return new WaitForSeconds(.55f);Assert.Less(first,0);Assert.IsTrue(c.TargetVisible);
            float deadline=Time.time+2;while(first<0&&Time.time<deadline)yield return null;Assert.That(first-start,Is.InRange(.69f,1.5f));
            var wall=Solid("TemporarySightWall",new Vector2(X+3,2),new Vector2(.2f,5));Physics2D.SyncTransforms();yield return new WaitForSeconds(1.3f);Assert.IsFalse(c.TargetVisible);Vector2 remembered=c.LastKnownPosition;Assert.AreEqual(0,c.RecognitionRemaining);
            Object.Destroy(wall.gameObject);yield return new WaitForSeconds(.15f);Assert.IsTrue(c.TargetVisible);Assert.AreEqual(0,c.RecognitionRemaining,"Do not restart .7 seconds after a remembered encounter");Assert.AreEqual(remembered,c.LastKnownPosition);
        }
        [UnityTest]public IEnumerator OffscreenRecognitionDoesNotPermitInstantCameraEntryAttack()
        {
            var g=Actor();var c=g.GetComponent<EnemyCombat>();var camera=CameraAt(100);c.BindGameplayCamera(camera);c.BindTarget(Target());var attack=g.GetComponent<AttackExecutor>();
            yield return new WaitForSeconds(1.1f);Assert.IsTrue(c.TargetVisible);Assert.AreEqual(0,c.RecognitionRemaining);Assert.AreEqual(0,attack.ShotsFired);
            camera.transform.position=new Vector3(X+3,4,-10);yield return null;yield return new WaitForSeconds(.09f);Assert.AreEqual(0,attack.ShotsFired);Assert.IsFalse(c.MayInitiateFire);
            yield return new WaitForSeconds(.25f);Assert.Greater(attack.ShotsFired,0);Assert.AreEqual(0,c.RecognitionRemaining);
            camera.transform.position+=Vector3.right*100;yield return new WaitForSeconds(.1f);int count=attack.ShotsFired;yield return new WaitForSeconds(.25f);Assert.AreEqual(count,attack.ShotsFired);
        }
        [UnityTest]public IEnumerator UpperPlatformHearingIsApproximateThrottledLocalAndExpiresWithoutSight()
        {
            var g=Actor(y:4.2f);var c=g.GetComponent<EnemyCombat>();c.BindGameplayCamera(CameraAt(100));c.BindTarget(Target(3.1f));Solid("Deck",new Vector2(X+2,2.6f),new Vector2(20,.4f));Physics2D.SyncTransforms();yield return null;
            var noise=new CombatNoiseEvent(null,CombatTeam.Player,CombatNoiseKind.Gunfire,new Vector2(X+3.1f,.7f),new Vector2(X+3.1f,.7f),1);
            for(int i=0;i<30;i++)CombatNoise.Report(noise);
            Assert.AreEqual(1,c.NoiseEventsHeard);Assert.AreEqual(ThreatAwareness.Suspicious,c.Awareness);Assert.Greater(Vector2.Distance(c.HeardPosition,noise.Position),.1f);Assert.IsFalse(c.TargetVisible);Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);Vector2 heard=c.HeardPosition;
            yield return new WaitForSeconds(.3f);CombatNoise.Report(new CombatNoiseEvent(null,CombatTeam.Player,CombatNoiseKind.Explosion,new Vector2(X+80,1),new Vector2(X+80,1),1));Assert.AreEqual(heard,c.HeardPosition);
            yield return new WaitForSeconds(4.1f);Assert.AreEqual(ThreatAwareness.Unaware,c.Awareness);Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);
        }
        [UnityTest]public IEnumerator SmokeDoesNotMakeDistantAudibleGunfireSilentAndAlliedNoiseDoesNotRebroadcast()
        {
            var g=Actor();var c=g.GetComponent<EnemyCombat>();yield return null;
            var cloud=Keep(new GameObject("SoundThroughSmoke"));cloud.transform.position=new Vector2(X+7,.7f);cloud.AddComponent<SmokeCloud2D>().Activate(2,10);
            int broadcasts=0;System.Action<CombatNoiseEvent> observe=_=>broadcasts++;CombatNoise.Reported+=observe;
            try{CombatNoise.Report(new CombatNoiseEvent(null,CombatTeam.Enemy,CombatNoiseKind.Gunfire,new Vector2(X+15,.7f),new Vector2(X+15,.7f),1));Assert.AreEqual(1,broadcasts);Assert.AreEqual(1,c.NoiseEventsHeard);Assert.AreEqual(0,c.Suppression);Assert.AreEqual(ThreatAwareness.Suspicious,c.Awareness);}
            finally{CombatNoise.Reported-=observe;}
        }
        [UnityTest]public IEnumerator StuckRecoveryIsBoundedDoesNotTriggerWhileStationaryAndRecoversAfterWallRemoval()
        {
            var g=Actor(frozen:false);g.GetComponent<EnemyCombat>().enabled=false;var brain=g.GetComponent<LarperBrain>();brain.enabled=true;brain.DecisionsSuspended=true;brain.SetPursuit(0);yield return new WaitForSeconds(2);Assert.AreEqual(0,brain.StuckRecoveries);
            var wall=Solid("ImpossibleWall",new Vector2(X+1,3),new Vector2(.7f,6));Physics2D.SyncTransforms();brain.SetPursuit(1);yield return new WaitForSeconds(4.5f);
            Assert.That(brain.StuckRecoveries,Is.InRange(1,2));Assert.Less(g.transform.position.x,X+.6f);Assert.Less(g.transform.position.y,1);int count=brain.StuckRecoveries;
            brain.SetPursuit(0);yield return new WaitForSeconds(2);Assert.AreEqual(count,brain.StuckRecoveries);
            Object.Destroy(wall.gameObject);yield return null;brain.SetPursuit(1);yield return new WaitForSeconds(2);Assert.Greater(g.transform.position.x,X+1,"Resume physical movement when the route becomes reachable");
        }
        [UnityTest]public IEnumerator PlatformSupportIgnoresLadderTriggersAndFailedEdgesRetryAfterCooldown()
        {
            var deck=Solid("UpperStep",new Vector2(X+4,1.8f),new Vector2(3,.4f));var trigger=Solid("LadderTrigger",new Vector2(X,2),new Vector2(2,6));trigger.isTrigger=true;trigger.gameObject.AddComponent<LadderVolume>();Physics2D.SyncTransforms();
            var routes=new SamuraiPlatformRoutes();Vector2 start=new Vector2(X,.7f),goal=new Vector2(X+4,2.7f);
            Assert.IsTrue(routes.Select(start,goal,.7f,1,3,5,8.5f,6.5f),routes.LastFailure);Assert.IsFalse(routes.Source.isTrigger);Assert.AreEqual(deck,routes.Destination);
            routes.Fail(routes.Source,deck,.3f,"Controlled blocked edge");Assert.IsFalse(routes.Select(start,goal,.7f,1,3,5,8.5f,6.5f));yield return new WaitForSeconds(.35f);Assert.IsTrue(routes.Select(start,goal,.7f,1,3,5,8.5f,6.5f));
        }
        [UnityTest]public IEnumerator SamuraiRecognitionDelaysPursuitBrieflyThenRemainsAggressive()
        {
            var g=Actor("Samurai",y:.53f,frozen:false);var c=g.GetComponent<EnemyCombat>();c.BindGameplayCamera(CameraAt());c.BindTarget(Target(8));g.GetComponent<LarperBrain>().enabled=true;float x=g.transform.position.x;
            yield return new WaitForSeconds(.12f);Assert.Less(Mathf.Abs(g.transform.position.x-x),.08f);Assert.Greater(c.RecognitionRemaining,0);
            yield return new WaitForSeconds(.55f);Assert.IsTrue(c.HasEngaged);Assert.Greater(g.transform.position.x,x+.25f);
        }
        [UnityTest]public IEnumerator LocalSimultaneousNoiseDoesNotBecomeAnAlertStorm()
        {
            var actors=new EnemyCombat[16];for(int i=0;i<actors.Length;i++)actors[i]=Actor(dx:i*.25f).GetComponent<EnemyCombat>();yield return null;
            int broadcasts=0;System.Action<CombatNoiseEvent> observe=_=>broadcasts++;CombatNoise.Reported+=observe;
            try{for(int i=0;i<20;i++)CombatNoise.Report(new CombatNoiseEvent(null,CombatTeam.Enemy,CombatNoiseKind.Gunfire,new Vector2(X+3,2),new Vector2(X+3,2),1));Assert.AreEqual(20,broadcasts);foreach(var c in actors){Assert.AreEqual(1,c.NoiseEventsHeard);Assert.AreEqual(0,c.Suppression);Assert.IsFalse(c.TargetVisible);}}
            finally{CombatNoise.Reported-=observe;}
        }
        [UnityTest]public IEnumerator RealGunshotDispatchReachesListenerAboveSolidDeckWithoutGrantingSight()
        {
            var g=Actor(y:4.2f);var c=g.GetComponent<EnemyCombat>();var target=Target(3.1f);c.BindTarget(target);c.BindGameplayCamera(CameraAt());Solid("OccludingDeck",new Vector2(X+2,2.6f),new Vector2(20,.4f));Physics2D.SyncTransforms();yield return null;
            var executor=target.gameObject.AddComponent<AttackExecutor>();var gun=new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),24);
            Assert.IsTrue(executor.Attack(gun,Vector2.right,Time.time));Assert.AreEqual(1,c.NoiseEventsHeard);Assert.AreEqual(CombatNoiseKind.Gunfire,c.LastHeardKind);Assert.IsFalse(c.TargetVisible);Assert.AreEqual(0,g.GetComponent<AttackExecutor>().ShotsFired);yield return null;
        }
        [UnityTest]public IEnumerator SmallSamuraiPhysicallyTraversesIntermediatePlatformWithoutSnapping()
        {
            Solid("LaunchDeck",new Vector2(X+.5f,5.2f),new Vector2(3,.4f));Solid("IntermediateDeck",new Vector2(X+6,7.1f),new Vector2(4,.4f));Solid("FinalDeck",new Vector2(X+14.5f,9.1f),new Vector2(4,.4f));
            var g=Actor("Samurai",dx:1.5f,y:5.93f,frozen:false);var c=g.GetComponent<EnemyCombat>();var target=Target(6,8.15f);c.BindTarget(target);c.BindGameplayCamera(CameraAt(8));var brain=g.GetComponent<LarperBrain>();brain.Configure(AssetDatabase.LoadAssetAtPath<EnemyArchetype>("Assets/Data/Enemies/Samurai.asset"));brain.enabled=true;Physics2D.SyncTransforms();
            var samurai=g.GetComponent<SamuraiCombat>();var body=g.GetComponent<Rigidbody2D>();float deadline=Time.time+18,last=body.position.x;bool intermediate=false,reached=false;var trace=new System.Text.StringBuilder();float sample=0;
            while(Time.time<deadline)
            {
                yield return new WaitForFixedUpdate();Assert.Less(Mathf.Abs(body.position.x-last),1,"No teleporting between platforms");last=body.position.x;
                if(Time.time>=sample){sample=Time.time+.5f;trace.Append(body.position+" "+c.State+" "+samurai.FailedTransitionReason+"; ");}
                if(g.GetComponent<PlayerMotor2D>().IsGrounded)
                {
                    if(!intermediate&&body.position.y>7.7f&&body.position.y<8.5f){intermediate=true;target.transform.position=new Vector2(X+14.5f,10.15f);Physics2D.SyncTransforms();}
                    reached=body.position.y>9.7f;if(reached)break;
                }
            }
            Assert.IsTrue(intermediate,"Route must land on the intermediate deck: "+trace);Assert.IsTrue(reached,trace.ToString());Assert.GreaterOrEqual(samurai.PlatformJumps,2);
        }
    }
}
#endif
