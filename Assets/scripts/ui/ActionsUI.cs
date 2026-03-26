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

        [SerializeField] string characterId = "";
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
            foreach (CharacterAnimsManager.AnimData d in Data.Instance.characterAnimsManager.all)
            {
                int buttonID = id;
                Button b = Instantiate(button, container);
                b.GetComponentInChildren<TMPro.TMP_Text>().text = d.name;
                b.onClick.AddListener(() => Clicked(buttonID));
                buttons.Add(b);
                id++;
            }
        }
        private void OnEnable()
        {
          //  Invoke("SelectLast", 0.1f);
        }
        //void SelectLast()
        //{
        //    Clicked(lastActionSelected);
        //}
        
        void Clicked(int id)
        {
            if (buttons.Count == 0) return;
            print("Clicked " + (id ));

            buttons[lastActionSelected].animator.SetBool("active", false);
            
            buttons[id].animator.SetBool("active", true);

            lastActionSelected = id;
            string anim = Data.Instance.characterAnimsManager.all[id].clip.name;
            Events.EditMode(false);
            Events.OnCharacterAnim(characterId, anim);
        }
        public void SetCharacterId(string id) {
            characterId = id;
        }


        public GridLayoutGroup grid;
        public int columns = 2;
        public float spacing = 2f;

        void Update()
        {
            if (!grid) return;

            RectTransform rt = grid.GetComponent<RectTransform>();
            float totalWidth = rt.rect.width;

            float totalSpacing = spacing * (columns - 1);
            float cellWidth = (totalWidth - totalSpacing) / columns;

            grid.cellSize = new Vector2(cellWidth, grid.cellSize.y);
        }
    }
}
