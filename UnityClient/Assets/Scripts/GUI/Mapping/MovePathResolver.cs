using H3Engine.Engine.PathFinder;
using H3Engine.GUI;
using H3Engine.MapObjects;
using System.Collections.Generic;
using UnityClient.GUI.Rendering;
using UnityEngine;

namespace UnityClient.GUI.Mapping
{
    /// <summary>
    /// Handles hero movement path arrows and hero walking/idle animations on the map.
    /// </summary>
    public class MovePathResolver
    {
        private readonly MapComponent mapComponent;
        private readonly MapTextureManager mapTextureManager;
        private readonly Dictionary<uint, GameObject> heroGameObjects;
        private readonly Dictionary<uint, string> heroDefFileNames;

        private readonly List<GameObject> pathArrowObjects = new List<GameObject>();

        /// <summary>
        /// Offset to convert reachable (green) arrow index to unreachable (orange) variant.
        /// adag.def: indices 0-24 = reachable, 25-49 = unreachable (same shapes).
        /// </summary>
        private const int UNREACHABLE_ARROW_OFFSET = 25;

        /// <summary>
        /// VCMI's 9x9 direction-to-arrow-index lookup table from MapRenderer.cpp.
        /// Rows = enter direction (from previous tile), Columns = leave direction (to next tile).
        /// Direction index: (dx+1) + 3*(dy+1) where dx/dy are -1, 0, or 1.
        /// 0=NW, 1=N, 2=NE, 3=W, 4=center, 5=E, 6=SW, 7=S, 8=SE
        /// </summary>
        private static readonly int[,] DirectionToArrowIndex = new int[,]
        {
            {16, 17, 18, 7,  0, 19, 6,  5,  12},
            {8,  9,  18, 7,  0, 19, 6,  13, 20},
            {8,  1,  10, 7,  0, 19, 14, 21, 20},
            {24, 17, 18, 15, 0, 11, 6,  5,  4 },
            {0,  0,  0,  0,  0, 0,  0,  0,  0 },
            {8,  1,  2,  15, 0, 11, 22, 21, 20},
            {24, 17, 10, 23, 0, 3,  14, 5,  4 },
            {24, 9,  2,  23, 0, 3,  22, 13, 4 },
            {16, 1,  2,  23, 0, 3,  22, 21, 12},
        };

        public MovePathResolver(MapComponent mapComponent, MapTextureManager mapTextureManager,
            Dictionary<uint, GameObject> heroGameObjects, Dictionary<uint, string> heroDefFileNames)
        {
            this.mapComponent = mapComponent;
            this.mapTextureManager = mapTextureManager;
            this.heroGameObjects = heroGameObjects;
            this.heroDefFileNames = heroDefFileNames;
        }

        /// <summary>
        /// Convert hero tile coordinates to world position.
        /// Hero sprites are wider than 1 tile, so X is shifted +1 to align the
        /// visible character with the correct terrain tile.
        /// </summary>
        public Vector3 HeroTileToWorldPosition(int tileX, int tileY)
        {
            return MapComponent.GetMapPosition(tileX + 1, tileY);
        }

        /// <summary>
        /// Display path arrows on the map for a list of tile positions.
        /// All arrows are shown in green (reachable) style. Use the MapPathNode overload
        /// for green/orange differentiation based on turn reachability.
        /// </summary>
        public void ShowPath(List<Vector2Int> path)
        {
            ClearPath();

            if (path == null || path.Count < 2) return;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2Int current = path[i];
                bool isDestination = (i == path.Count - 1);

                Sprite arrowSprite;
                if (isDestination)
                {
                    arrowSprite = mapTextureManager.LoadCursorSpriteByIndex(0);
                }
                else
                {
                    Vector2Int prev = path[i - 1];
                    int enterDir = GetDirectionIndex(prev, current);
                    Vector2Int next = path[i + 1];
                    int leaveDir = GetDirectionIndex(current, next);

                    int arrowIndex = DirectionToArrowIndex[enterDir, leaveDir];
                    arrowSprite = mapTextureManager.LoadCursorSpriteByIndex(arrowIndex);
                }

                if (arrowSprite != null)
                {
                    GameObject arrow = mapComponent.CreateSubChildObject("PathArrows", MapComponent.GetMapPosition(current.x, current.y), arrowSprite, MapComponent.SortOrder_PathArrow, "PathArrow");
                    pathArrowObjects.Add(arrow);
                }
            }
        }

