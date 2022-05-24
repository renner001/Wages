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
    [HarmonyPatch(typeof(ResearchManager), nameof(ResearchManager.ResetAllProgress))]
    public static class Patch_ResearchManager
    {
        /// <summary>
        /// on a complete reset of all research, also reset the counter of number of completions
        /// </summary>
        public static void Postfix()
        {
            var repeatableResearchGameComponent = Current.Game?.GetComponent<GameComponent_ResearchRepeatTracker>();
            if (repeatableResearchGameComponent == null)
            {
                Log.Error("failed to find the running ResearchRepeatTracker_GameComponent");
                return;
            }
            repeatableResearchGameComponent.ResetAllProgress();
        }
    }
}
