using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public class Trapper : MonoBehaviour
    {

        Trapper _instance;

        void Awake() {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
                Destroy(this);

        }

        public static void LaySpikeTraps(Room room, RoomData data, int parts)
        {
            var selectedCoordinates = new List<Coordinate>();
            var potentialSources = RoomSearch.GetTileIndicesWithType(TileType.Walkable, data.tileTypeMap);
            var position = potentialSources[Random.Range(0, potentialSources.Count)];

            selectedCoordinates.Add(Coordinate.FromPosition(position, data.width));
            parts--;
            room.SetTileType(position, TileType.SpikeTrap);
            data.tileTypeMap[position] = (int)TileType.SpikeTrap;

            while (parts > 0)
            {
                var prevCoordIndex = selectedCoordinates.Count - 1;
                var nextCandidates = RoomSearch.GetPositionsAtDistance(
                    data.tileTypeMap, 
                    selectedCoordinates[prevCoordIndex].ToPosition(data.width, data.height), 
                    new Range(1, 2), TileType.Walkable, 
                    false, 
                    data.width);
                
                if (nextCandidates.Count == 0)
                    return;
                else if (selectedCoordinates.Count == 1)
                {
                    position = nextCandidates[Random.Range(0, nextCandidates.Count)];
                } else
                {
                    
                    var offset = selectedCoordinates[prevCoordIndex] - selectedCoordinates[prevCoordIndex - 1];
                    position = (offset + selectedCoordinates[prevCoordIndex]).ToPosition(data.width, data.height);
                    if (!nextCandidates.Contains(position))
                    {
                        position = (offset.Rotated90CCW() + selectedCoordinates[prevCoordIndex]).ToPosition(data.width, data.height);
                        if (!nextCandidates.Contains(position))
                        {
                            position = (offset.Rotated90CW() + selectedCoordinates[prevCoordIndex]).ToPosition(data.width, data.height);
                            if (!nextCandidates.Contains(position))
                                return;
                        }
                    }
                }

                selectedCoordinates.Add(Coordinate.FromPosition(position, data.width));
                parts--;
                room.SetTileType(position, TileType.SpikeTrap);
                data.tileTypeMap[position] = (int)TileType.SpikeTrap;

            }


        }

    }

}
