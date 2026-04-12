using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using H3Engine.DataAccess;
using H3Engine.Common;
using H3Engine.Core.Constants;
using H3Engine.Mapping;
using H3Engine.FileSystem;
using H3Engine.MapObjects;
using H3Engine.GUI;

namespace UnityClient.GUI.Rendering
{
    public class MapTextureManager
    {
        private H3Map h3Map;

        private H3DataAccess h3dataAccess;

        private TextureSheet terrainTextureSheet = null;
        private TextureSheet roadTextureSheet = null;
        private TextureSheet riverTextureSheet = null;

        private TextureSheet edgeTextureSheet = null;
        private TextureSheet cursorTextureSheet = null;

        // Index-based cursor sprite array (adag.def has 50 sprites: 0-24 reachable, 25-49 unreachable)
        private Sprite[] cursorSprites = null;

        // All map objects (artifacts, heroes, mines, resources, towns, decorations, etc.)
        // share a single atlas to maximize sprite batching and minimize draw calls.
        private BundleImageSheet mapObjectTextureSheet = null;

        // Cache for hero directional sprites: defFileName → (groupIndex → Sprite[])
        private Dictionary<string, Dictionary<int, Sprite[]>> heroGroupSpriteCache = new Dictionary<string, Dictionary<int, Sprite[]>>();



        public MapTextureManager(H3Map h3map)
        {
            this.h3dataAccess = H3DataAccess.GetInstance();
            this.h3Map = h3map;
        }

