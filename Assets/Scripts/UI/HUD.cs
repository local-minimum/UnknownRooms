using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ProcRoom.UI
{

    public class HUD : MonoBehaviour
    {
        [SerializeField]
        HUDStats ActionPoints;

        [SerializeField]
        HUDStats Health;

        [SerializeField]
        HUDStats Defence;

        [SerializeField]
        Physical.Ability DefenceAbility;

        [SerializeField]
        HUDStats Ammo;

        [SerializeField]
        HUDStats Keys;

        [SerializeField]
        Text LevelText;

        [SerializeField]
        Text PlayerName;

        [SerializeField]
        Text Points;

        [SerializeField]
        Image PlayerIcon;

        Player player;

        void Start()
        {
            Showing = false;
        }

        public bool Showing {

            get
            {
                return transform.GetChild(0).gameObject.activeSelf;
            }

            set
            {
                for (int i=0, l=transform.childCount; i< l; i++)
                    transform.GetChild(i).gameObject.SetActive(value);
            }
        }

        void OnEnable() {
            Tower.OnNewGame += HandleNewGame;
            Tower.OnNewLevel += HandleNewLevel;
            Weapon.OnAmmoChange += Weapon_OnAmmoChange;
            if (player)
                ConnectPlayerEvents(player);
        }

        void OnDisable()
        {
            Weapon.OnAmmoChange -= Weapon_OnAmmoChange;
            Tower.OnNewGame -= HandleNewGame;
            Tower.OnNewLevel -= HandleNewLevel;
            if (player)
                DisconnectPlayerEvents();
        }

        private void Weapon_OnAmmoChange(Weapon weapon)
        {
            if (weapon == Tower.Player.Weapon)
            {
                Ammo.currentValue = weapon.ammo;
                Ammo.maxValue = weapon.Stats.clipSize;
            }
        }

        void ConnectPlayerEvents(Player player)
        {
            if (this.player)
                DisconnectPlayerEvents();
            this.player = player;
            player.OnAgentActionChange += HandlePlayerActionPoints;
            player.OnAgentHealthChange += HandlePlayerHealth;
            player.OnAgentHasKeyChange += HandleKeyChange;
            player.OnAgentXPChange += HandleXPChange;
            player.OnAgentUpgrade += HandleNewStats;
        }

        private void HandleXPChange(int xp)
        {
            Points.text = "XP: " + xp;
        }

        private void HandleKeyChange(bool hasKey)
        {
            Keys.currentValue = hasKey ? 1 : 0;
        }

        private void HandleNewLevel(int level)
        {
            LevelText.text = "LVL: " + level;
        }


        void DisconnectPlayerEvents() {
            player.OnAgentActionChange -= HandlePlayerActionPoints;
            player.OnAgentHealthChange -= HandlePlayerHealth;
            player.OnAgentHasKeyChange -= HandleKeyChange;
            player.OnAgentXPChange -= HandleXPChange;
            player.OnAgentUpgrade -= HandleNewStats;
        }

        private void HandlePlayerAmmo(int remainingAmmo)
        {
            Ammo.currentValue = remainingAmmo;
        }

        private void HandlePlayerActionPoints(int actionPoints)
        {
            ActionPoints.currentValue = actionPoints;
        }

        private void HandlePlayerHealth(int health)
        {
            Health.currentValue = health;
        }

        private void HandleNewGame(Player player)
        {
            ConnectPlayerEvents(player);
            var stats = player.stats;
            HandleNewStats(stats);
        }

        void HandleNewStats(AgentStats stats) {

            Debug.Log("New stats");
            ActionPoints.maxValue = stats.actionPointsPerTurn;
            ActionPoints.currentValue = stats.actionPoints;

            Health.maxValue = stats.maxHealth;
            Health.currentValue = stats.health;

            var defenceLevel = DefenceAbility.GetLevel(stats.defence) + 1;            
            Defence.maxValue = defenceLevel;
            Defence.currentValue = defenceLevel;

            Keys.maxValue = 3;
            Keys.currentValue = stats.keys;

            PlayerName.text = stats.name;
            HandleXPChange(stats.xp);

            //PlayerIcon.image = ??            

            HandleNewLevel(Tower.ActiveLevel);
            Showing = true;
            
        }

    }
}