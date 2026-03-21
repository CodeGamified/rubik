# Next — Game Pitches

## Stratego — Table

**Hidden-information war game on a 10×10 grid.**

- **Code loop**: Students write AI that decides piece placement and movement *without knowing enemy ranks*. The core challenge is probabilistic reasoning — tracking which enemy pieces have been revealed, inferring rank from movement patterns, and deciding when to attack vs. scout.
- **Depth gradient**: Level 1 — random moves. Level 2 — attack-if-stronger logic. Level 3 — Bayesian rank inference from opponent behavior. Level 4 — bluff strategies (moving a low piece aggressively to fake high rank).
- **Visual payoff**: Fog-of-war reveal animations, battle resolution sequences, formation heatmaps showing AI confidence levels per enemy tile.
- **Teaching concepts**: Information theory, probability tracking, adversarial reasoning, state machines for piece behavior.

## Tower Defense — Combat

**Wave-based creep defense on grid maps with buildable tower positions.**

- **Code loop**: Students write targeting policies (`nearest`, `weakest`, `most_dangerous`), tower placement strategies, and upgrade timing logic. Each wave tests whether their code survives escalating pressure.
- **Depth gradient**: Level 1 — place towers manually, basic targeting. Level 2 — auto-placement heuristics (choke points, coverage overlap). Level 3 — economic optimization (spend vs. save for stronger towers). Level 4 — full wave-planning AI that reads upcoming wave composition and pre-builds counters.
- **Visual payoff**: Projectile arcs, splash damage radii, creep pathfinding lines, tower range overlays, wave-clear celebrations. Grid-based aesthetic fits the retro CRT look perfectly.
- **Teaching concepts**: A* pathfinding, priority queues, resource allocation, event-driven design, spatial reasoning.

## Rubik's — Puzzle

**3D cube manipulation with scramble-and-solve progression.**

- **Code loop**: Students write `rotate(face, direction)` commands and immediately see the cube transform. The challenge escalates from "fix one face" to "solve optimally in N moves."
- **Depth gradient**: Level 1 — brute-force random rotations. Level 2 — layer-by-layer (beginner method, ~100 moves). Level 3 — CFOP (intuitive F2L, OLL, PLL — ~55 moves). Level 4 — Kociemba two-phase (~22 moves). Level 5 — God's number research (provably ≤20 moves).
- **Visual payoff**: Smooth 3D rotation animations, move-count scoreboard, color-coded state diff showing progress, replay of solve path at speed. The 3D cube is inherently visually striking.
- **Teaching concepts**: State representation (array encoding of 54 stickers), group theory basics, search algorithms, heuristic design, recursion.

## Missile Command — Arcade

**Defend cities from incoming ballistic missiles by launching counter-missiles.**

- **Code loop**: Students write intercept logic — calculate where to aim counter-missiles to meet incoming threats based on trajectory prediction. Resource management layer: 3 bases, limited ammo, 6 cities to protect.
- **Depth gradient**: Level 1 — click-to-fire (manual). Level 2 — auto-intercept nearest threat. Level 3 — predict MIRV splits and pre-position detonation zones. Level 4 — triage logic (sacrifice far cities, consolidate defense around survivors). Level 5 — ammo budgeting across waves with escalating missile counts.
- **Visual payoff**: Explosion radii expanding and catching clusters, trail lines, city destruction sequences, retro vector aesthetic is native to the CRT theme.
- **Teaching concepts**: Trajectory prediction (linear interpolation), area-of-effect geometry, resource allocation under pressure, priority sorting.

## Lander — Arcade

**Land a spacecraft on a terrain surface using thrust and rotation controls.**

- **Code loop**: Students write thrust and rotation commands to softly land on a target pad. The physics is immediate — every `thrust(power)` and `rotate(degrees)` changes velocity and angle in real time. Fuel is finite.
- **Depth gradient**: Level 1 — manual control mapping. Level 2 — auto-hover (PID controller). Level 3 — fuel-optimal descent (minimize burn while hitting target velocity at touchdown). Level 4 — terrain-aware landing (scan surface, pick safest pad, route around obstacles). Level 5 — multi-stage descent with orbital insertion.
- **Visual payoff**: Flame particles, velocity/altitude HUD, terrain scrolling, crash explosions vs. smooth landings, fuel gauge draining. The stark moon-surface aesthetic is peak retro.
- **Teaching concepts**: PID control loops, vector math (velocity, acceleration, gravity), optimization under constraints, real-time physics simulation.

## Sokoban — Puzzle

**Push crates onto target tiles in a warehouse grid. No pulling. No undo (unless you code it).**

- **Code loop**: Student writes a solver — `move(direction)` pushes a crate if one is adjacent. Each level is a new puzzle. The student can play manually first, then automate. The API is dead simple; the *search space* is the challenge.
- **Depth gradient**: Level 1 — manual play, understand the rules. Level 2 — BFS brute-force (works on small puzzles, blows up on large ones). Level 3 — A* with Manhattan distance heuristic. Level 4 — deadlock detection (crate in corner = unsolvable, prune early). Level 5 — macro moves (push crate from A to B as single action, collapsing search depth). Level 6 — pattern databases for optimal solve.
- **Visual payoff**: Grid movement is clean and readable. Solution replays at speed are satisfying. Heatmaps of explored-vs-pruned states visualize *why* A* beats BFS. Level-complete animations.
- **Teaching concepts**: BFS vs. DFS vs. A*, state-space search, heuristic design, pruning, deadlock analysis. This is **the** search-algorithm teaching game.
- **Why it fills a gap**: The Puzzle category currently has Tetris (reflexes), Minesweeper (probability), Connect 4 (minimax). Sokoban adds *pure search* — the missing algorithmic pillar. And Rubik's teaches state-space too, but in 3D group theory; Sokoban teaches it in 2D grid pathfinding. Different enough to coexist.

## Battleship — Table

**Hunt-and-sink on dual 10×10 grids. Pure information game.**

