import React, { useState, useEffect, useRef } from 'react';
import {
  ScreenMode,
  SimitoneAndroidSettings,
  TouchControlProfile,
  SimPerson,
  LotObject,
  GameSpeed,
  GameMode,
  WallCutaway,
  CatalogItem,
  TouchAction,
} from './types';
import {
  INITIAL_SETTINGS,
  INITIAL_SIM,
  INITIAL_LOT_OBJECTS,
  getStoredSettings,
  saveStoredSettings,
  getStoredProfiles,
  saveStoredProfiles,
} from './services/gameState';
import { sounds } from './audio/sound';
import { SetupActivity } from './components/Launcher/SetupActivity';
import { LotCanvas } from './components/Game/LotCanvas';
import { UCP } from './components/Game/UCP';
import { ClockPanel } from './components/Game/ClockPanel';
import { PieMenu } from './components/Game/PieMenu';
import { CheatConsole } from './components/Game/CheatConsole';
import { TouchOverlay } from './components/Touch/TouchOverlay';
import { TouchLayoutEditor } from './components/Touch/TouchLayoutEditor';
import { CreateASim } from './components/CAS/CreateASim';
import { Smartphone, Monitor } from 'lucide-react';

export const App: React.FC = () => {
  // Navigation Screen State
  const [screen, setScreen] = useState<ScreenMode>('setup');

  // Android Settings & Profiles
  const [settings, setSettings] = useState<SimitoneAndroidSettings>(getStoredSettings);
  const [profiles, setProfiles] = useState<Record<string, TouchControlProfile>>(getStoredProfiles);
  const [activeProfileName, setActiveProfileName] = useState<string>('Default');

  // Simulation State
  const [sim, setSim] = useState<SimPerson>(INITIAL_SIM);
  const [familyFunds, setFamilyFunds] = useState<number>(20000);
  const [objects, setObjects] = useState<LotObject[]>(INITIAL_LOT_OBJECTS);
  const [gameMode, setGameMode] = useState<GameMode>('live');
  const [gameSpeed, setGameSpeed] = useState<GameSpeed>(1);
  const [wallCutaway, setWallCutaway] = useState<WallCutaway>('cutaway');

  // Time of Day (SimTime)
  const [simMinutes, setSimMinutes] = useState<number>(8 * 60 + 30); // Starts Monday 8:30 AM
  const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];
  const dayIndex = Math.floor(simMinutes / (24 * 60)) % 7;
  const currentDayName = days[dayIndex];
  const hour = Math.floor((simMinutes % (24 * 60)) / 60);
  const minute = Math.floor(simMinutes % 60);
  const simTimeFormatted = `${(hour % 12 || 12).toString().padStart(2, '0')}:${minute.toString().padStart(2, '0')} ${hour >= 12 ? 'PM' : 'AM'}`;

  // Camera & Viewport
  const [cameraPan, setCameraPan] = useState({ x: 0, y: 0 });
  const [zoomLevel, setZoomLevel] = useState(1.0);
  const [cameraRotation, setCameraRotation] = useState(0);

  // Interaction & Pie Menu
  const [pieMenu, setPieMenu] = useState<{
    x: number;
    y: number;
    title: string;
    options: string[];
    targetObject: LotObject;
  } | null>(null);
  const [currentActionLabel, setCurrentActionLabel] = useState<string | null>(null);

  // Buy Mode Selection
  const [selectedCatalogItem, setSelectedCatalogItem] = useState<CatalogItem | null>(null);

  // Cheats & Layout Editor
  const [showCheatConsole, setShowCheatConsole] = useState(false);
  const [selectedEditorWidgetId, setSelectedEditorWidgetId] = useState<string | null>(null);
  const [showDeviceFrame, setShowDeviceFrame] = useState(false);

  const activeProfile = profiles[activeProfileName] || Object.values(profiles)[0];

  // Simulation Clock & Motive Decay Loop
  useEffect(() => {
    if (gameSpeed === 0) return;
    const intervalMs = gameSpeed === 1 ? 500 : gameSpeed === 2 ? 200 : 80;

    const timer = setInterval(() => {
      setSimMinutes((prev) => prev + 1);

      // Natural motive decay
      setSim((prevSim) => {
        const decay = gameSpeed === 3 ? 0.3 : 0.1;
        return {
          ...prevSim,
          motives: {
            hunger: Math.max(0, prevSim.motives.hunger - decay * 0.9),
            comfort: Math.max(0, prevSim.motives.comfort - decay * 0.4),
            hygiene: Math.max(0, prevSim.motives.hygiene - decay * 0.5),
            bladder: Math.max(0, prevSim.motives.bladder - decay * 0.8),
            energy: Math.max(0, prevSim.motives.energy - decay * 0.4),
            fun: Math.max(0, prevSim.motives.fun - decay * 0.6),
            social: Math.max(0, prevSim.motives.social - decay * 0.5),
            room: Math.max(0, prevSim.motives.room - decay * 0.2),
          },
        };
      });
    }, intervalMs);

    return () => clearInterval(timer);
  }, [gameSpeed]);

  // Handle Touch Actions emitted from touch overlay / buttons
  const handleTouchAction = (action: TouchAction) => {
    switch (action) {
      case 'ZoomIn':
        setZoomLevel((z) => Math.min(1.8, z + 0.15));
        break;
      case 'ZoomOut':
        setZoomLevel((z) => Math.max(0.6, z - 0.15));
        break;
      case 'RotateLeft':
        setCameraRotation((r) => (r + 270) % 360);
        break;
      case 'RotateRight':
        setCameraRotation((r) => (r + 90) % 360);
        break;
      case 'Pause':
        setGameSpeed(0);
        break;
      case 'Resume':
      case 'Speed1':
        setGameSpeed(1);
        break;
      case 'Speed2':
        setGameSpeed(2);
        break;
      case 'Speed3':
        setGameSpeed(3);
        break;
      case 'FrameAdvance':
        setSimMinutes((m) => m + 1);
        break;
      case 'ModeLive':
        setGameMode('live');
        break;
      case 'ModeBuy':
        setGameMode('buy');
        break;
      case 'ModeBuild':
        setGameMode('build');
        break;
      case 'EditControls':
        setScreen('editor');
        break;
      case 'Menu':
      case 'OpenOptions':
        setScreen('setup');
        break;
      case 'Cancel':
        setPieMenu(null);
        setSelectedCatalogItem(null);
        break;
      case 'CameraForward':
        setCameraPan((p) => ({ ...p, y: p.y - 30 }));
        break;
      case 'CameraBack':
        setCameraPan((p) => ({ ...p, y: p.y + 30 }));
        break;
      case 'CameraLeft':
        setCameraPan((p) => ({ ...p, x: p.x - 30 }));
        break;
      case 'CameraRight':
        setCameraPan((p) => ({ ...p, x: p.x + 30 }));
        break;
    }
  };

  // Handle camera pan from joystick
  const handleJoystickPan = (dx: number, dy: number) => {
    setCameraPan((prev) => ({
      x: prev.x - dx * 16,
      y: prev.y - dy * 16,
    }));
  };

  // Object selected on lot
  const handleSelectObject = (obj: LotObject, screenPos: { x: number; y: number }) => {
    setPieMenu({
      x: screenPos.x,
      y: screenPos.y,
      title: obj.name.split(' ')[0],
      options: obj.interactions,
      targetObject: obj,
    });
  };

  // Execute selected interaction from Pie Menu
  const handleExecuteInteraction = (option: string) => {
    if (!pieMenu) return;
    const target = pieMenu.targetObject;
    setPieMenu(null);
    setCurrentActionLabel(option);
    sounds.playSimGibberish();

    // Check interaction costs / rewards
    if (option.includes('§5')) setFamilyFunds((f) => f - 5);
    if (option.includes('§10')) setFamilyFunds((f) => f - 10);
    if (option.includes('§15')) setFamilyFunds((f) => f - 15);
    if (option.includes('§20')) setFamilyFunds((f) => f - 20);
    if (option.includes('§30')) setFamilyFunds((f) => f - 30);
    if (option.includes('§40')) setFamilyFunds((f) => f - 40);

    // Boost motives based on object
    setTimeout(() => {
      setSim((prev) => {
        const bonuses = target.motiveBonus || {};
        return {
          ...prev,
          motives: {
            hunger: Math.min(100, prev.motives.hunger + (bonuses.hunger || (option.includes('Meal') || option.includes('Snack') ? 45 : 0))),
            comfort: Math.min(100, prev.motives.comfort + (bonuses.comfort || (option.includes('Sit') || option.includes('Sleep') ? 40 : 0))),
            energy: Math.min(100, prev.motives.energy + (bonuses.energy || (option.includes('Sleep') || option.includes('Nap') ? 60 : 0))),
            hygiene: Math.min(100, prev.motives.hygiene + (bonuses.hygiene || (option.includes('Shower') || option.includes('Wash') ? 70 : 0))),
            bladder: Math.min(100, prev.motives.bladder + (bonuses.bladder || (option.includes('Toilet') ? 85 : 0))),
            fun: Math.min(100, prev.motives.fun + (bonuses.fun || (option.includes('TV') || option.includes('Stereo') ? 35 : 0))),
            social: Math.min(100, prev.motives.social + (option.includes('Neighbor') || option.includes('Talk') ? 30 : 0)),
            room: Math.min(100, prev.motives.room + (bonuses.room || 10)),
          },
          skills: {
            ...prev.skills,
            cooking: option.includes('Study Cooking') ? Math.min(10, prev.skills.cooking + 1) : prev.skills.cooking,
            mechanical: option.includes('Study Mechanical') ? Math.min(10, prev.skills.mechanical + 1) : prev.skills.mechanical,
          },
        };
      });
      setCurrentActionLabel(null);
      sounds.playLevelUp();
    }, 3000);
  };

  // Place object from Buy Mode
  const handlePlaceObject = (gridX: number, gridY: number) => {
    if (!selectedCatalogItem) return;
    if (familyFunds < selectedCatalogItem.price) {
      sounds.playClick();
      alert('Insufficient Simoleons § to purchase this item!');
      return;
    }

    sounds.playCashRegister();
    setFamilyFunds((f) => f - selectedCatalogItem.price);

    const newObj: LotObject = {
      id: `obj_${Date.now()}`,
      catalogId: selectedCatalogItem.id,
      name: selectedCatalogItem.name,
      x: gridX,
      y: gridY,
      width: selectedCatalogItem.width,
      height: selectedCatalogItem.height,
      rotation: 0,
      category: selectedCatalogItem.category,
      price: selectedCatalogItem.price,
      interactions: selectedCatalogItem.interactions,
      motiveBonus: selectedCatalogItem.motiveBonus,
      color: selectedCatalogItem.color,
      icon: selectedCatalogItem.icon,
    };

    setObjects((prev) => [...prev, newObj]);
    setSelectedCatalogItem(null);
  };

  // Execute Cheat Console
  const handleExecuteCheat = (cheat: string) => {
    const code = cheat.toLowerCase().trim();
    if (code === 'rosebud' || code === 'klapaucius') {
      setFamilyFunds((f) => f + 1000);
      return { success: true, message: 'Granted §1,000 Simoleons!' };
    }
    if (code === 'motherlode') {
      setFamilyFunds((f) => f + 50000);
      return { success: true, message: 'Granted §50,000 Simoleons!' };
    }
    if (code === 'maxmotives' || code === 'max_motives') {
      setSim((prev) => ({
        ...prev,
        motives: {
          hunger: 100,
          comfort: 100,
          hygiene: 100,
          bladder: 100,
          energy: 100,
          fun: 100,
          social: 100,
          room: 100,
        },
      }));
      return { success: true, message: 'All Sim motives restored to 100%!' };
    }
    if (code === 'help' || code === '?') {
      return {
        success: true,
        message: 'Commands: rosebud, motherlode, maxmotives, moveobjects on, set_speed 1-3',
      };
    }
    return { success: false, message: `Unknown cheat command: "${cheat}". Type "help".` };
  };

  // Wall cutaway cycle
  const handleCycleCutaway = () => {
    const modes: WallCutaway[] = ['wallsDown', 'cutaway', 'wallsUp', 'roof'];
    const idx = modes.indexOf(wallCutaway);
    setWallCutaway(modes[(idx + 1) % modes.length]);
  };

  return (
    <div className="w-screen h-screen bg-black flex items-center justify-center overflow-hidden font-sans select-none">
      {/* Device Frame Wrapper (Optional phone/tablet bezels) */}
      <div
        className={`w-full h-full relative overflow-hidden flex flex-col ${
          showDeviceFrame ? 'max-w-6xl max-h-[780px] rounded-3xl border-8 border-slate-800 shadow-2xl' : ''
        }`}
      >
        {/* Device Bezel Top Notch toggle */}
        <div className="absolute top-1 right-24 z-50 pointer-events-auto">
          <button
            onClick={() => setShowDeviceFrame(!showDeviceFrame)}
            className="p-1.5 bg-black/50 hover:bg-black/80 rounded-lg text-slate-400 hover:text-white transition text-[10px] flex items-center gap-1 border border-white/10"
            title="Toggle device bezel preview"
          >
            {showDeviceFrame ? <Monitor className="w-3.5 h-3.5" /> : <Smartphone className="w-3.5 h-3.5" />}
            <span className="hidden sm:inline">{showDeviceFrame ? 'Fullscreen' : 'Device Bezel'}</span>
          </button>
        </div>

        {/* 1. SETUP / LAUNCHER ACTIVITY */}
        {screen === 'setup' && (
          <SetupActivity
            settings={settings}
            onSaveSettings={(newSettings) => {
              setSettings(newSettings);
              saveStoredSettings(newSettings);
            }}
            onLaunchGame={() => setScreen('game')}
            onOpenCAS={() => setScreen('cas')}
            onOpenEditor={() => setScreen('editor')}
          />
        )}

        {/* 2. CAS STUDIO */}
        {screen === 'cas' && (
          <CreateASim
            initialSim={sim}
            onSaveSim={(newSim) => {
              setSim(newSim);
              setScreen('game');
            }}
            onCancel={() => setScreen('game')}
          />
        )}

        {/* 3. SIMULATION GAME SCREEN */}
        {(screen === 'game' || screen === 'editor') && (
          <div className="relative w-full h-full overflow-hidden bg-slate-950">
            {/* Interactive 3D / Isometric Lot Canvas */}
            <LotCanvas
              objects={objects}
              activeSim={sim}
              gameMode={gameMode}
              wallCutaway={wallCutaway}
              cameraPan={cameraPan}
              zoomLevel={zoomLevel}
              cameraRotation={cameraRotation}
              selectedCatalogItem={selectedCatalogItem}
              onSelectObject={handleSelectObject}
              onPlaceObject={handlePlaceObject}
              timeOfDayHours={simMinutes / 60}
              currentActionLabel={currentActionLabel}
            />

            {/* Top HUD Clock & Speed Panel */}
            <ClockPanel
              dayName={currentDayName}
              simTimeFormatted={simTimeFormatted}
              gameSpeed={gameSpeed}
              wallCutaway={wallCutaway}
              onSetSpeed={setGameSpeed}
              onCycleWallCutaway={handleCycleCutaway}
              onZoomIn={() => setZoomLevel((z) => Math.min(1.8, z + 0.15))}
              onZoomOut={() => setZoomLevel((z) => Math.max(0.6, z - 0.15))}
              onRotateCW={() => setCameraRotation((r) => (r + 90) % 360)}
              onRotateCCW={() => setCameraRotation((r) => (r + 270) % 360)}
              onOpenCheat={() => setShowCheatConsole(true)}
              onOpenSetup={() => setScreen('setup')}
            />

            {/* On-Screen Touch Controls Overlay */}
            <TouchOverlay
              profile={activeProfile}
              isEditMode={screen === 'editor'}
              selectedWidgetId={selectedEditorWidgetId}
              onSelectWidget={setSelectedEditorWidgetId}
              onAction={handleTouchAction}
              onPan={handleJoystickPan}
            />

            {/* Bottom User Control Panel (UCP) */}
            {screen === 'game' && (
              <UCP
                sim={sim}
                gameMode={gameMode}
                familyFunds={familyFunds}
                onSelectGameMode={setGameMode}
                onSelectCatalogItem={setSelectedCatalogItem}
                selectedCatalogItem={selectedCatalogItem}
                onOpenCAS={() => setScreen('cas')}
              />
            )}

            {/* Radial Pie Menu Popup */}
            {pieMenu && (
              <PieMenu
                x={pieMenu.x}
                y={pieMenu.y}
                title={pieMenu.title}
                options={pieMenu.options}
                onSelectOption={handleExecuteInteraction}
                onClose={() => setPieMenu(null)}
              />
            )}

            {/* Cheats Prompt Modal */}
            {showCheatConsole && (
              <CheatConsole
                onExecuteCheat={handleExecuteCheat}
                onClose={() => setShowCheatConsole(false)}
              />
            )}

            {/* In-Game Touch Layout Editor Drawer */}
            {screen === 'editor' && (
              <TouchLayoutEditor
                currentProfile={activeProfile}
                allProfiles={profiles}
                selectedWidgetId={selectedEditorWidgetId}
                onSelectWidget={setSelectedEditorWidgetId}
                onUpdateProfile={(updated) => {
                  const newProfiles = { ...profiles, [updated.name]: updated };
                  setProfiles(newProfiles);
                  setActiveProfileName(updated.name);
                }}
                onSaveProfiles={(newProfiles) => {
                  setProfiles(newProfiles);
                  saveStoredProfiles(newProfiles);
                }}
                onClose={() => setScreen('game')}
              />
            )}
          </div>
        )}
      </div>
    </div>
  );
};
