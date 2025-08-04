using BoardItems;
using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] Image image;
    ItemsUI items;
    public ItemData itemData;

    public void Init(ItemsUI items, ItemData itemData, Sprite sprite)
    {
        this.items = items;
        this.itemData = itemData;
        image.sprite = sprite;
    }
    public void OnClick()
    {
        print("asd");
        items.OnClicked(this);
    }
}