        public void PreloadTextures()
        {
            DateTime lastTime = DateTime.Now;

            PreloadTerrainTextures();
            MonoBehaviour.print("PreloadTerrainTextures:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            PreloadRoadTextures();
            MonoBehaviour.print("PreloadRoadTextures:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            PreloadRiverTextures();
            MonoBehaviour.print("PreloadRiverTextures:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            PreloadEdgeTextures();

            PreloadMapObjectTextures();
            MonoBehaviour.print("PreloadMapObjectTextures:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            PreloadCursorTextures();
        }

        /// <summary>
        /// Coroutine version of PreloadTextures that yields per-column and reports progress.
        /// Caller is responsible for setting up LoadProgress total steps before calling.
        /// Steps consumed: mapWidth * 3 (terrain/road/river columns) + 3 (pack) + 3 (edge/objects/cursor).
        /// </summary>
        public IEnumerator PreloadTexturesCoroutine(LoadProgress progress)
        {
            int mapWidth = (int)h3Map.Header.Width;
            int mapHeight = (int)h3Map.Header.Height;

            // --- Terrain ---
            progress.SetStatus("Loading terrain textures...");
            terrainTextureSheet = new TextureSheet();
            for (int xx = 0; xx < mapWidth; xx++)
            {
                for (int yy = 0; yy < mapHeight; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                    ImageData imageData = h3dataAccess.RetrieveTerrainImage(tile.TerrainType, tile.TerrainView);
                    Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.TerrainRotation);
                    string textureKey = string.Format(@"{0}{1}{2}", tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                    terrainTextureSheet.AddImageData(textureKey, texture);
                }
                progress.Step();
                yield return null;
            }
            terrainTextureSheet.PackTextures();
            progress.Step();
            yield return null;

            // --- Road ---
            progress.SetStatus("Loading road textures...");
            roadTextureSheet = new TextureSheet();
            for (int xx = 0; xx < mapWidth; xx++)
            {
                for (int yy = 0; yy < mapHeight; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                    if (tile.RoadType == ERoadType.NO_ROAD) continue;
                    ImageData imageData = h3dataAccess.RetrieveRoadImage(tile.RoadType, tile.RoadDir);
                    Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.RoadRotation);
                    string textureKey = string.Format(@"{0}{1}{2}", tile.RoadType, tile.RoadDir, tile.RoadRotation);
                    roadTextureSheet.AddImageData(textureKey, texture);
                }
                progress.Step();
                yield return null;
            }
            roadTextureSheet.PackTextures();
            progress.Step();
            yield return null;

            // --- River ---
            progress.SetStatus("Loading river textures...");
            riverTextureSheet = new TextureSheet();
            for (int xx = 0; xx < mapWidth; xx++)
            {
                for (int yy = 0; yy < mapHeight; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[0, xx, yy];
                    if (tile.RiverType == ERiverType.NO_RIVER) continue;
                    ImageData imageData = h3dataAccess.RetrieveRiverImage(tile.RiverType, tile.RiverDir);
                    Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.RiverRotation);
                    string textureKey = string.Format(@"{0}{1}{2}", tile.RiverType, tile.RiverDir, tile.RiverRotation);
                    riverTextureSheet.AddImageData(textureKey, texture);
                }
                progress.Step();
                yield return null;
            }
            riverTextureSheet.PackTextures();
            progress.Step();
            yield return null;

            // --- Edge (small, no per-item yield needed) ---
            progress.SetStatus("Loading edge textures...");
            PreloadEdgeTextures();
            progress.Step();
            yield return null;

            // --- Map Objects ---
            progress.SetStatus("Loading object textures...");
            PreloadMapObjectTextures();
            progress.Step();
            yield return null;

            // --- Cursor ---
            PreloadCursorTextures();
            progress.Step();
            yield return null;
        }

        /// <summary>
        /// Returns the number of progress steps consumed by PreloadTexturesCoroutine.
        /// </summary>
        public int GetPreloadStepCount()
        {
            int mapWidth = (int)h3Map.Header.Width;
            // terrain columns + pack + road columns + pack + river columns + pack + edge + objects + cursor
            return mapWidth * 3 + 3 + 3;
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

        public Sprite LoadEdgeSprite(string edgeKey)
        {
            return edgeTextureSheet.RetrieveSprite(edgeKey);
        }

        public Sprite[] LoadArtifactSprites(string defFileName)
        {
            return mapObjectTextureSheet.LoadSprites(defFileName);
        }

        public Sprite[] LoadMineSprites(string defFileName)
        {
            return mapObjectTextureSheet.LoadSprites(defFileName);
        }

        public Sprite[] LoadHeroSprites(string defFileName)
        {
            return mapObjectTextureSheet.LoadSprites(defFileName);
        }

        /// <summary>
        /// Load hero sprites for a specific animation group from the DEF file.
        /// Hero walking DEF structure: groups 0-4 = standing (N, NE, E, SE, S),
        /// groups 5-9 = walking (N, NE, E, SE, S).
        /// For NW/W/SW, use NE/E/SE groups with flipX=true to create pre-flipped sprites.
        /// </summary>
        public Sprite[] LoadHeroGroupSprites(string defFileName, int groupIndex, bool flipX = false)
        {
            // Use negative groupIndex in cache key for flipped versions
            int cacheKey = flipX ? -(groupIndex + 1) : groupIndex;

            if (!heroGroupSpriteCache.TryGetValue(defFileName, out var groupDict))
            {
                groupDict = new Dictionary<int, Sprite[]>();
                heroGroupSpriteCache[defFileName] = groupDict;
            }

            if (groupDict.TryGetValue(cacheKey, out Sprite[] cached))
            {
                return cached;
            }

            BundleImageDefinition bundleImage = h3dataAccess.RetrieveBundleImage(defFileName);
            if (bundleImage == null)
            {
                return null;
            }

            if (groupIndex >= bundleImage.Groups.Count)
            {
                return null;
            }

            var group = bundleImage.Groups[groupIndex];
            Sprite[] sprites = new Sprite[group.Frames.Count];
            for (int f = 0; f < group.Frames.Count; f++)
            {
                ImageData imgData = bundleImage.GetImageData(groupIndex, f);
                if (imgData == null) continue;
                Texture2D tex = Texture2DExtension.LoadFromData(imgData);
                if (tex == null) continue;

                if (flipX)
                {
                    FlipTextureHorizontally(tex);
                }

                // Always use pivot (1, 0) = bottom-right to match action tile anchor
                sprites[f] = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(1, 0),
                    Texture2DExtension.PIXELS_PER_UNIT);
            }

            groupDict[cacheKey] = sprites;
            return sprites;
        }

        public Sprite[] LoadTownSprites(string defFileName)
        {
            return mapObjectTextureSheet.LoadSprites(defFileName);
        }

        public Sprite[] LoadResourceSprites(string defFileName)
        {
            return mapObjectTextureSheet.LoadSprites(defFileName);
        }

        public Sprite LoadDecorationSprite(string defFileName, int decorationTypeId)
        {
            Sprite[] sprites = mapObjectTextureSheet.LoadSprites(defFileName);
            if (sprites == null || sprites.Length < 1)
            {
                return null;
            }

            return sprites[0];
        }

        public Sprite[] LoadSingleBundleImageSprites(string defFileName)
        {
            return mapObjectTextureSheet.LoadSprites(defFileName);
        }

        /// <summary>
        /// Retrieve a path/cursor sprite by key from the preloaded adag.def texture sheet.
        /// Keys correspond to directional path arrows (e.g. "X", "/A", ">>", "VV", etc.)
        /// </summary>
        public Sprite LoadCursorSprite(string cursorKey)
        {
            if (cursorTextureSheet == null)
            {
                return null;
            }
            return cursorTextureSheet.RetrieveSprite(cursorKey);
        }

        /// <summary>
        /// Retrieve a path/cursor sprite by index (0-49) from the preloaded adag.def sprites.
        /// Indices 0-24 are reachable arrows, 25-49 are unreachable variants.
        /// </summary>
        public Sprite LoadCursorSpriteByIndex(int index)
        {
            if (cursorSprites == null || index < 0 || index >= cursorSprites.Length)
            {
                return null;
            }
            return cursorSprites[index];
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

                    ImageData imageData = h3dataAccess.RetrieveTerrainImage(tile.TerrainType, tile.TerrainView);
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

                    ImageData imageData = h3dataAccess.RetrieveRoadImage(tile.RoadType, tile.RoadDir);
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

                    ImageData imageData = h3dataAccess.RetrieveRiverImage(tile.RiverType, tile.RiverDir);
                    Texture2D texture = Texture2DExtension.LoadFromData(imageData, tile.RiverRotation);

                    string textureKey = string.Format(@"{0}{1}{2}", tile.RiverType, tile.RiverDir, tile.RiverRotation);
                    riverTextureSheet.AddImageData(textureKey, texture);
                }
            }

            riverTextureSheet.PackTextures();
        }

