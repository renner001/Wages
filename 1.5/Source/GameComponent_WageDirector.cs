using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.Wages
{
    public class GameComponent_WageDirector : GameComponent
    {
        // saved variables
        int lastTickWagesApplied = 0;

        // temporary variables
        Game loadedGame;

        public GameComponent_WageDirector(Game game)
        {
            Log.Debug("GameComponent_WageDirector created");
            loadedGame = game;
        }


        public override void GameComponentTick()
        {
            base.GameComponentTick();

            
            if (Find.TickManager.TicksGame % 300000 == 0) // 2500 -> one in-game hour -> every 5 days being 2500*24*5 = 300.000 /*GenTicks.TickRareInterval*/
            {
                // update the moodCache
                var validPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
                PayHelperUtility.CalcWageSteps();
                PayHelperUtility.FixWrongWages();
                var totalWagesOwed = 0;
                foreach (Pawn pawn in validPawns)
                {
                    var wageOfPawn = (int)pawn.records.GetValue(DefOfs_Wages.CurrentWage);
                    Log.Debug("Incrementing owed silver for pawn " + pawn + "by " + wageOfPawn);
                    totalWagesOwed += wageOfPawn;
                    pawn.records.AddTo(DefOfs_Wages.TotalWagesOwed, wageOfPawn);
                    // apply the wage mood effect
                    pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_Wages.WageLevelEffect, PayHelperUtility.CalcMoodStage(pawn, wageOfPawn)));
                }
                if (totalWagesOwed > 0)
                {
                    Messages.Message("Payday! " + totalWagesOwed + " silver will be collected by your colonists. Make sure, you have enough on hand.", MessageTypeDefOf.CautionInput);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            try
            {
                Scribe_Values.Look(ref lastTickWagesApplied, "lastTickWagesApplied", 0);
                //Scribe_Collections.Look(ref knownThreatSettings, "knownThreatSettings", LookMode.Reference, LookMode.Deep, ref tempKeyListThreatSettings, ref tempValueListThreatSettings);
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to load settings of GameComponent_RennPondManager. This is an error the game will recover from within the next seconds. Details: " + ex);
            }
            //Log.Debug("GameComponent_WageDirector.ExposeData(): " + String.Join(", ", knownThreatSettings.Select(setting => { return setting.Key.ToString() + ": threatCap=" + setting.Value.threatCap + "; threatMultiplier=" + setting.Value.threatMultiplier; })));
        }
    }
}
