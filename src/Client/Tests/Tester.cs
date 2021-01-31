using Godot;
using System;
using NUnit;
using CraigStars.Tests;

public class Tester : Node
{
    [Export]
    public string TestMethodToRun { get; set; } = "";

    async public override void _Ready()
    {
        TestRunner testRunner = new TestRunner(GetTree(), TestMethodToRun);
        int passed = 0;
        int failed = 0;

        GD.Print("\nTest Results:\n");

        await testRunner.Run((TestResult testResult) =>
        {
            if (testResult.result == TestResult.Result.Failed)
            {
                failed++;
                GD.Print("FAILED: " + testResult.classType.Name + "." + testResult.testMethod.Name + "\n" + testResult.exception.Message);
            }
            else
            {
                GD.Print("PASSED: " + testResult.classType.Name + "." + testResult.testMethod.Name);
                passed++;
            }
        });

        GD.Print("Success: " + passed);
        GD.Print("Failed: " + failed);

        CallDeferred(nameof(Quit));
    }

    void Quit()
    {
        GetTree().Quit();
    }

}
