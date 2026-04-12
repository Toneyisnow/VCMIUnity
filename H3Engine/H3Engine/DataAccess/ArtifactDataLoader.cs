// Loads artifact type definitions from H3Engine/Config/artifacts.json.
//
// The JSON format mirrors VCMI's config/artifacts.json with these additions:
//   • "type" array  → bearer types (HERO / CREATURE / COMMANDER)
//   • Slot assignment is resolved from a hardcoded lookup table (matching H3
//     ARTRAITS.TXT positions) since VCMI's JSON omits possibleSlots.
//   • "bonuses" / "instanceBonuses" use VCMI's keyed-object format.
//   • "components" string arrays are resolved in a second pass so combined
//     artifacts can reference any artifact by identifier.

using System;
using System.Collections.Generic;
using System.IO;
using H3Engine.Core;
using H3Engine.Core.Bonus;
using H3Engine.Core.Constants;
using Newtonsoft.Json.Linq;
using BonusItem = H3Engine.Core.Bonus.Bonus;

namespace H3Engine.DataAccess
{
    /// <summary>
    /// Reads <c>artifacts.json</c> and populates an <see cref="ArtifactHandler"/>
    /// with fully initialised <see cref="ArtifactType"/> objects, including bonus
    /// definitions and slot assignments.
    ///
    /// Usage:
    /// <code>
    ///   var handler = ArtifactDataLoader.LoadFromFile("Config/artifacts.json");
    /// </code>
    /// </summary>
    public static class ArtifactDataLoader
    {
        // ──────────────────────────────────────────────────────────────────────
        //  Public entry point
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses <paramref name="jsonPath"/>, builds every <see cref="ArtifactType"/>,
        /// registers them with a new <see cref="ArtifactHandler"/>, and returns it.
        /// </summary>
        public static ArtifactHandler LoadFromFile(string jsonPath)
        {
            string text = File.ReadAllText(jsonPath);
            return LoadFromText(text);
        }

