using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;

namespace IconianPsycasts
{
    public class Pawn_Summoned : Pawn, IMinHeatGiver
    {
        public bool IsActive => base.Spawned || CaravanUtility.GetCaravan((Pawn)this) != null;

        public int MinHeat => 20;
    }
}
