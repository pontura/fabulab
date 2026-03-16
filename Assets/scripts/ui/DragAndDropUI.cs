using BoardItems;
using BoardItems.BoardData;
using BoardItems.Characters;
using System.Collections;
using System.Collections.Generic;
using UI.MainApp;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEditor.Progress;

namespace UI
{
    public class DragAndDropUI : MonoBehaviour
    {
        [SerializeField] Transform container;
        [SerializeField] ItemButton itemButton;
        [SerializeField] Dictionary<ItemData, ItemButton> all;
        [SerializeField] Items items;
        [SerializeField] InputManager inputManager;
        CanvasGroup canvasGroup;

        private void Awake()
        {
            Events.OnStopDrag += OnStopDrag;
        }
        public void Init()
        {
            GetComponent<RectTransform>().anchorMin = Vector2.zero;
            GetComponent<RectTransform>().anchorMax = Vector2.one;
            GetComponent<RectTransform>().offsetMin = Vector2.zero;
            GetComponent<RectTransform>().offsetMax = Vector2.zero;
        }
        public void SetOn(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                CharacterAnims.anims anim = CharacterAnims.anims.edit;
                Events.OnCharacterAnim("", anim);
            }
        }
        public void Reset()
        {
            print("__________ResetResetResetReset");
            all = new Dictionary<ItemData, ItemButton>();
            Utils.RemoveAllChildsIn(container);
            canvasGroup = GetComponent<CanvasGroup>();
        }
        private void OnDestroy()
        {
            Events.OnStopDrag -= OnStopDrag;
        }
       
        private void OnStopDrag(ItemInScene scene, Vector3 vector)
        {
            canvasGroup.alpha = 1;
        }
        public void Add(ItemData itemData, Sprite s)
        {
            print("AddItem " + itemData.id);
            if (all == null) Reset();
            ItemButton i = Instantiate(itemButton, container);
            i.Init(this, itemData, s);
            all.Add(itemData, i);
            print("AddItem done " + itemData.id + " count: " + all.Count);
        }
        public void OnClicked(ItemButton b)
        {
            ItemInScene i = b.itemData.GetComponent<ItemInScene>();
            ItemInScene newItem = items.AddNewItemFromMenu(i);
            CharacterPartsHelper.parts part = UIManager.Instance.part;
            BoardItems.BodyPart bp = UIManager.Instance.boardUI.activeBoardItem.GetBodyPart(part);
            Vector3 dest = bp.transform.position;
            newItem.transform.position = dest;
            newItem.data.part = (CharacterPartsHelper.parts)(int)UIManager.Instance.part;
            newItem.InitItemInPart(bp);
            Events.OnStopDrag(newItem, dest);
            StartCoroutine(Cascade(newItem));
        }
        IEnumerator Cascade(ItemInScene i)
        {
            i.Appear(2);
            yield return new WaitForSeconds(0.05f);
            i.AppearAction();
            yield return new WaitForSeconds(0.05f);
            i.SetCollider(true);
        }
    }
}
