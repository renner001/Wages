using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DanielRenner.RepeatableResearch
{
    class CompProperties_SpecificRefuelable : CompProperties_Refuelable
    {
        public CompProperties_SpecificRefuelable() : base()
        {
            compClass = typeof(Comp_SpecificRefuelable);
        }
    }
}
