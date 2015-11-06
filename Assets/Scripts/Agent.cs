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
    }

    public abstract class Agent : MonoBehaviour
    {

        [SerializeField]
        protected AgentStats _stats;

        protected int roomWidth;
        protected int roomHeight;

        abstract public void Enact();

    }
}
