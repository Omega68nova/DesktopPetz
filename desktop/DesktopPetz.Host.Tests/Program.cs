using System.Drawing;
using DesktopPetz.Host;

static void Near(float actual, float expected)
{
    if (Math.Abs(actual - expected) > 0.01f)
        throw new Exception($"Expected {expected}, got {actual}");
}
var desktop = new Rectangle(-1920, -200, 3840, 1280);
var source = new RectangleF(100, 50, 800, 600);
var leftTop = DesktopCoordinates.Map(100, 50, source, desktop);
Near(leftTop.X, -1920); Near(leftTop.Y, -200);
var middle = DesktopCoordinates.Map(500, 350, source, desktop);
Near(middle.X, 0); Near(middle.Y, 440);
var bottomRight = DesktopCoordinates.Map(900, 650, source, desktop);
Near(bottomRight.X, 1920); Near(bottomRight.Y, 1080);
var clamped = DesktopCoordinates.Map(-999, 9999, source, desktop);
Near(clamped.X, -1920); Near(clamped.Y, 1080);
var world = new DesktopWorld(desktop);
var moved = world.Move(new PointF(-100, 0), new PointF(-1000, 200), 1);
Near(moved.X, -1100); Near(moved.Y, 200);
var edge = world.Move(new PointF(1919, 1079), new PointF(100, 100), 1);
Near(edge.X, 1919); Near(edge.Y, 1079);
Console.WriteLine("Desktop coordinate mapping checks passed.");

var ai = new DesktopBehavior(desktop, 100);
ai.Observe(new[] { new PetState("a", "A", -100, 0, null) });
if (!ai.Command(new DesktopPetCommand("a", "move", -500, 200)))
    throw new Exception("Move command failed");
var movedPet = ai.Tick(1).Single();
Near(movedPet.Position.X, -189.44272f);
Near(movedPet.Position.Y, 44.72136f);
ai.Observe(new[] { new PetState("a", "B", 1000, 1000, null) });
Near(ai.Tick(0).Single().Position.X, movedPet.Position.X);
if (ai.Command(new DesktopPetCommand("a", "move", float.NaN, 1)))
    throw new Exception("Invalid move accepted");
Console.WriteLine("Desktop behavior checks passed.");
