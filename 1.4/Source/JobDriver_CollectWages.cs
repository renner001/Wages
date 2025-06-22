using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DanielRenner.Wages
{
    class JobDriver_CollectWages : JobDriver
	{
		private const TargetIndex CollectibleIndex = TargetIndex.A;
		public const int CollectingDuration = 80;

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.GetTarget(CollectibleIndex).Thing, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedNullOrForbidden(CollectibleIndex);
			//AddEndCondition(() => (!RefuelableComp.IsFull) ? JobCondition.Ongoing : JobCondition.Succeeded);
			//AddFailCondition(() => !job.playerForced && !RefuelableComp.ShouldAutoRefuelNowIgnoringFuelPct);
			//AddFailCondition(() => !RefuelableComp.allowAutoRefuel && !job.playerForced);

			yield return Toils_General.DoAtomic(delegate
			{
				Log.Debug("first toil of gathering wages called");
				job.count = getItemCountToFullyPay(pawn, ThingDefOf.Silver);
			});
			Toil reserveCollectible = Toils_Reserve.Reserve(CollectibleIndex);
			yield return reserveCollectible;
			yield return Toils_Goto.GotoThing(CollectibleIndex, PathEndMode.ClosestTouch).FailOnDespawnedNullOrForbidden(CollectibleIndex).FailOnSomeonePhysicallyInteracting(CollectibleIndex);
			//yield return Toils_Haul.StartCarryThing(CollectibleIndex, putRemainderInQueue: false, subtractNumTakenFromJobCount: true);
			//yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveCollectible, CollectibleIndex, TargetIndex.None, takeFromValidStorage: true);
			//yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(CollectingDuration).WithProgressBarToilDelay(CollectibleIndex);
			yield return Toils_General.DoAtomic(delegate
			{
				Log.Debug("last toil of gathering wages called");
				Thing collectible = job.GetTarget(CollectibleIndex).Thing;
				int numItemsToFullyPay = getItemCountToFullyPay(pawn, collectible.def);
				int numItemsToSplitOff = Mathf.Min(numItemsToFullyPay, collectible.stackCount);
				Log.Debug("crediting " + numItemsToSplitOff + " of a total " + collectible.stackCount + " " + collectible.def);
				creditWage(pawn, collectible.def, numItemsToSplitOff); // credit the worth of the items
				collectible.SplitOff(numItemsToSplitOff).Destroy(); // destroy the items
			});
			yield break;
			// FinalizeCollectWages(pawn, CollectibleIndex);
		}

		/*
		public static Toil FinalizeCollectWages(Pawn pawn, TargetIndex collectibleInd)
		{
			Log.Debug("last toil of gathering wages called");
			Toil toil = new Toil();
			toil.initAction = delegate
			{
				Job curJob = toil.actor.CurJob;
				Thing collectible = curJob.GetTarget(collectibleInd).Thing;
				int numItemsToFullyPay = getItemCountToFullyPay(pawn, collectible.def);
				int numItemsToSplitOff = Mathf.Min(numItemsToFullyPay, collectible.stackCount);
				Log.Debug("crediting " + numItemsToSplitOff + " of a total " + collectible.stackCount + " " + collectible.def);
				creditWage(pawn, collectible.def, numItemsToSplitOff); // credit the worth of the items
				collectible.SplitOff(numItemsToSplitOff).Destroy(); // destroy the items
			};
			toil.defaultCompleteMode = ToilCompleteMode.Instant;
			return toil;
		}
		*/

		private static int getItemCountToFullyPay(Pawn pawn, ThingDef itemType)
        {
			var valuePerItem = itemType.BaseMarketValue;
			var owedWages = pawn.records.GetValue(DefOfs_Wages.TotalWagesOwed);
			var collectedWages = pawn.records.GetValue(DefOfs_Wages.TotalWagesCollected);
			var requiredItemCount = (int)Math.Ceiling(((float)owedWages - collectedWages) / valuePerItem);
			return requiredItemCount;
		}

		private static void creditWage(Pawn pawn, ThingDef itemType, int count)
        {
			var valuePerItem = itemType.BaseMarketValue;
			var totalWorth = count * valuePerItem;
			Log.Debug("adding " + totalWorth + " worth to pawn " + pawn);
			pawn.records.AddTo(DefOfs_Wages.TotalWagesCollected, count* valuePerItem);
		}
	}
}
