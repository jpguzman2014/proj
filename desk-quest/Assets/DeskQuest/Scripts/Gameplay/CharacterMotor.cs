using System.Collections.Generic;
using UnityEngine;

namespace DeskQuest.Gameplay
{
    /// <summary>What the character is doing, which is all the animator needs to know.</summary>
    public enum LocomotionState
    {
        Idle,
        Walk,
        Run,
        Crouch,
        CrouchWalk,
        Jump,
        Fall,
        Climb,
        Celebrate
    }

    /// <summary>One frame of player intent, independent of where it came from.</summary>
    public struct MotorInput
    {
        /// <summary>Left and right across the play area, from -1 to 1.</summary>
        public float MoveX;

        /// <summary>Towards and away from the player, from -1 to 1.</summary>
        public float MoveZ;

        public bool Run;
        public bool Crouch;
        public bool JumpPressed;
        public bool ClimbPressed;

        public bool HasMovement
        {
            get { return Mathf.Abs(MoveX) > 0.01f || Mathf.Abs(MoveZ) > 0.01f; }
        }
    }

    /// <summary>
    /// Moves the character around the play area. Deliberately not built on Unity's
    /// physics or CharacterController.
    ///
    /// The character is about nine centimetres tall. At that scale Unity's solver is
    /// working near its default tolerances, where skin width, minimum move distance and
    /// contact offset are all comparable to the character itself, and the usual results
    /// are jitter, sinking through surfaces and tunnelling through thin obstacles. A
    /// kinematic solver over axis-aligned boxes has none of those problems, is
    /// predictable, and costs almost nothing on a phone already busy tracking the room.
    ///
    /// It also has no Unity dependency beyond vector maths, which means the movement
    /// rules are unit-tested rather than only eyeballed on a device.
    ///
    /// Everything is in play-area local space, in metres, with Y measured up from the
    /// desk surface and Z running away from the player.
    /// </summary>
    public class CharacterMotor
    {
        readonly MotorSettings m_Settings;

        Vector2 m_PlayAreaSize;
        bool m_Climbing;
        Vector3 m_ClimbFrom;
        Vector3 m_ClimbTo;
        float m_ClimbElapsed;
        bool m_Celebrating;

        /// <summary>Feet position in play-area local space.</summary>
        public Vector3 Position;

        /// <summary>Yaw in degrees, zero facing +Z, away from the player.</summary>
        public float FacingDegrees;

