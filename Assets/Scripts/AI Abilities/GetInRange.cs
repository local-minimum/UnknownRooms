using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class GetInRange : AimedWalk
    {

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

        protected override void SetNewAim()
        {
            _aim = monster.trackingPlayer ? monster.player.position : monster.playerLastSeenPosition;
        }

        public override bool Enact()
        {
            pathTruncation = monster.trackingPlayer ? 1 : 0;
            return base.Enact();
        }
    }
}
