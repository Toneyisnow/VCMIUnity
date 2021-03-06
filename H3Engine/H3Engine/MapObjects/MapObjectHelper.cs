﻿using H3Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.MapObjects
{
    public class MapObjectHelper
    {
        private static HashSet<int> decorationTemplateIds = new HashSet<int>()
        {
            116, 117, 118, 119, 120, 121, 124, 125, 126, 127, 128, 129, 130, 131, 132, 133, 134, 135, 136, 137,
            143, 147, 148, 149, 150, 151, 153, 155, 158, 161, 171, 189, 199, 206, 207, 208, 209, 210, 211
        };

        public static bool IsDecorationObject(EObjectType objectType)
        {
            return decorationTemplateIds.Contains(objectType.GetHashCode());
        }

        /// <summary>
        /// Return true if the map object is static and cannot be removed from map
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public static bool IsStaticObject(EObjectType objectType)
        {
            return decorationTemplateIds.Contains(objectType.GetHashCode())
                || (objectType == EObjectType.MINE)
                || (objectType == EObjectType.TOWN)
                || (objectType == EObjectType.SCHOLAR);
        }

        public static bool IsRemovableObject(EObjectType objectType)
        {
            return (objectType == EObjectType.HERO)
                || (objectType == EObjectType.ARTIFACT)
                || (objectType == EObjectType.BOAT)
                || (objectType == EObjectType.RESOURCE)
                || (objectType == EObjectType.TREASURE_CHEST);
        }


    }
}
