# DesktopPetz — first development slice

This directory contains an **experimental Windows diagnostic overlay**, not yet a working Petz 5 desktop pet. The original PetzA code remains untouched.

## Build

Requires Windows and .NET 8 SDK:

```powershell
dotnet run --project desktop/DesktopPetz.Host/DesktopPetz.Host.csproj
```

The overlay is transparent, click-through, and displays markers for pet coordinates received through a local named pipe. Exit via Task Manager or Ctrl+C when run from a terminal; a tray menu and graceful UI exit are planned.

## State bridge contract (v1)

Pipe: `DesktopPetz.State.v1`, newline-delimited UTF-8 JSON, one snapshot per line.

```json
{"version":1,"pets":[{"id":"test","name":"Test pet","x":400,"y":500,"animation":"idle"}]}
```

Coordinates are absolute Windows virtual-screen pixels (including negative coordinates on secondary monitors). `animation` is reserved for later rendering. The host never modifies pet files and currently has **no connection to PetzA**.

## Next steps

1. Identify version-gated, read-only pet enumeration hooks in the Delphi PetzA source.
2. Publish a safe snapshot of pet identifiers, positions, and animation states.
3. Translate Petz playscene coordinates into desktop coordinates.
4. Replace markers with actual transparent pet rendering.
5. Introduce an extensible behavior layer: original behavior is a useful foundation, not an immutable constraint.

Do not distribute game assets or executables with this project. Back up adopted pet files before testing native hooks.
