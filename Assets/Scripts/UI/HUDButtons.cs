using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ProcRoom.UI
{
    public class HUDButtons : MonoBehaviour
    {

        Button[] buttons;
        bool interactable = true;

        void Awake()
        {
            buttons = GetComponentsInChildren<Button>();
        }

        void OnEnable()
        {
            Tower.OnNewActiveAgent += HandleNewAgent;
        }

        void OnDisable()
        {
            Tower.OnNewActiveAgent -= HandleNewAgent;
        }

        private void HandleNewAgent(Agent agent)
        {
            SetButtonsInteractable(agent == Tower.Player);
            
        }

        void SetButtonsInteractable(bool interactable)
        {
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].interactable = interactable;
            this.interactable = interactable;
        }

        public void Reload()
        {
            Tower.Player.Reload();
        }

        public void EndTurn() {
            Tower.Player.actionPoints = 0;
        }

        public void UpgradeCharacter()
        {
            CharacterCreation.UpgradePlayer();
        }

        void Update()
        {
            if (Tower.Alive && interactable != Tower.Player.myTurn)
                SetButtonsInteractable(Tower.Player.myTurn);
            
        }
    }
}
