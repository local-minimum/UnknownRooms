using UnityEngine;
using UnityEngine.UI;

namespace ProcRoom.UI
{

    public class CharacterSelectGun : MonoBehaviour
    {
        Text text;

        [SerializeField]
        string[] buttonMessages;

        [SerializeField]
        int[] pointsThresholds;

        void Awake()
        {
            text = GetComponentInChildren<Text>();
            HandleNewPoints(CharacterCreation.Points);
        }

        void OnEnable()
        {
            CharacterCreation.OnNewPoints += HandleNewPoints;
        }

        void OnDisable()
        {
            CharacterCreation.OnNewPoints -= HandleNewPoints;
        }

        private void HandleNewPoints(int points)
        {
            for (int i=0, l=Mathf.Min(buttonMessages.Length, pointsThresholds.Length); i< l;i++)
            {
                if (points < pointsThresholds[i])
                {
                    text.text = buttonMessages[i];
                    return;
                }
            }
            text.text = buttonMessages[buttonMessages.Length - 1];
        }
    }
}