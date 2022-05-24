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
        public static int wagesMultipilerSettingsPercent = 100;

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
            LabeledScrollbar(listMain, "Adapt Mood Effects (%)", ref ModSettings_Wages.wagesMultipilerSettingsPercent, "Change the mood effects that this mod applies to your colonists. 50% would mean, that a normally applied mood effect of -10 mood would turn into a -5 mood effect.");
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
            Scribe_Values.Look(ref wagesMultipilerSettingsPercent, "moodMultipilerSettingsPercent", 100);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                Log.Debug("ModSettings_RennOrganisms.ExposeData() post load init called");
            }

        }

        public static void LabeledScrollbar(Listing_Standard listing_Standard, string label, ref int setting, string tooltip = null)
        {
            Rect rect = listing_Standard.GetRect(Text.LineHeight).Rounded();
            Rect SliderOffset = rect.RightHalf().Rounded().RightPartPixels(400);
            Widgets.Label(rect, label + ": " + setting + "%");
            var settingUnrounded = Widgets.HorizontalSlider(
            SliderOffset,
            setting, 0f, 200f, true);
            setting = (int)(Math.Round(settingUnrounded / (double)5, 0) * 5);
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
