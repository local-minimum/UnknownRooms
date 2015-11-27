using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public class Treasure : MonoBehaviour
    {

        SpriteRenderer rend;

        [Range(1, 10)]
        public int value = 1;
        Coordinate _position;

        public Coordinate position {
            set
            {
                _position = value;
                transform.position = Tower.ActiveRoom.GetTileCentre(value);
            }
        }

        public bool Showing
        {
            get
            {
                return rend.enabled;
            }

            set
            {
                rend.enabled = value;
            }
        }

        void Awake() {
            rend = GetComponent<SpriteRenderer>();
            Showing = false;
        }

        void OnEnable()
        {
            Tower.OnLevelEnd += HandleLevelEnd;
            Player.OnPlayerEnterNewPosition += HandlePlayerMove;
        }

        private void HandlePlayerMove(Player player, Coordinate position, TileType tileType)
        {
            if (Showing && position == _position)
            {
                player.AwardPoints(value);
                Showing = false;
            }
        }

        void OnDisable()
        {
            Tower.OnLevelEnd -= HandleLevelEnd;
            Player.OnPlayerEnterNewPosition -= HandlePlayerMove;
        }

        private void HandleLevelEnd()
        {
            Showing = false;
        }
    }
}