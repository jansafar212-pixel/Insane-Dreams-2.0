using UnityEngine;

namespace InsaneDreams.Visuals
{
    public sealed class CutoutRig : MonoBehaviour
    {
        public CharacterSkin skin;
        public Transform head, torso, leftArm, rightArm, leftLeg, rightLeg, weaponMount, accessoryMounts, speechAnchor;
        public SpriteRenderer face, weapon, parachute;
        public Transform muzzle;
        public SpriteRenderer magazine;
        [SerializeField] private float crouchedMuzzleCeiling = .02f;
        public void SetMagazine(Sprite sprite)
        {
            if (sprite != null && magazine == null)
            {
                var go = new GameObject("ExtendedMagazine"); go.transform.SetParent(weapon.transform, false);
                magazine = go.AddComponent<SpriteRenderer>(); magazine.sortingLayerID = weapon.sortingLayerID; magazine.sortingOrder = weapon.sortingOrder + 1;
            }
            if (magazine != null) { magazine.sprite = sprite; magazine.enabled = sprite != null && weapon.enabled; }
        }
        public Vector2 MuzzlePosition => muzzle != null ? (Vector2)muzzle.position : (Vector2)weapon.transform.position;
        public Vector2 BarrelDirection => weapon.transform.TransformVector(Vector3.right).normalized;
        [SerializeField] private float standingFeetY = -.7f;
        [SerializeField] private float torsoCenterY = .09375f;
        [SerializeField] private float shoulderHalfWidth = .1875f;
        private float climbBlend;
        public Transform[] BodyParts => new[] { torso, head, leftArm, rightArm, leftLeg, rightLeg };
        public bool IsBound => skin != null && skin.IsComplete && head && torso && leftArm && rightArm && leftLeg && rightLeg && weaponMount && accessoryMounts && speechAnchor && face && weapon && parachute;
        public void ApplySkin(CharacterSkin value)
        {
            skin = value;
            if (skin == null) return;
            Set(head, skin.head); Set(torso, skin.torso); Set(leftArm, skin.arm); Set(rightArm, skin.arm); Set(leftLeg, skin.leg); Set(rightLeg, skin.leg);
            torso.GetComponent<SpriteRenderer>().color = skin.clothing;
            weapon.sprite = skin.weapon; parachute.sprite = skin.parachute;
            SetExpression(FaceExpression.Neutral);
        }
        private static void Set(Transform part, Sprite sprite) { if (part != null) part.GetComponent<SpriteRenderer>().sprite = sprite; }
        public void SetExpression(FaceExpression expression) { if (face != null && skin != null) face.sprite = skin.Face(expression); }
        private void Awake() { ApplySkin(skin); EnsureMuzzle(); }
        public void EnsureMuzzle()
        {
            if (weapon == null) return;
            if (muzzle == null) { muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(weapon.transform, false); }
            // SpriteRenderer.flipY mirrors pixels without mirroring child transforms.
            if (weapon.sprite != null) muzzle.localPosition = new Vector3(weapon.sprite.bounds.max.x, (weapon.flipY ? -1f : 1f) / 32, 0);
        }

