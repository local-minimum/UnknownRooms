using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class LookAround : Ability
    {
        Coordinate GetRandomDirectionFrom(Coordinate coordinate)
        {
            if (Random.value < 0.5)
            {
                coordinate.x = 0;
                coordinate.y = coordinate.y > 0 ? 1 : -1;
            }
            else
            {
                coordinate.x = coordinate.x > 0 ? 1 : -1;
                coordinate.y = 0;
            }

            return coordinate;
        }

        public override bool Enact()
        {
            Coordinate offset;
            if (monster.trackingPlayer)
            {
                offset = monster.player.position - monster.position;
                if (offset.x != 0 && offset.y != 0)
                    offset = GetRandomDirectionFrom(offset);
            } else
            {
                offset.x = Random.Range(-1, 1);
                offset.y = Random.Range(-1, 1);
                if ((offset.x ^ offset.y) == 0)
                    offset = GetRandomDirectionFrom(offset);
            }

            monster.lookDirection = offset;
            return base.Enact();
        }

    }
}
