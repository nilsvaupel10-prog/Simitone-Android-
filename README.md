![image](https://user-images.githubusercontent.com/6294155/29047328-2bb60b54-7bc3-11e7-8d88-cee5309495ed.png)

[Latest Pre-release](https://github.com/riperiperi/Simitone/releases/latest/) | [Download](https://github.com/riperiperi/Simitone/releases/latest/download/SimitoneWindows.zip)

Alternative C# Windows Frontend for The Sims 1, based off of FreeSO. http://freeso.org 
(***REQUIRES*** a legitimate copy of The Sims: Complete Collection)

![image](https://user-images.githubusercontent.com/6294155/68995217-112b2680-0883-11ea-9f92-1acc839a7ec0.png)

NOTE! Currently does not support the entire Fame career track, saving on vacation and a few other important things. While all objects run, many of them have bugs that can make certain lot types unplayable. For current development progress, see [this issue.](https://github.com/riperiperi/Simitone/issues/8)

*Only for Desktop Windows.* Other platforms cannot be officially supported.

# Android port

This fork contains an **experimental Android port** of Simitone. It uses the
modern .NET Android stack (`net9.0-android`) with MonoGame and keeps the
engine, the SimAntics VM, the content system and the whole client logic
untouched.

* The app starts with a setup screen where the folder of your copy of
  *The Sims: Complete Collection* is selected (Storage Access Framework), the
  resolution, the graphics backend (OpenGL ES), the 3D mode and the interface
  scale are configured and the path is validated.
* A complete, editable system of on-screen controls (virtual stick, camera
  buttons, speed controls, ...) with real icons, a layout editor and savable
  profiles. The controls are translated into the key presses the engine
  already understands, so no game logic had to be changed.

Build and design notes: [`Client/Simitone/Simitone.Android/README.md`](Client/Simitone/Simitone.Android/README.md)
Touch module: [`Client/Simitone/Simitone.Touch/README.md`](Client/Simitone/Simitone.Touch/README.md)

The port has **not been tested on a device yet** - it was written against the
MonoGame 3.8.4 sources, but no Android SDK or device was available. You still
need your own copy of *The Sims: Complete Collection*; no game assets are
included.


# Purpose

On modern operating systems, The Sims has a few nagging issues that make it less than playable. Simitone can be seen as a tool to improve the playability of your existing installation of The Sims. Here are some features:

- Playable in 3D - using a combination of models generated from the 2D sprites and created by the community.
  - [Download the mesh pack here!](http://forum.freeso.org/forums/3d-remeshing.40/)
- Custom user interface that works at modern resolutions. Working on a more desktop oriented interface.
- Improved graphical performance, support for high resolutions and refresh rates.
- Custom lighting - directional lights with smooth falloffs and shadows using generated 3D meshes.
- *Volcanic*, a program which allows you to examine, modify and create new game objects. (from FreeSO)

# Why is it called Simitone?

Simitone -> Semitone -> musical term -> C# -> a note

Further questions can be directed at my PR manager, uh, ... burglar cop.
