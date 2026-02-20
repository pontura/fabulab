using UI.MainApp;
using UnityEngine;

namespace BoardItems.SceneObjects
{
    public class SceneObjectManager : BoardItemManager
    {
        public int id = 0;
        [SerializeField] Collider2D container;

        private void Awake()
        {
        }
        private void OnDestroy()
        {
        }
        public override void Init()
        {
        }
        public override void AttachItem(ItemInScene item)
        {
            item.transform.SetParent(container.transform);
        }
    }
}
