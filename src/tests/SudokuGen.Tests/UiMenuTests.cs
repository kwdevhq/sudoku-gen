using SudokuGen.Tui;

namespace SudokuGen.Tests;

public class UiMenuTests
{
    [Test]
    public async Task Esc_OpensTheGameMenuInsteadOfQuitting_AndEscResumes()
    {
        using var f = new UiFixture();

        f.Key("Esc");

        await Assert.That(f.Ui.QuitRequested).IsFalse();
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Menu);
        await Assert.That(f.Paint().Contains("Game menu")).IsTrue();

        f.Key("x");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Menu);

        f.Key("Esc");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);

        f.Key("Esc");
        f.Key("Enter");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);
    }

    [Test]
    public async Task Menu_MainMenuAndQuitWorkByKeyAndByClick()
    {
        using var f = new UiFixture();

        f.Key("Esc");
        f.Key("m");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);

        f.Key("Esc");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);

        f.Key("Esc");
        f.ClickLabel("Resume");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Playing);

        f.Key("Esc");
        f.ClickLabel("Main menu");
        await Assert.That(f.Ui.Screen).IsEqualTo(Screen.Start);

        f.Key("Esc");
        f.Key("Esc");
        f.Key("q");
        await Assert.That(f.Ui.QuitRequested).IsTrue();
    }

    [Test]
    public async Task Menu_QuitButtonQuits()
    {
        using var f = new UiFixture();
        f.Key("Esc");

        f.ClickLabel("Quit game");

        await Assert.That(f.Ui.QuitRequested).IsTrue();
    }

    [Test]
    public async Task StartScreen_HasAQuitButtonReachableByClickAndByTabbing()
    {
        using var f = new UiFixture(startGame: false);
        f.ClickLabel("Quit");
        await Assert.That(f.Ui.QuitRequested).IsTrue();

        using var g = new UiFixture(startGame: false);
        g.Key("Tab");
        g.Key("Tab");
        g.Key("Enter");
        await Assert.That(g.Ui.QuitRequested).IsTrue();
    }
}
