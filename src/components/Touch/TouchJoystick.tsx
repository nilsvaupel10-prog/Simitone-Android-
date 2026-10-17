import React, { useState, useRef, useEffect } from 'react';
import { TouchAnchor } from '../../types';
import { sounds } from '../../audio/sound';

interface TouchJoystickProps {
  id: string;
  anchor: TouchAnchor;
  offsetX: number;
  offsetY: number;
  width: number;
  height: number;
  scale: number;
  opacity: number;
  haptics: boolean;
  isEditMode?: boolean;
  isSelected?: boolean;
  onSelect?: () => void;
  onPan?: (dx: number, dy: number) => void;
}

export const TouchJoystick: React.FC<TouchJoystickProps> = ({
  anchor,
  offsetX,
  offsetY,
  width,
  height,
  scale,
  opacity,
  haptics,
  isEditMode,
  isSelected,
  onSelect,
  onPan,
}) => {
  const [knobPos, setKnobPos] = useState({ x: 0, y: 0 });
  const [isDragging, setIsDragging] = useState(false);
  const baseRef = useRef<HTMLDivElement>(null);
  const dragCenter = useRef({ x: 0, y: 0 });

  const style: React.CSSProperties = {
    opacity: isEditMode ? 0.95 : opacity,
    transform: `scale(${scale})`,
    transformOrigin: 'center center',
  };

  switch (anchor) {
    case 'TopLeft':
      style.top = `${offsetY * 100}%`;
      style.left = `${offsetX * 100}%`;
      break;
    case 'TopRight':
      style.top = `${offsetY * 100}%`;
      style.right = `${offsetX * 100}%`;
      break;
    case 'BottomLeft':
      style.bottom = `${offsetY * 100}%`;
      style.left = `${offsetX * 100}%`;
      break;
    case 'BottomRight':
      style.bottom = `${offsetY * 100}%`;
      style.right = `${offsetX * 100}%`;
      break;
    case 'Center':
      style.top = `calc(50% + ${offsetY * 100}%)`;
      style.left = `calc(50% + ${offsetX * 100}%)`;
      style.transform = `translate(-50%, -50%) scale(${scale})`;
      break;
  }

  const handlePointerDown = (e: React.PointerEvent) => {
    e.stopPropagation();
    if (isEditMode) {
      onSelect?.();
      return;
    }
    if (baseRef.current) {
      const rect = baseRef.current.getBoundingClientRect();
      dragCenter.current = {
        x: rect.left + rect.width / 2,
        y: rect.top + rect.height / 2,
      };
      setIsDragging(true);
      if (haptics) sounds.triggerHaptic(20);
      handlePointerMove(e);
    }
  };

  const handlePointerMove = (e: React.PointerEvent) => {
    if (!isDragging || isEditMode) return;
    const dx = e.clientX - dragCenter.current.x;
    const dy = e.clientY - dragCenter.current.y;
    const maxRadius = 45;
    const dist = Math.sqrt(dx * dx + dy * dy);
    const clampedDist = Math.min(dist, maxRadius);
    const angle = Math.atan2(dy, dx);
    const finalX = Math.cos(angle) * clampedDist;
    const finalY = Math.sin(angle) * clampedDist;

    setKnobPos({ x: finalX, y: finalY });
    onPan?.(finalX / maxRadius, finalY / maxRadius);
  };

  const handlePointerUp = () => {
    if (isEditMode) return;
    setIsDragging(false);
    setKnobPos({ x: 0, y: 0 });
    onPan?.(0, 0);
  };

  useEffect(() => {
    if (!isDragging) return;
    const handleGlobalUp = () => {
      setIsDragging(false);
      setKnobPos({ x: 0, y: 0 });
      onPan?.(0, 0);
    };
    window.addEventListener('pointerup', handleGlobalUp);
    return () => window.removeEventListener('pointerup', handleGlobalUp);
  }, [isDragging, onPan]);

  const sizeRem = Math.max(7, width * 35);

  return (
    <div
      ref={baseRef}
      style={{
        ...style,
        width: `${sizeRem}rem`,
        height: `${sizeRem}rem`,
      }}
      className={`absolute z-30 select-none pointer-events-auto touch-none ${
        isSelected ? 'ring-2 ring-cyan-400 rounded-full p-1' : ''
      }`}
      onPointerDown={handlePointerDown}
      onPointerMove={handlePointerMove}
      onPointerUp={handlePointerUp}
      onPointerCancel={handlePointerUp}
    >
      {/* Outer base plate */}
      <div
        className={`w-full h-full rounded-full border border-slate-600/70 bg-slate-950/65 shadow-2xl backdrop-blur-md flex items-center justify-center relative ${
          isEditMode ? 'cursor-move border-dashed border-cyan-400' : ''
        }`}
      >
        {/* Directional ticks */}
        <div className="absolute top-2 w-1.5 h-1.5 rounded-full bg-slate-500/60" />
        <div className="absolute bottom-2 w-1.5 h-1.5 rounded-full bg-slate-500/60" />
        <div className="absolute left-2 w-1.5 h-1.5 rounded-full bg-slate-500/60" />
        <div className="absolute right-2 w-1.5 h-1.5 rounded-full bg-slate-500/60" />

        {/* Center thumb stick */}
        <div
          style={{
            transform: `translate(${knobPos.x}px, ${knobPos.y}px)`,
          }}
          className={`w-14 h-14 rounded-full border border-slate-500 shadow-lg flex items-center justify-center transition-transform duration-75 ${
            isDragging
              ? 'bg-blue-600 border-blue-300 text-white shadow-[0_0_15px_rgba(59,130,246,0.6)]'
              : 'bg-slate-800/90 hover:bg-slate-700/90 text-slate-300'
          }`}
        >
          <div className="w-5 h-5 rounded-full border border-slate-400/40 bg-slate-900/40 flex items-center justify-center">
            <div className="w-2 h-2 rounded-full bg-blue-400" />
          </div>
        </div>

        {/* Label */}
        <span className="absolute -bottom-5 text-[10px] font-semibold text-white/90 bg-black/60 px-1.5 py-0.5 rounded whitespace-nowrap shadow backdrop-blur-sm pointer-events-none">
          Camera Pan
        </span>
      </div>
    </div>
  );
};
