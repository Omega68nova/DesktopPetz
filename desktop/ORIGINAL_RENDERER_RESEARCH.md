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

## Additional public-source feasibility findings (2026-11-03)

- PetzA upstream explicitly documents running its debugger with the Petz executable as the **Host Application**, corroborating in-process integration rather than a separate callable rendering DLL: https://github.com/thenickdude/PetzA
- The PetzA developer distributes a **replacement Dogz 5 executable**, separately from PetzA, as a compatibility aid: https://www.sherlocksoftware.org/page.php?id=7 . This reinforces the importance of matching exact executable versions/addresses; it does not itself prove renderer location.
- Animation BHD/BHT files are resources inside Dogz/Catz 5 Rez.dll: https://github.com/thenickdude/petz-file-formats/blob/master/animations/Readme.md . They are animation data, not evidence of an exported renderer.
- PetzNET parses breed .cat/.dog files as PE resource containers: https://github.com/faebots/PetzNET . PE formatting alone does not imply executable rendering functions or reusable exports.
- The PetzA draw hook accesses absolute global addresses for the stage and screen and calls instance methods via the game's function-address table. Rehosting will require more than importing DLL functions: reconstructing initialization, object lifetimes, data loading, graphics context, and application dependencies.

### Decision gate and minimal experiment

1. Inspect the exact Petz 5 EXE and original DLLs with a passive PE analyzer. Record SHA-256 and 32-bit architecture, import/export directories and module names.
2. Map `petmodule_dodrawframe`, `petzapp_preparedrawframe`, screen-port creation and class construction to their owning modules and call graphs. Confirm whether they are internal EXE routines or callable DLL exports.
3. Identify renderer initialization and globals; count dependencies and external side effects. A call to a method on a game-owned object cannot be reused without creating that object correctly.
4. Only if initialization looks bounded: create a disposable isolated 32-bit process that loads the required original modules and draws one frame offscreen. Do not run this in the user's main Petz installation or alter saves.
5. **Stop condition:** if original engine functions cannot be initialized outside the Petz executable without reproducing most of its application startup, treat direct renderer reuse as impractical and report that outcome before any more desktop AI work.

No Petz 5 binary is present in the repository. Therefore steps 1–5 are **not yet experimentally verified**, and the correct answer is *undetermined*, not *viable* or *impossible*.
