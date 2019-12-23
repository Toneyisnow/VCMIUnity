using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public class BinaryData : IFileData
    {
        private byte[] rawBytes = null;

        public BinaryData(byte[] rawBytes)
        {
            this.rawBytes = rawBytes;
        }

        public BinaryData()
        {
        }


        public byte[] Bytes
        {
            get
            {
                return rawBytes;
            }
        }

        public byte[] SerializeToBytes()
        {
            return rawBytes;
        }

        public void DeserializeFromBytes(byte[] data)
        {
            rawBytes = data;
        }
    }
}
