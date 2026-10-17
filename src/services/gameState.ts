import { SimPerson, LotObject, SimitoneAndroidSettings, TouchControlProfile } from '../types';
import { CATALOG_ITEMS } from './catalog';
import { createDefaultProfile, createOneHandedProfile, createTabletProfile } from './touchProfiles';

export const INITIAL_SETTINGS: SimitoneAndroidSettings = {
  gamePath: '/storage/emulated/0/The Sims',
  treeUri: 'content://com.android.externalstorage.documents/tree/primary%3AThe%20Sims',
  resolution: 'DeviceNative',
  customWidth: 1280,
  customHeight: 720,
  enable3D: true,
  graphics: 'OpenGLEs3',
  uiScale: 'Automatic',
  haptics: true,
};

export const INITIAL_SIM: SimPerson = {
  id: 'sim_1',
  firstName: 'Bob',
  lastName: 'Newbie',
  gender: 'male',
  age: 'adult',
  skinTone: 'light',
  headIndex: 1,
  bodyIndex: 2,
  bio: 'The classic Sim pioneer, discovering life in Simitone!',
  zodiac: 'Gemini',
  motives: {
    hunger: 75,
    comfort: 80,
    hygiene: 85,
    bladder: 80,
    energy: 90,
    fun: 65,
    social: 55,
    room: 70,
  },
  skills: {
    cooking: 2,
    mechanical: 3,
    charisma: 1,
    logic: 2,
    body: 1,
    creativity: 1,
  },
  personality: {
    neat: 4,
    outgoing: 5,
    active: 6,
    playful: 5,
    nice: 5,
  },
  job: {
    title: 'Executive Assistant',
    careerTrack: 'Business',
    level: 2,
    salary: 180,
    hours: '9:00 AM - 4:00 PM',
    carpool: '8:00 AM',
  },
  inventory: ['Newspaper', 'Pizza Coupon', 'House Deed'],
  relationships: {
    'Betty Newbie': { daily: 85, lifetime: 70 },
    'Bella Goth': { daily: 45, lifetime: 30 },
    'Mortimer Goth': { daily: 40, lifetime: 25 },
  },
};

