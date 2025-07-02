using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using static UnityEngine.GraphicsBuffer;

namespace IconianPsycasts
{
    public static class Helper
    {
        public const int TurretHealthTimeRatio = 90;
        public const float CommonCutoff = 2f;
        public const float RareCutoff = 3f;
        public const float EpicCutoff = 5f;
        public const float negLegendaryCutoff = 0.75f;
        public const float negEpicCutoff = 0.5f;
        public const float negRareCutoff = 0.25f;


        public enum Rarity
        {
            Common, Rare, Epic, Legendary
        }
        public static void AdjustWeights(ref float common, ref float rare, ref float epic, ref float legendary, string changedKey, float newValue)
        {
            // Get current weights
            var weights = new Dictionary<string, float>{{ "Common", common },
                                                        { "Rare", rare },
                                                        { "Epic", epic },
                                                        { "Legendary", legendary }};
            if (weights[changedKey] == newValue)
            {
                return;
            }

            // Calculate how much needs to be distributed
            float delta = newValue - weights[changedKey];
            float remaining = 1f - newValue;

            // Remove the changed key temporarily
            weights[changedKey] = -1f;

            // Sum of other weights (for proportional redistribution)
            float sumOther = weights.Values.Where(v => v >= 0f).Sum();

            if (sumOther <= 0f)
            {
                // If others are zero or negative (e.g., initial state), split evenly
                float even = remaining / 3f;
                foreach (var key in weights.Keys.ToList())
                {
                    if (key != changedKey) weights[key] = even;
                }
            }
            else
            {
                // Redistribute proportionally
                foreach (var key in weights.Keys.ToList())
                {
                    if (weights[key] >= 0f)
                        weights[key] = (weights[key] / sumOther) * remaining;
                }
            }

            // Set the changed key back
            weights[changedKey] = newValue;

            // Write back the values
            common = weights["Common"];
            rare = weights["Rare"];
            epic = weights["Epic"];
            legendary = weights["Legendary"];
        }

        public static Rarity RollRarity(float[] floats)
        {
            Rarity[] rarities = { Rarity.Common, Rarity.Rare, Rarity.Epic, Rarity.Legendary };
            float val = Rand.Value;
            float chance = 0f;
            for (int i = 0; i < 4; i++)
            {
                chance += floats[i];
                if (val < chance)
                {
                    return rarities[i];
                }
            }
            return rarities[0];
        }
        public static float[] GetRarityWeightsCivvy(float psychicSensitivity)
        {
            if (psychicSensitivity >= 5f)
            {
                return new float[] { 0f, 0f, 0f, 1f };
            }
            float[] result = new float[] { IconianPsycasts_Mod.settings.ChanceCommon, IconianPsycasts_Mod.settings.ChanceRare, IconianPsycasts_Mod.settings.ChanceEpic, IconianPsycasts_Mod.settings.ChanceLegendary };
            if (psychicSensitivity == 1f)
            {
                return result;
            }
            result = GetRarityDistribution(result, psychicSensitivity);
            Console.WriteLine(result.ToStringSafeEnumerable());
            return result;
        }
        private static float[] GetRarityDistribution(float[] floats, float psychicSensitivity)
        {
            if (psychicSensitivity > 1f)
            {
                if (psychicSensitivity < CommonCutoff)
                {

                    float newVal = floats[0] * (1 - psychicSensitivity / CommonCutoff);
                    AdjustWeights(ref floats[0], ref floats[1], ref floats[2], ref floats[3], "Common", newVal);

                }
                else
                {
                    AdjustWeights(ref floats[0], ref floats[1], ref floats[2], ref floats[3], "Common", 0f);
                }
                if (psychicSensitivity < RareCutoff)
                {

                    float newVal = floats[1] * (1 - psychicSensitivity / RareCutoff);
                    AdjustWeights(ref floats[0], ref floats[1], ref floats[2], ref floats[3], "Rare", newVal);

                }
                else
                {
                    AdjustWeights(ref floats[0], ref floats[1], ref floats[2], ref floats[3], "Rare", 0f);
                }
                if (psychicSensitivity < EpicCutoff)
                {

                    float newVal = floats[2] * (1 - psychicSensitivity / EpicCutoff);
                    AdjustWeights(ref floats[0], ref floats[1], ref floats[2], ref floats[3], "Epic", newVal);

                }
                else
                {
                    AdjustWeights(ref floats[0], ref floats[1], ref floats[2], ref floats[3], "Epic", 0f);
                }
            }
            return floats;
        }

