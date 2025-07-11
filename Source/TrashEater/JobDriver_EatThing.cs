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
    internal class JobDriver_EatThing : JobDriver
    {
        public Thing ThingToEat => (Thing)job.GetTarget(TargetIndex.A);

        public ThingDef ThingDef => ThingToEat.def;

        public int Duration = 240;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            if (!pawn.Reserve(job.GetTarget(TargetIndex.A), job, 1, -1, null, errorOnFailed))
            {
                return false;
            }
            return true;
        }

        CompTrashEater comp => pawn.TryGetComp<CompTrashEater>();

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDestroyedOrNull(TargetIndex.A);
            this.FailOnForbidden(TargetIndex.A);
            this.FailOn(() => comp.maxToFill == 0);
            Toil getToEatTarget = Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch, canGotoSpawnedParent: true);
            Toil startEatingTarget = Toils_Haul.StartCarryThing(TargetIndex.A);
            startEatingTarget.AddFinishAction(delegate
            {
                if (comp != null)
                {

                    if (!ThingToEat.IsForbidden(pawn.Faction))
                    {
                        comp.currentStored += ThingToEat.stackCount;
                        ThingToEat.Destroy();
                        Duration = 240;

                    }
                    else
                    {
                        Duration = 1;
                    }

                }
            });
            yield return getToEatTarget;
            yield return startEatingTarget;
            Toil toil = Toils_General.Wait(Duration, TargetIndex.B);
            toil.WithProgressBarToilDelay(TargetIndex.B);

            yield return toil;

        }
    }
}
