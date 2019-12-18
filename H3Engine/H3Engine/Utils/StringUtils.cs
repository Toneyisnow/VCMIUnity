using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Utils
{
    public static class StringUtils
    {
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            bool isFirst = true;
            foreach (byte b in ba)
            {
                if (!isFirst)
                {
                    hex.Append(" ");
                }
                else
                {
                    isFirst = false;
                }
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Pattern supports: XXX*, *XX
        /// </summary>
        /// <param name="text"></param>
        /// <param name="wildcardPattern"></param>
        /// <returns></returns>
        public static bool WildCardMatching(this string text, string wildcardPattern)
        {
            wildcardPattern = wildcardPattern.ToLower();

            if (wildcardPattern.EndsWith("*"))
            {
                return text.StartsWith(wildcardPattern.Substring(0, wildcardPattern.Length - 1));
            }

            if (wildcardPattern.StartsWith("*"))
            {
                return text.EndsWith(wildcardPattern.Substring(1));
            }

            if (wildcardPattern.Contains("*"))
            {
                int starIndex = wildcardPattern.IndexOf("*");

                string startPart = wildcardPattern.Substring(0, starIndex);
                string endPart = wildcardPattern.Substring(starIndex + 1);
                return text.StartsWith(startPart) && text.EndsWith(endPart);
            }

            return text == wildcardPattern;
        }
    }
}
