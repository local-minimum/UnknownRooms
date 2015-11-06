using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    public delegate void PlayerEnterPosition(Player player, Coordinate position, TileType tileType);

    [RequireComponent(typeof(Animator))]
    public class Player : Agent
    {

        public static event PlayerEnterPosition OnPlayerEnterNewPosition;

        int steps = 0;
        int shots = 0;

        [SerializeField]
        int minDistanceSpawnInFirstLevel = 8;

        Room room;

        Animator anim;

        [SerializeField]
        Weapon weapon;

        int actionPoints
        {
            get
            {
                return _stats.actionPoints;
            }

            set
            {
                _stats.actionPoints = Mathf.Clamp(value, 0, _stats.actionPointsPerTurn);
                if (_stats.actionPoints == 0)
                    EndTurn();
            }
        }

        bool playerTurn {
            get
            {
                return !weapon.isShooting && actionPoints > 0;
            }
        }

        void Start()
        {
            NewGame();
            anim = GetComponent<Animator>();
        }

        void OnEnable()
        {
            Room.OnRoomGeneration += HandleNewRoom;
            Tile.OnTileAction += HandleTileAction;
            Projectile.OnProjectileHit += HandleProjectileHit;
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleNewRoom;
            Tile.OnTileAction -= HandleTileAction;
            Projectile.OnProjectileHit -= HandleProjectileHit;
        }

        private void HandleTileAction(Tile tile, TileType typeOfTile, Coordinate position)
        {
            if (position.Equals(_stats.position) && typeOfTile == TileType.SpikeTrap)
            {
                tile.Maim();
                Hurt();
            }
        }

        private void HandleNewRoom(Room room, RoomData data)
        {
            roomWidth = data.width;
            roomHeight = data.height;
            this.room = room;
            UpdatePlayerPosition(Coordinate.FromPosition(PlayerSpawnPosition(data), roomWidth));
        }

        private void HandleProjectileHit(Projectile projectile, Coordinate position)
        {
            if (position.Equals(_stats.position))
                Hurt();
        }

        int PlayerSpawnPosition(RoomData data)
        {
            if (RoomSearch.HasAnyOfType(TileType.StairsDown, data.tileTypeMap))
            {
                return RoomSearch.GetFirstOccurance(data.tileTypeMap, TileType.StairsDown);
            }
            else
            {

                var candidates = RoomSearch.GetPositionsAtDistance(data.tileTypeMap,
                    RoomSearch.GetFirstOccurance(data.tileTypeMap, TileType.StairsUp),
                    new Range(minDistanceSpawnInFirstLevel), TileType.Walkable, false, data.width);

                return candidates[Random.Range(0, candidates.Count)];
            }

        }

        void Update()
        {
            if (!playerTurn)
                return;

            if (Input.GetButtonDown("right"))
            {
                if (_stats.lookDirection.Equals(Coordinate.Right))
                    AttemptMoveTo(_stats.position.RightSide());
                else
                {
                    _stats.lookDirection = Coordinate.Right;
                    anim.SetTrigger("Right");
                }
            }
            else if (Input.GetButtonDown("left"))
            {
                if (_stats.lookDirection.Equals(Coordinate.Left))
                    AttemptMoveTo(_stats.position.LeftSide());
                else
                {
                    _stats.lookDirection = Coordinate.Left;
                    anim.SetTrigger("Left");
                }
            }
            else if (Input.GetButtonDown("up"))
            {
                if (_stats.lookDirection.Equals(Coordinate.Up))
                    AttemptMoveTo(_stats.position.UpSide());
                else
                {
                    _stats.lookDirection = Coordinate.Up;
                    anim.SetTrigger("Up");
                }
            }
            else if (Input.GetButtonDown("down"))
            {
                if (_stats.lookDirection.Equals(Coordinate.Down))
                    AttemptMoveTo(_stats.position.DownSide());
                else
                {
                    _stats.lookDirection = Coordinate.Down;
                    anim.SetTrigger("Down");
                }
            }
            else if (Input.GetButton("endTurn"))
                actionPoints = 0;
            else if (Input.GetButton("reload"))
                Reload();
            else if (Input.GetButton("shoot"))
                Shoot();

        }


        void Reload()
        {
            _stats.ammo = _stats.maxAmmo;
            actionPoints--;
        }

        void AttemptMoveTo(Coordinate newPosition)
        {
            var tileType = room.GetTileTypeAt(newPosition);

            if (tileType == TileType.Walkable || tileType == TileType.SpikeTrap || tileType == TileType.StairsUp) { 
                
                UpdatePlayerPosition(newPosition);
                if (OnPlayerEnterNewPosition != null)
                    OnPlayerEnterNewPosition(this, newPosition, tileType);
                actionPoints--;
                steps++;
            }

        }

        void Shoot() {
            if (_stats.ammo > 0 && weapon.Shoot(_stats.position, _stats.lookDirection)) {
                _stats.ammo--;
                shots++;
                actionPoints--;
            }
        }

        void UpdatePlayerPosition(Coordinate newPosition)
        {
            _stats.position = newPosition;
            transform.position = room.GetTileCentre(_stats.position.ToPosition(roomWidth, roomHeight));
        }

        public override void Enact()
        {
            actionPoints = _stats.actionPointsPerTurn;

        }

        public void EndTurn()
        {
            _stats.actionPoints = 0;
            Tower.PlayerDone();
        }

        public void Hurt()
        {
            _stats.health--;

            if (_stats.health < 1)
            {
                actionPoints = 0;
                Tower.Reset();
            }
        }

        public void NewGame()
        {
            _stats.health = _stats.maxHealth;
            steps = 0;
            shots = 0;
            _stats.ammo = _stats.maxAmmo;
        }

#if UNITY_EDITOR

        void OnGUI() {
            GUI.TextArea(new Rect(110, 10, 100, 50), string.Format("Health:\t{0}\nAmmo:\t{1}\nSteps:\t{2}", _stats.health, _stats.ammo, steps));
        }
#endif
    }
}