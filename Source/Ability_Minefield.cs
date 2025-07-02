using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VEF.Abilities;

namespace IconianPsycasts
{
    [StaticConstructorOnStartup]
    public class Ability_Minefield : Ability
    {
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            for (int i = 0; i < targets.Length; i++)
            {
                Log.Message("Test");
                GlobalTargetInfo globalTargetInfo = targets[i];
                Log.Message("Test1");

                Iconian_Minefield minefield = (Iconian_Minefield)GenSpawn.Spawn(DefOfs.Iconian_Minefield, globalTargetInfo.Cell, globalTargetInfo.Map);
                Log.Message("Test2");

                minefield.ticksLeftToDisappear = GetDurationForPawn();
                minefield.caster = pawn;
                minefield.radius = GetRadiusForPawn();
            }
        }
    }

}
