#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using InsaneDreams.Combat;
using InsaneDreams.Input;
using InsaneDreams.Inventory;
using InsaneDreams.Missions;
using InsaneDreams.Progression;
using InsaneDreams.Visuals;
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
    public sealed class AndroidPerformanceRegressionTests
    {
        const BindingFlags Flags=BindingFlags.NonPublic|BindingFlags.Instance;
        GameSession session;string save;bool background;float volume;WarehouseMission mission;WarehouseTouchControls hud;
        [UnitySetUp] public IEnumerator Setup()
        {
            Assert.IsNull(GameSession.Instance);background=Application.runInBackground;Application.runInBackground=true;volume=AudioListener.volume;AudioListener.volume=0;
            save=Path.Combine(Path.GetTempPath(),"AkumuPerfRules-"+Guid.NewGuid().ToString("N")+".json");
            session=new GameObject("PerformanceRulesSession").AddComponent<GameSession>();session.Initialize(AssetDatabase.LoadAssetAtPath<GameCatalog>("Assets/Data/Progression/DemoCatalog.asset"),save);
            session.Navigate(FlowScreen.Levels);session.SelectLevel("warehouse");session.PrepareMission();
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Demo.unity",new LoadSceneParameters(LoadSceneMode.Single));yield return null;yield return null;
            mission=Object.FindAnyObjectByType<WarehouseMission>();hud=Object.FindAnyObjectByType<WarehouseTouchControls>();mission.Player.GetComponent<Health>().ProtectFor(120);
            Assert.IsTrue(mission.Player.GetComponent<PlayerLoadout>().Equip(EquipmentSlot.Firearm2,new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/BackupPistol.asset"),42)));
            Assert.IsTrue(mission.Player.GetComponent<PlayerLoadout>().Equip(EquipmentSlot.Firearm1,new RuntimeWeapon(AssetDatabase.LoadAssetAtPath<WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset"),42))); // This fixture exercises two occupied slots; a fresh profile intentionally may have no primary.
            foreach(var enemy in Object.FindObjectsByType<InsaneDreams.Enemies.EnemyCombat>(FindObjectsSortMode.None))enemy.enabled=false;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            Time.timeScale=1;yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Scenes/Warehouse_Testbed.unity",new LoadSceneParameters(LoadSceneMode.Single));
            if(session!=null)Object.Destroy(session.gameObject);yield return null;
            foreach(var suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);
            Application.runInBackground=background;AudioListener.volume=volume;
        }
        void Refresh(){typeof(WarehouseTouchControls).GetField("nextReadout",Flags).SetValue(hud,0f);typeof(WarehouseTouchControls).GetMethod("Update",Flags).Invoke(hud,null);}
        [UnityTest] public IEnumerator CachedHudTracksAmmoSwitchesGrenadesAndRebind()
        {
            var loadout=mission.Player.GetComponent<PlayerLoadout>();var weapon=loadout.SelectedWeapon;Assert.NotNull(weapon);
            var root=hud.GetComponent<UIDocument>().rootVisualElement;Refresh();
            var changed=(Func<bool>)Delegate.CreateDelegate(typeof(Func<bool>),hud,typeof(WarehouseTouchControls).GetMethod("ReadoutChanged",Flags));Assert.IsFalse(changed(),"Unchanged values must skip formatting");
            weapon.SetAmmunition(3,42);Refresh();Assert.AreEqual("3/"+weapon.Definition.MagazineCapacity,root.Q<Label>("ammoValue").text);Assert.AreEqual("RESERVE 42",root.Q<Label>("reserveValue").text);
            weapon.SetAmmunition(weapon.Definition.MagazineCapacity,20);Refresh();Assert.AreEqual(weapon.Definition.MagazineCapacity+"/"+weapon.Definition.MagazineCapacity,root.Q<Label>("ammoValue").text);
            loadout.Select(EquipmentSlot.Firearm2);Refresh();Assert.AreEqual(loadout.SelectedWeapon.Definition.DisplayName.ToUpperInvariant(),root.Q<Label>("loadout").text);
            var grenades=mission.Player.GetComponent<PlayerThrowables>();grenades.RestoreStock(new[]{1,2,3});loadout.Select(EquipmentSlot.Melee);grenades.Select(ThrowableKind.Smoke);Refresh();Assert.AreEqual("3/"+grenades.MaximumCapacity,root.Q<Label>("ammoValue").text);
            grenades.RestoreStock(new[]{1,2,2});Refresh();Assert.AreEqual("2/"+grenades.MaximumCapacity,root.Q<Label>("ammoValue").text);
            hud.Bind(mission.Player);Assert.IsTrue(changed(),"Rebinding must invalidate the snapshot");
            Assert.AreEqual(60,InsaneDreams.CameraSystem.AndroidFramePacing.TargetFramesPerSecond);yield return null;
        }
        [UnityTest] public IEnumerator EquipmentPosePreservesMuzzleMagazineAndExpressionChanges()
        {
            var player=mission.Player;var loadout=player.GetComponent<PlayerLoadout>();var rig=player.GetComponentInChildren<CutoutRig>();
            var visual=player.GetComponent<CharacterVisualDriver>();var pose=(Action)Delegate.CreateDelegate(typeof(Action),visual,typeof(CharacterVisualDriver).GetMethod("LateUpdate",Flags));
            foreach(var slot in new[]{EquipmentSlot.Firearm1,EquipmentSlot.Firearm2})
            {
                loadout.Select(slot);var weapon=loadout.SelectedWeapon;Assert.NotNull(weapon);pose();var sprite=rig.weapon.sprite;
                for(int i=0;i<30;i++)pose();
                // Imports can rematerialize managed wrappers; native identity is the contract.
                Assert.IsNotNull(rig.weapon.sprite);Assert.AreEqual(sprite.GetEntityId(),rig.weapon.sprite.GetEntityId(),"Native sprite identity must remain stable");Assert.AreEqual(weapon.Definition.EquippedSprite.GetEntityId(),rig.weapon.sprite.GetEntityId(),"Equipped native sprite must match its definition");
                rig.SetExpression(FaceExpression.Scared);Assert.AreEqual(rig.skin.Face(FaceExpression.Scared).GetEntityId(),rig.face.sprite.GetEntityId());rig.SetExpression(FaceExpression.Neutral);Assert.AreEqual(rig.skin.Face(FaceExpression.Neutral).GetEntityId(),rig.face.sprite.GetEntityId());
                player.SetTouchAim(Vector2.left);yield return null;Assert.IsFalse(float.IsNaN(rig.MuzzlePosition.x));
            }
        }
    }
}
#endif


