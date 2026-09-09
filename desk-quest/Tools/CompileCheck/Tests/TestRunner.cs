using System;
using UnityEngine;

/// <summary>Minimal test harness. No framework, because one file is enough.</summary>
static class TestRunner
{
    public static int Failures;

    public static void Suite(string name)
    {
        Console.WriteLine("  " + name);
    }

    public static void Run(string name, Action test)
    {
        try
        {
            test();
            Console.WriteLine("    pass  " + name);
        }
        catch (Exception exception)
        {
            Failures++;
            Console.WriteLine("    FAIL  " + name + ": " + exception.Message);
        }
    }

    public static void IsTrue(bool condition, string what)
    {
        if (!condition)
            throw new Exception(what);
    }

    public static void AreClose(float expected, float actual, float tolerance, string what)
    {
        if (Mathf.Abs(expected - actual) > tolerance)
            throw new Exception(what + ": expected " + expected + " but got " + actual);
    }

    public static void AreEqual(object expected, object actual, string what)
    {
        if (!expected.Equals(actual))
            throw new Exception(what + ": expected " + expected + " but got " + actual);
    }
}
