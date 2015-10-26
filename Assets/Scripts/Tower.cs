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

        void Awake() {
            if (_instance == null)
            {
                _instance = this;
                
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
            var pos = Room.GetCorrespondingPosition(refRoom, TileType.StairsUp, roomWidth, roomHeight, true);
            var coord = Room.PositionToCoordinate(pos, roomWidth);
            
            if (Room.CoordinateOnCorner(coord, roomWidth, roomHeight))
            {
                if (coord.x == 0)
                    coord.x = 1;
                else if (coord.y == 0)
                    coord.y = 1;
                else
                    coord.x -= 1;
            }
            return Room.CoordinateToPosition(coord, roomWidth, roomHeight);
        }

        void OnEnable() {
            Room.OnRoomGeneration += HandleRoomGenerated;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleRoomGenerated;
        }

        private void HandleRoomGenerated(Room room, RoomData data)
        {
            this.room = room;
            roomHistory.Add(data);

        }
    }
}
