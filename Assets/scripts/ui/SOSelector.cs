using BoardItems;
using BoardItems.BoardData;
using System.Collections.Generic;
using UI.MainApp;
using UI.MainApp.Home.User;
using UnityEngine;
using UnityEngine.UI;
using static PalettesManager;

namespace UI
{
    public class SOSelector : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Transform container;
        [SerializeField] PresetButton itemButton;
        [SerializeField] CharacterSelectorBtn workBtn_prefab;
        [SerializeField] Dictionary<ItemData, ItemButton> all;
        private void Awake()
        {
            Reset();
        }
        public void SetOn(bool isOn)
        {
            int artID = 0;
            gameObject.SetActive(isOn);
        }
        public void SetColores()
        {
            Utils.RemoveAllChildsIn(container);
            OpenColors();
        }
        public void SetObjects()
        {
            Utils.RemoveAllChildsIn(container);
            List<SObjectData> generics = Data.Instance.sObjectsData.GetDataByType(SObjectData.types.generic);
            foreach (SObjectData cd in generics)
            {
                CharacterSelectorBtn go = Instantiate(workBtn_prefab, container);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
        }
        public void OpenWork(string id)
        {
            print("abre " + id);
            SOPartData o = Data.Instance.sObjectsData.GetSO(id);
            GameObject go = new GameObject();
            BoardItemManager boardItemManager_to_add = go.AddComponent<BoardItemManager>();
            UIManager.Instance.boardUI.items.AddSceneObjectTo(o, boardItemManager_to_add, target);
        }

        public void Reset()
        {
            all = new Dictionary<ItemData, ItemButton>();
            Utils.RemoveAllChildsIn(container);
        }
        public void OnClicked(PresetButton pb)
        {
            Events.ColorizeBG(pb.color);
        }
        void OpenColors()
        {
            foreach (colorNames s in Data.Instance.palettesManager.backgrounds)
            {
                PresetButton b = Instantiate(itemButton, container);
                b.Init(OnClicked, s);
            }
        }
    }
}
