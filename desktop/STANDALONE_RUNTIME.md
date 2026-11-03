# Standalone Petz runtime: extraction plan

## Correct target

A self-contained pet simulation and renderer that can run inside a bounded window without starting Petz 5. Later the bounds can become the Windows desktop. The current named-pipe marker prototype is not this target and should not be mistaken for an extracted engine.

## What the repository actually contains

PetzA is a Delphi **extension** that hooks the proprietary Petz executable, not the Petz 5 engine source. Evidence in `source/petzclassesunit.pas`:

- `TPetzPetSprite` wraps game-owned instances, with fields/accessors including `id`, `name`, `stateflag`, `GoalManager` and `scriptstack`.
- `petzclassesman.findclassinstances(cnpetsprite, ...)` enumerates hooked game instances.
- `mypetzapp_dodrawframe` calls `rimports.petmodule_dodrawframe` and game drawing-port functions.
- `alposprite_getgrabrect` is an imported game function; it is not a portable sprite renderer.
- `source/PetzA.dpr` is built as a Petz extension and checks the game executable identity.

Thus copying PetzA units into a new process will not provide working pet animation or AI. A standalone implementation requires either (a) a separate renderer/simulation capable of reading original Petz data or (b) a carefully isolated engine-host process that still depends on the original executable. The latter is **not** self-contained.

## Implementation boundary

`DesktopPetz.Sandbox` is now a bounded Windows executable with separate `IPetSimulation` and `IPetRenderer` contracts. It intentionally draws a placeholder circle and uses placeholder movement; it does **not** render an original Petz animal or reproduce original AI.

## Extraction investigation milestones

1. Identify original pet asset/container formats, skeleton/ball geometry, textures, animation and pose data. Establish lawful local asset loading from the user's Petz installation, without bundling proprietary assets.
2. Produce a file-based parser and test fixtures for one pet. Validate skeleton transforms and render a real still pose **without** launching Petz 5.
3. Implement animation evaluation and drawing for that pet inside the bounded sandbox.
4. Recover/replicate pet state machines, needs, goals, interactions, physics and navigation. Use the existing PetzA wrappers as behavioral clues, not as standalone engine code.
5. Integrate simulation with animation and bounded collisions; only then consider desktop/window interactions.

Do not claim full extraction until both rendering and behavior run without the Petz executable. No Windows or Delphi runtime tests have been performed.
