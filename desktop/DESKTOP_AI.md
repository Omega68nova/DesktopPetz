# Desktop-coordinate AI prototype

The host can own pet positions in absolute Windows virtual-screen pixels, independently of Petz 5's playscene. Set `DESKTOPPETZ_DESKTOP_AI=1` before starting the host to enable this experimental controller.

The native bridge still supplies pet identities and initial spawn positions; subsequent legacy positions do not override desktop movement. The `DesktopBehavior` controller provides deterministic wandering, velocity-based movement, explicit `move` and `idle` commands, and screen-bound clamping. Commands currently exist as an internal API; no UI or external command transport is connected.

This does not yet issue Petz 5 AI actions or select real animation frames. The overlay shows diagnostic markers moving independently of legacy coordinates. The original game can continue simulating its internal state while DesktopPetz owns the desktop position, but synchronization and real sprite rendering remain future work.

Limitations: rectangular virtual-desktop bounds can include gaps between monitors; window collisions, gravity, pathfinding, and DPI handling are not implemented. The policy is deterministic, not learned AI.

Run `dotnet run --project desktop/DesktopPetz.Host.Tests` to exercise coordinate and movement checks without Petz 5. Windows runtime and native Delphi compilation are still unverified.
