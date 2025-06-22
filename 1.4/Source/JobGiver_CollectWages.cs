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
                if (collectibles == null)
                {
                    var silverDef = ThingDefOf.Silver;
                    collectibles = new ThingFilter();
                    collectibles.SetAllow(silverDef, true);
                }
                return collectibles;
            }
        }


        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.DebugOnce("at least JobGiver_CollectWages.TryGiveJob() is getting called...");
            // check whether the pawn wants to collect wages now
            if (pawn.records != null)
            {
                var owedWages = pawn.records.GetValue(DefOfs_Wages.TotalWagesOwed);
                var collectedWages = pawn.records.GetValue(DefOfs_Wages.TotalWagesCollected);
                Job collectPaymentJob = null;
                if (owedWages - collectedWages > ModSettings_Wages.owedWageIgnoredBeforeGathering)
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
                if (collectPaymentJob == null && (owedWages - collectedWages) > ModSettings_Wages.owedWageIgnoredBeforeMoodlet)
                {
                    Log.Debug("pawn " + pawn + " will can't collect pay and is getting mad");
                    pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_Wages.NotGettingPaid));
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
