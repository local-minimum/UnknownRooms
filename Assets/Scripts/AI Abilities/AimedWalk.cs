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

        [SerializeField, Range(1, 100)]
        int minDistanceToNewAim = 6;

        virtual protected void SetNewAim()
        {
            _aim = Tower.ActiveRoom.GetRandomFreeTileCoordinate(monster.position, minDistanceToNewAim);
        }

        protected int PathPosition(Coordinate position)
        {
                for (int i=0; i<path.Length;i++)
                {
                    if (path[i] == position)
                        return i;
                    
                }
                return -1;
        }

        bool ValidatePath
        {
            get {
                var aimPos = PathPosition(_aim);
                if (aimPos == -1)
                    return false;
                if (aimPos < path.Length - 1)
                {
                    Debug.Log("Truncating path");
                    var newPath = new Coordinate[aimPos + 1];
                    System.Array.Copy(path, newPath, newPath.Length);
                    path = newPath;
                }
                    
                var monsterPos = PathPosition(monster.position);
                if (monsterPos < 0 || monsterPos >= path.Length - 1 - pathTruncation)
                    return false;
                pathPosition = monsterPos;
                return true;
            }
        }


        public override bool Enact()
        {
            //Debug.Log(name + " enacting aimed walk");

            if (!ValidatePath)
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

        void OnDrawGizmosSelected() {
            if (pathPosition < 0)
                return;

            Gizmos.color = Color.red;
            var refCoord = monster.position;
            for (int i=pathPosition; i<path.Length; i++)
            {
                Gizmos.DrawLine(Tower.ActiveRoom.GetTileCentre(refCoord), Tower.ActiveRoom.GetTileCentre(path[i]));
                refCoord = path[i];
            }
        }
    }
}