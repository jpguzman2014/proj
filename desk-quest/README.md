# Desk Quest

An augmented reality prototype. Point a phone at a real desk, tap the desk, and a game
world appears anchored to it. Later milestones put a stick-man character on the desk and
turn real clutter into obstacles the player clears by tidying up for real.

This is a technical proof of concept, not a product. It has no accounts, no backend, no
analytics, no ads and no paid AI service, and it is built so it stays that way.

**Status: Milestone 1 complete, awaiting device testing.** See `MILESTONES.md`.

## Before you start: this repository has no Unity in it

The container this code was written in has no Unity installation, no .NET SDK and no
Android SDK, and its network policy blocks the Unity download hosts. Unity could not be
installed, so **no Unity build has ever been run on this code**. What was run is a
stub-based type check (`Tools/CompileCheck`), which catches syntax and signature errors
but cannot stand in for a real build. Expect to fix small things the first time you open
the project. Read `Tools/CompileCheck/README.md` for exactly what was and was not checked.

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

### iOS

The same scripts work on iOS with no changes. It needs a Mac with Xcode, an Apple
developer account for signing, and ARKit ticked under XR Plug-in Management for iOS. The
camera permission string is set for you. Android is the faster route to a first test and
is what the setup steps above assume.

## Testing Milestone 1 on the phone

Milestone 1 is: open the app, find a desk, tap it, and get a virtual object that stays
put. There is no character yet.

1. **Launch.** The camera feed fills the screen behind the interface. Android asks for
   camera permission the first time; allow it. Header reads DESK QUEST.
2. **Scan.** Status reads "Point your camera at your desk." Point at the desk and move
   the phone slowly side to side over it. A faint blue overlay should spread across the
   surface within a few seconds. Plain, shiny or dark surfaces take longer, and a plain
   white desk may never resolve; put a book or a mug on it and try again.
3. **Aim.** Once the surface is big enough, status reads "Desk found. Tap it to start."
   and a ring appears under the centre of the screen. It is amber while the surface under
   it is too small, green when it is usable.
4. **Place.** Tap the desk. The plane overlay disappears, a green rectangle marks the
   play area, and a blue cube sits at its centre turning slowly.
5. **The real test: anchoring.** Walk around the desk. Crouch. Look away, then look back.
   Move a metre away and return. The cube must stay on the same spot on the real desk,
   the right way up and the right size. Some drift when you look away and back is normal.
   The cube sliding across the desk, floating, or sinking through it is not.
6. **Rescan.** Press "Choose a different desk". Scanning restarts and you can place
   somewhere else.

### What to report back

Whether each of the six steps did what it says, the phone model and Android version, and
for anything that failed, what you saw instead. If nothing renders at all, or the screen
is black behind the interface, that is an XR Plug-in Management or graphics API problem
rather than a game bug; send the console output from **Build And Run**.

## Privacy

Camera frames are read by ARCore or ARKit on the device and used to track surfaces. The
game reads only the resulting geometry, never image data. Nothing is written to disk,
nothing is sent anywhere, and the app makes no network calls at all. There is no account,
no profile and no identifier.

Later milestones detect whether the desk has changed. That comparison is designed to run
on tracked geometry on the device, for the same reason. If a milestone ever needs
something stronger, it will be raised before it is built, not after.

## Layout

```
Assets/DeskQuest/
  Scripts/
    AR/         plane detection, tap-to-place, the anchored play area
    Core/       game state and the flow that drives it
    Input/      touch reading that works under either Unity input backend
    UI/         the heads-up display, built from code
    Common/     materials, colours, procedural meshes
  Editor/       scene builder and player settings, run from the Tools menu
Tools/
  CompileCheck/ type-check without Unity; read its README for what it proves
```
