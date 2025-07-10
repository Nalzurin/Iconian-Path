using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace IconianPsycasts
{
    public class PortalSpawnerExtension : DefModExtension
    {
        public PawnKindDef pawnKindToSpawn;
        public int maxSpawns = 5;
        public int spawnInterval = 6000;
    }
}
