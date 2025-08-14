using BoardItems;
using BoardItems.UI;
using System.Collections.Generic;
using UnityEngine;

public class ItemsUI : MonoBehaviour
{
    [SerializeField] Transform container;
    [SerializeField] ItemButton itemButton;
    [SerializeField] Dictionary<ItemData, ItemButton> all;
    [SerializeField] Items items;
    [SerializeField] InputManager inputManager;
    CanvasGroup canvasGroup;

    private void Awake()
    {
        Events.EditMode += EditMode;
        Events.OnStopDrag += OnStopDrag;
        Restart();
    }
    public void Restart()
    {
        all = new Dictionary<ItemData, ItemButton>();
        Utils.RemoveAllChildsIn(container);
        canvasGroup = GetComponent<CanvasGroup>();
    }
    private void OnDestroy()
    {
        Events.OnStopDrag -= OnStopDrag;
        Events.EditMode -= EditMode;
    }
    void EditMode(bool isOn)
    {
        gameObject.SetActive(isOn);
    }
    private void OnStopDrag(ItemInScene scene, Vector3 vector)
    {
        canvasGroup.alpha = 1;
    }
    public void Add(ItemData itemData, Sprite s)
    {
        ItemButton i = Instantiate(itemButton, container);
        i.Init(this, itemData, s);
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
