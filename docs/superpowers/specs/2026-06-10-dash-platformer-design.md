# Dash Platformer Design

## Purpose

Convert the existing Uni-Run Unity example from an auto-runner style project into a short, playable stage-clear 2D platformer prototype. The new core loop is horizontal movement, mouse-aimed dash, dash recovery through landing or recharge pickups, and reaching a goal.

## Current Project Context

The project at `D:\work\Uni-Run` is a Unity 2D project. It currently behaves like an auto-runner example:

- The background is designed to loop endlessly.
- Platforms move backward to create forward-motion feel.
- The player position is mostly fixed horizontally.
- The player currently jumps on the Y axis to pass platforms.
- Platform auto-generation logic exists as a stub and is not implemented.
- Existing assets include `PlayerController.cs`, `GameManager.cs`, `ScrollingObject.cs`, `BackgroundLoop.cs`, `PlatformSpawner.cs`, `Platform.prefab`, `Main.unity`, and temporary animations such as `Run`, `Jump`, and `Die`.

The requested game direction differs significantly from the runner structure, so the first implementation should convert the project to a player-centered stage structure instead of extending runner behavior.

## Chosen Approach

Use a player-centered stage-clear structure.

- Disable or stop relying on runner-style background/platform movement for the first prototype.
- Let the player move through world coordinates directly.
- Reuse existing Unity project assets where practical.
- Replace jump-focused player control with dash-focused control.
- Keep the implementation small enough to produce a short playable stage.

Rejected approaches:

- Minimal runner modification: fast, but conflicts with the requested stage-clear platformer direction.
- Separate new prototype scene: safer, but less useful for converting the existing example and adds extra scene management work.

## Gameplay Scope

The first playable stage includes:

- A fixed start point.
- Static ground and platforms.
- A player character controlled by keyboard and mouse.
- Hazards and a fall death zone.
- Dash recharge pickups placed in the air.
- A goal object that clears the stage.
- Game over and clear UI states.
- Restart input after game over or clear.

Out of scope for the first stage:

- Procedural platform generation.
- Final pixel-art sprite production.
- Multiple stages.
- Save data.
- Advanced input rebinding UI.
- Full game menu flow.

## Player Controls

Movement:

- `A` / `D` or left/right arrow keys move the player horizontally.
- There is no jump action.

Aiming:

- Mouse position determines the dash direction.
- Mouse aiming is the only aiming input in the first implementation. Gamepad aiming is out of scope for this prototype.

Dash:

- Left mouse button or right mouse button performs a dash.
- `Space` does not dash and is not used for jump.
- The dash travels in the aimed direction.
- Dash is short, fast, and deliberate.
- During dash, normal horizontal control is reduced or temporarily ignored so the dash feels consistent.
- After dashing, the player cannot dash again until dash is recharged.

Dash recharge:

- Landing on ground or platform restores dash availability.
- Touching a dash recharge pickup restores dash availability even in the air.
- A recharge pickup deactivates after it is collected.

## Player State

The player controller should track:

- `isGrounded`: true when standing on a valid surface.
- `isDashing`: true during dash movement.
- `canDash`: true when dash is available.
- `isDead`: true after trap or death-zone contact.
- `isControlLocked`: true while dead, clear, or dashing, used to prevent conflicting input.

Ground detection should use collision contact normals or a small ground check. The implementation should avoid treating wall contact as landing.

## Animation Design

Use temporary or existing sprites/animations first, but organize Animator state around the target game.

Target animation states:

- `Idle`: grounded with no horizontal input.
- `Run`: grounded with horizontal movement.
- `Dash`: currently dashing.
- `Fall`: airborne and not dashing.
- `Die`: dead.

Suggested Animator parameters:

- `Grounded` bool.
- `Speed` float.
- `IsDashing` bool.
- `VerticalSpeed` float.
- `Die` trigger.

Existing `Run`, `Jump`, and `Die` animations may be reused temporarily. `Jump` can stand in for `Fall` until proper pixel-art dash/fall clips are added.

## Scene Objects

Required objects:

- `Player`: `Rigidbody2D`, `Collider2D`, `Animator`, `AudioSource`, dash-focused `PlayerController`.
- `Main Camera`: follows the player with a simple `CameraFollow` script.
- `Ground` / `Platform`: static colliders used for movement and dash recharge on landing.
- `DashRecharge`: trigger pickup that restores dash and then deactivates.
- `Trap`: trigger hazard that kills the player.
- `DeadZone`: trigger below the stage that kills the player.
- `Goal`: trigger that clears the stage.
- `GameManager`: tracks playing, game-over, and clear states.
- UI: game-over UI and clear UI.

Tags or components:

- Keep existing `Dead` and `Trap` tags if already configured.
- Add a goal trigger using either a `Goal` tag or a `GoalTrigger` component.
- Use a dedicated `DashRechargePickup` component for recharge pickups.

## Game Flow

Normal play:

1. Player starts at the stage start point.
2. Player moves left/right and aims dash with the mouse.
3. Player uses left mouse button or right mouse button to dash.
4. Dash becomes unavailable after use.
5. Landing restores dash.
6. Recharge pickups can restore dash in the air.
7. Player reaches the goal to clear the stage.

Failure:

1. Player touches a trap or death zone.
2. Player dies.
3. GameManager enters game-over state.
4. Game-over UI appears.
5. Restart input reloads the scene.

Clear:

1. Player touches the goal.
2. GameManager enters clear state.
3. Player control stops.
4. Clear UI appears.
5. Restart input reloads the scene.

## Script Changes

`PlayerController.cs`:

- Remove jump-count and jump-force behavior.
- Add horizontal movement.
- Add mouse-aimed dash.
- Add dash availability and recharge handling.
- Keep death handling.
- Update Animator parameters.

`GameManager.cs`:

- Keep singleton structure.
- Keep game-over behavior.
- Add clear state.
- Add clear UI reference.
- Restart on input when game-over or clear.

New `CameraFollow.cs`:

- Follow player position smoothly.
- Preserve camera Z position.
- Use a fixed follow offset and no camera clamping in the first implementation.

New `DashRechargePickup.cs`:

- On player trigger, restore dash.
- Deactivate pickup after collection.

New `GoalTrigger.cs`:

- On player trigger, notify `GameManager` that the stage is clear.

## Verification Criteria

The prototype is successful when:

- The project compiles without Unity C# errors.
- `A` / `D` and left/right arrows move the player horizontally.
- Jump input no longer makes the player jump.
- Left mouse button and right mouse button trigger dash.
- `Space` does not trigger dash or jump.
- Dash travels toward the mouse position.
- The player cannot dash repeatedly in the air without recharge.
- Landing restores dash.
- Dash recharge pickup restores dash while airborne.
- Trap or death-zone contact causes game over.
- Goal contact causes clear state.
- Restart input works from game-over and clear states.
- Temporary animation states update consistently enough to inspect behavior.

## Implementation Notes

Scene editing may require Unity Editor automation. If requested, Coplay Unity MCP can be installed and connected for object placement, component wiring, and Play Mode checks. Until then, C# scripts and manual scene setup instructions can be prepared without editor automation.

The project is not currently a git repository, so the design document cannot be committed unless git is initialized or the project is placed under version control.