        public void Pose(CharacterPose pose, float phase, Vector2 aim, float facing, float recoil, FaceExpression expression, bool aimWhileClimbing = false, bool hasWeapon = true, bool reloading = false, float reloadProgress = 0)
        {
            // All values are local visual offsets. The gameplay root and collider are never moved here.
            transform.localScale = new Vector3(facing, 1, 1);
            bool crouch = pose == CharacterPose.Crouch || pose == CharacterPose.CrouchWalk;
            float step = Mathf.Sin(phase), bob = pose == CharacterPose.Walk || pose == CharacterPose.CrouchWalk ? Mathf.Abs(step) / 32 : 0;
            float torsoHalf = skin.torso.bounds.extents.y;
            float hipY = crouch ? -.48f : standingFeetY + skin.leg.bounds.size.y;
            float shoulderY = crouch ? -.2f : torsoCenterY + torsoHalf - .0625f;
            float headY = crouch ? -.1f : torsoCenterY + torsoHalf + skin.head.bounds.extents.y;
            torso.localPosition = new Vector3(0, crouch ? -.28f : torsoCenterY + bob, 0);
            torso.localScale = new Vector3(1, crouch ? .65f : 1, 1);
            torso.localRotation = Quaternion.Euler(0, 0, pose == CharacterPose.Panic ? -12 : 0);
            head.localPosition = new Vector3(crouch ? .06f : 0, headY + bob, 0);
            head.localScale = Vector3.one * (crouch ? .8f : 1);
            head.localRotation = Quaternion.Euler(0, 0, expression == FaceExpression.Smug ? -7 : pose == CharacterPose.Panic ? 12 : 0);
            leftLeg.localPosition = new Vector3(-.12f, hipY, 0); rightLeg.localPosition = new Vector3(.12f, hipY, 0);
            leftLeg.localScale = rightLeg.localScale = new Vector3(1, crouch ? .4f : 1, 1);
            float legAngle = pose == CharacterPose.Walk || pose == CharacterPose.CrouchWalk ? step * 27 : pose == CharacterPose.Jump ? 25 : pose == CharacterPose.Fall ? -12 : 0;
            if (pose == CharacterPose.Climb) legAngle = step * 32;
            leftLeg.localRotation = Quaternion.Euler(0, 0, legAngle); rightLeg.localRotation = Quaternion.Euler(0, 0, -legAngle);
            leftArm.localPosition = new Vector3(-shoulderHalfWidth, shoulderY + bob, 0); rightArm.localPosition = new Vector3(shoulderHalfWidth, shoulderY + bob, 0);
            leftArm.localScale = rightArm.localScale = Vector3.one * (crouch ? .75f : 1);
            rightArm.localScale = new Vector3(rightArm.localScale.x, rightArm.localScale.y * (1 - recoil * .1f), rightArm.localScale.z);
            // Mirror the direction into rig space; world-space aim stays independent of walking direction.
            Vector2 localAim = new Vector2(aim.x * facing, aim.y).normalized;
            float angle = Mathf.Atan2(localAim.y, localAim.x) * Mathf.Rad2Deg;
            float targetBlend = pose == CharacterPose.Climb ? 1 : 0;
            climbBlend = Application.isPlaying ? Mathf.MoveTowards(climbBlend, targetBlend, Time.deltaTime * 8) : targetBlend;
            float carryingAngle = Mathf.LerpAngle(angle, -30, aimWhileClimbing ? 0 : climbBlend);
            // A handling dip and two support-hand strokes follow the real reload interval.
            float handling = reloading ? Mathf.Sin(Mathf.PI * reloadProgress) : 0;
            bool oversized = magazine != null && magazine.sprite != null;
            float dip = oversized ? 75 + Mathf.Sin(reloadProgress * Mathf.PI * 8) * 12 : 32;
            carryingAngle += (Mathf.Cos(angle * Mathf.Deg2Rad) >= 0 ? -1 : 1) * handling * dip;
            float armAngle = carryingAngle + 90 - recoil * 18;
            bool raised = pose == CharacterPose.Panic || pose == CharacterPose.Parachute && !aimWhileClimbing && !reloading;
            leftArm.localRotation = Quaternion.Euler(0, 0, raised ? -155 : Mathf.LerpAngle(armAngle, 150 + step * 30, climbBlend));
            if (reloading && !raised) leftArm.localRotation = Quaternion.Euler(0, 0, armAngle + handling * ((oversized ? 85 : 35) + Mathf.Sin(reloadProgress * Mathf.PI * 4) * 35));
            rightArm.localRotation = Quaternion.Euler(0, 0, raised ? 155 : armAngle);
            Vector3 hand = rightArm.localPosition + rightArm.localRotation * (Vector3.down * skin.arm.bounds.size.y * rightArm.localScale.y);
            weaponMount.localPosition = hand;
            // Recoil shortens the carrying arm; it never falsifies the barrel/shot angle.
            weaponMount.localRotation = Quaternion.Euler(0, 0, carryingAngle);
            weapon.flipY = Mathf.Cos(carryingAngle * Mathf.Deg2Rad) < 0;
            weapon.enabled = hasWeapon && !raised && pose != CharacterPose.Defeated;
            parachute.enabled = pose == CharacterPose.Parachute;
            SetExpression(expression);
            EnsureMuzzle();
            if (crouch)
            {
                float excess = transform.InverseTransformPoint(muzzle.position).y - crouchedMuzzleCeiling;
                if (excess > 0) RetractMuzzle(MuzzlePosition + (Vector2)transform.TransformVector(Vector3.down * excess));
            }
            if (magazine != null)
            {
                float flip = weapon.flipY ? -1 : 1;
                magazine.enabled = magazine.sprite != null && weapon.enabled;
                magazine.transform.localPosition = new Vector3(.125f + (oversized ? Mathf.Sin(reloadProgress * Mathf.PI * 4) * handling * .18f : 0), flip * (-.0625f - handling * .42f), -.01f);
                magazine.transform.localScale = new Vector3(1, flip, 1);
                magazine.transform.localRotation = Quaternion.Euler(0, 0, flip * handling * Mathf.Sin(reloadProgress * Mathf.PI * 4) * 30);
            }
        }

        // Pull the held gun and carrying hand back together when the barrel meets cover.
        // This is presentation only; neither the actor nor any collider is teleported.
        public void RetractMuzzle(Vector2 position)
        {
            Vector3 shift = (Vector3)(position - MuzzlePosition);
            weaponMount.position += shift;
            Vector3 hand = rightArm.parent.InverseTransformPoint(weaponMount.position);
            Vector3 reach = hand - rightArm.localPosition;
            rightArm.localRotation = Quaternion.Euler(0,0,Mathf.Atan2(reach.y,reach.x)*Mathf.Rad2Deg+90);
            Vector3 scale=rightArm.localScale;scale.y=reach.magnitude/Mathf.Max(.001f,skin.arm.bounds.size.y);rightArm.localScale=scale;
        }
    }
}
