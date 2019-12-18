using H3Engine.Mapping;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Components;
using H3Engine.Common;
using H3Engine;
using H3Engine.FileSystem;

public class MapTextureManager
{
    private H3Map h3Map;

    private Engine h3Engine;

    private TextureSheet terrainTextureSheet = null;
    

    public MapTextureManager(H3Map h3map)
    {
        this.h3Engine = Engine.GetInstance();
        this.h3Map = h3map;
    }

    public void PreloadTextures()
    {
        PreloadTerrainTextures();
    }

    public Sprite LoadTerrainSprite(ETerrainType terrainType, byte terrainIndex, byte rotation)
    {
        string textureKey = string.Format(@"{0}{1}{2}", terrainType, terrainIndex, rotation);

        return terrainTextureSheet.RetrieveSprite(textureKey);
    }

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
}