export const INITIAL_LOT_OBJECTS: LotObject[] = [
  // Bedroom
  {
    id: 'obj_bed',
    catalogId: 'seat_bed_single',
    name: 'Spartan Single Bed',
    x: 2,
    y: 2,
    width: 2,
    height: 2,
    rotation: 0,
    category: 'seating',
    price: 300,
    interactions: ['Sleep', 'Make Bed', 'Relax'],
    color: '#0284c7',
    icon: 'Bed',
  },
  {
    id: 'obj_bed_lamp',
    catalogId: 'light_table',
    name: 'Shaded Table Lamp',
    x: 4,
    y: 2,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'lighting',
    price: 45,
    interactions: ['Turn On', 'Turn Off'],
    color: '#facc15',
    icon: 'Sun',
  },
  // Living Room
  {
    id: 'obj_sofa',
    catalogId: 'seat_sofa_cheap',
    name: 'Dignity Living Room Sofa',
    x: 8,
    y: 4,
    width: 2,
    height: 1,
    rotation: 0,
    category: 'seating',
    price: 250,
    interactions: ['Sit', 'Nap', 'Talk with Sim'],
    color: '#2563eb',
    icon: 'Sofa',
  },
  {
    id: 'obj_tv',
    catalogId: 'elec_tv_cheap',
    name: 'Monochrome Tube Television',
    x: 8,
    y: 2,
    width: 1,
    height: 1,
    rotation: 180,
    category: 'electronics',
    price: 350,
    interactions: ['Watch TV', 'Turn Off', 'Change Channel'],
    color: '#475569',
    icon: 'Tv',
  },
  {
    id: 'obj_stereo',
    catalogId: 'elec_stereo',
    name: 'GrooveMaster Boombox',
    x: 10,
    y: 2,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'electronics',
    price: 180,
    interactions: ['Listen', 'Dance', 'Turn Off'],
    color: '#64748b',
    icon: 'Music',
  },
  // Kitchen
  {
    id: 'obj_fridge',
    catalogId: 'appl_fridge_cheap',
    name: 'Llamark Frosty Fridge',
    x: 2,
    y: 9,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'appliances',
    price: 600,
    interactions: ['Have Quick Snack (§5)', 'Make Pancakes (§15)', 'Serve Dinner (§30)'],
    color: '#93c5fd',
    icon: 'Refrigerator',
  },
  {
    id: 'obj_counter',
    catalogId: 'surf_counter',
    name: 'Formica Countertop',
    x: 3,
    y: 9,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'surfaces',
    price: 140,
    interactions: ['Clean', 'Put Down'],
    color: '#d97706',
    icon: 'Table',
  },
  {
    id: 'obj_stove',
    catalogId: 'appl_stove_cheap',
    name: 'Dial-A-Flame Gas Stove',
    x: 4,
    y: 9,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'appliances',
    price: 400,
    interactions: ['Cook Meal (§10)', 'Clean Stove'],
    color: '#e2e8f0',
    icon: 'Flame',
  },
  // Dining
  {
    id: 'obj_table',
    catalogId: 'surf_dining_table',
    name: 'Pinecone Dining Table',
    x: 7,
    y: 9,
    width: 2,
    height: 2,
    rotation: 0,
    category: 'surfaces',
    price: 220,
    interactions: ['Eat Here', 'Clean Table'],
    color: '#b45309',
    icon: 'Table',
  },
  {
    id: 'obj_chair1',
    catalogId: 'seat_chair_cheap',
    name: 'Contempto Casual Chair',
    x: 6,
    y: 9,
    width: 1,
    height: 1,
    rotation: 90,
    category: 'seating',
    price: 80,
    interactions: ['Sit Down', 'Relax'],
    color: '#3b82f6',
    icon: 'Armchair',
  },
  {
    id: 'obj_chair2',
    catalogId: 'seat_chair_cheap',
    name: 'Contempto Casual Chair',
    x: 9,
    y: 9,
    width: 1,
    height: 1,
    rotation: 270,
    category: 'seating',
    price: 80,
    interactions: ['Sit Down', 'Relax'],
    color: '#3b82f6',
    icon: 'Armchair',
  },
  // Bathroom
  {
    id: 'obj_toilet',
    catalogId: 'plum_toilet',
    name: 'Porcelain Throne Toilet',
    x: 13,
    y: 9,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'plumbing',
    price: 280,
    interactions: ['Use Toilet', 'Clean Toilet'],
    color: '#f8fafc',
    icon: 'Bath',
  },
  {
    id: 'obj_shower',
    catalogId: 'plum_shower',
    name: 'PowerSpray Stall Shower',
    x: 14,
    y: 9,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'plumbing',
    price: 520,
    interactions: ['Take Shower', 'Clean Shower'],
    color: '#06b6d4',
    icon: 'Droplet',
  },
  {
    id: 'obj_sink',
    catalogId: 'plum_sink',
    name: 'Oval Vanity Sink',
    x: 12,
    y: 9,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'plumbing',
    price: 150,
    interactions: ['Wash Hands', 'Brush Teeth'],
    color: '#e2e8f0',
    icon: 'Pipette',
  },
  // Study & Deco
  {
    id: 'obj_bookshelf',
    catalogId: 'misc_bookshelf',
    name: 'Scholarly Hardwood Bookcase',
    x: 12,
    y: 4,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'misc',
    price: 420,
    interactions: ['Study Cooking', 'Study Mechanical', 'Read Mystery Novel'],
    color: '#854d0e',
    icon: 'BookOpen',
  },
  {
    id: 'obj_plant',
    catalogId: 'deco_plant',
    name: 'Potted Ficus Elastica',
    x: 11,
    y: 5,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'decorative',
    price: 60,
    interactions: ['Water Plant', 'Admire'],
    color: '#16a34a',
    icon: 'Flower2',
  },
  {
    id: 'obj_phone',
    catalogId: 'elec_phone',
    name: 'Rotary Wall Telephone',
    x: 5,
    y: 7,
    width: 1,
    height: 1,
    rotation: 0,
    category: 'electronics',
    price: 75,
    interactions: ['Call Neighbor', 'Order Pizza (§40)', 'Call Repairman (§50)'],
    color: '#e2e8f0',
    icon: 'Phone',
  },
];

export function getStoredSettings(): SimitoneAndroidSettings {
  try {
    const item = localStorage.getItem('simitone_setup');
    if (item) return JSON.parse(item);
  } catch {
    // ignore
  }
  return INITIAL_SETTINGS;
}

export function saveStoredSettings(settings: SimitoneAndroidSettings) {
  try {
    localStorage.setItem('simitone_setup', JSON.stringify(settings));
  } catch {
    // ignore
  }
}

export function getStoredProfiles(): Record<string, TouchControlProfile> {
  try {
    const item = localStorage.getItem('simitone_touch_profiles');
    if (item) return JSON.parse(item);
  } catch {
    // ignore
  }
  const def = createDefaultProfile();
  const oneHanded = createOneHandedProfile();
  const tablet = createTabletProfile();
  const map: Record<string, TouchControlProfile> = {
    [def.name]: def,
    [oneHanded.name]: oneHanded,
    [tablet.name]: tablet,
  };
  return map;
}

export function saveStoredProfiles(profiles: Record<string, TouchControlProfile>) {
  try {
    localStorage.setItem('simitone_touch_profiles', JSON.stringify(profiles));
  } catch {
    // ignore
  }
}
