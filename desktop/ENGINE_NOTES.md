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
