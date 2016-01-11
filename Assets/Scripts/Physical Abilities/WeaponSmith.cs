using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom.Physical
{
    public class WeaponSmith : MonoBehaviour
    {
        [SerializeField]
        Ability WeaponsRange;

        [SerializeField]
        Ability WeaponsPrecisionLoss;

        [SerializeField]
        Ability WeaponsPrecision;

        [SerializeField]
        Ability WeaponsClipSize;

        [SerializeField]
        Ability WeaponsCritChance;

        [SerializeField, Range(0, 1)]
        float likelihoodToBias = 0.7f;

        Dictionary<Ability, int> blankRangeState
        {
            get
            {
                var abilityState = new Dictionary<Ability, int>();

                abilityState[WeaponsPrecision] = 0;
                abilityState[WeaponsPrecisionLoss] = 0;
                abilityState[WeaponsRange] = 0;
                abilityState[WeaponsClipSize] = 0;
                return abilityState;
            }
        }

        Dictionary<Ability, int> blankMeleeState
        {
            get
            {
                var abilityState = new Dictionary<Ability, int>();
                abilityState[WeaponsPrecision] = 0;
                abilityState[WeaponsClipSize] = 0;
                abilityState[WeaponsCritChance] = -1;
                return abilityState;

            }
        }

        public static int Worth(Weapon weapon)
        {
            int worth = 0;
            var stats = weapon.Stats;
            worth += instance.WeaponsPrecision.Cost(stats.precision);
            worth += instance.WeaponsPrecisionLoss.Cost(stats.precisionLossPerTile);
            worth += instance.WeaponsRange.Cost(stats.maxRange);
            Debug.Log(string.Format("{0} weapon has worth {1}", weapon.name, worth));
            return worth;
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

        public static WeaponStats Smith(int points, bool isMelee)
        {
            Debug.Log("Smitting a " + (isMelee ? "melee" : "ranged") + " weapon with " + points + " points");

            var abilityStates = isMelee ? instance.blankMeleeState : instance.blankRangeState;
            Ability fancy = null;

            if (Random.value < instance.likelihoodToBias)
                fancy = new List<Ability>(abilityStates.Keys)[Random.Range(0, abilityStates.Count)];


            while (Upgrade(ref abilityStates, ref points, fancy)) ;
            return instance.createWeapon(abilityStates, isMelee);
        }

        static bool Upgrade(ref Dictionary<Ability, int> state, ref int points, Ability fancy)
        {
            var availables = new Ability[state.Count + 1];
            int totalAvailable = 0;
            foreach (KeyValuePair<Ability, int> kvp in state)
            {
                var cost = kvp.Key.Length > kvp.Value + 1 ? kvp.Key[kvp.Value + 1].cost : -1;
                if (cost > 0 && cost <= points)
                {
                    //Debug.Log(string.Format("Possible upgrade on {0} to lvl {1} at cost {2} ({3})", kvp.Key.name, kvp.Value + 1, cost, points));
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
            Debug.Log(string.Format("Upgrading {0} ({1} available) to lvl {3} (cost {2})", 
                upgradeAbility.name, totalAvailable, upgradeAbility[state[upgradeAbility]].cost, state[upgradeAbility]));
            points -= upgradeAbility[state[upgradeAbility]].cost;
            return true;
        }

        WeaponStats createWeapon(Dictionary<Ability, int> state, bool isMelee)
        {
            if (isMelee)
                return new WeaponStats(WeaponsPrecision.GetValue(state[WeaponsPrecision]),
                    WeaponsCritChance.GetValue(state[WeaponsCritChance]),
                    WeaponsClipSize.GetValue(state[WeaponsClipSize]));
            else
                return new WeaponStats(WeaponsPrecision[state[WeaponsPrecision]].value,
                    WeaponsRange[state[WeaponsRange]].value,
                    WeaponsPrecisionLoss[state[WeaponsPrecisionLoss]].value,
                    WeaponsClipSize[state[WeaponsClipSize]].value);

        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

    }
}