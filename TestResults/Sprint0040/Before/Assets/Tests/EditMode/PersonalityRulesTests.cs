using InsaneDreams.Enemies;
using NUnit.Framework;
using UnityEditor;
namespace InsaneDreams.Tests
{
    public sealed class PersonalityRulesTests
    {
        [Test] public void EncounterGateLimitsOrdinaryLinesAndRareOverridesAcrossSpeakers()
        {
            var gate=new EncounterSpeechGate();Assert.IsTrue(gate.TryReserve(0,30,false));
            Assert.IsFalse(gate.TryReserve(1,30,false));Assert.IsFalse(gate.TryReserve(4,30,true));
            Assert.IsTrue(gate.TryReserve(6,30,true));Assert.IsFalse(gate.TryReserve(12,30,true));
            Assert.IsFalse(gate.TryReserve(35,30,false));Assert.IsTrue(gate.TryReserve(36,30,false));
            Assert.IsFalse(gate.TryReserve(float.NaN,30,true));
        }
        [Test] public void ConfiguredProfilesHaveDialogueAndDifferentMoraleProbabilities()
        {
            var ordinary=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Ordinary.asset");
            var over=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Overconfident.asset");
            var coward=AssetDatabase.LoadAssetAtPath<LarperPersonalityProfile>("Assets/Data/Enemies/Personalities/Cowardly.asset");
            Assert.Less(over.PanicChance,ordinary.PanicChance);Assert.Less(ordinary.PanicChance,coward.PanicChance);
            foreach(var profile in new[]{ordinary,over,coward})foreach(LarperSpeechEvent context in System.Enum.GetValues(typeof(LarperSpeechEvent)))Assert.IsNotEmpty(profile.Lines(context));
            Assert.IsFalse(LarperMoralePolicy.ShouldPanic(0,0));Assert.IsTrue(LarperMoralePolicy.ShouldPanic(1,.99f));
            Assert.IsFalse(LarperMoralePolicy.ShouldPanic(.08f,.5f));Assert.IsTrue(LarperMoralePolicy.ShouldPanic(.2f,.1f));
        }
        [Test] public void WarehousePassageHasCorrectClearanceAndDoesNotOverlapCheckpointSpawn()
        {
            var scene=UnityEditor.SceneManagement.EditorSceneManager.OpenPreviewScene("Assets/Scenes/Warehouse_Testbed.unity");
            try
            {
                var environment=System.Array.Find(scene.GetRootGameObjects(),r=>r.name=="Environment");
                var beam=environment.transform.Find("TraversableSurfaces/CrouchPassage");var box=beam.GetComponent<UnityEngine.BoxCollider2D>();
                Assert.That(box.bounds.size.x,Is.EqualTo(2).Within(.001f));Assert.That(box.bounds.size.y,Is.EqualTo(.4f).Within(.001f));
                Assert.That(box.bounds.min.y,Is.EqualTo(1.05f).Within(.001f));Assert.IsFalse(box.autoTiling);
                Assert.That(beam.GetComponent<UnityEngine.SpriteRenderer>().bounds.size.x,Is.EqualTo(box.bounds.size.x).Within(.001f));
                var interactions=System.Array.Find(scene.GetRootGameObjects(),r=>r.name=="Interactions");
                float spawnX=interactions.transform.Find("Checkpoint_Entry/SafeSpawn").position.x;
                Assert.Greater(box.bounds.min.x-spawnX,.5f);
            }
            finally{UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);}
        }
    }
}
