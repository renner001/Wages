using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.Wages
{
    class ModSettings_Wages : ModSettings
    {
        public static int maxWage = 200;
        public static int minWage = 0;
        public static int zeroPointWage = 10;
        public static bool fixedIncome = false;
        public static int owedWageIgnoredBeforeGathering = 50;
        public static int owedWageIgnoredBeforeMoodlet = 500;
        public static bool hideExemptBehindDevMode = true;
        public static ThingDef currencyDef;
        public static bool exemptChildren = true;
        public static bool exemptSlaves = true;
        public static bool exemptColonists = false;
        public static int breakChancePerHourPercent = 5;


        public static void DoSettingsWindowContents(Rect rect)
        {
            Rect descriptionRect = rect.TopPartPixels(Text.CalcHeight(Translations_Wages.Static.SettingsPanelChangeSettingsEffect, rect.width));
            Rect mainRect = rect.BottomPartPixels(rect.height - descriptionRect.height - 50);
            Widgets.Label(descriptionRect, Translations_Wages.Static.SettingsPanelChangeSettingsEffect);

            Listing_Standard listMain = new Listing_Standard()
            {
                ColumnWidth = mainRect.width,
            };

            listMain.Begin(mainRect);

            // show the select dialog for the currency
            Rect currencyRowRect = listMain.GetRect(Text.LineHeight).Rounded();
            Widgets.Label(currencyRowRect.LeftHalf(), "Wages.Settings_CurrencyToUse".Translate());
            Rect buttonRect = currencyRowRect.RightHalf().RightPartPixels(400); // Matches your slider width
            string buttonLabel = "Wages.Settings_SelectCurrency".Translate();
            if (currencyDef != null)
                buttonLabel = currencyDef.LabelCap;
            if (Widgets.ButtonText(buttonRect, buttonLabel))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();

                // Scan the database for valid items (no buildings, no abstracts, must have value)
                var validItems = DefDatabase<ThingDef>.AllDefs.Where(d =>
                    d.category == ThingCategory.Item &&
                    d.BaseMarketValue > 0 &&
                    !d.destroyOnDrop &&
                    d.tradeability != Tradeability.None);

                foreach (ThingDef def in validItems)
                {
                    // Capture the 'def' variable for the delegate
                    ThingDef localDef = def;

                    // Create a clickable option. We pass localDef to automatically draw the item's icon!
                    options.Add(new FloatMenuOption(localDef.LabelCap, delegate
                    {
                        currencyDef = localDef;
                    }, localDef));
                }

                // Alphabetize the list so players can actually find what they are looking for
                options.SortBy(o => o.Label);

                // Open the dropdown menu
                Find.WindowStack.Add(new FloatMenu(options));
            }

            // Add a tooltip for the row
            if (Mouse.IsOver(currencyRowRect))
            {
                Widgets.DrawHighlight(currencyRowRect);
                TooltipHandler.TipRegion(currencyRowRect, "Wages.Settings_CurrencyTooltip".Translate());
            }

            listMain.Gap();
            listMain.Label("Wages.Settings_GlobalExemptions".Translate());
            listMain.CheckboxLabeled("Wages.Settings_ExemptChildren".Translate(), ref exemptChildren, "Wages.Settings_ExemptChildrenTooltip".Translate());
            listMain.CheckboxLabeled("Wages.Settings_ExemptSlaves".Translate(), ref exemptSlaves, "Wages.Settings_ExemptSlavesTooltip".Translate());
            listMain.CheckboxLabeled("Wages.Settings_ExemptColonists".Translate(), ref exemptColonists, "Wages.Settings_ExemptColonistsTooltip".Translate());
            listMain.Gap();

            string currentUnit = currencyDef != null ? currencyDef.label : "Wages.Silver".Translate().ToString();

            LabeledScrollbar(listMain, "Wages.Settings_MinWage".Translate(), 0, maxWage, 10, currentUnit, ref minWage, "Wages.Settings_MinWageTooltip".Translate());
            LabeledScrollbar(listMain, "Wages.Settings_MaxWage".Translate(), minWage, 1000, 10, currentUnit, ref maxWage, "Wages.Settings_MaxWageTooltip".Translate());
            LabeledScrollbar(listMain, "Wages.Settings_OwedBeforeGathering".Translate(), 0, 2000, 20, currentUnit, ref owedWageIgnoredBeforeGathering, "Wages.Settings_OwedBeforeGatheringTooltip".Translate());
            LabeledScrollbar(listMain, "Wages.Settings_OwedBeforeMoodlet".Translate(), 0, 4000, 40, currentUnit, ref owedWageIgnoredBeforeMoodlet, "Wages.Settings_OwedBeforeMoodletTooltip".Translate());

            LabeledScrollbar(listMain, "Wages.Settings_BreakChance".Translate(), 0, 100, 1, "%", ref breakChancePerHourPercent, "Wages.Settings_BreakChanceTooltip".Translate());

            listMain.CheckboxLabeled("Wages.Settings_HideExempt".Translate(), ref hideExemptBehindDevMode, "Wages.Settings_HideExemptTooltip".Translate());
            listMain.Gap();

            // build the checkbox
            Rect fixedIncomeCheckboxRect = listMain.GetRect(Text.LineHeight).Rounded();
            Widgets.CheckboxLabeled(fixedIncomeCheckboxRect, "Wages.Settings_FixedIncome".Translate(), ref fixedIncome, false);

            if (zeroPointWage > maxWage)
            {
                zeroPointWage = maxWage;
            }
            if (zeroPointWage < minWage)
            {
                zeroPointWage = minWage;
            }
            if (fixedIncome)
            {
                LabeledScrollbar(listMain, "Wages.Settings_ZeroPointWage".Translate(), minWage, maxWage, 10, currentUnit, ref zeroPointWage, "Wages.Settings_ZeroPointWageTooltip".Translate());
            }
            else
            {

            }
            listMain.End();

        }

        public override void ExposeData()
        {
            base.ExposeData();

            // all scribes...
            Scribe_Values.Look(ref maxWage, "maxWage", 200);
            Scribe_Values.Look(ref minWage, "minWage", 0);
            Scribe_Values.Look(ref zeroPointWage, "zeroPointWage", 10);
            Scribe_Values.Look(ref fixedIncome, "fixedIncome", false);
            Scribe_Values.Look(ref owedWageIgnoredBeforeGathering, "owedWageIgnoredBeforeGathering", 50);
            Scribe_Values.Look(ref owedWageIgnoredBeforeMoodlet, "owedWageIgnoredBeforeMoodlet", 500);
            Scribe_Values.Look(ref hideExemptBehindDevMode, "hideExemptBehindDevMode", true);
            Scribe_Defs.Look(ref currencyDef, "currencyDef");
            Scribe_Values.Look(ref exemptChildren, "exemptChildren", true);
            Scribe_Values.Look(ref exemptSlaves, "exemptSlaves", true);
            Scribe_Values.Look(ref exemptColonists, "exemptColonists", false);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Log.Debug("ModSettings_RennOrganisms.ExposeData() post load init called");
                if (currencyDef == null)
                {
                    currencyDef = ThingDefOf.Silver;
                }
            }

        }

        public static void LabeledScrollbar(Listing_Standard listing_Standard, string label, int min, int max, int steps, string unit, ref int setting, string tooltip = null)
        {
            Rect rect = listing_Standard.GetRect(Text.LineHeight).Rounded();
            Rect SliderOffset = rect.RightHalf().Rounded().RightPartPixels(400);
            Widgets.Label(rect, label + ": " + setting + " " + unit);
            var settingUnrounded = Widgets.HorizontalSlider(
            SliderOffset,
            setting, min, max, true);
            setting = (int)(Math.Round(settingUnrounded / (double)steps, 0) * steps);
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(rect))
                {
                    Widgets.DrawHighlight(rect);
                }
                TooltipHandler.TipRegion(rect, tooltip);
            }
        }
    }
}
