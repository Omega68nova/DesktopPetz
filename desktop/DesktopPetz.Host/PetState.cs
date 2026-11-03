namespace DesktopPetz.Host;

// Raw observations from the Petz simulation; coordinates retain the frame's space.
public sealed record PetState(string Id, string Name, float X, float Y,
    string? Animation, uint? StateFlag = null, string? InteractingPetId = null);