        /// <summary>
        /// Display path arrows for a list of MapPathNodes.
        /// Nodes with Turns == 0 are reachable this turn (green arrows, indices 0-24).
        /// Nodes with Turns > 0 are unreachable this turn (orange arrows, indices 25-49).
        /// </summary>
        public void ShowPath(List<MapPathNode> path)
        {
            ClearPath();

            if (path == null || path.Count < 2) return;

            for (int i = 1; i < path.Count; i++)
            {
                MapPathNode currentNode = path[i];
                Vector2Int current = new Vector2Int(currentNode.Position.PosX, currentNode.Position.PosY);
                bool isDestination = (i == path.Count - 1);
                bool isReachableThisTurn = (currentNode.Turns == 0);

                Sprite arrowSprite;
                if (isDestination)
                {
                    // Destination marker: green "X" (index 0) or orange "X" (index 25)
                    int destIndex = isReachableThisTurn ? 0 : UNREACHABLE_ARROW_OFFSET;
                    arrowSprite = mapTextureManager.LoadCursorSpriteByIndex(destIndex);
                }
                else
                {
                    Vector2Int prev = new Vector2Int(path[i - 1].Position.PosX, path[i - 1].Position.PosY);
                    int enterDir = GetDirectionIndex(prev, current);
                    Vector2Int next = new Vector2Int(path[i + 1].Position.PosX, path[i + 1].Position.PosY);
                    int leaveDir = GetDirectionIndex(current, next);

                    int arrowIndex = DirectionToArrowIndex[enterDir, leaveDir];
                    if (!isReachableThisTurn)
                    {
                        arrowIndex += UNREACHABLE_ARROW_OFFSET;
                    }
                    arrowSprite = mapTextureManager.LoadCursorSpriteByIndex(arrowIndex);
                }

                if (arrowSprite != null)
                {
                    GameObject arrow = mapComponent.CreateSubChildObject("PathArrows", MapComponent.GetMapPosition(current.x, current.y), arrowSprite, MapComponent.SortOrder_PathArrow, "PathArrow");
                    pathArrowObjects.Add(arrow);
                }
            }
        }

        /// <summary>
        /// Remove all path arrow objects from the map.
        /// </summary>
        public void ClearPath()
        {
            foreach (GameObject arrow in pathArrowObjects)
            {
                if (arrow != null) Object.Destroy(arrow);
            }
            pathArrowObjects.Clear();
        }

        /// <summary>
        /// Remove a single path arrow by its index in the pathArrowObjects list.
        /// Used during movement to remove arrows one by one as the hero enters each tile.
        /// </summary>
        public void RemovePathArrowAtIndex(int index)
        {
            if (index < 0 || index >= pathArrowObjects.Count) return;
            GameObject arrow = pathArrowObjects[index];
            if (arrow != null) Object.Destroy(arrow);
            pathArrowObjects[index] = null;
        }

        /// <summary>
        /// Remove the first 'count' arrows from the list (already destroyed ones are null).
        /// Used after pause to keep only the remaining arrows in sync with the trimmed path.
        /// </summary>
        public void TrimPathArrowsFromStart(int count)
        {
            if (count <= 0) return;
            int removeCount = Mathf.Min(count, pathArrowObjects.Count);
            for (int i = 0; i < removeCount; i++)
            {
                if (pathArrowObjects[i] != null) Object.Destroy(pathArrowObjects[i]);
            }
            pathArrowObjects.RemoveRange(0, removeCount);
        }

