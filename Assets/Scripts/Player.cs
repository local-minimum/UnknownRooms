using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    public delegate void PlayerEnterPosition(Player player, Coordinate position, TileType tileType);

    [RequireComponent(typeof(Animator))]
    public class Player : MonoBehaviour
    {

        public static event PlayerEnterPosition OnPlayerEnterNewPosition;

        int actionPoints = -1;
        Coordinate position;
        [SerializeField]
        int startHealth = 7;

        [SerializeField]
        int ammmoFull = 11;

        [SerializeField]
        int actionPointsPerTurn = 3;

        [SerializeField]
        int evasion = 10;

        int health;
        int ammo = 0;
        int roomWidth;
        int roomHeight;

        int steps = 0;
        int shots = 0;

        Coordinate lookDirection = Coordinate.Right;

        [SerializeField]
        int minDistanceSpawninfFirstLevel = 8;

        Room room;

        Animator anim;

        bool playerTurn {
            get
            {
                return actionPoints > 0;
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
        }

        void OnDisable()
        {
            Room.OnRoomGeneration -= HandleNewRoom;
            Tile.OnTileAction -= HandleTileAction;
        }

        private void HandleTileAction(Tile tile, TileType typeOfTile, Coordinate position)
        {
            if (position.Equals(this.position) && typeOfTile == TileType.SpikeTrap)
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
                    new Range(minDistanceSpawninfFirstLevel), TileType.Walkable, false, data.width);

                return candidates[Random.Range(0, candidates.Count)];
            }

        }

        void Update()
        {
            if (!playerTurn)
                return;

            if (Input.GetButtonDown("right"))
            {
                if (lookDirection.Equals(Coordinate.Right))
                    AttemptMoveTo(position.RightSide());
                else
                {
                    lookDirection = Coordinate.Right;
                    anim.SetTrigger("Right");
                }
            }
            else if (Input.GetButtonDown("left"))
            {
                if (lookDirection.Equals(Coordinate.Left))
                    AttemptMoveTo(position.LeftSide());
                else
                {
                    lookDirection = Coordinate.Left;
                    anim.SetTrigger("Left");
                }
            }
            else if (Input.GetButtonDown("up"))
            {
                if (lookDirection.Equals(Coordinate.Up))
                    AttemptMoveTo(position.UpSide());
                else
                {
                    lookDirection = Coordinate.Up;
                    anim.SetTrigger("Up");
                }
            }
            else if (Input.GetButtonDown("down"))
            {
                if (lookDirection.Equals(Coordinate.Down))
                    AttemptMoveTo(position.DownSide());
                else
                {
                    lookDirection = Coordinate.Down;
                    anim.SetTrigger("Down");
                }
            }
            else if (Input.GetButton("endTurn"))
                actionPoints = 0;
            else if (Input.GetButton("reload"))
                Reload();

            if (actionPoints < 1)
                EndTurn();
        }


        void Reload()
        {
            ammo = ammmoFull;
            actionPoints--;
        }

        void AttemptMoveTo(Coordinate newPosition)
        {
            var tileType = room.GetTileAt(newPosition);

            if (tileType == TileType.Walkable || tileType == TileType.SpikeTrap || tileType == TileType.StairsUp) { 
                
                UpdatePlayerPosition(newPosition);
                if (OnPlayerEnterNewPosition != null)
                    OnPlayerEnterNewPosition(this, newPosition, tileType);
                actionPoints--;
                steps++;
            }

        }

        void UpdatePlayerPosition(Coordinate newPosition)
        {
            position = newPosition;
            transform.position = room.GetTileCentre(position.ToPosition(roomWidth, roomHeight));
        }

        public void Enact()
        {
            actionPoints = actionPointsPerTurn;

        }

        public void EndTurn()
        {
            actionPoints = 0;
            Tower.PlayerDone();
        }

        public void Hurt()
        {
            health--;
            Debug.Log(health);

            if (health < 1)
            {
                actionPoints = -1;
                Tower.Reset();
            }
        }

        public void NewGame()
        {
            health = startHealth;
            steps = 0;
            shots = 0;
            Reload();
        }

#if UNITY_EDITOR

        void OnGUI() {
            GUI.TextArea(new Rect(110, 10, 100, 50), string.Format("Health:\t{0}\nAmmo:\t{1}\nSteps:\t{2}", health, ammo, steps));
        }
#endif
    }
}