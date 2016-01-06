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
                if (value < 0f)
                {
                    SetAutoScale(imageFrame.rectTransform, imageFrame, 1f, false);
                    SetAutoScale(imageBar.rectTransform, imageBar, 7f/15f, true);
                }
                else
                {
                    imageFrame.rectTransform.sizeDelta = new Vector2(imageFrame.sprite.bounds.extents.x * value / imageFrame.sprite.bounds.extents.y, value);
                    imageBar.rectTransform.sizeDelta = new Vector2(imageBar.sprite.bounds.extents.x * value / imageBar.sprite.bounds.extents.y * 7f / 15f, value * 7f / 15f);
                }
            }
        }


        void SetAutoScale(RectTransform t, Image image, float factor, bool centerX)
        {
            var aspect = image.sprite.rect.size.x / image.sprite.rect.size.y;
            image.preserveAspect = true;

            if (centerX)
            {
                t.anchorMin = new Vector2(0.5f, 0.5f - factor / 2f);
                t.anchorMax = new Vector2(0.5f, 0.5f + factor / 2f);
                t.pivot = new Vector2(0.5f, 0.5f);

            }
            else { 
                t.anchorMin = new Vector2(0f, 0.5f - factor / 2f);
                t.anchorMax = new Vector2(0f, 0.5f + factor / 2f);
                t.pivot = new Vector2(0f, 0.5f);
            }
            t.offsetMin = new Vector2(t.offsetMin.x, 0f);
            t.offsetMax = new Vector2(t.offsetMax.x, 0f);
            
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.height * aspect);
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
