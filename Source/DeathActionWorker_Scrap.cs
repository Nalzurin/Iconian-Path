using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VanillaPsycastsExpanded;
using Verse;
using Verse.AI.Group;

namespace IconianPsycasts
{
    public class DeathActionWorker_Scrap : DeathActionWorker
    {
        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            if (corpse.Map != null)
            {
                Thing newThing = ThingMaker.MakeThing(DefOfs.Iconian_ScrapPile);
                GenSpawn.Spawn(newThing, corpse.Position, corpse.Map);
                corpse.Destroy();
                CompSummonedEntity comp = corpse.InnerPawn.TryGetComp<CompSummonedEntity>();
                if (comp != null)
                {
                    comp.summonSource.RemoveSummon(corpse.InnerPawn);
                }
            }
        }
    }
}
