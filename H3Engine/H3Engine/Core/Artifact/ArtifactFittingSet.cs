// Migrated from VCMI lib/entities/artifact/CArtifactFittingSet.h/.cpp
// A disposable copy of an ArtifactSet used to simulate artifact placement
// (e.g. for assembly checks) without touching the real hero state.

using H3Engine.Common;
using H3Engine.Core.Constants;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// A shallow copy of an <see cref="ArtifactSet"/> with a fixed bearer type,
    /// used to "try on" artifact arrangements before committing changes.
    ///
    /// Typical usage (mirrors VCMI's ArtifactUtils::assemblyPossibilities):
    /// <code>
    ///   var fitting = new ArtifactFittingSet(heroArtifactSet);
    ///   fitting.LockSlot(someSlot);
    ///   // … test whether a combined artifact can be assembled …
    /// </code>
    ///
    /// Corresponds to VCMI's CArtifactFittingSet class.
    /// </summary>
    public class ArtifactFittingSet : ArtifactSet
    {
        private readonly EArtBearer bearer;

        // ------------------------------------------------------------------ //
        //  Construction                                                        //
        // ------------------------------------------------------------------ //

        /// <summary>Creates an empty fitting set for the given bearer type.</summary>
        public ArtifactFittingSet(EArtBearer bearer)
        {
            this.bearer = bearer;
        }

        /// <summary>
        /// Creates a fitting set that is a shallow copy of <paramref name="source"/>.
        /// The bearer type and all slot states are copied; subsequent mutations do
        /// not affect the original set.
        /// </summary>
        public ArtifactFittingSet(ArtifactSet source, EArtBearer bearer) : this(bearer)
        {
            // Deep-copy worn slots so mutations stay local
            foreach (var kvp in source.ArtifactsWorn)
                ArtifactsWorn[kvp.Key] = new ArtSlotInfo
                {
                    ArtifactId         = kvp.Value.ArtifactId,
                    ArtifactInstanceId = kvp.Value.ArtifactInstanceId,
                    Locked             = kvp.Value.Locked,
                };

            // Shallow-copy backpack (read-only for assembly checks)
            foreach (var slot in source.ArtifactsInBackpack)
                ArtifactsInBackpack.Add(new ArtSlotInfo
                {
                    ArtifactId         = slot.ArtifactId,
                    ArtifactInstanceId = slot.ArtifactInstanceId,
                    Locked             = slot.Locked,
                });

            ArtifactsTransitionPos = source.ArtifactsTransitionPos;
        }

        // ------------------------------------------------------------------ //
        //  Bearer                                                              //
        // ------------------------------------------------------------------ //

        /// <summary>The bearer type this fitting set represents.</summary>
        public EArtBearer BearerType => bearer;

        // ------------------------------------------------------------------ //
        //  Simulation helpers                                                  //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Marks <paramref name="pos"/> as locked, simulating that a combined
        /// artifact constituent is "claiming" this slot.
        /// Used by <see cref="ArtifactUtils.AssemblyPossibilities"/> to walk the
        /// constituent list without double-counting a single equipped copy.
        /// </summary>
        public void LockSlot(EArtifactPosition pos)
        {
            if (ArtifactsWorn.TryGetValue(pos, out var slot))
            {
                ArtifactsWorn[pos] = new ArtSlotInfo
                {
                    ArtifactId         = slot.ArtifactId,
                    ArtifactInstanceId = slot.ArtifactInstanceId,
                    Locked             = true,
                };
            }
        }

        /// <summary>
        /// Returns the first equipped (non-backpack) position that holds
        /// <paramref name="artId"/> and is not locked, or
        /// <see cref="EArtifactPosition.PRE_FIRST"/> if none.
        /// Corresponds to CArtifactSet::getArtPos(..., onlyEquipped=true).
        /// </summary>
        public EArtifactPosition GetEquippedArtPos(EArtifactId artId)
        {
            foreach (var kvp in ArtifactsWorn)
                if (!kvp.Value.Locked && kvp.Value.ArtifactId == artId)
                    return kvp.Key;
            return EArtifactPosition.PRE_FIRST;
        }

        /// <summary>
        /// Returns the first backpack position that holds <paramref name="artId"/>,
        /// expressed as BACKPACK_START + index, or PRE_FIRST if not found.
        /// </summary>
        public EArtifactPosition GetBackpackArtPos(EArtifactId artId)
        {
            for (int i = 0; i < ArtifactsInBackpack.Count; i++)
                if (ArtifactsInBackpack[i].ArtifactId == artId)
                    return (EArtifactPosition)((sbyte)EArtifactPosition.BACKPACK_START + i);
            return EArtifactPosition.PRE_FIRST;
        }

        /// <summary>
        /// Returns the position of <paramref name="artId"/> searching worn slots first,
        /// then backpack. Returns PRE_FIRST if not found.
        /// Corresponds to CArtifactSet::getArtPos(..., onlyEquipped=false).
        /// </summary>
        public EArtifactPosition GetArtPos(EArtifactId artId, bool onlyEquipped = false)
        {
            var pos = GetEquippedArtPos(artId);
            if (pos != EArtifactPosition.PRE_FIRST || onlyEquipped)
                return pos;
            return GetBackpackArtPos(artId);
        }
    }
}
