// Migrated from VCMI lib/entities/artifact/ArtSlotInfo.h
// Describes a single equipment slot: which artifact occupies it and whether it is locked.

using H3Engine.Common;

namespace H3Engine.Core
{
    /// <summary>
    /// State of one artifact equipment slot on a bearer (hero, creature, commander).
    /// A slot is "locked" when it belongs to a combined artifact – the constituent
    /// artifact occupies one slot while every other required slot is locked and points
    /// back to the combined artifact's primary slot.
    ///
    /// Corresponds to VCMI's ArtSlotInfo struct.
    /// </summary>
    public class ArtSlotInfo
    {
        /// <summary>
        /// ID of the artifact occupying this slot, or <see cref="EArtifactId.NONE"/> if empty.
        /// For locked constituent slots this holds the combined artifact's ID.
        /// </summary>
        public EArtifactId ArtifactId
        {
            get; set;
        } = EArtifactId.NONE;

        /// <summary>
        /// Instance ID of the specific artifact copy in this slot (-1 = no instance / not tracked).
        /// Corresponds to ArtSlotInfo::artifactID (ArtifactInstanceID) in VCMI.
        /// </summary>
        public int ArtifactInstanceId
        {
            get; set;
        } = -1;

        /// <summary>
        /// True when this slot is locked by a combined artifact.
        /// A locked slot cannot be used for equipping another artifact independently.
        /// </summary>
        public bool Locked
        {
            get; set;
        }

        /// <summary>Returns true if the slot is empty and not locked.</summary>
        public bool IsEmpty => ArtifactId == EArtifactId.NONE && !Locked;
    }
}
