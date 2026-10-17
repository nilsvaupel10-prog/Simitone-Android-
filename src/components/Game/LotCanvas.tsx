import React, { useRef, useEffect, useState } from 'react';
import { LotObject, SimPerson, WallCutaway, GameMode, CatalogItem } from '../../types';
import { sounds } from '../../audio/sound';

interface LotCanvasProps {
  objects: LotObject[];
  activeSim: SimPerson;
  gameMode: GameMode;
  wallCutaway: WallCutaway;
  cameraPan: { x: number; y: number };
  zoomLevel: number; // 0.6 .. 1.8
  cameraRotation: number; // 0, 90, 180, 270
  selectedCatalogItem?: CatalogItem | null;
  onSelectObject: (obj: LotObject, screenPos: { x: number; y: number }) => void;
  onPlaceObject?: (gridX: number, gridY: number) => void;
  timeOfDayHours: number; // 0 .. 24
  currentActionLabel?: string | null;
}

export const LotCanvas: React.FC<LotCanvasProps> = ({
  objects,
  activeSim,
  gameMode,
  wallCutaway,
  cameraPan,
  zoomLevel,
  cameraRotation,
  selectedCatalogItem,
  onSelectObject,
  onPlaceObject,
  timeOfDayHours,
  currentActionLabel,
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [simPos, setSimPos] = useState({ x: 7, y: 7 });
  const [simTarget, setSimTarget] = useState({ x: 7, y: 7 });
  const [simAnimFrame, setSimAnimFrame] = useState(0);
  const [plumbobRotation, setPlumbobRotation] = useState(0);

  // Sim locomotion loop
  useEffect(() => {
    const interval = setInterval(() => {
      setSimPos((current) => {
        const dx = simTarget.x - current.x;
        const dy = simTarget.y - current.y;
        if (Math.abs(dx) < 0.1 && Math.abs(dy) < 0.1) {
          return current;
        }
        const speed = 0.08;
        return {
          x: current.x + Math.sign(dx) * Math.min(Math.abs(dx), speed),
          y: current.y + Math.sign(dy) * Math.min(Math.abs(dy), speed),
        };
      });
      setSimAnimFrame((f) => (f + 1) % 8);
      setPlumbobRotation((r) => (r + 0.1) % (Math.PI * 2));
    }, 50);

    return () => clearInterval(interval);
  }, [simTarget]);

  // Canvas render loop
  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Handle high DPI
    const dpr = window.devicePixelRatio || 1;
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    canvas.width = width * dpr;
    canvas.height = height * dpr;
    ctx.scale(dpr, dpr);

    // Clear background
    ctx.clearRect(0, 0, width, height);

    // Calculate isometric projection center
    const tileWidth = 64 * zoomLevel;
    const tileHeight = 32 * zoomLevel;
    const originX = width / 2 + cameraPan.x;
    const originY = height / 3 + cameraPan.y;

    // Convert grid (gx, gy) to 2D isometric screen coords
    const toIso = (gx: number, gy: number, gz = 0) => {
      let rx = gx;
      let ry = gy;
      if (cameraRotation === 90) {
        rx = 16 - gy;
        ry = gx;
      } else if (cameraRotation === 180) {
        rx = 16 - gx;
        ry = 16 - gy;
      } else if (cameraRotation === 270) {
        rx = gy;
        ry = 16 - gx;
      }
      const isoX = originX + (rx - ry) * (tileWidth / 2);
      const isoY = originY + (rx + ry) * (tileHeight / 2) - gz * (tileHeight * 1.5);
      return { x: isoX, y: isoY };
    };

    // 1. Draw Lot Ground Grass & Floor Tiles (16x16 grid)
    for (let gy = 0; gy < 16; gy++) {
      for (let gx = 0; gx < 16; gx++) {
        const { x, y } = toIso(gx, gy);

        // Check if inside house bounds (x: 1..15, y: 1..11)
        const isHouseFloor = gx >= 1 && gx <= 15 && gy >= 1 && gy <= 11;
        const isKitchen = gx <= 5 && gy >= 8 && isHouseFloor;
        const isBath = gx >= 12 && gy >= 8 && isHouseFloor;

        ctx.beginPath();
        ctx.moveTo(x, y);
        ctx.lineTo(x + tileWidth / 2, y + tileHeight / 2);
        ctx.lineTo(x, y + tileHeight);
        ctx.lineTo(x - tileWidth / 2, y + tileHeight / 2);
        ctx.closePath();

        if (isKitchen) {
          // Checkered black and white tile
          ctx.fillStyle = (gx + gy) % 2 === 0 ? '#e2e8f0' : '#1e293b';
        } else if (isBath) {
          // Blue tile
          ctx.fillStyle = (gx + gy) % 2 === 0 ? '#0284c7' : '#38bdf8';
        } else if (isHouseFloor) {
          // Warm parquet hardwood
          ctx.fillStyle = (gx + gy) % 2 === 0 ? '#b45309' : '#92400e';
        } else {
          // Lush green lawn
          ctx.fillStyle = (gx + gy) % 2 === 0 ? '#15803d' : '#166534';
        }
        ctx.fill();

        // Grid lines (subtle, or brighter in buy/build mode)
        ctx.strokeStyle = gameMode === 'buy' || gameMode === 'build' ? 'rgba(255,255,255,0.25)' : 'rgba(0,0,0,0.12)';
        ctx.lineWidth = 1;
        ctx.stroke();
      }
    }

    // 2. Draw Walls (if not wallsDown)
    if (wallCutaway !== 'wallsDown') {
      const wallHeight = tileHeight * 2.2;
      ctx.strokeStyle = '#475569';
      ctx.lineWidth = 4 * zoomLevel;

      // Outer house back walls (gy = 1, gx = 1..15)
      for (let gx = 1; gx <= 15; gx++) {
        const p1 = toIso(gx, 1);
        const p2 = toIso(gx + 1, 1);
        ctx.beginPath();
        ctx.moveTo(p1.x, p1.y);
        ctx.lineTo(p1.x, p1.y - wallHeight);
        ctx.lineTo(p2.x, p2.y - wallHeight);
        ctx.lineTo(p2.x, p2.y);
        ctx.fillStyle = '#64748b';
        ctx.fill();
        ctx.stroke();
      }

      // Outer house left walls (gx = 1, gy = 1..11)
      for (let gy = 1; gy <= 11; gy++) {
        const p1 = toIso(1, gy);
        const p2 = toIso(1, gy + 1);
        ctx.beginPath();
        ctx.moveTo(p1.x, p1.y);
        ctx.lineTo(p1.x, p1.y - wallHeight);
        ctx.lineTo(p2.x, p2.y - wallHeight);
        ctx.lineTo(p2.x, p2.y);
        ctx.fillStyle = '#475569';
        ctx.fill();
        ctx.stroke();
      }
    }

    // 3. Sort & Render Placed Objects
    const sortedObjects = [...objects].sort((a, b) => a.x + a.y - (b.x + b.y));

    sortedObjects.forEach((obj) => {
      const { x, y } = toIso(obj.x + obj.width / 2, obj.y + obj.height / 2);
      const w = obj.width * tileWidth * 0.7;
      const h = obj.height * tileHeight * 1.5;

      // Draw object footprint shadow
      ctx.beginPath();
      ctx.ellipse(x, y + tileHeight / 3, w * 0.45, h * 0.25, 0, 0, Math.PI * 2);
      ctx.fillStyle = 'rgba(0, 0, 0, 0.28)';
      ctx.fill();

      // Draw isometric 3D-styled block / furniture
      ctx.save();
      ctx.translate(x, y);

      // Main box
      ctx.fillStyle = obj.color;
      ctx.strokeStyle = 'rgba(0, 0, 0, 0.6)';
      ctx.lineWidth = 1.5;

      const boxW = Math.max(24, w * 0.65);
      const boxH = Math.max(22, h * 0.85);

      // Top face
      ctx.beginPath();
      ctx.moveTo(0, -boxH);
      ctx.lineTo(boxW / 2, -boxH + boxW / 4);
      ctx.lineTo(0, -boxH + boxW / 2);
      ctx.lineTo(-boxW / 2, -boxH + boxW / 4);
      ctx.closePath();
      ctx.fillStyle = obj.color;
      ctx.fill();
      ctx.stroke();

      // Front Left face
      ctx.beginPath();
      ctx.moveTo(-boxW / 2, -boxH + boxW / 4);
      ctx.lineTo(0, -boxH + boxW / 2);
      ctx.lineTo(0, boxW / 4);
      ctx.lineTo(-boxW / 2, 0);
      ctx.closePath();
      ctx.fillStyle = adjustBrightness(obj.color, -25);
      ctx.fill();
      ctx.stroke();

      // Front Right face
      ctx.beginPath();
      ctx.moveTo(0, -boxH + boxW / 2);
      ctx.lineTo(boxW / 2, -boxH + boxW / 4);
      ctx.lineTo(boxW / 2, 0);
      ctx.lineTo(0, boxW / 4);
      ctx.closePath();
      ctx.fillStyle = adjustBrightness(obj.color, -45);
      ctx.fill();
      ctx.stroke();

      // Label
      ctx.fillStyle = '#ffffff';
      ctx.font = `bold ${Math.max(9, 10 * zoomLevel)}px sans-serif`;
      ctx.textAlign = 'center';
      ctx.shadowColor = '#000000';
      ctx.shadowBlur = 4;
      ctx.fillText(obj.name.split(' ')[0], 0, -boxH - 6);
      ctx.restore();
    });

    // 4. Render Active Sim Character
    const { x: simScreenX, y: simScreenY } = toIso(simPos.x, simPos.y);

    // Sim drop shadow
    ctx.beginPath();
    ctx.ellipse(simScreenX, simScreenY + tileHeight / 3, 16 * zoomLevel, 8 * zoomLevel, 0, 0, Math.PI * 2);
    ctx.fillStyle = 'rgba(0, 0, 0, 0.35)';
    ctx.fill();

    // Sim Body
    const simHeight = 44 * zoomLevel;
    ctx.save();
    ctx.translate(simScreenX, simScreenY - simHeight / 2);

    // Legs / Pants
    const legOffset = Math.sin(simAnimFrame) * 3;
    ctx.fillStyle = '#1e3a8a'; // Blue jeans
    ctx.fillRect(-6 * zoomLevel + legOffset, 4 * zoomLevel, 5 * zoomLevel, 14 * zoomLevel);
    ctx.fillRect(1 * zoomLevel - legOffset, 4 * zoomLevel, 5 * zoomLevel, 14 * zoomLevel);

    // Torso / Shirt
    ctx.fillStyle = activeSim.gender === 'male' ? '#dc2626' : '#ec4899'; // Red shirt / pink
    ctx.beginPath();
    ctx.roundRect(-8 * zoomLevel, -12 * zoomLevel, 16 * zoomLevel, 18 * zoomLevel, 4 * zoomLevel);
    ctx.fill();

    // Head
    ctx.fillStyle = activeSim.skinTone === 'light' ? '#fde047' : activeSim.skinTone === 'medium' ? '#d97706' : '#78350f';
    ctx.beginPath();
    ctx.arc(0, -20 * zoomLevel, 8 * zoomLevel, 0, Math.PI * 2);
    ctx.fill();

    // Hair
    ctx.fillStyle = '#451a03'; // Brown hair
    ctx.beginPath();
    ctx.arc(0, -23 * zoomLevel, 8.5 * zoomLevel, Math.PI, Math.PI * 2);
    ctx.fill();

    // Sim Name Tag
    ctx.fillStyle = '#ffffff';
    ctx.font = `bold ${Math.max(10, 11 * zoomLevel)}px sans-serif`;
    ctx.textAlign = 'center';
    ctx.shadowColor = '#000000';
    ctx.shadowBlur = 4;
    ctx.fillText(`${activeSim.firstName} ${activeSim.lastName}`, 0, -32 * zoomLevel);

    // Current Action Speech Bubble if executing interaction
    if (currentActionLabel) {
      ctx.fillStyle = 'rgba(15, 23, 42, 0.85)';
      ctx.strokeStyle = '#38bdf8';
      ctx.lineWidth = 1.5;
      const bubbleW = ctx.measureText(currentActionLabel).width + 16;
      ctx.beginPath();
      ctx.roundRect(-bubbleW / 2, -56 * zoomLevel, bubbleW, 18 * zoomLevel, 6);
      ctx.fill();
      ctx.stroke();

      ctx.fillStyle = '#38bdf8';
      ctx.font = `600 ${Math.max(9, 10 * zoomLevel)}px sans-serif`;
      ctx.fillText(currentActionLabel, 0, -43 * zoomLevel);
    }

    // 5. Plumbob Floating Above Sim's Head!
    const plumbobY = -48 * zoomLevel + Math.sin(plumbobRotation * 2) * 4;
    const plumbobSize = 14 * zoomLevel;

    // Plumbob color changes based on Sim's hunger/energy mood
    const avgMotive = (activeSim.motives.hunger + activeSim.motives.energy + activeSim.motives.fun) / 3;
    let plumbobColor = '#22c55e'; // Green (happy)
    if (avgMotive < 35) {
      plumbobColor = '#ef4444'; // Red (distressed)
    } else if (avgMotive < 60) {
      plumbobColor = '#eab308'; // Yellow (mediocre)
    }

    // Diamond Plumbob polygon
    ctx.save();
    ctx.translate(0, plumbobY);
    ctx.scale(Math.cos(plumbobRotation), 1); // 3D spin effect

    // Top triangle
    ctx.beginPath();
    ctx.moveTo(0, -plumbobSize);
    ctx.lineTo(plumbobSize / 2, 0);
    ctx.lineTo(0, plumbobSize * 0.4);
    ctx.lineTo(-plumbobSize / 2, 0);
    ctx.closePath();
    ctx.fillStyle = plumbobColor;
    ctx.shadowColor = plumbobColor;
    ctx.shadowBlur = 12;
    ctx.fill();
    ctx.strokeStyle = '#ffffff';
    ctx.lineWidth = 1;
    ctx.stroke();

    // Bottom triangle
    ctx.beginPath();
    ctx.moveTo(0, plumbobSize * 1.2);
    ctx.lineTo(plumbobSize / 2, 0);
    ctx.lineTo(0, plumbobSize * 0.4);
    ctx.lineTo(-plumbobSize / 2, 0);
    ctx.closePath();
    ctx.fillStyle = adjustBrightness(plumbobColor, -30);
    ctx.fill();
    ctx.stroke();

    ctx.restore();
    ctx.restore();

    // 6. Day/Night ambient lighting overlay
    const hour = timeOfDayHours % 24;
    let ambientOverlay = 'rgba(0, 0, 0, 0)';
    if (hour < 5 || hour >= 22) {
      ambientOverlay = 'rgba(15, 23, 42, 0.45)'; // Night
    } else if (hour >= 5 && hour < 7) {
      ambientOverlay = 'rgba(249, 115, 22, 0.15)'; // Dawn
    } else if (hour >= 18 && hour < 22) {
      ambientOverlay = 'rgba(99, 102, 241, 0.25)'; // Twilight
    }
    ctx.fillStyle = ambientOverlay;
    ctx.fillRect(0, 0, width, height);
  }, [
    objects,
    activeSim,
    gameMode,
    wallCutaway,
    cameraPan,
    zoomLevel,
    cameraRotation,
    simPos,
    simAnimFrame,
    plumbobRotation,
    timeOfDayHours,
    currentActionLabel,
  ]);

  // Handle click on canvas
  const handleCanvasClick = (e: React.MouseEvent<HTMLCanvasElement>) => {
    const canvas = canvasRef.current;
    if (!canvas) return;
    const rect = canvas.getBoundingClientRect();
    const clickX = e.clientX - rect.left;
    const clickY = e.clientY - rect.top;

    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    const tileWidth = 64 * zoomLevel;
    const tileHeight = 32 * zoomLevel;
    const originX = width / 2 + cameraPan.x;
    const originY = height / 3 + cameraPan.y;

    // Invert isometric projection
    const relX = clickX - originX;
    const relY = clickY - originY;
    const invX = (relY / (tileHeight / 2) + relX / (tileWidth / 2)) / 2;
    const invY = (relY / (tileHeight / 2) - relX / (tileWidth / 2)) / 2;

    const gridX = Math.floor(invX);
    const gridY = Math.floor(invY);

    if (gridX >= 0 && gridX < 16 && gridY >= 0 && gridY < 16) {
      // In Buy/Build mode, place item
      if (gameMode === 'buy' || gameMode === 'build') {
        sounds.playClick();
        onPlaceObject?.(gridX, gridY);
        return;
      }

      // Check if clicked on a placed object
      const hitObj = objects.find(
        (o) =>
          gridX >= o.x &&
          gridX < o.x + o.width &&
          gridY >= o.y &&
          gridY < o.y + o.height
      );

      if (hitObj) {
        sounds.playPieMenu();
        // Move Sim towards object
        setSimTarget({ x: hitObj.x, y: hitObj.y });
        onSelectObject(hitObj, { x: e.clientX, y: e.clientY });
        return;
      }

      // Clicked on floor tile -> Walk here
      sounds.playClick();
      setSimTarget({ x: gridX, y: gridY });
    }
  };

  return (
    <div className="absolute inset-0 w-full h-full overflow-hidden bg-slate-950 cursor-crosshair">
      <canvas
        ref={canvasRef}
        onClick={handleCanvasClick}
        className="w-full h-full block"
      />
    </div>
  );
};

function adjustBrightness(hex: string, percent: number): string {
  const num = parseInt(hex.replace('#', ''), 16);
  if (isNaN(num)) return hex;
  const amt = Math.round(2.55 * percent);
  const R = (num >> 16) + amt;
  const G = ((num >> 8) & 0x00ff) + amt;
  const B = (num & 0x0000ff) + amt;
  return (
    '#' +
    (
      0x1000000 +
      (R < 255 ? (R < 1 ? 0 : R) : 255) * 0x10000 +
      (G < 255 ? (G < 1 ? 0 : G) : 255) * 0x100 +
      (B < 255 ? (B < 1 ? 0 : B) : 255)
    )
      .toString(16)
      .slice(1)
  );
}