- **Code loop**: Student writes a targeting algorithm: `fire(row, col)` → hit or miss → update probability model → pick next shot. Every single turn is a code decision with immediate visual feedback (splash vs. explosion). No physics, no reflexes — pure algorithmic thinking.
- **Depth gradient**: Level 1 — random firing. Level 2 — hunt/target mode (random until hit, then search adjacent). Level 3 — parity optimization (checkerboard pattern, skip impossible cells). Level 4 — probability density heatmap (for each cell, count how many remaining ship placements overlap it, fire at max). Level 5 — adaptive inference (if opponent tends to cluster ships, weight edges lower).
- **Visual payoff**: Probability heatmaps overlaid on the grid are *visually stunning* — students literally see their algorithm's "thinking." Hit chains lighting up ship silhouettes. Fog clearing. The dual-grid layout with retro CRT scan lines is natural.
- **Teaching concepts**: Probability, Bayesian updating, search optimization, information gain, enumeration.
- **Why it fills a gap**: The Table category is heavy on perfect-information games (Chess, Checkers) and cards (Poker, Blackjack). Battleship adds **hidden-information search** — a fundamentally different algorithmic challenge. Pairs well with Stratego but is simpler to implement and faster to play.

## Worms — Combat

**Turn-based artillery on destructible 2D terrain. Aim, set power, account for wind, fire.**

- **Code loop**: `aim(angle)`, `power(force)`, `fire()` — then watch the projectile arc across the screen, crater the terrain, and damage (or miss) the opponent. Every shot is a physics calculation the student can see play out. Wind changes each turn, so the formula is never memorized — it must be *computed*.
- **Depth gradient**: Level 1 — manual aim/fire. Level 2 — ballistic calculator (angle + power → landing point, ignoring wind). Level 3 — wind-compensated targeting. Level 4 — weapon selection (grenades bounce, homing missiles track, airstrikes drop vertically — each needs different math). Level 5 — positional strategy (move to high ground, dig tunnels, use terrain as cover). Level 6 — multi-worm team coordination.
- **Visual payoff**: Projectile arcs tracing across the sky, terrain chunking away on impact, worms ragdolling off edges, water rising. Explosions are *the* retro visual reward. Destructible terrain means the map is visually different every game.
- **Teaching concepts**: Projectile physics (parabolic motion), trigonometry, wind vector addition, terrain collision, state mutation (map destruction).
- **Why it fills a gap**: Combat currently has Galaga (pattern dodge), Royale (real-time), Tanks (direct fire), Fighter (melee). Worms adds **turn-based indirect fire** — the only combat game where you have *time to think and calculate* before acting. That's the CodeGamified sweet spot.

---

## Ambitious Scope — The Big Swings

---

## Roguelike — Sandbox

**Procedurally generated dungeon crawl. Permadeath. Every run is unique because the student writes the generator.**

- **Code loop**: Two audiences, two loops. **Player side**: write AI that navigates dungeon floors — `move()`, `attack()`, `useItem()`, `descend()`. **Builder side**: write the generation — `placeRoom()`, `connectCorridor()`, `spawnEnemy()`, `populateLoot()`. The student is simultaneously the architect and the player.
- **Depth gradient**: Level 1 — hand-placed rooms, manual play. Level 2 — BSP-tree dungeon gen (binary space partition → room carving → corridor connection). Level 3 — cellular automata for cave systems. Level 4 — wave function collapse for thematic tilesets. Level 5 — loot table probability design with rarity curves. Level 6 — enemy behavior trees (patrol, chase, flee, call allies). Level 7 — write an auto-player that clears generated dungeons using BFS + combat heuristics.
- **Visual payoff**: Infinite replayability. Fog-of-war reveal as rooms light up. ASCII-to-sprite toggle (fits the CRT aesthetic *perfectly*). Minimap generation in real time. Death screens showing how deep you got.
- **Teaching concepts**: Procedural generation (BSP, cellular automata, WFC), behavior trees, probability distributions, graph connectivity, random seeding, permadeath as consequence design.
- **Why it's ambitious**: This is the **capstone game** — it touches nearly every CS concept in the roster. Data structures, AI, randomness, graph theory, resource management. A student who builds a full roguelike has touched more programming concepts than a semester of coursework.

## Bot Arena — Meta (New Category)

**Students write bots. Bots fight each other. The game IS the code.**

- **Code loop**: Each student submits a bot script with a standard API: `scan()`, `move()`, `shoot()`, `shield()`. Bots are dropped into an arena and run simultaneously. No manual play — the code IS the player. Round results replay as a visual battle.
- **Depth gradient**: Level 1 — spin-and-shoot. Level 2 — dodge when scanned. Level 3 — predictive targeting (lead shots based on enemy velocity). Level 4 — terrain usage (walls as cover, chokepoint camping). Level 5 — adaptive strategies (detect opponent patterns, switch between aggressive/defensive). Level 6 — team coordination (2v2, 3v3 — bots communicate via shared state).
- **Visual payoff**: Battle replays rendered as top-down arena combat with projectile trails, shield flashes, explosion particles. Leaderboard brackets. Replay scrubbing to see exactly where strategy A beat strategy B. Tournament mode with bracket visualization.
- **Teaching concepts**: API design (the bot API itself teaches interface contracts), prediction algorithms, state machines, adversarial reasoning, and — critically — **reading other people's code** to counter their strategies.
- **Why it's ambitious**: This is the **multiplayer layer** for CodeGamified. Every other game is student-vs-game. Bot Arena is student-vs-student. It creates a competitive ecosystem where improving your code is directly rewarded by climbing the leaderboard. This is the game that makes CodeGamified *social*.

## Space Trader — Sim

**Fly a ship between star systems. Buy low, sell high. Upgrade, survive, profit.**

- **Code loop**: `navigate(star)`, `buy(commodity, qty)`, `sell(commodity, qty)`, `upgrade(ship_component)`. The student writes a trading AI that maximizes profit across a procedurally generated star map with dynamic supply/demand.
- **Depth gradient**: Level 1 — manual trade (fly, look at prices, buy/sell by hand). Level 2 — single-route optimizer (find the best A→B pair). Level 3 — multi-hop route planner (traveling salesman variant with fuel constraints). Level 4 — dynamic pricing model (prices fluctuate based on supply/demand curves — predict future prices from trends). Level 5 — risk management (pirate encounters, cargo insurance, escort hiring). Level 6 — fleet management (multiple ships, parallel routes, supply chain logistics).
- **Visual payoff**: Star map with trade route lines glowing. Ship traveling between nodes. Price tickers scrolling. Cargo hold filling/emptying. Profit/loss graphs over time. The retro star-map aesthetic (think Elite 1984) is native to the CRT theme.
- **Teaching concepts**: Graph traversal, optimization under constraints (fuel, cargo capacity, credits), dynamic programming, time-series prediction, the traveling salesman problem as a real game mechanic.
- **Why it's ambitious**: It takes the Sim category from physics toys (Pool, Racer) into **systems thinking**. The student isn't simulating one physical system — they're managing an *economy*. It's the first game where the student's code runs a business.

