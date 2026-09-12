using UnityEngine;

namespace DeskQuest.Character
{
    /// <summary>
    /// The joints of a stick man, in the order the animator poses them.
    ///
    /// Every limb is a pivot at the joint with its mesh hanging below it, so animating is
    /// a matter of rotating pivots rather than moving meshes. That is what a skeleton
    /// would do, without needing a rigged model, a skinned mesh renderer or an animation
    /// controller asset.
    /// </summary>
    public class StickManRig : MonoBehaviour
    {
        public Transform Pelvis;
        public Transform Spine;
        public Transform Head;

        public Transform ShoulderLeft;
        public Transform ShoulderRight;
        public Transform ElbowLeft;
        public Transform ElbowRight;

        public Transform HipLeft;
        public Transform HipRight;
        public Transform KneeLeft;
        public Transform KneeRight;

        /// <summary>Height of the pelvis above the feet when standing, in metres.</summary>
        public float RestPelvisHeight;

        /// <summary>Full height of the character in metres, used to scale the animation.</summary>
        public float Height;

        /// <summary>True when every joint the animator needs is present.</summary>
        public bool IsComplete
        {
            get
            {
                return Pelvis != null && Spine != null && Head != null
                    && ShoulderLeft != null && ShoulderRight != null
                    && ElbowLeft != null && ElbowRight != null
                    && HipLeft != null && HipRight != null
                    && KneeLeft != null && KneeRight != null;
            }
        }
    }
}
