using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    [System.Serializable]
    public class AgentStats
    {
        public int actionPointsPerTurn = 3;
        public int actionPoints = -1;

        public int maxHealth = 7;
        public int health;

        public int maxAmmo = 11;
        public int ammo = 0;

        public int evasion = 10;

        public Coordinate position;
        public Coordinate lookDirection = Coordinate.Right;

        public bool hasAmmo
        {
            get
            {
                return maxAmmo < 1 || ammo > 0;
            }
        }
    }

    public abstract class Agent : MonoBehaviour
    {

        [SerializeField]
        protected AgentStats _stats;


        [SerializeField]
        protected Weapon weapon;

        protected int roomWidth;
        protected int roomHeight;

        protected int shots = 0;

        protected int actionPoints
        {
            get
            {
                return _stats.actionPoints;
            }

            set
            {
                _stats.actionPoints = Mathf.Clamp(value, 0, _stats.actionPointsPerTurn);
                if (_stats.actionPoints == 0)
                    EndTurn();
            }
        }

        protected bool myTurn
        {
            get
            {
                return !weapon.isShooting && actionPoints > 0;
            }
        }


        public bool alive
        {
            get
            {
                return _stats.health > 0;
            }

        }

        abstract public void Enact();

        protected void HandleTileAction(Tile tile, TileType typeOfTile, Coordinate position)
        {
            if (position.Equals(_stats.position) && typeOfTile == TileType.SpikeTrap)
            {
                tile.Maim();
                Hurt();
            }
        }

        public void Hurt()
        {
            _stats.health--;

            if (_stats.health < 1)
            {
                actionPoints = 0;
                Death();
            }
        }

        public void EndTurn()
        {
            _stats.actionPoints = 0;
            Tower.AgentDone(this);
        }

        abstract protected void Death();

        protected void Attack()
        {
            if (_stats.hasAmmo && weapon.Shoot(_stats.position, _stats.lookDirection))
            {
                _stats.ammo--;
                shots++;
                actionPoints--;
            }
        }
    }
}
