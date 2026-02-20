using BoardItems.Characters;
using UnityEngine;

namespace UI
{
    public class ZoomsManager : MonoBehaviour
    {
        [SerializeField] Animator animator;
        public CharacterData.parts part;
        public CharacterData.parts lastZoom;

        private void Awake()
        {
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.Zoom -= Zoom;
        }
        public void ZoomToLastPart()
        {
            Events.Zoom(lastZoom, true);
        }
        public void Zoom(CharacterData.parts _part, bool saving = false)
        {
            this.lastZoom = part;
            this.part = _part;
            if(UIManager.Instance.boardUI.editingType == BoardUI.editingTypes.OBJECT)
                animator.SetInteger("zoom", 0);
            else
                animator.SetInteger("zoom", (int)part);
        }
    }
}
