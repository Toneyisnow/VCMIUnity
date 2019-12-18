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

    private TextureSheet artifactTextureSheet = null;
    private Dictionary<int, TextureSheet> decorationTextureSheets = null;

    private Dictionary<string, TextureSheet> monsterTextureSheets = null;


    private static HashSet<int> decorationTemplateIds = new HashSet<int>()
        {
            116, 117, 118, 119, 120, 121, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137,
            143, 147, 148, 149, 150, 151, 153, 155, 158, 161, 171, 189, 199, 206, 207, 208, 209, 210
    };

    public MapTextureManager(H3Map h3map)
    {
        this.h3Engine = Engine.GetInstance();
        this.h3Map = h3map;
    }

    public static bool IsDecoration(int typeId)
    {
        return decorationTemplateIds.Contains(typeId);
    }

    public void PreloadTextures()
    {
        PreloadTerrainTextures();
        PreloadRoadTextures();
        PreloadRiverTextures();

        PreloadArtifactTextures();

        PreloadDecorationTextures();

        PreloadMonsterTextures();
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
        List<Sprite> sprites = new List<Sprite>();

        int animationIndex = 0;

        Sprite sprite = null;
        while ((sprite = artifactTextureSheet.RetrieveSprite(MapArtifactTextureSet.TextureKey(defFileName, animationIndex))) != null)
        {
            sprites.Add(sprite);
            animationIndex++;
        }

        return sprites.ToArray();
    }

    public Sprite LoadDecorationSprite(string defFileName, int decorationTypeId)
    {
        if (!decorationTextureSheets.ContainsKey(decorationTypeId))
        {
            return null;
        }

        TextureSheet textureSheet = decorationTextureSheets[decorationTypeId];
        return textureSheet.RetrieveSprite(defFileName);
    }

    public Sprite[] LoadMonsterSprites(string defFileName)
    {
        if (!monsterTextureSheets.ContainsKey(defFileName))
        {
            return null;
        }

        TextureSheet textureSheet = monsterTextureSheets[defFileName];

        List<Sprite> sprites = new List<Sprite>();

        int animationIndex = 0;

        Sprite sprite = null;
        while ((sprite = textureSheet.RetrieveSprite(animationIndex.ToString())) != null)
        {
            sprites.Add(sprite);
            animationIndex++;
        }

        return sprites.ToArray();
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

    private void PreloadArtifactTextures()
    {
        artifactTextureSheet = new TextureSheet();

        foreach (CGObject obj in h3Map.Objects)
        {
            ObjectTemplate template = obj.Template;
            if (template == null)
            {
                continue;
            }

            if (template.Type == EObjectType.ARTIFACT)
            {
                string defFileName = template.AnimationFile;
                BundleImageDefinition bundleImageDefinition = h3Engine.RetrieveBundleImage(defFileName);

                int animationIndex = 0;
                for (int group = 0; group < bundleImageDefinition.Groups.Count; group++)
                {
                    var groupObj = bundleImageDefinition.Groups[group];
                    for (int frame = 0; frame < groupObj.Frames.Count; frame++)
                    {
                        ImageData imageData = bundleImageDefinition.GetImageData(group, frame);

                        Texture2D texture = Texture2DExtension.LoadFromData(imageData);
                        string key = MapArtifactTextureSet.TextureKey(defFileName, animationIndex++);
                        artifactTextureSheet.AddImageData(key, texture);
                    }
                }
            }
        }

        artifactTextureSheet.PackTextures();
    }

    private void PreloadDecorationTextures()
    {
        decorationTextureSheets = new Dictionary<int, TextureSheet>();

        foreach (CGObject obj in h3Map.Objects)
        {
            ObjectTemplate template = obj.Template;
            if (template == null || !decorationTemplateIds.Contains(template.Type.GetHashCode()))
            {
                continue;
            }

            int decorationTypeId = template.Type.GetHashCode();
            TextureSheet textureSheet = null;
            if (decorationTextureSheets.ContainsKey(decorationTypeId))
            {
                textureSheet = decorationTextureSheets[decorationTypeId];
            }
            else
            {
                decorationTextureSheets[decorationTypeId] = new TextureSheet();
                textureSheet = decorationTextureSheets[decorationTypeId];
            }

            string defFileName = template.AnimationFile;
            BundleImageDefinition bundleImageDefinition = h3Engine.RetrieveBundleImage(defFileName);
            ImageData imageData = bundleImageDefinition.GetImageData(0, 0);

            Texture2D texture = Texture2DExtension.LoadFromData(imageData);
            textureSheet.AddImageData(defFileName, texture);
        }

        foreach(var sheet in decorationTextureSheets.Values)
        {
            sheet.PackTextures();
        }
    }

    private void PreloadMonsterTextures()
    {
        monsterTextureSheets = new Dictionary<string, TextureSheet>();

        foreach (CGObject obj in h3Map.Objects)
        {
            ObjectTemplate template = obj.Template;
            if (template == null || template.Type != EObjectType.MONSTER)
            {
                continue;
            }

            string monsterDefName = template.AnimationFile;
            TextureSheet textureSheet = null;
            if (monsterTextureSheets.ContainsKey(monsterDefName))
            {
                textureSheet = monsterTextureSheets[monsterDefName];
            }
            else
            {
                monsterTextureSheets[monsterDefName] = new TextureSheet();
                textureSheet = monsterTextureSheets[monsterDefName];
            }
            
            BundleImageDefinition bundleImageDefinition = h3Engine.RetrieveBundleImage(monsterDefName);
            ImageData[] imageData = bundleImageDefinition.GetAllImageData();
            for(int index = 0; index < imageData.Length; index++)
            {
                Texture2D texture = Texture2DExtension.LoadFromData(imageData[index]);
                textureSheet.AddImageData(index.ToString(), texture);

            }
        }

        foreach (var sheet in monsterTextureSheets.Values)
        {
            sheet.PackTextures();
        }
    }

}
