import React, { useState } from 'react';
import { SimitoneAndroidSettings, ResolutionPreset, GraphicsMode, UiScaleOption } from '../../types';
import { sounds } from '../../audio/sound';
import { Folder, CheckCircle, AlertTriangle, ShieldCheck, Play, RotateCcw, UserPlus, Sliders, Smartphone, Monitor } from 'lucide-react';

interface SetupActivityProps {
  settings: SimitoneAndroidSettings;
  onSaveSettings: (settings: SimitoneAndroidSettings) => void;
  onLaunchGame: () => void;
  onOpenCAS: () => void;
  onOpenEditor: () => void;
}

export const SetupActivity: React.FC<SetupActivityProps> = ({
  settings: initialSettings,
  onSaveSettings,
  onLaunchGame,
  onOpenCAS,
  onOpenEditor,
}) => {
  const [settings, setSettings] = useState<SimitoneAndroidSettings>(initialSettings);
  const [manualPathVisible, setManualPathVisible] = useState(false);
  const [manualPathInput, setManualPathInput] = useState(settings.gamePath);
  const [hasAllFilesPermission, setHasAllFilesPermission] = useState(true);
  const [statusMessage, setStatusMessage] = useState({
    isValid: true,
    message: 'GameData/Behavior.iff and Gameplay.iff verified (The Sims Complete Collection)',
  });
  const [toastMessage, setToastMessage] = useState<string | null>(null);

  const showToast = (msg: string) => {
    setToastMessage(msg);
    setTimeout(() => setToastMessage(null), 3000);
  };

  const validatePath = (path: string) => {
    if (!path || path.trim() === '') {
      setStatusMessage({
        isValid: false,
        message: 'Missing folder: Please select the folder containing GameData/Behavior.iff',
      });
      return false;
    }
    if (!path.toLowerCase().includes('sims') && !path.toLowerCase().includes('gamedata')) {
      setStatusMessage({
        isValid: false,
        message: 'Warning: Folder does not appear to contain The Sims installation files.',
      });
      return true;
    }
    setStatusMessage({
      isValid: true,
      message: 'OK - GameData/Behavior.iff and Gameplay.iff verified (Complete Collection found)',
    });
    return true;
  };

  const handlePickFolder = () => {
    sounds.playClick();
    // Simulate SAF Document Tree picker
    const samplePaths = [
      '/storage/emulated/0/The Sims: Complete Collection',
      '/storage/emulated/0/Games/The Sims',
      '/storage/sdcard1/The Sims',
    ];
    const picked = samplePaths[Math.floor(Math.random() * samplePaths.length)];
    const updated = {
      ...settings,
      gamePath: picked,
      treeUri: `content://com.android.externalstorage.documents/tree/primary%3A${encodeURIComponent(picked)}`,
    };
    setSettings(updated);
    setManualPathInput(picked);
    onSaveSettings(updated);
    validatePath(picked);
    showToast(`Storage Access Framework: Selected ${picked}`);
  };

  const handleApplyManualPath = () => {
    sounds.playClick();
    if (!manualPathInput.trim()) {
      showToast('Please enter a valid path.');
      return;
    }
    const updated = {
      ...settings,
      gamePath: manualPathInput.trim(),
    };
    setSettings(updated);
    onSaveSettings(updated);
    validatePath(manualPathInput.trim());
    showToast('Manual path applied.');
  };

  const handleGrantPermission = () => {
    sounds.playClick();
    setHasAllFilesPermission(true);
    showToast('All files access permission granted (MANAGE_EXTERNAL_STORAGE).');
  };

  const handleReset = () => {
    sounds.playClick();
    const defaultSettings: SimitoneAndroidSettings = {
      gamePath: '/storage/emulated/0/The Sims',
      treeUri: '',
      resolution: 'DeviceNative',
      customWidth: 1280,
      customHeight: 720,
      enable3D: true,
      graphics: 'OpenGLEs3',
      uiScale: 'Automatic',
      haptics: true,
    };
    setSettings(defaultSettings);
    setManualPathInput(defaultSettings.gamePath);
    onSaveSettings(defaultSettings);
    validatePath(defaultSettings.gamePath);
    showToast('Setup has been reset to defaults.');
  };

  const handleLaunch = () => {
    sounds.playClick();
    if (!statusMessage.isValid) {
      showToast('Please specify a valid game directory before launching.');
      return;
    }
    onSaveSettings(settings);
    onLaunchGame();
  };

  const uiScaleDescriptions: Record<UiScaleOption, string> = {
    Automatic: 'Derived from device screen density (recommended for high-DPI phones & tablets).',
    X100: 'Normal size (1.0x). Fits more controls and lot view on compact screens.',
    X125: 'Enlarged by 25%. Good for medium-sized phones.',
    X150: 'Enlarged by 50%. Easier thumb tapping on 6" to 7" screens.',
    X200: 'Doubled in size (2.0x). Maximum accessibility on high resolution tablets.',
  };

  return (
    <div className="w-full h-full bg-[#111622] text-slate-100 overflow-y-auto p-4 sm:p-6 md:p-8 font-sans">
      <div className="max-w-3xl mx-auto space-y-6">
        {/* Header matching LauncherActivity */}
        <div className="flex items-start justify-between border-b border-slate-700/60 pb-4">
          <div>
            <div className="flex items-center gap-3">
              <img src="/plumbob.svg" alt="Plumbob" className="w-8 h-10 drop-shadow-[0_0_12px_rgba(74,222,128,0.5)]" />
              <div>
                <h1 className="text-3xl font-bold tracking-tight text-white flex items-center gap-2">
                  Simitone
                  <span className="text-xs font-semibold px-2 py-0.5 bg-blue-600/30 text-blue-400 border border-blue-500/40 rounded-full">
                    Android Port
                  </span>
                </h1>
                <p className="text-sm text-slate-400 mt-0.5">Initial setup & configuration</p>
              </div>
            </div>
          </div>
          <div className="flex gap-2">
            <button
              onClick={() => { sounds.playClick(); onOpenCAS(); }}
              className="px-3 py-1.5 bg-emerald-600/30 hover:bg-emerald-600/50 border border-emerald-500/40 rounded-lg text-emerald-300 text-xs font-medium flex items-center gap-1.5 transition-colors"
            >
              <UserPlus className="w-3.5 h-3.5" />
              CAS Studio
            </button>
            <button
              onClick={() => { sounds.playClick(); onOpenEditor(); }}
              className="px-3 py-1.5 bg-indigo-600/30 hover:bg-indigo-600/50 border border-indigo-500/40 rounded-lg text-indigo-300 text-xs font-medium flex items-center gap-1.5 transition-colors"
            >
              <Sliders className="w-3.5 h-3.5" />
              Touch Layout
            </button>
          </div>
        </div>

        {/* Toast alert */}
        {toastMessage && (
          <div className="p-3 bg-blue-600/20 border border-blue-500/50 rounded-xl text-blue-200 text-xs font-medium animate-fadeIn">
            {toastMessage}
          </div>
        )}

        {/* Section 1: Game Files */}
        <div className="bg-slate-800/40 border border-slate-700/60 rounded-2xl p-5 shadow-lg space-y-4">
          <div className="flex items-center gap-2 text-blue-400 font-semibold text-lg border-b border-slate-700/40 pb-2">
            <Folder className="w-5 h-5 text-blue-400" />
            Game Files
          </div>

          <div className="space-y-1">
            <label className="text-xs font-semibold uppercase tracking-wider text-slate-300">
              The Sims: Complete Collection folder
            </label>
            <p className="text-xs text-slate-400">
              Simitone requires your own copy of the game. Game assets are read directly from storage.
            </p>
          </div>

          <div className="p-3 bg-black/40 border border-slate-700/50 rounded-xl font-mono text-xs text-blue-200 break-all select-all flex items-center justify-between">
            <span>{settings.gamePath || 'No folder selected yet.'}</span>
            {statusMessage.isValid ? (
              <CheckCircle className="w-4 h-4 text-emerald-400 shrink-0 ml-2" />
            ) : (
              <AlertTriangle className="w-4 h-4 text-rose-400 shrink-0 ml-2" />
            )}
          </div>

          <div className={`text-xs px-2 py-1 rounded-md ${statusMessage.isValid ? 'text-emerald-400 bg-emerald-950/30' : 'text-rose-400 bg-rose-950/30'}`}>
            {statusMessage.message}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-3 gap-2.5 pt-1">
            <button
              onClick={handlePickFolder}
              className="px-4 py-2.5 bg-white/10 hover:bg-white/20 border border-white/20 rounded-xl text-white text-xs font-medium flex items-center justify-center gap-2 transition"
            >
              <Folder className="w-4 h-4 text-blue-400" />
              Choose folder...
            </button>
            <button
              onClick={() => { sounds.playClick(); setManualPathVisible(!manualPathVisible); }}
              className="px-4 py-2.5 bg-white/10 hover:bg-white/20 border border-white/20 rounded-xl text-white text-xs font-medium flex items-center justify-center gap-2 transition"
            >
              Enter path manually
            </button>
            <button
              onClick={handleGrantPermission}
              className={`px-4 py-2.5 border rounded-xl text-xs font-medium flex items-center justify-center gap-2 transition ${
                hasAllFilesPermission
                  ? 'bg-emerald-600/20 border-emerald-500/40 text-emerald-300'
                  : 'bg-amber-600/20 border-amber-500/40 text-amber-300'
              }`}
            >
              <ShieldCheck className="w-4 h-4" />
              {hasAllFilesPermission ? 'All Files Access: Granted' : 'Grant All Files Access'}
            </button>
          </div>

          {manualPathVisible && (
            <div className="p-3 bg-black/30 border border-slate-700/60 rounded-xl space-y-2">
              <input
                type="text"
                value={manualPathInput}
                onChange={(e) => setManualPathInput(e.target.value)}
                placeholder="/storage/emulated/0/The Sims"
                className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-lg text-xs text-white focus:outline-none focus:border-blue-500"
              />
              <button
                onClick={handleApplyManualPath}
                className="w-full py-2 bg-blue-600 hover:bg-blue-500 rounded-lg text-white text-xs font-semibold transition"
              >
                Use this path
              </button>
            </div>
          )}
        </div>

        {/* Section 2: Display & Graphics */}
        <div className="bg-slate-800/40 border border-slate-700/60 rounded-2xl p-5 shadow-lg space-y-4">
          <div className="flex items-center gap-2 text-blue-400 font-semibold text-lg border-b border-slate-700/40 pb-2">
            <Smartphone className="w-5 h-5 text-blue-400" />
            Display & Graphics
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {/* Resolution */}
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-300 flex items-center gap-1.5">
                <Monitor className="w-3.5 h-3.5 text-blue-400" />
                Resolution Preset
              </label>
              <select
                value={settings.resolution}
                onChange={(e) => {
                  sounds.playClick();
                  const updated = { ...settings, resolution: e.target.value as ResolutionPreset };
                  setSettings(updated);
                  onSaveSettings(updated);
                }}
                className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-blue-500"
              >
                <option value="DeviceNative">Device native resolution</option>
                <option value="W1280x720">1280 x 720 (720p HD)</option>
                <option value="W1600x900">1600 x 900 (HD+)</option>
                <option value="W1920x1080">1920 x 1080 (1080p FHD)</option>
                <option value="W2560x1440">2560 x 1440 (2K QHD)</option>
                <option value="Custom">Custom resolution...</option>
              </select>
              <p className="text-[11px] text-slate-400">
                The logical width of the game engine. Height adapts to device aspect ratio.
              </p>
            </div>

            {/* Graphics Backend */}
            <div className="space-y-1.5">
              <label className="text-xs font-medium text-slate-300">Graphics Backend</label>
              <select
                value={settings.graphics}
                onChange={(e) => {
                  sounds.playClick();
                  const updated = { ...settings, graphics: e.target.value as GraphicsMode };
                  setSettings(updated);
                  onSaveSettings(updated);
                }}
                className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-blue-500"
              >
                <option value="OpenGLEs3">OpenGL ES 3.0 (recommended - modern MRT)</option>
                <option value="OpenGLEs2Compatibility">OpenGL ES 2.0 (compatibility mode)</option>
              </select>
              <p className="text-[11px] text-slate-400">
                Uses Content/OGL shaders for modern devices or Content/iOS for compatibility.
              </p>
            </div>
          </div>

          {settings.resolution === 'Custom' && (
            <div className="p-3 bg-black/30 border border-slate-700 rounded-xl grid grid-cols-2 gap-3">
              <div>
                <label className="text-[11px] text-slate-400">Width (px)</label>
                <input
                  type="number"
                  value={settings.customWidth}
                  onChange={(e) => {
                    const updated = { ...settings, customWidth: parseInt(e.target.value) || 1280 };
                    setSettings(updated);
                    onSaveSettings(updated);
                  }}
                  className="w-full px-2.5 py-1.5 bg-slate-900 border border-slate-700 rounded-lg text-xs text-white"
                />
              </div>
              <div>
                <label className="text-[11px] text-slate-400">Height (px)</label>
                <input
                  type="number"
                  value={settings.customHeight}
                  onChange={(e) => {
                    const updated = { ...settings, customHeight: parseInt(e.target.value) || 720 };
                    setSettings(updated);
                    onSaveSettings(updated);
                  }}
                  className="w-full px-2.5 py-1.5 bg-slate-900 border border-slate-700 rounded-lg text-xs text-white"
                />
              </div>
            </div>
          )}

          {/* Interface Scale */}
          <div className="space-y-1.5 pt-1">
            <label className="text-xs font-medium text-slate-300">Interface Scale (UI Zoom)</label>
            <select
              value={settings.uiScale}
              onChange={(e) => {
                sounds.playClick();
                const updated = { ...settings, uiScale: e.target.value as UiScaleOption };
                setSettings(updated);
                onSaveSettings(updated);
              }}
              className="w-full px-3 py-2 bg-slate-900 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-blue-500"
            >
              <option value="Automatic">Automatic (derived from screen density)</option>
              <option value="X100">100% (Default standard scale)</option>
              <option value="X125">125% (Slightly larger)</option>
              <option value="X150">150% (Comfortable handheld)</option>
              <option value="X200">200% (Double size for high-DPI)</option>
            </select>
            <p className="text-[11px] text-slate-400">{uiScaleDescriptions[settings.uiScale]}</p>
          </div>

          {/* Toggles */}
          <div className="pt-2 grid grid-cols-1 sm:grid-cols-2 gap-3">
            <label className="flex items-center gap-3 p-3 bg-black/20 hover:bg-black/30 border border-slate-700/50 rounded-xl cursor-pointer transition">
              <input
                type="checkbox"
                checked={settings.enable3D}
                onChange={(e) => {
                  sounds.playClick();
                  const updated = { ...settings, enable3D: e.target.checked };
                  setSettings(updated);
                  onSaveSettings(updated);
                }}
                className="w-4 h-4 rounded text-blue-600 focus:ring-0 bg-slate-900 border-slate-600"
              />
              <div>
                <span className="text-xs font-semibold text-white block">Enable 3D mode</span>
                <span className="text-[11px] text-slate-400">Renders 3D meshes and dynamic lighting</span>
              </div>
            </label>

            <label className="flex items-center gap-3 p-3 bg-black/20 hover:bg-black/30 border border-slate-700/50 rounded-xl cursor-pointer transition">
              <input
                type="checkbox"
                checked={settings.haptics}
                onChange={(e) => {
                  sounds.playClick();
                  const updated = { ...settings, haptics: e.target.checked };
                  setSettings(updated);
                  onSaveSettings(updated);
                  if (updated.haptics) sounds.triggerHaptic(40);
                }}
                className="w-4 h-4 rounded text-blue-600 focus:ring-0 bg-slate-900 border-slate-600"
              />
              <div>
                <span className="text-xs font-semibold text-white block">Haptic feedback</span>
                <span className="text-[11px] text-slate-400">Vibrate on touch control press</span>
              </div>
            </label>
          </div>
        </div>

        {/* Section 3: Launch Actions */}
        <div className="space-y-3 pt-2">
          <button
            onClick={handleLaunch}
            className="w-full py-4 bg-gradient-to-r from-blue-600 via-blue-500 to-indigo-600 hover:from-blue-500 hover:to-indigo-500 rounded-2xl text-white font-bold text-base shadow-[0_0_25px_rgba(59,130,246,0.35)] flex items-center justify-center gap-2.5 transition active:scale-[0.99]"
          >
            <Play className="w-5 h-5 fill-current" />
            Launch Game
          </button>

          <button
            onClick={handleReset}
            className="w-full py-2.5 bg-white/5 hover:bg-white/10 border border-slate-700/60 rounded-xl text-slate-300 text-xs font-medium flex items-center justify-center gap-2 transition"
          >
            <RotateCcw className="w-3.5 h-3.5" />
            Reset Setup to Defaults
          </button>
        </div>

        {/* Footer legal disclaimer */}
        <p className="text-center text-[11px] text-slate-500 pt-2 pb-6">
          Simitone is an open-source client for The Sims 1. It contains no copyrighted game assets and requires your own copy of The Sims: Complete Collection.
        </p>
      </div>
    </div>
  );
};
