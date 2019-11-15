﻿using H3Engine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public static class BinaryReaderExtension
    {
        public static void Skip(this BinaryReader reader, int count)
        {
            reader.ReadBytes(count);
        }


        public static void Seek(this BinaryReader reader, int count)
        {
            reader.BaseStream.Seek(count, SeekOrigin.Begin);
        }

        public static MapPosition ReadPosition(this BinaryReader reader)
        {
            int x = reader.ReadByte();
            int y = reader.ReadByte();
            int z = reader.ReadByte();

            MapPosition position = new MapPosition(x, y, z);

            return position;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="byteCount"></param>
        /// <param name="limit"></param>
        /// <param name="negate"></param>
        public static void ReadBitMask(this BinaryReader reader, HashSet<int> dest, int byteCount, int limit, bool negate = true)
        {
            bool[] boolDest = new bool[limit];
            reader.ReadBitMask(boolDest, byteCount, limit, negate);

            for (int i = 0; i < limit; i++)
            {
                if (boolDest[i])
                {
                    dest.Add(i);
                }
            }
        }
        
        public static void ReadBitMask(this BinaryReader reader, bool[] dest, int byteCount, int limit, bool negate)
        {
            for (int nowByte = 0; nowByte < byteCount; nowByte++)
            {
                int mask = reader.ReadByte();
                for (int bit = 0; bit < 8; ++bit)
                {
                    if (nowByte * 8 + bit < limit)
                    {
                        bool flag = (mask & (1 << bit)) > 0;
                        dest[nowByte * 8 + bit] = (flag != negate);        // FIXME: check PR388
                    }
                }
            }
        }

        public static string ReadStringWithLength(this BinaryReader reader)
        {
            UInt32 length = reader.ReadUInt32();
            byte[] result = new byte[length];

            for (var i = 0; i < length; i++)
            {
                result[i] = reader.ReadByte();
                //if (result[i] == '\0')
                {
                //    break;
                }

                if (reader.BaseStream.Position >= reader.BaseStream.Length)
                {
                    // If the reader is beyond the file length, just skip
                    break;
                }
            }

            return Encoding.ASCII.GetString(result);
        }
        

        public static string ReadStringToEnd(this BinaryReader reader)
        {
            byte[] result = new byte[1024];
            for (var i = 0; i < 1024; i++)
            {
                result[i] = reader.ReadByte();
                if (result[i] == '\0')
                    break;
            }

            return Encoding.ASCII.GetString(result);
        }


    }
}