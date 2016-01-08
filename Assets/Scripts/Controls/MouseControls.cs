using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{

    public class MouseControls : MonoBehaviour
    {
        struct MouseAction
        {
            public enum ActionType { Move, Shoot, Reload};

            public ActionType type;
            public Coordinate destination;

            public MouseAction(ActionType type, Coordinate destination)
            {
                this.type = type;
                this.destination = destination;
            }

            public MouseAction(Coordinate destination)
            {
                type = ActionType.Move;
                this.destination = destination;
            }

        }


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

        bool allowSubmitActions = false;

        List<MouseAction> actions = new List<MouseAction>();
        bool processingActions = false;

        [SerializeField, Range(0, 2)]
        float actionDuration;

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

        //TODO: Make unified controls interface
        public static bool controlEnabled
        {
            get
            {
                    return _instance != null && _instance.enabled;
            }

            set
            {
                    _instance.enabled = value;
            }
            
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
            if (agent != Tower.Player || lastTile == null || path.Length == 0)
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
                }
                UpdateTrail(path);
            }
        }

        private void Tile_OnTileHover(Tile tile, MouseEvent type)
        {
            if (processingActions)
            {
                return;
            }

            if (type == MouseEvent.Exit)
            {
                allowSubmitActions = false;
            } else if (Tower.Player.myTurn && tile != lastTile) 
            {
                if (tile.position == Tower.Player.position)
                {
                    lastTile = tile;
                    UpdateTrail(new Coordinate[0]);
                    if (!Tower.Player.ammoIsFull)
                        actions.Add(new MouseAction(MouseAction.ActionType.Reload, tile.position));
                }
                else
                {
                    var room = Tower.ActiveRoom;
                    var tileType = room.GetTileTypeAt(tile.position);
                    if (tileType == TileType.None || tileType == TileType.Wall)
                        return;
                    shootAim = room.HasAgent(tile.position);
                    lastTile = tile;
                    FindPath();
                }
                allowSubmitActions = true;
            }
        }

        void FindPath()
        {
            //TODO: Compare new and old path and smart join if possible
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
            if (!processingActions)
                actions.Clear();
            var room = Tower.ActiveRoom;
            int weaponsRange = Tower.Player.ammo > 0 ? Tower.Player.Weapon.range : -1;
            bool restIsShot = false;
            int ap = Tower.Player.actionPoints;
            for (int i=0, l=trail.Count; i< l; i++)
            {
                bool enabledSprite = i < path.Length;
                if (enabledSprite)
                {

                    trail[i].transform.position = room.GetTileCentre(path[i]);
                    if (truncateAtActionpoints)
                    {
                        bool inRange = weaponsRange > 0 && path.Length <= weaponsRange + i + 1;

                        if (shootAim && (restIsShot || inRange && ClearLineOfSight(i) && ap > 0))
                        {
                            if (i == 0 && DirectShot() && weaponsRange >= path.Length)
                            {
                                
                                trail[i].sprite = shootScan;
                                ap --;
                                if (!processingActions)
                                {
                                    actions.Add(new MouseAction(MouseAction.ActionType.Shoot, path[path.Length - 1].asDirection));
                                }
                                restIsShot = true;
                            }
                            else if (restIsShot)
                            {
                                trail[i].sprite = shootScan;
                            } else if (ap > 1)
                            {
                                trail[i].sprite = pathScan;
                                ap -=2;
                                if (!processingActions)
                                {
                                    actions.Add(new MouseAction(path[i]));
                                    actions.Add(new MouseAction(MouseAction.ActionType.Shoot, path[path.Length - 1]));
                                }
                                restIsShot = true;
                            }
                            
                        } else if (shootAim == true && !inRange && i == path.Length - 1)
                        {
                            enabledSprite = false;
                        }
                        else
                        {
                            trail[i].sprite = pathScan;
                            if (!processingActions && ap > 0)
                            {
                                actions.Add(new MouseAction(path[i]));
                            }
                            ap--;
                        }
                        trail[i].color = ap >= 0 ? (Color32)Color.white : truncationColor;
                    }
                    else
                    {
                        trail[i].sprite = shootAim ? shootScan : pathScan;
                        trail[i].color = Color.white;
                    }

                }
                trail[i].enabled = enabledSprite;
            }
        }

        bool DirectShot()
        {
            if (path.Length == 1)
                return true;
            var aim = path[1] - path[0];
            return aim.asDirection.Rotated180() + path[0] == Tower.Player.position;
        }

        bool ClearLineOfSight(int index)
        {
            var room = Tower.ActiveRoom;
            var aim = (path[path.Length - 1] - path[index]).asDirection;
            var pos = path[index];
            for (int i=index+1; i<path.Length; i++)
            {
                pos += aim;
                if ((path[i] != pos) || !room.PassableTile(path[i], false, TileType.Walkable, TileType.SpikeTrap))
                {
                    return false;
                }
            }
            return true;
        }

        void Update()
        {
            if (lastTile != null)
            {
                if (!Tower.Player.myTurn)
                {
                    lastTile = null;
                    UpdateTrail(new Coordinate[0]);
                } else if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    if (processingActions)
                    {
                        processingActions = false;
                    }
                    else if (allowSubmitActions)
                    {
                        StartCoroutine(SubmitActions());
                    }

                }
            }
                
        }

        IEnumerator<WaitForSeconds> SubmitActions()
        {
            processingActions = true;
            allowSubmitActions = false;
            int i = 0;
            while (processingActions)
            {
                var action = actions[i];

                if (action.type == MouseAction.ActionType.Reload)
                {
                    Tower.Player.Reload();
                    break;
                }
                else
                {

                    var aim = Tower.Player.lookDirection;
                    var actionAim = (action.destination - Tower.Player.position).asDirection;

                    if (aim != actionAim)
                    {
                        Tower.Player.lookDirection = actionAim;
                    }
                    else
                    {
                        if (action.type == MouseAction.ActionType.Move)
                        {
                            Tower.Player.MoveTo(action.destination);
                        }
                        else if (action.type == MouseAction.ActionType.Shoot)
                        {
                            Tower.Player.Attack();
                        }
                        i++;
                        if (i >= actions.Count)
                            break;
                    }
                }
                yield return new WaitForSeconds(actionDuration);
            }
            actions.Clear();
            processingActions = false;
        }

    }
}