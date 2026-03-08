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
        private static readonly string GAME_DATA_FOLDER = Path.Combine(Application.dataPath, @"..\..\GameData\SOD");

        public static string GetGameDataFilePath(string filename)
        {
            return Path.Combine(GAME_DATA_FOLDER, filename);
        }
    }
}
