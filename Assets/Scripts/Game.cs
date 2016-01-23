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

        [SerializeField]
        string playerScores = "fame.player.score.";

        [SerializeField]
        string playerName = "fame.player.name.";

        [SerializeField, Range(0, 10)]
        float killScoreFactor = 3f;

        [SerializeField, Range(0, 10)]
        float levelScoreFactor = 2f;

        [SerializeField, Range(0, 5)]
        int famePositions = 3;

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

        static float GetScore(AgentStats stats)
        {
            float score = 0;
            score += stats.health + stats.xp;
            score += stats.kills * _instance.killScoreFactor;
            score += stats.maxLevel * _instance.levelScoreFactor;
            return score;
        }

        static bool Famous(AgentStats player, float score)
        {
            for (int i=0; i<_instance.famePositions; i++)
            {
                if (score > PlayerPrefs.GetFloat(_instance.playerScores + i, 0f))
                {
                    for (int j = _instance.famePositions - 1; j > i; j--)
                    {
                        PlayerPrefs.SetFloat(_instance.playerScores + j, PlayerPrefs.GetFloat(_instance.playerScores + (j - 1), 0f));
                        PlayerPrefs.SetString(_instance.playerName + j, PlayerPrefs.GetString(_instance.playerName + (j - 1), ""));
                    }

                    PlayerPrefs.SetFloat(_instance.playerScores + i, score);
                    PlayerPrefs.SetString(_instance.playerName + i, player.name);

                    return true;                    
                }
            }

            return false;
        }

        public static void ReportScore(AgentStats player)
        {
            var score = GetScore(player);
            if (Famous(player, score))
            {

            } else
            {

            }

        }

    }
}