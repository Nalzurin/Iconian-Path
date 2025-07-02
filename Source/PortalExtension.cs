/*using RimWorld.Planet;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using 
.Abilities;
using Ability = VEF.Abilities.Ability;
using System.Linq;

namespace IconianPsycasts;
public class PortalExtension : AbilityExtension_AbilityMod
{
    public List<ThingDef> allowedBuildings;
    public int count;
    public int durationTicks;
    public List<IntVec2> pattern;
    public PawnKindDef pawnKind;
    public bool teleportCaster;
    public ThingDef thingDef;

    public override void Cast(GlobalTargetInfo[] targets, Ability ability)
    {
        base.Cast(targets, ability);

        GlobalTargetInfo target = targets[0];
        IntVec3 cellToTeleport = GetCellToTeleport(ability, target);

        if (teleportCaster)
        {
            Effecter portalEffecter = DefOfs.AAD_TeleportEffect.Spawn(cellToTeleport, ability.pawn.Map, new Vector3(0, 3, 0));
            ability.AddEffecterToMaintain(portalEffecter, cellToTeleport, 180, ability.pawn.Map);
            Effecter portalEffecter2 = DefOfs.AAD_TeleportEffect.Spawn(ability.pawn.Position, ability.pawn.Map, new Vector3(0, 3, 0));
            ability.AddEffecterToMaintain(portalEffecter2, ability.pawn.Position, 180, ability.pawn.Map);
            foreach (SubEffecter_Sprayer sub in portalEffecter2.children.OfType<SubEffecter_Sprayer>())
            {
                if (sub.mote != null)
                {
                    sub.mote.yOffset -= 6.2f;
                }
            }

            ability.pawn.Position = cellToTeleport;
            ability.pawn.Notify_Teleported();
            if (target.Thing is Pawn pawnTarget && pawnTarget.HostileTo(ability.pawn))
            {
                pawnTarget.stances?.stunner.StunFor(180, ability.pawn);
            }
        }
        else if (pattern != null)
        {
            List<Thing> things = GetThingsToTeleport();
            List<IntVec3> cells = AffectedCells(cellToTeleport, ability.pawn.Map).ToList();
            for (int i = 0; i < cells.Count; i++)
            {
                DoTeleport(ability, things[i], cells[i]);
            }
        }
        else
        {
            foreach (Thing thing in GetThingsToTeleport())
            {
                DoTeleport(ability, thing, cellToTeleport);
            }
        }
    }

    private void DoTeleport(Ability ability, Thing thing, IntVec3 cell)
    {
        cell = AdjustCell(ability, cell, thing);
        TeleportThing(ability, cell, thing);
        Effecter portalEffecter = DefOfs.AAD_TeleportEffect.Spawn(cell, ability.pawn.Map, new Vector3(0, 3, 0));
        ability.AddEffecterToMaintain(portalEffecter, cell, 180, ability.pawn.Map);
    }

    private void TeleportThing(Ability ability, IntVec3 cellToTeleport, Thing thing)
    {
        if (thing.def.CanHaveFaction)
        {
            thing.SetFaction(ability.pawn.Faction);
        }

        CompDestroyOnCasterDowned comp = thing.TryGetComp<CompDestroyOnCasterDowned>();
        if (comp != null)
        {
            comp.caster = ability.pawn;
        }

        if (thing is Pawn pawn && durationTicks > 0)
        {
            Hediff hediff = pawn.health.AddHediff(DefOfs.AAD_MechControllable);
            hediff.TryGetComp<HediffComp_Disappears>().ticksToDisappear = durationTicks;
            PawnComponentsUtility.AddAndRemoveDynamicComponents(pawn);
        }

        GenPlace.TryPlaceThing(thing, cellToTeleport, ability.pawn.Map, ThingPlaceMode.Direct);

        CompExplosive compExplosive = thing.TryGetComp<CompExplosive>();
        if (compExplosive == null)
        {
            return;
        }

        compExplosive.StartWick(ability.pawn);
    }

    private IntVec3 AdjustCell(Ability ability, IntVec3 cellToTeleport, Thing thing)
    {
        if (!CanTeleport(cellToTeleport))
        {
            RCellFinder.TryFindRandomCellNearWith(
                cellToTeleport,
                CanTeleport,
                ability.pawn.Map,
                out cellToTeleport,
                1,
                1000
            );
        }

        return cellToTeleport;


        bool CanTeleport(IntVec3 cell)
        {
            return thing is Pawn ? CanTeleportPawnTo(cell, ability) : CanTeleportThingTo(cell, ability);
        }
    }

    private static bool CanTeleportPawnTo(IntVec3 cell, Ability ability)
    {
        // check pawns can walk on the cell
        if (!cell.WalkableBy(ability.pawn.Map, ability.pawn))
        {
            return false;
        }

        // check there is no pawn
        return cell.GetFirstPawn(ability.pawn.Map) == null;
    }

    private bool CanTeleportThingTo(IntVec3 cell, Ability ability)
    {
        // check there is no building or it is an allowed building
        Building building = cell.GetFirstBuilding(ability.pawn.Map);
        if (building is not null && !allowedBuildings.Contains(building.def))
        {
            return false;
        }

        // check there is no item
        return cell.GetFirstItem(ability.pawn.Map) == null;
    }

    private List<Thing> GetThingsToTeleport()
    {
        List<Thing> things = [];
        if (pawnKind != null)
        {
            for (int i = 0; i < count; i++)
            {
                Pawn pawn = PawnGenerator.GeneratePawn(pawnKind);
                things.Add(pawn);
            }
        }
        else
        {
            int countToMake = count;
            while (countToMake > 0)
            {
                int curStack = Mathf.Min(thingDef.stackLimit, countToMake);
                countToMake -= curStack;
                Thing thing = ThingMaker.MakeThing(thingDef);
                thing.stackCount = curStack;
                things.Add(thing);
            }
        }

        return things;
    }

    private static IntVec3 GetCellToTeleport(Ability ability, GlobalTargetInfo target)
    {
        if (!target.HasThing)
        {
            return target.Cell;
        }

        IntVec3 pos = target.Cell + target.Thing.Rotation.Opposite.FacingCell;
        return pos.WalkableBy(ability.pawn.Map, ability.pawn) ? pos : target.Cell;
    }

    private IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map)
    {
        return pattern.Select(item => target.Cell + new IntVec3(item.x, 0, item.z))
            .Where(intVec => intVec.InBounds(map));
    }
}
*/