using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.Wages
{
    [DefOf]
    public static class DefOfs_Wages
    {
        public static RecordDef TotalWagesOwed;
        public static RecordDef TotalWagesCollected;
        public static RecordDef TotalWagesSpent;
        public static RecordDef CurrentWage;

        public static JobDef CollectWages;
        public static JobDef Wages_DestroyItem;

        public static ThoughtDef WageLevelEffect;
        public static ThoughtDef NotGettingPaid;

        public static HediffDef WagesExempt;

        public static MentalStateDef Wages_WageTheft;

        [MayRequireBiotech]
        public static GeneDef Wages_Exempt;
    }
}
