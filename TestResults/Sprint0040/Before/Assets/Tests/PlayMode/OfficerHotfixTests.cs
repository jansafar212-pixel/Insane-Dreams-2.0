#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using InsaneDreams.Combat;
using InsaneDreams.Enemies;
using InsaneDreams.Visuals;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class OfficerHotfixTests
    {
        readonly List<GameObject> objects=new List<GameObject>();
        GameObject actor;EnemyCombat combat;OfficerShotTelegraph telegraph;CharacterVisualDriver driver;CutoutRig rig;Camera camera;
        bool background;float volume,scale;Random.State random;
        GameObject Keep(GameObject go){objects.Add(go);return go;}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;scale=Time.timeScale;Time.timeScale=1;random=Random.state;Random.InitState(7081);
            var floor=Keep(new GameObject("OfficerTestFloor"));floor.transform.position=new Vector2(10000,9998.75f);floor.AddComponent<BoxCollider2D>().size=new Vector2(80,1);
            actor=Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Data/FirstOperation/Officer.prefab"),new Vector3(10000,10000),Quaternion.identity));
            combat=actor.GetComponent<EnemyCombat>();combat.BindTarget(null);actor.GetComponent<LarperBrain>().enabled=false;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
            telegraph=actor.GetComponent<OfficerShotTelegraph>();driver=actor.GetComponent<CharacterVisualDriver>();rig=driver.Rig;
            camera=Keep(new GameObject("OfficerTestCamera")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=6;camera.aspect=16f/9;camera.transform.position=new Vector3(10003,10000,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.15f,.18f,.2f);combat.BindGameplayCamera(camera);
            yield return null;yield return new WaitForFixedUpdate();
        }
        [UnityTearDown]public IEnumerator Cleanup()
        {Time.timeScale=scale;foreach(var go in objects)if(go!=null)Object.Destroy(go);objects.Clear();yield return null;Application.runInBackground=background;AudioListener.volume=volume;Random.state=random;}
        void Capture(string name)
        {
            var old=RenderTexture.active;var previous=camera.targetTexture;var rt=new RenderTexture(960,540,24){filterMode=FilterMode.Point};camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var t=new Texture2D(960,540,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,960,540),0,0);t.Apply();File.WriteAllBytes("TestResults/OfficerHotfix/"+name+".png",t.EncodeToPNG());camera.targetTexture=previous;RenderTexture.active=old;Object.Destroy(t);Object.Destroy(rt);
        }
        [UnityTest]public IEnumerator LaserReachesViewportInAllDirectionsAndAspectRatiosButStopsAtCover()
        {
            objects[0].GetComponent<BoxCollider2D>().enabled=false;
            foreach(float aspect in new[]{4f/3,16f/9,20f/9})foreach(var aim in new[]{Vector2.right,Vector2.left,Vector2.up,Vector2.down})
            {
                camera.aspect=aspect;telegraph.Cancel();for(int i=0;i<3;i++){driver.PrepareShot(aim);telegraph.AllowShot(aim,0,2,out _);yield return new WaitForEndOfFrame();}
                Assert.IsTrue(telegraph.LaserVisible);var end=camera.WorldToViewportPoint(telegraph.LaserEnd);Assert.IsTrue(end.x<=0||end.x>=1||end.y<=0||end.y>=1,"Unblocked beam must cross the viewport boundary");
                var line=actor.transform.Find("OfficerRedLaser").GetComponent<LineRenderer>();Assert.Less(Vector2.Distance(line.GetPosition(0),actor.GetComponent<AttackExecutor>().Origin),.002f);
                Assert.Less(Vector2.Distance(rig.weaponMount.position,rig.Segmented.grip.position),.002f,"Weapon must remain attached to the articulated hand");
            }
            telegraph.Cancel();camera.aspect=16f/9;camera.orthographicSize=2.5f;driver.PrepareShot(Vector2.right);var origin=actor.GetComponent<AttackExecutor>().Origin;
            var wall=Keep(new GameObject("OfficerLaserWall"));wall.transform.position=origin+Vector2.right*3;wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,4);Physics2D.SyncTransforms();
            for(int i=0;i<3;i++){driver.PrepareShot(Vector2.right);telegraph.AllowShot(Vector2.right,0,2,out _);yield return new WaitForEndOfFrame();}
            Assert.AreEqual(wall.transform.position.x-.1f,telegraph.LaserEnd.x,.01f);Capture("LaserCover");
        }
        [UnityTest]public IEnumerator HalfSecondWarningIsVisualOnlyAndActualMagentaShotIsTwicePistolWidthThenFades()
        {
            var executor=actor.GetComponent<AttackExecutor>();int ammo=combat.Weapon.MagazineAmmo,noises=0;
            System.Action<CombatNoiseEvent> hear=n=>{if(n.Source==actor)noises++;};CombatNoise.Reported+=hear;
            try
            {
                Assert.AreEqual(.5f,new SerializedObject(telegraph).FindProperty("windupSeconds").floatValue);
                float start=Time.time;Vector2 shot;while(!telegraph.AllowShot(Vector2.right,0,20,out shot))
                {Assert.AreEqual(ammo,combat.Weapon.MagazineAmmo);Assert.AreEqual(0,executor.ShotsFired);Assert.AreEqual(0,noises);yield return new WaitForEndOfFrame();Assert.IsTrue(telegraph.LaserVisible);}
                Assert.That(Time.time-start,Is.InRange(.5f,.5f+Mathf.Max(.1f,Time.deltaTime*2)));
                Assert.IsTrue(executor.Attack(combat.Weapon,shot,Time.time));telegraph.ShotFired();Assert.IsFalse(telegraph.LaserVisible);Assert.AreEqual(ammo-1,combat.Weapon.MagazineAmmo);Assert.AreEqual(1,noises);
                var baseline=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab").GetComponent<AttackExecutor>();float normal=new SerializedObject(baseline).FindProperty("tracerWidth").floatValue;
                var lines=actor.GetComponentsInChildren<LineRenderer>();LineRenderer trail=null;foreach(var line in lines)if(line.enabled&&line.gameObject.name!="OfficerRedLaser")trail=line;
                Assert.NotNull(trail);Assert.That(trail.startWidth,Is.InRange(normal*2*.85f,normal*2*1.15f));Assert.Greater(trail.startColor.r,.95f);Assert.Greater(trail.startColor.b,.95f);Assert.Less(trail.startColor.g,.26f);
                float alpha=trail.startColor.a;yield return new WaitForSeconds(.07f);Assert.Less(trail.startColor.a,alpha);yield return new WaitForSeconds(.15f);Assert.IsFalse(trail.enabled);
                telegraph.AllowShot(Vector2.left,0,20,out _);telegraph.Cancel();yield return new WaitForEndOfFrame();Assert.IsFalse(telegraph.LaserVisible);Assert.AreEqual(1,executor.ShotsFired);
            }
            finally{CombatNoise.Reported-=hear;}
        }
        [UnityTest]public IEnumerator SevenTelegraphedShotsThenVisibleReloadNoEarlyRefillOrReloadFire()
        {
            Time.timeScale=3;
            var target=Keep(new GameObject("OfficerTarget"));target.layer=2;target.transform.position=new Vector2(10008,10000);target.AddComponent<BoxCollider2D>().size=new Vector2(.8f,1.6f);var hp=target.AddComponent<Health>();var h=new SerializedObject(hp);h.FindProperty("team").enumValueIndex=(int)CombatTeam.Player;h.FindProperty("maximum").floatValue=10000;h.ApplyModifiedPropertiesWithoutUndo();hp.Restore();
            var settings=new SerializedObject(telegraph);settings.FindProperty("retreatDistance").floatValue=0;settings.ApplyModifiedPropertiesWithoutUndo();settings=new SerializedObject(combat);settings.FindProperty("closeSpread").floatValue=settings.FindProperty("distantSpread").floatValue=0;settings.ApplyModifiedPropertiesWithoutUndo();combat.BindTarget(hp);
            int shots=0;float lastReloadEnd=0;var executor=actor.GetComponent<AttackExecutor>();executor.Fired+=(a,b,c)=>{shots++;Assert.GreaterOrEqual(Time.time-telegraph.WarningStartedAt,.49f);if(shots<=7)Assert.AreEqual(7-shots,combat.Weapon.MagazineAmmo);Assert.IsFalse(combat.Weapon.Cycle.IsReloading);if(lastReloadEnd>0)Assert.GreaterOrEqual(Time.time-lastReloadEnd,.49f);};
            float deadline=Time.time+65;while(!combat.Weapon.Cycle.IsReloading&&Time.time<deadline)yield return null;
            Assert.AreEqual(7,shots);Assert.AreEqual(0,combat.Weapon.MagazineAmmo);Assert.AreEqual(0,combat.ReloadFraction);Assert.AreEqual(10000-7*55,hp.Current,.2f);
            var cycle=combat.Weapon.Cycle;Assert.AreEqual(2.8f,cycle.ReloadEndsAt-cycle.ReloadStartedAt,.01f);Assert.IsFalse(cycle.TryReload(Time.time));
            Assert.IsFalse(executor.Attack(combat.Weapon,Vector2.right,Time.time));
            var presenter=actor.GetComponent<PistolMagazineReload>();while(cycle.IsReloading&&Time.time<deadline)
            {
                Assert.AreEqual(0,combat.Weapon.MagazineAmmo);Assert.AreEqual(7,shots);float p=cycle.ReloadProgress(Time.time);
                if(p>.68f&&p<.8f){Assert.IsFalse(rig.magazine.enabled);Assert.IsTrue(rig.weapon.transform.Find("PistolReplacementMagazine").GetComponent<SpriteRenderer>().enabled);}
                yield return null;
            }
            lastReloadEnd=Time.time;yield return new WaitForEndOfFrame();Assert.AreEqual(7,combat.Weapon.MagazineAmmo);Assert.AreEqual(128,combat.Weapon.ReserveAmmo);Assert.AreEqual(1,presenter.Releases);Assert.IsTrue(rig.magazine.enabled);Assert.IsFalse(rig.weapon.transform.Find("PistolReplacementMagazine").GetComponent<SpriteRenderer>().enabled);
            while(shots<8&&Time.time<deadline)yield return null;Assert.AreEqual(8,shots);Assert.AreEqual(6,combat.Weapon.MagazineAmmo);
        }
        [UnityTest]public IEnumerator OneHandedRecoilAndMagazineExchangeStayAttachedBothFacingsAndCancelSafely()
        {
            camera.orthographicSize=1.7f;camera.transform.position=new Vector3(10000.15f,10000,-10);
            foreach(float facing in new[]{1f,-1f})
            {
                var body=actor.GetComponent<Rigidbody2D>();body.linearVelocity=Vector2.right*facing;driver.PrepareShot(Vector2.right*facing);body.linearVelocity=Vector2.zero;
                combat.Weapon.SetAmmunition(7,135);var executor=actor.GetComponent<AttackExecutor>();while(Time.time<combat.Weapon.Cycle.NextAttackAt)yield return null;
                driver.PrepareShot(Vector2.right*facing);float before=rig.weaponMount.localEulerAngles.z;Assert.IsTrue(executor.Attack(combat.Weapon,Vector2.right*facing,Time.time));
                yield return new WaitForSeconds(.07f);yield return new WaitForEndOfFrame();Assert.Greater(actor.GetComponent<OfficerWeaponPresentation>().RecoilAmount,.7f);Assert.Greater(Mathf.Abs(Mathf.DeltaAngle(before,rig.weaponMount.localEulerAngles.z)),20);Assert.Less(Vector2.Distance(rig.weaponMount.position,rig.Segmented.grip.position),.002f);Capture(facing>0?"RecoilRight":"RecoilLeft");
                yield return new WaitForSeconds(.55f);Assert.AreEqual(0,actor.GetComponent<OfficerWeaponPresentation>().RecoilAmount);combat.Weapon.SetAmmunition(0,135);Assert.IsTrue(combat.Weapon.Cycle.TryReload(Time.time));yield return new WaitForSeconds(2.05f);yield return new WaitForEndOfFrame();Capture(facing>0?"ReloadRight":"ReloadLeft");Assert.Less(Vector2.Distance(rig.weaponMount.position,rig.Segmented.grip.position),.002f);
                combat.enabled=false;yield return new WaitForEndOfFrame();Assert.IsFalse(combat.Weapon.Cycle.IsReloading);Assert.AreEqual(0,combat.Weapon.MagazineAmmo);Assert.IsFalse(rig.weapon.transform.Find("PistolReplacementMagazine").GetComponent<SpriteRenderer>().enabled);combat.enabled=true;combat.BindTarget(null);
            }
            actor.GetComponent<Health>().ApplyDamage(new DamageInfo(10000,CombatTeam.Player));yield return null;Assert.IsTrue(actor.GetComponent<Health>().IsDefeated);Assert.IsFalse(telegraph.LaserVisible);
        }
    }
}
#endif
