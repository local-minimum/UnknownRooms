using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    public delegate void RoomEvent(Room room, RoomData data);

    [System.Serializable]
    public struct Range
    {
        public int min;
        public int max;

        public Range(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
    }

    [System.Serializable]
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;

        }

        public Coordinate(float x, float y)
        {
            this.x = Mathf.RoundToInt(x);
            this.y = Mathf.RoundToInt(y);
        }
    }

    [System.Serializable]
    public struct RoomData
    {
        public int width;
        public int height;
        public int[] tileTypeMap;
        public Range wallIslands;
        public Range wallGrowthIterations;
        public float biasTowardsGrowingIslands;
        public int minDistanceBetweenStairs;

    }

    public class Room : MonoBehaviour
    {
        
        public static event RoomEvent OnRoomGeneration;

        [SerializeField, Tooltip("Size includes wall perimeter"), Range(3, 30)]
        int width;

        [SerializeField, Tooltip("Size includes wall perimeter"), Range(3, 30)]
        int height;

        int[] tileTypeMap;

        [SerializeField]
        Vector2 tileSpacing = new Vector2(1, 1);

        List<Tile> tiles = new List<Tile>();

        [SerializeField]
        GameObject tileModel;

        [SerializeField]
        int minWallIslands = 0;
        [SerializeField]
        int maxWallIslands = 10;

        [SerializeField]
        int minWallGrowthIterations = 2;

        [SerializeField]
        int maxWallGrowthIterations = 50;

        [SerializeField, Range(-1, 1)]
        float biasTowardsGrowingTheIslands = 0;

        [SerializeField]
        int minDistanceBetweenStairs = 6;

        void Awake()
        {
            for (int i=0; i<transform.childCount; i++)
            {
                var tile = transform.GetChild(i).GetComponent<Tile>();
                if (tile != null)
                {
                    tiles.Add(tile);
                    tile.gameObject.SetActive(false);
                }
            }
        }

        void Start()
        {
            Generate();
        }

        void Generate()
        {
            GenerateBlank();
            var islands = GenerateWallIsands();
            var growthIterations = Random.Range(minWallGrowthIterations, maxWallGrowthIterations);
            int islandGrowthIterations =  Mathf.RoundToInt(growthIterations * Mathf.Clamp01(Random.value + biasTowardsGrowingTheIslands));
            GrowWallIslands(islands, islandGrowthIterations);
            GrowWalls(growthIterations - islandGrowthIterations);
            JoinWalkableAreas();
            AddStairs();
            if (OnRoomGeneration != null)
                OnRoomGeneration(this, GetData());
        }

        RoomData GetData()
        {
            var data = new RoomData();
            data.biasTowardsGrowingIslands = biasTowardsGrowingTheIslands;
            data.height = height;
            data.width = width;
            data.wallIslands = new Range(minWallIslands, maxWallIslands);
            data.wallGrowthIterations = new Range(minWallGrowthIterations, maxWallGrowthIterations);
            data.minDistanceBetweenStairs = minDistanceBetweenStairs;
            data.tileTypeMap = tileTypeMap;
            
            return data;
        }

        void GenerateBlank()
        {
            tileTypeMap = new int[width * height];
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                if (tiles.Count == i)
                    tiles.Add(NewTile());
                else
                    tiles[i].gameObject.SetActive(true);

                var typeOfTile = IndexOnPerimeter(i) ? TileType.Wall : TileType.None;
                tiles[i].transform.localPosition = GetTileLocalPosition(i);
                SetTileType(i, typeOfTile);
            }

            for (int i = tileTypeMap.Length, l = tiles.Count; i < l; i++)
                tiles[i].gameObject.SetActive(false);
        }

        int[] GenerateWallIsands()
        {
            var undecidedTiles = GetTileIndicesWithType(TileType.None);
            var islandsToCreate = Random.Range(minWallIslands, maxWallIslands);
            var islands = new int[Mathf.Max(islandsToCreate, 0)];

            while (islandsToCreate > 0)
            {
                var tileIndex = undecidedTiles[Random.Range(0, undecidedTiles.Count)];
                undecidedTiles.Remove(tileIndex);
                SetTileType(tileIndex, TileType.Wall);
                islands[islandsToCreate - 1] = tileIndex;
                islandsToCreate--;
            }

            return islands;
        }

        void GrowWallIslands(int[] islands, int iterations)
        {
            var islandList = new List<int>(islands);
            while (iterations > 0)
            {
                var borderingUndecided = GetNeighbourIndices(islandList, TileType.None);
                if (borderingUndecided.Count == 0) 
                    return;

                var newIsland = borderingUndecided[Random.Range(0, borderingUndecided.Count)];
                islandList.Add(newIsland);
                SetTileType(newIsland, TileType.Wall);
                iterations--;
            }
        }

        void GrowWalls(int iterations)
        {
            var walls = GetTileIndicesWithType(TileType.Wall);
            GrowWallIslands(walls.ToArray(), iterations);
        }

        void JoinWalkableAreas()
        {
            var undecided = GetTileIndicesWithType(TileType.None);
            bool firstIteration = true;
            int iterations = 0;

            while (HasAnyOfType(TileType.None))
            {
                if (firstIteration)
                {
                    var floodOrigin = undecided[Random.Range(0, undecided.Count)];
                    FloodFillAs(floodOrigin, TileType.None, TileType.Walkable);
                    firstIteration = false;
                }

                if (!HasAnyOfType(TileType.None))
                    break;

                //Mapping the through wall distance
                EatWallFromFirstFound(GetTilesBorderingWalls(), TileType.None);

                iterations++;
                if (iterations > width * height)
                    return;
            }
        }

        void AddStairs()
        {
            var downStairs = Tower.StairsDownNextToGenerate(width, height);
            List<int> upPositions;
            if (downStairs >= 0)
            {
                SetTileType(downStairs, TileType.StairsDown);
                if (GetNeighbourIndices(downStairs, TileType.Walkable).Count == 0)
                    EatWallFromFirstFound(new List<int>(new int[] { downStairs }), TileType.Walkable);
                upPositions = GetPositionsAtDistance(downStairs, minDistanceBetweenStairs, TileType.Wall, true);

            }
            else
                upPositions = GetNonCornerPerimiterPositions();

            if (upPositions.Count == 0)
            {
                Debug.LogError("No valid up stairs position");
            }

            var upStairs = upPositions[Random.Range(0, upPositions.Count)];
            SetTileType(upStairs, TileType.StairsUp);
            if (GetNeighbourIndices(upStairs, TileType.Walkable).Count == 0)
                EatWallFromFirstFound(new List<int>(new int[] { upStairs }), TileType.Walkable);

        }

        void EatWallFromFirstFound(List<int> edge, TileType searchType)
        {            
            var distanceToEdge = new int[tileTypeMap.Length];
            int distance = 0;
            int wall = -1;

            //Find nearest connectable area
            while (true)
            {
                distance++;
                edge = GetNonPerimeterNeighbourIndices(edge, TileType.Wall, distanceToEdge, 0);
                for (int i = 0, l = edge.Count; i < l; i++)
                {
                    distanceToEdge[edge[i]] = distance;
                }

                var bordersToUndecieded = GetNonPerimeterTilesThatBorderToType(edge, searchType);
                if (bordersToUndecieded.Count > 0)
                {
                    wall = bordersToUndecieded[Random.Range(0, bordersToUndecieded.Count)];
                    break;
                }
                if (distance > width + height)
                    break;

            }

            //Digging a connection between walkable areas
            while (wall >= 0)
            {
                SetTileType(wall, TileType.None);
                distance--;

                if (distance <= 0)
                    break;
                var neighbours = GetNeighbourIndices(wall, TileType.Wall, distanceToEdge, distance);
                if (neighbours.Count > 0)
                    wall = neighbours[Random.Range(0, neighbours.Count)];
            }

            if (wall > 0)
                FloodFillAs(wall, TileType.None, TileType.Walkable);
        }

        void FloodFillAs(int floodOrigin, TileType selector, TileType fillType)
        {
            var fillingIndex = 0;
            var filling = new List<int>();
            filling.Add(floodOrigin);

            while (fillingIndex < filling.Count)
            {
                SetTileType(filling[fillingIndex], TileType.Walkable);
                var neighbourUndecided = GetNeighbourIndices(filling[fillingIndex], TileType.None);
                SetTileType(neighbourUndecided, TileType.Walkable);
                filling.AddRange(neighbourUndecided);
                fillingIndex++;
            }
        }

        Tile NewTile()
        {
            var GO = Instantiate(tileModel);
            GO.transform.SetParent(transform);
            return GO.GetComponent<Tile>();
        }

        void SetTileType(int index, TileType type)
        {
            tileTypeMap[index] = (int)type;
            tiles[index].tileType = type;
        }

        void SetTileType(List<int> indices, TileType type)
        {
            for (int i = 0, l = indices.Count; i < l; i++)
                SetTileType(indices[i], type);
        }

        int CoordinateToIndex(Coordinate coord)
        {
            return coord.y * width + coord.x;
        }

        int CoordinateToIndex(int x, int y)
        {
            return y * width + x;
        }


        List<int> GetNonCornerPerimiterPositions()
        {
            var perimeter = new List<int>();

            for (int i=0; i< tileTypeMap.Length; i++)
            {
                if (CoordinateOnNonCornerPerimeter(PositionToCoordinate(i, width), width, height))
                    perimeter.Add(i);
            }

            return perimeter;
        }

        static public bool CoordinateOnNonCornerPerimeter(Coordinate coord, int width, int height)
        {
            return CoordinateOnPerimeter(coord, width, height) && !CoordinateOnCorner(coord, width, height);
        }

        static public bool CoordinateOnCorner(Coordinate coord, int width, int height)
        {
            return coord.x == 0 && (coord.y == 0 || coord.y == height - 1) || coord.x == width - 1 && (coord.y == 0 || coord.y == height -1);
        }

        static public bool CoordinateOnPerimeter(Coordinate coord, int width, int height)
        {
            return coord.x == 0 || coord.y == 0 || coord.x == width - 1 || coord.y == height - 1;
        }

        bool IndexOnPerimeter(int index)
        {
            var y = index / width;
            if (y == 0 || y == height - 1)
                return true;

    
            var x = index % width;
            return x == 0 || x == width - 1;
        }

        List<int> GetPositionsAtDistance(int origin, int distance, TileType typeOfTile, bool requireStairsPosition)
        {
            var matchingPositions = new List<int>();
            for (int i=0; i<tileTypeMap.Length; i++)
            {
                if (tileTypeMap[i] == (int)typeOfTile && (!requireStairsPosition || CoordinateOnNonCornerPerimeter(PositionToCoordinate(i, width), width, height))
                        && GetManhattanDistance(origin, i) >= distance)

                    matchingPositions.Add(i);
            }
            return matchingPositions;
        }

        List<int> GetNeighbourIndices(int index, TileType neighbourType)
        {
            var neighbours = new List<int>();
            
            var x = index % width;
            var y = index / width;
            var typeInt = (int)neighbourType;

            if (x > 0) {
                var neighbourIndex = CoordinateToIndex(x - 1, y);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);
            }
            if (x < width - 1) {
                var neighbourIndex = CoordinateToIndex(x + 1, y);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);
            }
            if (y > 0)
            {
                var neighbourIndex = CoordinateToIndex(x, y - 1);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);

            }
            if (y < height - 1)
            {
                var neighbourIndex = CoordinateToIndex(x, y + 1);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);

            }

            return neighbours;
        }

        List<int> GetNeighbourIndices(int index, TileType neighbourType, int[] selector, int selectionValue)
        {
            var neighbours = new List<int>();
            var tileNeighbours = GetNeighbourIndices(index, neighbourType);
            for (int i = 0, l = tileNeighbours.Count; i < l; i++)
            {
                if (selector[tileNeighbours[i]] == selectionValue)
                    neighbours.Add(tileNeighbours[i]);
            }

            return neighbours;
        }

        List<int> GetNeighbourIndices(List<int> indices, TileType neighbourType)
        {
            var neighbours = new HashSet<int>();
            for (int i=0, j=indices.Count; i<j; i++)
            {
                var tileNeighbours = GetNeighbourIndices(indices[i], neighbourType);
                for (int k=0, l=tileNeighbours.Count; k<l; k++)
                    neighbours.Add(tileNeighbours[k]);
                    
            }

            return new List<int>(neighbours);
        }

        List<int> GetNonPerimeterNeighbourIndices(List<int> indices, TileType neighbourType, int[] selector, int selectionValue)
        {
            var neighbours = new HashSet<int>();
            for (int i = 0, j = indices.Count; i < j; i++)
            {
                var tileNeighbours = GetNeighbourIndices(indices[i], neighbourType);
                for (int k = 0, l = tileNeighbours.Count; k < l; k++)
                {
                    if (selector[tileNeighbours[k]] == selectionValue && !IndexOnPerimeter(tileNeighbours[k]))
                        neighbours.Add(tileNeighbours[k]);
                }

            }

            return new List<int>(neighbours);
        }

        List<int> GetTileIndicesWithType(TileType type)
        {
            var matchingIndices = new List<int>();

            for (int i=0; i<tileTypeMap.Length; i++)
            {
                if (tileTypeMap[i] == (int)type)
                    matchingIndices.Add(i);
            }

            return matchingIndices;
        }

        bool HasAnyOfType(TileType type)
        {
            for (int i = 0; i < tileTypeMap.Length; i++) {
                if (tileTypeMap[i] == (int)type)
                    return true;
            }
            return false;
        }

        List<int> GetNonPerimeterTilesThatBorderToType(List<int> candidates, TileType borderType)
        {
            var borderingTiles = new List<int>();
            for (int i = 0, l = candidates.Count; i < l; i++)
            {
                if (GetNeighbourIndices(candidates[i], borderType).Count > 0 && !IndexOnPerimeter(candidates[i]))
                    borderingTiles.Add(candidates[i]);
            }
            return borderingTiles;
        }

        List<int> GetTilesBorderingWalls()
        {
            var matchingByType = GetTileIndicesWithType(TileType.Walkable);
            var edge = new List<int>();
            for (int i=0,l=matchingByType.Count; i< l; i++)
            {
                var tileNeighbours = GetNeighbourIndices(matchingByType[i], TileType.Wall);
                if (tileNeighbours.Count > 0)
                    edge.Add(matchingByType[i]);
            }
            return edge;
        }

        Vector2 GetTileLocalPosition(int index)
        {
           var coord = GetTilePositionCentered(index, width, height);
           return new Vector2( coord.x * tileSpacing.x, coord.y * tileSpacing.y);
        }

        public int GetManhattanDistance(int A, int B)
        {
            return Mathf.Abs(A % width - B % width) + Mathf.Abs(A / width - B / width);
        }

        public static Vector2 GetTilePositionCentered(int index, int width, int height)
        {
            return new Vector2((index % width) - width / 2.0f, (index / width) - height / 2.0f);
        }
        
        public static Coordinate PositionToCoordinate(int index, int width)
        {
            return new Coordinate(index % width, index / width);
        }

        public static int CoordinateToPosition(Coordinate coord, int width, int height)
        {
            return coord.x + coord.y * width;
        }


        public static int GetFirstOccurance(int[] map, TileType type)
        {
            for (int i=0; i< map.Length; i++)
            {
                if (map[i] == (int)type)
                    return i;
            }
            return -1;
        }

        public static int GetCorrespondingPosition(RoomData data, TileType tileType, int roomWidth, int roomHeight, bool stayOnEdge)
        {
            var pos = GetFirstOccurance(data.tileTypeMap, tileType);
            if (pos < 0)
                return pos;

            return GetCorrespondingPosition(data, pos, roomWidth, roomHeight, stayOnEdge);
        }

        public static int GetCorrespondingPosition(RoomData data, int position, int roomWidth, int roomHeight, bool stayOnEdge)
        {
            var coord = PositionToCoordinate(position, data.width);
            var midRef = new Coordinate(data.width / 2f, data.height / 2f);
            var midNew = new Coordinate(roomWidth / 2f, roomHeight / 2f);

            if (stayOnEdge && (coord.x == 0 | coord.x == data.width - 1))
                coord.x = coord.x == 0 ? 0 : coord.x = roomWidth;
            else
                coord.x = coord.x - midRef.x + midNew.x;

            if (stayOnEdge && (coord.y == 0 | coord.y == data.height - 1))
                coord.y = coord.y == 0 ? 0 : coord.y = roomHeight;
            else
                coord.y = coord.y - midRef.y + midNew.y;

            if (stayOnEdge)
            {
                //Todo: bad logic
                coord.x = Mathf.Clamp(coord.x, 0, roomWidth - 1);
                coord.y = Mathf.Clamp(coord.y, 0, roomHeight - 1);
            }

            if (coord.x < 0 || coord.y < 0 || coord.x >= roomWidth || coord.y >= roomHeight)
                return -1;

            return CoordinateToPosition(coord, roomWidth, roomHeight);
        }

#if UNITY_EDITOR

        void OnGUI() {
            if (GUI.Button(new Rect(20, 10, 80, 30), "Next Lvl"))
                Generate();
        }

#endif
    }

}
