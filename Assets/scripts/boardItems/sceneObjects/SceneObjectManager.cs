using UI.MainApp;
using UnityEngine;

namespace BoardItems.SceneObjects
{
    public class SceneObjectManager : BoardItemManager
    {
        public string id = "";
        [SerializeField] BodyPart bp;

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
            item.transform.SetParent(bp.transform);
        }
        public override void OnStopDrag(ItemInScene item)
        {
            bp.SetArrengedItems();
        }
        public override void MoveBack(ItemInScene itemSelected)
        {
            bp.SendToBack(itemSelected);
        }
        public override void MoveUp(ItemInScene itemSelected)
        {
            bp.SendToTop(itemSelected);
        }
    }
}
