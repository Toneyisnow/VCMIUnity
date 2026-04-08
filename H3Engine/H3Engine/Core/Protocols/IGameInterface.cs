using H3Engine.Engine.PathFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H3Engine.Core.Constants;

namespace H3Engine.Core.Protocols
{
    public interface IGameInterface
    {
        void showHeroMoving(int heroId, List<MapPathNode> pathNodes);

    }
}


