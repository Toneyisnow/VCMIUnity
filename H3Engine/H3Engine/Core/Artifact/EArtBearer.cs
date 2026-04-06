// Migrated from VCMI lib/entities/artifact/ArtBearer.h
// Identifies which entity type can equip an artifact.

namespace H3Engine.Core
{
    /// <summary>
    /// The type of entity that can wear an artifact.
    /// Each <see cref="ArtifactType"/> carries a set of allowed slots per bearer.
    /// Corresponds to VCMI's ArtBearer enum.
    /// </summary>
    public enum EArtBearer
    {
        HERO      = 0,
        CREATURE  = 1,
        COMMANDER = 2,
        ALTAR     = 3,
    }
}
