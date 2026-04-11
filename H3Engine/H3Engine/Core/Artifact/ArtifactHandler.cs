// Migrated from VCMI lib/entities/artifact/CArtHandler.h/.cpp
// Registry that owns all ArtifactType definitions loaded from game data.
// Replaces the former H3Artifact placeholder and provides slot/class helpers
// that mirror CArtHandler's static methods.

using H3Engine.Common;
using H3Engine.Core.Constants;
using System;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// Owns the master list of all <see cref="ArtifactType"/> definitions and
    /// provides lookup and validation helpers.
    ///
    /// In H3Engine the handler is populated by the map / data loaders rather than
    /// by JSON config; the interface intentionally mirrors CArtHandler so the rest
    /// of the engine can be ported from VCMI with minimal changes.
    ///
    /// Corresponds to VCMI's CArtHandler class.
    /// </summary>
    public class ArtifactHandler
    {
        // ------------------------------------------------------------------ //
        //  Registry                                                             //
        // ------------------------------------------------------------------ //

        private readonly Dictionary<EArtifactId, ArtifactType> objects
            = new Dictionary<EArtifactId, ArtifactType>();

        /// <summary>All loaded artifact type definitions, keyed by ID.</summary>
        public IReadOnlyDictionary<EArtifactId, ArtifactType> Objects => objects;

        /// <summary>Registers or replaces the definition for <paramref name="art"/>.</summary>
        public void Register(ArtifactType art)
        {
            if (art == null) throw new ArgumentNullException(nameof(art));
            objects[art.Id] = art;
        }

        /// <summary>
        /// Returns the <see cref="ArtifactType"/> for <paramref name="id"/>,
        /// or null if not registered.
        /// </summary>
        public ArtifactType GetById(EArtifactId id)
        {
            objects.TryGetValue(id, out var art);
            return art;
        }

        // ------------------------------------------------------------------ //
        //  Slot helpers (mirrors CArtHandler static methods)                   //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Adds a slot (or expands aliases MISC / RING) to <paramref name="art"/>'s
        /// HERO possible-slot list.
        /// Corresponds to CArtHandler::addSlot.
        /// </summary>
        public static void AddSlot(ArtifactType art, EArtifactPosition slot)
        {
            if (art.PossibleSlots == null)
                art.PossibleSlots = new Dictionary<EArtBearer, List<EArtifactPosition>>();

            if (!art.PossibleSlots.ContainsKey(EArtBearer.HERO))
                art.PossibleSlots[EArtBearer.HERO] = new List<EArtifactPosition>();

            art.PossibleSlots[EArtBearer.HERO].Add(slot);
        }

        /// <summary>
        /// Adds all MISC slots (MISC1–MISC5) to the HERO possible-slot list.
        /// </summary>
        public static void AddMiscSlots(ArtifactType art)
        {
            foreach (var s in new[]
            {
                EArtifactPosition.MISC1, EArtifactPosition.MISC2, EArtifactPosition.MISC3,
                EArtifactPosition.MISC4, EArtifactPosition.MISC5,
            })
                AddSlot(art, s);
        }

        /// <summary>
        /// Adds both ring slots (LEFT_RING, RIGHT_RING) to the HERO possible-slot list.
        /// </summary>
        public static void AddRingSlots(ArtifactType art)
        {
            AddSlot(art, EArtifactPosition.RIGHT_RING);
            AddSlot(art, EArtifactPosition.LEFT_RING);
        }

        /// <summary>
        /// Restricts <paramref name="art"/> to CREATURE_SLOT only (removes hero / commander slots).
        /// Corresponds to CArtHandler::makeItCreatureArt.
        /// </summary>
        public static void MakeItCreatureArt(ArtifactType art, bool onlyCreature = true)
        {
            if (art.PossibleSlots == null)
                art.PossibleSlots = new Dictionary<EArtBearer, List<EArtifactPosition>>();

            if (onlyCreature)
            {
                art.PossibleSlots[EArtBearer.HERO]      = new List<EArtifactPosition>();
                art.PossibleSlots[EArtBearer.COMMANDER] = new List<EArtifactPosition>();
            }

            if (!art.PossibleSlots.ContainsKey(EArtBearer.CREATURE))
                art.PossibleSlots[EArtBearer.CREATURE] = new List<EArtifactPosition>();

            if (!art.PossibleSlots[EArtBearer.CREATURE].Contains(EArtifactPosition.CREATURE_SLOT))
                art.PossibleSlots[EArtBearer.CREATURE].Add(EArtifactPosition.CREATURE_SLOT);
        }

        /// <summary>
        /// Restricts <paramref name="art"/> to COMMANDER slots only (removes hero / creature slots).
        /// Corresponds to CArtHandler::makeItCommanderArt.
        /// </summary>
        public static void MakeItCommanderArt(ArtifactType art, bool onlyCommander = true)
        {
            if (art.PossibleSlots == null)
                art.PossibleSlots = new Dictionary<EArtBearer, List<EArtifactPosition>>();

            if (onlyCommander)
            {
                art.PossibleSlots[EArtBearer.HERO]    = new List<EArtifactPosition>();
                art.PossibleSlots[EArtBearer.CREATURE] = new List<EArtifactPosition>();
            }

            if (!art.PossibleSlots.ContainsKey(EArtBearer.COMMANDER))
                art.PossibleSlots[EArtBearer.COMMANDER] = new List<EArtifactPosition>();

            foreach (var slot in ArtifactUtils.CommanderSlots)
                if (!art.PossibleSlots[EArtBearer.COMMANDER].Contains(slot))
                    art.PossibleSlots[EArtBearer.COMMANDER].Add(slot);
        }

        // ------------------------------------------------------------------ //
        //  Class helpers                                                        //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Parses a rarity string ("TREASURE", "MINOR", "MAJOR", "RELIC", "SPECIAL")
        /// into <see cref="EArtifactClass"/>.
        /// Corresponds to CArtHandler::stringToClass.
        /// </summary>
        public static EArtifactClass StringToClass(string className)
        {
            switch (className?.ToUpperInvariant())
            {
                case "TREASURE": return EArtifactClass.ART_TREASURE;
                case "MINOR":    return EArtifactClass.ART_MINOR;
                case "MAJOR":    return EArtifactClass.ART_MAJOR;
                case "RELIC":    return EArtifactClass.ART_RELIC;
                default:         return EArtifactClass.ART_SPECIAL;
            }
        }

        // ------------------------------------------------------------------ //
        //  Legality                                                             //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// Returns true if <paramref name="art"/> is eligible for random placement
        /// on the adventure map (not combined, has valid class and at least one slot).
        /// Corresponds to CArtHandler::legalArtifact.
        /// </summary>
        public static bool LegalArtifact(ArtifactType art)
        {
            if (art.IsCombined())
                return false;

            if (art.ArtClass < EArtifactClass.ART_TREASURE || art.ArtClass > EArtifactClass.ART_RELIC)
                return false;

            if (art.PossibleSlots == null)
                return false;

            if (art.PossibleSlots.TryGetValue(EArtBearer.HERO, out var heroSlots) && heroSlots.Count > 0)
                return true;

            if (art.PossibleSlots.TryGetValue(EArtBearer.CREATURE, out var creatureSlots) && creatureSlots.Count > 0)
                return true;

            if (art.PossibleSlots.TryGetValue(EArtBearer.COMMANDER, out var commanderSlots) && commanderSlots.Count > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Returns the set of all non-combined artifact IDs — those eligible for
        /// default random map placement.
        /// Corresponds to CArtHandler::getDefaultAllowed.
        /// </summary>
        public HashSet<EArtifactId> GetDefaultAllowed()
        {
            var allowed = new HashSet<EArtifactId>();
            foreach (var art in objects.Values)
                if (!art.IsCombined())
                    allowed.Add(art.Id);
            return allowed;
        }

        // ------------------------------------------------------------------ //
        //  Tradability                                                          //
        // ------------------------------------------------------------------ //

        /// <summary>
        /// After all artifacts are registered, call this to add ALTAR slots
        /// to every tradable artifact, mirroring CArtHandler::afterLoadFinalization.
        /// </summary>
        public void FinalizeLoad()
        {
            foreach (var art in objects.Values)
            {
                if (!art.IsTradable()) continue;

                if (art.PossibleSlots == null)
                    art.PossibleSlots = new Dictionary<EArtBearer, List<EArtifactPosition>>();

                if (!art.PossibleSlots.ContainsKey(EArtBearer.ALTAR))
                    art.PossibleSlots[EArtBearer.ALTAR] = new List<EArtifactPosition>();

                if (!art.PossibleSlots[EArtBearer.ALTAR].Contains(EArtifactPosition.ALTAR))
                    art.PossibleSlots[EArtBearer.ALTAR].Add(EArtifactPosition.ALTAR);
            }
        }
    }
}