## Platformer — Arcade

**2D side-scrolling platformer with tile-based levels. The student writes the character controller, enemy AI, and level generator.**

- **Code loop**: `jump()`, `run(direction)`, `attack()` as player inputs — but the real coding loop is authoring the systems: gravity, collision response, coyote time, wall sliding, enemy patrol/chase behaviors, moving platforms. Each mechanic is a discrete code module the student wires up.
- **Depth gradient**: Level 1 — basic movement + gravity. Level 2 — tile collision (AABB vs. tilemap). Level 3 — enemy AI (patrol → detect → chase → return). Level 4 — advanced movement (wall jump, dash, double jump — each is a state machine extension). Level 5 — procedural level generation (rhythm-based gap/platform placement that guarantees solvability). Level 6 — boss AI with phase transitions.
- **Visual payoff**: Side-scrollers are the most visually legible genre — left to right, jumps arcing, enemies patrolling. Sprite animations, parallax backgrounds, particle effects on land/dash. Students see their physics tuning *immediately* — change gravity from 9.8 to 15 and watch the character drop like a rock.
- **Teaching concepts**: Physics integration (velocity, acceleration, gravity), AABB collision, state machines (grounded, airborne, wall-sliding, dashing), tilemap data structures, procedural level design.
- **Why it's ambitious**: This is the **game that every student imagines when they hear "make a game."** It's the most intuitive genre. And it teaches the widest range of real-time game programming fundamentals — physics, collision, animation state machines, enemy AI, level design — in the format players already understand from decades of Mario/Sonic/Celeste.

## Civilization-Lite — Sandbox

**Turn-based 4X on a hex grid. Explore, expand, exploit, exterminate — but the student writes the AI advisor (or the entire opponent).**

- **Code loop**: `found_city(tile)`, `research(tech)`, `build(unit_or_building)`, `move_unit(unit, tile)`, `declare_war(player)`. Each turn, the student's code makes every decision for their civilization. The map is procedurally generated with terrain types, resources, and rival AI civs.
- **Depth gradient**: Level 1 — manual play, learn mechanics. Level 2 — auto-expand (find optimal city placement based on tile yields). Level 3 — tech-tree pathfinding (shortest path to a target tech). Level 4 — military AI (unit production, army composition, attack timing). Level 5 — diplomacy state machine (trade, alliance, betrayal triggers based on relative power). Level 6 — full AI player that competes against other students' AI civs.
- **Visual payoff**: Hex grid with terrain textures, fog-of-war rolling back as scouts explore, cities growing with buildings, unit armies marching, tech tree branching visualization. The hex map alone is visually compelling. Minimap showing territorial expansion over time.
- **Teaching concepts**: Graph theory (hex grids, tech trees), resource optimization, multi-objective decision-making, state machines for diplomacy, evaluating trade-offs (build army vs. build economy), long-horizon planning.
- **Why it's ambitious**: This is the **most complex system a student can build**. It's not one algorithm — it's dozens of interlocking systems. A student who writes a competitive Civ AI has demonstrated mastery of data structures, search, optimization, state management, and strategic reasoning. This is the PhD thesis of CodeGamified games.

## Maze — Puzzle

**Generate, solve, race. The student writes both the labyrinth and the pathfinder.**

- **Code loop**: Two-phase loop. **Phase 1 — Generate**: `carve(cell, direction)` using algorithms (recursive backtracker, Kruskal's, Prim's, Eller's). Each algorithm produces mazes with different characteristics (long corridors vs. branchy, biased directions, dead-end density). **Phase 2 — Solve**: write an agent that navigates from start to exit. Then optimize: fewest steps, fastest wall-follower, A* with perfect information, or — the hard version — solve with *fog* (agent only sees adjacent cells).
- **Depth gradient**: Level 1 — random DFS carving + manual solve. Level 2 — Kruskal's generation (teaches union-find). Level 3 — wall-follower solve (always turn right). Level 4 — BFS solve (guaranteed shortest path). Level 5 — fog-of-war solve (Trémaux's algorithm, pledge algorithm). Level 6 — adversarial generation (create the *hardest* maze for a given solver — this inverts the problem and teaches adversarial thinking).
- **Visual payoff**: Maze generation animated cell-by-cell is mesmerizing. Solve path tracing in a different color. Side-by-side comparison of BFS vs. DFS vs. A* paths. Fog-of-war explore animations. The generation itself is the visual reward — students watch their algorithm *build a world*.
- **Teaching concepts**: Graph algorithms (DFS, BFS, Kruskal's, Prim's), union-find data structure, pathfinding, adversarial design, the duality of generation and solving.
- **Why it's ambitious**: It's the only game where the student builds *both sides* — the challenge and the solution. That duality teaches something no other game in the roster touches: that algorithms for *creating* problems and *solving* problems are deeply related. Kruskal's maze gen uses the same union-find as Kruskal's MST. The generator IS the solver, inverted.

## Hacker — Sim (or new category)

**Network intrusion simulation. Traverse a graph of nodes, breach firewalls, exfiltrate data, avoid trace.**

- **Code loop**: `scan(node)`, `exploit(node, vulnerability)`, `connect(node)`, `download(file)`, `cover_tracks()`. The network is a graph. Each node has defenses (firewall level, IDS sensitivity, patch status). The student writes an intrusion script that finds the optimal path from entry to target data while keeping trace level below detection threshold.
- **Depth gradient**: Level 1 — linear network, no defenses. Level 2 — branching network, find shortest path. Level 3 — firewalls require specific exploits (match vulnerability type to tool). Level 4 — intrusion detection systems accumulate trace — must `cover_tracks()` periodically or get caught. Level 5 — adaptive defenses (network responds to intrusion, reroutes, patches). Level 6 — time pressure (trace accumulates passively, creating an optimization race). Level 7 — multi-target missions (exfiltrate 3 files across different subnets, planning the route).
- **Visual payoff**: Network graph visualization with nodes pulsing, connection lines tracing, firewalls as shields, trace meter climbing, data flowing along paths. Think the Uplink/Hacknet aesthetic — it IS the CRT retro vibe. Terminal text scrolling alongside the graph. This might be the most visually on-brand game possible for CodeGamified.
- **Teaching concepts**: Graph traversal, constraint optimization, resource management (exploits are limited), risk assessment, stealth as an optimization metric, network topology.
- **Why it's ambitious**: It reframes *programming itself* as the game's narrative. The student isn't controlling a character — they're writing a script that executes against a system. The code-to-game metaphor collapses entirely: the student IS a programmer, doing programming, in a game about programming. It's meta in the best way. And the aesthetic — green text on black, network graphs, firewall breaches — is the exact visual identity CodeGamified already has.

