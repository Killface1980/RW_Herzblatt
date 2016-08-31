using System;
using RimWorld;
using Verse;

namespace RW_Herzblatt
{
    public class ThoughtWorker_Disfigured : ThoughtWorker
    {
        protected override ThoughtState CurrentSocialStateInternal(Pawn pawn, Pawn other)
        {
            if (!other.RaceProps.Humanlike || other.Dead)
            {
                return false;
            }
            if (!RelationsUtility.PawnsKnowEachOther(pawn, other))
            {
                return false;
            }

            if (RelationsUtility.IsDisfigured(pawn) && RelationsUtility.IsDisfigured(other))
            {
                return ThoughtState.ActiveAtStage(1);
            }

            if (!RelationsUtility.IsDisfigured(other))
            {
                return false;
            }
            return ThoughtState.ActiveAtStage(0);
        }
    }
}
