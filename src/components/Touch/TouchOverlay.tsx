import React from 'react';
import { TouchControlProfile, TouchAction } from '../../types';
import { TouchButton } from './TouchButton';
import { TouchJoystick } from './TouchJoystick';

interface TouchOverlayProps {
  profile: TouchControlProfile;
  isEditMode?: boolean;
  selectedWidgetId?: string | null;
  onSelectWidget?: (id: string) => void;
  onAction: (action: TouchAction) => void;
  onPan: (dx: number, dy: number) => void;
}

const WIDGET_ACTION_MAP: Record<string, TouchAction> = {
  zoom_in: 'ZoomIn',
  zoom_out: 'ZoomOut',
  rotate_left: 'RotateLeft',
  rotate_right: 'RotateRight',
  pause: 'Pause',
  speed_1: 'Speed1',
  speed_2: 'Speed2',
  speed_3: 'Speed3',
  menu: 'Menu',
  options: 'OpenOptions',
  edit: 'EditControls',
  mode_live: 'ModeLive',
  mode_build: 'ModeBuild',
  mode_buy: 'ModeBuy',
  inventory: 'OpenInventory',
  family: 'OpenFamily',
  skills: 'OpenSkills',
  cancel: 'Cancel',
  free_camera: 'FreeCamera',
  frame_advance: 'FrameAdvance',
};

export const TouchOverlay: React.FC<TouchOverlayProps> = ({
  profile,
  isEditMode = false,
  selectedWidgetId,
  onSelectWidget,
  onAction,
  onPan,
}) => {
  const { widgets, settings } = profile;

  return (
    <div className="absolute inset-0 pointer-events-none z-20 overflow-hidden">
      {Object.entries(widgets).map(([id, widget]) => {
        if (!widget.visible && !isEditMode) return null;

        if (id === 'camera_stick') {
          return (
            <TouchJoystick
              key={id}
              id={id}
              anchor={widget.anchor}
              offsetX={widget.offsetX}
              offsetY={widget.offsetY}
              width={widget.width}
              height={widget.height}
              scale={widget.scale * settings.scale}
              opacity={widget.opacity * settings.opacity}
              haptics={settings.haptics}
              isEditMode={isEditMode}
              isSelected={selectedWidgetId === id}
              onSelect={() => onSelectWidget?.(id)}
              onPan={onPan}
            />
          );
        }

        const action = WIDGET_ACTION_MAP[id] || 'Menu';

        return (
          <TouchButton
            key={id}
            id={id}
            action={action}
            anchor={widget.anchor}
            offsetX={widget.offsetX}
            offsetY={widget.offsetY}
            width={widget.width}
            height={widget.height}
            scale={widget.scale * settings.scale}
            opacity={widget.opacity * settings.opacity}
            haptics={settings.haptics}
            showLabels={settings.showLabels}
            isEditMode={isEditMode}
            isSelected={selectedWidgetId === id}
            onSelect={() => onSelectWidget?.(id)}
            onAction={onAction}
          />
        );
      })}
    </div>
  );
};
