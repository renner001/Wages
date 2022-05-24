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
        public static int owedWageIgnoredBeforeGathering = 50;
        public static int owedWageIgnoredBeforeMoodlet = 500;

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
            //listMain.CheckboxLabeled(Translations_RennOrganisms.EnableResearchGlobalWorkSpeed, ref ModSettings_RennOrganisms.enabledResearchGlobalWorkSpeed, Translations_RennOrganisms.EnableResearchGlobalWorkSpeedTooltip);
            LabeledScrollbar(listMain, "Minimum Wage (in silver every 5 days)", 0, maxWage, 10, "silver", ref minWage, "The smallest wage that can be paid to a pawn. This wage will result in the highest mood penalty, if it is below the Zero Mood Point.");
            LabeledScrollbar(listMain, "Maximum Wage (in silver every 5 days)", minWage, 1000, 10, "silver", ref maxWage, "The highest wage that can be paid to a pawn. This wage will result in the highest mood boost.");
            if (zeroPointWage > maxWage)
            {
                zeroPointWage = maxWage;
            }
            if (zeroPointWage < minWage)
            {
                zeroPointWage = minWage;
            }
            LabeledScrollbar(listMain, "Zero Mood Point (in silver every 5 days)", minWage, maxWage, 10, "silver", ref zeroPointWage, "The wage at which no mood boost or penalty is applied. Paying this wage will cause no mood effect. Going above will increase the mood. Going below will reduce the mood.");
            LabeledScrollbar(listMain, "Colonists start collecting wages at x silver owed", 0, 2000, 20, "silver", ref owedWageIgnoredBeforeGathering, "Colonists will not collect wages immediately to avoid running for a single silver. Only if they are owed more than this threshold will they start collecting.");
            LabeledScrollbar(listMain, "Colonists feel cheated starting at x silver owed", 0, 4000, 40, "silver", ref owedWageIgnoredBeforeMoodlet, "If colonists try to collect their wages and can't despite already being owed this amount will they feel cheated applying a mood penalty until they are paid.");
            listMain.End();

            //Rect fullWidthSettingsRect = mainRect.TopPartPixels()
            //Rect leftRect = mainRect.LeftHalf().Rounded();
            //Rect rightRect = mainRect.RightHalf().Rounded();

            //Listing_Standard listLeft = new Listing_Standard()
            //{
            //    ColumnWidth = leftRect.width,
            //};

            //listLeft.Begin(leftRect);
            //listLeft.CheckboxLabeled(Translations_RennOrganisms.EnableResearchGlobalWorkSpeed, ref ModSettings_RennOrganisms.enabledResearchGlobalWorkSpeed, Translations_RennOrganisms.EnableResearchGlobalWorkSpeedTooltip);
            //listLeft.End();

        }

        public override void ExposeData()
        {
            base.ExposeData();

            // all scribes...
            Scribe_Values.Look(ref maxWage, "maxWage", 200);
            Scribe_Values.Look(ref minWage, "minWage", 0);
            Scribe_Values.Look(ref zeroPointWage, "zeroPointWage", 10);
            Scribe_Values.Look(ref owedWageIgnoredBeforeGathering, "owedWageIgnoredBeforeGathering", 50);
            Scribe_Values.Look(ref owedWageIgnoredBeforeMoodlet, "owedWageIgnoredBeforeMoodlet", 500);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Log.Debug("ModSettings_RennOrganisms.ExposeData() post load init called");
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
