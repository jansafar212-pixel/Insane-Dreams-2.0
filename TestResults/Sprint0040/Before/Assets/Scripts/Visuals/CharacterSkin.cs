using UnityEngine;

namespace InsaneDreams.Visuals
{
    public enum CharacterPose { Idle, Walk, Jump, Fall, Crouch, CrouchWalk, Climb, Parachute, Panic, Defeated }
    public enum FaceExpression { Neutral, Smug, Angry, Surprised, Scared, Stunned }

    // Art only. Replacing a skin never changes collision, health, input or weapon definitions.
    [CreateAssetMenu(menuName = "Insane Dreams/Character Skin")]
    public sealed class CharacterSkin : ScriptableObject
    {
        public Sprite head, torso, arm, leg, weapon, parachute;
        public Sprite secondaryWeapon, meleeWeapon;
        public Sprite panicFace;
        public Sprite[] faces = new Sprite[6];
        public Color clothing = Color.white;
        [TextArea] public string provenance;
        public Sprite Face(FaceExpression expression) => faces != null && (int)expression < faces.Length ? faces[(int)expression] : null;
        public bool IsComplete => head != null && torso != null && arm != null && leg != null && weapon != null && parachute != null && faces != null && faces.Length == 6 && System.Array.TrueForAll(faces, s => s != null);
    }
}
