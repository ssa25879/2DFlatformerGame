using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorControllerEditModeTests
{
    private const string ControllerPath = "Assets/Animations/Player.controller";

    [Test]
    public void PlayerAnimatorController_HasDashPlatformerStates()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        string[] stateNames = controller.layers[0].stateMachine.states
            .Select(child => child.state.name)
            .ToArray();

        CollectionAssert.AreEquivalent(
            new[] { "Idle", "Run", "Dash", "Fall", "Die" },
            stateNames);
    }

    [Test]
    public void PlayerAnimatorController_UsesIdleAsDefaultState()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        Assert.AreEqual("Idle", controller.layers[0].stateMachine.defaultState.name);
    }

    [Test]
    public void PlayerAnimatorController_HasDashPlatformerParameters()
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

        string[] parameterNames = controller.parameters
            .Select(parameter => parameter.name)
            .ToArray();

        CollectionAssert.IsSubsetOf(
            new[] { "Grounded", "IsDashing", "Speed", "VerticalSpeed", "Die" },
            parameterNames);
    }
}
