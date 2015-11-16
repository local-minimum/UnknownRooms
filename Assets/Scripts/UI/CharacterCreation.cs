using UnityEngine;
using UnityEngine.UI;

namespace ProcRoom.UI
{

    public delegate void NewPoints(int points);

    public class CharacterCreation : MonoBehaviour
    {

        [SerializeField, Range(1, 100)]
        int points;

        [SerializeField]
        AbilitySelector health;

        [SerializeField]
        AbilitySelector actionPoints;

        [SerializeField]
        AbilitySelector defence;

        [SerializeField]
        AbilitySelector clipSize;

        [SerializeField]
        PlayerNamer namer;

        static CharacterCreation _instance;

        static CharacterCreation instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<CharacterCreation>();
                return _instance;
            }
        }

        public static event NewPoints OnNewPoints;


        public static void NewTransaction(int cost)
        {
            instance.points -= cost;
            if (OnNewPoints != null)
                OnNewPoints(instance.points);
        }

        public static int Points
        {
            get
            {
                return instance.points;
            }
        }

        public void CommitStats()
        {
            var stats = new AgentStats();
            stats.actionPointsPerTurn = actionPoints.Value;
            stats.maxHealth = health.Value;
            stats.clipSize = clipSize.Value;
            stats.defence = defence.Value;
            stats.name = namer.Name;

            var player = FindObjectOfType<Player>();
            if (player)
                player.SetStats(stats);
            gameObject.SetActive(false);
        }
    }
}