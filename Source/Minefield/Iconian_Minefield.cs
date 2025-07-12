using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using VEF.Abilities;
using Verse;
using Verse.Noise;
using Verse.Sound;
using static HarmonyLib.Code;
using Ability = VEF.Abilities.Ability;

namespace IconianPsycasts
{
    public class Iconian_Minefield : ThingWithComps
    {
        public Ability ability;
        private List<Faction> affectedFactions;
        public Pawn Pawn;

        protected override void Tick()
        {
            base.Tick();
            if (!Pawn.psychicEntropy.TryAddEntropy(1f, this) || Pawn.Downed)
            {
                Destroy();
                return;
            }
            if (affectedFactions == null)
            {
                affectedFactions = new List<Faction>();
            }
            List<Pawn> list2 = Map.mapPawns.AllPawnsSpawned.ToList();
            foreach (Pawn item2 in list2)
            {
                if (InAffectedArea(item2.Position))
                {
                    Hediff firstHediffOfDef = item2.health.hediffSet.GetFirstHediffOfDef(DefOfs.Iconian_MinefieldDebuff);
                    if (firstHediffOfDef != null)
                    {
                        firstHediffOfDef.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 120;
                    }
                    else
                    {
                        firstHediffOfDef = HediffMaker.MakeHediff(DefOfs.Iconian_MinefieldDebuff, item2);
                        item2.health.AddHediff(firstHediffOfDef);
                    }
                    if (item2.IsHashIntervalTick(60))
                    {
                        HealthUtility.AdjustSeverity(item2, DefOfs.Iconian_MinefieldDebuff, 0.02f);
                        DamageInfo dinfo = new DamageInfo(DamageDefOf.Cut, Rand.RangeInclusive(1, 3));
                        item2.TakeDamage(dinfo);
                    }
                    if (Pawn.Faction == Faction.OfPlayer)
                    {
                        AffectGoodwill(item2.HomeFaction, item2);
                    }
                }
            }
        }
        private void AffectGoodwill(Faction faction, Pawn p)
        {
            if (faction != null && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer) && (p == null || !p.IsSlaveOfColony) && !affectedFactions.Contains(faction))
            {
                Faction.OfPlayer.TryAffectGoodwillWith(faction, ability.def.goodwillImpact, canSendMessage: true, canSendHostilityLetter: true, HistoryEventDefOf.UsedHarmfulAbility);
            }
        }
        public bool InAffectedArea(IntVec3 cell)
        {
            return cell.InHorDistOf(Position, ability.GetRadiusForPawn());
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref Pawn, "pawn");
            Scribe_References.Look(ref ability, "ability");

        }

    }
}
