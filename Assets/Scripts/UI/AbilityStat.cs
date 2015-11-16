using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace ProcRoom.UI
{
    public class AbilityStat : MonoBehaviour
    {

        AbilitySelector selector;
        int index = -1;
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

        public int Index
        {
            get
            {
                return index;
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

        public void SetIndex(int value)
        {
            if (index < 0)
                index = value;
            else
                Debug.LogWarning("Refused re-indexing");
        }

        void Click()
        {
            selector.Select(this);
        }
    }
}