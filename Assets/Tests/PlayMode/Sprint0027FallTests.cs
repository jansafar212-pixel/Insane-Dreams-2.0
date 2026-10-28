#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using InsaneDreams.Combat;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class Sprint0027FallTests
    {
        readonly List<GameObject> objects=new List<GameObject>();bool background;float scale,volume;GameObject actor;
        [UnitySetUp]public IEnumerator Setup(){background=Application.runInBackground;Application.runInBackground=true;scale=Time.timeScale;Time.timeScale=3;volume=AudioListener.volume;AudioListener.volume=0;var floor=new GameObject("FallTestFloor");objects.Add(floor);floor.transform.position=new Vector2(28000,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(40,1);yield return null;}
        [UnityTearDown]public IEnumerator Cleanup(){foreach(var g in objects)if(g!=null)Object.Destroy(g);objects.Clear();yield return null;Time.timeScale=scale;Application.runInBackground=background;AudioListener.volume=volume;}
        IEnumerator Drop(float height,bool parachute,bool late=false)
        {
            actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(28000,height+.85f),Quaternion.identity);objects.Add(actor);var hp=actor.GetComponent<Health>();hp.ProtectFor(0);var fall=actor.GetComponent<PlayerFallDamage>();Assert.NotNull(fall);var chute=actor.GetComponent<PlayerTraversal2D>();if(parachute&&!late)Assert.IsTrue(chute.ToggleParachute());
            float until=Time.time+30;bool deployed=!late;while(fall.Landings==0&&Time.time<until)
            {if(late&&!deployed&&actor.transform.position.y<3){deployed=chute.ToggleParachute();}yield return new WaitForFixedUpdate();}Assert.AreEqual(1,fall.Landings);if(parachute){Assert.IsTrue(deployed);Assert.AreEqual(100,hp.Current);Assert.AreEqual(0,fall.LastDamage);}else if(height<8)Assert.AreEqual(100,hp.Current);else if(height>30)Assert.IsTrue(hp.IsDefeated);else{Assert.Greater(fall.LastDamage,0);Assert.Greater(hp.Current,0);}
        }
        [UnityTest]public IEnumerator SmallFallIsSafe(){yield return Drop(3,false);}
        [UnityTest]public IEnumerator MediumFallHurtsAndExtremeFallCanDefeat(){yield return Drop(12,false);Object.Destroy(actor);yield return null;yield return Drop(40,false);}
        [UnityTest]public IEnumerator MediumAndExtremeParachuteLandingsHaveExactlyZeroDamage(){yield return Drop(12,true);Object.Destroy(actor);yield return null;yield return Drop(40,true);}
        [UnityTest]public IEnumerator LateParachuteClearsPendingImpactButDoesNotBlockCombatDamage()
        {yield return Drop(40,true,true);var hp=actor.GetComponent<Health>();actor.GetComponent<PlayerTraversal2D>().ClearParachute();yield return new WaitForSeconds(.5f);Assert.AreEqual(100,hp.Current);Assert.IsTrue(hp.ApplyDamage(new DamageInfo(9,CombatTeam.Enemy)));Assert.AreEqual(91,hp.Current);}
        [UnityTest]public IEnumerator ActiveAirborneParachuteAllowsCombatDamageAndLandingAddsNone()
        {
            actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"),new Vector3(28000,12.85f),Quaternion.identity);objects.Add(actor);var hp=actor.GetComponent<Health>();hp.ProtectFor(0);Assert.IsTrue(actor.GetComponent<PlayerTraversal2D>().ToggleParachute());yield return new WaitForFixedUpdate();Assert.IsTrue(hp.ApplyDamage(new DamageInfo(9,CombatTeam.Enemy)));var fall=actor.GetComponent<PlayerFallDamage>();float end=Time.time+20;while(fall.Landings==0&&Time.time<end)yield return new WaitForFixedUpdate();Assert.AreEqual(1,fall.Landings);Assert.AreEqual(91,hp.Current);Assert.AreEqual(0,fall.LastDamage);
        }
        [UnityTest]public IEnumerator NormalDoubleJumpReturnsToGroundWithoutFallDamage()
        {
            yield return Drop(3,false);var actions=actor.GetComponent<InsaneDreams.Input.PlayerActions>();var motor=actor.GetComponent<InsaneDreams.Prototype.PlayerMotor2D>();var fall=actor.GetComponent<PlayerFallDamage>();int before=fall.Landings;actions.Jump();yield return new WaitForSeconds(.2f);actions.Jump();yield return new WaitForFixedUpdate();Assert.AreEqual(2,motor.JumpsUsed);float end=Time.time+10;while(fall.Landings==before&&Time.time<end)yield return new WaitForFixedUpdate();Assert.Greater(fall.Landings,before);Assert.AreEqual(100,actor.GetComponent<Health>().Current);Assert.AreEqual(0,fall.LastDamage);
        }
    }
}
#endif
