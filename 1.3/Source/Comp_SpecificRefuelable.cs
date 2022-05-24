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
    /// <summary>
    /// The original CompRefuelable does not notify about what has been fueled into it. Therefore we override the class and extend the Refuel(num) method to also notify about the type of item fueled into it
    /// </summary>
    class Comp_SpecificRefuelable : CompRefuelable
    {
        public ThingDef lastRefueledWith;

        public void PreRefuel(List<Thing> fuelThings)
        {
            Log.Debug("Comp_SpecificRefuelable.Refuel() called");
            lastRefueledWith = fuelThings.First().def;
        }

    }

    [HarmonyPatch(typeof(CompRefuelable), nameof(CompRefuelable.Refuel), new[] { typeof(List<Thing>) })]
    /// <summary>
    /// Unfortunately the Refuel() method in CompRefuelable is not defined virtual, so in order to overload it, we need to include a Prefix that redirecty the Refuel calls to the Comp_SpecificRefuelable Refuel method if the object is of that type  
    /// </summary>
    public static class Patch_CompRefuelable
    {
        public static bool Prefix(List<Thing> fuelThings, CompRefuelable __instance)
        {
            if (__instance is Comp_SpecificRefuelable)
            {
                var instance = __instance as Comp_SpecificRefuelable;
                instance.PreRefuel(fuelThings);
            }
            return true;
        }
    }
}
