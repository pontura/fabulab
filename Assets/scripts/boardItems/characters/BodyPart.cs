using BoardItems.Characters;
using BoardItems.SceneObjects;
using Google.MiniJSON;
using System;
using UI;
using UI.MainApp;
using UnityEngine;

namespace BoardItems
{
    public class BodyPart : MonoBehaviour
    {
        Collider2D col2D;
        public CharacterPartsHelper.parts part;
        [SerializeField] GameObject selectedBodySignal;
        [SerializeField] BodyPart mirror;
        float z_displacement_inside = 0.001f;
        float z_displacement_outside = 0.1f;

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
        float GetZDisplacement()
        {
            if(part != CharacterPartsHelper.parts.BODY || GetComponentInParent<CharacterManager>() != null)
                return z_displacement_inside;
            else
                return z_displacement_outside;
        }
        public void SetArrengedItems()
        {
            print("SetArrengedItems");
            ItemInScene[] all = GetComponentsInChildren<ItemInScene>();
            float _z = 0;
            foreach (ItemInScene i in all)
            {
                if (!i.itemInsideContainer)
                {
                    _z -= GetZDisplacement();
                    Vector3 pos = i.data.position;
                    pos.z = _z;
                    i.data.position = pos;
                    i.SetPosByData();
                }
            }
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
