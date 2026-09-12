# Desk Quest

An augmented reality prototype. Point a phone at a real desk, tap the desk, and a game
world appears anchored to it. Later milestones put a stick-man character on the desk and
turn real clutter into obstacles the player clears by tidying up for real.

This is a technical proof of concept, not a product. It has no accounts, no backend, no
analytics, no ads and no paid AI service, and it is built so it stays that way.

**Status: Milestones 1 to 7 written, none tested on a phone.** See `MILESTONES.md`.

## Before you start: this repository has no Unity in it

The container this code was written in has no Unity installation, no .NET SDK and no
Android SDK, and its network policy blocks the Unity download hosts. Unity could not be
installed, so **no Unity build has ever been run on this code and it has never run on a
phone**.

Two things were run in its place:

- A stub-based type check across both Unity input backends, which catches syntax and
  signature errors but is not a build.
- 35 unit tests over the movement rules, the level layout and the change-detection
  decision, which are real tests. Those three are written without any Unity dependency
  precisely so they can be tested, so walking, jumping, crouching under overhangs,
  climbing, vaulting and obstacle clearing are genuinely verified, as is the fact that the
  barriers cannot be walked around or jumped over, and that a noisy coverage signal does
  not open the path by accident.

What neither can tell you is whether the AR half works, or whether any of it feels good
in the hand. Expect to fix small things the first time you open the project. Read
`Tools/CompileCheck/README.md` for exactly what was and was not checked.

Everything below has to happen on a machine with Unity installed.

## Setting up

### 1. Create the Unity project

In Unity Hub, create a new project from the **AR** template (called *AR Mobile* or *AR
Core* depending on Hub version) using **Unity 6 LTS (6000.0.x)**.

Use the AR template rather than a blank 3D project. It ships AR Foundation, the ARCore
and ARKit providers and XR Plug-in Management already installed, versioned to match each
other, and with the Android XR provider already enabled. Wiring that by hand is where
most AR projects lose their first day.

Unity 2022.3 LTS with AR Foundation 5.1 also works; the code avoids the AR Foundation
APIs that changed between 5 and 6.

If you must start from a blank project, install these packages from Package Manager and
let it choose versions: AR Foundation, Google ARCore XR Plugin, Apple ARKit XR Plugin
(macOS only), XR Plugin Management.

### 2. Copy the game in

Copy `desk-quest/Assets/DeskQuest` from this repository into your Unity project's
`Assets/` folder, so you end up with `Assets/DeskQuest/`. Unity will import and compile
it. Fix any compile errors before continuing; if there are any, they are almost certainly
an AR Foundation API that moved, and the error will name it.

### 3. Build the scene

Open the AR template's scene, then run **Tools > Desk Quest > Set Up Scene**.

That command adds the plane, raycast and anchor managers to the XR Origin, creates a
plane visualiser so detected surfaces are actually drawn, and creates the Desk Quest
object with the game logic and interface on it. It reuses whatever is already in the
scene, so running it twice is safe.

Save the scene, and make sure it is the enabled scene in **File > Build Settings**.

### 4. Configure for Android

Run **Tools > Desk Quest > Configure Player Settings (Android)**. This sets the minimum
API level to 24, IL2CPP, ARM64, OpenGLES3, portrait orientation and the iOS camera
permission text.

Then run **Tools > Desk Quest > Validate Setup** and read the console. Anything it cannot
confirm is listed with what to do about it. The one setting it deliberately does not
change is **Project Settings > XR Plug-in Management > Android > ARCore**, which you tick
yourself; the AR template has it ticked already.

### 5. Build to the phone

Enable developer mode and USB debugging on the phone, connect it, then **File > Build
Settings > Android > Build And Run**.

### Getting later fixes

The Unity project lives outside this repository, because a Unity project contains
generated files that cannot be produced without Unity installed. So when a fix lands here,
the scripts have to be copied across again.

```
git pull
.\desk-quest\Tools\sync-to-unity.ps1 -UnityProject "C:\path\to\your\unity\project"
```

Copying the folder by hand in Explorer works too. Choose replace when Windows asks.
**Never delete the DeskQuest folder first.** Unity stores the scene's references to your
scripts as ids kept in .meta files alongside them; delete the folder and Unity issues new
ids, the scene loses every component, and the game stops working with no obvious cause.
Copying over the top leaves those files alone.

### iOS

The same scripts work on iOS with no changes. It needs a Mac with Xcode, an Apple
developer account for signing, and ARKit ticked under XR Plug-in Management for iOS. The
camera permission string is set for you. Android is the faster route to a first test and
is what the setup steps above assume.

## Testing on the phone

Before you start, put two or three small objects on the desk: a book, a toy, a pencil
case. The game needs real clutter to work with, and a bare desk makes the last step
impossible to test.

**Finding the desk.**

1. **Launch.** The camera feed fills the screen behind the interface. Android asks for
   camera permission the first time; allow it. Header reads DESK QUEST.
