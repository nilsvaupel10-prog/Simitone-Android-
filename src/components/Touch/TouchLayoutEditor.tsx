import React, { useState } from 'react';
import { TouchControlProfile, TouchAnchor, TouchWidgetLayout } from '../../types';
import { ACTION_DESCRIPTIONS } from '../../services/touchProfiles';
import { sounds } from '../../audio/sound';
import { Sliders, Eye, EyeOff, Save, Trash2, Plus, Download, Upload, ArrowLeft, Move, Grid, Check } from 'lucide-react';

interface TouchLayoutEditorProps {
  currentProfile: TouchControlProfile;
  allProfiles: Record<string, TouchControlProfile>;
  selectedWidgetId: string | null;
  onSelectWidget: (id: string | null) => void;
  onUpdateProfile: (profile: TouchControlProfile) => void;
  onSaveProfiles: (profiles: Record<string, TouchControlProfile>) => void;
  onClose: () => void;
}

export const TouchLayoutEditor: React.FC<TouchLayoutEditorProps> = ({
  currentProfile,
  allProfiles,
  selectedWidgetId,
  onSelectWidget,
  onUpdateProfile,
  onSaveProfiles,
  onClose,
}) => {
  const [profile, setProfile] = useState<TouchControlProfile>(currentProfile);
  const [newProfileName, setNewProfileName] = useState('');
  const [showProfileModal, setShowProfileModal] = useState(false);
  const [statusText, setStatusText] = useState<string | null>(null);

  const selectedWidget: TouchWidgetLayout | undefined = selectedWidgetId
    ? profile.widgets[selectedWidgetId]
    : undefined;

  const showStatus = (text: string) => {
    setStatusText(text);
    setTimeout(() => setStatusText(null), 3000);
  };

  const handleUpdateWidget = (id: string, updates: Partial<TouchWidgetLayout>) => {
    const updated = {
      ...profile,
      widgets: {
        ...profile.widgets,
        [id]: {
          ...profile.widgets[id],
          ...updates,
        },
      },
    };
    setProfile(updated);
    onUpdateProfile(updated);
  };

  const handleSaveProfile = () => {
    sounds.playClick();
    const updatedProfiles = {
      ...allProfiles,
      [profile.name]: profile,
    };
    onSaveProfiles(updatedProfiles);
    showStatus(`Saved profile "${profile.name}" to storage.`);
  };

  const handleSwitchProfile = (name: string) => {
    sounds.playClick();
    if (allProfiles[name]) {
      const p = allProfiles[name];
      setProfile(p);
      onUpdateProfile(p);
      showStatus(`Switched to "${name}".`);
    }
  };

  const handleCreateProfile = () => {
    sounds.playClick();
    if (!newProfileName.trim()) return;
    const name = newProfileName.trim();
    const newP: TouchControlProfile = {
      ...profile,
      name,
      description: `Custom profile created on ${new Date().toLocaleDateString()}`,
      isBuiltIn: false,
    };
    const updatedProfiles = {
      ...allProfiles,
      [name]: newP,
    };
    setProfile(newP);
    onUpdateProfile(newP);
    onSaveProfiles(updatedProfiles);
    setNewProfileName('');
    setShowProfileModal(false);
    showStatus(`Created profile "${name}".`);
  };

  const handleDeleteProfile = (name: string) => {
    sounds.playClick();
    if (allProfiles[name]?.isBuiltIn) {
      showStatus('Cannot delete built-in profile.');
      return;
    }
    const updated = { ...allProfiles };
    delete updated[name];
    onSaveProfiles(updated);
    // fallback to Default
    if (updated['Default']) {
      setProfile(updated['Default']);
      onUpdateProfile(updated['Default']);
    }
    showStatus(`Deleted profile "${name}".`);
  };

  const handleExportJSON = () => {
    sounds.playClick();
    const dataStr = 'data:text/json;charset=utf-8,' + encodeURIComponent(JSON.stringify(profile, null, 2));
    const downloadAnchor = document.createElement('a');
    downloadAnchor.setAttribute('href', dataStr);
    downloadAnchor.setAttribute('download', `${profile.name.toLowerCase().replace(/\s+/g, '_')}_touch.json`);
    document.body.appendChild(downloadAnchor);
    downloadAnchor.click();
    downloadAnchor.remove();
    showStatus('Profile JSON downloaded.');
  };

  const handleImportJSON = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;
    const reader = new FileReader();
    reader.onload = (event) => {
      try {
        const imported = JSON.parse(event.target?.result as string) as TouchControlProfile;
        if (imported && imported.widgets) {
          const name = imported.name || 'Imported Profile';
          const updated = { ...imported, name };
          setProfile(updated);
          onUpdateProfile(updated);
          const updatedProfiles = { ...allProfiles, [name]: updated };
          onSaveProfiles(updatedProfiles);
          showStatus(`Imported profile "${name}".`);
        }
      } catch {
        showStatus('Failed to parse profile JSON.');
      }
    };
    reader.readAsText(file);
  };

  const anchors: TouchAnchor[] = ['TopLeft', 'TopRight', 'BottomLeft', 'BottomRight', 'Center'];

  return (
    <div className="absolute inset-y-0 right-0 w-80 sm:w-96 bg-slate-900/95 border-l border-slate-700/80 shadow-2xl backdrop-blur-xl z-50 flex flex-col text-white font-sans overflow-hidden">
      {/* Top Header */}
      <div className="p-4 border-b border-slate-800 flex items-center justify-between bg-slate-950/60">
        <div className="flex items-center gap-2">
          <button
            onClick={() => { sounds.playClick(); onClose(); }}
            className="p-1.5 hover:bg-slate-800 rounded-lg text-slate-300 hover:text-white transition"
            title="Exit layout editor"
          >
            <ArrowLeft className="w-5 h-5" />
          </button>
          <div>
            <h2 className="text-sm font-bold text-white flex items-center gap-1.5">
              <Sliders className="w-4 h-4 text-cyan-400" />
              Touch Layout Editor
            </h2>
            <p className="text-[11px] text-slate-400">Position & scale on-screen controls</p>
          </div>
        </div>
        <button
          onClick={handleSaveProfile}
          className="px-2.5 py-1 bg-cyan-600 hover:bg-cyan-500 rounded-lg text-xs font-semibold flex items-center gap-1 transition"
        >
          <Save className="w-3.5 h-3.5" />
          Save
        </button>
      </div>

      {statusText && (
        <div className="px-4 py-2 bg-cyan-950/80 border-b border-cyan-800/60 text-cyan-300 text-xs flex items-center gap-1.5">
          <Check className="w-3.5 h-3.5" />
          {statusText}
        </div>
      )}

      {/* Editor Content */}
      <div className="flex-1 overflow-y-auto p-4 space-y-4">
        {/* Profile Selector */}
        <div className="p-3 bg-slate-950/50 rounded-xl border border-slate-800 space-y-2">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-300">Active Profile</span>
            <div className="flex items-center gap-1">
              <button
                onClick={() => setShowProfileModal(true)}
                className="p-1 hover:bg-slate-800 rounded text-cyan-400 text-xs flex items-center gap-0.5"
                title="Create new profile"
              >
                <Plus className="w-3.5 h-3.5" />
                New
              </button>
              <button
                onClick={handleExportJSON}
                className="p-1 hover:bg-slate-800 rounded text-slate-400 hover:text-white"
                title="Export profile JSON"
              >
                <Download className="w-3.5 h-3.5" />
              </button>
              <label
                className="p-1 hover:bg-slate-800 rounded text-slate-400 hover:text-white cursor-pointer"
                title="Import profile JSON"
              >
                <Upload className="w-3.5 h-3.5" />
                <input type="file" accept=".json" onChange={handleImportJSON} className="hidden" />
              </label>
            </div>
          </div>

          <select
            value={profile.name}
            onChange={(e) => handleSwitchProfile(e.target.value)}
            className="w-full px-2.5 py-1.5 bg-slate-900 border border-slate-700 rounded-lg text-xs text-white"
          >
            {Object.keys(allProfiles).map((name) => (
              <option key={name} value={name}>
                {name} {allProfiles[name]?.isBuiltIn ? '(Built-in)' : ''}
              </option>
            ))}
          </select>
        </div>

        {/* Global Overlay Settings */}
        <div className="p-3 bg-slate-950/50 rounded-xl border border-slate-800 space-y-3">
          <span className="text-xs font-semibold text-slate-300 block">Overlay Settings</span>

          {/* Master Opacity */}
          <div>
            <div className="flex justify-between text-[11px] text-slate-400 mb-1">
              <span>Master Opacity</span>
              <span>{Math.round(profile.settings.opacity * 100)}%</span>
            </div>
            <input
              type="range"
              min="0.2"
              max="1.0"
              step="0.05"
              value={profile.settings.opacity}
              onChange={(e) => {
                const updated = {
                  ...profile,
                  settings: { ...profile.settings, opacity: parseFloat(e.target.value) },
                };
                setProfile(updated);
                onUpdateProfile(updated);
              }}
              className="w-full h-1.5 bg-slate-700 rounded-lg appearance-none cursor-pointer accent-cyan-500"
            />
          </div>

          {/* Master Scale */}
          <div>
            <div className="flex justify-between text-[11px] text-slate-400 mb-1">
              <span>Master Scale</span>
              <span>{profile.settings.scale.toFixed(2)}x</span>
            </div>
            <input
              type="range"
              min="0.7"
              max="1.5"
              step="0.05"
              value={profile.settings.scale}
              onChange={(e) => {
                const updated = {
                  ...profile,
                  settings: { ...profile.settings, scale: parseFloat(e.target.value) },
                };
                setProfile(updated);
                onUpdateProfile(updated);
              }}
              className="w-full h-1.5 bg-slate-700 rounded-lg appearance-none cursor-pointer accent-cyan-500"
            />
          </div>

          {/* Grid Snap & Labels */}
          <div className="grid grid-cols-2 gap-2 pt-1 text-xs">
            <label className="flex items-center gap-1.5 cursor-pointer text-slate-300">
              <input
                type="checkbox"
                checked={profile.settings.snapToGrid}
                onChange={(e) => {
                  const updated = {
                    ...profile,
                    settings: { ...profile.settings, snapToGrid: e.target.checked },
                  };
                  setProfile(updated);
                  onUpdateProfile(updated);
                }}
                className="w-3.5 h-3.5 rounded text-cyan-600 bg-slate-800 border-slate-600"
              />
              Snap to grid
            </label>

            <label className="flex items-center gap-1.5 cursor-pointer text-slate-300">
              <input
                type="checkbox"
                checked={profile.settings.showLabels}
                onChange={(e) => {
                  const updated = {
                    ...profile,
                    settings: { ...profile.settings, showLabels: e.target.checked },
                  };
                  setProfile(updated);
                  onUpdateProfile(updated);
                }}
                className="w-3.5 h-3.5 rounded text-cyan-600 bg-slate-800 border-slate-600"
              />
              Show labels
            </label>
          </div>
        </div>

        {/* Selected Widget Properties */}
        {selectedWidget && selectedWidgetId ? (
          <div className="p-3 bg-cyan-950/30 rounded-xl border border-cyan-800/60 space-y-3">
            <div className="flex items-center justify-between border-b border-cyan-800/40 pb-2">
              <div className="flex items-center gap-2">
                <Move className="w-4 h-4 text-cyan-400" />
                <span className="text-xs font-bold text-white capitalize">
                  {selectedWidgetId.replace(/_/g, ' ')}
                </span>
              </div>
              <button
                onClick={() =>
                  handleUpdateWidget(selectedWidgetId, {
                    visible: !selectedWidget.visible,
                  })
                }
                className={`p-1 rounded text-xs flex items-center gap-1 ${
                  selectedWidget.visible
                    ? 'text-emerald-400 hover:bg-emerald-950/40'
                    : 'text-slate-400 hover:bg-slate-800'
                }`}
              >
                {selectedWidget.visible ? (
                  <>
                    <Eye className="w-3.5 h-3.5" /> Visible
                  </>
                ) : (
                  <>
                    <EyeOff className="w-3.5 h-3.5" /> Hidden
                  </>
                )}
              </button>
            </div>

            {/* Anchor Selection */}
            <div>
              <label className="text-[11px] text-slate-400 mb-1 block">Screen Anchor</label>
              <div className="grid grid-cols-3 gap-1">
                {anchors.map((anc) => (
                  <button
                    key={anc}
                    onClick={() => handleUpdateWidget(selectedWidgetId, { anchor: anc })}
                    className={`py-1 px-1 text-[11px] font-medium rounded ${
                      selectedWidget.anchor === anc
                        ? 'bg-cyan-600 text-white font-bold'
                        : 'bg-slate-800 text-slate-300 hover:bg-slate-700'
                    }`}
                  >
                    {anc}
                  </button>
                ))}
              </div>
            </div>

            {/* Position Sliders */}
            <div className="space-y-2">
              <div>
                <div className="flex justify-between text-[11px] text-slate-400 mb-0.5">
                  <span>Horizontal Offset (X)</span>
                  <span>{Math.round(selectedWidget.offsetX * 100)}%</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="0.8"
                  step="0.01"
                  value={selectedWidget.offsetX}
                  onChange={(e) =>
                    handleUpdateWidget(selectedWidgetId, {
                      offsetX: parseFloat(e.target.value),
                    })
                  }
                  className="w-full h-1.5 bg-slate-700 rounded-lg appearance-none cursor-pointer accent-cyan-400"
                />
              </div>

              <div>
                <div className="flex justify-between text-[11px] text-slate-400 mb-0.5">
                  <span>Vertical Offset (Y)</span>
                  <span>{Math.round(selectedWidget.offsetY * 100)}%</span>
                </div>
                <input
                  type="range"
                  min="0"
                  max="0.8"
                  step="0.01"
                  value={selectedWidget.offsetY}
                  onChange={(e) =>
                    handleUpdateWidget(selectedWidgetId, {
                      offsetY: parseFloat(e.target.value),
                    })
                  }
                  className="w-full h-1.5 bg-slate-700 rounded-lg appearance-none cursor-pointer accent-cyan-400"
                />
              </div>

              <div>
                <div className="flex justify-between text-[11px] text-slate-400 mb-0.5">
                  <span>Widget Scale</span>
                  <span>{selectedWidget.scale.toFixed(2)}x</span>
                </div>
                <input
                  type="range"
                  min="0.5"
                  max="2.0"
                  step="0.05"
                  value={selectedWidget.scale}
                  onChange={(e) =>
                    handleUpdateWidget(selectedWidgetId, {
                      scale: parseFloat(e.target.value),
                    })
                  }
                  className="w-full h-1.5 bg-slate-700 rounded-lg appearance-none cursor-pointer accent-cyan-400"
                />
              </div>
            </div>
          </div>
        ) : (
          <div className="p-4 bg-slate-950/40 rounded-xl border border-slate-800 text-center text-xs text-slate-400 space-y-1">
            <p className="font-semibold text-slate-300">Tap any control on screen</p>
            <p>Or select from the list below to adjust its layout coordinates.</p>
          </div>
        )}

        {/* All Widgets List */}
        <div className="space-y-1">
          <span className="text-xs font-semibold text-slate-300 block mb-2">All Controls</span>
          <div className="space-y-1 max-h-48 overflow-y-auto pr-1">
            {Object.entries(profile.widgets).map(([id, w]) => (
              <div
                key={id}
                onClick={() => { sounds.playClick(); onSelectWidget(id); }}
                className={`p-2 rounded-lg flex items-center justify-between cursor-pointer text-xs transition ${
                  selectedWidgetId === id
                    ? 'bg-cyan-600/30 border border-cyan-500/50 text-white'
                    : 'bg-slate-950/30 hover:bg-slate-800/40 text-slate-300'
                }`}
              >
                <span className="capitalize font-medium">{id.replace(/_/g, ' ')}</span>
                <span
                  className={`text-[10px] px-1.5 py-0.5 rounded font-mono ${
                    w.visible ? 'bg-emerald-900/40 text-emerald-300' : 'bg-slate-800 text-slate-500'
                  }`}
                >
                  {w.visible ? 'ON' : 'OFF'}
                </span>
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Profile Creation Modal */}
      {showProfileModal && (
        <div className="absolute inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-900 border border-slate-700 rounded-2xl p-4 w-full max-w-xs space-y-3">
            <h3 className="text-sm font-bold text-white">New Control Profile</h3>
            <input
              type="text"
              placeholder="e.g. Left Handed, Driving Stick"
              value={newProfileName}
              onChange={(e) => setNewProfileName(e.target.value)}
              className="w-full px-3 py-2 bg-slate-950 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-cyan-500"
            />
            <div className="flex gap-2">
              <button
                onClick={() => setShowProfileModal(false)}
                className="flex-1 py-1.5 bg-slate-800 hover:bg-slate-700 rounded-xl text-xs text-slate-300"
              >
                Cancel
              </button>
              <button
                onClick={handleCreateProfile}
                className="flex-1 py-1.5 bg-cyan-600 hover:bg-cyan-500 rounded-xl text-xs font-bold text-white"
              >
                Create
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
