using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace DanielRenner.Wages
{
    public class JobGiver_CollectWages : ThinkNode_JobGiver
    {

        private static ThingFilter collectibles;
        private static ThingFilter Collectibles
        {
            get
            {
                if (collectibles == null || Find.TickManager.TicksGame % 2500 == 0) // once an ingame hour we refresh what currency should be collected
                {
                    var currencyDef = ModSettings_Wages.currencyDef;
                    collectibles = new ThingFilter();
                    collectibles.SetAllow(currencyDef, true);
                }
                return collectibles;
            }
        }


        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.DebugOnce("at least JobGiver_CollectWages.TryGiveJob() is getting called...");
            // check whether the pawn wants to collect wages now
            if (pawn.IsHashIntervalTick(60) && pawn.records != null) // once a minute
            {
                var owedWages = pawn.records.GetValue(DefOfs_Wages.TotalWagesOwed);
                var collectedWages = pawn.records.GetValue(DefOfs_Wages.TotalWagesCollected);
                Job collectPaymentJob = null;
                if (owedWages - collectedWages > ModSettings_Wages.owedWageIgnoredBeforeGathering) // once a minute
                {
                    Log.Debug("pawn " + pawn + " wants to collect his wages");
                    // collect wages!
                    var nextPayment = FindBestPayment(pawn);
                    if (nextPayment != null)
                    {
                        Log.Debug("pawn " + pawn + " will collect " + nextPayment);
                        collectPaymentJob = JobMaker.MakeJob(DefOfs_Wages.CollectWages, nextPayment);
                    }
                }
                if (pawn.IsHashIntervalTick(2400) && collectPaymentJob == null && (owedWages - collectedWages) > ModSettings_Wages.owedWageIgnoredBeforeMoodlet) // around once an hour
                {
                    float breakChancePerRoughlyAnHour = ModSettings_Wages.breakChancePerHourPercent / 100f;
                    Log.Debug("pawn " + pawn + " can't collect pay and is getting mad");
                    pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_Wages.NotGettingPaid));
                    // make the pawn break rarely
                    if (Rand.Chance(breakChancePerRoughlyAnHour))
                    {
                        Log.Debug("pawn " + pawn + " is getting mental for not receiving their pay!");
                        bool snapped = pawn.mindState.mentalStateHandler.TryStartMentalState(
                            DefOfs_Wages.Wages_WageTheft,
                            "Unpaid wages",
                            forceWake: true,
                            causedByMood: false
                        );

                        if (snapped)
                        {
                            // Return null immediately; the mental state takes over their brain
                            return null;
                        }
                    }
                }
                return collectPaymentJob;
            }
            return null;
        }

        private static Thing FindBestPayment(Pawn pawn)
        {
            Predicate<Thing> validator = delegate (Thing x)
            {
                if (!x.IsForbidden(pawn) && pawn.CanReserve(x))
                {
                    return true;
                }
                return false;
            };
            return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, Collectibles.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn), 9999f, validator);
        }

    }
}
