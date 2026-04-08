// Numeric game constants and configuration values
// Migrated from VCMI lib/constants and H3Engine.Common

using System;

namespace H3Engine.Core.Constants
{
    /// <summary>
    /// Numeric game constants: player limits, creature counts, resource quantities, etc.
    /// Consolidated from VCMI GameConstants and H3Engine.Common.GameConstants.
    /// </summary>
    public static class NumericConstants
    {
        // ── Players ───────────────────────────────────────────────────────
        public const int PLAYER_LIMIT_I = 8;
        public const int PLAYER_LIMIT_T = 8;
        public const int MAX_HEROES_PER_PLAYER = 8;
        public const int AVAILABLE_HEROES_PER_PLAYER = 2;
        public const int ALL_PLAYERS = 255; // bitfield
        public const int F_NUMBER = 9;

        // ── Heroes ────────────────────────────────────────────────────────
        public const int HEROES_QUANTITY = 156;
        public const int HEROES_PER_TYPE = 8;
        public const int HERO_GOLD_COST = 2500;
        public const int SPELLBOOK_GOLD_COST = 500;
        public const int SKILL_GOLD_COST = 2000;
        public const int BASE_MOVEMENT_COST = 100;
        public const int HERO_PORTRAIT_SHIFT = 30; // 2 special frames + some extra portraits
        public const int HERO_HIGH_LEVEL = 10; // affects primary skill upgrade order

        // ── Creatures and troops ──────────────────────────────────────────
        public const int CREATURES_PER_TOWN = 7; // without upgrades
        public const int CREATURES_COUNT = 197;
        public const int ARMY_SIZE = 7; // number of creature stacks per army
        public const int CRE_LEVELS = 10; // number of creature experience levels

        // ── Artifacts ─────────────────────────────────────────────────────
        public const int ARTIFACTS_QUANTITY = 171;

        // ── Spells ────────────────────────────────────────────────────────
        public const int SPELLS_QUANTITY = 70;
        public const int SPELL_LEVELS = 5;
        public const int SPELL_SCHOOL_LEVELS = 4;

        // ── Skills ────────────────────────────────────────────────────────
        public const int SKILL_QUANTITY = 28;
        public const int PRIMARY_SKILLS = 4;
        public const int SKILL_PER_HERO = 8;
        public const int RESOURCE_QUANTITY = 7;

        // ── Battle ────────────────────────────────────────────────────────
        public const int BATTLE_PENALTY_DISTANCE = 10; // shooting penalty distance
        public const int BATTLE_SHOOTING_PENALTY_DISTANCE = 10; // if distance > this, shooting stack has distance penalty
        public const int BATTLE_SHOOTING_RANGE_DISTANCE = 255; // unlimited shooting range
        public const int TOURNAMENT_RULES_DD_MAP_TILES_THRESHOLD = 144 * 144 * 2;

        // ── Map ───────────────────────────────────────────────────────────
        public const int PUZZLE_MAP_PIECES = 48;
        public const int FULL_MAP_RANGE = int.MaxValue;

        // ── Economy ───────────────────────────────────────────────────────
        public const long PLAYER_RESOURCES_CAP = 1000 * 1000 * 1000;
        public const int ALTAR_ARTIFACTS_SLOTS = 22;

        // ── UI ────────────────────────────────────────────────────────────
        public const int KINGDOM_WINDOW_HEROES_SLOTS = 4;
        public const int INFO_WINDOW_ARTIFACTS_MAX_ITEMS = 14;

        // ── Runtime ───────────────────────────────────────────────────────
        public static readonly int[] POSSIBLE_TURNTIME = { 1, 2, 4, 6, 8, 10, 15, 20, 25, 30, 0 };
    }
}

