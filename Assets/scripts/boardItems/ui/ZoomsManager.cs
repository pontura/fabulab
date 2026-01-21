using UnityEngine;
namespace BoardItems.UI
{
    public class ZoomsManager : MonoBehaviour
    {
        [SerializeField] TMPro.TMP_Text field;
        [SerializeField] Animator animator;
        public zoomTypes zoomType;
        int idZoom = 0;
        public enum zoomTypes
        {
            BODY,
            HEAD,
            HAND,
            BELLY,
            FEET
        }
        void Start()
        {
            SetZoom();
        }
        public void OnClicked()
        {
            idZoom++;
            if (idZoom > 4)
                idZoom = 0;
            SetZoom();
        }
        void SetZoom()
        {
            field.text = ((zoomTypes)idZoom).ToString();    
            Events.Zoom(zoomType);
            animator.SetInteger("zoom", idZoom);
        }
    }
}
