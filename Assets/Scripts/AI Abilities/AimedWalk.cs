using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{

    public class AimedWalk : Ability
    {

        protected Coordinate _aim;
        protected Coordinate[] path = new Coordinate[0];
        protected int pathPosition = -1;
        protected int pathTruncation = 0;

        virtual protected void SetNewAim()
        {
            _aim = Tower.ActiveRoom.GetRandomFreeTileCoordinate(monster.position, 10);
        }

        public override bool Enact()
        {
            Debug.Log(name + " enacting GetInRange");

            if (pathPosition < 0 || pathPosition >= path.Length - pathTruncation)
            {
                SetNewAim();
                path = RoomSearch.FindShortestPath(Tower.ActiveRoom, monster.position, _aim);

                pathPosition = -1;
            }
            pathPosition++;
            if (pathPosition < path.Length - pathTruncation)
            {
                monster.lookDirection = path[pathPosition] - monster.position;
                monster.RequestMove(path[pathPosition]);
            }

            base.Enact();
            return false;
        }
    }
}