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
        static Dictionary<Pawn,KeyValuePair<int, int>[]> moodStepCache;
        public static Dictionary<Pawn, KeyValuePair<int, int>[]> MoodStepCache
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
        public static Dictionary<Pawn,KeyValuePair<int,int>[]> CalcWageSteps()
        {
            var allPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves.ToList();
            moodStepCache = new Dictionary<Pawn, KeyValuePair<int, int>[]>();


            // in case of fixed income, we calculate one step list for all pawns based on the settings set zero point:
            if (ModSettings_Wages.fixedIncome)
            {
                List<KeyValuePair<int, int>> steps = BuildSteps(AverageWage);
                Log.Debug("calculated new mood by wage steps based on fix income=" + AverageWage + ": " + string.Join(", ", steps.Select(step => { return step.Key + " silver -> " + step.Value + " mood step"; })));
                allPawns.ForEach(pawn =>
                {
                    moodStepCache[pawn] = steps.ToArray();
                });
            }
            else
            {
                // we build an individual mood step list for each pawn based on his skills:
                allPawns.ForEach(pawn =>
                {
                    float factor = 0.5f; // fallback is right down the middle - 50%
                    int numSkills = 0;
                    int summarySkills = 0;
                    if (pawn.skills != null && pawn.skills.skills != null && pawn.skills.skills.Count > 0)
                    {
                        // we use the highest skill with 50%; the second skill with 25% and the average of all skills with 25% of the maximum expected wage
                        var skillCache = pawn.skills.skills.ToList();
                        numSkills = skillCache.Count;
                        skillCache.Sort((x, y) => x.Level.CompareTo(y.Level));
                        var highestSkillValue = skillCache[numSkills - 1];
                        var secondHighestSkill = skillCache[numSkills - 2];
                        var factorHighestSkill = highestSkillValue.Level / 20.0f;
                        var factorSecondHighestSkill = secondHighestSkill.Level / 20.0f;
                        skillCache.ForEach(skill =>
                        {
                            summarySkills += skill.Level;
                        });
                        var factorAverageSkills = summarySkills / (numSkills * 20.0f); // some mods change the limits beyond 20... let's tomorrow thing about that
                        factor = 0.3f * factorAverageSkills + 0.3f * factorSecondHighestSkill + 0.4f * factorHighestSkill;
                    }
                    else
                    {
                        Log.ErrorOnce("skills not found for pawn=" + pawn + ". Wages might be off!", 5491650);
                    }
                    // even with a very skilled pawn with factor 1.0, we still want the option to go to +10 mood -> with factor 1.0, we need to be right in the middle
                    var zeroPointSilver = (int)Math.Round(MinWage + (MaxWage - MinWage) * factor / 2.0f);

                    List<KeyValuePair<int, int>> steps = BuildSteps(zeroPointSilver);
                    Log.Debug("calculated new mood by wage steps based on numSkills=" + numSkills + " ,skillSum=" + summarySkills + " for pawn=" + pawn + ": " + string.Join(", ", steps.Select(step => { return step.Key + " silver -> " + step.Value + " mood step"; })));
                    moodStepCache[pawn] = steps.ToArray();
                });
            }

            return moodStepCache;
        }

        private static List<KeyValuePair<int, int>> BuildSteps(int zeroPointWage)
        {
            // build the step distances to use:
            var distancePositive = MaxWage - zeroPointWage;
            var distanceNegative = zeroPointWage - MinWage;
            int stepSizeInSilver = 10;
            if (distancePositive >= distanceNegative)
            {
                // positive side is bigger -> we derive our step size from there
                stepSizeInSilver = (int)Math.Floor((double)distancePositive / 10);
            }
            else
            {
                stepSizeInSilver = (int)Math.Floor((double)distanceNegative / 10);
            }
            // if we have too small steps etc, we simply use the same mood for everything, which is 0
            if (stepSizeInSilver < 1)
            {
                Log.Debug("calculated new mood by wage steps: always 0 mood effect");
                return (new[] { new KeyValuePair<int, int>(0, 0) }).ToList();
            }

            // calculate the steps:
            List<KeyValuePair<int, int>> steps = new List<KeyValuePair<int, int>>();

            // add negative moods if we have a "negative area"
            if (MinWage < zeroPointWage)
            {
                // calculate the boundries
                var highestNegativeStack = (int)Math.Ceiling((double)distanceNegative / stepSizeInSilver);

                // add all the negative steps

                steps.Add(new KeyValuePair<int, int>(MinWage, 10 + highestNegativeStack));
                highestNegativeStack -= 1;
                while (highestNegativeStack > 0)
                {
                    var curWageBorder = zeroPointWage - stepSizeInSilver * highestNegativeStack;
                    var moodStep = highestNegativeStack + 10;
                    steps.Add(new KeyValuePair<int, int>(curWageBorder, moodStep));
                    highestNegativeStack -= 1;
                }
            }
            // add the average value
            steps.Add(new KeyValuePair<int, int>(zeroPointWage, 0)); // step 0 -> no effect

            var stepNum = 1;
            while (zeroPointWage + stepNum * stepSizeInSilver <= MaxWage)
            {
                steps.Add(new KeyValuePair<int, int>(zeroPointWage + stepNum * stepSizeInSilver, stepNum)); // step 1..10 -> positive effects
                stepNum += 1;
            }

            return steps;
        }

        public static int CalcMoodStage(Pawn pawn, int silverWage)
        {
            var steps = moodStepCache[pawn];
            if (steps == null || steps.Length == 0)
            {
                Log.Error("failed to calculate the mood stage for pawn=" + pawn + " and a wage of " + silverWage + " silver");
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
        public static int MoodFromWage(Pawn pawn, int silversWage)
        {
            return (int)DefOfs_Wages.WageLevelEffect.stages[CalcMoodStage(pawn, silversWage)].baseMoodEffect;
        }
    }
}
