using H3Engine.FileSystem;
using H3Engine.Utils;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.API
{
    /// <summary>
    /// Centralized place for all resource data
    /// </summary>
    public class ResourceStorage
    {
        private string temporaryCacheFolderPath = null;

        private Dictionary<string, H3ArchiveData> loadedArchiveDataDict = new Dictionary<string, H3ArchiveData>();

        private Dictionary<string, IFileData> resourceFileCache = new Dictionary<string, IFileData>();


        public ResourceStorage()
        {

        }

        public void LoadArchive(string fileFullName)
        {
            // Bug: Since on the iOS, the file path is case sensitive, we should not do the ToLower() here.
            ////fileFullName = fileFullName.ToLower().Trim();
            string archiveKey = GetArchiveKey(fileFullName);

            if (DoesSupportCaching())
            {
                string archiveCacheFolder = Path.Combine(temporaryCacheFolderPath, archiveKey);

                // If the folder exists, it will be considered loaded for this archive. Updating the archive is not implemented yet.
                if (Directory.Exists(archiveCacheFolder))
                {
                    loadedArchiveDataDict[archiveKey] = null;
                    return;
                }

                // Load all files from the Archive into folder
                Directory.CreateDirectory(archiveCacheFolder);

                H3ArchiveData archiveData = new H3ArchiveData(fileFullName);
                foreach (ArchivedFileInfo fileInfo in archiveData.FileInfos)
                {
                    string fileFullPath = Path.Combine(archiveCacheFolder, fileInfo.FileName);
                    IFileData data = archiveData.ExtractFileData(fileInfo);

                    StreamHelper.WriteBytesToFile(fileFullPath, data.SerializeToBytes());
                }

                loadedArchiveDataDict[archiveKey] = null;

                return;
            }
            else
            {
                if (!loadedArchiveDataDict.ContainsKey(archiveKey))
                {
                    loadedArchiveDataDict[archiveKey] = new H3ArchiveData(fileFullName);
                }
            }
        }
        
        /// <summary>
        /// From design perspective, this method should not be used any more, since all of the resources should be specifically requested by precise name
        /// </summary>
        /// <param name="namePattern"></param>
        /// <returns></returns>
        public List<string> SearchResourceFiles(string namePattern)
        {
            List<string> result = new List<string>();

            foreach(string key in loadedArchiveDataDict.Keys)
            {
                H3ArchiveData archiveData = loadedArchiveDataDict[key];

                foreach(var fileInfo in archiveData.FileInfos)
                {
                    if (fileInfo.FileName.WildCardMatching(namePattern))
                    {
                        result.Add(fileInfo.FileName);
                    }
                }
            }

            result.Sort();

            return result;
        }

        public void SetTemporaryCachePath(string tempFolderPath)
        {
            temporaryCacheFolderPath = tempFolderPath;
        }

        public IFileData ExtractFileData(string fileName)
        {
            if (fileName == "tl.def")
            {
                fileName = "grastl.def";
            }

            if (DoesSupportCaching())
            {
                foreach(string archiveKey in loadedArchiveDataDict.Keys)
                {
                    string archiveCacheFolder = Path.Combine(temporaryCacheFolderPath, archiveKey);
                    if (!Directory.Exists(archiveCacheFolder))
                    {
                        continue;
                    }

                    string fileFullPath = Path.Combine(archiveCacheFolder, fileName);
                    if (File.Exists(fileFullPath))
                    {
                        byte[] data = File.ReadAllBytes(fileFullPath);
                        if (H3ArchiveData.IsPCXImageFile(fileName))
                        {
                            ImageData imageData = new ImageData();
                            imageData.DeserializeFromBytes(data);
                            return imageData;
                        }
                        else
                        {
                            BinaryData binaryData = new BinaryData();
                            binaryData.DeserializeFromBytes(data);
                            return binaryData;
                        }
                    }
                }
            }
            else
            {
                foreach (H3ArchiveData archiveData in loadedArchiveDataDict.Values)
                {
                    IFileData data = archiveData.ExtractFileData(fileName);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }

            throw new FileNotFoundException();
        }

        private string GetArchiveKey(string archiveFileFullPath)
        {
            string archiveKey = Path.GetFileName(archiveFileFullPath).Replace(".", "");
            return archiveKey;
        }

        private bool DoesSupportCaching()
        {
            return !string.IsNullOrWhiteSpace(temporaryCacheFolderPath) && Directory.Exists(temporaryCacheFolderPath);
        }
    }
}
