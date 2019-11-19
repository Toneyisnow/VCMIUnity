using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Mapping
{

    public class TerrainTile
    {
        public int TerrainType
        {
            get; set;
        }

        public int TerrainView
        {
            get; set;
        }

        public int RiverType
        {
            get; set;
        }

        public int RiverDir
        {
            get; set;
        }

        public int RoadType
        {
            get; set;
        }

        public int RoadDir
        {
            get; set;
        }

        public int ExtTileFlags
        {
            get; set;
        }
    }
}
