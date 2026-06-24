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

## Capture Tool Art

- Capture tools are grouped under `Capture Tools` in `Main.unity`.
- The net is `Capture Tools/Net Sweep Pivot/Net Sprite`. Replace `Assets/FishBalatro/Art/Generated/net.png` or add an Animator to `Net Sprite` to swap in final net animation.
- The claw is `Capture Tools/Claw Shot Hazard`, with `Claw Path` and `Claw Head` children.
- The electric pattern is `Capture Tools/Electric Wave Hazard`, with `Electric Wave 1` through `Electric Wave 5` children.
- `NetSweepHazard` moves and rotates the parent pivot, while the `Net Sprite` child handles the visible art and the `BoxCollider2D` catch area.

## UI Art Hooks

- Score-related UI is grouped under `Fish UI/Score UI`.
- Alert-related UI is grouped under `Fish UI/Alert UI`.
- Controls use a world-space `Controls Hint` object near the lower-right of the arena. It sits at a low sorting order so player fish, bait, and capture tools draw over it.
- The HUD uses replaceable pixel UI sprites in `Assets/FishBalatro/Art/Generated`: `ui_score_panel.png`, `ui_multiplier_panel.png`, `ui_alert_panel.png`, `ui_alert_segment.png`, `ui_controls_panel.png`, and `ui_keycap.png`.
- Existing text and bars still drive live values, but these containers and sprites give UI artists stable objects for animation, replacement sprites, and layout work.

## Big Fish Mechanic

The big fish is now a level-gating ally, not a banking/safe-zone mechanic. It
sits in the lower-left corner as a visual cue, but the player can trigger it
with `E` from anywhere.

- Level 1 attack cost: `240`
- Cost increases by `140` every level.
- Calling the big fish spends score, clears the current run state, resets Alert/combo pressure, makes the current fisherman flee, and starts the next level with fresh bait.

## Fisherman Types

Fishermen cycle by level: claw, electric, net, then repeat. The boat shape stays
the same, but each type has its own scene entity so artists can replace sprites
or add an Animator without touching gameplay code.

- Claw Fisherman: fires claw shots from the boat at `45`, `90`, and `135` degrees toward the sea floor.
- Electric Fisherman: sends slower horizontal electric waves downward one layer at a time, with larger fish-sized gaps between waves.
- Net Fisherman: swings a large sprite-based pendulum net across roughly half the screen.
- Dodging any capture tool keeps `TotalScore` and clears Alert/combo pressure.
- Touching any capture tool ends the run. Press `R` to reload the scene and start again.
- Generated net sprite: `Assets/FishBalatro/Art/Generated/net.png`.
- Generated claw sprite: `Assets/FishBalatro/Art/Generated/claw.png`.

Artist-facing fisherman entities live under `Fisherman Rig`:

- `Fisherman Rig/Claw Fisherman`
- `Fisherman Rig/Electric Fisherman`
- `Fisherman Rig/Net Fisherman`

Each one contains a `Fisherman Body`, `Boat`, `Line Anchor`, type-specific
`... Tool Prop`, `Notice`, and `FishermanName`. Only the current level's
fisherman is active during play.

## Code Map

- `FishGameManager`: central game state, scoring, fisherman type selection, capture tool resolution, and big fish level transition.
- `FishPlayerController`: small fish movement, dash, arena bounds, and input compatibility.
- `BaitPickup`: bait collision and the bait stat table.
- `BaitSpawner`: weighted bait spawning and level-based bait count.
- `NetSweepHazard`: sprite-based pendulum net timing and catch hitbox.
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
- The rebuild tool preserves existing PNG files in `Assets/FishBalatro/Art/Generated`, so replacing `player_fish.png`, `Fish_Large_1.png.png`, `net.png`, or `water_panel.png` will not be overwritten by generated placeholder art.
- Most tuning values are public fields on `FishGameManager`, `FishPlayerController`, and `BaitSpawner`.

## Next Best Upgrades

- Add bite input timing instead of automatic pickup on touch.
- Add fisherman variants per level.
- Replace generated placeholder sprites with final pixel art.
- Add sound effects for bait pops, warning pulse, net sweep, and big fish attack.
