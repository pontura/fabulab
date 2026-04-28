using UnityEngine;
using static AnimationsManager;
namespace UI.MainApp
{
    public class TopSignal : MonoBehaviour
    {
        [SerializeField] GameObject panel;
        [SerializeField] TMPro.TMP_Text field;
        [SerializeField] Animator anim;

        void Start()
        {
            SetOff();
            Events.OnPopupTopSignalText += OnPopupTopSignalText;
        }
        void OnDestroy()
        {
            Events.OnPopupTopSignalText -= OnPopupTopSignalText;
        }
        void OnPopupTopSignalText(string text)
        {
            panel.SetActive(true); 
            field.text = text;
            anim.Play("on", 0,0);
            CancelInvoke();
            Invoke("SetOff", 2);
        }
        void SetOff()
        {
            panel.SetActive(false);
        }
    }
}