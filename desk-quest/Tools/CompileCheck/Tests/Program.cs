using System;

static class Program
{
    static void Main()
    {
        MotorTests.RunAll();
        LevelTests.RunAll();
        DetectionTests.RunAll();

        Console.WriteLine();
        if (TestRunner.Failures == 0)
            Console.WriteLine("All tests passed.");
        else
            Console.WriteLine(TestRunner.Failures + " test(s) failed.");

        Environment.Exit(TestRunner.Failures);
    }
}
