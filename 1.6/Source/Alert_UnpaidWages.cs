using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace DanielRenner.Wages
{
    public class Alert_UnpaidWages : Alert_Critical
    {
        public Alert_UnpaidWages()
        {
            // Translate the label directly in the constructor
            defaultLabel = "Wages.Alert_UnpaidWages_Label".Translate();
        }

        private IEnumerable<Pawn> UnpaidPawns
        {
            get
            {
                foreach (Pawn pawn in PayHelperUtility.AllValidPawnsIncludingExempt)
                {
                    if (PayHelperUtility.IsWageExempt(pawn)) continue;
                    if (PayHelperUtility.GetOwedCredit(pawn) > ModSettings_Wages.owedWageIgnoredBeforeMoodlet)
                        yield return pawn;
                }
            }
        }

        public override AlertReport GetReport()
        {
            var pawns = UnpaidPawns;
            if (!pawns.Any())
                return false;

            return AlertReport.CulpritsAre(pawns.ToList());
        }

        public override TaggedString GetExplanation()
        {
            // StringBuilder is much more performant for UI elements that redraw frequently
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Wages.Alert_UnpaidWages_Explanation".Translate());
            sb.AppendLine(); // Adds the empty line

            foreach (Pawn p in UnpaidPawns)
            {
                int owed = PayHelperUtility.GetOwedCredit(p);
                string currencyName = ModSettings_Wages.currencyDef.label;

                // {0} = Pawn Name, {1} = Money Amount, {2} = Currency Label
                sb.AppendLine("Wages.Alert_UnpaidWages_LineItem".Translate(p.NameShortColored, ((float)owed).ToStringMoney(), currencyName));
            }

            return sb.ToString().TrimEnd();
        }
    }
}