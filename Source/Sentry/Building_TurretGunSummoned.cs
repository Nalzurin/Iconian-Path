using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;

namespace IconianPsycasts
{
    public class Building_TurretGunSummoned : Building_TurretGun, IMinHeatGiver
    {
        public bool IsActive => base.Spawned;

        public virtual int MinHeat => 15;

    }
}
