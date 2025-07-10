using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace IconianPsycasts
{
    public class JobGiver_FollowEnemy : ThinkNode_JobGiver
    {
        public float maxDistance;
        public PathEndMode PathEndMode => PathEndMode.Touch;

        public bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            if (!(t is Pawn p) || !p.AnimalOrWildMan() || !p.HostileTo(pawn.Faction) || p.DeadOrDowned)
            {
                return false;
            }
            return true;
        }
        protected override Job TryGiveJob(Pawn pawn)
        {

            Predicate<Thing> validator = (Thing x) => HasJobOnThing(pawn, x);
            Pawn p = (Pawn)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.Pawn), PathEndMode, TraverseParms.For(pawn, Danger.Deadly), maxDistance, validator);
            if (p == null)
            {
                return null;
            }
            if (!pawn.CanReach(p, PathEndMode.Touch, Danger.Deadly))
            {
                return null;
            }
            if (!JobDriver_FollowClose.FarEnoughAndPossibleToStartJob(pawn, p, maxDistance))
            {
                return null;
            }
            Job job = JobMaker.MakeJob(JobDefOf.FollowClose, p);
            job.expiryInterval = 120;
            job.checkOverrideOnExpire = true;
            job.followRadius = 0;
            return job;
        }
    }
}
