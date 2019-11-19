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

        public ResourceUsage(ResourceHandler handler)
        {
            this.resourceHandler = handler;
        }

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

            return definition.GetImageData(0, terrainIndex);
        }
    }
}
