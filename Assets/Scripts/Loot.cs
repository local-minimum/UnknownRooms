using UnityEngine;
using System.Collections;

namespace ProcRoom
{

    public enum LootType {HealthUp, HealthFull, Key, Weapon};

    [System.Serializable]
    public class LootStats
    {
        public Coordinate position;
        public Sprite image;
        public Color32 lootColor;
        public LootType effect;
        public string lootName;
        [SerializeField, Range(0, 1)]
        public float probablility;
    }

    public class Loot : MonoBehaviour
    {

        [SerializeField]
        LootStats[] lootTypes;

        [SerializeField]
        SpriteRenderer lootObject;

        [SerializeField, Range(1, 20)]
        int weaponWorthPerLevel;

        [SerializeField, Range(1, 20)]
        int weaponBaseWorh;

        LootStats activeLoot;

        int level;

        public void Generate(Coordinate position)
        {
            activeLoot = GetRandomLootType();
            activeLoot.position = position;

            lootObject.transform.position = Tower.ActiveRoom.GetTileCentre(activeLoot.position);
            lootObject.sprite = activeLoot.image;
            lootObject.color = activeLoot.lootColor;
            lootObject.enabled = true;

        }

        void OnEnable()
        {
            Player.OnPlayerEnterNewPosition += HandlePlayerMove;
            Tower.OnNewLevel += HandleNewLevel;
            Agent.OnAgentDeath += HandleAgentDeath;
        }

        void OnDisable()
        {
            Player.OnPlayerEnterNewPosition -= HandlePlayerMove;
            Tower.OnNewLevel -= HandleNewLevel;
            Agent.OnAgentDeath -= HandleAgentDeath;
        }



        private void HandleAgentDeath(Agent agent)
        {
            if (agent != Tower.Player && !Tower.AnyMonsterAlive)
                Generate(agent.position);
        }

        private void HandleNewLevel(int level)
        {
            this.level = level;
            activeLoot = null;
            lootObject.enabled = false;
        }

        private void HandlePlayerMove(Player player, Coordinate position, TileType tileType)
        {
            if (activeLoot != null && activeLoot.position == position)
            {
                if (activeLoot.effect == LootType.HealthFull)
                    player.health = player.stats.maxHealth;
                else if (activeLoot.effect == LootType.HealthUp)
                    player.health += 1;
                else if (activeLoot.effect == LootType.Key)
                    player.AwardKey();
                else if (activeLoot.effect == LootType.Weapon)
                    UI.WeaponSelect.Show(Physical.WeaponSmith.Smith(weaponBaseWorh + level * weaponWorthPerLevel, true));
                activeLoot = null;
                lootObject.enabled = false;
            }
        }

        float totalProbability
        {
            get
            {
                float p = 0;
                for (int i = 0; i < lootTypes.Length; i++)
                    p += lootTypes[i].probablility; // lootTypes[i].effect != LootType.Key || !Tower.ActiveRoom.HasDoor ? lootTypes[i].probablility : 0;
                return p;
            }
        }

        LootStats GetRandomLootType()
        {
            var p = Random.Range(0, totalProbability);
            for (int i=0; i<lootTypes.Length; i++)
            {
                /*if (lootTypes[i].effect == LootType.Key && Tower.ActiveRoom.HasDoor)
                {
                    if (i == lootTypes.Length - 1)
                        return lootTypes[i - 1];
                    else
                        continue;
                }*/

                if (p < lootTypes[i].probablility)
                    return lootTypes[i];
                else
                    p -= lootTypes[i].probablility;
            }
            return lootTypes[lootTypes.Length - 1];
        }
    }
}