        public LocomotionState State { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public float VerticalSpeed { get; private set; }

        /// <summary>Current capsule height, which shrinks while crouching.</summary>
        public float Height { get; private set; }

        /// <summary>
        /// True for the frame in which climb was pressed with nothing in reach to climb.
        /// The brief asks for a subtle response rather than silence, and this is what the
        /// controller uses to trigger it.
        /// </summary>
        public bool ClimbRejected { get; private set; }

        /// <summary>True for the frame the character lands after being airborne.</summary>
        public bool JustLanded { get; private set; }

        public bool IsClimbing { get { return m_Climbing; } }

        public CharacterMotor(MotorSettings settings, Vector2 playAreaSize)
        {
            m_Settings = settings;
            m_PlayAreaSize = playAreaSize;
            Height = settings.StandHeight;
            State = LocomotionState.Idle;
            IsGrounded = true;
        }

        public void SetPlayAreaSize(Vector2 size)
        {
            m_PlayAreaSize = size;
        }

        /// <summary>Drops the character at a starting point, upright and still.</summary>
        public void Teleport(Vector3 position, float facingDegrees)
        {
            Position = position;
            FacingDegrees = facingDegrees;
            VerticalSpeed = 0f;
            IsGrounded = true;
            IsCrouching = false;
            m_Climbing = false;
            m_Celebrating = false;
            Height = m_Settings.StandHeight;
            State = LocomotionState.Idle;
        }

        /// <summary>Stops accepting input and plays the celebration, for reaching the goal.</summary>
        public void Celebrate()
        {
            m_Celebrating = true;
            m_Climbing = false;
            State = LocomotionState.Celebrate;
        }

        /// <summary>Advances one frame.</summary>
        public void Step(MotorInput input, List<ObstacleBox> obstacles, float deltaTime)
        {
            if (deltaTime <= 0f)
                return;

            ClimbRejected = false;
            JustLanded = false;

            if (m_Celebrating)
            {
                SettleOnGround(obstacles, deltaTime);
                State = LocomotionState.Celebrate;
                return;
            }

            if (m_Climbing)
            {
                StepClimb(deltaTime);
                return;
            }

            UpdateStance(input, obstacles);

            if (input.ClimbPressed && IsGrounded)
            {
                TryStartClimb(obstacles);
                if (m_Climbing)
                    return;
            }

            // Crouching blocks jumping rather than doing both at once; the two stances
            // fight each other and a crouch-jump is not in the brief.
            if (input.JumpPressed && IsGrounded && !IsCrouching)
            {
                VerticalSpeed = m_Settings.JumpSpeed;
                IsGrounded = false;
            }

            MoveHorizontal(input, obstacles, deltaTime);
            MoveVertical(obstacles, deltaTime);
            UpdateState(input);
        }

        void UpdateStance(MotorInput input, List<ObstacleBox> obstacles)
        {
            bool wantsCrouch = input.Crouch && IsGrounded;

            // Standing up under an overhang would put the character's head inside it.
            // Staying crouched until there is room is the behaviour a player expects.
            if (IsCrouching && !wantsCrouch && !HasHeadroomToStand(obstacles))
                wantsCrouch = true;

            IsCrouching = wantsCrouch;
            Height = m_Settings.HeightFor(IsCrouching);
        }

        bool HasHeadroomToStand(List<ObstacleBox> obstacles)
        {
            return !IsBlockedAt(Position.x, Position.z, Position.y, m_Settings.StandHeight, obstacles);
        }

        void MoveHorizontal(MotorInput input, List<ObstacleBox> obstacles, float deltaTime)
        {
            Vector3 direction = new Vector3(input.MoveX, 0f, input.MoveZ);

            // Diagonals would otherwise be faster than the cardinals by a factor of 1.41.
            if (direction.sqrMagnitude > 1f)
                direction = direction.normalized;

            if (direction.sqrMagnitude < 0.0001f)
                return;

            float speed = m_Settings.SpeedFor(IsCrouching, input.Run);
            float step = speed * deltaTime;

            // Resolved one axis at a time so that walking into a box at an angle slides
            // along it instead of stopping dead.
            TryMove(direction.x * step, 0f, obstacles);
            TryMove(0f, direction.z * step, obstacles);

            float targetFacing = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            FacingDegrees = Mathf.MoveTowardsAngle(FacingDegrees, targetFacing,
                m_Settings.TurnSpeedDegreesPerSecond * deltaTime);
        }

        void TryMove(float deltaX, float deltaZ, List<ObstacleBox> obstacles)
        {
            if (deltaX == 0f && deltaZ == 0f)
                return;

            float halfX = m_PlayAreaSize.x * 0.5f - m_Settings.Radius;
            float halfZ = m_PlayAreaSize.y * 0.5f - m_Settings.Radius;

            float x = Mathf.Clamp(Position.x + deltaX, -halfX, halfX);
            float z = Mathf.Clamp(Position.z + deltaZ, -halfZ, halfZ);

            if (IsBlockedAt(x, z, Position.y, Height, obstacles))
                return;

            Position.x = x;
            Position.z = z;
        }

        void MoveVertical(List<ObstacleBox> obstacles, float deltaTime)
        {
            VerticalSpeed += m_Settings.Gravity * deltaTime;
            if (VerticalSpeed < m_Settings.TerminalSpeed)
                VerticalSpeed = m_Settings.TerminalSpeed;

            float nextY = Position.y + VerticalSpeed * deltaTime;
            float support = SupportHeightAt(Position.x, Position.z, Position.y, obstacles);

            bool wasGrounded = IsGrounded;

            if (VerticalSpeed <= 0f && nextY <= support)
            {
                nextY = support;
                VerticalSpeed = 0f;
                IsGrounded = true;
                JustLanded = !wasGrounded;
            }
            else
            {
                IsGrounded = false;
            }

            Position.y = nextY;
        }

        void SettleOnGround(List<ObstacleBox> obstacles, float deltaTime)
        {
            MoveVertical(obstacles, deltaTime);
        }

        /// <summary>
        /// The highest surface the character can be standing on at this point: the top of
        /// an obstacle it is over, or the desk itself. Obstacles above the character's
        /// feet cannot support it, which is what stops it snapping up onto a box it is
        /// walking underneath.
        /// </summary>
        float SupportHeightAt(float x, float z, float feetY, List<ObstacleBox> obstacles)
        {
            float support = 0f;

            for (int i = 0; i < obstacles.Count; i++)
            {
                ObstacleBox box = obstacles[i];

                if (box.Cleared)
                    continue;

                if (!box.OverlapsCircle(x, z, m_Settings.Radius))
                    continue;

                if (box.Top > feetY + m_Settings.StepHeight)
                    continue;

                if (box.Top > support)
                    support = box.Top;
            }

            return support;
        }

        bool IsBlockedAt(float x, float z, float feetY, float height, List<ObstacleBox> obstacles)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                ObstacleBox box = obstacles[i];

                if (!box.BlocksVertically(feetY, height, m_Settings.StepHeight))
                    continue;

                if (box.OverlapsCircle(x, z, m_Settings.Radius))
                    return true;
            }

