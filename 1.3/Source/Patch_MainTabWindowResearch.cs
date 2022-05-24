using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    [HarmonyPatch(typeof(MainTabWindow_Research), nameof(MainTabWindow_Research.DoWindowContents))]
    public static class Patch_MainTabWindowResearch
    {
        public static void Postfix(MainTabWindow_Research __instance, Rect inRect)
        {
            Log.DebugOnce("Patch_MainTabWindowResearch.Postfix() is getting called");
            var selectedProject = getSelectedProject(__instance);
            var repeatableResearchGameComponent = Current.Game?.GetComponent<GameComponent_ResearchRepeatTracker>();
            if (repeatableResearchGameComponent != null && repeatableResearchGameComponent.progressRepeats != null && selectedProject != null)
            {
                float finishedRepeats = repeatableResearchGameComponent.GetFinishedRepeats(selectedProject);
                if (finishedRepeats > 0)
                {
                    float widthSidepanel = Mathf.Max(200f, inRect.width * 0.22f);
                    float widthRepeatOverlay = Mathf.Max(100f, inRect.width * 0.11f);
                    Rect repeatsOverlay = new Rect(widthSidepanel - widthRepeatOverlay, inRect.height - 165f, widthRepeatOverlay, Text.LineHeight);
                    var text = "RepeatableResearch.timesAlreadyResearched".Translate(finishedRepeats);
                    //Log.Debug("rendering '" + text + "' repeats to " + String.Join(":", new[] { repeatsOverlay.x, repeatsOverlay.y, repeatsOverlay.width, repeatsOverlay.height }));
                    Text.Anchor = TextAnchor.MiddleRight;
                    Text.Font = GameFont.Small;
                    Widgets.LabelCacheHeight(ref repeatsOverlay, text);
                    Text.Anchor = TextAnchor.UpperLeft; // restore the ancor to avoid an error in the log
                }
            }
        }

        // since the selectedProject member from the window is protected, we have to access it via reflection
        public static ResearchProjectDef getSelectedProject(MainTabWindow_Research window)
        {
            Type researchManagerType = typeof(MainTabWindow_Research);
            FieldInfo fieldType = researchManagerType.GetField("selectedProject", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldType != null)
            {
                var selectedProject = fieldType.GetValue(window) as ResearchProjectDef;
                return selectedProject;
            }
            return null;
        }
    }
}
