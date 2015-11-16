using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom.Physical
{
    public class WeaponSmith : MonoBehaviour
    {
        [SerializeField]
        Ability WeaponsRange;

        [SerializeField]
        Ability WeaponsPrecision;

        [SerializeField]
        Ability WeaponsPower;

        Dictionary<Ability, int> blankState
        {
            get
            {
                var abilityState = new Dictionary<Ability, int>();

                abilityState[WeaponsPower] = 0;
                abilityState[WeaponsPrecision] = 0;
                abilityState[WeaponsRange] = 0;

                return abilityState;
            }
        }


        static WeaponSmith _instance;

        static WeaponSmith instance
        {

            get
            {
                if (_instance == null)
                    FindObjectOfType<WeaponSmith>();

                return _instance;
            }
        }

        public static WeaponStats Smith(int points)
        {
            Debug.Log("Smitting a weapon with " + points + " points");
            var abilityStates = instance.blankState;
            while (Upgrade(ref abilityStates, ref points)) ;
            return instance.createWeapon(abilityStates);
        }

        static bool Upgrade(ref Dictionary<Ability, int> state, ref int points)
        {
            var availables = new Ability[state.Count];
            int totalAvailable = 0;
            foreach (KeyValuePair<Ability, int> kvp in state)
            {
                var cost = kvp.Key.Length > kvp.Value + 1 ? kvp.Key[kvp.Value + 1].cost : -1;
                if (cost >= 0 && cost <= points)
                {
                    availables[totalAvailable] = kvp.Key;
                    totalAvailable++;
                }
            }
            if (totalAvailable == 0)
                return false;

            var upgradeAbility = availables[Random.Range(0, totalAvailable)];
            state[upgradeAbility]++;
            points -= upgradeAbility[state[upgradeAbility]].cost;
            return true;
        }

        WeaponStats createWeapon(Dictionary<Ability, int> state)
        {
            var stats = new WeaponStats();
            stats.maxRange = WeaponsRange[state[WeaponsRange]].value;
            stats.accuracyLossPerTile = WeaponsPrecision[state[WeaponsPrecision]].value;
            stats.attack = WeaponsPower[state[WeaponsPower]].value;
            return stats;
        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

    }
}