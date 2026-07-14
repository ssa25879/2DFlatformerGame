using NUnit.Framework;

public class IntroControllerEditModeTests
{
    [Test]
    public void IsContinueInputPressedForTest_UsesMouseButtonsAndSpace()
    {
        Assert.IsTrue(IntroController.IsContinueInputPressedForTest(true, false, false));
        Assert.IsTrue(IntroController.IsContinueInputPressedForTest(false, true, false));
        Assert.IsTrue(IntroController.IsContinueInputPressedForTest(false, false, true));
        Assert.IsFalse(IntroController.IsContinueInputPressedForTest(false, false, false));
    }
}
