﻿using BoardItems.UI;
using System.Collections.Generic;
using UnityEngine;

namespace BoardItems
{
    public class ToolsSubMenu : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private SubMenuButton subMenuButton;
        [SerializeField] private Sprite colorImage;
        public ItemData itemData;
        public types type;
        public InputManager inputManager;
        int id;

        public enum types
        {
            COLORS,
            ACTIONS
        }
        private void Start()
        {
            gameObject.SetActive(false);
        }
        public void Init(ItemData itemData, Vector3 pos, types type)
        {
            AudioManager.Instance.uiSfxManager.Play("pop", 0.5f);
            Utils.RemoveAllChildsIn(container);
            this.itemData = itemData;
            this.type = type;
            id = 0;
            gameObject.SetActive(true);
            transform.position = pos;
            GetComponent<ToolMenuesAdaptative>().Init();
            switch (type)
            {
                case types.COLORS:
                    foreach (PalettesManager.colorNames colorNames in Data.Instance.palettesManager.GetPaletteData(itemData.paletteName).colors)
                    {
                        Sprite s = Instantiate(colorImage);
                        print(colorNames);
                        AddButton(s, Data.Instance.palettesManager.GetColorsByName(colorNames));
                    }
                    break;
                case types.ACTIONS:
                    foreach (AnimationsManager.anim animName in itemData.anims)
                    {
                        AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(animName);
                        Sprite s = Instantiate(animData.icon);
                        AddButtonAnims(s, animData);
                    }
                    break;
            }
        }

        void AddButton(Sprite sprite, List<Color> colors)
        {
            SubMenuButton newSubMenuButton = Instantiate(subMenuButton);
            newSubMenuButton.transform.SetParent(container);
            newSubMenuButton.transform.localScale = Vector3.one;
            newSubMenuButton.Init(id, sprite, OnClick, colors);
            id++;
        }
        void AddButtonAnims(Sprite sprite, AnimationsManager.AnimData animData)
        {
            SubMenuButton newSubMenuButton = Instantiate(subMenuButton);
            newSubMenuButton.transform.SetParent(container);
            newSubMenuButton.transform.localScale = Vector3.one;
            newSubMenuButton.Init(id, sprite, OnClick);
            id++;
        }
        public void OnClick(int id)
        {
            switch (type)
            {
                case types.COLORS:
                    PalettesManager.PaletteData pData = Data.Instance.palettesManager.GetPaletteData(itemData.paletteName);
                    Events.Colorize(pData.colors[id]);
                    break;
                case types.ACTIONS:
                    AnimationsManager.AnimData animData = Data.Instance.animationsManager.GetAnimByName(itemData.anims[id]);
                    Events.AnimateItem(animData);
                    break;
            }
        }
        public void Close()
        {
            gameObject.SetActive(false);
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
    }
}