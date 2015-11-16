﻿using UnityEngine;
using System.Collections;


namespace ProcRoom.UI
{

    public class AbilitySelector : MonoBehaviour {

        AbilityStat[] selectors;
        AbilityStat selected;

        CharacterCreation characterCreation;

        protected virtual void OnEnable()
        {
            if (characterCreation)
                CharacterCreation.OnNewPoints += OnNewPointsAvailable;
        }

        protected virtual void OnDisable()
        {
            if (characterCreation)
                CharacterCreation.OnNewPoints -= OnNewPointsAvailable;
        }

        protected virtual void OnNewPointsAvailable(int points)
        {
            for (int i=0; i<selectors.Length; i++)
            {
                if (!selectors[i].selected)
                {
                    if (selectors[i].cost <= points)
                    {
                        selectors[i].allowed = true;
                        points -= selectors[i].cost;
                    } else
                    {
                        selectors[i].allowed = false;
                        points = -1;
                    }
                }
            }
        }

        void Start() {
            characterCreation = GetComponentInParent<CharacterCreation>();
            var selectors = new AbilityStat[transform.childCount];
            int nextIndex = 0;
            for (int i = 0; i < selectors.Length; i++)
            {
                var sel = transform.GetChild(i).GetComponent<AbilityStat>();
                if (sel)
                {
                    selectors[nextIndex] = sel;
                    nextIndex++;
                }
            }
            this.selectors = new AbilityStat[nextIndex];
            System.Array.Copy(selectors, this.selectors, nextIndex);
            if (characterCreation)
                OnNewPointsAvailable(CharacterCreation.Points);
            for (int i=0; i<this.selectors.Length; i++)
            {
                if (this.selectors[i].selected)
                    selected = this.selectors[i];
            }
        }
                
        public void EmulateSelect(AbilityStat selector)
        {
            bool shouldBeSelected = selector != null;
            int culmulativeCost = 0;
            for (int i=0; i<selectors.Length;i++)
            {
                if (!selectors[i].allowed)
                    shouldBeSelected = false;

                if (shouldBeSelected && !selectors[i].selected)
                {
                    selectors[i].selected = true;
                    culmulativeCost += selectors[i].cost;
                } else if (!shouldBeSelected && selectors[i].selected)
                {
                    selectors[i].selected = false;
                    culmulativeCost -= selectors[i].cost;
                }

                //All coming should be not selected
                if (selector == selectors[i])
                    shouldBeSelected = false;
            }
            if (characterCreation)
                CharacterCreation.NewTransaction(culmulativeCost);
        }

        public void FreeEmulatedSelection()
        {
            EmulateSelect(selected);
        }

        public void Select(AbilityStat selector)
        {
            selected = selector;
            selected.selected = true;
            
        }

        public void Increase()
        {
            if (selected == null)
            {
                if (selectors.Length > 0)
                    selected = selectors[0];
            } else
            {
                for (int i=0; i<selectors.Length - 1; i++)
                {
                    if (selectors[i] == selected)
                    {
                        if (selectors[i + 1].allowed)
                            Select(selectors[i + 1]);
                    }
                }
            }

        }

        public int Value
        {
            get
            {
                return selected == null ? 0 : selected.value;
            }

            set
            {
                if (selectors == null)
                    return;
                bool doSelect = true;
                for (int i=0; i<selectors.Length; i++)
                {
                    if (doSelect)
                        selectors[i].allowed = doSelect;
                    selectors[i].selected = doSelect;

                    if (selectors[i].value == value)
                    {
                        doSelect = false;
                        selected = selectors[i];
                    }
                }
            }
        }
    }
}