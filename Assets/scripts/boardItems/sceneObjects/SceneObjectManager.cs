using UnityEngine;

namespace BoardItems.SceneObjects
{
    public class SceneObjectManager : MonoBehaviour
    {
        public int id = 0;
        [SerializeField] Collider2D container;

        private void Awake()
        {
        }
        private void OnDestroy()
        {
        }
        public void Init()
        {
        }
        public void AttachItem(ItemInScene item)
        {
            item.transform.SetParent(container.transform);
        }
    }
}
