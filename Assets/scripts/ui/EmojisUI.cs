using System;
using UnityEngine;
using UnityEngine.UI;
using static CharacterAnimsManager;

namespace UI
{
    public class EmojisUI : MonoBehaviour
    {
        [SerializeField] string characterId = "";
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
            foreach (EmojiData d in Data.Instance.characterAnimsManager.emojis)
            {
                int buttonID = id;
                Button b = Instantiate(button, container);
                b.GetComponentInChildren<TMPro.TMP_Text>().text = d.name; 
                b.onClick.AddListener(() => Clicked(buttonID));
                id++;
            }
        }
        void Clicked(int id)
        {
            print(id);
            string e = Data.Instance.characterAnimsManager.emojis[id].clip.name;
            Events.OnCharacterExpression(characterId, e);
        }

        public void SetCharacterId(string id) {
            characterId = id;
        }
    }
}
