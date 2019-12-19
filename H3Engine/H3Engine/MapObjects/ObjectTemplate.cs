using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.MapObjects
{
    public class ObjectTemplate
    {
        public enum EBlockMapBits
        {
            VISIBLE = 1,
            VISITABLE = 2,
            BLOCKED = 4
        }

        public EObjectType Type
        {
            get; set;
        }

        public int SubId
        {
            get; set;
        }

        public byte Width
        {
            get; set;
        }

        public byte Height
        {
            get; set;
        }

        public int PrintPriority
        {
            get; set;
        }

        public string AnimationFile
        {
            get; set;
        }

        public string EditorAnimationFile
        {
            get; set;
        }

        public string StringId
        {
            get; set;
        }

        /// <summary>
        /// BlockMask Read from .msk file.
        /// Note: the data is read from 6 Bytes data, with 8 bits per line. Even if the Object width is not 8, it will still read 8 bits per line
        /// 
        /// 7 6 5 4 3*2*1*0
        /// 7 6 5 4*3*2*1*0*
        /// 7 6 5 4*3*2 1*0*
        /// 
        /// 
        /// </summary>
        public bool[] BlockMask
        {
            get; set;
        }

        public bool[] VisitMask
        {
            get; set;
        }


    }
}
