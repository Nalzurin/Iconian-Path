using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VEF.Abilities;
using Verse;
using static HarmonyLib.Code;

namespace IconianPsycasts
{
    public class Building_TurretSentry : Building_TurretGunSummoned
    {
        public CompBreakLinkBuilding compBreakLink => this.TryGetComp<CompBreakLinkBuilding>();
        public CompExplosive compExplosive => this.TryGetComp<CompExplosive>();
        public override int MinHeat => 25;
        public int Duration = 90000;
        private int halfHour = 1250;
        protected override void Tick()
        {
            base.Tick();
            if (this.HitPoints == 0)
            {
                Destroy();
            }
            if (this.HitPoints > 0 && this.IsHashIntervalTick(Helper.TurretHealthTimeRatio))
            {
                this.HitPoints--;
            }

        }
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }
            yield return new Command_Action
            {
                defaultLabel = "IconianSentryExplode".Translate(),
                defaultDesc = "IconianSentryExplodeDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Buildings/AlienLaserTurret_Top"),
                action = delegate
                {
                    if (compExplosive != null)
                    {
                        CompProperties_Explosive props = (CompProperties_Explosive)compExplosive.props;
                        props.damageAmountBase = (int)(props.damageAmountBase * (HitPoints / (float)Duration));
                        props.explosiveRadius *= HitPoints / (float)Duration;
                        compExplosive.StartWick();
                    }
                }
            };
            yield return new Command_Target
            {
                defaultLabel = "IconianSentryTeleport".Translate(),
                defaultDesc = "IconianSentryTeleportDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("Buildings/AlienLaserTurret_Top"),
                targetingParams = TargetingParameters.ForDropPodsDestination(),
                action = delegate(LocalTargetInfo target)
                {
                    Effecter portalEffecter = DefOfs.Iconian_TeleportEffect.Spawn(PositionHeld, MapHeld, new Vector3(0, 3, 0));
                    Building_TurretSentry thing = (Building_TurretSentry)ThingMaker.MakeThing(def);
                    thing.HitPoints = HitPoints - 1250 / Helper.TurretHealthTimeRatio;
                    thing.Duration = Duration;
                    thing.TryGetComp<CompBreakLinkBuilding>().Pawn = compBreakLink.Pawn;
                    GenSpawn.Spawn(thing, target.Cell, MapHeld);
                    Effecter portalEffecterTarget = DefOfs.Iconian_TeleportEffect.Spawn(target.Cell, MapHeld, new Vector3(0, 3, 0));

                    this.Destroy(DestroyMode.Vanish);

                }
            };


        }

    }
}
