// Editor diagnostic outside Assets: renders existing art and actual procedural poses.
var preview = new UnityEditor.PreviewRenderUtility();
UnityEngine.Texture2D image = null;
try
{
    var sheet = new UnityEngine.GameObject("CoverPosePreview"); preview.AddSingleGO(sheet);
    var skinPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Prefabs/Visuals/PlayerCutoutVisual.prefab");
    var crateSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Sprite>("Assets/Art/Sprint008/ShippingCrate.png");
    for (int row = 0; row < 2; row++) for (int col = 0; col < 3; col++)
    {
        float facing = row == 0 ? 1 : -1;
        float x = -5.1f + col * 4.8f, y = row == 0 ? 1.2f : -1.2f;
        var go = UnityEngine.Object.Instantiate(skinPrefab, sheet.transform); go.transform.localPosition = new UnityEngine.Vector3(x, y + .7f);
        var rig = go.GetComponent<InsaneDreams.Visuals.CutoutRig>(); rig.ApplySkin(rig.skin);
        var definition = UnityEditor.AssetDatabase.LoadAssetAtPath<InsaneDreams.Inventory.WeaponDefinition>("Assets/Data/Items/WarehouseCarbine.asset");
        if (definition.EquippedSprite != null) rig.weapon.sprite = definition.EquippedSprite;
        float height = col == 2 ? 1.35f : .85f;
        float blend = UnityEngine.Mathf.Clamp01((1 - height / 1.4f) / .3f);
        rig.Pose(InsaneDreams.Visuals.CharacterPose.Crouch, 0, UnityEngine.Vector2.right * facing, facing, 0, InsaneDreams.Visuals.FaceExpression.Neutral,
            true, true, false, 0, blend, UnityEngine.Mathf.Min(0, height - .98f), col > 0);
        if (col > 0) rig.RaiseWeaponOverCover(y + .9f, x + facing * .5f);
        // Static preview has no physics world; apply the same near-face stop used
        // by AttackExecutor for the concealed, below-cover muzzle.
        else rig.RetractMuzzle(new UnityEngine.Vector2(x + facing * .485f, rig.MuzzlePosition.y));
        var crate = new UnityEngine.GameObject("ExistingCrateSprite"); crate.transform.SetParent(sheet.transform);
        crate.transform.localPosition = new UnityEngine.Vector3(x + facing * 1.3f, y + .45f);
        var sr = crate.AddComponent<UnityEngine.SpriteRenderer>(); sr.sprite = crateSprite; sr.drawMode = UnityEngine.SpriteDrawMode.Tiled;
        sr.size = new UnityEngine.Vector2(1.6f, .9f); sr.sortingOrder = 2;
        var label = new UnityEngine.GameObject("PoseLabel"); label.transform.SetParent(sheet.transform);
        label.transform.localPosition = new UnityEngine.Vector3(x + facing * .7f, y - .35f);
        var text = label.AddComponent<UnityEngine.TextMesh>(); text.text = new[] { "CONCEALED", "BLIND-FIRE", "PEEK" }[col];
        text.font = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Font>("LegacyRuntime.ttf"); text.fontSize = 48; text.characterSize = .045f;
        text.anchor = UnityEngine.TextAnchor.MiddleCenter; text.alignment = UnityEngine.TextAlignment.Center;
        text.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial = text.font.material;
    }
    preview.camera.orthographic = true; preview.camera.orthographicSize = 3.5f;
    preview.camera.transform.position = new UnityEngine.Vector3(0, 1, -10); preview.camera.transform.rotation = UnityEngine.Quaternion.identity;
    preview.camera.clearFlags = UnityEngine.CameraClearFlags.SolidColor; preview.camera.backgroundColor = new UnityEngine.Color(.1f, .14f, .18f);
    preview.BeginStaticPreview(new UnityEngine.Rect(0, 0, 1920, 900)); preview.Render(true, false); image = preview.EndStaticPreview();
    string path = System.IO.Path.Combine(System.IO.Directory.GetParent(UnityEngine.Application.dataPath).FullName, "TestResults/Sprint022_Poses.png");
    System.IO.File.WriteAllBytes(path, image.EncodeToPNG()); return path;
}
finally { if (image != null) UnityEngine.Object.DestroyImmediate(image); preview.Cleanup(); }
