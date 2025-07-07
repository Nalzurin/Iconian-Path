using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace IconianPsycasts
{
    public class Building_PortalSpawner : Building, IMinHeatGiver
    {
        public bool IsActive => base.Spawned;
        public virtual int MinHeat => 15;
        public CompBreakLinkBuilding CompBreakLink => this.TryGetComp<CompBreakLinkBuilding>();
        public PortalSpawnerExtension SpawnerExtension => this.def.GetModExtension<PortalSpawnerExtension>();
        private List<Pawn> spawnedPawns = [];
        private int spawnedpawnsCount => spawnedPawns.Count();
        protected override void Tick()
        {
            base.Tick();
            if (this.IsHashIntervalTick(SpawnerExtension.spawnInterval) && spawnedpawnsCount < SpawnerExtension.maxSpawns)
            {
                SpawnPawn();
            }
            
        }
        public void SpawnPawn()
        {
            Pawn mob = PawnGenerator.GeneratePawn(SpawnerExtension.pawnKindToSpawn, Faction);
            GenSpawn.Spawn(mob, Position, MapHeld);
            spawnedPawns.Add(mob);
            Effecter portalEffecterTarget = DefOfs.Iconian_TeleportEffect.Spawn(Position, MapHeld, new Vector3(0, 3, 0));
        }
    }
}
