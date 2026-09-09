// Tests for the default level's shape.
//
// These exist because of a bug that compiled and ran perfectly: the two barriers the
// player is meant to clear by tidying up left a walkable gap at one edge, so the
// character could stroll past and the game's whole mechanic never fired. Nothing in a
// type check catches that. These do.
using System.Collections.Generic;
using DeskQuest.Gameplay;
using UnityEngine;

static class LevelTests
{
    const float Dt = 1f / 60f;
    const float CharacterHeight = 0.09f;
    static readonly Vector2 PlayArea = new Vector2(0.6f, 0.4f);

    public static void RunAll()
    {
        TestRunner.Suite("level layout");
        TestRunner.Run("there are barriers that need tidying", HasBarriersToClear);
        TestRunner.Run("barriers span the whole desk", BarriersSpanTheDesk);
        TestRunner.Run("no way around the first barrier", CannotWalkAroundFirstBarrier);
        TestRunner.Run("no way over the first barrier", CannotJumpOverFirstBarrier);
        TestRunner.Run("the goal sits beyond the last barrier", GoalIsBeyondLastBarrier);
        TestRunner.Run("clearing the barriers opens a route to the goal", ClearedRouteReachesGoal);
        TestRunner.Run("watched patches are smaller than the barriers", DetectionPatchesAreSmaller);
    }

