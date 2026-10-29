using UnityEngine;

namespace InsaneDreams.Visuals
{
    // Optional segmented cutout extension; keeps the existing pose and ragdoll owners.
    public sealed class SamuraiArticulatedRig : MonoBehaviour
    {
        public Transform leftElbow,rightElbow,leftHand,rightHand,leftKnee,rightKnee,leftFoot,rightFoot,grip;
        public Sprite upperArm,forearm,hand,thigh,shin,foot;
        public bool firearm;
        [SerializeField,Range(8,140)] private float elbowLimit=135;
        [SerializeField,Range(45,140)] private float kneeLimit=120;
        [SerializeField,Min(.01f)] private float blendSeconds=.09f;
        private CutoutRig rig;private float airJumpUntil,landUntil;private bool grounded=true,posed;
        public bool IsBound=>leftElbow&&rightElbow&&leftHand&&rightHand&&leftKnee&&rightKnee&&leftFoot&&rightFoot&&grip&&upperArm&&forearm&&hand&&thigh&&shin&&foot;
        private float ArmUpperLength=>upperArm.bounds.size.y-.01125f;
        private float ArmLowerLength=>forearm.bounds.size.y-.01125f;
        public Transform[] ExtraParts=>new[]{leftElbow,leftHand,rightElbow,rightHand,leftKnee,leftFoot,rightKnee,rightFoot};
        private void Awake(){rig=GetComponent<CutoutRig>();BindSprites();}
        public void BindSprites()
        {
            if(rig==null)rig=GetComponent<CutoutRig>();if(!IsBound||rig==null)return;
            Set(rig.leftArm,upperArm);Set(rig.rightArm,upperArm);Set(leftElbow,forearm);Set(rightElbow,forearm);Set(leftHand,hand);Set(rightHand,hand);
            Set(rig.leftLeg,thigh);Set(rig.rightLeg,thigh);Set(leftKnee,shin);Set(rightKnee,shin);Set(leftFoot,foot);Set(rightFoot,foot);
            leftElbow.localPosition=rightElbow.localPosition=Vector3.down*ArmUpperLength;
            leftHand.localPosition=rightHand.localPosition=Vector3.down*ArmLowerLength;
            leftKnee.localPosition=rightKnee.localPosition=Vector3.down*(thigh.bounds.size.y-.01125f);
            leftFoot.localPosition=rightFoot.localPosition=Vector3.down*(shin.bounds.size.y-.01125f);
            grip.localPosition=Vector3.down*hand.bounds.size.y*.55f;
        }
        private static void Set(Transform t,Sprite s){t.GetComponent<SpriteRenderer>().sprite=s;}
        public void AirJump()=>airJumpUntil=Time.time+.24f;
        public void Pose(CharacterPose pose,float phase,float bladeAngle,bool onGround,float dt)
        {
            if(!IsBound)return;if(rig==null)rig=GetComponent<CutoutRig>();
            if(onGround&&!grounded)landUntil=Time.time+.24f;grounded=onGround;
            // Base poses reset root joints each frame; preserve our filtered angles separately.
            float step=Mathf.Sin(phase),compression=Mathf.Sin(Mathf.PI*Mathf.Clamp01(1-(landUntil-Time.time)/.24f));
            bool run=pose==CharacterPose.Walk||pose==CharacterPose.CrouchWalk;
            bool air=pose==CharacterPose.Jump||pose==CharacterPose.Fall;
            bool tuck=Time.time<airJumpUntil&&air;
            float bend=pose==CharacterPose.Crouch||pose==CharacterPose.CrouchWalk?95:air?(tuck?100:55):compression*45;
            Leg(rig.leftLeg,leftKnee,leftFoot,run?step*28:air?(tuck?50:28):bend*.5f,Mathf.Min(kneeLimit,bend+(run?Mathf.Max(0,-step)*55:0)),dt,0);
            Leg(rig.rightLeg,rightKnee,rightFoot,run?-step*28:air?(tuck?38:18):bend*.5f,Mathf.Min(kneeLimit,bend+(run?Mathf.Max(0,step)*55:0)),dt,3);
            float rear=pose==CharacterPose.Climb?135+step*25:run?-12-step*22:-12;
            ArmAngles(rig.leftArm,leftElbow,leftHand,rear,pose==CharacterPose.Climb?45:25,0,dt,6);
            if(pose==CharacterPose.Climb)ArmAngles(rig.rightArm,rightElbow,rightHand,135-step*25,45,0,dt,9);
            else SolveSword(bladeAngle,dt);
            FollowGrip();
            if(onGround)
            {
                float toe=Mathf.Min(transform.InverseTransformPoint(leftFoot.TransformPoint(Vector3.down*foot.bounds.size.y)).y,transform.InverseTransformPoint(rightFoot.TransformPoint(Vector3.down*foot.bounds.size.y)).y);
                transform.position+=transform.TransformVector(Vector3.up*Mathf.Clamp(-.7f-toe,-.15f,.08f));
            }
            posed=true;
        }
        private readonly float[] angles=new float[12];
        private void Joint(Transform t,float angle,float dt,int index)
        {
            angles[index]=posed&&dt>0?Mathf.LerpAngle(angles[index],angle,1-Mathf.Exp(-dt/Mathf.Max(.01f,blendSeconds))):angle;
            t.localRotation=Quaternion.Euler(0,0,angles[index]);
        }
        private void Leg(Transform hip,Transform knee,Transform ankle,float swing,float bend,float dt,int index)
        {
            hip.localScale=Vector3.one;Joint(hip,swing,dt,index);Joint(knee,-Mathf.Clamp(bend,2,kneeLimit),dt,index+1);
            Joint(ankle,Mathf.Clamp(-swing+bend,-40,45),dt,index+2);
        }
        private void ArmAngles(Transform shoulder,Transform elbow,Transform wrist,float upper,float bend,float wristAngle,float dt,int index)
        {
            shoulder.localScale=Vector3.one;Joint(shoulder,upper,dt,index);Joint(elbow,Mathf.Clamp(bend,8,elbowLimit),dt,index+1);Joint(wrist,Mathf.Clamp(wristAngle,-55,55),dt,index+2);
        }
        public void PoseSword(float angle){if(rig==null)rig=GetComponent<CutoutRig>();if(IsBound)SolveSword(angle,0);}
        private void SolveSword(float blade,float dt)
        {
            float reach=(ArmUpperLength+ArmLowerLength)*Mathf.Lerp(.80f,.96f,Mathf.Clamp01((65-blade)/100));
            Vector2 direction=new Vector2(Mathf.Cos(blade*Mathf.Deg2Rad),Mathf.Sin(blade*Mathf.Deg2Rad));
            Vector2 target=(Vector2)rig.rightArm.localPosition+direction*(reach+hand.bounds.size.y*.55f);
            SolveGrip(target,blade,dt);
        }
        public void PoseFirearm(CharacterPose pose,float phase,float dt)
        {
            if(!IsBound)return;if(rig==null)rig=GetComponent<CutoutRig>();
            // Reuse the base pose's aim/reload/cover hand target, then articulate it.
            Vector2 target=transform.InverseTransformPoint(rig.weaponMount.position);
            float barrel=rig.weaponMount.localEulerAngles.z;
            float rear=rig.leftArm.localEulerAngles.z;
            bool crouch=pose==CharacterPose.Crouch||pose==CharacterPose.CrouchWalk;
            bool air=pose==CharacterPose.Jump||pose==CharacterPose.Fall;
            float bend=crouch?95:air?55:2;
            float left=rig.leftLeg.localEulerAngles.z,right=rig.rightLeg.localEulerAngles.z;
            if(crouch)left=right=47.5f;
            Leg(rig.leftLeg,leftKnee,leftFoot,left,bend,dt,0);
            Leg(rig.rightLeg,rightKnee,rightFoot,right,bend,dt,3);
            ArmAngles(rig.leftArm,leftElbow,leftHand,rear,pose==CharacterPose.Climb?45:25,0,dt,6);
            int rearOrder=rig.leftArm.GetComponent<SpriteRenderer>().sortingOrder;
            leftElbow.GetComponent<SpriteRenderer>().sortingOrder=leftHand.GetComponent<SpriteRenderer>().sortingOrder=rearOrder;
            if(!air&&pose!=CharacterPose.Climb){Plant(rig.leftLeg,leftFoot);Plant(rig.rightLeg,rightFoot);}
            SolveGrip(target,barrel,dt);
            // Firearms retain the authoritative shot direction, independent of wrist limits.
            rig.weaponMount.localRotation=Quaternion.Euler(0,0,barrel);
            rig.weapon.flipY=Mathf.Cos(barrel*Mathf.Deg2Rad)<0;
            posed=true;
        }
        public void PosePistolRecoil(float kick,float setback)
        {
            if(!firearm||!IsBound)return;if(rig==null)rig=GetComponent<CutoutRig>();
            float barrel=rig.weaponMount.localEulerAngles.z;
            Vector2 axis=new Vector2(Mathf.Cos(barrel*Mathf.Deg2Rad),Mathf.Sin(barrel*Mathf.Deg2Rad));
            Vector2 target=(Vector2)transform.InverseTransformPoint(rig.weaponMount.position)-axis*setback;
            barrel+=kick*(axis.x>=0?1:-1);SolveGrip(target,barrel,0);
            rig.weaponMount.localRotation=Quaternion.Euler(0,0,barrel);
            rig.weapon.flipY=Mathf.Cos(barrel*Mathf.Deg2Rad)<0;
        }
        public void PoseWrapping(float phase)
        {
            if(!IsBound)return;if(rig==null)rig=GetComponent<CutoutRig>();Vector2 target=(Vector2)rig.torso.localPosition+new Vector2(.08f,Mathf.Sin(phase)*.04f);SolveGrip(target,-35,0);ArmAngles(rig.leftArm,leftElbow,leftHand,-55+Mathf.Sin(phase)*12,65,-15,0,6);
        }
        // Enemy-only adaptation of the existing pistol exchange to this rig's
        // shorter, articulated hand grip. Player pistol/carbine poses never call it.
        public void PosePistolExchange(float progress)
        {
            if(!firearm||!IsBound||rig.magazine==null)return;
            float blend=Mathf.SmoothStep(0,1,progress/.18f)*(1-Mathf.SmoothStep(0,1,(progress-.88f)/.12f));
            var spare=rig.weapon.transform.Find("PistolReplacementMagazine");if(spare==null)return;
            float swap=Mathf.SmoothStep(0,1,(progress-.42f)/.16f);
            Vector3 target=Vector3.Lerp(rig.magazine.transform.position,spare.position,swap);
            Vector2 socket=rig.weaponMount.InverseTransformPoint(target);
            float barrel=rig.weaponMount.localEulerAngles.z;
            float reach=ArmUpperLength+ArmLowerLength+hand.bounds.size.y*.55f;
            Vector2 offset=(Vector2)(rig.rightArm.localPosition-rig.leftArm.localPosition)+(Vector2)(Quaternion.Euler(0,0,barrel)*(Vector3)socket);
            float carry=Mathf.Atan2(offset.y,offset.x)*Mathf.Rad2Deg-Mathf.Asin(Mathf.Clamp(offset.magnitude/(2*reach),0,1))*Mathf.Rad2Deg;
            rig.rightArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(rig.rightArm.localEulerAngles.z,carry,blend));
            rightElbow.localRotation=Quaternion.Slerp(rightElbow.localRotation,Quaternion.identity,blend);rightHand.localRotation=Quaternion.Slerp(rightHand.localRotation,Quaternion.identity,blend);
            leftElbow.localRotation=Quaternion.Slerp(leftElbow.localRotation,Quaternion.identity,blend);leftHand.localRotation=Quaternion.Slerp(leftHand.localRotation,Quaternion.identity,blend);
            rig.weaponMount.position=grip.position;rig.weaponMount.localRotation=Quaternion.Euler(0,0,barrel);
            target=Vector3.Lerp(rig.magazine.transform.position,spare.position,swap);
            Vector3 delta=transform.InverseTransformPoint(target)-rig.leftArm.localPosition;
            rig.leftArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(rig.leftArm.localEulerAngles.z,Mathf.Atan2(delta.x,-delta.y)*Mathf.Rad2Deg,blend));
            rig.EnsureMuzzle();
        }
        private void Plant(Transform hip,Transform ankle)
        {float toe=transform.InverseTransformPoint(ankle.TransformPoint(Vector3.down*foot.bounds.size.y)).y;hip.localPosition+=Vector3.up*(-.7f-toe);}
        private void SolveGrip(Vector2 target,float blade,float dt)
        {
            // Analytic two-link arm with a bounded wrist, not a general IK framework.
            Vector2 direction=new Vector2(Mathf.Cos(blade*Mathf.Deg2Rad),Mathf.Sin(blade*Mathf.Deg2Rad));
            Vector2 wrist=target-direction*hand.bounds.size.y*.55f;
            Vector2 delta=wrist-(Vector2)rig.rightArm.localPosition;
            float a=ArmUpperLength,b=ArmLowerLength,d=Mathf.Clamp(delta.magnitude,Mathf.Abs(a-b)+.015f,a+b-.01f);
            float bend=Mathf.Clamp(Mathf.Acos(Mathf.Clamp((d*d-a*a-b*b)/(2*a*b),-1,1))*Mathf.Rad2Deg,8,elbowLimit);
            float theta=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg+90-Mathf.Atan2(b*Mathf.Sin(bend*Mathf.Deg2Rad),a+b*Mathf.Cos(bend*Mathf.Deg2Rad))*Mathf.Rad2Deg;
            ArmAngles(rig.rightArm,rightElbow,rightHand,theta,bend,Mathf.DeltaAngle(theta+bend,blade+90),dt,9);
            FollowGrip();
        }
        private void FollowGrip()
        {
            // Grip follows the hand's actual filtered pose, including its wrist angle.
            rig.weaponMount.position=grip.position;
            Vector3 axis=rig.weaponMount.parent.InverseTransformVector(rightHand.TransformVector(Vector3.down));
            rig.weaponMount.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(axis.y,axis.x)*Mathf.Rad2Deg);
            rig.weapon.flipY=false;
        }
        public void FollowImpactGrip(){if(IsBound)FollowGrip();}
        public void RetractMuzzle(Vector2 safe)
        {
            Vector2 shift=safe-rig.MuzzlePosition;
            Vector2 target=transform.InverseTransformPoint((Vector2)grip.position+shift);
            float blade=rig.weaponMount.localEulerAngles.z;SolveGrip(target,blade,0);
            if(firearm)rig.weaponMount.localRotation=Quaternion.Euler(0,0,blade);
        }
        public Rigidbody2D[] BuildRagdoll(CartoonRagdoll ragdoll)
        {
            if(!IsBound)return ragdoll.parts;
            Transform[] source=ExtraParts;int[] parent={2,6,3,8,4,10,5,12};
            var all=new Rigidbody2D[14];System.Array.Copy(ragdoll.parts,all,6);
            for(int i=2;i<6;i++){Fit(ragdoll.parts[i]);ragdoll.parts[i].mass*=.5f;}
            for(int i=0;i<8;i++)
            {
                var from=source[i];var go=new GameObject(from.name);go.layer=ragdoll.parts[2].gameObject.layer;go.transform.SetParent(ragdoll.transform,false);
                var scale=from.lossyScale;go.transform.localScale=new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));
                Vector3 down=from.TransformVector(Vector3.down);go.transform.SetPositionAndRotation(from.position,Quaternion.Euler(0,0,Mathf.Atan2(down.y,down.x)*Mathf.Rad2Deg+90));
                var original=from.GetComponent<SpriteRenderer>();var r=go.AddComponent<SpriteRenderer>();r.sprite=original.sprite;r.color=original.color;r.flipX=transform.lossyScale.x<0;r.sortingOrder=original.sortingOrder;r.sharedMaterial=original.sharedMaterial;
                var body=go.AddComponent<Rigidbody2D>();body.simulated=false;
                int limb=i<2?2:i<4?3:i<6?4:5;body.mass=ragdoll.parts[limb].mass*2*(i%2==0?.32f:.18f);body.gravityScale=ragdoll.parts[limb].gravityScale;body.interpolation=ragdoll.parts[limb].interpolation;
                body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                var collider=go.AddComponent<BoxCollider2D>();var sourceCollider=ragdoll.parts[limb].GetComponent<Collider2D>();
                collider.excludeLayers=sourceCollider.excludeLayers;collider.includeLayers=sourceCollider.includeLayers;collider.layerOverridePriority=sourceCollider.layerOverridePriority;
                Fit(body);all[6+i]=body;
                var joint=go.AddComponent<HingeJoint2D>();joint.connectedBody=all[parent[i]];joint.autoConfigureConnectedAnchor=true;joint.anchor=Vector2.zero;joint.useLimits=true;joint.enableCollision=false;
                float relative=Mathf.DeltaAngle(joint.connectedBody.rotation,body.rotation);
                bool leg=i>=4;float min=leg?(i%2==0?-120:-40):i%2==0?8:-55,max=leg?(i%2==0?-2:45):i%2==0?135:55;
                if(transform.lossyScale.x<0){float old=min;min=-max;max=-old;}
                joint.limits=new JointAngleLimits2D{min=min-relative,max=max-relative};
            }
            return all;
        }
        private static void Fit(Rigidbody2D body)
        {var sprite=body.GetComponent<SpriteRenderer>().sprite;var box=body.GetComponent<BoxCollider2D>();box.size=sprite.bounds.size;box.offset=sprite.bounds.center;}
    }
}
