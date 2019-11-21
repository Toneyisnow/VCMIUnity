using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Mapping
{

    public class TerrainTile
    {
        public ETerrainType TerrainType
        {
            get; set;
        }

        public byte TerrainView
        {
            get; set;
        }

        public ERiverType RiverType
        {
            get; set;
        }

        public byte RiverDir
        {
            get; set;
        }

        public ERoadType RoadType
        {
            get; set;
        }

        public byte RoadDir
        {
            get; set;
        }

        public byte TerrainRotation
        {
            get; private set;
        }

        public byte RoadRotation
        {
            get; private set;
        }

        public byte RiverRotation
        {
            get; private set;
        }

        public bool HasFavorableWinds
        {
            get; private set;
        }

        public bool IsCoastal
        {
            get; private set;
        }


        public bool IsBlocked
        {
            get; set;
        }

        public bool IsVisitable
        {
            get; set;
        }

        public bool IsWater
        {
            get
            {
                return TerrainType == ETerrainType.WATER;
            }
        }

        /// <summary>
        /// first two bits - how to rotate terrain graphic (next two - river graphic, next two - road);
        ///	7th bit - whether tile is coastal (allows disembarking if land or block movement if water); 8th bit - Favorable Winds effect
        /// </summary>
        /// <param name="flags"></param>
        public void SetExtTileFlags(byte flags)
        {
            int flagsInt = flags;
            this.TerrainRotation = (byte)(flagsInt % 4);
            this.RiverRotation = (byte)((flagsInt >> 2) % 4);
            this.RoadRotation = (byte)((flagsInt >> 4) % 4);

            this.IsCoastal = ((flagsInt & 64) > 0);
            this.HasFavorableWinds = ((flagsInt & 128) > 0);
        }
    }
}
