using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IconianPsycasts
{
    public class CompAbilityEffect_ExplodeLeap : CompAbilityEffect, ICompAbilityEffectOnJumpCompleted
    {
        private new CompProperties_ExplodeLeap Props => (CompProperties_ExplodeLeap)props;

        public void OnJumpCompleted(IntVec3 origin, LocalTargetInfo target)
        {
            GenExplosion.DoExplosion(target.Cell, target.Pawn.MapHeld, Props.radius, Props.damageDef, this.parent.pawn, Props.damage);
            this.parent.pawn.Destroy();

        }
    }
}
