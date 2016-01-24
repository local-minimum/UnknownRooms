using UnityEngine;
using System.Collections;

namespace ProcRoom.UI
{
    public class DeathClick : MonoBehaviour
    {

        [SerializeField, Range(0, 10)]
        float maxShowDuration;

        float endOfShow;

        void Start()
        {
            endOfShow = Time.timeSinceLevelLoad + maxShowDuration;
        }

        void Update()
        {
            if (Time.timeSinceLevelLoad > endOfShow || Input.anyKeyDown)
                Game.NewGame();
        }
    }
}