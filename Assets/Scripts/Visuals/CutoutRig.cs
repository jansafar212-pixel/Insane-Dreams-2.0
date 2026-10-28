using UnityEngine;

namespace InsaneDreams.Visuals
{
    public sealed class CutoutRig : MonoBehaviour
    {
        [SerializeField] private SamuraiArticulatedRig segmented;
        public SamuraiArticulatedRig Segmented=>segmented;
        [SerializeField, Range(.5f,1.5f)] private float visualScale=1;
        public float VisualScale=>visualScale;
        [SerializeField] private Vector2 presentationOffset;
        public Vector2 PresentationOffset=>presentationOffset;
        public float GroundAlignmentOffset=>standingFeetY*(1-visualScale);
        public CharacterSkin skin;
        public Transform head, torso, leftArm, rightArm, leftLeg, rightLeg, weaponMount, accessoryMounts, speechAnchor;
        public SpriteRenderer face, weapon, parachute;
        public Transform muzzle;
        public SpriteRenderer magazine;
        [SerializeField] private float crouchedMuzzleCeiling = -.06f;
        private Vector2 magazineMountOffset=new Vector2(.125f,-.0625f);
        private bool physicalMagazineReload;
        private SpriteRenderer replacementMagazine;
        private SpriteRenderer reloadShell;
        public SpriteRenderer ReloadShell=>reloadShell;
        public void PoseReloadShell(Sprite sprite,bool active,float progress)
        {
            if(reloadShell==null&&sprite!=null){var go=new GameObject("ReloadShell");go.transform.SetParent(weapon.transform,false);reloadShell=go.AddComponent<SpriteRenderer>();reloadShell.sortingLayerID=weapon.sortingLayerID;reloadShell.sortingOrder=weapon.sortingOrder+2;}
            if(reloadShell==null)return;reloadShell.sprite=sprite;reloadShell.enabled=active&&sprite!=null&&weapon.enabled&&progress<.92f;
            float flip=weapon.flipY?-1:1;
            reloadShell.transform.localPosition=Vector3.Lerp(new Vector3(.22f,-.4f*flip,-.02f),new Vector3(.3f,-.04f*flip,-.02f),Mathf.SmoothStep(0,1,progress));
            reloadShell.transform.localRotation=Quaternion.Euler(0,0,90);reloadShell.transform.localScale=new Vector3(1,flip,1);
            if(active&&sprite!=null)
            {
                Vector3 to=transform.InverseTransformPoint(reloadShell.transform.position)-leftArm.localPosition;
                float reach=Mathf.Atan2(to.x,-to.y)*Mathf.Rad2Deg;
                leftArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(leftArm.localEulerAngles.z,reach,Mathf.Sin(progress*Mathf.PI)));
            }
        }
        public void SetMagazine(Sprite sprite,Vector2? mountOffset=null,bool physicalExchange=false)
        {
            physicalMagazineReload=physicalExchange;
            magazineMountOffset=mountOffset??new Vector2(.125f,-.0625f);
            if (sprite != null && magazine == null)
            {
                var go = new GameObject("ExtendedMagazine"); go.transform.SetParent(weapon.transform, false);
                magazine = go.AddComponent<SpriteRenderer>(); magazine.sortingLayerID = weapon.sortingLayerID; magazine.sortingOrder = weapon.sortingOrder + 1;
            }
            if (magazine != null) { if(magazine.sprite!=sprite)magazine.sprite = sprite; magazine.enabled = sprite != null && weapon.enabled; }
            if(physicalExchange&&replacementMagazine==null)
            {var go=new GameObject("ReplacementMagazine");go.transform.SetParent(weapon.transform,false);replacementMagazine=go.AddComponent<SpriteRenderer>();replacementMagazine.sortingLayerID=weapon.sortingLayerID;replacementMagazine.sortingOrder=weapon.sortingOrder+1;}
            if(replacementMagazine!=null){if(replacementMagazine.sprite!=sprite)replacementMagazine.sprite=sprite;replacementMagazine.enabled=false;}
        }
        public void FinishMagazineExchange(){if(replacementMagazine!=null)replacementMagazine.enabled=false;if(magazine!=null)magazine.enabled=magazine.sprite!=null&&weapon.enabled;}
        public void HideDefaultReplacementMagazine(){if(replacementMagazine!=null)replacementMagazine.enabled=false;}
        public Vector2 MuzzlePosition => muzzle != null ? (Vector2)muzzle.position : (Vector2)weapon.transform.position;
        public Vector2 BarrelDirection => weapon.transform.TransformVector(Vector3.right).normalized;
        [SerializeField] private float standingFeetY = -.7f;
        [SerializeField] private float torsoCenterY = .09375f;
        [SerializeField] private float shoulderHalfWidth = .1875f;
        private float climbBlend;
        [SerializeField, Min(.02f)] private float crouchTransitionSeconds=.14f;
        [SerializeField, Range(0,65)] private float crouchLegDegrees=55;
        private float crouchBlend;
        [SerializeField, Range(.5f,1)] private float rearArmShade=.88f;
        private SpriteRenderer rearArmRenderer;
        private int rearArmOrder;
        public Color RearArmTint=>new Color(rearArmShade,rearArmShade,rearArmShade,1);
        public Transform[] BodyParts => new[] { torso, head, leftArm, rightArm, leftLeg, rightLeg };
        public bool IsBound => skin != null && skin.IsComplete && head && torso && leftArm && rightArm && leftLeg && rightLeg && weaponMount && accessoryMounts && speechAnchor && face && weapon && parachute;
        public void ApplySkin(CharacterSkin value)
        {
            skin = value;
            if (skin == null) return;
            Set(head, skin.head); Set(torso, skin.torso); Set(leftArm, skin.arm); Set(rightArm, skin.arm); Set(leftLeg, skin.leg); Set(rightLeg, skin.leg);
            torso.GetComponent<SpriteRenderer>().color = skin.clothing;
            rearArmRenderer=leftArm.GetComponent<SpriteRenderer>();
            rearArmRenderer.enabled=rightArm.GetComponent<SpriteRenderer>().enabled=true;
            rearArmRenderer.color=RearArmTint;
            rearArmRenderer.sortingLayerID=torso.GetComponent<SpriteRenderer>().sortingLayerID;
            rearArmOrder=torso.GetComponent<SpriteRenderer>().sortingOrder-1;rearArmRenderer.sortingOrder=rearArmOrder;
            weapon.sprite = skin.weapon; parachute.sprite = skin.parachute;
            SetExpression(FaceExpression.Neutral);
            if(segmented!=null)segmented.BindSprites();
        }
        private static void Set(Transform part, Sprite sprite) { if (part != null) part.GetComponent<SpriteRenderer>().sprite = sprite; }
        public void SetExpression(FaceExpression expression) { if (face != null && skin != null) {var sprite=skin.Face(expression);if(face.sprite!=sprite)face.sprite=sprite;} }
        private void Awake() { ApplySkin(skin); EnsureMuzzle(); }
        public void EnsureMuzzle()
        {
            if (weapon == null) return;
            if (muzzle == null) { muzzle = new GameObject("Muzzle").transform; muzzle.SetParent(weapon.transform, false); }
            // SpriteRenderer.flipY mirrors pixels without mirroring child transforms.
            if (weapon.sprite != null) muzzle.localPosition = new Vector3(weapon.sprite.bounds.max.x, (weapon.flipY ? -1f : 1f) / 32, 0);
        }
        public void ApplyWeaponMuzzle(Vector2 value,bool crouched)
        {
            if(muzzle==null)return;muzzle.localPosition=new Vector3(value.x,(weapon.flipY?-1:1)*value.y,0);
            if(crouched)
            {
                float excess=transform.InverseTransformPoint(muzzle.position).y-crouchedMuzzleCeiling;
                if(excess>0)RetractMuzzle(MuzzlePosition+(Vector2)transform.TransformVector(Vector3.down*excess));
            }
        }