        private void PreloadEdgeTextures()
        {
            edgeTextureSheet = new TextureSheet();

            BundleImageDefinition bundleImage = h3dataAccess.RetrieveBundleImage("EDG.def");

            Texture2D texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 15));
            edgeTextureSheet.AddImageData("X", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 16));
            edgeTextureSheet.AddImageData("UL", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 17));
            edgeTextureSheet.AddImageData("UR", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 18));
            edgeTextureSheet.AddImageData("DR", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 19));
            edgeTextureSheet.AddImageData("DL", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 20));
            edgeTextureSheet.AddImageData("U", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 24));
            edgeTextureSheet.AddImageData("R", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 28));
            edgeTextureSheet.AddImageData("D", texture);

            texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, 32));
            edgeTextureSheet.AddImageData("L", texture);
            
            edgeTextureSheet.PackTextures();
        }

        private void PreloadCursorTextures()
        {
            cursorTextureSheet = new TextureSheet();
            cursorSprites = new Sprite[50];

            BundleImageDefinition bundleImage = h3dataAccess.RetrieveBundleImage("adag.def");

            AddToTextureSheet(cursorTextureSheet, bundleImage, 0,  @"X");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 1,  @"/A");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 2,  @">/");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 3,  @"\>");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 4,  @"V\");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 5,  @"/V");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 6,  @"</");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 7,  @"<\");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 8,  @"A\");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 9,  @"AA");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 10, @"//A");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 11, @">>");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 12, @"\\V");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 13, @"VV");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 14, @"//V");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 15, @"<<");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 16, @"\\A");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 17, @"\A");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 18, @"A/");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 19, @"/>");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 20, @">\");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 21, @"\V");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 22, @"V/");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 23, @"/<");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 24, @"<\");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 25, @"X_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 26, @"/A_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 27, @">/_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 28, @"\>_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 29, @"V\_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 30, @"/V_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 31, @"</_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 32, @"<\_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 33, @"A\_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 34, @"AA_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 35, @"//A_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 36, @">>_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 37, @"\\V_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 38, @"VV_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 39, @"//V_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 40, @"<<_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 41, @"\\A_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 42, @"\A_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 43, @"A/_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 44, @"/>_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 45, @">\_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 46, @"\V_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 47, @"V/_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 48, @"/<_U");
            AddToTextureSheet(cursorTextureSheet, bundleImage, 49, @"<\_U");

            cursorTextureSheet.PackTextures();

            // Populate index-based sprite array directly from bundle image data.
            // Cannot rely on TextureSheet keys because some indices share the same key
            // (e.g. index 7 and 24 both map to "<\"), causing duplicates to be dropped.
            for (int i = 0; i < 50; i++)
            {
                ImageData imgData = bundleImage.GetImageData(0, i);
                if (imgData == null) continue;
                Texture2D tex = Texture2DExtension.LoadFromData(imgData);
                if (tex == null) continue;
                cursorSprites[i] = Sprite.Create(
                    tex,
                    new Rect(0, 0, tex.width, tex.height),
                    new Vector2(1, 0),
                    Texture2DExtension.PIXELS_PER_UNIT);
            }
        }

        private void AddToTextureSheet(TextureSheet textureSheet, BundleImageDefinition bundleImage, int index, string key)
        {
            Texture2D texture = Texture2DExtension.LoadFromData(bundleImage.GetImageData(0, index));
            textureSheet.AddImageData(key, texture);
        }

        private void PreloadMapObjectTextures()
        {
            ProfilerLogger.RecordProfile("PreloadMapObjectTextures: before");

            mapObjectTextureSheet = new BundleImageSheet();
            HashSet<string> loadedObjectTemplate = new HashSet<string>();

            int objectCount = 0;
            TimeSpan loadImageDataTimeSpan = new TimeSpan();
            TimeSpan buildTextureTimeSpan = new TimeSpan();

            foreach (CGObject obj in h3Map.Objects)
            {
                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }

                string defFileName = template.AnimationFile;
                if (loadedObjectTemplate.Contains(defFileName))
                {
                    continue;
                }

                mapObjectTextureSheet.AddBundleImage(defFileName, ref loadImageDataTimeSpan, ref buildTextureTimeSpan);
                objectCount++;
                loadedObjectTemplate.Add(defFileName);
            }

            ProfilerLogger.RecordProfile("PreloadMapObjectTextures: texturesheet filled:");

            MonoBehaviour.print(string.Format(@"Total map object templates: {0}", objectCount));
            MonoBehaviour.print(string.Format(@"Total time for loadImageData:" + loadImageDataTimeSpan.ToString()));
            MonoBehaviour.print(string.Format(@"Total time for buildTexture:" + buildTextureTimeSpan.ToString()));

            mapObjectTextureSheet.PackTextures();
            ProfilerLogger.RecordProfile("PreloadMapObjectTextures: texturesheet packed:");
        }

        /// <summary>
        /// Flip a Texture2D horizontally in-place (mirror left-right).
        /// </summary>
        private static void FlipTextureHorizontally(Texture2D texture)
        {
            Color32[] pixels = texture.GetPixels32();
            int width = texture.width;
            int height = texture.height;
            Color32[] flipped = new Color32[pixels.Length];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flipped[y * width + x] = pixels[y * width + (width - 1 - x)];
                }
            }

            texture.SetPixels32(flipped);
            texture.Apply();
        }
    }
}