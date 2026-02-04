using BoardItems.Characters;
using UnityEngine;

namespace UI
{
    public class ZoomsManager : MonoBehaviour
    {
        [SerializeField] Animator animator;
        public CharacterData.parts part;

        CharacterData.parts lastZoom;

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
            Events.Zoom(lastZoom);
        }
        public void Zoom(CharacterData.parts _part)
        {
            this.lastZoom = part;
            this.part = _part;
            animator.SetInteger("zoom", (int)part);
        }
    }
}
