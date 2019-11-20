using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine;
using H3Engine.Campaign;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Mapping;
using H3Engine.Utils;

namespace H3Console
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRetrieveRiverBundleImage();

            Console.WriteLine("Press Any Key...");
            Console.ReadKey();
        }

        static void TestRetrieveFile()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3ab_bmp.lod");
            byte[] h3cFile = engine.RetrieveFileData("ab.h3c");
            StreamHelper.WriteBytesToFile(@"D:\Temp\ab.h3c", h3cFile);

        }

        static void TestRetrieveImage()
        {
            Engine engine = Engine.GetInstance();

            engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_bmp.lod");
            ImageData image = engine.RetrieveImage("Bo53Muck.pcx");

            byte[] imageBytes = image.GetPNGData();
            StreamHelper.WriteBytesToFile(@"D:\Temp\Bo53Muck.png", imageBytes);
        }

        static void TestRetrieveBundleImage()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_spr.lod");

            BundleImageDefinition bundleImage = engine.RetrieveBundleImage("AVG2ele.def");
            for (int g = 0; g < bundleImage.Groups.Count; g++)
            {
                for (int i = 0; i < bundleImage.Groups[g].Frames.Count; i++)
                {
                    ImageData image = bundleImage.GetImageData(g, i);
                    byte[] imageBytes = image.GetPNGData();
                    StreamHelper.WriteBytesToFile(string.Format(@"D:\Temp\AVG2ele-{0}-{1}.png", g, i), imageBytes);
                }
            }
        }

        static void TestRetrieveRiverBundleImage()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3sprite.lod");

            BundleImageDefinition bundleImage = engine.RetrieveBundleImage("dirtrd.def");
            for (int g = 0; g < bundleImage.Groups.Count; g++)
            {
                for (int i = 0; i < bundleImage.Groups[g].Frames.Count; i++)
                {
                    ImageData image = bundleImage.GetImageData(g, i);
                    byte[] imageBytes = image.GetPNGData();
                    StreamHelper.WriteBytesToFile(string.Format(@"D:\Temp\dirtrd-{0}-{1}.png", g, i), imageBytes);
                }
            }
        }

        static void TestRetrieveCampaign()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3ab_bmp.lod");
            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3ab_spr.lod");
            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3bitmap.lod");
            engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3sprite.lod");

            H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");

            H3Map map1 = campaign.Scenarios[0].MapData;

            for (int xx = 0; xx < map1.Header.Width; xx++)
            {
                for (int yy = 0; yy < map1.Header.Height; yy++)
                {
                    TerrainTile tile = map1.TerrainTiles[0, xx, yy];
                    //// Console.WriteLine(string.Format(@"Tile [{0},{1}]: Road={2},{3}, River={4},{5}", xx, yy, tile.RoadType,tile.RoadDir, tile.RiverType, tile.RiverDir));

                    //ImageData tileImage = engine.RetrieveTerrainImage((H3Engine.Common.ETerrainType)tile.TerrainType, tile.TerrainView);
                    //StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\tiles\tile-{0}-{1}.png", xx, yy), tileImage.GetPNGData());

                    if ((H3Engine.Common.ERoadType)tile.RoadType != H3Engine.Common.ERoadType.NO_ROAD)
                    {
                        ImageData roadImage = engine.RetrieveRoadImage((H3Engine.Common.ERoadType)tile.RoadType, tile.RoadDir);
                        if (roadImage != null)
                        {
                            StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\roads\road-{0}-{1}.png", xx, yy), roadImage.GetPNGData());
                        }
                    }

                    if ((H3Engine.Common.ERiverType)tile.RiverType != H3Engine.Common.ERiverType.NO_RIVER)
                    {
                        ImageData riverImage = engine.RetrieveRiverImage((H3Engine.Common.ERiverType)tile.RiverType, tile.RiverDir);
                        if (riverImage != null)
                        {
                            StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\rivers\river-{0}-{1}.png", xx, yy), riverImage.GetPNGData());
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
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile.png", tileImage.GetPNGData());

            ImageData tileImage2 = engine.RetrieveTerrainImage(H3Engine.Common.ETerrainType.SAND, 13);
            StreamHelper.WriteBytesToFile(@"D:\PlayGround\tile2.png", tileImage2.GetPNGData());

        }
    }
}
