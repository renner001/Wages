using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    [HarmonyPatch(typeof(ThingListGroupHelper), nameof(ThingListGroupHelper.Includes))]
    public static class Patch_ThingListGroupHelper
    {
        public static bool Prefix(ThingRequestGroup group, ThingDef def, ref bool __result)
        {
            if (group == ThingRequestGroup.Refuelable && def.HasComp(typeof(Comp_SpecificRefuelable)))
            {
                Log.DebugOnce("overriding ThingListGroupHelper.Includes() for '" + def + "' on component containing Comp_SpecificRefuelable");
                __result = true;
                return false;
            }
            return true;
        }
    }
}
