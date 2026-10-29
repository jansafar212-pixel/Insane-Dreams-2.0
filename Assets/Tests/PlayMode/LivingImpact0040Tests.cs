#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Visuals;
using InsaneDreams.Enemies;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class LivingImpact0040Tests
    {
        const float X=500,GameplayOrtho=4.3290043f; // Native factory projection at1280x720, including pixel snapping/Android scale; GameplayCamera.json.
        [Serializable]public sealed class Row{public string profile,weapon;public float strength,maxHeadPixelsAt720,maxTorsoJointDegrees,visibleSeconds;}
        [Serializable]public sealed class Report{public int width=1280,height=720;public float orthographicSize=GameplayOrtho;public List<Row> rows=new List<Row>();}
        readonly List<GameObject> owned=new List<GameObject>();Camera camera;RenderTexture rt;Texture2D texture;bool background;float volume;
        GameObject Keep(GameObject g){owned.Add(g);return g;}
        static void F(Object o,string key,float value){var s=new SerializedObject(o);s.FindProperty(key).floatValue=value;s.ApplyModifiedPropertiesWithoutUndo();}
        [UnitySetUp]public IEnumerator Setup()
        {
            background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            var floor=Keep(new GameObject("ReadabilityFloor"));floor.transform.position=new Vector3(X,-.5f);floor.AddComponent<BoxCollider2D>().size=new Vector2(50,1);
            camera=Keep(new GameObject("GameplayScaleProbe")).AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=GameplayOrtho;camera.transform.position=new Vector3(X,1.8f,-10);camera.aspect=16f/9;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.14f,.18f,.19f);camera.enabled=false;
            rt=new RenderTexture(1280,720,24);texture=new Texture2D(1280,720,TextureFormat.RGB24,false);camera.targetTexture=rt;
            Directory.CreateDirectory("TestResults/Sprint0040");yield return null;
        }
        [UnityTearDown]public IEnumerator Cleanup(){camera.targetTexture=null;Object.Destroy(rt);Object.Destroy(texture);foreach(var g in owned)if(g!=null)Object.Destroy(g);yield return null;Application.runInBackground=background;AudioListener.volume=volume;}
        GameObject Actor(bool old)
        {
            var holder=Keep(new GameObject("ImpactFactory"));holder.SetActive(false);
            var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(X,.8f),Quaternion.identity,holder.transform);
            actor.GetComponent<EnemyCombat>().enabled=false;actor.GetComponent<LarperBrain>().enabled=false;
            // Isolate body motion from the independently tested red tint. Native damage/region math is unchanged.
            actor.GetComponent<CombatFeedback>().enabled=false;F(actor.GetComponent<Health>(),"maximum",1000);
            if(old)foreach(var p in new[]{("springReturn",18f),("impulseDegrees",115f),("jointDamping",5f),("springDamping",1.5f),("maximumAngularSpeed",320f),("maximumJointAngle",26f),("maximumTorsoAngle",12f),("springTorque",.25f),("responseSeconds",.8f),("torsoSpill",.25f),("heavyTorsoSpill",.55f)})F(actor.GetComponent<HitReaction2D>(),p.Item1,p.Item2);
            holder.SetActive(true);return actor;
        }
        void CapturePixels(){var previous=RenderTexture.active;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();RenderTexture.active=previous;}
        void SaveCapture(string name)=>File.WriteAllBytes("TestResults/Sprint0040/"+name+".png",texture.EncodeToPNG());
        static WeaponDefinition Gun(string name)=>AssetDatabase.LoadAssetAtPath<WeaponDefinition>(name=="Glock"?"Assets/Data/Items/BackupPistol.asset":name=="Rifle"?"Assets/Data/Items/WarehouseCarbine.asset":"Assets/Resources/Arsenal/"+name+".asset");
        [UnityTest]public IEnumerator GameplayZoomReadabilityProbe()
        {
            var report=new Report();
            foreach(bool old in new[]{true,false})foreach(string name in new[]{"Glock","SMG","Rifle","LMG","DesertEagle","Shotgun","Sniper"})
            {
                var actor=Actor(old);yield return new WaitForSeconds(.3f);actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;
                var rig=actor.GetComponentInChildren<CutoutRig>();var h=actor.GetComponent<Health>();var reaction=actor.GetComponent<HitReaction2D>();var gun=Gun(name);
                Vector3 start=rig.head.position;float maxPixels=0,maxAngle=0,visible=0;int accepted=0;h.Damaged+=_=>accepted++;
                CapturePixels();SaveCapture((old?"Before_":"After_")+name+"_Rest");
                for(int pellet=0;pellet<gun.Pellets;pellet++)Assert.IsTrue(h.ApplyDamage(new DamageInfo(gun.Damage,CombatTeam.Player,null,new PhysicalImpact(rig.torso.position,Vector2.right,gun.ImpactImpulse,null,gun.HitReactionStrength))));
                for(int i=0;i<32;i++)
                {
                    yield return new WaitForFixedUpdate();yield return null;
                    Vector3 delta=camera.WorldToViewportPoint(rig.head.position)-camera.WorldToViewportPoint(start);float pixels=new Vector2(delta.x*1280,delta.y*720).magnitude;
                    if(pixels>maxPixels){maxPixels=pixels;CapturePixels();}
                    if(pixels>=2)visible+=Time.fixedDeltaTime;maxAngle=Mathf.Max(maxAngle,Mathf.Abs(reaction.RegionAngle(0)));
                }
                SaveCapture((old?"Before_":"After_")+name+"_Peak");
                report.rows.Add(new Row{profile=old?"old-settings replay":"native0040",weapon=name,strength=reaction.LastIntensity,maxHeadPixelsAt720=maxPixels,maxTorsoJointDegrees=maxAngle,visibleSeconds=visible});
                Assert.AreEqual(gun.Pellets,accepted);Assert.AreEqual(1000-gun.Damage*gun.Pellets,h.Current,.01f);Assert.AreEqual(1,reaction.PhysicalReactions,"Pellets are one physical batch");Assert.IsFalse(h.IsDefeated);Assert.AreEqual(6,reaction.JointCount);Assert.LessOrEqual(maxAngle,old?26.1f:34.1f);
                Object.Destroy(actor.transform.parent.gameObject);yield return null;
            }
            File.WriteAllText("TestResults/Sprint0040/ReadabilityLatest.json",JsonUtility.ToJson(report,true));
            var rows=report.rows.Where(r=>r.profile=="native0040").ToArray();Func<string,Row> row=n=>rows.Single(r=>r.weapon==n);
            Assert.GreaterOrEqual(row("Glock").maxHeadPixelsAt720,3);Assert.GreaterOrEqual(row("SMG").maxHeadPixelsAt720,1.5f);Assert.Greater(row("Rifle").maxHeadPixelsAt720,row("Glock").maxHeadPixelsAt720);Assert.Greater(row("LMG").maxHeadPixelsAt720,row("Rifle").maxHeadPixelsAt720);Assert.GreaterOrEqual(row("DesertEagle").maxHeadPixelsAt720,12);Assert.Greater(row("Sniper").maxHeadPixelsAt720,row("DesertEagle").maxHeadPixelsAt720);
        }
        [UnityTest]public IEnumerator RegionalDiagonalImpulsesStayAttachedAndRespectFacingWithoutRootForces()
        {
            var actor=Actor(false);yield return new WaitForSeconds(.3f);var body=actor.GetComponent<Rigidbody2D>();body.constraints=RigidbodyConstraints2D.FreezeRotation;
            var h=actor.GetComponent<Health>();var rig=actor.GetComponentInChildren<CutoutRig>();var reaction=actor.GetComponent<HitReaction2D>();var driver=actor.GetComponent<CharacterVisualDriver>();Vector2 root=body.position;
            foreach(float facing in new[]{1f,-1f})foreach(int region in new[]{0,1,2,4})foreach(float side in new[]{1f,-1f})
            {
                reaction.CancelReaction();typeof(CharacterVisualDriver).GetField("facing",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(driver,facing);yield return null;yield return null;
                var renderer=rig.BodyParts[region].GetComponent<SpriteRenderer>();Vector3 before=renderer.bounds.center;Vector2 direction=new Vector2(side,.35f).normalized;
                Assert.IsTrue(h.ApplyDamage(new DamageInfo(1,CombatTeam.Player,null,new PhysicalImpact(before,direction,1,null,1.1f))));
                float away=0;for(int i=0;i<12;i++){yield return new WaitForFixedUpdate();yield return null;away=Mathf.Max(away,Vector2.Dot((Vector2)(renderer.bounds.center-before),direction));}
                Assert.AreEqual(region,reaction.LastRegion);Assert.Greater(away,.001f,"Localized response follows the incoming shot: facing="+facing+" region="+region+" side="+side);Assert.Less(Vector2.Distance(root,body.position),.05f);Assert.IsFalse(h.IsDefeated);Assert.AreEqual(6,reaction.JointCount);
            }
            yield return new WaitForSeconds(1.2f);for(int i=0;i<6;i++)Assert.AreEqual(0,reaction.RegionAngle(i),.01f);Assert.IsFalse(reaction.KnockedDown);Assert.AreEqual(0,CartoonRagdoll.ActiveCount);
        }
    }
}
#endif