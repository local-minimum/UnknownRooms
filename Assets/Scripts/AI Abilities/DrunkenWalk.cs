using UnityEngine;
using System.Collections;

namespace ProcRoom.AI.Abilities
{
    public class DrunkenWalk : Ability
    {

        [SerializeField, Range(0, 1)]
        float eagerness = 0.7f;

        bool AttemptMove(Coordinate lookDirection)
        {
            if (Random.value > eagerness)
                return false;

            var suggestedPosition = monster.position + lookDirection;
            if (Tower.ActiveRoom.PassableTile(suggestedPosition) && (Tower.ActiveRoom.GetTileTypeAt(suggestedPosition) != TileType.SpikeTrap || monster.actionPoints > 1)) {
                if (!monster.lookDirection.Equals(lookDirection))
                    monster.lookDirection = lookDirection;
                monster.RequestMove(suggestedPosition);
                return true;
            }

            return false;
        }

        public override bool Enact()
        {
            Debug.Log(name + " enacting drunken walk");
            if (AttemptMove(monster.lookDirection)) {
                if (Random.value < 0.5f) {
                    if (AttemptMove(monster.lookDirection.Rotated90CW()) || AttemptMove(monster.lookDirection.Rotated90CCW()) || AttemptMove(monster.lookDirection.Rotated180())) ;
                } else
                {
                    if (AttemptMove(monster.lookDirection.Rotated90CCW()) || AttemptMove(monster.lookDirection.Rotated90CW()) || AttemptMove(monster.lookDirection.Rotated180())) ;
                }
            }
            return base.Enact();
        }
    }
}