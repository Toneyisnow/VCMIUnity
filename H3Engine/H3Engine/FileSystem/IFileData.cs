using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.FileSystem
{
    public enum EFileDataType
    {
        Binary = 0,
        Image = 1
    }

    public interface IFileData
    {
        
        byte[] SerializeToBytes();

        void DeserializeFromBytes(byte[] data);
    }
}
