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
            Reset();
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
            ItemButton i = Instantiate(itemButton, container);
            i.Init(this, itemData, s);
            if(all == null) Reset();
            all.Add(itemData, i);
        }
        public void OnClicked(ItemButton b)
        {
            ItemInScene i = b.itemData.GetComponent<ItemInScene>();
            ItemInScene newItem = items.AddNewItemFromMenu(i);
            inputManager.OnInitDragging(newItem);
            canvasGroup.alpha = 0.2f;
        }
    }
}
