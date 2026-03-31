using H3Engine.Common;
using H3Engine.Components.Data;
using H3Engine.Core;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections;
using UnityClient.GUI.Rendering;
using UnityEngine;

namespace UnityClient.GUI.Mapping
{
    /// <summary>
    /// Plain C# helper that loads and renders all map tiles and objects into the scene.
    /// Instantiated by MapComponent during initialization; calls back into MapComponent
    /// to create GameObjects and populate hero tracking data.
    /// </summary>
    public class MapTileRenderer
    {
        private readonly MapComponent mapComponent;
        private readonly GameMap gameMap;
        private readonly MapTextureManager mapTextureManager;

        public MapTileRenderer(MapComponent mapComponent, GameMap gameMap, MapTextureManager mapTextureManager)
        {
            this.mapComponent = mapComponent;
            this.gameMap = gameMap;
            this.mapTextureManager = mapTextureManager;
        }

        public void RenderMap()
        {
            var lastTime = DateTime.Now;

            mapTextureManager.PreloadTextures();
            Debug.Log("PreloadTextures:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderTerrain();
            Debug.Log("RenderTerrain:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderRoad();
            Debug.Log("RenderRoad:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderRiver();
            Debug.Log("RenderRiver:" + (DateTime.Now - lastTime).ToString());
            lastTime = DateTime.Now;

            RenderMapObjects();
            Debug.Log("RenderMapObjects:" + (DateTime.Now - lastTime).ToString());

            RenderEdge();
        }

        public IEnumerator RenderMapCoroutine(LoadProgress progress)
        {
            int mapHeight = gameMap.Height;

            int preloadSteps = mapTextureManager.GetPreloadStepCount();
            int renderSteps = mapHeight * 3 + 2;
            progress.SetupSteps(preloadSteps + renderSteps);

            yield return mapTextureManager.PreloadTexturesCoroutine(progress);

            progress.SetStatus("Rendering terrain...");
            for (int yy = 0; yy < mapHeight; yy++)
            {
                for (int xx = 0; xx < gameMap.Width; xx++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                    GameObject gametile = mapComponent.CreateSubChildObject("Terrain", MapComponent.GetMapPosition(xx, yy), sprite, MapComponent.SortOrder_Terrain);
                    MapTile mapTileComponent = gametile.AddComponent<MapTile>();
                    mapTileComponent.Initialize(xx, yy);
                }
                progress.Step();
                yield return null;
            }

            progress.SetStatus("Rendering roads...");
            for (int yy = 0; yy < mapHeight; yy++)
            {
                for (int xx = 0; xx < gameMap.Width; xx++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
                    mapComponent.CreateSubChildObject("Road", MapComponent.GetMapPosition(xx, yy), sprite, MapComponent.SortOrder_Road);
                }
                progress.Step();
                yield return null;
            }

            progress.SetStatus("Rendering rivers...");
            for (int yy = 0; yy < mapHeight; yy++)
            {
                for (int xx = 0; xx < gameMap.Width; xx++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);
                    mapComponent.CreateSubChildObject("River", MapComponent.GetMapPosition(xx, yy), sprite, MapComponent.SortOrder_River);
                }
                progress.Step();
                yield return null;
            }

            progress.SetStatus("Rendering map objects...");
            RenderMapObjects();
            progress.Step();
            yield return null;

            progress.SetStatus("Rendering map edges...");
            RenderEdge();
            progress.Step();

            progress.Finish();
        }

        private void RenderTerrain()
        {
            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                for (int yy = 0; yy < gameMap.Height; yy++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadTerrainSprite(tile.TerrainType, tile.TerrainView, tile.TerrainRotation);
                    GameObject gametile = mapComponent.CreateSubChildObject("Terrain", MapComponent.GetMapPosition(xx, yy), sprite, MapComponent.SortOrder_Terrain);
                    MapTile mapTileComponent = gametile.AddComponent<MapTile>();
                    mapTileComponent.Initialize(xx, yy);
                }
            }
        }

        private void RenderRoad()
        {
            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                for (int yy = 0; yy < gameMap.Height; yy++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadRoadSprite(tile.RoadType, tile.RoadDir, tile.RoadRotation);
                    mapComponent.CreateSubChildObject("Road", MapComponent.GetMapPosition(xx, yy), sprite, MapComponent.SortOrder_Road);
                }
            }
        }

        private void RenderRiver()
        {
            for (int xx = 0; xx < gameMap.Width; xx++)
            {
                for (int yy = 0; yy < gameMap.Height; yy++)
                {
                    TerrainTile tile = gameMap.TerrainTiles[xx, yy];
                    Sprite sprite = mapTextureManager.LoadRiverSprite(tile.RiverType, tile.RiverDir, tile.RiverRotation);
                    mapComponent.CreateSubChildObject("River", MapComponent.GetMapPosition(xx, yy), sprite, MapComponent.SortOrder_River);
                }
            }
        }

