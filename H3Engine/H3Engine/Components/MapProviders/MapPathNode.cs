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
			ACCESSIBLE = 1, //tile can be entered and passed
			VISITABLE = 2, //tile can be entered as the last tile in path
			BLOCKVISIT = 3,  //visitable from neighboring tile but not passable
			FLYABLE = 4, //can only be accessed in air layer
			BLOCKED = 5 //tile can't be entered nor visited
		}

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

		public int Turns
		{
			get; set;
		}

		public MapPathNode()
		{

		}




	}
}
