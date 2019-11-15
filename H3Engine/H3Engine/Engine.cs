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

        /// <summary>
        /// Retrive the H3Map data from data
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public H3Map ReteiveMap(string mapName)
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageName"></param>
        /// <returns></returns>
        public ImageData RetrieveImageTest()
        {
            string imageName = "Bo53Muck.pcx";
            return gameResourceStorage.ExtractImage(imageName);

            /*
            using (FileStream file = new FileStream(@"D:\PlayGround\H3sprite\CABEHE.def", FileMode.Open, FileAccess.Read))
            {
                H3DefFileHandler def = new H3DefFileHandler(file);

                def.LoadAllFrames();

                AnimationDefinition animation = def.GetAnimation();

                for (int g = 0; g < animation.Groups.Count; g++)
                {
                    for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
                    {
                        ImageData image = animation.ComposeFrameImage(g, i);
                        return image;

                        //string filename = string.Format(@"D:\PlayGround\H3sprite\cabehe.{0:00}.{1:00}.png", g, i);
                        //using (FileStream outputFile = new FileStream(filename, FileMode.Create, FileAccess.Write))
                        {
                        //    image.SaveAsPNGStream(outputFile);
                        }
                    }
                }
            }

            return null;
            */
        }

        public ImageData RetrieveImage(string imageName)
        {
            return gameResourceStorage.ExtractImage(imageName);
        }

    }
}
