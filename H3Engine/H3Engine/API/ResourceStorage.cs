﻿using H3Engine.FileSystem;
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
        private Dictionary<string, H3ArchiveData> loadedArchiveDataDict = new Dictionary<string, H3ArchiveData>();

        public ResourceStorage()
        {

        }

        public void LoadArchive(string fileFullName)
        {
            // Bug: Since on the iOS, the file path is case sensitive, we should not do the ToLower() here.
            ////fileFullName = fileFullName.ToLower().Trim();
            if (loadedArchiveDataDict.ContainsKey(fileFullName))
            {
                return;
            }

            H3ArchiveData archiveData = new H3ArchiveData(fileFullName);

            loadedArchiveDataDict.Add(fileFullName, archiveData);
        }

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
