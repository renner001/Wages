using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse;
using RimWorld;

namespace DanielRenner.Wages
{
    public class JobDriver_WageDestruction : JobDriver
    {
        private Thing TargetItem => job.GetTarget(TargetIndex.A).Thing;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            // Reserve the item so no one hauls it away while they are walking towards it
            return pawn.Reserve(TargetItem, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);

            // 1. Walk to the target
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);

            // 2. The Smashing Phase (4 seconds / 240 ticks)
            Toil smashToil = Toils_General.Wait(240);
            smashToil.WithProgressBarToilDelay(TargetIndex.A);
            smashToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            smashToil.tickAction = delegate
            {
                pawn.rotationTracker.FaceTarget(TargetItem);
            };
            yield return smashToil;

            // 3. The Destruction and Math Phase
            yield return new Toil
            {
                initAction = delegate
                {
                    if (TargetItem != null && !TargetItem.Destroyed)
                    {
                        float itemValue = TargetItem.MarketValue * TargetItem.stackCount;

                        // reduce debt
                        const float creditedValuePercent = 0.75f;
                        PayHelperUtility.AddWage(pawn, (int)(itemValue * creditedValuePercent));

                        // Send a nasty message to the player
                        Messages.Message($"{pawn.NameShortColored} repurposed {TargetItem.Label} to settle their unpaid wages!", pawn, MessageTypeDefOf.NegativeEvent);

                        // Poof! Item is gone.
                        TargetItem.Destroy(DestroyMode.Vanish);

                        // End the mental state gracefully
                        pawn.mindState.mentalStateHandler.CurState.RecoverFromState();
                    }
                },
                defaultCompleteMode = ToilCompleteMode.Instant
            };
        }
    }
}
