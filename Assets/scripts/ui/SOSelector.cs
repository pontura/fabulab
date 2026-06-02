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
    public class SOSelector : ThumbsScreen
    {
        [SerializeField] Transform target;
        [SerializeField] PresetButton itemButton;
        [SerializeField] Dictionary<ItemData, ItemButton> all;
        private void Awake()
        {
            Reset();
        }
        public void SetOn(bool isOn)
        {
            gameObject.SetActive(isOn);
        }
        public void SetColores()
        {
            isActive = false;
            Utils.RemoveAllChildsIn(worksContainer);
            OpenColors();
        }
        public void SetObjects()
        {
            firstImageCache = false;
            if (imageCache == null)
                imageCache = new Dictionary<int, Texture2D>();
            imageCache.Clear();
            downloading.Clear();
            Init();

            Utils.RemoveAllChildsIn(worksContainer);
            List<SObjectData> generics = Data.Instance.sObjectsData.GetDataByType(SObjectData.types.generic);
            foreach (SObjectData cd in generics)
            {
                ItemSelectorBtn go = Instantiate(workBtn_prefab, worksContainer);
                print("go " + go);
                go.Init(cd);
                go.GetComponent<Button>().onClick.AddListener(() => OpenWork(cd.id));
            }
            isActive = true;
            OnLoadedDone();
        }
        public override void OpenWork(string id)
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
            Utils.RemoveAllChildsIn(worksContainer);
        }
        public void OnClicked(PresetButton pb)
        {
            Events.ColorizeBG(pb.color);
        }
        void OpenColors()
        {
            foreach (colorNames s in Data.Instance.palettesManager.backgrounds)
            {
                PresetButton b = Instantiate(itemButton, worksContainer);
                b.Init(OnClicked, s);
            }
        }

        protected override void LoadImage(int index, ItemSelectorBtn isb) {
            PropMetaData pMD = Data.Instance.sObjectsData.metaData.Find(x => x.id == isb.Id);
            if (pMD != null) {
                downloading.Add(index);
                Debug.Log($"$ DownloadTexture index: {index} Id: {pMD.id}");
                Data.Instance.cacheData.LoadImage(BoardItems.BoardData.MetadataTypes.so.ToString(), pMD.id, (tex) => {
                    downloading.Remove(index);
                    isb.SetSprite(tex);
                    imageCache[index] = tex;
                    Debug.Log($"ImageCache: {imageCache.Count}");
                    if (!firstImageCache && imageCache.Count >= (visibleRows * itemsPerRows)) {
                        firstImageCache = true;
                        Events.OnLoading(false);
                    }
                }, pMD.timestamp, pMD.userID);
            } else {
                Debug.LogError("Couldn´t find Film Metadata with ID " + isb.Id);
            }
        }
    }
}
