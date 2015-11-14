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
                    path = RoomSearch.FindShortestPath(Tower.ActiveRoom, monster.position, playerPosition);
                    Debug.Log("Shortest path is " + path.Length);
                    pathPosition = -1;
                }
                pathPosition++;
                if (pathPosition < path.Length)
                {
                    if (pathPosition < path.Length - (monster.trackingPlayer ? 1 : 0))
                    {
                        monster.lookDirection = path[pathPosition] - monster.position;
                        monster.RequestMove(path[pathPosition]);
                    }
                    else
                        path = new Coordinate[0];
                }
            }

            return base.Enact();
        }
    }
}
