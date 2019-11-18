using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public class GZipStreamHelper
    {
        public static byte[] DecompressBytes(byte[] rawBytes)
        {
            using (GZipStream stream = new GZipStream(new MemoryStream(rawBytes), CompressionMode.Decompress))
            {
                const int size = 2096;
                byte[] buffer = new byte[size];
                using (MemoryStream output = new MemoryStream())
                {
                    int count = 0;
                    do
                    {
                        count = stream.Read(buffer, 0, size);
                        if (count > 0)
                        {
                            output.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);

                    return output.ToArray();
                }
            }
        }

    }
}
