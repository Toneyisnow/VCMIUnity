// Moved from H3Engine.Components.Data.ArtifactSet → H3Engine.Core
// (VCMI: lib/entities/artifact/CArtifactSet.h)

using H3Engine.Common;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// The complete set of artifacts carried by one bearer (hero, creature, commander).
    /// Tracks both equipped slots and the backpack.
    /// Corresponds to VCMI's CArtifactSet class.
    /// </summary>
    public class ArtifactSet
    {
        public ArtifactSet()
        {
            ArtifactsWorn       = new Dictionary<EArtifactPosition, ArtSlotInfo>();
            ArtifactsInBackpack = new List<ArtSlotInfo>();
            ArtifactsTransitionPos = new ArtSlotInfo();
        }

        /// <summary>
        /// Worn equipment slots keyed by position (HEAD … MISC5).
        /// Corresponds to CArtifactSet::artifactsWorn.
        /// </summary>
        public Dictionary<EArtifactPosition, ArtSlotInfo> ArtifactsWorn
        {
            get; set;
        }

        /// <summary>
        /// Artifacts in the hero's backpack, ordered by insertion.
        /// Corresponds to CArtifactSet::artifactsInBackpack.
        /// </summary>
        public List<ArtSlotInfo> ArtifactsInBackpack
        {
            get; set;
        }

        /// <summary>
        /// Temporary slot for drag-and-drop artifact swapping in the UI.
        /// Corresponds to CArtifactSet::artifactsTransitionPos.
        /// </summary>
        public ArtSlotInfo ArtifactsTransitionPos
        {
            get; set;
        }

        // ── Queries ───────────────────────────────────────────────────────────

        public ArtSlotInfo GetSlot(EArtifactPosition pos)
        {
            ArtifactsWorn.TryGetValue(pos, out var slot);
            return slot;
        }

        public bool IsPositionFree(EArtifactPosition pos)
        {
            if (!ArtifactsWorn.TryGetValue(pos, out var slot))
                return true;
            return slot.IsEmpty;
        }

        public EArtifactId GetArtId(EArtifactPosition pos)
        {
            if (ArtifactsWorn.TryGetValue(pos, out var slot) && !slot.Locked)
                return slot.ArtifactId;
            return EArtifactId.NONE;
        }

        public bool HasArtEquipped(EArtifactId artId)
        {
            foreach (var slot in ArtifactsWorn.Values)
                if (!slot.Locked && slot.ArtifactId == artId)
                    return true;
            return false;
        }

        public bool HasArt(EArtifactId artId)
        {
            if (HasArtEquipped(artId)) return true;
            foreach (var slot in ArtifactsInBackpack)
                if (slot.ArtifactId == artId)
                    return true;
            return false;
        }

        // ── Mutations ─────────────────────────────────────────────────────────

        public void PutAt(EArtifactId artId, EArtifactPosition pos, int instanceId = -1)
        {
            ArtifactsWorn[pos] = new ArtSlotInfo
            {
                ArtifactId         = artId,
                ArtifactInstanceId = instanceId,
                Locked             = false,
            };
        }

        public void RemoveFrom(EArtifactPosition pos)
        {
            if (ArtifactsWorn.TryGetValue(pos, out var slot))
            {
                ArtifactsWorn.Remove(pos);
                if (slot.ArtifactId != EArtifactId.NONE)
                {
                    var toUnlock = new List<EArtifactPosition>();
                    foreach (var kvp in ArtifactsWorn)
                        if (kvp.Value.Locked && kvp.Value.ArtifactId == slot.ArtifactId)
                            toUnlock.Add(kvp.Key);
                    foreach (var lockPos in toUnlock)
                        ArtifactsWorn.Remove(lockPos);
                }
            }
        }

        public bool CanPutAt(EArtifactId artId, EArtifactPosition pos)
            => IsPositionFree(pos);

        public void AddToBackpack(EArtifactId artId, int instanceId = -1)
        {
            ArtifactsInBackpack.Add(new ArtSlotInfo
            {
                ArtifactId         = artId,
                ArtifactInstanceId = instanceId,
            });
        }
    }
}
