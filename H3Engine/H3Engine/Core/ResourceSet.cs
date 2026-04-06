// Moved from H3Engine.Components.Data.ResourceSet → H3Engine.Core

using H3Engine.Common;
using System.Collections.Generic;

namespace H3Engine.Core
{
    /// <summary>
    /// A set of resource amounts (gold, wood, ore, …).
    /// Corresponds to VCMI's TResources (array of resource counts).
    /// </summary>
    public class ResourceSet
    {
        public Dictionary<EResourceType, int> Resources
        {
            get; set;
        } = new Dictionary<EResourceType, int>();

        public int GetAmount(EResourceType type)
        {
            Resources.TryGetValue(type, out int amount);
            return amount;
        }

        public void SetAmount(EResourceType type, int amount)
        {
            Resources[type] = amount;
        }
    }
}
