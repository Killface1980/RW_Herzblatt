using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace RW_Herzblatt.Detouring
{
    internal static class _Pawn_RelationsTracker
    {
        internal static FieldInfo _pawn;

        internal static Pawn GetPawn(this Pawn_RelationsTracker _this)
        {
            if (_pawn == null)
            {
                _pawn = typeof(Pawn_RelationsTracker).GetField("pawn", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_pawn == null)
                {
                    Log.ErrorOnce("Unable to reflect Pawn_RelationsTracker.pawn!", 305432421);
                }
            }
            return (Pawn)_pawn.GetValue(_this);
        }

        [Detour(typeof(Pawn_RelationsTracker), bindingFlags = (BindingFlags.Instance | BindingFlags.Public))]
        public static float AttractionTo(this Pawn_RelationsTracker _this, Pawn otherPawn)
        {
            var pawn = _this.GetPawn();

            if (pawn.def != otherPawn.def || pawn == otherPawn)
            {
                return 0f;
            }
            float sexualityMod = 1f;
            float ageMod = 1f;
            float pawnAgeBiological = pawn.ageTracker.AgeBiologicalYearsFloat;
            float otherPawnAgeBiological = otherPawn.ageTracker.AgeBiologicalYearsFloat;
            if (pawn.gender == Gender.Male)
            {
                // if (pawn.RaceProps.Humanlike && pawn.story.traits.HasTrait(TraitDefOf.Gay))
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
                    if (!pawn.story.traits.HasTrait(TraitDef.Named("Bisexual"))&& otherPawn.gender == Gender.Male)
                    {
                        sexualityMod = 0.03f;
                    }
                }
                ageMod = GenMath.FlatHill(16f, 20f, pawnAgeBiological, pawnAgeBiological + 15f, otherPawnAgeBiological);
            }
            else if (pawn.gender == Gender.Female)
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
                    if (!pawn.story.traits.HasTrait(TraitDef.Named("Bisexual")) && otherPawn.gender == Gender.Female)
                    {
                        sexualityMod = 0.15f;
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
                    ageMod = GenMath.FlatHill(0.2f, pawnAgeBiological - 3f, pawnAgeBiological, pawnAgeBiological + 10f, pawnAgeBiological + 40f, 0.1f, otherPawnAgeBiological);
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
                    beautyMod = 1.2f;
                }
                else
                {
                    beautyMod = 0.3f;
                }
            }
            else if (otherPawnDegreeOfBeauty == 1)
            {
                beautyMod = 1.8f;
            }
            else if (otherPawnDegreeOfBeauty == 2)
            {
                beautyMod = 2.4f;
            }
            float num7 = Mathf.InverseLerp(15f, 18f, pawnAgeBiological);
            float num8 = Mathf.InverseLerp(15f, 18f, otherPawnAgeBiological);
            return sexualityMod * ageMod * interactionMod * attractionMod * num7 * num8 * beautyMod;
        }

    }
}
