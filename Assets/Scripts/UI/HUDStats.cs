using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ProcRoom.UI
{
    public enum AlignRelation { None, Before, After};

    public delegate void NewBarShape(HUDStats bar);

    public class HUDStats : MonoBehaviour
    {

        public static event NewBarShape OnNewBarShape;

        bool setUp = false;

        List<HUDStatsBar> HUDbars = new List<HUDStatsBar>();

        [SerializeField]
        Color32 activeColor;

        [SerializeField]
        Color32 inactiveColor;

        [SerializeField]
        Sprite labelSprite;

        [SerializeField]
        Sprite barSprite;

        [SerializeField]
        Sprite barFrameSprit;

        [SerializeField]
        Sprite endCapSprite;

        [SerializeField]
        HUDStats alignTo;

        [SerializeField]
        AlignRelation alignment;

        [SerializeField]
        float distance = 10;

        [SerializeField]
        HUDStatsBar bar;

        [SerializeField]
        float height = 48f;

        [SerializeField]
        bool warnOnEmpty = false;

        Image labelImage;

        Image capImage;

        int _maxValue;

        public int maxValue
        {
            set
            {
                _maxValue = value;

                setUp = HUDbars.Count == _maxValue;

                if (!setUp)
                    SetUpComponents();
        
                
            }

        }

        public int currentValue
        {
            set
            {
                if (!setUp)
                    SetUpComponents();

                for (int i = 0, l = HUDbars.Count; i < l; i++)
                {
                    HUDbars[i].On = i < value;
                }

                if (warnOnEmpty && value == 0)
                    WarningFlasher.GetByName("Ammo").Warn();
            }
        }

        void OnEnable()
        {
            OnNewBarShape += HUDStats_OnNewBarShape;
            
            if (alignTo)
                HUDStats_OnNewBarShape(alignTo);

            Game.OnScreenSizeChange += Game_OnScreenSizeChange;
        }

        void OnDisable()
        {
            OnNewBarShape -= HUDStats_OnNewBarShape;
            Game.OnScreenSizeChange -= Game_OnScreenSizeChange;
        }

        private void Game_OnScreenSizeChange(int from, int to)
        {
            Debug.Log(from + " -> " + to);
            SetUpComponents();
        }

        private void HUDStats_OnNewBarShape(HUDStats bar)
        {
            if (alignment == AlignRelation.After && alignTo == bar && alignTo.capImage != null)
            {
                if (!setUp)
                    SetUpComponents();


                var otherRT = alignTo.capImage.rectTransform;
                var pos = alignTo.Right + Vector3.right * distance;

                labelImage.rectTransform.position = pos;

                if (OnNewBarShape != null)
                    OnNewBarShape(this);
            }
        }

        void SetUpComponents()
        {

            SetupLabel();

            for (int i = 0, l = HUDbars.Count; i < l; i++)
            {
                HUDbars[i].height = height;
                if (i == 0)
                    HUDbars[i].AlignAfter(labelImage);
                else
                    HUDbars[i].AlignAfter(HUDbars[i - 1]);
            }

            while (HUDbars.Count < _maxValue)
                AddBar();

            SetupCap();

            setUp = true;

            if (OnNewBarShape != null)
                OnNewBarShape(this);
        }

        void SetupLabel()
        {
            if (labelImage == null)
            {
                labelImage = gameObject.AddComponent<Image>();
                labelImage.sprite = labelSprite;
                labelImage.color = activeColor;
            }
            var t = labelImage.rectTransform;        
            t.localScale = new Vector3(1, 1, 1);

            if (height < 0f)
            {
                SetAutoScale(t, labelImage);
            }
            else
            {
                t.anchorMax = new Vector2(0, 0.5f);
                t.anchorMin = t.anchorMax;
                t.pivot = t.anchorMax;
                t.sizeDelta = new Vector2(labelSprite.bounds.extents.x * height / labelSprite.bounds.extents.y, height);
            }

        }

        void AddBar()
        {
            var GO = Instantiate(bar.gameObject);
            GO.transform.SetParent(transform);
            var HUDbar = GO.GetComponent<HUDStatsBar>();

            HUDbar.On = false;
            HUDbar.inactiveColor = inactiveColor;
            HUDbar.activeColor = activeColor;
            HUDbar.Bar = barSprite;
            HUDbar.Frame = barFrameSprit;
            HUDbar.height = height;

            if (HUDbars.Count == 0)
                HUDbar.AlignAfter(labelImage);
            else
                HUDbar.AlignAfter(HUDbars[HUDbars.Count - 1]);

            HUDbars.Add(HUDbar);
        }

        void SetupCap()
        {
            if (capImage == null)
            {
                var GO = new GameObject(name + " cap");
                GO.transform.SetParent(transform);
                capImage = GO.AddComponent<Image>();
                capImage.sprite = endCapSprite;
                capImage.color = activeColor;
            }

            var t = capImage.rectTransform;
            t.localScale = Vector3.one;
            if (height < 0f)
            {
                SetAutoScale(t, capImage);
            }
            else
            {
                t.anchorMax = new Vector2(0, 0.5f);
                t.anchorMin = t.anchorMax;
                t.pivot = t.anchorMax;
                t.sizeDelta = new Vector2(endCapSprite.bounds.extents.x * height / endCapSprite.bounds.extents.y, height);
            }

            if (HUDbars.Count > 0)
                t.position = HUDbars[HUDbars.Count - 1].RightEdge;
            else
                t.position = labelImage.rectTransform.position + Vector3.right * labelImage.rectTransform.sizeDelta.x;
        }

        void SetAutoScale(RectTransform t, Image image)
        {
            var aspect = image.sprite.rect.size.x / image.sprite.rect.size.y;
            image.preserveAspect = true;

            t.anchorMin = new Vector2(0f, 0f);
            t.anchorMax = new Vector2(0f, 1f);
            t.pivot = new Vector2(0f, 0.5f);

            t.offsetMin = new Vector2(t.offsetMin.x, 0f);
            t.offsetMax = new Vector2(t.offsetMax.x, 0f);
            t.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, t.rect.height * aspect);
            
        }

        public Vector3 Right
        {
            get
            {
                var t = capImage.rectTransform;
                return t.TransformPoint(Vector3.right * t.sizeDelta.x);
            }
        }

    }
 
}