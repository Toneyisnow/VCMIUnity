using H3Engine.Common;
using H3Engine.Utils;
using System;
using System.Collections.Generic;


//// using System.Drawing;
//// using System.Drawing.Imaging;
/// 
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public class ImageData
    {
        private byte[] rawData;

        private Dictionary<byte, byte[]> plainData = null;
        
        private int dataIndex = 0;

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

        public ImageData(int width, int height)
        {
            this.Width = width;
            this.Height = height;

            this.rawData = new byte[width * height * 4];
            this.plainData = new Dictionary<byte, byte[]>();

            this.dataIndex = 0;
        }

        public void WriteData(byte red, byte green, byte blue, byte alpha)
        {
            this.rawData[this.dataIndex++] = red;
            this.rawData[this.dataIndex++] = green;
            this.rawData[this.dataIndex++] = blue;
            this.rawData[this.dataIndex++] = alpha;
        }

        public void WriteColor(Color color)
        {

            this.rawData[this.dataIndex++] = color.R;
            this.rawData[this.dataIndex++] = color.G;
            this.rawData[this.dataIndex++] = color.B;
            this.rawData[this.dataIndex++] = color.A;
        }
        
        public Color GetPixelColor(int x, int y, byte rotation = 0)
        {
            if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
            {
                throw new IndexOutOfRangeException();
            }

            byte[] data = GetPlainData(rotation);
            int index = (y * this.Width + x) << 2;

            byte r = data[index];
            byte g = data[index + 1];
            byte b = data[index + 2];
            byte a = data[index + 3];

            return Color.FromArgb(a, r, g, b);
        }

        public byte[] RawData
        {
            get
            {
                return rawData;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rotationIndex"></param>
        /// <returns></returns>
        private byte[] GetPlainData(byte rotation = 0)
        {
            if (this.plainData.ContainsKey(rotation))
            {
                return this.plainData[rotation];
            }

            byte[] data = null;
            switch(rotation)
            {
                case 0:
                    data = rawData;
                    break;
                case 1:
                    data = FlipLeftAndRight(rawData);
                    break;
                case 2:
                    data = FlipUpAndDown(rawData);
                    break;
                case 3:
                    data = Rotate180(rawData);
                    break;
                default:
                    return null;
            }

            this.plainData[rotation] = data;
            return data;
        }
        
        private byte[] Rotate180(byte[] raw)
        {
            byte[] resultData = new byte[Width * Height * 4];

            for (int j = 0; j < this.Height; j++)
            {
                for (int i = 0; i < this.Width; i++)
                {
                    int originIndex = i + Width * j;
                    int newIndex = Width - 1 - i + Width * (Height - j - 1);

                    for (int w = 0; w < 4; w++)
                    {
                        resultData[newIndex * 4 + w] = raw[originIndex * 4 + w];
                    }
                }
            }

            return resultData;
        }

        private byte[] FlipUpAndDown(byte[] raw)
        {
            byte[] resultData = new byte[Width * Height * 4];

            for (int j = 0; j < this.Height; j++)
            {
                for (int i = 0; i < this.Width; i++)
                {
                    int originIndex = i + Width * j;
                    int newIndex = i + Width * (Height - j - 1);

                    for (int w = 0; w < 4; w++)
                    {
                        resultData[newIndex * 4 + w] = raw[originIndex * 4 + w];
                    }
                }
            }

            return resultData;
        }

        private byte[] FlipLeftAndRight(byte[] raw)
        {
            byte[] resultData = new byte[Width * Height * 4];

            for (int j = 0; j < this.Height; j++)
            {
                for (int i = 0; i < this.Width; i++)
                {
                    int originIndex = i + Width * j;
                    int newIndex = Width - 1 - i + Width * j;

                    for (int w = 0; w < 4; w++)
                    {
                        resultData[newIndex * 4 + w] = raw[originIndex * 4 + w];
                    }
                }
            }

            return resultData;
        }
    }
}
