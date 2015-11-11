using UnityEngine;
using System.Collections.Generic;


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
    }

    [System.Serializable]
    public struct Coordinate
    {

        static Coordinate _Right = new Coordinate(1, 0);
        static Coordinate _Left = new Coordinate(-1, 0);
        static Coordinate _Up = new Coordinate(0, 1);
        static Coordinate _Down = new Coordinate(0, -1);

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

        public static Coordinate Rotate90CCW(this Coordinate coord)
        {
            return new Coordinate(-coord.y, coord.x);
        }

        public static Coordinate  Rotate90CW(this Coordinate coord)
        {
            return new Coordinate(coord.y, -coord.x);
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

        public static bool Inside(this Range range, int value)
        {
            return range.min <= value && (range.noUpperBound || value <= range.max);
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


        public static List<int> GetNeighbourIndices(int index, TileType neighbourType, int[] tileTypeMap, int width)
        {
            var neighbours = new List<int>();

            var x = index % width;
            var y = index / width;
            var height = tileTypeMap.Length / width;
            var typeInt = (int)neighbourType;

            if (x > 0)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x - 1, y, width);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);
            }
            if (x < width - 1)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x + 1, y, width);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);
            }
            if (y > 0)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x, y - 1, width);
                if (tileTypeMap[neighbourIndex] == typeInt)
                    neighbours.Add(neighbourIndex);

            }
            if (y < height - 1)
            {
                var neighbourIndex = Coordinate.CalculateIndexValue(x, y + 1, width);
                if (tileTypeMap[neighbourIndex] == typeInt)
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
                if (GetNeighbourIndices(candidates[i], borderType, tileTypeMap, width).Count > 0 && !RoomMath.IndexOnPerimeter(candidates[i], width, height))
                    borderingTiles.Add(candidates[i]);
            }
            return borderingTiles;
        }
    }
}