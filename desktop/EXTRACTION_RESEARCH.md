# Petz 5 standalone extraction — initial source and format analysis

Status: research only. No original Petz AI or renderer has yet been extracted.

## Primary finding

PetzA is a DLL-style extension and imports game functions, so its Delphi units are not a standalone engine. The source wrapper `TPetzPetSprite`, `GoalManager`, `scriptstack`, `stateflag` and `mypetzapp_dodrawframe` provide investigation anchors, but PetzA's draw hook delegates to Petz 5. The current desktop marker bridge is therefore not a foundation for extracting original animation/AI.

## External leads (verify implementation against real Petz 5 files)

1. https://github.com/thenickdude/petz-file-formats — BHD/BHT animation decoder and browser viewer. Animations are resources in `Resources/Dogz 5 Rez.dll` or `Catz 5 Rez.dll`. BHD indexes frame offsets, counts and ball sizes; BHT contains per-frame 3D ball positions plus partly unknown per-ball fields. First priority: port decoder with test vectors and pose visualization.
2. https://github.com/faebots/PetzNET — MIT-licensed .NET parser with .cat/.dog, LNZ, RCData support and version detection. It does not claim a full renderer or .pet parser. Evaluate APIs and licensing before adopting it.
3. https://www.gamedeveloper.com/design/gdc-classic-multiple-character-interaction-in-i-petz-i-games — developer account of behavior/animation architecture: goal/plans, biorhythms, personality, constrained randomness, layered animations.
4. https://homebody.eu/carolyn/downloabx/tutorials/tutorialbits/Summary.htm — LNZ field inventory including Ballz Info, Linez, Paint Ballz, Add Ball, texture list, draw-order and scale controls.

## Rendering dependency order

- Read a locally supplied Petz 5 breed file and its LNZ (geometry/color/texture configuration).
- Read BHD/BHT from locally installed resource DLL; establish skeleton index agreement with breed LNZ.
- Render an untextured static pose of a known breed, with ball and line ordering.
- Reproduce color palette, paintballz, texture sampling, depth sorting, addballz and fur details.
- Interpolate animations, apply head/body overlays, compare output against original screenshots.
- Integrate pose selection with sandbox simulation.

The original assets are proprietary. Do not commit or redistribute game DLLs, adopted pets, textures or breed resources; load from a user's installation.

## AI dependency order

- Inventory PetzA wrappers for accessible goals, biorhythm, state and script stack.
- Determine which AI data is serialized in adopted pet files versus runtime-only game state.
- Build a standalone state model (needs, personality, goals, emotion, action, relationship, stimuli).
- Validate action and animation selection against recorded original-game traces where available.
- Keep exact behavior reconstruction distinct from a new approximate policy. No evidence yet that original compiled decision code can be extracted into a standalone library.

## Initial validation gate

Before claiming standalone Petz rendering: the bounded sandbox must load one locally supplied breed and corresponding animation resources and draw one recognizably correct original pet pose, with no Petz 5 process running. The existing placeholder sandbox does not meet this gate.

Before claiming standalone AI: demonstrate need/goal-driven action selection and animation changes without Petz 5 running.

## Risks

- BHT per-ball fields remain partly undocumented.
- Breed LNZ and animation skeleton indexing may require species-specific interpretation.
- Render fidelity includes texture/palette/depth/overlays, not just circles.
- Petz 5's original AI logic is proprietary binary code; a compatible reimplementation is a separate, potentially long investigation.
- This research has not yet been verified against a user's actual Petz 5 installation.
