using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO.Pipes;
using System.Text.Json;
using System.Windows.Forms;

namespace DesktopPetz.Host;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new DesktopOverlay());
    }
}

// Diagnostic protocol only. PetzA must explicitly publish live pet states in a later phase.
public sealed record BridgeFrame(int Version, List<PetState> Pets, string? Space = null);

internal sealed class DesktopOverlay : Form
{
    private readonly System.Windows.Forms.Timer repaint = new() { Interval = 33 };
    private readonly CancellationTokenSource shutdown = new();
    private readonly object gate = new();
    private List<PetState> pets = [];
    private readonly bool desktopAiEnabled = Environment.GetEnvironmentVariable("DESKTOPPETZ_DESKTOP_AI") == "1";
    private readonly DesktopBehavior desktopAi = new(SystemInformation.VirtualScreen);
    private DateTime lastTick = DateTime.UtcNow;
    private readonly RectangleF? sourceRect = LoadSourceRect();

    private static RectangleF? LoadSourceRect()
    {
        var value = Environment.GetEnvironmentVariable("DESKTOPPETZ_SOURCE_RECT");
        return string.IsNullOrWhiteSpace(value) ? null : DesktopCoordinates.ParseSource(value);
    }
    private readonly Font labelFont = new("Segoe UI", 10, FontStyle.Bold);
    private readonly Rectangle desktop = SystemInformation.VirtualScreen;

    public DesktopOverlay()
    {
        Text = "DesktopPetz diagnostic overlay";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        Bounds = desktop;
        ShowInTaskbar = false;
        TopMost = true;
        BackColor = Color.Magenta;
        TransparencyKey = Color.Magenta;
        DoubleBuffered = true;
        // Do not capture clicks: the diagnostic overlay must not interfere with desktop use.
        repaint.Tick += (_, _) =>
        {
            if (desktopAiEnabled)
            {
                var now = DateTime.UtcNow;
                var elapsed = (float)(now - lastTick).TotalSeconds;
                lastTick = now;
                lock (gate)
                {
                    var poses = desktopAi.Tick(Math.Clamp(elapsed, 0f, 1f));
                    pets = poses.Select(p => new PetState(p.Id, p.Name,
                        p.Position.X, p.Position.Y, p.Activity)).ToList();
                }
            }
            Invalidate();
        };
        repaint.Start();
        _ = Task.Run(() => ReadBridgeAsync(shutdown.Token));
    }

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x00000080 | 0x00000020; // WS_EX_TOOLWINDOW | WS_EX_TRANSPARENT
            return cp;
        }
    }

    private async Task ReadBridgeAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", "DesktopPetz.State.v1", PipeDirection.In);
                await pipe.ConnectAsync(1000, token);
                using var reader = new StreamReader(pipe);
                while (!token.IsCancellationRequested && pipe.IsConnected)
                {
                    var line = await reader.ReadLineAsync(token);
                    if (line is null) break;
                    if (line.Length > 1_000_000) continue;
                    try
                    {
                        var frame = JsonSerializer.Deserialize<BridgeFrame>(line,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (frame is null || frame.Version != 1 || frame.Pets is null || frame.Pets.Count > 64) continue;
                        // Explicit spaces prevent silently treating Petz playscene coordinates
                        // as desktop pixels. No guessed default stage size.
                        if (frame.Space is not (null or "desktop" or "playscene")) continue;
                        if (frame.Space == "playscene" && sourceRect is null) continue;
                        var mapped = frame.Space == "playscene"
                            ? frame.Pets.Select(p =>
                            {
                                var point = DesktopCoordinates.Map(p.X, p.Y, sourceRect!.Value, desktop);
                                return p with { X = point.X, Y = point.Y };
                            }).ToList()
                            : frame.Pets;
                        lock (gate)
                        {
                            if (desktopAiEnabled) desktopAi.Observe(mapped);
                            else pets = mapped;
                        }
                    }
                    catch (JsonException) { /* Ignore malformed diagnostics. */ }
                }
            }
            catch (OperationCanceledException) { break; }
            catch (IOException) { }
            catch (TimeoutException) { }
            // Do not spin while the game/bridge is absent.
            try { await Task.Delay(500, token); } catch (OperationCanceledException) { break; }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        List<PetState> snapshot;
        lock (gate) snapshot = [.. pets];
        foreach (var pet in snapshot)
        {
            if (!float.IsFinite(pet.X) || !float.IsFinite(pet.Y)) continue;
            // Marker, NOT a Petz renderer. Coordinates are absolute virtual-desktop pixels.
            float x = pet.X - desktop.Left, y = pet.Y - desktop.Top;
            e.Graphics.FillEllipse(Brushes.CornflowerBlue, x - 10, y - 10, 20, 20);
            e.Graphics.DrawString(pet.Name ?? pet.Id, labelFont, Brushes.White, x + 14, y - 10);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        shutdown.Cancel();
        repaint.Dispose();
        labelFont.Dispose();
        shutdown.Dispose();
        base.OnFormClosed(e);
    }
}
