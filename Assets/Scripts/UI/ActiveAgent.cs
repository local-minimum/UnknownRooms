using UnityEngine;
using System.Collections;

namespace ProcRoom.UI
{
    public class ActiveAgent : MonoBehaviour
    {

        void OnEnable()
        {
            Tower.OnNewActiveAgent += HandleNewActiveAgent;
        }

        void OnDisable() {
            Tower.OnNewActiveAgent -= HandleNewActiveAgent;
        }

        private void HandleNewActiveAgent(Agent agent)
        {
            transform.SetParent(agent.transform);
            transform.localPosition = Vector3.zero;
        }
    }
}