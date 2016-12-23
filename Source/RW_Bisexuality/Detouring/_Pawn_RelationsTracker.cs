using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_Herzblatt.Detouring
{
    public static class _Pawn_RelationsTracker
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this Pawn_RelationsTracker _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_RelationsTracker.pawn!", 0x12348765);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }


        internal static float OutcastScore(Pawn pawn)
        {
            float num = 1f;
            if (pawn.story.traits.HasTrait(TraitDefOf.AnnoyingVoice))
                num += 0.25f;
            if (pawn.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
                num += 0.25f;
            if (RelationsUtility.IsDisfigured(pawn))
                num += 0.15f;
            if (pawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty) == -1)
                num += 0.2f;
            if (pawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty) == -2)
                num += 0.8f;
            return num;
        }

        [Detour(typeof(Pawn_RelationsTracker), bindingFlags = (BindingFlags.Instance | BindingFlags.Public))]
        internal static float SecondaryRomanceChanceFactor(this Pawn_RelationsTracker _this, Pawn otherPawn)
        {
            Pawn pawn = _this.GetPawn();
            bool match = PawnsAreValidMatches(pawn, otherPawn);
            if (!match)
            {
                return 0f;
            }

            Rand.PushSeed();
            Rand.Seed = pawn.HashOffset();
            bool flag = Rand.Value < 0.015f;
            Rand.PopSeed();

            float sexualityMod = 1f;
            float ageMod = 1f;
            float pawnAgeBiological = pawn.ageTracker.AgeBiologicalYearsFloat;
            float otherPawnAgeBiological = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if (pawn.gender == Gender.Male)
            {
                // if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                if (!flag)
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                        {
                            if (otherPawn.gender == Gender.Female)
                            {
                                return 0f;
                            }
                        }
                    }
                    else
                    {
                        if (!pawn.story.traits.HasTrait(TraitDefOfHerzblatt.Bisexual) && otherPawn.gender == Gender.Male)
                        {
                            sexualityMod = 0f;
                        }
                    } 
                }
                ageMod = GenMath.FlatHill(0f, 16f, 20f, pawnAgeBiological, pawnAgeBiological + 15f,0.07f, otherPawnAgeBiological);
            }
            else if (pawn.gender == Gender.Female)
            {
                if (!flag)
                {
                    if (pawn.story.traits.HasTrait(TraitDefOf.Gay))
                    {
                        if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
                        {
                            if (otherPawn.gender == Gender.Male)
                            {
                                return 0f;
                            }
                        }
                    }
                    else
                    {
                        if (!pawn.story.traits.HasTrait(TraitDefOfHerzblatt.Bisexual) && otherPawn.gender == Gender.Female)
                        {
                            sexualityMod = 0f;
                        }
                    } 
                }

                if (otherPawnAgeBiological < pawnAgeBiological - 10f)
                {
                    return 0f;
                }
                if (otherPawnAgeBiological < pawnAgeBiological - 3f)
                {
                    ageMod = Mathf.InverseLerp(pawnAgeBiological - 10f, pawnAgeBiological - 3f, otherPawnAgeBiological) * 0.2f;
                }
                else
                {
                    ageMod = GenMath.FlatHill(0.2f, pawnAgeBiological - 3f, pawnAgeBiological, pawnAgeBiological + 10f, pawnAgeBiological + 30f, 0.1f, otherPawnAgeBiological);
                }
            }
            float interactionMod = 1f;
            interactionMod *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Talking));
            interactionMod *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Manipulation));
            interactionMod *= Mathf.Lerp(0.2f, 1f, otherPawn.health.capacities.GetEfficiency(PawnCapacityDefOf.Moving));
            float attractionMod = 1f;
            foreach (PawnRelationDef current in pawn.GetRelations(otherPawn))
            {
                attractionMod *= current.attractionFactor;
            }

            //Outcasts
            float outCast = 1f;
            if (OutcastScore(pawn) > OutcastScore(otherPawn))
                outCast *= OutcastScore(pawn) * OutcastScore(otherPawn);
            if (OutcastScore(pawn) != 1f && OutcastScore(otherPawn) >= OutcastScore(pawn))
                outCast *= 0.5f * OutcastScore(pawn) * OutcastScore(otherPawn);

            // Beauty
            int otherPawnDegreeOfBeauty = 0;
            int pawnDegreeOfBeauty = pawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            if (otherPawn.RaceProps.Humanlike)
            {
                otherPawnDegreeOfBeauty = otherPawn.story.traits.DegreeOfTrait(TraitDefOf.Beauty);
            }
            float beautyMod = 1f;
            if (otherPawnDegreeOfBeauty < 0)
            {
                if (pawnDegreeOfBeauty < otherPawnDegreeOfBeauty)
                {
                    beautyMod = 1.6f;
                }
                else if (pawnDegreeOfBeauty == otherPawnDegreeOfBeauty)
                {
                    beautyMod = 0.8f;
                }
                else
                {
                    beautyMod = 0.3f;
                }
            }
            else if (otherPawnDegreeOfBeauty == 1)
            {
                beautyMod = 1.6f;
            }
            else if (otherPawnDegreeOfBeauty == 2)
            {
                beautyMod = 2.4f;
            }
            float num7 = Mathf.InverseLerp(15f, 18f, pawnAgeBiological);
            float num8 = Mathf.InverseLerp(15f, 18f, otherPawnAgeBiological);
            return sexualityMod * ageMod * interactionMod * attractionMod * num7 * num8 * beautyMod * outCast;
        }

        internal static bool PawnsAreValidMatches(Pawn pawn1, Pawn pawn2)
        {
            /*
Log.Message(
    string.Format(
        "PawnsAreValidMatches( {0}, {1} )\n\t{0} is human like: {2}\n\t{1} is human like: {3}\n\t{0} gender: {4}\n\t{1} gender: {5}\n\t{0} is gay: {6}\n\t{1} is gay: {7}\n\t{0} PawnKindDef: {8}\n\t{1} PawnKindDef: {9}",
        pawn1.LabelShort,
        pawn2.LabelShort,
        pawn1.RaceProps.Humanlike,
        pawn2.RaceProps.Humanlike,
        pawn1.gender,
        pawn2.gender,
        pawn1.story.traits.HasTrait(TraitDefOf.Gay),
        pawn2.story.traits.HasTrait(TraitDefOf.Gay),
        pawn1.kindDef.defName,
        pawn2.kindDef.defName
    ) );
*/

            if (!pawn1.RaceProps.Humanlike || (pawn1.RaceProps.Humanlike && !pawn2.RaceProps.Humanlike) ||
                pawn1 == pawn2 || pawn1.RaceProps.fleshType != FleshType.Normal ||
                pawn2.RaceProps.fleshType > FleshType.Normal)
                return false;
            return true;
        }

        [Detour(typeof(Pawn_RelationsTracker), bindingFlags = (BindingFlags.Instance | BindingFlags.Public))]
        internal static float CompatibilityWith(this Pawn_RelationsTracker _this, Pawn otherPawn)
        {
            var pawn = _this.GetPawn();
            if (!PawnsAreValidMatches(pawn, otherPawn))
            {
                return 0f;
            }
            float x = Mathf.Abs(pawn.ageTracker.AgeBiologicalYearsFloat - otherPawn.ageTracker.AgeBiologicalYearsFloat);
            float num = GenMath.LerpDouble(0f, 20f, 0.45f, -0.45f, x);
            num = Mathf.Clamp(num, -0.45f, 0.45f);
            float num2 = _this.ConstantPerPawnsPairCompatibilityOffset(otherPawn.thingIDNumber);
            return num + num2;
        }

    }
}
