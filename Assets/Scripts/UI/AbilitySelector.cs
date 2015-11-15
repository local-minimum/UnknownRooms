using UnityEngine;
using System.Collections;


namespace ProcRoom.UI
{

    public class AbilitySelector : MonoBehaviour {

        AbilityStat[] selectors;
        AbilityStat selected;

        void OnEnable()
        {
            CharacterCreation.OnNewPoints += OnNewPointsAvailable;
        }

        void OnDisable()
        {
            CharacterCreation.OnNewPoints -= OnNewPointsAvailable;
        }

        private void OnNewPointsAvailable(int points)
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
            OnNewPointsAvailable(CharacterCreation.Points);
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
        }
    }
}