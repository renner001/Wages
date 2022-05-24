using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    class ResearchMod_Repeatable : ResearchMod
    {
        // the research project this mod has been attached to
        public ResearchProjectDef def;

        public override void Apply()
        {
            Log.Debug("ResearchMod_Repeatable.Apply() has been called");
            //Find.ResearchManager.
            if (def == null)
            {
                Log.Error("def is null for ResearchMod_Repeatable! The Research project has not been found!");
                return;
            }
            Find.ResearchManager.IncrementRepeatableResearchCompletions(def);
        }
    }

    // since the interesting fields and members of ReseachManager are private, we need to add then using c# Extension Methods 
    public static class ResearchManagerExtensions
    {
        public static void IncrementRepeatableResearchCompletions(this ResearchManager researchManager, ResearchProjectDef repeatableResearch)
        {
            Log.Debug("ResearchMod_Repeatable.IncrementRepeatableResearchCompletions() called");
            if (repeatableResearch == null)
            {
                Log.Error("tried to reset progress on null value research");
                return;
            }
            // get the private progress member
            Type researchManagerType = typeof(ResearchManager);
            FieldInfo type = researchManagerType.GetField("progress", BindingFlags.NonPublic | BindingFlags.Instance);
            var progress = type.GetValue(researchManager) as Dictionary<ResearchProjectDef, float>;
            if (progress == null)
            {
                Log.Error("failed to get progress member from ResearchManager");
            }
            // set the progress to 0
            if (!progress.ContainsKey(repeatableResearch))
            {
                Log.Debug("failed to reset progress on " + repeatableResearch.defName + " since that research does not even exist");
            } 
            else
            {
                Log.Debug("resetting research progress on " + repeatableResearch.defName);
                progress[repeatableResearch] = 0;
            }
            // and increment the store for total times completed
            var repeatableResearchGameComponent = Current.Game?.GetComponent<GameComponent_ResearchRepeatTracker>();
            if (repeatableResearchGameComponent == null)
            {
                Log.Error("failed to find the running ResearchRepeatTracker_GameComponent");
                return;
            }
            if (!repeatableResearchGameComponent.progressRepeats.ContainsKey(repeatableResearch))
            {
                Log.Debug("first time encountering " + repeatableResearch.defName + " in the repeatable research");
                repeatableResearchGameComponent.progressRepeats[repeatableResearch] = 0f;
            }
            repeatableResearchGameComponent.progressRepeats[repeatableResearch] = repeatableResearchGameComponent.progressRepeats[repeatableResearch] + 1f;
            Log.Debug("incremented " + repeatableResearch.defName + " repeats to " + repeatableResearchGameComponent.progressRepeats[repeatableResearch]);
        }
    }
}
