# Desktop-coordinate AI prototype

The host can own pet positions in absolute Windows virtual-screen pixels, independently of Petz 5's playscene. Set `DESKTOPPETZ_DESKTOP_AI=1` before starting the host to enable this experimental controller.

The native bridge still supplies pet identities and initial spawn positions; subsequent legacy positions do not override desktop movement. The `DesktopBehavior` controller provides deterministic wandering, velocity-based movement, explicit `move` and `idle` commands, and screen-bound clamping. Commands currently exist as an internal API; no UI or external command transport is connected.

This does not yet issue Petz 5 AI actions or select real animation frames. The overlay shows diagnostic markers moving independently of legacy coordinates. The original game can continue simulating its internal state while DesktopPetz owns the desktop position, but synchronization and real sprite rendering remain future work.

Limitations: rectangular virtual-desktop bounds can include gaps between monitors; window collisions, gravity, pathfinding, and DPI handling are not implemented. The policy is deterministic, not learned AI.

Run `dotnet run --project desktop/DesktopPetz.Host.Tests` to exercise coordinate and movement checks without Petz 5. Windows runtime and native Delphi compilation are still unverified.

## Native Petz AI to desktop marker bridge

PetzA now includes a raw `stateFlag` in its pet snapshots. The desktop controller accepts incoming pet position observations, computes bounded movement deltas, and applies those deltas to the desktop pet's existing position. It does **not** replace the desktop position with the original stage coordinate on every update. A short hold after observed native movement avoids competing autonomous wandering; when the native pet stops moving, desktop wandering resumes.

A displacement greater than 240 desktop pixels per observation is treated as a teleport/outlier and ignored. The `stateFlag` is retained as an opaque native observation: its bits have not yet been reverse-engineered, so it must not be interpreted as a particular mood or action. Existing source rectangle mapping is still required for playscene frames.

This provides **Petz simulation -> desktop marker** communication, not full bidirectional AI control. `DesktopPetCommand` currently controls the desktop-side movement policy only; no native command pipe or safe Petz AI command dispatch exists yet. Also, marker activity is not a real Petz animation. No runtime build or Windows integration test has been performed.
