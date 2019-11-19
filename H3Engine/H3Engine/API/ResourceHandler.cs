using H3Engine.Campaign;
using H3Engine.FileSystem;
using H3Engine.GUI;
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

            byte[] data = resourceStorage.ExtractFileData(fileName);

            H3CampaignLoader loader = new H3CampaignLoader(fileName, data);
            H3Campaign campaign = loader.LoadCampaign();

            campaignsCache[fileName] = campaign;

            return campaign;
        }

        public BundleImageDefinition RetrieveBundleImage(string defFileName)
        {
            if (bundleImageCache.ContainsKey(defFileName))
            {
                return bundleImageCache[defFileName];
            }

            byte[] animationRawData = resourceStorage.ExtractFileData(defFileName);
            if (animationRawData == null)
            {
                return null;
            }

            using (MemoryStream animationStream = new MemoryStream(animationRawData))
            {
                H3DefFileHandler defHandler = new H3DefFileHandler(animationStream);
                defHandler.LoadAllFrames();

                bundleImageCache[defFileName] = defHandler.GetBundleImage();

                return bundleImageCache[defFileName];
            }
        }



    }
}
