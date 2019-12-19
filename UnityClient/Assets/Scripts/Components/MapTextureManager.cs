using H3Engine.Mapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Components;
using H3Engine.Common;
using H3Engine;
using H3Engine.FileSystem;
using H3Engine.MapObjects;
using H3Engine.GUI;

public class MapTextureManager
{
    private H3Map h3Map;

    private Engine h3Engine;

    private TextureSheet terrainTextureSheet = null;
    private TextureSheet roadTextureSheet = null;
    private TextureSheet riverTextureSheet = null;

    private BundleImageSheet artifactTextureSheet = null;
    private BundleImageSheet heroTextureSheet = null;
    private BundleImageSheet mineTextureSheet = null;
    private BundleImageSheet resourceTextureSheet = null;
    private BundleImageSheet townTextureSheet = null;

    private Dictionary<int, BundleImageSheet> decorationTextureSheets = null;

    private BundleImageSheet singleBundleTextureSheets = null;



    public MapTextureManager(H3Map h3map)
    {
        this.h3Engine = Engine.GetInstance();
        this.h3Map = h3map;
    }
    
    public void PreloadTextures()
    {
        PreloadTerrainTextures();
        PreloadRoadTextures();
        PreloadRiverTextures();

        PreloadMapObjectTextures();
    }

    public Sprite LoadTerrainSprite(ETerrainType terrainType, byte terrainIndex, byte rotation)
    {
        string textureKey = string.Format(@"{0}{1}{2}", terrainType, terrainIndex, rotation);

        return terrainTextureSheet.RetrieveSprite(textureKey);
    }

    public Sprite LoadRoadSprite(ERoadType roadType, byte roadDir, byte rotation)
    {
        string textureKey = string.Format(@"{0}{1}{2}", roadType, roadDir, rotation);

        return roadTextureSheet.RetrieveSprite(textureKey);
    }

    public Sprite LoadRiverSprite(ERiverType riverType, byte riverDir, byte rotation)
    {
        string textureKey = string.Format(@"{0}{1}{2}", riverType, riverDir, rotation);

        return riverTextureSheet.RetrieveSprite(textureKey);
    }

    public Sprite[] LoadArtifactSprites(string defFileName)
    {
        return artifactTextureSheet.LoadSprites(defFileName);
    }

    public Sprite[] LoadMineSprites(string defFileName)
    {
        return mineTextureSheet.LoadSprites(defFileName);
    }

    public Sprite[] LoadHeroSprites(string defFileName)
    {
        return heroTextureSheet.LoadSprites(defFileName);
    }

    public Sprite[] LoadTownSprites(string defFileName)
    {
        return townTextureSheet.LoadSprites(defFileName);
    }

    public Sprite[] LoadResourceSprites(string defFileName)
    {
        return resourceTextureSheet.LoadSprites(defFileName);
    }

    public Sprite LoadDecorationSprite(string defFileName, int decorationTypeId)
    {
        if (!decorationTextureSheets.ContainsKey(decorationTypeId))
        {
            return null;
        }

        BundleImageSheet textureSheet = decorationTextureSheets[decorationTypeId];

        Sprite[] sprites = textureSheet.LoadSprites(defFileName);
        if (sprites == null || sprites.Length < 1)
        {
            return null;
        }

        return sprites[0];
    }

    /// <summary>
    /// This is for map objects that with animation all within one DEF file
    /// </summary>
    /// <param name="defFileName"></param>
    /// <returns></returns>
    public Sprite[] LoadSingleBundleImageSprites(string defFileName)
    {
        return singleBundleTextureSheets.LoadSprites(defFileName);
    }

    //////////////////// Preload Functions ///////////////////


    /// <summary>
    /// 
    /// </summary>

    private void PreloadTerrainTextures()
    {
        terrainTextureSheet = new TextureSheet();

        for (int xx = 0; xx < h3Map.Header.Width; xx++)
        {
            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];

                ImageData imageData = h3Engine.RetrieveTerrainImage(tile.TerrainType, tile.TerrainView);
                Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.TerrainRotation);

