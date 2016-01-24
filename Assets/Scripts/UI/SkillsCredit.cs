using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ProcRoom.UI
{
    public class SkillsCredit : MonoBehaviour
    {

        Text text;

        void Awake()
        {
            text = GetComponent<Text>();
        }


        void OnEnable()
        {
            CharacterCreation.OnNewPoints += HandleNewPoints;

        }

        void OnDisable()
        {
            CharacterCreation.OnNewPoints -= HandleNewPoints;
        }

        void HandleNewPoints(int points)
        {
            if (text)
                text.text = "Credit: " + points;
        }
    }
}