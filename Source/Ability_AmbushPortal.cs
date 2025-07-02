using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RimWorld.EffecterMaintainer;
using Verse;
using Ability = VEF.Abilities.Ability;
using RimWorld.Planet;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace IconianPsycasts
{
    public class Ability_AmbushPortal : Ability
    {

        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);

            if(targets[0].Pawn == null)
            {
                return;
            }
            Pawn target = targets[0].Pawn;
            IntVec3 cellToTeleport = Helper.GetCellToAmbushTeleport(this, target);

            Effecter portalEffecter = DefOfs.Iconian_TeleportEffect.Spawn(cellToTeleport, pawn.Map, new Vector3(0, 3, 0));
            AddEffecterToMaintain(portalEffecter, cellToTeleport, 180, pawn.Map);
            Effecter portalEffecter2 = DefOfs.Iconian_TeleportEffect.Spawn(pawn.Position, pawn.Map, new Vector3(0, 3, 0));
            AddEffecterToMaintain(portalEffecter2, pawn.Position, 180, pawn.Map);
            SoundDefOf.Psycast_Skip_Entry.PlayOneShot(pawn);
            SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(target.Position, target.Map));
            foreach (SubEffecter_Sprayer sub in portalEffecter2.children.OfType<SubEffecter_Sprayer>())
            {
                if (sub.mote != null)
                {
                    sub.mote.yOffset -= 6.2f;
                }
            }
            pawn.Position = cellToTeleport;
            pawn.Notify_Teleported();
            float sens = pawn.GetStatValue(StatDefOf.PsychicSensitivity);

            if (target.HostileTo(pawn))
            {
                Helper.ApplyStun(target, sens);
                Helper.ApplyAttackSpeedHediff(pawn, sens);
            }
            else
            {
                Helper.ApplySpeedHediff(pawn, sens);
                Helper.ApplySpeedHediff(target, sens);
            }
            
        }
        public override void Tick()
        {
            base.Tick();
            if (!maintainedEffecters.Any())
            {
                return;
            }
            AdjustPos(maintainedEffecters[0].First, -0.01f);
            AdjustPos(maintainedEffecters[1].First, 0.01f);

        }

        public static void AdjustPos(Effecter effecter, float offset)
        {
            foreach (SubEffecter_Sprayer sub in effecter.children.OfType<SubEffecter_Sprayer>())
            {
                if (sub.mote != null)
                {
                    sub.mote.yOffset += offset;
                }
            }
        }
    }
}
