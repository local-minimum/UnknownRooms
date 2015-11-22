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
        Text LevelText;

        [SerializeField]
        Text PlayerName;

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
                transform.GetChild(0).gameObject.SetActive(value);
            }
        }

        void OnEnable() {
            Tower.OnNewGame += HandleNewGame;
            if (player)
                ConnectPlayerEvents(player);
        }

        void OnDisable()
        {
            Tower.OnNewGame -= HandleNewGame;
            if (player)
                DisconnectPlayerEvents();
        }

        void ConnectPlayerEvents(Player player)
        {
            if (this.player)
                DisconnectPlayerEvents();
            this.player = player;
            player.OnAgentActionChange += HandlePlayerActionPoints;
            player.OnAgentAmmoChange += HandlePlayerAmmo;
            player.OnAgentHealthChange += HandlePlayerHealth;
        }

        void DisconnectPlayerEvents() {
            player.OnAgentActionChange -= HandlePlayerActionPoints;
            player.OnAgentHealthChange -= HandlePlayerHealth;
            player.OnAgentAmmoChange -= HandlePlayerAmmo;
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
            

            ActionPoints.maxValue = stats.actionPointsPerTurn;
            ActionPoints.currentValue = stats.actionPoints;

            Health.maxValue = stats.maxHealth;
            Health.currentValue = stats.health;

            var defenceLevel = DefenceAbility.GetLevel(stats.defence) + 1;            
            Defence.maxValue = defenceLevel;
            Defence.currentValue = defenceLevel;

            Ammo.maxValue = stats.clipSize;
            Ammo.currentValue = stats.ammo;

            PlayerName.text = stats.name;
            
            //PlayerIcon.image = ??            

            Showing = true;
            Debug.Log("Set new game UI");
        }

    }
}