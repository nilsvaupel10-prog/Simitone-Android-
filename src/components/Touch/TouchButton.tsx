import React, { useState } from 'react';
import { TouchAction, TouchAnchor } from '../../types';
import { ACTION_DESCRIPTIONS } from '../../services/touchProfiles';
import { sounds } from '../../audio/sound';

interface TouchButtonProps {
  id: string;
  action: TouchAction;
  anchor: TouchAnchor;
  offsetX: number;
  offsetY: number;
  width: number;
  height: number;
  scale: number;
  opacity: number;
  haptics: boolean;
  showLabels: boolean;
  isEditMode?: boolean;
  isSelected?: boolean;
  onSelect?: () => void;
  onAction?: (action: TouchAction) => void;
  onActionStart?: (action: TouchAction) => void;
  onActionEnd?: (action: TouchAction) => void;
}

export const TouchButton: React.FC<TouchButtonProps> = ({
  id,
  action,
  anchor,
  offsetX,
  offsetY,
  width,
  height,
  scale,
  opacity,
  haptics,
  showLabels,
  isEditMode,
  isSelected,
  onSelect,
  onAction,
  onActionStart,
  onActionEnd,
}) => {
  const [isActive, setIsActive] = useState(false);
  const desc = ACTION_DESCRIPTIONS[action] || { label: id, icon: 'unknown.png' };

  // Calculate position based on anchor
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
    setIsActive(true);
    if (haptics) sounds.triggerHaptic(20);
    sounds.playClick();
    onActionStart?.(action);
    onAction?.(action);
  };

  const handlePointerUp = (e: React.PointerEvent) => {
    e.stopPropagation();
    if (isEditMode) return;
    setIsActive(false);
    onActionEnd?.(action);
  };

  const buttonSizeRem = Math.max(2.8, height * 30);

  return (
    <div
      style={style}
      className={`absolute z-30 flex flex-col items-center select-none pointer-events-auto touch-none transition-transform duration-75 ${
        isSelected ? 'ring-2 ring-cyan-400 rounded-full p-1' : ''
      }`}
      onPointerDown={handlePointerDown}
      onPointerUp={handlePointerUp}
      onPointerLeave={handlePointerUp}
      onPointerCancel={handlePointerUp}
    >
      <div
        style={{
          width: `${buttonSizeRem}rem`,
          height: `${buttonSizeRem}rem`,
        }}
        className={`relative rounded-full flex items-center justify-center border shadow-lg backdrop-blur-md transition-all active:scale-95 ${
          isActive
            ? 'bg-blue-600/90 border-blue-300 shadow-[0_0_15px_rgba(59,130,246,0.6)] text-white'
            : 'bg-slate-900/75 hover:bg-slate-800/85 border-slate-600/60 text-slate-200'
        } ${isEditMode ? 'cursor-move border-dashed border-cyan-400' : ''}`}
      >
        <img
          src={`/icons/${desc.icon}`}
          alt={desc.label}
          className="w-3/5 h-3/5 object-contain pointer-events-none drop-shadow"
          onError={(e) => {
            // fallback if icon not found
            (e.target as HTMLElement).style.display = 'none';
          }}
        />
        {/* Subtle highlight ring */}
        <div className="absolute inset-0 rounded-full border border-white/10 pointer-events-none" />
      </div>

      {showLabels && !isEditMode && (
        <span className="text-[10px] font-semibold text-white/90 bg-black/60 px-1.5 py-0.5 rounded mt-1 whitespace-nowrap shadow backdrop-blur-sm pointer-events-none">
          {desc.label}
        </span>
      )}
    </div>
  );
};
