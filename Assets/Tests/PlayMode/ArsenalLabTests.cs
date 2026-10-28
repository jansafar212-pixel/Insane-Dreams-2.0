#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Inventory;
using InsaneDreams.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object=UnityEngine.Object;
namespace InsaneDreams.Tests
{
    public sealed class ArsenalLabTests
    {
        GameSession session;string path;bool background;float volume;
        [UnitySetUp] public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            path=Path.Combine(Path.GetTempPath(),"AkumuArsenal-"+Guid.NewGuid().ToString("N")+".json");CreateSession();session.Save();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/MainMenu.unity",new LoadSceneParameters(LoadSceneMode.Single));for(int i=0;i<5;i++)yield return null;
        }
        void CreateSession(){session=new GameObject("ArsenalProfileTest").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),path);}
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));if(session!=null)Object.Destroy(session.gameObject);yield return null;
            typeof(ArsenalTestBench).GetField("requested",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,false);
            foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(path+suffix))File.Delete(path+suffix);Application.runInBackground=background;AudioListener.volume=volume;
        }
        [UnityTest] public IEnumerator LabLoadsAllWeaponsRefillsAndLeavesNormalProfileUntouched()
        {
            var bench=Object.FindAnyObjectByType<ArsenalTestBench>();Assert.NotNull(bench);int balance=session.Data.balance;byte[] original=File.ReadAllBytes(path);
            Assert.NotNull(Object.FindAnyObjectByType<GameFlowController>().GetComponent<UIDocument>().rootVisualElement.Q<Button>("developerArsenalEntry"));
            var entry=(IEnumerator)typeof(ArsenalTestBench).GetMethod("Enter",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(bench,null);yield return entry;for(int i=0;i<6;i++)yield return null;
            Assert.IsNull(GameSession.Instance);Assert.AreEqual("Warehouse_Testbed",SceneManager.GetActiveScene().name);bench=Object.FindAnyObjectByType<ArsenalTestBench>();Assert.NotNull(bench);
            var loadout=Object.FindAnyObjectByType<PlayerLoadout>();loadout.GetComponent<Health>().ProtectFor(120);
            for(int i=0;i<Resources.Load<ArsenalCatalog>("ArsenalCatalog").weapons.Length;i++)
            {
                bench.Equip(i);yield return new WaitForSeconds(.25f);Assert.AreSame(Resources.Load<ArsenalCatalog>("ArsenalCatalog").weapons[i],loadout.SelectedWeapon.Definition);
                if(i==1||i==4||i==5||i==6)
                {
                    var actions=loadout.GetComponent<Input.PlayerActions>();actions.BeginTouchAim();actions.SetTouchAim(Vector2.right);actions.SetFireIntent(true);yield return new WaitForSeconds(.05f);
                    var combat=loadout.GetComponent<PlayerCombat>();var attack=loadout.GetComponent<AttackExecutor>();var rig=loadout.GetComponentInChildren<Visuals.CutoutRig>();
                    File.WriteAllText("TestResults/Arsenal/Lab-"+loadout.SelectedWeapon.Definition.name+"-state.txt",$"frame={Time.frameCount}; combatEnabled={combat.enabled}; actionsEnabled={actions.enabled}; aim={actions.HasAimIntent}; fire={actions.FireHeld}; sniper={combat.SniperAiming}; shots={attack.ShotsFired}; ammo={loadout.SelectedWeapon.MagazineAmmo}; origin={attack.LastShotOrigin}; muzzle={rig.MuzzlePosition}; parent={rig.muzzle.parent.name}");
                    if(i==6){Assert.IsTrue(combat.SniperAiming);Assert.IsTrue(combat.AimLaser.enabled);}else Assert.Less(loadout.SelectedWeapon.MagazineAmmo,loadout.SelectedWeapon.Definition.MagazineCapacity);
                    yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("TestResults/Arsenal/Lab-"+loadout.SelectedWeapon.Definition.name+".png",image.EncodeToPNG());Object.Destroy(image);actions.EndTouchAim(false);actions.SuspendInput();
                }
                loadout.SelectedWeapon.SetAmmunition(0,0);bench.Refill();Assert.AreEqual(loadout.SelectedWeapon.Definition.MagazineCapacity,loadout.SelectedWeapon.MagazineAmmo);Assert.AreEqual(loadout.SelectedWeapon.Definition.DeveloperReserve,loadout.SelectedWeapon.ReserveAmmo);
            }
            CollectionAssert.AreEqual(original,File.ReadAllBytes(path),"Lab cannot save cheat equipment/ammo/progress");
            // Test normal return using the same isolated saved profile, never the user's file.
            CreateSession();bench.Exit();for(int i=0;i<5;i++)yield return null;Assert.AreEqual("MainMenu",SceneManager.GetActiveScene().name);Assert.AreEqual(balance,session.Data.balance);CollectionAssert.AreEqual(original,File.ReadAllBytes(path));
        }
    }
}
#endif

