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

        void Awake()
        {
            _player = FindObjectOfType<Player>();
            abilities = GetComponentsInChildren<AI.Abilities.Ability>();
            lookDirection = _stats.lookDirection;
            for (int i = 0; i < abilities.Length; i++)
                abilities[i].enabled = true;
        }

        protected override void Death()
        {
            enabled = false;
        }

        protected override void HandleNewRoom(Room room, RoomData data)
        {
            base.HandleNewRoom(room, data);
            UpdatePosition(_stats.position);

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
            }
        }

        void Update()
        {
            if (!myTurn || !actionTick)
                return;

            if (queuedMove)
            {
                UpdatePosition(moveTarget);
                actionPoints--;
                queuedMove = false;
                return;
            }

            var usables = GetUsableAbilities();

            if (usables.Count == 0)
                EndTurn();
            else
            {
                var ability = SelectAbility(usables);
                ability.Enact();
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
            Debug.Log(string.Format("{0} abilities possible", options.Count));
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
