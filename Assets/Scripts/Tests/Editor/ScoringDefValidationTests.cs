using NUnit.Framework;
using System;
using UnityEngine;

/// <summary>
/// Tests for the ScoringDef validation logic. Ensures misconfigured assets throw exceptions.
/// </summary>
public class ScoringDefValidationTests
{
    [Test]
    public void ThrowsWhenLengthsDiffer()
    {
        var def = ScriptableObject.CreateInstance<ScoringDef>();
        def.thresholds = new int[] { 1, 10 };
        def.baseTimes = new float[] { 0.5f };
        Assert.Throws<InvalidOperationException>(() => def.OnValidate());
    }

    [Test]
    public void ThrowsWhenThresholdsDescend()
    {
        var def = ScriptableObject.CreateInstance<ScoringDef>();
        def.thresholds = new int[] { 10, 5 };
        def.baseTimes = new float[] { 1f, 2f };
        Assert.Throws<InvalidOperationException>(() => def.OnValidate());
    }
}