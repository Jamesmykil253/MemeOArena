using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Tests that the locomotion controller reads input from an injected input source rather than UnityEngine.Input.
/// </summary>
public class InputSourceTests
{
    private class TestInputSource : IInputSource
    {
        public float Horizontal;
        public bool JumpDown;
        public float GetAxis(string axisName) => Horizontal;
        public bool GetButtonDown(string buttonName) => JumpDown;
        public bool GetButton(string buttonName) => false;
    }

    [Test]
    public void LocomotionReadsInjectedInput()
    {
        var go = new GameObject("Player");
        var controller = go.AddComponent<LocomotionController>();
        var input = new TestInputSource { Horizontal = 1f, JumpDown = true };
        controller.Initialize(input);
        controller.Update();
        Assert.AreEqual(1f, controller.DesiredVelocity.x, 1e-4f);
    }
}