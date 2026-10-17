# Simitone (Android Port)

Simitone is an interactive client and simulation front-end for *The Sims 1*, inspired by FreeSO and the Simitone project. This rewrite brings the full Android touch client experience to the web and mobile devices, preserving the core engine structure, touch control system, launcher configuration, live lot simulation, and Create-A-Sim (CAS) studio.

## Features

### 1. Android Launcher & Setup Screen (`LauncherActivity`)
- **Game Files Management**: Folder path selector (Storage Access Framework URI mapping), manual path input (`/storage/emulated/0/The Sims`), file validation for `GameData/Behavior.iff`, `Gameplay.iff`, `Objects.iff`, and `TS1Archive.iff`.
- **All Files Access**: `MANAGE_EXTERNAL_STORAGE` permission grant simulation.
- **Display Configurations**:
  - Resolution presets: Native, 1280x720 (720p), 1600x900 (HD+), 1920x1080 (1080p), 2560x1440 (2K), and Custom Width/Height.
  - Graphics Backend: OpenGL ES 3.0 (MRT / Content/OGL) vs OpenGL ES 2.0 (Compatibility / Content/iOS).
  - 3D mode toggle (3D mesh rendering & lighting).
  - Interface scale (UI Zoom: Automatic, 100%, 125%, 150%, 200%).
  - Haptic feedback toggle.

### 2. Touch Control System & Live Layout Editor (`Simitone.Touch`)
- **Authentic Touch Controls**: Virtual analog camera pan stick, Zoom In, Zoom Out, Rotate Left, Rotate Right, Pause, Speed 1x, Speed 2x, Speed 3x, Frame Advance, Live/Buy/Build Mode switchers, and Menu.
- **Interactive Layout Editor**:
  - Drag and place any control directly on-screen.
  - Snap-to-grid with configurable cell sizing.
  - Per-widget scale, opacity, anchor position (TopLeft, TopRight, BottomLeft, BottomRight, Center), and visibility toggles.
  - **Profiles Store**: Create, Save, Rename, Delete, and Export/Import JSON layout profiles (`Default`, `One-Handed Right`, `Tablet / Large Screen`, custom).

### 3. Live Lot Simulation & 2.5D Isometric World
- **House Lot**: Parquet wood flooring, kitchen checkered tile, bathroom tile, walls with dynamic cutaway modes (Walls Down, Cutaway, Walls Up, Roof).
- **Furniture & Objects**: Beds, Sofas, Refrigerator, Stove, Countertops, Dining Tables & Chairs, Television, Boombox Stereo, Telephone, Toilet, Shower, Vanity Sink, Bookshelves, Lamps, Plants.
- **Live Sim Character**: Walking animations, autonomous motive decay, speech bubbles, and rotating 3D Plumbob that changes color according to mood (Green = happy, Yellow = moderate, Red = critical).
- **Radial Pie Menu**: Authentic TS1 radial object menu for executing interactions (e.g., "Have Quick Snack", "Cook Dinner", "Watch TV", "Sleep", "Use Toilet", "Take Shower", "Study Cooking").
- **Day/Night Cycle & SimTime**: Sim clock tracking day and hour with dynamic ambient lighting overlays.

### 4. User Control Panel (UCP)
- **Live Mode Tabs**:
  - **Motives**: Real-time meters for Hunger, Comfort, Hygiene, Bladder, Energy, Fun, Social, Room.
  - **Career**: Job title, salary, working hours, carpool arrival, promotion requirements.
  - **Skills**: Skill stars for Cooking, Mechanical, Charisma, Logic, Body, Creativity.
  - **Relationships**: Household and neighbor friend list with Daily and Lifetime scores.
  - **Pocket**: Inventory items.
- **Buy Mode**: Categorized catalog (Seating, Surfaces, Appliances, Electronics, Plumbing, Decorative, Lighting, Misc) with live § Simoleons purchasing and tile placement.
- **Build Mode**: Wall, floor, door, window, and roof tools.

### 5. Create-A-Sim (CAS) Studio
- Head & outfit customization, gender, life stage (Adult / Child), and skin tone.
- **25-Point Personality Allocation**: Neat, Outgoing, Active, Playful, Nice.
- **Zodiac Calculation**: Automatic astrological sign assignment (Aries through Pisces) based on personality point distribution.
- Save & Move In directly into the active household.

### 6. Cheats Console
- Classic cheat codes: `rosebud`, `klapaucius` (grants §1,000), `motherlode` (grants §50,000), `maxmotives` (restores all motives to 100%), `moveobjects on`.

## Technical Stack
- **Runtime**: Node.js 22, React 18, TypeScript, Tailwind CSS, Vite.
- **Audio Engine**: Web Audio API synthesizer generating authentic Sim chimes, cash register sounds, speed clicks, and Sim speech.
- **Deployment**: Served on host `0.0.0.0` port `3000`.
