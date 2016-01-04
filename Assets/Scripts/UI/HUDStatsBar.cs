using UnityEngine;
using UnityEngine.UI;


namespace ProcRoom.UI
{
    public class HUDStatsBar : MonoBehaviour
    {
        Image imageFrame;
        Image imageBar;

        float scale = 0.3f;

        bool _on = false;

        public Sprite Frame {
            set
            {
                if (imageFrame == null)
                    SetupFrame();
                imageFrame.sprite = value;
            }
        }

        public Sprite Bar {
            set
            {
                if (imageBar == null)
                    SetupBar();
                imageBar.sprite = value;
            }
        }

        public float height
        {
            set
            {
                imageFrame.rectTransform.sizeDelta = new Vector2(imageFrame.sprite.bounds.extents.x * value / imageFrame.sprite.bounds.extents.y, value);
                imageBar.rectTransform.sizeDelta = new Vector2(imageBar.sprite.bounds.extents.x * value / imageBar.sprite.bounds.extents.y / 2f, value /2f);
            }
        }

        [HideInInspector]
        public Color32 activeColor;

        [HideInInspector]
        public Color32 inactiveColor;
                
        void Awake()
        {
            if (imageFrame == null)
                SetupFrame();
            if (imageBar == null)
                SetupBar();
        }

        public bool On
        {
            get
            {
                return _on;
            }

            set
            {
                _on = value;
                if (imageBar == null)
                    SetupBar();
                imageBar.color = _on ? activeColor : inactiveColor;
            }
        }

        void SetupBar()
        {
            if (transform.childCount > 0)
                imageBar = transform.GetChild(0).GetComponent<Image>();
            if (imageBar == null)
            {
                var GO = new GameObject("Bar");
                GO.transform.SetParent(transform);
                imageBar = GO.AddComponent<Image>();
                imageBar.preserveAspect = true;
                imageBar.SetNativeSize();
            }
        }

        void SetupFrame()
        {
            imageFrame = GetComponent<Image>();
            if (imageFrame == null)
            {
                imageFrame = gameObject.AddComponent<Image>();
                imageFrame.rectTransform.sizeDelta = new Vector2(16, 32);
                imageFrame.preserveAspect = true;
                imageFrame.SetNativeSize();

                if (imageBar != null)
                    imageBar.SetNativeSize();
            }
        }

        public void AlignAfter(HUDStatsBar bar)
        {
            transform.position = bar.RightEdge;
        }

        public void AlignAfter(Image image)
        {
            var size = image.rectTransform.sizeDelta.x;
            var rightOther = image.rectTransform.position + Vector3.right * size;
            transform.position = rightOther;
        }

        public Vector3 RightEdge
        {
            get
            {
                return imageFrame.rectTransform.position + Vector3.right * imageFrame.rectTransform.sizeDelta.x;
            }
        }
    }
}
