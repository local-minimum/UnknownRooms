using UnityEngine;
using System.Collections;

namespace ProcRoom
{
    public class CamSizer : MonoBehaviour
    {

        Camera cam;

        [SerializeField, Range(1, 5)]
        float maxScale;

        float baseSize = 0;

        float sizeSource;
        float sizeTarget;
    
        [SerializeField]
        AnimationCurve resumeNormalSize;

        [SerializeField]
        AnimationCurve rescale;

        AnimationCurve currentCurve;

        [SerializeField, Range(0, 2)]
        float animationDuration = 1f;

        float actionStartTime;

        [SerializeField,Range(0, 10)]
        int rescaleFromDelta = 2;

        [SerializeField, Range(0, 10)]
        int deltaCap = 7;

        bool resting = true;
        int lastDelta;

        void Start()
        {
            cam = GetComponent<Camera>();
            if (cam)
                baseSize = cam.orthographicSize;
        }

        void OnEnable() {
            Tile.OnTileHover += Tile_OnTileHover;
            Tower.OnNewActiveAgent += Tower_OnNewActiveAgent;
        }

        private void Tower_OnNewActiveAgent(Agent agent)
        {
            if (agent != Tower.Player && sizeTarget != baseSize)
            {
                lastDelta = 0;
                sizeSource = cam.orthographicSize;
                actionStartTime = Time.timeSinceLevelLoad;
                currentCurve = resumeNormalSize;
                sizeTarget = baseSize;
                resting = false;
            }
        }

        void OnDisable()
        {
            Tile.OnTileHover -= Tile_OnTileHover;
            Tower.OnNewActiveAgent -= Tower_OnNewActiveAgent;
        }

        private void Tile_OnTileHover(Tile tile, MouseEvent type)
        {
            if (type == MouseEvent.Exit)
                return;

            if (Tower.Alive && Tower.Player.myTurn)
            {
                int delta = Mathf.Min(Mathf.Abs(tile.position.y - Tower.Player.position.y), deltaCap);
                if (delta == lastDelta && !resting)
                    return;
                lastDelta = delta;
                if (delta < rescaleFromDelta)
                {
                    currentCurve = resumeNormalSize;
                    sizeTarget = baseSize;
                } else
                {
                    currentCurve = rescale;
                    sizeTarget = baseSize * Mathf.Lerp(1f, maxScale, delta/(float) deltaCap);
                }
                sizeSource = cam.orthographicSize;
                actionStartTime = Time.timeSinceLevelLoad;
                resting = false;
            }
        }

        void Update()
        {
            if (!resting)
            {
                float progression = Mathf.Min(1f, (Time.timeSinceLevelLoad - actionStartTime) / animationDuration);
                cam.orthographicSize = Mathf.Lerp(sizeSource, sizeTarget, currentCurve.Evaluate(progression));
                resting = progression == 1f;
            }
        }
    }
}