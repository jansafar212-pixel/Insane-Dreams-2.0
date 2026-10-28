#if UNITY_EDITOR
using System.Collections;
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
    public sealed class Sprint0027RegionTests
    {
        [UnityTest]public IEnumerator RegionalDamageAccumulatesAcrossDefeatAndSeparatesOnlyAtThreshold()
        {
            bool background=Application.runInBackground;Application.runInBackground=true;var actor=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Larper.prefab"),new Vector3(34000,.72f),Quaternion.identity);CartoonRagdoll corpse=null;
            try
            {
                actor.GetComponent<EnemyCombat>().enabled=false;actor.GetComponent<LarperBrain>().enabled=false;actor.GetComponent<Rigidbody2D>().constraints=RigidbodyConstraints2D.FreezeAll;yield return null;var regions=actor.GetComponent<BodyRegionDamage>();Assert.NotNull(regions);var s=new SerializedObject(regions);s.FindProperty("headThreshold").floatValue=45;s.ApplyModifiedPropertiesWithoutUndo();var rig=actor.GetComponent<CharacterVisualDriver>().Rig;var hp=actor.GetComponent<Health>();Vector2 head=rig.head.GetComponent<SpriteRenderer>().bounds.center;
                for(int i=0;i<3;i++)hp.ApplyDamage(new DamageInfo(10,CombatTeam.Player,null,new PhysicalImpact(head,Vector2.right,.1f)));Assert.AreEqual(30,regions.DamageAt(1));hp.ApplyDamage(new DamageInfo(1000,CombatTeam.Player,null,new PhysicalImpact(rig.torso.position,Vector2.right,.1f)));corpse=actor.GetComponent<RagdollOnDefeat>().LastRagdoll;Assert.NotNull(corpse);Assert.AreEqual(30,corpse.RegionDamage(1));Assert.IsFalse(corpse.IsRegionSeparated(1));var body=corpse.Bodies[1];var impact=new PhysicalImpact(body.position,Vector2.right,.1f,body);corpse.ReceiveRegionDamage(10,impact);Assert.IsFalse(corpse.IsRegionSeparated(1));corpse.ReceiveRegionDamage(10,impact);Assert.IsTrue(corpse.IsRegionSeparated(1));Assert.IsFalse(body.GetComponent<HingeJoint2D>().enabled);Assert.AreEqual(50,corpse.RegionDamage(1));
                corpse.ReceiveImpact(new PhysicalImpact(body.position,Vector2.right,1000,body));for(int i=0;i<15;i++){yield return new WaitForFixedUpdate();foreach(var part in corpse.Bodies)Assert.LessOrEqual(part.linearVelocity.magnitude,12.5f);}
            }
            finally{if(corpse!=null)Object.Destroy(corpse.gameObject);Object.Destroy(actor);Application.runInBackground=background;}
        }
    }
}
#endif
