using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IconianPsycasts
{
    public class CompProperties_ExplodeLeap : CompProperties_AbilityEffect
    {
        public float radius = 15.9f;
        public int damage = 50;
        public DamageDef damageDef = DamageDefOf.Bomb;
        public CompProperties_ExplodeLeap()
        {
            compClass = typeof(CompAbilityEffect_ExplodeLeap);
        }
    }
}
