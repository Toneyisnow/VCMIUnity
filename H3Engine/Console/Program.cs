using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Utils;

namespace H3Console
{
    class Program
    {
        static void Main(string[] args)
        {
            TestRetrieveCampaign();

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

        static void TestRetrieveAnimation()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_spr.lod");

            AnimationDefinition animation = engine.RetrieveAnimation("AVG2ele.def");
            for (int g = 0; g < animation.Groups.Count; g++)
            {
                for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
                {
                    ImageData image = animation.ComposeFrameImage(g, i);
                    byte[] imageBytes = image.GetPNGData();
                    StreamHelper.WriteBytesToFile(string.Format(@"D:\Temp\AVG2ele-{0}-{1}.png", g, i), imageBytes);
                }
            }
        }

        static void TestRetrieveCampaign()
        {
            Engine engine = Engine.GetInstance();
            engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_bmp.lod");

            engine.RetrieveCampaign("ab.h3c");


        }

        static void TestUnZip()
        {
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

            // engine.UnZipFile(@"D:\PlayGround\AVLXsu07.zip", @"D:\PlayGround");

        }

        static void TestSplitH3C()
        {
            Engine engine = Engine.GetInstance();
            engine.HandleH3CFile(@"D:\PlayGround\H3ab_bmp\ab.h3c", @"D:\PlayGround\H3ab_bmp\ab");
        }

    }
}
