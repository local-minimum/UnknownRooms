using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom
{
    public enum TouchAction {SwipeLeft, SwipeUp, SwipeRight, SwipeDown, Tap, DoubleTap};
    public delegate void TouchEvent(TouchAction action, Vector2 position);

    public class TouchControls : MonoBehaviour
    {
        public static event TouchEvent OnTouchEvent;

        [SerializeField]
        float sqMagnitudeSwipeMin = 0.5f;

        [SerializeField]
        float dimensionDifferenceMin = 0.9f;

        [SerializeField]
        float doubleTapTime = 0.4f;

        Vector2 _touchOrigin;
        Vector2 _touchDestination;
        
        bool tapping = false;

        void Update()
        {
            if (Input.touchSupported && Input.touchCount > 0)
            {
                var touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                    _touchOrigin = new Vector2(touch.position.x, touch.position.y);
                else if (touch.phase == TouchPhase.Ended)
                    ProcessEndedTouch(touch);

            }
        }

        void ProcessEndedTouch(Touch touch)
        {
            _touchDestination = new Vector2(touch.position.x, touch.position.y);
            var swipe = _touchDestination - _touchOrigin;
            if (swipe.sqrMagnitude < sqMagnitudeSwipeMin)
            {
                if (tapping)
                {
                    tapping = false;
                    EmitTouch(TouchAction.DoubleTap);
                }
                else
                    delayTap();
            } else
            {
                tapping = false;
                var normedSwipe = swipe.normalized;
                if (Mathf.Abs(1 - Mathf.Abs(normedSwipe.x) / Mathf.Abs(normedSwipe.y)) < dimensionDifferenceMin)
                {
                    Debug.LogWarning("Discarded swipe as unclear direction");
                } else
                {
                    if (Mathf.Abs(normedSwipe.x) > Mathf.Abs(normedSwipe.y))
                        EmitTouch(Mathf.Sign(normedSwipe.x) == 1 ? TouchAction.SwipeLeft : TouchAction.SwipeRight);
                    else
                        EmitTouch(Mathf.Sign(normedSwipe.y) == 1 ? TouchAction.SwipeUp : TouchAction.SwipeDown);

                }

            }
        }

        void EmitTouch(TouchAction action)
        {
            if (OnTouchEvent != null)
                OnTouchEvent(action, _touchDestination);
        }

        IEnumerator<WaitForSeconds> delayTap()
        {
            tapping = true;
            yield return new WaitForSeconds(doubleTapTime);
            if (tapping)
                EmitTouch(TouchAction.Tap);

        }

    }

}
