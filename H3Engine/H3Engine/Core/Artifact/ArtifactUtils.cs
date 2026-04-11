// Migrated from VCMI lib/entities/artifact/ArtifactUtils.h/.cpp
// Static helpers for artifact slot queries, position classification,
// and combined-artifact assembly detection.

using H3Engine.Common;
using H3Engine.Core.Constants;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// Stateless utility methods for artifact placement logic.
    /// Corresponds to the <c>ArtifactUtils</c> namespace in VCMI.
    /// </summary>
    public static class ArtifactUtils
    {
        // ------------------------------------------------------------------ //
        //  Slot classification                                                  //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true when <paramref name="slot"/> is a backpack position
        /// (i.e. >= BACKPACK_START).
        /// Corresponds to ArtifactUtils::isSlotBackpack.
        /// </summary>
        public static bool IsSlotBackpack(EArtifactPosition slot)
            => slot >= EArtifactPosition.BACKPACK_START;

        /// <summary>
        /// Returns true when <paramref name="slot"/> is an equipment (worn) position
        /// (i.e. in range [0, BACKPACK_START)).
        /// Corresponds to ArtifactUtils::isSlotEquipment.
        /// </summary>
        public static bool IsSlotEquipment(EArtifactPosition slot)
            => slot >= 0 && slot < EArtifactPosition.BACKPACK_START;

        /// <summary>
        /// Returns true when the slot is one of the unmovable worn slots
        /// (SPELLBOOK, MACH4). These can never be taken off via normal UI.
        /// Corresponds to ArtifactUtils::unmovableSlots.
        /// </summary>
        public static bool IsUnmovableSlot(EArtifactPosition slot)
            => slot == EArtifactPosition.SPELLBOOK || slot == EArtifactPosition.MACH4;

        // ------------------------------------------------------------------ //
        //  Slot lists (mirrors VCMI's static vectors)                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// All worn slots that are movable (excludes war machines and spellbook).
        /// Corresponds to ArtifactUtils::commonWornSlots.
        /// </summary>
        public static readonly IReadOnlyList<EArtifactPosition> CommonWornSlots =
            new List<EArtifactPosition>
            {
                EArtifactPosition.HEAD, EArtifactPosition.SHOULDERS, EArtifactPosition.NECK,
                EArtifactPosition.RIGHT_HAND, EArtifactPosition.LEFT_HAND, EArtifactPosition.TORSO,
                EArtifactPosition.RIGHT_RING, EArtifactPosition.LEFT_RING, EArtifactPosition.FEET,
                EArtifactPosition.MISC1, EArtifactPosition.MISC2, EArtifactPosition.MISC3,
                EArtifactPosition.MISC4, EArtifactPosition.MISC5,
            };

        /// <summary>
        /// All hero worn slots including war machines and spellbook.
        /// Corresponds to ArtifactUtils::allWornSlots.
        /// </summary>
        public static readonly IReadOnlyList<EArtifactPosition> AllWornSlots =
            new List<EArtifactPosition>
            {
                EArtifactPosition.HEAD, EArtifactPosition.SHOULDERS, EArtifactPosition.NECK,
                EArtifactPosition.RIGHT_HAND, EArtifactPosition.LEFT_HAND, EArtifactPosition.TORSO,
                EArtifactPosition.RIGHT_RING, EArtifactPosition.LEFT_RING, EArtifactPosition.FEET,
                EArtifactPosition.MISC1, EArtifactPosition.MISC2, EArtifactPosition.MISC3,
                EArtifactPosition.MISC4, EArtifactPosition.MISC5,
                EArtifactPosition.MACH1, EArtifactPosition.MACH2, EArtifactPosition.MACH3,
                EArtifactPosition.MACH4, EArtifactPosition.SPELLBOOK,
            };

        /// <summary>
        /// Commander worn slots (COMMANDER1–COMMANDER9).
        /// Corresponds to ArtifactUtils::commanderSlots.
        /// </summary>
        public static readonly IReadOnlyList<EArtifactPosition> CommanderSlots =
            new List<EArtifactPosition>
            {
                EArtifactPosition.COMMANDER1, EArtifactPosition.COMMANDER2,
                EArtifactPosition.COMMANDER3, EArtifactPosition.COMMANDER4,
                EArtifactPosition.COMMANDER5, EArtifactPosition.COMMANDER6,
                EArtifactPosition.COMMANDER7, EArtifactPosition.COMMANDER8,
                EArtifactPosition.COMMANDER9,
            };

        // ------------------------------------------------------------------ //
        //  Slot validity                                                        //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true when <paramref name="slot"/> is a valid position for the
        /// given bearer's <paramref name="artSet"/>.
        /// Corresponds to ArtifactUtils::checkIfSlotValid.
        /// </summary>
        public static bool CheckIfSlotValid(ArtifactSet artSet, EArtBearer bearer, EArtifactPosition slot)
        {
            switch (bearer)
            {
                case EArtBearer.HERO:
                    return IsSlotEquipment(slot) || IsSlotBackpack(slot)
                           || slot == EArtifactPosition.TRANSITION_POS;

                case EArtBearer.ALTAR:
                    return IsSlotBackpack(slot);

                case EArtBearer.COMMANDER:
                    foreach (var s in CommanderSlots)
                        if (s == slot) return true;
                    return false;

                case EArtBearer.CREATURE:
                    return slot == EArtifactPosition.CREATURE_SLOT;

                default:
                    return false;
            }
        }

        // ------------------------------------------------------------------ //
        //  Position search                                                      //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns the first equipped (worn) position that holds <paramref name="artId"/>,
        /// or PRE_FIRST if not equipped.
        /// Corresponds to ArtifactUtils::getArtEquippedPosition.
        /// </summary>
        public static EArtifactPosition GetArtEquippedPosition(ArtifactSet artSet, EArtifactId artId)
        {
            foreach (var kvp in artSet.ArtifactsWorn)
                if (!kvp.Value.Locked && kvp.Value.ArtifactId == artId)
                    return kvp.Key;
            return EArtifactPosition.PRE_FIRST;
        }

        /// <summary>
        /// Returns the first backpack position (as BACKPACK_START + index) that holds
        /// <paramref name="artId"/>, or PRE_FIRST if not found.
        /// Corresponds to ArtifactUtils::getArtBackpackPosition.
        /// </summary>
        public static EArtifactPosition GetArtBackpackPosition(ArtifactSet artSet, EArtifactId artId)
        {
            for (int i = 0; i < artSet.ArtifactsInBackpack.Count; i++)
                if (artSet.ArtifactsInBackpack[i].ArtifactId == artId)
                    return (EArtifactPosition)((sbyte)EArtifactPosition.BACKPACK_START + i);
            return EArtifactPosition.PRE_FIRST;
        }

        /// <summary>
        /// Searches worn slots first, then backpack. Returns PRE_FIRST if not found.
        /// Corresponds to ArtifactUtils::getArtAnyPosition.
        /// </summary>
        public static EArtifactPosition GetArtAnyPosition(ArtifactSet artSet, EArtifactId artId)
        {
            var pos = GetArtEquippedPosition(artSet, artId);
            return pos != EArtifactPosition.PRE_FIRST ? pos
                   : GetArtBackpackPosition(artSet, artId);
        }

        // ------------------------------------------------------------------ //
        //  Removability                                                         //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true if the artifact in <paramref name="slot"/> can be removed
        /// by the player (not locked, not an unmovable slot, and slot is non-empty).
        /// Corresponds to ArtifactUtils::isArtRemovable.
        /// </summary>
        public static bool IsArtRemovable(EArtifactPosition pos, ArtSlotInfo slot)
            => slot != null && slot.ArtifactId != EArtifactId.NONE
               && !slot.Locked && !IsUnmovableSlot(pos);

        // ------------------------------------------------------------------ //
        //  Backpack capacity                                                    //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true if the backpack of the given hero <paramref name="artSet"/>
        /// has room for at least <paramref name="reqSlots"/> more artifacts.
        /// Pass <c>backpackCap &lt; 0</c> for unlimited capacity (VCMI default).
        /// Corresponds to ArtifactUtils::isBackpackFreeSlots.
        /// </summary>
        public static bool IsBackpackFreeSlots(ArtifactSet artSet, EArtBearer bearer, int reqSlots = 1, int backpackCap = -1)
        {
            if (bearer != EArtBearer.HERO)
                return false;
            if (backpackCap < 0)
                return true;
            return artSet.ArtifactsInBackpack.Count + reqSlots <= backpackCap;
        }

        // ------------------------------------------------------------------ //
        //  Combined artifact assembly                                           //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns all combined <see cref="ArtifactType"/> instances that could be
        /// assembled right now given the artifacts already held in <paramref name="artSet"/>.
        ///
        /// Works by iterating every combined artifact that <paramref name="artType"/> is
        /// a constituent of, then using an <see cref="ArtifactFittingSet"/> to simulate
        /// locking one slot per constituent to avoid counting the same equipped copy twice.
        ///
        /// Pass <c>onlyEquipped = true</c> to ignore backpack artifacts.
        ///
        /// Corresponds to ArtifactUtils::assemblyPossibilities.
        /// </summary>
        public static List<ArtifactType> AssemblyPossibilities(
            ArtifactSet artSet,
            EArtBearer bearer,
            ArtifactType artType,
            IReadOnlyList<ArtifactType> allArtifactTypes,
            bool onlyEquipped = false)
        {
            var result = new List<ArtifactType>();

            // Combined artifacts themselves cannot trigger further assembly
            if (artType.IsCombined())
                return result;

            // Find every combined artifact that lists artType as a constituent
            foreach (var candidate in allArtifactTypes)
            {
                if (!candidate.IsCombined()) continue;
                if (candidate.Constituents == null) continue;

                bool containsThisArt = false;
                foreach (var cid in candidate.Constituents)
                    if (cid == artType.Id) { containsThisArt = true; break; }
                if (!containsThisArt) continue;

                // Simulate: create a fitting copy of the set and lock one slot per part
                var fitting = new ArtifactFittingSet(artSet, bearer);
                bool possible = true;

                foreach (var constituentId in candidate.Constituents)
                {
                    var slot = onlyEquipped
                        ? fitting.GetEquippedArtPos(constituentId)
                        : fitting.GetArtPos(constituentId, onlyEquipped: false);

                    if (slot == EArtifactPosition.PRE_FIRST)
                    {
                        possible = false;
                        break;
                    }

                    fitting.LockSlot(slot);
                }

                if (possible)
                    result.Add(candidate);
            }

            return result;
        }

        // ------------------------------------------------------------------ //
        //  Spellbook check                                                      //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true when equipping <paramref name="artId"/> at <paramref name="slot"/>
        /// should also grant the hero a spellbook (Titan's Thunder special case).
        /// Corresponds to ArtifactUtils::checkSpellbookIsNeeded.
        /// </summary>
        public static bool CheckSpellbookIsNeeded(ArtifactSet artSet, EArtifactId artId, EArtifactPosition slot)
        {
            if (artId == EArtifactId.TITANS_THUNDER && slot == EArtifactPosition.RIGHT_HAND)
                return !artSet.HasArtEquipped(EArtifactId.SPELLBOOK);
            return false;
        }
    }
}
