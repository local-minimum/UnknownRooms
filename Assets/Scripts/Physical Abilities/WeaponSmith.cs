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

        [SerializeField, Range(0, 1)]
        float likelihoodToBias = 0.7f;

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
            var fancy =  new List<Ability>(abilityStates.Keys)[Random.Range(0, abilityStates.Count)];
            if (Random.value > instance.likelihoodToBias)
                fancy = null;
            Debug.Log("Fancy: " + fancy);
            while (Upgrade(ref abilityStates, ref points, fancy)) ;
            return instance.createWeapon(abilityStates);
        }

        static bool Upgrade(ref Dictionary<Ability, int> state, ref int points, Ability fancy)
        {
            var availables = new Ability[state.Count + 1];
            int totalAvailable = 0;
            foreach (KeyValuePair<Ability, int> kvp in state)
            {
                var cost = kvp.Key.Length > kvp.Value + 1 ? kvp.Key[kvp.Value + 1].cost : -1;
                if (cost >= 0 && cost <= points)
                {
                    Debug.Log(string.Format("Possible upgrade on {0} to lvl {1} at cost {2} ({3})", kvp.Key.name, kvp.Value + 1, cost, points));
                    availables[totalAvailable] = kvp.Key;
                    totalAvailable++;
                    if (kvp.Key == fancy)
                    {
                        availables[totalAvailable] = fancy;
                        totalAvailable++;
                    }

                }
            }
            if (totalAvailable == 0)
                return false;

            var upgradeAbility = availables[Random.Range(0, totalAvailable)];
            state[upgradeAbility]++;
            Debug.Log(string.Format("Upgrading {0} of {1} available", upgradeAbility.name, totalAvailable));
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