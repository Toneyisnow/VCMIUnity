using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.MapProviders
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
