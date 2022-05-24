using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    public class GameComponent_ResearchRepeatTracker : GameComponent
    {
		public GameComponent_ResearchRepeatTracker(Game game) {
			Log.Debug("ResearchRepeatTracker_GameComponent created");
        }

		public Dictionary<ResearchProjectDef, float> progressRepeats = new Dictionary<ResearchProjectDef, float>();
		public override void ExposeData()
		{
			Log.Debug("ResearchRepeatTracker_GameComponent.ExposeData() called");
			base.ExposeData();
			Scribe_Collections.Look(ref progressRepeats, "progressRepeats", LookMode.Def, LookMode.Value);
			Log.Debug("progressRepeats: " + string.Join(", ", progressRepeats.Select(progressEntry => { return progressEntry.Key.defName + ":" + progressEntry.Value; })));
		}

		public float GetFinishedRepeats(ResearchProjectDef def)
        {
			if (progressRepeats == null || !progressRepeats.ContainsKey(def))
            {
				return 0;
            }
			return progressRepeats[def];

		}

		public void ResetAllProgress()
        {
			progressRepeats = new Dictionary<ResearchProjectDef, float>();
		}
	}
}
