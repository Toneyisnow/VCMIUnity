using H3Engine.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine
{
    /// <summary>
    /// Centralized place for all resource data
    /// </summary>
    public class GameResourceStorage
    {
        private Dictionary<string, H3ArchiveData> loadedArchiveDataDict = new Dictionary<string, H3ArchiveData>();

        public GameResourceStorage()
        {

        }

        public void LoadArchive(string fileFullName)
        {
            fileFullName = fileFullName.ToLower().Trim();
            if (loadedArchiveDataDict.ContainsKey(fileFullName))
            {
                return;
            }

            H3ArchiveData archiveData = new H3ArchiveData(fileFullName);

            loadedArchiveDataDict.Add(fileFullName, archiveData);
        }

        public byte[] ExtractFileData(string fileName)
        {
            foreach(H3ArchiveData archiveData in loadedArchiveDataDict.Values)
            {
                byte[] data = archiveData.ExtractFileData(fileName);
                if (data != null)
                {
                    return data;
                }
            }

            throw new FileNotFoundException();
        }

        public ImageData ExtractImage(string fileName)
        {
            foreach (H3ArchiveData archiveData in loadedArchiveDataDict.Values)
            {
               ImageData image = archiveData.ExtractImage(fileName);
                if (image != null)
                {
                    return image;
                }
            }

            throw new FileNotFoundException();
        }
    }
}
