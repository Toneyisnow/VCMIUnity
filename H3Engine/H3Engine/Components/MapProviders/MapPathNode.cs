using H3Engine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.MapProviders
{
    public class MapPathNode
    {
        public enum ENodeAction
        {
            UNKNOWN = 0,
            NORMAL = 1,
            BATTLE = 2,
            VISIT = 3,
            BLOCKING_VISIT = 4,
            EMBARK = 5,
            DISEMBARK = 6,
            TELEPORT_NORMAL = 7,
            TELEPORT_BLOCKING_VISIT = 8,
            TELEPORT_BATTLE = 9
        }

        public enum ENodeAccessibility
        {
            NOT_SET = 0,
            ACCESSIBLE = 1,  // tile can be entered and passed through
            VISITABLE = 2,   // tile can be entered as the last tile in path (visit/fight)
            BLOCKVISIT = 3,  // visitable from neighboring tile but hero doesn't step on it
            FLYABLE = 4,     // can only be accessed in air layer
            BLOCKED = 5,     // tile can't be entered nor visited
            GUARDED = 6      // tile is accessible but is in the zone of a guarding monster
        }

        /// <summary>
        /// Link to the previous node in the path (for path reconstruction).
        /// </summary>
        public MapPathNode PreviousNode
        {
            get; set;
        }

        public MapPosition Position
        {
            get; set;
        }

        public ENodeAction NodeAction
        {
            get; set;
        }

        public ENodeAccessibility Accessibility
        {
            get; set;
        }

        /// <summary>
        /// Number of turns required to reach this tile (0 = current turn).
        /// Value of int.MaxValue means this tile has not been reached yet.
        /// </summary>
        public int Turns
        {
            get; set;
        }

        /// <summary>
        /// Movement points remaining after the hero arrives at this tile.
        /// </summary>
        public int MoveRemains
        {
            get; set;
        }

        /// <summary>
        /// Total path cost in "turns + remaining fraction" — used as priority queue key.
        /// Computed as: Turns + (maxMovePoints - MoveRemains) / maxMovePoints
        /// </summary>
        public float Cost
        {
            get; set;
        }

        /// <summary>
        /// True when this node has been finalized by Dijkstra (popped from the open set).
        /// Used for lazy-deletion in the priority queue.
        /// </summary>
        public bool Locked
        {
            get; set;
        }

        /// <summary>
        /// Returns true if this tile has been reached (i.e., is part of the computed path network).
        /// </summary>
        public bool IsReachable => Turns < int.MaxValue;

        public MapPathNode()
        {
            Reset();
        }

        /// <summary>
        /// Resets the node to its initial (unreached) state, preserving Position.
        /// </summary>
        public void Reset()
        {
            Cost = float.MaxValue;
            MoveRemains = 0;
            Turns = int.MaxValue;
            Locked = false;
            PreviousNode = null;
            Accessibility = ENodeAccessibility.NOT_SET;
            NodeAction = ENodeAction.UNKNOWN;
        }
    }
}