                string textureKey = string.Format(@"{0}{1}{2}", tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                terrainTextureSheet.AddImageData(textureKey, texture);
            }
        }

        terrainTextureSheet.PackTextures();
    }

    private void PreloadRoadTextures()
    {
        roadTextureSheet = new TextureSheet();

        for (int xx = 0; xx < h3Map.Header.Width; xx++)
        {
            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                if (tile.RoadType == ERoadType.NO_ROAD)
                {
                    continue;
                }

                ImageData imageData = h3Engine.RetrieveRoadImage(tile.RoadType, tile.RoadDir);
                Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.RoadRotation);

                string textureKey = string.Format(@"{0}{1}{2}", tile.RoadType, tile.RoadDir, tile.RoadRotation);
                roadTextureSheet.AddImageData(textureKey, texture);
            }
        }

        roadTextureSheet.PackTextures();
    }

    private void PreloadRiverTextures()
    {
        riverTextureSheet = new TextureSheet();

        for (int xx = 0; xx < h3Map.Header.Width; xx++)
        {
            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                if (tile.RiverType == ERiverType.NO_RIVER)
                {
                    continue;
                }

                ImageData imageData = h3Engine.RetrieveRiverImage(tile.RiverType, tile.RiverDir);
                Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.RiverRotation);

                string textureKey = string.Format(@"{0}{1}{2}", tile.RiverType, tile.RiverDir, tile.RiverRotation);
                riverTextureSheet.AddImageData(textureKey, texture);
            }
        }

        riverTextureSheet.PackTextures();
    }

    private void PreloadMapObjectTextures()
    {
        artifactTextureSheet = new BundleImageSheet();
        mineTextureSheet = new BundleImageSheet();
        heroTextureSheet = new BundleImageSheet();
        townTextureSheet = new BundleImageSheet();
        resourceTextureSheet = new BundleImageSheet();

        decorationTextureSheets = new Dictionary<int, BundleImageSheet>();

        singleBundleTextureSheets = new BundleImageSheet();

        foreach (CGObject obj in h3Map.Objects)
        {
            ObjectTemplate template = obj.Template;
            if (template == null)
            {
                continue;
            }

            string defFileName = template.AnimationFile;

            if (MapObjectHelper.IsDecorationObject(template.Type))
            {
                int decorationTypeId = template.Type.GetHashCode();
                BundleImageSheet textureSheet = null;
                if (decorationTextureSheets.ContainsKey(decorationTypeId))
                {
                    textureSheet = decorationTextureSheets[decorationTypeId];
                }
                else
                {
                    decorationTextureSheets[decorationTypeId] = new BundleImageSheet();
                    textureSheet = decorationTextureSheets[decorationTypeId];
                }
                textureSheet.AddBundleImage(defFileName);

                continue;
            }

            switch (template.Type)
            {
                case EObjectType.ARTIFACT:
                    artifactTextureSheet.AddBundleImage(defFileName);
                    break;
                case EObjectType.HERO:
                case EObjectType.HERO_PLACEHOLDER:
                case EObjectType.RANDOM_HERO:
                    heroTextureSheet.AddBundleImage(defFileName);
                    break;
                case EObjectType.MINE:
                    mineTextureSheet.AddBundleImage(defFileName);
                    break;
                case EObjectType.TOWN:
                case EObjectType.RANDOM_TOWN:
                    townTextureSheet.AddBundleImage(defFileName);
                    break;
                case EObjectType.RESOURCE:
                case EObjectType.RANDOM_RESOURCE:
                    resourceTextureSheet.AddBundleImage(defFileName);
                    break;
                default:
                    // For other types, just put them into Single Bundle Image Sheet
                    singleBundleTextureSheets.AddBundleImage(defFileName);
                    break;
            }
        }

        artifactTextureSheet.PackTextures();
        mineTextureSheet.PackTextures();
        heroTextureSheet.PackTextures();
        townTextureSheet.PackTextures();
        resourceTextureSheet.PackTextures();

        foreach (var sheet in decorationTextureSheets.Values)
        {
            sheet.PackTextures();
        }

        singleBundleTextureSheets.PackTextures();
    }
}
