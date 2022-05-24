using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    class Building_RennBase : Building
    {
        RennPondSettings mySettings;
        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            Log.Debug("Building_RennBase.Destroy() called");
            var rennPondManager = Current.Game?.GetComponent<GameComponent_RennPondManager>();
            rennPondManager.RemoveTrackedThing(this);
            base.Destroy(mode);
        }

        protected void SetThreatPoints(int moodEffect, float multiplier, int cap)
        {
            Log.Debug("Building_RennBase.SetThreatPoints(): updating threat points");
            if (mySettings == null)
            {
                mySettings = new RennPondSettings()
                {
                    threatMultiplier = multiplier,
                    threatCap = cap
                };
            }
            mySettings.threatMultiplier = multiplier;
            mySettings.threatCap = cap;
            var rennPondManager = Current.Game?.GetComponent<GameComponent_RennPondManager>();
            rennPondManager.SetThreatPoints(this, mySettings);
        }
    }
}
