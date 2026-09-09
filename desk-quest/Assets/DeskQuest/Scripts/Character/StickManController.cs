using System;
using System.Collections.Generic;
using DeskQuest.Gameplay;
using DeskQuest.InputHandling;
using UnityEngine;

namespace DeskQuest.Character
{
    /// <summary>
    /// Puts the character in the scene: builds the body, runs the motor each frame, and
    /// pushes the result onto the transform and the animator.
    ///
    /// It lives under the play area, so its local space is the play area's space and the
    /// motor's coordinates can be written straight onto the transform. That is what keeps
    /// the character stuck to the real desk: the anchor moves the play area, the play area
    /// moves the character, and nothing in the gameplay code has to know that the world
    /// underneath it is being corrected by the tracker.
    /// </summary>
    public class StickManController : MonoBehaviour
    {
        [SerializeField] MotorSettings m_Settings = new MotorSettings();

        [Tooltip("Character height in metres. About a tenth of a desk's depth reads well.")]
        [SerializeField] float m_Height = 0.09f;

        [SerializeField] Color m_BodyColor = new Color(0.12f, 0.14f, 0.18f, 1f);
        [SerializeField] Color m_HeadColor = new Color(1.00f, 0.82f, 0.35f, 1f);

        CharacterMotor m_Motor;
        StickManAnimator m_Animator;
        IMotorInputSource m_Input;
        IObstacleProvider m_Obstacles;
        bool m_AcceptInput = true;

        static readonly List<ObstacleBox> s_NoObstacles = new List<ObstacleBox>();

        /// <summary>Raised the frame a jump starts.</summary>
        public event Action Jumped;

        /// <summary>Raised the frame the character lands after being airborne.</summary>
        public event Action Landed;

        /// <summary>Raised when climb was pressed with nothing in reach.</summary>
        public event Action ClimbRejected;

        public CharacterMotor Motor { get { return m_Motor; } }
        public MotorSettings Settings { get { return m_Settings; } }

        /// <summary>Height of the character in metres.</summary>
        public float Height { get { return m_Height; } }

        /// <summary>
        /// Builds the body and starts the motor. Call once, after the play area exists.
        /// </summary>
        public void Initialise(Vector2 playAreaSize, IMotorInputSource input, IObstacleProvider obstacles)
        {
            m_Input = input;
            m_Obstacles = obstacles;

            // Keep the character's proportions sane on a small desk: never taller than a
            // fifth of the shorter side, or it cannot fit between obstacles.
            float maxHeight = Mathf.Min(playAreaSize.x, playAreaSize.y) * 0.25f;
            m_Height = Mathf.Min(m_Height, maxHeight);
            ScaleSettingsTo(m_Height);

            StickManRig rig = StickManFactory.Create(transform, m_Height, m_BodyColor, m_HeadColor);

            m_Animator = gameObject.AddComponent<StickManAnimator>();
            m_Animator.SetRig(rig);

            m_Motor = new CharacterMotor(m_Settings, playAreaSize);
        }

        /// <summary>
        /// Rescales the movement figures if the character had to be shrunk to fit a small
        /// desk, so a shorter character does not end up sprinting or leaping proportionally
        /// further than a taller one.
        /// </summary>
        void ScaleSettingsTo(float height)
        {
            const float referenceHeight = 0.09f;
            float scale = height / referenceHeight;
            if (Mathf.Approximately(scale, 1f))
                return;

            m_Settings.Radius *= scale;
            m_Settings.StandHeight *= scale;
            m_Settings.CrouchHeight *= scale;
            m_Settings.WalkSpeed *= scale;
            m_Settings.RunSpeed *= scale;
            m_Settings.CrouchSpeed *= scale;
            m_Settings.JumpSpeed *= scale;
            m_Settings.TerminalSpeed *= scale;
            m_Settings.Gravity *= scale;
            m_Settings.StepHeight *= scale;
            m_Settings.MaxClimbHeight *= scale;
            m_Settings.ClimbReach *= scale;
            m_Settings.MaxVaultDepth *= scale;
        }

        /// <summary>Places the character at a starting point on the desk, facing the goal.</summary>
        public void PlaceAt(Vector3 localPosition, float facingDegrees)
        {
            if (m_Motor == null)
                return;

            m_Motor.Teleport(localPosition, facingDegrees);
            PushToTransform();
        }

        public void SetPlayAreaSize(Vector2 size)
        {
            if (m_Motor != null)
                m_Motor.SetPlayAreaSize(size);
        }

        /// <summary>Stops the character responding to the controls, for cutscenes and menus.</summary>
        public void SetInputEnabled(bool enabled)
        {
            m_AcceptInput = enabled;
        }

        /// <summary>Switches to the celebration and ignores input from then on.</summary>
        public void Celebrate()
        {
            m_AcceptInput = false;
            if (m_Motor != null)
                m_Motor.Celebrate();
        }

        void Update()
        {
            if (m_Motor == null)
                return;

            MotorInput input = new MotorInput();
            if (m_AcceptInput && m_Input != null)
                input = m_Input.Read();

            // Captured before stepping, because the motor consumes the request.
            bool jumping = input.JumpPressed && m_Motor.IsGrounded && !m_Motor.IsCrouching;

            m_Motor.Step(input, CurrentObstacles(), Time.deltaTime);
            PushToTransform();

            if (m_Animator != null)
                m_Animator.Play(m_Motor.State, SpeedFraction(input));

            if (jumping && Jumped != null)
                Jumped();

            if (m_Motor.JustLanded && Landed != null)
                Landed();

            if (m_Motor.ClimbRejected && ClimbRejected != null)
                ClimbRejected();
        }

        List<ObstacleBox> CurrentObstacles()
        {
            return m_Obstacles != null ? m_Obstacles.Obstacles : s_NoObstacles;
        }

        void PushToTransform()
        {
            transform.localPosition = m_Motor.Position;
            transform.localRotation = Quaternion.Euler(0f, m_Motor.FacingDegrees, 0f);
        }

        /// <summary>Ground speed as a fraction of running speed, which drives the gait rate.</summary>
        float SpeedFraction(MotorInput input)
        {
            if (!input.HasMovement)
                return 0f;

            return m_Settings.SpeedFor(m_Motor.IsCrouching, input.Run) / m_Settings.RunSpeed;
        }
    }
}
