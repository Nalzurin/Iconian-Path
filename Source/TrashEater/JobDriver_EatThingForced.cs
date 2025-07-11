using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse.AI;
using Verse;

namespace IconianPsycasts
{
    internal class JobDriver_EatThingForced : JobDriver
    {

        public Thing ThingToCarry => (Thing)job.GetTarget(TargetIndex.A);

        public Thing Container => (Thing)job.GetTarget(TargetIndex.B);

        public ThingDef ThingDef => ThingToCarry.def;

        public int Duration = 240;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            pawn.ReserveAsManyAsPossible(job.GetTargetQueue(TargetIndex.A), job);
            return true;
        }

        CompTrashEater comp => Container.TryGetComp<CompTrashEater>();

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOn(delegate
            {
                Thing thing = GetActor().jobs.curJob.GetTarget(TargetIndex.B).Thing;
                Thing thing2 = GetActor().jobs.curJob.GetTarget(TargetIndex.C).Thing;
                if (thing == null)
                {
                    return true;
                }
                if (thing2 != null && thing2.Destroyed)
                {
                    job.SetTarget(TargetIndex.C, null);
                }
                if (!thing.Spawned || (thing.Destroyed))
                {
                    if (job.targetQueueB.NullOrEmpty())
                    {
                        return true;
                    }
                    if (!Toils_Haul.TryGetNextDestinationFromQueue(TargetIndex.C, TargetIndex.B, ThingDef, job, pawn, out var nextTarget))
                    {
                        return true;
                    }
                    job.targetQueueB.RemoveAll((LocalTargetInfo target) => target.Thing == nextTarget);
                    job.targetB = nextTarget;
                }
                ThingOwner thingOwner = Container.TryGetInnerInteractableThingOwner();
                if (thingOwner != null && !thingOwner.CanAcceptAnyOf(ThingToCarry))
                {
                    return true;
                }

                return (Container is IHaulDestination haulDestination && !haulDestination.Accepts(ThingToCarry)) ? true : false;
            });
            this.FailOnForbidden(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.B);
            this.FailOn(() => EnterPortalUtility.WasLoadingCanceled(Container));
            this.FailOn(() => TransporterUtility.WasLoadingCanceled(Container));
            this.FailOn(() => comp.currentStored >= comp.maxStored);




            Toil getToHaulTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, canGotoSpawnedParent: true);



            Toil startCarryingThing = Toils_Haul.StartCarryThing(TargetIndex.A, putRemainderInQueue: false, subtractNumTakenFromJobCount: true, failIfStackCountLessThanJobCount: false, reserve: true, canTakeFromInventory: true);
            //Toil carryToContainer = Toils_Haul.CarryHauledThingToContainer();
            startCarryingThing.AddFinishAction(delegate
            {
                if (comp != null)
                {
                    if (Container.PositionHeld == ThingToCarry.PositionHeld)
                    {
                        if (!ThingToCarry.IsForbidden(pawn.Faction) && comp.currentStored >= comp.maxStored)
                        {
                            comp.currentStored += ThingToCarry.stackCount;
                            ThingToCarry.Destroy();
                            Duration = 240;

                        }
                        else
                        {
                            Duration = 1;
                        }
                    }
                }
            });

            yield return getToHaulTarget;
            yield return startCarryingThing;
            //yield return carryToContainer;
            Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
            toil.WithProgressBarToilDelay(TargetIndex.B);

            yield return toil;


            //yield return Toils_Haul.DepositHauledThingInContainer(TargetIndex.B, TargetIndex.C);
        }

    }
}
