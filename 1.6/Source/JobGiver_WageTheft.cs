using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;

namespace DanielRenner.Wages
{
    public class JobGiver_WageTheft : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            // get the owed amount
            float owedSilver = PayHelperUtility.GetOwedCredit(pawn);
            if (owedSilver <= 0) return null;

            // validator: Find an item that belongs to the colony, isn't currently equipped, and has value
            Predicate<Thing> validator = delegate (Thing t)
            {
                if (!pawn.CanReserve(t)) return false;
                if (t.def.destroyOnDrop) return false; // Don't target weird invisible/system items
                if (t.ParentHolder is Pawn_InventoryTracker || t.ParentHolder is Pawn_EquipmentTracker) return false; // Don't target things people are wearing

                float totalValue = t.MarketValue * t.stackCount;
                // Target items that cover at least some of their debt, or are just generally expensive
                return totalValue >= (owedSilver * 0.25f);
            };

            // search the map for the closest valid target
            Thing targetItem = GenClosest.ClosestThingReachable(
                pawn.Position,
                pawn.Map,
                ThingRequest.ForGroup(ThingRequestGroup.HaulableAlways), // Look for standard items
                PathEndMode.Touch,
                TraverseParms.For(pawn),
                9999f,
                validator
            );

            if (targetItem == null)
            {
                // if they can't find an item, they might just wander or end the mental state
                return null;
            }

            Job job = JobMaker.MakeJob(DefOfs_Wages.Wages_DestroyItem, targetItem);
            return job;
        }
    }
}
