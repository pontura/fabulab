using BoardItems;
using UnityEngine;

namespace UI.MainApp
{
    public class BoardItemManager : MonoBehaviour
    {
        public virtual void Init() { }
        public virtual void AttachItem(ItemInScene item)  {  }
        public virtual void OnStopDrag(ItemInScene item)  {  }
        public virtual void MoveBack(ItemInScene itemSelected)  {  }
        public virtual void MoveUp(ItemInScene itemSelected) {  }
    }
}
