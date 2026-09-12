using UnityEngine;

namespace DeskQuest.Gameplay
{
    /// <summary>
    /// What kind of thing an obstacle is, which decides how the player is meant to get
    /// past it. The collision solver treats every uncleared obstacle as solid regardless;
    /// the type only decides whether jumping or climbing can beat it.
    /// </summary>
    public enum ObstacleType
    {
        /// <summary>Short enough to jump onto or over.</summary>
        LowObstacle,

        /// <summary>Too tall to jump or climb. Go around it.</summary>
        HighObstacle,

        /// <summary>Spans the play area. There is no way around, only over or through.</summary>
        Wall,

        /// <summary>Can be climbed or vaulted with the climb action.</summary>
        Climbable,

        /// <summary>Cleared only by tidying the matching real object off the desk.</summary>
        BlockedPath
    }

    /// <summary>
    /// An axis-aligned box in play-area local space, in metres, with Y measured up from
    /// the desk surface.
    ///
    /// A box whose bottom sits above zero is an overhang: the character can crouch-walk
    /// underneath it. That falls out of the geometry rather than needing its own type.
    ///
    /// This is a value type with no Unity references beyond the vector maths, which is
    /// what lets the movement solver be tested outside Unity.
    /// </summary>
    public struct ObstacleBox
    {
        public int Id;
        public Vector3 Center;
        public Vector3 Size;
        public ObstacleType Type;

        /// <summary>Cleared obstacles are ignored by collision, as if they were not there.</summary>
        public bool Cleared;

        public float MinX { get { return Center.x - Size.x * 0.5f; } }
        public float MaxX { get { return Center.x + Size.x * 0.5f; } }
        public float MinZ { get { return Center.z - Size.z * 0.5f; } }
        public float MaxZ { get { return Center.z + Size.z * 0.5f; } }
        public float Bottom { get { return Center.y - Size.y * 0.5f; } }
        public float Top { get { return Center.y + Size.y * 0.5f; } }

        public bool IsClimbable { get { return Type == ObstacleType.Climbable; } }

        /// <summary>Depth along the axis the character crosses it on. Used to tell a vault from a climb.</summary>
        public float SmallestFootprintSide { get { return Mathf.Min(Size.x, Size.z); } }

        public static ObstacleBox Create(int id, Vector3 center, Vector3 size, ObstacleType type)
        {
            ObstacleBox box = new ObstacleBox();
            box.Id = id;
            box.Center = center;
            box.Size = size;
            box.Type = type;
            box.Cleared = false;
            return box;
        }

        /// <summary>
        /// Shortest distance from a point on the desk surface to this box's footprint,
        /// measured in the XZ plane. Zero when the point is inside the footprint.
        /// </summary>
        public float HorizontalDistanceTo(float x, float z)
        {
            float dx = Mathf.Max(Mathf.Max(MinX - x, 0f), x - MaxX);
            float dz = Mathf.Max(Mathf.Max(MinZ - z, 0f), z - MaxZ);
            return Mathf.Sqrt(dx * dx + dz * dz);
        }

        /// <summary>True when a circle of this radius on the desk surface touches the footprint.</summary>
        public bool OverlapsCircle(float x, float z, float radius)
        {
            return HorizontalDistanceTo(x, z) < radius;
        }

        /// <summary>
        /// True when this box blocks a character whose feet are at <paramref name="feetY"/>
        /// and who is <paramref name="height"/> tall.
        ///
        /// Anything low enough to step onto does not block, and anything entirely above
        /// the character's head does not block either, which is what lets crouching pass
        /// under an overhang.
        /// </summary>
        public bool BlocksVertically(float feetY, float height, float stepHeight)
        {
            if (Cleared)
                return false;

            float headY = feetY + height;
            bool lowEnoughToStepOnto = Top <= feetY + stepHeight;
            bool entirelyAboveHead = Bottom >= headY;
            return !lowEnoughToStepOnto && !entirelyAboveHead;
        }
    }
}
