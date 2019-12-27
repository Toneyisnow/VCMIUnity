using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine;
using H3Engine.Campaign;
using H3Engine.Common;
using H3Engine.Core;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using H3Engine.Utils;
using H3Engine.Components.MapProviders;
using H3Console.Utils;

namespace H3Console
{
    class Program
    {
        static readonly string HEROES3_DATA_FOLDER = @"D:\PlayGround\Heroes3\SOD_DATA\";

        static void Main(string[] args)
        {
            TestRetrieveAllBundleImages();

            Console.WriteLine("Press Any Key...");
            Console.ReadKey();
        }

        static void TestRetrieveFile()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            BinaryData h3cFile = engine.RetrieveFileData("ab.h3c");
            StreamHelper.WriteBytesToFile(@"D:\Temp\ab.h3c", h3cFile.Bytes);

        }

        static void TestRetrieveImage()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");

            ImageData image = engine.RetrieveImage("ArA_CoBr.pcx");

            //image.ExportDataToPNG();
            //byte[] data = image.GetPNGData();
            //StreamHelper.WriteBytesToFile(@"D:\Temp\h3\Bo53Muck.png", data);

            ImageDataExporter exporter = new ImageDataExporter(image);
            exporter.ExportToPng(@"D:\Temp\h3\ArA_CoBr.png");
        }

        static void TestRetrieveAllImages()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");

            List<string> pcxFiles = engine.SearchResourceFiles("*.pcx");

            foreach (string pcxFile in pcxFiles)
            {
                ImageData image = engine.RetrieveImage(pcxFile);
                ImageDataExporter exporter = new ImageDataExporter(image);
                exporter.ExportToPng(@"D:\Temp\h3\" + pcxFile.Replace(".pcx", ".png"));
            }
        }

        static void TestRetrieveAllBundleImages()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            List<string> defFiles = engine.SearchResourceFiles("*.def");

            string exportFolderPath = @"D:\PlayGround\Heroes3\H3sprite\";
            foreach (string defFile in defFiles)
            {
                BundleImageDefinition bundleImage = engine.RetrieveBundleImage(defFile);

                string defId = defFile.Replace(".def", "");
                string defFolder = Path.Combine(exportFolderPath, defId);
                if (!Directory.Exists(defFolder))
                {
                    Directory.CreateDirectory(defFolder);
                }
                
                for(int g = 0; g< bundleImage.Groups.Count; g++)
                {
                    for(int f = 0; f < bundleImage.Groups[g].Frames.Count; f++)
                    {
                        ImageData image = bundleImage.GetImageData(g, f);
                        ImageDataExporter exporter = new ImageDataExporter(image);

                        string fileName = string.Format(@"{0}_{1}_{2}.png", defId, g, f);
                        exporter.ExportToPng(Path.Combine(defFolder, fileName));
                    }
                }
            }
        }
        
        static void TestRetrieveBundleImage()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            BundleImageDefinition bundleImage = engine.RetrieveBundleImage("lavatl.def");
            for (int g = 0; g < bundleImage.Groups.Count; g++)
            {
                for (int i = 0; i < bundleImage.Groups[g].Frames.Count; i++)
                {
                    ImageData image = bundleImage.GetImageData(g, i);
                    ImageDataExporter exporter = new ImageDataExporter(image);
                    exporter.ExportToPng(string.Format(@"D:\PlayGround\Heroes3\H3sprite\lavatl-{0}-{1}.png", g, i));
                }
            }
        }

        static void TestRetrieveBundleImage2()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            BundleImageDefinition bundleImage = engine.RetrieveBundleImage("MMENUNG.def");
            
            ImageData image = bundleImage.GetImageData(0, 0);
            ImageDataExporter exporter = new ImageDataExporter(image);
            exporter.ExportToPng(@"D:\Temp\h3\AVA0041.png");
        }

        static void TestRetrieveRiverBundleImage()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            BundleImageDefinition bundleImage = engine.RetrieveBundleImage("clrrvr.def");
            for (int g = 0; g < bundleImage.Groups.Count; g++)
            {
                for (int i = 0; i < bundleImage.Groups[g].Frames.Count; i++)
                {
                    ImageData image = bundleImage.GetImageData(g, i);
                    ImageDataExporter exporter = new ImageDataExporter(image);
                    for(byte r = 0; r < 4; r++)
                    {
                        exporter.ExportToPng(string.Format(@"D:\Temp\h3\clrrvr-{0}-{1}.png", i, r), r);
                    }
                }
            }
        }

        static void TestRetrieveMap()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            H3Map map = engine.LoadH3MapFile(@"D:\PlayGround\Heroes3\suiyi.h3m");
            foreach(CGObject obj in map.Objects)
            {
                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }

                MapPosition position = obj.Position;

                BundleImageDefinition bundleImage = engine.RetrieveBundleImage(template.AnimationFile);
                ImageData image = bundleImage.GetImageData(0, 0);
                ImageDataExporter exporter = new ImageDataExporter(image);
                exporter.ExportToPng(string.Format(@"D:\PlayGround\Heroes3\suiyi_map\obj-{0}-{1}.png", position.PosX, position.PosY));
            }
        }

        static void TestRetrieveCampaign()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
            H3Map map = H3CampaignLoader.LoadScenarioMap(campaign, 0);

            Dictionary<EObjectType, HashSet<string>> objectTypeList = new Dictionary<EObjectType, HashSet<string>>();
            HashSet<int> decorationTemplateIds = new HashSet<int>() {
                    116, 117, 118, 119, 120, 121, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137,
                    143, 147, 148, 149, 150, 151, 153, 155, 158, 161, 171, 189, 199, 206, 207, 208, 209, 210
            };

            foreach (CGObject obj in map.Objects)
            {
                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }

                EObjectType objectType = template.Type;
                if (
                    objectType == EObjectType.MONSTER
                    || objectType == EObjectType.CAMPFIRE
                    || objectType == EObjectType.LEARNING_STONE
                    || objectType == EObjectType.CREATURE_GENERATOR1
                    || objectType == EObjectType.CREATURE_GENERATOR2
                    || objectType == EObjectType.CREATURE_GENERATOR3
                    || objectType == EObjectType.CREATURE_GENERATOR4
                    )
                {
                    
                }
                else if (template.Type == EObjectType.ARTIFACT)
                {
                    
                }
                else if (decorationTemplateIds.Contains(template.Type.GetHashCode()))     // Map Decorations
                {
                    
                }
                else if (template.Type == EObjectType.HERO || template.Type == EObjectType.HERO_PLACEHOLDER)
                {
                }
                else
                {
                    if (objectTypeList.ContainsKey(objectType))
                    {
                        objectTypeList[objectType].Add(template.AnimationFile);
                    }
                    else
                    {
                        objectTypeList[objectType] = new HashSet<string>() { template.AnimationFile };
                    }
                }
            }

            foreach(KeyValuePair<EObjectType, HashSet<string>> values in objectTypeList)
            {
                Console.WriteLine("ObjectType: " + values.Key.ToString());

                foreach(string deffilename in values.Value)
                {
                    Console.WriteLine("      " + deffilename);
                }
            }


            for (int xx = 0; xx < map.Header.Width; xx++)
            {
                for (int yy = 0; yy < map.Header.Height; yy++)
                {
                    TerrainTile tile = map.TerrainTiles[0, xx, yy];
                    //// Console.WriteLine(string.Format(@"Tile [{0},{1}]: Road={2},{3}, River={4},{5}", xx, yy, tile.RoadType,tile.RoadDir, tile.RiverType, tile.RiverDir));

                    // ImageData tileImage = engine.RetrieveTerrainImage((H3Engine.Common.ETerrainType)tile.TerrainType, tile.TerrainView);
                    // StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\ab_map\tile-{0}-{1}.png", yy, xx), tileImage.GetPNGData(tile.TerrainRotation));

                    if (tile.RoadType != H3Engine.Common.ERoadType.NO_ROAD)
                    {
                        ImageData roadImage = engine.RetrieveRoadImage((H3Engine.Common.ERoadType)tile.RoadType, tile.RoadDir);
                        if (roadImage != null)
                        {
                            //// StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\ab_map\road-{0}-{1}.png", yy, xx), roadImage.GetPNGData(tile.RoadRotation));
                        }
                    }
                }
            }

        }

        static void TestRetrieveMapBlock()
        {
            using (FileStream fileStream = new FileStream(@"trace.log", FileMode.Append, FileAccess.Write))
            {
                LoggerInstance.SetConsoleLogger(new ConsoleLogger());

                Engine engine = Engine.GetInstance();
                engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
                engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
                engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
                engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

                H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");

                for (int i = 0; i < campaign.Scenarios.Count; i++)
                {
                    H3Map map = engine.RetrieveMap(campaign, i);

                    for(int l = 0; l < (map.Header.IsTwoLevel ? 2 : 1); l++)
                    {
                        MapBlockManager mapBlockManager = new MapBlockManager();
                        mapBlockManager.Initialize(map, 0);
                        mapBlockManager.PrintBlocks();
                    }
                }
            }
        }

        static void TestResourceStorage()
        {
            Engine engine = Engine.GetInstance();
            engine.SetTemporaryCachePath(@"D:\Temp\h3");

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
            // engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            LoggerInstance.SetConsoleLogger(new ConsoleLogger());
            H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
            H3Map map = engine.RetrieveMap(campaign, 0);
            
            


        }

            static void TestUnZip()
        {
            /*
            Engine engine = Engine.GetInstance();
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-0", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-1", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-2", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-3", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-4", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-5", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-6", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-7", @"D:\PlayGround\H3ab_bmp\ab");
            engine.UnZipFile(@"D:\PlayGround\H3ab_bmp\ab\out-8", @"D:\PlayGround\H3ab_bmp\ab");
            */
            // engine.UnZipFile(@"D:\PlayGround\AVLXsu07.zip", @"D:\PlayGround");

        }
        
        static void TestSearchResourceFiles()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            List<string> files = engine.SearchResourceFiles(@"AVA*.def");

        }
    }
}
