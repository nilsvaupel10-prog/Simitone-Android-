import React, { useState } from 'react';
import { sounds } from '../../audio/sound';
import { Terminal, X, CornerDownLeft } from 'lucide-react';

interface CheatConsoleProps {
  onExecuteCheat: (cheat: string) => { success: boolean; message: string };
  onClose: () => void;
}

export const CheatConsole: React.FC<CheatConsoleProps> = ({
  onExecuteCheat,
  onClose,
}) => {
  const [input, setInput] = useState('');
  const [history, setHistory] = useState<string[]>([
    'Simitone Debug Console v1.0',
    'Type "help" for a list of cheat commands.',
  ]);

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!input.trim()) return;
    const command = input.trim();
    const result = onExecuteCheat(command);
    if (result.success) {
      sounds.playCashRegister();
    } else {
      sounds.playClick();
    }
    setHistory((prev) => [...prev, `> ${command}`, result.message]);
    setInput('');
  };

  return (
    <div className="fixed top-14 left-1/2 -translate-x-1/2 w-full max-w-lg z-50 p-3 pointer-events-auto select-none font-mono">
      <div className="bg-slate-950/95 border border-emerald-500/60 rounded-2xl shadow-2xl backdrop-blur-xl overflow-hidden flex flex-col">
        {/* Console Header */}
        <div className="px-3 py-2 bg-emerald-950/40 border-b border-emerald-800/40 flex items-center justify-between text-emerald-400 text-xs">
          <div className="flex items-center gap-1.5 font-bold">
            <Terminal className="w-3.5 h-3.5" />
            Cheat Command Prompt
          </div>
          <button
            onClick={() => { sounds.playClick(); onClose(); }}
            className="p-1 hover:bg-emerald-900/40 rounded text-slate-400 hover:text-white"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Console Output */}
        <div className="p-3 text-xs text-emerald-300/90 max-h-40 overflow-y-auto space-y-1 bg-black/50">
          {history.map((line, i) => (
            <div key={i} className="leading-tight break-all">
              {line}
            </div>
          ))}
        </div>

        {/* Input */}
        <form onSubmit={handleSubmit} className="p-2 bg-slate-900/90 border-t border-slate-800 flex gap-2">
          <span className="text-emerald-400 text-xs self-center font-bold pl-1">&gt;</span>
          <input
            type="text"
            autoFocus
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder="rosebud, motherlode, maxmotives..."
            className="flex-1 bg-transparent border-0 text-white text-xs focus:outline-none placeholder:text-slate-600 font-mono"
          />
          <button
            type="submit"
            className="px-2.5 py-1 bg-emerald-600 hover:bg-emerald-500 rounded-lg text-white text-xs font-bold flex items-center gap-1"
          >
            <CornerDownLeft className="w-3 h-3" />
            Enter
          </button>
        </form>
      </div>
    </div>
  );
};
