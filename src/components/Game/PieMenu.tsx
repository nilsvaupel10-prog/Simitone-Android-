import React from 'react';
import { sounds } from '../../audio/sound';
import { X } from 'lucide-react';

interface PieMenuProps {
  x: number;
  y: number;
  title: string;
  options: string[];
  onSelectOption: (option: string) => void;
  onClose: () => void;
}

export const PieMenu: React.FC<PieMenuProps> = ({
  x,
  y,
  title,
  options,
  onSelectOption,
  onClose,
}) => {
  const radius = 80;
  const count = options.length;

  return (
    <div
      className="fixed z-50 pointer-events-auto select-none"
      style={{
        left: `${x}px`,
        top: `${y}px`,
        transform: 'translate(-50%, -50%)',
      }}
    >
      {/* Center hub */}
      <div className="relative flex items-center justify-center">
        {/* Central object badge */}
        <div className="w-16 h-16 rounded-full bg-gradient-to-b from-blue-600 to-indigo-900 border-2 border-cyan-300 shadow-[0_0_20px_rgba(34,197,94,0.4)] flex flex-col items-center justify-center p-1 text-center backdrop-blur-md">
          <img src="/plumbob.svg" alt="Plumbob" className="w-4 h-5 drop-shadow animate-bounce" />
          <span className="text-[10px] font-bold text-white truncate max-w-[50px] leading-tight">
            {title}
          </span>
        </div>

        {/* Outer radial action petals */}
        {options.map((opt, i) => {
          const angle = (i * (2 * Math.PI)) / count - Math.PI / 2;
          const optX = Math.cos(angle) * radius;
          const optY = Math.sin(angle) * radius;

          return (
            <button
              key={opt}
              onClick={(e) => {
                e.stopPropagation();
                sounds.playClick();
                onSelectOption(opt);
              }}
              style={{
                transform: `translate(${optX}px, ${optY}px) translate(-50%, -50%)`,
              }}
              className="absolute left-1/2 top-1/2 px-3 py-1.5 rounded-full bg-slate-900/90 hover:bg-blue-600 border border-cyan-400/80 shadow-[0_4px_12px_rgba(0,0,0,0.5)] text-white text-xs font-semibold whitespace-nowrap active:scale-95 transition-all hover:scale-105 hover:shadow-[0_0_15px_rgba(59,130,246,0.6)] cursor-pointer"
            >
              {opt}
            </button>
          );
        })}

        {/* Close button */}
        <button
          onClick={(e) => {
            e.stopPropagation();
            sounds.playClick();
            onClose();
          }}
          className="absolute -top-7 right-0 p-1 bg-red-600 hover:bg-red-500 rounded-full text-white shadow"
          title="Cancel"
        >
          <X className="w-3.5 h-3.5" />
        </button>
      </div>
    </div>
  );
};
