using System.Drawing;

namespace DesktopPetz.Host;

public sealed record DesktopPetCommand(string PetId, string Kind, float? X = null, float? Y = null);
public sealed record DesktopPetPose(string Id, string Name, PointF Position, string Activity);

public sealed class DesktopBehavior
{
    private sealed class Actor
    {
        public required string Id;
        public required string Name;
        public PointF Position;
        public PointF Target;
        public string Activity = "idle";
        public float IdleSeconds;
        public int Step;
        public PointF LastObserved;
        public bool HasObservation;
        public float NativeMotionHold;
        public uint? NativeStateFlag;
    }

    private readonly Dictionary<string, Actor> actors = new(StringComparer.Ordinal);
    private readonly DesktopWorld world;
    private readonly float speed;

    public DesktopBehavior(Rectangle bounds, float speedPixelsPerSecond = 90)
    {
        if (!float.IsFinite(speedPixelsPerSecond) || speedPixelsPerSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(speedPixelsPerSecond));
        world = new DesktopWorld(bounds);
        speed = speedPixelsPerSecond;
    }

    public void Resize(Rectangle bounds) => world.Resize(bounds);

    public void Observe(IEnumerable<PetState> pets)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var pet in pets)
        {
            if (string.IsNullOrWhiteSpace(pet.Id) || !float.IsFinite(pet.X) ||
                !float.IsFinite(pet.Y) || !seen.Add(pet.Id)) continue;
            if (actors.TryGetValue(pet.Id, out var existing))
            {
                existing.Name = pet.Name;
                existing.NativeStateFlag = pet.StateFlag;
                // Use the original simulation's movement delta, not its absolute
                // playscene position, so the desktop pet does not snap backwards.
                var observed = new PointF(pet.X, pet.Y);
                if (existing.HasObservation)
                {
                    var dx = observed.X - existing.LastObserved.X;
                    var dy = observed.Y - existing.LastObserved.Y;
                    var magnitude = MathF.Sqrt(dx * dx + dy * dy);
                    if (magnitude > 1 && magnitude <= 240)
                    {
                        existing.Position = DesktopCoordinates.ClampToDesktop(
                            existing.Position.X + dx, existing.Position.Y + dy, world.Bounds);
                        existing.Target = existing.Position;
                        existing.NativeMotionHold = 1.5f;
                        existing.Activity = "native-motion";
                    }
                }
                existing.LastObserved = observed;
                existing.HasObservation = true;
                continue;
            }
            var position = DesktopCoordinates.ClampToDesktop(pet.X, pet.Y, world.Bounds);
            actors.Add(pet.Id, new Actor
            {
                Id = pet.Id, Name = pet.Name, Position = position, Target = position,
                LastObserved = new PointF(pet.X, pet.Y), HasObservation = true,
                NativeStateFlag = pet.StateFlag
            });
        }
        foreach (var id in actors.Keys.Where(id => !seen.Contains(id)).ToArray())
            actors.Remove(id);
    }

    public bool Command(DesktopPetCommand command)
    {
        if (!actors.TryGetValue(command.PetId, out var actor)) return false;
        switch (command.Kind)
        {
            case "idle":
                actor.Target = actor.Position;
                actor.NativeMotionHold = 0;
                actor.Activity = "idle";
                actor.IdleSeconds = 0;
                return true;
            case "move":
                if (command.X is not float x || command.Y is not float y ||
                    !float.IsFinite(x) || !float.IsFinite(y)) return false;
                actor.Target = DesktopCoordinates.ClampToDesktop(x, y, world.Bounds);
                actor.NativeMotionHold = 0;
                actor.Activity = "walking";
                return true;
            default:
                return false;
        }
    }

    public IReadOnlyList<DesktopPetPose> Tick(float seconds)
    {
        if (!float.IsFinite(seconds) || seconds < 0 || seconds > 1)
            throw new ArgumentOutOfRangeException(nameof(seconds));
        foreach (var actor in actors.Values)
        {
            if (actor.NativeMotionHold > 0)
            {
                actor.NativeMotionHold = MathF.Max(0, actor.NativeMotionHold - seconds);
                continue;
            }
            var dx = actor.Target.X - actor.Position.X;
            var dy = actor.Target.Y - actor.Position.Y;
            var distance = MathF.Sqrt(dx * dx + dy * dy);
            if (distance > 0.5f)
            {
                var travel = MathF.Min(distance, speed * seconds);
                actor.Position = world.Move(actor.Position,
                    new PointF(dx / distance * speed, dy / distance * speed),
                    travel / speed);
                actor.Activity = "walking";
                actor.IdleSeconds = 0;
            }
            else
            {
                actor.Position = actor.Target;
                actor.Activity = "idle";
                actor.IdleSeconds += seconds;
                if (actor.IdleSeconds >= 2)
                {
                    actor.IdleSeconds = 0;
                    var angle = (actor.Step++ * 2.3999632f) +
                        (StableHash(actor.Id) % 360) * MathF.PI / 180;
                    actor.Target = DesktopCoordinates.ClampToDesktop(
                        actor.Position.X + MathF.Cos(angle) * 180,
                        actor.Position.Y + MathF.Sin(angle) * 120, world.Bounds);
                }
            }
        }
        return actors.Values.Select(a =>
            new DesktopPetPose(a.Id, a.Name, a.Position, a.Activity)).ToArray();
    }

    private static uint StableHash(string value)
    {
        uint hash = 2166136261;
        foreach (var c in value) hash = (hash ^ c) * 16777619;
        return hash;
    }
}
