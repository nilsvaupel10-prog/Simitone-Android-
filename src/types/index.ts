export type ScreenMode = 'setup' | 'game' | 'cas' | 'editor';

export type ResolutionPreset =
  | 'DeviceNative'
  | 'W1280x720'
  | 'W1600x900'
  | 'W1920x1080'
  | 'W2560x1440'
  | 'Custom';

export type GraphicsMode = 'OpenGLEs3' | 'OpenGLEs2Compatibility';

export type UiScaleOption = 'Automatic' | 'X100' | 'X125' | 'X150' | 'X200';

export interface SimitoneAndroidSettings {
  gamePath: string;
  treeUri: string;
  resolution: ResolutionPreset;
  customWidth: number;
  customHeight: number;
  enable3D: boolean;
  graphics: GraphicsMode;
  uiScale: UiScaleOption;
  haptics: boolean;
}

export type TouchAnchor =
  | 'TopLeft'
  | 'TopRight'
  | 'BottomLeft'
  | 'BottomRight'
  | 'Center';

export type TouchAction =
  | 'CameraForward'
  | 'CameraBack'
  | 'CameraLeft'
  | 'CameraRight'
  | 'ZoomIn'
  | 'ZoomOut'
  | 'RotateLeft'
  | 'RotateRight'
  | 'FreeCamera'
  | 'Pause'
  | 'Resume'
  | 'Speed1'
  | 'Speed2'
  | 'Speed3'
  | 'FrameAdvance'
  | 'ModeLive'
  | 'ModeBuild'
  | 'ModeBuy'
  | 'OpenOptions'
  | 'OpenInventory'
  | 'OpenFamily'
  | 'OpenSkills'
  | 'Cancel'
  | 'EditControls'
  | 'Menu';

export interface TouchWidgetLayout {
  id: string;
  anchor: TouchAnchor;
  offsetX: number; // 0..1 fraction of screen width
  offsetY: number; // 0..1 fraction of screen height
  width: number;   // fraction of screen height
  height: number;  // fraction of screen height
  opacity: number; // 0..1
  visible: boolean;
  scale: number;
}

export interface TouchControlSettings {
  opacity: number;
  scale: number;
  haptics: boolean;
  hapticStrength: number;
  pinchToZoom: boolean;
  twoFingerRotate: boolean;
  dragToPan: boolean;
  stickDeadZone: number;
  snapToGrid: boolean;
  gridSize: number;
  showLabels: boolean;
  activeProfile: string;
}

export interface TouchControlProfile {
  name: string;
  description: string;
  settings: TouchControlSettings;
  widgets: Record<string, TouchWidgetLayout>;
  order: string[];
  isBuiltIn?: boolean;
}

// Sims Game Types
export interface Motives {
  hunger: number;   // 0 - 100
  comfort: number;
  hygiene: number;
  bladder: number;
  energy: number;
  fun: number;
  social: number;
  room: number;
}

export interface Skills {
  cooking: number;    // 0 - 10
  mechanical: number;
  charisma: number;
  logic: number;
  body: number;
  creativity: number;
}

export interface Personality {
  neat: number;      // 0 - 10
  outgoing: number;
  active: number;
  playful: number;
  nice: number;
}

export type ZodiacSign =
  | 'Aries'
  | 'Taurus'
  | 'Gemini'
  | 'Cancer'
  | 'Leo'
  | 'Virgo'
  | 'Libra'
  | 'Scorpio'
  | 'Sagittarius'
  | 'Capricorn'
  | 'Aquarius'
  | 'Pisces';

export interface SimJob {
  title: string;
  careerTrack: string;
  level: number;
  salary: number; // per day §
  hours: string;
  carpool: string;
}

export interface SimPerson {
  id: string;
  firstName: string;
  lastName: string;
  gender: 'male' | 'female';
  age: 'adult' | 'child';
  skinTone: 'light' | 'medium' | 'dark';
  headIndex: number;
  bodyIndex: number;
  bio: string;
  zodiac: ZodiacSign;
  motives: Motives;
  skills: Skills;
  personality: Personality;
  job: SimJob;
  inventory: string[];
  relationships: Record<string, { daily: number; lifetime: number }>;
}

export type GameSpeed = 0 | 1 | 2 | 3;
export type GameMode = 'live' | 'buy' | 'build' | 'options';
export type WallCutaway = 'wallsDown' | 'cutaway' | 'wallsUp' | 'roof';

export interface LotObject {
  id: string;
  catalogId: string;
  name: string;
  x: number; // grid coords 0..15
  y: number;
  width: number;
  height: number;
  rotation: number; // 0, 90, 180, 270
  category: CatalogCategory;
  price: number;
  interactions: string[];
  motiveBonus?: Partial<Motives>;
  color: string;
  icon: string;
}

export type CatalogCategory =
  | 'seating'
  | 'surfaces'
  | 'appliances'
  | 'electronics'
  | 'plumbing'
  | 'decorative'
  | 'lighting'
  | 'misc';

export interface CatalogItem {
  id: string;
  name: string;
  category: CatalogCategory;
  price: number;
  description: string;
  width: number;
  height: number;
  color: string;
  icon: string;
  motiveBonus?: Partial<Motives>;
  interactions: string[];
}

export interface InteractionAction {
  id: string;
  label: string;
  targetObjectId?: string;
  targetObjectName?: string;
  durationSeconds: number;
  motiveEffects: Partial<Motives>;
  animation: string;
}
