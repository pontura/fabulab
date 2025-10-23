using BoardItems.UI;
using UnityEngine;
using UnityEngine.UI;

namespace BoardItems
{
    public class ToolsMenu : MonoBehaviour
    {
        [SerializeField] private ToolsSubMenu subMenu;
        ItemData itemData;
        InputManager inputManager;
        public GameObject noWebGLPanel;
        public GameObject commonButtons;
        public Image bg;

        private void Start()
        {
            gameObject.SetActive(false);

#if UNITY_ANDROID || UNITY_IOS
        noWebGLPanel.SetActive(false);
        commonButtons.transform.localPosition = Vector3.zero;
        bg.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 506);
#endif

        }
        public void Init(ItemData itemData, Vector3 pos, InputManager inputManager)
        {
            print("Init tools");
            this.itemData = itemData;
            this.inputManager = inputManager;
            gameObject.SetActive(true);
            transform.position = pos;
            GetComponent<ToolMenuesAdaptative>().Init();
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
            subMenu.Init(itemData, transform.position, ToolsSubMenu.types.COLORS);
            gameObject.SetActive(false);
        }
        public void Actions()
        {
            subMenu.Init(itemData, transform.position, ToolsSubMenu.types.ACTIONS);
            gameObject.SetActive(false);
        }
        public void Clonate()
        {
            Vector3 pos = UIManager.Instance.boardUI.items.GetItemSelected().data.position;
            UIManager.Instance.boardUI.items.Clonate(pos + new Vector3(1,0,0));
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }
        public void MoveBack()
        {
            UIManager.Instance.boardUI.items.MoveBack();
            inputManager.OnCloseTools(InputManager.states.IDLE);
        }

        public void ResetBtn()
        {
            UIManager.Instance.boardUI.items.RotateSnaped();
            //inputManager.OnCloseTools(InputManager.states.IDLE);
        }

    }

}