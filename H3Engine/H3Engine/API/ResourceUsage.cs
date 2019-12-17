using H3Engine.Common;
using H3Engine.FileSystem;
using H3Engine.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.API
{
    public class ResourceUsage
    {
        private ResourceHandler resourceHandler = null;

        private Dictionary<ETerrainType, BundleImageDefinition> terrainImageDefinitions = new Dictionary<ETerrainType, BundleImageDefinition>();
        private Dictionary<ERoadType, BundleImageDefinition> roadImageDefinitions = new Dictionary<ERoadType, BundleImageDefinition>();
        private Dictionary<ERiverType, BundleImageDefinition> riverImageDefinitions = new Dictionary<ERiverType, BundleImageDefinition>();

        public ResourceUsage(ResourceHandler handler)
        {
            this.resourceHandler = handler;
        }

        /// <summary>
        /// This is legacy
        /// </summary>
        /// <param name="terrainType"></param>
        /// <param name="terrainIndex"></param>
        /// <returns></returns>
        public ImageData RetrieveTerrainImage(ETerrainType terrainType, int terrainIndex)
        {
            if (!terrainImageDefinitions.ContainsKey(terrainType))
            {
                string terrainDefFileName = string.Empty;
                switch(terrainType)
                {
                    case ETerrainType.DIRT:
                        terrainDefFileName = "dirt";
                        break;
                    case ETerrainType.GRASS:
                        terrainDefFileName = "gras";
                        break;
                    case ETerrainType.LAVA:
                        terrainDefFileName = "lava";
                        break;
                    case ETerrainType.ROCK:
                        terrainDefFileName = "rock";
                        break;
                    case ETerrainType.SAND:
                        terrainDefFileName = "sand";
                        break;
                    case ETerrainType.SNOW:
                        terrainDefFileName = "snow";
                        break;
                    case ETerrainType.SUBTERRANEAN:
                        terrainDefFileName = "subb";
                        break;
                    case ETerrainType.SWAMP:
                        terrainDefFileName = "swmp";
                        break;
                    case ETerrainType.WATER:
                        terrainDefFileName = "watr";
                        break;
                    default:
                        break;
                }

                terrainDefFileName += "tl.def";

                BundleImageDefinition def = resourceHandler.RetrieveBundleImage(terrainDefFileName);
                if (def != null)
                {
                    terrainImageDefinitions[terrainType] = def;
                }
                else
                {
                    return null;
                }
            }

            BundleImageDefinition definition = terrainImageDefinitions[terrainType];
            if (definition == null || definition.Groups.Count < 1 || definition.Groups[0].Frames.Count <= terrainIndex)
            {
                return null;
            }

            ImageData image = definition.GetImageData(0, terrainIndex);
            image.ExportDataToPNG(true);

            return image;
        }

        /// <summary>
        /// This should be used by the higher level
        /// </summary>
        /// <param name="terrainType"></param>
        /// <returns></returns>
        public ImageData[] RetrieveAllTerrainImages(ETerrainType terrainType)
        {
            if (!terrainImageDefinitions.ContainsKey(terrainType))
            {
                string terrainDefFileName = string.Empty;
                switch (terrainType)
                {
                    case ETerrainType.DIRT:
                        terrainDefFileName = "dirt";
                        break;
                    case ETerrainType.GRASS:
                        terrainDefFileName = "gras";
                        break;
                    case ETerrainType.LAVA:
                        terrainDefFileName = "lava";
                        break;
                    case ETerrainType.ROCK:
                        terrainDefFileName = "rock";
                        break;
                    case ETerrainType.SAND:
                        terrainDefFileName = "sand";
                        break;
                    case ETerrainType.SNOW:
                        terrainDefFileName = "snow";
                        break;
                    case ETerrainType.SUBTERRANEAN:
                        terrainDefFileName = "subb";
                        break;
                    case ETerrainType.SWAMP:
                        terrainDefFileName = "swmp";
                        break;
                    case ETerrainType.WATER:
                        terrainDefFileName = "watr";
                        break;
                    default:
                        break;
                }

                terrainDefFileName += "tl.def";

                BundleImageDefinition def = resourceHandler.RetrieveBundleImage(terrainDefFileName);
                if (def != null)
                {
                    terrainImageDefinitions[terrainType] = def;
                }
                else
                {
                    return null;
                }
            }

            BundleImageDefinition definition = terrainImageDefinitions[terrainType];
            if (definition == null || definition.Groups.Count < 1)
            {
                return null;
            }

            int frameCount = definition.Groups[0].Frames.Count;

            ImageData[] result = new ImageData[frameCount];
            for(int i = 0; i < frameCount; i ++)
            {
                ImageData image = definition.GetImageData(0, i);
                image.ExportDataToPNG(true);
                result[i] = image;
            }
            
            return result;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="roadType"></param>
        /// <param name="roadIndex"></param>
        /// <returns></returns>
        public ImageData RetrieveRoadImage(ERoadType roadType, int roadIndex)
        {
            if (!roadImageDefinitions.ContainsKey(roadType))
            {
                string roadDefFileName = string.Empty;
                switch (roadType)
                {
                    case ERoadType.DIRT_ROAD:
                        roadDefFileName = "dirt";
                        break;
                    case ERoadType.GRAVEL_ROAD:
                        roadDefFileName = "grav";
                        break;
                    case ERoadType.COBBLESTONE_ROAD:
                        roadDefFileName = "cobb";
                        break;
                    case ERoadType.NO_ROAD:
                        return null;
                    default:
                        return null;
                }

                roadDefFileName += "rd.def";

                BundleImageDefinition def = resourceHandler.RetrieveBundleImage(roadDefFileName);
                if (def != null)
                {
                    roadImageDefinitions[roadType] = def;
                }
                else
                {
                    return null;
                }
            }

            BundleImageDefinition definition = roadImageDefinitions[roadType];
            if (definition == null || definition.Groups.Count < 1 || definition.Groups[0].Frames.Count <= roadIndex)
            {
                return null;
            }

            ImageData image = definition.GetImageData(0, roadIndex);
            image.ExportDataToPNG(true);

            return image;
        }

        public ImageData[] RetrieveAllRoadImages(ERoadType roadType)
        {
            if (!roadImageDefinitions.ContainsKey(roadType))
            {
                string roadDefFileName = string.Empty;
                switch (roadType)
                {
                    case ERoadType.DIRT_ROAD:
                        roadDefFileName = "dirt";
                        break;
                    case ERoadType.GRAVEL_ROAD:
                        roadDefFileName = "grav";
                        break;
                    case ERoadType.COBBLESTONE_ROAD:
                        roadDefFileName = "cobb";
                        break;
                    case ERoadType.NO_ROAD:
                        return null;
                    default:
                        return null;
                }

                roadDefFileName += "rd.def";

                BundleImageDefinition def = resourceHandler.RetrieveBundleImage(roadDefFileName);
                if (def != null)
                {
                    roadImageDefinitions[roadType] = def;
                }
                else
                {
                    return null;
                }
            }

            BundleImageDefinition definition = roadImageDefinitions[roadType];
            if (definition == null || definition.Groups.Count < 1)
            {
                return null;
            }

            int frameCount = definition.Groups[0].Frames.Count;

            ImageData[] result = new ImageData[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                ImageData image = definition.GetImageData(0, i);
                image.ExportDataToPNG(true);
                result[i] = image;
            }

            return result;
        }

        public ImageData RetrieveRiverImage(ERiverType riverType, int riverIndex)
        {
            if (!riverImageDefinitions.ContainsKey(riverType))
            {
                string riverDefFileName = string.Empty;
                switch (riverType)
                {
                    case ERiverType.CLEAR_RIVER:
                        riverDefFileName = "clr";
                        break;
                    case ERiverType.ICY_RIVER:
                        riverDefFileName = "icy";
                        break;
                    case ERiverType.LAVA_RIVER:
                        riverDefFileName = "lav";
                        break;
                    case ERiverType.MUDDY_RIVER:
                        riverDefFileName = "mud";
                        break;
                    case ERiverType.NO_RIVER:
                        return null;
                    default:
                        return null;
                }

                riverDefFileName += "rvr.def";

                BundleImageDefinition def = resourceHandler.RetrieveBundleImage(riverDefFileName);
                if (def != null)
                {
                    riverImageDefinitions[riverType] = def;
                }
                else
                {
                    return null;
                }
            }

            BundleImageDefinition definition = riverImageDefinitions[riverType];
            if (definition == null || definition.Groups.Count < 1 || definition.Groups[0].Frames.Count <= riverIndex)
            {
                return null;
            }

            ImageData image = definition.GetImageData(0, riverIndex);
            image.ExportDataToPNG(true);

            return image;
        }

        public ImageData[] RetrieveAllRiverImages(ERiverType riverType)
        {
            if (!riverImageDefinitions.ContainsKey(riverType))
            {
                string riverDefFileName = string.Empty;
                switch (riverType)
                {
                    case ERiverType.CLEAR_RIVER:
                        riverDefFileName = "clr";
                        break;
                    case ERiverType.ICY_RIVER:
                        riverDefFileName = "icy";
                        break;
                    case ERiverType.LAVA_RIVER:
                        riverDefFileName = "lav";
                        break;
                    case ERiverType.MUDDY_RIVER:
                        riverDefFileName = "mud";
                        break;
                    case ERiverType.NO_RIVER:
                        return null;
                    default:
                        return null;
                }

                riverDefFileName += "rvr.def";

                BundleImageDefinition def = resourceHandler.RetrieveBundleImage(riverDefFileName);
                if (def != null)
                {
                    riverImageDefinitions[riverType] = def;
                }
                else
                {
                    return null;
                }
            }

            BundleImageDefinition definition = riverImageDefinitions[riverType];
            if (definition == null || definition.Groups.Count < 1)
            {
                return null;
            }

            int frameCount = definition.Groups[0].Frames.Count;
            ImageData[] result = new ImageData[frameCount];
            for (int i = 0; i < frameCount; i++)
            {
                ImageData image = definition.GetImageData(0, i);
                image.ExportDataToPNG(true);
                result[i] = image;
            }

            return result;
        }
    }
}
