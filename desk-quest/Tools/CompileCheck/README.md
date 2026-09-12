# Compile check

A type-check for the Desk Quest C# scripts that runs without Unity installed.

## Why this exists

The development container this project was written in has no Unity, no Unity Hub, no
.NET SDK and no Android SDK, and the network policy blocks the Unity and Microsoft
download hosts. There is no way to run a real Unity build here. The project brief asks
for a compile check after every milestone, so this is the closest honest substitute.

## What it actually does

`Stubs/` contains hand-written declarations of the Unity, AR Foundation and UnityEditor
types this project touches. `check.sh` compiles the game scripts against those stubs with
Mono's C# compiler, three times:

1. Runtime scripts with `ENABLE_INPUT_SYSTEM` defined (new Input System).
2. Runtime scripts without it (legacy Input Manager).
3. Runtime plus Editor scripts.

Both input passes matter because the input backend is a compile-time branch, and a bug
on the branch you are not using stays invisible until someone opens the project with the
other setting.

## What it proves, and what it does not

It catches syntax errors, typos, wrong member names, bad signatures, unreachable usings
and broken references **inside our own code**. That is the bulk of what breaks between
writing a script and opening Unity.

It does **not** prove the project compiles in Unity. The stubs are a reconstruction of
Unity's API from memory, so a stub can be wrong in the same direction as the code that
uses it, and both will agree while Unity disagrees. Treat a pass as "no obvious errors",
not as a green build. The real check is opening the project in Unity, and that has to
happen on a machine with Unity on it.

The stubs are also deliberately incomplete: they cover only what the project calls today
and grow as later milestones add calls.

## Running it

```bash
apt-get install -y mono-devel     # provides mcs
./check.sh
```

Exit status is the number of failed passes.