        public void Pose(CharacterPose pose, float phase, Vector2 aim, float facing, float recoil, FaceExpression expression, bool aimWhileClimbing = false, bool hasWeapon = true, bool reloading = false, float reloadProgress = 0, float coverCrouch = -1, float upperBodyTuck = 0, bool raisedCoverWeapon = false, bool advanceTransitions = true)
        {
            if(rearArmRenderer==null)ApplySkin(skin);
            // All values are local visual offsets. The gameplay root and collider are never moved here.
            transform.localScale = new Vector3(facing*visualScale, visualScale, 1);
            bool crouch = pose == CharacterPose.Crouch || pose == CharacterPose.CrouchWalk;
            if(advanceTransitions)crouchBlend=Application.isPlaying?Mathf.MoveTowards(crouchBlend,crouch?1:0,Time.deltaTime/crouchTransitionSeconds):crouch?1:0;
            if (crouch && coverCrouch >= 0) crouchBlend = coverCrouch;
            float step = Mathf.Sin(phase), bob = pose == CharacterPose.Walk || pose == CharacterPose.CrouchWalk ? Mathf.Abs(step) / 32 : 0;
            float torsoHalf = skin.torso.bounds.extents.y;
            float anatomyOffset = torsoCenterY - .09375f;
            float shoulderY = Mathf.Lerp(torsoCenterY + torsoHalf - .0625f,-.03f + anatomyOffset,crouchBlend);
            float headY = Mathf.Lerp(torsoCenterY + torsoHalf + skin.head.bounds.extents.y,.12f + anatomyOffset,crouchBlend);
            shoulderY += upperBodyTuck; headY += upperBodyTuck;
            torso.localPosition = new Vector3(0,Mathf.Lerp(torsoCenterY,-.18f + anatomyOffset,crouchBlend)+bob*(1-crouchBlend),0);
            torso.localScale = Vector3.one;
            torso.localPosition += Vector3.up * upperBodyTuck;
            torso.localRotation = Quaternion.Euler(0, 0, pose == CharacterPose.Panic ? -12 : -8*crouchBlend);
            head.localPosition = new Vector3(.08f*crouchBlend, headY + bob*(1-crouchBlend), 0);
            head.localScale = Vector3.one;
            head.localRotation = Quaternion.Euler(0, 0, expression == FaceExpression.Smug ? -7 : pose == CharacterPose.Panic ? 12 : 0);
            leftLeg.localScale = rightLeg.localScale = Vector3.one;
            float legAngle = pose == CharacterPose.Walk || pose == CharacterPose.CrouchWalk ? step * 27 : pose == CharacterPose.Jump ? 25 : pose == CharacterPose.Fall ? -12 : 0;
            if (pose == CharacterPose.Climb) legAngle = step * 32;
            legAngle=Mathf.Lerp(legAngle,crouchLegDegrees+(pose==CharacterPose.CrouchWalk?step*5:0),crouchBlend);
            // Rigid limbs fold at their hips; solve the pivot height from their actual
            // sprite bounds so the feet stay on the same floor throughout the blend.
            float hipY=standingFeetY+skin.leg.bounds.size.y*Mathf.Cos(legAngle*Mathf.Deg2Rad)+skin.leg.bounds.extents.x*Mathf.Abs(Mathf.Sin(legAngle*Mathf.Deg2Rad));
            leftLeg.localPosition=new Vector3(-.09f,hipY,0);rightLeg.localPosition=new Vector3(.09f,hipY,0);
            leftLeg.localRotation = Quaternion.Euler(0, 0, legAngle); rightLeg.localRotation = Quaternion.Euler(0, 0, -legAngle);
            leftArm.localPosition = new Vector3(-shoulderHalfWidth, shoulderY + bob, 0); rightArm.localPosition = new Vector3(shoulderHalfWidth, shoulderY + bob, 0);
            leftArm.localScale = rightArm.localScale = Vector3.one;
            rightArm.localScale = new Vector3(rightArm.localScale.x, rightArm.localScale.y * (1 - recoil * .1f), rightArm.localScale.z);
            // Mirror the direction into rig space; world-space aim stays independent of walking direction.
            Vector2 localAim = new Vector2(aim.x * facing, aim.y).normalized;
            float angle = Mathf.Atan2(localAim.y, localAim.x) * Mathf.Rad2Deg;
            float targetBlend = pose == CharacterPose.Climb ? 1 : 0;
            if(advanceTransitions)climbBlend = Application.isPlaying ? Mathf.MoveTowards(climbBlend, targetBlend, Time.deltaTime * 8) : targetBlend;
            float carryingAngle = Mathf.LerpAngle(angle, -30, aimWhileClimbing ? 0 : climbBlend);
            // A handling dip and two support-hand strokes follow the real reload interval.
            float handling = reloading ? Mathf.Sin(Mathf.PI * reloadProgress) : 0;
            bool oversized = magazine != null && magazine.sprite != null;
            float dip = physicalMagazineReload ? 18 : oversized ? 75 + Mathf.Sin(reloadProgress * Mathf.PI * 8) * 12 : 32;
            carryingAngle += (Mathf.Cos(angle * Mathf.Deg2Rad) >= 0 ? -1 : 1) * handling * dip;
            float armAngle = carryingAngle + 90 - recoil * 18;
            bool raised = pose == CharacterPose.Panic || pose == CharacterPose.Parachute && !aimWhileClimbing && !reloading;
            // The weapon remains in the original carrying hand. The free arm hangs
            // outside the torso silhouette instead of coinciding with the carrying arm.
            float freeArmAngle=(crouch?-20:-14)-(pose==CharacterPose.Walk||pose==CharacterPose.CrouchWalk?step*12:0)-recoil*4;
            freeArmAngle -= Mathf.Clamp(-upperBodyTuck * 160, 0, 25);
            float rearAngle=Mathf.LerpAngle(freeArmAngle,150+step*30,climbBlend);
            if(reloading&&!raised)rearAngle=Mathf.LerpAngle(freeArmAngle,armAngle+handling*((oversized?85:35)+Mathf.Sin(reloadProgress*Mathf.PI*4)*35),handling);
            leftArm.localRotation=Quaternion.Euler(0,0,raised?-155:rearAngle);
            // A reaching hand passes in front of the shirt while handling the magazine,
            // still behind the carrying arm and gun; otherwise it stays behind the torso.
            rearArmRenderer.sortingOrder=rearArmOrder+(reloading&&!raised&&climbBlend<.5f?2:0);
            rightArm.localRotation = Quaternion.Euler(0, 0, raised ? 155 : armAngle);
            float exchangeBlend=physicalMagazineReload&&reloading&&!raised?Mathf.SmoothStep(0,1,reloadProgress/.18f)*(1-Mathf.SmoothStep(0,1,(reloadProgress-.88f)/.12f)):0;
            if(exchangeBlend>0){rightArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(armAngle,-65,exchangeBlend));carryingAngle=Mathf.LerpAngle(carryingAngle,-15,exchangeBlend);}
            Vector3 hand = rightArm.localPosition + rightArm.localRotation * (Vector3.down * skin.arm.bounds.size.y * rightArm.localScale.y);
            weaponMount.localPosition = hand;
            // Recoil shortens the carrying arm; it never falsifies the barrel/shot angle.
            weaponMount.localRotation = Quaternion.Euler(0, 0, carryingAngle);
            weapon.flipY = Mathf.Cos(carryingAngle * Mathf.Deg2Rad) < 0;
            weapon.enabled = hasWeapon && !raised && pose != CharacterPose.Defeated;
            parachute.enabled = pose == CharacterPose.Parachute;
            SetExpression(expression);
            if(pose==CharacterPose.Panic&&skin.panicFace!=null&&face.sprite!=skin.panicFace)face.sprite=skin.panicFace;
            EnsureMuzzle();
            if(segmented!=null&&segmented.firearm)segmented.PoseFirearm(pose,phase,0);
            if (crouch && !raisedCoverWeapon)
            {
                float excess = transform.InverseTransformPoint(muzzle.position).y - crouchedMuzzleCeiling;
                if (excess > 0) RetractMuzzle(MuzzlePosition + (Vector2)transform.TransformVector(Vector3.down * excess));
            }
            if (magazine != null)
            {
                float flip = weapon.flipY ? -1 : 1;
                magazine.enabled = magazine.sprite != null && weapon.enabled;
                magazine.transform.localPosition = new Vector3(magazineMountOffset.x + (oversized ? Mathf.Sin(reloadProgress * Mathf.PI * 4) * handling * .18f : 0), flip * (magazineMountOffset.y - handling * .42f), -.01f);
                magazine.transform.localScale = new Vector3(1, flip, 1);
                magazine.transform.localRotation = Quaternion.Euler(0, 0, flip * handling * Mathf.Sin(reloadProgress * Mathf.PI * 4) * 30);
                if(physicalMagazineReload)
                {
                    float p=reloadProgress;Vector2 socket=magazineMountOffset;
                    Vector2 away=socket+new Vector2(-.2f,-.2f);
                    Vector2 old=Vector2.Lerp(socket,away,Mathf.SmoothStep(0,1,(p-.18f)/.28f));
                    Vector2 fresh=Vector2.Lerp(socket+new Vector2(.08f,-.17f),socket,Mathf.SmoothStep(0,1,(p-.62f)/.26f));
                    magazine.transform.localPosition=reloading?new Vector3(old.x,flip*old.y,-.01f):new Vector3(socket.x,flip*socket.y,-.01f);
                    magazine.transform.localRotation=Quaternion.identity;magazine.enabled=weapon.enabled&&(!reloading||p<.5f);
                    replacementMagazine.enabled=weapon.enabled&&reloading&&p>=.6f;
                    replacementMagazine.transform.localPosition=new Vector3(fresh.x,flip*fresh.y,-.01f);
                    replacementMagazine.transform.localScale=new Vector3(1,flip,1);replacementMagazine.transform.localRotation=Quaternion.identity;
                    if(reloading&&exchangeBlend>0)
                    {
                        var handTarget=Vector3.Lerp(magazine.transform.position,replacementMagazine.transform.position,Mathf.SmoothStep(0,1,(p-.5f)/.1f));
                        Vector3 to=transform.InverseTransformPoint(handTarget)-leftArm.localPosition;
                        float reach=Mathf.Atan2(to.x,-to.y)*Mathf.Rad2Deg;
                        leftArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(rearAngle,reach,exchangeBlend));
                    }
                }
            }
        }

        public void PoseSamurai(float bladeAngle)
        {
            if(segmented!=null){segmented.PoseSword(bladeAngle);return;}
            rightArm.localRotation=Quaternion.Euler(0,0,bladeAngle+90);
            weaponMount.localPosition=rightArm.localPosition+rightArm.localRotation*(Vector3.down*skin.arm.bounds.size.y);
            weaponMount.localRotation=Quaternion.Euler(0,0,bladeAngle);
            weapon.flipY=false;
        }

        public void PoseFireworks(float handAngle,bool showBundle)
        {
            rightArm.localRotation=Quaternion.Euler(0,0,handAngle);
            weaponMount.localPosition=rightArm.localPosition+rightArm.localRotation*(Vector3.down*skin.arm.bounds.size.y*rightArm.localScale.y);
            weaponMount.localRotation=Quaternion.identity;weapon.flipY=false;weapon.enabled=showBundle;
            if(magazine!=null)magazine.enabled=false;
        }

        public void RaiseWeaponOverCover(float top, float nearEdge, float clearance = .1f)
        {
            if (!weapon.enabled) return;
            // Rotate the existing rigid carrying arm; retain the real barrel angle and
            // shoulder attachment. The raised hand clears the near face before the gun.
            Vector2 shoulder = rightArm.position;
            float length = skin.arm.bounds.size.y * Mathf.Abs(rightArm.lossyScale.y);
            float direction = Mathf.Abs(BarrelDirection.x) > .1f ? Mathf.Sign(BarrelDirection.x) : Mathf.Sign(transform.lossyScale.x);
            float muzzleRise = MuzzlePosition.y - weaponMount.position.y;
            float requiredRise = Mathf.Clamp(top + clearance - muzzleRise - shoulder.y, 0, length);
            float gap = Mathf.Max(.005f, direction * (nearEdge - shoulder.x));
            float angle = Mathf.Max(Mathf.Asin(requiredRise / Mathf.Max(.001f, length)), Mathf.Atan2(Mathf.Max(0, top + .04f - shoulder.y), gap));
            Vector2 hand = shoulder + new Vector2(direction * Mathf.Cos(angle), Mathf.Sin(angle)) * length;
            weaponMount.position = hand;
            Vector3 localHand = transform.InverseTransformPoint(hand);
            Vector3 reach = localHand - rightArm.localPosition;
            rightArm.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(reach.y, reach.x) * Mathf.Rad2Deg + 90);
        }

        // Pull the held gun and carrying hand back together when the barrel meets cover.
        // This is presentation only; neither the actor nor any collider is teleported.
        public void RetractMuzzle(Vector2 position)
        {
            if(segmented!=null){segmented.RetractMuzzle(position);return;}
            Vector3 shift = (Vector3)(position - MuzzlePosition);
            weaponMount.position += shift;
            Vector3 hand = rightArm.parent.InverseTransformPoint(weaponMount.position);
            Vector3 reach = hand - rightArm.localPosition;
            rightArm.localRotation = Quaternion.Euler(0,0,Mathf.Atan2(reach.y,reach.x)*Mathf.Rad2Deg+90);
            Vector3 scale=rightArm.localScale;scale.y=reach.magnitude/Mathf.Max(.001f,skin.arm.bounds.size.y);rightArm.localScale=scale;
        }
    }
}
