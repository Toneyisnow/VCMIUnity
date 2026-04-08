using H3Engine.Common;
using H3Engine.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Engine.PathFinder
{
    public class MapPathNodeInfo
    {
        public MapPathNodeInfo()
        {

        }

        public ETerrainType TerrainType
        {
            get; set;
        }

        public MapPathNode Node
        {
            get; set;
        }

    }
}