        /// <summary>Parses artifact JSON from a string (useful for unit tests).</summary>
        public static ArtifactHandler LoadFromText(string json)
        {
            // Strip C-style // comments (VCMI JSON uses them) before parsing.
            string cleaned = StripLineComments(json);
            var root = JObject.Parse(cleaned);

            var handler = new ArtifactHandler();

            // First pass: create ArtifactType objects (without component resolution).
            // We store component identifier strings for the second pass.
            var pendingComponents = new Dictionary<EArtifactId, List<string>>();

            foreach (var kvp in root)
            {
                string identifier = kvp.Key;
                var obj = (JObject)kvp.Value;

                var art = ParseArtifactType(identifier, obj);
                if (art == null) continue;

                handler.Register(art);

                // Stash "components" string list for second pass.
                var componentsToken = obj["components"];
                if (componentsToken is JArray compArray && compArray.Count > 0)
                {
                    var names = new List<string>();
                    foreach (var t in compArray)
                        names.Add(t.Value<string>());
                    pendingComponents[art.Id] = names;
                }
            }

            // Second pass: resolve component identifier strings → EArtifactId.
            foreach (var kvp in pendingComponents)
            {
                var combined = handler.GetById(kvp.Key);
                if (combined == null) continue;

                combined.Constituents = new List<EArtifactId>();
                foreach (var name in kvp.Value)
                {
                    var compId = IdentifierToArtifactId(name);
                    if (compId != EArtifactId.NONE)
                        combined.Constituents.Add(compId);
                }
            }

            handler.FinalizeLoad();
            return handler;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Per-artifact parsing
        // ──────────────────────────────────────────────────────────────────────

        private static ArtifactType ParseArtifactType(string identifier, JObject obj)
        {
            // "index" is required; skip entries that lack it.
            var indexToken = obj["index"];
            if (indexToken == null) return null;

            int index = indexToken.Value<int>();
            var id = (EArtifactId)index;

            var art = new ArtifactType
            {
                Id         = id,
                Identifier = identifier,
            };

            // ── Class ──────────────────────────────────────────────────────────
            var classToken = obj["class"];
            art.ArtClass = classToken != null
                ? ArtifactHandler.StringToClass(classToken.Value<string>())
                : EArtifactClass.ART_TREASURE;   // default if omitted

            // ── Flags ──────────────────────────────────────────────────────────
            var waterToken = obj["onlyOnWaterMap"];
            if (waterToken != null)
                art.OnlyOnWaterMap = waterToken.Value<bool>();

            // ── War machine ────────────────────────────────────────────────────
            var warMachineToken = obj["warMachine"];
            if (warMachineToken != null)
                art.WarMachine = WarMachineIdentifier(warMachineToken.Value<string>());

            // ── Bonuses ────────────────────────────────────────────────────────
            var bonusesToken  = obj["bonuses"]         as JObject;
            var instBonusToken = obj["instanceBonuses"] as JObject;

            var allBonuses = new List<BonusItem>();
            if (bonusesToken != null)
                ParseBonusObject(bonusesToken, allBonuses);
            if (instBonusToken != null)
                ParseBonusObject(instBonusToken, allBonuses);

            if (allBonuses.Count > 0)
                art.Bonuses = allBonuses;

            // ── Slot / bearer setup ────────────────────────────────────────────
            // Determine bearer types from "type" array.
            var typeToken = obj["type"] as JArray;
            bool isHero       = false;
            bool isCreature   = false;
            bool isCommander  = false;
            if (typeToken != null)
            {
                foreach (var t in typeToken)
                {
                    switch (t.Value<string>()?.ToUpperInvariant())
                    {
                        case "HERO":      isHero      = true; break;
                        case "CREATURE":  isCreature  = true; break;
                        case "COMMANDER": isCommander = true; break;
                    }
                }
            }

            // Combined artifacts (have components) get no hero slot of their own.
            bool isCombined = obj["components"] is JArray ca && ca.Count > 0;

            if (isHero && !isCombined)
            {
                // Assign specific hero equipment slot(s) from the static table.
                AssignHeroSlots(art, index);
            }

            if (isCreature)
                ArtifactHandler.MakeItCreatureArt(art);

            if (isCommander)
                ArtifactHandler.MakeItCommanderArt(art);

            return art;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Bonus parsing
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Parses one "bonuses" or "instanceBonuses" object (keyed by bonus name)
        /// and appends the resulting <see cref="Bonus"/> objects to <paramref name="out"/>.
        /// </summary>
        private static void ParseBonusObject(JObject bonusesObj, List<BonusItem> @out)
        {
            foreach (var entry in bonusesObj)
            {
                var bObj = entry.Value as JObject;
                if (bObj == null) continue;

                var b = ParseSingleBonus(bObj);
                if (b != null)
                    @out.Add(b);
            }
        }

        private static BonusItem ParseSingleBonus(JObject bObj)
        {
            var typeStr = bObj["type"]?.Value<string>();
            if (string.IsNullOrEmpty(typeStr)) return null;

            if (!TryParseBonusType(typeStr, out var bonusType))
                return null;   // unknown type — skip gracefully

            var bonus = new BonusItem
            {
                Type     = bonusType,
                Source   = BonusSource.ARTIFACT,
                Duration = BonusDuration.PERMANENT,
            };

            // Val
            var valToken = bObj["val"];
            if (valToken != null) bonus.Val = valToken.Value<int>();

            // ValueType
            var vtToken = bObj["valueType"];
            if (vtToken != null && TryParseValueType(vtToken.Value<string>(), out var vt))
                bonus.ValType = vt;

            // Subtype (string → int)
            var stToken = bObj["subtype"];
            if (stToken != null)
                bonus.Subtype = ParseSubtype(stToken.Value<string>(), bonusType);

            // Stacking
            var stackToken = bObj["stacking"];
            if (stackToken != null)
                bonus.Stacking = stackToken.Value<string>() ?? string.Empty;

            // Propagator (stored as string in EffectRange for now; full propagator
            // objects are battle-only and not needed for adventure-map hero stats).
            // We map well-known propagator names to BonusLimitEffect / EffectRange.
            var propToken = bObj["propagator"];
            if (propToken != null)
            {
                // EffectRange stores which context a propagator targets.
                // We encode the propagator name as a tag; runtime battle logic
                // can read it. For hero stat queries (our current use case) these
                // propagators are irrelevant, so we just stash the raw string.
                bonus.Stacking = string.IsNullOrEmpty(bonus.Stacking)
                    ? ("PROPAGATOR:" + propToken.Value<string>())
                    : bonus.Stacking;
            }

            // Limiters (complex object / string array) — store as a placeholder;
            // full limiter evaluation requires runtime creature context and is
            // beyond the scope of the loader.
            // The bonus is still added so non-limited stats (attack, movement…)
            // are available for hero stat queries.

            return bonus;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Type / subtype / valueType string → enum helpers
        // ──────────────────────────────────────────────────────────────────────

        private static bool TryParseBonusType(string s, out BonusType result)
        {
            // Map VCMI bonus type strings to BonusType enum values.
            switch (s)
            {
                case "MOVEMENT":                     result = BonusType.MOVEMENT;                    return true;
                case "FLYING_MOVEMENT":              result = BonusType.FLYING_MOVEMENT;             return true;
                case "WATER_WALKING":                result = BonusType.WATER_WALKING;               return true;
                case "FLYING":                       result = BonusType.FLYING;                      return true;
                case "PRIMARY_SKILL":                result = BonusType.PRIMARY_SKILL;               return true;
                case "SECONDARY_SKILL_PREMY":        result = BonusType.SECONDARY_SKILL_PREMY;       return true;
                case "STACKS_SPEED":                 result = BonusType.STACKS_SPEED;                return true;
                case "CREATURE_DAMAGE":              result = BonusType.CREATURE_DAMAGE;             return true;
                case "CREATURE_HP":                  result = BonusType.CREATURE_HP;                 return true;
                case "STACK_HEALTH":                 result = BonusType.CREATURE_HP;                 return true;
                case "SHOTS":                        result = BonusType.SHOTS;                       return true;
                case "CREATURE_GROWTH":              result = BonusType.CREATURE_GROWTH;             return true;
                case "CREATURE_GROWTH_PERCENT":      result = BonusType.CREATURE_GROWTH_PERCENT;     return true;
                case "GENERAL_ATTACK_REDUCTION":     result = BonusType.GENERAL_ATTACK_REDUCTION;    return true;
                case "GENERAL_DAMAGE_REDUCTION":     result = BonusType.GENERAL_DAMAGE_REDUCTION;    return true;
                case "PERCENTAGE_DAMAGE_BOOST":      result = BonusType.PERCENTAGE_DAMAGE_BOOST;     return true;
                case "MORALE":                       result = BonusType.MORALE;                      return true;
                case "LUCK":                         result = BonusType.LUCK;                        return true;
                case "SPELL_DAMAGE":                 result = BonusType.SPELL_DAMAGE;                return true;
                case "SPELL_DURATION":               result = BonusType.SPELL_DURATION;              return true;
                case "SPELL_IMMUNITY":               result = BonusType.SPELL_IMMUNITY;              return true;
                case "LEVEL_SPELL_IMMUNITY":         result = BonusType.LEVEL_SPELL_IMMUNITY;        return true;
                case "BLOCK_MAGIC_ABOVE":            result = BonusType.BLOCK_MAGIC_ABOVE;           return true;
                case "MAGIC_RESISTANCE":             result = BonusType.MAGIC_RESISTANCE;            return true;
                case "MAGIC_MIRROR":                 result = BonusType.MAGIC_MIRROR;                return true;
                case "MIND_IMMUNITY":                result = BonusType.MIND_IMMUNITY;               return true;
                case "FIRE_IMMUNITY":                result = BonusType.FIRE_IMMUNITY;               return true;
                case "WATER_IMMUNITY":               result = BonusType.WATER_IMMUNITY;              return true;
                case "EARTH_IMMUNITY":               result = BonusType.EARTH_IMMUNITY;              return true;
                case "AIR_IMMUNITY":                 result = BonusType.AIR_IMMUNITY;                return true;
                case "NEGATE_ALL_NATURAL_IMMUNITIES": result = BonusType.NEGATE_ALL_NATURAL_IMMUNITIES; return true;
                case "SHOOTER":                      result = BonusType.SHOOTER;                     return true;
                case "UNLIMITED_RETALIATIONS":       result = BonusType.UNLIMITED_RETALIATIONS;      return true;
                case "ADDITIONAL_ATTACK":            result = BonusType.ADDITIONAL_ATTACK;           return true;
                case "JOUSTING":                     result = BonusType.JOUSTING;                    return true;
                case "NO_DISTANCE_PENALTY":          result = BonusType.NO_DISTANCE_PENALTY;         return true;
                case "NO_MELEE_PENALTY":             result = BonusType.NO_MELEE_PENALTY;            return true;
                case "NO_OBSTACLES_PENALTY":         result = BonusType.NO_OBSTACLES_PENALTY;        return true;
                case "BLOCKS_RETALIATION":           result = BonusType.BLOCKS_RETALIATION;          return true;
                case "SIGHT_RANGE":                  result = BonusType.SIGHT_RANGE;                 return true;
                case "SIGHT_RADIUS":                 result = BonusType.SIGHT_RANGE;                 return true;
                case "SURRENDER_DISCOUNT":           result = BonusType.SURRENDER_DISCOUNT;          return true;
                case "MANA_CHANNELING":              result = BonusType.MANA_CHANNELING;             return true;
                case "MANA_REGENERATION":            result = BonusType.MANA_REGENERATION;           return true;
                case "NECROMANCER_RESURRECTION_LEVEL": result = BonusType.NECROMANCER_RESURRECTION_LEVEL; return true;
                case "LEARN_BATTLE_SPELL_CHANCE":    result = BonusType.LEARN_BATTLE_SPELL_CHANCE;   return true;
                case "LEARN_BATTLE_SPELL_LEVEL_LIMIT": result = BonusType.LEARN_BATTLE_SPELL_LEVEL_LIMIT; return true;
                case "ENEMY_CANT_ESCAPE":            result = BonusType.ENEMY_CANT_ESCAPE;           return true;
                case "BONUS_EXPERIENCE_POINTS":      result = BonusType.BONUS_EXPERIENCE_POINTS;     return true;
                case "FREE_SHIP_BOARDING":           result = BonusType.FREE_SHIP_BOARDING;          return true;
                case "CREATURE_JOINS_FOR_FREE":      result = BonusType.CREATURE_JOINS_FOR_FREE;     return true;
                case "GENERATE_RESOURCE":            result = BonusType.GENERATE_RESOURCE;           return true;
                case "SPELLCASTER":                  result = BonusType.SPELLCASTER;                 return true;
                case "SPELL":                        result = BonusType.SPELLCASTER;                 return true;
                case "HP_REGENERATION":              result = BonusType.HP_REGENERATION;             return true;
                case "BLOCK_MORALE":                 result = BonusType.BLOCK_MORALE;                return true;
                case "BLOCK_LUCK":                   result = BonusType.BLOCK_LUCK;                  return true;
                case "UNDEAD_RAISE_PERCENTAGE":      result = BonusType.UNDEAD_RAISE_PERCENTAGE;     return true;
                case "NO_WALL_PENALTY":              result = BonusType.NO_WALL_PENALTY;             return true;
                case "FREE_SHOOTING":                result = BonusType.FREE_SHOOTING;               return true;
                case "OPENING_BATTLE_SPELL":         result = BonusType.OPENING_BATTLE_SPELL;        return true;
                case "SPELLS_OF_SCHOOL":             result = BonusType.SPELLS_OF_SCHOOL;            return true;
                case "SPELLS_OF_LEVEL":              result = BonusType.SPELLS_OF_LEVEL;             return true;
                case "MANA_PERCENTAGE_REGENERATION": result = BonusType.MANA_PERCENTAGE_REGENERATION; return true;
                case "WHIRLPOOL_PROTECTION":         result = BonusType.WHIRLPOOL_PROTECTION;        return true;
                case "BATTLE_NO_FLEEING":            result = BonusType.BATTLE_NO_FLEEING;           return true;
                case "BLOCK_ALL_MAGIC":              result = BonusType.BLOCK_ALL_MAGIC;             return true;
                case "IMPROVED_NECROMANCY":          result = BonusType.IMPROVED_NECROMANCY;         return true;
                case "NONEVIL_ALIGNMENT_MIX":        result = BonusType.NONEVIL_ALIGNMENT_MIX;       return true;
                case "SPELL_RESISTANCE_AURA":        result = BonusType.SPELL_RESISTANCE_AURA;       return true;
                default:
                    result = BonusType.NONE;
                    return false;
            }
        }

        private static bool TryParseValueType(string s, out BonusValueType result)
        {
            switch (s)
            {
                case "BASE_NUMBER":            result = BonusValueType.BASE_NUMBER;            return true;
                case "ADDITIVE_VALUE":         result = BonusValueType.ADDITIVE_VALUE;         return true;
                case "PERCENT_TO_ALL":         result = BonusValueType.PERCENT_TO_ALL;         return true;
                case "PERCENT_TO_BASE":        result = BonusValueType.PERCENT_TO_BASE;        return true;
                case "PERCENT_TO_SOURCE":      result = BonusValueType.PERCENT_TO_SOURCE;      return true;
                case "PERCENT_TO_TARGET_TYPE": result = BonusValueType.PERCENT_TO_TARGET_TYPE; return true;
                case "INDEPENDENT_MAX":        result = BonusValueType.INDEPENDENT_MAX;        return true;
                case "INDEPENDENT_MIN":        result = BonusValueType.INDEPENDENT_MIN;        return true;
                default:
                    result = BonusValueType.BASE_NUMBER;
                    return false;
            }
        }

        /// <summary>
        /// Converts a VCMI subtype string to the integer stored in <see cref="Bonus.Subtype"/>.
        /// Context-sensitive: primary skill subtypes differ from resource subtypes etc.
        /// </summary>
        private static int ParseSubtype(string s, BonusType bonusType)
        {
            if (string.IsNullOrEmpty(s)) return -1;

            // ── Primary skill ──────────────────────────────────────────────────
            switch (s)
            {
                case "primarySkill.attack":    return (int)EPrimarySkill.ATTACK;
                case "primarySkill.defence":   return (int)EPrimarySkill.DEFENSE;
                case "primarySkill.spellpower":return (int)EPrimarySkill.SPELL_POWER;
                case "primarySkill.knowledge": return (int)EPrimarySkill.KNOWLEDGE;
            }

            // ── Spell school ───────────────────────────────────────────────────
            switch (s)
            {
                case "air":   case "spellSchool.air":   return (int)ESpellSchool.AIR;
                case "fire":  case "spellSchool.fire":  return (int)ESpellSchool.FIRE;
                case "water": case "spellSchool.water": return (int)ESpellSchool.WATER;
                case "earth": case "spellSchool.earth": return (int)ESpellSchool.EARTH;
            }

            // ── Movement subtype ───────────────────────────────────────────────
            if (s == "heroMovementLand" || s == "heroMovementSea") return -1;

            // ── Resource ───────────────────────────────────────────────────────
            if (s.StartsWith("resource."))
            {
                switch (s)
                {
                    case "resource.gold":    return (int)EResourceType.GOLD;
                    case "resource.wood":    return (int)EResourceType.WOOD;
                    case "resource.ore":     return (int)EResourceType.ORE;
                    case "resource.gems":    return (int)EResourceType.GEMS;
                    case "resource.sulfur":  return (int)EResourceType.SULFUR;
                    case "resource.crystal": return (int)EResourceType.CRYSTALS;
                    case "resource.mercury": return (int)EResourceType.PEARLS; // mercury ~ pearls slot
                }
            }

            // ── Spell ID ───────────────────────────────────────────────────────
            if (s.StartsWith("spell."))
            {
                var spellName = s.Substring(6); // strip "spell."
                var spellId = SpellIdentifier(spellName);
                return (int)spellId;
            }

            // ── Creature level (Legion parts) ──────────────────────────────────
            switch (s)
            {
                case "creatureLevel1": return 0;
                case "creatureLevel2": return 1;
                case "creatureLevel3": return 2;
                case "creatureLevel4": return 3;
                case "creatureLevel5": return 4;
                case "creatureLevel6": return 5;
                case "creatureLevel7": return 6;
            }

            // ── Spell level (Spellbinder's Hat) ────────────────────────────────
            switch (s)
            {
                case "spellLevel1": return 1;
                case "spellLevel2": return 2;
                case "spellLevel3": return 3;
                case "spellLevel4": return 4;
                case "spellLevel5": return 5;
            }

            // ── Creature ID (for HATE, IMPROVED_NECROMANCY, etc.) ─────────────
            if (s.StartsWith("creature."))
            {
                var cName = s.Substring(9);
                return (int)CreatureIdentifier(cName);
            }

            // ── Damage type (for PERCENTAGE_DAMAGE_BOOST) ─────────────────────
            if (s == "damageTypeRanged") return 1;
            if (s == "damageTypeMelee")  return 0;

            // ── Secondary skill (for SECONDARY_SKILL_PREMY) ───────────────────
            if (s.StartsWith("secondarySkill."))
            {
                var skName = s.Substring(15);
                return (int)SecondarySkillIdentifier(skName);
            }

            // Fallback: unknown subtype → -1 (no subtype)
            return -1;
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Slot assignment (from H3 ARTRAITS.TXT knowledge)
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Assigns the correct hero equipment slot(s) to <paramref name="art"/>
        /// based on the canonical H3 artifact index.
        /// Matches the slot data from H3's ARTRAITS.TXT, which VCMI loads separately
        /// and does not include in artifacts.json.
        /// </summary>
        private static void AssignHeroSlots(ArtifactType art, int index)
        {
            switch (index)
            {
                // ── Spellbook / Grail / War machines ────────────────────────────
                case 0:  ArtifactHandler.AddSlot(art, EArtifactPosition.SPELLBOOK); break;
                case 1:  ArtifactHandler.AddMiscSlots(art);                         break; // Spell Scroll
                case 2:  /* Grail: no hero slot */                                   break;
                case 3:  ArtifactHandler.AddSlot(art, EArtifactPosition.MACH1);    break; // Catapult
                case 4:  ArtifactHandler.AddSlot(art, EArtifactPosition.MACH2);    break; // Ballista
                case 5:  ArtifactHandler.AddSlot(art, EArtifactPosition.MACH3);    break; // Ammo Cart
                case 6:  ArtifactHandler.AddSlot(art, EArtifactPosition.MACH4);    break; // First Aid Tent

                // ── Weapons (RIGHT_HAND) ─────────────────────────────────────────
                case 7:  case 8:  case 9:  case 10: case 11: case 12:
                    ArtifactHandler.AddSlot(art, EArtifactPosition.RIGHT_HAND); break;

                // ── Shields (LEFT_HAND) ──────────────────────────────────────────
                case 13: case 14: case 15: case 16: case 17: case 18:
                    ArtifactHandler.AddSlot(art, EArtifactPosition.LEFT_HAND); break;

                // ── Helmets (HEAD) ───────────────────────────────────────────────
                case 19: case 20: case 21: case 22: case 23: case 24:
                    ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD); break;

                // ── Armors (TORSO) ───────────────────────────────────────────────
                case 25: case 26: case 27: case 28: case 29: case 30:
                    ArtifactHandler.AddSlot(art, EArtifactPosition.TORSO); break;

                // ── Heaven's Gate set ────────────────────────────────────────────
                case 31: ArtifactHandler.AddSlot(art, EArtifactPosition.TORSO);      break; // Armor of Wonder
                case 32: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);       break; // Sandals of the Saint
                case 33: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);       break; // Celestial Necklace of Bliss
                case 34: ArtifactHandler.AddSlot(art, EArtifactPosition.LEFT_HAND);  break; // Lion's Shield of Courage
                case 35: ArtifactHandler.AddSlot(art, EArtifactPosition.RIGHT_HAND); break; // Sword of Judgement
                case 36: ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD);       break; // Helm of Heavenly Enlightenment

                // ── Dragon set ───────────────────────────────────────────────────
                case 37: ArtifactHandler.AddMiscSlots(art);                          break; // Quiet Eye of the Dragon
                case 38: ArtifactHandler.AddSlot(art, EArtifactPosition.RIGHT_HAND); break; // Red Dragon Flame Tongue
                case 39: ArtifactHandler.AddSlot(art, EArtifactPosition.LEFT_HAND);  break; // Dragon Scale Shield
                case 40: ArtifactHandler.AddSlot(art, EArtifactPosition.TORSO);      break; // Dragon Scale Armor
                case 41: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);       break; // Dragonbone Greaves
                case 42: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS);  break; // Dragon Wing Tabard
                case 43: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);       break; // Necklace of Dragonteeth
                case 44: ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD);       break; // Crown of Dragontooth
                case 45: ArtifactHandler.AddMiscSlots(art);                          break; // Still Eye of the Dragon

                // ── Luck / Morale MISC items ─────────────────────────────────────
                case 46: case 47: case 48: case 49: case 50: case 51:
                    ArtifactHandler.AddMiscSlots(art); break;

                // ── Sight MISC items ─────────────────────────────────────────────
                case 52: case 53:
                    ArtifactHandler.AddMiscSlots(art); break;

                // ── Necromancy set ───────────────────────────────────────────────
                case 54: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);      break; // Amulet of the Undertaker
                case 55: ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD);      break; // Vampire's Cowl
                case 56: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);      break; // Dead Man's Boots

                // ── Magic resistance set ─────────────────────────────────────────
                case 57: ArtifactHandler.AddMiscSlots(art);                         break; // Garniture of Interference
                case 58: ArtifactHandler.AddSlot(art, EArtifactPosition.TORSO);    break; // Surcoat of Counterpoise
                case 59: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);     break; // Boots of Polarity

                // ── Archery set (Bow of the Sharpshooter) ───────────────────────
                case 60: ArtifactHandler.AddSlot(art, EArtifactPosition.RIGHT_HAND); break; // Bow of Elven Cherrywood
                case 61: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS);  break; // Bowstring of the Unicorn's Mane
                case 62: ArtifactHandler.AddMiscSlots(art);                          break; // Angel Feather Arrows

                // ── Eagle Eye set ────────────────────────────────────────────────
                case 63: ArtifactHandler.AddMiscSlots(art);                         break; // Bird of Perception
                case 64: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);     break; // Stoic Watchman
                case 65: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS);break; // Emblem of Cognizance

                // ── Diplomacy set ────────────────────────────────────────────────
                case 66: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);     break; // Statesman's Medal
                case 67: ArtifactHandler.AddRingSlots(art);                        break; // Diplomat's Ring
                case 68: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS);break; // Ambassador's Sash

                // ── Speed / movement items ───────────────────────────────────────
                case 69: ArtifactHandler.AddRingSlots(art);                         break; // Ring of the Wayfarer
                case 70: ArtifactHandler.AddSlot(art, EArtifactPosition.LEFT_HAND); break; // Equestrian's Gloves
                case 71: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);      break; // Necklace of Ocean Guidance
                case 72: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS); break; // Angel Wings

                // ── Mana regeneration set (Wizard's Well) ───────────────────────
                case 73: ArtifactHandler.AddMiscSlots(art);                        break; // Charm of Mana
                case 74: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);    break; // Talisman of Mana
                case 75: ArtifactHandler.AddRingSlots(art);                       break; // Mystic Orb of Mana

                // ── Spell duration set (Ring of the Magi) ───────────────────────
                case 76: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);     break; // Collar of Conjuring
                case 77: ArtifactHandler.AddRingSlots(art);                        break; // Ring of Conjuring
                case 78: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS);break; // Cape of Conjuring

                // ── Spell damage orbs ────────────────────────────────────────────
                case 79: case 80: case 81: case 82:
                    ArtifactHandler.AddMiscSlots(art); break;

                // ── Special MISC items ───────────────────────────────────────────
                case 83: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS); break; // Recanter's Cloak
                case 84: ArtifactHandler.AddMiscSlots(art);                         break; // Spirit of Oppression
                case 85: ArtifactHandler.AddMiscSlots(art);                         break; // Hourglass of the Evil Hour

                // ── Tomes of magic (MISC, charged) ───────────────────────────────
                case 86: case 87: case 88: case 89:
                    ArtifactHandler.AddMiscSlots(art); break;

                // ── Adventure movement ───────────────────────────────────────────
                case 90: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);      break; // Boots of Levitation
                case 91: ArtifactHandler.AddSlot(art, EArtifactPosition.RIGHT_HAND);break; // Golden Bow
                case 92: ArtifactHandler.AddMiscSlots(art);                         break; // Sphere of Permanence
                case 93: ArtifactHandler.AddMiscSlots(art);                         break; // Orb of Vulnerability

                // ── Life / health rings ──────────────────────────────────────────
                case 94: ArtifactHandler.AddRingSlots(art);   break; // Ring of Vitality
                case 95: ArtifactHandler.AddRingSlots(art);   break; // Ring of Life
                case 96: ArtifactHandler.AddMiscSlots(art);   break; // Vial of Lifeblood

                // ── Speed ────────────────────────────────────────────────────────
                case 97: ArtifactHandler.AddSlot(art, EArtifactPosition.NECK);     break; // Necklace of Swiftness
                case 98: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);     break; // Boots of Speed
                case 99: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS);break; // Cape of Velocity

                // ── Pendants (NECK) ──────────────────────────────────────────────
                case 100: case 101: case 102: case 103: case 104:
                case 105: case 106: case 107: case 108:
                    ArtifactHandler.AddSlot(art, EArtifactPosition.NECK); break;

                // ── Resource generation ──────────────────────────────────────────
                case 109: ArtifactHandler.AddSlot(art, EArtifactPosition.SHOULDERS); break; // Everflowing Crystal Cloak
                case 110: ArtifactHandler.AddRingSlots(art);                         break; // Ring of Infinite Gems
                case 111: ArtifactHandler.AddMiscSlots(art);                         break; // Everpouiring Vial of Mercury
                case 112: ArtifactHandler.AddMiscSlots(art);                         break; // Inexhaustible Cart of Ore
                case 113: ArtifactHandler.AddRingSlots(art);                         break; // Everpsmoking Ring of Sulfur
                case 114: ArtifactHandler.AddMiscSlots(art);                         break; // Inexhaustible Cart of Lumber
                case 115: ArtifactHandler.AddMiscSlots(art);                         break; // Endless Sack of Gold
                case 116: ArtifactHandler.AddMiscSlots(art);                         break; // Endless Bag of Gold
                case 117: ArtifactHandler.AddMiscSlots(art);                         break; // Endless Purse of Gold

                // ── Legion set ───────────────────────────────────────────────────
                case 118: ArtifactHandler.AddSlot(art, EArtifactPosition.FEET);     break; // Legs of Legion
                case 119: ArtifactHandler.AddMiscSlots(art);                        break; // Loins of Legion
                case 120: ArtifactHandler.AddSlot(art, EArtifactPosition.TORSO);   break; // Torso of Legion
                case 121: ArtifactHandler.AddSlot(art, EArtifactPosition.LEFT_HAND);break; // Arms of Legion
                case 122: ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD);    break; // Head of Legion

                // ── Sea / water artifacts ────────────────────────────────────────
                case 123: ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD);    break; // Sea Captain's Hat

                // ── Special scenario artifacts ───────────────────────────────────
                case 124: ArtifactHandler.AddSlot(art, EArtifactPosition.HEAD);    break; // Spellbinder's Hat
                case 125: ArtifactHandler.AddMiscSlots(art);                       break; // Shackles of War
                case 126: ArtifactHandler.AddMiscSlots(art);                       break; // Orb of Inhibition
                case 127: ArtifactHandler.AddMiscSlots(art);                       break; // Vial of Dragon Blood (SPECIAL)
                case 128: ArtifactHandler.AddSlot(art, EArtifactPosition.RIGHT_HAND); break; // Armageddon's Blade (SPECIAL)

                // ── Combined artifacts (129-140): handled by isCombined check above ──
                // ── Unused (141-143) ─────────────────────────────────────────────
                default:
                    break;
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Identifier → enum helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Maps an artifact identifier string to its <see cref="EArtifactId"/>.</summary>
        private static EArtifactId IdentifierToArtifactId(string name)
        {
            // Build the map lazily the first time.
            if (s_artIdMap == null)
                BuildArtIdMap();
            return s_artIdMap.TryGetValue(name, out var id) ? id : EArtifactId.NONE;
        }

        private static Dictionary<string, EArtifactId> s_artIdMap;

        private static void BuildArtIdMap()
        {
            s_artIdMap = new Dictionary<string, EArtifactId>(StringComparer.Ordinal)
            {
                { "spellBook",                    EArtifactId.SPELLBOOK },
                { "spellScroll",                  EArtifactId.SPELL_SCROLL },
                { "grail",                        EArtifactId.GRAIL },
                { "catapult",                     EArtifactId.CATAPULT },
                { "ballista",                     EArtifactId.BALLISTA },
                { "ammoCart",                     EArtifactId.AMMO_CART },
                { "firstAidTent",                 EArtifactId.FIRST_AID_TENT },
                { "centaurAxe",                   (EArtifactId)7 },
                { "blackshardOfTheDeadKnight",    (EArtifactId)8 },
                { "greaterGnollsFlail",           (EArtifactId)9 },
                { "ogresClubOfHavoc",             (EArtifactId)10 },
                { "swordOfHellfire",              (EArtifactId)11 },
                { "titansGladius",                (EArtifactId)12 },
                { "shieldOfTheDwarvenLords",      (EArtifactId)13 },
                { "shieldOfTheYawningDead",       (EArtifactId)14 },
                { "bucklerOfTheGnollKing",        (EArtifactId)15 },
                { "targOfTheRampagingOgre",       (EArtifactId)16 },
                { "shieldOfTheDamned",            (EArtifactId)17 },
                { "sentinelsShield",              (EArtifactId)18 },
                { "helmOfTheAlabasterUnicorn",    (EArtifactId)19 },
                { "skullHelmet",                  (EArtifactId)20 },
                { "helmOfChaos",                  (EArtifactId)21 },
                { "crownOfTheSupremeMagi",        (EArtifactId)22 },
                { "hellstormHelmet",              (EArtifactId)23 },
                { "thunderHelmet",                (EArtifactId)24 },
                { "breastplateOfPetrifiedWood",   (EArtifactId)25 },
                { "ribCage",                      (EArtifactId)26 },
                { "scalesOfTheGreaterBasilisk",   (EArtifactId)27 },
                { "tunicOfTheCyclopsKing",        (EArtifactId)28 },
                { "breastplateOfBrimstone",       (EArtifactId)29 },
                { "titansCuirass",                (EArtifactId)30 },
                { "armorOfWonder",                (EArtifactId)31 },
                { "sandalsOfTheSaint",            (EArtifactId)32 },
                { "celestialNecklaceOfBliss",     (EArtifactId)33 },
                { "lionsShieldOfCourage",         (EArtifactId)34 },
                { "swordOfJudgement",             (EArtifactId)35 },
                { "helmOfHeavenlyEnlightenment",  (EArtifactId)36 },
                { "quietEyeOfTheDragon",          (EArtifactId)37 },
                { "redDragonFlameTongue",         (EArtifactId)38 },
                { "dragonScaleShield",            (EArtifactId)39 },
                { "dragonScaleArmor",             (EArtifactId)40 },
                { "dragonboneGreaves",            (EArtifactId)41 },
                { "dragonWingTabard",             (EArtifactId)42 },
                { "necklaceOfDragonteeth",        (EArtifactId)43 },
                { "crownOfDragontooth",           (EArtifactId)44 },
                { "stillEyeOfTheDragon",          (EArtifactId)45 },
                { "cloverOfFortune",              (EArtifactId)46 },
                { "cardsOfProphecy",              (EArtifactId)47 },
                { "ladybirdOfLuck",               (EArtifactId)48 },
                { "badgeOfCourage",               (EArtifactId)49 },
                { "crestOfValor",                 (EArtifactId)50 },
                { "glyphOfGallantry",             (EArtifactId)51 },
                { "speculum",                     (EArtifactId)52 },
                { "spyglass",                     (EArtifactId)53 },
                { "amuletOfTheUndertaker",        (EArtifactId)54 },
                { "vampiresCowl",                 (EArtifactId)55 },
                { "deadMansBoots",                (EArtifactId)56 },
                { "garnitureOfInterference",      (EArtifactId)57 },
                { "surcoatOfCounterpoise",        (EArtifactId)58 },
                { "bootsOfPolarity",              (EArtifactId)59 },
                { "bowOfElvenCherrywood",         (EArtifactId)60 },
                { "bowstringOfTheUnicornsMane",   (EArtifactId)61 },
                { "angelFeatherArrows",           (EArtifactId)62 },
                { "birdOfPerception",             (EArtifactId)63 },
                { "stoicWatchman",                (EArtifactId)64 },
                { "emblemOfCognizance",           (EArtifactId)65 },
                { "statesmansMedal",              (EArtifactId)66 },
                { "diplomatsRing",                (EArtifactId)67 },
                { "ambassadorsSash",              (EArtifactId)68 },
                { "ringOfTheWayfarer",            (EArtifactId)69 },
                { "equestriansGloves",            (EArtifactId)70 },
                { "necklaceOfOceanGuidance",      (EArtifactId)71 },
                { "angelWings",                   (EArtifactId)72 },
                { "charmOfMana",                  (EArtifactId)73 },
                { "talismanOfMana",               (EArtifactId)74 },
                { "mysticOrbOfMana",              (EArtifactId)75 },
                { "collarOfConjuring",            (EArtifactId)76 },
                { "ringOfConjuring",              (EArtifactId)77 },
                { "capeOfConjuring",              (EArtifactId)78 },
                { "orbOfTheFirmament",            (EArtifactId)79 },
                { "orbOfSilt",                    (EArtifactId)80 },
                { "orbOfTempestuousFire",         (EArtifactId)81 },
                { "orbOfDrivingRain",             (EArtifactId)82 },
                { "recantersCloak",               (EArtifactId)83 },
                { "spiritOfOppression",           (EArtifactId)84 },
                { "hourglassOfTheEvilHour",       (EArtifactId)85 },
                { "tomeOfFireMagic",              (EArtifactId)86 },
                { "tomeOfAirMagic",               (EArtifactId)87 },
                { "tomeOfWaterMagic",             (EArtifactId)88 },
                { "tomeOfEarthMagic",             (EArtifactId)89 },
                { "bootsOfLevitation",            (EArtifactId)90 },
                { "goldenBow",                    (EArtifactId)91 },
                { "sphereOfPermanence",           (EArtifactId)92 },
                { "orbOfVulnerability",           (EArtifactId)93 },
                { "ringOfVitality",               (EArtifactId)94 },
                { "ringOfLife",                   (EArtifactId)95 },
                { "vialOfLifeblood",              (EArtifactId)96 },
                { "necklaceOfSwiftness",          (EArtifactId)97 },
                { "bootsOfSpeed",                 (EArtifactId)98 },
                { "capeOfVelocity",               (EArtifactId)99 },
                { "pendantOfDispassion",          (EArtifactId)100 },
                { "pendantOfSecondSight",         (EArtifactId)101 },
                { "pendantOfHoliness",            (EArtifactId)102 },
                { "pendantOfLife",                (EArtifactId)103 },
                { "pendantOfDeath",               (EArtifactId)104 },
                { "pendantOfFreeWill",            (EArtifactId)105 },
                { "pendantOfNegativity",          (EArtifactId)106 },
                { "pendantOfTotalRecall",         (EArtifactId)107 },
                { "pendantOfCourage",             (EArtifactId)108 },
                { "everflowingCrystalCloak",      (EArtifactId)109 },
                { "ringOfInfiniteGems",           (EArtifactId)110 },
                { "everpouringVialOfMercury",     (EArtifactId)111 },
                { "inexhaustibleCartOfOre",       (EArtifactId)112 },
                { "eversmokingRingOfSulfur",      (EArtifactId)113 },
                { "inexhaustibleCartOfLumber",    (EArtifactId)114 },
                { "endlessSackOfGold",            (EArtifactId)115 },
                { "endlessBagOfGold",             (EArtifactId)116 },
                { "endlessPurseOfGold",           (EArtifactId)117 },
                { "legsOfLegion",                 (EArtifactId)118 },
                { "loinsOfLegion",                (EArtifactId)119 },
                { "torsoOfLegion",                (EArtifactId)120 },
                { "armsOfLegion",                 (EArtifactId)121 },
                { "headOfLegion",                 (EArtifactId)122 },
                { "seaCaptainsHat",               (EArtifactId)123 },
                { "spellbindersHat",              (EArtifactId)124 },
                { "shacklesOfWar",                (EArtifactId)125 },
                { "orbOfInhibition",              (EArtifactId)126 },
                { "vialOfDragonBlood",            (EArtifactId)127 },
                { "armageddonsBlade",             (EArtifactId)128 },
                { "angelicAlliance",              (EArtifactId)129 },
                { "cloakOfTheUndeadKing",         (EArtifactId)130 },
                { "elixirOfLife",                 (EArtifactId)131 },
                { "armorOfTheDamned",             (EArtifactId)132 },
                { "statueOfLegion",               (EArtifactId)133 },
                { "powerOfTheDragonFather",       (EArtifactId)134 },
                { "titansThunder",                (EArtifactId)135 },
                { "admiralsHat",                  (EArtifactId)136 },
                { "bowOfTheSharpshooter",         (EArtifactId)137 },
                { "wizardsWell",                  (EArtifactId)138 },
                { "ringOfTheMagi",                (EArtifactId)139 },
                { "cornucopia",                   (EArtifactId)140 },
                { "unusedArtifact1",              (EArtifactId)141 },
                { "unusedArtifact2",              (EArtifactId)142 },
                { "unusedArtifact3",              (EArtifactId)143 },
            };
        }

        private static ECreatureId WarMachineIdentifier(string name)
        {
            switch (name)
            {
                case "catapult":    return ECreatureId.CATAPULT;
                case "ballista":    return ECreatureId.BALLISTA;
                case "ammoCart":    return ECreatureId.AMMO_CART;
                case "firstAidTent":return ECreatureId.FIRST_AID_TENT;
                default:            return ECreatureId.NONE;
            }
        }

        private static ESpellId SpellIdentifier(string name)
        {
            // Map camelCase spell names from VCMI JSON to ESpellId enum values.
            switch (name)
            {
                case "summonBoat":          return ESpellId.SUMMON_BOAT;
                case "scuttleBoat":         return ESpellId.SCUTTLE_BOAT;
                case "visions":             return ESpellId.VISIONS;
                case "viewEarth":           return ESpellId.VIEW_EARTH;
                case "disguise":            return ESpellId.DISGUISE;
                case "viewAir":             return ESpellId.VIEW_AIR;
                case "fly":                 return ESpellId.FLY;
                case "waterWalk":           return ESpellId.WATER_WALK;
                case "dimensionDoor":       return ESpellId.DIMENSION_DOOR;
                case "townPortal":          return ESpellId.TOWN_PORTAL;
                case "quicksand":           return ESpellId.QUICKSAND;
                case "landMine":            return ESpellId.LAND_MINE;
                case "forceField":          return ESpellId.FORCE_FIELD;
                case "fireWall":            return ESpellId.FIRE_WALL;
                case "earthquake":          return ESpellId.EARTHQUAKE;
                case "magicArrow":          return ESpellId.MAGIC_ARROW;
                case "iceBolt":             return ESpellId.ICE_BOLT;
                case "lightningBolt":       return ESpellId.LIGHTNING_BOLT;
                case "implosion":           return ESpellId.IMPLOSION;
                case "chainLightning":      return ESpellId.CHAIN_LIGHTNING;
                case "frostRing":           return ESpellId.FROST_RING;
                case "fireball":            return ESpellId.FIREBALL;
                case "inferno":             return ESpellId.INFERNO;
                case "meteorShower":        return ESpellId.METEOR_SHOWER;
                case "deathRipple":         return ESpellId.DEATH_RIPPLE;
                case "destroyUndead":       return ESpellId.DESTROY_UNDEAD;
                case "armageddon":          return ESpellId.ARMAGEDDON;
                case "shield":              return ESpellId.SHIELD;
                case "airShield":           return ESpellId.AIR_SHIELD;
                case "fireShield":          return ESpellId.FIRE_SHIELD;
                case "protectionFromAir":   return ESpellId.PROTECTION_FROM_AIR;
                case "protectionFromFire":  return ESpellId.PROTECTION_FROM_FIRE;
                case "protectionFromWater": return ESpellId.PROTECTION_FROM_WATER;
                case "protectionFromEarth": return ESpellId.PROTECTION_FROM_EARTH;
                case "antiMagic":           return ESpellId.ANTI_MAGIC;
                case "dispel":              return ESpellId.DISPEL;
                case "magicMirror":         return ESpellId.MAGIC_MIRROR;
                case "cure":                return ESpellId.CURE;
                case "resurrection":        return ESpellId.RESURRECTION;
                case "animateDead":         return ESpellId.ANIMATE_DEAD;
                case "sacrifice":           return ESpellId.SACRIFICE;
                case "bless":               return ESpellId.BLESS;
                case "curse":               return ESpellId.CURSE;
                case "bloodlust":           return ESpellId.BLOODLUST;
                case "precision":           return ESpellId.PRECISION;
                case "weakness":            return ESpellId.WEAKNESS;
                case "stoneSkin":           return ESpellId.STONE_SKIN;
                case "disruptingRay":       return ESpellId.DISRUPTING_RAY;
                case "prayer":              return ESpellId.PRAYER;
                case "mirth":               return ESpellId.MIRTH;
                case "sorrow":              return ESpellId.SORROW;
                case "fortune":             return ESpellId.FORTUNE;
                case "misfortune":          return ESpellId.MISFORTUNE;
                case "haste":               return ESpellId.HASTE;
                case "slow":                return ESpellId.SLOW;
                case "slayer":              return ESpellId.SLAYER;
                case "frenzy":              return ESpellId.FRENZY;
                case "titansLightningBolt": return ESpellId.TITANS_LIGHTNING_BOLT;
                case "titanBolt":           return ESpellId.TITANS_LIGHTNING_BOLT;
                case "counterstrike":       return ESpellId.COUNTERSTRIKE;
                case "berserk":             return ESpellId.BERSERK;
                case "hypnotize":           return ESpellId.HYPNOTIZE;
                case "forgetfulness":       return ESpellId.FORGETFULNESS;
                case "blind":               return ESpellId.BLIND;
                case "teleport":            return ESpellId.TELEPORT;
                case "removeObstacle":      return ESpellId.REMOVE_OBSTACLE;
                case "clone":               return ESpellId.CLONE;
                case "summonFireElemental": return ESpellId.SUMMON_FIRE_ELEMENTAL;
                case "summonEarthElemental":return ESpellId.SUMMON_EARTH_ELEMENTAL;
                case "summonWaterElemental":return ESpellId.SUMMON_WATER_ELEMENTAL;
                case "summonAirElemental":  return ESpellId.SUMMON_AIR_ELEMENTAL;
                case "dispelHelpful":       return ESpellId.DISPEL_HELPFUL_SPELLS;
                default:                    return ESpellId.NONE;
            }
        }

        private static ECreatureId CreatureIdentifier(string name)
        {
            switch (name)
            {
                case "skeleton":    return ECreatureId.SKELETON;
                case "walkingDead": return ECreatureId.WALKING_DEAD;
                case "wight":       return ECreatureId.WIGHT;
                case "lich":        return ECreatureId.LICH;
                case "vampire":     return ECreatureId.VAMPIRE;
                default:            return ECreatureId.NONE;
            }
        }

        private static ESecondarySkill SecondarySkillIdentifier(string name)
        {
            switch (name)
            {
                case "archery":     return ESecondarySkill.ARCHERY;
                case "logistics":   return ESecondarySkill.LOGISTICS;
                case "scouting":    return ESecondarySkill.SCOUTING;
                case "diplomacy":   return ESecondarySkill.DIPLOMACY;
                case "navigation":  return ESecondarySkill.NAVIGATION;
                case "leadership":  return ESecondarySkill.LEADERSHIP;
                case "wisdom":      return ESecondarySkill.WISDOM;
                case "mysticism":   return ESecondarySkill.MYSTICISM;
                case "luck":        return ESecondarySkill.LUCK;
                case "necromancy":  return ESecondarySkill.NECROMANCY;
                default:            return ESecondarySkill.DEFAULT;
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        //  Utility: strip // line comments from JSON
        // ──────────────────────────────────────────────────────────────────────

        private static string StripLineComments(string json)
        {
            var sb = new System.Text.StringBuilder(json.Length);
            bool inString = false;
            int  i        = 0;

            while (i < json.Length)
            {
                char c = json[i];

                if (inString)
                {
                    sb.Append(c);
                    if (c == '\\' && i + 1 < json.Length)
                    {
                        // Escaped character inside string — copy both chars.
                        sb.Append(json[++i]);
                    }
                    else if (c == '"')
                    {
                        inString = false;
                    }
                    i++;
                }
                else
                {
                    if (c == '"')
                    {
                        inString = true;
                        sb.Append(c);
                        i++;
                    }
                    else if (c == '/' && i + 1 < json.Length && json[i + 1] == '/')
                    {
                        // Skip everything until end of line.
                        while (i < json.Length && json[i] != '\n')
                            i++;
                    }
                    else
                    {
                        sb.Append(c);
                        i++;
                    }
                }
            }

            return sb.ToString();
        }
    }
}
