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
        public int spikeTrapClusterSize;

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
        int spikeTrapClusters = 2;

        [SerializeField]
        int spikeTrapClusterSize = 3;

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
            if (runOnRealTime)
                StartCoroutine(InfiniWave());
        }

        public void Generate()
        {
            GenerateBlank();
            var islands = GenerateWallIsands();
            var growthIterations = Random.Range(minWallGrowthIterations, maxWallGrowthIterations);
            int islandGrowthIterations =  Mathf.RoundToInt(growthIterations * Mathf.Clamp01(Random.value + biasTowardsGrowingTheIslands));
            GrowWallIslands(islands, islandGrowthIterations);
            GrowWalls(growthIterations - islandGrowthIterations);
            JoinWalkableAreas();
            AddStairs();
            RigTraps();

            StartCoroutine(Enact());
            if (OnRoomGeneration != null)
                OnRoomGeneration(this, GetData());
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
            data.spikeTrapClusters = spikeTrapClusters;
            data.spikeTrapClusterSize = spikeTrapClusterSize;
            return data;
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
            var fillingIndex = 0;
            var filling = new List<int>();
            filling.Add(floodOrigin);

            while (fillingIndex < filling.Count)
            {
                SetTileType(filling[fillingIndex], TileType.Walkable);
                var neighbourUndecided = RoomSearch.GetNeighbourIndices(filling[fillingIndex], TileType.None, tileTypeMap, width);
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

        void RigTraps()
        {
            for (int i = 0; i < spikeTrapClusters; i++)
                Trapper.LaySpikeTraps(this, this.GetData(), spikeTrapClusterSize);
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
            var tileNeighbours = RoomSearch.GetNeighbourIndices(index, neighbourType, tileTypeMap, width);
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
                var tileNeighbours = RoomSearch.GetNeighbourIndices(indices[i], neighbourType, tileTypeMap, width);
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
                var tileNeighbours = RoomSearch.GetNeighbourIndices(indices[i], neighbourType, tileTypeMap, width);
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
                var tileNeighbours = RoomSearch.GetNeighbourIndices(matchingByType[i], TileType.Wall, tileTypeMap, width);
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

        public bool PassableTile(Coordinate position)
        {
            if (!position.Inside(width, height))
                return false;
            
            var tileType = GetTileTypeAt(position);
            if (!(tileType == TileType.Walkable || tileType == TileType.SpikeTrap))
                return false;
            for (int i = 0, l = Tower.Agents; i < l; i++) {
                if (Tower.GetAgentPosition(i) == position)
                    return false;
            }
            return true;
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
                Tower.RoomDone();
                yield break;
            }

            NewWaveSource();

            yield return StartCoroutine(PropagateWave());

            wavePeak++;
            Tower.RoomDone();
        }

        IEnumerator InfiniWave()
        {
            while (true) {
                if (wavePeak > highestDistance)
                    NewWaveSource();

                yield return StartCoroutine(PropagateWave());
                wavePeak++;
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

#if UNITY_EDITOR

        void OnGUI() {
            if (GUI.Button(new Rect(2, 2, 80, 30), "Next Lvl"))
                Generate();
        }

#endif
    }

}
