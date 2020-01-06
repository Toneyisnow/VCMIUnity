using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    public class H3DataUtil
    {
        public static string GetGameDataFilePath(string filename)
        {
            return Path.Combine(Application.streamingAssetsPath, @"GameData\SOD.zh-cn", filename);
        }
    }
}
