using H3Engine.Campaign;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine
{
    public class Engine
    {
        private static Engine engineInstance = null;

        private GameResourceStorage gameResourceStorage = null;

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
            gameResourceStorage = new GameResourceStorage();
        }

        /// <summary>
        /// Loading the archive files (LOD, etc.) to the memory
        /// </summary>
        /// <param name="fileFullPath"></param>
        public void LoadArchiveFile(string fileFullPath)
        {
            gameResourceStorage.LoadArchive(fileFullPath);

        }

        public byte[] RetrieveFileData(string fileName)
        {
            return gameResourceStorage.ExtractFileData(fileName);
        }

        /// <summary>
        /// Retrive the H3Map data from data
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public H3Map ReteiveMap(string mapName)
        {
            return null;
        }

        public H3Campaign RetrieveScenario(string fileName)
        {
            byte[] data = gameResourceStorage.ExtractFileData(fileName);

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animationName">The name of the .def file, not including the ext name</param>
        /// <returns></returns>
        public AnimationDefinition RetrieveAnimation(string animationName)
        {
            byte[] animationRawData = gameResourceStorage.ExtractFileData(animationName);
            if (animationRawData == null)
            {
                return null;
            }

            using (MemoryStream animationStream = new MemoryStream(animationRawData))
            {
                H3DefFileHandler defHandler = new H3DefFileHandler(animationStream);

                defHandler.LoadAllFrames();

                return defHandler.GetAnimation();
            }
            
        }
        

        public ImageData RetrieveImage(string imageName)
        {
            return gameResourceStorage.ExtractImage(imageName);
        }

    }
}
