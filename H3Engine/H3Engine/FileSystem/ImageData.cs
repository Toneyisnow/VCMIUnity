using H3Engine.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public class ImageData
    {
        private byte[] rawData;

        private Dictionary<byte, byte[]> pngData = null;

        private MemoryStream pngStream = null;

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
            this.pngData = new Dictionary<byte, byte[]>();

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

        public void ExportDataToPNG(bool needRotate = false)
        {
            if (rawData == null)
            {
                return;
            }

            this.pngData = new Dictionary<byte, byte[]>();
            pngData[0] = GeneratePNGData(rawData);

            if (needRotate)
            {
                pngData[1] = GeneratePNGData(FlipLeftAndRight(rawData));
                pngData[2] = GeneratePNGData(FlipUpAndDown(rawData));
                pngData[3] = GeneratePNGData(Rotate180(rawData));
            }

            // Clean up the rawData to free memory
            rawData = null;
        }

        public byte[] GetPNGData(byte rotation = 0)
        {
            if (pngData == null)
            {
                return null;
            }

            if(rotation >= pngData.Count)
            {
                throw new ArgumentOutOfRangeException("Rotation index is out of range of PNG Data.");
            }

            return pngData[rotation];
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

        private byte[] GeneratePNGData(byte[] raw)
        {
            using (MemoryStream output = new MemoryStream())
            {
                unsafe
                {
                    fixed (byte* ptr = raw)
                    {
                        using (Bitmap image = new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                        {
                            image.Save(output, ImageFormat.Png);
                        }
                    }
                }

                return StreamHelper.ReadToEnd(output);
            }
        }

        /// <summary>
        /// This should not be used any more, it's replaced by ExportDataToPNG()
        /// </summary>
        /// <param name="outputStream"></param>
        public void SaveAsPNGStream(Stream outputStream)
        {
            if (outputStream == null)
            {
                return;
            }

            unsafe
            {
                fixed (byte* ptr = rawData)
                {
                    using (Bitmap image = new Bitmap(this.Width, this.Height, this.Width * 4, PixelFormat.Format32bppRgb, new IntPtr(ptr)))
                    {
                        image.Save(outputStream, ImageFormat.Png);
                    }
                }
            }
        }
    }
}
