using BoardItems;
using System.Collections.Generic;
using UnityEngine;

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
        public void SetOn(bool isOn)
        {
            gameObject.SetActive(isOn);
            if (isOn)
            {
                CharacterAnims.anims anim = CharacterAnims.anims.edit;
                Events.OnCharacterAnim(0, anim);
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
            newItem.data.part = UIManager.Instance.zoomManager.part;
            inputManager.OnInitDragging(newItem);
            canvasGroup.alpha = 0.2f;
        }
    }
}
