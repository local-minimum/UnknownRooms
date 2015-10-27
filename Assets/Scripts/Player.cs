using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    public delegate void PlayerEnterPosition(Player player, Coordinate position, TileType tileType);

    public class Player : MonoBehaviour
    {

        public static event PlayerEnterPosition OnPlayerEnterNewPosition;

        int actionPoints = -1;
        Coordinate position;
        [SerializeField]
        int startHealth = 7;

        int health;
        int ammo = 11;
        int roomWidth;
        int roomHeight;

        [SerializeField]
        int minDistanceSpawninfFirstLevel = 8;

        Room room;

        bool playerTurn {
            get
            {
                return actionPoints > 0;
            }
        }

        void OnEnable()
        {
            Room.OnRoomGeneration += HandleNewRoom;
            Tile.OnTileAction += HandleTileAction;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleNewRoom;
            Tile.OnTileAction -= HandleTileAction;
        }

        private void HandleTileAction(Tile tile, TileType typeOfTile, Coordinate position)
        {
            if (position.Equals(this.position) && typeOfTile == TileType.SpikeTrap)
            {
                tile.Maim();
                Hurt();
            }
        }

        private void HandleNewRoom(Room room, RoomData data)
        {
            roomWidth = data.width;
            roomHeight = data.height;
            this.room = room;
            UpdatePlayerPosition(Coordinate.FromPosition(PlayerSpawnPosition(data), roomWidth));
        }

        int PlayerSpawnPosition(RoomData data)
        {
            if (RoomSearch.HasAnyOfType(TileType.StairsDown, data.tileTypeMap))
            {
                return RoomSearch.GetFirstOccurance(data.tileTypeMap, TileType.StairsDown);
            }
            else
            {

                var candidates = RoomSearch.GetPositionsAtDistance(data.tileTypeMap,
                    RoomSearch.GetFirstOccurance(data.tileTypeMap, TileType.StairsUp),
                    new Range(minDistanceSpawninfFirstLevel), TileType.Walkable, false, data.width);

                return candidates[Random.Range(0, candidates.Count)];
            }

        }

        void Update()
        {
            if (!playerTurn)
                return;

            if (Input.GetButtonDown("right"))
                AttemptMoveTo(position.RightSide());
            else if (Input.GetButtonDown("left"))
                AttemptMoveTo(position.LeftSide());
            else if (Input.GetButtonDown("up"))
                AttemptMoveTo(position.UpSide());
            else if (Input.GetButtonDown("down"))
                AttemptMoveTo(position.DownSide());
            else if (Input.GetButton("endTurn"))
                EndTurn();

        }

        void AttemptMoveTo(Coordinate newPosition)
        {
            var tileType = room.GetTileAt(newPosition);

            if (tileType == TileType.Walkable || tileType == TileType.SpikeTrap || tileType == TileType.StairsUp) { 
                
                UpdatePlayerPosition(newPosition);
                if (OnPlayerEnterNewPosition != null)
                    OnPlayerEnterNewPosition(this, newPosition, tileType);
                actionPoints--;
                if (actionPoints < 1)
                    EndTurn();
            }

        }

        void UpdatePlayerPosition(Coordinate newPosition)
        {
            position = newPosition;
            transform.position = room.GetTileCentre(position.ToPosition(roomWidth, roomHeight));
        }

        public void Enact()
        {
            actionPoints = 3;

        }

        public void EndTurn()
        {
            actionPoints = 0;
            Tower.PlayerDone();
        }

        public void Hurt()
        {
            health--;
            Debug.Log(health);

            if (health < 1)
            {
                actionPoints = -1;
                Tower.Reset();
            }
        }

        public void SetFullHealth()
        {
            health = startHealth;
        }
    }
}