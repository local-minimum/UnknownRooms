using UnityEngine;
using System.Collections.Generic;


namespace ProcRoom
{
    public class Monster : Agent
    {
        Player _player;
        AI.Abilities.Ability[] abilities;

        [HideInInspector]
        public bool trackingPlayer = false;

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

        void Update()
        {
            if (!myTurn)
                return;

            var usables = GetUsableAbilities();

            if (usables.Count == 0)
                EndTurn();
            else if (actionTick)
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
