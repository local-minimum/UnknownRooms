using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom.Physical
{
    public class MonsterSmith : MonoBehaviour
    {
        [SerializeField]
        GameObject _monsterPrefab;

        static MonsterSmith _instance;

        List<Monster> pool = new List<Monster>();

        [SerializeField]
        Ability Health;

        [SerializeField]
        Ability Defence;

        [SerializeField]
        Ability ActionPoints;

        [SerializeField]
        Ability ClipSize;

        [SerializeField, Range(0, 1)]
        float likelihoodToBias;

        [SerializeField]
        FloatRange weaponsSavings = new FloatRange(0.4f, 1f);

        Dictionary<Ability, int> blankState
        {
            get
            {
                var abilityState = new Dictionary<Ability, int>();

                abilityState[Health] = 0;
                abilityState[Defence] = 0;
                abilityState[ActionPoints] = 0;
                abilityState[ClipSize] = 0;

                return abilityState;
            }
        }



        static MonsterSmith instance
        {
            get {
                if (_instance == null)
                    _instance = FindObjectOfType<MonsterSmith>();
                return _instance;

            }
        }

        public static Monster Smith(int points)
        {
            var monster = instance.GetFreeMonster();
            var abilityState = instance.blankState;
            Ability fancy = new List<Ability>(abilityState.Keys)[Random.Range(0, abilityState.Count)];
            if (Random.value > instance.likelihoodToBias)
                fancy = null;
            int weaponPoints = instance.GetWeaponPoints(ref points);
            while (Upgrade(ref abilityState, ref points, fancy)) ;
            monster.SetStats(instance.createMonster(abilityState));
            monster.Weapon.SetStats(WeaponSmith.Smith(weaponPoints + points));
            return monster;
        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;

            SetupMonsterPool();
        }
        
        int GetWeaponPoints(ref int points)
        {
            int weaponPoints = Mathf.RoundToInt(points * weaponsSavings.RandomValue);
            points -= weaponPoints;
            return weaponPoints;
        }

        void SetupMonsterPool()
        {
            var monsters = FindObjectsOfType<Monster>();
            for (int i = 0; i < monsters.Length; i++)
            {
                ParentMonster(monsters[i].transform);
            }
            pool.AddRange(monsters);
        }

        void ParentMonster(Transform monster)
        {
            if (monster.parent == transform)
                return;
            else if (monster.parent == null)
                monster.SetParent(transform);
            else
                ParentMonster(monster.parent);
        }

        Monster GetFreeMonster()
        {
            for (int i=0, l=pool.Count; i< l; i++)
            {
                if (!pool[i].alive)
                    return pool[i];
            }

            return InstantiateNewMonster();
        }

        Monster InstantiateNewMonster()
        {
            var GO = Instantiate(_monsterPrefab);
            GO.transform.SetParent(transform);
            var monster = GO.GetComponentInChildren<Monster>();
            pool.Add(monster);
            return monster;
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

        AgentStats createMonster(Dictionary<Ability, int> state)
        {
            var stats = new AgentStats();
            stats.maxHealth = Health[state[Health]].value;
            stats.actionPoints = ActionPoints[state[ActionPoints]].value;
            stats.defence = Defence[state[Defence]].value;
            stats.clipSize = ClipSize[state[ClipSize]].value;
            return stats;
        }
    }
}