    static void HasBarriersToClear()
    {
        List<ObstacleBox> boxes = BuildBoxes();
        int barriers = 0;

        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Type == ObstacleType.BlockedPath)
                barriers++;
        }

        TestRunner.IsTrue(barriers >= 2, "at least two obstacles require tidying, found " + barriers);
    }

    static void BarriersSpanTheDesk()
    {
        List<ObstacleBox> boxes = BuildBoxes();

        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Type != ObstacleType.BlockedPath)
                continue;

            TestRunner.IsTrue(boxes[i].MinX <= -PlayArea.x * 0.5f + 0.001f,
                "barrier " + i + " reaches the left edge, min x is " + boxes[i].MinX);
            TestRunner.IsTrue(boxes[i].MaxX >= PlayArea.x * 0.5f - 0.001f,
                "barrier " + i + " reaches the right edge, max x is " + boxes[i].MaxX);
        }
    }

    static void CannotWalkAroundFirstBarrier()
    {
        List<ObstacleBox> boxes = BuildBoxes();
        ObstacleBox barrier = FirstBarrier(boxes);

        // Sweep the full width, walking straight at the barrier from each start.
        for (int i = 0; i <= 20; i++)
        {
            float x = Mathf.Lerp(-PlayArea.x * 0.5f + 0.02f, PlayArea.x * 0.5f - 0.02f, i / 20f);

            CharacterMotor motor = NewMotor();
            motor.Teleport(new Vector3(x, 0f, barrier.MinZ - 0.05f), 0f);

            MotorInput forward = new MotorInput();
            forward.MoveZ = 1f;
            forward.Run = true;

            Simulate(motor, forward, boxes, 5f);

            TestRunner.IsTrue(motor.Position.z < barrier.MaxZ,
                "walking forward from x=" + x + " got past the barrier to z=" + motor.Position.z);
        }
    }

    static void CannotJumpOverFirstBarrier()
    {
        List<ObstacleBox> boxes = BuildBoxes();
        ObstacleBox barrier = FirstBarrier(boxes);

        CharacterMotor motor = NewMotor();
        motor.Teleport(new Vector3(0f, 0f, barrier.MinZ - 0.05f), 0f);

        MotorInput running = new MotorInput();
        running.MoveZ = 1f;
        running.Run = true;

        // Hold forward and hammer jump, which is what a player does when stuck.
        for (int frame = 0; frame < 900; frame++)
        {
            MotorInput input = running;
            input.JumpPressed = frame % 20 == 0;
            motor.Step(input, boxes, Dt);
        }

        TestRunner.IsTrue(motor.Position.z < barrier.MaxZ,
            "jumping got past the barrier to z=" + motor.Position.z);
    }

    static void GoalIsBeyondLastBarrier()
    {
        LevelDefinition level = LevelLayout.BuildDefaultLevel();
        List<ObstacleBox> boxes = BuildBoxes();

        float furthestBarrier = float.MinValue;
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Type == ObstacleType.BlockedPath && boxes[i].MaxZ > furthestBarrier)
                furthestBarrier = boxes[i].MaxZ;
        }

        Vector3 goal = LevelLayout.ToLocalPosition(level.Goal, PlayArea);
        Vector3 start = LevelLayout.ToLocalPosition(level.Start, PlayArea);

        TestRunner.IsTrue(goal.z > furthestBarrier,
            "the goal at z=" + goal.z + " is past the last barrier at z=" + furthestBarrier);
        TestRunner.IsTrue(start.z < furthestBarrier,
            "the start at z=" + start.z + " is before the last barrier");
    }

    static void ClearedRouteReachesGoal()
    {
        LevelDefinition level = LevelLayout.BuildDefaultLevel();
        List<ObstacleBox> boxes = BuildBoxes();

        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Type != ObstacleType.BlockedPath)
                continue;

            ObstacleBox box = boxes[i];
            box.Cleared = true;
            boxes[i] = box;
        }

        CharacterMotor motor = NewMotor();
        motor.Teleport(LevelLayout.ToLocalPosition(level.Start, PlayArea), 0f);

        Vector3 goal = LevelLayout.ToLocalPosition(level.Goal, PlayArea);
        float distance = SeekGoal(motor, boxes, goal, 60f);

        // The goal's arrival radius is nine tenths of the character's height.
        TestRunner.IsTrue(distance < CharacterHeight * 0.9f,
            "reached the treasure, ended " + distance + "m away at " +
            motor.Position.x + ", " + motor.Position.z);
    }

    /// <summary>
    /// Walks towards the goal, and when it stops making progress tries the two things a
    /// player would try: jumping, then going along the obstruction. Returns how close it
    /// got. Deliberately greedy rather than a memorised route, because the point is to
    /// prove a cleared level is navigable at all, not that one particular path works.
    /// </summary>
    static float SeekGoal(CharacterMotor motor, List<ObstacleBox> boxes, Vector3 goal, float timeLimit)
    {
        const float arrivalRadius = CharacterHeight * 0.5f;
        const float progressWindow = 0.5f;
        const float minimumProgress = 0.005f;

        int steps = (int)(timeLimit / Dt);
        Vector3 lastCheckPosition = motor.Position;
        float sinceCheck = 0f;

        // 0 none, 1 jump over it, 2 walk along it.
        int recovery = 0;
        float recoveryRemaining = 0f;
        float sidestepDirection = 1f;
        float sinceJumpPress = 0f;

        for (int i = 0; i < steps; i++)
        {
            float dx = goal.x - motor.Position.x;
            float dz = goal.z - motor.Position.z;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);

            if (distance < arrivalRadius)
                return distance;

            MotorInput input = new MotorInput();
            input.Run = true;
            input.MoveX = dx / distance;
            input.MoveZ = dz / distance;

            if (recovery == 1)
            {
                sinceJumpPress += Dt;
                if (sinceJumpPress >= 0.3f)
                {
                    input.JumpPressed = true;
                    sinceJumpPress = 0f;
                }
            }
            else if (recovery == 2)
            {
                input.MoveX = sidestepDirection;
                input.MoveZ = 0f;
            }

            motor.Step(input, boxes, Dt);

            if (recoveryRemaining > 0f)
            {
                recoveryRemaining -= Dt;
                if (recoveryRemaining <= 0f)
                    recovery = 0;
            }

            sinceCheck += Dt;
            if (sinceCheck < progressWindow)
                continue;

            float moved = (motor.Position - lastCheckPosition).magnitude;
            lastCheckPosition = motor.Position;
            sinceCheck = 0f;

            if (moved >= minimumProgress || recoveryRemaining > 0f)
                continue;

            // Stuck. Try jumping first, since most of what is in the way is low, and
            // fall back to walking along the obstruction if jumping did not help.
            if (recovery == 1)
            {
                recovery = 2;
                recoveryRemaining = 2.5f;
                sidestepDirection = -sidestepDirection;
            }
            else
            {
                recovery = 1;
                recoveryRemaining = 1.5f;
                sinceJumpPress = 1f;
            }
        }

        float finalDx = goal.x - motor.Position.x;
        float finalDz = goal.z - motor.Position.z;
        return Mathf.Sqrt(finalDx * finalDx + finalDz * finalDz);
    }

    static void DetectionPatchesAreSmaller()
    {
        LevelDefinition level = LevelLayout.BuildDefaultLevel();

        for (int i = 0; i < level.Obstacles.Count; i++)
        {
            ObstacleDefinition definition = level.Obstacles[i];
            if (definition.Type != ObstacleType.BlockedPath)
                continue;

            ObstacleBox box = LevelLayout.ToBox(definition, i, PlayArea, CharacterHeight);
            ObstacleBox patch = LevelLayout.ToDetectionFootprint(definition, i, PlayArea, CharacterHeight);

            TestRunner.IsTrue(patch.Size.x < box.Size.x,
                "the watched patch for obstacle " + i + " is narrower than the barrier");
            TestRunner.IsTrue(patch.Size.x > 0.01f,
                "the watched patch for obstacle " + i + " is big enough to sample");
        }
    }

    static ObstacleBox FirstBarrier(List<ObstacleBox> boxes)
    {
        for (int i = 0; i < boxes.Count; i++)
        {
            if (boxes[i].Type == ObstacleType.BlockedPath)
                return boxes[i];
        }

        throw new System.Exception("the level has no barrier that needs tidying");
    }

    static List<ObstacleBox> BuildBoxes()
    {
        LevelDefinition level = LevelLayout.BuildDefaultLevel();
        List<ObstacleBox> boxes = new List<ObstacleBox>();

        for (int i = 0; i < level.Obstacles.Count; i++)
            boxes.Add(LevelLayout.ToBox(level.Obstacles[i], i, PlayArea, CharacterHeight));

        return boxes;
    }

    static CharacterMotor NewMotor()
    {
        return new CharacterMotor(new MotorSettings(), PlayArea);
    }

    static void Walk(CharacterMotor motor, List<ObstacleBox> boxes, float x, float z, float seconds)
    {
        MotorInput input = new MotorInput();
        input.MoveX = x;
        input.MoveZ = z;
        input.Run = true;
        Simulate(motor, input, boxes, seconds);
    }

    static void Simulate(CharacterMotor motor, MotorInput input, List<ObstacleBox> boxes, float seconds)
    {
        int steps = (int)(seconds / Dt);
        for (int i = 0; i < steps; i++)
            motor.Step(input, boxes, Dt);
    }
}
