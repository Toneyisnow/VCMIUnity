using ComponentAce.Compression.Archiver;
using ComponentAce.Compression.Libs.ZLib;
using ComponentAce.Compression.ZipForge;
using H3Engine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public class ArchivedFileInfo
    {
        public string FileName
        {
            get; set;
        }

        public uint Offset
        {
            get; set;
        }

        public uint Size
        {
            get; set;
        }
        public uint CSize
        {
            get; set;
        }

    }

    public class H3ArchiveData
    {
        private BinaryReader reader = null;
        private FileStream file = null;

        private List<ArchivedFileInfo> fileInfos = null;

        public static bool IsPCXImageFile(string fileName)
        {
            return fileName.EndsWith(".pcx", StringComparison.InvariantCultureIgnoreCase);
        }

        public H3ArchiveData(string lodFileFullPath)
        {
            file = new FileStream(lodFileFullPath, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(file);
            LoadHeader();
        }

        private void LoadHeader()
        {
            reader.Seek(8);
            uint count = reader.ReadUInt32();

            fileInfos = new List<ArchivedFileInfo>();
            reader.Seek(92);

            for (int fileIndex = 0; fileIndex < count; fileIndex++)
            {
                byte[] buffer = new byte[16];
                for (int i = 0; i < 16; i++)
                {
                    buffer[i] = reader.ReadByte();
                }
                string filename = System.Text.Encoding.ASCII.GetString(buffer);

                filename = filename.Substring(0, filename.IndexOf('\0'));
                uint offset = reader.ReadUInt32();
                uint size = reader.ReadUInt32();
                uint placeholder = reader.ReadUInt32();
                uint csize = reader.ReadUInt32();
                
                ArchivedFileInfo info = new ArchivedFileInfo();
                info.FileName = filename.ToLower();
                info.Offset = offset;
                info.Size = size;
                info.CSize = csize;

                fileInfos.Add(info);
            }
        }

        public List<ArchivedFileInfo> FileInfos
        {
            get
            {
                return this.fileInfos;
            }
        }

        /// <summary>
        /// Test methods, will remove soon
        /// </summary>
        /// <param name="outputFolder"></param>
        public void DumpAllFiles(string outputFolder)
        {
            for (int fileIndex = 0; fileIndex < fileInfos.Count; fileIndex++)
            {
                ArchivedFileInfo info = fileInfos[fileIndex];

                Dump(info, outputFolder);
            }
        }

        /// <summary>
        /// Test methods, will remove soon
        /// </summary>
        /// <param name="outputFolder"></param>
        public void DumpFile(string fileName, string outputFolder)
        {
            for (int fileIndex = 0; fileIndex < fileInfos.Count; fileIndex++)
            {
                ArchivedFileInfo info = fileInfos[fileIndex];

                if (info.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    Dump(info, outputFolder);
                }
            }
        }

        /// <summary>
        /// Test methods, will remove soon
        /// </summary>
        /// <param name="outputFolder"></param>
        private void Dump(ArchivedFileInfo fileInfo, string outputFolder)
        {
            try
            {
                Directory.CreateDirectory(outputFolder);
            }
            catch (Exception)
            {
                ///
            }
            
            reader.Seek((int)fileInfo.Offset);
            byte[] content;

            string filename =Path.Combine(outputFolder, fileInfo.FileName);
            string ext = Path.GetExtension(filename);
            if (Path.GetExtension(filename) == ".pcx")
            {
                filename = Path.ChangeExtension(filename, ".png");
            }

            using (MemoryStream outputStream = new MemoryStream())
            {
                if (fileInfo.CSize > 0)
                {
                    content = reader.ReadBytes((int)fileInfo.CSize);
                    MemoryStream inputStream = new MemoryStream(content);
                    DecompressStream(inputStream, outputStream);
                }
                else
                {
                    content = reader.ReadBytes((int)fileInfo.Size);
                    outputStream.Write(content, 0, content.Length);
                }
                
                using (var outputFile = new FileStream(filename, FileMode.Create, FileAccess.Write))
                {
                    outputStream.Seek(0, SeekOrigin.Begin);

                    if (H3PcxFileHandler.IsPCX(outputStream))
                    {
                        ImageData image = H3PcxFileHandler.ExtractPCXStream(outputStream);
                        //// image.SaveAsPNGStream(outputFile);
                    }
                    else
                    {
                        outputStream.CopyTo(outputFile);
                    }
                }
            }
        }

        public ArchivedFileInfo GetFileInfo(string fileName)
        {
            for (int fileIndex = 0; fileIndex < fileInfos.Count; fileIndex++)
            {
                ArchivedFileInfo info = fileInfos[fileIndex];
                if (info.FileName.Equals(fileName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return info;
                }
            }

            return null;
        }

        public IFileData ExtractFileData(string fileName)
        {
            ArchivedFileInfo fileInfo = GetFileInfo(fileName);
            if (fileInfo == null)
            {
                return null;
            }

            return ExtractFileData(fileInfo);
        }

        public IFileData ExtractFileData(ArchivedFileInfo fileInfo)
        {
            reader.Seek((int)fileInfo.Offset);
            byte[] content;
            
            using (MemoryStream outputStream = new MemoryStream())
            {
                if (fileInfo.CSize > 0)
                {
                    content = reader.ReadBytes((int)fileInfo.CSize);
                    MemoryStream inputStream = new MemoryStream(content);
                    DecompressStream(inputStream, outputStream);
                }
                else
                {
                    content = reader.ReadBytes((int)fileInfo.Size);
                    outputStream.Write(content, 0, content.Length);
                }

                outputStream.Seek(0, SeekOrigin.Begin);

                if (H3PcxFileHandler.IsPCX(outputStream))
                {
                    ImageData image = H3PcxFileHandler.ExtractPCXStream(outputStream);
                    return image;
                }
                else
                {
                    byte[] bytes = StreamHelper.ReadToEnd(outputStream);
                    return new BinaryData(bytes);
                }
            }
        }

        public static void DecompressStream(Stream inputStream, Stream ouputStream, bool isGZip = false)
        {
            if (inputStream == null || !inputStream.CanRead)
            {
                return;
            }

            if (ouputStream == null || !ouputStream.CanWrite)
            {
                return;
            }

            using (CompressedStreamReader compressedStream = new CompressedStreamReader(inputStream, isGZip))
            {
                ulong bucketSize = 1024;
                byte[] buffer = new byte[bucketSize];
                ulong readSize = 0;

                do
                {
                    readSize = compressedStream.Read(buffer, bucketSize);
                    if (readSize > 0)
                    {
                        ouputStream.Write(buffer, 0, (int)readSize);
                    }
                }
                while (readSize > 0);
            }
        }
    }
}
