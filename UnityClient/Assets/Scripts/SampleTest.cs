using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.Utils;
using H3Engine.Mapping;
using H3Engine.Campaign;

public class SampleTest : MonoBehaviour
{
    private List<Sprite> mainSprites;

    private int spriteIndex = 0;
    private int frameCount = 0;

    private bool sceneLoaded = false;

    private GameObject gObject = null;

    // Start is called before the first frame update
    void Start()
    {
        mainSprites = new List<Sprite>();

        // LoadImage();

        // LoadAnimation();

        LoadTerrain();

        sceneLoaded = true;
        frameCount = 0;
    }

    void LoadImage()
    {
        Engine h3Engine = Engine.GetInstance();
        h3Engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_bmp.lod");
        ImageData image = h3Engine.RetrieveImage("Bo53Muck.pcx");
        
        Sprite sprite = CreateSpriteFromBytes(image.GetPNGData());

        GameObject go = new GameObject("SampleSprite");
        /// go.transform.position = new Vector3(10, 20, 0);

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
    }

    void LoadAnimation()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(@"D:\Toney\Personal\Git\toneyisnow\HeroesIII\External\HeroesIII_Data\H3ab_spr.lod");

        
        BundleImageDefinition animation = engine.RetrieveBundleImage("AVG2ele.def");
        for (int g = 0; g < animation.Groups.Count; g++)
        {
            for (int i = 0; i < animation.Groups[g].Frames.Count; i++)
            {
                ImageData image = animation.GetImageData(g, i);
                Sprite sprite = CreateSpriteFromBytes(image.GetPNGData());
                mainSprites.Add(sprite);
            }
        }

        gObject = new GameObject("SampleSprite2");
        /// gObject.transform.position = new Vector3(50, 50, 0);

        SpriteRenderer renderer = gObject.AddComponent<SpriteRenderer>();
        renderer.sprite = mainSprites[0];
    }

    void LoadTerrain()
    {
        Engine engine = Engine.GetInstance();
        engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3ab_bmp.lod");
        engine.LoadArchiveFile(@"D:\PlayGround\SOD_Data\H3sprite.lod");

        H3Campaign campaign = engine.RetrieveCampaign("ab.h3c");

        H3Map map1 = campaign.Scenarios[0].MapData;
        TerrainTile tile = map1.TerrainTiles[0, 3, 4];
        //// Console.WriteLine(string.Format(@"Tile [{0},{1}]: Road={2},{3}, River={4},{5}", xx, yy, tile.RoadType,tile.RoadDir, tile.RiverType, tile.RiverDir));

        ImageData tileImage = engine.RetrieveTerrainImage((H3Engine.Common.ETerrainType)tile.TerrainType, tile.TerrainView);
        Sprite sprite = CreateSpriteFromBytes(tileImage.GetPNGData());

        GameObject g = new GameObject(string.Format(@"TerrainObject-{0}-{1}", 3, 4));
        g.transform.position = new Vector3(3 * 32, 4 * 32, 0);

        SpriteRenderer renderer = gObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        /*
        for (int xx = 0; xx < map1.Header.Width; xx++)
        {
            for (int yy = 0; yy < map1.Header.Height; yy++)
            {
                TerrainTile tile = map1.TerrainTiles[0, xx, yy];
                //// Console.WriteLine(string.Format(@"Tile [{0},{1}]: Road={2},{3}, River={4},{5}", xx, yy, tile.RoadType,tile.RoadDir, tile.RiverType, tile.RiverDir));

                ImageData tileImage = engine.RetrieveTerrainImage((H3Engine.Common.ETerrainType)tile.TerrainType, tile.TerrainView);
                Sprite sprite = CreateSpriteFromBytes(tileImage.GetPNGData());

                gObject = new GameObject(string.Format(@"TerrainObject-{0}-{1}", xx, yy));
                gObject.transform.position = new Vector3(xx * 32, yy * 32, 0);

                SpriteRenderer renderer = gObject.AddComponent<SpriteRenderer>();
                renderer.sprite = sprite;

                //StreamHelper.WriteBytesToFile(string.Format(@"D:\PlayGround\tiles\tile-{0}-{1}.png", xx, yy), tileImage.GetPNGData());

                /*
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
                * /
            }
        }
        */
    }



    private Sprite CreateSpriteFromBytes(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.HDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
        texture.LoadImage(imageBytes);

        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        
        return sprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!sceneLoaded || mainSprites.Count == 0)
        {
            return;
        }

        if (frameCount ++ > 6)
        {
            frameCount = 0;

            SpriteRenderer renderer = gObject.GetComponent<SpriteRenderer>();
            spriteIndex = (spriteIndex + 1) % mainSprites.Count;
            renderer.sprite = mainSprites[spriteIndex];
        }
    }
}
