# Rubik's Cube — Keystone Reference Implementation Handoff

## Vision

A 3×3 (generalizable to NxN) Rubik's cube with seamless animated rotations where **animation is not a layer on top of state — it IS the state**. The cube should look and feel like a physical object. Stickers never flash, never repaint, never teleport. Cubies rotate and land in their new home.

---

## Core Principle: Cubie-Centric Architecture

The current implementation uses a **sticker-array + repaint** model:
- Logical state is a `int[6, 9]` sticker array
- Cubies are dumb geometry that get repainted from the array
- Animation rotates cubies visually, then snaps them back and recolors

**This is the wrong model.** It creates an inherent split between logical state and visual state that must be reconciled after every move — an endless source of mapping bugs.

### The Right Model: Cubies Own Their Identity

```
Cubie = GameObject + permanently colored sticker children
Grid  = GameObject[3,3,3] tracking which cubie is at which position
State = the grid itself (read sticker colors from cubies at face positions)
```

**Stickers are painted once at creation and never change color.** A red sticker is always red. It moves with its cubie. To rotate a face, you physically move cubies to new grid slots. What you see is what the state is.

---

## Architecture

### 1. RubikCubie (MonoBehaviour or plain class)

Each cubie knows nothing about the cube. It's just a black box with 0–3 colored face quads.

```csharp
public class RubikCubie : MonoBehaviour
{
    // Stickers are child quads, colored at Instantiate time.
    // No public methods to recolor. Immutable visual identity.
}
```

Created once during `Initialize()`. Never destroyed until cube is fully reset.

### 2. RubikCube (the grid + state)

```csharp
public class RubikCube : MonoBehaviour
{
    private RubikCubie[,,] _grid; // _grid[x,y,z] = cubie at that slot

    public void RotateFace(int face, int direction)
    {
        // 1. Permute _grid entries for this layer (pure index shuffle)
        // 2. Enqueue animation request
        // 3. IsSolved reads colors from _grid face positions
    }
}
```

**The grid IS the state.** `IsSolved` iterates face positions, reads child sticker colors from whatever cubie occupies each slot. No separate sticker array.

### 3. Grid Permutation (the only algorithm that matters)

A face rotation is a 2D rotation of a slice of the 3D grid. For a 3×3, the layer is always a 3×3 sub-grid. CW rotation of grid indices `(a,b)` → `(b, last-a)`. That's it.

```csharp
private void PermuteLayer(int face, int direction)
{
    // Extract 3×3 slice references
    var slice = new RubikCubie[N, N];
    for (int a = 0; a < N; a++)
        for (int b = 0; b < N; b++)
            slice[a, b] = GetSlotFromFace(face, a, b);

    // Write back rotated
    for (int a = 0; a < N; a++)
        for (int b = 0; b < N; b++)
        {
            RubikCubie source = direction == 1
                ? slice[last - b, a]    // CW
                : slice[b, last - a];   // CCW
            SetSlotFromFace(face, a, b, source);
        }
}
```

`GetSlotFromFace` / `SetSlotFromFace` map `(face, a, b)` → `(x, y, z)`. This is **6 cases**, each a one-liner. This is the ONLY face-specific mapping in the entire codebase. Verify it with a unit test per face (see §Testing).

### 4. RubikAnimator (decoupled animation consumer)

```csharp
public class RubikAnimator : MonoBehaviour
{
    private Queue<AnimRequest> _queue;

    struct AnimRequest
    {
        public int Face;
        public int Direction;
        public RubikCubie[] Cubies;      // the cubies to rotate
        public Vector3[] TargetPositions; // where they land after
    }
}
```

Flow per move:

1. **Before permutation:** Snapshot which cubies are in the layer AND compute their target positions (from the permuted grid).
2. **Permute the grid** (instant — logical state is now post-move).
3. **Enqueue animation:** Animator receives the cubie list, rotation axis, angle, and target positions.
4. **Animate:** Parent cubies to pivot, interpolate rotation.
5. **On finish:** Unparent, snap each cubie to its target position. **No recoloring. No rebuild. No remapping.** The grid already points to the right cubies.

### 5. Animation Direction — Derive, Don't Hardcode

Instead of a table of signs per face, **compute it:**

```csharp
private float GetCWAngle(int face)
{
    // Take a known cubie in the layer, compute where it should end up
    // after CW rotation, determine if +90 or -90 around the axis
    // achieves that. Do this ONCE at init, cache the result.
    Vector3 axis = FaceAxis(face);
    Vector3 testPos = GetTestCubiePosition(face);
    Vector3 rotated = Quaternion.AngleAxis(90f, axis) * testPos;
    Vector3 expected = GetExpectedCWPosition(face, testPos);
    return Vector3.Dot(rotated, expected) > 0 ? 90f : -90f;
}
```

