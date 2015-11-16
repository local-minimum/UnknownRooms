using UnityEngine;
using System.Collections;

namespace ProcRoom.Physical
{
    public class AbilityStat : MonoBehaviour
    {

        [SerializeField, Range(1, 10)]
        int _cost = 1;

        [SerializeField]
        int _translatedStatsValue;
        
        Ability selector;


        public int cost
        {
            get
            {
                return _cost;
            }
        }

        public int value
        {
            get
            {
                return _translatedStatsValue;
            }
        }
    }
}