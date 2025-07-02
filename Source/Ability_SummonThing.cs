using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using VEF.Abilities;

namespace IconianPsycasts
{
    public class Ability_SummonThing : VEF.Abilities.Ability
    {
        public override void Tick()
        {
            base.Tick();
            if (!maintainedEffecters.Any())
            {
                return;
            }


            foreach (Pair<Effecter, TargetInfo> item in maintainedEffecters)
            {
                AdjustPos(item.First, -0.01f);
            }
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
