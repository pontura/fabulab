using BoardItems;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ItemButton : MonoBehaviour
    {
        [SerializeField] Image image;
        DragAndDropUI dragAndDropUI;
        public ItemData itemData;

        public void Init(DragAndDropUI dragAndDropUI, ItemData itemData, Sprite sprite)
        {
            this.dragAndDropUI = dragAndDropUI;
            this.itemData = itemData;
            image.sprite = sprite;
        }
        public void OnClick()
        {
            dragAndDropUI.OnClicked(this);
        }
    }

}