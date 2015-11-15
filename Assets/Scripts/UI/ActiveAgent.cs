using UnityEngine;
using System.Collections;

namespace ProcRoom.UI
{
    public class ActiveAgent : MonoBehaviour
    {
        SpriteRenderer rend;

        bool inTransit = false;

        [SerializeField, Range(0, 1)]
        float transitionEagerness = 0.1f;

        void Awake() {
            rend = GetComponent<SpriteRenderer>();
        }

        void Update()
        {
            if (inTransit)
            {
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, transitionEagerness);
                inTransit = transform.localPosition.sqrMagnitude < 0.01f;
                if (!inTransit)
                    transform.localPosition = Vector3.zero;
            }
        }

        void OnEnable()
        {
            if (rend)
                rend.enabled = false;
            Tower.OnNewActiveAgent += HandleNewActiveAgent;
        }

        void OnDisable() {
            Tower.OnNewActiveAgent -= HandleNewActiveAgent;
        }

        private void HandleNewActiveAgent(Agent agent)
        {
            if (rend && !rend.enabled)
                rend.enabled = true;

            transform.SetParent(agent.transform);
            inTransit = true;
        }
    }
}