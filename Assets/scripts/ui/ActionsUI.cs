using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI
{
    public class ActionsUI : MonoBehaviour
    {
        bool initialized;
        [SerializeField] Button button;
        [SerializeField] List<Button> buttons;
        [SerializeField] Transform container;
        int lastActionSelected = 1;
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
                if (id > 0)// esquiva EDIT:
                {
                    int buttonID = id;
                    Button b = Instantiate(button, container);
                    b.GetComponentInChildren<TMPro.TMP_Text>().text = name;
                    b.onClick.AddListener(() => Clicked(buttonID));
                    buttons.Add(b);
                }
                id++;
            }
        }
        private void OnEnable()
        {
            Invoke("SelectLast", 0.1f);
        }
        void SelectLast()
        {
            Clicked(lastActionSelected);
        }
        int characterEditorID = 0;
        void Clicked(int id)
        {
            if (buttons.Count == 0) return;
            print("Clicked " + (id - 1 ));

            buttons[lastActionSelected - 1].animator.SetBool("active", false);
            
            buttons[id - 1].animator.SetBool("active", true);

            lastActionSelected = id;
            CharacterAnims.anims anim = (CharacterAnims.anims)id;
            Events.EditMode(false);
            Events.OnCharacterAnim(characterEditorID, anim);
        }
    }
}
