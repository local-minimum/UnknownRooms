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
                if (index < stages.Length && index >= 0)
                    return stages[index];
                else
                {
                    Debug.LogError("Attempting to retrieve " + name + " stage " + index + " when length is " + Length);
                    return null;
                }
            }
        }

        public int Cost(int value)
        {
            int cost = 0;
            for (int i=0, l=Length; i< l; i++)
            {
                cost += this[i].cost;

                if (this[i].value == value)
                    break;
            }
            return cost;
        }

        public int GetLevel(int value)
        {
            for (int i=0, l=Length; i< l; i++)
            {
                if (this[i].value == value)
                    return i;
            }
            return -1;

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

        public int GetValue(int index)
        {

            if (index < 0)
            {
                Debug.LogWarning("Returning zero value for " + name);
                return 0;
            }
            //Debug.Log(name + " index " + index + " is " + this[index].name + ": " + this[index].value);
            return this[index].value;

        }

    }
}
