using InsaneDreams.Inventory;
using UnityEngine;
namespace InsaneDreams.Visuals
{
    // Player-pistol presentation only. Carbine poses and ammunition clocks are untouched.
    public sealed class PistolMagazineReload : MonoBehaviour
    {
        [SerializeField] private WeaponDefinition[] pistols;
        [SerializeField,Range(1,6)] private int debrisCapacity=3;
        [SerializeField,Min(.5f)] private float debrisLifetime=4;
        [SerializeField] private LayerMask geometry=1;
        private Rigidbody2D owner;
        private Rigidbody2D[] bodies;
        private SpriteRenderer[] sprites;
        private BoxCollider2D[] boxes;
        private float[] expires;
        private int next;
        private SpriteRenderer replacement;
        private bool wasReloading,released;
        private float reloadToken;
        private WeaponDefinition previous;
        public int Releases{get;private set;}
        public Rigidbody2D LastReleased{get;private set;}
        public int ActiveDebris{get{int n=0;if(bodies!=null)foreach(var b in bodies)if(b!=null&&b.simulated)n++;return n;}}
        private void Awake()
        {
            owner=GetComponent<Rigidbody2D>();int count=Mathf.Clamp(debrisCapacity,1,6);
            bodies=new Rigidbody2D[count];sprites=new SpriteRenderer[count];boxes=new BoxCollider2D[count];expires=new float[count];
            for(int i=0;i<count;i++)
            {
                // Cosmetic layer is outside the project's actor/geometry/shot masks.
                var g=new GameObject("ReleasedPistolMagazine");g.layer=31;UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(g,gameObject.scene);
                sprites[i]=g.AddComponent<SpriteRenderer>();sprites[i].enabled=false;
                bodies[i]=g.AddComponent<Rigidbody2D>();bodies[i].mass=.03f;bodies[i].gravityScale=1;bodies[i].interpolation=RigidbodyInterpolation2D.Interpolate;bodies[i].collisionDetectionMode=CollisionDetectionMode2D.Continuous;
                boxes[i]=g.AddComponent<BoxCollider2D>();boxes[i].includeLayers=geometry;boxes[i].excludeLayers=~geometry.value;boxes[i].layerOverridePriority=100;
                bodies[i].simulated=false;
            }
        }
        private void Update(){for(int i=0;i<bodies.Length;i++)if(bodies[i].simulated&&Time.time>=expires[i]){bodies[i].simulated=false;sprites[i].enabled=false;}}
        private void OnDisable(){wasReloading=false;if(replacement!=null)replacement.enabled=false;if(bodies!=null)for(int i=0;i<bodies.Length;i++){if(bodies[i]!=null)bodies[i].simulated=false;if(sprites[i]!=null)sprites[i].enabled=false;}}
        private void OnDestroy(){if(bodies!=null)foreach(var b in bodies)if(b!=null)Destroy(b.gameObject);}
        public bool Handles(WeaponDefinition d)=>d!=null&&pistols!=null&&System.Array.IndexOf(pistols,d)>=0;
        public void Pose(CutoutRig rig,RuntimeWeapon weapon,bool reloading,float progress)
        {
            var d=weapon!=null?weapon.Definition:null;
            if(!Handles(d)){if(replacement!=null)replacement.enabled=false;wasReloading=false;previous=null;return;}
            if(rig.magazine==null||d.MagazineSprite==null){if(replacement!=null)replacement.enabled=false;wasReloading=false;return;}
            if(replacement==null){var g=new GameObject("PistolReplacementMagazine");g.transform.SetParent(rig.weapon.transform,false);replacement=g.AddComponent<SpriteRenderer>();}
            replacement.sprite=d.MagazineSprite;replacement.sortingLayerID=rig.magazine.sortingLayerID;replacement.sortingOrder=rig.magazine.sortingOrder;
            if(d!=previous||reloading&&(!wasReloading||reloadToken!=weapon.Cycle.ReloadStartedAt)){released=false;reloadToken=weapon.Cycle.ReloadStartedAt;}previous=d;wasReloading=reloading;
            float p=Mathf.Clamp01(progress),blend=reloading?Mathf.SmoothStep(0,1,p/.18f)*(1-Mathf.SmoothStep(0,1,(p-.88f)/.12f)):0;
            Vector2 socket=d.MagazineMountOffset;
            Vector2 pulled=socket+Vector2.down*.2f*Mathf.SmoothStep(0,1,(p-.2f)/.2f);
            Vector2 fresh=Vector2.Lerp(socket+new Vector2(.08f,-.2f),socket,Mathf.SmoothStep(0,1,(p-.58f)/.3f));
            if(blend>0)
            {
                // Solve the two existing rigid arm lengths around the magazine grip.
                // Both shoulders stay attached; no limb stretching or floating hand.
                Vector2 grip=Vector2.Lerp(pulled,fresh,Mathf.SmoothStep(0,1,(p-.42f)/.16f));
                Vector2 offset=(Vector2)(rig.rightArm.localPosition-rig.leftArm.localPosition)+(Vector2)(Quaternion.Euler(0,0,-12)*(Vector3)grip);
                float length=rig.skin.arm.bounds.size.y,radius=offset.magnitude;
                float carry=Mathf.Atan2(offset.y,offset.x)*Mathf.Rad2Deg-Mathf.Asin(Mathf.Clamp(radius/(2*length),0,1))*Mathf.Rad2Deg;
                float angle=Mathf.LerpAngle(rig.rightArm.localEulerAngles.z,carry,blend);rig.rightArm.localRotation=Quaternion.Euler(0,0,angle);
                rig.weaponMount.localPosition=rig.rightArm.localPosition+rig.rightArm.localRotation*(Vector3.down*rig.skin.arm.bounds.size.y);
                rig.weaponMount.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(rig.weaponMount.localEulerAngles.z,-12,blend));rig.weapon.flipY=false;
            }
            float flip=rig.weapon.flipY?-1:1;
            rig.magazine.transform.localPosition=new Vector3(pulled.x,flip*pulled.y,-.01f);rig.magazine.transform.localRotation=Quaternion.identity;rig.magazine.transform.localScale=new Vector3(1,flip,1);
            if(!reloading)rig.magazine.transform.localPosition=new Vector3(socket.x,flip*socket.y,-.01f);
            rig.magazine.enabled=rig.weapon.enabled&&(!reloading||p<.42f);
            if(reloading&&p>=.42f&&!released){Release(rig.magazine);released=true;}
            replacement.transform.localPosition=new Vector3(fresh.x,flip*fresh.y,-.01f);replacement.transform.localRotation=Quaternion.identity;replacement.transform.localScale=new Vector3(1,flip,1);replacement.enabled=rig.weapon.enabled&&reloading&&p>=.58f;
            if(reloading&&blend>0)
            {
                Vector3 target=Vector3.Lerp(rig.magazine.transform.position,replacement.transform.position,Mathf.SmoothStep(0,1,(p-.42f)/.16f));
                Vector3 delta=rig.transform.InverseTransformPoint(target)-rig.leftArm.localPosition;
                rig.leftArm.localRotation=Quaternion.Euler(0,0,Mathf.LerpAngle(rig.leftArm.localEulerAngles.z,Mathf.Atan2(delta.x,-delta.y)*Mathf.Rad2Deg,blend));
            }
            rig.EnsureMuzzle();
        }
        private void Release(SpriteRenderer source)
        {
            int i=next;next=(next+1)%bodies.Length;var b=bodies[i];b.simulated=false;
            sprites[i].sprite=source.sprite;sprites[i].sortingLayerID=source.sortingLayerID;sprites[i].sortingOrder=source.sortingOrder;sprites[i].enabled=true;
            b.transform.localScale=new Vector3(Mathf.Sign(source.transform.lossyScale.x),Mathf.Sign(source.transform.lossyScale.y),1);
            // A disabled pooled body takes its pose from its Transform when simulation resumes.
            b.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);
            boxes[i].size=source.sprite.bounds.size;boxes[i].offset=source.sprite.bounds.center;
            b.simulated=true;b.linearVelocity=(owner!=null?owner.linearVelocity:Vector2.zero)+new Vector2(Mathf.Sign(source.transform.lossyScale.x)*.25f,-.4f);b.angularVelocity=120*Mathf.Sign(source.transform.lossyScale.x);
            expires[i]=Time.time+debrisLifetime;LastReleased=b;Releases++;
        }
    }
}
