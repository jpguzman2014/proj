using DeskQuest.Gameplay;
using UnityEngine;

namespace DeskQuest.Character
{
    /// <summary>
    /// Animates the stick man by rotating its joints from code, with no animation clips,
    /// animator controller or imported model.
    ///
    /// The brief asked for procedural animation if full animation was too complex, and at
    /// this scale procedural is not the compromise. Eight named clips would need a rigged
    /// model, an animator controller and a state machine to maintain, and the character is
    /// nine centimetres tall on a phone screen. A sine wave through the hips reads exactly
    /// the same and can be retuned by changing a number.
    ///
    /// Poses are targets that the joints ease towards, so switching state is a blend
    /// rather than a snap.
    /// </summary>
    public class StickManAnimator : MonoBehaviour
    {
        /// <summary>Joint angles for one frame. Positive swings a limb forwards, towards +Z.</summary>
        struct Pose
        {
            public float PelvisScale;
            public float Bob;
            public float SpineLean;
            public float HeadNod;
            public float HipLeft;
            public float HipRight;
            public float KneeLeft;
            public float KneeRight;
            public float ShoulderLeft;
            public float ShoulderRight;
            public float ElbowLeft;
            public float ElbowRight;
        }

        [SerializeField] StickManRig m_Rig;

        [Tooltip("How quickly joints ease towards the target pose. Higher is snappier.")]
        [SerializeField] float m_Smoothing = 14f;

        [Tooltip("Pelvis height while crouching, as a fraction of standing height.")]
        [SerializeField] float m_CrouchRatio = 0.62f;

        LocomotionState m_State = LocomotionState.Idle;
        float m_Speed01 = 0f;
        float m_Phase;
        float m_Elapsed;
        Pose m_Current;
        bool m_Initialised;

        public void SetRig(StickManRig rig)
        {
            m_Rig = rig;
            m_Initialised = false;
        }

        /// <summary>
        /// Tells the animator what the character is doing. <paramref name="speed01"/> is
        /// ground speed as a fraction of running speed, which drives how fast the gait
        /// cycles so slow walking does not look like it is running on the spot.
        /// </summary>
        public void Play(LocomotionState state, float speed01)
        {
            m_State = state;
            m_Speed01 = Mathf.Clamp01(speed01);
        }

        void LateUpdate()
        {
            if (m_Rig == null || !m_Rig.IsComplete)
                return;

            float dt = Time.deltaTime;
            m_Elapsed += dt;
            m_Phase = Mathf.Repeat(m_Phase + CycleSpeed() * dt, 1f);

            Pose target = BuildPose();

            if (!m_Initialised)
            {
                m_Current = target;
                m_Initialised = true;
            }
            else
            {
                float t = Mathf.Clamp01(dt * m_Smoothing);
                m_Current = Blend(m_Current, target, t);
            }

            Apply(m_Current);
        }

        float CycleSpeed()
        {
            switch (m_State)
            {
                case LocomotionState.Walk:
                    return Mathf.Lerp(1.4f, 2.4f, m_Speed01);
                case LocomotionState.Run:
                    return Mathf.Lerp(2.6f, 3.8f, m_Speed01);
                case LocomotionState.CrouchWalk:
                    return 1.5f;
                case LocomotionState.Celebrate:
                    return 2.0f;
                default:
                    return 0.5f;
            }
        }

        Pose BuildPose()
        {
            switch (m_State)
            {
                case LocomotionState.Walk:
                    return Gait(swing: 26f, kneeBend: 42f, armSwing: 20f, elbow: 12f, lean: 5f, bob: 0.012f);
                case LocomotionState.Run:
                    return Gait(swing: 44f, kneeBend: 62f, armSwing: 38f, elbow: 55f, lean: 18f, bob: 0.022f);
                case LocomotionState.CrouchWalk:
                    return Crouched(Gait(swing: 14f, kneeBend: 20f, armSwing: 10f, elbow: 30f, lean: 22f, bob: 0.004f));
                case LocomotionState.Crouch:
                    return Crouched(Still());
                case LocomotionState.Jump:
                    return Jumping();
                case LocomotionState.Fall:
                    return Falling();
                case LocomotionState.Climb:
                    return Climbing();
                case LocomotionState.Celebrate:
                    return Celebrating();
                default:
                    return Idling();
            }
        }

