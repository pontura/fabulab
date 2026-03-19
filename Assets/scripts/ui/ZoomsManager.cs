using BoardItems.Characters;
using UnityEngine;

namespace UI
{
    public enum ZoomStates
    {
        NONE,
        HEAD = 1,
        BODY = 2,
        HAND = 3,
        FOOT = 4,
        FOOT_LEFT = 5,
        HAND_LEFT = 6,
        HAIR = 7,
        FACE = 8,
        STORY = 9,
        BACKGROUND = 10
    }

    public class ZoomsManager : MonoBehaviour
    {
        [SerializeField] Animator animator;
        public ZoomStates currentZoom;
        public ZoomStates lastZoom;

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
        public void Zoom(ZoomStates _part, bool saving = false)
        {
            this.lastZoom = currentZoom;
            this.currentZoom = _part;
            animator.enabled = true;
            animator.SetInteger("zoom", (int)currentZoom);
        }
    }
}
