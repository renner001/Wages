using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DanielRenner.RepeatableResearch
{
    public enum FeedOptions { RennFiber, RennFiberDomestic };

    class Building_RennPond : Building_RennBase
    {
        // saved variables
        public FeedOptions feedOptionSelected = FeedOptions.RennFiber;
        public FeedOptions lastFed = FeedOptions.RennFiber;

        public override void PostMake()
        {
            base.PostMake();
            var comp = GetComp<CompRefuelable>();
            // todo!
        }

        protected override void ReceiveCompSignal(string signal)
        {
            Log.Debug("Building_RennPond.ReceiveCompSignal() called with signal '" + signal + "'");
            base.ReceiveCompSignal(signal);
            if (signal == "Refueled")
            {
                var refuelable = GetComp<Comp_SpecificRefuelable>();
                var lastRefueledWith = refuelable.lastRefueledWith;
                if (lastRefueledWith == DefOfs_RennOrganisms.RennFiber)
                {
                    Log.Debug("refueled with fuel '" + lastRefueledWith + "'");
                    lastFed = FeedOptions.RennFiber;
                }
                else if (lastRefueledWith == DefOfs_RennOrganisms.RennFiberDomestic)
                {
                    Log.Debug("refueled with fuel '" + lastRefueledWith + "'");
                    lastFed = FeedOptions.RennFiberDomestic;
                } 
                else
                {
                    Log.Error("refueled with invalid fuel '" + lastRefueledWith + "'");
                }
            }
        }

        public override void Tick()
        {
            base.Tick();
            if (Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                var refuelable = GetComp<CompRefuelable>();
                int moodEffect = 0;
                if (refuelable.HasFuel)
                {
                    switch (lastFed)
                    {
                        case FeedOptions.RennFiber:
                            moodEffect = 6;
                            break;
                        case FeedOptions.RennFiberDomestic:
                            moodEffect = 9;
                            break;
                    }
                }
                else
                {
                    moodEffect = 3;
                }
                SetThreatPoints(moodEffect, 1.0f - 0.04f * moodEffect, int.MaxValue);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref feedOptionSelected, "feedOptionSelected", FeedOptions.RennFiber);
            Scribe_Values.Look(ref lastFed, "lastFed", FeedOptions.RennFiber);
            updateRefuelComp(feedOptionSelected);
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            return base.GetGizmos().Concat(buildCodeGizmos());
        }

        private IEnumerable<Gizmo> buildCodeGizmos()
        {
            string labelName = (feedOptionSelected == FeedOptions.RennFiber ? DefOfs_RennOrganisms.RennFiber.label : DefOfs_RennOrganisms.RennFiberDomestic.label);
            var refuelModeCommand = new Command_Action()
            {
                icon = feedOptionSelected == FeedOptions.RennFiber ? Textures_RennOrganisms.RennFiber : Textures_RennOrganisms.RennFiberDomestic,
                defaultLabel = "Feed " + labelName,
                defaultDesc = "Feed the microbes " + labelName,
                hotKey = KeyBindingDefOf.Misc1,
                activateSound = SoundDefOf.Click,
                action = delegate ()
                {
                    Log.Debug("Building_RennPond.Feed was clicked");
                    Find.WindowStack.Add(MakeModeDropdown(this));
                },
            };
            yield return refuelModeCommand;
            // todo: rest?

            yield break;
        }

        public void updateRefuelComp(FeedOptions option)
        {
            feedOptionSelected = option;
            var refuelableComp = GetComp<CompRefuelable>();
            CompProperties_Refuelable props = refuelableComp.props as CompProperties_Refuelable;
            switch (option)
            {
                case FeedOptions.RennFiber:
                    Log.Debug("Building_RennPond: setting allowed fuels to RennFiber");
                    props.fuelFilter.SetAllow(DefOfs_RennOrganisms.RennFiber, true);
                    props.fuelFilter.SetAllow(DefOfs_RennOrganisms.RennFiberDomestic, false);
                    break;
                case FeedOptions.RennFiberDomestic:
                    Log.Debug("Building_RennPond: setting allowed fuels to RennFiberDomestic");
                    props.fuelFilter.SetAllow(DefOfs_RennOrganisms.RennFiber, false);
                    props.fuelFilter.SetAllow(DefOfs_RennOrganisms.RennFiberDomestic, true);
                    break;
            }
        }

        private FloatMenu MakeModeDropdown(Building_RennPond sender)
        {
            List<FloatMenuOption> floatMenu = new List<FloatMenuOption>();

            floatMenu.Add(new FloatMenuOption("Renn fibers", delegate ()
            {
                sender.updateRefuelComp(FeedOptions.RennFiber);
            }, MenuOptionPriority.Default, null, null, 0f, null, null));

            floatMenu.Add(new FloatMenuOption("Domestic renn fibers", delegate ()
            {
                sender.updateRefuelComp(FeedOptions.RennFiberDomestic);
            }, MenuOptionPriority.Default, null, null, 0f, null, null));

            return new FloatMenu(floatMenu);
        }
    }
}