        Pose Still()
        {
            Pose pose = new Pose();
            pose.PelvisScale = 1f;
            return pose;
        }

        Pose Idling()
        {
            Pose pose = Still();
            // A small breathing rise, plus arms resting slightly away from the body so the
            // silhouette does not read as a single block.
            pose.Bob = Mathf.Sin(m_Elapsed * 1.6f) * 0.006f;
            pose.SpineLean = 2f;
            pose.ShoulderLeft = -6f;
            pose.ShoulderRight = -6f;
            pose.ElbowLeft = -8f;
            pose.ElbowRight = -8f;
            pose.HeadNod = Mathf.Sin(m_Elapsed * 1.1f) * 3f;
            return pose;
        }

        Pose Gait(float swing, float kneeBend, float armSwing, float elbow, float lean, float bob)
        {
            float cycle = m_Phase * Mathf.PI * 2f;
            float left = Mathf.Sin(cycle);
            float right = Mathf.Sin(cycle + Mathf.PI);

            Pose pose = Still();
            pose.SpineLean = lean;
            pose.HipLeft = left * swing;
            pose.HipRight = right * swing;

            // Knees only fold backwards, and fold most while the leg is behind the body
            // and lifting for the next step.
            pose.KneeLeft = -Mathf.Max(0f, -left) * kneeBend;
            pose.KneeRight = -Mathf.Max(0f, -right) * kneeBend;

            // Arms counter-swing against the legs.
            pose.ShoulderLeft = -left * armSwing;
            pose.ShoulderRight = -right * armSwing;
            pose.ElbowLeft = -elbow;
            pose.ElbowRight = -elbow;

            // Twice per stride: the body rises on each supporting leg.
            pose.Bob = Mathf.Abs(Mathf.Sin(cycle)) * bob;
            pose.HeadNod = -lean * 0.5f;
            return pose;
        }

        Pose Crouched(Pose pose)
        {
            pose.PelvisScale = m_CrouchRatio;
            pose.SpineLean = Mathf.Max(pose.SpineLean, 24f);
            pose.HipLeft += 42f;
            pose.HipRight += 42f;
            pose.KneeLeft -= 78f;
            pose.KneeRight -= 78f;
            pose.ShoulderLeft += 25f;
            pose.ShoulderRight += 25f;
            pose.HeadNod = -18f;
            return pose;
        }

        Pose Jumping()
        {
            Pose pose = Still();
            pose.SpineLean = -6f;
            pose.HipLeft = 28f;
            pose.HipRight = 20f;
            pose.KneeLeft = -55f;
            pose.KneeRight = -40f;
            pose.ShoulderLeft = 145f;
            pose.ShoulderRight = 145f;
            pose.ElbowLeft = -20f;
            pose.ElbowRight = -20f;
            return pose;
        }

        Pose Falling()
        {
            Pose pose = Still();
            pose.SpineLean = 10f;
            pose.HipLeft = -12f;
            pose.HipRight = 16f;
            pose.KneeLeft = -18f;
            pose.KneeRight = -28f;
            pose.ShoulderLeft = 100f;
            pose.ShoulderRight = 110f;
            pose.ElbowLeft = -35f;
            pose.ElbowRight = -30f;
            return pose;
        }

        Pose Climbing()
        {
            // Reaching overhead with the legs tucked, which reads as pulling up over an edge.
            Pose pose = Still();
            pose.PelvisScale = 0.9f;
            pose.SpineLean = 16f;
            pose.HipLeft = 55f;
            pose.HipRight = 35f;
            pose.KneeLeft = -80f;
            pose.KneeRight = -60f;

            float reach = Mathf.Sin(m_Elapsed * 6f) * 12f;
            pose.ShoulderLeft = 160f + reach;
            pose.ShoulderRight = 160f - reach;
            pose.ElbowLeft = -30f;
            pose.ElbowRight = -30f;
            return pose;
        }

