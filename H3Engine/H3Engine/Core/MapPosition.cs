﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Core
{
    public class MapPosition
    {
        public int PosX
        {
            get; private set;
        }

        public int PosY
        {
            get; private set;
        }

        public int Level
        {
            get; private set;
        }

        public MapPosition(int x, int y, int z)
        {
            this.PosX = x;
            this.PosY = y;
            this.Level = z;
        }
    }
}
