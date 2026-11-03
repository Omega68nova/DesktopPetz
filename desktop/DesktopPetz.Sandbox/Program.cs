using System.Drawing;
using System.Windows.Forms;

namespace DesktopPetz.Sandbox;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new SandboxWindow());
    }
}

// Intentionally not a claim of original Petz rendering or AI extraction.
// A bounded executable harness with explicit simulation and renderer boundaries.
public readonly record struct PetPose(float X, float Y, string Action);

public interface IPetSimulation
{
    PetPose Current { get; }
    void Advance(float seconds);
}

public interface IPetRenderer
{
    void Draw(Graphics graphics, PetPose pose);
}

public sealed class SandboxWindow : Form
{
    private readonly System.Windows.Forms.Timer clock = new() { Interval = 16 };
    private readonly IPetSimulation simulation;
    private readonly IPetRenderer renderer;
    private DateTime previous = DateTime.UtcNow;

    public SandboxWindow()
    {
        Text = "DesktopPetz - bounded runtime research harness";
        ClientSize = new Size(800, 500);
        MinimumSize = new Size(320, 240);
        DoubleBuffered = true;
        simulation = new PlaceholderSimulation(() => ClientSize);
        renderer = new PlaceholderRenderer();
        clock.Tick += (_, _) =>
        {
            var now = DateTime.UtcNow;
            var seconds = Math.Clamp((float)(now - previous).TotalSeconds, 0, 0.05f);
            previous = now;
            simulation.Advance(seconds);
            Invalidate();
        };
        clock.Start();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        renderer.Draw(e.Graphics, simulation.Current);
        e.Graphics.DrawString("Placeholder only: original Petz renderer/AI not extracted",
            Font, Brushes.Gray, 10, 10);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        clock.Stop();
        clock.Dispose();
        base.OnFormClosed(e);
    }
}

internal sealed class PlaceholderSimulation : IPetSimulation
{
    private readonly Func<Size> bounds;
    private float x = 100;
    private float direction = 1;
    public PlaceholderSimulation(Func<Size> bounds) => this.bounds = bounds;
    public PetPose Current => new(x, Math.Max(40, bounds().Height - 60), "walking");
    public void Advance(float seconds)
    {
        x += direction * 90 * seconds;
        if (x > Math.Max(25, bounds().Width - 25))
        {
            x = Math.Max(25, bounds().Width - 25);
            direction = -1;
        }
        if (x < 25) { x = 25; direction = 1; }
    }
}

internal sealed class PlaceholderRenderer : IPetRenderer
{
    public void Draw(Graphics graphics, PetPose pose)
    {
        graphics.FillEllipse(Brushes.CornflowerBlue, pose.X - 12, pose.Y - 24, 24, 24);
        graphics.DrawString(pose.Action, SystemFonts.DefaultFont, Brushes.Black,
            pose.X + 15, pose.Y - 24);
    }
}