2. **Scan.** Status reads "Point your camera at your desk." Point at the desk and move
   the phone slowly side to side. A faint blue overlay should spread across the surface
   within a few seconds. Plain, shiny or dark surfaces take longer; a bare white desk may
   never resolve, which is another reason to have objects on it.
3. **Aim and place.** Once the surface is big enough the status changes and a ring appears
   under the centre of the screen, amber when the surface is too small and green when it
   is usable. Tap the desk.

**Anchoring, which everything else depends on.**

4. Walk around the desk, crouch, look away and back, step a metre away and return. The
   character and the blocks must stay on the same spot on the real desk, upright and the
   same size. Some drift when you look away and back is normal. Sliding across the desk,
   floating, or sinking through it is not.

**Playing.**

5. **Move.** The four direction buttons walk the stick man. UP is away from you, whichever
   way you are holding the phone, because the play area is oriented at the moment you tap.
6. **Actions.** Hold RUN with a direction to sprint. Hold CROUCH with a direction to
   crouch-walk. JUMP over the amber block. CLIMB next to the green block to climb it, and
   press CLIMB in open space to confirm it gives a small blip rather than doing nothing
   silently.
7. **The red barriers.** These span the whole desk and cannot be jumped, climbed or walked
   around. Each one has a red square drawn on the desk in front of it. The objective line
   names what to take off that square.

**The mechanic the whole thing is for.**

8. **Tidy up.** Physically take the real object off the marked square and put it somewhere
   else. Keep the camera pointed at that patch of desk for a few seconds afterwards, so the
   tracker can see the surface that was underneath. The square should tint from red towards
   amber as the desk becomes visible, then the barrier shrinks away with a sound.
9. **Progress.** Walk the stick man through the gap and repeat for the second barrier.
10. **Finish.** Walk up to the treasure. It grows and the objective line asks you to press
    USE. Press it. The character celebrates and DESK QUEST COMPLETE appears. If the USE
    button does nothing, the treasure opens on its own after four seconds, and that is
    worth telling me about because it means the interact button is not working.

If a barrier does not clear after about 25 seconds of the desk being genuinely clear, the
objective line changes to offer a fallback and you can tap the red block to clear it by
hand. That fallback exists so a level is never unfinishable. **If you find yourself using
it every time, the change detection is not working on your desk, and that is the single
most important thing to tell me.**

### The diagnostics panel

There is a small **i** button in the top-left corner. It opens a readout of what the game
thinks is happening, and it is the fastest way to turn "it did not work" into something
fixable. It shows:

- **session** and **state**: whether AR is tracking, and which phase the game is in.
- **planes** and **best surface**: how much of the desk the tracker has found.
- **character**: what the stick man is doing and where he is.
- **cover** per barrier, with **NO BASELINE**, **armed** or **cleared**.

That last line is the important one. `NO BASELINE` means the game never saw anything on
that patch of desk, so it cannot detect it being removed; point the camera at the square
for a few seconds. `armed` with a coverage number that stays low means the tracker is not
seeing the desk surface even though the object has gone.

### What to report back

- Phone model and Android version.
- Which of the ten steps worked and which did not, and what you saw instead.
- For step 8 especially: a photo or reading of the diagnostics panel while you are stuck.
  The coverage number and whether it says armed is exactly what I need.
- If nothing renders at all, or the screen is black behind the interface, that is an XR
  Plug-in Management or graphics API problem rather than a game bug. Send the console
  output from **Build And Run**.

If the game misbehaves before you get that far, run **Tools > Desk Quest > Validate Setup**
in Unity and send the console output. It checks the scene and the build settings and names
what is missing.

## Privacy

Camera frames are read by ARCore or ARKit on the device and used to track surfaces. The
game reads only the resulting geometry, never image data. Nothing is written to disk,
nothing is sent anywhere, and the app makes no network calls at all. There is no account,
no profile and no identifier.

Detecting that the desk has changed uses the plane geometry the AR session already
produces, on the device. No camera image is read by the game, and none is stored or sent.
There is no object recognition anywhere in the project and nothing in the interface claims
there is.

## Layout

```
Assets/DeskQuest/
  Scripts/
    AR/         plane detection, tap-to-place, the anchored play area
    Gameplay/   movement, obstacles, level layout. No Unity scene types, so it is testable
    Character/  the stick man: built from primitives, animated from code
    Obstacles/  obstacle and goal objects, and the clear-zone markers
    Detection/  reading change in the real desk from plane geometry
    Core/       game state and the flow that drives it
    Input/      touch reading that works under either Unity input backend
    UI/         heads-up display and on-screen controls, built from code
    Audio/      sound effects generated as waveforms at runtime
    Common/     materials, colours, procedural meshes
  Editor/       scene builder and player settings, run from the Tools menu
Tools/
  CompileCheck/ type check and unit tests without Unity; read its README for what it proves
```

There are no prefabs, materials, textures, models, animation clips or audio files. Every
asset is built in code at runtime, which is why the project survives being copied between
machines and Unity versions.
