using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.Wages
{
        public class Hediff_WageExempt : Hediff
        {
            // This overrides the base behavior to hide it from the Health tab UI
            public override bool Visible => false;
        }
}
