using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.Wages
{
    public class GameComponent_WageDirector : GameComponent
    {
        // saved variables
        int lastTickWagesApplied = 0;

        // temporary variables
        Game loadedGame;

        public GameComponent_WageDirector(Game game)
        {
            Log.Debug("GameComponent_WageDirector created");
            loadedGame = game;
        }


        public override void GameComponentTick()
        {
            base.GameComponentTick();

            if (Find.TickManager.TicksGame == PayHelperUtility.NextPaydayTick)
            {
                // update the moodCache
                var validPawns = PayHelperUtility.AllValidPawnsIncludingExempt;
                PayHelperUtility.CalcWageSteps();
                PayHelperUtility.FixWrongWages();
                var totalWagesOwed = 0;
                foreach (Pawn pawn in validPawns)
                {
                    if (PayHelperUtility.IsWageExempt(pawn))
                    {
                        Log.Debug($"pawn {pawn} is exempt from wages. Skipping pay day.");
                        continue;
                    }
                    var wageOfPawn = (int)pawn.records.GetValue(DefOfs_Wages.CurrentWage);
                    Log.Debug("Incrementing owed silver for pawn " + pawn + "by " + wageOfPawn);
                    totalWagesOwed += wageOfPawn;
                    pawn.records.AddTo(DefOfs_Wages.TotalWagesOwed, wageOfPawn);
                    // directly handle gathering wages for pawns not on a map but a caravan or similar:
                    if (pawn.Map == null)
                    {
                        gatherWageOffmap(pawn);
                    }
                    // apply the wage mood effect
                    pawn.needs.mood.thoughts.memories.TryGainMemory((Thought_Memory)ThoughtMaker.MakeThought(DefOfs_Wages.WageLevelEffect, PayHelperUtility.CalcMoodStage(pawn, wageOfPawn)));
                }
                if (totalWagesOwed > 0)
                {
                    Messages.Message("Payday! " + totalWagesOwed + " silver will be collected by your colonists. Make sure, you have enough on hand.", MessageTypeDefOf.CautionInput);
                }
            }
        }

        private void gatherWageOffmap(Pawn pawn)
        {
            int owed = PayHelperUtility.GetOwedCredit(pawn);
            if (owed <= 0) return;

            var currency = ModSettings_Wages.currencyDef;
            float itemValue = currency.BaseMarketValue;
            int itemsNeededToPay = (int)Math.Ceiling(owed / itemValue);

            // SCENARIO 1: The pawn is in a Vanilla Caravan
            Caravan caravan = pawn.GetCaravan();
            if (caravan != null)
            {
                // Manually count the currency using the available AllInventoryItems method
                int currencyInCaravan = 0;
                List<Thing> invItems = CaravanInventoryUtility.AllInventoryItems(caravan);
                for (int i = 0; i < invItems.Count; i++)
                {
                    if (invItems[i].def == currency)
                    {
                        currencyInCaravan += invItems[i].stackCount;
                    }
                }

                if (currencyInCaravan > 0)
                {
                    int amountToTake = Math.Min(itemsNeededToPay, currencyInCaravan);
                    int remainingToTake = amountToTake;

                    // This delegate runs on every item in the caravan. If it returns > 0, it extracts that amount.
                    List<Thing> extractedCurrency = CaravanInventoryUtility.TakeThings(caravan, delegate (Thing thing)
                    {
                        if (thing.def == currency && remainingToTake > 0)
                        {
                            int take = Math.Min(thing.stackCount, remainingToTake);
                            remainingToTake -= take;
                            return take;
                        }
                        return 0; // Don't take anything else
                    });

                    // Destroy the extracted stacks
                    int actuallyTaken = 0;
                    for (int i = 0; i < extractedCurrency.Count; i++)
                    {
                        actuallyTaken += extractedCurrency[i].stackCount;
                        extractedCurrency[i].Destroy(); // Poof!
                    }

                    if (actuallyTaken > 0)
                    {
                        PayHelperUtility.AddWage(pawn, (int)(actuallyTaken * itemValue));
                    }
                }
                return; 
            }

            // SCENARIO 2: The pawn is in a VOE Outpost, Transport Pod, Shuttle, Space Ship, etc.
            IThingHolder holder = pawn.ParentHolder;
            if (holder != null)
            {
                // Let RimWorld grab EVERY item inside the outpost/pod recursively
                List<Thing> allThings = new List<Thing>();
                ThingOwnerUtility.GetAllThingsRecursively(holder, allThings, false);

                // Now filter that list down to just our currency stacks
                List<Thing> allCurrencyStacks = allThings.Where(t => t.def == currency).ToList();

                int currencyAvailable = allCurrencyStacks.Sum(t => t.stackCount);

                if (currencyAvailable > 0)
                {
                    int amountToTake = Math.Min(itemsNeededToPay, currencyAvailable);
                    int taken = 0;

                    foreach (Thing stack in allCurrencyStacks)
                    {
                        if (taken >= amountToTake) break;

                        int takeFromStack = Math.Min(amountToTake - taken, stack.stackCount);
                        stack.SplitOff(takeFromStack).Destroy();
                        taken += takeFromStack;
                    }

                    if (taken > 0)
                    {
                        PayHelperUtility.AddWage(pawn, (int)(taken * itemValue));
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            try
            {
                Scribe_Values.Look(ref lastTickWagesApplied, "lastTickWagesApplied", 0);
                //Scribe_Collections.Look(ref knownThreatSettings, "knownThreatSettings", LookMode.Reference, LookMode.Deep, ref tempKeyListThreatSettings, ref tempValueListThreatSettings);
            }
            catch (Exception ex)
            {
                Log.Warning("Failed to load settings of GameComponent_RennPondManager. This is an error the game will recover from within the next seconds. Details: " + ex);
            }
            //Log.Debug("GameComponent_WageDirector.ExposeData(): " + String.Join(", ", knownThreatSettings.Select(setting => { return setting.Key.ToString() + ": threatCap=" + setting.Value.threatCap + "; threatMultiplier=" + setting.Value.threatMultiplier; })));
        }
    }
}
