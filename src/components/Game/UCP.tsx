import React, { useState } from 'react';
import { SimPerson, GameMode, CatalogItem, CatalogCategory } from '../../types';
import { CATALOG_ITEMS } from '../../services/catalog';
import { sounds } from '../../audio/sound';
import {
  Heart,
  Briefcase,
  Sparkles,
  Users,
  Package,
  ShoppingBag,
  Hammer,
  DollarSign,
  ChevronUp,
  ChevronDown,
  Armchair,
  Table,
  Flame,
  Tv,
  Bath,
  Flower2,
  Sun,
  BookOpen,
} from 'lucide-react';

interface UCPProps {
  sim: SimPerson;
  gameMode: GameMode;
  familyFunds: number;
  onSelectGameMode: (mode: GameMode) => void;
  onSelectCatalogItem: (item: CatalogItem) => void;
  selectedCatalogItem: CatalogItem | null;
  onOpenCAS: () => void;
}

type LiveTab = 'motives' | 'job' | 'skills' | 'relationships' | 'inventory';

export const UCP: React.FC<UCPProps> = ({
  sim,
  gameMode,
  familyFunds,
  onSelectGameMode,
  onSelectCatalogItem,
  selectedCatalogItem,
  onOpenCAS,
}) => {
  const [isExpanded, setIsExpanded] = useState(true);
  const [activeLiveTab, setActiveLiveTab] = useState<LiveTab>('motives');
  const [selectedCategory, setSelectedCategory] = useState<CatalogCategory>('seating');

  const filteredCatalog = CATALOG_ITEMS.filter((item) => item.category === selectedCategory);

  const getMotiveColor = (val: number) => {
    if (val >= 60) return 'bg-emerald-500';
    if (val >= 30) return 'bg-amber-400';
    return 'bg-rose-500';
  };

  const categories: { id: CatalogCategory; label: string; icon: React.ReactNode }[] = [
    { id: 'seating', label: 'Seating', icon: <Armchair className="w-4 h-4" /> },
    { id: 'surfaces', label: 'Surfaces', icon: <Table className="w-4 h-4" /> },
    { id: 'appliances', label: 'Appliances', icon: <Flame className="w-4 h-4" /> },
    { id: 'electronics', label: 'Electronics', icon: <Tv className="w-4 h-4" /> },
    { id: 'plumbing', label: 'Plumbing', icon: <Bath className="w-4 h-4" /> },
    { id: 'decorative', label: 'Deco', icon: <Flower2 className="w-4 h-4" /> },
    { id: 'lighting', label: 'Lighting', icon: <Sun className="w-4 h-4" /> },
    { id: 'misc', label: 'Misc', icon: <BookOpen className="w-4 h-4" /> },
  ];

  return (
    <div className="absolute bottom-0 inset-x-0 z-40 flex flex-col items-center pointer-events-auto select-none font-sans">
      {/* Top Toggle Handle */}
      <button
        onClick={() => {
          sounds.playClick();
          setIsExpanded(!isExpanded);
        }}
        className="px-4 py-1 bg-slate-900/90 hover:bg-slate-800 text-slate-300 border-t border-x border-slate-700/80 rounded-t-xl text-xs font-semibold flex items-center gap-1.5 backdrop-blur-md shadow-lg"
      >
        {isExpanded ? <ChevronDown className="w-3.5 h-3.5" /> : <ChevronUp className="w-3.5 h-3.5" />}
        <span>{gameMode.toUpperCase()} PANEL</span>
      </button>

      {isExpanded && (
        <div className="w-full max-w-5xl bg-gradient-to-b from-slate-900/95 to-slate-950/98 border-t border-slate-700/80 shadow-[0_-5px_25px_rgba(0,0,0,0.6)] backdrop-blur-xl p-3 sm:p-4 text-white">
          <div className="flex flex-col sm:flex-row items-stretch gap-3">
            {/* Sim Info & Mode Switcher Column */}
            <div className="flex sm:flex-col items-center justify-between sm:justify-start gap-2 sm:w-44 border-b sm:border-b-0 sm:border-r border-slate-700/60 pb-2 sm:pb-0 sm:pr-3 shrink-0">
              {/* Sim Card */}
              <div
                onClick={() => { sounds.playClick(); onOpenCAS(); }}
                className="flex items-center gap-2 p-1.5 bg-slate-800/80 hover:bg-slate-700/80 border border-slate-600/50 rounded-xl cursor-pointer w-full transition"
                title="Click to open CAS"
              >
                <div className="w-10 h-10 rounded-full bg-blue-600/40 border border-blue-400 flex items-center justify-center shrink-0">
                  <img src="/plumbob.svg" alt="Plumbob" className="w-5 h-6 animate-pulse" />
                </div>
                <div className="overflow-hidden">
                  <div className="text-xs font-bold text-white truncate">
                    {sim.firstName} {sim.lastName}
                  </div>
                  <div className="text-[10px] text-emerald-400 font-medium">
                    §{familyFunds.toLocaleString()}
                  </div>
                </div>
              </div>

              {/* Mode Buttons */}
              <div className="grid grid-cols-3 sm:grid-cols-1 gap-1.5 w-full">
                <button
                  onClick={() => { sounds.playClick(); onSelectGameMode('live'); }}
                  className={`py-1.5 px-2.5 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition ${
                    gameMode === 'live'
                      ? 'bg-blue-600 text-white shadow-[0_0_12px_rgba(59,130,246,0.5)]'
                      : 'bg-slate-800/80 hover:bg-slate-700 text-slate-300'
                  }`}
                >
                  <img src="/icons/mode_live.png" alt="Live" className="w-3.5 h-3.5" />
                  Live
                </button>
                <button
                  onClick={() => { sounds.playClick(); onSelectGameMode('buy'); }}
                  className={`py-1.5 px-2.5 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition ${
                    gameMode === 'buy'
                      ? 'bg-blue-600 text-white shadow-[0_0_12px_rgba(59,130,246,0.5)]'
                      : 'bg-slate-800/80 hover:bg-slate-700 text-slate-300'
                  }`}
                >
                  <img src="/icons/mode_buy.png" alt="Buy" className="w-3.5 h-3.5" />
                  Buy
                </button>
                <button
                  onClick={() => { sounds.playClick(); onSelectGameMode('build'); }}
                  className={`py-1.5 px-2.5 rounded-lg text-xs font-bold flex items-center justify-center gap-1.5 transition ${
                    gameMode === 'build'
                      ? 'bg-blue-600 text-white shadow-[0_0_12px_rgba(59,130,246,0.5)]'
                      : 'bg-slate-800/80 hover:bg-slate-700 text-slate-300'
                  }`}
                >
                  <img src="/icons/mode_build.png" alt="Build" className="w-3.5 h-3.5" />
                  Build
                </button>
              </div>
            </div>

            {/* Mode-Specific Subpanel */}
            <div className="flex-1 min-w-0">
              {/* LIVE MODE */}
              {gameMode === 'live' && (
                <div className="space-y-2">
                  {/* Tabs */}
                  <div className="flex gap-1 border-b border-slate-700/60 pb-1.5 overflow-x-auto">
                    {[
                      { id: 'motives', label: 'Motives', icon: <Heart className="w-3.5 h-3.5" /> },
                      { id: 'job', label: 'Career', icon: <Briefcase className="w-3.5 h-3.5" /> },
                      { id: 'skills', label: 'Skills', icon: <Sparkles className="w-3.5 h-3.5" /> },
                      { id: 'relationships', label: 'Friends', icon: <Users className="w-3.5 h-3.5" /> },
                      { id: 'inventory', label: 'Pocket', icon: <Package className="w-3.5 h-3.5" /> },
                    ].map((tab) => (
                      <button
                        key={tab.id}
                        onClick={() => { sounds.playClick(); setActiveLiveTab(tab.id as LiveTab); }}
                        className={`px-3 py-1 rounded-lg text-xs font-semibold flex items-center gap-1.5 shrink-0 transition ${
                          activeLiveTab === tab.id
                            ? 'bg-blue-600/30 text-blue-300 border border-blue-500/50'
                            : 'text-slate-400 hover:text-white hover:bg-slate-800'
                        }`}
                      >
                        {tab.icon}
                        {tab.label}
                      </button>
                    ))}
                  </div>

                  {/* Motives Tab */}
                  {activeLiveTab === 'motives' && (
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-2 pt-1">
                      {Object.entries(sim.motives).map(([motive, val]) => (
                        <div key={motive} className="bg-slate-950/60 p-2 rounded-xl border border-slate-800">
                          <div className="flex justify-between text-[11px] font-semibold text-slate-300 capitalize mb-1">
                            <span>{motive}</span>
                            <span>{Math.round(val)}%</span>
                          </div>
                          <div className="w-full h-2 bg-slate-800 rounded-full overflow-hidden">
                            <div
                              className={`h-full rounded-full transition-all duration-300 ${getMotiveColor(val)}`}
                              style={{ width: `${Math.max(5, Math.min(100, val))}%` }}
                            />
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Job Tab */}
                  {activeLiveTab === 'job' && (
                    <div className="p-3 bg-slate-950/60 rounded-xl border border-slate-800 grid grid-cols-1 sm:grid-cols-3 gap-3">
                      <div>
                        <div className="text-[11px] text-slate-400">Position & Track</div>
                        <div className="text-sm font-bold text-white">{sim.job.title}</div>
                        <div className="text-xs text-blue-400 font-medium">{sim.job.careerTrack} (Level {sim.job.level})</div>
                      </div>
                      <div>
                        <div className="text-[11px] text-slate-400">Salary & Hours</div>
                        <div className="text-xs font-bold text-emerald-400">§{sim.job.salary} / day</div>
                        <div className="text-xs text-slate-300">{sim.job.hours}</div>
                      </div>
                      <div>
                        <div className="text-[11px] text-slate-400">Carpool Arrival</div>
                        <div className="text-xs font-semibold text-amber-300">{sim.job.carpool}</div>
                        <div className="text-[11px] text-slate-400 mt-0.5">Family friends needed: 2</div>
                      </div>
                    </div>
                  )}

                  {/* Skills Tab */}
                  {activeLiveTab === 'skills' && (
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
                      {Object.entries(sim.skills).map(([skill, level]) => (
                        <div key={skill} className="bg-slate-950/60 p-2 rounded-xl border border-slate-800 flex items-center justify-between">
                          <span className="text-xs font-semibold text-slate-300 capitalize">{skill}</span>
                          <div className="flex gap-0.5">
                            {Array.from({ length: 10 }).map((_, i) => (
                              <div
                                key={i}
                                className={`w-1.5 h-3 rounded-sm ${
                                  i < level ? 'bg-cyan-400 shadow-[0_0_6px_rgba(34,211,238,0.6)]' : 'bg-slate-800'
                                }`}
                              />
                            ))}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Relationships Tab */}
                  {activeLiveTab === 'relationships' && (
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-2">
                      {Object.entries(sim.relationships).map(([name, scores]) => (
                        <div key={name} className="bg-slate-950/60 p-2.5 rounded-xl border border-slate-800 flex items-center justify-between">
                          <div>
                            <div className="text-xs font-bold text-white">{name}</div>
                            <div className="text-[11px] text-slate-400">
                              Daily: <span className={scores.daily >= 50 ? 'text-emerald-400' : 'text-slate-300'}>{scores.daily}</span> | Lifetime: {scores.lifetime}
                            </div>
                          </div>
                          <Heart className={`w-4 h-4 ${scores.daily >= 60 ? 'text-rose-500 fill-current' : 'text-slate-600'}`} />
                        </div>
                      ))}
                    </div>
                  )}

                  {/* Inventory Tab */}
                  {activeLiveTab === 'inventory' && (
                    <div className="flex gap-2 overflow-x-auto py-1">
                      {sim.inventory.map((item, i) => (
                        <div key={i} className="px-3 py-2 bg-slate-950/60 rounded-xl border border-slate-800 flex items-center gap-2 text-xs font-semibold text-slate-200">
                          <Package className="w-4 h-4 text-amber-400" />
                          {item}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {/* BUY MODE */}
              {gameMode === 'buy' && (
                <div className="space-y-2">
                  {/* Category Buttons */}
                  <div className="flex gap-1 overflow-x-auto pb-1 border-b border-slate-700/60">
                    {categories.map((cat) => (
                      <button
                        key={cat.id}
                        onClick={() => { sounds.playClick(); setSelectedCategory(cat.id); }}
                        className={`px-2.5 py-1 rounded-lg text-xs font-semibold flex items-center gap-1.5 shrink-0 transition ${
                          selectedCategory === cat.id
                            ? 'bg-amber-600/30 text-amber-300 border border-amber-500/50'
                            : 'text-slate-400 hover:text-white hover:bg-slate-800'
                        }`}
                      >
                        {cat.icon}
                        {cat.label}
                      </button>
                    ))}
                  </div>

                  {/* Catalog items scrollable row */}
                  <div className="flex gap-2.5 overflow-x-auto py-1 max-h-36">
                    {filteredCatalog.map((item) => (
                      <div
                        key={item.id}
                        onClick={() => {
                          sounds.playClick();
                          onSelectCatalogItem(item);
                        }}
                        className={`w-36 p-2 rounded-xl border cursor-pointer shrink-0 transition flex flex-col justify-between ${
                          selectedCatalogItem?.id === item.id
                            ? 'bg-amber-950/50 border-amber-400 ring-2 ring-amber-400/50'
                            : 'bg-slate-950/60 border-slate-800 hover:border-slate-600'
                        }`}
                      >
                        <div>
                          <div className="flex items-center justify-between mb-1">
                            <span className="text-[10px] font-bold text-amber-400">§{item.price}</span>
                            <span className="text-[10px] text-slate-400">{item.width}x{item.height}</span>
                          </div>
                          <div className="text-xs font-bold text-white leading-tight truncate">{item.name}</div>
                          <p className="text-[10px] text-slate-400 line-clamp-2 mt-0.5">{item.description}</p>
                        </div>
                        <button className="w-full mt-2 py-1 bg-amber-600 hover:bg-amber-500 rounded-lg text-[10px] font-bold text-white shadow">
                          {selectedCatalogItem?.id === item.id ? 'Tap lot to place' : 'Buy §' + item.price}
                        </button>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* BUILD MODE */}
              {gameMode === 'build' && (
                <div className="p-3 bg-slate-950/60 rounded-xl border border-slate-800 grid grid-cols-2 sm:grid-cols-4 gap-2">
                  {['Drywall Wall Tool', 'Parquet Wood Flooring', 'Checkered Kitchen Tiles', 'Oak Archway Door', 'Bay Window', 'Staircase', 'Mansard Roof Style', 'Terrain Grass'].map((tool, i) => (
                    <button
                      key={i}
                      onClick={() => sounds.playClick()}
                      className="p-2.5 rounded-xl bg-slate-900 hover:bg-slate-800 border border-slate-700 text-xs font-semibold text-slate-200 flex items-center gap-2"
                    >
                      <Hammer className="w-4 h-4 text-cyan-400" />
                      {tool}
                    </button>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
