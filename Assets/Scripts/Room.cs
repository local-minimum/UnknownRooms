using UnityEngine;
using System.Collections.Generic;
using System.Collections;


namespace ProcRoom
{
    public delegate void RoomEvent(Room room, RoomData data);

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
        public int spikeTrapClusters;
        public Range spikeTrapClusterSize;

    }

    [RequireComponent(typeof(Trapper))]
    public class Room : MonoBehaviour
    {

        public static event RoomEvent OnRoomGeneration;

        [SerializeField, Tooltip("Size includes wall perimeter"), Range(3, 30)]
        int width;

        [SerializeField, Tooltip("Size includes wall perimeter"), Range(3, 30)]
        int height;

        [SerializeField]
        bool runOnRealTime = false;

        [SerializeField, Range(0, 2)]
        float interWaveDelay = 1;

        [SerializeField, Range(0, 0.1f)]
        float waveSpeed = 0.04f;

        int[] tileTypeMap;
        int[] distanceMap;
        int highestDistance = -1;
        int wavePeak = 0;
        int waveLength = 3;
        TileType waveSource = TileType.StairsUp;

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

        [SerializeField]
        Range spikeTrapClusters;
        int _spikeTrapClusters;

        [SerializeField]
        Range spikeTrapClusterSize;        

        bool roomGenerating = true;
        
        [SerializeField, Range(0, 2)]
        float roomStartDelay = 1f;

        public bool isGenerating
        {
            get { return roomGenerating; }
        }

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
 
            if (runOnRealTime)
                StartCoroutine(InfiniWave());
        }

        public void Generate()
        {
            Debug.Log("Generating room");
            roomGenerating = true;
            GenerateBlank();
            var islands = GenerateWallIsands();
            var growthIterations = Random.Range(minWallGrowthIterations, maxWallGrowthIterations);
            int islandGrowthIterations =  Mathf.RoundToInt(growthIterations * Mathf.Clamp01(Random.value + biasTowardsGrowingTheIslands));
            GrowWallIslands(islands, islandGrowthIterations);
            GrowWalls(growthIterations - islandGrowthIterations);
            JoinWalkableAreas();
            AddStairs();
            RigTraps();
            if (Tower.ActiveLevel > 1)
                RigDoorAndTreasure();            
            
            StartCoroutine(Enact());
            if (OnRoomGeneration != null)
                OnRoomGeneration(this, GetData());
                
            StartCoroutine(delayRoomStart()); 
            Debug.Log("Room generated");
        }

        IEnumerator<WaitForSeconds> delayRoomStart()
        {
            yield return new WaitForSeconds(roomStartDelay);
            roomGenerating = false;
        }

        public RoomData GetData()
        {
            var data = new RoomData();
            data.biasTowardsGrowingIslands = biasTowardsGrowingTheIslands;
            data.height = height;
            data.width = width;
            data.wallIslands = new Range(minWallIslands, maxWallIslands);
            data.wallGrowthIterations = new Range(minWallGrowthIterations, maxWallGrowthIterations);
            data.minDistanceBetweenStairs = minDistanceBetweenStairs;
            data.tileTypeMap = tileTypeMap;
            data.spikeTrapClusters = spikeTrapClusters.RandomValue;
            data.spikeTrapClusterSize = spikeTrapClusterSize;
            return data;
        }

        public bool HasDoor
        {
            get
            {
                return RoomSearch.HasAnyOfType(TileType.Door, tileTypeMap);
                
            }
        }

        public int Width
        {
            get
            {
                return width;
            }
        }

        public int Height
        {
            get
            {
                return height;
            }
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

                var typeOfTile = RoomMath.IndexOnPerimeter(i, width, height) ? TileType.Wall : TileType.None;
                tiles[i].transform.localPosition = GetTileLocalPosition(i);
                tiles[i].position = Coordinate.FromPosition(i, width);
                SetTileType(i, typeOfTile);

            }

            for (int i = tileTypeMap.Length, l = tiles.Count; i < l; i++)
                tiles[i].gameObject.SetActive(false);
        }

        int[] GenerateWallIsands()
        {
            var undecidedTiles = RoomSearch.GetTileIndicesWithType(TileType.None, tileTypeMap);
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
            var walls = RoomSearch.GetTileIndicesWithType(TileType.Wall, tileTypeMap);
            GrowWallIslands(walls.ToArray(), iterations);
        }

        void JoinWalkableAreas()
        {
            var undecided = RoomSearch.GetTileIndicesWithType(TileType.None, tileTypeMap);
            bool firstIteration = true;
            int iterations = 0;

            while (RoomSearch.HasAnyOfType(TileType.None, tileTypeMap))
            {
                if (firstIteration)
                {
                    var floodOrigin = undecided[Random.Range(0, undecided.Count)];
                    FloodFillAs(floodOrigin, TileType.None, TileType.Walkable);
                    firstIteration = false;
                }

                if (!RoomSearch.HasAnyOfType(TileType.None, tileTypeMap))
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
                if (GetNeighbourIndices(downStairs, TileType.Walkable, tileTypeMap, width).Count == 0)
                    EatWallFromFirstFound(new List<int>(new int[] { downStairs }), TileType.Walkable);
                upPositions = RoomSearch.GetPositionsAtDistance(tileTypeMap, downStairs, new Range(minDistanceBetweenStairs), TileType.Wall, true, width);

            }
            else
                upPositions = RoomSearch.GetNonCornerPerimiterPositions(width, height);

            if (upPositions.Count == 0)
            {
                Debug.LogError("No valid up stairs position");
            }

            var upStairs = upPositions[Random.Range(0, upPositions.Count)];
            SetTileType(upStairs, TileType.StairsUp);
            if (GetNeighbourIndices(upStairs, TileType.Walkable, tileTypeMap, width).Count == 0)
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

                var bordersToUndecieded = RoomSearch.GetNonPerimeterTilesThatBorderToType(tileTypeMap, edge, searchType, width);
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

            var filling = RoomSearch.FloodSearch(tileTypeMap, width, floodOrigin, selector);

            for (int i=0, l=filling.Count; i<l; i++)
            {
                SetTileType(filling[i], fillType);
            }
        }

        Tile NewTile()
        {
            var GO = Instantiate(tileModel);
            GO.transform.SetParent(transform);
            return GO.GetComponent<Tile>();
        }

        void RigDoorAndTreasure()
        {
            var candidates = RoomSearch.GetTileIndicesWithType(TileType.Walkable, tileTypeMap).ToArray();
            if (candidates.Length > 1)
                candidates = ShuffleArray<int>.Shuffle(candidates);

            var upStairs = Coordinate.FromPosition(RoomSearch.GetFirstOccurance(tileTypeMap, TileType.StairsUp), width);
            var downStairs = Coordinate.FromPosition(RoomSearch.GetFirstOccurance(tileTypeMap, TileType.StairsDown), width);

            for (int i=0; i<candidates.Length; i++)
            {
                var coord = Coordinate.FromPosition(candidates[i], width);
                if (PermissableDoorPosition(coord))
                {
                    var path = RoomSearch.FindShortestPath(this, upStairs, downStairs);
                    if (path.Length > 0 && !System.Array.Exists<Coordinate>(path, e => e == coord)) {
                        SetTileType(coord, TileType.Door);
                        Coordinate deadEnd;
                        if (CountPathsToTargets(coord, out deadEnd, upStairs, downStairs) == 1)
                        { 
                            if (deadEnd != coord)
                                PlaceCoins(deadEnd);
                            return;
                        }
                            
                    }
                }
            }
            
        }

        void PlaceCoins(Coordinate deadEnd) {
            var treasurePositions = RoomSearch.FloodSearch(tileTypeMap, width, deadEnd.ToPosition(width, height), TileType.Walkable, TileType.SpikeTrap);
            TreasureChest.Spawn(treasurePositions, width);
        }

        int CountPathsToTargets(Coordinate origin, out Coordinate lastDeadEnd, params Coordinate[] targets)
        {
            int paths = 0;
            lastDeadEnd = origin;
            foreach (var neighbour in origin.Neighbours())
            {
                if (!PassableTile(neighbour, false, TileType.Walkable, TileType.SpikeTrap))
                    continue;
                for (int i = 0; i < targets.Length; i++)
                {
                    var pathsBefore = paths;
                    if (RoomSearch.FindShortestPath(this, neighbour, targets[i], false, TileType.SpikeTrap, TileType.Walkable).Length > 1)
                    {
                        paths++;
                        break;
                    }
                    if (pathsBefore == paths)
                        lastDeadEnd = neighbour; 
                }
            }
            return paths;
        }

        bool PermissableDoorPosition(Coordinate coord)
        {
            bool up = PassableTileAccordingToDoor(coord.UpSide());
            bool vertical = up == PassableTileAccordingToDoor(coord.DownSide());
            bool left = PassableTileAccordingToDoor(coord.LeftSide());
            
            bool val = vertical && up != left && left == PassableTileAccordingToDoor(coord.RightSide());
            if (val)
                Debug.Log(string.Format("{0} UP {1} DOWN {2} -> {3} LEFT {4} RIGHT {5} -> {6} --> DoorPosition",
                    coord, up, PassableTileAccordingToDoor(coord.DownSide()), vertical, left, PassableTileAccordingToDoor(coord.RightSide()), left == PassableTileAccordingToDoor(coord.RightSide())));
            return val;
        }

        void RigTraps()
        {
            _spikeTrapClusters = spikeTrapClusters.RandomValue;
            Debug.Log("Laying spike clusters: " + _spikeTrapClusters);
            for (int i = 0; i < _spikeTrapClusters; i++)
                Trapper.LaySpikeTraps(this, this.GetData(), spikeTrapClusterSize.RandomValue);
        }

        public void SetTileType(Coordinate coordinate, TileType type) {
            SetTileType(coordinate.ToPosition(width, height), type);
        }

        public void SetTileType(int index, TileType type)
        {
            tileTypeMap[index] = (int)type;
            tiles[index].tileType = type;
        }

        public void SetTileType(List<int> indices, TileType type)
        {
            for (int i = 0, l = indices.Count; i < l; i++)
                SetTileType(indices[i], type);
        }

        List<int> GetNeighbourIndices(int index, TileType neighbourType, int[] selector, int selectionValue)
        {
            var neighbours = new List<int>();
            var tileNeighbours = RoomSearch.GetNeighbourIndices(tileTypeMap, width, index, neighbourType);
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
                var tileNeighbours = RoomSearch.GetNeighbourIndices(tileTypeMap, width, indices[i], neighbourType);
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
                var tileNeighbours = RoomSearch.GetNeighbourIndices(tileTypeMap, width, indices[i], neighbourType);
                for (int k = 0, l = tileNeighbours.Count; k < l; k++)
                {
                    if (selector[tileNeighbours[k]] == selectionValue && !RoomMath.IndexOnPerimeter(tileNeighbours[k], width, height))
                        neighbours.Add(tileNeighbours[k]);
                }

            }

            return new List<int>(neighbours);
        }

        List<int> GetTilesBorderingWalls()
        {
            var matchingByType = RoomSearch.GetTileIndicesWithType(TileType.Walkable, tileTypeMap);
            var edge = new List<int>();
            for (int i=0,l=matchingByType.Count; i< l; i++)
            {
                var tileNeighbours = RoomSearch.GetNeighbourIndices(tileTypeMap, width, matchingByType[i], TileType.Wall);
                if (tileNeighbours.Count > 0)
                    edge.Add(matchingByType[i]);
            }
            return edge;
        }

        Vector2 GetTileLocalPosition(int index)
        {
           var coord = RoomMath.GetTilePositionCentered(index, width, height);
           return new Vector2( coord.x * tileSpacing.x, coord.y * tileSpacing.y);
        }

        public Vector3 GetTileCentre(Coordinate coordinate)
        {
            return GetTileCentre(coordinate.ToPosition(width, height));
        }

        public Vector3 GetTileCentre(int position)
        {
            return transform.TransformPoint(GetTileLocalPosition(position));
        }

        public TileType GetTileTypeAt(Coordinate position)
        {
            if (RoomMath.CoordinateOnMap(position, width, height))
                return (TileType) tileTypeMap[position.ToPosition(width, height)];
            return TileType.None;
        }

        public Tile GetTile(Coordinate position)
        {
            return tiles[position.ToPosition(width, height)];
        }

        public bool PassableTileAccordingToDoor(Coordinate position)
        {
            return PassableTile(position, false, TileType.Walkable, TileType.StairsDown, TileType.SpikeTrap, TileType.StairsUp);
        }

        public bool PassableTile(Coordinate position)
        {
            return PassableTile(position, true, TileType.Walkable, TileType.SpikeTrap);
        }

        public bool PassableTile(Coordinate position, bool checkAgents, params TileType[] permissibles)
        {
            if (!position.Inside(width, height))
                return false;

            var typeAtPos = GetTileTypeAt(position);
            if (!System.Array.Exists<TileType>(permissibles, e => e == typeAtPos))
                return false;

            if (!checkAgents)
                return true;

            for (int i = 0, l = Tower.Agents; i < l; i++) {
                if (Tower.GetAgentPosition(i) == position)
                    return false;
            }
            return true;
        }

        public Coordinate GetRandomFreeTileCoordinate(Coordinate referencePosition, int minDistance)
        {
            var distances = RoomSearch.GetDistanceMap(this, referencePosition);

            //Count all valid positions and set them to value 1
            var validPositions = 0;
            var invalidDistance = Height + Width;
            for (int x=0, lX = distances.GetLength(0); x<lX;x++)
            {
                for (int y=0, lY = distances.GetLength(1); y<lY;y++)
                {
                    if (distances[x, y] >= minDistance && distances[x, y] < invalidDistance)
                    {
                        distances[x, y] = 1;
                        validPositions++;
                    }
                    else
                        distances[x, y] = 0;
                }
            }

            //Select position based on count and iterate until found it
            int selectedPosition = Random.Range(0, validPositions);
            for (int x = 0, lX = distances.GetLength(0); x < lX; x++)
            {
                for (int y = 0, lY = distances.GetLength(1); y < lY; y++)
                {
                    if (distances[x, y] == 1)
                    {
                        if (selectedPosition < 1)
                            return new Coordinate(x, y);
                        else
                            selectedPosition--;
                    }
                }
            }
            Debug.LogWarning("No valid position found");
            return Coordinate.InvalidPlacement;
        }

        void SetDistanceMap(int source)
        {
            distanceMap = new int[tileTypeMap.Length];
            highestDistance = -1;
            for (int i=0; i<distanceMap.Length; i++)
            {
                distanceMap[i] = RoomMath.GetManhattanDistance(i, source, width);
                highestDistance = Mathf.Max(highestDistance, distanceMap[i]);
            }
            
        }

        public IEnumerator Enact()
        {
            if (runOnRealTime)
            {
                Tower.RoomEnactDone();
                yield break;
            }

            NewWaveSource();

            yield return StartCoroutine(PropagateWave());

            wavePeak++;
            Tower.RoomEnactDone();
        }

        IEnumerator InfiniWave()
        {
            while (true) {
                if (!isGenerating)
                {

                    if (wavePeak > highestDistance)
                        NewWaveSource();

                    yield return StartCoroutine(PropagateWave());
                    wavePeak++;
                }
                yield return new WaitForSeconds(interWaveDelay);
            }
        }


        void NewWaveSource()
        {
            waveSource = waveSource == TileType.StairsUp ? (RoomSearch.HasAnyOfType(TileType.StairsDown, tileTypeMap) ? TileType.StairsDown : TileType.StairsUp) : TileType.StairsUp;
            SetDistanceMap(RoomSearch.GetFirstOccurance(tileTypeMap, waveSource));
            wavePeak = 0;
        }

        IEnumerator<WaitForSeconds> PropagateWave()
        {

            for (int i = 0; i <= highestDistance; i++)
            {
                var cycleStep = Mathf.Abs(wavePeak - i) % waveLength;
                for (int j = 0; j < distanceMap.Length; j++)
                {
                    if (distanceMap[j] == i)
                        tiles[j].Enact(cycleStep);
                }
                if (i % 3 == 2)
                    yield return new WaitForSeconds(waveSpeed);
            }
        }

    }

}
