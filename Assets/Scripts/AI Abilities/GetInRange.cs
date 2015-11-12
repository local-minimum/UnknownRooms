using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class GetInRange : Ability
    {

        Coordinate[] path = new Coordinate[0];
        int pathPosition = -1;

        public override void NewTurn()
        {
            base.NewTurn();
            pathPosition = -1;
            path = new Coordinate[0];
        }

        public override bool Allowed
        {
            get
            {
                if (monster.trackingPlayer)
                    return base.Allowed && monster.weaponRange < RoomMath.GetManhattanDistance(monster.player.position, monster.position);
                else
                    return base.Allowed && monster.hasEverSeenPlayer && !monster.position.Equals(monster.playerLastSeenPosition); 
            }
        }

        public override bool Enact()
        {
            Debug.Log(name + " enacting GetInRange");
            if (monster.trackingPlayer)
            {
                if (path.Length == 0)
                {
                    Coordinate playerPosition = monster.trackingPlayer ? monster.player.position : monster.playerLastSeenPosition;
                    var roomData = Tower.ActiveRoom.GetData();
                    path = RoomSearch.FindShortestPath(monster.position, playerPosition, roomData.tileTypeMap, roomData.width);
                    Debug.Log("Shortest path is " + path.Length);
                    pathPosition = path.Length > 0 ? 0 : -1;
                }
                if (pathPosition >= 0)
                {
                    pathPosition++;
                    if (pathPosition < path.Length - (monster.trackingPlayer ? 1 : 0))
                    {
                        monster.lookDirection = path[pathPosition] - path[pathPosition - 1];
                        monster.RequestMove(path[pathPosition]);
                    }
                }
            }

            return base.Enact();
        }
    }
}
