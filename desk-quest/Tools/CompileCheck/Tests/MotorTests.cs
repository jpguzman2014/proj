// Unit tests for the movement rules.
//
// These are real tests, not a compile check. CharacterMotor is deliberately free of
// Unity dependencies beyond vector maths, and the stubbed Mathf is really implemented,
// so the behaviour asserted here is the behaviour the game will have. What they cannot
// tell you is whether the numbers feel good in the hand. That needs a phone.
using System;
using System.Collections.Generic;
using DeskQuest.Gameplay;
using UnityEngine;

static class MotorTests
{
    const float Dt = 1f / 60f;
    static readonly Vector2 PlayArea = new Vector2(0.6f, 0.4f);

    static int s_Failures;

    static void Main()
    {
        Run("falls to the desk under gravity", FallsToDesk);
        Run("walks forward at roughly walk speed", WalksForward);
        Run("running is faster than walking", RunIsFaster);
        Run("diagonal is not faster than straight", DiagonalNotFaster);
        Run("cannot leave the play area", StaysInsidePlayArea);
        Run("a wall blocks movement", WallBlocks);
        Run("slides along a wall hit at an angle", SlidesAlongWall);
        Run("steps up onto a kerb without jumping", StepsUpOntoKerb);
        Run("jumps and lands back on the desk", JumpsAndLands);
        Run("crouching fits under an overhang", CrouchFitsUnderOverhang);
        Run("standing does not fit under an overhang", StandingBlockedByOverhang);
        Run("stays crouched while under an overhang", StaysCrouchedUnderOverhang);
        Run("climbs onto a climbable block", ClimbsOntoBlock);
        Run("vaults over a thin block", VaultsThinBlock);
        Run("climb with nothing in reach is rejected", ClimbRejectedWithNothingInReach);
        Run("a cleared obstacle stops blocking", ClearedObstacleStopsBlocking);
        Run("falls when the block underfoot is cleared", FallsWhenSupportCleared);
        Run("landing is reported once", LandingReportedOnce);

        Console.WriteLine();
        if (s_Failures == 0)
            Console.WriteLine("All motor tests passed.");
        else
            Console.WriteLine(s_Failures + " motor test(s) failed.");

        Environment.Exit(s_Failures);
    }

    // --- tests ---

    static void FallsToDesk()
    {
        CharacterMotor motor = NewMotor();
        motor.Teleport(new Vector3(0f, 0.05f, 0f), 0f);
        Simulate(motor, MotorInput(), NoObstacles(), 1.5f);

        AreClose(0f, motor.Position.y, 0.001f, "resting height");
        IsTrue(motor.IsGrounded, "grounded after landing");
        AreEqual(LocomotionState.Idle, motor.State, "state once still");
    }

    static void WalksForward()
    {
        MotorSettings settings = new MotorSettings();
        CharacterMotor motor = NewMotor(settings);
        MotorInput input = MotorInput();
        input.MoveZ = 1f;

        Simulate(motor, input, NoObstacles(), 1f);

        // One second of walking should cover about one second of walk speed. The
        // tolerance absorbs the single frame spent turning to face the direction.
        AreClose(settings.WalkSpeed, motor.Position.z, settings.WalkSpeed * 0.1f, "distance in one second");
        AreEqual(LocomotionState.Walk, motor.State, "state while walking");
    }

    static void RunIsFaster()
    {
        CharacterMotor walker = NewMotor();
        CharacterMotor runner = NewMotor();

        MotorInput walk = MotorInput();
        walk.MoveZ = 1f;
        MotorInput run = walk;
        run.Run = true;

        Simulate(walker, walk, NoObstacles(), 1f);
        Simulate(runner, run, NoObstacles(), 1f);

        IsTrue(runner.Position.z > walker.Position.z * 1.4f, "running covers more ground");
        AreEqual(LocomotionState.Run, runner.State, "state while running");
    }

    static void DiagonalNotFaster()
    {
        CharacterMotor straight = NewMotor();
        CharacterMotor diagonal = NewMotor();

        MotorInput forward = MotorInput();
        forward.MoveZ = 1f;

        MotorInput corner = MotorInput();
        corner.MoveX = 1f;
        corner.MoveZ = 1f;

        Simulate(straight, forward, NoObstacles(), 1f);
        Simulate(diagonal, corner, NoObstacles(), 1f);

        float straightDistance = Length(straight.Position);
        float diagonalDistance = Length(diagonal.Position);

        IsTrue(diagonalDistance <= straightDistance * 1.05f,
            "diagonal distance " + diagonalDistance + " is not more than straight " + straightDistance);
    }

    static void StaysInsidePlayArea()
    {
        MotorSettings settings = new MotorSettings();
        CharacterMotor motor = NewMotor(settings);
        MotorInput input = MotorInput();
        input.MoveZ = 1f;
        input.Run = true;

        Simulate(motor, input, NoObstacles(), 20f);

        float limit = PlayArea.y * 0.5f - settings.Radius;
        AreClose(limit, motor.Position.z, 0.0005f, "stopped at the boundary");
    }

