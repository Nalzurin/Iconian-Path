using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using VEF.Abilities;
using VanillaPsycastsExpanded;
using VanillaPsycastsExpanded.Staticlord;

namespace IconianPsycasts
{
    [StaticConstructorOnStartup]
    public class Ability_Minefield : Ability, IAbilityToggle, IChannelledPsycast
    {
        private Iconian_Minefield minefield;
        public bool IsActive => minefield?.Spawned ?? false;
        public bool Toggle
        {
            get
            {
                return minefield?.Spawned ?? false;
            }
            set
            {
                if (value)
                {
                    DoAction();
                }
                else
                {
                    minefield?.Destroy();
                }
            }
        }
        public string OffLabel => "Iconian_StopMinefield".Translate();
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            for (int i = 0; i < targets.Length; i++)
            {
                Log.Message("Test");
                GlobalTargetInfo globalTargetInfo = targets[i];
                Log.Message("Test1");

                minefield = (Iconian_Minefield)ThingMaker.MakeThing(DefOfs.Iconian_Minefield);
                minefield.Pawn = pawn;
                minefield.ability = this;
                GenSpawn.Spawn(minefield, globalTargetInfo.Cell, globalTargetInfo.Map);
                Log.Message("Test2");


            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref minefield, "minefield");
        }
        public override Gizmo GetGizmo()
        {
            return new Command_AbilityToggle(pawn, this);
        }
    }

}
