using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace IconianPsycasts
{
    public class CompBreakLinkBuilding : ThingComp
    {
        public Pawn Pawn;

        public CompProperties_BreakLinkBuilding Props => props as CompProperties_BreakLinkBuilding;

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                defaultLabel = "IconianSelectCaster".Translate(),
                defaultDesc = "IconianSelectCasterDesc".Translate(),
                icon = null,
                action = delegate
                {
                    Find.Selector.ClearSelection();
                    Find.Selector.Select(Pawn);
                },
                onHover = delegate
                {
                    if (Pawn != null)
                    {
                        GenDraw.DrawArrowPointingAt(Pawn.TrueCenter());
                    }
                }
            };
            yield return new Command_Action
            {
                defaultLabel = Props.gizmoLabel.Translate(),
                defaultDesc = Props.gizmoDesc.Translate(),
                icon = ContentFinder<Texture2D>.Get(Props.gizmoImage),
                action = delegate
                {
                    BreakLink();

                }
            };

        }


        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (parent is IMinHeatGiver giver)
            {
                Pawn.Psycasts().AddMinHeatGiver(giver);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            Pawn pawn = Pawn;
            if ((pawn == null || pawn.Dead || pawn.Destroyed) ? true : false)
            {
                BreakLink();
            }
        }
        public void BreakLink()
        {
            Effecter portalEffecter = DefOfs.Iconian_TeleportEffect.Spawn(parent.Position, parent.Map, new Vector3(0, 3, 0));
            parent.Destroy();

        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_References.Look(ref Pawn, "pawn");
        }
    }
}
