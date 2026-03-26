using UI;
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
            Animator anim = GetComponent<Animator>();
            if (anim == null) return;

            switch (Data.Instance.sObjectsData.Type)
            {
                case BoardItems.BoardData.SObjectData.types.generic:
                    anim.Play("generic");
                    break;
                case BoardItems.BoardData.SObjectData.types.background:
                    anim.Play("background");
                    break;
            }
            bp.SetSelection(true);
            Events.DragAndDropActive(true);
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

        public Transform GetContainer() {
            return bp.transform;
        }
    }
}
