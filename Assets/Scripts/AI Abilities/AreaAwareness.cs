using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class AreaAwareness : Ability
    {
        [SerializeField, Range(0, 40)]
        int awarenessRange;

        public override bool Enact()
        {
            if (RoomMath.GetManhattanDistance(monster.player.position, monster.position) <= awarenessRange)
            {
                monster.trackingPlayer = true;
                return base.Enact();
            }
            return false;
        }
    }
}