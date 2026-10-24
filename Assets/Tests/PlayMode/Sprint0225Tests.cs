#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InsaneDreams.CameraSystem;
using InsaneDreams.Input;
using InsaneDreams.Prototype;
using InsaneDreams.Traversal;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace InsaneDreams.Tests
{
    public sealed class Sprint0225Tests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private PlayerActions actions;
        private PlayerMotor2D motor;
        private PlayerTraversal2D traversal;
        private Rigidbody2D body;
        private UIDocument ui;
        private bool background;
        private GameObject Keep(GameObject go) { objects.Add(go); return go; }
        private void Set(Object target, string name, float value)
        {
            var so = new SerializedObject(target); so.FindProperty(name).floatValue = value; so.ApplyModifiedPropertiesWithoutUndo();
        }
        [UnitySetUp] public IEnumerator Setup()
        {
            background = Application.runInBackground; Application.runInBackground = true;
            var floor = Keep(new GameObject("S0225Floor")); floor.transform.position = new Vector2(32000, -.5f);
            floor.AddComponent<BoxCollider2D>().size = new Vector2(100, 1);
            var player = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player/WarehousePlayer.prefab"), new Vector3(32000, .85f), Quaternion.identity));
            actions = player.GetComponent<PlayerActions>(); motor = player.GetComponent<PlayerMotor2D>();
            traversal = player.GetComponent<PlayerTraversal2D>(); body = player.GetComponent<Rigidbody2D>();
            var so = new SerializedObject(actions); so.FindProperty("readKeyboard").boolValue = false; so.ApplyModifiedPropertiesWithoutUndo();
            ui = Keep(Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/WarehouseControls.prefab"))).GetComponent<UIDocument>();
            ui.GetComponent<WarehouseTouchControls>().Bind(actions);
            yield return new WaitForSeconds(.25f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            foreach (var go in objects) if (go != null) Object.Destroy(go);
            objects.Clear(); yield return null; Application.runInBackground = background;
        }
        private void Down(VisualElement element)
        {
            using (var evt = PointerDownEvent.GetPooled(new Event { type = EventType.MouseDown, button = 0, mousePosition = element.worldBound.center }))
            { evt.target = element; element.SendEvent(evt); }
        }
        private void Up(VisualElement element)
        {
            using (var evt = PointerUpEvent.GetPooled(new Event { type = EventType.MouseUp, button = 0, mousePosition = element.worldBound.center }))
            { evt.target = element; element.SendEvent(evt); }
        }
        private void TouchDown(VisualElement element, int finger, Vector2 position)
        {
            using (var evt = PointerDownEvent.GetPooled(new Touch { fingerId = finger, position = position, phase = TouchPhase.Began }, EventModifiers.None))
            { evt.target = element; element.SendEvent(evt); }
        }
        private void TouchUp(VisualElement element, int finger)
        {
            using (var evt = PointerUpEvent.GetPooled(new Touch { fingerId = finger, position = element.worldBound.center, phase = TouchPhase.Ended }, EventModifiers.None))
            { evt.target = element; element.SendEvent(evt); }
        }
        private IEnumerator LadderJump(Vector2 direction)
        {
            var go = Keep(new GameObject("S0225Ladder")); go.transform.position = new Vector2(32000, 5);
            var trigger = go.AddComponent<BoxCollider2D>(); trigger.isTrigger = true; trigger.size = new Vector2(1.8f, 12); go.AddComponent<LadderVolume>();
            body.position = new Vector2(32000, 3); Physics2D.SyncTransforms();
            actions.SetTouchMove(Vector2.up); yield return new WaitForSeconds(.1f);
            Assert.IsTrue(traversal.IsClimbing);
            actions.SetTouchMove(direction);
            var button = ui.rootVisualElement.Q<VisualElement>("jumpButton"); Down(button);
            yield return null; yield return new WaitForFixedUpdate();
            Assert.IsFalse(traversal.IsClimbing); Assert.IsTrue(motor.enabled);
            Assert.AreEqual(1, motor.JumpsUsed); Assert.Greater(body.linearVelocity.y, 10.5f);
            Assert.AreEqual(3, body.gravityScale);
            if (Mathf.Abs(direction.x) > .1f) Assert.AreEqual(Mathf.Sign(direction.x), Mathf.Sign(body.linearVelocity.x));
            yield return new WaitForSeconds(.12f);
            Assert.IsFalse(traversal.IsClimbing); Assert.AreEqual(1, motor.JumpsUsed, "Holding the button cannot repeat a jump");
            Up(button); yield return null; Down(button); yield return null; yield return new WaitForFixedUpdate();
            Assert.AreEqual(2, motor.JumpsUsed); Assert.Greater(body.linearVelocity.y, 8);
            Up(button); yield return null; Down(button); yield return null; yield return new WaitForFixedUpdate();
            Assert.AreEqual(2, motor.JumpsUsed);
            Up(button);
            yield return new WaitForSeconds(.35f);
            Assert.IsFalse(traversal.IsClimbing, "Held climb input cannot reattach after lockout without release");
        }
        [UnityTest] public IEnumerator TouchJumpDetachesWhileHoldingUpAndLeavesOneAirJump() => LadderJump(Vector2.up);
        [UnityTest] public IEnumerator TouchJumpDetachesWhileHoldingDownAndLeavesOneAirJump() => LadderJump(Vector2.down);
        [UnityTest] public IEnumerator TouchJumpDetachesWhileHoldingLeftAndLeavesOneAirJump() => LadderJump(Vector2.left);
        [UnityTest] public IEnumerator TouchJumpDetachesWhileHoldingRightAndLeavesOneAirJump() => LadderJump(Vector2.right);
        [UnityTest] public IEnumerator ExplicitTouchAndFreshUpGestureSpendOnlyOneJumpAndResetOnLanding()
        {
            actions.SetTouchMove(Vector2.zero); yield return null;
            actions.SetTouchMove(Vector2.up); var jump = ui.rootVisualElement.Q<VisualElement>("jumpButton"); Down(jump);
            yield return new WaitForSeconds(.12f); Assert.AreEqual(1, motor.JumpsUsed); Up(jump);
            actions.SetTouchMove(Vector2.zero); yield return new WaitForSeconds(1.2f);
            Assert.IsTrue(motor.IsGrounded); Assert.AreEqual(0, motor.JumpsUsed);
            Down(jump); yield return new WaitForSeconds(.08f); Assert.AreEqual(1, motor.JumpsUsed); Up(jump);
        }
        [UnityTest] public IEnumerator ThreeCapturedTouchPointersKeepMovingAndFiringWhileJumpIsTapped()
        {
            var root = ui.rootVisualElement;
            var move = root.Q<VisualElement>("moveStick"); var aim = root.Q<VisualElement>("aimStick");
            var jump = root.Q<VisualElement>("jumpButton");
            TouchDown(move, 0, move.worldBound.center + Vector2.right * move.contentRect.width * .35f);
            TouchDown(aim, 1, aim.worldBound.center + Vector2.right * aim.contentRect.width * .35f);
            yield return null;
            Assert.Greater(actions.Move.x,.9f); Assert.IsTrue(actions.FireHeld);
            TouchDown(jump, 2, jump.worldBound.center);
            yield return new WaitForSeconds(.08f);
            Assert.AreEqual(1,motor.JumpsUsed); Assert.Greater(body.linearVelocity.y,8);
            Assert.Greater(actions.Move.x,.9f); Assert.IsTrue(actions.FireHeld); Assert.Greater(body.linearVelocity.x,0);
            TouchUp(jump,2); yield return null;
            Assert.Greater(actions.Move.x,.9f); Assert.IsTrue(actions.FireHeld);
            TouchUp(move,0); TouchUp(aim,1); yield return null;
            Assert.AreEqual(Vector2.zero,actions.Move); Assert.IsFalse(actions.FireHeld);
        }
        [UnityTest] public IEnumerator BothHudAssetsHaveLargerHitAreasAndSeparatedActionsAcrossAspectRatios()
        {
            foreach (var path in new[] { "Assets/UI/WarehouseControls.uxml", "Assets/UI/MissionControls.uxml" })
            {
                ui.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(path);
                ui.GetComponent<WarehouseTouchControls>().enabled = false; ui.GetComponent<WarehouseTouchControls>().enabled = true;
                foreach (var size in new[] { new Vector2(1280,720), new Vector2(960,720), new Vector2(1560,720), new Vector2(720,1280), new Vector2(648,1280) })
                {
                    var root = ui.rootVisualElement; root.style.width = size.x; root.style.height = size.y;
                    Assert.IsNull(root.Q("fireButton"));var names = new[] { "moveStick","aimStick","heal","reload","parachuteButton","jumpButton","supplyInteract" };
                    root.Q<VisualElement>("supplyInteract").RemoveFromClassList("hidden");
                    yield return null; yield return null; yield return null;
                    var controls = names.Select(n => root.Q<VisualElement>(n)).ToArray();
                    foreach (var pad in controls.Take(2))
                    {
                        float expected = Mathf.Min(215.625f,size.x*.5f-144);
                        Assert.AreEqual(expected, pad.resolvedStyle.width, 1);
                        Assert.AreEqual(expected, pad.resolvedStyle.height, 1);
                        Assert.AreEqual(PickingMode.Position, pad.pickingMode);
                    }
                    var safe = root.Q<VisualElement>("safeArea").worldBound;
                    foreach (var control in controls)
                    { Assert.IsTrue(safe.Contains(control.worldBound.center)); }
                    for (int i=0;i<controls.Length;i++) for(int j=i+1;j<controls.Length;j++)
                        Assert.IsFalse(controls[i].worldBound.Overlaps(controls[j].worldBound), path+" "+names[i]+" overlaps "+names[j]+" at "+size);
                }
            }
        }
        [UnityTest] public IEnumerator ZoomDividesExistingFramingByOnePointOneAtEveryAspectAndPreservesBounds()
        {
            var camera = Keep(new GameObject("S0225Camera")).AddComponent<Camera>();
            var follow = camera.gameObject.AddComponent<WarehouseCameraFollow>();
            var so = new SerializedObject(follow); so.FindProperty("levelMin").vector2Value = new Vector2(31950,-9);
            so.FindProperty("levelMax").vector2Value = new Vector2(32050,30); so.ApplyModifiedPropertiesWithoutUndo();
            foreach (float aspect in new[] { 4f/3,16f/9,20f/9,9f/16 })
            {
                camera.aspect = aspect;
                foreach (bool android in new[] { false,true })
                {
                    so.Update(); so.FindProperty("previewAndroidFraming").boolValue=android; so.ApplyModifiedPropertiesWithoutUndo();
                    Set(follow,"gameplayZoomMultiplier",1); follow.Bind(actions.transform); float before=camera.orthographicSize;
                    Set(follow,"gameplayZoomMultiplier",1.1f); follow.SnapToTarget();
                    Assert.AreEqual(before/1.1f,camera.orthographicSize,.001f);
                    Assert.GreaterOrEqual(camera.transform.position.y-camera.orthographicSize,-9.001f);
                    Assert.GreaterOrEqual(camera.transform.position.x-camera.orthographicSize*aspect,31949.999f);
                }
            }
            yield return null;
        }
    }
}
#endif
