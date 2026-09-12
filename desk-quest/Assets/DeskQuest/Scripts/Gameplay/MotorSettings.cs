using UnityEngine;

namespace DeskQuest.Gameplay
{
    /// <summary>
    /// Movement tuning, in metres and seconds at desk scale.
    ///
    /// The character is about nine centimetres tall, so every figure here is a fraction
    /// of its real-world equivalent. Gravity is the important one: real gravity applied to
    /// a nine-centimetre character makes it drop the equivalent of twenty storeys in a
    /// blink and reads as a glitch. Scaling it by the ratio of character height to human
    /// height, roughly one twentieth, makes a jump look like a jump.
    /// </summary>
    [System.Serializable]
    public class MotorSettings
    {
        [Header("Size")]
        public float Radius = 0.012f;
        public float StandHeight = 0.09f;
        public float CrouchHeight = 0.055f;

        [Header("Speed")]
        public float WalkSpeed = 0.06f;
        public float RunSpeed = 0.11f;
        public float CrouchSpeed = 0.03f;
        public float TurnSpeedDegreesPerSecond = 540f;

        [Header("Jumping and falling")]
        public float Gravity = -0.55f;
        public float JumpSpeed = 0.20f;
        public float TerminalSpeed = -1.2f;

        [Header("Steps and climbing")]
        [Tooltip("Anything up to this height is walked over without needing a jump.")]
        public float StepHeight = 0.012f;

        [Tooltip("Tallest obstacle the climb action can get onto.")]
        public float MaxClimbHeight = 0.05f;

        [Tooltip("How far in front the climb action looks for something to climb.")]
        public float ClimbReach = 0.02f;

        [Tooltip("Seconds a climb or vault takes from start to finish.")]
        public float ClimbDuration = 0.6f;

        [Tooltip("An obstacle no deeper than this is vaulted over and landed on the far side, rather than climbed onto.")]
        public float MaxVaultDepth = 0.035f;

        /// <summary>Height of the character's capsule in the given stance.</summary>
        public float HeightFor(bool crouching)
        {
            return crouching ? CrouchHeight : StandHeight;
        }

        /// <summary>Ground speed for the given stance and effort.</summary>
        public float SpeedFor(bool crouching, bool running)
        {
            if (crouching)
                return CrouchSpeed;

            return running ? RunSpeed : WalkSpeed;
        }
    }
}