Or even simpler: animate by rotating a test vector both ways and checking which one moves the cubie toward its target grid slot. Zero hardcoded sign tables.

---

## The 6-Case Mapping (GetSlotFromFace)

This is the only place where face-specific logic lives. For a cube with grid origin at `(0,0,0)` and axes `X=right, Y=up, Z=forward`:

| Face | Fixed axis | a maps to | b maps to |
|------|-----------|-----------|-----------|
| U (y=last) | y = last | a → x | b → z |
| D (y=0) | y = 0 | a → x | b → z |
| F (z=last) | z = last | a → x | b → y |
| B (z=0) | z = 0 | a → x | b → y |
| L (x=0) | x = 0 | a → z | b → y |
| R (x=last) | x = last | a → z | b → y |

**Critical detail:** The `a,b` orientation must be consistent such that `CW in (a,b)` = physical CW when looking at the face from outside. Get this right for one face, verify with a test, replicate the pattern.

Dedicate a unit test per face: start solved, rotate CW, check that 4 specific edge cubies moved to the right slots.

---

## Testing Strategy

### Unit Tests (no Unity, no rendering)

The grid permutation is pure logic — test it without MonoBehaviour:

```
TEST: Solved cube, F CW.
  - Cubie at (0, 2, 2) should move to (0, 0, 2)  ← top-left of F goes to bottom-left
  - Cubie at (2, 2, 2) should move to (0, 2, 2)  ← wait, is this right?
  → Don't guess. Place a NAMED cubie at each slot.
  → After PermuteLayer(F, CW), assert each named cubie is at the expected slot.
  → Do this for all 6 faces × 2 directions = 12 tests.
```

### Visual Smoke Test (the debug scramble)

Keep the deterministic `F CW → U CW → R CW` scramble sequence. Add a second phase that reverses it: `R CCW → U CCW → F CCW`. If the cube returns to solved state both logically and visually, the implementation is correct.

### Stress Test

100 random moves forward, then the same 100 moves reversed. Must return to solved.

---

## File Structure

```
Assets/Game/
  RubikCubie.cs        — Cubie identity + sticker children (immutable after creation)
  RubikGrid.cs         — 3D grid, permutation logic, IsSolved, scramble
  RubikAnimator.cs     — Queue-based animation consumer, pivot rotation, snap
  RubikMatchManager.cs — Scoring, match lifecycle (unchanged)
```

Separate grid logic from animation completely. `RubikGrid` has zero `using UnityEngine` visual dependencies beyond `MonoBehaviour` and `Transform` for positioning. `RubikAnimator` has zero state logic — it's a dumb mover.

---

## Key Invariants (enforce these or die)

1. **Stickers never change color.** If you find yourself calling `material.SetColor` after initialization, the architecture is wrong.

2. **The grid is the state.** If you find yourself maintaining a parallel `int[,]` sticker array, the architecture is wrong.

3. **Animation is cosmetic.** At the moment a move is requested, the grid permutes instantly. Animation catches up visually. If animation were deleted entirely, the cube would still function correctly (just jump-cut).

4. **One mapping, not two.** `GetSlotFromFace(face, a, b) → (x,y,z)` is the single source of truth for how face coordinates map to grid coordinates. It's used for permutation, for `IsSolved`, for cubie creation, and for animation gathering. If you have a second mapping anywhere, delete it.

5. **12 unit tests pass before writing any rendering code.** 6 faces × CW/CCW. Non-negotiable.

---

## What NOT to Do

- **Don't maintain a separate sticker state array.** That's what caused every bug in the current implementation — two sources of truth drifting apart.

- **Don't repaint cubies.** A cubie's visual identity is permanent. Move the cubie, don't repaint it.

- **Don't hardcode animation angles per face.** Compute or derive them. Tables of `{D, B, L} → positive, {U, F, R} → negative` are impossible to verify by reading.

- **Don't snap cubies back to original positions after animation.** Move them to their NEW positions. The grid already knows where they go.

- **Don't rebuild/destroy cubies after each move.** That's a band-aid for broken state management. Cubies should survive the entire session.

---

## Estimated Complexity

| Component | Lines | Difficulty |
|-----------|-------|------------|
| RubikCubie | ~30 | Trivial |
| RubikGrid | ~150 | Medium — the permutation + GetSlotFromFace is the core |
| RubikAnimator | ~120 | Medium — pivot parenting, interpolation, snap |
| Unit tests | ~100 | Easy but essential |
| **Total** | **~400** | One focused session |

This is a 400-line problem. If the implementation exceeds 600 lines, something is over-engineered.
