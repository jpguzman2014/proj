# Milestones

Progress against the build plan.

**Nothing below has run on a phone.** The container this was written in cannot install
Unity, so no milestone has been through the build-and-test step the plan asks for. Where
something is marked verified, that means unit-tested here, not tested on a device.

## Status

| Milestone | State |
|---|---|
| 1 AR scene, tap to place, anchored | Written, untested on device |
| 2 Stick man on the desk | Written, untested on device |
| 3 Movement, gravity, collision | Written, movement rules unit-tested |
| 4 Run, jump, crouch, climb, vault | Written, rules unit-tested |
| 5 Obstacles and goal | Written, layout unit-tested |
| 6 Environment change detection | Written, cannot be tested without a desk |
| 7 Cleaning connected to gameplay | Written, cannot be tested without a desk |
| 8 Polish | Sound and completion screen done, rest not started |

25 unit tests cover the parts that are pure logic. They pass. Everything AR, everything
visual and everything about how it feels is unverified.

## What each milestone produced

### 1. AR scene

`ARSurfaceManager` restricts detection to horizontal planes and reports the largest one.
`ARDeskPlacement` raycasts the screen against detected planes, drives the aiming ring, and
on a tap creates an `ARAnchor` with the play area parented to it. `DeskQuestSceneBuilder`
assembles the scene through Unity's own XR menu commands.

### 2. Stick man

`StickManFactory` builds the character from Unity's primitives as a joint hierarchy, sized
from one height parameter. `StickManAnimator` poses those joints from code for all eight
states the brief lists.

Procedural rather than keyframed, and not as a compromise. Eight clips would need a rigged
model, an animator controller and a state machine to maintain, for a character nine
centimetres tall on a phone screen. A sine wave through the hips reads the same and is
retuned by changing a number.

### 3 and 4. Movement and actions

`CharacterMotor` handles walking, running, crouching, jumping, gravity, collision,
step-ups, climbing and vaulting. It is plain C# with no Unity dependency beyond vector
maths.

Not built on Unity physics or `CharacterController`. At nine centimetres the character is
close enough to Unity's default skin width and contact offsets to jitter, sink and tunnel.
A kinematic solver over axis-aligned boxes is predictable at that scale, cheaper on a phone
already tracking a room, and testable without Unity, which is why the movement rules are
the best-verified part of the project.

Combinations fall out of the design rather than being special cases: direction and run are
both held states read in the same frame.

### 5. Obstacles and goal

`ObstacleBox` carries position, size, type, height and cleared state. The five types differ
by data, not by class. An obstacle whose underside sits above the desk is an overhang to
crouch under, which falls out of the geometry rather than needing its own type.

### 6. Environment change detection

`DeskOccupancyProbe` asks whether the tracker can see desk surface at a point. ARCore and
ARKit grow a horizontal plane across flat surface they can see; an object standing on the
desk is not flat desk, so the plane stops at its edge. Take the object away and the polygon
grows to include the space it occupied. Coverage rising from low to high over a watched
patch is the signal.

`EnvironmentChangeDetector` requires a patch to read as occupied first, then to stay
covered for a sustained period, because a single high reading means nothing.

This is the least certain part of the project. It cannot tell what an object was, cannot
tell tidying from nudging aside, cannot distinguish removal from the player simply looking
at that patch properly for the first time, and cannot see a flat object the tracker already
treats as desk. It is a proxy for tidying up, not recognition, and the interface never
claims otherwise. It is also the part most likely to need rethinking once it meets a real
desk.

### 7. Cleaning connected to gameplay

`GameplayDirector` closes the loop: barrier blocks, real object leaves the desk, detector
fires, obstacle clears, path opens, character progresses, treasure completes the level.

The barriers span the whole desk so there is genuinely no way past without tidying, and a
red square is drawn on the desk showing which patch is being watched. Without that square
the player has no way to know where to tidy.

A manual fallback opens after 25 seconds on the same obstacle, letting the player tap the
block. Detection will fail on some desks and a level should never be unfinishable. It is
offered in plain sight rather than hidden, and how often it gets used is the measure of
whether milestone 6 actually works.

### 8. Polish, partly

Sound effects are generated as waveforms at runtime, so there are no audio files to
license or ship and a sound is retuned by changing a frequency. The completion screen and
celebration are in. Tutorial, effects and better animation are not.

## Held back on purpose

Grab, push, pick up, place, throw, roll, drawers, treasure interaction and checkpoints are
not built. Actions run through one input structure and one motor, so they attach to that
rather than needing a redesign.

The same goes for the other quests: Bedroom, Toy, Book, School Bag, Wardrobe, Room. Levels
are defined in fractions of the play area rather than in centimetres, so a different
surface is a different configuration rather than a different game.

## The honest risk list

1. **Change detection may not work well enough.** Plane growth is a real signal but a
   coarse one. If the fallback gets used every time, this needs rethinking, possibly using
   depth data where the device supports it.
2. **None of the AR code has run.** Plane detection, anchoring and the render pipeline are
   the parts a type check cannot touch.
3. **The feel is unknown.** Speeds, jump height, button sizes and character scale are
   reasoned from the maths, not from a thumb on a screen.
4. **Battery.** Plane detection keeps running during play because the detector needs it.
   That costs power, and how much is unmeasured.
