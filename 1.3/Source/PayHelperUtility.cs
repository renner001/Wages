using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.Wages
{
    /// <summary>
    /// supports with some calculation methods to calculate payment sizes and mood effects
    /// 
    /// There are 1 neutral, 10 positive and 10 negative permanent mood stages
    /// Additionaly, there is a temporary "I am owed money" mood that is applied if the wages cannot be collected
    /// </summary>
    static public class PayHelperUtility
    {
        static KeyValuePair<int, int>[] moodStepCache;
        public static KeyValuePair<int, int>[] MoodStepCache
        {
            get { return moodStepCache; }
        }

        public static int AverageWage
        {
            get
            {
                return ModSettings_Wages.zeroPointWage;
            }
        }
        public static int MinWage
        {
            get
            {
                return ModSettings_Wages.minWage;
            }
        }

        public static int MaxWage
        {
            get
            {
                return ModSettings_Wages.maxWage;
            }
        }

        /// <summary>
        /// It can happen, that changes in the settings make some of the wages set for pawns invalid. This method returns the wages of our pawns valid again.
        /// </summary>
        public static void FixWrongWages()
        {
            var validPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
            foreach (Pawn pawn in validPawns)
            {
                var wageOfPawn = (int)pawn.records.GetValue(DefOfs_Wages.CurrentWage);
                if (wageOfPawn > MaxWage)
                {
                    Log.Debug("correcting bad wage for pawn " + pawn + ": is=" + wageOfPawn + "larger than MaxWage=" + MaxWage);
                    pawn.records.AddTo(DefOfs_Wages.CurrentWage, MaxWage - wageOfPawn);
                }
                if (wageOfPawn < MinWage)
                {
                    Log.Debug("correcting bad wage for pawn " + pawn + ": is=" + wageOfPawn + "smaller than MinWage=" + MinWage);
                    pawn.records.AddTo(DefOfs_Wages.CurrentWage, MinWage - wageOfPawn);
                }
            }
        }

        /// <summary>
        /// key = wage
        /// value = mood stage
        /// </summary>
        /// <returns></returns>
        public static KeyValuePair<int,int>[] CalcWageSteps()
        {
            var distancePositive = MaxWage - AverageWage;
            var distanceNegative = AverageWage - MinWage;
            int stepSize = 10; 
            if (distancePositive >= distanceNegative)
            {
                // positive side is bigger -> we derive our step size from there
                stepSize = (int)Math.Floor((double)distancePositive / 10);
            } 
            else
            {
                stepSize = (int)Math.Floor((double)distanceNegative / 10);
            }
            // if we have too small steps etc, we simply use the same mood for everything, which is 0
            if (stepSize < 1)
            {
                Log.Debug("calculated new mood by wage steps: always 0 mood effect");
                moodStepCache = new[] { new KeyValuePair<int, int>(0, 0) };
                return moodStepCache;
            }

            // calculate the steps:
            List<KeyValuePair<int, int>> steps = new List<KeyValuePair<int, int>>();

            // add negative moods if we have a "negative area"
            if (MinWage < AverageWage)
            {
                // calculate the boundries
                var highestNegativeStack = (int)Math.Ceiling((double)distanceNegative / stepSize);
                
                // add all the negative steps

                steps.Add(new KeyValuePair<int, int>(MinWage, 10 + highestNegativeStack));
                highestNegativeStack -= 1;
                while (highestNegativeStack > 0)
                {
                    var curWageBorder = AverageWage - stepSize * highestNegativeStack;
                    var moodStep = highestNegativeStack + 10;
                    steps.Add(new KeyValuePair<int, int>(curWageBorder, moodStep));
                    highestNegativeStack -= 1;
                }
            }
            // add the average value
            steps.Add(new KeyValuePair<int, int>(AverageWage, 0)); // step 0 -> no effect

            var stepNum = 1;
            while (AverageWage + stepNum * stepSize <= MaxWage)
            {
                steps.Add(new KeyValuePair<int, int>(AverageWage + stepNum * stepSize, stepNum)); // step 1..10 -> positive effects
                stepNum += 1;
            }

            Log.Debug("calculated new mood by wage steps: " + string.Join(", ", steps.Select(step => { return step.Key + " silver -> " + step.Value + " mood step"; })));
            moodStepCache = steps.ToArray();
            return moodStepCache;
        }

        public static int CalcMoodStage(int silverWage)
        {
            var steps = moodStepCache;
            if (steps == null || steps.Length == 0)
            {
                Log.Error("failed to calculate the mood stage for a wage of " + silverWage + " silver");
                return 0;
            }
            var index = 0;
            while (index + 1 < steps.Length && steps[index + 1].Key <= silverWage)
            {
                index += 1;
            }
            return steps[index].Value;
        }

        /// <summary>
        /// calculates the mood based on the stage
        /// </summary>
        public static int MoodFromMoodStage(int stage)
        {
            return (int)DefOfs_Wages.WageLevelEffect.stages[stage].baseMoodEffect;
        }

        /// <summary>
        /// calculates the mood based on the silver wages
        /// </summary>
        public static int MoodFromWage(int silversWage)
        {
            return (int)DefOfs_Wages.WageLevelEffect.stages[CalcMoodStage(silversWage)].baseMoodEffect;
        }
    }
}
