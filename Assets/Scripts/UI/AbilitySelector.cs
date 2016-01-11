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
        int visibleSelectors = 0;

        void OnEnable()
        {
            characterCreation = GetComponentInParent<CharacterCreation>();
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
            for (int i=0; i<visibleSelectors; i++)
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

        void Start() {
             
            selectors = new AbilityStat[ability.Length];
            var foundSelectors = GetComponentsInChildren<AbilityStat>();
            visibleSelectors = Mathf.Min(selectors.Length, foundSelectors.Length);

            for (int i = 0, l=foundSelectors.Length; i < l; i++)
            {

                if (i < visibleSelectors)
                {
                    selectors[i] = foundSelectors[i];
                    selectors[i].SetIndex(i);
                    selectors[i].allowed = false;
                }
                else
                    foundSelectors[i].gameObject.SetActive(false);
            }

            for (int i=0; i<visibleSelectors; i++)
            {
                if (this.selectors[i].selected)
                    selected = this.selectors[i];
            }

            if (characterCreation)
                OnNewPointsAvailable(CharacterCreation.Points);

            Select(selectors[0]);

        }

        public void EmulateSelect(AbilityStat selector)
        {
            if (selector.Index < minIndex)
                return;
            bool shouldBeSelected = selector != null;
            int culmulativeCost = 0;
            for (int i=minIndex; i<visibleSelectors;i++)
            {
                if (!selectors[i].allowed)
                    shouldBeSelected = false;

                if (shouldBeSelected && !selectors[i].selected)
                {
                    selectors[i].selected = true;
                    culmulativeCost += ability[i].cost;
                } else if (!shouldBeSelected)
                {
                    if (selectors[i].selected)
                        culmulativeCost -= ability[i].cost;
                    selectors[i].selected = false;
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
            if (selector.Index < minIndex)
                return;           
            selected = selector;
            EmulateSelect(selected);
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
                bool doSelect = value > 0;
                for (int i=0; i<visibleSelectors; i++)
                {
                    if (doSelect)
                        selectors[i].allowed = doSelect;
                    selectors[i].selected = doSelect;

                    if (i>= ability.Length || ability[i].value == value)
                    {
                        doSelect = false;
                        selected = selectors[i];
                    }
                }
            }
        }

        int minIndex = 0;

        public int MinValue
        {
            get
            {
                return ability[minIndex].value;
            }

            set
            {
                for (int i=0; i<visibleSelectors; i++)
                {
                    if (ability[i].value == value)
                    {
                        minIndex = value;
                        if (selected && selected.Index > minIndex)
                        {
                            Value = value;
                        }
                            
                        return;
                    }
                }
                Debug.LogWarning("Unable to set min value for " + name);
            }
        }

        public int MaxValue
        {
            get
            {
                for (int i=minIndex; i<visibleSelectors; i++)
                {
                    if (!selectors[i].allowed)
                        return i - 1;
                }
                return visibleSelectors - 1;
            }

            set
            {
                bool isAllowed = value > 0;
                for (int i=minIndex; i<visibleSelectors; i++)
                {                    
                    selectors[i].allowed = isAllowed;
                    if (ability[i].value == value)
                        isAllowed = false;
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