using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using H3Engine.DataAccess;
using H3Engine.FileSystem;
using H3Engine.GUI;

namespace LodDumper
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: LodDumper <lod-file-path> <output-path>");
                Console.WriteLine();
                Console.WriteLine("Extracts all files from an H3 LOD archive.");
                Console.WriteLine("DEF files are additionally expanded into subfolders with PNG frames.");
                Console.WriteLine("PCX images are converted to PNG. Text files are saved as TXT.");
                return 1;
            }

            string lodFilePath = args[0];
            string outputPath = args[1];

            if (!File.Exists(lodFilePath))
            {
                Console.Error.WriteLine("Error: LOD file not found: " + lodFilePath);
                return 1;
            }

            Directory.CreateDirectory(outputPath);

            Console.WriteLine("Loading LOD archive: " + lodFilePath);
            H3ArchiveData archive = new H3ArchiveData(lodFilePath);

            Console.WriteLine("Found {0} files in archive.", archive.FileInfos.Count);

            int fileIndex = 0;
            int defCount = 0;
            int pcxCount = 0;
            int otherCount = 0;

            foreach (ArchivedFileInfo fileInfo in archive.FileInfos)
            {
                fileIndex++;
                string fileName = fileInfo.FileName;
                string ext = Path.GetExtension(fileName).ToLowerInvariant();

                Console.Write("[{0}/{1}] {2}", fileIndex, archive.FileInfos.Count, fileName);

                try
                {
                    if (ext == ".def")
                    {
                        // Extract DEF file as raw binary
                        string defOutputPath = Path.Combine(outputPath, fileName);
                        ExtractRawFile(archive, fileInfo, defOutputPath);

                        // Also expand DEF into a subfolder with PNG frames
                        string defFolderName = Path.GetFileNameWithoutExtension(fileName);
                        string defFolder = Path.Combine(outputPath, defFolderName);
                        int frameCount = ExpandDefFile(archive, fileInfo, defFolder);
                        Console.WriteLine(" -> DEF expanded ({0} frames)", frameCount);
                        defCount++;
                    }
                    else if (ext == ".pcx")
                    {
                        // Convert PCX to PNG
                        string pngPath = Path.Combine(outputPath, Path.ChangeExtension(fileName, ".png"));
                        ExtractPcxAsPng(archive, fileInfo, pngPath);
                        Console.WriteLine(" -> PNG");
                        pcxCount++;
                    }
                    else
                    {
                        // Text or other binary files: extract as-is
                        string filePath = Path.Combine(outputPath, fileName);

                        // If it looks like a text file, save with .txt extension
                        if (IsTextExtension(ext))
                        {
                            ExtractRawFile(archive, fileInfo, filePath);
                        }
                        else
                        {
                            ExtractRawFile(archive, fileInfo, filePath);
                        }

                        Console.WriteLine(" -> extracted");
                        otherCount++;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" -> ERROR: " + ex.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Done. DEF: {0}, PCX->PNG: {1}, Other: {2}, Total: {3}",
                defCount, pcxCount, otherCount, fileIndex);

            return 0;
        }

        /// <summary>
        /// Extracts a file from the archive as raw bytes and writes to disk.
        /// </summary>
        static void ExtractRawFile(H3ArchiveData archive, ArchivedFileInfo fileInfo, string outputPath)
        {
            IFileData fileData = archive.ExtractFileData(fileInfo);
            if (fileData == null)
                throw new Exception("Failed to extract file data");

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            if (fileData is ImageData imageData)
            {
                // PCX file was auto-decoded to ImageData, save as PNG
                SaveImageDataAsPng(imageData, Path.ChangeExtension(outputPath, ".png"));
            }
            else if (fileData is BinaryData binaryData)
            {
                File.WriteAllBytes(outputPath, binaryData.Bytes);
            }
        }

        /// <summary>
        /// Extracts a PCX file from the archive and saves as PNG.
        /// </summary>
        static void ExtractPcxAsPng(H3ArchiveData archive, ArchivedFileInfo fileInfo, string pngPath)
        {
            IFileData fileData = archive.ExtractFileData(fileInfo);
            if (fileData == null)
                throw new Exception("Failed to extract file data");

            Directory.CreateDirectory(Path.GetDirectoryName(pngPath));

            if (fileData is ImageData imageData)
            {
                SaveImageDataAsPng(imageData, pngPath);
            }
            else
            {
                // Fallback: save raw data
                BinaryData binaryData = (BinaryData)fileData;
                File.WriteAllBytes(Path.ChangeExtension(pngPath, ".pcx"), binaryData.Bytes);
            }
        }

        /// <summary>
        /// Expands a DEF file into a folder with PNG frames organized by group.
        /// Returns total number of frames extracted.
        /// </summary>
        static int ExpandDefFile(H3ArchiveData archive, ArchivedFileInfo fileInfo, string defFolder)
        {
            // Extract raw bytes and parse as DEF
            IFileData fileData = archive.ExtractFileData(fileInfo);
            if (fileData == null || !(fileData is BinaryData binaryData))
                return 0;

            MemoryStream defStream = new MemoryStream(binaryData.Bytes);
            H3DefFileHandler defHandler = new H3DefFileHandler(defStream);
            defHandler.LoadAllFrames();
            BundleImageDefinition bundle = defHandler.GetBundleImage();
            if (bundle == null || bundle.Groups == null)
                return 0;

            Directory.CreateDirectory(defFolder);

            int totalFrames = 0;
            for (int g = 0; g < bundle.Groups.Count; g++)
            {
                BundleImageGroup group = bundle.Groups[g];
                string groupFolder = defFolder;

                // If multiple groups, create subfolders
                if (bundle.Groups.Count > 1)
                {
                    groupFolder = Path.Combine(defFolder, string.Format("group_{0}", g));
                    Directory.CreateDirectory(groupFolder);
                }

                for (int f = 0; f < group.Frames.Count; f++)
                {
                    ImageData imageData = bundle.GetImageData(g, f);
                    if (imageData == null)
                        continue;

                    string framePath = Path.Combine(groupFolder, string.Format("frame_{0:D3}.png", f));
                    SaveImageDataAsPng(imageData, framePath);
                    totalFrames++;
                }
            }

            return totalFrames;
        }

        /// <summary>
        /// Saves an ImageData object as a PNG file using System.Drawing.
        /// </summary>
        static void SaveImageDataAsPng(ImageData imageData, string pngPath)
        {
            if (imageData == null || imageData.Width <= 0 || imageData.Height <= 0)
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(pngPath));

            byte[] pixelData = imageData.GetPlainData(0);

            unsafe
            {
                fixed (byte* ptr = pixelData)
                {
                    using (Bitmap bitmap = new Bitmap(imageData.Width, imageData.Height,
                        imageData.Width * 4, PixelFormat.Format32bppPArgb, new IntPtr(ptr)))
                    {
                        bitmap.Save(pngPath, ImageFormat.Png);
                    }
                }
            }
        }

        static bool IsTextExtension(string ext)
        {
            switch (ext)
            {
                case ".txt":
                case ".csv":
                case ".json":
                case ".xml":
                case ".xsd":
                case ".msk":
                    return true;
                default:
                    return false;
            }
        }
    }
}
