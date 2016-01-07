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

        List<SpriteRenderer> trail = new List<SpriteRenderer>();

        Tile lastTile;

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
        }

        void OnDisable()
        {
            Tower.OnNewLevel -= Tower_OnNewLevel;
            Tile.OnTileHover -= Tile_OnTileHover;
        }

        private void Tile_OnTileHover(Tile tile)
        {
            if (Tower.Player.myTurn && tile != lastTile && tile.position != Tower.Player.position)
            {
                var room = Tower.ActiveRoom;
                var tileType = room.GetTileTypeAt(tile.position);
                if (tileType == TileType.None || tileType == TileType.Wall)
                    return;
                bool shootAim = room.HasAgent(tile.position);
                lastTile = tile;

                var path = RoomSearch.FindShortestPath(Tower.ActiveRoom, Tower.Player.position, tile.position, false, TileType.Door, TileType.Walkable, TileType.StairsUp, TileType.SpikeTrap);
                GrowTrailWhileNeeded(path);
                UpdateTrail(path, shootAim ? shootScan : pathScan);
            }
        }

        private void Tower_OnNewLevel(int level)
        {
            UpdateTrail(new Coordinate[0], pathScan);
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

        void UpdateTrail(Coordinate[] path, Sprite sprite)
        {
            var room = Tower.ActiveRoom;
            for (int i=0, l=trail.Count; i< l; i++)
            {
                bool enabledSprite = i < path.Length;
                if (enabledSprite)
                {
                    trail[i].sprite = sprite;
                    trail[i].transform.position = room.GetTileCentre(path[i]);
                }
                trail[i].enabled = enabledSprite;
            }
        }

        void Update()
        {
            if (lastTile != null &&  !Tower.Player.myTurn)
            {
                lastTile = null;
                UpdateTrail(new Coordinate[0], pathScan);
            }
                
        }

    }
}