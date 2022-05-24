using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    class StatPart_ResearchImpact : StatPart
    {
        // variables loaded from the DEF
        public List<ResearchImpactDef> researchImpact;

        public override string ExplanationPart(StatRequest req)
        {
            string explanation = "";
            if (researchImpact != null && !researchImpact.NullOrEmpty())
            {
                var repeatableResearchGameComponent = Current.Game?.GetComponent<GameComponent_ResearchRepeatTracker>();
                foreach (var impact in researchImpact)
                {
                    float repeats = repeatableResearchGameComponent.GetFinishedRepeats(impact.researchProject);
                    float multiplier = impact.multiplierRepeatableMultiplierCurve.Evaluate(repeats);
                    if (multiplier > 0)
                    {
                        explanation += "RepeatableResearch.statBonusExplanation".Translate(impact.researchProject.label, repeats, multiplier * 100);
                    }
                }
            }
            Log.DebugOnce(explanation);
            return explanation;
        }

        public override void TransformValue(StatRequest req, ref float val)
        {
            float multiplierTotal = 1f;
            if (researchImpact != null && !researchImpact.NullOrEmpty())
            {
                var repeatableResearchGameComponent = Current.Game?.GetComponent<GameComponent_ResearchRepeatTracker>();
                foreach (var impact in researchImpact)
                {
                    float repeats = repeatableResearchGameComponent.GetFinishedRepeats(impact.researchProject);
                    float multiplier = impact.multiplierRepeatableMultiplierCurve.Evaluate(repeats);
                    multiplierTotal *= multiplier;
                    //Log.Debug("increased multiplier by " + multiplier * 100 + " % by research on " + impact.researchProject.label);
                }
            }
            Log.DebugOnce("total multiplier is " + multiplierTotal * 100 + " % for " + req.Def.ToString());
            if (multiplierTotal != 1f)
            {
                val *= multiplierTotal;
            }
        }
    }

    // structure definition of the DEF files
    public class ResearchImpactDef
    {
        public ResearchProjectDef researchProject;
        public float multiplierIfFinished = 1f;
        public SimpleCurve multiplierRepeatableMultiplierCurve;
    }
}
