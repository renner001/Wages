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
        protected override Job TryGiveJob(Pawn pawn)
        {
            Log.DebugOnce("at least JobGiver_CollectWages.TryGiveJob() is getting called...")
            // check whether the pawn wants to collect wages now
            ;
            return null;
        }
    }
}
