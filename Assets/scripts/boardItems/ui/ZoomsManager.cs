using UnityEngine;
namespace BoardItems.UI
{
    public class ZoomsManager : MonoBehaviour
    {
        [SerializeField] Animator animator;
        public zoomTypes zoomType;
        int idZoom = 0;
        public enum zoomTypes
        {
            HEAD,
            BELLY,
            HAND,
            FEET
        }
        private void Awake()
        {
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.Zoom -= Zoom;
        }
        public void Zoom(int idZoom)
        {
            animator.SetInteger("zoom", idZoom);
        }
    }
}
