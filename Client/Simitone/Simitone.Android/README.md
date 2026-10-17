# Simitone for Android

This folder contains the Android port of Simitone - the alternative C# client for
The Sims 1 that is based on [FreeSO](https://github.com/riperiperi/FreeSO).

The port keeps the engine intact: the SimAntics VM, the content system, the
renderer and the whole FreeSO/Simitone client logic are the *same* code that the
Windows build uses. Everything that is specific to Android lives in this project
(and in the self contained `Simitone.Touch` module).

> **You need your own copy of The Sims: Complete Collection.**
> No game assets are bundled with this project and none may be redistributed.
> The app only reads the files from the folder you select on your device.

---

## 1. Status

| Part | State |
|------|-------|
| Solution structure | complete (`Simitone.sln` contains `Simitone.Android` and `Simitone.Touch`) |
| Android launcher/setup screen | complete (SAF folder picker, validation, resolution, backend, 3D toggle, UI scale) |
| Path persistence & validation | complete (`SharedPreferences` + `GameData/Behavior.iff` check) |
| Touch control system | complete (module, icons, layout editor, profiles) |
| Engine / game logic | unchanged, shared with the desktop build |
| Device testing | **not done in this environment** - no Android device or emulator was available, so the port was written against the documented MonoGame 3.8.4 Android API but never executed on a device |

The last point is important: everything below is written from the MonoGame
sources and the FreeSO sources, but the build has not been executed end to end
(see [Limitations](#7-limitations--todo)).

---

## 2. Requirements

| Tool | Version |
|------|---------|
| .NET SDK | **9.0** (`net9.0-android`) |
| Android workload | `dotnet workload install android` |
| JDK | 17 (the one that ships with Visual Studio / Android Studio) |
| Android SDK | API 35 platform + build tools, `ANDROID_HOME` set |
| MonoGame | 3.8.4 (see `Directory.Build.props`, `MonoGameVersion`) |

```bash
dotnet workload install android
dotnet build Client/Simitone/Simitone.Android/Simitone.Android.csproj \
    -f net9.0-android -c Release -r android-arm64
```

The result is an APK in
`Client/Simitone/Simitone.Android/bin/Release/net9.0-android/android-arm64/`.

A universal APK (all architectures) is produced when no `-r` is given.

### Building the content first

The compiled FreeSO content (`.xnb`) has to exist before the Android project can
be built, because the APK assets are created from it:

```
FreeSO/TSOClient/tso.content/Content/OGL    <- OpenGL ES 3.0 shaders + fonts
FreeSO/TSOClient/tso.content/Content/iOS    <- OpenGL ES 2.0 shaders (compatibility)
FreeSO/TSOClient/tso.content/Content/Fonts  <- the vector fonts
```

They are compiled with the MonoGame Content Builder from
`FreeSO/TSOClient/tso.content/ContentSrc/TSOClientContent.mgcb`
(`ContentSrc/TSOClientContentiOS.mgcb` for the GLES2 variant).
The already compiled files are committed to the repository, so a normal build
works without running the content builder.

Everything that is read through `System.IO` (UI scripts, city data, meshes, the
CAS database, ...) is **not** compiled and therefore has to be a real directory.
The app copies those files out of the APK into its private storage on the first
start (see `Platform/AndroidContentInstaller.cs`).

---

## 3. Project layout

```
Client/Simitone/
├── Directory.Build.props        # MonoGameVersion, LangVersion for every project
├── Simitone.sln
├── Simitone.Client/             # shared client (net9.0)
│   └── UI/Touch/                # <-- the only new shared code (see section 6)
├── Simitone.Touch/              # self contained on-screen control module
└── Simitone.Android/            # <-- this project
    ├── LauncherActivity.cs      # the setup screen
    ├── MainActivity.cs          # hosts the MonoGame client
    ├── Platform/
    │   ├── SimitoneAndroidBootstrap.cs  # FSOEnvironment / GlobalSettings for Android
    │   ├── SimitoneAndroidSettings.cs   # the settings of the launcher
    │   ├── AndroidGamePathStore.cs      # SAF <-> path, validation, preferences
    │   ├── AndroidContentInstaller.cs   # unpacks the loose content
    │   ├── AndroidBitmapReader.cs       # image decoding without System.Drawing
    │   └── AndroidHaptics.cs           # vibration for the touch controls
    ├── Properties/AndroidManifest.xml
    └── Resources/               # icons, strings, themes
```

---

## 4. The setup screen

`LauncherActivity` is the launcher of the app and the setup screen. It is shown

* on the first start, and
* whenever the in-game **menu** button is pressed (the bridge forwards the
  `TouchAction.Menu` action to the app, which opens this activity).

It offers:

| Section | Content |
|---------|---------|
| Game files | "Choose folder..." (SAF `ACTION_OPEN_DOCUMENT_TREE`), "Enter the path manually", "Grant access to all files", a live validation message |
| Display | resolution presets (device native, 1280x720, 1600x900, 1920x1080, 2560x1440, custom W/H), graphics backend (OpenGL ES 3.0 / OpenGL ES 2.0 compatibility), 3D mode on/off, interface scale, haptic feedback on/off |
| Actions | "Launch game", "Reset the setup" |

Everything is persisted in the shared preferences of the app
(`simitone_setup`), so the game can be started directly from the launcher
after the first configuration.

### Path handling

FreeSO reads the game data through `System.IO`, so the game folder has to be a
real path. The SAF tree is therefore translated back into a file system path:

* `DocumentsContract.GetTreeDocumentId()` gives `volume:relative/path`,
* `primary` is mapped to `/storage/emulated/0`,
* removable volumes are resolved through `StorageManager` (their UUID is the
  volume id of the SAF),
* if the mapping fails the user can type the path by hand.

On top of that the app needs the **"all files access"** permission
(`MANAGE_EXTERNAL_STORAGE`) on Android 11+, because a plain `SAF` grant does not
give the process access to the raw files. The setup screen links to the system
settings and explains why it is needed.

The folder is validated by looking for the files of a The Sims 1 installation:

```
GameData/Behavior.iff   (required)
GameData/Gameplay.iff   (required)
GameData/Objects.iff    (optional, shown as "found")
TS1Archive.iff          (optional)
Downloads/  Music/  SFX/ (optional)
```

When something is missing the screen shows a precise error instead of letting
the game start with a broken installation.

### Resolution and DPI

MonoGame renders into the surface of the window, so on Android the *device*
resolution is what ends up on the screen. The engine draws its interface in a
logical resolution that is derived from the device resolution
(`FSOEnvironment.DPIScaleFactor`):

```
DPIScaleFactor = deviceWidth / requestedWidth
logicalWidth   = viewportWidth  / DPIScaleFactor   (= the requested width)
logicalHeight  = viewportHeight / DPIScaleFactor   (the aspect ratio of the device)
```

The height always follows the aspect ratio of the device so that nothing is
stretched. The interface scale additionally sets `FSOEnvironment.UIZoomFactor`,
which FreeSO uses for the pie menu and the UCP (the automatic value takes the
screen density into account, exactly like the legacy Android build did).

### Graphics backends

| Backend | `FSOEnvironment` | Content | Shaders |
|---------|------------------|---------|---------|
| OpenGL ES 3.0 (default) | `GLVer = 3`, `SoftwareDepth = false`, `UseMRT = true` | `Content/OGL/` | `Effects/Vitaboy`, `Effects/SpriteEffects`, ... |
| OpenGL ES 2.0 (compatibility) | `GLVer = 2`, `SoftwareDepth = true`, `UseMRT = false` | `Content/iOS/` | `Effects/VitaboyiOS`, `Effects/SpriteEffectsiOS`, ... |

The compatibility mode exists for devices whose driver does not support
`gl_FragDepth` or multiple render targets. Both content sets are part of the
APK.

---

## 5. The on-screen controls

The controls live in their own assembly, `Client/Simitone/Simitone.Touch`, which
does **not** depend on FreeSO or on Simitone.Client - it only knows MonoGame
primitives. See `Simitone.Touch/README.md` for the details.

The connection to the engine is made by
`Simitone.Client/UI/Touch/TouchControlBridge.cs`:

* an **input layer** is inserted at the front of `GameScreen.Layers`. It reads
  the fingers, removes the ones that belong to a control from
  `UpdateState.MouseStates` and replaces the (empty) keyboard state with the
  keys the controls want to press. The camera, the zoom, the rotation, the
  speed and the pause are therefore driven by the *normal* key handling of the
  engine - no game logic had to be touched.
* an **overlay layer** is appended at the end of the list, so the controls are
  drawn on top of the user interface.

The layout editor is opened with the "edit" button of the overlay (or with the
back button while the editor is open). It offers drag, resize, hide, opacity,
size, a grid snap and a profile browser. Profiles are stored as JSON in
`<UserDir>/touch/`.

---

## 6. Changes to the shared code

The port is deliberately invasive-free. The complete list of changes to code
that is not in `Simitone.Android` / `Simitone.Touch`:

| File | Change | Why |
|------|--------|-----|
| `Client/Simitone/Simitone.Client/SimitoneGame.cs` | 1 call in `Initialize()` and 1 call in `UnloadContent()` (both marked with a comment) | hooks the touch layers into the screen |
| `Client/Simitone/Simitone.Client/Simitone.Client.csproj` | one `ProjectReference` to `Simitone.Touch` | the bridge needs the module |
| `Client/Simitone/Directory.Build.props` / `.targets` | new files | centralise the MonoGame version for every project |

Nothing inside `FreeSO/` was modified - the submodule is untouched.

---

## 7. Limitations / TODO

* **No device test.** The port was written against the MonoGame 3.8.4 Android
  sources; it was never started on a phone or in an emulator (no Android SDK
  and no device were available in the development environment). Expect to have
  to fix the first-run experience.
* **The SAF path mapping is a heuristic.** Folders that are not on the primary
  volume and not on a removable volume with a known UUID cannot be mapped and
  have to be entered by hand. A real solution needs a file system abstraction
  inside FreeSO (there is none today - it uses `System.IO` directly).
* **The resolution presets change the logical resolution, not the frame
  buffer.** The 3D world is always rendered with the resolution of the screen.
* **Some of the optional controls do nothing yet.** The buttons for the
  inventory, the family panel, the skills panel and the free camera are part of
  the layout, but the client does not expose an API for them yet; they show a
  short message instead.
* **The built-in touch helper of FreeSO is still active.** It handles the
  pie menu and the scrolling of the lot. It works together with the overlay
  (the overlay only swallows the fingers that hit a control), but the two
  systems can feel redundant on a phone.
* **Audio** uses the normal OpenAL implementation of MonoGame; the sound
  effects of the game are not verified on a device.
* **The Vulkan/Metal backends** of MonoGame are not used - the port targets
  OpenGL ES, like the rest of FreeSO.

---

## 8. Legal

Simitone is a fan project. It contains no game assets and does not include any
part of The Sims 1. Using it requires a legitimate copy of
*The Sims: Complete Collection*, which you have to provide yourself.
