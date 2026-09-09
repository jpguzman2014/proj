using System.Collections.Generic;
using UnityEngine;

namespace DeskQuest.Gameplay
{
    /// <summary>
    /// One obstacle in a level, described in fractions of the play area rather than in
    /// metres, so the same level fits a small desk and a large one.
    /// </summary>
    public struct ObstacleDefinition
    {
        /// <summary>Centre, as a fraction of the play area from its middle. -0.5 to 0.5 on each axis.</summary>
        public Vector2 Centre;

        /// <summary>Footprint, as a fraction of the play area.</summary>
        public Vector2 Size;

        /// <summary>Height as a multiple of the character's height.</summary>
        public float Height;

        /// <summary>Underside as a multiple of the character's height. Above zero makes an overhang to crouch under.</summary>
        public float Bottom;

        public ObstacleType Type;

        /// <summary>
        /// Footprint watched for real-world change, as a fraction of the play area.
        /// Smaller than the obstacle on purpose: the virtual barrier spans the desk so it
        /// genuinely blocks, while the real object it stands for sits on one patch. Zero
        /// means watch the whole footprint.
        /// </summary>
        public Vector2 DetectionSize;

        /// <summary>
        /// The real object this stands for, shown to the player when it needs clearing.
        /// Only ever a category, never a claim about what is actually on the desk: the
        /// prototype does not recognise objects and must not pretend to.
        /// </summary>
        public string Label;
    }

    /// <summary>A level: what blocks the path, and where the goal is.</summary>
    public struct LevelDefinition
    {
        public List<ObstacleDefinition> Obstacles;

        /// <summary>Where the character starts, as a fraction of the play area.</summary>
        public Vector2 Start;

        /// <summary>Where the treasure sits, as a fraction of the play area.</summary>
        public Vector2 Goal;
    }

    /// <summary>
    /// Builds the starting level.
    ///
    /// Hand-placed, as the brief asks for at this stage. The point of the layout is to
    /// exercise every action the player has: something to jump, something to climb,
    /// something to duck under, and two things that cannot be beaten by skill at all and
    /// only open when the matching real object leaves the desk.
    /// </summary>
    public static class LevelLayout
    {
        public static LevelDefinition BuildDefaultLevel()
        {
            List<ObstacleDefinition> obstacles = new List<ObstacleDefinition>();

            // Near the start: a low ridge to jump or step over. Teaches the jump button
            // before anything depends on it.
            obstacles.Add(New(
                centre: new Vector2(0f, -0.22f),
                size: new Vector2(0.55f, 0.05f),
                height: 0.32f,
                type: ObstacleType.LowObstacle,
                label: "Pencil"));

            // A block to climb, offset so it can also be walked around. Climbing is the
            // faster route, not the only one.
            obstacles.Add(New(
                centre: new Vector2(-0.22f, -0.05f),
                size: new Vector2(0.26f, 0.14f),
                height: 0.52f,
                type: ObstacleType.Climbable,
                label: "Eraser"));

            // The first real barrier. It spans the whole desk, so there is no walking
            // around it: the only way on is to take the matching real object away.
            obstacles.Add(New(
                centre: new Vector2(0f, 0.02f),
                size: new Vector2(1.0f, 0.09f),
                height: 1.30f,
                type: ObstacleType.BlockedPath,
                label: "Book",
                detectionSize: new Vector2(0.30f, 0.16f)));

            // An overhang: its underside is above a crouching character but below a
            // standing one.
            obstacles.Add(New(
                centre: new Vector2(-0.24f, 0.18f),
                size: new Vector2(0.32f, 0.12f),
                height: 1.40f,
                bottom: 0.70f,
                type: ObstacleType.HighObstacle,
                label: "Ruler"));

            // The last barrier before the goal, and the one the level cannot be finished
            // without clearing for real.
            obstacles.Add(New(
                centre: new Vector2(0f, 0.28f),
                size: new Vector2(1.0f, 0.09f),
                height: 1.60f,
                type: ObstacleType.BlockedPath,
                label: "Toy",
                detectionSize: new Vector2(0.30f, 0.16f)));

            LevelDefinition level = new LevelDefinition();
            level.Obstacles = obstacles;
            level.Start = new Vector2(0f, -0.40f);
            level.Goal = new Vector2(0.02f, 0.42f);
            return level;
        }

        static ObstacleDefinition New(Vector2 centre, Vector2 size, float height,
            ObstacleType type, string label, float bottom = 0f, Vector2 detectionSize = default(Vector2))
        {
            ObstacleDefinition definition = new ObstacleDefinition();
            definition.Centre = centre;
            definition.Size = size;
            definition.Height = height;
            definition.Bottom = bottom;
            definition.Type = type;
            definition.Label = label;
            definition.DetectionSize = detectionSize;
            return definition;
        }

        /// <summary>
        /// The patch of desk watched for change under an obstacle, in local metres.
        /// Falls back to the obstacle's own footprint when no smaller patch was given.
        /// </summary>
        public static ObstacleBox ToDetectionFootprint(ObstacleDefinition definition, int id,
            Vector2 playAreaSize, float characterHeight)
        {
            ObstacleBox box = ToBox(definition, id, playAreaSize, characterHeight);

            if (definition.DetectionSize.x <= 0f || definition.DetectionSize.y <= 0f)
                return box;

            box.Size = new Vector3(
                definition.DetectionSize.x * playAreaSize.x,
                box.Size.y,
                definition.DetectionSize.y * playAreaSize.y);

            return box;
        }

        /// <summary>
        /// Converts a definition into the metres-based box the motor collides against.
        /// </summary>
        public static ObstacleBox ToBox(ObstacleDefinition definition, int id,
            Vector2 playAreaSize, float characterHeight)
        {
            float bottom = definition.Bottom * characterHeight;
            float top = definition.Height * characterHeight;
            float height = Mathf.Max(top - bottom, 0.001f);

            Vector3 centre = new Vector3(
                definition.Centre.x * playAreaSize.x,
                bottom + height * 0.5f,
                definition.Centre.y * playAreaSize.y);

            Vector3 size = new Vector3(
                definition.Size.x * playAreaSize.x,
                height,
                definition.Size.y * playAreaSize.y);

            return ObstacleBox.Create(id, centre, size, definition.Type);
        }

        /// <summary>Converts a normalised play-area point into local metres on the desk.</summary>
        public static Vector3 ToLocalPosition(Vector2 normalised, Vector2 playAreaSize)
        {
            return new Vector3(normalised.x * playAreaSize.x, 0f, normalised.y * playAreaSize.y);
        }
    }
}
