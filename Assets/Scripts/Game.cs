using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace ProcRoom
{
    public delegate void ScreenSizeChange(int from, int to);

    public enum GameAction { NewGame, ResumeGame};

    public class Game : MonoBehaviour
    {
        public static event ScreenSizeChange OnScreenSizeChange;

        static Game _instance = null;
        
        static Game instance
        {
            get
            {
                if (_instance == null)
                    Spawn();
                return _instance;
            }
        }

        [SerializeField, Range(0, 1)]
        float sizeCheckFrequency = 0.5f;

        [SerializeField]
        string playerScores = "fame.player.score.";

        [SerializeField]
        string playerName = "fame.player.name.";

        [SerializeField]
        string _gameAction = "game.action";

        public static string gameAction
        {
            get
            {
                return instance._gameAction;
            }
        }

        [SerializeField, Range(0, 10)]
        float killScoreFactor = 3f;

        [SerializeField, Range(0, 10)]
        float levelScoreFactor = 2f;

        [SerializeField, Range(0, 5)]
        int famePositions = 3;

        bool resentDeathWasFamous = false;

        static void Spawn()
        {
            var GO = new GameObject("Game");
            _instance = GO.AddComponent<Game>();
        }

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
            //TODO: This should use some proper serialization that allows saving player avatar too
            //Should save: Name, Icon, Score, Day, MaxLevel

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
            _instance.resentDeathWasFamous = Famous(player, score);
            Debug.Log("Got score: " + score + " (famous=" + _instance.resentDeathWasFamous + ")");
            SceneManager.LoadScene("death", LoadSceneMode.Single);

        }

        public static void NewGame()
        {
            PlayerPrefs.SetInt(_instance._gameAction, (int) GameAction.NewGame);
            SceneManager.LoadScene("play", LoadSceneMode.Single);

        }

    }
}