            return false;
        }

        /// <summary>Unit vector the character is facing, on the desk surface.</summary>
        public Vector3 Facing
        {
            get
            {
                float radians = FacingDegrees * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(radians), 0f, Mathf.Cos(radians));
            }
        }

        void TryStartClimb(List<ObstacleBox> obstacles)
        {
            Vector3 forward = Facing;
            float reach = m_Settings.Radius + m_Settings.ClimbReach;
            float probeX = Position.x + forward.x * reach;
            float probeZ = Position.z + forward.z * reach;

            int index = FindClimbTarget(probeX, probeZ, obstacles);
            if (index < 0)
            {
                // Nothing to climb. The controller turns this into a small nudge and a
                // sound rather than leaving the button feeling broken.
                ClimbRejected = true;
                return;
            }

            ObstacleBox box = obstacles[index];

            // A thin obstacle is vaulted: the character ends up on the far side rather
            // than standing on top of a ledge too narrow to stand on.
            float depthAlongTravel = Mathf.Abs(forward.x) > Mathf.Abs(forward.z) ? box.Size.x : box.Size.z;
            bool vault = depthAlongTravel <= m_Settings.MaxVaultDepth;

            float distanceToEdge = box.HorizontalDistanceTo(Position.x, Position.z);
            float travel = vault
                ? distanceToEdge + depthAlongTravel + m_Settings.Radius * 2f
                : distanceToEdge + m_Settings.Radius * 2f;

            float targetX = Position.x + forward.x * travel;
            float targetZ = Position.z + forward.z * travel;

            float halfX = m_PlayAreaSize.x * 0.5f - m_Settings.Radius;
            float halfZ = m_PlayAreaSize.y * 0.5f - m_Settings.Radius;
            targetX = Mathf.Clamp(targetX, -halfX, halfX);
            targetZ = Mathf.Clamp(targetZ, -halfZ, halfZ);

            float targetY = vault
                ? SupportHeightAt(targetX, targetZ, box.Top, obstacles)
                : box.Top;

            m_ClimbFrom = Position;
            m_ClimbTo = new Vector3(targetX, targetY, targetZ);
            m_ClimbElapsed = 0f;
            m_Climbing = true;
            VerticalSpeed = 0f;
            IsGrounded = false;
            IsCrouching = false;
            Height = m_Settings.StandHeight;
            State = LocomotionState.Climb;
        }

        int FindClimbTarget(float x, float z, List<ObstacleBox> obstacles)
        {
            for (int i = 0; i < obstacles.Count; i++)
            {
                ObstacleBox box = obstacles[i];

                if (box.Cleared || !box.IsClimbable)
                    continue;

                // Below step height there is nothing to climb, and above the limit the
                // character cannot reach the top.
                if (box.Top <= m_Settings.StepHeight || box.Top > m_Settings.MaxClimbHeight)
                    continue;

                if (box.OverlapsCircle(x, z, m_Settings.Radius))
                    return i;
            }

            return -1;
        }

        void StepClimb(float deltaTime)
        {
            m_ClimbElapsed += deltaTime;
            float t = Mathf.Clamp01(m_ClimbElapsed / m_Settings.ClimbDuration);

            Position.x = Mathf.Lerp(m_ClimbFrom.x, m_ClimbTo.x, t);
            Position.z = Mathf.Lerp(m_ClimbFrom.z, m_ClimbTo.z, t);

            // Arc up over the obstacle rather than cutting through it diagonally.
            float peak = Mathf.Max(m_ClimbFrom.y, m_ClimbTo.y) + m_Settings.StepHeight;
            Position.y = t < 0.5f
                ? Mathf.Lerp(m_ClimbFrom.y, peak, t * 2f)
                : Mathf.Lerp(peak, m_ClimbTo.y, (t - 0.5f) * 2f);

            State = LocomotionState.Climb;

            if (t >= 1f)
            {
                m_Climbing = false;
                IsGrounded = true;
                JustLanded = true;
                VerticalSpeed = 0f;
            }
        }

        void UpdateState(MotorInput input)
        {
            if (!IsGrounded)
            {
                State = VerticalSpeed > 0f ? LocomotionState.Jump : LocomotionState.Fall;
                return;
            }

            if (!input.HasMovement)
            {
                State = IsCrouching ? LocomotionState.Crouch : LocomotionState.Idle;
                return;
            }

            if (IsCrouching)
                State = LocomotionState.CrouchWalk;
            else
                State = input.Run ? LocomotionState.Run : LocomotionState.Walk;
        }
    }
}
