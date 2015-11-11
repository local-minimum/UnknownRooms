using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProcRoom
{
    [System.Serializable]
    public class AgentStats
    {
        public int actionPointsPerTurn = 3;
        public int actionPoints = -1;

        public int maxHealth = 7;
        public int health;

        public int maxAmmo = 11;
        public int ammo = 0;

        public int evasion = 10;

        public Coordinate position;
        public Coordinate lookDirection = Coordinate.Right;

        public bool hasAmmo
        {
            get
            {
                return maxAmmo < 1 || ammo > 0;
            }
        }
    }

    public abstract class Agent : MonoBehaviour
    {

        [SerializeField]
        protected AgentStats _stats;


        [SerializeField]
        protected Weapon weapon;

        protected int roomWidth;
        protected int roomHeight;

        protected int shots = 0;

        protected Room room;

        Animator anim;

        [SerializeField, Range(0, 2)]
        float moveSpeed = 0.3f;

        float lastMove;

        protected bool actionTick
        {
            get
            {
                if (Time.timeSinceLevelLoad - lastMove > moveSpeed)
                {
                    lastMove = Time.timeSinceLevelLoad;
                    return true;
                }
                return false;
            }
        }

        public Coordinate position
        {
            get
            {
                return _stats.position;
            }
        }

        public Coordinate lookDirection
        {
            get
            {
                return _stats.lookDirection;
            }

            set
            {
                if ((value.x ^ value.y) == 0 || Mathf.Abs(value.x * value.y) > 1)
                {
                    Debug.LogWarning(string.Format("Attempting invalid look direction x: {0} y: {1}", value.x, value.y));
                    Debug.Log((value.x ^ value.y));
                    Debug.Log(Mathf.Abs(value.x * value.y));
                }
                else
                {
                    _stats.lookDirection = value;
                    if (anim)
                    {
                        if (Coordinate.Right.Equals(value))
                            anim.SetTrigger("Right");
                        else if (Coordinate.Left.Equals(value))
                            anim.SetTrigger("Left");
                        else if (Coordinate.Down.Equals(value))
                            anim.SetTrigger("Down");
                        else if (Coordinate.Up.Equals(value))
                            anim.SetTrigger("Up");
                    } else
                    {
                        if (Coordinate.Right.Equals(value))
                            transform.rotation = Quaternion.Euler(0, 0, 0);
                        else if (Coordinate.Left.Equals(value))
                            transform.rotation = Quaternion.Euler(0, 0, 180);
                        else if (Coordinate.Up.Equals(value))
                            transform.rotation = Quaternion.Euler(0, 0, 90);
                        else if (Coordinate.Down.Equals(value))
                            transform.rotation = Quaternion.Euler(0, 0, 270);
                    }
                }
            }
        }

        public int actionPoints
        {
            get
            {
                return _stats.actionPoints;
            }

            protected set
            {
                _stats.actionPoints = Mathf.Clamp(value, 0, _stats.actionPointsPerTurn);
                if (_stats.actionPoints == 0)
                    EndTurn();
            }
        }

        protected bool myTurn
        {
            get
            {
                return !weapon.isShooting && actionPoints > 0;
            }
        }


        public bool alive
        {
            get
            {
                return _stats.health > 0;
            }

        }

        void OnEnable()
        {
            if (anim == null)
                anim = GetComponent<Animator>();
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

        protected virtual void HandleNewRoom(Room room, RoomData data)
        {
            roomWidth = data.width;
            roomHeight = data.height;
            this.room = room;
        
        }

        virtual public void Enact() {
            actionPoints = _stats.actionPointsPerTurn;
        }

        protected virtual void HandleTileAction(Tile tile, TileType typeOfTile, Coordinate position)
        {
            if (position.Equals(_stats.position) && typeOfTile == TileType.SpikeTrap)
            {
                tile.Maim();
                Hurt();
            }
        }

        protected virtual void HandleProjectileHit(Projectile projectile, Coordinate position)
        {
            if (position.Equals(_stats.position))
                Hurt();
        }

        public void Hurt()
        {
            _stats.health--;

            if (_stats.health < 1)
            {
                actionPoints = 0;
                Death();
            }
        }

        public void EndTurn()
        {
            Debug.Log(name + " turn ended.");
            _stats.actionPoints = 0;
            Tower.AgentDone(this);
        }

        abstract protected void Death();

        protected void Attack()
        {
            if (_stats.hasAmmo && weapon.Shoot(_stats.position, _stats.lookDirection))
            {
                _stats.ammo--;
                shots++;
                actionPoints--;
            }
        }

        protected void UpdatePosition(Coordinate newPosition)
        {
            _stats.position = newPosition;
            transform.position = room.GetTileCentre(_stats.position.ToPosition(roomWidth, roomHeight));
        }

#if UNITY_EDITOR

        string LookDirectionText
        {
            get
            {
                switch (_stats.lookDirection.x + _stats.lookDirection.y * 2)
                {
                    case -2:
                        return "Down";
                    case 2:
                        return "Up";
                    case -1:
                        return "Left";
                    case 1:
                        return "Right";
                    default:
                        return "Unknown";                        
                }
            }
        }

        void OnGUI()
        {
            if (Selection.activeObject == gameObject)
                GUI.TextArea(new Rect(110, 2, 140, 70), string.Format("Health:\t{0}\nAmmo:\t{1}\nAP:\t{2}\nLookDir\t{3}", _stats.health, _stats.ammo, actionPoints, LookDirectionText));
        }
#endif

    }
}
