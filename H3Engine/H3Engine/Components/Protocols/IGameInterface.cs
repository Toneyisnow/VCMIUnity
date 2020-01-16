using H3Engine.Components.MapProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.Protocols
{
    public interface IGameInterface
    {
        void showHeroMoving(int heroId, List<MapPathNode> pathNodes);

    }
}
