using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    public class Monster : Agent
    {
        Player _player;
        AI.Abilities.Ability[] abilities;

        [HideInInspector]
        bool _trackingPlayer = false;

        bool queuedMove = false;
        Coordinate moveTarget;

        [SerializeField, Range(1, 20)]
        int minSpawnDistanceToPlayer = 7;
       
        public bool trackingPlayer
        {
            get
            {
                return _trackingPlayer;
            }

            set
            {
                if (value) {
                    _playerLastSeenPosition = player.position;
                    _playerLastSeenDirection = player.lookDirection;
                    _hasEverSeenPlayer = true;
                }
                _trackingPlayer = value;
            }
        }

        bool _hasEverSeenPlayer = false;

        public bool hasEverSeenPlayer
        {
            get
            {
                return _hasEverSeenPlayer;
            }
        }
        Coordinate _playerLastSeenPosition;

        public Coordinate playerLastSeenPosition
        {
            get
            {
                return _playerLastSeenPosition;
            }
        }

        Coordinate _playerLastSeenDirection;

        public Coordinate playerLastSeenDirection
        {
            get
            {
                return _playerLastSeenDirection;
            }
        }
        
        public Player player
        {
            get
            {
                return _player;
            }
        }


        public bool PlayerInSight
        {
            get
            {                
                return trackingPlayer && RoomMath.IsInLookDirection(_stats.position, _player.position, _stats.lookDirection);
            }
        }

        public bool canShootPlayer
        {
            get {
                var distOK = weaponRange >= RoomMath.GetManhattanDistance(player.position, position);
                
                //Debug.Log("Within range " + distOK + "( " + weaponRange + " <= " + RoomMath.GetManhattanDistance(player.position, position));
                return distOK && RoomSearch.IsClearStraightPath(room, position, player.position);
            }
        }

        void Awake()
        {
            _player = FindObjectOfType<Player>();
            abilities = GetComponentsInChildren<AI.Abilities.Ability>();
            lookDirection = _stats.lookDirection;
            for (int i = 0; i < abilities.Length; i++)
                abilities[i].enabled = true;
            alive = false;
        }

        
        protected override void Death()
        {
            enabled = false;
            alive = false;
        }

        protected override void HandleNewRoom(Room room, RoomData data)
        {
            queuedMove = false;
            base.HandleNewRoom(room, data);
            alive = false;
            StartCoroutine(delayMonsterSpawn());
        }

        IEnumerator<WaitForSeconds> delayMonsterSpawn()
        {
            yield return new WaitForSeconds(0.5f);
            int minDistance = minSpawnDistanceToPlayer;
            Coordinate pos = Coordinate.InvalidPlacement;
            do
            {
                pos = room.GetRandomFreeTileCoordinate(player.position, minDistance);
                minDistance--;
            } while (pos == Coordinate.InvalidPlacement);
            //Debug.Log(name + string.Format(" will start level at {0},{1}", pos.x, pos.y));
            UpdatePosition(pos);
            alive = true;

        }

        public int weaponRange
        {
            get
            {
                return weapon.range;
            }
        }

        public void RequestMove(Coordinate target)
        {
            if (RoomMath.GetManhattanDistance(target, position) == 1 && target.Inside(roomWidth, roomHeight))
            {
                moveTarget = target;
                queuedMove = true;
            } else
            {
                Debug.LogWarning(name + string.Format(" tried invalid move to {0},{1}", target.x, target.y));
            }
        }

        void Update()
        {
            if (!myTurn || !actionAllowed)
                return;

            if (queuedMove && room.PassableTile(moveTarget))
            {
                UpdatePosition(moveTarget);
                actionPoints--;
                actionTick();
                queuedMove = false;
                return;
            }

            var usables = GetUsableAbilities();

            if (usables.Count == 0)
                EndTurn();
            else
            {
                var ability = SelectAbility(usables);
                if (ability.Enact())
                    actionTick();
            }
        }

        List<AI.Abilities.Ability> GetUsableAbilities()
        {
            var usables = new List<AI.Abilities.Ability>();

            for (int i=0; i<abilities.Length; i++)
            {
                if (abilities[i].Allowed)
                    usables.Add(abilities[i]);
            }
            return usables;
        }

        AI.Abilities.Ability SelectAbility(List<AI.Abilities.Ability> options)
        {
            int totalPrio = 0;
            for (int i = 0, l = options.Count; i < l; i++)
                totalPrio += options[i].Priority;

            int roll = Random.Range(0, totalPrio);
            for (int i = 0, l = options.Count; i< l; i ++)
            {
                if (roll <= options[i].Priority)
                    return options[i];
                else
                    roll -= options[i].Priority;
            }
            return options[Random.Range(0, options.Count)];
        }

        public override void Enact()
        {
            trackingPlayer = false;
            for (int i = 0; i < abilities.Length; i++)
                abilities[i].NewTurn();
            base.Enact();
        }

    }
}
