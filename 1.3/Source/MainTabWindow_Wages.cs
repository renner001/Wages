using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DanielRenner.Wages
{
    public class MainTabWindow_Wages : MainTabWindow
    {
        const int iconWidth = 50;
        // temporary variables
        Vector2 scrollPosition;
        Dictionary<Pawn, int> wageSettingsPerPawn;
        List<Pawn> allPawns;

        int tempValueSilver = 100;

        public MainTabWindow_Wages()
        { }

        public override void PreOpen()
        {
            Log.Debug("MainTabWindow_Wages.PreOpen() called");
            base.PreOpen();
            // get the pawn list; we do this now before updating the wage cache to make sure we don't use pawns in this panel that joined after opening the panel
            allPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists_NoSlaves;
            // update the wage steps
            PayHelperUtility.CalcWageSteps();
            PayHelperUtility.FixWrongWages();
            wageSettingsPerPawn = new Dictionary<Pawn, int>();
            foreach(var pawn in allPawns)
            {
                var recordedValue = (int)pawn.records.GetValue(DefOfs_Wages.CurrentWage);
                wageSettingsPerPawn[pawn] = recordedValue;
            }
        }

        public override void PreClose()
        {
            Log.Debug("MainTabWindow_Wages.PreClose() called");
            base.PreClose();

            foreach (var pawn in wageSettingsPerPawn.Keys)
            {
                var recordedValue = pawn.records.GetValue(DefOfs_Wages.CurrentWage);
                var setValue = wageSettingsPerPawn[pawn];
                if (setValue != recordedValue)
                {
                    Log.Debug("setting new recorded wage from " + recordedValue + " for pawn " + pawn + " to " + setValue);
                    pawn.records.AddTo(DefOfs_Wages.CurrentWage, wageSettingsPerPawn[pawn] - pawn.records.GetValue(DefOfs_Wages.CurrentWage));
                }                
                    
            }
        }

        public override void DoWindowContents(Rect canvas)
        {
            // setup the font and don't expect it to be right
            Text.Font = GameFont.Small;

            // draw header row
            var topRow = canvas.TopPartPixels(35);
            topRow.width -= 20;
            var topRowIconAndNameAndValue = topRow.LeftPart(0.33f);
            //Widgets.DrawRectFast(topRowIconAndNameAndValue, Color.grey);
            if (Mouse.IsOver(topRowIconAndNameAndValue))
            {
                GUI.DrawTexture(topRowIconAndNameAndValue, TexUI.HighlightTex);
            }
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(topRowIconAndNameAndValue, "Pawn");

            var topRowSlider = topRow.RightPart(0.66f);
            //Widgets.DrawRectFast(topRowSlider, new Color(1f, 1f, 1f, 0.2f));
            if (Mouse.IsOver(topRowSlider))
            {
                GUI.DrawTexture(topRowSlider, TexUI.HighlightTex);
            }
            Widgets.Label(topRowSlider, "Wages every five days in silver");
            Widgets.DrawLineHorizontal(topRow.x, topRow.height + topRow.y, topRow.width);
            Text.Anchor = TextAnchor.UpperLeft;

            // draw the mood lines
            var oldColor = GUI.color;
            var moodSteps = PayHelperUtility.MoodStepCache;
            var pixelWidthTotalSlider = ((canvas.width - 20) * 0.66f) - 10 - 10; // 10 distance of the slider to the right end and 10 for 2xwidth of the slider bubble
            var silverTotalRange = PayHelperUtility.MaxWage - PayHelperUtility.MinWage;
            var baseOffsetLines = (canvas.width - 20) * 0.34f + 5;
            for (int i = 0; i < moodSteps.Length; i++)
            {
                var offsetThisLine = pixelWidthTotalSlider * moodSteps[i].Key / silverTotalRange;
                var textRect = topRow.BottomPartPixels(20);
                textRect.x = baseOffsetLines + offsetThisLine + 2;
                textRect.width = 50;
                if (moodSteps[i].Value == 0)
                {
                    GUI.color = Color.white;
                }
                else if (moodSteps[i].Value > 10)
                {
                    GUI.color = Color.red;
                }
                else
                {
                    GUI.color = Color.green;
                }
                Widgets.Label(textRect, PayHelperUtility.MoodFromMoodStage(moodSteps[i].Value).ToString());
                Widgets.DrawLineVertical(baseOffsetLines + offsetThisLine, 15, canvas.height - 15);

            }
            GUI.color = oldColor; // restore default colors

            // setup data for the table
            var pawnEntryHeight = 2 * Text.LineHeight;
            // lets estimate the required height by number of pawns
            var estimatedContentsHeight = pawnEntryHeight * allPawns.Count;
            var scrollView = new Rect(0, 0, canvas.width - 20, estimatedContentsHeight);
            // lets start with the scrollable list that will contain the pawns
            Widgets.BeginScrollView(canvas.BottomPartPixels(canvas.height - topRow.height), ref scrollPosition, scrollView, true);

            // render the pawns in a table
            float offsetCurrRow = 0;
            int numEntry = 0;

            foreach (var pawn in allPawns)
            {
                var row = new Rect(0f, offsetCurrRow, scrollView.width, pawnEntryHeight);
                offsetCurrRow += pawnEntryHeight;
                // background renderings
                if (numEntry % 2 == 1)
                {
                    Widgets.DrawAltRect(row);
                }
                numEntry += 1;

                var leftHalf = row.LeftPart(0.33f);
                if (Mouse.IsOver(leftHalf))
                {
                    GUI.DrawTexture(leftHalf, TexUI.HighlightTex);
                }
                var iconRect = leftHalf.LeftPartPixels(iconWidth);
                Widgets.ThingIcon(iconRect, pawn);
                var nameAndValueRect = leftHalf.RightPartPixels(leftHalf.width - iconWidth);
                Text.Anchor = TextAnchor.UpperLeft;
                Widgets.Label(nameAndValueRect, pawn.NameFullColored);
                Text.Anchor = TextAnchor.LowerRight;
                Widgets.Label(nameAndValueRect, wageSettingsPerPawn[pawn] + " silver -> mood=" + PayHelperUtility.MoodFromWage(wageSettingsPerPawn[pawn]));
                var rightHalf = row.RightPart(0.66f);
                wageSettingsPerPawn[pawn] = Scrollbar(rightHalf, PayHelperUtility.MinWage, PayHelperUtility.MaxWage, 1, wageSettingsPerPawn[pawn], "The wage that " + pawn.Name + " receives every five days is " + wageSettingsPerPawn[pawn] + ".");
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static int Scrollbar(Rect drawIn, int min, int max, int steps, int settingIn, string tooltip = null)
        {
            Rect SliderOffset = drawIn.LeftPartPixels(drawIn.width - 10);
            //Widgets.Label(drawIn, setting.ToString() + " silver");
            var settingUnrounded = Widgets.HorizontalSlider(
            SliderOffset,
            settingIn, min, max, true);
            if (!tooltip.NullOrEmpty())
            {
                if (Mouse.IsOver(drawIn))
                {
                    Widgets.DrawHighlight(drawIn);
                }
                TooltipHandler.TipRegion(drawIn, tooltip);
            }
            return (int)(Math.Round(settingUnrounded / (double)steps, 0) * steps);
        }

    }
}
