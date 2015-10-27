using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public class Tower : MonoBehaviour
    {

        static Tower _instance;

        List<RoomData> roomHistory = new List<RoomData>();

        int activeLevel = 0;

        Room room;
        Player player;
        
        void Awake() {
            if (_instance == null)
            {
                _instance = this;
                player = FindObjectOfType<Player>();
                player.SetFullHealth();
                
            } else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        public static int StairsDownNextToGenerate(int roomWidth, int roomHeight)
        {
            if (_instance.roomHistory.Count == 0)
                return -1;

            
            var refRoom = _instance.roomHistory[_instance.roomHistory.Count - 1];
            var pos = RoomMath.GetCorrespondingPosition(refRoom, TileType.StairsUp, roomWidth, roomHeight, true);
            var coord = Coordinate.FromPosition(pos, roomWidth);
            
            if (RoomMath.CoordinateOnCorner(coord, roomWidth, roomHeight))
            {
                if (coord.x == 0)
                    coord.x = 1;
                else if (coord.y == 0)
                    coord.y = 1;
                else
                    coord.x -= 1;
            }
            return coord.ToPosition(roomWidth, roomHeight);
        }

        void OnEnable() {
            Room.OnRoomGeneration += HandleRoomGenerated;
            Player.OnPlayerEnterNewPosition += HandleNewPlayerPosition;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleRoomGenerated;
            Player.OnPlayerEnterNewPosition -= HandleNewPlayerPosition;
        }

        private void HandleRoomGenerated(Room room, RoomData data)
        {
            this.room = room;
            roomHistory.Add(data);

        }

        private void HandleNewPlayerPosition(Player player, Coordinate position, TileType tileType)
        {
            if (tileType == TileType.StairsUp)
                room.Generate();
        }

        public static void RoomDone()
        {
            _instance.player.Enact();
        }

        public static void PlayerDone() {
            _instance.animateRoom();
        }

        public static void Reset()
        {
            _instance.roomHistory.Clear();
            _instance.room.Generate();
            _instance.player.SetFullHealth();
        }

        public void animateRoom()
        {
            StartCoroutine(_instance.room.Enact());
        }

    }
}
