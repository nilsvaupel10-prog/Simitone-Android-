import React from 'react';
import { GameSpeed, WallCutaway } from '../../types';
import { sounds } from '../../audio/sound';
import { Play, Pause, FastForward, RotateCcw, RotateCw, ZoomIn, ZoomOut, Layers, Terminal, Menu } from 'lucide-react';

interface ClockPanelProps {
  dayName: string;
  simTimeFormatted: string;
  gameSpeed: GameSpeed;
  wallCutaway: WallCutaway;
  onSetSpeed: (speed: GameSpeed) => void;
  onCycleWallCutaway: () => void;
  onZoomIn: () => void;
  onZoomOut: () => void;
  onRotateCW: () => void;
  onRotateCCW: () => void;
  onOpenCheat: () => void;
  onOpenSetup: () => void;
}

export const ClockPanel: React.FC<ClockPanelProps> = ({
  dayName,
  simTimeFormatted,
  gameSpeed,
  wallCutaway,
  onSetSpeed,
  onCycleWallCutaway,
  onZoomIn,
  onZoomOut,
  onRotateCW,
  onRotateCCW,
  onOpenCheat,
  onOpenSetup,
}) => {
  const cutawayLabels: Record<WallCutaway, string> = {
    wallsDown: 'Walls Down',
    cutaway: 'Cutaway',
    wallsUp: 'Walls Up',
    roof: 'Roof',
  };

  return (
    <div className="absolute top-3 left-3 right-3 flex items-center justify-between pointer-events-none z-30 select-none">
      {/* Top Left: Menu, Cheats, & Cutaway */}
      <div className="flex items-center gap-2 pointer-events-auto">
        <button
          onClick={() => { sounds.playClick(); onOpenSetup(); }}
          className="p-2 bg-slate-900/85 hover:bg-slate-800 border border-slate-700/80 rounded-xl text-white shadow-lg backdrop-blur-md transition flex items-center gap-1.5 text-xs font-bold"
          title="Return to Setup / Launcher Menu"
        >
          <Menu className="w-4 h-4 text-blue-400" />
          <span className="hidden sm:inline">Launcher</span>
        </button>

        <button
          onClick={() => { sounds.playClick(); onCycleWallCutaway(); }}
          className="px-2.5 py-1.5 bg-slate-900/85 hover:bg-slate-800 border border-slate-700/80 rounded-xl text-slate-200 shadow-lg backdrop-blur-md transition flex items-center gap-1.5 text-xs font-semibold"
          title="Toggle Wall Cutaway Mode"
        >
          <Layers className="w-3.5 h-3.5 text-cyan-400" />
          <span className="hidden sm:inline">{cutawayLabels[wallCutaway]}</span>
        </button>

        <button
          onClick={() => { sounds.playClick(); onOpenCheat(); }}
          className="p-2 bg-slate-900/85 hover:bg-slate-800 border border-slate-700/80 rounded-xl text-slate-300 shadow-lg backdrop-blur-md transition"
          title="Cheats Console"
        >
          <Terminal className="w-4 h-4 text-emerald-400" />
        </button>
      </div>

      {/* Top Right: Time & Speed Controls */}
      <div className="flex items-center gap-2 pointer-events-auto">
        {/* Clock Badge */}
        <div className="px-3 py-1.5 bg-slate-900/90 border border-slate-700/80 rounded-xl shadow-lg backdrop-blur-md text-right">
          <div className="text-[10px] uppercase font-bold text-slate-400 tracking-wider">{dayName}</div>
          <div className="text-sm font-black font-mono text-cyan-300 leading-none">{simTimeFormatted}</div>
        </div>

        {/* Speeds */}
        <div className="flex p-1 bg-slate-900/90 border border-slate-700/80 rounded-xl shadow-lg backdrop-blur-md gap-1">
          <button
            onClick={() => { sounds.playSpeed(0); onSetSpeed(0); }}
            className={`p-1.5 rounded-lg transition ${
              gameSpeed === 0 ? 'bg-red-600 text-white shadow' : 'text-slate-400 hover:text-white hover:bg-slate-800'
            }`}
            title="Pause (P)"
          >
            <Pause className="w-4 h-4" />
          </button>
          <button
            onClick={() => { sounds.playSpeed(1); onSetSpeed(1); }}
            className={`p-1.5 rounded-lg transition ${
              gameSpeed === 1 ? 'bg-emerald-600 text-white shadow' : 'text-slate-400 hover:text-white hover:bg-slate-800'
            }`}
            title="Normal Speed 1x (1)"
          >
            <Play className="w-4 h-4" />
          </button>
          <button
            onClick={() => { sounds.playSpeed(2); onSetSpeed(2); }}
            className={`p-1.5 rounded-lg transition ${
              gameSpeed === 2 ? 'bg-blue-600 text-white shadow' : 'text-slate-400 hover:text-white hover:bg-slate-800'
            }`}
            title="Fast Speed 2x (2)"
          >
            <FastForward className="w-4 h-4" />
          </button>
          <button
            onClick={() => { sounds.playSpeed(3); onSetSpeed(3); }}
            className={`p-1.5 rounded-lg transition ${
              gameSpeed === 3 ? 'bg-indigo-600 text-white shadow' : 'text-slate-400 hover:text-white hover:bg-slate-800'
            }`}
            title="Ultra Speed 3x (3)"
          >
            <span className="font-bold text-xs px-1">3x</span>
          </button>
        </div>

        {/* Quick Camera buttons */}
        <div className="hidden sm:flex p-1 bg-slate-900/90 border border-slate-700/80 rounded-xl shadow-lg backdrop-blur-md gap-1">
          <button
            onClick={() => { sounds.playClick(); onRotateCCW(); }}
            className="p-1.5 text-slate-400 hover:text-white hover:bg-slate-800 rounded-lg transition"
            title="Rotate Left"
          >
            <RotateCcw className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={() => { sounds.playClick(); onRotateCW(); }}
            className="p-1.5 text-slate-400 hover:text-white hover:bg-slate-800 rounded-lg transition"
            title="Rotate Right"
          >
            <RotateCw className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={() => { sounds.playClick(); onZoomIn(); }}
            className="p-1.5 text-slate-400 hover:text-white hover:bg-slate-800 rounded-lg transition"
            title="Zoom In"
          >
            <ZoomIn className="w-3.5 h-3.5" />
          </button>
          <button
            onClick={() => { sounds.playClick(); onZoomOut(); }}
            className="p-1.5 text-slate-400 hover:text-white hover:bg-slate-800 rounded-lg transition"
            title="Zoom Out"
          >
            <ZoomOut className="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </div>
  );
};
