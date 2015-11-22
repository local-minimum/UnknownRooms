using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace ProcRoom.UI
{

    public class HUDStats : MonoBehaviour
    {
        bool setUp = false;

        List<Image> images = new List<Image>();

        [SerializeField]
        Color32 activeColor;
        [SerializeField]
        Color32 inactiveColor;

        public int maxValue
        {
            set
            {
                if (!setUp)
                    SetUpComponents();

                for (int i = 0, l = images.Count; i < l; i++)
                    images[i].enabled = i < value;           

            }

        }

        public int currentValue
        {
            set
            {
                if (!setUp)
                    SetUpComponents();

                for (int i = 0, l = images.Count; i < l; i++)
                {
                    if (images[i].enabled)
                        images[i].color = i < value ? activeColor : inactiveColor;
                }

            }
        }

        void Awake()
        {
            if (!setUp)
                SetUpComponents();

        }


        void SetUpComponents()
        {
            for (int i=0, l=transform.childCount; i< l; i++)
            {
                var img = transform.GetChild(i).GetComponentInChildren<Image>();
                if (img)
                    images.Add(img);
            }
         
            setUp = true;
        }
    }
}