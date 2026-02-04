using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class EmojisUI : MonoBehaviour
    {
        [SerializeField] int characterEditorID = 0;
        [SerializeField] Button button;
        [SerializeField] Transform container;

        bool initialized;
        public void SetOn(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (!initialized)
                Init();
        }
        private void Init()
        {
            Utils.RemoveAllChildsIn(container);
            initialized = true;
            print("initialized");
            int id = 0;
            foreach (string name in Enum.GetNames(typeof(CharacterExpressions.expressions)))
            {
                int buttonID = id;
                Button b = Instantiate(button, container);
                b.GetComponentInChildren<TMPro.TMP_Text>().text = name; 
                b.onClick.AddListener(() => Clicked(buttonID));
                id++;
            }
        }
        void Clicked(int id)
        {
            print(id);
            CharacterExpressions.expressions e = (CharacterExpressions.expressions)id;
            Events.OnCharacterExpression(characterEditorID, e);
        }
    }
}
