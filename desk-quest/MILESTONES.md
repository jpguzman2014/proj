# Milestones

Progress against the build plan. Each milestone stops for device testing before the next
one starts.

## Milestone 1 — AR scene (code complete, untested on device)

Open the app, detect a horizontal surface, tap it, get a virtual object anchored to it.

| Criterion | State |
|---|---|
| App launches, camera runs | Implemented, untested on device |
| Horizontal plane detected and drawn | Implemented, untested on device |
| Player can select the desk | Implemented, untested on device |
| Virtual object appears anchored | Implemented, untested on device |

Nothing here has run on a phone. Testing steps are in `README.md`.

### What was built

- `ARSurfaceManager` restricts detection to horizontal planes and reports the largest one
  and whether it is big enough to be a desk.
- `ARDeskPlacement` raycasts the screen against detected planes, drives the aiming ring,
  and on a tap creates an `ARAnchor` with the play area parented to it.
- `DeskPlayArea` draws the boundary and the marker cube. Later milestones parent the
  stick man, obstacles and goal to this same transform, so they inherit the anchor.
- `ARGameManager` holds the state machine and is the only thing that decides what the
  player is told.
- `UIManager` and `UIFactory` build the interface from code, with no prefabs, sprites or
  font assets.
- `DeskQuestSceneBuilder` assembles the scene through Unity's own XR menu commands.

### Decisions worth knowing about

**Planes are polled, not subscribed to.** AR Foundation renamed the plane-changed event
between 5.x and 6.x. Polling a handful of trackables four times a second costs nothing
and works on both versions.

**Anchors are created by adding the component**, not through the anchor manager's helper
methods, which were also renamed between 5 and 6.

**Input goes through `TouchInput`.** The AR template uses the new Input System, where
legacy `Input` calls throw at runtime. Routing every read through one file means the game
works under either backend.

**Materials are created at runtime from built-in shaders.** No material assets, and the
same code works under the Built-in and Universal render pipelines.

**The play area is yawed away from the camera** at the moment of placement, so the UP
button will mean "away from the player" when movement arrives in Milestone 3.

## Milestone 2 — Stick man

3D stick man from primitives, anchored to the desk, with an idle animation. Built by a
factory so a prefab can be baked from it later. Procedural animation first; keyframed
clips only if procedural is not enough.

## Milestone 3 — Movement

Four-direction movement relative to the play area, gravity, ground detection, collision,
and the on-screen direction pad. Movement is driven through a small input interface so
the pad, and later anything else, feed the same controller.

## Milestone 4 — Actions

Run, jump, crouch, climb and vault, plus the combinations: direction with run, direction
with crouch, direction with jump, and running jump. Climb does nothing but give feedback
when there is no obstacle in reach.

## Milestone 5 — Obstacles and goal

A modular obstacle with position, size, type, height and traversal requirements, so
`LOW_OBSTACLE`, `HIGH_OBSTACLE`, `WALL`, `CLIMBABLE` and `BLOCKED_PATH` differ by data
rather than by class. Obstacles are placed by hand at this stage. The goal detects arrival
and completes the level.

## Milestone 6 — Environment change detection

Detect that the real desk area has materially changed, on the device, using tracked
geometry rather than image recognition. This is the milestone with the most technical
risk and the one most likely to need its scope cut. It will be scoped in detail before it
is started, and what it can and cannot detect will be stated plainly rather than implied.

## Milestone 7 — Cleaning connected to gameplay

Removing a real object clears the virtual obstacle in that area and opens the path.

## Milestone 8 — Polish

Better animation, sound, effects, tutorial and success screen. Only after the loop works.

## Held back on purpose

Grab, push, pick up, place, throw, roll, drawers, treasure and checkpoints are not built.
The action system in Milestone 4 is structured so they attach to it later without being
designed for now.

The same goes for the other quests: Bedroom, Toy, Book, School Bag, Wardrobe and Room.
Nothing in the code assumes a desk beyond the play area's default size, so a different
surface is a different configuration rather than a different game.
