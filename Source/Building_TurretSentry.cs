using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static HarmonyLib.Code;

namespace IconianPsycasts
{
    public class Building_TurretSentry : Building_TurretGunSummoned
    {
        public CompExplosive compExplosive => this.TryGetComp<CompExplosive>();
        public override int MinHeat => 25;
        public int Duration = 90000;
        protected override void Tick()
        {
            base.Tick();
            if (this.HitPoints == 0)
            {
                Destroy();
            }
            if (this.HitPoints > 0 && this.IsHashIntervalTick(90))
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
                        Log.Message("test");
                        CompProperties_Explosive props = (CompProperties_Explosive)compExplosive.props;
                        props.damageAmountBase = (int)(props.damageAmountBase * (HitPoints / (float)Duration));
                        props.explosiveRadius *= HitPoints / (float)Duration;
                        Log.Message(props.explosiveRadius);
                        Log.Message(HitPoints);
                        Log.Message(Duration);  
                        compExplosive.StartWick();
                    }
                }
            };


        }

    }
}
