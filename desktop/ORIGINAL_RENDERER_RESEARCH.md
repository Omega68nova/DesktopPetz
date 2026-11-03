# Original Petz renderer reuse: feasibility investigation

## Confirmed from PetzA source

In `source/petzclassesunit.pas`, `mypetzapp_dodrawframe` calls:

1. `petzapp_preparedrawframe`
2. `xdrawport_openscreendrawport`
3. `petmodule_dodrawframe` (with the game-owned `petmodule` instance)
4. `xdrawport_closescreendrawport`

The unit also exposes `getxstage`, `getxscreen`, and a `displayport` helper using `xdrawport_getbits`. Petz 5 global pointers are read at absolute addresses `$668E30` (stage) and `$663500` (screen), demonstrating this integration is tied to a specific game executable layout.

These are **not DLL exports**. They are imported function addresses / instance methods accessed through PetzA's reverse-engineered import table. The fact that PetzA can call them *inside Petz 5* does not prove they can be invoked from a standalone process.

## Options under investigation

A. Original renderer inside a minimal independent host: determine whether Petz 5's render code resides in a separately loadable module, and enumerate required initialization, heaps, global state, data paths, graphics ports and message loop. This is the desired path, but currently unproven.

B. Original engine hosted in a bounded Petz process: isolate drawing output from its existing stage into a separate window or capture surface. Preserves exact rendering and simulation but does not yet meet the requirement of standalone independence. Useful as a diagnostic stepping stone.

C. Resource-based reimplementation: only if A is infeasible; not the current priority.

## First executable inspection

The new `desktop/DesktopPetz.BinarySurvey` project performs **passive PE header inspection** of local EXE/DLL files, printing architecture, entrypoint, import/export/resource directory locations and sections. It does not load or execute the binaries, change files, or bundle game assets.

`dotnet run --project desktop/DesktopPetz.BinarySurvey -- "C:\\Path\\Petz 5.exe" "C:\\Path\\Resources\\Dogz 5 Rez.dll"`

Next step is to extend this to enumerate import/export tables and map known PetzA call addresses to exact executable sections, then trace the render function's dependencies with a disassembler and debugger.

## Decision gate

We can claim original renderer reuse in an independent host **only after** drawing a real pet frame into a bounded test window with the original game executable *not running*. Capturing a Petz frame from an already running game is useful instrumentation but is not proof of standalone reuse.

Current status: identified internal entry points and created an executable survey tool. No binary has been supplied or analyzed, and no renderer isolation has been demonstrated. The sandbox still renders placeholders.
