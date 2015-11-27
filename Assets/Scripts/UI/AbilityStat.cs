using UnityEngine;
using UnityEngine.UI;
using System.Collections;


namespace ProcRoom.UI
{
    public class AbilityStat : MonoBehaviour
    {

        AbilitySelector selector;
        int index = -1;
        
        [SerializeField]
        Image selectionImage;
        [SerializeField]
        Image backgroundImage;

        public int Index
        {
            get
            {
                return index;
            }
        }

        bool _allowed = false;

        public bool allowed
        {
            get
            {
                if (selectionImage == null)
                    SetupComponents();

                return _allowed;
            }

            set
            {
                if (selectionImage == null)
                    SetupComponents();

                backgroundImage.color = value ? Color.white : Color.black;
                if (selectionImage.enabled && !value)
                    selectionImage.enabled = false;
                _allowed = value;
            }
        }

        public bool selected
        {
            get
            {
                if (selectionImage == null)
                    SetupComponents();

                return selectionImage.enabled; 
            }

            set
            {
                if (selectionImage == null)
                    SetupComponents();
                if (allowed)
                    selectionImage.enabled = value;
            }
        }

        void Awake()
        {
            SetupComponents();
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

        public void Click()
        {
            selector.Select(this);
        }

        void SetupComponents()
        {
          
            backgroundImage = GetComponentInChildren<Image>();
            selectionImage = GetComponentsInChildren<Image>()[1];
        }
    }
}