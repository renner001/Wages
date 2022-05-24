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
    public class WorkGiver_CollectWages : WorkGiver_Scanner
    {
        public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForGroup(ThingRequestGroup.Pawn);

        public override PathEndMode PathEndMode => PathEndMode.Touch;

        public virtual JobDef JobStandard => JobDefOf.Refuel;

        /*
        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (CanRefuelThing(t))
            {
                return RefuelWorkGiverUtility.CanRefuel(pawn, t, forced);
            }
            return false;
        }

        public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            return RefuelWorkGiverUtility.RefuelJob(pawn, t, forced, JobStandard, JobAtomic);
        }

        */
    }
}