        /// <summary>
        /// Set the hero's animation to the walking sprites for the given movement direction.
        /// dx, dy: tile movement direction (-1, 0, or 1).
        /// </summary>
        public void SetHeroMovingAnimation(HeroInstance hero, int dx, int dy)
        {
            if (hero == null) return;
            if (!heroGameObjects.TryGetValue(hero.Identifier, out GameObject heroGO)) return;
            if (!heroDefFileNames.TryGetValue(hero.Identifier, out string defFileName)) return;

            var (group, flipX) = GetHeroMoveGroup(dx, dy);
            Sprite[] groupSprites = mapTextureManager.LoadHeroGroupSprites(defFileName, group, flipX);

            AnimatedMapObject animated = heroGO.GetComponent<AnimatedMapObject>();
            if (animated != null && groupSprites != null && groupSprites.Length > 0)
            {
                animated.SetSprites(groupSprites, 8);
            }
        }

        /// <summary>
        /// Set hero to idle/standing animation facing the given movement direction.
        /// Standing groups 0-4 correspond to walking groups 5-9 (N, NE, E, SE, S).
        /// </summary>
        public void SetHeroIdleAnimation(HeroInstance hero, int dx, int dy)
        {
            if (hero == null) return;
            if (!heroGameObjects.TryGetValue(hero.Identifier, out GameObject heroGO)) return;
            if (!heroDefFileNames.TryGetValue(hero.Identifier, out string defFileName)) return;

            // Standing group = walking group - 5 (groups 0-4 = standing N, NE, E, SE, S)
            var (walkGroup, flipX) = GetHeroMoveGroup(dx, dy);
            int standGroup = walkGroup - 5;

            Sprite[] groupSprites = mapTextureManager.LoadHeroGroupSprites(defFileName, standGroup, flipX);

            AnimatedMapObject animated = heroGO.GetComponent<AnimatedMapObject>();
            if (animated != null && groupSprites != null && groupSprites.Length > 0)
            {
                animated.SetSprites(groupSprites, 18);
            }
        }

        /// <summary>
        /// Hero DEF group mapping by movement direction (dx, dy).
        /// Hero DEF structure: groups 0-4 = standing (N, NE, E, SE, S),
        /// groups 5-9 = walking (N, NE, E, SE, S).
        /// NW/W/SW use flipped NE/E/SE groups.
        /// Based on VCMI moveGroups: {0, 10, 5, 6, 7, 8, 9, 12, 11}
        /// Returns (groupIndex, flipX).
        /// </summary>
        private static (int group, bool flipX) GetHeroMoveGroup(int dx, int dy)
        {
            // Walking groups in DEF: 5=N, 6=NE, 7=E, 8=SE, 9=S
            // Groups 6/7/8 face right (NE/E/SE) natively.
            // For NW/W/SW, flip the corresponding right-facing group.
            if (dx == 0 && dy == -1) return (5, false);  // N
            if (dx == 1 && dy == -1) return (6, false);  // NE
            if (dx == 1 && dy == 0)  return (7, false);  // E
            if (dx == 1 && dy == 1)  return (8, false);  // SE
            if (dx == 0 && dy == 1)  return (9, false);  // S
            if (dx == -1 && dy == 1)  return (8, true);  // SW = flipped SE
            if (dx == -1 && dy == 0)  return (7, true);  // W  = flipped E
            if (dx == -1 && dy == -1) return (6, true);  // NW = flipped NE

            return (0, false); // Fallback: standing N
        }

        /// <summary>
        /// Calculate the direction index (0-8) for movement from one tile to an adjacent tile.
        /// Formula: (dx+1) + 3*(dy+1)
        /// </summary>
        private int GetDirectionIndex(Vector2Int from, Vector2Int to)
        {
            int dx = Mathf.Clamp(to.x - from.x, -1, 1);
            int dy = Mathf.Clamp(to.y - from.y, -1, 1);
            return (dx + 1) + 3 * (dy + 1);
        }
    }
}
