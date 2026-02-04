using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class ActionsUI : MonoBehaviour
    {
        bool initialized;
        [SerializeField] Button button;
        [SerializeField] Transform container;

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
            int id = 0;
            foreach (string name in Enum.GetNames(typeof(CharacterAnims.anims)))
            {
                int buttonID = id;
                Button b = Instantiate(button, container);
                b.GetComponentInChildren<TMPro.TMP_Text>().text = name;
                b.onClick.AddListener(() => Clicked(buttonID));
                id++;
            }
        }

        int characterEditorID = 0;
        void Clicked(int id)
        {
            print(id);
            CharacterAnims.anims anim = (CharacterAnims.anims)id;
            Events.EditMode(anim == CharacterAnims.anims.edit);
            Events.OnCharacterAnim(characterEditorID, anim);
        }
    }
}
