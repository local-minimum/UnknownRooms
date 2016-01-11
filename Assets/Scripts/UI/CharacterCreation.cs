using UnityEngine;
using UnityEngine.UI;

namespace ProcRoom.UI
{

    public delegate void NewPoints(int points);

    public class CharacterCreation : MonoBehaviour
    {

        [SerializeField, Range(0, 100)]
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

        [SerializeField]
        OptionButtonFrame spriteGroup;

        int selectedSprite = 0;

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
       
        static void Show()
        {
            var ui = instance.transform.GetChild(0);
            ui.gameObject.SetActive(true);
            ui.GetComponent<Image>().enabled = true;
            var uiTop = instance.transform.GetChild(1);
            uiTop.gameObject.SetActive(!instance.upgrading);
        }

        public static event NewPoints OnNewPoints;

        AgentStats playerStats;
        bool upgrading = false;

        public static void CreatNewPlayer()
        {
            Time.timeScale = 0;
            instance.upgrading = false;
            Show();
        }

        public static void UpgradePlayer()
        {
            var stats = Tower.Player.stats;
            if (stats.xp <= 0)
                return;
            Time.timeScale = 0;
            instance.playerStats = stats;
            instance.upgrading = true;
            instance.health.MinValue = stats.maxHealth;
            instance.actionPoints.MinValue = stats.actionPointsPerTurn;
            instance.clipSize.MinValue = stats.clipSize;
            instance.defence.MinValue = stats.defence;            
            instance.points = stats.xp;

            Show();

            if (OnNewPoints != null)
                OnNewPoints(instance.points);
        }

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
            if (!upgrading)
            {
                stats.name = namer.Name;
                Tower.Player.SetStats(stats);
                Tower.Player.NewGame();
                var weapon1 = Physical.WeaponSmith.Smith(points);
                Tower.Player.Weapon.SetStats(weapon1);
                if (points > 0)
                {
                    WeaponStats weapon2 = WeaponStats.DefaultWeapon;
                    for (int i = 0; i < 5; i++)
                    {
                        weapon2 = Physical.WeaponSmith.Smith(points);
                        if (weapon1 != weapon2)
                            break;

                    }
                    WeaponSelect.Show(weapon2);
                }
                else
                    Time.timeScale = 1;

            } else
            {
                stats.name = playerStats.name;
                stats.xp = Mathf.Max(0, instance.points);
                stats.health = playerStats.health;
                stats.ammo = playerStats.ammo;
                stats.actionPoints = playerStats.actionPoints;
                stats.keys = playerStats.keys;
                stats.position = playerStats.position;
                
                Tower.Player.SetStats(stats);
                Time.timeScale = 1;
            }
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(1).gameObject.SetActive(false);
        }

        void OnEnable()
        {            
            OptionButtonFrame.OnSelectAction += HandleNewSpriteSelect;
        }

        void OnDisable()
        {
            OptionButtonFrame.OnSelectAction -= HandleNewSpriteSelect;                
        }
        
        private void HandleNewSpriteSelect(OptionButtonFrame group, int selected)
        {
            if (group == spriteGroup)
                selectedSprite = selected;
        }
    }
}