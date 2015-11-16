using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom.Physical
{
    public class Ability : MonoBehaviour
    {

        AbilityStat[] stages;

        public int Length
        {
            get
            {
                if (stages == null)
                    CollectStages();

                return stages == null ? 0 : stages.Length;
            }
        }

        public AbilityStat this[int index]
        {
            get
            {
                return stages[index];
            }
        }

        void CollectStages()
        {
            var stats = new List<AbilityStat>();
            for (int i=0, l=transform.childCount; i< l; i++)
            {
                var stat = transform.GetChild(i).GetComponent<AbilityStat>();
                if (stat)
                    stats.Add(stat);
            }
            stages = stats.ToArray();
        }

        void Awake() {
            CollectStages();
        }

    }
}
