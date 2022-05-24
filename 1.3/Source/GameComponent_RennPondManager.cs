using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    public class RennPondSettings
    {
        public int moodEffect = 0;
        public int tickLastReported = Find.TickManager.TicksGame;
        public int threatCap = int.MaxValue;
        public float threatMultiplier = 1.0f;
    }

    public class GameComponent_RennPondManager : GameComponent
    {
        // saved settings
        Dictionary<Thing, RennPondSettings> knownThreatSettings = new Dictionary<Thing, RennPondSettings>();

        public GameComponent_RennPondManager(Game game)
        {
            Log.Debug("GameComponent_RennPondManager created");
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                applyMoodEffect(GetCurrentSettings().moodEffect);
            }
        }

        private void applyMoodEffect(int moodEffect)
        {
            var validPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_OfPlayerFaction; //todo: limit to same map as pond .Where(pawn => { return pawn.Map ==  });
            //var validPawns = Find.WorldPawns.AllPawnsAlive.Where(pawn => { return pawn.Faction == Faction.OfPlayer && pawn.needs.mood != null; });
            Log.Debug("total of " + Find.WorldPawns.AllPawnsAlive.Count + " pawns found of which " + validPawns.Count() + " are valid: " + String.Join(", ", validPawns.Select(pawn => pawn.Name)));
            foreach (var pawn in validPawns)
            {
                if (pawn.needs != null && pawn.needs.mood != null)
                {
                    // todo: replace by new mood definition
                    pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_RepeatableResearch.MoodBoost, 2));
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref knownThreatSettings, "knownThreatSettings", LookMode.Reference, LookMode.Value);
            Log.Debug("GameComponent_RennPondManager.ExposeData(): " + String.Join(", ", knownThreatSettings.Select(setting => { return setting.Key.ToString() + ": threatCap=" + setting.Value.threatCap + "; threatMultiplier=" + setting.Value.threatMultiplier; })));
        }

        public RennPondSettings GetCurrentSettings()
        {
            Log.DebugOnce("GameComponent_RennPondManager.GetCurrentSettings() is getting called");
            shrinkOldSettings();
            if (knownThreatSettings.Count > 0)
            {
                var bestEntry = knownThreatSettings.MinBy(setting => { return setting.Value.threatMultiplier; });
                return bestEntry.Value;
            }
            return new RennPondSettings();
        }

        public void SetThreatPoints(Thing sender, RennPondSettings settings)
        {
            settings.tickLastReported = Find.TickManager.TicksGame;
            knownThreatSettings[sender] = settings;
        }

        public void RemoveTrackedThing(Thing thing)
        {
            Log.Debug("GameComponent_RennPondManager.RemoveTrackedThing() of " + thing);
            if (knownThreatSettings.ContainsKey(thing))
            {
                knownThreatSettings.Remove(thing);
            }
        }

        private void shrinkOldSettings()
        {
            var oldestValidTick = Find.TickManager.TicksGame - GenTicks.TickLongInterval;
            var outdatedEntries = knownThreatSettings.Where(setting => { return setting.Value.tickLastReported < oldestValidTick; });
            foreach (var entry in outdatedEntries)
            {
                Log.Warning("An object seems to have gotten lost: " + entry.Key);
                knownThreatSettings.Remove(entry.Key);
            }
        }
    }
}
