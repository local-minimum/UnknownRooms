using UnityEngine;
using System.Collections;

public enum FollowMode { ActiveAgent, Player};

namespace ProcRoom
{
    public class CamFollow : MonoBehaviour
    {

        [SerializeField]
        FollowMode followMode;


        [SerializeField]
        AnimationCurve easeCurve;

        Agent following;
        Vector3 cameraPositionTarget;
        Vector3 cameraPositionSource;

        [SerializeField, Range(0, 2)]
        float cameraSpeed = 0.5f;

        float easeStart;
        bool easing = false;
        bool updatingTarget = false;

        float zPosition;

        void Awake()
        {
            zPosition = transform.position.z;
        }

        void Update()
        {
            if (easing)
            {
                var progress = Mathf.Min(1f, (Time.timeSinceLevelLoad - easeStart) / cameraSpeed);
                transform.position = Vector3.Lerp(cameraPositionSource, cameraPositionTarget, easeCurve.Evaluate(progress));
                easing = progress < 1f;
            }
        }

        void OnEnable()
        {
            Tower.OnNewActiveAgent += Tower_OnNewActiveAgent;
            Agent.OnAgentMove += Agent_OnAgentMove;            
        }

        void OnDisable()
        {
            Tower.OnNewActiveAgent -= Tower_OnNewActiveAgent;
            Agent.OnAgentMove -= Agent_OnAgentMove;
        }

        private void Tower_OnNewActiveAgent(Agent agent)
        {
            if (followMode == FollowMode.ActiveAgent || agent == Tower.Player)
                SetNewTarget(agent);

        }

        private void Agent_OnAgentMove(Agent agent)
        {
            if (agent == following)
                SetNewTarget(agent);
        }


        void SetNewTarget(Agent a)
        {
            updatingTarget = true;
            following = a;
            easeStart = Time.timeSinceLevelLoad;
            cameraPositionTarget = a.transform.position;
            cameraPositionTarget.z = zPosition;
            cameraPositionSource = transform.position;
            easing = true;
            updatingTarget = false;
        }
    }
}