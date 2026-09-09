// Tests for the decision at the heart of the cleaning mechanic.
//
// These matter more than most. The coverage readings this logic consumes come from AR
// plane geometry, which cannot be produced in this environment, so on a device the whole
// mechanic is unverified. What can be checked is that given a sequence of readings, the
// right decision comes out: that noise does not open the path, that an area already clear
// does not clear itself, and that a genuine change does get through.
using DeskQuest.Gameplay;

static class DetectionTests
{
    const float Dt = 0.35f;
    const float Occupied = 0.35f;
    const float Cleared = 0.60f;
    const float Hold = 1.5f;

    public static void RunAll()
    {
        TestRunner.Suite("change detection");
        TestRunner.Run("an occupied patch does not clear itself", OccupiedStaysOccupied);
        TestRunner.Run("an already-clear patch never fires", AlreadyClearNeverFires);
        TestRunner.Run("removing the object clears the patch", RemovalClears);
        TestRunner.Run("a brief spike does not clear the patch", SpikeDoesNotClear);
        TestRunner.Run("coverage dropping back restarts the wait", DropRestartsHold);
        TestRunner.Run("clearing is reported exactly once", ClearsOnlyOnce);
        TestRunner.Run("borderline coverage does not clear the patch", BorderlineDoesNotClear);
        TestRunner.Run("progress stays at zero until armed", ProgressIsHonestBeforeBaseline);
        TestRunner.Run("progress reaches one on clearing", ProgressCompletes);
        TestRunner.Run("reset returns it to the start", ResetWorks);
    }

    static void OccupiedStaysOccupied()
    {
        AreaClearanceTracker tracker = New();

        // The object stays put for ten seconds.
        for (int i = 0; i < 30; i++)
            TestRunner.IsTrue(!tracker.Observe(0.1f, Dt), "did not clear while occupied");

        TestRunner.IsTrue(tracker.HasBaseline, "armed itself by seeing the patch occupied");
        TestRunner.IsTrue(!tracker.IsCleared, "still not cleared");
    }

    static void AlreadyClearNeverFires()
    {
        AreaClearanceTracker tracker = New();

        // Coverage is high from the very first reading, meaning the game put a barrier
        // over a patch of desk with nothing on it. There is no removal to detect.
        for (int i = 0; i < 60; i++)
            TestRunner.IsTrue(!tracker.Observe(0.95f, Dt), "did not clear without ever being occupied");

        TestRunner.IsTrue(!tracker.HasBaseline, "never armed");
        TestRunner.IsTrue(!tracker.IsCleared, "never cleared");
    }

    static void RemovalClears()
    {
        AreaClearanceTracker tracker = New();

        Feed(tracker, 0.05f, 8);

        bool fired = false;
        for (int i = 0; i < 20 && !fired; i++)
            fired = tracker.Observe(0.85f, Dt);

        TestRunner.IsTrue(fired, "cleared once the desk became visible and stayed visible");
        TestRunner.IsTrue(tracker.IsCleared, "is marked cleared");
    }

    static void SpikeDoesNotClear()
    {
        AreaClearanceTracker tracker = New();
        Feed(tracker, 0.05f, 8);

        // One high reading, then back to occupied. A hand passing over the patch, or the
        // tracker briefly guessing wrong, should not open the path.
        TestRunner.IsTrue(!tracker.Observe(0.9f, Dt), "one high reading did not clear it");
        Feed(tracker, 0.1f, 10);

        TestRunner.IsTrue(!tracker.IsCleared, "still not cleared after the spike passed");
    }

    static void DropRestartsHold()
    {
        AreaClearanceTracker tracker = New();
        Feed(tracker, 0.05f, 8);

        // Nearly long enough, then interrupted.
        tracker.Observe(0.9f, Dt);
        tracker.Observe(0.9f, Dt);
        tracker.Observe(0.9f, Dt);
        TestRunner.IsTrue(tracker.HeldFor > 0f, "was part way through the wait");

        tracker.Observe(0.1f, Dt);
        TestRunner.AreClose(0f, tracker.HeldFor, 0.0001f, "the wait restarted");
        TestRunner.IsTrue(!tracker.IsCleared, "did not clear");
    }

    static void ClearsOnlyOnce()
    {
        AreaClearanceTracker tracker = New();
        Feed(tracker, 0.05f, 8);

        int firings = 0;
        for (int i = 0; i < 40; i++)
        {
            if (tracker.Observe(0.9f, Dt))
                firings++;
        }

        TestRunner.AreEqual(1, firings, "times the clearing was reported");
    }

    static void BorderlineDoesNotClear()
    {
        AreaClearanceTracker tracker = New();
        Feed(tracker, 0.05f, 8);

        // Sitting just under the threshold for a long time. This is the case where the
        // object was moved a little but not taken away.
        for (int i = 0; i < 60; i++)
            TestRunner.IsTrue(!tracker.Observe(Cleared - 0.05f, Dt), "did not clear on borderline coverage");
    }

    static void ProgressIsHonestBeforeBaseline()
    {
        AreaClearanceTracker tracker = New();

        tracker.Observe(0.95f, Dt);
        TestRunner.AreClose(0f, tracker.Progress, 0.0001f,
            "shows no progress towards a decision it cannot yet make");
    }

    static void ProgressCompletes()
    {
        AreaClearanceTracker tracker = New();
        Feed(tracker, 0.05f, 8);

        tracker.Observe(0.9f, Dt);
        TestRunner.IsTrue(tracker.Progress > 0f && tracker.Progress < 1f, "progress is part way through");

        Feed(tracker, 0.9f, 20);
        TestRunner.AreClose(1f, tracker.Progress, 0.0001f, "progress is complete once cleared");
    }

    static void ResetWorks()
    {
        AreaClearanceTracker tracker = New();
        Feed(tracker, 0.05f, 8);
        Feed(tracker, 0.9f, 20);
        TestRunner.IsTrue(tracker.IsCleared, "cleared before the reset");

        tracker.Reset();

        TestRunner.IsTrue(!tracker.IsCleared, "no longer cleared");
        TestRunner.IsTrue(!tracker.HasBaseline, "no longer armed");
        TestRunner.AreClose(0f, tracker.HeldFor, 0.0001f, "the wait was cleared");
    }

    static AreaClearanceTracker New()
    {
        return new AreaClearanceTracker(Occupied, Cleared, Hold);
    }

    static void Feed(AreaClearanceTracker tracker, float coverage, int readings)
    {
        for (int i = 0; i < readings; i++)
            tracker.Observe(coverage, Dt);
    }
}
