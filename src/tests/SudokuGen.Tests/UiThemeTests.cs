using SudokuGen.Tui;

namespace SudokuGen.Tests;

// The theme is process-wide, so these tests must not overlap with any other test.
[NotInParallel]
public class UiThemeTests
{
    [Test]
    public async Task TKey_SwitchesBetweenDarkAndLightWhilePlaying()
    {
        using var f = new UiFixture();
        var dark = f.Paint().StyleAt(79, 0);

        f.Key("t");
        var light = f.Paint().StyleAt(79, 0);

        await Assert.That(Theme.Light).IsTrue();
        await Assert.That(light).IsNotEqualTo(dark);

        f.Key("t");
        await Assert.That(Theme.Light).IsFalse();
        await Assert.That(f.Paint().StyleAt(79, 0)).IsEqualTo(dark);
    }

    [Test]
    public async Task CtrlT_AndTheFooterButton_SwitchTheThemeOnTheStartScreen()
    {
        using var f = new UiFixture(startGame: false);

        f.Key("t", ctrl: true);
        await Assert.That(Theme.Light).IsTrue();

        f.ClickLabel("Theme");
        await Assert.That(Theme.Light).IsFalse();
    }

    [Test]
    public async Task TKey_SwitchesTheThemeOnTheWinScreenButTypesNothingElse()
    {
        using var f = new UiFixture();
        f.SolveWithTheLastKey();
        f.Ui.Tick();
        f.Clock.Advance(TimeSpan.FromSeconds(2));
        f.Ui.Tick();

        f.Key("t");

        await Assert.That(Theme.Light).IsTrue();
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Won);
    }

    [Test]
    public async Task TheChoice_IsRememberedForTheNextSession()
    {
        using var dir = new TempDirectory();
        var first = new Session(dir.Path, new ManualClock());
        await Assert.That(first.LightTheme).IsFalse();

        first.ToggleTheme();

        await Assert.That(new Session(dir.Path, new ManualClock()).LightTheme).IsTrue();
    }

    [Test]
    public async Task EveryLightColor_DiffersFromItsDarkCounterpartWhereTheBackgroundChanges()
    {
        var dark = new[] { Theme.Background, Theme.Text, Theme.CellLight, Theme.Selected, Theme.Given };
        Theme.Light = true;
        try
        {
            var light = new[] { Theme.Background, Theme.Text, Theme.CellLight, Theme.Selected, Theme.Given };
            await Assert.That(light.Zip(dark).All(pair => pair.First != pair.Second)).IsTrue();
        }
        finally
        {
            Theme.Light = false;
        }
    }
}
