using BoardItems.Characters;
using UnityEngine;
namespace BoardItems.UI
{
    public class ZoomsManager : MonoBehaviour
    {
        [SerializeField] Animator animator;
        public CharacterData.parts part;
        private void Awake()
        {
            Events.Zoom += Zoom;
        }
        private void OnDestroy()
        {
            Events.Zoom -= Zoom;
        }
        public void Zoom(CharacterData.parts part)
        {
            this.part = part;
            animator.SetInteger("zoom", (int)part);
        }
    }
}
