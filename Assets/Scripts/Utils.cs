using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace ProcRoom
{
    [System.Serializable]
    public struct Range
    {
        public int min;
        public int max;
        public bool noUpperBound;

        public Range(int min, int max)
        {
            this.min = min;
            this.max = max;
            noUpperBound = false;
        }

        public Range(int min)
        {
            this.min = min;
            this.max = min;
            noUpperBound = true;
        }

        public int RandomValue
        {
            get
            {
                if (!noUpperBound)
                {
                    if (min == max)
                        return min;
                    else
                        return Random.Range(min, max);
                }
                else
                    return -1;
            }
        }
    }
    

    [System.Serializable]
    public struct FloatRange
    {
        [SerializeField, HideInInspector]
        float _min;

        [SerializeField, HideInInspector]
        float _max;

        public float min
        {
            get { return _min; }
            set { _min = Mathf.Min(_max, value); }
        }

        public float max
        {
            get { return _max; }
            set { _max = Mathf.Max(_min, value); }
        }
        public FloatRange(float min, float max)
        {
            _min = min;
            _max = Mathf.Max(min, max);
        }

        public float RandomValue
        {
            get
            {
                if (_min == _max)
                    return _min;
                return Random.Range(_min, _max);
            }

        }

    }

    [System.Serializable]
    public struct Coordinate
    {

        static Coordinate _Right = new Coordinate(1, 0);
        static Coordinate _Left = new Coordinate(-1, 0);
        static Coordinate _Up = new Coordinate(0, 1);
        static Coordinate _Down = new Coordinate(0, -1);

        public static Coordinate InvalidPlacement
        {
            get { return new Coordinate(-1, -1); }
        }

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

        public Coordinate asDirection
        {
            get
            {
                if (Mathf.Abs(x) > Mathf.Abs(y))
                    return new Coordinate(Mathf.Sign(x), 0);
                else
                    return new Coordinate(0, Mathf.Sign(y));
            }
        }

        public override string ToString()
        {
            return string.Format("<{0},{1}>", x, y);
        }

        public static Coordinate FromPosition(int index, int width)
        {
            return new Coordinate(index % width, index / width);
        }

        public static int CalculateIndexValue(int x, int y, int with)
        {
            return x + y * with;
        }

        public static Coordinate operator -(Coordinate A, Coordinate B)
        {
            return new Coordinate(A.x - B.x, A.y - B.y);
        }

        public static Coordinate operator +(Coordinate A, Coordinate B)
        {
            return new Coordinate(A.x + B.x, A.y + B.y);
        }

        public static bool operator ==(Coordinate A, Coordinate B)
        {
            return object.ReferenceEquals(A, B) || A.Equals(B);
        }

        public static bool operator !=(Coordinate A, Coordinate B)
        {
            return !object.ReferenceEquals(A, B) || !A.Equals(B);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Coordinate))
                return false;

            Coordinate other = (Coordinate) obj;

            return this.x == other.x && this.y == other.y;

        }

        public static Coordinate Right {
            get
            {
                return _Right;
            }
        }

        public static Coordinate Left
        {
            get
            {
                return _Left;

            }
        }

        public static Coordinate Up
        {
            get
            {
                return _Up;
            }
        }

        public static Coordinate Down
        {
            get
            {
                return _Down;
            }
        }
    }

    public static class UtilExtensions
    {
        public static int ToPosition(this Coordinate coord, int width, int height)
        {
            return coord.x + coord.y * width;
        }

        public static Coordinate Rotated90CCW(this Coordinate coord)
        {
            return new Coordinate(-coord.y, coord.x);
        }

        public static Coordinate  Rotated90CW(this Coordinate coord)
        {
            return new Coordinate(coord.y, -coord.x);
        }

        public static Coordinate Rotated180(this Coordinate coord)
        {
            return new Coordinate(-coord.y, -coord.x);
        }

        public static Coordinate LeftSide(this Coordinate coord)
        {
            return new Coordinate(coord.x - 1, coord.y);
        }

        public static Coordinate RightSide(this Coordinate coord)
        {
            return new Coordinate(coord.x + 1, coord.y);
        }

        public static Coordinate UpSide(this Coordinate coord)
        {
            return new Coordinate(coord.x, coord.y + 1);
        }

        public static Coordinate DownSide(this Coordinate coord)
        {
            return new Coordinate(coord.x, coord.y - 1);
        }

        public static IEnumerable<Coordinate> Neighbours(this Coordinate coord)
        {
            yield return coord.RightSide();
            yield return coord.UpSide();
            yield return coord.LeftSide();
            yield return coord.DownSide();
        }

        public static bool Inside(this Coordinate coord, int width, int height)
        {
            return coord.x >= 0 && coord.y >= 0 && coord.x < width && coord.y < height;
        }

        public static bool Inside(this Coordinate coord, int[,] map2D)
        {
            return coord.Inside(map2D.GetLength(0), map2D.GetLength(1));
        }

        public static bool Inside(this Range range, int value)
        {
            return range.min <= value && (range.noUpperBound || value <= range.max);
        }

    }

    public static class ShuffleArray<T>
    {
        static System.Random _random = new System.Random();

        public static T[] Shuffle(T[] array)
        {
            List<KeyValuePair<int, T>> list = new List<KeyValuePair<int, T>>();
            foreach (var element in array)
                list.Add(new KeyValuePair<int, T>(_random.Next(), element));

            var sorted = from item in list orderby item.Key select item;

            T[] result = new T[array.Length];
            int index = 0;
            foreach(KeyValuePair<int, T> pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }

            return result;
        }

    }

    public static class RoomMath {

        static public bool CoordinateOnMap(Coordinate coord, int width, int height)
        {
            return coord.x >= 0 && coord.x < width && coord.y >= 0 && coord.y < height;
        }

        static public bool CoordinateOnNonCornerPerimeter(Coordinate coord, int width, int height)
        {
            return CoordinateOnPerimeter(coord, width, height) && !CoordinateOnCorner(coord, width, height);
        }

        static public bool CoordinateOnCorner(Coordinate coord, int width, int height)
        {
            return coord.x == 0 && (coord.y == 0 || coord.y == height - 1) || coord.x == width - 1 && (coord.y == 0 || coord.y == height - 1);
        }

        static public bool CoordinateOnPerimeter(Coordinate coord, int width, int height)
        {
            return coord.x == 0 || coord.y == 0 || coord.x == width - 1 || coord.y == height - 1;
        }

        public static bool IndexOnPerimeter(int index, int width, int height)
        {
            var y = index / width;
            if (y == 0 || y == height - 1)
                return true;


            var x = index % width;
            return x == 0 || x == width - 1;
        }

        public static Vector2 GetTilePositionCentered(int index, int width, int height)
        {
            return new Vector2((index % width) - width / 2.0f, (index / width) - height / 2.0f);
        }

        public static int GetCorrespondingPosition(RoomData data, TileType tileType, int roomWidth, int roomHeight, bool stayOnEdge)
        {
            var pos = RoomSearch.GetFirstOccurance(data.tileTypeMap, tileType);
            if (pos < 0)
                return pos;

            return GetCorrespondingPosition(data, pos, roomWidth, roomHeight, stayOnEdge);
        }

        public static int GetManhattanDistance(int A, int B, int width)
        {
            return Mathf.Abs(A % width - B % width) + Mathf.Abs(A / width - B / width);
        }

        public static int GetManhattanDistance(Coordinate A, Coordinate B)
        {
            return Mathf.Abs(A.x - B.x) + Mathf.Abs(A.y - B.y);
        }

        public static int GetCorrespondingPosition(RoomData data, int position, int roomWidth, int roomHeight, bool stayOnEdge)
        {
            var coord = Coordinate.FromPosition(position, data.width);
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

            return coord.ToPosition(roomWidth, roomHeight);
        }

        public static bool IsInLookDirection(Coordinate source, Coordinate target, Coordinate lookDirection)
        {
            if (source.x == target.x)
                return Mathf.Sign(target.y - source.y) == lookDirection.y;
            else if (source.y == target.y)
                return Mathf.Sign(target.x - source.x) == lookDirection.x;
            else
                return false;
        }

    }

    public static class RoomSearch {

        public static int GetFirstOccurance(int[] map, TileType type)
        {
            for (int i = 0; i < map.Length; i++)
            {
                if (map[i] == (int)type)
                    return i;
            }
            return -1;
        }

        public static bool HasAnyOfType(TileType type, int[] tileTypeMap)
        {
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                if (tileTypeMap[i] == (int)type)
                    return true;
            }
            return false;
        }

        public static List<int> GetTileIndicesWithType(TileType type, int[] tileTypeMap)
        {
            var matchingIndices = new List<int>();

            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                if (tileTypeMap[i] == (int)type)
                    matchingIndices.Add(i);
            }

            return matchingIndices;
        }

        public static List<int> FloodSearch(int[] tileTypeMap, int width, int source, params TileType[] selectors)
        {
            var fillingIndex = 0;
            var filling = new List<int>();
            filling.Add(source);

            while (fillingIndex < filling.Count)
            {
                var neighbourUndecided = RoomSearch.GetNeighbourIndices(tileTypeMap, width, filling[fillingIndex], selectors);
                for (int i=0, l=neighbourUndecided.Count; i< l; i++)
                {
                    if (!filling.Contains(neighbourUndecided[i]))
                        filling.AddRange(neighbourUndecided);
                }
                fillingIndex++;

            }
            return filling;
        }

        public static List<int> GetNeighbourIndices(int[] tileTypeMap, int width, int index, params TileType[] neighbourTypes)
        {
            var neighbours = new List<int>();

            var x = index % width;
            var y = index / width;
            var height = tileTypeMap.Length / width;

            if (x > 0)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x - 1, y, width);
                if (System.Array.Exists(neighbourTypes, t => (int) t == tileTypeMap[neighbourIndex]))
                    neighbours.Add(neighbourIndex);
            }
            if (x < width - 1)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x + 1, y, width);
                if (System.Array.Exists(neighbourTypes, t => (int)t == tileTypeMap[neighbourIndex]))
                    neighbours.Add(neighbourIndex);
            }
            if (y > 0)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x, y - 1, width);
                if (System.Array.Exists(neighbourTypes, t => (int)t == tileTypeMap[neighbourIndex]))
                    neighbours.Add(neighbourIndex);

            }
            if (y < height - 1)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x, y + 1, width);
                if (System.Array.Exists(neighbourTypes, t => (int)t == tileTypeMap[neighbourIndex]))
                    neighbours.Add(neighbourIndex);

            }

            return neighbours;
        }

        public static List<int> GetNonCornerPerimiterPositions(int width, int height)
        {
            var perimeter = new List<int>();

            for (int i = 0, l = width * height; i < l; i++)
            {
                if (RoomMath.CoordinateOnNonCornerPerimeter(Coordinate.FromPosition(i, width), width, height))
                    perimeter.Add(i);
            }

            return perimeter;
        }

        public static List<int> GetPositionsAtDistance(int[] tileTypeMap, int origin, Range permissableDistance, TileType typeOfTile, bool requireStairsPosition, int width)
        {
            var matchingPositions = new List<int>();
            int height = tileTypeMap.Length / width;
            for (int i = 0; i < tileTypeMap.Length; i++)
            {
                if (tileTypeMap[i] == (int)typeOfTile && (!requireStairsPosition || RoomMath.CoordinateOnNonCornerPerimeter(Coordinate.FromPosition(i, width), width, height))
                        && permissableDistance.Inside(RoomMath.GetManhattanDistance(origin, i, width)))

                    matchingPositions.Add(i);
            }
            return matchingPositions;
        }


        public static List<int> GetNonPerimeterTilesThatBorderToType(int[] tileTypeMap, List<int> candidates, TileType borderType, int width)
        {
            var borderingTiles = new List<int>();
            int height = tileTypeMap.Length / width;
            for (int i = 0, l = candidates.Count; i < l; i++)
            {
                if (GetNeighbourIndices(tileTypeMap, width, candidates[i], borderType).Count > 0 && !RoomMath.IndexOnPerimeter(candidates[i], width, height))
                    borderingTiles.Add(candidates[i]);
            }
            return borderingTiles;
        }

        static int[,] GetDistanceMapInit(int width, int height)
        {
            int[,] distances = new int[width, height];
            var maxDistance = width + height;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                    distances[x, y] = maxDistance;
            }
            return distances;
        }

        static Coordinate[] PathFromDistanceMap(int[,] map, Coordinate pathPosition)
        {
            var path = new Coordinate[map[pathPosition.x, pathPosition.y]];
            //Debug.Log("Valid path at " + path.Length);
            var neighbours = new List<Coordinate>();
            for (int index=path.Length - 1; index >=0; index--)
            {
                path[index] = pathPosition;

                foreach (Coordinate neighbour in pathPosition.Neighbours())
                {
                    if (neighbour.Inside(map) && map[neighbour.x, neighbour.y] == map[pathPosition.x, pathPosition.y] -1 )
                        neighbours.Add(neighbour);
                    /*else
                        Debug.Log(string.Format("{0},{1} has value {2} should be {3}", 
                            neighbour.x,
                            neighbour.y,
                            neighbour.Inside(map) ? map[neighbour.x, neighbour.y].ToString() : "outside map", 
                            map[pathPosition.x, pathPosition.y] - 1));*/
                }
                if (neighbours.Count > 0)
                    pathPosition = neighbours[Random.Range(0, neighbours.Count)];
                else
                {
                    Debug.LogWarning(string.Format("Error in path no valid step from {0}, {1}, value {2}", pathPosition.x, pathPosition.y, map[pathPosition.x, pathPosition.y]));
                    return new Coordinate[0];
                }
                neighbours.Clear();
            }
            return path;
        }

        public static int[,] GetDistanceMap(Room room, Coordinate source)
        {
            return GetDistanceMap(room, source, true, TileType.Walkable, TileType.SpikeTrap);
        }

        public static int[,] GetDistanceMap(Room room, Coordinate source, bool regardAgents, params TileType[] passables)
        {
            int height = room.Height;
            int width = room.Width;

            int[,] distances = GetDistanceMapInit(width, height);
            distances[source.x, source.y] = 0;

            var queue = new Queue<Coordinate>();
            queue.Enqueue(source);
            
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var nextDist = distances[current.x, current.y] + 1;

                foreach (Coordinate neighbour in current.Neighbours())
                {
                    
                    if (room.PassableTile(neighbour, regardAgents, passables) && distances[neighbour.x, neighbour.y] > nextDist)
                    {
                        distances[neighbour.x, neighbour.y] = nextDist;
                        if (!queue.Contains(neighbour) && neighbour.Inside(width, height))
                            queue.Enqueue(neighbour);
                    }

                }
            }

            return distances;
        }

        public static Coordinate[] FindShortestPath(Room room, Coordinate source, Coordinate target)
        {
            return FindShortestPath(room, source, target, true, TileType.Walkable, TileType.SpikeTrap);
        }

        public static Coordinate[] FindShortestPath(Room room, Coordinate source, Coordinate target, bool regardAgents, params TileType[] passables)
        {
            if (source == Coordinate.InvalidPlacement || target == Coordinate.InvalidPlacement)
                return new Coordinate[0];

            var distances = GetDistanceMap(room, source, regardAgents,passables);
            foreach (Coordinate neighbour in target.Neighbours())
            {
                if (neighbour.Inside(distances))
                    distances[target.x, target.y] = Mathf.Min(distances[neighbour.x, neighbour.y] + 1, distances[target.x, target.y]);                    
            }
            
            return PathFromDistanceMap(distances, target);
        }

        public static bool IsClearStraightPath(Room room, Coordinate source, Coordinate target)
        {
            var offset = (target - source).asDirection;
            //Debug.Log(string.Format("{0} -> {1} using {2}", source, target, offset));
            while (true)
            {
                source += offset;
                if (source == target)
                    return true;
                else if (!source.Inside(room.Width, room.Height))
                    return false;
                else if (!room.PassableTile(source))
                    return false;
                
            }
            
            
        }
    }
}