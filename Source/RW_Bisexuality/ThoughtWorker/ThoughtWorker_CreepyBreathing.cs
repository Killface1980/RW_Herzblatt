using System;
using RimWorld;
using Verse;

namespace RW_Herzblatt
{
    public class ThoughtWorker_CreepyBreathing : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || !RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }

            if (pawn.story.traits.HasTrait(TraitDefOf.CreepyBreathing) && other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
                return ThoughtState.ActiveAtStage(1);

            if (!other.story.traits.HasTrait(TraitDefOf.CreepyBreathing))
            {
                return false;
            }
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
