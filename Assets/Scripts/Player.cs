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

        [SerializeField]
        int minDistanceSpawnInFirstLevel = 8;

        void Start()
        {
            NewGame();
        
        }

        protected override void HandleNewRoom(Room room, RoomData data)
        {
            base.HandleNewRoom(room, data);
            UpdatePosition(Coordinate.FromPosition(PlayerSpawnPosition(data), roomWidth));
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
            if (!myTurn)            
                return;


            if (Input.GetButton("right") && actionTick)
            {
                if (_stats.lookDirection.Equals(Coordinate.Right))
                    AttemptMoveTo(_stats.position.RightSide());
                else
                {
                    lookDirection = Coordinate.Right;

                }
            }
            else if (Input.GetButton("left") && actionTick)
            {
                if (_stats.lookDirection.Equals(Coordinate.Left))
                    AttemptMoveTo(_stats.position.LeftSide());
                else
                {
                    lookDirection = Coordinate.Left;

                }
            }
            else if (Input.GetButton("up") && actionTick)
            {
                if (_stats.lookDirection.Equals(Coordinate.Up))
                    AttemptMoveTo(_stats.position.UpSide());
                else
                {
                    lookDirection = Coordinate.Up;
                }
            }
            else if (Input.GetButton("down") && actionTick)
            {
                if (_stats.lookDirection.Equals(Coordinate.Down))
                    AttemptMoveTo(_stats.position.DownSide());
                else
                {
                    lookDirection = Coordinate.Down;
                }
            }
            else if (Input.GetButtonDown("endTurn"))
                EndTurn();
            else if (Input.GetButtonDown("reload"))
                Reload();
            else if (Input.GetButton("shoot"))
                Attack();

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
                
                UpdatePosition(newPosition);
                if (OnPlayerEnterNewPosition != null)
                    OnPlayerEnterNewPosition(this, newPosition, tileType);
                if (tileType == TileType.StairsUp)
                    EndTurn();
                else
                    actionPoints--;
                steps++;
            }

        }

        public void NewGame()
        {
            _stats.health = _stats.maxHealth;
            steps = 0;
            shots = 0;
            _stats.ammo = _stats.maxAmmo;
        }

        protected override void Death()
        {
            Tower.Reset();
        }

    }
}