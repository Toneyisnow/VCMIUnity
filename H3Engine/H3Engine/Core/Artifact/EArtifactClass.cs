// Migrated from VCMI lib/entities/artifact/EArtifactClass.h

using System;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// Artifact rarity / class tier.
    /// Defined as a [Flags] enum to allow bitmask queries (e.g. "is treasure or minor").
    /// Corresponds to VCMI's EArtifactClass.
    /// </summary>
    [Flags]
    public enum EArtifactClass
    {
        ART_SPECIAL  = 1,   // non-randomisable (grail, war machines, spell scrolls, etc.)
        ART_TREASURE = 2,
        ART_MINOR    = 4,
        ART_MAJOR    = 8,
        ART_RELIC    = 16,
    }
}


