using BoardItems.UI;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace BoardItems
{
    public class ToolsMenu : MonoBehaviour
    {
        [SerializeField] private ToolsSubMenu subMenu;
        ItemData itemData;
        InputManager inputManager;
        public Button rotate;
        public Button scale;
        public Button clonate;
        public Button colorize;
        public GameObject arrow;
        public Image bg;
        Vector3 pos;

        private void Start()
        {
#if UNITY_EDITOR
#elif UNITY_ANDROID || UNITY_IOS           
            rotate.gameObject.SetActive(false);
            scale.gameObject.SetActive(false);
#endif

        }
        public void Init(ItemData itemData, Vector3 pos, InputManager inputManager)
        {
            this.pos = pos;
            this.itemData = itemData;
            this.inputManager = inputManager;
            gameObject.SetActive(true);
            arrow.transform.position = pos;
            bool isBoardingItemManager = false;
            if (itemData != null)
                isBoardingItemManager = itemData.IsBoardingItemManager();

            //clonate.gameObject.SetActive(!isBoardingItemManager);
            colorize.gameObject.SetActive(!isBoardingItemManager);
        }
        public void InitGroupTools(InputManager inputManager)
        {
            print("Init group tools");
            this.inputManager = inputManager;
            gameObject.SetActive(true);
            arrow.transform.position = Vector3.zero;
           // clonate.gameObject.SetActive(false);
            colorize.gameObject.SetActive(false);
        }
        public void SetBGColors()
        {
            subMenu.Init(null, pos, ToolsSubMenu.types.BG_COLORS);
            gameObject.SetActive(false);
        }
        public void Close()
        {
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void SetOff()
        {
            //AudioManager.Instance.uiSfxManager.PlayTransp("pop", -2, 0.5f);
            gameObject.SetActive(false);
        }
        public void Delete()
        {
            Events.OnPopupTopSignalText("Item borrado");
            inputManager.Delete();
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void Rotate()
        {
            Events.OnPopupTopSignalText("Rotando objeto");
            inputManager.Rotate();
            inputManager.OnCloseTools(InputManager.states.ROTATING);
        }
        public void Scale()
        {
            Events.OnPopupTopSignalText("Escalando objeto");
            inputManager.Scale();
            inputManager.OnCloseTools(InputManager.states.SCALING);
        }
        public void Color()
        {
            subMenu.Init(itemData, pos, ToolsSubMenu.types.COLORS);
            gameObject.SetActive(false);
        }
        public void Actions()
        {
            subMenu.Init(itemData, pos, ToolsSubMenu.types.ACTIONS);
            gameObject.SetActive(false);
        }
        public void Clonate()
        {
            Events.OnPopupTopSignalText("Objeto clonado");
            if (UIManager.Instance.part == Characters.CharacterPartsHelper.parts.BODY) ClonateSO();
            else ClonateItemInAvatar();
        }
        public void ClonateSO()
        {
            ItemInScene itemToClonate = UIManager.Instance.boardUI.items.GetItemSelected();
            itemToClonate.SetTools(false);
            GameObject go = Instantiate(itemToClonate.gameObject, itemToClonate.transform.parent);
            ItemInScene clon = go.GetComponent<ItemInScene>();
            clon.data.ClonateFrom(itemToClonate.data);
            clon.data.position.x = itemToClonate.data.position.x + 0.25f;
            clon.data.SetTransformByData();
            inputManager.OnCloseTools(InputManager.states.IDLE);
            UIManager.Instance.undoManager.OnNewStep();
        }
        public void ClonateItemInAvatar()
        {
            ItemInScene itemToClonate = UIManager.Instance.boardUI.items.GetItemSelected();
            Vector3 itemPos = itemToClonate.data.position;
            ItemInScene newItem =  UIManager.Instance.boardUI.items.Clonate(itemPos);
            newItem.data.ClonateFrom(itemToClonate.data);
            newItem.transform.SetParent(itemToClonate.transform.parent);
            newItem.transform.position = itemToClonate.transform.position + new Vector3(0.2f,0,0);
            newItem.transform.rotation = itemToClonate.transform.rotation;
            newItem.transform.localScale = itemToClonate.transform.localScale;
            inputManager.OnCloseTools(InputManager.states.IDLE);
            UIManager.Instance.undoManager.OnNewStep();
        }
        public void MoveBack()
        {
            Events.OnPopupTopSignalText("Item al fondo");
            UIManager.Instance.boardUI.items.MoveBack();
            inputManager.OnCloseTools(InputManager.states.IDLE);
            UIManager.Instance.undoManager.OnNewStep();
        }
        public void MoveUp()
        {
            Events.OnPopupTopSignalText("Item al frente");
            UIManager.Instance.boardUI.items.MoveUp();
            inputManager.OnCloseTools(InputManager.states.IDLE);
            UIManager.Instance.undoManager.OnNewStep();
        }
        public void ResetBtn()
        {
            UIManager.Instance.boardUI.items.RotateSnaped();
            //inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void ScaleSnaped(bool up)
        {
            Events.OnPopupTopSignalText("Escalado snap " + up.ToString());
            UIManager.Instance.boardUI.items.ScaleSnaped(up);
            UIManager.Instance.undoManager.OnNewStep();
            //inputManager.OnCloseTools(InputManager.states.IDLE);
        }

    }

}