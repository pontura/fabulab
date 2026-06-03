using BoardItems.BoardData;
using UnityEngine;
using Yaguar.StoryMaker.DB;

namespace UI
{
    public class ShareBtn : MonoBehaviour
    {   
        [SerializeField] GameObject sharedGO;
        [SerializeField] bool isPublic;
        private System.Action<bool> onSharedChanged;
        public void Init(bool _isPublic, System.Action<bool> _onSharedChanged)
        {
            this.isPublic = _isPublic;
            this.onSharedChanged = _onSharedChanged;
            SetShare();
        }
         public void Show(bool showIt)
        {
           gameObject.SetActive(showIt);
        }
        public void SetShare()
        {
            sharedGO.SetActive(isPublic);
        }
        public void Toggle()
        {
            if(!isPublic)
                Events.OnConfirm("Vas a compartir este contenido a toda la comunidad", "COMPARTIR", "NO", OnConfirm);
            else
                Events.OnConfirm("Vas a dejar de compartir este contenido a toda la comunidad", "NO COMPARTIR", "CANCELAR", OnNoShareConfirm);
        }
        void OnConfirm(bool confirm)
        {
            if (confirm)
            {
                this.isPublic = true;
                SetShare();
                onSharedChanged(true);
                
            }
        }
        void OnNoShareConfirm(bool confirm)
        {
            if (confirm)
            {
                this.isPublic = false;
                SetShare();
                onSharedChanged(false);
            }
        }
    }
}
