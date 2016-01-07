using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public class MouseControls : MonoBehaviour
    {

        [SerializeField]
        Sprite pathScan;

        [SerializeField]
        Sprite shootScan;

        [SerializeField]
        string layer;

        [SerializeField]
        int orderInLayer;

        [SerializeField]
        bool truncateAtActionpoints;

        [SerializeField]
        Color32 truncationColor;

        List<SpriteRenderer> trail = new List<SpriteRenderer>();

        Tile lastTile;

        bool shootAim;

        Coordinate[] path;

        static MouseControls _instance;

        public static MouseControls instance
        {
            get
            {
                if (_instance == null)
                    Spawn();
                return _instance;
            }
        }

        public static void Spawn()
        {
            _instance = FindObjectOfType<MouseControls>();
            if (_instance == null)
                return;

            var GO = new GameObject("Mouse Trail");
            GO.transform.SetParent(Tower.Player.transform.parent);
            _instance = GO.AddComponent<MouseControls>();
        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(this.gameObject);
        }


        void OnEnable()
        {
            Tower.OnNewLevel += Tower_OnNewLevel;
            Tile.OnTileHover += Tile_OnTileHover;
            Agent.OnAgentMove += Agent_OnAgentMove;
        }

        void OnDisable()
        {
            Tower.OnNewLevel -= Tower_OnNewLevel;
            Tile.OnTileHover -= Tile_OnTileHover;
            Agent.OnAgentMove -= Agent_OnAgentMove;
        }

        private void Agent_OnAgentMove(Agent agent)
        {
            if (agent != Tower.Player || lastTile == null)
                return;
            
            if (path[0] == agent.position)
            {
                if (path.Length == 1)
                {
                    path = new Coordinate[0];
                }
                else
                {
                    var newPath = new Coordinate[path.Length - 1];
                    System.Array.Copy(path, 1, newPath, 0, newPath.Length);
                    path = newPath;
                    FindPath();
                }
                UpdateTrail(path);
            }
        }

        private void Tile_OnTileHover(Tile tile)
        {
            if (Tower.Player.myTurn && tile != lastTile && tile.position != Tower.Player.position)
            {
                var room = Tower.ActiveRoom;
                var tileType = room.GetTileTypeAt(tile.position);
                if (tileType == TileType.None || tileType == TileType.Wall)
                    return;
                shootAim = room.HasAgent(tile.position);
                lastTile = tile;
                FindPath();
            }
        }

        void FindPath()
        {
            path = RoomSearch.FindShortestPath(Tower.ActiveRoom, Tower.Player.position, lastTile.position, true, TileType.Door, TileType.Walkable, TileType.StairsUp, TileType.SpikeTrap);
            GrowTrailWhileNeeded(path);
            UpdateTrail(path);
        }

        private void Tower_OnNewLevel(int level)
        {
            UpdateTrail(new Coordinate[0]);
        }

        void GrowTrailWhileNeeded(Coordinate[] path)
        {
            while (trail.Count < path.Length)
            {
                var GO = new GameObject("Trail Step " + trail.Count);
                GO.transform.SetParent(transform);
                var rend = GO.AddComponent<SpriteRenderer>();                
                rend.enabled = false;
                rend.sortingLayerName = layer;
                rend.sortingOrder = orderInLayer;
                trail.Add(rend);
            }
        }

        void UpdateTrail(Coordinate[] path)
        {
            var room = Tower.ActiveRoom;
            for (int i=0, l=trail.Count; i< l; i++)
            {
                bool enabledSprite = i < path.Length;
                if (enabledSprite)
                {
                    trail[i].sprite = shootAim ? shootScan : pathScan;
                    trail[i].transform.position = room.GetTileCentre(path[i]);
                    if (truncateAtActionpoints)
                    {
                        trail[i].color = (shootAim ? true : i < Tower.Player.actionPoints) ? (Color32) Color.white : truncationColor;
                    }
                    else
                        trail[i].color = Color.white;

                }
                trail[i].enabled = enabledSprite;
            }
        }

        void Update()
        {
            if (lastTile != null &&  !Tower.Player.myTurn)
            {
                lastTile = null;
                UpdateTrail(new Coordinate[0]);
            }
                
        }

    }
}