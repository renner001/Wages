using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace DanielRenner.Wages
{
    [StaticConstructorOnStartup]
    public class Mod_Wages : Mod
    {
        static Mod_Wages()
        {
            Verse.Log.Message("Mod 'Wages': loaded");
#if DEBUG
            Harmony.DEBUG = true;
#endif
            Harmony harmony = new Harmony("DanielRenner.Wages");
            harmony.PatchAll();
        }

        public Mod_Wages(ModContentPack mcp) : base(mcp)
        {
            LongEventHandler.ExecuteWhenFinished(() => { 
				base.GetSettings<ModSettings_RepeatableResearch>(); 
			});
        }

		public override void WriteSettings()
		{
			base.WriteSettings();
		}


		public override string SettingsCategory()
		{
			return Translations_RepeatableResearch.SettingsPanelName;
		}


		public override void DoSettingsWindowContents(Rect rect)
		{

			Rect descriptionRect = rect.TopPartPixels(Text.CalcHeight(Translations_RepeatableResearch.SettingsPanelChangeSettingsEffect, rect.width));
			Rect mainRect = rect.BottomPartPixels(rect.height - descriptionRect.height - 50);
			Widgets.Label(descriptionRect, Translations_RepeatableResearch.SettingsPanelChangeSettingsEffect);


			Rect leftRect = mainRect.LeftHalf().Rounded();
			Rect rightRect = mainRect.RightHalf().Rounded();

			Listing_Standard listLeft = new Listing_Standard()
			{
				ColumnWidth = leftRect.width,
			};

			listLeft.Begin(leftRect);
			listLeft.CheckboxLabeled(Translations_RepeatableResearch.EnableResearchGlobalWorkSpeed, ref ModSettings_RepeatableResearch.enabledResearchGlobalWorkSpeed, Translations_RepeatableResearch.EnableResearchGlobalWorkSpeedTooltip);
			listLeft.End();
		}
	}
}
