using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;

namespace DanielRenner.Wages
{
    // Patching the new 1.5 GetOptions method
    [HarmonyPatch(typeof(FloatMenuMakerMap), "GetOptions")]
    public static class Patch_GetOptions
    {
        [HarmonyPostfix]
        public static void Postfix(List<Pawn> selectedPawns, Vector3 clickPos, ref List<FloatMenuOption> __result)
        {
            if (__result == null)
                return;

            if (clickPos == null)
                return;

            // we only want this to work if the player has exactly ONE pawn selected.
            if (selectedPawns == null || selectedPawns.Count != 1) 
                return;

            Pawn pawn = selectedPawns[0];

            // 2. Basic sanity checks: Is the pawn controllable and conscious?
            if (pawn.Drafted || pawn.Downed || pawn.Dead)
            {
                return;
            }

            // 3. Figure out what exactly the player right-clicked on
            IntVec3 clickCell = IntVec3.FromVector3(clickPos);
            foreach (Thing thing in clickCell.GetThingList(pawn.Map))
            {
                // Check if the clicked item is our currency (fallback to Silver if currencyDef is null)
                ThingDef currentCurrency = ModSettings_Wages.currencyDef;

                if (currentCurrency != null && thing.def == currentCurrency)
                {
                    // 4. Check if this specific pawn is actually owed any money
                    int owedSilver = PayHelperUtility.GetOwedCredit(pawn);

                    if (owedSilver > 0)
                    {
                        // 5. Build the text for the button
                        string menuLabel = $"Collect wages ({owedSilver} owed in {currentCurrency.label})";

                        // 6. Define what happens when the player clicks the button
                        Action action = delegate
                        {
                            // Calculate how much they can actually pick up from this stack
                            int amountToTake = Math.Min(owedSilver, thing.stackCount);

                            // Create the job and force the pawn to do it
                            Job job = JobMaker.MakeJob(DefOfs_Wages.CollectWages, thing);
                            job.count = amountToTake;
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };

                        // 7. Add the option to the list!
                        // In Harmony, modifying the 'ref __result' modifies the actual list the game outputs.
                        __result.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(menuLabel, action, MenuOptionPriority.High), pawn, thing));
                    }
                    else
                    {
                        // Optional: Show a greyed-out option if they aren't owed anything
                        __result.Add(new FloatMenuOption($"Cannot collect wages: Not owed any {currentCurrency.label}", null));
                    }
                }
            }
        }
    }
}
