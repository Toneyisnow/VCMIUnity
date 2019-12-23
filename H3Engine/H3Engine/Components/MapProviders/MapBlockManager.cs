using H3Engine;
using H3Engine.Common;
using H3Engine.Core;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace H3Engine.Components.MapProviders
{

    public enum BlockAccessibility
    {
        FREE = 0,
        WATER = 1,
        BLOCKED = 2,
        VISITABLE = 3,
    }

    public class MapBlockManager
    {
        private H3Map h3Map = null;

        private int mapLevel = 0;

        private BlockAccessibility[,] blockAccessibilities;
        

        public MapBlockManager()
        {

        }

        public void Initialize(H3Map h3Map, int mapLevel)
        {
            this.h3Map = h3Map;
            this.mapLevel = mapLevel;

            int mapWidth = (int)h3Map.Header.Width;
            int mapHeight = (int)h3Map.Header.Height;
            blockAccessibilities = new BlockAccessibility[mapWidth, mapHeight];

            InitializeTerrain();
            InitializeMapObjects();
        }

        private void InitializeTerrain()
        {
            for (int xx = 0; xx < h3Map.Header.Width; xx++)
            {
                for (int yy = 0; yy < h3Map.Header.Height; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[mapLevel, xx, yy];
                    blockAccessibilities[xx, yy] = (tile.IsWater) ? BlockAccessibility.WATER : BlockAccessibility.FREE;
                }
            }
        }

        private void InitializeMapObjects()
        {
            int mapWidth = (int)h3Map.Header.Width;
            int mapHeight = (int)h3Map.Header.Height;

            foreach (CGObject obj in h3Map.Objects)
            {
                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }
                
                MapPosition position = obj.Position;
                if (position.Level != mapLevel)
                {
                    continue;
                }

                for (int i = 0; i < template.BlockMask.Length || i < template.VisitMask.Length; i++)
                {
                    bool isBlock = i < template.BlockMask.Length ? template.BlockMask[i] : false;
                    bool isVisit = i < template.VisitMask.Length ? template.VisitMask[i] : false;

                    if (isBlock || isVisit)
                    {
                        // Note: Since the mask is hardcoded as 8 bits per line (when read from ReadByte()), so here we just use 8
                        int xIndex = i % 8;
                        int yIndex = i / 8;
                        
                        int xPos = Math.Min(Math.Max(position.PosX - xIndex, 0), mapWidth - 1);
                        int yPos = Math.Min(Math.Max(position.PosY - yIndex, 0), mapHeight - 1);

                        if (isVisit)
                        {
                            blockAccessibilities[xPos, yPos] = BlockAccessibility.VISITABLE;
                        }
                        else if (blockAccessibilities[xPos, yPos] != BlockAccessibility.VISITABLE)
                        {
                            blockAccessibilities[xPos, yPos] = BlockAccessibility.BLOCKED;
                        }
                    }
                }
            }
        }

        public void PrintBlocks()
        {
            var logger = LoggerInstance.GetLogger();

            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                StringBuilder str = new StringBuilder();
                for (int xx = 0; xx < h3Map.Header.Width; xx++)
                {
                    str.Append(blockAccessibilities[xx, yy].GetHashCode());
                }

                logger.LogTrace(str.ToString());
            }
        }
    }

}