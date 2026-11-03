using System.Drawing;

namespace DesktopPetz.Host;

/// <summary>
/// Maps legacy playscene positions to the full virtual desktop. The desktop world
/// is authoritative for future AI; legacy coordinates are only an input adapter.
/// </summary>
public static class DesktopCoordinates
{
    public static PointF Map(float x, float y, RectangleF source, Rectangle desktop)
    {
        if (source.Width <= 0 || source.Height <= 0 ||
            desktop.Width <= 0 || desktop.Height <= 0 ||
            !float.IsFinite(x) || !float.IsFinite(y))
            throw new ArgumentException("Invalid coordinate space.");
        float nx = Math.Clamp((x - source.Left) / source.Width, 0f, 1f);
        float ny = Math.Clamp((y - source.Top) / source.Height, 0f, 1f);
        return new PointF(desktop.Left + nx * desktop.Width,
                          desktop.Top + ny * desktop.Height);
    }

    public static PointF ClampToDesktop(float x, float y, Rectangle desktop) =>
        new(Math.Clamp(x, desktop.Left, desktop.Right - 1),
            Math.Clamp(y, desktop.Top, desktop.Bottom - 1));

    public static RectangleF ParseSource(string? value)
    {
        // Explicit, configurable source rectangle: left,top,width,height.
        // No guessed hardcoded Petz 5 resolution.
        var parts = value?.Split(',');
        if (parts?.Length != 4 ||
            !float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var y) ||
            !float.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var w) ||
            !float.TryParse(parts[3], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var h) ||
            w <= 0 || h <= 0)
            throw new ArgumentException("Set DESKTOPPETZ_SOURCE_RECT=left,top,width,height for legacy playscene mapping.");
        return new RectangleF(x, y, w, h);
    }
}
