using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
namespace InsaneDreams.Enemies
{
    public sealed class LarperSpeechBubble : MonoBehaviour
    {
        [SerializeField, Min(.5f)] private float visibleSeconds=3.2f;
        [SerializeField] private Vector3 worldOffset=new Vector3(0,1.3f,0);
        private Camera view;
        private UIDocument document;
        private Label label;
        private float until;
        private Transform headAnchor;
        private static readonly List<LarperSpeechBubble> visible = new List<LarperSpeechBubble>();
        public Rect Placement { get; private set; }
        public static int VisibleCount => visible.Count;
        public Transform HeadAnchor => headAnchor;
        public bool IsVisible => label != null && label.style.display.value != DisplayStyle.None && Time.time < until;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetRegistry()=>visible.Clear();
        public string LastLine { get; private set; }
        public int LinesShown { get; private set; }
        public void Bind(Camera camera,UIDocument ui) { if(label!=null)label.RemoveFromHierarchy();label=null;view=camera;document=ui; }
        private void Start(){var rig=GetComponentInChildren<InsaneDreams.Visuals.CutoutRig>();if(rig!=null)headAnchor=rig.speechAnchor;}
        public bool IsRelevant(Vector3 position)
        {
            if(view==null)view=Camera.main;
            if(document==null)document=Object.FindFirstObjectByType<UIDocument>();
            if(view==null||document==null||document.rootVisualElement?.panel==null)return false;
            Vector3 point=view.WorldToViewportPoint(position);return point.z>0&&point.x>0&&point.x<1&&point.y>0&&point.y<1;
        }
        public void Show(string text,bool panic=false)=>TryShow(text,panic);
        public bool TryShow(string text,bool panic=false)
        {
            for(int i=visible.Count-1;i>=0;i--)if(visible[i]==null||!visible[i].IsVisible){var old=visible[i];if(old!=null)old.Hide();else visible.RemoveAt(i);}
            if(IsVisible||visible.Count>=2||!IsRelevant(transform.position)||string.IsNullOrWhiteSpace(text))return false;
            if(label==null)
            {
                label=new Label { name="larperSpeech",pickingMode=PickingMode.Ignore };
                label.style.position=Position.Absolute;label.style.width=220;label.style.whiteSpace=WhiteSpace.Normal;
                label.style.fontSize=18;label.style.unityFontStyleAndWeight=FontStyle.Bold;label.style.unityTextAlign=TextAnchor.MiddleCenter;
                label.style.color=Color.white;label.style.backgroundColor=new Color(.04f,.07f,.12f,.95f);
                label.style.paddingLeft=6;label.style.paddingRight=6;label.style.paddingTop=5;label.style.paddingBottom=5;
                label.style.borderTopLeftRadius=8;label.style.borderTopRightRadius=8;label.style.borderBottomLeftRadius=8;label.style.borderBottomRightRadius=8;
                document.rootVisualElement.Add(label);
            }
            if(!visible.Contains(this))visible.Add(this);
            label.style.backgroundColor=panic?new Color(.35f,.08f,.05f,.97f):new Color(.04f,.07f,.12f,.95f);
            LastLine=text;LinesShown++;label.text=text;until=Time.time+Mathf.Clamp(visibleSeconds+Mathf.Max(0,text.Length-20)*.025f,3,4);label.style.display=DisplayStyle.Flex;return true;
        }
        public static Rect AvoidOverlap(Rect desired,Rect screen,IReadOnlyList<Rect> occupied)
        {
            desired.width=Mathf.Min(desired.width,screen.width);desired.height=Mathf.Min(desired.height,screen.height);
            Rect best=desired;float bestOverlap=float.PositiveInfinity;
            for(int attempt=0;attempt<9;attempt++)
            {
                var candidate=desired;int row=(attempt+1)/2;
                candidate.y+=(attempt%2==1?-1:1)*row*(desired.height+6);
                candidate.x=Mathf.Clamp(candidate.x,screen.xMin,screen.xMax-candidate.width);
                candidate.y=Mathf.Clamp(candidate.y,screen.yMin,screen.yMax-candidate.height);
                float overlap=0;
                foreach(var other in occupied)if(candidate.Overlaps(other))overlap+=(Mathf.Min(candidate.xMax,other.xMax)-Mathf.Max(candidate.xMin,other.xMin))*(Mathf.Min(candidate.yMax,other.yMax)-Mathf.Max(candidate.yMin,other.yMin));
                if(overlap<bestOverlap){best=candidate;bestOverlap=overlap;}if(overlap==0)break;
            }
            return best;
        }
        private readonly List<Rect> occupied=new List<Rect>();
        private void LateUpdate()
        {
            if(label==null)return;
            if(Time.time>=until||!IsRelevant(transform.position)){Hide();return;}
            Vector3 point=view.WorldToViewportPoint(headAnchor!=null?headAnchor.position:transform.position+worldOffset);var root=document.rootVisualElement;
            float width=root.resolvedStyle.width,height=root.resolvedStyle.height;
            if(float.IsNaN(width)||width<=0||float.IsNaN(height)||height<=0)return;
            float bubbleHeight=label.resolvedStyle.height;if(float.IsNaN(bubbleHeight)||bubbleHeight<1)bubbleHeight=60;
            occupied.Clear();int index=visible.IndexOf(this);
            for(int i=0;i<index;i++)if(visible[i]!=null&&visible[i].document==document&&visible[i].IsVisible)occupied.Add(visible[i].Placement);
            Placement=AvoidOverlap(new Rect(point.x*width-110,(1-point.y)*height-bubbleHeight-8,220,bubbleHeight),new Rect(0,0,width,height),occupied);
            label.style.width=Placement.width;label.style.left=Placement.x;label.style.top=Placement.y;
        }
        private void Hide(){if(label!=null)label.style.display=DisplayStyle.None;visible.Remove(this);until=0;}
        private void OnDisable()=>Hide();
        private void OnDestroy(){label?.RemoveFromHierarchy();}
    }
}