        public static void AdjustPawnSkills(Pawn_SkillTracker skills, BackstoryDef oldBack, BackstoryDef newBack)
        {
            if (skills == null || oldBack == null || newBack == null)
            {
                return;
            }
            foreach (SkillGain gain in oldBack.skillGains)
            {
                skills.GetSkill(gain.skill).levelInt -= gain.amount;
            }
            foreach (SkillGain gain in newBack.skillGains)
            {
                skills.GetSkill(gain.skill).levelInt += gain.amount;
            }

        }
        public static void AdjustPawnSkills(Pawn_SkillTracker skills, Trait oldTrait, TraitDef newTrait)
        {
            if (skills == null || oldTrait == null || newTrait == null)
            {
                return;
            }
            foreach (TraitDegreeData data in oldTrait.def.degreeDatas)
            {
                if (data.skillGains != null)
                {
                    foreach (SkillGain gain in data.skillGains)
                    {
                        skills.GetSkill(gain.skill).levelInt -= gain.amount;

                    }
                }
            }
            if (oldTrait.def.forcedPassions != null)
            {
                foreach (SkillDef passion in oldTrait.def.forcedPassions)
                {
                    skills.GetSkill(passion).passion = Passion.None;
                }
            }
            foreach (TraitDegreeData data in newTrait.degreeDatas)
            {
                if (data.skillGains != null)
                {
                    foreach (SkillGain gain in data.skillGains)
                    {
                        skills.GetSkill(gain.skill).levelInt += gain.amount;

                    }
                }
            }
            if (newTrait.forcedPassions != null)
            {
                foreach (SkillDef passion in newTrait.forcedPassions)
                {
                    skills.GetSkill(passion).passion = Passion.None;
                }
            }

        }
        public static BackstoryDef GetBackstoryDef(VEF.Abilities.AbilityDef def, Rarity rarity)
        {
            if (def == null)
            {
                return null;
            }
            if (!def.HasModExtension<RerollExtension>())
            {
                return null;
            }

            RerollExtension ext = def.GetModExtension<RerollExtension>();
            BackstoryDef backstory = null;
            switch (rarity)
            {
                case Rarity.Common:
                    backstory = ext.backgrounds.common.RandomElement();
                    break;
                case Rarity.Rare:
                    backstory = ext.backgrounds.rare.RandomElement();
                    break;
                case Rarity.Epic:
                    backstory = ext.backgrounds.epic.RandomElement();
                    break;
                case Rarity.Legendary:
                    backstory = ext.backgrounds.legendary.RandomElement();
                    break;
            }
            return backstory;

        }
        public static TraitDef GetTraitDef(VEF.Abilities.AbilityDef def, Rarity rarity, out int degree)
        {
            if (def == null)
            {
                Log.Message("def null");
                degree = 0;
                return null;
            }
            if (!def.HasModExtension<RerollExtension>())
            {
                Log.Message("no extension");

                degree = 0;
                return null;
            }

            RerollExtension ext = def.GetModExtension<RerollExtension>();
            TraitDegree trait = new TraitDegree { def = "Wimp", degree = 0 };
            switch (rarity)
            {
                case Rarity.Common:
                    trait = ext.traits.common.RandomElement();
                    break;
                case Rarity.Rare:
                    trait = ext.traits.rare.RandomElement();
                    break;
                case Rarity.Epic:
                    trait = ext.traits.epic.RandomElement();
                    break;
                case Rarity.Legendary:
                    trait = ext.traits.legendary.RandomElement();
                    break;
            }
            degree = trait.degree;
            Log.Message(trait.def);
            return  DefDatabase<TraitDef>.GetNamed(trait.def);
        }

        public static IntVec3 GetCellToAmbushTeleport(Ability_AmbushPortal ability, GlobalTargetInfo target)
        {
            if (!target.HasThing)
            {
                return target.Cell;
            }

            IntVec3 pos = target.Cell + target.Thing.Rotation.Opposite.FacingCell;
            return pos.WalkableBy(ability.pawn.Map, ability.pawn) ? pos : target.Cell;
        }

        public static void ApplySpeedHediff(Pawn pawn, float psychicSensitivity)
        {
            Hediff hediff = HediffMaker.MakeHediff(DefOfs.Iconian_MovementSpeedBoost, pawn);
            HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
            comp.SetDuration((int)Mathf.Min(comp.disappearsAfterTicks * psychicSensitivity, IconianPsycasts_Mod.settings.MovementSpeedBoostDurationSecondsCap * 60));
            hediff.Severity *= psychicSensitivity;
            pawn.health.AddHediff(hediff);
        }
        public static void ApplyAttackSpeedHediff(Pawn pawn, float psychicSensitivity)
        {
            Hediff hediff = HediffMaker.MakeHediff(DefOfs.Iconian_AttackSpeedBoost, pawn);
            HediffComp_Disappears comp = hediff.TryGetComp<HediffComp_Disappears>();
            comp.SetDuration((int)Mathf.Min(comp.disappearsAfterTicks * psychicSensitivity, IconianPsycasts_Mod.settings.MovementSpeedBoostDurationSecondsCap * 60));
            hediff.Severity *= psychicSensitivity;
            pawn.health.AddHediff(hediff);
        }
        public static void ApplyStun(Pawn pawn, float psychicSensitivity)
        {

            pawn.stances?.stunner.StunFor((int)(180 * psychicSensitivity), pawn);
        }
    }
}
