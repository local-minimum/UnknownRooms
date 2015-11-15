using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace ProcRoom.UI
{
    public class AbilityStat : MonoBehaviour
    {

        [SerializeField, Range(1, 10)]
        int _cost = 1;

        [SerializeField]
        int translatedStatsValue;

        AbilitySelector selector;
        Toggle _toggle;

        Toggle toggle
        {
            get
            {
                if (_toggle == null)
                    _toggle = GetComponentInChildren<Toggle>();
                return _toggle;
            }
        }
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
                return translatedStatsValue;
            }
        }

        public bool allowed
        {
            get
            {
                return toggle.interactable;
            }

            set
            {
                toggle.interactable = value;
                toggle.image.color = value ? Color.white : Color.black;
            }
        }

        public bool selected
        {
            get
            {
                return toggle.isOn;
            }

            set
            {
                toggle.isOn = value;
            }
        }

        void Start()
        {
            selector = GetComponentInParent<AbilitySelector>();            
        }

        void Click()
        {
            selector.Select(this);
        }
    }
}