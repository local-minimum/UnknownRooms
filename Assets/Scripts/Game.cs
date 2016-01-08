using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public delegate void ScreenSizeChange(int from, int to);

    public class Game : MonoBehaviour
    {
        public static event ScreenSizeChange OnScreenSizeChange;

        static Game _instance = null;

        [SerializeField, Range(0, 1)]
        float sizeCheckFrequency;


        void Awake()
        {
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(this);
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (PlayerPrefs.GetInt("Game.NewGame", 1) == 1)
                UI.CharacterCreation.CreatNewPlayer();

            StartCoroutine(SizeChecker());
        }

        IEnumerator<WaitForSeconds> SizeChecker()
        {
            var size = Screen.width;
            while (true)
            {
                var newSize = Screen.width;
                
                if (size != newSize)
                {
                    if (OnScreenSizeChange != null)
                        OnScreenSizeChange(size, newSize);
                    size = newSize;
                }
                yield return new WaitForSeconds(sizeCheckFrequency);
            }
        }

    }
}