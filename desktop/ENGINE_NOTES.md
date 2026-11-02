# PetzA engine reconnaissance

This note records **verified source-level observations**, not claims of runtime testing.

## Existing extension points

- `source/PetzA.dpr` builds a Delphi library with the `.toy` extension and includes `petzaunit`, `petzclassesunit`, and `dllpatchunit`.
- `source/petzclassesunit.pas` declares `TPetzPetSprite` with `id`, `name`, `GoalManager`, `scriptstack`, `getbiorhythm`, `interactingpet`, and `stateflag`. This is a strong candidate for **read-only diagnostics** once lifetime and enumeration are understood.
- `source/petzacommsunit.pas` contains an older socket-based communications implementation and a `TPetzCommPushScript` message. Do not assume it is currently wired into the library.
- PetzA's entry point performs executable hash/version checks. Any future hook must remain behind this validation.

## Safety rules for the native bridge

1. No hard-coded offsets without a verified Petz 5 executable hash.
2. No raw pet pointers across IPC. Serialize copies of primitive values.
3. Read pet state only on a proven safe game-thread callback; never traverse game-owned collections from a worker thread.
4. Keep experimental hooks disabled by default, and never write pet save files.
5. Treat external desktop behavior as a separate policy layer; original AI can be extended or replaced deliberately.

## Current status

The .NET host and synthetic pipe producer can exercise the protocol, but there is **no native Petz 5 snapshot publisher yet**. The first native task is to locate safe pet enumeration and callback lifetimes in `petzaunit.pas` and `petzclassesunit.pas`.

## Experimental live Petz 5 diagnostics (new)

The existing `mypetzapp_dodrawframe` hook now invokes `DesktopPetzDiagnosticSnapshot` after the original pet module draw. This is an **opt-in**, read-only diagnostic for the English Petz 5 executable only.

Set `DESKTOPPETZ_DIAGNOSTICS=1` in the environment **before launching Petz 5**, then use a Windows debugger or Sysinternals DebugView to capture `OutputDebugString` messages. Expected format:

```text
DesktopPetz pet id=123 name=Example rect=10,20,100,150
```

The diagnostic uses PetzA's existing tracked `cnpetsprite` instances and `alposprite_getgrabrect` accessor, throttled to one scan per second. Coordinates are game-scene rectangles, **not** Windows desktop coordinates. It neither transmits over the named pipe nor alters the pet AI.

**Not yet compiled or tested against Petz 5.** If this diagnostic causes instability, unset the environment variable. Future work should move snapshots into a bounded queue and publish asynchronously, after confirming hook threading and pet lifetime.
