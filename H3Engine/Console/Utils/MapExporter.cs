using H3Engine.Common;
using H3Engine.FileSystem;
using H3Engine.GUI;
using H3Engine.MapObjects;
using H3Engine.Mapping;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Console.Utils
{
    public class MapExporter
    {
        private H3Map h3Map = null;

        private H3Engine.DataAccess.H3DataAccess h3Engine = null;

        private int unitWidth = 32;
        private int unitHeight = 32;

        private int mapWidth = 0;
        private int mapHeight = 0;

        private int canvasWidth = 0;
        private int canvasHeight = 0;

        private byte[] canvasData;

        public MapExporter(H3Map h3Map)
        {
            this.h3Map = h3Map;
            this.h3Engine = H3Engine.DataAccess.H3DataAccess.GetInstance();

            mapWidth = (int)h3Map.Header.Width;
            mapHeight = (int)h3Map.Header.Height;

            canvasWidth = (mapWidth + 2) * unitWidth;
            canvasHeight = (mapHeight + 2) * unitHeight;

        }
        

        public void ExportToPng(string pngFileName)
        {
            int levelCount = h3Map.Header.IsTwoLevel ? 2 : 1;

            if (levelCount == 1)
            {
                ExportLevelToPng(pngFileName + ".png", 0);
            }
            else
            {
                ExportLevelToPng(pngFileName + "_A.png", 0);
                ExportLevelToPng(pngFileName + "_B.png", 1);
            }
        }
        
        private void ExportLevelToPng(string pngFileName, int level)
        {
            canvasData = new byte[canvasWidth * canvasHeight * 4];

            // Render the Terrain Tiles
            for (int xx = 0; xx < h3Map.Header.Width; xx++)
            {
                for (int yy = 0; yy < h3Map.Header.Height; yy++)
                {
                    TerrainTile tile = h3Map.TerrainTiles[level, xx, yy];

                    ImageData imageData = h3Engine.RetrieveTerrainImage(tile.TerrainType, tile.TerrainView);
                    DrawOnCanvas(imageData, xx, yy, tile.TerrainRotation);

                    if (tile.RoadType != H3Engine.Common.ERoadType.NO_ROAD)
                    {
                        ImageData imageDataRoad = h3Engine.RetrieveRoadImage(tile.RoadType, tile.RoadDir);
                        DrawOnCanvas(imageDataRoad, xx, yy, tile.RoadRotation);
                    }

                    if (tile.RiverType != H3Engine.Common.ERiverType.NO_RIVER)
                    {
                        ImageData imageDataRiver = h3Engine.RetrieveRiverImage(tile.RiverType, tile.RiverDir);
                        DrawOnCanvas(imageDataRiver, xx, yy, tile.RiverRotation);
                    }
                }
            }

            // Render the MapObjects
            DrawMapObjects(level);

            // Draw Edges
            DrawEdges();
           
            using (FileStream output = new FileStream(pngFileName, FileMode.Create, FileAccess.Write))
            {
                unsafe
                {
                    fixed (byte* ptr = canvasData)
                    {
                        using (Bitmap image = new Bitmap(canvasWidth, canvasHeight, canvasWidth * 4, PixelFormat.Format32bppPArgb, new IntPtr(ptr)))
                        {
                            image.Save(output, ImageFormat.Png);
                        }
                    }
                }
            }
        }

        private void DrawMapObjects(int mapLevel)
        {
            h3Map.Objects.Sort((objA, objB) => {
                if (objA.Position.PosY < objB.Position.PosY || objA.Position.PosY == objB.Position.PosY && objA.Position.PosX > objB.Position.PosX)
                {
                    return -1;
                }
                if (objA.Position.PosX == objB.Position.PosX && objA.Position.PosY == objA.Position.PosY)
                {
                    return 0;
                }

                return 1;
            });

            foreach (CGObject obj in h3Map.Objects)
            {
                if (obj.Position.Level != mapLevel)
                {
                    continue;
                }

                ObjectTemplate template = obj.Template;
                if (template == null)
                {
                    continue;
                }

                string defFileName = template.AnimationFile;
                BundleImageDefinition bundleImage = h3Engine.RetrieveBundleImage(defFileName);
                ImageData imageData = bundleImage.GetImageData(0, 0);

                DrawOnCanvas(imageData, obj.Position.PosX, obj.Position.PosY);
            }
        }
        
        private void DrawEdges()
        {
            BundleImageDefinition bundleImage = h3Engine.RetrieveBundleImage("EDG.def");

            ImageData up = bundleImage.GetImageData(0, 20);
            ImageData down = bundleImage.GetImageData(0, 28);
            ImageData left = bundleImage.GetImageData(0, 32);
            ImageData right = bundleImage.GetImageData(0, 24);

            ImageData upleft = bundleImage.GetImageData(0, 16);
            ImageData upright = bundleImage.GetImageData(0, 17);
            ImageData downleft = bundleImage.GetImageData(0, 19);
            ImageData downright = bundleImage.GetImageData(0, 18);

            for (int xx = 0; xx < h3Map.Header.Width; xx++)
            {
                DrawOnCanvas(up, xx, -1);
                DrawOnCanvas(down, xx, mapHeight);
            }

            for (int yy = 0; yy < h3Map.Header.Height; yy++)
            {
                DrawOnCanvas(left, -1, yy);
                DrawOnCanvas(right, mapWidth, yy);
            }

            DrawOnCanvas(upleft, -1, -1);
            DrawOnCanvas(upright, mapWidth, -1);
            DrawOnCanvas(downleft, -1, mapHeight);
            DrawOnCanvas(downright, mapWidth, mapHeight);
        }

        private void DrawOnCanvas(ImageData imageData, int posX, int posY, byte rotation = 0)
        {
            int canvasX = (posX + 2) * unitWidth - 1;
            int canvasY = (posY + 2) * unitHeight - 1;
            
            for(int i = imageData.Width - 1; i >= 0; i--)
            {
                for(int j = imageData.Height - 1; j >= 0; j--)
                {
                    int targetX = canvasX + i + 1 - imageData.Width;
                    int targetY = canvasY + j + 1 - imageData.Height;

                    if (targetX < 0 || targetY < 0 || targetX >= canvasWidth || targetY >= canvasHeight)
                    {
                        continue;
                    }

                    int targetIndex = (targetY * canvasWidth + targetX) << 2;

                    H3Engine.Common.Color h3Color = imageData.GetPixelColor(i, j, rotation);

                    // For transparent color, just skip
                    if (h3Color.A == 0 && h3Color.R == 0 && h3Color.G == 255 && h3Color.B == 255)
                    {
                        continue;
                    }

                    if (h3Color.A != 0 && h3Color.A != 255)
                    {
                        canvasData[targetIndex] = (byte)((canvasData[targetIndex] + h3Color.R) / 2);
                        canvasData[targetIndex + 1] = (byte)((canvasData[targetIndex] + h3Color.G) / 2);
                        canvasData[targetIndex + 2] = (byte)((canvasData[targetIndex] + h3Color.B) / 2);
                        canvasData[targetIndex + 3] = 255;
                    }
                    else
                    {
                        canvasData[targetIndex] = h3Color.R;
                        canvasData[targetIndex + 1] = h3Color.G;
                        canvasData[targetIndex + 2] = h3Color.B;
                        canvasData[targetIndex + 3] = h3Color.A;
                    }
                }
            }
        }
    }


}
