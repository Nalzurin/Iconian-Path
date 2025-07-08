/*using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace IconianPsycasts
{
    public class CompTrashEater : CompSummonedEntity, ISummonSource
    {
        CompProperties_TrashEater Props => (CompProperties_TrashEater)props;
        private List<Thing> summonList = [];
        public List<Thing> SummonListForReading => summonList;

        private int cooldownTicksRemaining;
        private int cooldownTicks => Props.spawnAbilityCooldownTicks;
        private int maxStored => Props.maxStored;
        private int currentStored = 0;
        private PawnKindDef pawnToSummon => Props.pawnToSpawn;
        public AcceptanceReport CanSpawn()
        {
            if (parent is Pawn pawn)
            {
                if (pawn.IsSelfShutdown())
                {
                    return "SelfShutdown".Translate();
                }
                if (pawn.Faction == Faction.OfPlayer && !pawn.IsColonyMechPlayerControlled)
                {
                    return false;
                }
                if (!pawn.Awake() || pawn.Downed || pawn.Dead || !pawn.Spawned)
                {
                    return false;
                }
            }
            if (currentStored < 1)
            {
                return "IconianNoScrap".Translate();
            }
            if (cooldownTicksRemaining > 0)
            {
                return "CooldownTime".Translate() + " " + cooldownTicksRemaining.ToStringSecondsFromTicks();
            }
            return true;
        }
        public void TrySpawnPawn()
        {
            if (!CanSpawn())
            {
                return;
            }
            PawnGenerationRequest request = new PawnGenerationRequest(pawnToSummon, parent.Faction, PawnGenerationContext.NonPlayer, -1, forceGenerateNewPawn: true, allowDead: false, allowDowned: false, canGeneratePawnRelations: true, mustBeCapableOfViolence: false, 1f, forceAddFreeWarmLayerIfNeeded: false, allowGay: true, allowPregnant: false, allowFood: true, allowAddictions: true, inhabitant: false, certainlyBeenInCryptosleep: false, forceRedressWorldPawnIfFormerColonist: false, worldPawnFactionDoesntMatter: false, 0f, 0f, null, 1f, null, null, null, null, null, null, null, null, null, null, null, null, forceNoIdeo: false, forceNoBackstory: false, forbidAnyTitle: false, forceDead: false, null, null, null, null, null, 0f, DevelopmentalStage.Newborn);
            cooldownTicksRemaining = cooldownTicks;
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            GenSpawn.Spawn(pawn, parent.Position, parent.Map);
            AddSummon(pawn);
            currentStored--;
        }
        public void AddSummon(Thing thing)
        {
            summonList.Add(thing);
        }

        public void RemoveSummon(Thing thing)
        {
            summonList.Remove(thing);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (!(parent is Pawn pawn) || !pawn.IsColonyMech || pawn.GetOverseer() == null)
            {
                yield break;
            }
            foreach (Gizmo item in base.CompGetGizmosExtra())
            {
                yield return item;
            }
            if (Find.Selector.SingleSelectedThing == parent)
            {
                if (gizmo == null)
                {
                    gizmo = new MechCarrierChoiceGizmo(this);
                }
                yield return gizmo;
            }

            if (innerContainer.Any())
            {
                Command_Action command_Action4 = new Command_Action();
                command_Action4.defaultLabel = "CommandEjectContents".Translate();
                command_Action4.icon = ContentFinder<Texture2D>.Get("UI/Icons/EjectContentFromAtomizer");
                command_Action4.action = delegate
                {
                    innerContainer.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
                };
                yield return command_Action4;
            }

            foreach (WorkqueenSpawnerdef spawnerdef in Props.workqueenSpawnerdefs)
            {
                AcceptanceReport canSpawn = CanSpawn(spawnerdef);
                Command_ActionWithCooldown act = new Command_ActionWithCooldown
                {
                    cooldownPercentGetter = () => Mathf.InverseLerp(Props.cooldownTicks, 0f, cooldownTicksRemaining),
                    action = delegate
                    {
                        TrySpawnPawns(spawnerdef);
                    },
                    //hotKey = KeyBindingDefOf.Misc2,
                    Disabled = !canSpawn.Accepted,
                    icon = ContentFinder<Texture2D>.Get(spawnerdef.GizmoTexPath),
                    defaultLabel = "MechCarrierRelease".Translate(spawnerdef.spawnPawnKind.labelPlural),
                    defaultDesc = "MechCarrierDesc".Translate(spawnerdef.maxPawnsToSpawn, spawnerdef.spawnPawnKind.labelPlural, spawnerdef.spawnPawnKind.label, spawnerdef.costPerPawn, Props.fixedIngredient.label)
                };

                if (!canSpawn.Reason.NullOrEmpty())
                {
                    act.Disable(canSpawn.Reason);
                }
                yield return act;
            }

            // devour ability
            Command_ActionWithCooldown forceRefill = new Command_ActionWithCooldown
            {
                cooldownPercentGetter = () => Mathf.InverseLerp(DevourCooldownTicks, 0f, DevourCooldownTicksRemaining),
                action = delegate
                {
                    if (parent is Pawn)
                    {
                        Pawn p = (Pawn)parent;

                        List<Thing> listDevourableUrchins = UrchinDevourHelper.ListDevourableUrchins(this, pawn);

                        if (!listDevourableUrchins.NullOrEmpty())
                        {
                            Job job = UrchinDevourHelper.HaulToDevourJob(pawn, listDevourableUrchins[0], parent);
                            job.count = Mathf.Min(job.count, UrchinDevourHelper.AmountToDevourIgnoreAutofill(this));
                            job.targetQueueB = (from i in listDevourableUrchins.Skip(1)
                                                select new LocalTargetInfo(i)).ToList();

                            //p.jobs.StartJob(job); //throws dev warnings

                            if (job.count > 0)
                            {
                                p.jobs.TryTakeOrderedJob(job, JobTag.MiscWork, false);
                            }

                            *//*
                            WorkGiver_HaulToDevour workGiver = new WorkGiver_HaulToDevour();    // gives nullpointer here

                            if(workGiver == null)
                            {
                                Log.Warning("workGiver is null");
                            }
                            if (listDevourableUrchins[0].PositionHeld == null)
                            {
                                Log.Warning("listDevourableUrchins[0].PositionHeld is null");
                            }
                           
                            p.jobs.TryTakeOrderedJobPrioritizedWork(job, workGiver, listDevourableUrchins[0].PositionHeld);
                             *//*
                        }
                    }
                },

                Disabled = !UrchinDevourHelper.DevourIsValidIgnoreAutofill(this), // !canSpawn.Accepted,
                icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_DevourUrchin"),
                defaultLabel = "AV_DevourUrchins".Translate(),
                defaultDesc = "AV_DevourUrchins_Description".Translate()
            };
            yield return forceRefill;

            //autocast switch

            Command_Toggle spawnToggle = new Command_Toggle();

            if (lastSpawnerdef == null)
            {
                spawnToggle.defaultLabel = "AV_AutocastUrchins".Translate() + "AV_Urchins".Translate();
            }
            else
            {
                spawnToggle.defaultLabel = "AV_AutocastUrchins".Translate() + lastSpawnerdef.spawnPawnKind.label;
            }

            spawnToggle.icon = ContentFinder<Texture2D>.Get("UI/Gizmos/AV_UrchinAutocast");
            spawnToggle.defaultDesc = "AV_AutocastUrchins_Description".Translate();
            spawnToggle.toggleAction = () => AllowedToAutocast = !AllowedToAutocast;
            spawnToggle.isActive = (() => AllowedToAutocast);

            yield return spawnToggle;


            if (DebugSettings.ShowDevGizmos)
            {
                if (cooldownTicksRemaining > 0)
                {
                    Command_Action command_Action = new Command_Action();
                    command_Action.defaultLabel = "DEV: Reset cooldown";
                    command_Action.action = delegate    
                    {
                        cooldownTicksRemaining = 0;
                    };
                    yield return command_Action;
                }
                Command_Action command_Action2 = new Command_Action();
                command_Action2.defaultLabel = "DEV: Fill with " + Props.fixedIngredient.label;
                command_Action2.action = delegate
                {
                    while (IngredientCount < Props.maxIngredientCount)
                    {
                        int stackCount = Mathf.Min(Props.maxIngredientCount - IngredientCount, Props.fixedIngredient.stackLimit);
                        Thing thing = ThingMaker.MakeThing(Props.fixedIngredient);
                        thing.stackCount = stackCount;
                        innerContainer.TryAdd(thing, thing.stackCount);
                    }
                };
                yield return command_Action2;
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "DEV: Empty " + Props.fixedIngredient.label;
                command_Action3.action = delegate
                {
                    innerContainer.ClearAndDestroyContents();
                };
                yield return command_Action3;
            }
        }

    }
}
*/