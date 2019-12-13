using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine;
using H3Engine.Campaign;
using H3Engine.Core;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using H3Engine.Utils;

namespace H3Console
{
    class Program
    {
        static readonly string HEROES3_DATA_FOLDER = @"D:\PlayGround\Heroes3\SOD_DATA\";

        static void Main(string[] args)
        {
            TestRetrieveImage();

            Console.WriteLine("Press Any Key...");
            Console.ReadKey();
        }

        static void TestRetrieveFile()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            byte[] h3cFile = engine.RetrieveFileData("ab.h3c");
            StreamHelper.WriteBytesToFile(@"D:\Temp\ab.h3c", h3cFile);

        }

        static void TestRetrieveImage()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            ImageData image = engine.RetrieveImage("Bo53Muck.pcx");
            image.ExportDataToPNG();

            byte[] imageBytes = image.GetPNGData();
            StreamHelper.WriteBytesToFile(@"D:\Temp\h3\Bo53Muck.png", imageBytes);
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
                    image.ExportDataToPNG();

                    byte[] imageBytes = image.GetPNGData();
                    StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\H3sprite\lavatl-{0}-{1}.png", g, i), imageBytes);
                }
            }
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
                    image.ExportDataToPNG(true);

                    for(byte r = 0; r < 4; r++)
                    {
                        byte[] imageBytes = image.GetPNGData(r);
                        StreamHelper.WriteBytesToFile(string.Format(@"D:\Temp\h3\clrrvr-{0}-{1}.png", i, r), imageBytes);
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
                image.ExportDataToPNG();

                StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\suiyi_map\obj-{0}-{1}.png", position.PosX, position.PosY), image.GetPNGData());
            }

            /*
            for (int yy = 0; yy < map.Header.Height; yy++)
            {
                for (int xx = 0; xx < map.Header.Width; xx++)
                {
                    TerrainTile tile = map.TerrainTiles[0, xx, yy];
                    Console.WriteLine(string.Format(@"Tile [{0},{1}]: Terrain={2},{3},{8} Road={4},{5} River={6},{7}", 
                            xx, yy, tile.TerrainType, tile.TerrainView, tile.RoadType,tile.RoadDir, tile.RiverType, tile.RiverDir,
                            tile.TerrainRotation));

                    ImageData tileImage = engine.RetrieveRoadImage((H3Engine.Common.ERoadType)tile.RoadType, tile.RoadDir);
                    if (tileImage != null)
                    {
                        StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\suiyi_map\road-{0}-{1}.png", yy, xx), tileImage.GetPNGData(tile.RoadRotation));
                    }
                }
            }*/

        }

        static void TestRetrieveCampaign()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_bmp.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3ab_spr.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3bitmap.lod");
            engine.LoadArchiveFile(HEROES3_DATA_FOLDER + "H3sprite.lod");

            H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");
            H3Map map1 = H3CampaignLoader.LoadScenarioMap(campaign, 0);

            for (int xx = 0; xx < map1.Header.Width; xx++)
            {
                for (int yy = 0; yy < map1.Header.Height; yy++)
                {
                    TerrainTile tile = map1.TerrainTiles[0, xx, yy];
                    Console.WriteLine(string.Format(@"Tile [{0},{1}]: Road={2},{3}, River={4},{5}", xx, yy, tile.RoadType,tile.RoadDir, tile.RiverType, tile.RiverDir));

                    // ImageData tileImage = engine.RetrieveTerrainImage((H3Engine.Common.ETerrainType)tile.TerrainType, tile.TerrainView);
                    // StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\ab_map\tile-{0}-{1}.png", yy, xx), tileImage.GetPNGData(tile.TerrainRotation));

                    if (tile.RoadType != H3Engine.Common.ERoadType.NO_ROAD)
                    {
                        ImageData roadImage = engine.RetrieveRoadImage((H3Engine.Common.ERoadType)tile.RoadType, tile.RoadDir);
                        if (roadImage != null)
                        {
                            StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\Heroes3\ab_map\road-{0}-{1}.png", yy, xx), roadImage.GetPNGData(tile.RoadRotation));
                        }
                    }
                }
            }

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

        static void TestRetrieveTerrainImage()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3sprite.lod");

            ImageData tileImage = engine.RetrieveTerrainImage(H3Engine.Common.ETerrainType.SAND, 2);
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile-0.png", tileImage.GetPNGData(2));

            ImageData tileImage2 = engine.RetrieveTerrainImage(H3Engine.Common.ETerrainType.WATER, 3);
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile2-0.png", tileImage2.GetPNGData(0));
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile2-1.png", tileImage2.GetPNGData(1));
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile2-2.png", tileImage2.GetPNGData(2));
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile2-3.png", tileImage2.GetPNGData(3));

        }
    }
}
