using UnityEngine;
using System.Collections;


namespace ProcRoom.UI
{

    public class AbilitySelector : MonoBehaviour {

        AbilityStat[] selectors;
        AbilityStat selected;

        [SerializeField]
        Physical.Ability ability;

        CharacterCreation characterCreation;
        
        void OnEnable()
        {
            if (characterCreation)
                CharacterCreation.OnNewPoints += OnNewPointsAvailable;
        }

        void OnDisable()
        {
            if (characterCreation)
                CharacterCreation.OnNewPoints -= OnNewPointsAvailable;
        }

        void OnNewPointsAvailable(int points)
        {
            for (int i=0; i<selectors.Length; i++)
            {
                if (!selectors[i].selected)
                {
                    if (i < ability.Length && ability[i].cost <= points)
                    {
                        selectors[i].allowed = true;
                        points -= ability[i].cost;
                    } else
                    {
                        selectors[i].allowed = false;
                        points = -1;
                    }
                }
            }
        }

        void Awake()
        {
            characterCreation = GetComponentInParent<CharacterCreation>();
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
                    selectors[nextIndex].SetIndex(nextIndex);
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
                if (!selectors[i].allowed && i < ability.Length)
                    shouldBeSelected = false;

                if (shouldBeSelected && !selectors[i].selected)
                {
                    selectors[i].selected = true;
                    culmulativeCost += ability[i].cost;
                } else if (!shouldBeSelected && selectors[i].selected)
                {
                    selectors[i].selected = false;
                    if (i < ability.Length)
                        culmulativeCost -= ability[i].cost;
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
                return selected == null ? 0 : ability[selected.Index].value;
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

                    if (i >= ability.Length || ability[i].value == value)
                    {
                        doSelect = false;
                        selected = selectors[i];
                    }
                }
            }
        }

        public int CostForNext
        {
            get
            {
                if (selectors == null)
                    return -1;
                
                var nextIndex = selected == null ? 1 : selected.Index + 1;

                if (ability.Length > nextIndex)
                    return ability[nextIndex].cost;
                return -1;
            }

        }
    }
}