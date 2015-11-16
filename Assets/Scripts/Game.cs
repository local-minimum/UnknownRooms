using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public class Game : MonoBehaviour
    {

        void Start()
        {
            if (PlayerPrefs.GetInt("Game.NewGame", 1) == 1)
                UI.CharacterCreation.Show();
        }


    }
}