    static void WallBlocks()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(ObstacleBox.Create(1, new Vector3(0f, 0.04f, 0.08f),
            new Vector3(0.6f, 0.08f, 0.02f), ObstacleType.Wall));

        CharacterMotor motor = NewMotor();
        MotorInput input = MotorInput();
        input.MoveZ = 1f;

        Simulate(motor, input, obstacles, 5f);

        IsTrue(motor.Position.z < 0.07f, "stopped short of the wall, at z=" + motor.Position.z);
        AreClose(0f, motor.Position.y, 0.001f, "did not climb the wall");
    }

    static void SlidesAlongWall()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(ObstacleBox.Create(1, new Vector3(0f, 0.04f, 0.08f),
            new Vector3(0.6f, 0.08f, 0.02f), ObstacleType.Wall));

        CharacterMotor motor = NewMotor();
        MotorInput input = MotorInput();
        input.MoveX = 1f;
        input.MoveZ = 1f;

        Simulate(motor, input, obstacles, 2f);

        IsTrue(motor.Position.x > 0.02f, "kept moving sideways along the wall, x=" + motor.Position.x);
    }

    static void StepsUpOntoKerb()
    {
        MotorSettings settings = new MotorSettings();
        float kerbTop = settings.StepHeight * 0.8f;

        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(ObstacleBox.Create(1, new Vector3(0f, kerbTop * 0.5f, 0.08f),
            new Vector3(0.3f, kerbTop, 0.06f), ObstacleType.LowObstacle));

        CharacterMotor motor = NewMotor(settings);
        MotorInput input = MotorInput();
        input.MoveZ = 1f;

        Simulate(motor, input, obstacles, 2f);

        AreClose(kerbTop, motor.Position.y, 0.001f, "standing on the kerb");
        IsTrue(motor.Position.z > 0.06f, "walked onto it rather than stopping, z=" + motor.Position.z);
    }

    static void JumpsAndLands()
    {
        CharacterMotor motor = NewMotor();
        MotorInput jump = MotorInput();
        jump.JumpPressed = true;

        motor.Step(jump, NoObstacles(), Dt);
        IsTrue(!motor.IsGrounded, "left the ground");
        AreEqual(LocomotionState.Jump, motor.State, "state going up");

        float peak = 0f;
        MotorInput idle = MotorInput();
        for (int i = 0; i < 200; i++)
        {
            motor.Step(idle, NoObstacles(), Dt);
            if (motor.Position.y > peak)
                peak = motor.Position.y;
            if (motor.IsGrounded)
                break;
        }

        IsTrue(peak > 0.02f, "cleared a useful height, peaked at " + peak);
        IsTrue(motor.IsGrounded, "landed again");
        AreClose(0f, motor.Position.y, 0.001f, "back on the desk");
    }

    static void CrouchFitsUnderOverhang()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(Overhang());

        CharacterMotor motor = NewMotor();
        MotorInput input = MotorInput();
        input.MoveZ = 1f;
        input.Crouch = true;

        Simulate(motor, input, obstacles, 6f);

        IsTrue(motor.Position.z > 0.13f, "passed under the overhang, z=" + motor.Position.z);
        AreEqual(LocomotionState.CrouchWalk, motor.State, "state while crouch-walking");
    }

    static void StandingBlockedByOverhang()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(Overhang());

        CharacterMotor motor = NewMotor();
        MotorInput input = MotorInput();
        input.MoveZ = 1f;

        Simulate(motor, input, obstacles, 4f);

        IsTrue(motor.Position.z < 0.07f, "stopped at the overhang, z=" + motor.Position.z);
    }

    static void StaysCrouchedUnderOverhang()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(Overhang());

        CharacterMotor motor = NewMotor();
        MotorInput crouchForward = MotorInput();
        crouchForward.MoveZ = 1f;
        crouchForward.Crouch = true;

        // Get under it, then let go of crouch while still underneath.
        Simulate(motor, crouchForward, obstacles, 3f);
        IsTrue(motor.Position.z > 0.075f && motor.Position.z < 0.105f,
            "positioned under the overhang, z=" + motor.Position.z);

        MotorInput released = MotorInput();
        Simulate(motor, released, obstacles, 0.5f);

        IsTrue(motor.IsCrouching, "stayed crouched rather than standing into the overhang");
    }

    static void ClimbsOntoBlock()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(ObstacleBox.Create(1, new Vector3(0f, 0.015f, 0.04f),
            new Vector3(0.1f, 0.03f, 0.05f), ObstacleType.Climbable));

        MotorSettings settings = new MotorSettings();
        CharacterMotor motor = NewMotor(settings);

        MotorInput climb = MotorInput();
        climb.ClimbPressed = true;
        motor.Step(climb, obstacles, Dt);
        IsTrue(motor.IsClimbing, "climb started");
        IsTrue(!motor.ClimbRejected, "climb was not rejected");

        Simulate(motor, MotorInput(), obstacles, settings.ClimbDuration + 0.5f);

        AreClose(0.03f, motor.Position.y, 0.002f, "ended up on top of the block");
        IsTrue(motor.IsGrounded, "grounded on top");
    }

    static void VaultsThinBlock()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        obstacles.Add(ObstacleBox.Create(1, new Vector3(0f, 0.015f, 0.04f),
            new Vector3(0.1f, 0.03f, 0.02f), ObstacleType.Climbable));

        MotorSettings settings = new MotorSettings();
        CharacterMotor motor = NewMotor(settings);

        MotorInput climb = MotorInput();
        climb.ClimbPressed = true;
        motor.Step(climb, obstacles, Dt);
        IsTrue(motor.IsClimbing, "vault started");

        Simulate(motor, MotorInput(), obstacles, settings.ClimbDuration + 0.5f);

        AreClose(0f, motor.Position.y, 0.002f, "landed back on the desk");
        IsTrue(motor.Position.z > 0.05f, "ended up past the block, z=" + motor.Position.z);
    }

    static void ClimbRejectedWithNothingInReach()
    {
        CharacterMotor motor = NewMotor();
        MotorInput climb = MotorInput();
        climb.ClimbPressed = true;

        motor.Step(climb, NoObstacles(), Dt);

        IsTrue(motor.ClimbRejected, "reported that there was nothing to climb");
        IsTrue(!motor.IsClimbing, "did not start a climb");
    }

    static void ClearedObstacleStopsBlocking()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        ObstacleBox blocker = ObstacleBox.Create(1, new Vector3(0f, 0.04f, 0.08f),
            new Vector3(0.6f, 0.08f, 0.02f), ObstacleType.BlockedPath);
        obstacles.Add(blocker);

        CharacterMotor motor = NewMotor();
        MotorInput input = MotorInput();
        input.MoveZ = 1f;

        Simulate(motor, input, obstacles, 3f);
        IsTrue(motor.Position.z < 0.07f, "blocked while the obstacle stands");

        blocker.Cleared = true;
        obstacles[0] = blocker;

        Simulate(motor, input, obstacles, 3f);
        IsTrue(motor.Position.z > 0.12f, "walked through once cleared, z=" + motor.Position.z);
    }

    static void FallsWhenSupportCleared()
    {
        List<ObstacleBox> obstacles = new List<ObstacleBox>();
        ObstacleBox platform = ObstacleBox.Create(1, new Vector3(0f, 0.015f, 0.04f),
            new Vector3(0.1f, 0.03f, 0.05f), ObstacleType.LowObstacle);
        obstacles.Add(platform);

        CharacterMotor motor = NewMotor();
        motor.Teleport(new Vector3(0f, 0.03f, 0.04f), 0f);
        Simulate(motor, MotorInput(), obstacles, 0.2f);
        AreClose(0.03f, motor.Position.y, 0.001f, "standing on the platform");

        platform.Cleared = true;
        obstacles[0] = platform;

        Simulate(motor, MotorInput(), obstacles, 1.5f);
        AreClose(0f, motor.Position.y, 0.001f, "fell to the desk once it was cleared");
    }

    static void LandingReportedOnce()
    {
        CharacterMotor motor = NewMotor();
        MotorInput jump = MotorInput();
        jump.JumpPressed = true;
        motor.Step(jump, NoObstacles(), Dt);

        int landings = 0;
        MotorInput idle = MotorInput();
        for (int i = 0; i < 300; i++)
        {
            motor.Step(idle, NoObstacles(), Dt);
            if (motor.JustLanded)
                landings++;
        }

        AreEqual(1, landings, "landing events during one jump");
    }

    // --- helpers ---

    static ObstacleBox Overhang()
    {
        // Bottom above crouch height, top well above standing height: a bridge to duck under.
        return ObstacleBox.Create(1, new Vector3(0f, 0.09f, 0.09f),
            new Vector3(0.3f, 0.06f, 0.06f), ObstacleType.HighObstacle);
    }

    static CharacterMotor NewMotor()
    {
        return NewMotor(new MotorSettings());
    }

    static CharacterMotor NewMotor(MotorSettings settings)
    {
        return new CharacterMotor(settings, PlayArea);
    }

    static List<ObstacleBox> NoObstacles()
    {
        return new List<ObstacleBox>();
    }

    static MotorInput MotorInput()
    {
        return new MotorInput();
    }

    static void Simulate(CharacterMotor motor, MotorInput input, List<ObstacleBox> obstacles, float seconds)
    {
        int steps = (int)(seconds / Dt);
        for (int i = 0; i < steps; i++)
            motor.Step(input, obstacles, Dt);
    }

    static float Length(Vector3 v)
    {
        return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    // --- assertions ---

    static void Run(string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine("  pass  " + name);
        }
        catch (Exception exception)
        {
            s_Failures++;
            Console.WriteLine("  FAIL  " + name + ": " + exception.Message);
        }
    }

    static void IsTrue(bool condition, string what)
    {
        if (!condition)
            throw new Exception(what);
    }

    static void AreClose(float expected, float actual, float tolerance, string what)
    {
        if (Mathf.Abs(expected - actual) > tolerance)
            throw new Exception(what + ": expected " + expected + " but got " + actual);
    }

    static void AreEqual(object expected, object actual, string what)
    {
        if (!expected.Equals(actual))
            throw new Exception(what + ": expected " + expected + " but got " + actual);
    }
}
