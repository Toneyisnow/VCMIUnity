using H3Engine.Campaign;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.API
{
    public class ResourceHandler
    {
        private ResourceStorage resourceStorage = null;

        private Dictionary<string, H3Campaign> campaignsCache = new Dictionary<string, H3Campaign>();

        private Dictionary<string, BundleImageDefinition> bundleImageCache = new Dictionary<string, BundleImageDefinition>();

        public ResourceHandler(ResourceStorage storage)
        {
            this.resourceStorage = storage;
        }

        public H3Campaign RetrieveCampaign(string fileName)
        {
            if (campaignsCache.ContainsKey(fileName))
            {
                return campaignsCache[fileName];
            }

            BinaryData data = resourceStorage.ExtractFileData(fileName) as BinaryData;

            H3CampaignLoader loader = new H3CampaignLoader(fileName, data.Bytes);
            H3Campaign campaign = loader.LoadCampaign();

            campaignsCache[fileName] = campaign;

            return campaign;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h3mFileFullPath"></param>
        /// <returns></returns>
        public H3Map RetrieveMap(string h3mFileFullPath)
        {
            H3MapLoader mapLoader = new H3MapLoader(h3mFileFullPath);   // TODO: Should we save the map object  into cache?
            H3Map map = mapLoader.LoadMap();

            foreach (ObjectTemplate template in map.ObjectTemplates)
            {
                EnhanceObjectTemplateByMask(template);
            }

            return map;
        }

        public H3Map RetrieveMap(H3Campaign campaign, int mapIndex)
        {
            H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, mapIndex);

            ////foreach (ObjectTemplate template in map.ObjectTemplates)
            {
                ////EnhanceObjectTemplateByMask(template);
            }

            return map;
        }

        public BundleImageDefinition RetrieveBundleImage(string defFileName)
        {
            defFileName = defFileName.ToLower();

            if (bundleImageCache.ContainsKey(defFileName))
            {
                return bundleImageCache[defFileName];
            }

            BinaryData animationRawData = resourceStorage.ExtractFileData(defFileName) as BinaryData;
            if (animationRawData == null)
            {
                return null;
            }

            using (MemoryStream animationStream = new MemoryStream(animationRawData.Bytes))
            {
                H3DefFileHandler defHandler = new H3DefFileHandler(animationStream);
               
                defHandler.LoadAllFrames();

                bundleImageCache[defFileName] = defHandler.GetBundleImage();
                bundleImageCache[defFileName].Identity = defFileName.Replace(".def", "");

                return bundleImageCache[defFileName];
            }
        }

        public void ReleaseBundleImage(string defFileName)
        {
            defFileName = defFileName.ToLower();
            if (bundleImageCache.ContainsKey(defFileName))
            {
                bundleImageCache.Remove(defFileName);
            }
        }

        /// <summary>
        /// Read the .msk file from resource storage, and fill the Width and Height information
        /// </summary>
        /// <param name="objectTemplate"></param>
        private void EnhanceObjectTemplateByMask(ObjectTemplate objectTemplate)
        {
            if(objectTemplate == null)
            {
                return;
            }

            string maskFileName = objectTemplate.AnimationFile.Replace(@".def", @".msk");
            BinaryData bData = resourceStorage.ExtractFileData(maskFileName) as BinaryData;
            byte[] data = bData.Bytes;

            if (data != null && data.Length > 2)
            {
                objectTemplate.Width = data[0];
                objectTemplate.Height = data[1];
            }
        }
    }
}
