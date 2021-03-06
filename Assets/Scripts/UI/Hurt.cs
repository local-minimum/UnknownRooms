﻿using UnityEngine;
using System.Collections.Generic;

namespace ProcRoom.UI
{
    public enum HurtEffects {Sprite, Particles};

    public class Hurt : MonoBehaviour
    {

        static Hurt _instance;

        static Hurt instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<Hurt>();

                return _instance;
            }
        }

        public static void Place(Coordinate position)
        {
            instance.AnimateHurt(Tower.ActiveRoom.GetTileCentre(position));
        }


        SpriteRenderer rend;
        ParticleSystem effect;

        [SerializeField]
        HurtEffects hurtEffect;    

        void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        void AnimateHurt(Vector3 position)
        {
            if (rend == null)
            {
                rend = GetComponent<SpriteRenderer>();
                rend.enabled = false;
            }
            if (effect == null)
            {
                effect = GetComponent<ParticleSystem>();
            }

            if (hurtEffect == HurtEffects.Sprite)
                StartCoroutine(animate(position));
            else if (hurtEffect == HurtEffects.Particles)
            {
                transform.position = position;
                effect.Play();
            }
        }

        IEnumerator<WaitForSeconds> animate(Vector3 position)
        {
            while (rend.enabled)
                yield return new WaitForSeconds(0.01f);
            
            transform.position = position;
            float scale = 2f;
            float targetScale = 6f;
            transform.localScale = Vector3.one * scale;
            rend.enabled = true;
            yield return new WaitForSeconds(0.1f);

            while (scale < targetScale)
            {
                scale = Mathf.Lerp(scale, targetScale * 1.1f, 0.5f);
                transform.localScale = Vector3.one * scale;
                yield return new WaitForSeconds(0.01f);
            }
            yield return new WaitForSeconds(0.2f);
            rend.enabled = false;
        }
    }
}
