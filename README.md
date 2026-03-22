# Rubik's Cube — CodeGamified

**3×3 Rubik's Cube with code-controlled face rotations.**

Students write code to rotate faces and solve scrambled cubes. The challenge
escalates from random moves to layer-by-layer algorithms to optimal solvers.

## Quick Start

1. Open `rubik/Rubik/` in Unity (2022.3+, URP)
2. Open `Assets/Scenes/SampleScene.unity`
3. Attach `RubikBootstrap` to a GameObject
4. Press Play — cube appears scrambled, default script runs

## Architecture

```
Assets/
├── Core/       RubikBootstrap, RubikSimulationTime
├── Game/       RubikCube, RubikMatchManager, RubikRenderer
├── Scripting/  RubikCompilerExtension, RubikIOHandler, RubikProgram,
│               RubikEditorExtension, RubikInputProvider
└── UI/         RubikTUIManager, RubikStatusPanel, RubikCodeDebugger, RubikDebuggerData
```

## Cube Layout

- **6 faces**: U(White), D(Yellow), F(Green), B(Blue), L(Orange), R(Red)
- **9 stickers per face** (row-major): `0 1 2 / 3 4 5 / 6 7 8`
- Center sticker (4) never moves — defines face color

## Opcodes (23 total)

### Queries (10)
| Function | Args | Returns |
|----------|------|---------|
| `get_solved()` | — | 1 if entire cube solved |
| `get_solved_faces()` | — | count of solved faces (0-6) |
| `get_solved_stickers()` | — | correct stickers (0-54) |
| `get_moves()` | — | moves this solve |
| `get_scramble_length()` | — | scramble move count |
| `get_sticker(face, pos)` | face 0-5, pos 0-8 | color 0-5 |
| `get_face_solved(face)` | face 0-5 | 0 or 1 |
| `get_score()` | — | current score |
| `get_game_over()` | — | 0 or 1 |
| `get_input()` | — | keyboard code |

### Commands (13)
| Function | Effect |
|----------|--------|
| `rotate_u()` / `rotate_u_prime()` | U face CW / CCW |
| `rotate_d()` / `rotate_d_prime()` | D face CW / CCW |
| `rotate_f()` / `rotate_f_prime()` | F face CW / CCW |
| `rotate_b()` / `rotate_b_prime()` | B face CW / CCW |
| `rotate_l()` / `rotate_l_prime()` | L face CW / CCW |
| `rotate_r()` / `rotate_r_prime()` | R face CW / CCW |
| `rotate(face, dir)` | Generic (face 0-5, dir 1=CW/-1=CCW) |

## Keyboard Controls (User Mode)

| Key | Move | Key | Move |
|-----|------|-----|------|
| U | U  | I | U' |
| J | D  | K | D' |
| F | F  | G | F' |
| V | B  | B | B' |
| D | L  | E | L' |
| H | R  | Y | R' |

## Script Presets

Press **[1]-[5]** or click buttons in the right panel:

1. **Easy** — Random face rotations
2. **Medium** — White cross solver heuristics
3. **Hard** — Layer-by-layer approach (sexy move, Sune, etc.)
4. **Keyboard** — Manual play via key bindings
5. **Reset** — Restore default starter code

## Scoring

- +1 per sticker in correct position (max 54)
- +10 per fully solved face (max 60)
- +100 for complete solve
- Move efficiency bonus: `max(0, 200 - moves×2)`
