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
            TestRetrieveAnimation();

            Console.WriteLine("Press Any Key...");
            Console.ReadKey();
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
    }
}
