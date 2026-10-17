import React, { useState } from 'react';
import { SimPerson, Personality, ZodiacSign } from '../../types';
import { sounds } from '../../audio/sound';
import { Sparkles, ArrowLeft, Check, Dice5, User } from 'lucide-react';

interface CreateASimProps {
  initialSim: SimPerson;
  onSaveSim: (sim: SimPerson) => void;
  onCancel: () => void;
}

export const CreateASim: React.FC<CreateASimProps> = ({
  initialSim,
  onSaveSim,
  onCancel,
}) => {
  const [firstName, setFirstName] = useState(initialSim.firstName);
  const [lastName, setLastName] = useState(initialSim.lastName);
  const [gender, setGender] = useState<'male' | 'female'>(initialSim.gender);
  const [age, setAge] = useState<'adult' | 'child'>(initialSim.age);
  const [skinTone, setSkinTone] = useState<'light' | 'medium' | 'dark'>(initialSim.skinTone);
  const [headIndex, setHeadIndex] = useState(initialSim.headIndex);
  const [bodyIndex, setBodyIndex] = useState(initialSim.bodyIndex);
  const [bio, setBio] = useState(initialSim.bio);

  const [personality, setPersonality] = useState<Personality>({ ...initialSim.personality });

  const totalPointsUsed = Object.values(personality).reduce((a, b) => a + b, 0);
  const pointsRemaining = 25 - totalPointsUsed;

  const calculateZodiac = (p: Personality): ZodiacSign => {
    // Classic Sims 1 zodiac derivation algorithm based on dominant personality traits
    if (p.active >= 8 && p.outgoing >= 7) return 'Aries';
    if (p.neat >= 8 && p.nice >= 6) return 'Taurus';
    if (p.outgoing >= 8 && p.playful >= 7) return 'Gemini';
    if (p.nice >= 8 && p.neat >= 6) return 'Cancer';
    if (p.outgoing >= 9 && p.active >= 7) return 'Leo';
    if (p.neat >= 9) return 'Virgo';
    if (p.nice >= 7 && p.outgoing >= 6) return 'Libra';
    if (p.active >= 7 && p.playful <= 4) return 'Scorpio';
    if (p.playful >= 8 && p.active >= 6) return 'Sagittarius';
    if (p.neat >= 7 && p.nice <= 4) return 'Capricorn';
    if (p.outgoing >= 6 && p.playful >= 6) return 'Aquarius';
    return 'Pisces';
  };

  const currentZodiac = calculateZodiac(personality);

  const handleAdjustTrait = (trait: keyof Personality, delta: number) => {
    sounds.playClick();
    const current = personality[trait];
    const next = current + delta;
    if (next < 0 || next > 10) return;
    if (delta > 0 && pointsRemaining <= 0) return;

    setPersonality((prev) => ({
      ...prev,
      [trait]: next,
    }));
  };

  const handleRandomize = () => {
    sounds.playSimGibberish();
    const firstNames = gender === 'male' ? ['Bob', 'Mortimer', 'Michael', 'Chris', 'Gunther', 'Dirk'] : ['Bella', 'Betty', 'Cassandra', 'Dina', 'Nina', 'Kaylynn'];
    setFirstName(firstNames[Math.floor(Math.random() * firstNames.length)]);
    setHeadIndex(Math.floor(Math.random() * 5) + 1);
    setBodyIndex(Math.floor(Math.random() * 5) + 1);

    // Randomize 25 points
    let pool = 25;
    const traits: (keyof Personality)[] = ['neat', 'outgoing', 'active', 'playful', 'nice'];
    const newP: Personality = { neat: 0, outgoing: 0, active: 0, playful: 0, nice: 0 };
    traits.forEach((t, i) => {
      if (i === 4) {
        newP[t] = Math.min(10, pool);
      } else {
        const val = Math.min(10, Math.floor(Math.random() * (pool / (4 - i) + 3)));
        newP[t] = val;
        pool -= val;
      }
    });
    setPersonality(newP);
  };

  const handleSave = () => {
    sounds.playLevelUp();
    const updatedSim: SimPerson = {
      ...initialSim,
      firstName: firstName.trim() || 'Sim',
      lastName: lastName.trim() || 'Family',
      gender,
      age,
      skinTone,
      headIndex,
      bodyIndex,
      bio,
      zodiac: currentZodiac,
      personality,
    };
    onSaveSim(updatedSim);
  };

  return (
    <div className="w-full h-full bg-gradient-to-br from-slate-950 via-[#13203c] to-slate-950 text-slate-100 overflow-y-auto p-4 sm:p-6 font-sans">
      <div className="max-w-4xl mx-auto space-y-6">
        {/* Top Header */}
        <div className="flex items-center justify-between border-b border-slate-700/60 pb-4">
          <div className="flex items-center gap-3">
            <button
              onClick={() => { sounds.playClick(); onCancel(); }}
              className="p-2 hover:bg-slate-800 rounded-xl text-slate-300 hover:text-white transition"
            >
              <ArrowLeft className="w-5 h-5" />
            </button>
            <div>
              <h1 className="text-2xl font-bold tracking-tight text-white flex items-center gap-2">
                Create-A-Sim Studio
                <span className="text-xs px-2 py-0.5 bg-blue-600/30 text-blue-400 border border-blue-500/40 rounded-full font-semibold">
                  TS1 CAS
                </span>
              </h1>
              <p className="text-xs text-slate-400">Design household identity, appearance & personality</p>
            </div>
          </div>

          <div className="flex gap-2">
            <button
              onClick={handleRandomize}
              className="px-3 py-2 bg-slate-800 hover:bg-slate-700 border border-slate-600 rounded-xl text-xs font-semibold flex items-center gap-1.5 transition text-slate-200"
            >
              <Dice5 className="w-4 h-4 text-amber-400" />
              Randomize
            </button>
            <button
              onClick={handleSave}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-500 border border-blue-400 rounded-xl text-xs font-bold flex items-center gap-1.5 transition text-white shadow-[0_0_15px_rgba(59,130,246,0.5)]"
            >
              <Check className="w-4 h-4" />
              Save Sim
            </button>
          </div>
        </div>

        {/* Main 2-column layout */}
        <div className="grid grid-cols-1 md:grid-cols-12 gap-6">
          {/* Left Preview Pedestal (5 cols) */}
          <div className="md:col-span-5 bg-slate-900/50 border border-slate-700/60 rounded-3xl p-6 flex flex-col items-center justify-between relative shadow-xl backdrop-blur-md">
            {/* Rotating Plumbob */}
            <div className="flex flex-col items-center">
              <img
                src="/plumbob.svg"
                alt="Plumbob"
                className="w-8 h-12 drop-shadow-[0_0_20px_rgba(34,197,94,0.7)] animate-bounce"
              />
              <span className="text-xs font-bold text-cyan-400 mt-1 uppercase tracking-wider">
                {currentZodiac}
              </span>
            </div>

            {/* Sim Model Representation */}
            <div className="my-8 flex flex-col items-center">
              {/* Head */}
              <div
                className={`w-20 h-20 rounded-full border-2 border-white/20 shadow-xl flex items-center justify-center relative overflow-hidden ${
                  skinTone === 'light' ? 'bg-[#fed7aa]' : skinTone === 'medium' ? 'bg-[#d97706]' : 'bg-[#78350f]'
                }`}
              >
                {/* Hair */}
                <div className="absolute top-0 inset-x-0 h-8 bg-amber-950 rounded-b-lg" />
                <div className="w-3 h-3 rounded-full bg-slate-800 absolute top-9 left-4" />
                <div className="w-3 h-3 rounded-full bg-slate-800 absolute top-9 right-4" />
                <div className="w-5 h-1.5 rounded-full bg-rose-700 absolute bottom-4" />
              </div>

              {/* Torso */}
              <div
                className={`w-28 h-32 mt-1 rounded-2xl border border-white/20 shadow-lg flex items-center justify-center relative ${
                  gender === 'male' ? 'bg-blue-600' : 'bg-rose-500'
                }`}
              >
                <span className="text-[10px] uppercase font-bold text-white/80 tracking-widest">
                  Style #{bodyIndex}
                </span>
              </div>

              {/* Pedestal */}
              <div className="w-40 h-8 rounded-full bg-gradient-to-r from-slate-800 via-slate-700 to-slate-800 border border-slate-600/60 shadow-2xl mt-4" />
            </div>

            {/* Sim Quick Bio Summary */}
            <div className="w-full text-center bg-slate-950/60 p-3 rounded-2xl border border-slate-800">
              <div className="text-sm font-bold text-white">
                {firstName || 'First'} {lastName || 'Last'}
              </div>
              <div className="text-xs text-slate-400 capitalize">
                {age} {gender} • {skinTone} Skin
              </div>
            </div>
          </div>

          {/* Right Customization Controls (7 cols) */}
          <div className="md:col-span-7 space-y-4">
            {/* Identity Details */}
            <div className="bg-slate-900/50 border border-slate-700/60 rounded-2xl p-4 space-y-3">
              <span className="text-xs font-semibold text-blue-400 uppercase tracking-wider block">
                Identity & Appearance
              </span>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">First Name</label>
                  <input
                    type="text"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    className="w-full px-3 py-1.5 bg-slate-950 border border-slate-700 rounded-xl text-xs text-white"
                  />
                </div>
                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">Last Name</label>
                  <input
                    type="text"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    className="w-full px-3 py-1.5 bg-slate-950 border border-slate-700 rounded-xl text-xs text-white"
                  />
                </div>
              </div>

              <div className="grid grid-cols-3 gap-2 pt-1">
                {/* Gender */}
                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">Gender</label>
                  <div className="flex rounded-lg overflow-hidden border border-slate-700">
                    <button
                      onClick={() => { sounds.playClick(); setGender('male'); }}
                      className={`flex-1 py-1 text-xs font-semibold ${
                        gender === 'male' ? 'bg-blue-600 text-white' : 'bg-slate-900 text-slate-400'
                      }`}
                    >
                      Male
                    </button>
                    <button
                      onClick={() => { sounds.playClick(); setGender('female'); }}
                      className={`flex-1 py-1 text-xs font-semibold ${
                        gender === 'female' ? 'bg-rose-600 text-white' : 'bg-slate-900 text-slate-400'
                      }`}
                    >
                      Female
                    </button>
                  </div>
                </div>

                {/* Age */}
                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">Life Stage</label>
                  <div className="flex rounded-lg overflow-hidden border border-slate-700">
                    <button
                      onClick={() => { sounds.playClick(); setAge('adult'); }}
                      className={`flex-1 py-1 text-xs font-semibold ${
                        age === 'adult' ? 'bg-indigo-600 text-white' : 'bg-slate-900 text-slate-400'
                      }`}
                    >
                      Adult
                    </button>
                    <button
                      onClick={() => { sounds.playClick(); setAge('child'); }}
                      className={`flex-1 py-1 text-xs font-semibold ${
                        age === 'child' ? 'bg-indigo-600 text-white' : 'bg-slate-900 text-slate-400'
                      }`}
                    >
                      Child
                    </button>
                  </div>
                </div>

                {/* Skin Tone */}
                <div>
                  <label className="text-[11px] text-slate-400 block mb-1">Skin Tone</label>
                  <div className="flex gap-1.5 pt-0.5">
                    {[
                      { id: 'light', color: 'bg-[#fed7aa]' },
                      { id: 'medium', color: 'bg-[#d97706]' },
                      { id: 'dark', color: 'bg-[#78350f]' },
                    ].map((skin) => (
                      <button
                        key={skin.id}
                        onClick={() => { sounds.playClick(); setSkinTone(skin.id as 'light' | 'medium' | 'dark'); }}
                        className={`w-7 h-7 rounded-full border-2 ${skin.color} ${
                          skinTone === skin.id ? 'border-cyan-400 ring-2 ring-cyan-400/50' : 'border-slate-700'
                        }`}
                      />
                    ))}
                  </div>
                </div>
              </div>
            </div>

            {/* Personality Points Allocator (25 Points) */}
            <div className="bg-slate-900/50 border border-slate-700/60 rounded-2xl p-4 space-y-3">
              <div className="flex items-center justify-between border-b border-slate-800 pb-2">
                <span className="text-xs font-semibold text-blue-400 uppercase tracking-wider flex items-center gap-1.5">
                  <Sparkles className="w-3.5 h-3.5" />
                  Personality Allocation
                </span>
                <span className={`text-xs font-bold px-2 py-0.5 rounded-full ${
                  pointsRemaining === 0 ? 'bg-emerald-900/40 text-emerald-400' : 'bg-amber-900/40 text-amber-300'
                }`}>
                  {pointsRemaining} Points Left
                </span>
              </div>

              <div className="space-y-2">
                {(['neat', 'outgoing', 'active', 'playful', 'nice'] as (keyof Personality)[]).map((trait) => (
                  <div key={trait} className="flex items-center justify-between text-xs">
                    <span className="capitalize font-semibold text-slate-300 w-20">{trait}</span>
                    <div className="flex items-center gap-1.5 flex-1 max-w-[200px]">
                      <button
                        onClick={() => handleAdjustTrait(trait, -1)}
                        className="w-6 h-6 rounded bg-slate-800 hover:bg-slate-700 font-bold text-slate-300 flex items-center justify-center"
                      >
                        -
                      </button>
                      <div className="flex-1 flex gap-0.5 justify-center">
                        {Array.from({ length: 10 }).map((_, i) => (
                          <div
                            key={i}
                            className={`w-2 h-4 rounded-sm ${
                              i < personality[trait]
                                ? 'bg-cyan-400 shadow-[0_0_6px_rgba(34,211,238,0.5)]'
                                : 'bg-slate-800'
                            }`}
                          />
                        ))}
                      </div>
                      <button
                        onClick={() => handleAdjustTrait(trait, 1)}
                        className="w-6 h-6 rounded bg-slate-800 hover:bg-slate-700 font-bold text-slate-300 flex items-center justify-center"
                      >
                        +
                      </button>
                    </div>
                    <span className="text-xs font-mono font-bold text-white w-6 text-right">
                      {personality[trait]}
                    </span>
                  </div>
                ))}
              </div>
            </div>

            {/* Sim Bio */}
            <div className="bg-slate-900/50 border border-slate-700/60 rounded-2xl p-4 space-y-2">
              <label className="text-xs font-semibold text-blue-400 uppercase tracking-wider block">
                Sim Biography
              </label>
              <textarea
                value={bio}
                onChange={(e) => setBio(e.target.value)}
                rows={2}
                placeholder="Write your Sim's personal history, ambitions, or favorite pizza toppings..."
                className="w-full px-3 py-2 bg-slate-950 border border-slate-700 rounded-xl text-xs text-white focus:outline-none focus:border-blue-500"
              />
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
