using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using static IconianPsycasts.Helper;

namespace IconianPsycasts
{
    public class Ability_RerollBackgroundTrait : VEF.Abilities.Ability
    {
        Pawn targetPawn;

        public override bool CanHitTarget(LocalTargetInfo target)
        {
            bool result = base.CanHitTarget(target);
            if (!result)
            {
                return result;
            }
            if (target.TryGetPawn(out Pawn p) && p.RaceProps.Humanlike && p.ageTracker.Adult)
            {
                return result;
            }
            return false;
           
        }
        const int maxRetries = 10;
        public override void Cast(params GlobalTargetInfo[] targets)
        {
            base.Cast(targets);
            targetPawn = targets[0].Thing as Pawn;
            Find.WindowStack.Add(new Dialog_SelectTrait(this, targetPawn));
            


        }

        public void DoReroll(Trait oldTrait)
        {
            if(oldTrait == null)
            {
                Cancel("ErrorCouldNotGetTraitToRemove".Translate());
                return;
            }
            float[] floats = Helper.GetRarityWeightsCivvy(CasterPawn.psychicEntropy.PsychicSensitivity);
            Log.Message(floats.ToStringSafeEnumerable());
            Rarity rarity = Helper.RollRarity(floats);
            Log.Message(rarity);
            BackstoryDef backstory = null;
            bool backstorySuccess = false;
            for (int i = 0; i < maxRetries; i++)
            {
                backstory = Helper.GetBackstoryDef(def, rarity);
                if (backstory != null && targetPawn.story.Adulthood != backstory)
                {
                    backstorySuccess = true;
                    break;
                }
            }
            if (!backstorySuccess)
            {
                Cancel("ErrorUnableToSelectBackstory".Translate());
                return;
            }
            Log.Message(backstory.defName);
            int degree = 0;
            TraitDef newTrait = null;
            bool traitSuccess = false;
            for (int i = 0; i < maxRetries; i++)
            {
                newTrait = Helper.GetTraitDef(def, rarity, out degree);
                Log.Message(newTrait);
                if (newTrait != null && !targetPawn.story.traits.HasTrait(newTrait))
                {
                    traitSuccess = true;
                    break;
                }
            }
            if (!traitSuccess)
            {
                Cancel("ErrorUnableToSelectTrait".Translate());
                return;
            }

            Helper.AdjustPawnSkills(targetPawn.skills, targetPawn.story.Adulthood, backstory);
            targetPawn.story.Adulthood = backstory;
            targetPawn.skills.Notify_SkillDisablesChanged();
            targetPawn.Notify_DisabledWorkTypesChanged();
            Helper.AdjustPawnSkills(targetPawn.skills, oldTrait, newTrait);
            targetPawn.story.traits.RemoveTrait(oldTrait);
            targetPawn.story.traits.GainTrait(new Trait(newTrait, degree));
            targetPawn.skills.Notify_SkillDisablesChanged();
            targetPawn.Notify_DisabledWorkTypesChanged();



        }
        public void Cancel(string info = "")
        {
            if(info != "")
            {
                Messages.Message(info, MessageTypeDefOf.NegativeEvent);

            }
            cooldown = 1;
        }
    }
}
