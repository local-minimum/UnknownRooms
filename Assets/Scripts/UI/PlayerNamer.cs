using UnityEngine;
using UnityEngine.UI;

namespace ProcRoom.UI
{
    public class PlayerNamer : MonoBehaviour
    {

        [SerializeField]
        Text defaultText;

        [SerializeField]
        Text enteredText;

        [SerializeField]
        string[] qualifier;

        [SerializeField]
        string[] profession;

        [SerializeField]
        string[] morphemes;

        public string Name
        {

            get
            {
                if (enteredText.text != null && enteredText.text != "" && enteredText.enabled)
                    return enteredText.text;
                return defaultText.text;
            }
        }

        void Awake()
        {
            Generate();
        }

        public void Generate()
        {
            string name = "";
            int steps = (Random.Range(1, 6) % 4) + 1;
            while (steps > 0)
            {
                name += morphemes[Random.Range(0, morphemes.Length)];
                steps--;
            }

            name = string.Format("{0} {1} {2}{3}", qualifier[Random.Range(0, qualifier.Length)], profession[Random.Range(0, profession.Length)], char.ToUpper(name[0]), name.Substring(1));
            enteredText.text = name;
            defaultText.text = name;
        }
    }
}