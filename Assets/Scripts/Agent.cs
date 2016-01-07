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

        public string name;
        public int actionPointsPerTurn = 3;
        public int actionPoints = -1;

        public int maxHealth = 7;
        public int health;

        public int clipSize = 11;
        public int ammo = 0;

        public int xp = 0;

        public int defence = 10;

        public bool hasKey = false;

        public Coordinate position;
        public Coordinate lookDirection = Coordinate.Right;

        public bool hasAmmo
        {
            get
            {
                return ammo > 0;
            }
        }
    }

    public delegate void AgentActions(int actionPoints);
    public delegate void AgentAmmo(int remainingAmmo);
    public delegate void AgentHealth(int health);
    public delegate void AgentUpgrade(AgentStats stats);
    public delegate void AgentDeath(Agent agent);
    public delegate void AgentKeyChange(bool hasKey);
    public delegate void AgentXPChange(int xp);
    public delegate void AgentMove(Agent agent);

    public abstract class Agent : MonoBehaviour
    {

        [SerializeField]
        protected AgentStats _stats;

        public static event AgentDeath OnAgentDeath;
        public static event AgentMove OnAgentMove;

        public event AgentActions OnAgentActionChange;
        public event AgentAmmo OnAgentAmmoChange;
        public event AgentHealth OnAgentHealthChange;
        public event AgentUpgrade OnAgentUpgrade;
        public event AgentKeyChange OnAgentHasKeyChange;
        public event AgentXPChange OnAgentXPChange;

        [SerializeField]
        protected Weapon weapon;

        protected int roomWidth;
        protected int roomHeight;

        protected int shots = 0;

        protected Room room;

        public bool rotateAsDirection = false;

        Animator anim;

        [SerializeField, Range(0, 2)]
        float actionSpeed = 0.3f;

        float lastAction;

        public void SetStats(AgentStats stats)
        {
            _stats = stats;
            if (OnAgentUpgrade != null)
                OnAgentUpgrade(stats);
        }

        public AgentStats stats
        {
            get
            {
                return _stats;
            }
        }

        public Weapon Weapon {
            get {
                return weapon;
            }
        }

        protected bool actionTick()
        {
            if (actionAllowed)
            {
                lastAction = Time.timeSinceLevelLoad;
                return true;
            }
            return false;
        }

        protected bool actionAllowed
        {
            get { return Time.timeSinceLevelLoad - lastAction > actionSpeed; }
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
                    } else if (rotateAsDirection)
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

            set
            {
                var actionPoints = Mathf.Clamp(value, 0, _stats.actionPointsPerTurn);
                if (_stats.actionPoints != actionPoints)
                {
                    _stats.actionPoints = actionPoints;
                    if (OnAgentActionChange != null)
                        OnAgentActionChange(_stats.actionPoints);
                }
                if (_stats.actionPoints == 0)
                    Tower.AgentDone(this);
            }
        }

        public int ammo
        {
            get
            {
                return _stats.ammo;
            }

            protected set
            {
                var ammo = Mathf.Clamp(value, 0, _stats.clipSize);
                if (_stats.ammo != ammo) {
                    _stats.ammo = ammo;
                    if (OnAgentAmmoChange != null)
                        OnAgentAmmoChange(_stats.ammo);
                }
            }
        }

        public int health
        {
            get
            {
                return _stats.health;
            }

            set
            {
                var health = Mathf.Clamp(value, 0, _stats.maxHealth);
                if (_stats.health != health)
                {
                    _stats.health = health;
                    if (OnAgentHealthChange != null)
                        OnAgentHealthChange(_stats.health);

                    if (_stats.health == 0 && Tower.playingRoom && OnAgentDeath != null)
                        OnAgentDeath(this);
                }
            }
        }        

        public bool myTurn
        {
            get
            {
                return room != null && !room.isGenerating && !weapon.isShooting && actionPoints > 0;
            }
        }


        public bool alive
        {
            get
            {
                return _stats.health > 0;
            }

            set
            {
                
                health = value ? _stats.maxHealth : 0;
                ammo = value ? _stats.clipSize : 0;
                
                if (anim)
                {
                    anim.enabled = value;
                }
                foreach (var rend in GetComponentsInChildren<SpriteRenderer>())
                    rend.enabled = value;
            }

        }

        public void AwardKey()
        {
            _stats.hasKey = true;
            if (OnAgentHasKeyChange != null)
                OnAgentHasKeyChange(true);
        }

        public bool ConsumeKey()
        {
            if (_stats.hasKey)
            {
                actionTick();
                _stats.hasKey = false;
                if (OnAgentHasKeyChange != null)
                    OnAgentHasKeyChange(false);
                return true;
            }
            return false;
        }

        public void AwardPoints(int points) {
            _stats.xp += Mathf.Max(0, points);
            if (OnAgentXPChange != null)
                OnAgentXPChange(_stats.xp);
        }

        public virtual void Reload()
        {
            if (!actionAllowed)
                return;
            ammo = _stats.clipSize;
            actionPoints--;
            actionTick();
        }

        virtual protected void OnEnable()
        {
            if (anim == null)
                anim = GetComponent<Animator>();
            Room.OnRoomGeneration += HandleNewRoom;
            Tile.OnTileAction += HandleTileAction;
            Projectile.OnProjectileHit += HandleProjectileHit;
        }

        virtual protected void OnDisable()
        {
            Room.OnRoomGeneration -= HandleNewRoom;
            Tile.OnTileAction -= HandleTileAction;
            Projectile.OnProjectileHit -= HandleProjectileHit;
        }

        protected virtual void HandleNewRoom(Room room, RoomData data)
        {
            roomWidth = data.width;
            roomHeight = data.height;
            if (this == Tower.Player)
                ammo = _stats.clipSize;
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
                MoveOutOfTurn();
            }
        }

        protected virtual void MoveOutOfTurn()
        {

        }

        protected virtual void HandleProjectileHit(Projectile projectile, Coordinate position)
        {
            if (position.Equals(_stats.position))
            {
                var power = projectile.power;
                while (power > 0)
                {
                    if (power > _stats.defence && alive)
                    {
                        UI.Hurt.Place(_stats.position);
                        Hurt();
                    }
                    power -= 80 + Random.Range(0, 100);
                }
            }
        }

        public void Hurt()
        {
            health--;
             
            if (_stats.health < 1)
            {
                if (myTurn)
                    actionPoints = 0;
                Death();
            }
        }

        abstract protected void Death();

        protected void Attack()
        {
            if (actionAllowed && _stats.hasAmmo && weapon.Shoot(_stats.position, _stats.lookDirection))
            {
                ammo--;
                shots++;
                actionPoints--;
                actionTick();
            }
        }

        protected void UpdatePosition(Coordinate newPosition)
        {
            _stats.position = newPosition;
            transform.position = room.GetTileCentre(_stats.position.ToPosition(roomWidth, roomHeight));
            if (OnAgentMove != null)
                OnAgentMove(this);
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
            if (Selection.activeObject == gameObject || gameObject.transform.parent == Selection.activeObject)
            {
                GUI.TextArea(new Rect(110, 2, 140, 70), string.Format("Health:\t{0}\nAmmo:\t{1}\nAP:\t{2}\nLookDir\t{3}", _stats.health, _stats.ammo, actionPoints, LookDirectionText));
                if (alive && GUI.Button(new Rect(2, 33, 80, 30), "Hurt")) {
                    Hurt();
                }

            }
        }
#endif

    }
}
