using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public class TreasureChest : MonoBehaviour
    {
        [SerializeField]
        GameObject[] treasurePrefabs;

        List<Treasure> treasures = new List<Treasure>();

        static TreasureChest _instance;

        static TreasureChest instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<TreasureChest>();
                return _instance;
            }
        }

        void Awake() {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(this);
        }
        
        public static void Spawn(List<int> mapIndices, int mapWidth)
        {
            for (int i=0, l=mapIndices.Count; i< l; i++)
            {
                instance.Spawn(Coordinate.FromPosition(mapIndices[i], mapWidth));
            }
        }

        void Spawn(Coordinate pos)
        {
            var treasure = GetInactiveTreasure(Random.Range(1, treasurePrefabs.Length));
            treasure.position = pos;
            treasure.Showing = true;
        }

        Treasure GetInactiveTreasure(int value)
        {
            for (int i = 0, l = treasures.Count; i < l; i++) {
                if (!treasures[i].Showing && treasures[i].value == value)
                    return treasures[i];                    
            }
            return CreateNewFromPrefab(value);
        }

        Treasure CreateNewFromPrefab(int value)
        {
            var GO = Instantiate(treasurePrefabs[Random.Range(0, Mathf.Min(treasurePrefabs.Length, value) - 1)]);
            GO.transform.SetParent(transform);
            var treasure = GO.GetComponent<Treasure>();
            treasures.Add(treasure);
            return treasure;
        }
    }
}