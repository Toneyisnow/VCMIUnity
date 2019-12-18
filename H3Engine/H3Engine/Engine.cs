using H3Engine.API;
using H3Engine.Campaign;
using H3Engine.Common;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Mapping;
using H3Engine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine
{
    public class Engine
    {
        private static Engine engineInstance = null;

        private ResourceStorage resourceStorage = null;

        private ResourceHandler resourceHandler = null;

        private ResourceUsage resourceUsage = null;


        public static Engine GetInstance()
        {
            if (engineInstance == null)
            {
                engineInstance = new Engine();
            }

            return engineInstance;
        }

        private Engine()
        {
            resourceStorage = new ResourceStorage();

            resourceHandler = new ResourceHandler(resourceStorage);

            resourceUsage = new ResourceUsage(resourceHandler);
        }

        public ResourceStorage ResourceStorage
        {
            get
            {
                return this.resourceStorage;
            }
        }

        public ResourceHandler ResourceHandler
        {
            get
            {
                return this.resourceHandler;
            }
        }

        public ResourceUsage ResourceUsage
        {
            get
            {
                return this.resourceUsage;
            }
        }


        private void UnZipFile_CompressedStream(string fileFullPath, string targetDirectoryPath)
        {
            using (FileStream file = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
            {
                using (FileStream output = new FileStream(targetDirectoryPath + @"\output", FileMode.Create, FileAccess.Write))
                {
                    H3ArchiveData.DecompressStream(file, output, true);
                }
            }
        }

        private void UnZipFile_GZip(string fileFullPath, string targetDirectoryPath)
        {
            using (FileStream file = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
            {
                byte[] gzip = StreamHelper.ReadToEnd(file);
                using (GZipStream stream = new GZipStream(new MemoryStream(gzip), CompressionMode.Decompress))
                {
                    const int size = 2096;
                    byte[] buffer = new byte[size];
                    using (FileStream output = new FileStream(fileFullPath + ".out", FileMode.Create, FileAccess.Write))
                    {
                        int count = 0;
                        do
                        {
                            count = stream.Read(buffer, 0, size);
                            if (count > 0)
                            {
                                output.Write(buffer, 0, count);
                            }
                        }
                        while (count > 0);
                    }
                }
            }
        }

        private void UnZipFile(string fileFullPath, string targetDirectoryPath)
        {
            UnZipFile_GZip(fileFullPath, targetDirectoryPath);
        }


        ///////// API from Storage Layer //////////

        /// <summary>
        /// Loading the archive files (LOD, etc.) to the memory
        /// </summary>
        /// <param name="fileFullPath"></param>
        public void LoadArchiveFile(string fileFullPath)
        {
            resourceStorage.LoadArchive(fileFullPath);
        }

        /// <summary>
        /// This is a most common low level method, that could return the byte array of a given file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public byte[] RetrieveFileData(string fileName)
        {
            return resourceStorage.ExtractFileData(fileName);
        }

        public ImageData RetrieveImage(string imageName)
        {
            return resourceStorage.ExtractImage(imageName);
        }

        public List<string> SearchResourceFiles(string namePattern)
        {
            return resourceStorage.SearchResourceFiles(namePattern);
        }

        ///////// API from Handler Layer //////////

        /// <summary>
        /// Retrive the H3Map data from file
        /// </summary>
        /// <param name="mapFileFullPath"></param>
        /// <returns></returns>
        public H3Map LoadH3MapFile(string h3mFileFullPath)
        {
            return resourceHandler.RetrieveMap(h3mFileFullPath);
        }

        /// <summary>
        /// Retrive the H3Map data from data
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public H3Map ReteiveMap(string h3mFileFullPath)
        {
            return resourceHandler.RetrieveMap(h3mFileFullPath);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public H3Campaign RetrieveCampaign(string fileName)
        {
            return resourceHandler.RetrieveCampaign(fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="defFileName">The name of the .def file, not including the ext name</param>
        /// <returns></returns>
        public BundleImageDefinition RetrieveBundleImage(string defFileName)
        {
            return resourceHandler.RetrieveBundleImage(defFileName);
        }


        ////// Resource Usage Layer ///////

        /// <summary>
        /// 
        /// </summary>
        public ImageData RetrieveTerrainImage(ETerrainType terrainType, int terrainIndex)
        {
            return resourceUsage.RetrieveTerrainImage(terrainType, terrainIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terrainType"></param>
        /// <returns></returns>
        public ImageData[] RetrieveAllTerrainImages(ETerrainType terrainType)
        {
            return resourceUsage.RetrieveAllTerrainImages(terrainType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roadType"></param>
        /// <returns></returns>
        public ImageData[] RetrieveAllRoadImages(ERoadType roadType)
        {
            return resourceUsage.RetrieveAllRoadImages(roadType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="riverType"></param>
        /// <returns></returns>
        public ImageData[] RetrieveAllRiverImages(ERiverType riverType)
        {
            return resourceUsage.RetrieveAllRiverImages(riverType);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roadType"></param>
        /// <param name="roadDir"></param>
        /// <returns></returns>
        public ImageData RetrieveRoadImage(ERoadType roadType, int roadDir)
        {
            return resourceUsage.RetrieveRoadImage(roadType, roadDir);
        }

        public ImageData RetrieveRiverImage(ERiverType riverType, int riverDir)
        {
            return resourceUsage.RetrieveRiverImage(riverType, riverDir);
        }
    }
}
