using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    class ResearchMod_MoodBoost : ResearchMod
    {
        // the research project this mod has been attached to
        public ResearchProjectDef def;
        // heureka chances between 100f = 100% and 0f = 0%
        public float heurekaChance;
        public int stage = 0;

        public override void Apply()
        {
            Log.Debug("ResearchMod_HeurekaCandidate.Apply() has been called");
            if (def == null)
            {
                Log.Error("def is null for ResearchMod_HeurekaCandidate! The Research project has not been found!");
                return;
            }
            var validPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction;
            //var validPawns = Find.WorldPawns.AllPawnsAlive.Where(pawn => { return pawn.Faction == Faction.OfPlayer && pawn.needs.mood != null; });
            Log.Debug("total of " + Find.WorldPawns.AllPawnsAlive.Count + " pawns found of which " + validPawns.Count() + " are valid: " + String.Join(", ", validPawns.Select(pawn => pawn.Name)));
            foreach (var pawn in validPawns)
            {
                if (pawn.needs != null && pawn.needs.mood != null)
                {
                    if (Rand.Range(0, 100) <= heurekaChance)
                    {
                        Log.Debug("Dice won for applying '" + DefOfs_RepeatableResearch.MoodBoost.defName + "' thought to pawn '" + pawn.Name + "'");
                        pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_RepeatableResearch.MoodBoost, stage));
                    }
                    else
                    {
                        Log.Debug("Dice loss for applying '" + DefOfs_RepeatableResearch.MoodBoost.defName + "' thought to pawn '" + pawn.Name + "'");
                    }
                } 
                else
                {
                    Log.Debug("Cannot apply '" + DefOfs_RepeatableResearch.MoodBoost.defName + "' thought to pawn '" + pawn.Name + "', since he has no mood");
                }
            }
        }
    }
}
