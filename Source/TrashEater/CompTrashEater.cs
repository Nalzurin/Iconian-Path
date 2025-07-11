using RimWorld;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VanillaPsycastsExpanded;
using Verse;
using Verse.AI;

namespace IconianPsycasts
{
    [StaticConstructorOnStartup]
    public class CompTrashEater : CompSummonedEntity, ISummonSource
    {
        private int DevourCooldownTicksRemaining = 0;
        private int DevourCooldownTicks = 300;
        private TrashEaterGizmo gizmo;
        public CompProperties_TrashEater Props => (CompProperties_TrashEater)props;
        private List<Thing> summonList = [];
        public List<Thing> SummonListForReading => summonList;

        private int cooldownTicksRemaining;
        private int cooldownTicks => Props.spawnAbilityCooldownTicks;
        public int maxStored => Props.maxStored;
        public int currentStored = 0;
        public int maxToFill;
        public int AmountToAutofill => Mathf.Max(0, maxToFill - currentStored);
        public float PercentageFull => (float)currentStored / (float)maxStored;
        private PawnKindDef pawnToSummon => Props.pawnToSpawn;

        public AcceptanceReport CanSpawn()
        {
            if (parent is Pawn pawn)
            {
 
                if (pawn.Faction != Faction.OfPlayer)
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
            if (!(parent is Pawn pawn))
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
                    gizmo = new TrashEaterGizmo(this);
                }
                yield return gizmo;
            }

            AcceptanceReport canSpawn = CanSpawn();
            Command_ActionWithCooldown act = new Command_ActionWithCooldown
            {
                cooldownPercentGetter = () => Mathf.InverseLerp(Props.spawnAbilityCooldownTicks, 0f, cooldownTicksRemaining),
                action = delegate
                {
                    TrySpawnPawn();
                },
                //hotKey = KeyBindingDefOf.Misc2,
                Disabled = !canSpawn.Accepted,
                icon = ContentFinder<Texture2D>.Get(Props.gizmoIcon),
                defaultLabel = "IconianSummonPawn".Translate(pawnToSummon.labelPlural),
                defaultDesc = "IconianSummonPawnDec".Translate(pawnToSummon.labelPlural, Props.thingToEat.label)
            };

            if (!canSpawn.Reason.NullOrEmpty())
            {
                act.Disable(canSpawn.Reason);
            }
            yield return act;


            // devour ability
            Command_ActionWithCooldown forceRefill = new Command_ActionWithCooldown
            {
                cooldownPercentGetter = () => Mathf.InverseLerp(DevourCooldownTicks, 0f, DevourCooldownTicksRemaining),
                action = delegate
                {
                    if (parent is Pawn)
                    {
                        Pawn p = (Pawn)parent;

                        List<Thing> listEatableThings = ListEatableThings(p);

                        if (!listEatableThings.NullOrEmpty())
                        {
                            Job job = EatThingJob(pawn, listEatableThings[0]);
                            job.count = Mathf.Min(job.count, AmountToAutofill);
                            job.targetQueueB = (from i in listEatableThings.Skip(1)
                                                select new LocalTargetInfo(i)).ToList();

                            if (job.count > 0)
                            {
                                p.jobs.TryTakeOrderedJob(job, JobTag.MiscWork, false);
                            }

                        }
                    }
                },

                Disabled = currentStored == maxStored, // !canSpawn.Accepted,
                icon = ContentFinder<Texture2D>.Get(Props.gizmoIcon),
                defaultLabel = "Iconian_EatScrap".Translate(),
                defaultDesc = "Iconian_EatScrapDesc".Translate()
            };
            yield return forceRefill;

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
                command_Action2.defaultLabel = "DEV: Fill with " + Props.thingToEat.label;
                command_Action2.action = delegate
                {
                    currentStored = maxStored;
                };
                yield return command_Action2;
                Command_Action command_Action3 = new Command_Action();
                command_Action3.defaultLabel = "DEV: Empty " + Props.thingToEat.label;
                command_Action3.action = delegate
                {
                    currentStored = 0;
                };
                yield return command_Action3;
            }
        }

        private Job EatThingJob(Pawn pawn, Thing thing)
        {
            Job job = JobMaker.MakeJob(DefOfs.Iconian_EatThingForced, thing, this.parent);
            job.count = Mathf.Min(thing.stackCount, AmountToAutofill);

            job.haulMode = HaulMode.ToContainer;

            return job;
        }

        private List<Thing> ListEatableThings(Pawn p)
        {
            return HaulAIUtility.FindFixedIngredientCount(p, Props.thingToEat, 1);
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (!Find.Selector.IsSelected(parent))
            {
                return;
            }
            for (int i = 0; i < summonList.Count; i++)
            {
                if (!summonList[i].DestroyedOrNull() && parent.Map == summonList[i].Map)
                {
                    GenDraw.DrawLineBetween(parent.TrueCenter(), summonList[i].TrueCenter());
                }
            }
        }
        public override void CompTick()
        {
            base.CompTick();
            if(DevourCooldownTicksRemaining > 0)
            {
                DevourCooldownTicksRemaining--;
            }
            if (cooldownTicksRemaining > 0)
            {
                cooldownTicksRemaining--;
            }
        }
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownTicksRemaining, "cooldownTicksRemaining", 0);
            Scribe_Values.Look(ref maxToFill, "maxToFill", 0);
            Scribe_Collections.Look(ref summonList, "summonList", LookMode.Reference);
            Scribe_Values.Look(ref currentStored, "currentStored", 0);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                summonList.RemoveAll((Thing x) => x == null);
            }
        }

    }
}
