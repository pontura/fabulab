using BoardItems.Characters;
using BoardItems.SceneObjects;
using Google.MiniJSON;
using System;
using UI;
using UnityEngine;

namespace BoardItems
{
    public class BodyPart : MonoBehaviour
    {
        Collider2D col2D;
        public CharacterPartsHelper.parts part;
        [SerializeField] GameObject selectedBodySignal;
        [SerializeField] BodyPart mirror;
        float z_displacement = 0.001f;

        void Start()
        {
            col2D = GetComponentInChildren<Collider2D>();
            Events.OnNewBodyPartSelected += OnNewBodyPartSelected;
            Events.OnBodyPartActive += OnBodyPartActive;
            if (UIManager.Instance.boardUI.items.storyMode)
                selectedBodySignal.gameObject.SetActive(false);
        }
        void OnDestroy()
        {
            Events.OnNewBodyPartSelected -= OnNewBodyPartSelected;
            Events.OnBodyPartActive -= OnBodyPartActive;
            col2D = GetComponentInChildren<Collider2D>();
        }

        private void OnBodyPartActive(CharacterPartsHelper.parts part)
        {
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            bool canInteract = this.part == part;
            foreach (ItemInScene i in all)
            {
                if(i != null && i.rb != null)
                    i.rb.simulated = canInteract;
            }
        }

        void OnNewBodyPartSelected(BodyPart bp)
        {
            SetSelection(bp == this);          
        }
        public void SetSelection(bool isOn)
        {
            print("SetSelection " + isOn + " part: " + part + " GO: " + gameObject.name);
            selectedBodySignal.gameObject.SetActive(isOn);
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            foreach (ItemInScene i in all)
            {
                i.SetCollider(isOn);
            }
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
        void Arrage(bool back, ItemInScene item, Transform t)
        {
            item.transform.SetParent(t.parent);
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            foreach (ItemInScene i in all) print(i.gameObject.name);
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
            print("SetArrengedItems");
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
            return (all.Length +1) * -z_displacement;
            //foreach (ItemInScene i in all)
            //{
            //    if (i.transform.position.z < z)
            //        z = i.transform.position.z;
            //}
            //if(z==0)
            //    return 0;
            //return z - z_displacement;
        }
        public void OnStopTransformModify()
        {
            print("OnStopTransformModify");
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            foreach (ItemInScene i in all) i.transform.SetParent(transform.parent);
            transform.localPosition = Vector3.zero;
            foreach (ItemInScene i in all)
            {
                i.transform.SetParent(transform);
                i.data.position = i.transform.localPosition;
            }
        }
    }
}
