using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Engine.Components.Protocols
{
    public interface IGameEventReceiver
    {
        void buildingChanged(object town, object buildingId, int changeType); // 1 - built, 2 - demolished

        void battleResultApplied();

        void garrisonChanged(object id1, object id2);


        // Artifact Operations
        void artifactPut();
        void artifactRemoved();
        void artifactAssembled();
        void artifactDessembled();
        void artifactMoved(object location1, object location2);


        // Hero Operations
        void heroVisit(object visitor, object visitedObj, bool start);
        void heroCreated(object hero);
        void heroInGarrisonChange(object town);



    }
}
