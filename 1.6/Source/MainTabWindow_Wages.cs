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
        Pawn[] allPawns;
        float availableTreasury;
        float creditWithPawns;
        int ticksUntilNextPayday;

        public MainTabWindow_Wages()
        { }

        public override void PreOpen()
        {
            Log.Debug("MainTabWindow_Wages.PreOpen() called");
            base.PreOpen();
            // get the pawn list; we do this now before updating the wage cache to make sure we don't use pawns in this panel that joined after opening the panel
            allPawns = PayHelperUtility.AllValidPawnsIncludingExempt.ToArray();
            // update the wage steps
            PayHelperUtility.CalcWageSteps();
            PayHelperUtility.FixWrongWages();
            wageSettingsPerPawn = new Dictionary<Pawn, int>();
            creditWithPawns = 0;
            foreach (var pawn in allPawns)
            {
                wageSettingsPerPawn[pawn] = PayHelperUtility.GetCurrentWage(pawn);
                creditWithPawns += PayHelperUtility.GetOwedCredit(pawn);
            }
            availableTreasury = PayHelperUtility.AvailableFunds;
        }

        public override void PreClose()
        {
            Log.Debug("MainTabWindow_Wages.PreClose() called");
            base.PreClose();
            // set the new wages
            foreach (var pawn in wageSettingsPerPawn.Keys)
            {
                PayHelperUtility.SetCurrentWage(pawn, wageSettingsPerPawn[pawn]);
            }
        }

        public override void DoWindowContents(Rect canvas)
        {
            // refresh the next payday live
            ticksUntilNextPayday = PayHelperUtility.NextPaydayTick - Find.TickManager.TicksGame;
            // sometimes refresh owed wages
            if (Find.TickManager.TicksGame % 60 == 0)
            {
                creditWithPawns = 0;
                foreach (var pawn in allPawns)
                    creditWithPawns += PayHelperUtility.GetOwedCredit(pawn);
            }
            
            // setup the font and don't expect it to be right
            Text.Font = GameFont.Small;

            var remainingSpace = canvas;

            // --- 1. Draw Summary Top ---
            const int entriesInSummary = 4;
            const int gapHeightperSummaryEntry = 3;
            const int summaryBorder = 10;
            var summaryHeight = 2 * summaryBorder + entriesInSummary * Text.LineHeight + (entriesInSummary - 1) * gapHeightperSummaryEntry;

            var summaryRect = remainingSpace.TopPartPixels(summaryHeight);
            remainingSpace.y += summaryHeight + 10f; // Added 10px gap between summary and table
            remainingSpace.height -= summaryHeight + 10f;

            Widgets.DrawMenuSection(summaryRect); // background coloring for beauty
            var innerSummaryRect = summaryRect.ContractedBy(summaryBorder);

            // Custom drawing to optimize spacing instead of relying on Listing_Standard's 50/50 split
            float curY = innerSummaryRect.y;
            void DrawSummaryLine(string label, string value, string tooltip)
            {
                Rect lineRect = new Rect(innerSummaryRect.x, curY, innerSummaryRect.width, Text.LineHeight);
                if (Mouse.IsOver(lineRect))
                {
                    Widgets.DrawHighlight(lineRect);
                    TooltipHandler.TipRegion(lineRect, tooltip);
                }
                Widgets.Label(new Rect(lineRect.x, lineRect.y, 220f, lineRect.height), label); // Tight 220px label width
                Widgets.Label(new Rect(lineRect.x + 220f, lineRect.y, lineRect.width - 220f, lineRect.height), value);
                curY += Text.LineHeight + gapHeightperSummaryEntry;
            }

            var sumOfWage = wageSettingsPerPawn.Select(perPawn => { return PayHelperUtility.IsWageExempt(perPawn.Key) ? 0 : perPawn.Value; }).Sum();
            var wageSummaryText = ((float)sumOfWage).ToStringMoney();
            DrawSummaryLine("Wages.MainWindow_TotalWageLabel".Translate(), wageSummaryText, "Wages.MainWindow_TotalWageTooltip".Translate());
            DrawSummaryLine("Wages.MainWindow_Treasury".Translate(), availableTreasury.ToStringMoney(), "Wages.MainWindow_TreasuryTooltip".Translate());
            DrawSummaryLine("Wages.MainWindow_PayDayLabel".Translate(), ticksUntilNextPayday.ToStringTicksToPeriod(), "Wages.MainWindow_PayDayTooltip".Translate());
            DrawSummaryLine("Wages.MainWindow_CreditLabel".Translate(), creditWithPawns.ToStringMoney(), "Wages.MainWindow_CreditTooltip".Translate());


            // --- 2. Calculate Strict Column Widths ---
            bool showExemptColumn = !ModSettings_Wages.hideExemptBehindDevMode || DebugSettings.godMode;

            float contentWidth = canvas.width - 20f; // Account for scrollbar
            float exemptColWidth = showExemptColumn ? 60f : 0f;
            float pawnColWidth = 180f;
            float valuesColWidth = 90f; // Far right stats
            float sliderColWidth = contentWidth - exemptColWidth - pawnColWidth - valuesColWidth;


            // --- 3. Draw Header Row ---
            const float headerHeight = 35;
            var tableHeaderRect = new Rect(remainingSpace.x, remainingSpace.y, contentWidth, headerHeight);
            remainingSpace.y += headerHeight;
            remainingSpace.height -= headerHeight;

            float currentHeaderX = tableHeaderRect.x;

            // Exempt Header
            if (showExemptColumn)
            {
                Rect topRowExempt = new Rect(currentHeaderX, tableHeaderRect.y, exemptColWidth, headerHeight);
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(topRowExempt, "Wages.Exempt".Translate());
                if (Mouse.IsOver(topRowExempt)) GUI.DrawTexture(topRowExempt, TexUI.HighlightTex);
                TooltipHandler.TipRegion(topRowExempt, "Wages.ToolTipExempt".Translate());
                currentHeaderX += exemptColWidth;
            }

            // Pawn Header (Offset by iconWidth so it aligns perfectly over the names)
            Rect topRowPawn = new Rect(currentHeaderX + iconWidth, tableHeaderRect.y, pawnColWidth - iconWidth, headerHeight);
            Text.Anchor = TextAnchor.UpperLeft;
            Widgets.Label(topRowPawn, "Wages.Pawn".Translate());
            if (Mouse.IsOver(new Rect(currentHeaderX, tableHeaderRect.y, pawnColWidth, headerHeight))) GUI.DrawTexture(new Rect(currentHeaderX, tableHeaderRect.y, pawnColWidth, headerHeight), TexUI.HighlightTex);
            currentHeaderX += pawnColWidth;

            // Slider Header
            Rect topRowSlider = new Rect(currentHeaderX, tableHeaderRect.y, sliderColWidth, headerHeight);
            Widgets.Label(topRowSlider, "Wages.WageSliderLabel".Translate());
            if (Mouse.IsOver(topRowSlider)) GUI.DrawTexture(topRowSlider, TexUI.HighlightTex);

            Widgets.DrawLineHorizontal(tableHeaderRect.x, tableHeaderRect.yMax, tableHeaderRect.width);
            Text.Anchor = TextAnchor.UpperLeft;


            // --- 4. Setup Scroll View ---
            var pawnEntryHeight = 2 * Text.LineHeight;
            var estimatedContentsHeight = pawnEntryHeight * allPawns.Length;
            var scrollView = new Rect(0, 0, contentWidth, estimatedContentsHeight);

            Widgets.BeginScrollView(remainingSpace, ref scrollPosition, scrollView, true);

            float offsetCurrRow = 0;
            int numEntry = 0;

            foreach (var pawn in allPawns)
            {
                var row = new Rect(0f, offsetCurrRow, scrollView.width, pawnEntryHeight);
                offsetCurrRow += pawnEntryHeight;

                if (numEntry % 2 == 1) Widgets.DrawAltRect(row);
                numEntry += 1;

                bool isExempt = PayHelperUtility.IsWageExempt(pawn);
                float rowCurrentX = 0f;

                // Column 1: Exempt Checkbox
                if (showExemptColumn)
                {
                    Rect exemptRect = new Rect(rowCurrentX, row.y, exemptColWidth, row.height);
                    bool toggleExempt = isExempt;
                    var checkboxPos = new Vector2(exemptRect.x + (exemptRect.width - 24f) / 2f, exemptRect.y + (exemptRect.height - 24f) / 2f);
                    Widgets.Checkbox(checkboxPos, ref toggleExempt);

                    if (toggleExempt != isExempt)
                    {
                        PayHelperUtility.SetWageExempt(pawn, toggleExempt);
                        isExempt = toggleExempt;
                        PayHelperUtility.CalcWageSteps();
                    }
                    rowCurrentX += exemptColWidth;
                }

                // Column 2: Pawn Icon & Name
                Rect pawnAreaRect = new Rect(rowCurrentX, row.y, pawnColWidth, row.height);
                if (Mouse.IsOver(pawnAreaRect)) GUI.DrawTexture(pawnAreaRect, TexUI.HighlightTex);

                Rect iconRect = new Rect(pawnAreaRect.x, pawnAreaRect.y, iconWidth, pawnAreaRect.height);
                Widgets.ThingIcon(iconRect, pawn);

                Rect nameRect = new Rect(iconRect.xMax, pawnAreaRect.y, pawnAreaRect.width - iconWidth, pawnAreaRect.height);
                Widgets.Label(nameRect, pawn.NameFullColored);

                rowCurrentX += pawnColWidth;

                // Column 3: The Slider
                Rect sliderRect = new Rect(rowCurrentX, row.y, sliderColWidth, row.height);
                rowCurrentX += sliderColWidth;

                if (isExempt)
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    GUI.color = Color.gray;
                    Widgets.Label(sliderRect, "Wages.PawnExempt".Translate());
                    GUI.color = Color.white;
                    Text.Anchor = TextAnchor.UpperLeft;
                }
                else
                {
                    var moodSteps = PayHelperUtility.MoodStepCache[pawn];

                    float vanillaSliderMargin = 8f;
                    float trackWidth = sliderRect.width - (vanillaSliderMargin * 2) - 10f; // -10f for the Scrollbar offset
                    float trackStartX = sliderRect.x + vanillaSliderMargin;
                    float silverTotalRange = PayHelperUtility.MaxWage - PayHelperUtility.MinWage;

                    float blockHeight = 12f;
                    float blockY = sliderRect.y + (sliderRect.height - blockHeight) / 2f;
                    float gapPixels = 4f;

                    for (int i = 0; i < moodSteps.Length; i++)
                    {
                        float startWage = moodSteps[i].Key;
                        float endWage = (i + 1 < moodSteps.Length) ? moodSteps[i + 1].Key : PayHelperUtility.MaxWage;

                        float startPct = (startWage - PayHelperUtility.MinWage) / silverTotalRange;
                        float endPct = (endWage - PayHelperUtility.MinWage) / silverTotalRange;

                        float blockStartX = trackStartX + (startPct * trackWidth);
                        float blockEndX = trackStartX + (endPct * trackWidth);

                        int actualMoodOffset = PayHelperUtility.MoodFromWage(pawn, (int)startWage);

                        Color currentColor;
                        if (actualMoodOffset == 0)
                            currentColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
                        else if (actualMoodOffset < 0)
                        {
                            float intensity = Mathf.Clamp01(Mathf.Abs(actualMoodOffset) / 15f);
                            currentColor = Color.Lerp(new Color(0.8f, 0.3f, 0.3f, 0.3f), new Color(1f, 0.1f, 0.1f, 0.8f), intensity);
                        }
                        else
                        {
                            float intensity = Mathf.Clamp01(actualMoodOffset / 15f);
                            currentColor = Color.Lerp(new Color(0.3f, 0.8f, 0.3f, 0.3f), new Color(0.1f, 1f, 0.1f, 0.8f), intensity);
                        }

                        float blockWidth = (blockEndX - blockStartX);
                        if (i < moodSteps.Length - 1) blockWidth -= gapPixels;

                        Rect solidRect = new Rect(blockStartX, blockY, Mathf.Max(0, blockWidth), blockHeight);
                        Widgets.DrawRectFast(solidRect, currentColor);
                    }

                    wageSettingsPerPawn[pawn] = Scrollbar(sliderRect, PayHelperUtility.MinWage, PayHelperUtility.MaxWage, 1, wageSettingsPerPawn[pawn], "Wages.TooltipWagesReceived".Translate() + " " + pawn.Name + ": " + wageSettingsPerPawn[pawn]);
                }

                // Column 4: Far Right Values (Wage & Mood)
                Rect valuesRect = new Rect(rowCurrentX, row.y, valuesColWidth, row.height);

                GUI.color = isExempt ? Color.gray : Color.white; // Fade out text/icons if exempt

                int displayWage = isExempt ? 0 : wageSettingsPerPawn[pawn];
                string wageText = ((float)displayWage).ToStringMoney();

                int actualMood = isExempt ? 0 : PayHelperUtility.MoodFromWage(pawn, wageSettingsPerPawn[pawn]);
                string moodText = isExempt ? "-" : (actualMood >= 0 ? "+" + actualMood : actualMood.ToString());

                // Wage rendering
                var valuesSilverRect = valuesRect.TopHalf();
                var valuesSilverIconRect = valuesSilverRect.LeftPartPixels(Text.LineHeight);
                Widgets.ThingIcon(valuesSilverIconRect, ModSettings_Wages.currencyDef);
                Rect textWageRect = new Rect(valuesSilverIconRect.xMax + 5f, valuesSilverRect.y, valuesSilverRect.width - Text.LineHeight - 5f, valuesSilverRect.height);
                Widgets.Label(textWageRect, wageText);

                // Mood rendering
                var valuesMoodRect = valuesRect.BottomHalf();
                var valuesMoodIconRect = valuesMoodRect.LeftPartPixels(Text.LineHeight);
                Textures_Wages.DrawIcon(valuesMoodIconRect, Textures_Wages.MoodIcon, 1.0f);
                Rect textMoodRect = new Rect(valuesMoodIconRect.xMax + 5f, valuesMoodRect.y, valuesMoodRect.width - Text.LineHeight - 5f, valuesMoodRect.height);
                Widgets.Label(textMoodRect, moodText);

                GUI.color = Color.white; // Always reset GUI color!
            }

            Widgets.EndScrollView();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public static int Scrollbar(Rect drawIn, int min, int max, int steps, int settingIn, string tooltip = null)
        {
            Rect SliderOffset = drawIn.LeftPartPixels(drawIn.width - 10);
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