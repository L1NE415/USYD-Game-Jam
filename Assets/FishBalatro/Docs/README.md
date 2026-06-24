# Fish Balatro Starter

This Unity project is now a clean Fish Balatro starting point for the team.
The old game jam/platformer preparation files have been removed from `Assets`
and the playable scene is `Main.unity`.

## Scene

- Main playable scene: `Assets/Scenes/Main.unity`
- Rebuild tool: `Game Jam/Fish Balatro/Rebuild Main Scene`
- Open scene shortcut: `Game Jam/Fish Balatro/Open Main Scene`
- Add animation props: `Game Jam/Fish Balatro/Add Environment Animation Props`
- Missing sprite repair: `Game Jam/Fish Balatro/Repair Missing Sprite Artwork`
- Generated prototype assets: `Assets/FishBalatro/Art/Generated`

If Unity opens to an empty `Untitled` scene, open `Assets/Scenes/Main.unity`
or use the menu shortcut above.

## Current Loop

1. Swim around as the small fish.
2. Touch bait to steal it and build score, multiplier, next-bait bonuses, and Alert.
3. Score is usable immediately, but the current streak is still at risk.
4. Press `E` at any time to spend score and make the big fish attack the fisherman.
5. Each fisherman has a capture type: net, claw, or electric.
6. If Alert reaches 100%, the current fisherman uses their capture tool.
7. Swim or dash out of the capture pattern. Touching any tool ends the run.
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

## Animation Props

- `Water Wave` is a separate root object with `Water Wave Sprite` as its child. Teammates can animate the root for looping surface drift, bobbing, or parallax.
- `Seaweed` is a separate root object with `Seaweed Left`, `Seaweed Mid`, and `Seaweed Right` children. Teammates can animate the root or each clump independently.
- Placeholder sprites live at `Assets/FishBalatro/Art/Generated/water_wave.png` and `Assets/FishBalatro/Art/Generated/seaweed.png`.
- Use `Game Jam/Fish Balatro/Add Environment Animation Props` to add these objects to `Main.unity` without rebuilding the whole scene.

## Big Fish Mechanic

The big fish is now a level-gating ally, not a banking/safe-zone mechanic. It
sits in the lower-left corner as a visual cue, but the player can trigger it
with `E` from anywhere.

- Level 1 attack cost: `240`
- Cost increases by `140` every level.
- Calling the big fish spends score, clears the current run state, resets Alert/combo pressure, makes the current fisherman flee, and starts the next level with fresh bait.

## Fisherman Types

Fishermen cycle by level: net, claw, electric, then repeat. The boat stays the
same, but the fisherman color and label change so the player can read the next
threat before Alert fills.

- Net Fisherman: drops a large pendulum net across roughly half the screen.
- Claw Fisherman: fires claw shots from the boat at `45`, `90`, and `135` degrees toward the sea floor.
- Electric Fisherman: sends slower horizontal electric waves downward one layer at a time, with larger fish-sized gaps between waves.
- Dodging any capture tool keeps `TotalScore` and clears Alert/combo pressure.
- Touching any capture tool ends the run. Press `R` to reload the scene and start again.
- Generated claw sprite: `Assets/FishBalatro/Art/Generated/claw.png`.

## Code Map

- `FishGameManager`: central game state, scoring, fisherman type selection, capture tool resolution, and big fish level transition.
- `FishPlayerController`: small fish movement, dash, arena bounds, and input compatibility.
- `BaitPickup`: bait collision and the bait stat table.
- `BaitSpawner`: weighted bait spawning and level-based bait count.
- `NetSweepHazard`: runtime-built pendulum net visual, timing, and catch hitbox.
- `ClawShotHazard`: three-angle claw volley and claw hit detection.
- `ElectricWaveHazard`: top-to-bottom horizontal electric wave pattern.
- `BigFishAlly`: big fish prompt and attack animation.
- `FishermanController`: fisherman type appearance, warning visuals, and flee/return animation.
- `FishingLineView`: optional line telegraph helper, currently hidden in the net-sweep loop.
- `FishUIController`: copies game values into UI text and bars.
- `FloatingText`: temporary score/effect popups.
- `FishBalatroSceneBuilder`: editor rebuild tool for generated art, prefabs, and `Main.unity`.
- `FishBalatroStartupSceneLoader`: editor helper that opens `Main.unity` when the project starts on Unity's blank `Untitled` scene.

## Notes For Teammates

- There is no banking/safe-zone mechanic in this version.
- `TotalScore` is both the visible score and the currency used by the `E` attack.
- `CurrentRunScore` is the greed streak cleared when the fish dodges a capture tool; getting caught ends the run instead.
- Custom sprites assigned in `Main.unity` should stay untouched. The startup helper no longer auto-applies generated placeholder sprites, and the repair menu only fills objects whose SpriteRenderer is missing a sprite.
- The rebuild tool preserves existing PNG files in `Assets/FishBalatro/Art/Generated`, so replacing `player_fish.png`, `big_fish.png`, or `water_panel.png` will not be overwritten by generated placeholder art.
- Most tuning values are public fields on `FishGameManager`, `FishPlayerController`, and `BaitSpawner`.

## Next Best Upgrades

- Add bite input timing instead of automatic pickup on touch.
- Add fisherman variants per level.
- Replace generated placeholder sprites with final pixel art.
- Add sound effects for bait pops, warning pulse, net sweep, and big fish attack.