        private void RenderMapObjects()
        {
            foreach (CGObject obj in gameMap.Objects)
            {
                ObjectTemplate template = obj.Template;
                MapPosition position = obj.Position;

                if (template.Type == EObjectType.ARTIFACT)
                {
                    Sprite[] sprites = mapTextureManager.LoadArtifactSprites(template.AnimationFile);
                    mapComponent.CreateSubChildAnimatedObject("Artifacts", MapComponent.GetMapPosition(position.PosX, position.PosY), sprites, MapComponent.SortOrder_Artifact);
                }
                else if (MapObjectHelper.IsDecorationObject(template.Type))
                {
                    Sprite sprite = mapTextureManager.LoadDecorationSprite(template.AnimationFile, template.Type.GetHashCode());
                    mapComponent.CreateSubChildObject("Decorations", MapComponent.GetMapPosition(position.PosX, position.PosY), sprite, MapComponent.SortOrder_Decoration);
                }
                else if (template.Type == EObjectType.MINE)
                {
                    Sprite[] sprites = mapTextureManager.LoadMineSprites(template.AnimationFile);
                    mapComponent.CreateSubChildAnimatedObject("Mines", MapComponent.GetMapPosition(position.PosX, position.PosY), sprites, MapComponent.SortOrder_Mine, "Mine_" + template.SubId);
                }
                else if (template.Type == EObjectType.RESOURCE)
                {
                    Sprite[] sprites = mapTextureManager.LoadResourceSprites(template.AnimationFile);
                    mapComponent.CreateSubChildAnimatedObject("Resources", MapComponent.GetMapPosition(position.PosX, position.PosY), sprites, MapComponent.SortOrder_Resource, "Resource_" + template.SubId);
                }
                else if (template.Type == EObjectType.TOWN || template.Type == EObjectType.RANDOM_TOWN)
                {
                    Sprite[] sprites = mapTextureManager.LoadTownSprites(template.AnimationFile);
                    mapComponent.CreateSubChildAnimatedObject("Town", MapComponent.GetMapPosition(position.PosX, position.PosY), sprites, MapComponent.SortOrder_Town, "Town_" + template.SubId);
                }
                else if (template.Type == EObjectType.HERO || template.Type == EObjectType.HERO_PLACEHOLDER
                    || template.Type == EObjectType.RANDOM_HERO || obj is HeroInstance)
                {
                    Sprite[] sprites = mapTextureManager.LoadHeroSprites(template.AnimationFile);
                    GameObject heroGO = mapComponent.CreateSubChildAnimatedObject("Heroes", MapComponent.GetMapPosition(position.PosX + 1, position.PosY), sprites, MapComponent.SortOrder_Hero, "Hero_" + obj.Identifier);

                    mapComponent.heroGameObjects[obj.Identifier] = heroGO;
                    mapComponent.heroOriginalSprites[obj.Identifier] = sprites;

                    string walkDefFile = template.AnimationFile;
                    if (walkDefFile.Contains("_e.def"))
                        walkDefFile = walkDefFile.Replace("_e.def", "_.def");
                    mapComponent.heroDefFileNames[obj.Identifier] = walkDefFile;

                    Debug.Log(string.Format("[MapTileRenderer] Hero {0}: templateType={1}, templateDef={2}, walkDef={3}",
                        obj.Identifier, template.Type, template.AnimationFile, walkDefFile));
                }
                else
                {
                    Sprite[] sprites = mapTextureManager.LoadSingleBundleImageSprites(template.AnimationFile);
                    mapComponent.CreateSubChildAnimatedObject("AnimatedObjects", MapComponent.GetMapPosition(position.PosX, position.PosY), sprites, MapComponent.SortOrder_Building,
                        string.Format("{0}_{1}", template.Type.ToString(), template.SubId));
                }
            }
        }

        private void RenderEdge()
        {
            int mapHeight = gameMap.Height;
            int mapWidth = gameMap.Width;

            Sprite spaceSprite = mapTextureManager.LoadEdgeSprite("X");

            for (int xx = 0; xx < mapWidth; xx++)
            {
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(xx, -1), mapTextureManager.LoadEdgeSprite("U"), MapComponent.SortOrder_Edge);
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(xx, -2), spaceSprite, MapComponent.SortOrder_Edge);
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(xx, mapHeight), mapTextureManager.LoadEdgeSprite("D"), MapComponent.SortOrder_Edge);
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(xx, mapHeight + 1), spaceSprite, MapComponent.SortOrder_Edge);
            }

            for (int yy = 0; yy < mapHeight; yy++)
            {
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-1, yy), mapTextureManager.LoadEdgeSprite("L"), MapComponent.SortOrder_Edge);
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-2, yy), spaceSprite, MapComponent.SortOrder_Edge);
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth, yy), mapTextureManager.LoadEdgeSprite("R"), MapComponent.SortOrder_Edge);
                mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth + 1, yy), spaceSprite, MapComponent.SortOrder_Edge);
            }

            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-1, -1), mapTextureManager.LoadEdgeSprite("UL"), MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-2, -1), spaceSprite, MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-1, -2), spaceSprite, MapComponent.SortOrder_Edge);

            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth, -1), mapTextureManager.LoadEdgeSprite("UR"), MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth, -2), spaceSprite, MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth + 1, -1), spaceSprite, MapComponent.SortOrder_Edge);

            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-1, mapHeight), mapTextureManager.LoadEdgeSprite("DL"), MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-2, mapHeight), spaceSprite, MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(-1, mapHeight + 1), spaceSprite, MapComponent.SortOrder_Edge);

            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth, mapHeight), mapTextureManager.LoadEdgeSprite("DR"), MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth + 1, mapHeight), spaceSprite, MapComponent.SortOrder_Edge);
            mapComponent.CreateSubChildObject("Edge", MapComponent.GetMapPosition(mapWidth, mapHeight + 1), spaceSprite, MapComponent.SortOrder_Edge);
        }
    }
}
