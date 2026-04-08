// Moved from H3Engine.Components.Data.CreatureSet 鈫?H3Engine.Core
// (VCMI: lib/mapObjects/army/CCreatureSet.h)

using H3Engine.Common;
using System.Collections.Generic;
using H3Engine.Core.Constants;

namespace H3Engine.Core
{
    /// <summary>
    /// Describes one creature stack: creature type + count.
    /// Corresponds to VCMI's CStackBasicDescriptor.
    /// </summary>
    public class StackDescriptor
    {
        public ECreatureId CreatureId
        {
            get; set;
        } = ECreatureId.NONE;

        public H3Creature Creature
        {
            get; set;
        }

        public int Amount
        {
            get; set;
        }
    }

    /// <summary>
    /// An army of up to 7 creature stacks carried by a hero, town, or guarding creature.
    /// Corresponds to VCMI's CCreatureSet.
    /// </summary>
    public class CreatureSet
    {
        public CreatureSet()
        {
            Stacks = new List<StackDescriptor>();
        }

        public EArmyFormationType FormationType
        {
            get; set;
        }

        public List<StackDescriptor> Stacks
        {
            get; set;
        }
    }
}


