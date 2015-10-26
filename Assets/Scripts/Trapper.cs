using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public class Trapper : MonoBehaviour
    {

        Trapper _instance;

        void Awake() {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
                Destroy(this);

        }

        public static void LaySpikeTraps(Room room, RoomData data, int parts)
        {

        }

    }

}