        Pose Celebrating()
        {
            float cycle = m_Phase * Mathf.PI * 2f;
            Pose pose = Still();
            pose.Bob = Mathf.Abs(Mathf.Sin(cycle)) * 0.03f;
            pose.SpineLean = -4f;
            pose.ShoulderLeft = 165f + Mathf.Sin(cycle) * 15f;
            pose.ShoulderRight = 165f - Mathf.Sin(cycle) * 15f;
            pose.ElbowLeft = -10f;
            pose.ElbowRight = -10f;
            pose.HipLeft = Mathf.Sin(cycle) * 10f;
            pose.HipRight = -Mathf.Sin(cycle) * 10f;
            pose.HeadNod = 8f;
            return pose;
        }

        static Pose Blend(Pose from, Pose to, float t)
        {
            Pose result = new Pose();
            result.PelvisScale = Mathf.Lerp(from.PelvisScale, to.PelvisScale, t);
            result.Bob = Mathf.Lerp(from.Bob, to.Bob, t);
            result.SpineLean = Mathf.Lerp(from.SpineLean, to.SpineLean, t);
            result.HeadNod = Mathf.Lerp(from.HeadNod, to.HeadNod, t);
            result.HipLeft = Mathf.Lerp(from.HipLeft, to.HipLeft, t);
            result.HipRight = Mathf.Lerp(from.HipRight, to.HipRight, t);
            result.KneeLeft = Mathf.Lerp(from.KneeLeft, to.KneeLeft, t);
            result.KneeRight = Mathf.Lerp(from.KneeRight, to.KneeRight, t);
            result.ShoulderLeft = Mathf.Lerp(from.ShoulderLeft, to.ShoulderLeft, t);
            result.ShoulderRight = Mathf.Lerp(from.ShoulderRight, to.ShoulderRight, t);
            result.ElbowLeft = Mathf.Lerp(from.ElbowLeft, to.ElbowLeft, t);
            result.ElbowRight = Mathf.Lerp(from.ElbowRight, to.ElbowRight, t);
            return result;
        }

        void Apply(Pose pose)
        {
            m_Rig.Pelvis.localPosition = new Vector3(
                0f,
                m_Rig.RestPelvisHeight * pose.PelvisScale + pose.Bob * m_Rig.Height,
                0f);

            SwingUp(m_Rig.Spine, pose.SpineLean);
            SwingUp(m_Rig.Head, pose.HeadNod);

            SwingDown(m_Rig.HipLeft, pose.HipLeft);
            SwingDown(m_Rig.HipRight, pose.HipRight);
            SwingDown(m_Rig.KneeLeft, pose.KneeLeft);
            SwingDown(m_Rig.KneeRight, pose.KneeRight);

            SwingDown(m_Rig.ShoulderLeft, pose.ShoulderLeft);
            SwingDown(m_Rig.ShoulderRight, pose.ShoulderRight);
            SwingDown(m_Rig.ElbowLeft, pose.ElbowLeft);
            SwingDown(m_Rig.ElbowRight, pose.ElbowRight);
        }

        /// <summary>
        /// Rotates a joint whose limb hangs below it. A positive angle swings the far end
        /// forwards. Rotating about X moves a downward limb's tip towards -Z, hence the
        /// negation.
        /// </summary>
        static void SwingDown(Transform joint, float forwardDegrees)
        {
            joint.localRotation = Quaternion.Euler(-forwardDegrees, 0f, 0f);
        }

        /// <summary>Rotates a joint whose limb rises above it, such as the spine.</summary>
        static void SwingUp(Transform joint, float forwardDegrees)
        {
            joint.localRotation = Quaternion.Euler(forwardDegrees, 0f, 0f);
        }
    }
}
