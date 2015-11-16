using UnityEngine;
using UnityEngine.UI;

namespace ProcRoom.UI
{
    public delegate void OpionButtonSelected(OptionButtonFrame group, int selected);


    public class OptionButtonFrame : MonoBehaviour
    {
        [SerializeField]
        Image frame;

        [SerializeField]
        OptionButtonFrame group;

        public static event OpionButtonSelected OnSelectAction;

        [SerializeField]
        int value;

        void Start() {
            frame.enabled = false;
            if (value == 0)
                ClickAction();
            
                
        }

        void OnEnable()
        {
            OnSelectAction += HandleSelection;
        }

        void OnDisable()
        {
            OnSelectAction -= HandleSelection;
        }

        private void HandleSelection(OptionButtonFrame group, int selected)
        {
            if (group == this.group)
            {
                frame.enabled = selected == value;
            }
        }

        public void ClickAction()
        {
            if (!frame.enabled && OnSelectAction != null)
                OnSelectAction(group, value);
        }
    }
}