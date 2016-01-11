using UnityEngine;


namespace ProcRoom {
    public class MeleeSystem : MonoBehaviour {

        static MeleeSystem _instance;

        [SerializeField, Range(0, 1)]
        float precisionToAgility = 0.75f;

        [SerializeField, Range(0, 1)]
        float defenceToAgility = 0.75f;

        [SerializeField, Range(1, 5)]
        int attackRndN = 3;

        [SerializeField]
        FloatRange attackRndRange;

        [SerializeField, Range(1, 5)]
        int defenceRndN = 3;

        [SerializeField]
        FloatRange defenceRndRange;

        [SerializeField, Range(1, 5)]
        int boostRndN = 2;

        [SerializeField]
        FloatRange boostRndRange;


        [SerializeField, Range(1, 5)]
        int critDefenceRndN = 4;

        [SerializeField]
        FloatRange critDefenceRange;

        static MeleeSystem instance
        {
            get
            {
                if (_instance == null)
                    Spawn();

                return _instance;
            }
        }

        static void Spawn() {
            _instance = Tower.T.gameObject.AddComponent<MeleeSystem>();
        }

        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(this);
        }


        public static bool Execute(Agent attacker, Agent defender)
        {
            var attackerStats = attacker.stats;
            var defenderStats = defender.stats;
            var weaponStats = attacker.Weapon.Stats;

            var attack = ((1 - instance.precisionToAgility) * attackerStats.agility + 
                instance.precisionToAgility * weaponStats.precision) * attackerStats.health / attackerStats.maxHealth *
                Stat.SumOfUniformRange(instance.attackRndN, instance.attackRndRange.min, instance.attackRndRange.max);

            var defence = (instance.defenceToAgility * defenderStats.defence + 
                (1 - instance.defenceToAgility) * defenderStats.agility) *
                Stat.SumOfUniformRange(instance.defenceRndN, instance.defenceRndRange.min, instance.defenceRndRange.max);

            float boost;
            if (defenderStats.boosted)
            {
                boost = Stat.SumOfUniformRange(instance.boostRndN, instance.boostRndRange.min, instance.boostRndRange.max);

            } else
            {
                boost = 0f;
            }

            bool hit = attack > defence + boost;

            if (hit)
            {
                var critDefence = defenderStats.agility * defenderStats.health / defenderStats.maxHealth *
                    Stat.SumOfUniformRange(instance.critDefenceRndN, instance.critDefenceRange.min, instance.critDefenceRange.max);

                bool criticalHit = weaponStats.critChance > critDefence;

                if (criticalHit)
                    defender.CriticalHurt();
                else
                    defender.Hurt();
            }

            return hit;
        }
    }
}
