using BoardItems.Characters;
using UnityEngine;

namespace BoardItems
{
    public class BodyPart : MonoBehaviour
    {
        Collider2D col2D;
        public CharacterData.parts part;
        [SerializeField] GameObject selectedBodySignal;
        [SerializeField] BodyPart mirror;
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
        public void SendToBack(ItemInScene item)
        {
            Arrage(true, item, transform);
            if(mirror != null)
                mirror.Arrage(true, item.GetMirror(), mirror.transform);
        }
        public void SendToTop(ItemInScene item)
        {
            Arrage(false, item, transform);
            if (mirror != null)
                mirror.Arrage(false, item.GetMirror(), mirror.transform);
        }
        public void Arrage(bool back, ItemInScene item, Transform t)
        {
            item.transform.SetParent(t.parent);
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            if (back)
            {
                foreach (ItemInScene i in all) i.transform.SetParent(t.parent);
                item.transform.SetParent(t);
                foreach (ItemInScene i in all) i.transform.SetParent(t);
            }
            else
            {
                foreach (ItemInScene i in all) i.transform.SetParent(t.parent);
                foreach (ItemInScene i in all) i.transform.SetParent(t);
                item.transform.SetParent(t);
            }
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
        public float GetLastZ()
        {
            float z = 0;
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            foreach (ItemInScene i in all)
            {
                if (i.transform.position.z < z)
                    z = i.transform.position.z;
            }
            if(z==0)
                return 0;
            return z - z_displacement;
        }
    }
}
