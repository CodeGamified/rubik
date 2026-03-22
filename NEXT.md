# Next — Game Pitches

## Rubik's — Puzzle

**3D cube manipulation with scramble-and-solve progression.**

- **Code loop**: Students write `rotate(face, direction)` commands and immediately see the cube transform. The challenge escalates from "fix one face" to "solve optimally in N moves."
- **Depth gradient**: Level 1 — brute-force random rotations. Level 2 — layer-by-layer (beginner method, ~100 moves). Level 3 — CFOP (intuitive F2L, OLL, PLL — ~55 moves). Level 4 — Kociemba two-phase (~22 moves). Level 5 — God's number research (provably ≤20 moves).
- **Visual payoff**: Smooth 3D rotation animations, move-count scoreboard, color-coded state diff showing progress, replay of solve path at speed. The 3D cube is inherently visually striking.
- **Teaching concepts**: State representation (array encoding of 54 stickers), group theory basics, search algorithms, heuristic design, recursion.
