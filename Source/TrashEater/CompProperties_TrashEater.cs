using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IconianPsycasts
{
    public class CompProperties_TrashEater : CompProperties
    {
        public int maxStored = 4;
        public PawnKindDef pawnToSpawn;
        public int spawnAbilityCooldownTicks = 300;
        public ThingDef thingToEat;
        public string gizmoIcon;
        public CompProperties_TrashEater()
        {
            compClass = typeof(CompTrashEater);
        }
    }
}
