using BoardItems.Characters;
using UnityEngine;

namespace BoardItems
{
    public class BodyPart : MonoBehaviour
    {
        Collider2D col2D;
        public CharacterData.parts part;
        [SerializeField] GameObject selectedBodySignal;
        float z_displacement = 0.001f;

        void Start()
        {
            Events.OnNewBodyPartSelected += OnNewBodyPartSelected;
            col2D = GetComponentInChildren<Collider2D>();
            SetSelection(false);
        }
        void OnDestroy()
        {
            Events.OnNewBodyPartSelected -= OnNewBodyPartSelected;
            col2D = GetComponentInChildren<Collider2D>();
        }
        void OnNewBodyPartSelected(BodyPart bp)
        {
            SetSelection(bp == this);
        }
        public void SetSelection(bool isOn)
        {
            selectedBodySignal.gameObject.SetActive(isOn);
        }
        public void SendToTop(ItemInScene item)
        {
            item.transform.SetParent(transform.parent);
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            foreach (ItemInScene i in all)
            {
                i.transform.SetParent(transform.parent);
            }
            foreach (ItemInScene i in all)
            {
                i.transform.SetParent(transform);
            }
            item.transform.SetParent(transform);
            SetArrengedItems();
        }
        public void SetArrengedItems()
        {
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            float _z = 0;
            foreach (ItemInScene i in all)
            {
                _z -= z_displacement;
                Vector3 pos = i.data.position;
                pos.z = _z;
                i.data.position = pos;
                i.SetPosByData();
            }
        }
    }
}
