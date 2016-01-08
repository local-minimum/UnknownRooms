using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ProcRoom
{
    public class WarningFlasher : MonoBehaviour
    {

        bool isWarning = false;

        [SerializeField]
        AnimationCurve warningAnimation;

        [SerializeField, Range(0, 2)]
        float duration;

        static Dictionary<string, WarningFlasher> _instances = new Dictionary<string, WarningFlasher>();

        void Awake()
        {
            _instances.Add(name, this);
        }

        public static WarningFlasher GetByName(string name)
        {
            return _instances[name];
        }

        public void Warn()
        {
            if (!isWarning)
                StartCoroutine(Animate());
        }

        IEnumerator<WaitForSeconds> Animate()
        {
            isWarning = true;
            List<Image> images = new List<Image>();
            var img =GetComponent<Image>();
            if (img != null)
                images.Add(img);

            for (int i=0; i<transform.childCount; i++)
            {
                img = transform.GetChild(i).GetComponent<Image>();
                if (img != null)
                    images.Add(img);
            }
            
            var numberOfImages = images.Count;
            var startTime = Time.timeSinceLevelLoad;
            float progress = 0;

            do
            {
                progress = Mathf.Min((Time.timeSinceLevelLoad - startTime) / duration, 1f);
                for (int i = 0; i < numberOfImages; i++)
                {
                    var color = images[i].color;
                    color.a = warningAnimation.Evaluate(progress);
                    images[i].color = color;
                }
                yield return new WaitForSeconds(0.04f);
            } while (progress < 1);
            isWarning = false;
        }

    }
}