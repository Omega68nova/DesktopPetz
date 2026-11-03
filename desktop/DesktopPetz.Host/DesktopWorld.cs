using System.Drawing;

namespace DesktopPetz.Host;

/// <summary>
/// Desktop-space motion, independent of the legacy Petz playscene. Intended
/// for future behavior controllers; this prototype does not inject game input.
/// </summary>
public sealed class DesktopWorld
{
    public Rectangle Bounds { get; private set; }
    public DesktopWorld(Rectangle bounds) => Resize(bounds);

    public void Resize(Rectangle bounds)
    {
        if (bounds.Width <= 0 || bounds.Height <= 0)
            throw new ArgumentException("Desktop bounds must be nonempty.", nameof(bounds));
        Bounds = bounds;
    }

    public PointF Move(PointF current, PointF velocityPixelsPerSecond, float seconds)
    {
        if (!float.IsFinite(seconds) || seconds < 0)
            throw new ArgumentOutOfRangeException(nameof(seconds));
        return DesktopCoordinates.ClampToDesktop(
            current.X + velocityPixelsPerSecond.X * seconds,
            current.Y + velocityPixelsPerSecond.Y * seconds, Bounds);
    }
}
