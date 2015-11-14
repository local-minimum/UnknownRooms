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
                Debug.Log("Enacting Area Awareness");
                monster.trackingPlayer = true;
            }
            base.Enact();
            return false;
        }
    }
}