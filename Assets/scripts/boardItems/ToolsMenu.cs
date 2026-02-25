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
        public GameObject[] webGL;
        public GameObject arrow;
        public Image bg;
        Vector3 pos;

        private void Start()
        {
#if UNITY_EDITOR

#elif UNITY_ANDROID || UNITY_IOS
            foreach(GameObject go in webGL)
                go.SetActive(false);
#endif

        }
        public void Init(ItemData itemData, Vector3 pos, InputManager inputManager)
        {
            this.pos = pos;
            print("Init tools");
            this.itemData = itemData;
            this.inputManager = inputManager;
            gameObject.SetActive(true);
            arrow.transform.position = pos;
        }
        public void Close()
        {
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void SetOff()
        {
            AudioManager.Instance.uiSfxManager.PlayTransp("pop", -2, 0.5f);
            gameObject.SetActive(false);
        }
        public void Delete()
        {
            inputManager.Delete();
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void Rotate()
        {
            inputManager.Rotate();
            inputManager.OnCloseTools(InputManager.states.ROTATING);
        }
        public void Scale()
        {
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
            ItemInScene itemToClonate = UIManager.Instance.boardUI.items.GetItemSelected();
            Vector3 itemPos = itemToClonate.data.position;
            ItemInScene newItem =  UIManager.Instance.boardUI.items.Clonate(itemPos);
            newItem.data.ClonateFrom(itemToClonate.data);
            newItem.transform.SetParent(itemToClonate.transform.parent);
            newItem.transform.position = itemToClonate.transform.position + new Vector3(0.2f,0,0);
            newItem.transform.rotation = itemToClonate.transform.rotation;
            newItem.transform.localScale = itemToClonate.transform.localScale;
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void MoveBack()
        {
            UIManager.Instance.boardUI.items.MoveBack();
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void MoveUp()
        {
            UIManager.Instance.boardUI.items.MoveUp();
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void ResetBtn()
        {
            UIManager.Instance.boardUI.items.RotateSnaped();
            //inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void ScaleSnaped(bool up)
        {
            UIManager.Instance.boardUI.items.ScaleSnaped(up);
            //inputManager.OnCloseTools(InputManager.states.IDLE);
        }

    }

}