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
                base.GetSettings<ModSettings_Wages>();
            });
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }


        public override string SettingsCategory()
        {
            return Translations_Wages.Static.SettingsPanelName;
        }


        public override void DoSettingsWindowContents(Rect rect)
        {
            // we will put the rendering code into the settings class - where it belongs...
            ModSettings_Wages.DoSettingsWindowContents(rect);
        }
    }
}
