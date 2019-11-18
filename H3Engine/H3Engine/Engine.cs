using H3Engine.Campaign;
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

        private void UnZipFile_7z(string fileFullPath, string targetDirectoryPath)
        {
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Environment.Is64BitProcess ? "x64" : "x86", "7z.dll");
            SevenZip.SevenZipBase.SetLibraryPath(path);
            SevenZip.SevenZipExtractor.SetLibraryPath(path);
            using (var extractor = new SevenZip.SevenZipExtractor(fileFullPath))
            {
                extractor.ExtractArchive(targetDirectoryPath);
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

        public void HandleH3CFile(string fileFullPath, string targetDirectoryPath)
        {
            using (FileStream file = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read))
            {
                byte[] rawBytes = StreamHelper.ReadToEnd(file);

                List<byte> fileBytes = new List<byte>();
                int outFileIndex = 0;

                long index = 0;
                while (index < rawBytes.Length)
                {
                    if (rawBytes[index] == 0x1F)
                    {
                        if (rawBytes[index + 1] == 0x8B)
                        {
                            if (rawBytes[index + 2] == 0x08)
                            {
                                if (rawBytes[index + 3] == 0x00)
                                {
                                    if (fileBytes.Count > 0)
                                    {
                                        // Save the current bytes into file
                                        string outFileName = Path.Combine(targetDirectoryPath, string.Format(@"out-{0}", outFileIndex ++));
                                        StreamHelper.WriteBytesToFile(outFileName, fileBytes.ToArray());
                                    }

                                    fileBytes = new List<byte>() { 0x1F, 0x8B, 0x08, 0x00 };
                                    index += 4;
                                }
                                else
                                {
                                    fileBytes.Add(rawBytes[index]);
                                    fileBytes.Add(rawBytes[index + 1]);
                                    fileBytes.Add(rawBytes[index + 2]);
                                    index += 3;
                                }
                            }
                            else
                            {
                                fileBytes.Add(rawBytes[index]);
                                fileBytes.Add(rawBytes[index + 1]);
                                index += 2;
                            }
                        }
                        else
                        {
                            fileBytes.Add(rawBytes[index]);
                            index ++;
                        }
                    }
                    else
                    {
                        fileBytes.Add(rawBytes[index]);
                        index++;
                    }
                }

                if (fileBytes.Count > 0)
                {
                    // Save the current bytes into file
                    string outFileName = Path.Combine(targetDirectoryPath, string.Format(@"out-{0}", outFileIndex));
                    StreamHelper.WriteBytesToFile(outFileName, fileBytes.ToArray());
                }
            }
        }

        public void UnZipFile(string fileFullPath, string targetDirectoryPath)
        {
            UnZipFile_GZip(fileFullPath, targetDirectoryPath);



            //// ZipFile.ExtractToDirectory(fileFullPath, targetDirectoryPath);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public H3Campaign RetrieveCampaign(string fileName)
        {
            byte[] data = gameResourceStorage.ExtractFileData(fileName);

            H3CampaignLoader loader = new H3CampaignLoader(fileName, data);

            H3Campaign campaign = loader.LoadCampaign();

            return campaign;
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
