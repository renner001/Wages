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
	/// not used as of now, since the stage is directly enforced by the research mod
	/// </summary>
	public class ThoughtWorker_MoodBoost : ThoughtWorker
	{
		protected override ThoughtState CurrentStateInternal(Pawn p)
		{
			if (def == null)
            {
				Log.Error("def is not set!");
				return ThoughtState.Inactive;
			}
			if (def.defName != "MoodBoost")
            {
				Log.Error("def '" + def.defName + "' is of the wrong type!");
				return ThoughtState.Inactive;
			}
			// todo: calculate the right stage...
			return ThoughtState.Inactive;
			//return ThoughtState.ActiveAtStage(0);
		}
	}
}
