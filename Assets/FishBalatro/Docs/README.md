# Fish Balatro Starter

This Unity project is now a clean Fish Balatro starting point for the team.
The old game jam/platformer preparation files have been removed from `Assets`
and the playable scene is `Main.unity`.

## Scene

- Main playable scene: `Assets/Scenes/Main.unity`
- Rebuild tool: `Game Jam/Fish Balatro/Rebuild Main Scene`
- Open scene shortcut: `Game Jam/Fish Balatro/Open Main Scene`
- Generated prototype assets: `Assets/FishBalatro/Art/Generated`

If Unity opens to an empty `Untitled` scene, open `Assets/Scenes/Main.unity`
or use the menu shortcut above.

## Current Loop

1. Swim around as the small fish.
2. Touch bait to steal it and build score, multiplier, next-bait bonuses, and Alert.
3. Score is usable immediately, but the current streak is still at risk.
4. Press `E` at any time to spend score and make the big fish attack the fisherman.
5. If Alert reaches 100%, the fisherman drops a large pendulum net.
6. Swim or dash out of the sweeping net before it crosses the water.
7. Dodging keeps your total score and clears the current at-risk streak. Getting netted ends the run.
8. After a big fish attack, the fisherman flees and a new fisherman arrives as the next level.

## Controls

- `WASD` / arrow keys: swim
- `Shift`: burst dash
- `E`: spend score to attack the fisherman
- `R`: restart after getting caught

## Bait Effects

- Worm: `+10`, `+10 Alert`
- Shrimp: next scoring bait `x2`, `+15 Alert`
- Glow Bug: `+1 Multiplier`, `+20 Alert`
- Small Fish: repeats the previous bait effect, `+20 Alert`
- Golden Shrimp: `+100`, `+35 Alert`
- Fake Bait: `+0`, `+50 Alert`

## Bait Spawning

- Each level has a finite bait budget, so bait is not an infinite score source.
- The spawner keeps about `6-8` bait pieces active, depending on level.
- Bait must spawn away from the player and away from other bait, which prevents accidental chain pickups.
- Bait is blocked from the lower-left big fish area, so it will not appear under the ally sprite or prompt.
- If a level runs completely dry and the player still cannot afford an attack, the spawner gives one small emergency refill.

## Big Fish Mechanic

The big fish is now a level-gating ally, not a banking/safe-zone mechanic. It
sits in the lower-left corner as a visual cue, but the player can trigger it
with `E` from anywhere.

- Level 1 attack cost: `240`
- Cost increases by `140` every level.
- Calling the big fish spends score, clears the current run state, resets Alert/combo pressure, makes the current fisherman flee, and starts the next level with fresh bait.

## Net Sweep Hazard

Alert is no longer a grab minigame. At `100%` Alert, the fisherman
briefly warns the player, then a large net sweeps across roughly half of the
screen like a pendulum.

- The net is generated at runtime by `NetSweepHazard`, so no separate net art is required yet.
- During the yellow warning phase, the net is visible but not dangerous.
- During the cyan sweep phase, touching the net catches the player and ends the run.
- If the player dodges the sweep, `TotalScore` is kept and Alert/combo pressure resets.
- After getting caught, press `R` to reload the scene and start again.

## Code Map

- `FishGameManager`: central game state, scoring, net sweep resolution, and big fish level transition.
- `FishPlayerController`: small fish movement, dash, arena bounds, and input compatibility.
- `BaitPickup`: bait collision and the bait stat table.
- `BaitSpawner`: weighted bait spawning and level-based bait count.
- `NetSweepHazard`: runtime-built pendulum net visual, timing, and catch hitbox.
- `BigFishAlly`: big fish prompt and attack animation.
- `FishermanController`: fisherman warning visuals and flee/return animation.
- `FishingLineView`: optional line telegraph helper, currently hidden in the net-sweep loop.
- `FishUIController`: copies game values into UI text and bars.
- `FloatingText`: temporary score/effect popups.
- `FishBalatroSceneBuilder`: editor rebuild tool for generated art, prefabs, and `Main.unity`.
- `FishBalatroStartupSceneLoader`: editor helper that opens `Main.unity` when the project starts on Unity's blank `Untitled` scene.

## Notes For Teammates

- There is no banking/safe-zone mechanic in this version.
- `TotalScore` is both the visible score and the currency used by the `E` attack.
- `CurrentRunScore` is the greed streak cleared when the fish dodges the net; getting caught ends the run instead.
- Most tuning values are public fields on `FishGameManager`, `FishPlayerController`, and `BaitSpawner`.

## Next Best Upgrades

- Add bite input timing instead of automatic pickup on touch.
- Add fisherman variants per level.
- Replace generated placeholder sprites with final pixel art.
- Add sound effects for bait pops, warning pulse, net sweep, and big fish attack.