---

## Pop / Staple Games — Instant Recognition

---

## Pac-Man — Arcade

**The most recognizable game ever made. Maze chase with four ghosts, each with distinct AI personalities.**

- **Code loop**: The magic is the ghost AI. Students write behavior for each ghost: Blinky (chase directly), Pinky (ambush — target 4 tiles ahead of Pac-Man), Inky (flank — use Blinky's position to triangulate), Clyde (scatter when close, chase when far). Four different algorithms, one maze, emergent difficulty.
- **Depth gradient**: Level 1 — move Pac-Man manually, random ghosts. Level 2 — implement Blinky (direct chase via shortest path). Level 3 — add Pinky (predictive targeting). Level 4 — add Inky (relational positioning — requires reading another ghost's state). Level 5 — add Clyde (distance-based mode switching). Level 6 — write Pac-Man AI that survives all four using their known behaviors against them. Level 7 — optimal pellet routing (TSP variant).
- **Visual payoff**: The iconic maze, dot trails disappearing, ghost color/eye animations, power pellet reversal with blue-ghost panic, fruit bonuses. One of the most visually recognizable game screens in history.
- **Teaching concepts**: Finite state machines (chase/scatter/frightened), pathfinding with personality, target prediction, emergent behavior from simple rules, the classic AI lesson: *four simple algorithms create one complex system*.
- **Why it's essential**: If CodeGamified doesn't have Pac-Man, people will ask where Pac-Man is. It's the single most important "missing staple." And the ghost AI is legitimately one of the best CS teaching examples ever designed — Toru Iwatani accidentally created a masterclass in behavior trees in 1980.

## Space Invaders — Arcade

**The game that created the arcade industry. Formation of aliens descending row by row, speeding up as numbers thin.**

- **Code loop**: `move_cannon(direction)`, `fire()`. The coding depth is in the *enemy system*: formation grid management, descent triggers, speed scaling (fewer aliens = faster clock — the original's speed-up was a hardware limitation that became a design feature). Students write the invasion logic, then write the defense.
- **Depth gradient**: Level 1 — manual cannon, static invaders. Level 2 — formation movement (grid shifts, edge detection, row drop). Level 3 — speed scaling (tick rate increases as alien count decreases). Level 4 — shield erosion system (pixel-level barrier destruction). Level 5 — auto-aim cannon (prioritize lowest/fastest/most dangerous column). Level 6 — bonus UFO targeting (risk/reward — break formation focus to shoot the high-value target).
- **Visual payoff**: The marching formation is hypnotic. Row-by-row descent with the iconic accelerating beat. Shield chunks disappearing pixel by pixel. The visual tension of the last lone invader zipping across the screen at max speed.
- **Teaching concepts**: Grid data structures, tick-rate scaling, pixel-level collision, risk/reward decision-making, the emergent difficulty curve.
- **Why it's essential**: Historical significance alone warrants inclusion. It's also the simplest "complete game" in the arcade category — students can have a fully functional Space Invaders faster than almost any other title, making it a great early win.

## Bomberman — Arcade

**Grid-based bomb placement with chain-reaction explosions and destructible walls.**

- **Code loop**: `place_bomb()`, `move(direction)`. Bombs explode in cross patterns after a timer. The coding challenge: don't blow yourself up. Students write pathfinding that accounts for *future* explosions — bombs that haven't detonated yet are walls-that-will-become-lethal-in-N-ticks.
- **Depth gradient**: Level 1 — manual play, understand blast patterns. Level 2 — safe-zone calculator (given current bombs, which tiles will be safe after all detonations?). Level 3 — chain-reaction planning (place bomb A to trigger bomb B to blow wall C). Level 4 — enemy trapping (predict AI movement, cut off escape routes with bomb placement). Level 5 — power-up optimization (flame range vs. bomb count vs. speed — which pickup order maximizes survival?). Level 6 — 4-player AI battle royale.
- **Visual payoff**: Explosions cascading through corridors. Walls crumbling to reveal power-ups. Chain reactions rippling across the map. The grid aesthetic is clean and CRT-native.
- **Teaching concepts**: Temporal reasoning (predicting future state), BFS with time dimension, trap-setting as adversarial search, chain-reaction graph traversal.

## 2048 — Puzzle

**Slide numbered tiles on a 4×4 grid. Matching tiles merge and double. Reach 2048.**

- **Code loop**: `slide(direction)` — that's the entire API. Four directions, one action. The depth is in *choosing* which direction. Students write expectimax AI (like minimax but with random tile spawns as the "opponent").
- **Depth gradient**: Level 1 — manual play. Level 2 — greedy strategy (always merge the most). Level 3 — corner strategy (keep highest tile in corner, build monotonic rows). Level 4 — expectimax search (evaluate all possible moves + all possible random tile spawns, pick highest expected score). Level 5 — deep expectimax with pruning (search 6+ moves ahead). Level 6 — reach 4096, 8192 — push the AI to superhuman performance.
- **Visual payoff**: Tiles sliding with momentum, merge animations with number doubling, color gradient escalating from warm to hot as values climb. Score counter racing upward. The minimalist grid aesthetic is beautiful.
- **Teaching concepts**: Expectimax (minimax variant with chance nodes), heuristic evaluation functions, monotonicity/smoothness metrics, search depth vs. computation trade-offs.
- **Why it's essential**: Viral cultural moment. Everyone has played it. And the expectimax AI is the natural next step after minimax (Connect 4, Chess) — it introduces *randomness* into adversarial search. Completes a teaching progression.

## Wordle — Puzzle

**Guess a 5-letter word in 6 tries. Each guess reveals green (correct position), yellow (wrong position), gray (not in word).**

- **Code loop**: `guess(word)` → receive color feedback → narrow possibilities → guess again. The student writes an information-theoretic solver: which guess *maximally reduces the remaining word space*?
- **Depth gradient**: Level 1 — manual play. Level 2 — elimination filter (remove impossible words after each guess). Level 3 — frequency-based guessing (pick words with the most common remaining letters). Level 4 — entropy maximization (for each possible guess, compute expected information gain across all possible outcomes, pick the max). Level 5 — optimal opening word computation (which single word eliminates the most possibilities on average?). Level 6 — hard mode solver (every guess must use all revealed clues — constrains the search differently). Level 7 — adversarial Wordle (the answer changes to maximize your guesses — Absurdle).
- **Visual payoff**: The iconic green/yellow/gray grid revealing letter by letter. Remaining word count shrinking in real time. Entropy heatmaps showing which letters carry the most information. The guessing grid is already a perfect retro-CRT aesthetic.
- **Teaching concepts**: Information theory (entropy, information gain), constraint satisfaction, dictionary/trie data structures, combinatorial filtering, the concept of *optimal questions*.
- **Why it's essential**: Massive cultural penetration — everyone knows the rules. And it's secretly one of the best information theory teaching tools ever created. The jump from "I play Wordle" to "I wrote an AI that solves any Wordle in ≤3.5 guesses on average" is a powerful learning moment.

## Solitaire (Klondike) — Table

**The most played computer game in history. 7 tableau columns, 4 foundation piles, draw from stock.**

- **Code loop**: `move(card, destination)` — tableau to tableau, tableau to foundation, stock to waste to tableau. The student writes a solver that decides which moves to make and in which order. Unlike Chess, there's hidden information (face-down cards) — so the AI must reason under uncertainty.
- **Depth gradient**: Level 1 — manual play. Level 2 — greedy solver (always move to foundation if possible). Level 3 — heuristic priority (prefer moves that reveal face-down cards). Level 4 — look-ahead search (simulate N moves, evaluate board state). Level 5 — probability modeling (given remaining deck composition, estimate likelihood of completing a run). Level 6 — optimal play analysis (Klondike is ~79% winnable with perfect information — what win rate can the student's AI achieve with hidden cards?).
- **Visual payoff**: Card animations (flip, slide, fan), cascading stacks building up, the satisfying auto-complete waterfall when foundation piles are nearly done. Green felt background with the CRT overlay.
- **Teaching concepts**: Decision-making under uncertainty, heuristic design, partial-information search, probability estimation, the difference between "solvable" and "winnable with imperfect info."
- **Why it's essential**: More people have played Solitaire than any other computer game. Period. Microsoft shipped it with every Windows install for 30 years. It's the ultimate "I already know the rules" onramp. And the hidden-information AI challenge is genuinely hard — distinct from Chess (perfect info) and Poker (bluffing).

## Oregon Trail — Sim

**Travel 2,000 miles by wagon. Manage food, health, pace, hunting, river crossings, and random events. Try not to die of dysentery.**

- **Code loop**: `set_pace(speed)`, `set_rations(level)`, `buy(item, qty)`, `hunt()`, `ford_river()` / `caulk_wagon()` / `hire_ferry()`. Each day is a tick. The student writes a survival optimizer that balances speed against health against resources across a 5-month journey with randomized events.
- **Depth gradient**: Level 1 — manual play, learn the trade-offs. Level 2 — fixed strategy (set pace/rations and never change). Level 3 — adaptive pacing (push hard when healthy, rest when sick). Level 4 — supply optimization (buy exactly enough at each fort based on projected consumption). Level 5 — event response AI (river crossings: evaluate width/depth/speed → choose safest option). Level 6 — Monte Carlo simulation (run 10,000 trails, compute optimal strategy statistically). Level 7 — party composition optimization (who to bring, when to trade, sacrifice vs. save decisions).
- **Visual payoff**: Wagon rolling across pixel landscapes (prairie → mountains → desert → Oregon). Weather changing. Tombstone markers on the trail. The journal log scrolling events. Hunting minigame with pixel animals. River crossing animations. It's the *original* educational game aesthetic.
- **Teaching concepts**: Resource management over time, stochastic optimization, Monte Carlo simulation, risk assessment under uncertainty, multi-variable trade-off analysis.
- **Why it's essential**: Cultural icon of educational gaming. The irony of *CodeGamified* including the game that *started* educational gaming is too perfect. And the "optimize survival across random events" loop is a gameplay concept covered by nothing else in the roster.

## Lemmings — Puzzle / Sim

**100 mindless lemmings march forward. Assign jobs (digger, builder, blocker, climber, basher) to guide them to the exit. Save enough to pass.**

- **Code loop**: `assign(lemming_id, job)` as lemmings march in real time. The student writes a router: scan terrain, identify obstacles, assign the minimum jobs to create a viable path. Each lemming is an independent agent following simple rules — the student's code is the *coordinator*.
- **Depth gradient**: Level 1 — manual assignment, learn jobs. Level 2 — pre-plan the full path before unpausing (study terrain → sequence of assignments). Level 3 — auto-router (scan terrain features → generate assignment sequence algorithmically). Level 4 — optimization (minimize assigned jobs — unused lemmings = higher score). Level 5 — timing precision (assign basher at exactly the right tick to breach wall before lemmings pile up). Level 6 — procedural level generation + auto-solver verification.
- **Visual payoff**: Swarm of tiny lemmings marching, digging, building staircases, blocking traffic, bashing through walls. The chaos of 100 agents + the satisfaction of a clean route. Explosion animation when you nuke the remainder. Pure retro pixel charm.
- **Teaching concepts**: Multi-agent coordination, job scheduling, terrain analysis, constraint satisfaction with timing, the distinction between *agent behavior* (simple) and *coordinator behavior* (complex).
- **Why it's essential**: The only game in the roster where the student controls *many agents simultaneously* via job assignment. Every other game is 1 player, 1 unit, or turn-based. Lemmings teaches **swarm coordination in real time** — a fundamentally different programming paradigm.

---

## Missing Gameplay Loops — New Niches

---

## Soccer — Sports (New Category)

**2D top-down soccer with simple physics. The student writes team AI.**

- **Code loop**: Write behavior for each player: `move_to(position)`, `pass(teammate)`, `shoot(goal)`, `tackle()`, `mark(opponent)`. The student doesn't control one player — they write the **team brain**. Formation logic, passing lanes, marking assignments, when to attack vs. defend.
- **Depth gradient**: Level 1 — all players chase ball (schoolyard soccer). Level 2 — position holding (stay in formation zones). Level 3 — passing logic (find open teammate, weight by angle-to-goal). Level 4 — marking AI (assign defender per attacker based on proximity + threat). Level 5 — set pieces (corner kick routines, free kick walls). Level 6 — adaptive formation switching (4-4-2 when defending, 3-4-3 when attacking, triggered by ball position). Level 7 — full match AI vs. other students' teams.
- **Visual payoff**: Top-down pitch with tiny players running formations, ball physics with spin, passing lanes drawn as debug overlays, goal celebrations. Formation diagrams shifting in real time. The minimap IS the game view.
- **Teaching concepts**: Multi-agent coordination, spatial reasoning, role assignment, state machines per agent, formation as data structure, real-time decision-making.
- **Why it opens a new category**: Sports is **completely absent** from the roster. Soccer is the most universal sport on earth and the team-AI challenge is unlike anything else — it's the only game where the student programs *11 agents cooperating simultaneously*. Basketball, Hockey, Football could follow as variants.

## Guitar Hero / Rhythm — Sim

**Notes fall down lanes. Hit the key when the note crosses the line. Score on timing precision.**

- **Code loop**: Two sides. **Player side**: `hit(lane)` at the right millisecond — but the interesting code is the *timing window* system itself (perfect/great/good/miss thresholds). **Builder side**: write a beat mapper — `place_note(lane, beat)` synced to BPM. Parse a MIDI file → auto-generate a note chart. Students who build the chart generator are writing a music-analysis algorithm.
- **Depth gradient**: Level 1 — manual play on a premade chart. Level 2 — auto-player (read upcoming notes, fire inputs at exact timestamps). Level 3 — beat map generator from BPM (evenly spaced notes). Level 4 — MIDI-to-chart parser (read note events from .mid files, map pitches to lanes, extract rhythm). Level 5 — difficulty scaling (auto-simplify charts by removing notes while preserving rhythm feel). Level 6 — audio onset detection (analyze raw audio → detect beats → generate chart with no MIDI input).
- **Visual payoff**: Notes cascading down lanes with glow trails, hit explosions, combo counter climbing, streak flames, score multiplier visuals. The visual language is universally understood. Pairs perfectly with the existing Midi game's audio infrastructure.
- **Teaching concepts**: Timing systems (quantization, tolerance windows), audio/MIDI parsing, signal processing basics, mapping between data domains (audio → visual), BPM detection.
- **Why it fills a gap**: Midi already exists as a music *tool*. Rhythm Game turns music into a *challenge*. The gameplay loop — react to temporal patterns with precision timing — exists nowhere else in the roster. And the chart-generation side is a genuine algorithmic challenge that could reuse Midi's audio stack.

## Stealth — Combat

**Top-down stealth infiltration. Guards patrol, cameras sweep, the student writes the infiltrator's route.**

- **Code loop**: `move_to(tile)`, `wait(ticks)`, `distract(position)`, `disable(guard)`. The map has patrol routes (known) and vision cones (visible). The student writes a path planner that threads between guard cycles, waits in shadows, and reaches the objective without detection.
- **Depth gradient**: Level 1 — manual sneak, learn guard patterns. Level 2 — record guard patrol cycles, find safe windows. Level 3 — pathfinder that avoids all vision cones (static analysis). Level 4 — temporal pathfinding (account for guard *movement* — a tile is safe at tick 5 but not tick 8). Level 5 — distraction planning (throw noise to reroute a guard, creating a window that didn't exist). Level 6 — multi-guard manipulation (chain distractions to open a corridor through 3 overlapping patrols). Level 7 — procedural level gen + auto-routing.
- **Visual payoff**: Vision cones sweeping in real time, shadow zones highlighted, the infiltrator's planned path drawn as a dotted line, guards turning and the safe corridor opening/closing. Tension meter. The top-down stealth aesthetic (think Hotline Miami meets Metal Gear 1) is retro-native.
- **Teaching concepts**: Temporal graph search (space + time as state), patrol cycle analysis, constraint satisfaction with timing, adversarial path planning, the concept of *windows* in scheduling.
- **Why it fills a gap**: Every combat game in the roster is about *engaging* enemies. Stealth is about *avoiding* them. It inverts the combat paradigm entirely. The algorithmic challenge — pathfinding through time-varying obstacles — is a concept no other game teaches.

## Idle / Clicker — Sandbox

**Click to earn currency. Buy producers that earn passively. Prestige to reset with multipliers. Numbers go up forever.**

- **Code loop**: `buy(producer)`, `upgrade(producer)`, `prestige()`. The student writes an optimization AI: when is the optimal time to buy the next producer? When should you prestige (reset all progress for a permanent multiplier)? The math is exponential growth, and the strategy is *managing* exponential growth.
- **Depth gradient**: Level 1 — manual clicking and buying. Level 2 — auto-buyer (purchase cheapest available producer). Level 3 — ROI calculator (buy the producer with the best cost-to-production ratio). Level 4 — prestige timing optimizer (model post-prestige growth curve, prestige when the time-to-recover is minimized). Level 5 — multi-currency optimization (some producers feed others — find the optimal pipeline). Level 6 — offline progression modeling (compute what would have happened over N hours without running).
- **Visual payoff**: Numbers cascading upward, production chains animating, prestige reset with fireworks, exponential graphs showing growth curves, the satisfying moment when passive income exceeds active clicking by 1000x.
- **Teaching concepts**: Exponential growth, ROI analysis, optimization over time horizons, prestige as a mathematical concept (when does resetting maximize long-term yield?), economic modeling.
- **Why it fills a gap**: The only game where the challenge is **managing exponential systems**. Every other game has linear or polynomial scaling. Idle games teach students to *think in exponents* — a skill that maps directly to algorithm complexity analysis (O(n) vs. O(2^n)). And the code-to-result loop is instant: change your buy strategy, watch the growth curve bend.

## Survival — Sandbox

**Top-down survival on a procedural island. Gather resources, craft tools, build shelter, survive day/night cycles with escalating threats.**

- **Code loop**: `gather(resource)`, `craft(recipe)`, `build(structure, position)`, `eat(food)`, `equip(tool)`. Each day ticks through daylight (safe, gather) → dusk (return to base) → night (defend from threats). The student writes a daily planner that maximizes resource gathering while ensuring survival through the night.
- **Depth gradient**: Level 1 — manual play, learn the loop. Level 2 — auto-gather (nearest resource of needed type). Level 3 — crafting planner (given inventory, compute shortest recipe chain to target item). Level 4 — base placement optimizer (proximity to resources, defensibility, water access). Level 5 — day planner AI (given N daylight ticks, compute the optimal gather route that maximizes value and returns to base before dark). Level 6 — tech-tree rushing (find the fastest path from stone tools to iron walls). Level 7 — multi-day strategic planning (stockpile for upcoming threat escalation).
- **Visual payoff**: Day/night cycle with lighting changes, resource nodes being harvested, crafting animations, base growing over days, nighttime threats approaching with glowing eyes, seasonal weather changes. The island procedurally generated each playthrough.
- **Teaching concepts**: Recipe/dependency graphs, scheduling/planning under time pressure, the explore-exploit trade-off, procedural world generation, inventory management as a constraint problem.
- **Why it fills a gap**: The gather→craft→survive loop is the defining gameplay loop of the 2010s-2020s (Minecraft, Terraria, Valheim, Don't Starve). It's a cultural staple AND an algorithmic goldmine. The Sandbox category has world builders (PopVuj, Dwarf, Minecraft) but no *survival pressure* — this adds stakes to the sandbox.

## Tron / Light Cycles — Arcade

**Two players leave trails. Turn 90°. Don't hit any trail (yours or theirs). Last one alive wins.**

- **Code loop**: `turn(left)` or `turn(right)` — that's all. Two commands. The entire game is deciding *when* to turn. The student writes an AI that reads the board state (grid of occupied cells), evaluates which directions lead to open space, and avoids trapping itself while trying to trap the opponent.
- **Depth gradient**: Level 1 — random turns (dies immediately). Level 2 — wall avoidance (don't turn into an obstacle). Level 3 — flood-fill evaluation (for each possible move, flood-fill reachable empty space — pick the direction with the most room). Level 4 — opponent modeling (predict where they'll go, cut them off). Level 5 — Voronoi partitioning (compute which player controls more of the remaining space, optimize for territory). Level 6 — endgame solving (when space is small enough, minimax to forced win).
- **Visual payoff**: Neon trails on black background. This IS the CRT aesthetic. Glowing lines carving the grid, the claustrophobic tension as space disappears, the moment one player gets boxed in. Instant visual classic.
- **Teaching concepts**: Flood fill, Voronoi diagrams, spatial reasoning, minimax in continuous-state games, the concept of *space as resource*.
- **Why it fills a gap**: Simplest possible API (two commands) with the deepest possible strategic depth. The contrast is the lesson: *complexity comes from interaction, not from the API surface*. Also: it's a natural **1v1 bot arena** entry — students' Tron AIs can fight each other. Bridge to Bot Arena's social/competitive model.

---

## Unseen Paradigms — Filling the Last Gaps

---

## RPG Combat — Combat (or new category: RPG)

**Turn-based party combat. 4 heroes vs. enemy formations. Elemental weaknesses, status effects, mana management.**

- **Code loop**: `attack(target)`, `cast(spell, target)`, `defend()`, `use_item(item, target)`, `swap(party_member)`. Each turn, the student's code decides actions for up to 4 party members against an enemy group. The challenge is reading the enemy state (HP, buffs, type) and responding with the optimal sequence.
- **Depth gradient**: Level 1 — manual play, learn the damage formula. Level 2 — auto-attacker (always attack lowest HP enemy). Level 3 — elemental exploitation (fire vs. ice enemy = 2x damage; detect types, match spells). Level 4 — status effect strategy (sleep the healer, poison the tank, haste your DPS). Level 5 — mana economy (burst now or conserve for the boss?). Level 6 — party composition optimizer (given N available heroes, pick the 4 that counter this specific enemy formation). Level 7 — full dungeon AI that manages HP/MP across multiple encounters with no healing between fights.
- **Visual payoff**: Classic side-view battle screen (party left, enemies right). Spell animations, damage numbers floating, health bars draining, status effect icons, critical hit flashes. The most nostalgia-dense visual format in gaming — every 90s kid recognizes it instantly.
- **Teaching concepts**: Damage formulas (multiplicative vs. additive modifiers), type effectiveness graphs (rock-paper-scissors with depth), resource management across encounters, priority targeting, the concept of *burst vs. sustained* optimization.
- **Why it fills a gap**: RPG combat is a **massive** genre (Final Fantasy, Pokémon, Persona, Dragon Quest) with zero representation. The damage-formula system alone is a rich coding domain — students learn that `attack * (1 - defense/(defense + 100))` behaves completely differently from `attack - defense`. The math IS the gameplay.

## Deckbuilder — Table (or new category: Cards)

**Start with a weak deck. Fight encounters. Win cards as rewards. Build synergies. Survive the spire.**

- **Code loop**: Two decision layers. **In combat**: `play_card(card, target)` — choose which card to play given current hand, mana, and enemy intent. **Between combats**: `pick_reward(card_options)` — choose which new card to add to your deck (or skip). The student writes both the battle AI and the deck construction strategy.
- **Depth gradient**: Level 1 — manual play, learn card effects. Level 2 — greedy play (play highest damage card each turn). Level 3 — enemy intent reading (enemy shows it will attack for 20 → play block cards first). Level 4 — combo detection (play card A to draw card B to trigger card C). Level 5 — deck construction theory (thin your deck by removing weak cards; add cards that synergize with existing ones, not just "good" cards). Level 6 — Monte Carlo simulation (simulate the next 3 fights with candidate deck builds, pick the path with highest survival rate). Level 7 — full run optimizer that path-selects through a branching map.
- **Visual payoff**: Cards fanning in hand, playing onto the field with effects, energy crystals depleting, enemy intent icons, combo chains lighting up, deck size growing/shrinking. The run map branching and the student's path highlighted. Card art with the CRT filter.
- **Teaching concepts**: Combinatorics (card synergies as graph edges), expected value under deck randomization, greedy vs. strategic card selection, the thin-deck principle (less cards = more consistency = better expected draws), simulation-based planning.
- **Why it fills a gap**: Poker and Blackjack are *gambling* — fixed decks, playing the odds. Deckbuilding is *construction* — the student shapes the deck over time. The strategic depth is in *what you add* and *what you remove*, not in what you're dealt. Slay the Spire is one of the most acclaimed indie games of all time and has spawned an entire genre. The coding challenge is unique: optimize a system that changes with every decision you make.

## Farm — Sim

**Seasonal farming on a grid plot. Plant crops, manage soil, track weather, optimize profit per season.**

- **Code loop**: `plant(crop, tile)`, `water(tile)`, `harvest(tile)`, `buy_seeds(crop, qty)`, `sell(crop, qty)`. Each day ticks one growth stage. Crops have different growth times, seasonal windows, sell prices, and water needs. The student writes a seasonal planner that maximizes profit across spring/summer/fall/winter.
- **Depth gradient**: Level 1 — manual planting, learn the crops. Level 2 — single-crop optimizer (which crop maximizes gold per day?). Level 3 — multi-crop rotation (plant fast crops first, then slow crops, to double-harvest a season). Level 4 — weather adaptation (rain forecast → skip watering → plant rain-loving crops → save time). Level 5 — soil quality management (some crops deplete soil, legumes restore it — multi-season sustainability). Level 6 — market price fluctuation (supply/demand shifts daily — time your sales to sell high). Level 7 — full-year planner that optimizes across all four seasons with seed investment vs. profit return.
- **Visual payoff**: Crops growing tile-by-tile through sprite stages, seasonal palette shifts (green spring → golden summer → orange fall → white winter), rain animations, harvest celebrations, gold counter climbing. The pixel-farm aesthetic is cozy and iconic.
- **Teaching concepts**: Scheduling and planning, ROI analysis per crop, constraint satisfaction (time windows, budget, plot space), multi-objective optimization, the concept of *opportunity cost* (every tile planted with crop A is a tile not planted with crop B).
- **Why it fills a gap**: Stardew Valley is one of the best-selling indie games ever. The farming loop is *pure optimization* — no combat, no reflexes, no randomness in the core mechanic. It's the most accessible optimization game possible. Students who think they "don't like programming" might find they love optimizing a farm. It's also the first game where **seasonal time pressure** is the core constraint — unlike Survival (day/night) or Idle (infinite time).

## Escape Room — Puzzle

**Single-screen room. Click objects, combine items, decode clues, unlock the exit.**

- **Code loop**: `inspect(object)`, `take(item)`, `combine(item_a, item_b)`, `use(item, object)`. Each room is a dependency graph: examining the painting reveals a code → code opens the safe → safe contains a key → key opens the drawer → drawer has the wire → wire fixes the circuit → door unlocks. The student writes a solver that discovers and resolves the dependency chain.
- **Depth gradient**: Level 1 — manual room, point-and-click exploration. Level 2 — exhaustive try-everything solver (try every item on every object). Level 3 — dependency graph extraction (model the room as a DAG, topological sort the solution order). Level 4 — inference from clues (the painting's numbers correspond to the safe combo — pattern matching). Level 5 — red herring detection (not every item is useful — prune the search). Level 6 — procedural room generation (auto-generate solvable rooms with guaranteed solution paths and optional red herrings).
- **Visual payoff**: Detailed single-screen room art with clickable hotspots highlighting, items collecting into inventory bar, combination animations, lock mechanisms animating open, the final door swinging wide. Each room is a self-contained visual scene.
- **Teaching concepts**: Dependency graphs (DAGs), topological sorting, state-space search with item combinations, pattern matching, procedural puzzle generation with guaranteed solvability.
- **Why it fills a gap**: The only game where the challenge is **dependency resolution** — figuring out the *order* in which things must happen. Every other puzzle game is about spatial reasoning or search. Escape Room is about *logical prerequisite chains*. And it's a massive real-world industry (escape rooms are a $1B+ market) with instant recognition.

## Management — Sim (or new category)

**Run a hospital / restaurant / theme park. Build rooms, hire staff, serve customers, keep everyone happy, don't go bankrupt.**

- **Code loop**: `build(room, position)`, `hire(staff_type)`, `set_price(service, amount)`, `assign(staff, room)`, `research(upgrade)`. Customers stream in with needs (illness → treatment room, hunger → kitchen, fun → ride). The student writes the operations AI: routing customers to services, staffing rooms, managing queues, pricing for profit.
- **Depth gradient**: Level 1 — manual building and assignment. Level 2 — auto-assign (nearest free staff to nearest waiting customer). Level 3 — queue optimization (prioritize high-value or most-urgent customers). Level 4 — layout optimization (minimize walking distances with adjacency planning). Level 5 — financial planning (balance staff costs vs. customer revenue, expand only when profitable). Level 6 — emergency response (disease outbreak → quarantine → reallocate staff). Level 7 — multi-floor / multi-building scaling.
- **Visual payoff**: Top-down or isometric building with rooms filling up, customers flowing through corridors, satisfaction meters floating, queues forming and clearing, money ticking, staff rushing between rooms. The visual feedback of a well-run vs. chaotic operation is immediate and legible.
- **Teaching concepts**: Queuing theory, resource allocation, constraint scheduling (staff shifts, room capacity), layout optimization (a graph problem), dynamic pricing, multi-objective optimization (profit vs. customer satisfaction vs. staff fatigue).
- **Why it fills a gap**: The only game where the student manages a **complex service system** with interacting human needs. Civ-Lite is macro-strategy (nations). Management is micro-operations (one building). The queuing theory and scheduling concepts map directly to real-world software engineering problems (load balancing, job scheduling, resource pools).

## Dogfight — Combat

**1v1 aerial combat. Biplanes or jets in a 2D wrap-around arena. Maneuver behind the opponent, fire when aligned.**

- **Code loop**: `thrust(power)`, `turn(degrees)`, `fire()`. The student writes pursuit logic: get behind the enemy, match their heading, lead the target, fire. The opponent does the same. It's a continuous-space chase problem where position and heading matter.
- **Depth gradient**: Level 1 — manual flight controls. Level 2 — direct pursuit (always turn toward enemy). Level 3 — lead pursuit (turn toward where the enemy *will be* based on their velocity). Level 4 — evasion (when the enemy is behind you, break away — barrel roll, split-S). Level 5 — energy management (trade altitude for speed, climb to gain advantage). Level 6 — combat state machine (engage → pursue → fire → disengage → reposition → re-engage). Level 7 — 2v2 wingman coordination.
- **Visual payoff**: Planes banking and chasing with contrails, tracer rounds streaming, explosions on hit, the satisfying moment of sliding into a perfect tail position. Radar display overlay. Retro biplane or wireframe jet aesthetic.
- **Teaching concepts**: Pursuit curves (pure vs. lead), vector projection (aiming ahead of moving targets), energy management as a resource, state machines for engagement phases, 2D physics with rotation and thrust.
- **Why it fills a gap**: Every combat game is ground-based. Dogfight adds the **2D flight model** — continuous rotation + thrust in an unbounded (wrapping) arena. The pursuit-curve math is a classic CS/robotics problem with no coverage elsewhere. And biplanes-on-CRT is an aesthetic home run.
